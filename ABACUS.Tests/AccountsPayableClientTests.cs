using System.Net;
using System.Text;
using ABACUS.AccountsPayable;
using ABACUS.Core;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class AccountsPayableClientTests
{
    public static TheoryData<string, Func<AccountsPayableClient, CancellationToken, Task>> GetOperations => new()
    {
        { "/Suppliers", static (client, cancellationToken) => client.ListSuppliersAsync(cancellationToken) },
        { "/SupplierCurrencies", static (client, cancellationToken) => client.ListSupplierCurrenciesAsync(cancellationToken) },
        { "/SupplierPaymentMethods", static (client, cancellationToken) => client.ListSupplierPaymentMethodsAsync(cancellationToken) },
    };

    [Theory]
    [MemberData(nameof(GetOperations))]
    public async Task WrapperGetMethods_SendExpectedRequestAndForwardCancellationToken(
        string expectedPath,
        Func<AccountsPayableClient, CancellationToken, Task> operation)
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new AccountsPayableClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();

        await operation(client, cancellationSource.Token);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(expectedPath, request.RequestUri?.AbsolutePath);
        Assert.True(request.CancellationToken.CanBeCanceled);
    }

    [Fact]
    public async Task CreateSupplierAsync_SendsExpectedPostRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new AccountsPayableClient(httpClient);
        var payload = new { SupplierNumber = 42, Name = "Acme" };

        await client.CreateSupplierAsync(payload);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Suppliers", request.RequestUri?.AbsolutePath);
        Assert.Equal("application/json", request.ContentType);
        Assert.Contains("\"SupplierNumber\":42", request.Body);
        Assert.Contains("\"Name\":\"Acme\"", request.Body);
    }

    [Fact]
    public async Task CreateSupplierAsync_ThrowsArgumentNullException_ForNullPayload()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new AccountsPayableClient(httpClient);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => client.CreateSupplierAsync(null!));

        Assert.Equal("payload", exception.ParamName);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ListSuppliersAsync_MapsGeneratedApiException_ToAbacusApiException()
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
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new AccountsPayableClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListSuppliersAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("ApiException", exception.InnerException!.GetType().Name);
        Assert.Equal(["trace-123"], exception.Headers["X-Trace-Id"]);
    }
}
