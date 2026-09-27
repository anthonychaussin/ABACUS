namespace ABACUS.AssetsLedger;

/// <summary>
/// Minimal asset projection for typed list demos.
/// </summary>
public sealed class AssetSummary
{
    /// <summary>Asset identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>Asset number when present.</summary>
    public string? AssetNumber { get; set; }
}
