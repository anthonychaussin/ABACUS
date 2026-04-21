namespace ABACUS.Core;

/// <summary>
/// Common contract for all ABACUS module clients.
/// </summary>
public interface IAbacusModuleClient
{
    /// <summary>
    /// Module identifier.
    /// </summary>
    string ModuleName { get; }
}
