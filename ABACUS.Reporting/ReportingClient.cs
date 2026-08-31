using System.Net.Http.Json;
using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.Reporting;

/// <summary>Default client for the AbaReport REST API v1.</summary>
public sealed class ReportingClient : IReportingClient
{
    private const string ApiPath = "api/abareport/v1/";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public ReportingClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public string ModuleName => "Reporting";

    public async Task<ReportJob> StartReportAsync(
        int clientNumber,
        string reportName,
        ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(clientNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(reportName);
        ArgumentNullException.ThrowIfNull(request);

        if (reportName.Contains('\\') || reportName.Split('/').Any(segment => segment is "." or ".." or ""))
        {
            throw new ArgumentException(
                "The report name must be a relative AbaReport path without empty or traversal segments.",
                nameof(reportName));
        }

        if (request.Paging < 10)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "AbaReport paging must be at least 10.");
        }

        var path = $"{ApiPath}report/{clientNumber}/{Uri.EscapeDataString(reportName)}";
        using var response = await _httpClient.PostAsJsonAsync(path, request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return await ReadJobAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ReportJob> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var path = $"{ApiPath}jobs/{EscapeRequired(jobId, nameof(jobId))}";
        using var response = await _httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);
        return await ReadJobAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ReportOutput> GetOutputAsync(
        string jobId,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        var path = $"{ApiPath}jobs/{EscapeRequired(jobId, nameof(jobId))}/output/{page}";
        using var response = await _httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        return new ReportOutput(content, response.Content.Headers.ContentType?.ToString());
    }

    public async Task CloseJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var path = $"{ApiPath}jobs/{EscapeRequired(jobId, nameof(jobId))}";
        using var response = await _httpClient.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<ReportJob> ReadJobAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ReportJob>(JsonOptions, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new JsonException("The AbaReport API returned an empty job response.");
    }

    private static string EscapeRequired(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return Uri.EscapeDataString(value);
    }
}
