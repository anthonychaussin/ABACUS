using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.ProductionPlanning;

/// <summary>
/// Default implementation for the ProductionPlanning module wrapper.
/// </summary>
public sealed class ProductionPlanningClient : IProductionPlanningClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "ProductionPlanning";

    /// <inheritdoc />
    public ABACUS_ProductionPlanningClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public ProductionPlanningClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_ProductionPlanningClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListResourcesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/Resources", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateResourcesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/Resources", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetResourceAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, AbacusODataEntity.ParenIdPath("Resources", id), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListProductionOrderHeadersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/ProductionOrderHeaders", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetProductionOrderHeaderAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("ProductionOrderHeaders", id),
            query,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListProductionImportDatasAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/ProductionImportDatas", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateProductionImportDataAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/ProductionImportDatas", payload, prefer, cancellationToken);
}
