using ABACUS.Core;

namespace ABACUS.RealEstate;

/// <summary>
/// High-level SDK facade for the RealEstate module.
/// </summary>
public interface IRealEstateClient : IAbacusModuleClient
{
    /// <summary>
    /// Lists object contracts.
    /// </summary>
    Task ListObjectContractsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists partial object contracts.
    /// </summary>
    Task ListPartialObjectContractsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists code tables.
    /// </summary>
    Task ListCodeTablesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_RealEstateClient Raw { get; }
}
