using CoolNewProject.Api.Catalog.Models;
using CoolNewProject.Domain.DataAccess;
using CoolNewProject.Domain.Entities;
using CoolNewProject.Api.Catalog.Infrastructure;
using eShop.Catalog.API.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace CoolNewProject.Api.Catalog;

public static class CatalogApi {
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder app) {
        // Routes for querying catalog items.
        app.MapGet("/items", GetAllItems);
        app.MapGet("/items/by", GetItemsByIds);
        app.MapGet("/items/{id:int}", GetItemById);
        app.MapGet("/items/by/{name:minlength(1)}", GetItemsByName);
        app.MapGet("/items/{catalogItemId:int}/pic", GetItemPictureById);

        // Routes for resolving catalog items using AI.
        app.MapGet("/items/withsemanticrelevance/{text:minlength(1)}", GetItemsBySemanticRelevance);

        // Routes for resolving catalog items by type and brand.
        app.MapGet("/items/type/{typeId}/brand/{brandId?}", GetItemsByBrandAndTypeId);
        app.MapGet("/items/type/all/brand/{brandId:int?}", GetItemsByBrandId);
        app.MapGet("/catalogtypes",
            async (CatalogContext context) => await context.CatalogTypes.OrderBy(x => x.Type).ToListAsync());
        app.MapGet("/catalogbrands",
            async (CatalogContext context) => await context.CatalogBrands.OrderBy(x => x.Brand).ToListAsync());

        // Routes for modifying catalog items.
        app.MapPut("/items", UpdateItem);
        app.MapPost("/items", CreateItem);
        app.MapDelete("/items/{id:int}", DeleteItemById);

        return app;
    }

    public static async Task<Results<Ok<PaginatedItems<CatalogItem>>, BadRequest<string>>> GetAllItems(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services) {
        int pageSize = paginationRequest.PageSize;
        int pageIndex = paginationRequest.PageIndex;

        long totalItems = await services.Context.CatalogItems
            .LongCountAsync();

        List<CatalogItem> itemsOnPage = await services.Context.CatalogItems
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    public static async Task<Ok<List<CatalogItem>>> GetItemsByIds(
        [AsParameters] CatalogServices services,
        int[] ids) {
        List<CatalogItem> items = await services.Context.CatalogItems.Where(item => ids.Contains(item.Id)).ToListAsync();
        return TypedResults.Ok(items);
    }

    public static async Task<Results<Ok<CatalogItem>, NotFound, BadRequest<string>>> GetItemById(
        [AsParameters] CatalogServices services,
        int id) {
        if (id <= 0) {
            return TypedResults.BadRequest("Id is not valid.");
        }

        CatalogItem? item = await services.Context.CatalogItems.Include(ci => ci.CatalogBrand)
            .SingleOrDefaultAsync(ci => ci.Id == id);

        if (item == null) {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(item);
    }

    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByName(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        string name) {
        int pageSize = paginationRequest.PageSize;
        int pageIndex = paginationRequest.PageIndex;

        long totalItems = await services.Context.CatalogItems
            .Where(c => c.Name.StartsWith(name))
            .LongCountAsync();

        List<CatalogItem> itemsOnPage = await services.Context.CatalogItems
            .Where(c => c.Name.StartsWith(name))
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    public static async Task<Results<NotFound, PhysicalFileHttpResult>> GetItemPictureById(CatalogContext context,
        IWebHostEnvironment environment, int catalogItemId) {
        CatalogItem? item = await context.CatalogItems.FindAsync(catalogItemId);

        if (item is null) {
            return TypedResults.NotFound();
        }

        string path = GetFullPath(environment.ContentRootPath, item.PictureFileName);

        string imageFileExtension = Path.GetExtension(item.PictureFileName);
        string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);
        DateTime lastModified = File.GetLastWriteTimeUtc(path);

        return TypedResults.PhysicalFile(path, mimetype, lastModified: lastModified);
    }

    public static async Task<Results<BadRequest<string>, RedirectToRouteHttpResult, Ok<PaginatedItems<CatalogItem>>>>
        GetItemsBySemanticRelevance(
            [AsParameters] PaginationRequest paginationRequest,
            [AsParameters] CatalogServices services,
            string text) {
        int pageSize = paginationRequest.PageSize;
        int pageIndex = paginationRequest.PageIndex;

        if (!services.CatalogAI.IsEnabled) {
            return await GetItemsByName(paginationRequest, services, text);
        }

        // Create an embedding for the input search
        Vector vector = await services.CatalogAI.GetEmbeddingAsync(text);

        // Get the total number of items
        long totalItems = await services.Context.CatalogItems
            .LongCountAsync();

        // Get the next page of items, ordered by most similar (smallest distance) to the input search
        List<CatalogItem> itemsOnPage;
        if (services.Logger.IsEnabled(LogLevel.Debug)) {
            var itemsWithDistance = await services.Context.CatalogItems
                .Select(c => new { Item = c, Distance = c.Embedding.CosineDistance(vector) })
                .OrderBy(c => c.Distance)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            services.Logger.LogDebug("Results from {text}: {results}", text,
                string.Join(", ", itemsWithDistance.Select(i => $"{i.Item.Name} => {i.Distance}")));

            itemsOnPage = itemsWithDistance.Select(i => i.Item).ToList();
        } else {
            itemsOnPage = await services.Context.CatalogItems
                .OrderBy(c => c.Embedding.CosineDistance(vector))
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();
        }

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByBrandAndTypeId(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        int typeId,
        int? brandId) {
        int pageSize = paginationRequest.PageSize;
        int pageIndex = paginationRequest.PageIndex;

        IQueryable<CatalogItem> root = (IQueryable<CatalogItem>)services.Context.CatalogItems;
        root = root.Where(c => c.CatalogTypeId == typeId);
        if (brandId is not null) {
            root = root.Where(c => c.CatalogBrandId == brandId);
        }

        long totalItems = await root
            .LongCountAsync();

        List<CatalogItem> itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByBrandId(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        int? brandId) {
        int pageSize = paginationRequest.PageSize;
        int pageIndex = paginationRequest.PageIndex;

        IQueryable<CatalogItem> root = (IQueryable<CatalogItem>)services.Context.CatalogItems;

        if (brandId is not null) {
            root = root.Where(ci => ci.CatalogBrandId == brandId);
        }

        long totalItems = await root
            .LongCountAsync();

        List<CatalogItem> itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    public static async Task<Results<Created, NotFound<string>>> UpdateItem(
        [AsParameters] CatalogServices services,
        CatalogItem productToUpdate) {
        CatalogItem? catalogItem =
            await services.Context.CatalogItems.SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);

        if (catalogItem == null) {
            return TypedResults.NotFound($"Item with id {productToUpdate.Id} not found.");
        }

        // Update current product
        EntityEntry<CatalogItem> catalogEntry = services.Context.Entry(catalogItem);
        catalogEntry.CurrentValues.SetValues(productToUpdate);

        catalogItem.Embedding = await services.CatalogAI.GetEmbeddingAsync(catalogItem);

        await services.Context.SaveChangesAsync();
        return TypedResults.Created($"/api/v1/catalog/items/{productToUpdate.Id}");
    }

    public static async Task<Created> CreateItem(
        [AsParameters] CatalogServices services,
        CatalogItem product) {
        CatalogItem item = new CatalogItem {
            Id = product.Id,
            CatalogBrandId = product.CatalogBrandId,
            CatalogTypeId = product.CatalogTypeId,
            Description = product.Description,
            Name = product.Name,
            PictureFileName = product.PictureFileName,
            PictureUri = product.PictureUri,
            Price = product.Price,
            AvailableStock = product.AvailableStock,
            RestockThreshold = product.RestockThreshold,
            MaxStockThreshold = product.MaxStockThreshold
        };
        item.Embedding = await services.CatalogAI.GetEmbeddingAsync(item);

        services.Context.CatalogItems.Add(item);
        await services.Context.SaveChangesAsync();

        return TypedResults.Created($"/api/v1/catalog/items/{item.Id}");
    }

    public static async Task<Results<NoContent, NotFound>> DeleteItemById(
        [AsParameters] CatalogServices services,
        int id) {
        CatalogItem? item = services.Context.CatalogItems.SingleOrDefault(x => x.Id == id);

        if (item is null) {
            return TypedResults.NotFound();
        }

        services.Context.CatalogItems.Remove(item);
        await services.Context.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static string GetImageMimeTypeFromImageFileExtension(string extension) => extension switch {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".bmp" => "image/bmp",
        ".tiff" => "image/tiff",
        ".wmf" => "image/wmf",
        ".jp2" => "image/jp2",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };

    public static string GetFullPath(string contentRootPath, string pictureFileName) =>
        Path.Combine(contentRootPath, "Catalog", "Pics", pictureFileName);
}
