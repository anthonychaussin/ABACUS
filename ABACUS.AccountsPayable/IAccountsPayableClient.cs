using ABACUS.Core;
using System.Text.Json;

namespace ABACUS.AccountsPayable;

/// <summary>
/// High-level SDK facade for the ABACUS Accounts Payable module.
/// </summary>
public interface IAccountsPayableClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_AccountsPayableClient Raw { get; }

    /// <summary>
    /// Lists suppliers (first page). Prefer <see cref="EnumerateSuppliersAsync"/> for full sets.
    /// </summary>
    Task<ODataPage<JsonElement>> ListSuppliersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all suppliers following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateSuppliersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a supplier by id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetSupplierAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a supplier from a JSON-compatible payload (for example from <see cref="IAbacusFieldMapper"/>).
    /// </summary>
    Task<AbacusResponse<string>> CreateSupplierAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a supplier by mapping a model with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    Task<AbacusResponse<string>> CreateSupplierAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches a supplier.
    /// </summary>
    Task<AbacusResponse<string>> PatchSupplierAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches a supplier by mapping a model with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    Task<AbacusResponse<string>> PatchSupplierAsync<TModel>(
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists suppliers deserialized as <typeparamref name="T"/>.
    /// </summary>
    Task<ODataPage<T>> ListSuppliersAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a supplier.
    /// </summary>
    Task<AbacusResponse<string>> DeleteSupplierAsync(
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists supplier currencies (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListSupplierCurrenciesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists supplier payment methods (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListSupplierPaymentMethodsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);
}
