using ABACUS.Core;

namespace ABACUS.Finance;

/// <summary>
/// Default implementation for the Finance module wrapper.
/// </summary>
public sealed class FinanceClient : IFinanceClient
{
    /// <inheritdoc />
    public string ModuleName => "Finance";

    /// <inheritdoc />
    public ABACUS_FinanceClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public FinanceClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_FinanceClient(httpClient);
    }
}
