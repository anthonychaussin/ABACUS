using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.AccountsReceivable;

/// <summary>
/// High-level SDK facade for the AccountsReceivable module.
/// </summary>
public interface IAccountsReceivableClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_AccountsReceivableClient Raw { get; }

    /// <summary>
    /// Lists customers (first page). Prefer <see cref="EnumerateCustomersAsync"/> for full sets.
    /// </summary>
    Task<ODataPage<JsonElement>> ListCustomersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all customers following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateCustomersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by identifier.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetCustomerAsync(
        int customerId,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a customer from a JSON-compatible payload.
    /// </summary>
    Task<AbacusResponse<string>> CreateCustomerAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches a customer.
    /// </summary>
    Task<AbacusResponse<string>> PatchCustomerAsync(
        int customerId,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a customer by identifier.
    /// </summary>
    Task<AbacusResponse<string>> DeleteCustomerAsync(
        int customerId,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
