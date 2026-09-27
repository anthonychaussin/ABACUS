namespace ABACUS.Core;

/// <summary>
/// Fluent builder for <see cref="AbacusFieldMapping"/>.
/// </summary>
public sealed class AbacusFieldMappingBuilder
{
    private readonly AbacusFieldMapping _mapping = new();
    private string? _currentEntity;

    /// <summary>
    /// Selects the entity that subsequent <see cref="Field"/> calls configure.
    /// </summary>
    public AbacusFieldMappingBuilder Entity(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _currentEntity = name;
        return this;
    }

    /// <summary>
    /// Maps a logical source path to an ABACUS JSON destination path for the current entity.
    /// </summary>
    /// <param name="logicalName">Source property path (dot-separated for nesting).</param>
    /// <param name="abacusPath">Destination JSON path (dot-separated for nesting).</param>
    public AbacusFieldMappingBuilder Field(string logicalName, string abacusPath)
    {
        if (_currentEntity is null)
        {
            throw new InvalidOperationException("Call Entity(...) before Field(...).");
        }

        _mapping.SetField(_currentEntity, logicalName, abacusPath);
        return this;
    }

    /// <summary>
    /// Builds the immutable-style mapping registry.
    /// </summary>
    public AbacusFieldMapping Build() => _mapping;
}
