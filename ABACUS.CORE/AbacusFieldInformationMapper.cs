using System.Text.Json;

namespace ABACUS.Core;

/// <summary>
/// Validates field-mapping paths against FieldInformation payloads from Abacus.
/// </summary>
public static class AbacusFieldInformationMapper
{
    /// <summary>
    /// Extracts known Abacus field paths from a FieldInformation JSON payload.
    /// Supports arrays of objects with <c>Path</c>, <c>Name</c>, or <c>FieldName</c>.
    /// </summary>
    public static IReadOnlySet<string> ExtractFieldPaths(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(json);
        CollectPaths(document.RootElement, paths);
        return paths;
    }

    /// <summary>
    /// Returns mapping paths that are not present in the FieldInformation set.
    /// </summary>
    public static IReadOnlyList<string> FindUnknownPaths(
        AbacusFieldMapping mapping,
        string entity,
        IReadOnlySet<string> knownPaths)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);
        ArgumentNullException.ThrowIfNull(knownPaths);

        var unknown = new List<string>();
        foreach (var (_, abacusPath) in mapping.GetEntityMap(entity))
        {
            if (!knownPaths.Contains(abacusPath) &&
                !knownPaths.Contains(abacusPath.Split('.')[0]))
            {
                unknown.Add(abacusPath);
            }
        }

        return unknown;
    }

    private static void CollectPaths(JsonElement element, ISet<string> paths)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectPaths(item, paths);
                }

                break;
            case JsonValueKind.Object:
                if (element.TryGetProperty("value", out var value))
                {
                    CollectPaths(value, paths);
                }

                foreach (var name in new[] { "Path", "path", "Name", "name", "FieldName", "fieldName" })
                {
                    if (element.TryGetProperty(name, out var property) &&
                        property.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(property.GetString()))
                    {
                        paths.Add(property.GetString()!);
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        CollectPaths(property.Value, paths);
                    }
                }

                break;
        }
    }
}
