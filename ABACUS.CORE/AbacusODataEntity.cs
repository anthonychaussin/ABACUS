using System.Text.Json;

namespace ABACUS.Core;

/// <summary>
/// Thin helpers that wrap common OData entity CRUD patterns on top of <see cref="AbacusHttp"/>.
/// Useful when authoring new module facades without duplicating List/Get/Create/Patch/Delete.
/// </summary>
public static class AbacusODataEntity
{
    /// <summary>
    /// Lists the first page of a collection resource.
    /// </summary>
    public static Task<ODataPage<JsonElement>> ListAsync(
        HttpClient httpClient,
        string collectionPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<JsonElement>(httpClient, collectionPath, query, cancellationToken: cancellationToken);

    /// <summary>
    /// Enumerates all pages of a collection resource.
    /// </summary>
    public static IAsyncEnumerable<JsonElement> EnumerateAsync(
        HttpClient httpClient,
        string collectionPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<JsonElement>(httpClient, collectionPath, query, cancellationToken: cancellationToken);

    /// <summary>
    /// Gets a single entity by relative path (for example <c>/Suppliers(Id=1)</c>).
    /// </summary>
    public static Task<AbacusResponse<JsonElement>> GetAsync(
        HttpClient httpClient,
        string entityPath,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityPath);
        var path = query is null ? entityPath : query.ApplyTo(entityPath);
        return AbacusHttp.SendJsonAsync<JsonElement>(httpClient, HttpMethod.Get, path, cancellationToken: cancellationToken);
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
