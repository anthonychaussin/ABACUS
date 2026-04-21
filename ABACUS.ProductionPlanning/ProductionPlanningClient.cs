using ABACUS.Core;

namespace ABACUS.ProductionPlanning;

/// <summary>
/// Default implementation for the ProductionPlanning module wrapper.
/// </summary>
public sealed class ProductionPlanningClient : IProductionPlanningClient
{
    /// <inheritdoc />
    public string ModuleName => "ProductionPlanning";

    /// <inheritdoc />
    public ABACUS_ProductionPlanningClient Raw { get; }

    /// <summary>
    /// Creates the module client from a configured <see cref="HttpClient"/>.
    /// </summary>
    public ProductionPlanningClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        Raw = new ABACUS_ProductionPlanningClient(httpClient);
    }
}
