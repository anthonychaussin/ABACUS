using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.Core;
using ABACUS.WebShop;

namespace ABACUS.Tests;

public sealed class WebShopClientTests
{
    [Fact]
    public async Task ListShopperAccountsAsync_SendsODataQuery()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"1"}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new WebShopClient(httpClient);

        var page = await client.ListShopperAccountsAsync(ODataQuery.Create().Top(10));

        Assert.Equal("/ShopperAccounts?$top=10", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(JsonValueKind.Object, Assert.Single(page.Value).ValueKind);
    }

    [Fact]
    public async Task CreateShopperAccountAsync_SendsPreferHeader()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new WebShopClient(httpClient);

        await client.CreateShopperAccountAsync(new { Name = "Shopper" }, prefer: AbacusHttp.PreferContinueOnError);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/ShopperAccounts", request.RequestUri?.AbsolutePath);
        Assert.Equal(AbacusHttp.PreferContinueOnError, request.Prefer);
    }

    [Fact]
    public async Task GetPatchDeleteShopperAccount_UseIdPath()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"Id":"7"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new WebShopClient(httpClient);

        await client.GetShopperAccountAsync("7");
        await client.PatchShopperAccountAsync("7", new { Name = "X" });
        await client.DeleteShopperAccountAsync("7");

        Assert.Equal("/ShopperAccounts(Id=7)", handler.Requests[0].RequestUri?.AbsolutePath);
        Assert.Equal(HttpMethod.Patch, handler.Requests[1].Method);
        Assert.Equal(HttpMethod.Delete, handler.Requests[2].Method);
    }
}
