using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.RealEstate;

/// <summary>
/// High-level SDK facade for the RealEstate module.
/// </summary>
public interface IRealEstateClient : IAbacusModuleClient
{
    /// <summary>
    /// Lists object contracts (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListObjectContractsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists object contracts deserialized as <typeparamref name="T"/>.
    /// </summary>
    Task<ODataPage<T>> ListObjectContractsAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates object contracts following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateObjectContractsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an object contract by id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetObjectContractAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists partial object contracts (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListPartialObjectContractsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists code tables (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListCodeTablesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_RealEstateClient Raw { get; }
}
