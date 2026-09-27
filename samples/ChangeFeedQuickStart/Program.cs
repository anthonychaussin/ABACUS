using ABACUS.Core;
using ABACUS.Subscription;

// Pattern for change-feed sync — requires a live Abacus HttpClient with auth.
Console.WriteLine("Change feed sample (wire HttpClient to your Abacus base URI first).");
Console.WriteLine("""
  var subscriptions = new SubscriptionClient(httpClient);
  await subscriptions.SubscribeAsync("my-sync", ["Subject"]);
  await foreach (var batch in subscriptions.Changes.ConsumeUntilEmptyAsync("my-sync"))
  {
      // process batch.Changes then acknowledge
  }
""");
