using System.Net;

namespace ABACUS.Core;

/// <summary>
/// Retries transient failures for GET/HEAD requests only (never mutations).
/// </summary>
public sealed class AbacusReadRetryHandler : DelegatingHandler
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _baseDelay;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates a read-only retry handler.
    /// </summary>
    /// <param name="maxAttempts">Total attempts including the first try (minimum 1).</param>
    /// <param name="baseDelay">Backoff base delay.</param>
    /// <param name="timeProvider">Clock for delays.</param>
    public AbacusReadRetryHandler(
        int maxAttempts = 3,
        TimeSpan? baseDelay = null,
        TimeProvider? timeProvider = null)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be at least 1.");
        }

        _maxAttempts = maxAttempts;
        _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(200);
        if (_baseDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(baseDelay), "Base delay cannot be negative.");
        }

        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var isRead = request.Method == HttpMethod.Get || request.Method == HttpMethod.Head;
        if (!isRead || _maxAttempts <= 1)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        HttpResponseMessage? response = null;
        for (var attempt = 1; ; attempt++)
        {
            response?.Dispose();
            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException) when (attempt < _maxAttempts)
            {
                await Task.Delay(DelayFor(attempt), cancellationToken).ConfigureAwait(false);
                continue;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < _maxAttempts)
            {
                await Task.Delay(DelayFor(attempt), cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (response is not null &&
                IsTransient(response.StatusCode) &&
                attempt < _maxAttempts)
            {
                response.Dispose();
                response = null;
                await Task.Delay(DelayFor(attempt), cancellationToken).ConfigureAwait(false);
                continue;
            }

            return response!;
        }
    }

    private TimeSpan DelayFor(int attempt) =>
        TimeSpan.FromTicks((long)(_baseDelay.Ticks * Math.Pow(2, attempt - 1)));

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout
            || (int)statusCode >= 500;
}
