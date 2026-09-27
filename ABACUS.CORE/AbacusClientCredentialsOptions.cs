using System.Diagnostics.CodeAnalysis;

namespace ABACUS.Core;

/// <summary>
/// Settings used to obtain an ABACUS access token with the OAuth client credentials grant.
/// </summary>
public sealed class AbacusClientCredentialsOptions
{
    /// <summary>
    /// Maximum number of OAuth scopes Abacus accepts in a single token request.
    /// </summary>
    public const int MaxScopes = 25;

    /// <summary>
    /// Initializes an empty options object for later configuration.
    /// </summary>
    [SetsRequiredMembers]
    public AbacusClientCredentialsOptions()
    {
        ServerUri = null!;
        ClientId = "";
        ClientSecret = "";
    }

    /// <summary>
    /// ABACUS server origin, for example <c>https://host:40000</c>.
    /// The OpenID configuration is requested from this origin, not from the API base path.
    /// </summary>
    public required Uri ServerUri { get; set; }

    /// <summary>
    /// Service-user client id.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// Service-user client secret (Clientschlüssel).
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// Token endpoint. When set, OpenID discovery is skipped.
    /// </summary>
    public Uri? TokenEndpoint { get; set; }

    /// <summary>
    /// OAuth scopes requested with the token (maximum <see cref="MaxScopes"/>).
    /// </summary>
    public IList<string> Scopes { get; set; } = new List<string>();

    /// <summary>
    /// How long before expiry a cached access token is renewed.
    /// </summary>
    public TimeSpan RefreshSkew { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Clock used to decide when a cached access token must be renewed.
    /// </summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    internal void Validate()
    {
        if (ServerUri is null)
        {
            throw new ArgumentException("ServerUri is required.", nameof(ServerUri));
        }

        if (!ServerUri.IsAbsoluteUri)
        {
            throw new ArgumentException("ServerUri must be absolute.", nameof(ServerUri));
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new ArgumentException("ClientId is required.", nameof(ClientId));
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new ArgumentException("ClientSecret is required.", nameof(ClientSecret));
        }

        if (TokenEndpoint is not null && !TokenEndpoint.IsAbsoluteUri)
        {
            throw new ArgumentException("TokenEndpoint must be absolute.", nameof(TokenEndpoint));
        }

        if (RefreshSkew < TimeSpan.Zero)
        {
            throw new ArgumentException("RefreshSkew cannot be negative.", nameof(RefreshSkew));
        }

        if (TimeProvider is null)
        {
            throw new ArgumentException("TimeProvider is required.", nameof(TimeProvider));
        }

        if (Scopes is null)
        {
            throw new ArgumentException("Scopes is required.", nameof(Scopes));
        }

        if (Scopes.Count > MaxScopes)
        {
            throw new ArgumentException(
                $"Abacus accepts at most {MaxScopes} scopes per token request.",
                nameof(Scopes));
        }

        foreach (var scope in Scopes)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("Scopes cannot contain blank values.", nameof(Scopes));
            }
        }
    }
}
