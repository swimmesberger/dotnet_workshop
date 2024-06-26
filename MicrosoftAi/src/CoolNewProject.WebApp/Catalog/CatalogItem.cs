﻿namespace CoolNewProject.WebApp.Catalog;

public sealed record CatalogItem(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string PictureUri,
    int CatalogBrandId,
    CatalogBrand CatalogBrand,
    int CatalogTypeId,
    CatalogItemType CatalogType);

public sealed record CatalogResult(int PageIndex, int PageSize, int Count, List<CatalogItem> Data);

public sealed record CatalogBrand(int Id, string Brand);

public sealed record CatalogItemType(int Id, string Type);
