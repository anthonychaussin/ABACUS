using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.AccountsReceivable;

/// <summary>
/// Default implementation for the AccountsReceivable module wrapper.
/// </summary>
public sealed class AccountsReceivableClient : IAccountsReceivableClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "AccountsReceivable";

    /// <inheritdoc />
    public ABACUS_AccountsReceivableClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public AccountsReceivableClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_AccountsReceivableClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListCustomersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Customers", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateCustomersAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(_httpClient, "/Customers", query, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetCustomerAsync(
        int customerId,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        EnsureCustomerId(customerId);
        var path = $"/Customers(Id={customerId})";
        if (query is not null)
        {
            path = query.ApplyTo(path);
        }

        return AbacusHttp.SendJsonAsync<JsonElement>(_httpClient, HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateCustomerAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(_httpClient, HttpMethod.Post, "/Customers", payload, prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateCustomerAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);
        return CreateCustomerAsync(mapper.ToPayload("Customer", model), prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchCustomerAsync(
        int customerId,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        EnsureCustomerId(customerId);
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Patch,
            $"/Customers(Id={customerId})",
            payload,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchCustomerAsync<TModel>(
        int customerId,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);
        return PatchCustomerAsync(customerId, mapper.ToPayload("Customer", model), prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> DeleteCustomerAsync(
        int customerId,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        EnsureCustomerId(customerId);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Delete,
            $"/Customers(Id={customerId})",
            payload: null,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<T>> ListCustomersAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync<T>(_httpClient, "/Customers", query, cancellationToken);

    private static void EnsureCustomerId(int customerId)
    {
        if (customerId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(customerId), "customerId must be greater than 0.");
        }
    }
}
