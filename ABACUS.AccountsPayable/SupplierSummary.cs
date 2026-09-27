namespace ABACUS.AccountsPayable;

/// <summary>
/// Minimal supplier projection for typed list demos. Expand with Hub/$metadata-generated DTOs later.
/// </summary>
public sealed class SupplierSummary
{
    /// <summary>Supplier identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>Supplier number when present.</summary>
    public long? SupplierNumber { get; set; }
}
