using ABACUS.AccountsPayable;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class RawAccountsPayableSmokeTests
{
    [Fact]
    public async Task Raw_GetSuppliers_CallsExpectedEndpoint_WithoutRealNetwork()
    {
        using var handler = new FakeHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };

        var module = new AccountsPayableClient(httpClient);

        await module.Raw.Get_SuppliersAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/Suppliers", request.RequestUri?.AbsolutePath);
    }
}
