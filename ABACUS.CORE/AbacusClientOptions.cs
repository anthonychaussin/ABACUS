using System.Net.Http.Headers;

namespace ABACUS.Core;

/// <summary>
/// Global options used to configure an ABACUS SDK client.
/// </summary>
public sealed class AbacusClientOptions
{
    /// <summary>
    /// Base API URI (for example https://api.abacus.ch).
    /// </summary>
    public required Uri BaseUri { get; init; }
    /// <summary>
    /// Value sent in the HTTP User-Agent header.
    /// </summary>
    public string UserAgent { get; init; } = "ABACUS.SDK/1.0";
    /// <summary>
    /// HTTP request timeout.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(100);
    /// <summary>
    /// Extra headers applied to all requests.
    /// </summary>
    public IDictionary<string, string> DefaultHeaders { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    internal void Validate()
    {
        if (!BaseUri.IsAbsoluteUri)
        {
            throw new ArgumentException("BaseUri must be absolute.", nameof(BaseUri));
        }
    }
}
