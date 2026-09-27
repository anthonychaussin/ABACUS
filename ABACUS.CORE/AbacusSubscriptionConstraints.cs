namespace ABACUS.Core;

/// <summary>
/// Validation helpers for Abacus change-subscription names and topics.
/// </summary>
public static class AbacusSubscriptionConstraints
{
    /// <summary>
    /// Maximum number of subscriptions per service user.
    /// </summary>
    public const int MaxSubscriptionsPerUser = 10;

    /// <summary>
    /// Maximum length of a subscription name (URL-safe).
    /// </summary>
    public const int MaxSubscriptionNameLength = 23;

    /// <summary>
    /// Validates a subscription name against Abacus constraints.
    /// </summary>
    public static string ValidateName(string subscriptionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        var trimmed = subscriptionName.Trim();
        if (trimmed.Length > MaxSubscriptionNameLength)
        {
            throw new ArgumentException(
                $"Subscription name cannot exceed {MaxSubscriptionNameLength} characters.",
                nameof(subscriptionName));
        }

        foreach (var ch in trimmed)
        {
            if (!(char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_' or '.'))
            {
                throw new ArgumentException(
                    "Subscription name must be URL-compatible (letters, digits, '-', '_', '.').",
                    nameof(subscriptionName));
            }
        }

        return trimmed;
    }

    /// <summary>
    /// Validates that at least one topic is provided.
    /// </summary>
    public static IReadOnlyList<string> ValidateTopics(IEnumerable<string> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);
        var list = topics
            .Select(topic =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(topic);
                return topic.Trim();
            })
            .ToArray();

        if (list.Length == 0)
        {
            throw new ArgumentException("At least one topic is required.", nameof(topics));
        }

        return list;
    }
}
