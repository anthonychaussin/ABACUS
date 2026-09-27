using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.WebShop;

/// <summary>
/// High-level SDK facade for the WebShop module.
/// </summary>
public interface IWebShopClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_WebShopClient Raw { get; }

    /// <summary>
    /// Lists shopper accounts (first page).
    /// </summary>
    Task<ODataPage<JsonElement>> ListShopperAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists shopper accounts deserialized as <typeparamref name="T"/>.
    /// </summary>
    Task<ODataPage<T>> ListShopperAccountsAsAsync<T>(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates shopper accounts following <c>@odata.nextLink</c>.
    /// </summary>
    IAsyncEnumerable<JsonElement> EnumerateShopperAccountsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a shopper account by id.
    /// </summary>
    Task<AbacusResponse<JsonElement>> GetShopperAccountAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a shopper account from a JSON-compatible payload.
    /// </summary>
    Task<AbacusResponse<string>> CreateShopperAccountAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a shopper account by mapping a model with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    Task<AbacusResponse<string>> CreateShopperAccountAsync<TModel>(
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches a shopper account.
    /// </summary>
    Task<AbacusResponse<string>> PatchShopperAccountAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches a shopper account by mapping a model with <see cref="IAbacusFieldMapper"/>.
    /// </summary>
    Task<AbacusResponse<string>> PatchShopperAccountAsync<TModel>(
        string id,
        TModel model,
        IAbacusFieldMapper mapper,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a shopper account.
    /// </summary>
    Task<AbacusResponse<string>> DeleteShopperAccountAsync(
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
