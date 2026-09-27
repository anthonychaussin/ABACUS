# Observabilite

Le SDK peut emettre des activites OpenTelemetry via `ActivitySource` nomme `ABACUS.SDK`.

## Activation

```csharp
var options = new AbacusClientOptions
{
    ServerUri = new Uri("https://example.abacus:40000"),
    Mandant = "7777",
    EnableOpenTelemetry = true,
};

using var http = AbacusHttpClientFactory.Create(options, auth);
```

Avec DI :

```csharp
services.AddAbacusSdk(options);
services.AddAbacusOpenTelemetry(); // ou options.EnableOpenTelemetry = true
```

## Tags emis

Chaque requete HTTP du pipeline cree une activite `ABACUS.HTTP` avec notamment :

- `http.request.method`
- `url.path` / `server.address`
- `http.response.status_code`
- `abacus.retry` lorsque un retry 429 a eu lieu

Exporter via le SDK OpenTelemetry .NET habituel (`AddSource("ABACUS.SDK")`).

Voir aussi [resilience.md](resilience.md) pour 429, Prefer et journalisation debug.
