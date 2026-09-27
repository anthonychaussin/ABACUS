using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.CRM;

/// <summary>
/// Default implementation for the CRM module wrapper.
/// </summary>
public sealed class CRMClient : ICRMClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "CRM";

    /// <inheritdoc />
    public ABACUS_CRMClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public CRMClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_CRMClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListSubjectsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/Subjects", query, cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<T>> ListSubjectsAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync<T>(_httpClient, "/Subjects", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateSubjectsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/Subjects", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetSubjectAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var path = query is null ? $"/Subjects({id})" : query.ApplyTo($"/Subjects({id})");
        return AbacusHttp.SendJsonAsync<JsonElement>(_httpClient, HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateSubjectAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(_httpClient, HttpMethod.Post, "/Subjects", payload, prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateSubjectAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);
        return CreateSubjectAsync(mapper.ToPayload("Subject", model), prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchSubjectAsync(
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
            $"/Subjects({id})",
            payload,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchSubjectAsync<TModel>(
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);
        return PatchSubjectAsync(id, mapper.ToPayload("Subject", model), prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> DeleteSubjectAsync(
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Delete,
            $"/Subjects({id})",
            payload: null,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListAddressesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(_httpClient, "/Addresses", query, cancellationToken: cancellationToken);
}
