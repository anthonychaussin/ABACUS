using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.Core;
using ABACUS.CRM;

namespace ABACUS.Tests;

public sealed class CRMClientTests
{
    [Fact]
    public async Task ListSubjectsAsync_SendsODataQuery()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"1"}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new CRMClient(httpClient);

        var page = await client.ListSubjectsAsync(ODataQuery.Create().Top(10));

        Assert.Equal("/Subjects?$top=10", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(JsonValueKind.Object, Assert.Single(page.Value).ValueKind);
    }

    [Fact]
    public async Task CreateSubjectAsync_SendsPreferHeader()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new CRMClient(httpClient);

        await client.CreateSubjectAsync(new { Name = "Acme" }, prefer: AbacusHttp.PreferContinueOnError);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Subjects", request.RequestUri?.AbsolutePath);
        Assert.Equal(AbacusHttp.PreferContinueOnError, request.Prefer);
        Assert.Contains("\"Name\":\"Acme\"", request.Body);
    }

    [Fact]
    public async Task CreateSubjectAsync_ThrowsArgumentNullException_ForNullPayload()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new CRMClient(httpClient);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateSubjectAsync(null!));
        Assert.Equal("payload", exception.ParamName);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ListSubjectsAsync_ThrowsAbacusApiException_OnHttpFailure()
    {
        using var handler = new CapturingHttpMessageHandler(static (_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid\"}", Encoding.UTF8, "application/json"),
            };
            response.Headers.Add("X-Trace-Id", "trace-crm");
            return response;
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new CRMClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListSubjectsAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.Equal(["trace-crm"], exception.Headers["X-Trace-Id"]);
    }
}
