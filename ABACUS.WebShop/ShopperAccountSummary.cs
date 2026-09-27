namespace ABACUS.WebShop;

/// <summary>
/// Minimal shopper account projection for typed list demos.
/// </summary>
public sealed class ShopperAccountSummary
{
    /// <summary>Account identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Display name or email.</summary>
    public string? Name { get; set; }

    /// <summary>Email when present.</summary>
    public string? Email { get; set; }
}
