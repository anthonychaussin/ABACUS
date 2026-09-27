using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.Tests.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ABACUS.Tests;

public sealed class AuthenticationTests
{
    [Fact]
    public async Task BearerTokenAuthenticationProvider_ApplyAsync_SetsBearerAuthorizationHeader()
    {
        var provider = new BearerTokenAuthenticationProvider(_ => ValueTask.FromResult("token-123"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/test");

        await provider.ApplyAsync(request);

        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
        Assert.Equal("token-123", request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task BearerTokenAuthenticationProvider_ApplyAsync_ForwardsCancellationToken()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var seenToken = CancellationToken.None;
        var provider = new BearerTokenAuthenticationProvider(token =>
        {
            seenToken = token;
            return ValueTask.FromResult("token-123");
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/test");
        await provider.ApplyAsync(request, cancellationToken);

        Assert.Equal(cancellationToken, seenToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task BearerTokenAuthenticationProvider_ApplyAsync_ThrowsForBlankToken(string token)
    {
        var provider = new BearerTokenAuthenticationProvider(_ => ValueTask.FromResult(token));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.invalid/test");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.ApplyAsync(request).AsTask());

        Assert.Contains("empty bearer token", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AbacusAuthenticationHandler_SendAsync_AppliesAuthenticationBeforeSending()
    {
        using var innerHandler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(new AbacusAuthenticationHandler(
            new BearerTokenAuthenticationProvider(_ => ValueTask.FromResult("token-123")))
        {
            InnerHandler = innerHandler,
        });

        using var response = await httpClient.GetAsync("https://example.invalid/test");

        var request = Assert.Single(innerHandler.Requests);
        Assert.Equal("Bearer token-123", request.Authorization);
    }

    [Fact]
    public async Task AbacusAuthenticationHandler_SendAsync_ForwardsCancellationTokenToProviderAndInnerHandler()
    {
        var provider = new RecordingAuthenticationProvider();
        using var innerHandler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(new AbacusAuthenticationHandler(provider)
        {
            InnerHandler = innerHandler,
        });
        using var cancellationSource = new CancellationTokenSource();

        using var response = await httpClient.GetAsync("https://example.invalid/test", cancellationSource.Token);

        Assert.True(provider.SeenToken.CanBeCanceled);
        Assert.True(Assert.Single(innerHandler.Requests).CancellationToken.CanBeCanceled);
    }

    private sealed class RecordingAuthenticationProvider : IAbacusAuthenticationProvider
    {
        public CancellationToken SeenToken { get; private set; }

        public ValueTask ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            SeenToken = cancellationToken;
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token-123");
            return ValueTask.CompletedTask;
        }
    }
}

public sealed class ClientCredentialsAuthenticationTests
{
    private const string ClientId = "client-id";
    private const string ClientSecret = "super-secret";

    [Fact]
    public async Task ApplyAsync_DiscoversTokenEndpointAndSetsBearerHeader()
    {
        using var scope = CreateProvider(new CapturingHttpMessageHandler((request, _) => Respond(request)));
        var handler = scope.Handler;

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api/suppliers");
        await scope.Provider.ApplyAsync(request);

        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("token-123", request.Headers.Authorization?.Parameter);

        var discovery = Assert.Single(handler.Requests, captured => captured.Method == HttpMethod.Get);
        Assert.Equal("https://abacus.example/.well-known/openid-configuration", discovery.RequestUri?.AbsoluteUri);

        var tokenRequest = Assert.Single(handler.Requests, captured => captured.Method == HttpMethod.Post);
        Assert.Equal("https://abacus.example/oauth/oauth2/v1/token", tokenRequest.RequestUri?.AbsoluteUri);
        Assert.Equal("grant_type=client_credentials", tokenRequest.Body);
        Assert.Equal("Basic " + BasicCredential(ClientId, ClientSecret), tokenRequest.Authorization);
        Assert.DoesNotContain(ClientSecret, tokenRequest.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyAsync_IncludesScopesInTokenRequest()
    {
        using var scope = CreateProvider(
            new CapturingHttpMessageHandler((request, _) => Respond(request)),
            tokenEndpoint: new Uri("https://abacus.example/oauth/oauth2/v1/token"),
            scopes: ["abacus.pad", "openid"]);
        var handler = scope.Handler;

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api/suppliers");
        await scope.Provider.ApplyAsync(request);

        var tokenRequest = Assert.Single(handler.Requests, captured => captured.Method == HttpMethod.Post);
        Assert.Contains("grant_type=client_credentials", tokenRequest.Body, StringComparison.Ordinal);
        Assert.Contains("scope=abacus.pad+openid", tokenRequest.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ApplyAsync_ReusesCachedTokenUntilRefreshWindow()
    {
        var clock = new ManualTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        using var scope = CreateProvider(
            new CapturingHttpMessageHandler((request, _) => Respond(request, expiresIn: 60)),
            clock: clock,
            refreshSkew: TimeSpan.FromSeconds(30));
        var handler = scope.Handler;
        using var first = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api/suppliers");
        using var second = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api/suppliers");
        using var third = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api/suppliers");

        await scope.Provider.ApplyAsync(first);
        clock.UtcNow = clock.UtcNow.AddSeconds(29);
        await scope.Provider.ApplyAsync(second);
        clock.UtcNow = clock.UtcNow.AddSeconds(2);
        await scope.Provider.ApplyAsync(third);

        Assert.Equal(1, handler.Requests.Count(captured => captured.Method == HttpMethod.Get));
        Assert.Equal(2, handler.Requests.Count(captured => captured.Method == HttpMethod.Post));
        Assert.Equal("token-123", third.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task ApplyAsync_SkipsDiscoveryWhenTokenEndpointIsConfigured()
    {
        using var scope = CreateProvider(
            new CapturingHttpMessageHandler((request, _) => Respond(request)),
            tokenEndpoint: new Uri("https://abacus.example/oauth/oauth2/v1/token"));
        var handler = scope.Handler;

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api/suppliers");
        await scope.Provider.ApplyAsync(request);

        Assert.DoesNotContain(handler.Requests, captured => captured.Method == HttpMethod.Get);
        Assert.Single(handler.Requests, captured => captured.Method == HttpMethod.Post);
    }

    [Fact]
    public async Task ApplyAsync_ThrowsWithoutRevealingClientSecretWhenTokenRequestFails()
    {
        using var scope = CreateProvider(
            new CapturingHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(
                    """{"error":"invalid_client","client_secret":"super-secret"}""",
                    Encoding.UTF8,
                    "application/json"),
            }),
            tokenEndpoint: new Uri("https://abacus.example/oauth/oauth2/v1/token"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api/suppliers");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.Provider.ApplyAsync(request).AsTask());

        Assert.Contains("401", exception.Message, StringComparison.Ordinal);
        Assert.Contains("***", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(ClientSecret, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddAbacusClientCredentials_UsesServerAuthorityAndBearerToken()
    {
        var handler = new CapturingHttpMessageHandler((request, _) => Respond(request));
        var services = new ServiceCollection();
        services.AddAbacusSdk(new Uri("https://abacus.example/api/"));
        services.AddAbacusClientCredentials(ClientId, ClientSecret);
        services.AddHttpClient(AbacusHttpClientFactory.TokenHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        using var serviceProvider = services.BuildServiceProvider();
        var authentication = serviceProvider.GetRequiredService<IAbacusAuthenticationProvider>();
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api/suppliers");

        await authentication.ApplyAsync(request);

        Assert.IsType<ClientCredentialsAuthenticationProvider>(authentication);
        Assert.Equal("Bearer token-123", request.Headers.Authorization?.ToString());
        var discovery = Assert.Single(handler.Requests, captured => captured.Method == HttpMethod.Get);
        Assert.Equal("https://abacus.example/.well-known/openid-configuration", discovery.RequestUri?.AbsoluteUri);
    }

    private static ProviderScope CreateProvider(
        CapturingHttpMessageHandler handler,
        TimeProvider? clock = null,
        TimeSpan? refreshSkew = null,
        Uri? tokenEndpoint = null,
        IEnumerable<string>? scopes = null)
    {
        var httpClient = new HttpClient(handler);
        var provider = new ClientCredentialsAuthenticationProvider(new AbacusClientCredentialsOptions
        {
            ServerUri = new Uri("https://abacus.example/api/"),
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            TokenEndpoint = tokenEndpoint,
            RefreshSkew = refreshSkew ?? TimeSpan.FromSeconds(30),
            TimeProvider = clock ?? TimeProvider.System,
            Scopes = scopes?.ToList() ?? new List<string>(),
        }, httpClient);

        return new ProviderScope(provider, httpClient, handler);
    }

    private static HttpResponseMessage Respond(HttpRequestMessage request, int expiresIn = 600)
    {
        if (request.RequestUri?.AbsolutePath == "/.well-known/openid-configuration")
        {
            return Json("""{"token_endpoint":"https://abacus.example/oauth/oauth2/v1/token"}""");
        }

        return Json($$"""{"access_token":"token-123","token_type":"Bearer","expires_in":{{expiresIn}}}""");
    }

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    private static string BasicCredential(string clientId, string clientSecret) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Uri.EscapeDataString(clientId)}:{Uri.EscapeDataString(clientSecret)}"));

    private sealed class ProviderScope : IDisposable
    {
        private readonly HttpClient _httpClient;

        public ProviderScope(
            ClientCredentialsAuthenticationProvider provider,
            HttpClient httpClient,
            CapturingHttpMessageHandler handler)
        {
            Provider = provider;
            _httpClient = httpClient;
            Handler = handler;
        }

        public ClientCredentialsAuthenticationProvider Provider { get; }

        public CapturingHttpMessageHandler Handler { get; }

        public void Dispose()
        {
            Provider.Dispose();
            _httpClient.Dispose();
        }
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        public ManualTimeProvider(DateTimeOffset utcNow) => UtcNow = utcNow;

        public DateTimeOffset UtcNow { get; set; }

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}
