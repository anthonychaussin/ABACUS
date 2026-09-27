using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.AssetsLedger;

/// <summary>
/// High-level SDK facade for the AssetsLedger module.
/// </summary>
public interface IAssetsLedgerClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_AssetsLedgerClient Raw { get; }

    /// <summary>
    /// Lists assets (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListAssetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all assets following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateAssetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an asset by id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetAssetAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches an asset.
    /// </summary>
    Task<AbacusResponse<string>> PatchAssetAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists asset categories (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListAssetCategoriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);
}
