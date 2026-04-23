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

    /// <inheritdoc />
    public async Task ListSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.Get_subscriptionsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <inheritdoc />
    public async Task GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.Get_metadataAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <inheritdoc />
    public async Task ConsumeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.Get_consume_idAsync("TEST", cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <inheritdoc />
    public async Task SubscribeToChangesAsync(object payload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);

        try
        {
            await Raw.Post_SubscribeChangesAsync(payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }
}
