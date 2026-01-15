using System.Text.Json;

namespace Resolva.Api.Contracts.Internal.Surveys;

public record SurveyAnswerRequest(
    Guid TenantId,
    Guid SessionId,
    string QuestionId,
    JsonElement AnswerJson,
    DateTimeOffset? AnsweredAt
);
