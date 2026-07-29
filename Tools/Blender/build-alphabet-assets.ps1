[CmdletBinding()]
param(
    [string[]]$Letters = @("A-Z"),
    [switch]$Overwrite
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$Blender = "C:\Program Files\Blender Foundation\Blender 2.93\blender.exe"
$ExpectedBranch = "agent/scale-ra-foundation"
$LetterArgument = ($Letters -join ",")
$BuildDirectory = Join-Path $RepositoryRoot "Builds\Art\Alphabet"
$LogPath = Join-Path $BuildDirectory "build-alphabet-assets.log"

function Invoke-Checked {
    param([string]$Label, [string[]]$Arguments)
    Write-Host "==> $Label"
    & $Blender @Arguments 2>&1 | Tee-Object -FilePath $LogPath -Append
    if ($LASTEXITCODE -ne 0) {
        throw "$Label falhou com código $LASTEXITCODE. Consulte $LogPath"
    }
}

try {
    Set-Location $RepositoryRoot
    New-Item -ItemType Directory -Force -Path $BuildDirectory | Out-Null
    Set-Content -LiteralPath $LogPath -Value "RA Alphabet build - $(Get-Date -Format o)"

    if (-not (Test-Path -LiteralPath $Blender -PathType Leaf)) {
        throw "Blender 2.93 não encontrado em $Blender"
    }
    $version = (& $Blender --version 2>&1 | Select-Object -First 1)
    if ($version -notmatch "Blender 2\.93\.18") {
        throw "Versão inesperada do Blender: $version"
    }
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        throw "Git não encontrado"
    }
    $branch = (git branch --show-current).Trim()
    if ($LASTEXITCODE -ne 0 -or $branch -ne $ExpectedBranch) {
        throw "Branch inválida: '$branch'. Esperada: '$ExpectedBranch'"
    }
    $status = git status --short
    if ($LASTEXITCODE -ne 0) {
        throw "Não foi possível consultar o estado do Git"
    }
    if ($status) {
        throw "Git deve estar limpo antes do build. Alterações encontradas: $status"
    }

    $overwriteArgument = if ($Overwrite) { @("--overwrite") } else { @() }
    Invoke-Checked "Gerar letras" (@(
        "--background", "--factory-startup",
        "--python", "Tools/Blender/generate_alphabet.py", "--",
        "--letters", $LetterArgument,
        "--output-dir", "SourceAssets/Blender/Alphabet/blends",
        "--report", "Builds/Art/Alphabet/generation-report.json"
    ) + $overwriteArgument)

    Invoke-Checked "Validar arquivos Blender" @(
        "--background", "--factory-startup",
        "--python", "Tools/Blender/validate_alphabet.py", "--",
        "--letters", $LetterArgument,
        "--stage", "blend",
        "--report-json", "Builds/Art/Alphabet/blend-validation.json"
    )

    Invoke-Checked "Exportar FBX" (@(
        "--background", "--factory-startup",
        "--python", "Tools/Blender/export_alphabet_fbx.py", "--",
        "--letters", $LetterArgument,
        "--input-dir", "SourceAssets/Blender/Alphabet/blends",
        "--output-dir", "Assets/Models/Alphabet/FBX",
        "--report", "Builds/Art/Alphabet/export-report.json"
    ) + $overwriteArgument)

    Invoke-Checked "Validar pipeline completo" @(
        "--background", "--factory-startup",
        "--python", "Tools/Blender/validate_alphabet.py", "--",
        "--letters", $LetterArgument,
        "--stage", "all",
        "--report-json", "Builds/Art/Alphabet/validation-report.json",
        "--report-md", "docs/art-pipeline/VALIDATION_REPORT.md"
    )

    Write-Host "Pipeline concluído com sucesso."
    exit 0
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Set-Location $RepositoryRoot
}

