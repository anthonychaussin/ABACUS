using ABACUS.Core;

namespace ABACUS.Finance;

/// <summary>
/// High-level SDK facade for the Finance module.
/// </summary>
public interface IFinanceClient : IAbacusModuleClient
{
    /// <summary>
    /// Lists accounts.
    /// </summary>
    Task ListAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists general ledger entries.
    /// </summary>
    Task ListGeneralLedgerEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an account from a JSON-compatible payload.
    /// </summary>
    Task CreateAccountAsync(object payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_FinanceClient Raw { get; }
}
