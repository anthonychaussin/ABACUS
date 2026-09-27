namespace ABACUS.Core;

/// <summary>
/// One entry from OData <c>error.details</c>.
/// </summary>
public sealed class AbacusODataErrorDetail
{
    /// <summary>
    /// Creates a detail entry.
    /// </summary>
    public AbacusODataErrorDetail(string? code, string? message, string? target)
    {
        Code = code;
        Message = message;
        Target = target;
    }

    /// <summary>Error code.</summary>
    public string? Code { get; }

    /// <summary>Human-readable message.</summary>
    public string? Message { get; }

    /// <summary>Field / target path when present.</summary>
    public string? Target { get; }
}

/// <summary>
/// Unified exception type thrown by SDK wrappers.
/// </summary>
public class AbacusApiException : Exception
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
    /// OData <c>error.code</c> when parsed.
    /// </summary>
    public string? ODataErrorCode { get; }

    /// <summary>
    /// OData <c>error.message</c> when parsed.
    /// </summary>
    public string? ODataMessage { get; }

    /// <summary>
    /// OData <c>error.details</c> when parsed.
    /// </summary>
    public IReadOnlyList<AbacusODataErrorDetail> Details { get; }

    /// <summary>
    /// Creates a normalized ABACUS API exception.
    /// </summary>
    public AbacusApiException(
        string message,
        int? statusCode = null,
        string? responseBody = null,
        IReadOnlyDictionary<string, IEnumerable<string>>? headers = null,
        Exception? innerException = null,
        string? odataErrorCode = null,
        string? odataMessage = null,
        IReadOnlyList<AbacusODataErrorDetail>? details = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Headers = headers ?? new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
        ODataErrorCode = odataErrorCode;
        ODataMessage = odataMessage;
        Details = details ?? Array.Empty<AbacusODataErrorDetail>();
    }
}

/// <summary>
/// Thrown for HTTP 400 responses that include OData validation details.
/// </summary>
public sealed class AbacusValidationException : AbacusApiException
{
    /// <summary>
    /// Creates a validation exception.
    /// </summary>
    public AbacusValidationException(
        string message,
        int? statusCode = null,
        string? responseBody = null,
        IReadOnlyDictionary<string, IEnumerable<string>>? headers = null,
        Exception? innerException = null,
        string? odataErrorCode = null,
        string? odataMessage = null,
        IReadOnlyList<AbacusODataErrorDetail>? details = null)
        : base(
            message,
            statusCode,
            responseBody,
            headers,
            innerException,
            odataErrorCode,
            odataMessage,
            details)
    {
    }
}
