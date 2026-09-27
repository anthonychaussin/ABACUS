using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.General;

/// <summary>
/// Default implementation for the General module wrapper.
/// </summary>
public sealed class GeneralClient : IGeneralClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "General";

    /// <inheritdoc />
    public ABACUS_GeneralClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public GeneralClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_GeneralClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataBatchResponse> PostBatchAsync(
        ODataBatchRequest batch,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        return AbacusHttp.SendBatchAsync(_httpClient, batch, prefer, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListCountriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Countries", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateCountriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(_httpClient, "/Countries", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListCurrenciesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Currencies", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateCurrenciesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(_httpClient, "/Currencies", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListDivisionsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Divisions", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListEnterprisesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Enterprises", query, cancellationToken: cancellationToken);
}
