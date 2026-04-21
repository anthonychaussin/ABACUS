using ABACUS.Core;

namespace ABACUS.Subscription;

/// <summary>
/// Default implementation for the Subscription module wrapper.
/// </summary>
public sealed class SubscriptionClient : ISubscriptionClient
{
    /// <inheritdoc />
    public string ModuleName => "Subscription";

    /// <inheritdoc />
    public ABACUS_SubscriptionClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public SubscriptionClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_SubscriptionClient(httpClient);
    }
}
