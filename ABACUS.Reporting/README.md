# ABACUS.Reporting

Ce module couvre l'API REST AbaReport v1 (`/api/abareport/v1`). Il permet de :

- démarrer l'export d'un rapport ;
- suivre le job asynchrone jusqu'à `FinishedSuccess` ou `FinishedError` ;
- télécharger chaque page sous forme de texte ou de données binaires ;
- fermer la session du rapport.

Les formats récents sont inclus, notamment PDF (V2025), Excel et Word (V2026).
La liste de référence est publiée dans la
[documentation officielle AbaReport](https://downloads.abacus.ch/fileadmin/ablage/abaconnect/htmlfiles/docs/restapi/abacus_abareport_rest_api.html).

```csharp
var reporting = new ReportingClient(httpClient);
var job = await reporting.StartReportAsync(
    7777,
    "sales/report.avx",
    new ReportRequest(ReportOutputTypes.Excel, 200));

var status = await reporting.GetJobAsync(job.Id);
var firstPage = await reporting.GetOutputAsync(job.Id);
await reporting.CloseJobAsync(job.Id);
```
