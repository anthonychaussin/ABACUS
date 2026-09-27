using ABACUS.AbaReport;
using ABACUS.AccountsPayable;
using ABACUS.AccountsReceivable;
using ABACUS.AssetsLedger;
using ABACUS.Core;
using ABACUS.CRM;
using ABACUS.DossierFileUpload;
using ABACUS.FieldInformation;
using ABACUS.Finance;
using ABACUS.General;
using ABACUS.HumanResources;
using ABACUS.ProductionPlanning;
using ABACUS.ProjectManagement;
using ABACUS.RealEstate;
using ABACUS.Salary;
using ABACUS.Subscription;
using ABACUS.UserDependentAuth;
using ABACUS.WebShop;
using Microsoft.Extensions.DependencyInjection;

namespace ABACUS.DependencyInjection;

/// <summary>
/// Registers the packaged ABACUS module clients for dependency injection.
/// </summary>
public static class AbacusModuleServiceCollectionExtensions
{
    /// <summary>
    /// Registers all entity/mandant module clients that share the ABACUS SDK <see cref="HttpClient"/>.
    /// Call <see cref="AbacusServiceCollectionExtensions.AddAbacusSdk(IServiceCollection, AbacusClientOptions)"/> first.
    /// AbaReport is registered separately via <see cref="AddAbacusAbaReport"/>.
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
        services.AddAbacusModuleClient<IHumanResourcesClient, HumanResourcesClient>();
        services.AddAbacusModuleClient<ISalaryClient, SalaryClient>();
        services.AddAbacusModuleClient<IProjectManagementClient, ProjectManagementClient>();
        services.AddAbacusModuleClient<IProductionPlanningClient, ProductionPlanningClient>();
        services.AddAbacusModuleClient<IDossierFileUploadClient, DossierFileUploadClient>();
        services.AddAbacusModuleClient<IUserDependentAuthClient, UserDependentAuthClient>();

        return services;
    }

    /// <summary>
    /// Registers <see cref="IAbaReportClient"/> with a dedicated HttpClient whose BaseAddress is the server origin
    /// (not the entity/mandant path).
    /// </summary>
    public static IServiceCollection AddAbacusAbaReport(this IServiceCollection services, Uri serverUri)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serverUri);
        if (!serverUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Server URI must be absolute.", nameof(serverUri));
        }

        services.AddHttpClient(
            AbacusHttpClientFactory.AbaReportHttpClientName,
            client =>
            {
                client.BaseAddress = new Uri(serverUri.GetLeftPart(UriPartial.Authority) + "/");
            });

        services.AddTransient<IAbaReportClient>(serviceProvider =>
        {
            var httpClient = serviceProvider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient(AbacusHttpClientFactory.AbaReportHttpClientName);
            return new AbaReportClient(httpClient);
        });

        return services;
    }
}
