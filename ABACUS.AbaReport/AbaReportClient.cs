using ABACUS.Core;

namespace ABACUS.AbaReport;

/// <summary>
/// Default AbaReport client. Paths follow the Abacus AbaReport REST API conventions
/// under the configured <see cref="HttpClient.BaseAddress"/>.
/// </summary>
public sealed class AbaReportClient : IAbaReportClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "AbaReport";

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// Use a BaseAddress that points at the AbaReport API root on the Abacus server.
    /// </summary>
    public AbaReportClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> ListReportsAsync(CancellationToken cancellationToken = default) =>
        AbacusHttp.SendAsync(_httpClient, HttpMethod.Get, "/abareport/api/reports", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<byte[]>> ExportReportAsync(
        string reportId,
        string? format = null,
        CancellationToken cancellationToken = default) =>
        ExportReportAsync(reportId, parameters: null, format, cancellationToken);

    /// <inheritdoc />
    public async Task<AbacusResponse<byte[]>> ExportReportAsync(
        string reportId,
        object? parameters,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportId);
        var path = $"/abareport/api/reports/{Uri.EscapeDataString(reportId.Trim())}/export";
        if (!string.IsNullOrWhiteSpace(format))
        {
            path += "?format=" + Uri.EscapeDataString(format.Trim());
        }

        using var request = new HttpRequestMessage(
            parameters is null ? HttpMethod.Get : HttpMethod.Post,
            path);
        if (parameters is not null)
        {
            request.Content = AbacusHttp.CreateJsonContent(parameters);
        }

        try
        {
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            var headers = AbacusHttp.BuildHeaders(response);
            var bytes = response.Content is null
                ? Array.Empty<byte>()
                : await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = bytes.Length == 0 ? string.Empty : System.Text.Encoding.UTF8.GetString(bytes);
                throw new AbacusApiException(
                    $"The HTTP status code of the response was not expected ({(int)response.StatusCode}).",
                    (int)response.StatusCode,
                    body,
                    headers);
            }

            return new AbacusResponse<byte[]>(bytes, (int)response.StatusCode, headers);
        }
        catch (Exception ex) when (ex is not AbacusApiException)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }
}
