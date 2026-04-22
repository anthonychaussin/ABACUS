using ABACUS.Core;

namespace ABACUS.AccountsReceivable;

/// <summary>
/// Default implementation for the AccountsReceivable module wrapper.
/// </summary>
public sealed class AccountsReceivableClient : IAccountsReceivableClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "AccountsReceivable";

    /// <inheritdoc />
    public ABACUS_AccountsReceivableClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public AccountsReceivableClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_AccountsReceivableClient(httpClient);
    }

    /// <inheritdoc />
    public Task ListCustomersAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "/Customers", cancellationToken);

    /// <inheritdoc />
    public Task GetCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(customerId), "customerId must be greater than 0.");
        }

        return SendAsync(HttpMethod.Get, $"/Customers(Id={customerId})", cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteCustomerAsync(int customerId, CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(customerId), "customerId must be greater than 0.");
        }

        return SendAsync(HttpMethod.Delete, $"/Customers(Id={customerId})", cancellationToken);
    }

    private async Task SendAsync(HttpMethod method, string relativePath, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, relativePath);

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
