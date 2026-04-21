# ABACUS .NET SDK

SDK .NET modulaire pour ABACUS, avec publication NuGet par module.

## Architecture

- `ABACUS.Core`: options client, auth, HTTP client factory, erreurs unifiees.
- `ABACUS.<Module>`: wrappers par module avec:
  - interface publique documentee
  - implementation wrapper
  - client OpenAPI genere versionne dans `Generated/*.g.cs`

## Installation par module

Installation unitaire selon besoin:

```bash
dotnet add package ABACUS.Core --version 0.0.1
dotnet add package ABACUS.CRM --version 0.0.1
dotnet add package ABACUS.Finance --version 0.0.1
```

Version actuelle de preproduction: `0.0.x`.

## Utilisation rapide (mode modulaire)

```csharp
using ABACUS.AccountsPayable;
using ABACUS.Core;

var options = new AbacusClientOptions
{
    BaseUri = new Uri("https://api.abacus.ch"),
    UserAgent = "MyApp/1.0"
};

var auth = new BearerTokenAuthenticationProvider(_ => ValueTask.FromResult("<token>"));
using var httpClient = AbacusHttpClientFactory.Create(options, auth);

var accountsPayable = new AccountsPayableClient(httpClient);
await accountsPayable.ListSuppliersAsync();

// Acces brut si endpoint pas encore wrappe
await accountsPayable.Raw.GetSuppliers11AllSuppliersAsync();
```

## Pack local

```bash
dotnet pack ABACUS.slnx -c Release
```

## Publication automatique

Workflow: `.github/workflows/publish-nuget.yml`

- Declenchement sur tag `v*.*.*`.
- Publie tous les projets packables de la solution (Core + modules).
- Necessite le secret repo GitHub: `NUGET_API_KEY`.

## Avant publication

Mettre a jour `Directory.Build.props`:

- `RepositoryUrl`
- `PackageProjectUrl`
- `Authors`
- `Company`
