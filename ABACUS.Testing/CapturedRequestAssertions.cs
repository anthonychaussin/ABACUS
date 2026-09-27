namespace ABACUS.Testing;

/// <summary>
/// Assertion helpers for <see cref="CapturedHttpRequest"/>.
/// </summary>
public static class CapturedRequestAssertions
{
    /// <summary>
    /// Ensures the request uses the expected HTTP method.
    /// </summary>
    public static CapturedHttpRequest HasMethod(this CapturedHttpRequest request, HttpMethod method)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(method);
        if (request.Method != method)
        {
            throw new InvalidOperationException($"Expected method {method}, got {request.Method}.");
        }

        return request;
    }

    /// <summary>
    /// Ensures the Prefer header equals <paramref name="prefer"/>.
    /// </summary>
    public static CapturedHttpRequest HasPrefer(this CapturedHttpRequest request, string prefer)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!string.Equals(request.Prefer, prefer, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected Prefer '{prefer}', got '{request.Prefer}'.");
        }

        return request;
    }

    /// <summary>
    /// Ensures the request path (AbsolutePath) equals <paramref name="absolutePath"/>.
    /// </summary>
    public static CapturedHttpRequest HasPath(this CapturedHttpRequest request, string absolutePath)
    {
        ArgumentNullException.ThrowIfNull(request);
        var actual = request.RequestUri?.AbsolutePath;
        if (!string.Equals(actual, absolutePath, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected path '{absolutePath}', got '{actual}'.");
        }

        return request;
    }
}
