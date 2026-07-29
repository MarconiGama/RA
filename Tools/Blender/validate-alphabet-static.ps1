[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$Root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
Set-Location $Root

$errors = [System.Collections.Generic.List[string]]::new()
$letters = [char[]](65..90)

foreach ($letter in $letters) {
    foreach ($path in @(
        "SourceAssets/Blender/Alphabet/blends/RA_Letter_$letter.blend",
        "Assets/Models/Alphabet/FBX/RA_Letter_$letter.fbx"
    )) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            $errors.Add("Ausente: $path")
        }
        elseif ((Get-Item -LiteralPath $path).Length -le 0) {
            $errors.Add("Vazio: $path")
        }
    }
    if (-not (Test-Path -LiteralPath "Assets/Objetos/Ra$letter.blend" -PathType Leaf)) {
        $errors.Add("Legado ausente: Assets/Objetos/Ra$letter.blend")
    }
}

$requiredScripts = @(
    "Tools/Blender/generate_alphabet.py",
    "Tools/Blender/export_alphabet_fbx.py",
    "Tools/Blender/validate_alphabet.py",
    "Tools/Blender/inspect_blend.py",
    "Tools/Blender/build-alphabet-assets.ps1"
)
foreach ($path in $requiredScripts) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $errors.Add("Script ausente: $path")
    }
}

$content = Get-Content -Raw -LiteralPath "Assets/Resources/Content/alphabet-pt-br.json" | ConvertFrom-Json
if ($content.items.Count -ne 26) {
    $errors.Add("JSON deve conter 26 itens")
}
$targets = @($content.items | ForEach-Object { $_.target })
$ids = @($content.items | ForEach-Object { $_.id })
if (($targets | Sort-Object -Unique).Count -ne 26) {
    $errors.Add("Targets duplicados no JSON")
}
if (($ids | Sort-Object -Unique).Count -ne 26) {
    $errors.Add("IDs duplicados no JSON")
}

$forbidden = @(Get-ChildItem -Path . -Recurse -Force -File | Where-Object {
    $_.FullName -notmatch "[\\/]\.git[\\/]" -and (
        $_.Name -match "\.blend[12]$" -or
        $_.Name -match "\.autosave$" -or
        $_.Name -match "\.blend@$"
    )
})
foreach ($file in $forbidden) {
    $errors.Add("Arquivo temporário proibido: $($file.FullName)")
}
if (Test-Path -LiteralPath "Library") {
    $trackedLibrary = git ls-files -- "Library"
    if ($trackedLibrary) {
        $errors.Add("Library está versionada")
    }
}
$blendInModels = @(Get-ChildItem -Path "Assets/Models" -Recurse -Filter "*.blend" -File -ErrorAction SilentlyContinue)
foreach ($file in $blendInModels) {
    $errors.Add("Arquivo .blend proibido em Assets/Models: $($file.FullName)")
}

if ($errors.Count) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}
Write-Host "Validação estática aprovada: 26 fontes, 26 FBX, conteúdo e higiene do repositório."
exit 0

