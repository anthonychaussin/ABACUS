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
    $firstPart = [string]$parts[0]
    if ($firstPart.Length -eq 1) {
        $first = $firstPart.ToLowerInvariant()
    } else {
        $first = $firstPart.Substring(0,1).ToLowerInvariant() + $firstPart.Substring(1)
    }
    if ($parts.Count -eq 1) { return $first }
    $rest = $parts[1..($parts.Count - 1)] | ForEach-Object {
        $token = [string]$_
        if ($token.Length -eq 1) {
            $token.ToUpperInvariant()
        } else {
            $token.Substring(0,1).ToUpperInvariant() + $token.Substring(1)
        }
    }
    return $first + ($rest -join "")
}

function Sanitize-Segment([string]$segment) {
    if ($segment -match "^(?<base>[^()]+)\((?<inner>.*)\)$") {
        $base = $Matches["base"]
        $inner = $Matches["inner"]

        if ($inner -match "=") {
            $pairs = $inner -split ","
            $outPairs = @()
            $params = New-Object System.Collections.Generic.List[string]
            foreach ($pair in $pairs) {
                $kv = $pair -split "=", 2
                if ($kv.Count -ne 2) { continue }
                $rawKey = $kv[0].Trim()
                $rawValue = $kv[1].Trim()
                $paramName = To-CamelCase $rawKey
                $params.Add($paramName)
                if ($rawValue -match "^'.*'$") {
                    $outPairs += "$rawKey='{${paramName}}'"
                } else {
                    $outPairs += "$rawKey={${paramName}}"
                }
            }
            if ($outPairs.Count -gt 0) {
                return @{
                    Segment = "$base(" + ($outPairs -join ",") + ")"
                    Params = $params
                }
            }
        } else {
            return @{
                Segment = "$base({id})"
                Params = @("id")
            }
        }
    }

    if ($segment -match "^[0-9]+$" -or
        $segment -match "^[0-9a-fA-F]{8}-[0-9a-fA-F-]{27}$" -or
        $segment -in @("TEST", "Test")) {
        return @{
            Segment = "{id}"
            Params = @("id")
        }
    }

    return @{
        Segment = $segment
        Params = @()
    }
}

function Sanitize-Path([string]$path) {
    $parts = $path.TrimStart("/") -split "/"
    $out = New-Object System.Collections.Generic.List[string]
    $params = New-Object System.Collections.Generic.List[string]
    foreach ($part in $parts) {
        if ($part -eq "") { continue }
        $san = Sanitize-Segment $part
        $out.Add($san.Segment)
        foreach ($p in $san.Params) {
            if (-not $params.Contains($p)) { $params.Add($p) }
        }
    }
    return @{
        Path = "/" + ($out -join "/")
        Params = $params
    }
}

function Build-OperationId([string]$method, [string]$path, [System.Collections.Generic.HashSet[string]]$seen) {
    $raw = ($method.ToLowerInvariant() + "_" + $path.TrimStart("/")) -replace "[^A-Za-z0-9]+", "_"
    $raw = ($raw -replace "_{2,}", "_").Trim("_")
    if ($raw.Length -eq 0) { $raw = $method.ToLowerInvariant() + "_operation" }
    if ($raw.Length -gt 100) { $raw = $raw.Substring(0, 100).Trim("_") }
    $candidate = $raw
    $suffix = 2
    while ($seen.Contains($candidate)) {
        $candidate = "${raw}_$suffix"
        $suffix++
    }
    $seen.Add($candidate) | Out-Null
    return $candidate
}

