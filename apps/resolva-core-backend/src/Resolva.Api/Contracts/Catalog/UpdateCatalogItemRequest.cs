namespace Resolva.Api.Contracts.Catalog;

public record UpdateCatalogItemRequest(string? Name, string? Category, bool? IsActive);
