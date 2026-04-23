using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.RealEstate;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class RealEstateClientTests
{
    [Fact]
    public async Task ListObjectContractsAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new RealEstateClient(httpClient);

        await client.ListObjectContractsAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/ObjectContracts", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task ListPartialObjectContractsAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new RealEstateClient(httpClient);

        await client.ListPartialObjectContractsAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/PartialObjectContracts", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task ListCodeTablesAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new RealEstateClient(httpClient);

        await client.ListCodeTablesAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/Codetables", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task ListObjectContractsAsync_MapsGeneratedApiException_ToAbacusApiException()
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
