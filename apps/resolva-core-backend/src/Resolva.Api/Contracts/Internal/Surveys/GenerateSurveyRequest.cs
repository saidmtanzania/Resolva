using System.Text.Json;

namespace Resolva.Api.Contracts.Internal.Surveys;

public record GenerateSurveyRequest(
    Guid TenantId,
    Guid EventId,
    string Language,
    string CreatedBy,
    JsonElement SchemaJson
);
