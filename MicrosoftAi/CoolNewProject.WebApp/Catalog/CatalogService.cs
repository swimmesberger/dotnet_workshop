using System.Web;

namespace CoolNewProject.WebApp.Catalog;

public class CatalogService(HttpClient httpClient) {
    private readonly string remoteServiceBaseUrl = "api/v1/catalog/";

    public Task<CatalogItem?> GetCatalogItem(int id) {
        string? uri = $"{remoteServiceBaseUrl}items/{id}";
        return httpClient.GetFromJsonAsync<CatalogItem>(uri);
    }

    public async Task<CatalogResult> GetCatalogItems(int pageIndex, int pageSize, int? brand, int? type) {
        string? uri = GetAllCatalogItemsUri(remoteServiceBaseUrl, pageIndex, pageSize, brand, type);
        CatalogResult? result = await httpClient.GetFromJsonAsync<CatalogResult>(uri);
        return result!;
    }

    public async Task<List<CatalogItem>> GetCatalogItems(IEnumerable<int> ids) {
        string? uri = $"{remoteServiceBaseUrl}items/by?ids={string.Join("&ids=", ids)}";
        List<CatalogItem>? result = await httpClient.GetFromJsonAsync<List<CatalogItem>>(uri);
        return result!;
    }

    public async Task<CatalogResult> GetCatalogItemsWithSemanticRelevance(int page, int take, string text) {
        string url =
            $"{remoteServiceBaseUrl}items/withsemanticrelevance/{HttpUtility.UrlEncode(text)}?pageIndex={page}&pageSize={take}";
        CatalogResult? result = await httpClient.GetFromJsonAsync<CatalogResult>(url);
        return result!;
    }

    public async Task<IEnumerable<CatalogBrand>> GetBrands() {
        string? uri = $"{remoteServiceBaseUrl}catalogBrands";
        CatalogBrand[]? result = await httpClient.GetFromJsonAsync<CatalogBrand[]>(uri);
        return result!;
    }

    public async Task<IEnumerable<CatalogItemType>> GetTypes() {
        string? uri = $"{remoteServiceBaseUrl}catalogTypes";
        CatalogItemType[]? result = await httpClient.GetFromJsonAsync<CatalogItemType[]>(uri);
        return result!;
    }

    private static string GetAllCatalogItemsUri(string baseUri, int pageIndex, int pageSize, int? brand, int? type) {
        string filterQs;

        if (type.HasValue) {
            string? brandQs = brand.HasValue ? brand.Value.ToString() : string.Empty;
            filterQs = $"/type/{type.Value}/brand/{brandQs}";
        } else if (brand.HasValue) {
            string? brandQs = brand.HasValue ? brand.Value.ToString() : string.Empty;
            filterQs = $"/type/all/brand/{brandQs}";
        } else {
            filterQs = string.Empty;
        }

        return $"{baseUri}items{filterQs}?pageIndex={pageIndex}&pageSize={pageSize}";
    }
}
