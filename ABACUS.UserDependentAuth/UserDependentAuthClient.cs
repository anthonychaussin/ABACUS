using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.UserDependentAuth;

/// <summary>
/// Default implementation for the UserDependentAuth module wrapper.
/// Prefer <see cref="AbacusServiceCollectionExtensions.AddAbacusAuthorizationCode"/> for authentication.
/// </summary>
public sealed class UserDependentAuthClient : IUserDependentAuthClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "UserDependentAuth";

    /// <inheritdoc />
    public ABACUS_UserDependentAuthClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public UserDependentAuthClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_UserDependentAuthClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListProjectDocumentsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/ProjectDocuments", query, cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListStoragesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/Storages", query, cancellationToken);
}
