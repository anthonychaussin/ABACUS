using ABACUS.Core;

namespace ABACUS.AccountsPayable;

/// <summary>
/// Default implementation for Accounts Payable wrapper methods.
/// </summary>
public sealed class AccountsPayableClient : IAccountsPayableClient
{
    /// <inheritdoc />
    public string ModuleName => "AccountsPayable";

    /// <inheritdoc />
    public ABACUS_AccountsPayableClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public AccountsPayableClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_AccountsPayableClient(httpClient);
    }

    /// <inheritdoc />
    public async Task ListSuppliersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.GetSuppliers11AllSuppliersAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <inheritdoc />
    public async Task CreateSupplierAsync(object payload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        try
        {
            await Raw.PostSuppliers15SupplierWithBeneficiaryAccountAsync(payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <inheritdoc />
    public async Task ListSupplierCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.GetSuppliercurrencies41AllSupplierCurrenciesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <inheritdoc />
    public async Task ListSupplierPaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.GetSupplierpaymentmethods61AllSupplierPaymentMethodsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }
}
