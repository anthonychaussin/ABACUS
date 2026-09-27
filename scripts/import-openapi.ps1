#Requires -Version 7.0
<#
.SYNOPSIS
  Imports an Abacus OpenAPI document into sources/openapi for NSwag generation.

.DESCRIPTION
  Prefer the full OpenAPI export from the Abacus API Hub or from your server Swagger UI
  (typically http://localhost:40000/swagger-ui/index.html). Example-only Insomnia collections
  should be replaced with the real catalog when available (~26 MB for the full Hub export).

.PARAMETER Source
  Path or URL of an OpenAPI YAML/JSON document.

.PARAMETER Module
  Module project name without path, for example ABACUS.AccountsPayable.

.EXAMPLE
  ./scripts/import-openapi.ps1 -Source ./downloads/AccountsPayable.json -Module ABACUS.AccountsPayable
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $Source,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^ABACUS\.[A-Za-z0-9]+$')]
    [string] $Module
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$targetDir = Join-Path $repoRoot "sources/openapi"
New-Item -ItemType Directory -Force -Path $targetDir | Out-Null

$extension = [System.IO.Path]::GetExtension($Source)
if ([string]::IsNullOrWhiteSpace($extension)) {
    $extension = ".yaml"
}

$targetPath = Join-Path $targetDir "$Module.yaml"

if ($Source -match '^https?://') {
    Write-Host "Downloading $Source ..."
    Invoke-WebRequest -Uri $Source -OutFile $targetPath
}
else {
    $resolved = Resolve-Path $Source
    $content = Get-Content -Raw -Path $resolved
    if ($extension -ieq ".json") {
        # Keep JSON as YAML-compatible raw JSON (OpenAPI 3 JSON is valid for NSwag).
        $targetPath = Join-Path $targetDir "$Module.yaml"
        Set-Content -Path $targetPath -Value $content -NoNewline
    }
    else {
        Copy-Item -Path $resolved -Destination $targetPath -Force
    }
}

Write-Host "Imported OpenAPI for $Module -> $targetPath"
Write-Host "Next: dotnet build $Module/$Module.csproj"
Write-Host "Do not commit **/Generated/*.g.cs; NSwag regenerates clients at build time."
