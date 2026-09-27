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
    /// Lists accounts deserialized as <typeparamref name="T"/>.
    /// </summary>
    Task<ODataPage<T>> ListAccountsAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all accounts following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an account by enterprise id and account id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetAccountAsync(
        string enterpriseId,
        string id,
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

    /// <summary>
    /// Creates an account by mapping a model with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    Task<AbacusResponse<string>> CreateAccountAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches an account.
    /// </summary>
    Task<AbacusResponse<string>> PatchAccountAsync(
        string enterpriseId,
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches an account by mapping a model with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    Task<AbacusResponse<string>> PatchAccountAsync<TModel>(
        string enterpriseId,
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an account.
    /// </summary>
    Task<AbacusResponse<string>> DeleteAccountAsync(
        string enterpriseId,
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists cost centres (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListCostCentresAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates cost centres following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateCostCentresAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cost centre by enterprise id and id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetCostCentreAsync(
        string enterpriseId,
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a cost centre.
    /// </summary>
    Task<AbacusResponse<string>> CreateCostCentreAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches a cost centre.
    /// </summary>
    Task<AbacusResponse<string>> PatchCostCentreAsync(
        string enterpriseId,
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a cost centre.
    /// </summary>
    Task<AbacusResponse<string>> DeleteCostCentreAsync(
        string enterpriseId,
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
