using CoolNewProject.Api.Catalog.Models;
using CoolNewProject.Domain.Catalog;
using CoolNewProject.Domain.Catalog.DataAccess;
using CoolNewProject.Domain.Catalog.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

        // Routes for resolving catalog items by type and brand.
        app.MapGet("/items/type/{typeId:int}/brand/{brandId:int?}", GetItemsByBrandAndTypeId);
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
        [AsParameters] CatalogServices services,
        [FromQuery(Name = "q")] string? searchQuery) {
        return TypedResults.Ok(await SearchCatalog(paginationRequest, services, null, null, searchQuery));
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

    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByBrandAndTypeId(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        [FromRoute] int typeId,
        [FromRoute] int? brandId,
        [FromQuery(Name = "q")] string? searchQuery) {
        return TypedResults.Ok(await SearchCatalog(paginationRequest, services, typeId, brandId, searchQuery));
    }

    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByBrandId(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services,
        [FromRoute] int? brandId,
        [FromQuery(Name = "q")] string? searchQuery) {
        return TypedResults.Ok(await SearchCatalog(paginationRequest, services, null, brandId, searchQuery));
    }

    private static async Task<PaginatedItems<CatalogItem>> SearchCatalog(PaginationRequest paginationRequest, CatalogServices services,
        int? typeId = null, int? brandId = null, string? searchQuery = null) {
        int pageSize = paginationRequest.PageSize;
        int pageIndex = paginationRequest.PageIndex;

        IQueryable<CatalogItem> root = services.Context.CatalogItems;

        // Create an embedding for the input search
        if (!string.IsNullOrEmpty(searchQuery) && services.CatalogAi.IsEnabled) {
            Vector searchQueryVector = (await services.CatalogAi.GetEmbeddingAsync(searchQuery))!;
            root = root
                .Select(c => new { Item = c, Distance = c.Embedding!.CosineDistance(searchQueryVector) })
                .Where(c => c.Distance < CatalogConstants.SemanticSearchMaximumDistance)
                .OrderBy(c => c.Distance)
                .Select(c => c.Item);
        }

        if (typeId is not null) {
            root = root.Where(c => c.CatalogTypeId == typeId);
        }
        if (brandId is not null) {
            root = root.Where(c => c.CatalogBrandId == brandId);
        }

        long totalItems = await root
            .LongCountAsync();


        List<CatalogItem> itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
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

        catalogItem.Embedding = await services.CatalogAi.GetEmbeddingAsync(catalogItem);

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
        item.Embedding = await services.CatalogAi.GetEmbeddingAsync(item);

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
