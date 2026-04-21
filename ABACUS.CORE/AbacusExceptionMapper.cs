namespace ABACUS.Core;

/// <summary>
/// Maps generated client exceptions to <see cref="AbacusApiException"/>.
/// </summary>
public static class AbacusExceptionMapper
{
    /// <summary>
    /// Converts a generated exception to a unified SDK exception when possible.
    /// </summary>
    public static Exception Map(Exception exception)
    {
        if (exception is AbacusApiException)
        {
            return exception;
        }

        var type = exception.GetType();
        var statusCodeProperty = type.GetProperty("StatusCode");
        var responseProperty = type.GetProperty("Response");
        var headersProperty = type.GetProperty("Headers");

        if (statusCodeProperty is null && responseProperty is null)
        {
            return exception;
        }

        var statusCode = statusCodeProperty?.GetValue(exception) as int?;
        var response = responseProperty?.GetValue(exception) as string;
        var headers = headersProperty?.GetValue(exception) as IReadOnlyDictionary<string, IEnumerable<string>>;

        return new AbacusApiException(
            message: exception.Message,
            statusCode: statusCode,
            responseBody: response,
            headers: headers,
            innerException: exception);
    }
}
