# Resilience et quotas ABACUS

Ce document resume le comportement HTTP attendu cote SDK et les leviers disponibles.

## HTTP 429 (rate limit)

Abacus applique des quotas par client / endpoint. En cas de `429`, le SDK retente automatiquement via `AbacusRateLimitHandler` :

- active par defaut (`RateLimitMaxRetries = 3`)
- backoff a partir de `RateLimitBaseDelay` (500 ms)
- respecte `Retry-After` quand present
- desactiver avec `RateLimitMaxRetries = 0`

Les facades riches (AP, AR, Finance, CRM, AssetsLedger, RealEstate) passent par ce pipeline tant que le `HttpClient` vient de `AbacusHttpClientFactory` ou de `AddAbacusSdk`.

## HTTP 5xx

Par defaut le SDK **ne retente pas** les 5xx. Un retry aveugle sur une mutation peut creer des doublons.

Pour les **lectures** (GET/HEAD) uniquement, activer :

```csharp
options.EnableReadRetry = true;
options.ReadRetryMaxAttempts = 3; // inclut le premier essai
```

Les POST/PATCH/DELETE ne sont jamais retentes par `AbacusReadRetryHandler`.

## Erreurs OData

Les corps d'erreur OData (`error.code`, `error.message`, `error.details`) sont parses dans `AbacusApiException`.
Un HTTP 400 avec `details` devient `AbacusValidationException` :

```csharp
catch (AbacusValidationException ex)
{
    foreach (var d in ex.Details)
    {
        // d.Target, d.Message
    }
}
```

## Prefer par operation

Deux niveaux :

| Niveau | Usage |
| --- | --- |
| `AbacusClientOptions.Prefer` | En-tete par defaut sur tout le client |
| Parametre `prefer` sur Create / Patch / Delete | Surcharge pour une mutation |

Constante utile : `AbacusHttp.PreferContinueOnError` (`odata.continue-on-error`) pour continuer malgre des avertissements de validation.
Autres constantes : `AbacusHttp.PreferReturnRepresentation`, `AbacusHttp.PreferReturnMinimal`.

Exemple :

```csharp
await ap.CreateSupplierAsync(payload, prefer: AbacusHttp.PreferContinueOnError);
```

## Observabilite

Traces OpenTelemetry : [observability.md](observability.md) (`EnableOpenTelemetry` / `AddAbacusOpenTelemetry`).

## Annulation

Toutes les methodes async acceptent un `CancellationToken`. Annuler une requete en cours interrompt le transport ; une mutation deja acceptee cote serveur n'est pas rollback automatiquement.

## Idempotence

- **GET / listes** : sans effet de bord, rejouables.
- **PATCH** : en general rejouable si le payload est le meme etat cible.
- **POST Create** : non idempotent ; garder une cle metier (numero fournisseur, etc.) et gerer les conflits `409`.
- **DELETE** : souvent idempotent (404 apres la premiere suppression).
- **`$batch`** : chaque partie a son propre statut ; inspecter `ODataBatchResponse.Parts`.

## Journalisation optionnelle

```csharp
options.EnableRequestLogging = true;
using var http = AbacusHttpClientFactory.Create(options, auth, loggerFactory);
```

Avec DI, enregistrer un `ILoggerFactory` et fixer `EnableRequestLogging = true` : `AbacusLoggingHandler` logue methode, chemin et statut **sans** corps ni en-tetes d'autorisation.

## `$batch`

Pour regrouper plusieurs operations OData en un seul aller-retour :

```csharp
var batch = new ODataBatchRequest()
    .Get("Suppliers?$top=10")
    .Get("Customers?$top=10");

var response = await general.PostBatchAsync(batch);
```

Voir `ODataBatchRequest` / `AbacusHttp.SendBatchAsync` / `GeneralClient.PostBatchAsync`.
