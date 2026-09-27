using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.HumanResources;

/// <summary>
/// Default implementation for the HumanResources module wrapper.
/// </summary>
public sealed class HumanResourcesClient : IHumanResourcesClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "HumanResources";

    /// <inheritdoc />
    public ABACUS_HumanResourcesClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public HumanResourcesClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_HumanResourcesClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListOrganisationEmployeesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/OrganisationEmployees", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateOrganisationEmployeesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/OrganisationEmployees", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetOrganisationEmployeeAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("OrganisationEmployees", id),
            query,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListOrganisationOrgUnitsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/OrganisationOrgUnits", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetOrganisationOrgUnitAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("OrganisationOrgUnits", id),
            query,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListApplicantsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/Applicants", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetApplicantAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, AbacusODataEntity.ParenIdPath("Applicants", id), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListLearningTrainingsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/LearningTrainings", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateLearningTrainingAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/LearningTrainings", payload, prefer, cancellationToken);
}
