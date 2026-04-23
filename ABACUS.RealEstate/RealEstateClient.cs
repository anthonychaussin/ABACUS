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

    /// <inheritdoc />
    public async Task ListObjectContractsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.Get_ObjectContractsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <inheritdoc />
    public async Task ListPartialObjectContractsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.Get_PartialObjectContractsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }

    /// <inheritdoc />
    public async Task ListCodeTablesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Raw.Get_CodetablesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw AbacusExceptionMapper.Map(ex);
        }
    }
}
