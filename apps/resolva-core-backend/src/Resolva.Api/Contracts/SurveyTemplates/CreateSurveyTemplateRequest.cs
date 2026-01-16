using System.Text.Json;

namespace Resolva.Api.Contracts.SurveyTemplates;

public record CreateSurveyTemplateRequest(
    string Name,
    string EventType,
    string Language,
    bool IsActive,
    JsonElement FlowJson
);
