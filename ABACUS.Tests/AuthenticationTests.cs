using ABACUS.Core;
using ABACUS.Tests.Testing;

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
