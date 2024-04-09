using CoolNewProject.WebApp.Catalog;

namespace CoolNewProject.WebApp.Basket;

public sealed class BasketService(HttpClient httpClient) {
    private const string RemoteServiceBaseUrl = "api/v1/basket/";

    public async Task AddAsync(CatalogItem item, CancellationToken cancellationToken = default) {
        await httpClient.PostAsJsonAsync($"{RemoteServiceBaseUrl}items", item, cancellationToken: cancellationToken);
    }

    public async Task<List<BasketItem>> GetBasketItemsAsync(CancellationToken cancellationToken = default) {
        return (await httpClient.GetFromJsonAsync<List<BasketItem>>($"{RemoteServiceBaseUrl}items", cancellationToken: cancellationToken))!;
    }

    public async Task SetQuantityAsync(int productId, int quantity, CancellationToken cancellationToken = default) {
        await httpClient.PutAsJsonAsync($"{RemoteServiceBaseUrl}items",
            new SetQuantityRequest(productId, quantity), cancellationToken: cancellationToken);
    }
}
