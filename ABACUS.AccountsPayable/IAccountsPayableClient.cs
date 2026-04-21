using ABACUS.Core;

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
    /// List all suppliers.
    /// </summary>
    Task ListSuppliersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a supplier with beneficiary account details.
    /// </summary>
    Task CreateSupplierAsync(object payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all supplier currencies.
    /// </summary>
    Task ListSupplierCurrenciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// List all supplier payment methods.
    /// </summary>
    Task ListSupplierPaymentMethodsAsync(CancellationToken cancellationToken = default);
}
