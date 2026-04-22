# ABACUS

SDK .NET modulaire pour l'API ABACUS.

Le depot est organise par domaines metier. Chaque module embarque son client propre et depend de `ABACUS.CORE` pour la configuration HTTP, l'authentification et la gestion des erreurs.

## Installation

Depuis un feed NuGet :

```bash
dotnet add package AbacusBusinessSoftware.Core
dotnet add package AbacusBusinessSoftware.AccountsPayable
```

Depuis le code source du depot, reference `ABACUS.CORE` puis le module voulu.

## Demarrage rapide

Le guide de demarrage couvre :

- la creation de `AbacusClientOptions`
- l'usage de `AbacusHttpClientFactory`
- l'authentification via `BearerTokenAuthenticationProvider`
- l'instanciation d'un module
- les limites actuelles du SDK

Voir [docs/getting-started.md](docs/getting-started.md).

## Modules

- `ABACUS.CORE` : configuration commune, auth, HTTP et erreurs.
- `ABACUS.AccountsPayable` : comptabilite fournisseurs.
- `ABACUS.AccountsReceivable` : comptabilite clients.
- `ABACUS.AssetsLedger` : gestion des immobilisations.
- `ABACUS.CRM` : tiers, adresses, contacts et relations.
- `ABACUS.DossierFileUpload` : depot de fichiers dans les dossiers ABACUS.
- `ABACUS.FieldInformation` : informations de champs et metadonnees metier.
- `ABACUS.Finance` : comptes, centres de charges et ecritures.
- `ABACUS.General` : referentiels communs.
- `ABACUS.HumanResources` : RH, organisation, formation et recrutement.
- `ABACUS.ProductionPlanning` : ressources et suivi de production.
- `ABACUS.ProjectManagement` : projets, planification, tarifs et imputations.
- `ABACUS.RealEstate` : gestion immobiliere.
- `ABACUS.Salary` : paie et donnees salariales.
- `ABACUS.Subscription` : abonnements aux changements et consommation d'evenements.
- `ABACUS.UserDependentAuth` : autorisations liees a l'utilisateur.
- `ABACUS.WebShop` : comptes acheteurs et flux webshop.

## Etat actuel

Le depot compile et fournit une base exploitable, mais la couche SDK reste partielle sur plusieurs modules. Lis le guide de demarrage avant d'integrer un module en production.
