namespace ABACUS.Core;

/// <summary>
/// Helpers for building ABACUS entity API base URIs that include the mandant.
/// </summary>
public static class AbacusEntityUri
{
    /// <summary>
    /// Default entity API path prefix used by Abacus installations.
    /// </summary>
    public const string DefaultEntityPath = "/api/entity/v1/mandants";

    /// <summary>
    /// Builds <c>{server}/api/entity/v1/mandants/{mandant}/</c> from a server origin and mandant id.
    /// </summary>
    /// <param name="serverUri">ABACUS server origin, for example <c>https://host:40000</c>.</param>
    /// <param name="mandant">Mandant identifier (client number).</param>
    /// <param name="entityPath">Optional override of the entity path prefix.</param>
    public static Uri BuildBaseUri(Uri serverUri, string mandant, string entityPath = DefaultEntityPath)
    {
        ArgumentNullException.ThrowIfNull(serverUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(mandant);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityPath);

        if (!serverUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Server URI must be absolute.", nameof(serverUri));
        }

        var authority = serverUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        var path = entityPath.Trim('/');
        return new Uri($"{authority}/{path}/{Uri.EscapeDataString(mandant.Trim())}/");
    }
}
