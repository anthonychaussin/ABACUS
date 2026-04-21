using ABACUS.Core;

namespace ABACUS.AccountsReceivable;

/// <summary>
/// Default implementation for the AccountsReceivable module wrapper.
/// </summary>
public sealed class AccountsReceivableClient : IAccountsReceivableClient
{
    /// <inheritdoc />
    public string ModuleName => "AccountsReceivable";

    /// <inheritdoc />
    public ABACUS_AccountsReceivableClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public AccountsReceivableClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_AccountsReceivableClient(httpClient);
    }
}
