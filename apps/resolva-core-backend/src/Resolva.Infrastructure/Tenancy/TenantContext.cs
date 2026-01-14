using Microsoft.AspNetCore.Http;

namespace Resolva.Infrastructure.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
}

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _http;
    private Guid? _cached;

    public TenantContext(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid? TenantId
    {
        get
        {
            if (_cached.HasValue) return _cached;

            var user = _http.HttpContext?.User;
            var tenantClaim = user?.FindFirst("tenant_id")?.Value;

            if (Guid.TryParse(tenantClaim, out var tid))
            {
                _cached = tid;
                return _cached;
            }

            return null;
        }
    }
}
