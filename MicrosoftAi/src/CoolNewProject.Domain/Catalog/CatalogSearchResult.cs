using CoolNewProject.Domain.Catalog.Entities;

namespace CoolNewProject.Domain.Catalog;

public sealed record CatalogSearchResult(long TotalItems, List<CatalogItem> Data);
