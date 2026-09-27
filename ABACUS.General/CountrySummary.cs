namespace ABACUS.General;

/// <summary>
/// Minimal country projection for typed list demos.
/// </summary>
public sealed class CountrySummary
{
    /// <summary>Country code / identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>ISO code when present.</summary>
    public string? IsoCode { get; set; }
}
