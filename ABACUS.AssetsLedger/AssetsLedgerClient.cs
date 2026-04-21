using ABACUS.Core;

namespace ABACUS.AssetsLedger;

/// <summary>
/// Default implementation for the AssetsLedger module wrapper.
/// </summary>
public sealed class AssetsLedgerClient : IAssetsLedgerClient
{
    /// <inheritdoc />
    public string ModuleName => "AssetsLedger";

    /// <inheritdoc />
    public ABACUS_AssetsLedgerClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public AssetsLedgerClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_AssetsLedgerClient(httpClient);
    }
}
