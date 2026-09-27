using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.RealEstate;

/// <summary>
/// Default implementation for the RealEstate module wrapper.
/// </summary>
public sealed class RealEstateClient : IRealEstateClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "RealEstate";

    /// <inheritdoc />
    public ABACUS_RealEstateClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public RealEstateClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_RealEstateClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListObjectContractsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/ObjectContracts", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateObjectContractsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(_httpClient, "/ObjectContracts", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetObjectContractAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var path = query is null
            ? $"/ObjectContracts(Id={id})"
            : query.ApplyTo($"/ObjectContracts(Id={id})");
        return AbacusHttp.SendJsonAsync<JsonElement>(_httpClient, HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListPartialObjectContractsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/PartialObjectContracts", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListCodeTablesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Codetables", query, cancellationToken: cancellationToken);
}
