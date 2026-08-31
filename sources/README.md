# API Sources

Place the original API source files in this folder before regeneration.

Recommended structure:

- `sources/openapi/<module>.yaml` or `sources/openapi/<module>.yml`
- `sources/openapi/<module>.json`

Do not put generated C# files here.

The application-specific Entity API catalogue is produced by each ABACUS
installation. Export the current definitions from its Swagger UI
(`/swagger-ui/index.html`) or API-HUB before checking or regenerating a client;
the public ABACUS download site does not expose a single exhaustive OpenAPI
bundle for these endpoints.

`ABACUS.AssetsLedger` is currently a legacy exception: its generated client is
checked in, but the source OpenAPI document was not present in the repository's
initial history. Do not regenerate that module until an authoritative export
has been added under `sources/openapi`.
