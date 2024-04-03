using CoolNewProject.Domain.Basket;

namespace CoolNewProject.Api.Basket;

public static class BasketExtensions {
    public static void AddBasketServices(this IHostApplicationBuilder builder) {
        // singleton basket service for development purposes - in production a database with user specific data should be used
        builder.Services.AddSingleton<BasketService>();
    }
}
