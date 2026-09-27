# ABACUS DependencyInjection

Registers the packaged entity-module clients that share the mandant `HttpClient`.

```csharp
services.AddAbacusSdk(configuration); // reads Abacus:*
services.AddAbacusAllModules();
```

AbaReport is not included: it needs a separate `HttpClient` whose `BaseAddress` is the server origin.
