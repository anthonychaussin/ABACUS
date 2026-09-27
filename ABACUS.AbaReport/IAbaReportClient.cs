using ABACUS.Core;

namespace ABACUS.AbaReport;

/// <summary>
/// High-level SDK facade for the ABACUS AbaReport REST API.
/// </summary>
public interface IAbaReportClient : IAbacusModuleClient
{
    /// <summary>
    /// Lists available AbaReports configured on the server.
    /// </summary>
    Task<AbacusResponse<string>> ListReportsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes/exports a report by name or id.
    /// </summary>
    /// <param name="reportId">Report identifier as configured in Abacus.</param>
    /// <param name="format">Optional output format (for example <c>txt</c>, <c>pdf</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<AbacusResponse<byte[]>> ExportReportAsync(
        string reportId,
        string? format = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a report with a JSON parameter payload.
    /// </summary>
    Task<AbacusResponse<byte[]>> ExportReportAsync(
        string reportId,
        object parameters,
        string? format = null,
        CancellationToken cancellationToken = default);
}
