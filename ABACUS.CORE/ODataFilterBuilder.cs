using System.Globalization;
using System.Text;

namespace ABACUS.Core;

/// <summary>
/// Fluent builder for OData <c>$filter</c> expressions with safe string escaping.
/// Abacus typically supports AND across clauses; avoid complex OR unless confirmed on your server.
/// </summary>
public sealed class ODataFilterBuilder
{
    private readonly List<string> _parts = new();

    /// <summary>
    /// Starts a new filter builder.
    /// </summary>
    public static ODataFilterBuilder Create() => new();

    /// <summary>
    /// Adds <c>field eq 'value'</c> (or unquoted numeric/bool literals).
    /// </summary>
    public ODataFilterBuilder Equal(string field, object? value)
    {
        _parts.Add($"{NormalizeField(field)} eq {FormatLiteral(value)}");
        return this;
    }

    /// <summary>
    /// Adds <c>field ne ...</c>.
    /// </summary>
    public ODataFilterBuilder NotEqual(string field, object? value)
    {
        _parts.Add($"{NormalizeField(field)} ne {FormatLiteral(value)}");
        return this;
    }

    /// <summary>
    /// Adds <c>field gt ...</c>.
    /// </summary>
    public ODataFilterBuilder GreaterThan(string field, object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _parts.Add($"{NormalizeField(field)} gt {FormatLiteral(value)}");
        return this;
    }

    /// <summary>
    /// Adds <c>field gt ...</c>. Alias of <see cref="GreaterThan"/>.
    /// </summary>
    public ODataFilterBuilder Gt(string field, object value) => GreaterThan(field, value);

    /// <summary>
    /// Adds <c>field lt ...</c>.
    /// </summary>
    public ODataFilterBuilder LessThan(string field, object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _parts.Add($"{NormalizeField(field)} lt {FormatLiteral(value)}");
        return this;
    }

    /// <summary>
    /// Adds <c>contains(field,'value')</c>.
    /// </summary>
    public ODataFilterBuilder Contains(string field, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _parts.Add($"contains({NormalizeField(field)},{FormatLiteral(value)})");
        return this;
    }

    /// <summary>
    /// Combines with a previously built clause using AND (same builder instance already ANDs all parts).
    /// </summary>
    public ODataFilterBuilder And(Action<ODataFilterBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var nested = new ODataFilterBuilder();
        configure(nested);
        var expression = nested.ToFilterExpression();
        if (!string.IsNullOrWhiteSpace(expression))
        {
            _parts.Add($"({expression})");
        }

        return this;
    }

    /// <summary>
    /// Returns the filter expression without the <c>$filter=</c> prefix.
    /// </summary>
    public string ToFilterExpression() => string.Join(" and ", _parts);

    /// <inheritdoc />
    public override string ToString() => ToFilterExpression();

    private static string NormalizeField(string field)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        return field.Trim();
    }

    private static string FormatLiteral(object? value) =>
        value switch
        {
            null => "null",
            bool b => b ? "true" : "false",
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal =>
                Convert.ToString(value, CultureInfo.InvariantCulture)!,
            DateTime dt => $"'{dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}'",
            DateTimeOffset dto => $"'{dto.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}Z'",
            Guid g => $"{g}",
            _ => $"'{EscapeString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)}'",
        };

    private static string EscapeString(string value)
    {
        // OData single-quote escape: ' -> ''
        return value.Replace("'", "''", StringComparison.Ordinal);
    }
}
