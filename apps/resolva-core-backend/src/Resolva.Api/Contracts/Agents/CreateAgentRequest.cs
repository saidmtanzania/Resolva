namespace Resolva.Api.Contracts.Agents;

public record CreateAgentRequest(
    string Email,
    string Password,
    string DisplayName,
    string? Phone,
    string Role // Admin/Manager/Support/NOC/Technician
);
