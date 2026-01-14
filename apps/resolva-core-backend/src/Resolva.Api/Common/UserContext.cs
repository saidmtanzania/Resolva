using System.Security.Claims;

namespace Resolva.Api.Common;

public static class UserContext
{
    public static Guid GetTenantId(ClaimsPrincipal user)
    {
        var tid = user.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tid, out var tenantId))
            throw new UnauthorizedAccessException("Missing tenant_id claim");
        return tenantId;
    }
}
