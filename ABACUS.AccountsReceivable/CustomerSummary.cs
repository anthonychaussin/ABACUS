namespace ABACUS.AccountsReceivable;

/// <summary>
/// Minimal customer projection for typed list demos. Expand with Hub/$metadata-generated DTOs later.
/// </summary>
public sealed class CustomerSummary
{
    /// <summary>Customer identifier.</summary>
    public int Id { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>Customer number when present.</summary>
    public long? CustomerNumber { get; set; }
}
