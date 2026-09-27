using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ABACUS.Core;

/// <summary>
/// Dependency injection helpers for ABACUS SDK consumers.
/// </summary>
public static class AbacusServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared ABACUS SDK <see cref="HttpClient"/> using the supplied base URI.
    /// </summary>
    /// <param name="services">Service collection to update.</param>
    /// <param name="baseUri">Base API URI used by ABACUS module clients.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusSdk(this IServiceCollection services, Uri baseUri)
    {
        ArgumentNullException.ThrowIfNull(baseUri);

        return services.AddAbacusSdk(new AbacusClientOptions
        {
            BaseUri = baseUri,
        });
    }

    /// <summary>
    /// Registers the shared ABACUS SDK <see cref="HttpClient"/> and allows additional option customization.
    /// </summary>
    /// <param name="services">Service collection to update.</param>
    /// <param name="configure">Delegate used to configure <see cref="AbacusClientOptions"/>.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusSdk(this IServiceCollection services, Action<AbacusClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new AbacusClientOptions();
        configure(options);

        return services.AddAbacusSdk(options);
    }

    /// <summary>
    /// Registers the shared ABACUS SDK <see cref="HttpClient"/> using fully constructed options.
    /// </summary>
    /// <param name="services">Service collection to update.</param>
    /// <param name="options">ABACUS SDK HTTP options.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusSdk(this IServiceCollection services, AbacusClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();

        services.AddSingleton(options);
        services.AddHttpClient(
            AbacusHttpClientFactory.HttpClientName,
            static (serviceProvider, client) =>
            {
                var resolvedOptions = serviceProvider.GetRequiredService<AbacusClientOptions>();
                AbacusHttpClientFactory.Configure(client, resolvedOptions);
            })
            .ConfigureAdditionalHttpMessageHandlers(static (handlers, serviceProvider) =>
            {
                var resolvedOptions = serviceProvider.GetRequiredService<AbacusClientOptions>();
                if (resolvedOptions.RateLimitMaxRetries > 0)
                {
                    handlers.Add(new AbacusRateLimitHandler(
                        resolvedOptions.RateLimitMaxRetries,
                        resolvedOptions.RateLimitBaseDelay));
                }

                var authenticationProvider = serviceProvider.GetService<IAbacusAuthenticationProvider>();
                if (authenticationProvider is not null)
                {
                    handlers.Add(new AbacusAuthenticationHandler(authenticationProvider));
                }

                if (resolvedOptions.EnableRequestLogging)
                {
                    var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                    if (loggerFactory is not null)
                    {
                        handlers.Add(new AbacusLoggingHandler(loggerFactory));
                    }
                }
            });

        return services;
    }

    /// <summary>
    /// Registers the shared ABACUS SDK <see cref="HttpClient"/> from the <c>Abacus</c> configuration section.
    /// Supported keys: <c>ServerUri</c>, <c>Mandant</c>, <c>BaseUri</c>, <c>UserAgent</c>, <c>Timeout</c>,
    /// <c>Prefer</c>, <c>RateLimitMaxRetries</c>, <c>RateLimitBaseDelay</c>, <c>EnableRequestLogging</c>.
    /// </summary>
    public static IServiceCollection AddAbacusSdk(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddAbacusSdk(BindClientOptions(configuration));
    }

    /// <summary>
    /// Registers an <see cref="IAbacusAuthenticationProvider"/> used for all ABACUS SDK requests.
    /// </summary>
    /// <typeparam name="TProvider">Authentication provider implementation.</typeparam>
    /// <param name="services">Service collection to update.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusAuthenticationProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IAbacusAuthenticationProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IAbacusAuthenticationProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Registers an authorization-code provider for user-dependent Abacus access.
    /// Call <see cref="AuthorizationCodeAuthenticationProvider.ExchangeCodeAsync"/> after the user signs in.
    /// </summary>
    public static IServiceCollection AddAbacusAuthorizationCode(
        this IServiceCollection services,
        AbacusAuthorizationCodeOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        services.AddHttpClient(AbacusHttpClientFactory.TokenHttpClientName);
        services.AddSingleton(options);
        services.AddSingleton<IAbacusAuthenticationProvider>(serviceProvider =>
        {
            var httpClient = serviceProvider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient(AbacusHttpClientFactory.TokenHttpClientName);
            return new AuthorizationCodeAuthenticationProvider(
                serviceProvider.GetRequiredService<AbacusAuthorizationCodeOptions>(),
                httpClient);
        });

        return services;
    }

    /// <summary>
    /// Registers a client-credentials provider that obtains a bearer from the ABACUS token endpoint.
    /// The server origin is <see cref="AbacusClientOptions.ServerUri"/> when set, otherwise the
    /// authority of the resolved base URI.
    /// </summary>
    /// <param name="services">Service collection to update.</param>
    /// <param name="clientId">Service-user client id.</param>
    /// <param name="clientSecret">Service-user client secret.</param>
    /// <param name="scopes">Optional OAuth scopes (maximum <see cref="AbacusClientCredentialsOptions.MaxScopes"/>).</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusClientCredentials(
        this IServiceCollection services,
        string clientId,
        string clientSecret,
        params string[] scopes)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);

        services.AddHttpClient(AbacusHttpClientFactory.TokenHttpClientName);
        services.AddSingleton<IAbacusAuthenticationProvider>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<AbacusClientOptions>();
            var httpClient = serviceProvider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient(AbacusHttpClientFactory.TokenHttpClientName);
            var serverUri = options.ServerUri ?? new Uri(options.ResolveBaseUri().GetLeftPart(UriPartial.Authority));

            return new ClientCredentialsAuthenticationProvider(
                new AbacusClientCredentialsOptions
                {
                    ServerUri = serverUri,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Scopes = scopes?.ToList() ?? new List<string>(),
                },
                httpClient);
        });

        return services;
    }

    /// <summary>
    /// Registers a concrete ABACUS module client that exposes an <see cref="IAbacusModuleClient"/>.
    /// </summary>
    /// <typeparam name="TClient">Concrete module client type.</typeparam>
    /// <param name="services">Service collection to update.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusModuleClient<TClient>(this IServiceCollection services)
        where TClient : class, IAbacusModuleClient
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<TClient>(CreateClient<TClient>);
        return services;
    }

    /// <summary>
    /// Registers an ABACUS module client behind its interface using the shared ABACUS SDK <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TClient">Service contract resolved by consumers.</typeparam>
    /// <typeparam name="TImplementation">Concrete module client implementation.</typeparam>
    /// <param name="services">Service collection to update.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusModuleClient<TClient, TImplementation>(this IServiceCollection services)
        where TClient : class, IAbacusModuleClient
        where TImplementation : class, TClient
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<TImplementation>(CreateClient<TImplementation>);
        services.AddTransient<TClient>(static serviceProvider => serviceProvider.GetRequiredService<TImplementation>());
        return services;
    }

    /// <summary>
    /// Registers a generated/raw ABACUS client that exposes a constructor accepting <see cref="HttpClient"/>.
    /// </summary>
    /// <typeparam name="TClient">Generated/raw client type.</typeparam>
    /// <param name="services">Service collection to update.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusGeneratedClient<TClient>(this IServiceCollection services)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<TClient>(CreateClient<TClient>);
        return services;
    }

    /// <summary>
    /// Registers <see cref="IAbacusFieldMapper"/> from a fluent field mapping configuration.
    /// </summary>
    /// <param name="services">Service collection to update.</param>
    /// <param name="configure">Delegate used to configure entity field maps.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusFieldMapping(
        this IServiceCollection services,
        Action<AbacusFieldMappingBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        return AddAbacusFieldMapping(services, configure, configuration: null);
    }

    /// <summary>
    /// Registers <see cref="IAbacusFieldMapper"/> from the <c>Abacus:FieldMaps</c> configuration section.
    /// Configuration entries override any previously registered map for the same logical field.
    /// </summary>
    /// <param name="services">Service collection to update.</param>
    /// <param name="configuration">Root configuration containing <c>Abacus:FieldMaps</c>.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusFieldMapping(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return AddAbacusFieldMapping(services, configure: null, configuration);
    }

    /// <summary>
    /// Registers <see cref="IAbacusFieldMapper"/> from fluent configuration and optional
    /// <c>Abacus:FieldMaps</c> settings. Configuration wins field-by-field over the fluent map.
    /// </summary>
    /// <param name="services">Service collection to update.</param>
    /// <param name="configure">Optional fluent configuration.</param>
    /// <param name="configuration">Optional root configuration containing <c>Abacus:FieldMaps</c>.</param>
    /// <returns>The same <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddAbacusFieldMapping(
        this IServiceCollection services,
        Action<AbacusFieldMappingBuilder>? configure,
        IConfiguration? configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new AbacusFieldMappingBuilder();
        configure?.Invoke(builder);

        var mapping = builder.Build();
        if (configuration is not null)
        {
            mapping.MergeFrom(ReadFieldMapsFromConfiguration(configuration));
        }

        services.AddSingleton(mapping);
        services.AddSingleton<IAbacusFieldMapper, AbacusFieldMapper>();
        return services;
    }

    private static AbacusClientOptions BindClientOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection("Abacus");
        var options = new AbacusClientOptions();

        var baseUri = section["BaseUri"];
        if (!string.IsNullOrWhiteSpace(baseUri))
        {
            options.BaseUri = new Uri(baseUri, UriKind.Absolute);
        }

        var serverUri = section["ServerUri"];
        if (!string.IsNullOrWhiteSpace(serverUri))
        {
            options.ServerUri = new Uri(serverUri, UriKind.Absolute);
        }

        options.Mandant = section["Mandant"] ?? options.Mandant;

        var userAgent = section["UserAgent"];
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            options.UserAgent = userAgent;
        }

        var timeout = section["Timeout"];
        if (!string.IsNullOrWhiteSpace(timeout) && TimeSpan.TryParse(timeout, out var timeoutValue))
        {
            options.Timeout = timeoutValue;
        }

        options.Prefer = section["Prefer"] ?? options.Prefer;

        var maxRetries = section["RateLimitMaxRetries"];
        if (!string.IsNullOrWhiteSpace(maxRetries) && int.TryParse(maxRetries, out var retries))
        {
            options.RateLimitMaxRetries = retries;
        }

        var baseDelay = section["RateLimitBaseDelay"];
        if (!string.IsNullOrWhiteSpace(baseDelay) && TimeSpan.TryParse(baseDelay, out var delayValue))
        {
            options.RateLimitBaseDelay = delayValue;
        }

        var enableLogging = section["EnableRequestLogging"];
        if (!string.IsNullOrWhiteSpace(enableLogging) && bool.TryParse(enableLogging, out var logging))
        {
            options.EnableRequestLogging = logging;
        }

        return options;
    }

    private static AbacusFieldMapping ReadFieldMapsFromConfiguration(IConfiguration configuration)
    {
        var mapping = new AbacusFieldMapping();
        var section = configuration.GetSection("Abacus:FieldMaps");

        foreach (var entitySection in section.GetChildren())
        {
            foreach (var fieldSection in entitySection.GetChildren())
            {
                if (string.IsNullOrWhiteSpace(fieldSection.Value))
                {
                    continue;
                }

                mapping.SetField(entitySection.Key, fieldSection.Key, fieldSection.Value);
            }
        }

        return mapping;
    }

    private static TClient CreateClient<TClient>(IServiceProvider serviceProvider)
        where TClient : class
    {
        var httpClient = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(AbacusHttpClientFactory.HttpClientName);

        return ActivatorUtilities.CreateInstance<TClient>(serviceProvider, httpClient);
    }
}
