/// <summary>
/// Exception type used by generated clients when an HTTP call fails.
/// </summary>
public partial class ApiException : System.Exception
{
    /// <summary>
    /// HTTP status code returned by the server.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Raw response payload when available.
    /// </summary>
    public string? Response { get; }

    /// <summary>
    /// Response headers captured from the failed call.
    /// </summary>
    public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; }

    /// <summary>
    /// Creates a new API exception with HTTP metadata.
    /// </summary>
    public ApiException(
        string message,
        int statusCode,
        string? response,
        System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>>? headers,
        System.Exception? innerException)
        : base(
            message + "\n\nStatus: " + statusCode + "\nResponse: \n" + (response == null ? "(null)" : response[..(response.Length >= 512 ? 512 : response.Length)]),
            innerException)
    {
        StatusCode = statusCode;
        Response = response;
        Headers = headers ?? new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<string>>(System.StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
    }
}

/// <summary>
/// API exception with a typed error payload.
/// </summary>
public partial class ApiException<TResult> : ApiException
{
    /// <summary>
    /// Typed result payload returned by the generated client.
    /// </summary>
    public TResult Result { get; }

    /// <summary>
    /// Creates a new typed API exception with HTTP metadata.
    /// </summary>
    public ApiException(
        string message,
        int statusCode,
        string? response,
        System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>>? headers,
        TResult result,
        System.Exception? innerException)
        : base(message, statusCode, response, headers, innerException)
    {
        Result = result;
    }
}
