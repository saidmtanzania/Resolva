using System.Text.Json;

namespace Resolva.Api.Contracts.Events;

public record CreateEventRequest(
    string EventType,
    Guid CustomerId,
    string ContactPhone,
    Guid? ProductId,
    Guid? ServiceId,
    JsonElement? Metadata,
    DateTimeOffset OccurredAt
);
