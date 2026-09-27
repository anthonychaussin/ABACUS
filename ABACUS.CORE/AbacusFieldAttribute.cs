namespace ABACUS.Core;

/// <summary>
/// Declares a default ABACUS JSON path for a logical property.
/// Installation-specific configuration overrides this default for the same logical name.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class AbacusFieldAttribute : Attribute
{
    /// <summary>
    /// Creates an attribute that maps the decorated property to an ABACUS JSON path.
    /// </summary>
    /// <param name="path">Destination JSON path (dot-separated for nesting, for example <c>UserFields.UserField1</c>).</param>
    public AbacusFieldAttribute(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Path = path;
    }

    /// <summary>
    /// ABACUS JSON path used when no configuration override is present.
    /// </summary>
    public string Path { get; }
}
