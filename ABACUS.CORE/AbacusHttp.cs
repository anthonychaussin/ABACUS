using System.Text;
using System.Text.Json;

namespace ABACUS.Core;

/// <summary>
/// Shared HTTP helpers that return <see cref="AbacusResponse{T}"/> and OData pages.
/// </summary>
public static class AbacusHttp
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Prefer header value that asks Abacus to continue processing despite validation warnings.
    /// </summary>
    public const string PreferContinueOnError = "odata.continue-on-error";

    /// <summary>
    /// Sends an HTTP request and returns the response body as text with metadata.
    /// </summary>
    public static async Task<AbacusResponse<string>> SendAsync(
        HttpClient httpClient,
        HttpMethod method,
        string relativePath,
        object? payload = null,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        using var request = new HttpRequestMessage(method, relativePath);
        if (!string.IsNullOrWhiteSpace(prefer))
        {
            request.Headers.TryAddWithoutValidation("Prefer", prefer);
        }

        if (payload is not null)
        {
            var json = payload is string text
                ? text
                : JsonSerializer.Serialize(payload, SerializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        try
        {
            using var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            var body = response.Content is null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var headers = BuildHeaders(response);

            if (!response.IsSuccessStatusCode)
            {
                throw new AbacusApiException(
                    message: $"The HTTP status code of the response was not expected ({(int)response.StatusCode}).",
                    statusCode: (int)response.StatusCode,
                    responseBody: body,
                    headers: headers);
            }

            return new AbacusResponse<string>(body, (int)response.StatusCode, headers);
        }
        catch (Exception ex) when (ex is not AbacusApiException)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <summary>
    /// Sends a request and deserializes the JSON body as <typeparamref name="T"/>.
    /// </summary>
    public static async Task<AbacusResponse<T>> SendJsonAsync<T>(
        HttpClient httpClient,
        HttpMethod method,
        string relativePath,
        object? payload = null,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(httpClient, method, relativePath, payload, prefer, cancellationToken)
            .ConfigureAwait(false);

        T data;
        if (string.IsNullOrWhiteSpace(response.Data))
        {
            data = default!;
        }
        else
        {
            data = JsonSerializer.Deserialize<T>(response.Data, SerializerOptions)!;
        }

        return new AbacusResponse<T>(data, response.StatusCode, response.Headers);
    }

    /// <summary>
    /// GETs an OData collection page.
    /// </summary>
    public static async Task<ODataPage<T>> GetODataPageAsync<T>(
        HttpClient httpClient,
        string relativePath,
        ODataQuery? query = null,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var path = query is null ? relativePath : query.ApplyTo(relativePath);
        var response = await SendAsync(httpClient, HttpMethod.Get, path, payload: null, prefer, cancellationToken)
            .ConfigureAwait(false);
        return ODataPageParser.Parse<T>(response.Data, response.StatusCode, response.Headers);
    }

    /// <summary>
    /// GETs an OData page from an absolute or relative <c>@odata.nextLink</c>.
    /// </summary>
    public static async Task<ODataPage<T>> GetODataPageByLinkAsync<T>(
        HttpClient httpClient,
        string nextLink,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(nextLink);

        var response = await SendAsync(httpClient, HttpMethod.Get, nextLink, payload: null, prefer, cancellationToken)
            .ConfigureAwait(false);
        return ODataPageParser.Parse<T>(response.Data, response.StatusCode, response.Headers);
    }

    /// <summary>
    /// Enumerates all pages following <c>@odata.nextLink</c> until exhausted.
    /// </summary>
    public static async IAsyncEnumerable<T> EnumerateODataAsync<T>(
        HttpClient httpClient,
        string relativePath,
        ODataQuery? query = null,
        string? prefer = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = await GetODataPageAsync<T>(httpClient, relativePath, query, prefer, cancellationToken)
            .ConfigureAwait(false);

        while (true)
        {
            foreach (var item in page.Value)
            {
                yield return item;
            }

            if (!page.HasNextPage)
            {
                yield break;
            }

            page = await GetODataPageByLinkAsync<T>(httpClient, page.NextLink!, prefer, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Builds a dictionary of response headers including content headers.
    /// </summary>
    public static IReadOnlyDictionary<string, IEnumerable<string>> BuildHeaders(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
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

    /// <summary>
    /// Applies a Prefer header to an existing request message.
    /// </summary>
    public static void ApplyPrefer(HttpRequestMessage request, string? prefer)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(prefer))
        {
            return;
        }

        request.Headers.Remove("Prefer");
        request.Headers.TryAddWithoutValidation("Prefer", prefer);
    }

    /// <summary>
    /// Creates JSON content from an arbitrary payload.
    /// </summary>
    public static StringContent CreateJsonContent(object payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var json = payload is string text ? text : JsonSerializer.Serialize(payload, SerializerOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Posts an OData <c>$batch</c> multipart request and parses nested responses.
    /// </summary>
    public static async Task<ODataBatchResponse> SendBatchAsync(
        HttpClient httpClient,
        ODataBatchRequest batch,
        string? prefer = null,
        string relativePath = "/$batch",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(batch);

        var (contentType, body) = batch.Build();
        using var request = new HttpRequestMessage(HttpMethod.Post, relativePath)
        {
            Content = new StringContent(body, Encoding.UTF8),
        };
        request.Content.Headers.Remove("Content-Type");
        request.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
        if (!string.IsNullOrWhiteSpace(prefer))
        {
            request.Headers.TryAddWithoutValidation("Prefer", prefer);
        }

        try
        {
            using var response = await httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            var raw = response.Content is null
                ? string.Empty
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var headers = BuildHeaders(response);
            if (!response.IsSuccessStatusCode)
            {
                throw new AbacusApiException(
                    $"The HTTP status code of the response was not expected ({(int)response.StatusCode}).",
                    (int)response.StatusCode,
                    raw,
                    headers);
            }

            var responseContentType = response.Content?.Headers.ContentType?.ToString();
            return ODataBatchParser.Parse(raw, responseContentType, (int)response.StatusCode, headers);
        }
        catch (Exception ex) when (ex is not AbacusApiException)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }
}
