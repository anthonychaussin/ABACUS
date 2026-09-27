namespace ABACUS.CRM;

/// <summary>
/// Minimal subject projection for typed list demos.
/// </summary>
public sealed class SubjectSummary
{
    /// <summary>Subject identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>Subject number when present.</summary>
    public long? SubjectNumber { get; set; }
}
