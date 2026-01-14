namespace Resolva.Api.Contracts.Agents;

public record UpdateAgentRequest(
    string? DisplayName,
    string? Phone,
    string? Role,
    bool? IsActive
);
