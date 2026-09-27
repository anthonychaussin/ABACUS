using ABACUS.Core;

namespace ABACUS.Subscription;

/// <summary>
/// High-level SDK facade for the Subscription module.
/// </summary>
public interface ISubscriptionClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_SubscriptionClient Raw { get; }

    /// <summary>
    /// Change-feed helper (subscribe / consume / acknowledge).
    /// </summary>
    IAbacusChangeFeed Changes { get; }

    /// <summary>
    /// Lists current subscriptions.
    /// </summary>
    Task<AbacusResponse<string>> ListSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves OData metadata.
    /// </summary>
    Task<AbacusResponse<string>> GetMetadataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a subscription for the supplied topics.
    /// </summary>
    Task SubscribeAsync(string subscriptionName, IEnumerable<string> topics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes pending changes for a subscription.
    /// </summary>
    Task<AbacusChangeBatch> ConsumeAsync(string subscriptionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges a consumed batch.
    /// </summary>
    Task AcknowledgeAsync(string subscriptionName, string acknowledgeKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a subscription.
    /// </summary>
    Task UnsubscribeAsync(string subscriptionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to one or more change topics using a JSON-compatible payload.
    /// Prefer <see cref="SubscribeAsync"/> for validated names and topics.
    /// </summary>
    Task SubscribeToChangesAsync(object payload, CancellationToken cancellationToken = default);
}
