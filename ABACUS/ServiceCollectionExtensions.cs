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
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace ABACUS;

/// <summary>
/// Dependency injection registrations for ABACUS SDK.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers ABACUS SDK clients in the service collection.
    /// </summary>
    public static IServiceCollection AddAbacusSdk(
        this IServiceCollection services,
        AbacusClientOptions options,
        Func<IServiceProvider, IAbacusAuthenticationProvider?>? authenticationFactory = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);

        services
            .AddHttpClient(AbacusHttpClientFactory.HttpClientName, (_, client) =>
            {
                AbacusHttpClientFactory.Configure(client, options);
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler(() => new OverallTimeoutHandler(options.Timeout))
            .AddHttpMessageHandler(sp =>
            {
                var authenticationProvider = authenticationFactory?.Invoke(sp);
                return authenticationProvider is null
                    ? new NoopHttpMessageHandler()
                    : new AbacusAuthenticationHandler(authenticationProvider);
            })
            .AddPolicyHandler(CreateRetryPolicy());

        services.AddSingleton<AbacusClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(AbacusHttpClientFactory.HttpClientName);
            return new AbacusClient(httpClient);
        });

        services.AddSingleton<IAccountsPayableClient>(sp => sp.GetRequiredService<AbacusClient>().AccountsPayable);
        services.AddSingleton<IAccountsReceivableClient>(sp => sp.GetRequiredService<AbacusClient>().AccountsReceivable);
        services.AddSingleton<IAssetsLedgerClient>(sp => sp.GetRequiredService<AbacusClient>().AssetsLedger);
        services.AddSingleton<ICRMClient>(sp => sp.GetRequiredService<AbacusClient>().CRM);
        services.AddSingleton<IDossierFileUploadClient>(sp => sp.GetRequiredService<AbacusClient>().DossierFileUpload);
        services.AddSingleton<IFieldInformationClient>(sp => sp.GetRequiredService<AbacusClient>().FieldInformation);
        services.AddSingleton<IFinanceClient>(sp => sp.GetRequiredService<AbacusClient>().Finance);
        services.AddSingleton<IGeneralClient>(sp => sp.GetRequiredService<AbacusClient>().General);
        services.AddSingleton<IHumanResourcesClient>(sp => sp.GetRequiredService<AbacusClient>().HumanResources);
        services.AddSingleton<IProductionPlanningClient>(sp => sp.GetRequiredService<AbacusClient>().ProductionPlanning);
        services.AddSingleton<IProjectManagementClient>(sp => sp.GetRequiredService<AbacusClient>().ProjectManagement);
        services.AddSingleton<IRealEstateClient>(sp => sp.GetRequiredService<AbacusClient>().RealEstate);
        services.AddSingleton<ISalaryClient>(sp => sp.GetRequiredService<AbacusClient>().Salary);
        services.AddSingleton<ISubscriptionClient>(sp => sp.GetRequiredService<AbacusClient>().Subscription);
        services.AddSingleton<IUserDependentAuthClient>(sp => sp.GetRequiredService<AbacusClient>().UserDependentAuth);
        services.AddSingleton<IWebShopClient>(sp => sp.GetRequiredService<AbacusClient>().WebShop);

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(250 * Math.Pow(2, attempt - 1)));
    }

    private sealed class OverallTimeoutHandler : DelegatingHandler
    {
        private readonly TimeSpan _timeout;

        public OverallTimeoutHandler(TimeSpan timeout)
        {
            _timeout = timeout > TimeSpan.Zero ? timeout : Timeout.InfiniteTimeSpan;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_timeout == Timeout.InfiniteTimeSpan)
            {
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(_timeout);

            try
            {
                return await base.SendAsync(request, linkedCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && linkedCts.IsCancellationRequested)
            {
                throw new TimeoutRejectedException($"The ABACUS HTTP request exceeded the configured timeout of {_timeout}.", ex);
            }
        }
    }

    private sealed class NoopHttpMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => base.SendAsync(request, cancellationToken);
    }
}
