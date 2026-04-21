namespace ABACUS.Core;

/// <summary>
/// Unified exception type thrown by SDK wrappers.
/// </summary>
public sealed class AbacusApiException : Exception
{
    /// <summary>
    /// HTTP status code when available.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Raw response body when available.
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Response headers when available.
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// Creates a normalized ABACUS API exception.
    /// </summary>
    public AbacusApiException(
        string message,
        int? statusCode = null,
        string? responseBody = null,
        IReadOnlyDictionary<string, IEnumerable<string>>? headers = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Headers = headers ?? new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
    }
}
