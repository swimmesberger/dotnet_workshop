namespace CoolNewProject.Domain.Basket.Entities;

public sealed class BasketItem {
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
