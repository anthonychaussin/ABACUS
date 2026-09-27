using ABACUS.Core;

namespace ABACUS.General;

/// <summary>
/// Default implementation for the General module wrapper.
/// </summary>
public sealed class GeneralClient : IGeneralClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "General";

    /// <inheritdoc />
    public ABACUS_GeneralClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public GeneralClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_GeneralClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataBatchResponse> PostBatchAsync(
        ODataBatchRequest batch,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        return AbacusHttp.SendBatchAsync(_httpClient, batch, prefer, cancellationToken: cancellationToken);
    }
}
