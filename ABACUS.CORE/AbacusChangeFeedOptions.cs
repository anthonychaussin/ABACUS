namespace ABACUS.Core;

/// <summary>
/// Options for <see cref="AbacusChangeFeedHostedService"/>.
/// </summary>
public sealed class AbacusChangeFeedOptions
{
    /// <summary>
    /// Subscription name (max 23 URL-safe characters).
    /// </summary>
    public string SubscriptionName { get; set; } = "abacus-sdk-feed";

    /// <summary>
    /// Topics to subscribe (for example <c>Subject</c>).
    /// </summary>
    public IList<string> Topics { get; set; } = new List<string>();

    /// <summary>
    /// Delay between empty poll cycles.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// When greater than 0, stops after this many consecutive empty batches (useful for samples/tests).
    /// Zero means run until cancellation.
    /// </summary>
    public int MaxEmptyCycles { get; set; }

    /// <summary>
    /// When true, calls Subscribe on start.
    /// </summary>
    public bool SubscribeOnStart { get; set; } = true;
}
