using System.Text;

namespace ABACUS.Core;

/// <summary>
/// Builds OData 4.0 query options supported by the Abacus entity API.
/// </summary>
public sealed class ODataQuery
{
    private readonly List<string> _filters = new();
    private readonly List<string> _orderBy = new();
    private readonly List<string> _select = new();
    private readonly List<string> _expand = new();
    private int? _top;
    private int? _skip;
    private string? _format;
    private string? _rawQuery;

    /// <summary>
    /// Creates an empty query builder.
    /// </summary>
    public static ODataQuery Create() => new();

    /// <summary>
    /// Adds a <c>$filter</c> clause. Abacus supports logical AND but not OR across filters;
    /// multiple calls are combined with <c> and </c>.
    /// </summary>
    public ODataQuery Filter(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        _filters.Add(expression.Trim());
        return this;
    }

    /// <summary>
    /// Sets or appends a <c>$orderby</c> expression.
    /// </summary>
    public ODataQuery OrderBy(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        _orderBy.Add(expression.Trim());
        return this;
    }

    /// <summary>
    /// Sets or appends <c>$select</c> fields.
    /// </summary>
    public ODataQuery Select(params string[] fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        foreach (var field in fields)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(field);
            _select.Add(field.Trim());
        }

        return this;
    }

    /// <summary>
    /// Sets or appends <c>$expand</c> navigations.
    /// </summary>
    public ODataQuery Expand(params string[] navigations)
    {
        ArgumentNullException.ThrowIfNull(navigations);
        foreach (var navigation in navigations)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(navigation);
            _expand.Add(navigation.Trim());
        }

        return this;
    }

    /// <summary>
    /// Sets <c>$top</c>. Abacus returns at most 100 records per response.
    /// </summary>
    public ODataQuery Top(int top)
    {
        if (top < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(top), "Top cannot be negative.");
        }

        _top = top;
        return this;
    }

    /// <summary>
    /// Sets <c>$skip</c> when the endpoint supports it.
    /// </summary>
    public ODataQuery Skip(int skip)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip cannot be negative.");
        }

        _skip = skip;
        return this;
    }

    /// <summary>
    /// Sets <c>$format</c> (for example <c>json</c>).
    /// </summary>
    public ODataQuery Format(string format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        _format = format.Trim();
        return this;
    }

    /// <summary>
    /// Replaces structured options with a raw query string (without leading <c>?</c>).
    /// </summary>
    public ODataQuery Raw(string query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        _rawQuery = query.Trim().TrimStart('?');
        return this;
    }

    /// <summary>
    /// Returns the query string without a leading <c>?</c>, or an empty string when empty.
    /// </summary>
    public string ToQueryString()
    {
        if (_rawQuery is not null)
        {
            return _rawQuery;
        }

        var parts = new List<string>();
        if (_filters.Count > 0)
        {
            parts.Add("$filter=" + Uri.EscapeDataString(string.Join(" and ", _filters)));
        }

        if (_select.Count > 0)
        {
            parts.Add("$select=" + Uri.EscapeDataString(string.Join(",", _select)));
        }

        if (_expand.Count > 0)
        {
            parts.Add("$expand=" + Uri.EscapeDataString(string.Join(",", _expand)));
        }

        if (_orderBy.Count > 0)
        {
            parts.Add("$orderby=" + Uri.EscapeDataString(string.Join(",", _orderBy)));
        }

        if (_top is not null)
        {
            parts.Add("$top=" + _top.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        if (_skip is not null)
        {
            parts.Add("$skip=" + _skip.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        if (_format is not null)
        {
            parts.Add("$format=" + Uri.EscapeDataString(_format));
        }

        return string.Join("&", parts);
    }

    /// <summary>
    /// Appends this query to a relative or absolute path.
    /// </summary>
    public string ApplyTo(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var query = ToQueryString();
        if (string.IsNullOrEmpty(query))
        {
            return path;
        }

        var builder = new StringBuilder(path);
        builder.Append(path.Contains('?', StringComparison.Ordinal) ? '&' : '?');
        builder.Append(query);
        return builder.ToString();
    }

    /// <inheritdoc />
    public override string ToString() => ToQueryString();
}
