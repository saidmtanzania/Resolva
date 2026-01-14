namespace Resolva.Api.Contracts.Agents;

public record AgentResponse(
    string Id,
    string Email,
    string DisplayName,
    string? Phone,
    bool IsActive,
    Guid TenantId,
    string[] Roles
);
