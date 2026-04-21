using ABACUS.Core;

namespace ABACUS.RealEstate;

/// <summary>
/// Default implementation for the RealEstate module wrapper.
/// </summary>
public sealed class RealEstateClient : IRealEstateClient
{
    /// <inheritdoc />
    public string ModuleName => "RealEstate";

    /// <inheritdoc />
    public ABACUS_RealEstateClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public RealEstateClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_RealEstateClient(httpClient);
    }
}
