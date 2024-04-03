namespace CoolNewProject.Domain.Catalog;

public sealed class CatalogOptions {
    public string PicBaseUrl { get; set; }
    public bool UseCustomizationData { get; set; }
    public float SemanticSearchMaximumDistance  { get; set; }
}
