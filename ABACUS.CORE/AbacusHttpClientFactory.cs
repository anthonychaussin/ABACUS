using Microsoft.Extensions.Logging;

namespace ABACUS.Core;

/// <summary>
/// Builds configured <see cref="HttpClient"/> instances for ABACUS modules.
/// </summary>
public static class AbacusHttpClientFactory
{
    /// <summary>
    /// Logical name used for the shared ABACUS SDK HTTP client in DI.
    /// </summary>
    public const string HttpClientName = "ABACUS.SDK";

    /// <summary>
    /// Logical name used for the unauthenticated client that requests OAuth tokens.
    /// </summary>
    public const string TokenHttpClientName = "ABACUS.SDK.Token";

    /// <summary>
    /// Logical name used for the AbaReport REST client (server origin BaseAddress).
    /// </summary>
    public const string AbaReportHttpClientName = "ABACUS.SDK.AbaReport";

    /// <summary>
    /// Applies ABACUS SDK HTTP configuration to an existing client instance.
    /// </summary>
    public static void Configure(HttpClient client, AbacusClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        client.BaseAddress = options.ResolveBaseUri();
        client.Timeout = options.Timeout;

        if (!string.IsNullOrWhiteSpace(options.UserAgent))
        {
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        }

        foreach (var (name, value) in options.DefaultHeaders)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
        }

        if (!string.IsNullOrWhiteSpace(options.Prefer))
        {
            client.DefaultRequestHeaders.Remove("Prefer");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Prefer", options.Prefer);
        }
    }

    /// <summary>
    /// Creates an HTTP client configured with base URI, headers, timeout, rate-limit retries and optional authentication.
    /// </summary>
    public static HttpClient Create(
        AbacusClientOptions options,
        IAbacusAuthenticationProvider? authenticationProvider = null,
        ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        HttpMessageHandler handler = new HttpClientHandler();
        if (options.RateLimitMaxRetries > 0)
        {
            handler = new AbacusRateLimitHandler(options.RateLimitMaxRetries, options.RateLimitBaseDelay)
            {
                InnerHandler = handler,
            };
        }

        if (authenticationProvider is not null)
        {
            handler = new AbacusAuthenticationHandler(authenticationProvider) { InnerHandler = handler };
        }

        if (options.EnableRequestLogging && loggerFactory is not null)
        {
            handler = new AbacusLoggingHandler(loggerFactory) { InnerHandler = handler };
        }

        if (options.EnableOpenTelemetry)
        {
            handler = new AbacusTelemetryHandler { InnerHandler = handler };
        }

        if (options.EnableReadRetry)
        {
            handler = new AbacusReadRetryHandler(options.ReadRetryMaxAttempts) { InnerHandler = handler };
        }

        var client = new HttpClient(handler);
        Configure(client, options);
        return client;
    }
}
