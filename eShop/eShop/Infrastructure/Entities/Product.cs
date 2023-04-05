namespace eShop.Infrastructure.Entities;

public record Product {
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}