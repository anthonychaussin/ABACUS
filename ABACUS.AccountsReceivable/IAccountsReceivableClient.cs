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
    /// Lists all customers.
    /// </summary>
    Task ListCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific customer by identifier.
    /// </summary>
    Task GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific customer by identifier.
    /// </summary>
    Task DeleteCustomerAsync(int customerId, CancellationToken cancellationToken = default);
}
