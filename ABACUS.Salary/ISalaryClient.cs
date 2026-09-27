using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.Salary;

/// <summary>
/// High-level SDK facade for the Salary module.
/// </summary>
public interface ISalaryClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_SalaryClient Raw { get; }

    /// <summary>Lists employee personnel data sets (first page).</summary>
    Task<ODataPage<JsonElement>> ListEmployeePersonnelDataSetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Enumerates employee personnel data sets.</summary>
    IAsyncEnumerable<JsonElement> EnumerateEmployeePersonnelDataSetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a personnel data set by composite key.</summary>
    Task<AbacusResponse<JsonElement>> GetEmployeePersonnelDataSetAsync(
        string employeeId,
        string contractId,
        string validFrom,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists employee payroll data sets (first page).</summary>
    Task<ODataPage<JsonElement>> ListEmployeePayrollDataSetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Enumerates employee payroll data sets.</summary>
    IAsyncEnumerable<JsonElement> EnumerateEmployeePayrollDataSetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a payroll data set by composite key.</summary>
    Task<AbacusResponse<JsonElement>> GetEmployeePayrollDataSetAsync(
        string employeeId,
        string contractId,
        string validFrom,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists job assignments (first page).</summary>
    Task<ODataPage<JsonElement>> ListJobAssignmentsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a job assignment by id.</summary>
    Task<AbacusResponse<JsonElement>> GetJobAssignmentAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a job assignment.</summary>
    Task<AbacusResponse<string>> CreateJobAssignmentAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>Patches a job assignment.</summary>
    Task<AbacusResponse<string>> PatchJobAssignmentAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a job assignment.</summary>
    Task<AbacusResponse<string>> DeleteJobAssignmentAsync(
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists employee entry/leaving records (first page).</summary>
    Task<ODataPage<JsonElement>> ListEmployeeEntryLeavingsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an employee entry/leaving record by id.</summary>
    Task<AbacusResponse<JsonElement>> GetEmployeeEntryLeavingAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default);
}
