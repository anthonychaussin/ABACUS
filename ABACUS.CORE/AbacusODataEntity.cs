using System.Text.Json;

namespace ABACUS.Core;

/// <summary>
/// Thin helpers that wrap common OData entity CRUD patterns on top of <see cref="AbacusHttp"/>.
/// Useful when authoring new module facades without duplicating List/Get/Create/Patch/Delete.
/// </summary>
public static class AbacusODataEntity
{
    /// <summary>
    /// Lists the first page of a collection resource as <see cref="JsonElement"/>.
    /// </summary>
    public static Task<ODataPage<JsonElement>> ListAsync(
        HttpClient httpClient,
        string collectionPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        ListAsync<JsonElement>(httpClient, collectionPath, query, cancellationToken);

    /// <summary>
    /// Lists the first page of a collection resource deserialized as <typeparamref name="T"/>.
    /// </summary>
    public static Task<ODataPage<T>> ListAsync<T>(
        HttpClient httpClient,
        string collectionPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<T>(httpClient, collectionPath, query, cancellationToken: cancellationToken);

    /// <summary>
    /// Enumerates all pages of a collection resource as <see cref="JsonElement"/>.
    /// </summary>
    public static IAsyncEnumerable<JsonElement> EnumerateAsync(
        HttpClient httpClient,
        string collectionPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync<JsonElement>(httpClient, collectionPath, query, cancellationToken);

    /// <summary>
    /// Enumerates all pages deserialized as <typeparamref name="T"/>.
    /// </summary>
    public static IAsyncEnumerable<T> EnumerateAsync<T>(
        HttpClient httpClient,
        string collectionPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<T>(httpClient, collectionPath, query, cancellationToken: cancellationToken);

    /// <summary>
    /// Gets a single entity as <see cref="JsonElement"/>.
    /// </summary>
    public static Task<AbacusResponse<JsonElement>> GetAsync(
        HttpClient httpClient,
        string entityPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        GetAsync<JsonElement>(httpClient, entityPath, query, cancellationToken);

    /// <summary>
    /// Gets a single entity deserialized as <typeparamref name="T"/>.
    /// </summary>
    public static Task<AbacusResponse<T>> GetAsync<T>(
        HttpClient httpClient,
        string entityPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityPath);
        var path = query is null ? entityPath : query.ApplyTo(entityPath);
        return AbacusHttp.SendJsonAsync<T>(httpClient, HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates an entity with POST.
    /// </summary>
    public static Task<AbacusResponse<string>> CreateAsync(
        HttpClient httpClient,
        string collectionPath,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(httpClient, HttpMethod.Post, collectionPath, payload, prefer, cancellationToken);
    }

    /// <summary>
    /// Creates an entity by mapping <paramref name="model"/> with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    public static Task<AbacusResponse<string>> CreateAsync<TModel>(
        HttpClient httpClient,
        string collectionPath,
        string entityName,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        return CreateAsync(httpClient, collectionPath, mapper.ToPayload(entityName, model), prefer, cancellationToken);
    }

    /// <summary>
    /// Patches an entity.
    /// </summary>
    public static Task<AbacusResponse<string>> PatchAsync(
        HttpClient httpClient,
        string entityPath,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return AbacusHttp.SendAsync(httpClient, HttpMethod.Patch, entityPath, payload, prefer, cancellationToken);
    }

    /// <summary>
    /// Patches an entity by mapping <paramref name="model"/> with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    public static Task<AbacusResponse<string>> PatchAsync<TModel>(
        HttpClient httpClient,
        string entityPath,
        string entityName,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        return PatchAsync(httpClient, entityPath, mapper.ToPayload(entityName, model), prefer, cancellationToken);
    }

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    public static Task<AbacusResponse<string>> DeleteAsync(
        HttpClient httpClient,
        string entityPath,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.SendAsync(httpClient, HttpMethod.Delete, entityPath, payload: null, prefer, cancellationToken);

    /// <summary>
    /// Builds a simple <c>/Collection(Id={id})</c> path.
    /// </summary>
    public static string IdPath(string collectionName, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var name = collectionName.Trim('/');
        return $"/{name}(Id={id})";
    }

    /// <summary>
    /// Builds a parenthesized key path <c>/Collection({id})</c> (AssetsLedger style).
    /// </summary>
    public static string ParenIdPath(string collectionName, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var name = collectionName.Trim('/');
        return $"/{name}({id})";
    }
}
