using ABACUS.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Sketch: hosted change feed worker (MaxEmptyCycles for demo exit).
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(l => l.AddConsole());
builder.Services.AddAbacusSdk(new AbacusClientOptions
{
    ServerUri = new Uri("https://example.abacus:40000"),
    Mandant = "7777",
});
builder.Services.AddAbacusChangeFeedHostedService(o =>
{
    o.SubscriptionName = "demo-feed";
    o.Topics.Add("Subject");
    o.PollInterval = TimeSpan.FromSeconds(2);
    o.MaxEmptyCycles = 1;
    o.SubscribeOnStart = false; // set true against a live Abacus
});

Console.WriteLine("Hosted change feed registered. Run against a live Abacus with SubscribeOnStart=true.");
using var host = builder.Build();
// await host.RunAsync();
