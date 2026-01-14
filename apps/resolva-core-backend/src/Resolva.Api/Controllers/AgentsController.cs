using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resolva.Api.Common;
using Resolva.Api.Contracts.Agents;
using Resolva.Core.Entities;
using Resolva.Core.Enums;

namespace Resolva.Api.Controllers;

[ApiController]
[Route("api/agents")]
[Authorize(Policy = "AdminOnly")]
public class AgentsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AgentsController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<ActionResult<AgentResponse>> Create([FromBody] CreateAgentRequest req)
    {
        // Validate role
        if (!Roles.All.Contains(req.Role))
            return BadRequest(new { message = $"Invalid role. Allowed: {string.Join(", ", Roles.All)}" });

        var tenantId = UserContext.GetTenantId(User);

        // Prevent duplicates in same tenant
        var exists = await _userManager.Users.AnyAsync(u => u.TenantId == tenantId && u.Email == req.Email);
        if (exists)
            return Conflict(new { message = "User with this email already exists in this tenant" });

        var user = new ApplicationUser
        {
            UserName = req.Email,
            Email = req.Email,
            TenantId = tenantId,
            DisplayName = req.DisplayName,
            PhoneNumber = req.Phone,
            IsActive = true,
            EmailConfirmed = true
        };

        var createRes = await _userManager.CreateAsync(user, req.Password);
        if (!createRes.Succeeded)
            return BadRequest(new { message = "Failed to create user", errors = createRes.Errors.Select(e => e.Description) });

        var roleRes = await _userManager.AddToRoleAsync(user, req.Role);
        if (!roleRes.Succeeded)
            return BadRequest(new { message = "User created but failed to assign role", errors = roleRes.Errors.Select(e => e.Description) });

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AgentResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.PhoneNumber,
            user.IsActive,
            user.TenantId,
            roles.ToArray()
        ));
    }

    [HttpGet]
    public async Task<ActionResult<List<AgentResponse>>> List([FromQuery] string? search = null)
    {
        var tenantId = UserContext.GetTenantId(User);

        var q = _userManager.Users.Where(u => u.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(s)) ||
                (u.DisplayName != null && u.DisplayName.ToLower().Contains(s)));
        }

        var users = await q.OrderBy(u => u.DisplayName).ToListAsync();

        var results = new List<AgentResponse>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            results.Add(new AgentResponse(
                u.Id,
                u.Email!,
                u.DisplayName,
                u.PhoneNumber,
                u.IsActive,
                u.TenantId,
                roles.ToArray()
            ));
        }

        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AgentResponse>> Get(string id)
    {
        var tenantId = UserContext.GetTenantId(User);

        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AgentResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.PhoneNumber,
            user.IsActive,
            user.TenantId,
            roles.ToArray()
        ));
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<AgentResponse>> Update(string id, [FromBody] UpdateAgentRequest req)
    {
        var tenantId = UserContext.GetTenantId(User);

        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);
        if (user == null) return NotFound();

        if (req.DisplayName != null) user.DisplayName = req.DisplayName;
        if (req.Phone != null) user.PhoneNumber = req.Phone;
        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;

        var updateRes = await _userManager.UpdateAsync(user);
        if (!updateRes.Succeeded)
            return BadRequest(new { message = "Failed to update user", errors = updateRes.Errors.Select(e => e.Description) });

        // Update role (MVP: single role)
        if (!string.IsNullOrWhiteSpace(req.Role))
        {
            if (!Roles.All.Contains(req.Role))
                return BadRequest(new { message = $"Invalid role. Allowed: {string.Join(", ", Roles.All)}" });

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeRes = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRes.Succeeded)
                return BadRequest(new { message = "Failed to remove old roles", errors = removeRes.Errors.Select(e => e.Description) });

            var addRes = await _userManager.AddToRoleAsync(user, req.Role);
            if (!addRes.Succeeded)
                return BadRequest(new { message = "Failed to assign new role", errors = addRes.Errors.Select(e => e.Description) });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AgentResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.PhoneNumber,
            user.IsActive,
            user.TenantId,
            roles.ToArray()
        ));
    }

    // Soft deactivate
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var tenantId = UserContext.GetTenantId(User);

        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);
        if (user == null) return NotFound();

        user.IsActive = false;
        var res = await _userManager.UpdateAsync(user);

        if (!res.Succeeded)
            return BadRequest(new { message = "Failed to deactivate user", errors = res.Errors.Select(e => e.Description) });

        return NoContent();
    }
}
