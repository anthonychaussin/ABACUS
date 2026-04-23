using ABACUS.Core;
using System.Text;
using System.Text.Json;

namespace ABACUS.Finance;

/// <summary>
/// Default implementation for the Finance module wrapper.
/// </summary>
public sealed class FinanceClient : IFinanceClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "Finance";

    /// <inheritdoc />
    public ABACUS_FinanceClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public FinanceClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_FinanceClient(httpClient);
    }

    /// <inheritdoc />
    public Task ListAccountsAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "/Accounts", payload: null, cancellationToken);

    /// <inheritdoc />
    public Task ListGeneralLedgerEntriesAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "/GeneralLedgerEntries", payload: null, cancellationToken);

    /// <inheritdoc />
    public Task CreateAccountAsync(object payload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return SendAsync(HttpMethod.Post, "/Accounts", payload, cancellationToken);
    }

    private async Task SendAsync(HttpMethod method, string relativePath, object? payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, relativePath);

        if (payload is not null)
        {
            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        try
        {
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var responseBody = response.Content is null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            throw new AbacusApiException(
                message: $"The HTTP status code of the response was not expected ({(int)response.StatusCode}).",
                statusCode: (int)response.StatusCode,
                responseBody: responseBody,
                headers: BuildHeaders(response));
        }
        catch (Exception ex) when (ex is not AbacusApiException)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    private static IReadOnlyDictionary<string, IEnumerable<string>> BuildHeaders(HttpResponseMessage response)
    {
        var headers = response.Headers.ToDictionary(header => header.Key, header => header.Value);
        if (response.Content?.Headers is not null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        return headers;
    }
}
