using ABACUS.Core;

namespace ABACUS.ProjectManagement;

/// <summary>
/// High-level SDK facade for the ProjectManagement module.
/// </summary>
public interface IProjectManagementClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_ProjectManagementClient Raw { get; }
}
