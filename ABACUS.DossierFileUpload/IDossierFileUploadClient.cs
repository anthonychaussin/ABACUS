using System.Text.Json;
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

    /// <summary>Lists customer invoice documents (first page).</summary>
    Task<ODataPage<JsonElement>> ListCustomerInvoiceDocumentsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Uploads a customer invoice document (JSON metadata payload).</summary>
    Task<AbacusResponse<string>> UploadCustomerInvoiceDocumentAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>Uploads raw bytes to the user file-store.</summary>
    Task<AbacusResponse<string>> UploadToFileStoreAsync(
        byte[] content,
        CancellationToken cancellationToken = default);

    /// <summary>Uploads a stream to the user file-store.</summary>
    Task<AbacusResponse<string>> UploadToFileStoreAsync(
        Stream content,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a storage by id.</summary>
    Task<AbacusResponse<JsonElement>> GetStorageAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);
}
