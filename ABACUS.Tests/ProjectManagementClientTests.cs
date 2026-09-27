using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.ProjectManagement;

namespace ABACUS.Tests;

public sealed class ProjectManagementClientTests
{
    [Fact]
    public async Task ListAndGetProject_UseIdKeyedPath()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[{"Id":"1"}],"Id":"1"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new ProjectManagementClient(httpClient);

        await client.ListProjectsAsync(ODataQuery.Create().Top(5));
        await client.GetProjectAsync("99");
        await client.CreateProjectAsync(new { Name = "P" });
        await client.ListProjectBookingsAsync();
        await client.GetProjectDocumentAsync("3");

        Assert.Equal("/Projects?$top=5", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal("/Projects(Id=99)", handler.Requests[1].RequestUri?.AbsolutePath);
        Assert.Equal(HttpMethod.Post, handler.Requests[2].Method);
        Assert.Equal("/ProjectBookings", handler.Requests[3].RequestUri?.AbsolutePath);
        Assert.Equal("/ProjectDocuments(3)", handler.Requests[4].RequestUri?.AbsolutePath);
    }
}
