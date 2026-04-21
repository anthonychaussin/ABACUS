using ABACUS.Core;

namespace ABACUS.AssetsLedger;

/// <summary>
/// High-level SDK facade for the AssetsLedger module.
/// </summary>
public interface IAssetsLedgerClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_AssetsLedgerClient Raw { get; }
}
