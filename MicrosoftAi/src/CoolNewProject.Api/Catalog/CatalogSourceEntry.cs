namespace CoolNewProject.Api.Catalog;

internal sealed record CatalogSourceEntry {
    public int Id { get; init; }
    public string Type { get; init; }
    public string Brand { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
    public float[]? Embedding { get; init; }
}
