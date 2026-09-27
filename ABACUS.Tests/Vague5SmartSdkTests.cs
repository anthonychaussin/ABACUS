using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using ABACUS.AccountsPayable;
using ABACUS.AccountsReceivable;
using ABACUS.Core;
using ABACUS.CRM;

namespace ABACUS.Tests;

public sealed class Vague5SmartSdkTests
{
    [Fact]
    public void ODataFilterBuilder_EscapesQuotes_AndBuildsAndClauses()
    {
        var filter = ODataFilterBuilder.Create()
            .Equal("Status", "O'Reilly")
            .Gt("Amount", 10)
            .Contains("Name", "Acme")
            .ToFilterExpression();

        Assert.Equal("Status eq 'O''Reilly' and Amount gt 10 and contains(Name,'Acme')", filter);
    }

    [Fact]
    public void ODataQuery_Where_AppliesFilterBuilder()
    {
        var path = ODataQuery.Create()
            .Where(f => f.Equal("Name", "Acme").Equal("Status", "Open"))
            .Top(5)
            .ApplyTo("/Suppliers");

        Assert.Contains("$filter=" + Uri.EscapeDataString("Name eq 'Acme' and Status eq 'Open'"), path);
        Assert.Contains("$top=5", path);
    }

    [Fact]
    public async Task CreateSupplierAsync_WithFieldMapper_PostsMappedPayload()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsPayableClient(httpClient);
        var mapper = new AbacusFieldMapper(new AbacusFieldMappingBuilder()
            .Entity("Supplier")
            .Field("Name", "Name")
            .Field("VatNumber", "UserFields.UserField1")
            .Build());

        await client.CreateSupplierAsync(new MapperSupplier("Acme", "CHE-1"), mapper);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/Suppliers", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"Name\":\"Acme\"", request.Body);
        Assert.Contains("\"UserField1\":\"CHE-1\"", request.Body);
    }

    [Fact]
    public async Task PatchSupplierAsync_WithFieldMapper_SendsPatch()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsPayableClient(httpClient);
        var mapper = new AbacusFieldMapper(new AbacusFieldMappingBuilder()
            .Entity("Supplier")
            .Field("Name", "Name")
            .Build());

        await client.PatchSupplierAsync("42", new MapperSupplier("Beta", null), mapper);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal("/Suppliers(Id=42)", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"Name\":\"Beta\"", request.Body);
    }

    [Fact]
    public async Task CreateSubjectAsync_WithFieldMapper_PostsMappedPayload()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new CRMClient(httpClient);
        var mapper = new AbacusFieldMapper(new AbacusFieldMappingBuilder()
            .Entity("Subject")
            .Field("Name", "Name")
            .Build());

        await client.CreateSubjectAsync(new MapperSubject("Subject-1"), mapper);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("/Subjects", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"Name\":\"Subject-1\"", request.Body);
    }

    [Fact]
    public async Task ListSuppliersAsAsync_DeserializesSupplierSummary()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"value":[{"Id":"1","Name":"Acme","SupplierNumber":99}]}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsPayableClient(httpClient);

        var page = await client.ListSuppliersAsAsync<SupplierSummary>();

        var item = Assert.Single(page.Value);
        Assert.Equal("1", item.Id);
        Assert.Equal("Acme", item.Name);
        Assert.Equal(99, item.SupplierNumber);
    }

    [Fact]
    public async Task ListCustomersAsAsync_DeserializesCustomerSummary()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"value":[{"Id":7,"Name":"Cust","CustomerNumber":12}]}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsReceivableClient(httpClient);

        var page = await client.ListCustomersAsAsync<CustomerSummary>();

        var item = Assert.Single(page.Value);
        Assert.Equal(7, item.Id);
        Assert.Equal("Cust", item.Name);
        Assert.Equal(12, item.CustomerNumber);
    }

    [Fact]
    public async Task AbacusTelemetryHandler_CreatesActivity()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == AbacusTelemetry.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        Activity? observed = null;
        listener.ActivityStopped = activity => observed = activity;

        using var inner = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var handler = new AbacusTelemetryHandler { InnerHandler = inner };
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new AccountsPayableClient(httpClient);

        await client.ListSuppliersAsync();

        Assert.NotNull(observed);
        Assert.Contains("GET", observed!.DisplayName, StringComparison.Ordinal);
        Assert.Equal(200, observed.GetTagItem("http.response.status_code"));
    }

    private sealed record MapperSupplier(string Name, string? VatNumber);
    private sealed record MapperSubject(string Name);
}
