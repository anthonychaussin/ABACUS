using ABACUS.Core;

namespace ABACUS.Subscription;

/// <summary>
/// Default implementation for the Subscription module wrapper.
/// </summary>
public sealed class SubscriptionClient : ISubscriptionClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "Subscription";

    /// <inheritdoc />
    public ABACUS_SubscriptionClient Raw { get; }

    /// <inheritdoc />
    public IAbacusChangeFeed Changes { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public SubscriptionClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_SubscriptionClient(httpClient);
        Changes = new AbacusChangeFeed(httpClient);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> ListSubscriptionsAsync(CancellationToken cancellationToken = default) =>
        AbacusHttp.SendAsync(_httpClient, HttpMethod.Get, "/Subscriptions", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> GetMetadataAsync(CancellationToken cancellationToken = default) =>
        AbacusHttp.SendAsync(_httpClient, HttpMethod.Get, "/$metadata", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task SubscribeAsync(
        string subscriptionName,
        IEnumerable<string> topics,
        CancellationToken cancellationToken = default) =>
        Changes.SubscribeAsync(subscriptionName, topics, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusChangeBatch> ConsumeAsync(
        string subscriptionName,
        CancellationToken cancellationToken = default) =>
        Changes.ConsumeAsync(subscriptionName, cancellationToken);

    /// <inheritdoc />
    public Task AcknowledgeAsync(
        string subscriptionName,
        string acknowledgeKey,
        CancellationToken cancellationToken = default) =>
        Changes.AcknowledgeAsync(subscriptionName, acknowledgeKey, cancellationToken);

    /// <inheritdoc />
    public Task UnsubscribeAsync(string subscriptionName, CancellationToken cancellationToken = default) =>
        Changes.UnsubscribeAsync(subscriptionName, cancellationToken);

    /// <inheritdoc />
    public async Task SubscribeToChangesAsync(object payload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        await AbacusHttp.SendAsync(
                _httpClient,
                HttpMethod.Post,
                "/SubscribeChanges",
                payload,
                prefer: null,
                cancellationToken)
            .ConfigureAwait(false);
    }
}
