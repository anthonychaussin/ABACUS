# ABACUS

Ce depot contient un SDK .NET decoupe par domaines fonctionnels ABACUS.

L'idee est simple : un projet par module metier. Chaque dossier a maintenant son propre `README.md` qui explique ce que couvre le module, sans rentrer dans le detail du workflow du repo.

## Modules

- `ABACUS.CORE` : base commune du SDK.
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
- `ABACUS.Tests` : verification minimale du SDK.
- `ABACUS.UserDependentAuth` : autorisations liees a l'utilisateur.
- `ABACUS.WebShop` : comptes acheteurs et flux webshop.

## Lecture

Si tu veux comprendre un projet, ouvre directement le `README.md` du dossier concerne : il decrit ce que fait le module et les objets metier qu'il manipule.