function Process-OperationBlock(
    [string]$method,
    [string[]]$blockLines,
    [string[]]$pathParams,
    [string]$newOperationId,
    [ref]$removedRequestBodies
) {
    $lines = New-Object System.Collections.Generic.List[string]
    for ($i = 0; $i -lt $blockLines.Count; $i++) {
        $line = $blockLines[$i]

        if ($line -match "^ {6}operationId:") {
            $lines.Add("      operationId: $newOperationId")
            if ($line -match ">\-|>\+|\|") {
                while ($i + 1 -lt $blockLines.Count -and $blockLines[$i + 1] -match "^ {8}\S") {
                    $i++
                }
            }
            continue
        }

        if (($method -eq "get" -or $method -eq "delete") -and $line -match "^ {6}requestBody:\s*$") {
            $removedRequestBodies.Value++
            while ($i + 1 -lt $blockLines.Count -and ($blockLines[$i + 1] -match "^ {8,}\S" -or $blockLines[$i + 1].Trim().Length -eq 0)) {
                $i++
            }
            continue
        }

        $lines.Add($line)
    }

    $hasParameters = $lines | Where-Object { $_ -match "^ {6}parameters:\s*$" }
    if (-not $hasParameters -and $pathParams.Count -gt 0) {
        $insertAt = $lines.FindIndex({ param($x) $x -match "^ {6}responses:\s*$" })
        if ($insertAt -lt 0) { $insertAt = 1 }

        $paramLines = New-Object System.Collections.Generic.List[string]
        $paramLines.Add("      parameters:")
        foreach ($p in $pathParams) {
            $paramLines.Add("        - in: path")
            $paramLines.Add("          name: $p")
            $paramLines.Add("          required: true")
            $paramLines.Add("          schema:")
            $paramLines.Add("            type: string")
        }
        for ($j = $paramLines.Count - 1; $j -ge 0; $j--) {
            $lines.Insert($insertAt, $paramLines[$j])
        }
    }

    return ,$lines.ToArray()
}

$root = (Resolve-Path $InputFolder).Path
$files = Get-ChildItem -Path $root -Filter *.yaml | Sort-Object Name
if (-not $files) {
    Write-Host "No yaml files found in $InputFolder"
    exit 0
}

$totalPathsChanged = 0
$totalOpsChanged = 0
$totalBodiesRemoved = 0

foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $out = New-Object System.Collections.Generic.List[string]
    $seen = New-Object System.Collections.Generic.HashSet[string]
    $filePathsChanged = 0
    $fileOpsChanged = 0
    $fileBodiesRemoved = 0

    $i = 0
    while ($i -lt $lines.Count) {
        $line = $lines[$i]

        if ($line -match "^  (?<path>/.+):\s*$") {
            $rawPath = $Matches["path"]
            $san = Sanitize-Path $rawPath
            if ($san.Path -ne $rawPath) { $filePathsChanged++ }
            $out.Add("  $($san.Path):")
            $i++

            while ($i -lt $lines.Count -and $lines[$i] -match "^    ") {
                if ($lines[$i] -match "^    (?<method>get|post|put|patch|delete|options|head|trace):\s*$") {
                    $method = $Matches["method"]
                    $methodHeader = $lines[$i]
                    $i++
                    $block = New-Object System.Collections.Generic.List[string]
                    $block.Add($methodHeader)

                    while ($i -lt $lines.Count -and
                        -not ($lines[$i] -match "^    (get|post|put|patch|delete|options|head|trace):\s*$") -and
                        -not ($lines[$i] -match "^  /.+:\s*$")) {
                        $block.Add($lines[$i])
                        $i++
                    }

                    $newOpId = Build-OperationId $method $san.Path $seen
                    $oldOp = ($block | Where-Object { $_ -match "^ {6}operationId:\s*(.+)$" } | Select-Object -First 1)
                    if ($oldOp -notmatch [regex]::Escape($newOpId)) { $fileOpsChanged++ }

                    $processed = Process-OperationBlock $method $block.ToArray() $san.Params.ToArray() $newOpId ([ref]$fileBodiesRemoved)
                    foreach ($l in $processed) { $out.Add($l) }
                } else {
                    $out.Add($lines[$i])
                    $i++
                }
            }
            continue
        }

        $out.Add($line)
        $i++
    }

    if ($filePathsChanged -gt 0 -or $fileOpsChanged -gt 0 -or $fileBodiesRemoved -gt 0) {
        Set-Content -Path $file.FullName -Value $out -Encoding UTF8
    }

    $totalPathsChanged += $filePathsChanged
    $totalOpsChanged += $fileOpsChanged
    $totalBodiesRemoved += $fileBodiesRemoved
    Write-Host "$($file.Name): paths=$filePathsChanged, operationIds=$fileOpsChanged, removedRequestBodies=$fileBodiesRemoved"
}

Write-Host ""
Write-Host "Done."
Write-Host "Changed paths: $totalPathsChanged"
Write-Host "Changed operationIds: $totalOpsChanged"
Write-Host "Removed requestBody blocks on GET/DELETE: $totalBodiesRemoved"
