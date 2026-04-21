using ABACUS.Core;

namespace ABACUS.DossierFileUpload;

/// <summary>
/// Default implementation for the DossierFileUpload module wrapper.
/// </summary>
public sealed class DossierFileUploadClient : IDossierFileUploadClient
{
    /// <inheritdoc />
    public string ModuleName => "DossierFileUpload";

    /// <inheritdoc />
    public ABACUS_DossierFileUploadClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public DossierFileUploadClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_DossierFileUploadClient(httpClient);
    }
}
