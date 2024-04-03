﻿using CoolNewProject.Domain.DataAccess;
using CoolNewProject.Domain.Services;
using Microsoft.Extensions.Options;

namespace CoolNewProject.Api.Catalog.Models;

public class CatalogServices(
    CatalogContext context,
    ICatalogAI catalogAI,
    IOptions<CatalogOptions> options,
    ILogger<CatalogServices> logger) {
    public CatalogContext Context { get; } = context;
    public ICatalogAI CatalogAI { get; } = catalogAI;
    public IOptions<CatalogOptions> Options { get; } = options;
    public ILogger<CatalogServices> Logger { get; } = logger;
};
