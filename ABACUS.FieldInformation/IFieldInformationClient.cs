using ABACUS.Core;

namespace ABACUS.FieldInformation;

/// <summary>
/// High-level SDK facade for the FieldInformation module.
/// </summary>
public interface IFieldInformationClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_FieldinformationClient Raw { get; }

    /// <summary>
    /// Requests field metadata for an entity description payload.
    /// </summary>
    Task<AbacusResponse<string>> GetFieldInformationsAsync(
        object payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns Abacus field paths discovered from a FieldInformation response.
    /// </summary>
    Task<IReadOnlySet<string>> DiscoverFieldPathsAsync(
        object payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns mapping paths for <paramref name="entity"/> that are not present in FieldInformation.
    /// </summary>
    Task<IReadOnlyList<string>> ValidateMappingAsync(
        IAbacusFieldMapper mapper,
        AbacusFieldMapping mapping,
        string entity,
        object fieldInformationPayload,
        CancellationToken cancellationToken = default);
}
