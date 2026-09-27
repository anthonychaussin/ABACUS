using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.AssetsLedger;

/// <summary>
/// Default implementation for the AssetsLedger module wrapper.
/// </summary>
public sealed class AssetsLedgerClient : IAssetsLedgerClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "AssetsLedger";

    /// <inheritdoc />
    public ABACUS_AssetsLedgerClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public AssetsLedgerClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_AssetsLedgerClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListAssetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/Assets", query, cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<T>> ListAssetsAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync<T>(_httpClient, "/Assets", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateAssetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/Assets", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetAssetAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, AbacusODataEntity.ParenIdPath("Assets", id), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateAssetAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/Assets", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateAssetAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/Assets", "Asset", model, mapper, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchAssetAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("Assets", id),
            payload,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchAssetAsync<TModel>(
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("Assets", id),
            "Asset",
            model,
            mapper,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListAssetCategoriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/AssetCategories", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetAssetCategoryAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("AssetCategories", id),
            query,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateAssetCategoryAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/AssetCategories", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchAssetCategoryAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("AssetCategories", id),
            payload,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListAssetBookingsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/AssetBookings", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetAssetBookingAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("AssetBookings", id),
            query,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateAssetBookingAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/AssetBookings", payload, prefer, cancellationToken);
}
