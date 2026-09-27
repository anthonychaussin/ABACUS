using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.Subscription;

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

        var response = await client.ListSubscriptionsAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/Subscriptions", request.RequestUri?.AbsolutePath);
        Assert.Equal(200, response.StatusCode);
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
    public async Task SubscribeAsync_ValidatesNameAndPostsPayload()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new SubscriptionClient(httpClient);

        await client.SubscribeAsync("acme-sync", ["Subject"]);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/SubscribeChanges", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"Subscription\":\"acme-sync\"", request.Body, StringComparison.Ordinal);
        Assert.Contains("\"Topic\":\"Subject\"", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConsumeAsync_UsesNamedSubscriptionPath()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"value":[{"Id":1}],"AcknowledgeKey":"ack-1"}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new SubscriptionClient(httpClient);

        var batch = await client.ConsumeAsync("acme-sync");

        Assert.Equal("/Subscriptions('acme-sync')/ch.abacus.df.ConsumeChanges()", handler.Requests[0].RequestUri?.AbsolutePath);
        Assert.Equal("ack-1", batch.AcknowledgeKey);
        Assert.True(batch.HasChanges);
    }

    [Fact]
    public async Task AcknowledgeAsync_PostsAcknowledgeKey()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new SubscriptionClient(httpClient);

        await client.AcknowledgeAsync("acme-sync", "ack-1");

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Subscriptions('acme-sync')/ch.abacus.df.AcknowledgeChanges", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"AcknowledgeKey\":\"ack-1\"", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubscribeAsync_RejectsOversizedName()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };
        var client = new SubscriptionClient(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.SubscribeAsync(new string('a', 24), ["Subject"]));
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
        Assert.Contains("\"Topics\":[\"topic-1\"]", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListSubscriptionsAsync_MapsHttpFailure_ToAbacusApiException()
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

public sealed class AbacusFieldInformationMapperTests
{
    [Fact]
    public void ExtractAndFindUnknownPaths()
    {
        var known = AbacusFieldInformationMapper.ExtractFieldPaths(
            """{"value":[{"Path":"Name"},{"Name":"Address.City"}]}""");
        var mapping = new AbacusFieldMapping();
        mapping.SetField("Supplier", "Name", "Name");
        mapping.SetField("Supplier", "Vat", "UserFields.UserField1");

        var unknown = AbacusFieldInformationMapper.FindUnknownPaths(mapping, "Supplier", known);

        Assert.Contains("Name", known);
        Assert.Contains("Address.City", known);
        Assert.Equal(["UserFields.UserField1"], unknown);
    }
}
