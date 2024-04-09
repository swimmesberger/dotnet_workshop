using Microsoft.Extensions.Http.Resilience;

namespace CoolNewProject.WebApp.Basket;

public static class BasketExtensions {
    public static void AddBasketServices(this IHostApplicationBuilder builder) {
        // HTTP client registrations
        var resilienceBuilder = builder.Services.AddHttpClient<BasketService>(o =>
            o.BaseAddress = new Uri("http://catalog-api"))
            .AddStandardResilienceHandler();
        var resilienceOptions = builder.Configuration.GetSection("HttpClientResilience:Basket");
        if (resilienceOptions.GetChildren().Any()) {
            resilienceBuilder.Configure(resilienceOptions);
        }
    }
}
