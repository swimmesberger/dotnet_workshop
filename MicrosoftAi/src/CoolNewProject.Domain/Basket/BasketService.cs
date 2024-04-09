using CoolNewProject.Domain.Basket.Entities;
using CoolNewProject.Domain.Catalog.Entities;

namespace CoolNewProject.Domain.Basket;

/// <summary>
/// Singleton basket service, just for demonstration purposes DO NOT USE IN PRODUCTION
/// </summary>
public sealed class BasketService {
    private readonly List<BasketItem> _basketItems;
    private readonly AsyncLock _lock;
    private int _curId;

    public BasketService() {
        _basketItems = new List<BasketItem>();
        _lock = new AsyncLock();
        _curId = 1;
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
                Id = _curId++,
                ProductId = item.Id,
                ProductName = item.Name,
                UnitPrice = item.Price
            });
        }
    }

    public async Task<List<BasketItem>> GetBasketItemsAsync() {
        using var _ = await _lock.WaitAsync();
        return [.._basketItems];
    }

    public async Task<BasketItem?> SetQuantityAsync(int productId, int quantity) {
        using var _ = await _lock.WaitAsync();
        var basketItem = _basketItems.FirstOrDefault(x => x.ProductId == productId);
        if (basketItem is null) return null;
        if (quantity > 0) {
            basketItem.Quantity = quantity;
        } else {
            _basketItems.Remove(basketItem);
        }
        return basketItem;
    }
}
