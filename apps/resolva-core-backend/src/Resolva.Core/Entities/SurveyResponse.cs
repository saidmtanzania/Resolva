using System.Text.Json;

namespace Resolva.Core.Entities;

public class SurveyResponse : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public Guid SessionId { get; set; }
    public string QuestionId { get; set; } = "";   // "q1", "rating", etc.

    public JsonDocument? AnswerJson { get; set; } = JsonDocument.Parse("{}");

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
