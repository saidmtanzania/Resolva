using System.Text.Json;

namespace Resolva.Api.Contracts.Events;

public record UpdateEventRequest(
    string? Status,
    JsonElement? Metadata
);