namespace Resolva.Api.Contracts.Internal.Surveys;

public record CompleteSurveyRequest(
    Guid TenantId,
    Guid SessionId
);
