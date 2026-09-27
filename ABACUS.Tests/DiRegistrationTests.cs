using System.Text;
using ABACUS.AbaReport;
using ABACUS.AccountsPayable;
using ABACUS.Core;
using ABACUS.DependencyInjection;
using ABACUS.DossierFileUpload;
using ABACUS.General;
using ABACUS.HumanResources;
using ABACUS.ProductionPlanning;
using ABACUS.ProjectManagement;
using ABACUS.Salary;
using ABACUS.UserDependentAuth;
using ABACUS.WebShop;
using Microsoft.Extensions.Configuration;
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

    [Fact]
    public void AddAbacusSdk_FromConfiguration_BindsOptions()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(
            """
            {
              "Abacus": {
                "ServerUri": "https://example.invalid",
                "Mandant": "7777",
                "Prefer": "odata.continue-on-error",
                "RateLimitMaxRetries": 1,
                "EnableRequestLogging": true
              }
            }
            """));
        var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();

        var services = new ServiceCollection();
        services.AddAbacusSdk(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<AbacusClientOptions>();

        Assert.Equal("7777", options.Mandant);
        Assert.Equal("odata.continue-on-error", options.Prefer);
        Assert.Equal(1, options.RateLimitMaxRetries);
        Assert.True(options.EnableRequestLogging);
    }

    [Fact]
    public void AddAbacusAllModules_ResolvesEntityFacades()
    {
        var services = new ServiceCollection();
        services.AddAbacusSdk(new AbacusClientOptions
        {
            BaseUri = new Uri("https://example.invalid/api/entity/v1/mandants/7777/"),
        });
        services.AddAbacusAllModules();

        using var provider = services.BuildServiceProvider();

        Assert.Equal("AccountsPayable", provider.GetRequiredService<IAccountsPayableClient>().ModuleName);
        Assert.Equal("General", provider.GetRequiredService<IGeneralClient>().ModuleName);
        Assert.Equal("WebShop", provider.GetRequiredService<IWebShopClient>().ModuleName);
        Assert.Equal("HumanResources", provider.GetRequiredService<IHumanResourcesClient>().ModuleName);
        Assert.Equal("Salary", provider.GetRequiredService<ISalaryClient>().ModuleName);
        Assert.Equal("ProjectManagement", provider.GetRequiredService<IProjectManagementClient>().ModuleName);
        Assert.Equal("ProductionPlanning", provider.GetRequiredService<IProductionPlanningClient>().ModuleName);
        Assert.Equal("DossierFileUpload", provider.GetRequiredService<IDossierFileUploadClient>().ModuleName);
        Assert.Equal("UserDependentAuth", provider.GetRequiredService<IUserDependentAuthClient>().ModuleName);
    }

    [Fact]
    public void AddAbacusAbaReport_ResolvesWithServerOrigin()
    {
        var services = new ServiceCollection();
        services.AddAbacusAbaReport(new Uri("https://example.invalid:40000"));

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IAbaReportClient>();

        Assert.Equal("AbaReport", client.ModuleName);
    }
}
