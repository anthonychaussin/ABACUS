using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.Core;
using ABACUS.Testing;
using Xunit;

namespace ABACUS.Tests;

public sealed class AbacusEntityClientTests
{
    [Fact]
    public async Task ListsEndpointIntroducedInAbacus2026()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"value\":[{\"IssueNumber\":\"TASK-23\"}]}", Encoding.UTF8, "application/json"),
        });
        var client = new AbacusEntityClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://server/api/entity/v1/mandants/7777/"),
        });

        var page = await client.ListAsync<JsonElement>(
            "ServiceObjectIssues",
            new ODataQuery().Top(10).Select("Id", "IssueNumber"));

        Assert.Single(page.Value);
        Assert.Equal("TASK-23", page.Value[0].GetProperty("IssueNumber").GetString());
        var request = Assert.Single(handler.Requests);
        Assert.Equal(
            "/api/entity/v1/mandants/7777/ServiceObjectIssues?$select=Id%2CIssueNumber&$top=10",
            request.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task SupportsCompositeKeysAndPatch()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"Status\":\"Closed\"}", Encoding.UTF8, "application/json"),
        });
        var client = new AbacusEntityClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://server/api/entity/v1/mandants/7777/"),
        });

        await client.UpdateAsync<JsonElement>(
            "ServiceObjectIssues",
            "Id=42,IssueNumber='TASK-23'",
            new { Status = "Closed" });

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal(
            "/api/entity/v1/mandants/7777/ServiceObjectIssues(Id=42,IssueNumber='TASK-23')",
            request.RequestUri!.PathAndQuery);
        Assert.Contains(AbacusHttp.PreferReturnRepresentation, request.Headers.GetValues("Prefer"));
    }

    [Theory]
    [InlineData("../Users")]
    [InlineData("Subjects?$top=1")]
    [InlineData("Sales-Orders")]
    public async Task RejectsUnsafeEntitySetNames(string entitySet)
    {
        var client = new AbacusEntityClient(new HttpClient(new FakeHttpMessageHandler(_ => throw new InvalidOperationException())));

        await Assert.ThrowsAsync<ArgumentException>(() => client.ListAsync<JsonElement>(entitySet));
    }
}
