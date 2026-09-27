using System.Text.Json;

namespace ABACUS.Core;

/// <summary>
/// One page of an OData collection response.
/// </summary>
/// <typeparam name="T">Item type deserialized from the <c>value</c> array.</typeparam>
public sealed class ODataPage<T>
{
    /// <summary>
    /// Creates a page from items and an optional next-link.
    /// </summary>
    public ODataPage(IReadOnlyList<T> value, string? nextLink, int statusCode, IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        NextLink = nextLink;
        StatusCode = statusCode;
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    /// <summary>
    /// Items in this page (<c>value</c>).
    /// </summary>
    public IReadOnlyList<T> Value { get; }

    /// <summary>
    /// Absolute or relative <c>@odata.nextLink</c> when more pages exist.
    /// </summary>
    public string? NextLink { get; }

    /// <summary>
    /// HTTP status code of the response that produced this page.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Response headers.
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>
    /// Whether another page is available.
    /// </summary>
    public bool HasNextPage => !string.IsNullOrWhiteSpace(NextLink);
}

/// <summary>
/// Parses OData collection JSON payloads.
/// </summary>
public static class ODataPageParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Parses a collection page from a JSON body.
    /// </summary>
    public static ODataPage<T> Parse<T>(
        string? json,
        int statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        headers ??= new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ODataPage<T>(Array.Empty<T>(), nextLink: null, statusCode, headers);
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        IReadOnlyList<T> values;
        if (root.ValueKind == JsonValueKind.Array)
        {
            values = DeserializeArray<T>(root);
            return new ODataPage<T>(values, nextLink: null, statusCode, headers);
        }

        if (root.TryGetProperty("value", out var valueElement) && valueElement.ValueKind == JsonValueKind.Array)
        {
            values = DeserializeArray<T>(valueElement);
        }
        else
        {
            values = Array.Empty<T>();
        }

        string? nextLink = null;
        if (root.TryGetProperty("@odata.nextLink", out var nextLinkElement) &&
            nextLinkElement.ValueKind == JsonValueKind.String)
        {
            nextLink = nextLinkElement.GetString();
        }

        return new ODataPage<T>(values, nextLink, statusCode, headers);
    }

    private static IReadOnlyList<T> DeserializeArray<T>(JsonElement array)
    {
        var list = new List<T>(array.GetArrayLength());
        foreach (var element in array.EnumerateArray())
        {
            var item = element.Deserialize<T>(SerializerOptions);
            if (item is not null)
            {
                list.Add(item);
            }
        }

        return list;
    }
}
