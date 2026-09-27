using System.Diagnostics.CodeAnalysis;

namespace ABACUS.Core;

/// <summary>
/// Settings for the OAuth authorization-code flow (user-dependent Abacus access).
/// </summary>
public sealed class AbacusAuthorizationCodeOptions
{
    /// <summary>
    /// Initializes an empty options object for later configuration.
    /// </summary>
    [SetsRequiredMembers]
    public AbacusAuthorizationCodeOptions()
    {
        ServerUri = null!;
        ClientId = "";
        ClientSecret = "";
        RedirectUri = null!;
    }

    /// <summary>
    /// ABACUS server origin, for example <c>https://host:40000</c>.
    /// </summary>
    public required Uri ServerUri { get; set; }

    /// <summary>
    /// OAuth client id.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// OAuth client secret.
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// Redirect URI registered for the authorization-code flow.
    /// </summary>
    public required Uri RedirectUri { get; set; }

    /// <summary>
    /// Optional scopes (maximum <see cref="AbacusClientCredentialsOptions.MaxScopes"/>).
    /// Include <c>offline_access</c> to request a refresh token.
    /// </summary>
    public IList<string> Scopes { get; set; } = new List<string>();

    /// <summary>
    /// Token endpoint. When set, OpenID discovery is skipped for token requests.
    /// </summary>
    public Uri? TokenEndpoint { get; set; }

    /// <summary>
    /// Authorization endpoint. When set, OpenID discovery is skipped for authorize URLs.
    /// </summary>
    public Uri? AuthorizationEndpoint { get; set; }

    /// <summary>
    /// How long before expiry a cached access token is renewed.
    /// </summary>
    public TimeSpan RefreshSkew { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Clock used for token expiry.
    /// </summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    internal void Validate()
    {
        if (ServerUri is null || !ServerUri.IsAbsoluteUri)
        {
            throw new ArgumentException("ServerUri must be an absolute URI.", nameof(ServerUri));
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new ArgumentException("ClientId is required.", nameof(ClientId));
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new ArgumentException("ClientSecret is required.", nameof(ClientSecret));
        }

        if (RedirectUri is null || !RedirectUri.IsAbsoluteUri)
        {
            throw new ArgumentException("RedirectUri must be an absolute URI.", nameof(RedirectUri));
        }

        if (Scopes.Count > AbacusClientCredentialsOptions.MaxScopes)
        {
            throw new ArgumentException(
                $"Abacus accepts at most {AbacusClientCredentialsOptions.MaxScopes} scopes per token request.",
                nameof(Scopes));
        }
    }
}
