using System.Net;
using System.Text;
using ABACUS.AccountsReceivable;
using ABACUS.Core;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class AccountsReceivableClientTests
{
    [Fact]
    public async Task ListCustomersAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new AccountsReceivableClient(httpClient);

        await client.ListCustomersAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/Customers", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task GetCustomerAsync_SendsParameterizedPath()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new AccountsReceivableClient(httpClient);

        await client.GetCustomerAsync(42);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/Customers(Id=42)", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task DeleteCustomerAsync_SendsParameterizedPath()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
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
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
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
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new AccountsReceivableClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListCustomersAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.Equal(["trace-abc"], exception.Headers["X-Trace-Id"]);
    }
}
