using ABACUS.Core;

namespace ABACUS.General;

/// <summary>
/// Default implementation for the General module wrapper.
/// </summary>
public sealed class GeneralClient : IGeneralClient
{
    /// <inheritdoc />
    public string ModuleName => "General";

    /// <inheritdoc />
    public ABACUS_GeneralClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public GeneralClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_GeneralClient(httpClient);
    }
}
