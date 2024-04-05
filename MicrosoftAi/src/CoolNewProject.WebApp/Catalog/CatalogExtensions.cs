using Microsoft.Extensions.Http.Resilience;

namespace CoolNewProject.WebApp.Catalog;

public static class CatalogExtensions {
    public static void AddCatalogServices(this IHostApplicationBuilder builder) {
        // Application services
        builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();

        // HTTP client registrations
        var resilienceBuilder = builder.Services.AddHttpClient<CatalogService>(o =>
            o.BaseAddress = new Uri("http://catalog-api"))
            .AddStandardResilienceHandler();
        var resilienceOptions = builder.Configuration.GetSection("HttpClientResilience:Catalog");
        if (resilienceOptions.GetChildren().Any()) {
            resilienceBuilder.Configure(resilienceOptions);
        }
    }
}
