using System.Text.Json;
using ABACUS.Core;

namespace ABACUS.Salary;

/// <summary>
/// Default implementation for the Salary module wrapper.
/// </summary>
public sealed class SalaryClient : ISalaryClient
{
    private readonly HttpClient _httpClient;

    /// <inheritdoc />
    public string ModuleName => "Salary";

    /// <inheritdoc />
    public ABACUS_SalaryClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public SalaryClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        Raw = new ABACUS_SalaryClient(httpClient);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListEmployeePersonnelDataSetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/EmployeePersonnelDataSet", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateEmployeePersonnelDataSetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/EmployeePersonnelDataSet", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetEmployeePersonnelDataSetAsync(
        string employeeId,
        string contractId,
        string validFrom,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contractId);
        ArgumentException.ThrowIfNullOrWhiteSpace(validFrom);
        return AbacusODataEntity.GetAsync(_httpClient, PersonnelPath(employeeId, contractId, validFrom), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListEmployeePayrollDataSetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/EmployeePayrollDataSet", query, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<JsonElement> EnumerateEmployeePayrollDataSetsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.EnumerateAsync(_httpClient, "/EmployeePayrollDataSet", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetEmployeePayrollDataSetAsync(
        string employeeId,
        string contractId,
        string validFrom,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contractId);
        ArgumentException.ThrowIfNullOrWhiteSpace(validFrom);
        return AbacusODataEntity.GetAsync(_httpClient, PayrollPath(employeeId, contractId, validFrom), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListJobAssignmentsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/JobAssignments", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetJobAssignmentAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(_httpClient, AbacusODataEntity.ParenIdPath("JobAssignments", id), query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> CreateJobAssignmentAsync(
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.CreateAsync(_httpClient, "/JobAssignments", payload, prefer, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<string>> PatchJobAssignmentAsync(
        string id,
        object payload,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.PatchAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("JobAssignments", id),
            payload,
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AbacusResponse<string>> DeleteJobAssignmentAsync(
        string id,
        string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.DeleteAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("JobAssignments", id),
            prefer,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ODataPage<JsonElement>> ListEmployeeEntryLeavingsAsync(
        ODataQuery? query = null,
        CancellationToken cancellationToken = default) =>
        AbacusODataEntity.ListAsync(_httpClient, "/EmployeeEntryLeavings", query, cancellationToken);

    /// <inheritdoc />
    public Task<AbacusResponse<JsonElement>> GetEmployeeEntryLeavingAsync(
        string id,
        ODataQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return AbacusODataEntity.GetAsync(
            _httpClient,
            AbacusODataEntity.ParenIdPath("EmployeeEntryLeavings", id),
            query,
            cancellationToken);
    }

    private static string PersonnelPath(string employeeId, string contractId, string validFrom) =>
        $"/EmployeePersonnelDataSet(EmployeeId={employeeId},ContractId={contractId},ValidFrom={validFrom})";

    private static string PayrollPath(string employeeId, string contractId, string validFrom) =>
        $"/EmployeePayrollDataSet(EmployeeId={employeeId},ContractId={contractId},ValidFrom={validFrom})";
}
