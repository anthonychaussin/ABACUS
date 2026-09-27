using System.Net;
using System.Text;

namespace ABACUS.Core;

/// <summary>
/// One operation inside an OData <c>$batch</c> request.
/// </summary>
public sealed class ODataBatchOperation
{
    /// <summary>
    /// Creates a batch operation.
    /// </summary>
    public ODataBatchOperation(HttpMethod method, string relativeUrl, string? body = null, string? contentType = "application/json")
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativeUrl);
        Method = method;
        RelativeUrl = relativeUrl.TrimStart('/');
        Body = body;
        ContentType = contentType;
    }

    /// <summary>
    /// HTTP method.
    /// </summary>
    public HttpMethod Method { get; }

    /// <summary>
    /// Relative URL without leading slash (for example <c>Customers?$top=10</c>).
    /// </summary>
    public string RelativeUrl { get; }

    /// <summary>
    /// Optional JSON/text body.
    /// </summary>
    public string? Body { get; }

    /// <summary>
    /// Content-Type for the body when present.
    /// </summary>
    public string? ContentType { get; }
}

/// <summary>
/// Builds an OData 4.0 multipart/mixed <c>$batch</c> request body.
/// </summary>
public sealed class ODataBatchRequest
{
    private readonly List<ODataBatchOperation> _operations = new();

    /// <summary>
    /// Boundary used in the multipart payload.
    /// </summary>
    public string Boundary { get; } = "batch_" + Guid.NewGuid().ToString("N");

    /// <summary>
    /// Operations queued for the batch.
    /// </summary>
    public IReadOnlyList<ODataBatchOperation> Operations => _operations;

    /// <summary>
    /// Adds a GET operation.
    /// </summary>
    public ODataBatchRequest Get(string relativeUrl)
    {
        _operations.Add(new ODataBatchOperation(HttpMethod.Get, relativeUrl));
        return this;
    }

    /// <summary>
    /// Adds a POST operation with an optional JSON body.
    /// </summary>
    public ODataBatchRequest Post(string relativeUrl, string? jsonBody = null)
    {
        _operations.Add(new ODataBatchOperation(HttpMethod.Post, relativeUrl, jsonBody));
        return this;
    }

    /// <summary>
    /// Adds a PATCH operation with a JSON body.
    /// </summary>
    public ODataBatchRequest Patch(string relativeUrl, string jsonBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonBody);
        _operations.Add(new ODataBatchOperation(HttpMethod.Patch, relativeUrl, jsonBody));
        return this;
    }

    /// <summary>
    /// Adds a DELETE operation.
    /// </summary>
    public ODataBatchRequest Delete(string relativeUrl)
    {
        _operations.Add(new ODataBatchOperation(HttpMethod.Delete, relativeUrl));
        return this;
    }

    /// <summary>
    /// Adds a custom operation.
    /// </summary>
    public ODataBatchRequest Add(ODataBatchOperation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        _operations.Add(operation);
        return this;
    }

    /// <summary>
    /// Builds the multipart/mixed body and Content-Type header value.
    /// </summary>
    public (string ContentType, string Body) Build()
    {
        if (_operations.Count == 0)
        {
            throw new InvalidOperationException("A batch request requires at least one operation.");
        }

        var builder = new StringBuilder();
        foreach (var operation in _operations)
        {
            builder.Append("--").Append(Boundary).Append("\r\n");
            builder.Append("Content-Type: application/http\r\n");
            builder.Append("Content-Transfer-Encoding: binary\r\n\r\n");
            builder.Append(operation.Method.Method).Append(' ').Append(operation.RelativeUrl).Append(" HTTP/1.1\r\n");
            if (!string.IsNullOrEmpty(operation.Body))
            {
                builder.Append("Content-Type: ").Append(operation.ContentType ?? "application/json").Append("\r\n");
                builder.Append("Content-Length: ").Append(Encoding.UTF8.GetByteCount(operation.Body)).Append("\r\n\r\n");
                builder.Append(operation.Body).Append("\r\n");
            }
            else
            {
                builder.Append("\r\n");
            }
        }

        builder.Append("--").Append(Boundary).Append("--\r\n");
        return ($"multipart/mixed; boundary={Boundary}", builder.ToString());
    }
}

