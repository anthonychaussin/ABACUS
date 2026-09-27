# ABACUS DependencyInjection

Registers packaged module clients that share the mandant `HttpClient`, plus an optional AbaReport registration.

```csharp
services.AddAbacusSdk(configuration); // reads Abacus:*
services.AddAbacusAllModules();
services.AddAbacusAbaReport(new Uri("https://abacus.example:40000"));
```

`AddAbacusAllModules` includes AP, AR, AssetsLedger, CRM, Finance, General, RealEstate, Subscription, FieldInformation, WebShop, HumanResources, Salary, ProjectManagement, ProductionPlanning, DossierFileUpload, and UserDependentAuth.

AbaReport needs a separate BaseAddress (server origin) via `AddAbacusAbaReport`.
