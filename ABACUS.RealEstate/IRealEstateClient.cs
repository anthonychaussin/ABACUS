using ABACUS.Core;

namespace ABACUS.RealEstate;

/// <summary>
/// High-level SDK facade for the RealEstate module.
/// </summary>
public interface IRealEstateClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_RealEstateClient Raw { get; }
}
