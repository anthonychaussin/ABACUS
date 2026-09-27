using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.AccountsReceivable;
using ABACUS.Core;

namespace ABACUS.Tests;

public sealed class AccountsReceivableClientTests
{
    [Fact]
    public async Task ListCustomersAsync_SendsODataQuery()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":1}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsReceivableClient(httpClient);

        var page = await client.ListCustomersAsync(ODataQuery.Create().Top(10));

        Assert.Equal("/Customers?$top=10", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(JsonValueKind.Object, Assert.Single(page.Value).ValueKind);
    }

    [Fact]
    public async Task GetCustomerAsync_SendsParameterizedPath()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"Id":42}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsReceivableClient(httpClient);

        var response = await client.GetCustomerAsync(42);

        Assert.Equal("/Customers(Id=42)", handler.Requests[0].RequestUri?.AbsolutePath);
        Assert.Equal(42, response.Data.GetProperty("Id").GetInt32());
    }

    [Fact]
    public async Task CreateCustomerAsync_SendsPostWithPrefer()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsReceivableClient(httpClient);

        await client.CreateCustomerAsync(new { Name = "Acme" }, prefer: AbacusHttp.PreferContinueOnError);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Customers", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"Name\":\"Acme\"", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteCustomerAsync_SendsParameterizedPath()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsReceivableClient(httpClient);

        await client.DeleteCustomerAsync(42);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal("/Customers(Id=42)", request.RequestUri?.AbsolutePath);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CustomerIdMethods_ThrowForInvalidCustomerId(int customerId)
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsReceivableClient(httpClient);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetCustomerAsync(customerId));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.DeleteCustomerAsync(customerId));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ListCustomersAsync_ThrowsAbacusApiException_OnHttpFailure()
    {
        using var handler = new CapturingHttpMessageHandler(static (_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid\"}", Encoding.UTF8, "application/json"),
            };
            response.Headers.Add("X-Trace-Id", "trace-abc");
            return response;
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsReceivableClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListCustomersAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.Equal(["trace-abc"], exception.Headers["X-Trace-Id"]);
    }
}
