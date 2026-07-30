[CmdletBinding()]
param(
    [string]$Python,
    [string]$OutputDirectory,
    [switch]$ValidateOnly
)

$ErrorActionPreference = 'Stop'
$scriptPath = Join-Path $PSScriptRoot 'build-target-catalog.py'

if (-not $Python) {
    $pythonCommand = Get-Command python -ErrorAction SilentlyContinue
    if ($pythonCommand) {
        $Python = $pythonCommand.Source
    } else {
        $pyLauncher = Get-Command py -ErrorAction SilentlyContinue
        if (-not $pyLauncher) {
            throw 'Python 3 não foi encontrado no PATH.'
        }
        $Python = $pyLauncher.Source
    }
}

$arguments = @($scriptPath)
if ($OutputDirectory) {
    $arguments += @('--output-dir', $OutputDirectory)
}
if ($ValidateOnly) {
    $arguments += '--validate-only'
}

& $Python @arguments
if ($LASTEXITCODE -ne 0) {
    throw "O gerador encerrou com código $LASTEXITCODE."
}

