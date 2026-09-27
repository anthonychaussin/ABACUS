using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.Core;
using ABACUS.RealEstate;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class RealEstateClientTests
{
    [Fact]
    public async Task ListObjectContractsAsync_SendsODataQuery()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"1"}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new RealEstateClient(httpClient);

        var page = await client.ListObjectContractsAsync(ODataQuery.Create().Top(10));

        Assert.Equal("/ObjectContracts?$top=10", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(JsonValueKind.Object, Assert.Single(page.Value).ValueKind);
    }

    [Fact]
    public async Task ListPartialObjectContractsAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new RealEstateClient(httpClient);

        await client.ListPartialObjectContractsAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/PartialObjectContracts", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task ListCodeTablesAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new RealEstateClient(httpClient);

        await client.ListCodeTablesAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/Codetables", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task ListObjectContractsAsync_ThrowsAbacusApiException_OnHttpFailure()
    {
        using var handler = new CapturingHttpMessageHandler(static (_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid\"}", Encoding.UTF8, "application/json"),
            };
            response.Headers.Add("X-Trace-Id", "trace-re");
            return response;
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new RealEstateClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListObjectContractsAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.Equal(["trace-re"], exception.Headers["X-Trace-Id"]);
    }
}
