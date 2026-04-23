using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.Finance;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class FinanceClientTests
{
    [Fact]
    public async Task ListAccountsAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new FinanceClient(httpClient);

        await client.ListAccountsAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/Accounts", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task ListGeneralLedgerEntriesAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
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
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new FinanceClient(httpClient);

        await client.CreateAccountAsync(new { Name = "ACME" });

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Accounts", request.RequestUri?.AbsolutePath);
        Assert.Equal("application/json; charset=utf-8", request.ContentType);
        Assert.Contains("\"Name\":\"ACME\"", request.Body, StringComparison.Ordinal);
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
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new FinanceClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListAccountsAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.Equal(["trace-finance"], exception.Headers["X-Trace-Id"]);
    }
}
