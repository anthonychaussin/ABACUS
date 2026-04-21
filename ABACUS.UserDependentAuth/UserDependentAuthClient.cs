using ABACUS.Core;

namespace ABACUS.UserDependentAuth;

/// <summary>
/// Default implementation for the UserDependentAuth module wrapper.
/// </summary>
public sealed class UserDependentAuthClient : IUserDependentAuthClient
{
    /// <inheritdoc />
    public string ModuleName => "UserDependentAuth";

    /// <inheritdoc />
    public ABACUS_UserDependentAuthClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public UserDependentAuthClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_UserDependentAuthClient(httpClient);
    }
}
