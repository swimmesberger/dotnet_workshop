namespace CoolNewProject.WebApp.Catalog;

public static class ItemHelper {
    public static string Url(CatalogItem item)
        => $"item/{item.Id}";
}
