using System.Diagnostics;
using System.Net;

namespace ABACUS.Core;

/// <summary>
/// Retries ABACUS requests that fail with HTTP 429 (rate limit).
/// </summary>
public sealed class AbacusRateLimitHandler : DelegatingHandler
{
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates a handler that retries 429 responses with exponential backoff.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retries after the first 429.</param>
    /// <param name="baseDelay">Initial delay before the first retry.</param>
    /// <param name="timeProvider">Clock used for delays (injectable for tests).</param>
    public AbacusRateLimitHandler(
        int maxRetries = 3,
        TimeSpan? baseDelay = null,
        TimeProvider? timeProvider = null)
    {
        if (maxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Max retries cannot be negative.");
        }

        _maxRetries = maxRetries;
        _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(500);
        if (_baseDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(baseDelay), "Base delay cannot be negative.");
        }

        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpResponseMessage? response = null;
        for (var attempt = 0; ; attempt++)
        {
            response?.Dispose();
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.TooManyRequests || attempt >= _maxRetries)
            {
                return response;
            }

            Activity.Current?.SetTag("abacus.retry", attempt + 1);
            var delay = ResolveDelay(response, attempt);
            response.Dispose();
            response = null;
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    private TimeSpan ResolveDelay(HttpResponseMessage response, int attempt)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
        {
            return delta;
        }

        if (response.Headers.RetryAfter?.Date is { } date)
        {
            var remaining = date - _timeProvider.GetUtcNow();
            if (remaining > TimeSpan.Zero)
            {
                return remaining;
            }
        }

        var factor = Math.Pow(2, attempt);
        return TimeSpan.FromTicks((long)(_baseDelay.Ticks * factor));
    }
}
