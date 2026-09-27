using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.ProjectManagement;

/// <summary>
/// High-level SDK facade for the ProjectManagement module.
/// </summary>
public interface IProjectManagementClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_ProjectManagementClient Raw { get; }

    /// <summary>Lists projects (first page).</summary>
    Task<ODataPage<JsonElement>> ListProjectsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Enumerates projects following <c>@odata.nextLink</c>.</summary>
    IAsyncEnumerable<JsonElement> EnumerateProjectsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a project by id.</summary>
    Task<AbacusResponse<JsonElement>> GetProjectAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a project.</summary>
    Task<AbacusResponse<string>> CreateProjectAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>Patches a project.</summary>
    Task<AbacusResponse<string>> PatchProjectAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists project bookings (first page).</summary>
    Task<ODataPage<JsonElement>> ListProjectBookingsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a project booking by id.</summary>
    Task<AbacusResponse<JsonElement>> GetProjectBookingAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a project booking.</summary>
    Task<AbacusResponse<string>> CreateProjectBookingAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists project documents (first page).</summary>
    Task<ODataPage<JsonElement>> ListProjectDocumentsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a project document by id.</summary>
    Task<AbacusResponse<JsonElement>> GetProjectDocumentAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists monthly plannings (first page).</summary>
    Task<ODataPage<JsonElement>> ListMonthlyPlanningsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a monthly planning.</summary>
    Task<AbacusResponse<string>> CreateMonthlyPlanningAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
