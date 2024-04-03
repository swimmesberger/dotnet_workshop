namespace CoolNewProject.WebApp.Catalog;

public interface IProductImageUrlProvider {
    string GetProductImageUrl(CatalogItem item)
        => GetProductImageUrl(item.Id);

    string GetProductImageUrl(int productId);
}
