using System.Web;

namespace CoolNewProject.WebApp.Catalog;

public sealed class CatalogService(HttpClient httpClient) {
    private const string RemoteServiceBaseUrl = "api/v1/catalog/";

    public Task<CatalogItem?> GetCatalogItem(int id) {
        string uri = $"{RemoteServiceBaseUrl}items/{id}";
        return httpClient.GetFromJsonAsync<CatalogItem>(uri);
    }

    public async Task<CatalogResult> GetCatalogItems(int pageIndex, int pageSize, int? brand, int? type, string? searchText, CancellationToken cancellationToken = default) {
        string uri = GetAllCatalogItemsUri(RemoteServiceBaseUrl, pageIndex, pageSize, brand, type, searchText);
        CatalogResult? result = await httpClient.GetFromJsonAsync<CatalogResult>(uri, cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<List<CatalogItem>> GetCatalogItems(IEnumerable<int> ids) {
        string? uri = $"{RemoteServiceBaseUrl}items/by?ids={string.Join("&ids=", ids)}";
        List<CatalogItem>? result = await httpClient.GetFromJsonAsync<List<CatalogItem>>(uri);
        return result!;
    }

    public async Task<CatalogResult> GetCatalogItemsWithSemanticRelevance(int page, int take, string text) {
        string url =
            $"{RemoteServiceBaseUrl}items/withsemanticrelevance/{HttpUtility.UrlEncode(text)}?pageIndex={page}&pageSize={take}";
        CatalogResult? result = await httpClient.GetFromJsonAsync<CatalogResult>(url);
        return result!;
    }

    public async Task<IEnumerable<CatalogBrand>> GetBrands() {
        string? uri = $"{RemoteServiceBaseUrl}catalogBrands";
        CatalogBrand[]? result = await httpClient.GetFromJsonAsync<CatalogBrand[]>(uri);
        return result!;
    }

    public async Task<IEnumerable<CatalogItemType>> GetTypes() {
        string? uri = $"{RemoteServiceBaseUrl}catalogTypes";
        CatalogItemType[]? result = await httpClient.GetFromJsonAsync<CatalogItemType[]>(uri);
        return result!;
    }

    private static string GetAllCatalogItemsUri(string baseUri, int pageIndex, int pageSize, int? brand, int? type, string? searchText) {
        searchText ??= "";

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

        return $"{baseUri}items{filterQs}?pageIndex={pageIndex}&pageSize={pageSize}&q={searchText}";
    }
}
