using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.General;

/// <summary>
/// High-level SDK facade for the General module.
/// </summary>
public interface IGeneralClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_GeneralClient Raw { get; }

    /// <summary>
    /// Executes an OData <c>$batch</c> request (counts as one call against Abacus rate limits).
    /// </summary>
    Task<ODataBatchResponse> PostBatchAsync(
        ODataBatchRequest batch,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists countries (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListCountriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates countries following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateCountriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists currencies (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListCurrenciesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates currencies following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateCurrenciesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists divisions (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListDivisionsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists enterprises (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListEnterprisesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);
}
