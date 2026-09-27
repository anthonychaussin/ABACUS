using ABACUS.AccountsPayable;
using ABACUS.Core;
using ABACUS.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// Demo wiring only — replace BaseUri / credentials for a real Abacus server.
var services = new ServiceCollection();
services.AddAbacusSdk(new AbacusClientOptions
{
    BaseUri = new Uri("https://example.invalid/api/entity/v1/mandants/7777/"),
});
services.AddAbacusModuleClient<IAccountsPayableClient, AccountsPayableClient>();

using var provider = services.BuildServiceProvider();
var ap = provider.GetRequiredService<IAccountsPayableClient>();

Console.WriteLine($"Module ready: {ap.ModuleName}");
Console.WriteLine("Call ListSuppliersAsync(ODataQuery.Create().Top(10)) against a live server.");
