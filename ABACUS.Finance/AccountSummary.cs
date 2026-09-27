namespace ABACUS.Finance;

/// <summary>
/// Minimal account projection for typed list demos.
/// </summary>
public sealed class AccountSummary
{
    /// <summary>Account identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Enterprise identifier when present.</summary>
    public string? EnterpriseId { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }
}
