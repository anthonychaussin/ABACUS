using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.AccountsPayable;
using ABACUS.Core;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class AccountsPayableClientTests
{
    [Fact]
    public async Task ListSuppliersAsync_SendsODataQuery()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"1"}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsPayableClient(httpClient);

        var page = await client.ListSuppliersAsync(ODataQuery.Create().Top(10));

        Assert.Equal("/Suppliers?$top=10", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(JsonValueKind.Object, Assert.Single(page.Value).ValueKind);
    }

    [Fact]
    public async Task CreateSupplierAsync_SendsExpectedPostRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsPayableClient(httpClient);

        await client.CreateSupplierAsync(new { SupplierNumber = 42, Name = "Acme" });

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Suppliers", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"SupplierNumber\":42", request.Body);
        Assert.Contains("\"Name\":\"Acme\"", request.Body);
    }

    [Fact]
    public async Task CreateSupplierAsync_ThrowsArgumentNullException_ForNullPayload()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsPayableClient(httpClient);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateSupplierAsync(null!));
        Assert.Equal("payload", exception.ParamName);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ListSuppliersAsync_ThrowsAbacusApiException_OnHttpFailure()
    {
        using var handler = new CapturingHttpMessageHandler(static (_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid\"}", Encoding.UTF8, "application/json"),
            };
            response.Headers.Add("X-Trace-Id", "trace-123");
            return response;
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsPayableClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListSuppliersAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.Equal(["trace-123"], exception.Headers["X-Trace-Id"]);
    }
}
