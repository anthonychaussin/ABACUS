using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.Subscription;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class SubscriptionClientTests
{
    [Fact]
    public async Task ListSubscriptionsAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new SubscriptionClient(httpClient);

        await client.ListSubscriptionsAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/subscriptions", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task GetMetadataAsync_SendsExpectedRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new SubscriptionClient(httpClient);

        await client.GetMetadataAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/$metadata", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task SubscribeToChangesAsync_SendsExpectedPostRequest()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new SubscriptionClient(httpClient);

        await client.SubscribeToChangesAsync(new { Topics = new[] { "topic-1" } });

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/SubscribeChanges", request.RequestUri?.AbsolutePath);
        Assert.Equal("application/json", request.ContentType);
        Assert.Contains("\"Topics\":[\"topic-1\"]", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListSubscriptionsAsync_MapsGeneratedApiException_ToAbacusApiException()
    {
        using var handler = new CapturingHttpMessageHandler(static (_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid\"}", Encoding.UTF8, "application/json"),
            };
            response.Headers.Add("X-Trace-Id", "trace-sub");
            return response;
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new SubscriptionClient(httpClient);

        var exception = await Assert.ThrowsAsync<AbacusApiException>(() => client.ListSubscriptionsAsync());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("{\"error\":\"invalid\"}", exception.ResponseBody);
        Assert.Equal(["trace-sub"], exception.Headers["X-Trace-Id"]);
    }
}
