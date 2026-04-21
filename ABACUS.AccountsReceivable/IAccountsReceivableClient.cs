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
}
