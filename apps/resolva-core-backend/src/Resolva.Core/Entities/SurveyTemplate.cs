using System.Text.Json;

namespace Resolva.Core.Entities;

public class SurveyTemplate : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    // existing
    public string EventType { get; set; } = "";
    public string Language { get; set; } = "en";
    public int Version { get; set; } = 1;
    public JsonDocument SchemaJson { get; set; } = JsonDocument.Parse("{}");
    public string CreatedBy { get; set; } = "ai";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // NEW (dashboard + publish pipeline)
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = false;
    public string Channel { get; set; } = "whatsapp_flow"; // future: web, sms

    public string? WhatsAppFlowId { get; set; } // Meta flow id
    public string? WhatsAppStatus { get; set; } // DRAFT, PUBLISHED, ERROR
    public DateTimeOffset? PublishedAt { get; set; }

    public JsonDocument? ValidationErrors { get; set; } // jsonb (Meta validation errors)
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
