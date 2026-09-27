using System.Text.Json;

namespace ABACUS.Core;

/// <summary>
/// Result of a <c>ConsumeChanges</c> call.
/// </summary>
public sealed class AbacusChangeBatch
{
    /// <summary>
    /// Creates a change batch.
    /// </summary>
    public AbacusChangeBatch(
        string subscriptionName,
        IReadOnlyList<JsonElement> changes,
        string? acknowledgeKey,
        string rawJson,
        int statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        SubscriptionName = subscriptionName;
        Changes = changes;
        AcknowledgeKey = acknowledgeKey;
        RawJson = rawJson;
        StatusCode = statusCode;
        Headers = headers;
    }

    /// <summary>
    /// Subscription that was consumed.
    /// </summary>
    public string SubscriptionName { get; }

    /// <summary>
    /// Change payloads (best-effort extraction from common Abacus shapes).
    /// </summary>
    public IReadOnlyList<JsonElement> Changes { get; }

    /// <summary>
    /// Key required by <c>AcknowledgeChanges</c>, when present.
    /// </summary>
    public string? AcknowledgeKey { get; }

    /// <summary>
    /// Raw response body.
    /// </summary>
    public string RawJson { get; }

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Response headers.
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// Whether the batch contains changes.
    /// </summary>
    public bool HasChanges => Changes.Count > 0;
}

/// <summary>
/// Polls Abacus change subscriptions (subscribe / consume / acknowledge).
/// </summary>
public interface IAbacusChangeFeed
{
    /// <summary>
    /// Registers a subscription for one or more topics.
    /// </summary>
    Task SubscribeAsync(string subscriptionName, IEnumerable<string> topics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes pending changes for a subscription.
    /// </summary>
    Task<AbacusChangeBatch> ConsumeAsync(string subscriptionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges a previously consumed batch.
    /// </summary>
    Task AcknowledgeAsync(string subscriptionName, string acknowledgeKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a subscription.
    /// </summary>
    Task UnsubscribeAsync(string subscriptionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes batches until empty, acknowledging each non-empty batch.
    /// </summary>
    IAsyncEnumerable<AbacusChangeBatch> ConsumeUntilEmptyAsync(
        string subscriptionName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default <see cref="IAbacusChangeFeed"/> using OData subscription endpoints.
/// </summary>
public sealed class AbacusChangeFeed : IAbacusChangeFeed
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Creates a change feed over a configured ABACUS <see cref="HttpClient"/>.
    /// </summary>
    public AbacusChangeFeed(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task SubscribeAsync(
        string subscriptionName,
        IEnumerable<string> topics,
        CancellationToken cancellationToken = default)
    {
        var name = AbacusSubscriptionConstraints.ValidateName(subscriptionName);
        var topicList = AbacusSubscriptionConstraints.ValidateTopics(topics);

        object payload = topicList.Count == 1
            ? new { Subscription = name, Topic = topicList[0] }
            : new { Subscription = name, Topics = topicList };

        await AbacusHttp.SendAsync(
                _httpClient,
                HttpMethod.Post,
                "/SubscribeChanges",
                payload,
                prefer: null,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<AbacusChangeBatch> ConsumeAsync(
        string subscriptionName,
        CancellationToken cancellationToken = default)
    {
        var name = AbacusSubscriptionConstraints.ValidateName(subscriptionName);
        var path = $"/Subscriptions('{EscapeODataString(name)}')/ch.abacus.df.ConsumeChanges()";
        var response = await AbacusHttp.SendAsync(
                _httpClient,
                HttpMethod.Get,
                path,
                payload: null,
                prefer: null,
                cancellationToken)
            .ConfigureAwait(false);

        return ParseBatch(name, response);
    }

    /// <inheritdoc />
    public async Task AcknowledgeAsync(
        string subscriptionName,
        string acknowledgeKey,
        CancellationToken cancellationToken = default)
    {
        var name = AbacusSubscriptionConstraints.ValidateName(subscriptionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(acknowledgeKey);

        var path = $"/Subscriptions('{EscapeODataString(name)}')/ch.abacus.df.AcknowledgeChanges";
        await AbacusHttp.SendAsync(
                _httpClient,
                HttpMethod.Post,
                path,
                new { AcknowledgeKey = acknowledgeKey.Trim() },
                prefer: null,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UnsubscribeAsync(string subscriptionName, CancellationToken cancellationToken = default)
    {
        var name = AbacusSubscriptionConstraints.ValidateName(subscriptionName);
        var path = $"/Subscriptions('{EscapeODataString(name)}')/ch.abacus.df.UnsubscribeChanges";
        await AbacusHttp.SendAsync(
                _httpClient,
                HttpMethod.Post,
                path,
                payload: new { },
                prefer: null,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<AbacusChangeBatch> ConsumeUntilEmptyAsync(
        string subscriptionName,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var batch = await ConsumeAsync(subscriptionName, cancellationToken).ConfigureAwait(false);
            if (!batch.HasChanges)
            {
                yield break;
            }

            yield return batch;

            if (!string.IsNullOrWhiteSpace(batch.AcknowledgeKey))
            {
                await AcknowledgeAsync(subscriptionName, batch.AcknowledgeKey, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static AbacusChangeBatch ParseBatch(string subscriptionName, AbacusResponse<string> response)
    {
        var changes = new List<JsonElement>();
        string? acknowledgeKey = null;

        if (!string.IsNullOrWhiteSpace(response.Data))
        {
            using var document = JsonDocument.Parse(response.Data);
            var root = document.RootElement.Clone();

            if (TryGetString(root, "AcknowledgeKey", out var key) ||
                TryGetString(root, "acknowledgeKey", out key))
            {
                acknowledgeKey = key;
            }

            if (root.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                {
                    changes.Add(item.Clone());
                    if (acknowledgeKey is null &&
                        (TryGetString(item, "AcknowledgeKey", out key) || TryGetString(item, "acknowledgeKey", out key)))
                    {
                        acknowledgeKey = key;
                    }
                }
            }
            else if (root.TryGetProperty("Changes", out var changesElement) &&
                     changesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in changesElement.EnumerateArray())
                {
                    changes.Add(item.Clone());
                }
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    changes.Add(item.Clone());
                }
            }
            else if (root.ValueKind == JsonValueKind.Object && root.EnumerateObject().Any())
            {
                // Non-empty object without a known collection shape still counts as a change payload.
                changes.Add(root);
            }
        }

        return new AbacusChangeBatch(
            subscriptionName,
            changes,
            acknowledgeKey,
            response.Data,
            response.StatusCode,
            response.Headers);
    }

    private static bool TryGetString(JsonElement element, string name, out string? value)
    {
        value = null;
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(name, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string EscapeODataString(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}
