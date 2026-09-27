using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.HumanResources;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class HumanResourcesClientTests
{
    [Fact]
    public async Task EmployeesOrgUnitsApplicantsAndTrainings_SendExpectedPaths()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[],"Id":"1"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new HumanResourcesClient(httpClient);

        await client.ListOrganisationEmployeesAsync(ODataQuery.Create().Top(3));
        await client.GetOrganisationEmployeeAsync("12");
        await client.ListOrganisationOrgUnitsAsync();
        await client.ListApplicantsAsync();
        await client.CreateLearningTrainingAsync(new { Title = "Safety" });

        Assert.Equal("/OrganisationEmployees?$top=3", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal("/OrganisationEmployees(12)", handler.Requests[1].RequestUri?.AbsolutePath);
        Assert.Equal("/OrganisationOrgUnits", handler.Requests[2].RequestUri?.AbsolutePath);
        Assert.Equal("/Applicants", handler.Requests[3].RequestUri?.AbsolutePath);
        Assert.Equal("/LearningTrainings", handler.Requests[4].RequestUri?.AbsolutePath);
    }
}
