using Microsoft.Extensions.Logging;

namespace ABACUS.Core;

/// <summary>
/// Optional diagnostic handler that logs ABACUS request method, path and status.
/// Does not log bodies or authorization headers.
/// </summary>
public sealed class AbacusLoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a logging handler.
    /// </summary>
    public AbacusLoggingHandler(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a logging handler from an <see cref="ILoggerFactory"/>.
    /// </summary>
    public AbacusLoggingHandler(ILoggerFactory loggerFactory)
        : this((loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger("ABACUS.SDK"))
    {
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var path = request.RequestUri?.PathAndQuery ?? request.RequestUri?.ToString() ?? string.Empty;
        _logger.LogDebug("ABACUS {Method} {Path}", request.Method.Method, path);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(
            "ABACUS {Method} {Path} -> {StatusCode}",
            request.Method.Method,
            path,
            (int)response.StatusCode);
        return response;
    }
}
