using System.Text.Json;

namespace ABACUS.Core;

/// <summary>
/// Generic client for ABACUS entity endpoints that are not yet exposed by a module facade.
/// </summary>
/// <remarks>
/// This client keeps the SDK forward-compatible with entity sets introduced in newer ABACUS
/// releases. The configured <see cref="HttpClient.BaseAddress"/> must point at the mandant entity
/// API, for example <c>https://server/api/entity/v1/mandants/7777/</c>.
/// </remarks>
public sealed class AbacusEntityClient : IAbacusModuleClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "Entity";

    /// <summary>Creates a generic entity client from a configured HTTP client.</summary>
    public AbacusEntityClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <summary>Lists one page from an entity set.</summary>
    public Task<ODataPage<T>> ListAsync<T>(
        string entitySet,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.GetODataPageAsync<T>(_httpClient, EntitySetPath(entitySet), query, cancellationToken: cancellationToken);

    /// <summary>Enumerates every page from an entity set.</summary>
    public IAsyncEnumerable<T> EnumerateAsync<T>(
        string entitySet,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.EnumerateODataAsync<T>(_httpClient, EntitySetPath(entitySet), query, cancellationToken: cancellationToken);

    /// <summary>Gets an entity by an OData key expression.</summary>
    /// <param name="keyExpression">A scalar key or composite expression, for example <c>42</c> or <c>Id=42,IssueNumber='A-1'</c>.</param>
    public Task<AbacusResponse<T>> GetAsync<T>(
        string entitySet,
        string keyExpression,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyExpression);
        var path = $"{EntitySetPath(entitySet)}({keyExpression})";
        return AbacusHttp.SendJsonAsync<T>(
            _httpClient,
            HttpMethod.Get,
            query?.ApplyTo(path) ?? path,
            cancellationToken: cancellationToken);
    }

    /// <summary>Creates an entity.</summary>
    public Task<AbacusResponse<T>> CreateAsync<T>(
        string entitySet,
        object payload,
        string? prefer = AbacusHttp.PreferReturnRepresentation,
        CancellationToken cancellationToken = default) =>
        AbacusHttp.SendJsonAsync<T>(_httpClient, HttpMethod.Post, EntitySetPath(entitySet), payload, prefer, cancellationToken);

    /// <summary>Partially updates an entity.</summary>
    public Task<AbacusResponse<T>> UpdateAsync<T>(
        string entitySet,
        string keyExpression,
        object payload,
        string? prefer = AbacusHttp.PreferReturnRepresentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyExpression);
        return AbacusHttp.SendJsonAsync<T>(
            _httpClient,
            HttpMethod.Patch,
            $"{EntitySetPath(entitySet)}({keyExpression})",
            payload,
            prefer,
            cancellationToken);
    }

    /// <summary>Deletes an entity.</summary>
    public Task<AbacusResponse<string>> DeleteAsync(
        string entitySet,
        string keyExpression,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyExpression);
        return AbacusHttp.SendAsync(
            _httpClient,
            HttpMethod.Delete,
            $"{EntitySetPath(entitySet)}({keyExpression})",
            cancellationToken: cancellationToken);
    }

    /// <summary>Returns an entity as a JSON element when no DTO is available.</summary>
    public Task<AbacusResponse<JsonElement>> GetAsync(
        string entitySet,
        string keyExpression,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        GetAsync<JsonElement>(entitySet, keyExpression, query, cancellationToken);

    private static string EntitySetPath(string entitySet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entitySet);
        if (entitySet.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
        {
            throw new ArgumentException("Entity set names may contain only letters, digits, and underscores.", nameof(entitySet));
        }

        // Keep this relative so HttpClient preserves the configured
        // /api/entity/v1/mandants/{mandant}/ base path.
        return entitySet;
    }
}
