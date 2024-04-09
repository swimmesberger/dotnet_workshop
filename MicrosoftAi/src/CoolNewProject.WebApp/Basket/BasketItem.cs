namespace CoolNewProject.WebApp.Basket;

public sealed record BasketItem(
    int Id,
    int ProductId,
    int Quantity,
    string ProductName,
    decimal UnitPrice
);


public sealed record SetQuantityRequest(int ProductId, int Quantity);