/// <summary>
/// One part of an OData batch response.
/// </summary>
public sealed class ODataBatchPart
{
    /// <summary>
    /// Creates a batch response part.
    /// </summary>
    public ODataBatchPart(int statusCode, string body, IReadOnlyDictionary<string, string> headers)
    {
        StatusCode = statusCode;
        Body = body ?? string.Empty;
        Headers = headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// HTTP status of the nested response.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Nested response body.
    /// </summary>
    public string Body { get; }

    /// <summary>
    /// Nested response headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Whether the nested status is success (2xx).
    /// </summary>
    public bool IsSuccessStatusCode => StatusCode is >= 200 and <= 299;
}

/// <summary>
/// Parsed OData <c>$batch</c> response.
/// </summary>
public sealed class ODataBatchResponse
{
    /// <summary>
    /// Creates a batch response.
    /// </summary>
    public ODataBatchResponse(
        IReadOnlyList<ODataBatchPart> parts,
        int statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers,
        string rawBody)
    {
        Parts = parts;
        StatusCode = statusCode;
        Headers = headers;
        RawBody = rawBody;
    }

    /// <summary>
    /// Nested HTTP responses, in request order when the server preserves it.
    /// </summary>
    public IReadOnlyList<ODataBatchPart> Parts { get; }

    /// <summary>
    /// Outer HTTP status of the <c>$batch</c> call.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Outer response headers.
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// Raw multipart body.
    /// </summary>
    public string RawBody { get; }
}

/// <summary>
/// Parses multipart/mixed OData batch responses.
/// </summary>
public static class ODataBatchParser
{
    /// <summary>
    /// Parses a multipart batch response body.
    /// </summary>
    public static ODataBatchResponse Parse(
        string rawBody,
        string? contentType,
        int statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        ArgumentNullException.ThrowIfNull(rawBody);
        headers ??= new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

        var boundary = ExtractBoundary(contentType);
        if (string.IsNullOrWhiteSpace(boundary))
        {
            return new ODataBatchResponse(Array.Empty<ODataBatchPart>(), statusCode, headers, rawBody);
        }

        var parts = new List<ODataBatchPart>();
        var delimiter = "--" + boundary;
        var chunks = rawBody.Split(new[] { delimiter }, StringSplitOptions.None);
        foreach (var chunk in chunks)
        {
            var trimmed = chunk.Trim('\r', '\n', ' ');
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed == "--" || trimmed.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var httpStart = trimmed.IndexOf("HTTP/", StringComparison.OrdinalIgnoreCase);
            if (httpStart < 0)
            {
                continue;
            }

            var httpPayload = trimmed[httpStart..];
            using var reader = new StringReader(httpPayload);
            var statusLine = reader.ReadLine();
            if (statusLine is null)
            {
                continue;
            }

            var status = ParseStatusCode(statusLine);
            var partHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? headerLine;
            while ((headerLine = reader.ReadLine()) is not null)
            {
                if (headerLine.Length == 0)
                {
                    break;
                }

                var sep = headerLine.IndexOf(':');
                if (sep > 0)
                {
                    partHeaders[headerLine[..sep].Trim()] = headerLine[(sep + 1)..].Trim();
                }
            }

            var body = reader.ReadToEnd().TrimEnd('\r', '\n');
            parts.Add(new ODataBatchPart(status, body, partHeaders));
        }

        return new ODataBatchResponse(parts, statusCode, headers, rawBody);
    }

    private static string? ExtractBoundary(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        const string marker = "boundary=";
        var index = contentType.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var value = contentType[(index + marker.Length)..].Trim();
        if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
        {
            value = value[1..^1];
        }

        var semicolon = value.IndexOf(';');
        return semicolon >= 0 ? value[..semicolon].Trim() : value;
    }

    private static int ParseStatusCode(string statusLine)
    {
        var parts = statusLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && int.TryParse(parts[1], out var code))
        {
            return code;
        }

        return (int)HttpStatusCode.OK;
    }
}
