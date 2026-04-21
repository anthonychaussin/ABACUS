using ABACUS.Core;

namespace ABACUS.CRM;

/// <summary>
/// Default implementation for the CRM module wrapper.
/// </summary>
public sealed class CRMClient : ICRMClient
{
    /// <inheritdoc />
    public string ModuleName => "CRM";

    /// <inheritdoc />
    public ABACUS_CRMClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public CRMClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_CRMClient(httpClient);
    }
}
