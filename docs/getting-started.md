# Demarrage rapide

Ce guide montre la facon la plus directe de configurer le SDK ABACUS aujourd'hui.

## 1. Installer le coeur et un module

```bash
dotnet add package AbacusBusinessSoftware.Core
dotnet add package AbacusBusinessSoftware.AccountsPayable
```

Si tu travailles directement depuis ce depot, ajoute les `ProjectReference` equivalents.

## 2. Creer les options du client

Prefere `ServerUri` + `Mandant` pour construire
`{server}/api/entity/v1/mandants/{mandant}/`. Tu peux aussi fixer `BaseUri` directement.

```csharp
using ABACUS.Core;

var options = new AbacusClientOptions
{
    ServerUri = new Uri("https://example.abacus:40000"),
    Mandant = "7777",
    UserAgent = "MyCompany.MyApp/1.0.0",
    Timeout = TimeSpan.FromSeconds(30),
    RateLimitMaxRetries = 3,
};
```

## 3. Ajouter l'authentification

### Service-User (client credentials)

```csharp
using var auth = new ClientCredentialsAuthenticationProvider(
    new AbacusClientCredentialsOptions
    {
        ServerUri = new Uri("https://example.abacus:40000"),
        ClientId = "<client-id>",
        ClientSecret = "<client-secret>",
        Scopes = { "abacus.pad" }, // max 25 scopes
    });
```

Avec DI :

```csharp
services.AddAbacusSdk(configuration); // lit Abacus:ServerUri, Mandant, Prefer, retries, ...
services.AddAbacusClientCredentials("<client-id>", "<client-secret>", "abacus.pad");
services.AddAbacusAllModules(); // package AbacusBusinessSoftware.DependencyInjection
services.AddAbacusAbaReport(new Uri("https://example.abacus:40000")); // BaseAddress = origine serveur
```

Ou module par module :

```csharp
services.AddAbacusSdk(options);
services.AddAbacusModuleClient<IAccountsPayableClient, AccountsPayableClient>();
```

`UserDependentAuth` (documents / storages sous auth utilisateur) necessite `AddAbacusAuthorizationCode` plutot que client-credentials seul.

### Utilisateur interactif (authorization code)

```csharp
using var auth = new AuthorizationCodeAuthenticationProvider(
    new AbacusAuthorizationCodeOptions
    {
        ServerUri = new Uri("https://example.abacus:40000"),
        ClientId = "<client-id>",
        ClientSecret = "<client-secret>",
        RedirectUri = new Uri("https://myapp/callback"),
        Scopes = { "openid", "offline_access" },
    });

var url = await auth.CreateAuthorizationUrlAsync(state: "xyz");
// apres redirect :
await auth.ExchangeCodeAsync(authorizationCode);
```

## 4. Creer un `HttpClient`

```csharp
using var httpClient = AbacusHttpClientFactory.Create(options, auth);
```

La factory applique BaseAddress (mandant), timeout, User-Agent, Prefer, retries HTTP 429 et l'auth.

## 5. Appeler un module avec OData

```csharp
using ABACUS.AccountsPayable;
using ABACUS.Core;

var accountsPayable = new AccountsPayableClient(httpClient);

var page = await accountsPayable.ListSuppliersAsync(
    ODataQuery.Create().Top(50).Filter("Name eq 'Acme'"));

await foreach (var supplier in accountsPayable.EnumerateSuppliersAsync())
{
    // suit @odata.nextLink (max 100 enregistrements par page cote Abacus)
}
```

### Subscriptions (changements)

```csharp
using ABACUS.Subscription;

var subscriptions = new SubscriptionClient(httpClient);
await subscriptions.SubscribeAsync("acme-sync", ["Subject"]);
await foreach (var batch in subscriptions.Changes.ConsumeUntilEmptyAsync("acme-sync"))
{
    // traiter batch.Changes puis ack automatique si AcknowledgeKey present
}
```

Nom de subscription : max 23 caracteres, URL-safe. Max 10 subscriptions par utilisateur ; une subscription non consommee > 3 jours est purgee par Abacus.

### AbaReport

```csharp
using ABACUS.AbaReport;

// BaseAddress = origine serveur (pas le chemin entity/mandant)
var reports = new AbaReportClient(httpClient);
var bytes = await reports.ExportReportAsync("MyReport", format: "txt");
```

## 6. Mapper les champs (user fields inclus)

```csharp
services.AddAbacusFieldMapping(map => map
    .Entity("Supplier")
    .Field("Name", "Name")
    .Field("VatNumber", "UserFields.UserField1"));

var payload = mapper.ToPayload("Supplier", supplier);
await accountsPayable.CreateSupplierAsync(payload);
```

## Quotas Abacus (ordre de grandeur)

Jusqu'a la version 2025 : ~200 req/min, 12k/h, 30k/j. A partir de 2026 : ~400 / 18k / 40k.
Depassement = HTTP 429 (gere par `AbacusRateLimitHandler`). Une reponse liste au plus 100 lignes.

Details : [resilience.md](resilience.md) (429 vs 5xx, Prefer, `$batch`, journalisation).

## Limites actuelles du SDK

- Plusieurs modules exposent surtout `Raw` ; AP, Finance, CRM, Subscription ont des facades plus riches.
- Les OpenAPI dans `sources/openapi` restent partiels : reimporte le catalogue API Hub via `scripts/import-openapi.ps1` quand tu as un dump complet.
- Valide les appels critiques contre ton environnement ABACUS.
