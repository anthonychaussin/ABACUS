using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.AssetsLedger;
using ABACUS.Core;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class AssetsLedgerClientTests
{
    [Fact]
    public async Task ListAssetsAsync_SendsODataQuery()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"1"}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AssetsLedgerClient(httpClient);

        var page = await client.ListAssetsAsync(ODataQuery.Create().Top(25));

        Assert.Equal("/Assets?$top=25", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(JsonValueKind.Object, Assert.Single(page.Value).ValueKind);
    }

    [Fact]
    public async Task GetAssetAsync_SendsParameterizedPath()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"Id":"112001"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AssetsLedgerClient(httpClient);

        await client.GetAssetAsync("112001");

        Assert.Equal("/Assets(112001)", handler.Requests[0].RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task PatchAssetAsync_SendsPatch()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AssetsLedgerClient(httpClient);

        await client.PatchAssetAsync("112001", new { Name = "Updated" });

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal("/Assets(112001)", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"Name\":\"Updated\"", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAssetAsync_SendsPreferHeader()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AssetsLedgerClient(httpClient);

        await client.CreateAssetAsync(new { Name = "Machine" }, prefer: AbacusHttp.PreferContinueOnError);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Assets", request.RequestUri?.AbsolutePath);
        Assert.Equal(AbacusHttp.PreferContinueOnError, request.Prefer);
    }

    [Fact]
    public async Task AssetCategoriesAndBookings_UseParenIdPaths()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[],"Id":"1"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AssetsLedgerClient(httpClient);

        await client.GetAssetCategoryAsync("9");
        await client.CreateAssetCategoryAsync(new { Name = "Cat" });
        await client.PatchAssetCategoryAsync("9", new { Name = "Cat2" });
        await client.GetAssetBookingAsync("3");
        await client.CreateAssetBookingAsync(new { Amount = 10 });
        await client.ListAssetBookingsAsync(ODataQuery.Create().Top(1));

        Assert.Equal("/AssetCategories(9)", handler.Requests[0].RequestUri?.AbsolutePath);
        Assert.Equal(HttpMethod.Post, handler.Requests[1].Method);
        Assert.Equal(HttpMethod.Patch, handler.Requests[2].Method);
        Assert.Equal("/AssetBookings(3)", handler.Requests[3].RequestUri?.AbsolutePath);
        Assert.Equal("/AssetBookings?$top=1", handler.Requests[5].RequestUri?.PathAndQuery);
    }

    [Fact]
    public async Task ListAssetCategoriesAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AssetsLedgerClient(httpClient);

        await client.ListAssetCategoriesAsync();

        Assert.Equal("/AssetCategories", handler.Requests[0].RequestUri?.AbsolutePath);
    }
}
