using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

namespace ABACUS.Core;

/// <summary>
/// Obtains and caches an ABACUS bearer token from a service-user client id and client secret.
/// </summary>
public sealed class ClientCredentialsAuthenticationProvider : IAbacusAuthenticationProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly AbacusClientCredentialsOptions _options;
    private readonly Uri _serverAuthority;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Uri? _tokenEndpoint;
    private CachedAccessToken? _cached;

    /// <summary>
    /// Creates a provider that owns its token <see cref="HttpClient"/>.
    /// </summary>
    public ClientCredentialsAuthenticationProvider(Uri serverUri, string clientId, string clientSecret)
        : this(new AbacusClientCredentialsOptions
        {
            ServerUri = serverUri,
            ClientId = clientId,
            ClientSecret = clientSecret,
        })
    {
    }

    /// <summary>
    /// Creates a provider that owns its token <see cref="HttpClient"/>.
    /// </summary>
    public ClientCredentialsAuthenticationProvider(AbacusClientCredentialsOptions options)
        : this(options, new HttpClient(), ownsHttpClient: true)
    {
    }

    /// <summary>
    /// Creates a provider that sends token requests with the supplied client.
    /// The client is not disposed with this provider and must not use <see cref="AbacusAuthenticationHandler"/>.
    /// </summary>
    public ClientCredentialsAuthenticationProvider(AbacusClientCredentialsOptions options, HttpClient httpClient)
        : this(options, httpClient, ownsHttpClient: false)
    {
    }

    private ClientCredentialsAuthenticationProvider(AbacusClientCredentialsOptions options, HttpClient httpClient, bool ownsHttpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        options.Validate();

        _options = options;
        _httpClient = httpClient;
        _ownsHttpClient = ownsHttpClient;
        _serverAuthority = new Uri(options.ServerUri.GetLeftPart(UriPartial.Authority));
        _tokenEndpoint = options.TokenEndpoint;
    }

    /// <inheritdoc />
    public async ValueTask ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Releases the refresh gate and, when this instance created it, the token <see cref="HttpClient"/>.
    /// </summary>
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

            var endpoint = await GetTokenEndpointAsync(cancellationToken).ConfigureAwait(false);
            var refreshed = await RequestAccessTokenAsync(endpoint, cancellationToken).ConfigureAwait(false);
            WriteCached(refreshed);
            return refreshed.AccessToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsUsable(CachedAccessToken? cached) =>
        cached is not null && _options.TimeProvider.GetUtcNow() < cached.RefreshAt;

    private async ValueTask<Uri> GetTokenEndpointAsync(CancellationToken cancellationToken)
    {
        if (_tokenEndpoint is not null)
        {
            return _tokenEndpoint;
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
        if (configuration?.TokenEndpoint is null ||
            !Uri.TryCreate(configuration.TokenEndpoint, UriKind.Absolute, out var tokenEndpoint))
        {
            throw new InvalidOperationException("Abacus OpenID configuration did not contain a token_endpoint.");
        }

        _tokenEndpoint = tokenEndpoint;
        return tokenEndpoint;
    }

    private async ValueTask<CachedAccessToken> RequestAccessTokenAsync(Uri tokenEndpoint, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", CreateBasicCredential());
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
        };
        if (_options.Scopes.Count > 0)
        {
            form["scope"] = string.Join(' ', _options.Scopes);
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

        return new CachedAccessToken(token.AccessToken, ComputeRefreshAt(token.ExpiresIn));
    }

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
        var raw = $"{FormEncode(_options.ClientId)}:{FormEncode(_options.ClientSecret)}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    private string FormatBody(string? body)
    {
        var redacted = Redact(body);
        return string.IsNullOrWhiteSpace(redacted) ? string.Empty : $" Response: {redacted}";
    }

    private string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace(_options.ClientSecret, "***", StringComparison.Ordinal);
    }

    private CachedAccessToken? ReadCached() => Volatile.Read(ref _cached);

    private void WriteCached(CachedAccessToken value) => Volatile.Write(ref _cached, value);

    private static string FormEncode(string value) => Uri.EscapeDataString(value);

    private sealed class CachedAccessToken
    {
        public CachedAccessToken(string accessToken, DateTimeOffset refreshAt)
        {
            AccessToken = accessToken;
            RefreshAt = refreshAt;
        }

        public string AccessToken { get; }

        public DateTimeOffset RefreshAt { get; }
    }

    private sealed record OpenIdConfiguration(
        [property: JsonPropertyName("token_endpoint")] string? TokenEndpoint);

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
