using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ABACUS.Testing;

/// <summary>
/// Builds common OData JSON HTTP responses for tests.
/// </summary>
public static class ODataResponseFactory
{
    /// <summary>
    /// Empty <c>{"value":[]}</c> page.
    /// </summary>
    public static HttpResponseMessage EmptyPage(HttpStatusCode statusCode = HttpStatusCode.OK) =>
        Json(statusCode, """{"value":[]}""");

    /// <summary>
    /// Page with a JSON array fragment for <c>value</c> (for example <c>[{"Id":"1"}]</c>).
    /// </summary>
    public static HttpResponseMessage Page(
        string valueJsonArray,
        string? nextLink = null,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valueJsonArray);
        var sb = new StringBuilder();
        sb.Append("{\"value\":");
        sb.Append(valueJsonArray);
        if (!string.IsNullOrWhiteSpace(nextLink))
        {
            sb.Append(",\"@odata.nextLink\":\"");
            sb.Append(Escape(nextLink));
            sb.Append('"');
        }

        sb.Append('}');
        return Json(statusCode, sb.ToString());
    }

    /// <summary>
    /// Arbitrary JSON body.
    /// </summary>
    public static HttpResponseMessage Json(HttpStatusCode statusCode, string json) =>
        new(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    /// <summary>
    /// Builds a multipart mixed body suitable for asserting against <c>$batch</c> requests.
    /// </summary>
    public static string MultipartBatchBody(string boundary, params string[] partBodies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(boundary);
        var sb = new StringBuilder();
        foreach (var part in partBodies)
        {
            sb.Append("--").Append(boundary).Append("\r\n");
            sb.Append("Content-Type: application/http\r\n");
            sb.Append("Content-Transfer-Encoding: binary\r\n\r\n");
            sb.Append(part).Append("\r\n");
        }

        sb.Append("--").Append(boundary).Append("--\r\n");
        return sb.ToString();
    }

    /// <summary>
    /// Creates a successful empty OData batch multipart response.
    /// </summary>
    public static HttpResponseMessage EmptyBatch(string boundary = "batch_abacus")
    {
        var body = MultipartBatchBody(
            boundary,
            "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"value\":[]}");
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8),
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/mixed");
        response.Content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
        return response;
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}
