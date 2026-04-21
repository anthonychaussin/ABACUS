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
}
