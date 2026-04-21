namespace ABACUS.Core;

/// <summary>
/// Default bearer-token authentication provider.
/// </summary>
public sealed class BearerTokenAuthenticationProvider : IAbacusAuthenticationProvider
{
    private readonly Func<CancellationToken, ValueTask<string>> _getToken;

    /// <summary>
    /// Creates an auth provider from a token factory.
    /// </summary>
    public BearerTokenAuthenticationProvider(Func<CancellationToken, ValueTask<string>> getToken)
    {
        _getToken = getToken ?? throw new ArgumentNullException(nameof(getToken));
    }

    /// <inheritdoc />
    public async ValueTask ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var token = await _getToken(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Authentication provider returned an empty bearer token.");
        }

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
