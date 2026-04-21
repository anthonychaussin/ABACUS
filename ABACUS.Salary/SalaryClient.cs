using ABACUS.Core;

namespace ABACUS.Salary;

/// <summary>
/// Default implementation for the Salary module wrapper.
/// </summary>
public sealed class SalaryClient : ISalaryClient
{
    /// <inheritdoc />
    public string ModuleName => "Salary";

    /// <inheritdoc />
    public ABACUS_SalaryClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public SalaryClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_SalaryClient(httpClient);
    }
}
