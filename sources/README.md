# API Sources

Place OpenAPI documents for each module in this folder before (re)building.

## Layout

- `sources/openapi/ABACUS.<Module>.yaml`

The MSBuild pipeline (`Directory.Build.targets`) generates NSwag clients from these files at build time.
Committed `*/Generated/*.g.cs` files are ignored and excluded from compilation when a YAML exists.

## Importing from Abacus

1. Export OpenAPI from the Abacus API Hub, or from your server Swagger UI (`http://localhost:40000/swagger-ui/index.html`).
2. Run:

```powershell
./scripts/import-openapi.ps1 -Source path/or/url -Module ABACUS.AccountsPayable
```

3. Rebuild the module project so NSwag regenerates the client.

## Curation checklist (required before treating a module as production-ready)

Use [ABACUS.AccountsPayable.yaml](openapi/ABACUS.AccountsPayable.yaml) / [ABACUS.AssetsLedger.yaml](openapi/ABACUS.AssetsLedger.yaml) as templates.

1. **Merge duplicate path keys** — YAML cannot have the same path twice (e.g. two `/Customers(Id={id}):` blocks); fold GET/PATCH/DELETE into one path object.
2. **Parameterize example ids** — replace hardcoded keys like `/Assets(112001)` with `/Assets({id})` (`Sanitize-OpenApiSources.ps1` / `Finalize-OpenApiSources.ps1`).
3. **Declare OData query params** on collection GET operations: `$filter`, `$select`, `$top`, `$orderby`, `$expand`.
4. **Declare list response schema** with `value` + `@odata.nextLink` (`ODataCollection`).
5. **Open request bodies** — prefer `additionalProperties: true` over empty `properties: {}`.
6. **Keep operationIds stable** and descriptive (`get_Accounts`, not example-driven names).

Modules already curated for OData collections: AccountsPayable, AssetsLedger, AccountsReceivable, Finance, CRM, RealEstate (path merge + collection enrichment).

## Notes

- Prefer the full Hub catalog over Insomnia example collections.
- Do not put generated C# under this folder.
