namespace CoolNewProject.WebApp.Catalog;

public static class Extensions {
    public static void AddApplicationServices(this IHostApplicationBuilder builder) {
        builder.Services.AddHttpForwarderWithServiceDiscovery();

        // Application services
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();

        // HTTP and GRPC client registrations
        builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new Uri("http://catalog-api"));
    }
}
