using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.Finance;

/// <summary>
/// High-level SDK facade for the Finance module.
/// </summary>
public interface IFinanceClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_FinanceClient Raw { get; }

    /// <summary>
    /// Lists accounts (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all accounts following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists general ledger entries (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListGeneralLedgerEntriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates general ledger entries following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateGeneralLedgerEntriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an account from a JSON-compatible payload.
    /// </summary>
    Task<AbacusResponse<string>> CreateAccountAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
