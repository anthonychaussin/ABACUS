using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.ProductionPlanning;

/// <summary>
/// High-level SDK facade for the ProductionPlanning module.
/// </summary>
public interface IProductionPlanningClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_ProductionPlanningClient Raw { get; }

    /// <summary>Lists resources (first page).</summary>
    Task<ODataPage<JsonElement>> ListResourcesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Enumerates resources following <c>@odata.nextLink</c>.</summary>
    IAsyncEnumerable<JsonElement> EnumerateResourcesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a resource by id.</summary>
    Task<AbacusResponse<JsonElement>> GetResourceAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists production order headers (first page).</summary>
    Task<ODataPage<JsonElement>> ListProductionOrderHeadersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a production order header by id.</summary>
    Task<AbacusResponse<JsonElement>> GetProductionOrderHeaderAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists production import data (first page).</summary>
    Task<ODataPage<JsonElement>> ListProductionImportDatasAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates production import data.</summary>
    Task<AbacusResponse<string>> CreateProductionImportDataAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
