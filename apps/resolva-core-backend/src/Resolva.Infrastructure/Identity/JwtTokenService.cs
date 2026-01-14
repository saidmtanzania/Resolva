using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Resolva.Core.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Resolva.Infrastructure.Identity;

public class JwtTokenService
{
    private readonly IConfiguration _cfg;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(IConfiguration cfg, UserManager<ApplicationUser> userManager)
    {
        _cfg = cfg;
        _userManager = userManager;
    }

    public async Task<(string Token, int ExpiresInSeconds)> CreateTokenAsync(ApplicationUser user)
    {
        var jwtSection = _cfg.GetSection("Jwt");
        var issuer = jwtSection["Issuer"]!;
        var audience = jwtSection["Audience"]!;
        var expiresMinutes = int.Parse(jwtSection["ExpiresMinutes"]!);
        var key = jwtSection["Key"]!;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);

        // Claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("tenant_id", user.TenantId.ToString()),
            new("display_name", user.DisplayName ?? ""),
            new("is_active", user.IsActive ? "1" : "0"),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiresMinutes * 60);
    }
}
