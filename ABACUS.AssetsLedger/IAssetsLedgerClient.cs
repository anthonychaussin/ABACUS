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
    /// Lists assets deserialized as <typeparamref name="T"/>.
    /// </summary>
    Task<ODataPage<T>> ListAssetsAsAsync<T>(
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
    /// Creates an asset.
    /// </summary>
    Task<AbacusResponse<string>> CreateAssetAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an asset by mapping a model with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    Task<AbacusResponse<string>> CreateAssetAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
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
    /// Patches an asset by mapping a model with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    Task<AbacusResponse<string>> PatchAssetAsync<TModel>(
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists asset categories (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListAssetCategoriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an asset category by id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetAssetCategoryAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an asset category.
    /// </summary>
    Task<AbacusResponse<string>> CreateAssetCategoryAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches an asset category.
    /// </summary>
    Task<AbacusResponse<string>> PatchAssetCategoryAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists asset bookings (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListAssetBookingsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an asset booking by id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetAssetBookingAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an asset booking.
    /// </summary>
    Task<AbacusResponse<string>> CreateAssetBookingAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
