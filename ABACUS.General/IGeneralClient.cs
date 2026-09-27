using ABACUS.Core;

namespace ABACUS.General;

/// <summary>
/// High-level SDK facade for the General module.
/// </summary>
public interface IGeneralClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_GeneralClient Raw { get; }

    /// <summary>
    /// Executes an OData <c>$batch</c> request (counts as one call against Abacus rate limits).
    /// </summary>
    Task<ODataBatchResponse> PostBatchAsync(
        ODataBatchRequest batch,
        string? prefer = null,
        CancellationToken cancellationToken = default);
}
