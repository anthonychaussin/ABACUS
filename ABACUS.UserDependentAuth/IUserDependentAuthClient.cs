using ABACUS.Core;

namespace ABACUS.UserDependentAuth;

/// <summary>
/// High-level SDK facade for the UserDependentAuth module.
/// </summary>
public interface IUserDependentAuthClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_UserDependentAuthClient Raw { get; }
}
