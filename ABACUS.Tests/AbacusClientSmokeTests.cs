using ABACUS;
using ABACUS.Core;

namespace ABACUS.Tests;

public sealed class AbacusClientSmokeTests
{
    [Fact]
    public void Constructor_InitializesAllModuleClients()
    {
        using var client = new AbacusClient(new AbacusClientOptions
        {
            BaseUri = new Uri("https://example.invalid"),
        });

        Assert.NotNull(client.AccountsPayable);
        Assert.NotNull(client.AccountsReceivable);
        Assert.NotNull(client.AssetsLedger);
        Assert.NotNull(client.CRM);
        Assert.NotNull(client.DossierFileUpload);
        Assert.NotNull(client.FieldInformation);
        Assert.NotNull(client.Finance);
        Assert.NotNull(client.General);
        Assert.NotNull(client.HumanResources);
        Assert.NotNull(client.ProductionPlanning);
        Assert.NotNull(client.ProjectManagement);
        Assert.NotNull(client.RealEstate);
        Assert.NotNull(client.Salary);
        Assert.NotNull(client.Subscription);
        Assert.NotNull(client.UserDependentAuth);
        Assert.NotNull(client.WebShop);
    }
}
