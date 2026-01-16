using System.Text.Json;

namespace Resolva.Api.Contracts.SurveyTemplates;

public record UpdateSurveyTemplateRequest(
    string? Name,
    bool? IsActive,
    JsonElement? FlowJson
);
