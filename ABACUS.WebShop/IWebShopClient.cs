using ABACUS.Core;

namespace ABACUS.WebShop;

/// <summary>
/// High-level SDK facade for the WebShop module.
/// </summary>
public interface IWebShopClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_WebShopClient Raw { get; }
}
