using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.Finance;

namespace ABACUS.Tests;

public sealed class FinanceClientTests
{
    [Fact]
    public async Task ListAccountsAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(httpClient);

        await client.ListAccountsAsync(ODataQuery.Create().Top(5));

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/Accounts?$top=5", request.RequestUri?.PathAndQuery);
    }

    [Fact]
    public async Task ListGeneralLedgerEntriesAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(httpClient);

        await client.ListGeneralLedgerEntriesAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/GeneralLedgerEntries", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task CreateAccountAsync_SendsExpectedPostRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(httpClient);

        await client.CreateAccountAsync(new { Name = "ACME" });

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Accounts", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"Name\":\"ACME\"", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetAccountAsync_SendsCompositeKeyPath()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"Id":"3090500"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(httpClient);

        await client.GetAccountAsync("500", "3090500");

        Assert.Equal("/Accounts(EnterpriseId=500,Id=3090500)", handler.Requests[0].RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task PatchAccountAsync_SendsPreferHeader()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(httpClient);

        await client.PatchAccountAsync("500", "3090500", new { Name = "Updated" }, prefer: AbacusHttp.PreferContinueOnError);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal("/Accounts(EnterpriseId=500,Id=3090500)", request.RequestUri?.AbsolutePath);
        Assert.Equal(AbacusHttp.PreferContinueOnError, request.Prefer);
    }

    [Fact]
    public async Task DeleteAccountAsync_SendsCompositeKeyPath()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(httpClient);

        await client.DeleteAccountAsync("500", "3090500");

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal("/Accounts(EnterpriseId=500,Id=3090500)", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task CostCentres_Crud_UsesCompositeKey()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[],"Id":"1"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(httpClient);

        await client.ListCostCentresAsync(ODataQuery.Create().Top(5));
        await client.GetCostCentreAsync("500", "100");
        await client.CreateCostCentreAsync(new { Name = "CC" });
        await client.PatchCostCentreAsync("500", "100", new { Name = "CC2" });
        await client.DeleteCostCentreAsync("500", "100");

        Assert.Equal("/CostCentres?$top=5", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal("/CostCentres(EnterpriseId=500,Id=100)", handler.Requests[1].RequestUri?.AbsolutePath);
        Assert.Equal(HttpMethod.Post, handler.Requests[2].Method);
        Assert.Equal(HttpMethod.Patch, handler.Requests[3].Method);
        Assert.Equal(HttpMethod.Delete, handler.Requests[4].Method);
    }

    [Fact]
    public async Task ListAccountsAsync_ThrowsAbacusApiException_OnHttpFailure()
    {
        using var handler = new CapturingHttpMessageHandler(static (_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid\"}", Encoding.UTF8, "application/json"),
            };
            response.Headers.Add("X-Trace-Id", "trace-finance");
            return response;
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListAccountsAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.Equal(["trace-finance"], exception.Headers["X-Trace-Id"]);
    }
}
