namespace Resolva.Api.Contracts.Catalog;

public record CatalogItemResponse(
    Guid Id,
    string Name,
    string? Category,
    bool IsActive,
    DateTimeOffset CreatedAt
);
