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

Le SDK **ne retente pas** les erreurs serveur (500, 502, 503, 504) par defaut. Un retry aveugle sur une mutation non-idempotente peut creer des doublons. Preferer :

1. journaliser / alerter
2. rejouer manuellement ou via une file avec idempotence metier
3. utiliser `$batch` pour regrouper des lectures

Un circuit breaker / Polly pourra etre ajoute ulterieurement si le besoin est confirme.

## Prefer par operation

Deux niveaux :

| Niveau | Usage |
| --- | --- |
| `AbacusClientOptions.Prefer` | En-tete par defaut sur tout le client |
| Parametre `prefer` sur Create / Patch / Delete | Surcharge pour une mutation |

Constante utile : `AbacusHttp.PreferContinueOnError` (`odata.continue-on-error`) pour continuer malgre des avertissements de validation.

Exemple :

```csharp
await ap.CreateSupplierAsync(payload, prefer: AbacusHttp.PreferContinueOnError);
```

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
