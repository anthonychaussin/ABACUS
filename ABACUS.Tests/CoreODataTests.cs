using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.Tests;

public sealed class ODataQueryTests
{
    [Fact]
    public void ToQueryString_BuildsSupportedODataOptions()
    {
        var query = ODataQuery.Create()
            .Filter("Status eq 'Open'")
            .Filter("Amount gt 10")
            .Select("Id", "Name")
            .Expand("Addresses")
            .OrderBy("Name asc")
            .Top(50)
            .Skip(10)
            .Format("json");

        var result = query.ToQueryString();

        Assert.Contains("$filter=" + Uri.EscapeDataString("Status eq 'Open' and Amount gt 10"), result);
        Assert.Contains("$select=" + Uri.EscapeDataString("Id,Name"), result);
        Assert.Contains("$expand=" + Uri.EscapeDataString("Addresses"), result);
        Assert.Contains("$orderby=" + Uri.EscapeDataString("Name asc"), result);
        Assert.Contains("$top=50", result);
        Assert.Contains("$skip=10", result);
        Assert.Contains("$format=json", result);
    }

    [Fact]
    public void ApplyTo_AppendsQueryToPath()
    {
        var path = ODataQuery.Create().Top(25).ApplyTo("/Suppliers");
        Assert.Equal("/Suppliers?$top=25", path);
    }
}

public sealed class AbacusEntityUriTests
{
    [Fact]
    public void BuildBaseUri_IncludesMandant()
    {
        var uri = AbacusEntityUri.BuildBaseUri(new Uri("https://abacus.example:40000"), "7777");
        Assert.Equal("https://abacus.example:40000/api/entity/v1/mandants/7777/", uri.AbsoluteUri);
    }

    [Fact]
    public void ResolveBaseUri_FromServerAndMandant()
    {
        var options = new AbacusClientOptions
        {
            ServerUri = new Uri("https://abacus.example"),
            Mandant = "42",
        };

        Assert.Equal(
            "https://abacus.example/api/entity/v1/mandants/42/",
            options.ResolveBaseUri().AbsoluteUri);
    }
}

public sealed class ODataPageParserTests
{
    [Fact]
    public void Parse_ReadsValueAndNextLink()
    {
        var json = """{"value":[{"Id":1},{"Id":2}],"@odata.nextLink":"https://example/next"}""";
        var page = ODataPageParser.Parse<JsonElement>(json, 200, new Dictionary<string, IEnumerable<string>>());

        Assert.Equal(2, page.Value.Count);
        Assert.True(page.HasNextPage);
        Assert.Equal("https://example/next", page.NextLink);
    }
}

public sealed class AbacusHttpTests
{
    [Fact]
    public async Task GetODataPageAsync_FollowsQueryAndParsesBody()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"value":[{"Name":"Acme"}],"@odata.nextLink":"/Suppliers?$skiptoken=1"}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid/") };
        var page = await AbacusHttp.GetODataPageAsync<SupplierDto>(
            httpClient,
            "/Suppliers",
            ODataQuery.Create().Top(10));

        Assert.Equal("/Suppliers?$top=10", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal("Acme", Assert.Single(page.Value).Name);
        Assert.Equal("/Suppliers?$skiptoken=1", page.NextLink);
    }

    [Fact]
    public async Task EnumerateODataAsync_WalksNextLinks()
    {
        var call = 0;
        using var handler = new CapturingHttpMessageHandler((_, _) =>
        {
            call++;
            if (call == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"value":[{"Name":"A"}],"@odata.nextLink":"/page2"}""",
                        Encoding.UTF8,
                        "application/json"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"value":[{"Name":"B"}]}""",
                    Encoding.UTF8,
                    "application/json"),
            };
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid/") };

        var names = new List<string>();
        await foreach (var item in AbacusHttp.EnumerateODataAsync<SupplierDto>(httpClient, "/Suppliers"))
        {
            names.Add(item.Name!);
        }

        Assert.Equal(["A", "B"], names);
        Assert.Equal(2, handler.Requests.Count);
    }

    private sealed class SupplierDto
    {
        public string? Name { get; set; }
    }
}

public sealed class AbacusODataEntityTests
{
    [Fact]
    public async Task ListAndCreate_UseCollectionPath()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"1"}]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };

        await AbacusODataEntity.ListAsync(httpClient, "/Things", ODataQuery.Create().Top(5));
        await AbacusODataEntity.CreateAsync(httpClient, "/Things", new { Name = "X" });

        Assert.Equal("/Things?$top=5", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(HttpMethod.Post, handler.Requests[1].Method);
        Assert.Equal("/Things(Id=9)", AbacusODataEntity.IdPath("Things", "9"));
    }
}

public sealed class AbacusRateLimitHandlerTests
{
    [Fact]
    public async Task SendAsync_RetriesOn429ThenSucceeds()
    {
        var attempts = 0;
        using var inner = new CapturingHttpMessageHandler((_, _) =>
        {
            attempts++;
            if (attempts == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var httpClient = new HttpClient(new AbacusRateLimitHandler(maxRetries: 2, baseDelay: TimeSpan.FromMilliseconds(1))
        {
            InnerHandler = inner,
        });

        using var response = await httpClient.GetAsync("https://example.invalid/test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, attempts);
    }
}
