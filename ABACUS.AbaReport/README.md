# ABACUS.AbaReport

Client for the Abacus **AbaReport** REST API (report export), separate from the entity OData modules.

Requires a Service-User with AbaReport interface access in Q981 and the relevant licence option (for example DataScience).

Configure `HttpClient.BaseAddress` to the Abacus server origin (not the entity mandant path), then:

```csharp
var reports = new AbaReportClient(httpClient);
var list = await reports.ListReportsAsync();
var bytes = await reports.ExportReportAsync("MyReport", format: "txt");
```
