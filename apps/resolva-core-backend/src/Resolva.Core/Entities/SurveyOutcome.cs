namespace Resolva.Core.Entities;

public class SurveyOutcome : ITenantScoped
{
    public Guid SessionId { get; set; }           // PK = SessionId
    public Guid TenantId { get; set; }

    public string ConfirmationStatus { get; set; } = "partial"; // confirmed, not_confirmed, partial
    public decimal? SatisfactionScore { get; set; }            // e.g. 4.5
    public string? Sentiment { get; set; }                     // positive/neutral/negative (later)

    public DateTimeOffset ComputedAt { get; set; } = DateTimeOffset.UtcNow;
}
