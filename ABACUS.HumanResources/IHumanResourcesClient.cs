using ABACUS.Core;

namespace ABACUS.HumanResources;

/// <summary>
/// High-level SDK facade for the HumanResources module.
/// </summary>
public interface IHumanResourcesClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_HumanResourcesClient Raw { get; }
}
