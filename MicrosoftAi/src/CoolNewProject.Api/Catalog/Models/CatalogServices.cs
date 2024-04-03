using CoolNewProject.Domain.Catalog;
using CoolNewProject.Domain.Catalog.DataAccess;
using Microsoft.Extensions.Options;

namespace CoolNewProject.Api.Catalog.Models;

public class CatalogServices(
    CatalogContext context,
    CatalogAi catalogAi,
    IOptions<CatalogOptions> options,
    ILogger<CatalogServices> logger) {
    public CatalogContext Context { get; } = context;
    public CatalogAi CatalogAi { get; } = catalogAi;
    public IOptions<CatalogOptions> Options { get; } = options;
    public ILogger<CatalogServices> Logger { get; } = logger;
};
