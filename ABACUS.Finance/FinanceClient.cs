using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.Finance;

/// <summary>
/// Default implementation for the Finance module wrapper.
/// </summary>
public sealed class FinanceClient : IFinanceClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "Finance";

    /// <inheritdoc />
    public ABACUS_FinanceClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public FinanceClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_FinanceClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Accounts", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(_httpClient, "/Accounts", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListGeneralLedgerEntriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(
            _httpClient,
            "/GeneralLedgerEntries",
            query,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateGeneralLedgerEntriesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(
            _httpClient,
            "/GeneralLedgerEntries",
            query,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateAccountAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(_httpClient, HttpMethod.Post, "/Accounts", payload, prefer, cancellationToken);
    }
}
