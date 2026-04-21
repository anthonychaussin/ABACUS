# ABACUS .NET SDK

SDK .NET modulaire pour ABACUS, avec une base propre pour publication NuGet.

## Architecture

- `ABACUS.Core`: options client, auth, HTTP client factory, erreurs unifiees.
- `ABACUS.<Module>`: wrappers par module avec:
  - interface publique documentee
  - implementation wrapper
  - client OpenAPI genere versionne dans `Generated/*.g.cs`
- `ABACUS` (`ABACUS.SDK`): point d'entree unique `AbacusClient` + DI extension.

## Utilisation rapide

```csharp
using ABACUS;
using ABACUS.Core;

var options = new AbacusClientOptions
{
    BaseUri = new Uri("https://api.abacus.ch"),
    UserAgent = "MyApp/1.0"
};

var auth = new BearerTokenAuthenticationProvider(_ => ValueTask.FromResult("<token>"));

using var client = new AbacusClient(options, auth);
await client.AccountsPayable.ListSuppliersAsync();

// Acces brut si endpoint pas encore wrappe
await client.Finance.Raw.SomeOperationAsync();
```

## Dependency Injection

```csharp
services.AddAbacusSdk(
    new AbacusClientOptions { BaseUri = new Uri("https://api.abacus.ch") },
    sp => new BearerTokenAuthenticationProvider(_ => ValueTask.FromResult("<token>")));
```

## Pack local

```bash
dotnet pack ABACUS.CORE/ABACUS.Core.csproj -c Release
dotnet pack ABACUS.AccountsPayable/ABACUS.AccountsPayable.csproj -c Release
dotnet pack ABACUS/ABACUS.csproj -c Release
```

## Publication automatique

Workflow: `.github/workflows/publish-nuget.yml`

- Declenchement sur tag `v*.*.*`.
- Necessite le secret repo GitHub: `NUGET_API_KEY`.

## Avant publication

Mettre a jour `Directory.Build.props`:

- `RepositoryUrl`
- `PackageProjectUrl`
- `Authors`
- `Company`
