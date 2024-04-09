using CoolNewProject.Domain.Basket;
using CoolNewProject.Domain.Basket.Entities;
using CoolNewProject.Domain.Catalog.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoolNewProject.Api.Basket;

public static class BasketApi {
    public static IEndpointRouteBuilder MapBasketApi(this IEndpointRouteBuilder app) {
        app.MapGet("/items", GetAllItems);
        //app.MapGet("/items/{id:int}", GetItemById);
        app.MapPost("/items", CreateItem);
        app.MapPut("/items", UpdateItem);
        return app;
    }

    private static async Task<Results<Ok<List<BasketItem>>, BadRequest<string>>> GetAllItems([FromServices] BasketService basketService) {
        return TypedResults.Ok(await basketService.GetBasketItemsAsync());
    }

    private static async Task<Created> CreateItem([FromServices] BasketService basketService, CatalogItem product) {
        await basketService.AddAsync(product);
        return TypedResults.Created($"/api/v1/basket/items/{product.Id}");
    }

    private static async Task<Results<Created, NotFound<string>>> UpdateItem([FromServices] BasketService basketService, SetQuantityRequest request) {
        var item = await basketService.SetQuantityAsync(request.ProductId, request.Quantity);
        if (item == null) return TypedResults.NotFound($"Item with product id {request.ProductId} not found.");
        return TypedResults.Created($"/api/v1/basket/items/{item.Id}");
    }
}
