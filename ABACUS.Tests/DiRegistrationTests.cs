using ABACUS.AccountsPayable;
using ABACUS.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ABACUS.Tests;

public sealed class DiRegistrationTests
{
    [Fact]
    public void AddAbacusSdk_And_ModuleClient_ResolveTogether()
    {
        var services = new ServiceCollection();
        services.AddAbacusSdk(new AbacusClientOptions
        {
            BaseUri = new Uri("https://example.invalid/api/entity/v1/mandants/7777/"),
        });
        services.AddAbacusModuleClient<IAccountsPayableClient, AccountsPayableClient>();

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IAccountsPayableClient>();

        Assert.Equal("AccountsPayable", client.ModuleName);
        Assert.NotNull(client.Raw);
    }

    [Fact]
    public void AddAbacusSdk_RegistersSharedOptions()
    {
        var services = new ServiceCollection();
        services.AddAbacusSdk(options =>
        {
            options.ServerUri = new Uri("https://example.invalid");
            options.Mandant = "7777";
            options.EnableRequestLogging = true;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<AbacusClientOptions>();

        Assert.Equal("7777", options.Mandant);
        Assert.True(options.EnableRequestLogging);
        Assert.Equal(
            new Uri("https://example.invalid/api/entity/v1/mandants/7777/"),
            options.ResolveBaseUri());
    }
}
