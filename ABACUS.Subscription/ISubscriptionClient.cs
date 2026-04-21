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
}
