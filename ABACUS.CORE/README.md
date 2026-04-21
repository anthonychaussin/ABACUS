# ABACUS.CORE

Ce projet est la base commune de tous les autres modules du SDK.

Il ne porte pas un domaine metier ABACUS en particulier. Son role est de fournir les briques partagees dont les autres projets ont besoin pour parler a l'API de maniere uniforme.

## Ce que ce projet apporte

- la configuration commune du client ABACUS
- la gestion de l'authentification
- la construction du client HTTP
- la gestion standardisee des reponses et des erreurs

En pratique, `ABACUS.CORE` existe pour eviter que chaque module reimplemente les memes mecanismes transverses.
