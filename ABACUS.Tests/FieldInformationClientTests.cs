using System.Net;
using System.Text;
using ABACUS.Core;
using ABACUS.FieldInformation;

namespace ABACUS.Tests;

public sealed class FieldInformationClientTests
{
    [Fact]
    public async Task GetFieldInformationsAsync_PostsPayload()
    {
        using var handler = new CapturingHttpMessageHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FieldInformationClient(httpClient);

        await client.GetFieldInformationsAsync(new { Entity = "Supplier" });

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/FieldInformations", request.RequestUri?.AbsolutePath);
        Assert.Contains("\"Entity\":\"Supplier\"", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateMappingAsync_ReturnsUnknownPaths()
    {
        using var handler = new CapturingHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"value":[{"Name":"Name"},{"Name":"Number"}]}""",
                    Encoding.UTF8,
                    "application/json"),
            });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
        var client = new FieldInformationClient(httpClient);

        var mapping = new AbacusFieldMappingBuilder()
            .Entity("Supplier")
            .Field("Name", "Name")
            .Field("Vat", "UserFields.Vat")
            .Build();
        var mapper = new AbacusFieldMapper(mapping);

        var unknown = await client.ValidateMappingAsync(
            mapper,
            mapping,
            "Supplier",
            new { Entity = "Supplier" });

        Assert.Contains("UserFields.Vat", unknown);
        Assert.DoesNotContain("Name", unknown);
    }
}
