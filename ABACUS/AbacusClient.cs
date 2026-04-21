using ABACUS.AccountsPayable;
using ABACUS.AccountsReceivable;
using ABACUS.AssetsLedger;
using ABACUS.CRM;
using ABACUS.Core;
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

namespace ABACUS;

/// <summary>
/// Entry point client exposing all ABACUS SDK module clients.
/// </summary>
public sealed class AbacusClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    /// <summary>Accounts Payable module.</summary>
    public IAccountsPayableClient AccountsPayable { get; }
    /// <summary>Accounts Receivable module.</summary>
    public IAccountsReceivableClient AccountsReceivable { get; }
    /// <summary>Assets Ledger module.</summary>
    public IAssetsLedgerClient AssetsLedger { get; }
    /// <summary>CRM module.</summary>
    public ICRMClient CRM { get; }
    /// <summary>Dossier File Upload module.</summary>
    public IDossierFileUploadClient DossierFileUpload { get; }
    /// <summary>Field Information module.</summary>
    public IFieldInformationClient FieldInformation { get; }
    /// <summary>Finance module.</summary>
    public IFinanceClient Finance { get; }
    /// <summary>General module.</summary>
    public IGeneralClient General { get; }
    /// <summary>Human Resources module.</summary>
    public IHumanResourcesClient HumanResources { get; }
    /// <summary>Production Planning module.</summary>
    public IProductionPlanningClient ProductionPlanning { get; }
    /// <summary>Project Management module.</summary>
    public IProjectManagementClient ProjectManagement { get; }
    /// <summary>Real Estate module.</summary>
    public IRealEstateClient RealEstate { get; }
    /// <summary>Salary module.</summary>
    public ISalaryClient Salary { get; }
    /// <summary>Subscription module.</summary>
    public ISubscriptionClient Subscription { get; }
    /// <summary>User Dependent Auth module.</summary>
    public IUserDependentAuthClient UserDependentAuth { get; }
    /// <summary>WebShop module.</summary>
    public IWebShopClient WebShop { get; }

    /// <summary>
    /// Creates an ABACUS SDK client.
    /// </summary>
    public AbacusClient(AbacusClientOptions options, IAbacusAuthenticationProvider? authenticationProvider = null)
        : this(AbacusHttpClientFactory.Create(options, authenticationProvider), ownsHttpClient: true)
    {
    }

    /// <summary>
    /// Creates an ABACUS SDK client from an externally managed HTTP client.
    /// </summary>
    public AbacusClient(HttpClient httpClient)
        : this(httpClient, ownsHttpClient: false)
    {
    }

    private AbacusClient(HttpClient httpClient, bool ownsHttpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttpClient = ownsHttpClient;

        AccountsPayable = new AccountsPayableClient(_httpClient);
        AccountsReceivable = new AccountsReceivableClient(_httpClient);
        AssetsLedger = new AssetsLedgerClient(_httpClient);
        CRM = new CRMClient(_httpClient);
        DossierFileUpload = new DossierFileUploadClient(_httpClient);
        FieldInformation = new FieldInformationClient(_httpClient);
        Finance = new FinanceClient(_httpClient);
        General = new GeneralClient(_httpClient);
        HumanResources = new HumanResourcesClient(_httpClient);
        ProductionPlanning = new ProductionPlanningClient(_httpClient);
        ProjectManagement = new ProjectManagementClient(_httpClient);
        RealEstate = new RealEstateClient(_httpClient);
        Salary = new SalaryClient(_httpClient);
        Subscription = new SubscriptionClient(_httpClient);
        UserDependentAuth = new UserDependentAuthClient(_httpClient);
        WebShop = new WebShopClient(_httpClient);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        _disposed = true;
    }
}
