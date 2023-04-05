namespace eShop.Infrastructure.Entities;


public record Cart {
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public DateTimeOffset LastAccess { get; set; } = DateTimeOffset.UtcNow;
    
    public decimal TotalPrice {
        get {
            return Items.Select(i => i.TotalPrice).Sum();
        }
    }
}

public record CartItem {
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Amount { get; set; }

    public decimal TotalPrice => Product.Price * Amount;
}