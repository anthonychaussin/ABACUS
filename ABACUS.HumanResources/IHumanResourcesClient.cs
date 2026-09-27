using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.HumanResources;

/// <summary>
/// High-level SDK facade for the HumanResources module.
/// </summary>
public interface IHumanResourcesClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_HumanResourcesClient Raw { get; }

    /// <summary>Lists organisation employees (first page).</summary>
    Task<ODataPage<JsonElement>> ListOrganisationEmployeesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Enumerates organisation employees.</summary>
    IAsyncEnumerable<JsonElement> EnumerateOrganisationEmployeesAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an organisation employee by id.</summary>
    Task<AbacusResponse<JsonElement>> GetOrganisationEmployeeAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists organisation org units (first page).</summary>
    Task<ODataPage<JsonElement>> ListOrganisationOrgUnitsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an organisation org unit by id.</summary>
    Task<AbacusResponse<JsonElement>> GetOrganisationOrgUnitAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists applicants (first page).</summary>
    Task<ODataPage<JsonElement>> ListApplicantsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an applicant by id.</summary>
    Task<AbacusResponse<JsonElement>> GetApplicantAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists learning trainings (first page).</summary>
    Task<ODataPage<JsonElement>> ListLearningTrainingsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a learning training.</summary>
    Task<AbacusResponse<string>> CreateLearningTrainingAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
