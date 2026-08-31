using System.Net;
using System.Text;
using ABACUS.Reporting;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class ReportingClientTests
{
    [Fact]
    public async Task StartReportAsync_PostsRequestAndEncodesReportPath()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) => JsonResponse(
            """{"id":"job-1","state":"Running","message":""}"""));
        using var httpClient = CreateHttpClient(handler);
        var client = new ReportingClient(httpClient);

        var job = await client.StartReportAsync(
            7777,
            "sales/monthly report.avx",
            new ReportRequest(ReportOutputTypes.Excel, 200, new Dictionary<string, string>
            {
                ["year"] = "2026",
            }));

        Assert.Equal("job-1", job.Id);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(
            "https://example.invalid/api/abareport/v1/report/7777/sales%2Fmonthly%20report.avx",
            request.RequestUri!.AbsoluteUri);
        Assert.Contains("\"outputType\":\"excel-xlsx\"", request.Body);
        Assert.Contains("\"year\":\"2026\"", request.Body);
        Assert.Equal("application/json; charset=utf-8", request.ContentType);
    }

    [Fact]
    public async Task JobOutputAndClose_UseDocumentedEndpointsAndPreserveBinaryData()
    {
        var responseIndex = 0;
        using var handler = new CapturingHttpMessageHandler((_, _) => responseIndex++ switch
        {
            0 => JsonResponse("""{"id":"job/1","state":"FinishedSuccess","message":"pages=1"}"""),
            1 => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([0x50, 0x4B, 0x03, 0x04])
                {
                    Headers = { ContentType = new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") },
                },
            },
            _ => new HttpResponseMessage(HttpStatusCode.OK),
        });
        using var httpClient = CreateHttpClient(handler);
        var client = new ReportingClient(httpClient);

        var job = await client.GetJobAsync("job/1");
        var output = await client.GetOutputAsync("job/1", 2);
        await client.CloseJobAsync("job/1");

        Assert.Equal("FinishedSuccess", job.State);
        Assert.Equal([0x50, 0x4B, 0x03, 0x04], output.Content);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", output.ContentType);
        Assert.Collection(
            handler.Requests,
            request => Assert.Equal("https://example.invalid/api/abareport/v1/jobs/job%2F1", request.RequestUri!.AbsoluteUri),
            request => Assert.Equal("https://example.invalid/api/abareport/v1/jobs/job%2F1/output/2", request.RequestUri!.AbsoluteUri),
            request => Assert.Equal(HttpMethod.Delete, request.Method));
    }

    [Fact]
    public async Task StartReportAsync_RejectsPagingBelowServerMinimum()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = CreateHttpClient(handler);
        var client = new ReportingClient(httpClient);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.StartReportAsync(7777, "report", new ReportRequest(ReportOutputTypes.Json, 9)));
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData("../secret")]
    [InlineData("folder//report")]
    [InlineData("folder\\report")]
    public async Task StartReportAsync_RejectsUnsafeReportPaths(string reportName)
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = CreateHttpClient(handler);
        var client = new ReportingClient(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.StartReportAsync(7777, reportName, new ReportRequest(ReportOutputTypes.Json)));
        Assert.Empty(handler.Requests);
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) => new(handler)
    {
        BaseAddress = new Uri("https://example.invalid/"),
    };

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };
}
