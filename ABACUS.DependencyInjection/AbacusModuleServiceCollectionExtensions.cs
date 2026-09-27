using ABACUS.AccountsPayable;
using ABACUS.AccountsReceivable;
using ABACUS.AssetsLedger;
using ABACUS.Core;
using ABACUS.CRM;
using ABACUS.FieldInformation;
using ABACUS.Finance;
using ABACUS.General;
using ABACUS.RealEstate;
using ABACUS.Subscription;
using ABACUS.WebShop;
using Microsoft.Extensions.DependencyInjection;

namespace ABACUS.DependencyInjection;

/// <summary>
/// Registers the packaged ABACUS entity-module clients that share the entity/mandant <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
/// AbaReport uses a different API root (server origin) and is intentionally not registered here.
/// </remarks>
public static class AbacusModuleServiceCollectionExtensions
{
    /// <summary>
    /// Registers AP, AR, AssetsLedger, CRM, Finance, General, RealEstate, Subscription,
    /// FieldInformation and WebShop behind their interfaces.
    /// Call <see cref="AbacusServiceCollectionExtensions.AddAbacusSdk(IServiceCollection, AbacusClientOptions)"/> first.
    /// </summary>
    public static IServiceCollection AddAbacusAllModules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAbacusModuleClient<IAccountsPayableClient, AccountsPayableClient>();
        services.AddAbacusModuleClient<IAccountsReceivableClient, AccountsReceivableClient>();
        services.AddAbacusModuleClient<IAssetsLedgerClient, AssetsLedgerClient>();
        services.AddAbacusModuleClient<ICRMClient, CRMClient>();
        services.AddAbacusModuleClient<IFinanceClient, FinanceClient>();
        services.AddAbacusModuleClient<IGeneralClient, GeneralClient>();
        services.AddAbacusModuleClient<IRealEstateClient, RealEstateClient>();
        services.AddAbacusModuleClient<ISubscriptionClient, SubscriptionClient>();
        services.AddAbacusModuleClient<IFieldInformationClient, FieldInformationClient>();
        services.AddAbacusModuleClient<IWebShopClient, WebShopClient>();

        return services;
    }
}
