namespace ABACUS.RealEstate;

/// <summary>
/// Minimal object contract projection for typed list demos.
/// </summary>
public sealed class ObjectContractSummary
{
    /// <summary>Contract identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>Contract number when present.</summary>
    public string? ContractNumber { get; set; }
}
