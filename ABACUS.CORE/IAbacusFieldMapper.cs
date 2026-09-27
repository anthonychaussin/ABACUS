namespace ABACUS.Core;

/// <summary>
/// Maps between business models and ABACUS JSON payloads using a field mapping configuration.
/// </summary>
public interface IAbacusFieldMapper
{
    /// <summary>
    /// Projects <paramref name="source"/> into an ABACUS-shaped dictionary for <paramref name="entity"/>.
    /// Unmapped properties and null values are omitted.
    /// </summary>
    IReadOnlyDictionary<string, object?> ToPayload<T>(string entity, T source);

    /// <summary>
    /// Projects an ABACUS-shaped dictionary into a new instance of <typeparamref name="T"/>.
    /// </summary>
    T FromPayload<T>(string entity, IReadOnlyDictionary<string, object?> payload)
        where T : new();
}
