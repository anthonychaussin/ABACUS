using ABACUS.Core;

namespace ABACUS.Salary;

/// <summary>
/// High-level SDK facade for the Salary module.
/// </summary>
public interface ISalaryClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_SalaryClient Raw { get; }
}
