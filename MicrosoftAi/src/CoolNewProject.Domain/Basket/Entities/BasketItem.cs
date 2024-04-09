namespace CoolNewProject.Domain.Basket.Entities;

public sealed class BasketItem {
    public required int Id { get; set; }
    public required int ProductId { get; set; }
    public required string ProductName { get; set; }
    public required decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
}
