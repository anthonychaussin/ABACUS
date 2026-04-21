using ABACUS.Core;

namespace ABACUS.CRM;

/// <summary>
/// High-level SDK facade for the CRM module.
/// </summary>
public interface ICRMClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_CRMClient Raw { get; }
}
