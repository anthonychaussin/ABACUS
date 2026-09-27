using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.Core;
using ABACUS.ProductionPlanning;

namespace ABACUS.Tests;

public sealed class ProductionPlanningClientTests
{
    [Fact]
    public async Task ListResourcesAsync_SendsODataQuery()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"1"}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new ProductionPlanningClient(httpClient);

        var page = await client.ListResourcesAsync(ODataQuery.Create().Top(10));

        Assert.Equal("/Resources?$top=10", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(JsonValueKind.Object, Assert.Single(page.Value).ValueKind);
    }

    [Fact]
    public async Task GetResourceAndCreateImport_SendExpectedPaths()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"Id":"1"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new ProductionPlanningClient(httpClient);

        await client.GetResourceAsync("42");
        await client.GetProductionOrderHeaderAsync("7");
        await client.CreateProductionImportDataAsync(new { Code = "X" }, prefer: AbacusHttp.PreferContinueOnError);

        Assert.Equal("/Resources(42)", handler.Requests[0].RequestUri?.AbsolutePath);
        Assert.Equal("/ProductionOrderHeaders(7)", handler.Requests[1].RequestUri?.AbsolutePath);
        Assert.Equal(AbacusHttp.PreferContinueOnError, handler.Requests[2].Prefer);
    }
}
