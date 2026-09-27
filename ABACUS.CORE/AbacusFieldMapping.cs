namespace ABACUS.Core;

/// <summary>
/// Registry of logical field names to ABACUS JSON paths, keyed by entity name.
/// </summary>
public sealed class AbacusFieldMapping
{
    private readonly Dictionary<string, Dictionary<string, string>> _entities =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Field maps registered for each entity (logical name to ABACUS path).
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Entities =>
        _entities.ToDictionary(
            static pair => pair.Key,
            static pair => (IReadOnlyDictionary<string, string>)pair.Value,
            StringComparer.Ordinal);

    /// <summary>
    /// Returns the field map for <paramref name="entity"/>, or an empty map when none is registered.
    /// </summary>
    public IReadOnlyDictionary<string, string> GetEntityMap(string entity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);

        return _entities.TryGetValue(entity, out var map)
            ? map
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Sets or replaces the ABACUS path for a logical field on an entity.
    /// </summary>
    public void SetField(string entity, string logicalName, string abacusPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(abacusPath);

        if (!_entities.TryGetValue(entity, out var map))
        {
            map = new Dictionary<string, string>(StringComparer.Ordinal);
            _entities[entity] = map;
        }

        map[logicalName] = abacusPath;
    }

    /// <summary>
    /// Merges another mapping into this one. Fields from <paramref name="other"/> win on conflict.
    /// </summary>
    public void MergeFrom(AbacusFieldMapping other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (var (entity, fields) in other._entities)
        {
            foreach (var (logicalName, abacusPath) in fields)
            {
                SetField(entity, logicalName, abacusPath);
            }
        }
    }
}
