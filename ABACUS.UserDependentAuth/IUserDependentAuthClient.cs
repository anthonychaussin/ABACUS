using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.UserDependentAuth;

/// <summary>
/// High-level SDK facade for endpoints that typically require authorization-code (user) auth.
/// </summary>
public interface IUserDependentAuthClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_UserDependentAuthClient Raw { get; }

    /// <summary>Lists project documents (first page).</summary>
    Task<ODataPage<JsonElement>> ListProjectDocumentsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists storages (first page).</summary>
    Task<ODataPage<JsonElement>> ListStoragesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);
}
