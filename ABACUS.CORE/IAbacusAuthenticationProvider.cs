namespace ABACUS.Core;

/// <summary>
/// Applies authentication information to outgoing requests.
/// </summary>
public interface IAbacusAuthenticationProvider
{
    /// <summary>
    /// Adds authentication headers/tokens to the request.
    /// </summary>
    ValueTask ApplyAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
