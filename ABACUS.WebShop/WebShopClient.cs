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
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/ShopperAccounts", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateShopperAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(_httpClient, "/ShopperAccounts", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetShopperAccountAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var path = query is null
            ? $"/ShopperAccounts(Id={id})"
            : query.ApplyTo($"/ShopperAccounts(Id={id})");
        return AbacusHttp.SendJsonAsync<JsonElement>(_httpClient, HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateShopperAccountAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(_httpClient, HttpMethod.Post, "/ShopperAccounts", payload, prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchShopperAccountAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Patch,
            $"/ShopperAccounts(Id={id})",
            payload,
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
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Delete,
            $"/ShopperAccounts(Id={id})",
            payload: null,
            prefer,
            cancellationToken);
    }
}
