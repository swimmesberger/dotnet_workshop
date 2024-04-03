using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CoolNewProject.Domain.DataAccess;
using CoolNewProject.Domain.Entities;
using CoolNewProject.Domain.Services;
using CoolNewProject.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CoolNewProject.Api.Catalog;

public sealed class CatalogContextSeed(
    IHostEnvironment env,
    ICatalogAI catalogAi,
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

            await context.CatalogItems.AddRangeAsync(sourceItems.Select(source => new CatalogItem {
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
                Embedding = catalogAi.IsEnabled ? new Pgvector.Vector(source.Embedding) : null
            }));

            logger.LogInformation("Seeded catalog with {NumItems} items", context.CatalogItems.Count());
            await context.SaveChangesAsync();

            if (catalogAi.IsEnabled) {
                bool anyChanged = false;
                foreach (CatalogItem item in context.CatalogItems) {
                    if (item.Embedding is not null) {
                        continue;
                    }
                    logger.LogInformation("Creating embedding for catalog item {ItemId} ({ItemName})", item.Id,
                        item.Name);
                    item.Embedding = await catalogAi.GetEmbeddingAsync(item);
                    anyChanged = true;
                }

                if (anyChanged) {
                    await context.SaveChangesAsync();
                }
            }
        }
    }

    private class CatalogSourceEntry {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public float[] Embedding { get; set; }
    }
}
