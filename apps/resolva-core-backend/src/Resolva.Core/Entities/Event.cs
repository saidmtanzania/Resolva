using System.Text.Json;

namespace Resolva.Core.Entities;

public class Event : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string EventType { get; set; } = "";      // support_resolution, product_delivery, ...
    public Guid CustomerId { get; set; }
    public string ContactPhone { get; set; } = "";   // whatsapp destination (E.164-ish)

    public Guid? ProductId { get; set; }
    public Guid? ServiceId { get; set; }

    public string Status { get; set; } = "created";  // created, survey_sent, completed, archived

    // Store extra data like ticketId, issue, deliveryRef, etc.
    public JsonDocument Metadata { get; set; } = JsonDocument.Parse("{}");

    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
