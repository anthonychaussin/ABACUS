using System.Diagnostics.CodeAnalysis;

namespace ABACUS.Core;

/// <summary>
/// Global options used to configure an ABACUS SDK client.
/// </summary>
public sealed class AbacusClientOptions
{
    /// <summary>
    /// Initializes an empty options object for later configuration.
    /// </summary>
    [SetsRequiredMembers]
    public AbacusClientOptions()
    {
        BaseUri = null!;
    }

    /// <summary>
    /// Base API URI, for example <c>https://host:40000/api/entity/v1/mandants/7777/</c>.
    /// When <see cref="ServerUri"/> and <see cref="Mandant"/> are set, this can be left unset
    /// and will be computed by <see cref="ResolveBaseUri"/>.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// ABACUS server origin used with <see cref="Mandant"/> to build the entity base URI,
    /// for example <c>https://host:40000</c>.
    /// </summary>
    public Uri? ServerUri { get; set; }

    /// <summary>
    /// Mandant (client) identifier included in the entity API path.
    /// </summary>
    public string? Mandant { get; set; }

    /// <summary>
    /// Value sent in the HTTP User-Agent header.
    /// </summary>
    public string UserAgent { get; set; } = "ABACUS.SDK/1.0";

    /// <summary>
    /// HTTP request timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Extra headers applied to all requests.
    /// </summary>
    public IDictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Optional default OData <c>Prefer</c> header (for example <see cref="AbacusHttp.PreferContinueOnError"/>).
    /// </summary>
    public string? Prefer { get; set; }

    /// <summary>
    /// Maximum number of retries after HTTP 429. Set to 0 to disable rate-limit retries.
    /// </summary>
    public int RateLimitMaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial delay used by the rate-limit retry backoff.
    /// </summary>
    public TimeSpan RateLimitBaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// When true, wraps the pipeline with <see cref="AbacusLoggingHandler"/> if an
    /// <c>ILogger</c>/<c>ILoggerFactory</c> is available (DI) or provided to the factory.
    /// </summary>
    public bool EnableRequestLogging { get; set; }

    /// <summary>
    /// When true, wraps the pipeline with <see cref="AbacusTelemetryHandler"/> (ActivitySource <c>ABACUS.SDK</c>).
    /// </summary>
    public bool EnableOpenTelemetry { get; set; }

    /// <summary>
    /// When true, wraps the pipeline with <see cref="AbacusReadRetryHandler"/> (GET/HEAD only).
    /// </summary>
    public bool EnableReadRetry { get; set; }

    /// <summary>
    /// Total attempts for read retries when <see cref="EnableReadRetry"/> is true (including the first try).
    /// </summary>
    public int ReadRetryMaxAttempts { get; set; } = 3;

    /// <summary>
    /// Resolves the effective base URI from <see cref="BaseUri"/> or <see cref="ServerUri"/> + <see cref="Mandant"/>.
    /// </summary>
    public Uri ResolveBaseUri()
    {
        if (BaseUri is not null)
        {
            if (!BaseUri.IsAbsoluteUri)
            {
                throw new ArgumentException("BaseUri must be absolute.", nameof(BaseUri));
            }

            return BaseUri;
        }

        if (ServerUri is null || string.IsNullOrWhiteSpace(Mandant))
        {
            throw new ArgumentException(
                "Either BaseUri, or both ServerUri and Mandant, must be configured.",
                nameof(BaseUri));
        }

        return AbacusEntityUri.BuildBaseUri(ServerUri, Mandant);
    }

    internal void Validate()
    {
        _ = ResolveBaseUri();

        if (RateLimitMaxRetries < 0)
        {
            throw new ArgumentException("RateLimitMaxRetries cannot be negative.", nameof(RateLimitMaxRetries));
        }

        if (RateLimitBaseDelay < TimeSpan.Zero)
        {
            throw new ArgumentException("RateLimitBaseDelay cannot be negative.", nameof(RateLimitBaseDelay));
        }

        if (ReadRetryMaxAttempts < 1)
        {
            throw new ArgumentException("ReadRetryMaxAttempts must be at least 1.", nameof(ReadRetryMaxAttempts));
        }
    }
}
