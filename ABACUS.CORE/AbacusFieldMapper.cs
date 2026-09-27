using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace ABACUS.Core;

/// <summary>
/// Default <see cref="IAbacusFieldMapper"/> implementation.
/// </summary>
public sealed class AbacusFieldMapper : IAbacusFieldMapper
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, string>> AttributeCache = new();

    private readonly AbacusFieldMapping _mapping;

    /// <summary>
    /// Creates a mapper backed by the supplied field mapping registry.
    /// </summary>
    public AbacusFieldMapper(AbacusFieldMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        _mapping = mapping;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> ToPayload<T>(string entity, T source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);
        ArgumentNullException.ThrowIfNull(source);

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        WritePayload(entity, typeof(T), source, result);
        return result;
    }

    /// <inheritdoc />
    public T FromPayload<T>(string entity, IReadOnlyDictionary<string, object?> payload)
        where T : new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);
        ArgumentNullException.ThrowIfNull(payload);

        var target = new T();
        ApplyPayload(entity, typeof(T), target, payload);
        return target;
    }

    private void WritePayload(string entity, Type sourceType, object source, Dictionary<string, object?> target)
    {
        var fieldMap = BuildEffectiveMap(entity, sourceType);

        foreach (var (logicalName, abacusPath) in fieldMap)
        {
            if (!TryGetValueByPath(source, logicalName, out var value) || value is null)
            {
                continue;
            }

            if (IsComplexObject(value.GetType()) && HasNestedMap(value.GetType()))
            {
                var nestedPayload = new Dictionary<string, object?>(StringComparer.Ordinal);
                WritePayload(value.GetType().Name, value.GetType(), value, nestedPayload);
                if (nestedPayload.Count > 0)
                {
                    SetValueByPath(target, abacusPath, nestedPayload);
                }

                continue;
            }

            SetValueByPath(target, abacusPath, value);
        }

        // Nested complex properties without an explicit field entry still map when a type map exists.
        foreach (var property in GetProperties(sourceType))
        {
            if (fieldMap.ContainsKey(property.Name))
            {
                continue;
            }

            if (!IsComplexObject(property.PropertyType) || !HasNestedMap(property.PropertyType))
            {
                continue;
            }

            var nestedSource = property.GetValue(source);
            if (nestedSource is null)
            {
                continue;
            }

            var nestedPayload = new Dictionary<string, object?>(StringComparer.Ordinal);
            WritePayload(property.PropertyType.Name, property.PropertyType, nestedSource, nestedPayload);
            if (nestedPayload.Count == 0)
            {
                continue;
            }

            var destination = ResolveAttributePath(property) ?? property.Name;
            SetValueByPath(target, destination, nestedPayload);
        }
    }

    private void ApplyPayload(string entity, Type targetType, object target, IReadOnlyDictionary<string, object?> payload)
    {
        var fieldMap = BuildEffectiveMap(entity, targetType);

        foreach (var (logicalName, abacusPath) in fieldMap)
        {
            if (!TryGetDictionaryValueByPath(payload, abacusPath, out var value) || value is null)
            {
                continue;
            }

            var property = FindProperty(targetType, logicalName);
            if (property is null)
            {
                // Dotted logical paths: set nested property chain.
                SetObjectValueByPath(target, logicalName, value);
                continue;
            }

            if (IsComplexObject(property.PropertyType) && value is IDictionary nestedDictionary)
            {
                var nestedTarget = property.GetValue(target) ?? Activator.CreateInstance(property.PropertyType);
                if (nestedTarget is null)
                {
                    continue;
                }

                property.SetValue(target, nestedTarget);
                ApplyPayload(
                    property.PropertyType.Name,
                    property.PropertyType,
                    nestedTarget,
                    ToObjectDictionary(nestedDictionary));
                continue;
            }

            property.SetValue(target, ConvertValue(value, property.PropertyType));
        }

        foreach (var property in GetProperties(targetType))
        {
            if (fieldMap.ContainsKey(property.Name))
            {
                continue;
            }

            if (!IsComplexObject(property.PropertyType) || !HasNestedMap(property.PropertyType))
            {
                continue;
            }

            var destination = ResolveAttributePath(property) ?? property.Name;
            if (!TryGetDictionaryValueByPath(payload, destination, out var nestedValue) || nestedValue is null)
            {
                continue;
            }

            if (nestedValue is not IDictionary nestedDictionary)
            {
                continue;
            }

            var nestedTarget = property.GetValue(target) ?? Activator.CreateInstance(property.PropertyType);
            if (nestedTarget is null)
            {
                continue;
            }

            property.SetValue(target, nestedTarget);
            ApplyPayload(
                property.PropertyType.Name,
                property.PropertyType,
                nestedTarget,
                ToObjectDictionary(nestedDictionary));
        }
    }

    private IReadOnlyDictionary<string, string> BuildEffectiveMap(string entity, Type type)
    {
        var effective = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (logicalName, abacusPath) in GetAttributeMap(type))
        {
            effective[logicalName] = abacusPath;
        }

        foreach (var (logicalName, abacusPath) in _mapping.GetEntityMap(entity))
        {
            effective[logicalName] = abacusPath;
        }

        return effective;
    }

    private bool HasNestedMap(Type type)
    {
        if (GetAttributeMap(type).Count > 0)
        {
            return true;
        }

        return _mapping.GetEntityMap(type.Name).Count > 0;
    }

    private static IReadOnlyDictionary<string, string> GetAttributeMap(Type type)
    {
        return AttributeCache.GetOrAdd(type, static t =>
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in GetProperties(t))
            {
                var attribute = property.GetCustomAttribute<AbacusFieldAttribute>(inherit: true);
                if (attribute is not null)
                {
                    map[property.Name] = attribute.Path;
                }
            }

            return map;
        });
    }

    private static string? ResolveAttributePath(PropertyInfo property)
    {
        return property.GetCustomAttribute<AbacusFieldAttribute>(inherit: true)?.Path;
    }

    private static PropertyInfo[] GetProperties(Type type)
    {
        return PropertyCache.GetOrAdd(
            type,
            static t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(static p => p.CanRead && p.GetIndexParameters().Length == 0)
                .ToArray());
    }

    private static PropertyInfo? FindProperty(Type type, string name)
    {
        if (name.Contains('.', StringComparison.Ordinal))
        {
            return null;
        }

        return GetProperties(type).FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.Ordinal));
    }

    private static bool IsComplexObject(Type type)
    {
        if (type == typeof(string) || type.IsEnum)
        {
            return false;
        }

        if (type.IsPrimitive || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset)
            || type == typeof(Guid) || type == typeof(TimeSpan) || type == typeof(DateOnly) || type == typeof(TimeOnly))
        {
            return false;
        }

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
        {
            return IsComplexObject(underlying);
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            // Treat dictionaries produced by nesting as leaf containers when already mapped.
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return false;
            }

            return false;
        }

        return type.IsClass || (type.IsValueType && !type.IsPrimitive);
    }

    private static bool TryGetValueByPath(object source, string path, out object? value)
    {
        value = source;
        foreach (var segment in SplitPath(path))
        {
            if (value is null)
            {
                return false;
            }

            var property = GetProperties(value.GetType())
                .FirstOrDefault(p => string.Equals(p.Name, segment, StringComparison.Ordinal));
            if (property is null)
            {
                value = null;
                return false;
            }

            value = property.GetValue(value);
        }

        return true;
    }

    private static void SetObjectValueByPath(object target, string path, object? value)
    {
        var segments = SplitPath(path);
        object current = target;

        for (var i = 0; i < segments.Length - 1; i++)
        {
            var property = GetProperties(current.GetType())
                .FirstOrDefault(p => string.Equals(p.Name, segments[i], StringComparison.Ordinal));
            if (property is null || !IsComplexObject(property.PropertyType))
            {
                return;
            }

            var nested = property.GetValue(current);
            if (nested is null)
            {
                nested = Activator.CreateInstance(property.PropertyType);
                if (nested is null)
                {
                    return;
                }

                property.SetValue(current, nested);
            }

            current = nested;
        }

        var leaf = GetProperties(current.GetType())
            .FirstOrDefault(p => string.Equals(p.Name, segments[^1], StringComparison.Ordinal));
        if (leaf is null || !leaf.CanWrite)
        {
            return;
        }

        leaf.SetValue(current, ConvertValue(value, leaf.PropertyType));
    }

    private static void SetValueByPath(Dictionary<string, object?> target, string path, object? value)
    {
        var segments = SplitPath(path);
        var current = target;

        for (var i = 0; i < segments.Length - 1; i++)
        {
            if (!current.TryGetValue(segments[i], out var nested) || nested is not Dictionary<string, object?> nestedDictionary)
            {
                nestedDictionary = new Dictionary<string, object?>(StringComparer.Ordinal);
                current[segments[i]] = nestedDictionary;
            }

            current = nestedDictionary;
        }

        current[segments[^1]] = value;
    }

    private static bool TryGetDictionaryValueByPath(
        IReadOnlyDictionary<string, object?> source,
        string path,
        out object? value)
    {
        value = null;
        IReadOnlyDictionary<string, object?> current = source;
        var segments = SplitPath(path);

        for (var i = 0; i < segments.Length; i++)
        {
            if (!TryGetIgnoreCase(current, segments[i], out var next) || next is null)
            {
                value = null;
                return false;
            }

            if (i == segments.Length - 1)
            {
                value = next;
                return true;
            }

            if (next is IDictionary dictionary)
            {
                current = ToObjectDictionary(dictionary);
                continue;
            }

            value = null;
            return false;
        }

        return false;
    }

    private static bool TryGetIgnoreCase(IReadOnlyDictionary<string, object?> source, string key, out object? value)
    {
        if (source.TryGetValue(key, out value))
        {
            return true;
        }

        foreach (var pair in source)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    private static IReadOnlyDictionary<string, object?> ToObjectDictionary(IDictionary dictionary)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry.Key is string key)
            {
                result[key] = entry.Value;
            }
        }

        return result;
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null)
        {
            return null;
        }

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (underlying.IsInstanceOfType(value))
        {
            return value;
        }

        if (underlying.IsEnum)
        {
            if (value is string enumText)
            {
                return Enum.Parse(underlying, enumText, ignoreCase: true);
            }

            return Enum.ToObject(underlying, value);
        }

        return Convert.ChangeType(value, underlying);
    }

    private static string[] SplitPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
