using ABACUS.Core;

namespace ABACUS.WebShop;

/// <summary>
/// Default implementation for the WebShop module wrapper.
/// </summary>
public sealed class WebShopClient : IWebShopClient
{
    /// <inheritdoc />
    public string ModuleName => "WebShop";

    /// <inheritdoc />
    public ABACUS_WebShopClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public WebShopClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_WebShopClient(httpClient);
    }
}
