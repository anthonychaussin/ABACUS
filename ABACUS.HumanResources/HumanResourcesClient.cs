using ABACUS.Core;

namespace ABACUS.HumanResources;

/// <summary>
/// Default implementation for the HumanResources module wrapper.
/// </summary>
public sealed class HumanResourcesClient : IHumanResourcesClient
{
    /// <inheritdoc />
    public string ModuleName => "HumanResources";

    /// <inheritdoc />
    public ABACUS_HumanResourcesClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public HumanResourcesClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_HumanResourcesClient(httpClient);
    }
}
