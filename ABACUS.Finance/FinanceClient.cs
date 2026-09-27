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
        AbacusODataEntity.ListAsync(_httpClient, "/Accounts", query, cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<T>> ListAccountsAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync<T>(_httpClient, "/Accounts", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/Accounts", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetAccountAsync(
        string enterpriseId,
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(enterpriseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var path = AccountPath(enterpriseId, id);
        path = query is null ? path : query.ApplyTo(path);
        return AbacusHttp.SendJsonAsync<JsonElement>(_httpClient, HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

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
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/Accounts", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateAccountAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/Accounts", "Account", model, mapper, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchAccountAsync(
        string enterpriseId,
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(enterpriseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(
            _httpClient,
            AccountPath(enterpriseId, id),
            payload,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchAccountAsync<TModel>(
        string enterpriseId,
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(enterpriseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(
            _httpClient,
            AccountPath(enterpriseId, id),
            "Account",
            model,
            mapper,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> DeleteAccountAsync(
        string enterpriseId,
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(enterpriseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Delete,
            AccountPath(enterpriseId, id),
            payload: null,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListCostCentresAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/CostCentres", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateCostCentresAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/CostCentres", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetCostCentreAsync(
        string enterpriseId,
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(enterpriseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, CostCentrePath(enterpriseId, id), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateCostCentreAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/CostCentres", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchCostCentreAsync(
        string enterpriseId,
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(enterpriseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(_httpClient, CostCentrePath(enterpriseId, id), payload, prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> DeleteCostCentreAsync(
        string enterpriseId,
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(enterpriseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.DeleteAsync(_httpClient, CostCentrePath(enterpriseId, id), prefer, cancellationToken);
    }

    private static string AccountPath(string enterpriseId, string id) =>
        $"/Accounts(EnterpriseId={enterpriseId},Id={id})";

    private static string CostCentrePath(string enterpriseId, string id) =>
        $"/CostCentres(EnterpriseId={enterpriseId},Id={id})";
}
