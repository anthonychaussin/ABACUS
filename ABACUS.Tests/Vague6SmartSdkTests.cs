using System.Net;
using System.Text;
using ABACUS.AccountsPayable;
using ABACUS.Core;
using ABACUS.Core.Generated;
using ABACUS.Finance;

namespace ABACUS.Tests;

public sealed class Vague6SmartSdkTests
{
    [Fact]
    public void ODataErrorParser_CreatesValidationException_For400WithDetails()
    {
        const string body = """
            {"error":{"code":"Validation","message":"Invalid","details":[{"code":"Required","message":"Name required","target":"Name"}]}}
            """;

        var ex = AbacusODataErrorParser.CreateException(400, body);

        var validation = Assert.IsType<AbacusValidationException>(ex);
        Assert.Equal("Validation", validation.ODataErrorCode);
        Assert.Equal("Invalid", validation.ODataMessage);
        var detail = Assert.Single(validation.Details);
        Assert.Equal("Name", detail.Target);
    }

    [Fact]
    public async Task ListAccountsAsAsync_DeserializesAccountSummary()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            ODataResponseFactory.Page("""[{"Id":"A1","Name":"Cash","EnterpriseId":"1"}]"""));
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(http);

        var page = await client.ListAccountsAsAsync<AccountSummary>();
        var item = Assert.Single(page.Value);
        Assert.Equal("A1", item.Id);
        Assert.Equal("Cash", item.Name);
    }

    [Fact]
    public async Task ListAccountsAsAsync_DeserializesGeneratedDto()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            ODataResponseFactory.Page("""[{"Id":"A1","Name":"Cash","EnterpriseId":"1"}]"""));
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(http);

        var page = await client.ListAccountsAsAsync<AccountGenerated>();
        Assert.Equal("Cash", Assert.Single(page.Value).Name);
    }

    [Fact]
    public async Task CreateAccountAsync_WithMapper_PostsPayload()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FinanceClient(http);
        var mapper = new AbacusFieldMapper(new AbacusFieldMappingBuilder()
            .Entity("Account")
            .Field("Name", "Name")
            .Build());

        await client.CreateAccountAsync(new { Name = "Cash" }, mapper);

        handler.Requests[0].HasMethod(HttpMethod.Post).HasPath("/Accounts");
        Assert.Contains("\"Name\":\"Cash\"", handler.Requests[0].Body);
    }

    [Fact]
    public async Task ReadRetryHandler_RetriesGetOn503_ButNotPost()
    {
        var getAttempts = 0;
        using var getInner = new CapturingHttpMessageHandler((_, _) =>
        {
            getAttempts++;
            if (getAttempts < 3)
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }

            return ODataResponseFactory.Page("[]");
        });
        using var getHandler = new AbacusReadRetryHandler(maxAttempts: 3, baseDelay: TimeSpan.Zero)
        {
            InnerHandler = getInner,
        };
        using var getHttp = new HttpClient(getHandler) { BaseAddress = new Uri("https://example.invalid") };
        var ap = new AccountsPayableClient(getHttp);
        await ap.ListSuppliersAsync();
        Assert.Equal(3, getAttempts);

        var postAttempts = 0;
        using var postInner = new CapturingHttpMessageHandler((_, _) =>
        {
            postAttempts++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };
        });
        using var postHandler = new AbacusReadRetryHandler(maxAttempts: 3, baseDelay: TimeSpan.Zero)
        {
            InnerHandler = postInner,
        };
        using var postHttp = new HttpClient(postHandler) { BaseAddress = new Uri("https://example.invalid") };
        var ap2 = new AccountsPayableClient(postHttp);
        await Assert.ThrowsAsync<AbacusApiException>(() => ap2.CreateSupplierAsync(new { Name = "X" }));
        Assert.Equal(1, postAttempts);
    }

    [Fact]
    public async Task Http_ThrowsValidationException_OnOData400()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            ODataResponseFactory.Json(
                HttpStatusCode.BadRequest,
                """{"error":{"code":"Bad","message":"Nope","details":[{"target":"Id","message":"bad id"}]}}"""));
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };

        var ex = await Assert.ThrowsAsync<AbacusValidationException>(() =>
            AbacusHttp.SendAsync(http, HttpMethod.Get, "/Suppliers"));

        Assert.Equal("Nope", ex.ODataMessage);
        Assert.Equal("Id", Assert.Single(ex.Details).Target);
    }

    [Fact]
    public void SourceGenerator_ApplyAttributes_ProducesMapping()
    {
        var mapping = new AbacusFieldMappingBuilder()
            .ApplyAttributedDemo()
            .Build();
        var mapper = new AbacusFieldMapper(mapping);
        var payload = mapper.ToPayload("AttributedDemo", new AttributedDemo { Title = "Hi", Code = "X" });
        Assert.Equal("Hi", payload["Title"]);
        var userFields = Assert.IsType<Dictionary<string, object?>>(payload["UserFields"]);
        Assert.Equal("X", userFields["UserField1"]);
    }
}

internal sealed class AttributedDemo
{
    [AbacusField("Title")]
    public string Title { get; set; } = "";

    [AbacusField("UserFields.UserField1")]
    public string Code { get; set; } = "";
}
