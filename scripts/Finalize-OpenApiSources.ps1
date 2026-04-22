[CmdletBinding()]
param(
    [string]$InputFolder = "sources/openapi"
)

$ErrorActionPreference = "Stop"

function To-CamelCase([string]$value) {
    $parts = @()
    foreach ($m in [regex]::Matches($value, "[A-Za-z0-9]+")) {
        $parts += [string]$m.Value
    }
    if ($parts.Count -eq 0) { return "id" }
    $first = $parts[0].Substring(0,1).ToLowerInvariant() + $parts[0].Substring(1)
    if ($parts.Count -eq 1) { return $first }
    $rest = $parts[1..($parts.Count - 1)] | ForEach-Object {
        $_.Substring(0,1).ToUpperInvariant() + $_.Substring(1)
    }
    return $first + ($rest -join "")
}

$files = Get-ChildItem -Path (Resolve-Path $InputFolder) -Filter *.yaml
if (-not $files) {
    Write-Host "No yaml files found."
    exit 0
}

foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $updated = New-Object System.Collections.Generic.List[string]
    $currentMethod = ""
    $currentPathParams = @()
    $lastWasPathIn = $false
    $i = 0

    while ($i -lt $lines.Count) {
        $line = $lines[$i]

        if ($line -match "^  /.+:\s*$") {
            $currentMethod = ""
            $line = [regex]::Replace(
                $line,
                "([A-Za-z][A-Za-z0-9]*)=\{'?\{([a-z])\}'?\}",
                {
                    param($m)
                    $key = $m.Groups[1].Value
                    $param = To-CamelCase $key
                    if ($m.Value -match "='\{[a-z]\}'") {
                        return "$key='{${param}}'"
                    }
                    return "$key={${param}}"
                })
            $currentPathParams = @()
            foreach ($m in [regex]::Matches($line, "\{([A-Za-z][A-Za-z0-9]*)\}")) {
                $candidate = [string]$m.Groups[1].Value
                if (-not $currentPathParams.Contains($candidate)) {
                    $currentPathParams += $candidate
                }
            }
            $updated.Add($line)
            $i++
            continue
        }

        if ($line -match "^    (get|post|put|patch|delete|options|head|trace):\s*$") {
            $currentMethod = $Matches[1]
            $updated.Add($line)
            $i++
            continue
        }

        if ($line -match "^ {8}- in: path\s*$") {
            $lastWasPathIn = $true
            $updated.Add($line)
            $i++
            continue
        }

        if ($lastWasPathIn -and $line -match "^ {10}name:\s*(?<name>[A-Za-z][A-Za-z0-9]*)\s*$") {
            $existing = $Matches["name"]
            $replacement = $existing
            if ($currentPathParams.Count -eq 1) {
                $replacement = $currentPathParams[0]
            } elseif ($existing.Length -eq 1 -and $currentPathParams.Count -gt 1) {
                $matches = $currentPathParams | Where-Object { $_.Substring(0,1).Equals($existing, [System.StringComparison]::OrdinalIgnoreCase) }
                if ($matches.Count -eq 1) {
                    $replacement = $matches[0]
                }
            }
            $updated.Add("          name: $replacement")
            $lastWasPathIn = $false
            $i++
            continue
        }

        $lastWasPathIn = $false

        if (($currentMethod -eq "get" -or $currentMethod -eq "delete") -and $line -match "^      requestBody:\s*$") {
            while ($i + 1 -lt $lines.Count -and ($lines[$i + 1] -match "^ {8,}\S" -or $lines[$i + 1].Trim().Length -eq 0)) {
                $i++
            }
            $i++
            continue
        }

        if ($line -match "^  /.+:\s*$") {
            $currentMethod = ""
        }

        $updated.Add($line)
        $i++
    }

    Set-Content -Path $file.FullName -Value $updated -Encoding UTF8
    Write-Host "Finalized $($file.Name)"
}
