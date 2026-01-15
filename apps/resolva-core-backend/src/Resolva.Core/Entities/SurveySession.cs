namespace Resolva.Core.Entities;

public class SurveySession : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public Guid EventId { get; set; }
    public Guid TemplateId { get; set; }

    public string RecipientPhone { get; set; } = "";
    public string Channel { get; set; } = "whatsapp";

    public string Status { get; set; } = "pending"; // pending, sent, in_progress, completed, expired, failed

    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? LastInteractionAt { get; set; }

    public int ReminderCount { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
