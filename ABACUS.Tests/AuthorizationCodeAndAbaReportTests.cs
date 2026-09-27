using System.Net;
using System.Text;
using ABACUS.AbaReport;
using ABACUS.Core;

namespace ABACUS.Tests;

public sealed class AuthorizationCodeAuthenticationTests
{
    [Fact]
    public async Task CreateAuthorizationUrlAsync_IncludesClientAndScopes()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        using var provider = new AuthorizationCodeAuthenticationProvider(
            new AbacusAuthorizationCodeOptions
            {
                ServerUri = new Uri("https://abacus.example"),
                ClientId = "client-id",
                ClientSecret = "secret",
                RedirectUri = new Uri("https://app.example/callback"),
                AuthorizationEndpoint = new Uri("https://abacus.example/oauth/oauth2/v1/auth"),
                TokenEndpoint = new Uri("https://abacus.example/oauth/oauth2/v1/token"),
                Scopes = ["openid", "offline_access"],
            },
            httpClient);

        var url = await provider.CreateAuthorizationUrlAsync("state-1");

        Assert.Equal("https://abacus.example/oauth/oauth2/v1/auth", url.GetLeftPart(UriPartial.Path));
        Assert.Contains("client_id=client-id", url.Query, StringComparison.Ordinal);
        Assert.Contains("state=state-1", url.Query, StringComparison.Ordinal);
        Assert.Contains("scope=openid%20offline_access", url.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExchangeCodeAsync_CachesAccessToken()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"access_token":"access-1","refresh_token":"refresh-1","expires_in":600}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        using var httpClient = new HttpClient(handler);
        using var provider = new AuthorizationCodeAuthenticationProvider(
            new AbacusAuthorizationCodeOptions
            {
                ServerUri = new Uri("https://abacus.example"),
                ClientId = "client-id",
                ClientSecret = "secret",
                RedirectUri = new Uri("https://app.example/callback"),
                TokenEndpoint = new Uri("https://abacus.example/oauth/oauth2/v1/token"),
                AuthorizationEndpoint = new Uri("https://abacus.example/oauth/oauth2/v1/auth"),
            },
            httpClient);

        await provider.ExchangeCodeAsync("auth-code");
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://abacus.example/api");
        await provider.ApplyAsync(request);

        Assert.Equal("Bearer access-1", request.Headers.Authorization?.ToString());
        Assert.Contains("grant_type=authorization_code", handler.Requests[0].Body, StringComparison.Ordinal);
    }
}

public sealed class AbaReportClientTests
{
    [Fact]
    public async Task ExportReportAsync_ReturnsBytes()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("report-data")),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AbaReportClient(httpClient);

        var response = await client.ExportReportAsync("MyReport", format: "txt");

        Assert.Equal("/abareport/api/reports/MyReport/export?format=txt", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal("report-data", Encoding.UTF8.GetString(response.Data));
    }
}
