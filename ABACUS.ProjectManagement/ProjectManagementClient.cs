using ABACUS.Core;

namespace ABACUS.ProjectManagement;

/// <summary>
/// Default implementation for the ProjectManagement module wrapper.
/// </summary>
public sealed class ProjectManagementClient : IProjectManagementClient
{
    /// <inheritdoc />
    public string ModuleName => "ProjectManagement";

    /// <inheritdoc />
    public ABACUS_ProjectManagementClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public ProjectManagementClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_ProjectManagementClient(httpClient);
    }
}
