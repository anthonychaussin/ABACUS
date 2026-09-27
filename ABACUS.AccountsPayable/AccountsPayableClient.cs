using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.AccountsPayable;

/// <summary>
/// Default implementation for Accounts Payable wrapper methods.
/// </summary>
public sealed class AccountsPayableClient : IAccountsPayableClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "AccountsPayable";

    /// <inheritdoc />
    public ABACUS_AccountsPayableClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public AccountsPayableClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_AccountsPayableClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListSuppliersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Suppliers", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateSuppliersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(_httpClient, "/Suppliers", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetSupplierAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var path = query is null
            ? $"/Suppliers(Id={id})"
            : query.ApplyTo($"/Suppliers(Id={id})");
        return AbacusHttp.SendJsonAsync<JsonElement>(_httpClient, HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateSupplierAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(_httpClient, HttpMethod.Post, "/Suppliers", payload, prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateSupplierAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);
        return CreateSupplierAsync(mapper.ToPayload("Supplier", model), prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchSupplierAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Patch,
            $"/Suppliers(Id={id})",
            payload,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchSupplierAsync<TModel>(
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);
        return PatchSupplierAsync(id, mapper.ToPayload("Supplier", model), prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<T>> ListSuppliersAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync<T>(_httpClient, "/Suppliers", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> DeleteSupplierAsync(
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Delete,
            $"/Suppliers(Id={id})",
            payload: null,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListSupplierCurrenciesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(
            _httpClient,
            "/SupplierCurrencies",
            query,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListSupplierPaymentMethodsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(
            _httpClient,
            "/SupplierPaymentMethods",
            query,
            cancellationToken: cancellationToken);
}
