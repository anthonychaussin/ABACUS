using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ABACUS.Core;

/// <summary>
/// Background worker that polls <see cref="IAbacusChangeFeed"/> until cancelled.
/// </summary>
public sealed class AbacusChangeFeedHostedService : BackgroundService
{
    private readonly IAbacusChangeFeed _changeFeed;
    private readonly AbacusChangeFeedOptions _options;
    private readonly ILogger<AbacusChangeFeedHostedService> _logger;

    /// <summary>
    /// Creates the hosted change feed worker.
    /// </summary>
    public AbacusChangeFeedHostedService(
        IAbacusChangeFeed changeFeed,
        IOptions<AbacusChangeFeedOptions> options,
        ILogger<AbacusChangeFeedHostedService> logger)
    {
        _changeFeed = changeFeed ?? throw new ArgumentNullException(nameof(changeFeed));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.Topics.Count == 0)
        {
            _logger.LogWarning("Abacus change feed hosted service has no topics configured; exiting.");
            return;
        }

        if (_options.SubscribeOnStart)
        {
            await _changeFeed.SubscribeAsync(_options.SubscriptionName, _options.Topics, stoppingToken)
                .ConfigureAwait(false);
            _logger.LogInformation(
                "Subscribed change feed {Subscription} for {TopicCount} topic(s).",
                _options.SubscriptionName,
                _options.Topics.Count);
        }

        var emptyCycles = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = AbacusTelemetry.ActivitySource.StartActivity(
                "ABACUS ChangeFeed poll",
                ActivityKind.Internal);
            activity?.SetTag("abacus.subscription", _options.SubscriptionName);

            var processed = 0;
            await foreach (var batch in _changeFeed.ConsumeUntilEmptyAsync(_options.SubscriptionName, stoppingToken)
                               .ConfigureAwait(false))
            {
                processed += batch.Changes.Count;
                _logger.LogDebug(
                    "Change feed {Subscription}: batch with {Count} change(s).",
                    _options.SubscriptionName,
                    batch.Changes.Count);
            }

            activity?.SetTag("abacus.changes", processed);
            if (processed == 0)
            {
                emptyCycles++;
                if (_options.MaxEmptyCycles > 0 && emptyCycles >= _options.MaxEmptyCycles)
                {
                    _logger.LogInformation(
                        "Change feed {Subscription} reached MaxEmptyCycles={Max}; stopping.",
                        _options.SubscriptionName,
                        _options.MaxEmptyCycles);
                    break;
                }

                await Task.Delay(_options.PollInterval, stoppingToken).ConfigureAwait(false);
            }
            else
            {
                emptyCycles = 0;
            }
        }
    }
}
