using ABACUS.Core;

namespace ABACUS.Subscription;

/// <summary>
/// High-level SDK facade for the Subscription module.
/// </summary>
public interface ISubscriptionClient : IAbacusModuleClient
{
    /// <summary>
    /// Lists current subscriptions.
    /// </summary>
    Task ListSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves subscription metadata.
    /// </summary>
    Task GetMetadataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes pending events from the default sample topic.
    /// </summary>
    Task ConsumeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to one or more change topics using a JSON-compatible payload.
    /// </summary>
    Task SubscribeToChangesAsync(object payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_SubscriptionClient Raw { get; }
}
