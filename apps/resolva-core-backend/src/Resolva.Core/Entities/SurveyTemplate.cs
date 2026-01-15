using System.Text.Json;

namespace Resolva.Core.Entities;

public class SurveyTemplate : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string EventType { get; set; } = "";     // support_resolution, product_delivery...
    public string Language { get; set; } = "en";    // en, sw
    public int Version { get; set; } = 1;

    public string Status { get; set; } = "draft"; // draft, pending, published, archived

    public JsonDocument SchemaJson { get; set; } = JsonDocument.Parse("{}");

    public string CreatedBy { get; set; } = "ai";   // ai or user id/email
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
