using ABACUS.Core;

namespace ABACUS.Finance;

/// <summary>
/// High-level SDK facade for the Finance module.
/// </summary>
public interface IFinanceClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_FinanceClient Raw { get; }
}
