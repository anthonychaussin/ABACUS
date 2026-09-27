using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.DossierFileUpload;
using ABACUS.Tests.Testing;
using ABACUS.UserDependentAuth;

namespace ABACUS.Tests;

public sealed class DossierFileUploadClientTests
{
    [Fact]
    public async Task UploadToFileStoreAsync_SendsOctetStream()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new DossierFileUploadClient(httpClient);

        await client.UploadToFileStoreAsync(Encoding.UTF8.GetBytes("hello"));

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/api/file-store/v1/user", request.RequestUri?.AbsolutePath);
        Assert.Contains("octet-stream", request.ContentType, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadCustomerInvoiceDocumentAndGetStorage_SendExpectedPaths()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[],"Id":"1"}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new DossierFileUploadClient(httpClient);

        await client.UploadCustomerInvoiceDocumentAsync(new { Name = "inv.pdf" });
        await client.GetStorageAsync("42");

        Assert.Equal("/CustomerInvoiceDocuments", handler.Requests[0].RequestUri?.AbsolutePath);
        Assert.Equal("/Storages(42)", handler.Requests[1].RequestUri?.AbsolutePath);
    }
}

public sealed class UserDependentAuthClientTests
{
    [Fact]
    public async Task ListProjectDocumentsAndStorages_SendExpectedPaths()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"value":[]}""", Encoding.UTF8, "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new UserDependentAuthClient(httpClient);

        await client.ListProjectDocumentsAsync(ODataQuery.Create().Top(5));
        await client.ListStoragesAsync();

        Assert.Equal("/ProjectDocuments?$top=5", handler.Requests[0].RequestUri?.PathAndQuery);
        Assert.Equal("/Storages", handler.Requests[1].RequestUri?.AbsolutePath);
    }
}
