using CoolNewProject.Domain.Basket.Entities;
using CoolNewProject.Domain.Catalog.Entities;

namespace CoolNewProject.Domain.Basket;

/// <summary>
/// Singleton basket service, just for demonstration purposes DO NOT USE IN PRODUCTION
/// </summary>
public sealed class BasketService {
    private readonly List<BasketItem> _basketItems;
    private readonly AsyncLock _lock;

    public BasketService() {
        _basketItems = new List<BasketItem>();
        _lock = new AsyncLock();
    }

    public async Task AddAsync(CatalogItem item) {
        using var _ = await _lock.WaitAsync();
        bool found = false;
        foreach (BasketItem existing in _basketItems.Where(existing => existing.ProductId == item.Id)) {
            existing.Quantity += 1;
            found = true;
            break;
        }

        if (!found) {
            _basketItems.Add(new BasketItem() {
                ProductId = item.Id,
                ProductName = item.Name,
                Quantity = 1,
                UnitPrice = item.Price
            });
        }
    }

    public async Task<List<BasketItem>> GetBasketItemsAsync() {
        using var _ = await _lock.WaitAsync();
        return [.._basketItems];
    }
}
