using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CoolNewProject.Domain.Basket;
using CoolNewProject.Domain.Catalog;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CoolNewProject.Domain.Chatbot;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed class ChatbotInteractions {
    private readonly ILogger _logger;
    private readonly CatalogService _catalogService;
    private readonly BasketService _basketService;

    public ChatbotInteractions(ILogger<ChatbotInteractions> logger, CatalogService catalogService, BasketService basketService) {
        _logger = logger;
        _catalogService = catalogService;
        _basketService = basketService;
    }

    [KernelFunction, Description("Searches the Northern Mountains catalog for a provided product description")]
    public async Task<string> SearchCatalog([Description("The product description for which to search")] string productDescription) {
        try {
            // get TOP-3 items
            var result = await _catalogService.SearchCatalog(
                skip: 0,
                take: 3,
                searchQuery: productDescription
            );
            return JsonSerializer.Serialize(result.Data.Select(x =>
                new ChatCatalogItemResultDto(x.Id, x.Name, x.Description,
                    x.CatalogType.Type, x.CatalogBrand.Brand, x.Price)));
        } catch (HttpRequestException e) {
            return Error(e, "Error accessing catalog.");
        }
    }

    [KernelFunction, Description("Adds a product to the user's shopping cart.")]
    public async Task<string> AddToCart([Description("The id of the product to add to the shopping cart (basket)")] int itemId) {
        try {
            var item = await _catalogService.GetItemById(itemId);
            await _basketService.AddAsync(item!);
            return "Item added to shopping cart.";
        } catch (Exception e) {
            return Error(e, "Unable to add the item to the cart.");
        }
    }

    [KernelFunction, Description("Gets information about the contents of the user's shopping cart (basket)")]
    public async Task<string> GetCartContents() {
        try {
            var basketItems = await _basketService.GetBasketItemsAsync();
            return JsonSerializer.Serialize(basketItems);
        } catch (Exception e) {
            return Error(e, "Unable to get the cart's contents.");
        }
    }

    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    [SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    private string Error(Exception e, string message) {
        if (_logger.IsEnabled(LogLevel.Error)) {
            _logger.LogError(e, message);
        }
        return message;
    }

    // reduce result size to only include data that is interesting for the LLM
    private sealed record ChatCatalogItemResultDto(int ItemId, string Name, string Description, string Type, string Brand, decimal Price);
}
