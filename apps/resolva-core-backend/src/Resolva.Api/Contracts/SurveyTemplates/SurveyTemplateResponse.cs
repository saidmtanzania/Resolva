using System.Text.Json;

namespace Resolva.Api.Contracts.SurveyTemplates;

public record SurveyTemplateResponse(
    Guid Id,
    string Name,
    string EventType,
    string Language,
    int Version,
    bool IsActive,
    string Channel,
    string? WhatsAppFlowId,
    string? WhatsAppStatus,
    DateTimeOffset? PublishedAt,
    JsonElement? ValidationErrors,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    JsonElement FlowJson
);
