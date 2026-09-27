using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.WebShop;

/// <summary>
/// Default implementation for the WebShop module wrapper.
/// </summary>
public sealed class WebShopClient : IWebShopClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "WebShop";

    /// <inheritdoc />
    public ABACUS_WebShopClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public WebShopClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_WebShopClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListShopperAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/ShopperAccounts", query, cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<T>> ListShopperAccountsAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync<T>(_httpClient, "/ShopperAccounts", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateShopperAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/ShopperAccounts", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetShopperAccountAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(
            _httpClient,
            AbacusODataEntity.IdPath("ShopperAccounts", id),
            query,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateShopperAccountAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/ShopperAccounts", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateShopperAccountAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(
            _httpClient,
            "/ShopperAccounts",
            "ShopperAccount",
            model,
            mapper,
            prefer,
            cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchShopperAccountAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(
            _httpClient,
            AbacusODataEntity.IdPath("ShopperAccounts", id),
            payload,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchShopperAccountAsync<TModel>(
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(
            _httpClient,
            AbacusODataEntity.IdPath("ShopperAccounts", id),
            "ShopperAccount",
            model,
            mapper,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> DeleteShopperAccountAsync(
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.DeleteAsync(
            _httpClient,
            AbacusODataEntity.IdPath("ShopperAccounts", id),
            prefer,
            cancellationToken);
    }
}
