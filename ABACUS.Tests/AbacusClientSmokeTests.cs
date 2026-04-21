using ABACUS.AccountsPayable;
using ABACUS.Core;
using ABACUS.RealEstate;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class ModuleCompositionSmokeTests
{
    [Fact]
    public void CoreHttpFactory_CanCreateConfiguredHttpClient()
    {
        var client = AbacusHttpClientFactory.Create(new AbacusClientOptions
        {
            BaseUri = new Uri("https://example.invalid"),
            UserAgent = "Tests/0.0.1",
        });

        Assert.Equal(new Uri("https://example.invalid"), client.BaseAddress);
        Assert.Equal("Tests/0.0.1", client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public void Modules_CanBeInstantiatedIndependently()
    {
        using var handler = new FakeHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };

        var ap = new AccountsPayableClient(httpClient);
        var re = new RealEstateClient(httpClient);

        Assert.NotNull(ap.Raw);
        Assert.NotNull(re.Raw);
    }
}
