using System.Text.Json;

namespace Resolva.Api.Contracts.Events;

public record EventResponse(
    Guid Id,
    string EventType,
    Guid CustomerId,
    string ContactPhone,
    Guid? ProductId,
    Guid? ServiceId,
    string Status,
    JsonElement Metadata,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt
);
