using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.General;

namespace ABACUS.Tests;

public sealed class ODataBatchTests
{
    [Fact]
    public void ODataBatchRequest_Build_CreatesMultipartBody()
    {
        var batch = new ODataBatchRequest()
            .Get("Customers?$top=1")
            .Patch("Customers(Id=1)", """{"Name":"Acme"}""");

        var (contentType, body) = batch.Build();

        Assert.StartsWith("multipart/mixed; boundary=batch_", contentType, StringComparison.Ordinal);
        Assert.Contains("GET Customers?$top=1 HTTP/1.1", body, StringComparison.Ordinal);
        Assert.Contains("PATCH Customers(Id=1) HTTP/1.1", body, StringComparison.Ordinal);
        Assert.Contains("""{"Name":"Acme"}""", body, StringComparison.Ordinal);
    }

    [Fact]
    public void ODataBatchParser_Parse_ReadsNestedStatuses()
    {
        const string boundary = "batch_abc";
        var raw = $$"""
            --{{boundary}}
            Content-Type: application/http

            HTTP/1.1 200 OK
            Content-Type: application/json

            {"value":[]}
            --{{boundary}}
            Content-Type: application/http

            HTTP/1.1 204 No Content

            --{{boundary}}--
            """;

        var parsed = ODataBatchParser.Parse(
            raw,
            $"multipart/mixed; boundary={boundary}",
            200,
            new Dictionary<string, IEnumerable<string>>());

        Assert.Equal(2, parsed.Parts.Count);
        Assert.Equal(200, parsed.Parts[0].StatusCode);
        Assert.Contains("\"value\":[]", parsed.Parts[0].Body, StringComparison.Ordinal);
        Assert.Equal(204, parsed.Parts[1].StatusCode);
    }

    [Fact]
    public async Task GeneralClient_PostBatchAsync_SendsMultipart()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
        {
            const string boundary = "batch_resp";
            var body = $$"""
                --{{boundary}}
                Content-Type: application/http

                HTTP/1.1 200 OK

                {"ok":true}
                --{{boundary}}--
                """;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8),
            };
            response.Content.Headers.Remove("Content-Type");
            response.Content.Headers.TryAddWithoutValidation("Content-Type", $"multipart/mixed; boundary={boundary}");
            return response;
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new GeneralClient(httpClient);

        var result = await client.PostBatchAsync(new ODataBatchRequest().Get("Countries"));

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/$batch", request.RequestUri?.AbsolutePath);
        Assert.Contains("multipart/mixed", request.ContentType, StringComparison.OrdinalIgnoreCase);
        Assert.True(Assert.Single(result.Parts).IsSuccessStatusCode);
    }
}
