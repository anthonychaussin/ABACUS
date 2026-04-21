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
    /// Applies ABACUS SDK HTTP configuration to an existing client instance.
    /// </summary>
    public static void Configure(HttpClient client, AbacusClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        client.BaseAddress = options.BaseUri;
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
    }

    /// <summary>
    /// Creates an HTTP client configured with base URI, headers, timeout and optional authentication.
    /// </summary>
    public static HttpClient Create(AbacusClientOptions options, IAbacusAuthenticationProvider? authenticationProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        HttpMessageHandler handler = new HttpClientHandler();
        if (authenticationProvider is not null)
        {
            handler = new AbacusAuthenticationHandler(authenticationProvider) { InnerHandler = handler };
        }

        var client = new HttpClient(handler);
        Configure(client, options);

        return client;
    }
}
