using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.Core;
using ABACUS.General;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class GeneralClientTests
{
    [Fact]
    public async Task ListCountriesAsync_SendsODataQuery()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"CH"}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new GeneralClient(httpClient);

        var page = await client.ListCountriesAsync(ODataQuery.Create().Top(10));

        Assert.Equal("/Countries?$top=10", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(JsonValueKind.Object, Assert.Single(page.Value).ValueKind);
    }

    [Fact]
    public async Task ListCurrenciesAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new GeneralClient(httpClient);

        await client.ListCurrenciesAsync();

        Assert.Equal("/Currencies", handler.Requests[0].RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task ListDivisionsAsync_And_ListEnterprisesAsync_SendExpectedPaths()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new GeneralClient(httpClient);

        await client.ListDivisionsAsync();
        await client.ListEnterprisesAsync();

        Assert.Equal("/Divisions", handler.Requests[0].RequestUri?.AbsolutePath);
        Assert.Equal("/Enterprises", handler.Requests[1].RequestUri?.AbsolutePath);
    }
}
