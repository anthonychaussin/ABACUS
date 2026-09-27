# ABACUS.CORE

Ce projet est la base commune de tous les autres modules du SDK.

Il ne porte pas un domaine metier ABACUS en particulier. Son role est de fournir les briques partagees dont les autres projets ont besoin pour parler a l'API de maniere uniforme.

## Ce que ce projet apporte

- la configuration commune du client ABACUS
- la gestion de l'authentification (client credentials ou bearer deja obtenu)
- la construction du client HTTP
- la gestion standardisee des reponses et des erreurs
- le mapping configurable des champs (y compris les user fields propres a une installation)

En pratique, `ABACUS.CORE` existe pour eviter que chaque module reimplemente les memes mecanismes transverses.

## Mapping de champs

Les corps OpenAPI ABACUS sont souvent des objets ouverts. Les noms reels (champs standard et user fields) dependent du mandant. `IAbacusFieldMapper` transforme un modele metier en dictionnaire JSON ABACUS, et l'inverse.

```csharp
services.AddAbacusFieldMapping(map => map
    .Entity("Supplier")
    .Field("Name", "Name")
    .Field("VatNumber", "UserFields.UserField1")
    .Field("City", "Address.City"));

var payload = mapper.ToPayload("Supplier", supplier);
await accountsPayable.CreateSupplierAsync(payload);
```

La section de configuration `Abacus:FieldMaps` peut completer ou ecraser le mapping fluent, champ par champ. L'attribut `[AbacusField("...")]` fournit un defaut sur le modele.
