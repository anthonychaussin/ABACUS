# ABACUS.Testing

Helpers for unit-testing consumers of the ABACUS SDK without a live server.

```csharp
using ABACUS.Testing;

using var handler = new CapturingHttpMessageHandler((_, _) => ODataResponseFactory.Page("""[{"Id":"1"}]"""));
using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.invalid") };
```
