using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.Salary;

namespace ABACUS.Tests;

public sealed class SalaryClientTests
{
    [Fact]
    public async Task PersonnelPayrollAndJobAssignments_SendExpectedPaths()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[],"Id":"1"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new SalaryClient(httpClient);

        await client.ListEmployeePersonnelDataSetsAsync(ODataQuery.Create().Top(2));
        await client.GetEmployeePersonnelDataSetAsync("10", "1", "2024-01-01");
        await client.ListEmployeePayrollDataSetsAsync();
        await client.CreateJobAssignmentAsync(new { Title = "Dev" }, prefer: AbacusHttp.PreferContinueOnError);
        await client.GetEmployeeEntryLeavingAsync("5");

        Assert.Equal("/EmployeePersonnelDataSet?$top=2", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal(
            "/EmployeePersonnelDataSet(EmployeeId=10,ContractId=1,ValidFrom=2024-01-01)",
            handler.Requests[1].RequestUri?.AbsolutePath);
        Assert.Equal("/EmployeePayrollDataSet", handler.Requests[2].RequestUri?.AbsolutePath);
        Assert.Equal(AbacusHttp.PreferContinueOnError, handler.Requests[3].Prefer);
        Assert.Equal("/EmployeeEntryLeavings(5)", handler.Requests[4].RequestUri?.AbsolutePath);
    }
}
