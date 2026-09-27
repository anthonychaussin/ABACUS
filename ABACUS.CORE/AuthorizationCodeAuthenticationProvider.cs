using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

namespace ABACUS.Core;

/// <summary>
/// Obtains and refreshes ABACUS bearer tokens with the OAuth authorization-code grant.
/// </summary>
public sealed class AuthorizationCodeAuthenticationProvider : IAbacusAuthenticationProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly AbacusAuthorizationCodeOptions _options;
    private readonly Uri _serverAuthority;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Uri? _tokenEndpoint;
    private Uri? _authorizationEndpoint;
    private CachedTokens? _cached;

    /// <summary>
    /// Creates a provider that owns its token <see cref="HttpClient"/>.
    /// </summary>
    public AuthorizationCodeAuthenticationProvider(AbacusAuthorizationCodeOptions options)
        : this(options, new HttpClient(), ownsHttpClient: true)
    {
    }

    /// <summary>
    /// Creates a provider that sends token requests with the supplied client.
    /// </summary>
    public AuthorizationCodeAuthenticationProvider(AbacusAuthorizationCodeOptions options, HttpClient httpClient)
        : this(options, httpClient, ownsHttpClient: false)
    {
    }

    private AuthorizationCodeAuthenticationProvider(
        AbacusAuthorizationCodeOptions options,
        HttpClient httpClient,
        bool ownsHttpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        options.Validate();

        _options = options;
        _httpClient = httpClient;
        _ownsHttpClient = ownsHttpClient;
        _serverAuthority = new Uri(options.ServerUri.GetLeftPart(UriPartial.Authority));
        _tokenEndpoint = options.TokenEndpoint;
        _authorizationEndpoint = options.AuthorizationEndpoint;
    }

    /// <summary>
    /// Builds the authorize URL that the user must open to sign in to Abacus.
    /// </summary>
    public async ValueTask<Uri> CreateAuthorizationUrlAsync(
        string state,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        var endpoint = await GetAuthorizationEndpointAsync(cancellationToken).ConfigureAwait(false);
        var parts = new List<string>
        {
            "response_type=code",
            "client_id=" + Uri.EscapeDataString(_options.ClientId),
            "redirect_uri=" + Uri.EscapeDataString(_options.RedirectUri.AbsoluteUri),
            "state=" + Uri.EscapeDataString(state),
        };
        if (_options.Scopes.Count > 0)
        {
            parts.Add("scope=" + Uri.EscapeDataString(string.Join(' ', _options.Scopes)));
        }

        var builder = new UriBuilder(endpoint) { Query = string.Join('&', parts) };
        return builder.Uri;
    }

    /// <summary>
    /// Exchanges an authorization code for access (and optional refresh) tokens.
    /// </summary>
    public async ValueTask ExchangeCodeAsync(string authorizationCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorizationCode);
        var endpoint = await GetTokenEndpointAsync(cancellationToken).ConfigureAwait(false);
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = authorizationCode.Trim(),
            ["redirect_uri"] = _options.RedirectUri.AbsoluteUri,
        };

        var tokens = await RequestTokensAsync(endpoint, form, cancellationToken).ConfigureAwait(false);
        WriteCached(tokens);
    }

    /// <summary>
    /// Seeds the provider with tokens obtained elsewhere.
    /// </summary>
    public void SetTokens(string accessToken, string? refreshToken, int expiresInSeconds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        WriteCached(new CachedTokens(accessToken, refreshToken, ComputeRefreshAt(expiresInSeconds)));
    }

    /// <inheritdoc />
    public async ValueTask ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _gate.Dispose();
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var cached = ReadCached();
        if (IsUsable(cached))
        {
            return cached!.AccessToken;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cached = ReadCached();
            if (IsUsable(cached))
            {
                return cached!.AccessToken;
            }

            if (string.IsNullOrWhiteSpace(cached?.RefreshToken))
            {
                throw new InvalidOperationException(
                    "No usable access token is cached. Call ExchangeCodeAsync or SetTokens first.");
            }

            var endpoint = await GetTokenEndpointAsync(cancellationToken).ConfigureAwait(false);
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = cached.RefreshToken!,
            };
            var refreshed = await RequestTokensAsync(endpoint, form, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(refreshed.RefreshToken))
            {
                refreshed = refreshed with { RefreshToken = cached.RefreshToken };
            }

            WriteCached(refreshed);
            return refreshed.AccessToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async ValueTask<CachedTokens> RequestTokensAsync(
        Uri tokenEndpoint,
        Dictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", CreateBasicCredential());
        if (_options.Scopes.Count > 0)
        {
            form.TryAdd("scope", string.Join(' ', _options.Scopes));
        }

        request.Content = new FormUrlEncodedContent(form);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Abacus token request failed with status code {(int)response.StatusCode}.{FormatBody(body)}");
        }

        var token = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(body);
        if (string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            throw new InvalidOperationException("Abacus token response did not contain an access_token.");
        }

        return new CachedTokens(token.AccessToken, token.RefreshToken, ComputeRefreshAt(token.ExpiresIn));
    }

    private async ValueTask EnsureDiscoveryAsync(CancellationToken cancellationToken)
    {
        if (_tokenEndpoint is not null && _authorizationEndpoint is not null)
        {
            return;
        }

        var discoveryUri = new Uri(_serverAuthority, "/.well-known/openid-configuration");
        using var request = new HttpRequestMessage(HttpMethod.Get, discoveryUri);
        request.Headers.Accept.ParseAdd("application/json");
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Abacus OpenID configuration request failed with status code {(int)response.StatusCode}.{FormatBody(body)}");
        }

        var configuration = System.Text.Json.JsonSerializer.Deserialize<OpenIdConfiguration>(body);
        if (_tokenEndpoint is null)
        {
            if (configuration?.TokenEndpoint is null ||
                !Uri.TryCreate(configuration.TokenEndpoint, UriKind.Absolute, out var tokenEndpoint))
            {
                throw new InvalidOperationException("Abacus OpenID configuration did not contain a token_endpoint.");
            }

            _tokenEndpoint = tokenEndpoint;
        }

        if (_authorizationEndpoint is null)
        {
            if (configuration?.AuthorizationEndpoint is null ||
                !Uri.TryCreate(configuration.AuthorizationEndpoint, UriKind.Absolute, out var authorizationEndpoint))
            {
                throw new InvalidOperationException(
                    "Abacus OpenID configuration did not contain an authorization_endpoint.");
            }

            _authorizationEndpoint = authorizationEndpoint;
        }
    }

    private async ValueTask<Uri> GetTokenEndpointAsync(CancellationToken cancellationToken)
    {
        await EnsureDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        return _tokenEndpoint!;
    }

    private async ValueTask<Uri> GetAuthorizationEndpointAsync(CancellationToken cancellationToken)
    {
        await EnsureDiscoveryAsync(cancellationToken).ConfigureAwait(false);
        return _authorizationEndpoint!;
    }

    private bool IsUsable(CachedTokens? cached) =>
        cached is not null && _options.TimeProvider.GetUtcNow() < cached.RefreshAt;

    private DateTimeOffset ComputeRefreshAt(int expiresInSeconds)
    {
        var now = _options.TimeProvider.GetUtcNow();
        if (expiresInSeconds <= 0)
        {
            return now;
        }

        var lifetime = TimeSpan.FromSeconds(expiresInSeconds);
        var skew = _options.RefreshSkew;
        if (skew >= lifetime)
        {
            skew = TimeSpan.FromTicks(lifetime.Ticks / 2);
        }

        return now + (lifetime - skew);
    }

    private string CreateBasicCredential()
    {
        var raw = $"{Uri.EscapeDataString(_options.ClientId)}:{Uri.EscapeDataString(_options.ClientSecret)}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    private string FormatBody(string? body)
    {
        var redacted = string.IsNullOrEmpty(body)
            ? string.Empty
            : body.Replace(_options.ClientSecret, "***", StringComparison.Ordinal);
        return string.IsNullOrWhiteSpace(redacted) ? string.Empty : $" Response: {redacted}";
    }

    private CachedTokens? ReadCached() => Volatile.Read(ref _cached);

    private void WriteCached(CachedTokens value) => Volatile.Write(ref _cached, value);

    private sealed record CachedTokens(string AccessToken, string? RefreshToken, DateTimeOffset RefreshAt);

    private sealed record OpenIdConfiguration(
        [property: JsonPropertyName("token_endpoint")] string? TokenEndpoint,
        [property: JsonPropertyName("authorization_endpoint")] string? AuthorizationEndpoint);

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
