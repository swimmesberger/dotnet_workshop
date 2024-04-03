using CoolNewProject.Domain.Catalog;
using CoolNewProject.Domain.Catalog.DataAccess;
using CoolNewProject.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

namespace CoolNewProject.Api.Catalog;

public static class CatalogExtensions {
    public static void AddCatalogServices(this IHostApplicationBuilder builder) {
        builder.AddNpgsqlDbContext<CatalogContext>("catalogdb",
            configureDbContextOptions: dbContextOptionsBuilder => {
                dbContextOptionsBuilder.UseNpgsql(x => x.UseVector());
            });

        // REVIEW: This is done for development ease but shouldn't be here in production
        builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

        builder.Services.AddOptions<CatalogOptions>()
            .BindConfiguration(nameof(CatalogOptions));

        builder.Services.AddSingleton<CatalogEmbeddingGeneratorService>();
        builder.Services.AddScoped<CatalogService>();
    }
}
