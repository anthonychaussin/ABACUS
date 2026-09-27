using ABACUS.Core;
using ABACUS.General;
using Microsoft.Extensions.DependencyInjection;

// Sketch: OData $batch via GeneralClient (wire a real HttpClient for live calls).
var services = new ServiceCollection();
services.AddAbacusSdk(new AbacusClientOptions
{
    ServerUri = new Uri("https://example.abacus:40000"),
    Mandant = "7777",
});
services.AddAbacusModuleClient<IGeneralClient, GeneralClient>();

Console.WriteLine("Build a batch:");
var batch = new ODataBatchRequest()
    .Get("Suppliers?$top=5")
    .Get("Customers?$top=5");
Console.WriteLine($"Operations: {batch.Operations.Count}");
Console.WriteLine("Call general.PostBatchAsync(batch) with a live HttpClient from DI.");
