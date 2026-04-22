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
            await Raw.Get_SuppliersAsync(cancellationToken).ConfigureAwait(false);
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
            await Raw.Post_SuppliersAsync(payload, cancellationToken).ConfigureAwait(false);
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
            await Raw.Get_SupplierCurrenciesAsync(cancellationToken).ConfigureAwait(false);
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
            await Raw.Get_SupplierPaymentMethodsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }
}
