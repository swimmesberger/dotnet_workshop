using CoolNewProject.Domain.Catalog.DataAccess;
using CoolNewProject.Domain.Catalog.Entities;
using CoolNewProject.Domain.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace CoolNewProject.Domain.Catalog;

public sealed class CatalogService {
    private readonly ILogger _logger;
    private readonly CatalogContext _dbContext;
    private readonly CatalogOptions _catalogOptions;
    private readonly CatalogEmbeddingGeneratorService _catalogEmbeddingGenerator;

    public CatalogService(ILogger<CatalogService> logger, CatalogContext dbContext, IOptionsSnapshot<CatalogOptions> catalogOptions,
        CatalogEmbeddingGeneratorService catalogEmbeddingGenerator) {
        _logger = logger;
        _dbContext = dbContext;
        _catalogEmbeddingGenerator = catalogEmbeddingGenerator;
        _catalogOptions = catalogOptions.Value;
    }

    public async Task<List<CatalogItem>> GetItemsByIds(int[] ids) {
        return await _dbContext.CatalogItems
            .Where(item => ids.Contains(item.Id))
            .ToListAsync();
    }

    public async Task<CatalogItem?> GetItemById(int id) {
        return await _dbContext.CatalogItems
            .Include(ci => ci.CatalogBrand)
            .SingleOrDefaultAsync(ci => ci.Id == id);
    }

    public async Task<PaginatedItems<CatalogItem>> GetItemsByName(PaginationRequest paginationRequest, string name) {
        int pageSize = paginationRequest.PageSize;
        int pageIndex = paginationRequest.PageIndex;

        long totalItems = await _dbContext.CatalogItems
            .Where(c => c.Name.StartsWith(name))
            .LongCountAsync();

        List<CatalogItem> itemsOnPage = await _dbContext.CatalogItems
            .Where(c => c.Name.StartsWith(name))
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }

    public async Task<PaginatedItems<CatalogItem>> SearchCatalog(PaginationRequest paginationRequest, int? typeId = null, int? brandId = null,
        string? searchQuery = null) {
        int pageSize = paginationRequest.PageSize;
        int pageIndex = paginationRequest.PageIndex;

        IQueryable<CatalogItem> root = _dbContext.CatalogItems;

        // Create an embedding for the input search
        if (!string.IsNullOrEmpty(searchQuery) && _catalogEmbeddingGenerator.IsEnabled) {
            float semanticSearchMaximumDistance = _catalogOptions.SemanticSearchMaximumDistance;
            _logger.LogInformation("Using semantic vector search with distance {MaximumDistance}", semanticSearchMaximumDistance);
            Vector searchQueryVector = (await _catalogEmbeddingGenerator.GetEmbeddingAsync(searchQuery))!;
            root = root
                // this can be flipped to similarity
                // 1 - c.Embedding!.CosineDistance(searchQueryVector)
                // https://github.com/microsoft/semantic-kernel/blob/2ddb5efd5a31b04ee1142a8c29902e9f4d1eb62f/dotnet/src/Connectors/Connectors.Memory.Postgres/PostgresDbClient.cs#L156C54-L156C106
                .Select(c => new { Item = c, Distance = c.Embedding!.CosineDistance(searchQueryVector) })
                .Where(c => c.Distance < semanticSearchMaximumDistance)
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

    public Task<PaginatedItems<CatalogItem>> SearchCatalog(PaginationRequest paginationRequest, string searchQuery) {
        return SearchCatalog(paginationRequest, null, null, searchQuery);
    }

    public async Task<bool> UpdateItem(
        CatalogItem productToUpdate) {
        CatalogItem? catalogItem =
            await _dbContext.CatalogItems.SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);

        if (catalogItem == null) {
            return false;
        }

        // Update current product
        EntityEntry<CatalogItem> catalogEntry = _dbContext.Entry(catalogItem);
        catalogEntry.CurrentValues.SetValues(productToUpdate);

        catalogItem.Embedding = await _catalogEmbeddingGenerator.GetEmbeddingAsync(catalogItem);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task CreateItem(CatalogItem product) {
        var item = new CatalogItem {
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
        item.Embedding = await _catalogEmbeddingGenerator.GetEmbeddingAsync(item);

        _dbContext.CatalogItems.Add(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteItemById(int id) {
        CatalogItem? item = _dbContext.CatalogItems.SingleOrDefault(x => x.Id == id);

        if (item is null) {
            return false;
        }

        _dbContext.CatalogItems.Remove(item);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
