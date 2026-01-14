using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resolva.Core.Entities;
using Resolva.Infrastructure.Data;
using Resolva.Infrastructure.Identity;
using System.Security.Claims;

namespace Resolva.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ResolvaDbContext _db;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwt;

    public AuthController(
        ResolvaDbContext db,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwt)
    {
        _db = db;
        _signInManager = signInManager;
        _userManager = userManager;
        _jwt = jwt;
    }

    public record LoginRequest(string Tenant, string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var tenant = await _db.Tenants.SingleOrDefaultAsync(t => t.Slug == req.Tenant);
        if (tenant == null) return Unauthorized(new { message = "Invalid tenant" });

        var user = await _userManager.Users.SingleOrDefaultAsync(u =>
            u.TenantId == tenant.Id && u.Email == req.Email);

        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid credentials" });

        var check = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
        if (!check.Succeeded)
            return Unauthorized(new { message = "Invalid credentials" });

        var (token, expiresIn) = await _jwt.CreateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            accessToken = token,
            expiresIn,
            user = new
            {
                id = user.Id,
                displayName = user.DisplayName,
                email = user.Email,
                tenantId = user.TenantId,
                roles
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var tenantId = User.FindFirstValue("tenant_id");
        var displayName = User.FindFirstValue("display_name");
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray();

        return Ok(new { userId, tenantId, displayName, roles });
    }
}
