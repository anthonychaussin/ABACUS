namespace ABACUS.Core;

/// <summary>
/// Delegating handler that applies authentication to outgoing ABACUS requests.
/// </summary>
public sealed class AbacusAuthenticationHandler : DelegatingHandler
{
    private readonly IAbacusAuthenticationProvider _authenticationProvider;

    public AbacusAuthenticationHandler(IAbacusAuthenticationProvider authenticationProvider)
    {
        _authenticationProvider = authenticationProvider ?? throw new ArgumentNullException(nameof(authenticationProvider));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _authenticationProvider.ApplyAsync(request, cancellationToken).ConfigureAwait(false);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
