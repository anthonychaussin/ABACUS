using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.DossierFileUpload;

/// <summary>
/// Default implementation for the DossierFileUpload module wrapper.
/// </summary>
public sealed class DossierFileUploadClient : IDossierFileUploadClient
{
    private readonly HttpClient _httpClient;

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
        _httpClient = httpClient;
        Raw = new ABACUS_DossierFileUploadClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListCustomerInvoiceDocumentsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/CustomerInvoiceDocuments", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> UploadCustomerInvoiceDocumentAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/CustomerInvoiceDocuments", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> UploadToFileStoreAsync(
        byte[] content,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.SendBytesAsync(
            _httpClient,
            HttpMethod.Post,
            "/api/file-store/v1/user",
            content,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> UploadToFileStoreAsync(
        Stream content,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.SendStreamAsync(
            _httpClient,
            HttpMethod.Post,
            "/api/file-store/v1/user",
            content,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetStorageAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, AbacusODataEntity.ParenIdPath("Storages", id), query, cancellationToken);
    }
}
