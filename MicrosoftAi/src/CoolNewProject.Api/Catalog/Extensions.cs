using CoolNewProject.Domain.Catalog;
using CoolNewProject.Domain.Catalog.DataAccess;
using CoolNewProject.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace CoolNewProject.Api.Catalog;

public static class CatalogExtensions {
    public static void AddApplicationServices(this IHostApplicationBuilder builder) {
        builder.AddNpgsqlDbContext<CatalogContext>("catalogdb",
            configureDbContextOptions: dbContextOptionsBuilder => {
                dbContextOptionsBuilder.UseNpgsql(x => x.UseVector());
            });

        // REVIEW: This is done for development ease but shouldn't be here in production
        builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

        builder.Services.AddOptions<CatalogOptions>()
            .BindConfiguration(nameof(CatalogOptions));

        builder.Services.AddSingleton<CatalogAi>();
        builder.AddAiServices();
    }

    private static void AddAiServices(this IHostApplicationBuilder builder) {
        builder.Services
            .AddKernel()
            .AddLocalTextEmbeddingGeneration();
    }
}
