# ABACUS.CORE

Ce projet est la base commune de tous les autres modules du SDK.

Il ne porte pas un domaine metier ABACUS en particulier. Son role est de fournir les briques partagees dont les autres projets ont besoin pour parler a l'API de maniere uniforme.

## Ce que ce projet apporte

- la configuration commune du client ABACUS (mandant, Prefer, retries 429)
- la gestion de l'authentification (client credentials avec scopes, authorization code, ou bearer deja obtenu)
- la construction du client HTTP
- les primitives OData (`ODataQuery`, pagination `@odata.nextLink`, `AbacusHttp`)
- le change feed (`IAbacusChangeFeed`) pour Subscribe/Consume/Acknowledge
- la gestion standardisee des reponses et des erreurs
- le mapping configurable des champs (y compris les user fields propres a une installation)

En pratique, `ABACUS.CORE` existe pour eviter que chaque module reimplemente les memes mecanismes transverses.

## Helpers facade (`AbacusODataEntity`)

Pour eviter de dupliquer List/Get/Create/Patch dans chaque module :

```csharp
await AbacusODataEntity.CreateAsync(http, "/Accounts", "Account", model, mapper);
await AbacusODataEntity.PatchAsync(http, path, "Account", model, mapper);
var page = await AbacusODataEntity.ListAsync<AccountSummary>(http, "/Accounts", query);
```

## Mapping de champs

Les corps OpenAPI ABACUS sont souvent des objets ouverts. Les noms reels (champs standard et user fields) dependent du mandant. `IAbacusFieldMapper` transforme un modele metier en dictionnaire JSON ABACUS, et l'inverse.

```csharp
services.AddAbacusFieldMapping(map => map
    .Entity("Supplier")
    .Field("Name", "Name")
    .Field("VatNumber", "UserFields.UserField1")
    .Field("City", "Address.City"));

// Surcharge facade (AP / CRM / AR / Finance / …) :
await accountsPayable.CreateSupplierAsync(supplier, mapper);
await accountsPayable.PatchSupplierAsync(id, supplier, mapper);
```

La section de configuration `Abacus:FieldMaps` peut completer ou ecraser le mapping fluent, champ par champ. L'attribut `[AbacusField("...")]` fournit un defaut sur le modele.

## Filtres OData

Preferer `ODataFilterBuilder` / `ODataQuery.Where` pour echapper correctement les quotes :

```csharp
var query = ODataQuery.Create()
    .Where(f => f.Equal("Status", "Open").Contains("Name", "Acme"))
    .Top(50);
```

Limites Abacus : les filtres complexes avec OR / fonctions avancees ne sont pas toujours supportes. Garder des clauses AND simples et valider sur ton serveur. Pas de traduction LINQ complete dans ce SDK.

## DTOs typés

Les listes restent `JsonElement` par defaut. Pour un modele minimal :

```csharp
var page = await ap.ListSuppliersAsAsync<SupplierSummary>(query);
var customers = await ar.ListCustomersAsAsync<CustomerSummary>();
```

`SupplierSummary` / `CustomerSummary` (et equivalents) sont des projections manuelles.
Des DTOs preuve generes (`AccountGenerated`, `AssetGenerated`, `SubjectGenerated`) sont produits par
`scripts/generate-dtos.ps1` dans `Models/*.g.cs` (pas le dossier `Generated/` reserve au NSwag).
La generation depuis Hub `$metadata` (CSDL) reste une evolution documentee, hors auto dans cette vague.

## Source generator `[AbacusField]`

Reference le package / projet `ABACUS.Generators` en Analyzer dans ton app :

```xml
<ProjectReference Include="...\ABACUS.Generators.csproj"
                  OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

Pour chaque type avec `[AbacusField]`, le generateur emet `Apply{TypeName}` et
`AddAbacusFieldMappingFromAttributes_{TypeName}` dans `ABACUS.Core.Generated`.
