namespace CoolNewProject.WebApp.Catalog;

public static class CatalogExtensions {
    public static void AddCatalogServices(this IHostApplicationBuilder builder) {
        // Application services
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();

        // HTTP client registrations
        builder.Services.AddHttpClient<CatalogService>(o => {
            o.BaseAddress = new Uri("http://catalog-api");
            o.Timeout = TimeSpan.FromMinutes(5);
        });
    }
}
