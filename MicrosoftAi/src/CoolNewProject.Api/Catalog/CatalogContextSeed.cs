using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CoolNewProject.Domain.Catalog;
using CoolNewProject.Domain.Catalog.DataAccess;
using CoolNewProject.Domain.Catalog.Entities;
using CoolNewProject.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CoolNewProject.Api.Catalog;

public sealed class CatalogContextSeed(
    IHostEnvironment env,
    CatalogAi catalogAi,
    ILogger<CatalogContextSeed> logger) : IDbSeeder<CatalogContext> {
    [SuppressMessage("ReSharper", "EntityFramework.NPlusOne.IncompleteDataUsage")]
    [SuppressMessage("ReSharper", "EntityFramework.NPlusOne.IncompleteDataQuery")]
    [SuppressMessage("AOT", "IL3050:Calling members annotated with \'RequiresDynamicCodeAttribute\' may break functionality when AOT compiling.")]
    [SuppressMessage("Trimming", "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
    public async Task SeedAsync(CatalogContext context) {
        string contentRootPath = env.ContentRootPath;

        // Workaround from https://github.com/npgsql/efcore.pg/issues/292#issuecomment-388608426
        await context.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)context.Database.GetDbConnection()).ReloadTypesAsync();

        if (!context.CatalogItems.Any()) {
            string sourcePath = Path.Combine(contentRootPath, "Catalog", "Setup", "catalog.json");
            string sourceJson = await File.ReadAllTextAsync(sourcePath);
            CatalogSourceEntry[] sourceItems = JsonSerializer.Deserialize<CatalogSourceEntry[]>(sourceJson) ?? [];

            context.CatalogBrands.RemoveRange(context.CatalogBrands);
            await context.CatalogBrands.AddRangeAsync(sourceItems.Select(x => x.Brand).Distinct()
                .Select(brandName => new CatalogBrand { Brand = brandName }));
            logger.LogInformation("Seeded catalog with {NumBrands} brands", context.CatalogBrands.Count());

            context.CatalogTypes.RemoveRange(context.CatalogTypes);
            await context.CatalogTypes.AddRangeAsync(sourceItems.Select(x => x.Type).Distinct()
                .Select(typeName => new CatalogType { Type = typeName }));
            logger.LogInformation("Seeded catalog with {NumTypes} types", context.CatalogTypes.Count());

            await context.SaveChangesAsync();

            Dictionary<string, int> brandIdsByName =
                await context.CatalogBrands.ToDictionaryAsync(x => x.Brand, x => x.Id);
            Dictionary<string, int> typeIdsByName = await context.CatalogTypes.ToDictionaryAsync(x => x.Type, x => x.Id);

            var entities = new List<CatalogItem>(sourceItems.Length);
            foreach (CatalogSourceEntry source in sourceItems) {
                var entity = new CatalogItem {
                    Id = source.Id,
                    Name = source.Name,
                    Description = source.Description,
                    Price = source.Price,
                    CatalogBrandId = brandIdsByName[source.Brand],
                    CatalogTypeId = typeIdsByName[source.Type],
                    AvailableStock = 100,
                    MaxStockThreshold = 200,
                    RestockThreshold = 10,
                    PictureFileName = $"{source.Id}.webp",
                    Embedding = null
                };
                if (catalogAi.IsEnabled) {
                    logger.LogInformation("Creating embedding for catalog item {ItemId} ({ItemName})", source.Id,
                        source.Name);
                    entity.Embedding = await catalogAi.GetEmbeddingAsync(entity);
                }
                entities.Add(entity);
            }

            await context.CatalogItems.AddRangeAsync(entities);

            logger.LogInformation("Seeded catalog with {NumItems} items", context.CatalogItems.Count());
            await context.SaveChangesAsync();
        }
    }
}
