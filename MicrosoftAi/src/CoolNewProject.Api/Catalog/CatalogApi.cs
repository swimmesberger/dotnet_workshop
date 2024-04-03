using CoolNewProject.Domain.Catalog;
using CoolNewProject.Domain.Catalog.DataAccess;
using CoolNewProject.Domain.Catalog.Entities;
using CoolNewProject.Domain.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    private static async Task<Results<Ok<PaginatedItems<CatalogItem>>, BadRequest<string>>> GetAllItems(
        [AsParameters] PaginationRequest paginationRequest,
        [FromServices] CatalogService catalogService,
        [FromQuery(Name = "q")] string? searchQuery) {
        return TypedResults.Ok(await catalogService.SearchCatalog(paginationRequest, null, null, searchQuery));
    }

    private static async Task<Ok<List<CatalogItem>>> GetItemsByIds([FromServices] CatalogService catalogService, int[] ids) {
        return TypedResults.Ok(await catalogService.GetItemsByIds(ids));
    }

    private static async Task<Results<Ok<CatalogItem>, NotFound, BadRequest<string>>> GetItemById([FromServices] CatalogService catalogService, int id) {
        if (id <= 0) {
            return TypedResults.BadRequest("Id is not valid.");
        }

        CatalogItem? item = await catalogService.GetItemById(id);
        if (item == null) {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(item);
    }

    private static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByName([AsParameters] PaginationRequest paginationRequest,
        [FromServices] CatalogService catalogService, string name) {
        return TypedResults.Ok(await catalogService.GetItemsByName(paginationRequest, name));
    }

    private static async Task<Results<NotFound, PhysicalFileHttpResult>> GetItemPictureById(CatalogContext context,
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

    private static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByBrandAndTypeId(
        [AsParameters] PaginationRequest paginationRequest,
        [FromServices] CatalogService catalogService,
        [FromRoute] int typeId,
        [FromRoute] int? brandId,
        [FromQuery(Name = "q")] string? searchQuery) {
        return TypedResults.Ok(await catalogService.SearchCatalog(paginationRequest, typeId, brandId, searchQuery));
    }

    private static async Task<Ok<PaginatedItems<CatalogItem>>> GetItemsByBrandId(
        [AsParameters] PaginationRequest paginationRequest,
        [FromServices] CatalogService catalogService,
        [FromRoute] int? brandId,
        [FromQuery(Name = "q")] string? searchQuery) {
        return TypedResults.Ok(await catalogService.SearchCatalog(paginationRequest, null, brandId, searchQuery));
    }

    private static async Task<Results<Created, NotFound<string>>> UpdateItem([FromServices] CatalogService catalogService, CatalogItem productToUpdate) {
        if (!await catalogService.UpdateItem(productToUpdate)) {
            return TypedResults.NotFound($"Item with id {productToUpdate.Id} not found.");
        }
        return TypedResults.Created($"/api/v1/catalog/items/{productToUpdate.Id}");
    }

    private static async Task<Created> CreateItem([FromServices] CatalogService catalogService, CatalogItem product) {
        await catalogService.CreateItem(product);
        return TypedResults.Created($"/api/v1/catalog/items/{product.Id}");
    }

    private static async Task<Results<NoContent, NotFound>> DeleteItemById([FromServices] CatalogService catalogService, int id) {
        if (!await catalogService.DeleteItemById(id)) {
            return TypedResults.NotFound();
        }
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

    private static string GetFullPath(string contentRootPath, string pictureFileName) =>
        Path.Combine(contentRootPath, "Catalog", "Pics", pictureFileName);
}
