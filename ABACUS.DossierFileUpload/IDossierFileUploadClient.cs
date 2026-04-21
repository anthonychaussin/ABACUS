using ABACUS.Core;

namespace ABACUS.DossierFileUpload;

/// <summary>
/// High-level SDK facade for the DossierFileUpload module.
/// </summary>
public interface IDossierFileUploadClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_DossierFileUploadClient Raw { get; }
}
