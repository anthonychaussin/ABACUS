using ABACUS.Core;

namespace ABACUS.ProductionPlanning;

/// <summary>
/// High-level SDK facade for the ProductionPlanning module.
/// </summary>
public interface IProductionPlanningClient : IAbacusModuleClient
{
    /// <summary>
    /// Underlying generated client for advanced or not-yet-wrapped endpoints.
    /// </summary>
    ABACUS_ProductionPlanningClient Raw { get; }
}
