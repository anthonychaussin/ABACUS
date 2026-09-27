using System.Diagnostics;

namespace ABACUS.Core;

/// <summary>
/// OpenTelemetry / Activity instrumentation for ABACUS HTTP calls.
/// </summary>
public static class AbacusTelemetry
{
    /// <summary>
    /// Activity source name used by the SDK.
    /// </summary>
    public const string ActivitySourceName = "ABACUS.SDK";

    /// <summary>
    /// Shared activity source.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new(ActivitySourceName, "1.0.0");
}

/// <summary>
/// Delegating handler that creates an <see cref="Activity"/> per outbound ABACUS request.
/// </summary>
public sealed class AbacusTelemetryHandler : DelegatingHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        using var activity = AbacusTelemetry.ActivitySource.StartActivity(
            $"ABACUS {request.Method.Method} {path}",
            ActivityKind.Client);

        activity?.SetTag("http.request.method", request.Method.Method);
        activity?.SetTag("url.path", path);
        activity?.SetTag("abacus.path", request.RequestUri?.PathAndQuery ?? path);

        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            activity?.SetTag("http.response.status_code", (int)response.StatusCode);
            if (!response.IsSuccessStatusCode)
            {
                activity?.SetStatus(ActivityStatusCode.Error, response.ReasonPhrase);
            }

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
