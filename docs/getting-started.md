# Demarrage rapide

Ce guide montre la facon la plus directe de configurer le SDK ABACUS aujourd'hui.

## 1. Installer le coeur et un module

```bash
dotnet add package AbacusBusinessSoftware.Core
dotnet add package AbacusBusinessSoftware.AccountsPayable
```

Si tu travailles directement depuis ce depot, ajoute les `ProjectReference` equivalents.

## 2. Creer les options du client

`AbacusClientOptions` porte l'URL de base, le timeout, le `User-Agent` et les en-tetes par defaut.

```csharp
using ABACUS.Core;

var options = new AbacusClientOptions
{
    BaseUri = new Uri("https://example.abacus/api/"),
    UserAgent = "MyCompany.MyApp/1.0.0",
    Timeout = TimeSpan.FromSeconds(30),
    DefaultHeaders = new Dictionary<string, string>
    {
        ["X-Correlation-Id"] = Guid.NewGuid().ToString("N"),
    },
};
```

## 3. Ajouter l'authentification Bearer

`BearerTokenAuthenticationProvider` attend une fonction qui retourne le token a envoyer dans l'en-tete `Authorization: Bearer ...`.

```csharp
using ABACUS.Core;

var auth = new BearerTokenAuthenticationProvider(
    cancellationToken => ValueTask.FromResult("<access-token>"));
```

Si ton token vient d'un service OAuth ou d'un cache interne, remplace simplement la lambda par ton code de recuperation.

## 4. Creer un `HttpClient` avec `AbacusHttpClientFactory`

```csharp
using ABACUS.Core;

using var httpClient = AbacusHttpClientFactory.Create(options, auth);
```

La factory applique :

- `BaseAddress` depuis `BaseUri`
- `Timeout`
- le `User-Agent`
- les en-tetes definis dans `DefaultHeaders`
- l'authentification si un provider est fourni

Si tu geres deja un `HttpClient` ailleurs, tu peux aussi reutiliser `AbacusHttpClientFactory.Configure(httpClient, options)`.

## 5. Instancier un module

Chaque module prend un `HttpClient` configure.

```csharp
using ABACUS.AccountsPayable;
using ABACUS.Core;

var options = new AbacusClientOptions
{
    BaseUri = new Uri("https://example.abacus/api/"),
};

var auth = new BearerTokenAuthenticationProvider(
    cancellationToken => ValueTask.FromResult("<access-token>"));

using var httpClient = AbacusHttpClientFactory.Create(options, auth);

var accountsPayable = new AccountsPayableClient(httpClient);

await accountsPayable.ListSuppliersAsync();
```

Sur certains modules, l'API utile se trouve surtout sur la propriete `Raw`, qui expose directement le client genere.

```csharp
using ABACUS.RealEstate;

var realEstate = new RealEstateClient(httpClient);

// Exemple: appel via le client genere quand aucun wrapper haut niveau n'existe encore.
// await realEstate.Raw.SomeGeneratedMethodAsync(...);
```

## Limites actuelles du SDK

Le SDK est utilisable, mais il faut integrer avec prudence :

- Tous les modules n'ont pas encore une facade metier riche. Plusieurs exposent surtout `Raw`.
- Certains wrappers haut niveau utilisent encore des `object` en entree ou ne retournent pas de modeles metier forts.
- Les clients generes ont encore des noms de methodes tres bruts, issus de la generation OpenAPI.
- Une partie de la generation actuelle semble contenir des endpoints ou parametres imparfaits. Valide toujours les appels critiques contre ton environnement ABACUS.
- La documentation de demarrage existe maintenant, mais la publication NuGet et la chaine de regeneration ne sont pas encore entierement finalisees dans ce depot.

## Exemple complet

```csharp
using ABACUS.AccountsPayable;
using ABACUS.Core;

var options = new AbacusClientOptions
{
    BaseUri = new Uri("https://example.abacus/api/"),
    UserAgent = "MyCompany.AbacusIntegration/1.0.0",
    Timeout = TimeSpan.FromSeconds(30),
};

var auth = new BearerTokenAuthenticationProvider(
    async cancellationToken =>
    {
        await Task.Yield();
        return "<access-token>";
    });

using var httpClient = AbacusHttpClientFactory.Create(options, auth);
var module = new AccountsPayableClient(httpClient);

await module.ListSuppliersAsync();
```

Pour les details d'un domaine metier, consulte ensuite le `README.md` du module concerne.
