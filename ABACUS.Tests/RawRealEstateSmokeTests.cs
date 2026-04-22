using ABACUS.RealEstate;
using ABACUS.Tests.Testing;

namespace ABACUS.Tests;

public sealed class RawRealEstateSmokeTests
{
    [Fact]
    public async Task Raw_GetObjectContracts_CallsExpectedEndpoint_WithoutRealNetwork()
    {
        using var handler = new FakeHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.invalid"),
        };

        var module = new RealEstateClient(httpClient);

        await module.Raw.Get_ObjectContracts_Id_id_6Async("1");

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/ObjectContracts(Id=1)", request.RequestUri?.AbsolutePath);
    }
}
