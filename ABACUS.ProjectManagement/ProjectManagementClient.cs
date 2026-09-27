using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.ProjectManagement;

/// <summary>
/// Default implementation for the ProjectManagement module wrapper.
/// </summary>
public sealed class ProjectManagementClient : IProjectManagementClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "ProjectManagement";

    /// <inheritdoc />
    public ABACUS_ProjectManagementClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public ProjectManagementClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_ProjectManagementClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListProjectsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/Projects", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateProjectsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/Projects", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetProjectAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, AbacusODataEntity.IdPath("Projects", id), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateProjectAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/Projects", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchProjectAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(_httpClient, AbacusODataEntity.IdPath("Projects", id), payload, prefer, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListProjectBookingsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/ProjectBookings", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetProjectBookingAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, AbacusODataEntity.ParenIdPath("ProjectBookings", id), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateProjectBookingAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/ProjectBookings", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListProjectDocumentsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/ProjectDocuments", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetProjectDocumentAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, AbacusODataEntity.ParenIdPath("ProjectDocuments", id), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListMonthlyPlanningsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/MonthlyPlannings", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateMonthlyPlanningAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/MonthlyPlannings", payload, prefer, cancellationToken);
}
