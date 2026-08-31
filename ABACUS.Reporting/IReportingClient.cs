using ABACUS.Core;

namespace ABACUS.Reporting;

/// <summary>Client for the AbaReport REST API v1.</summary>
public interface IReportingClient : IAbacusModuleClient
{
    Task<ReportJob> StartReportAsync(
        int clientNumber,
        string reportName,
        ReportRequest request,
        CancellationToken cancellationToken = default);

    Task<ReportJob> GetJobAsync(string jobId, CancellationToken cancellationToken = default);

    Task<ReportOutput> GetOutputAsync(
        string jobId,
        int page = 1,
        CancellationToken cancellationToken = default);

    Task CloseJobAsync(string jobId, CancellationToken cancellationToken = default);
}
