using Microsoft.Extensions.DependencyInjection;

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
                var authenticationProvider = serviceProvider.GetService<IAbacusAuthenticationProvider>();
                if (authenticationProvider is not null)
                {
                    handlers.Add(new AbacusAuthenticationHandler(authenticationProvider));
                }
            });

        return services;
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

    private static TClient CreateClient<TClient>(IServiceProvider serviceProvider)
        where TClient : class
    {
        var httpClient = serviceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(AbacusHttpClientFactory.HttpClientName);

        return ActivatorUtilities.CreateInstance<TClient>(serviceProvider, httpClient);
    }
}
