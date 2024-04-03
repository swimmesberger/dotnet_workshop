namespace CoolNewProject.WebApp.Catalog;

public class ProductImageUrlProvider : IProductImageUrlProvider {
    public string GetProductImageUrl(int productId)
        => $"product-images/{productId}";
}
