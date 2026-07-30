param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")),
    [string]$Blender = "C:\Program Files\Blender Foundation\Blender 2.93\blender.exe"
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path -LiteralPath $Blender)) {
    throw "Blender 2.93 não encontrado em $Blender"
}

& $Blender --background --python (Join-Path $PSScriptRoot "generate_engagement_grayboxes.py") -- --project-root $ProjectRoot
if ($LASTEXITCODE -ne 0) {
    throw "Falha ao gerar grayboxes de engagement. Exit code: $LASTEXITCODE"
}

$required = @(
    (Join-Path $ProjectRoot "SourceAssets\Blender\Characters\Lumi\PLACEHOLDER_Lumi.blend"),
    (Join-Path $ProjectRoot "SourceAssets\Blender\Animals\Arara\PLACEHOLDER_Arara.blend"),
    (Join-Path $ProjectRoot "Assets\Models\Characters\Lumi\PLACEHOLDER_Lumi.fbx"),
    (Join-Path $ProjectRoot "Assets\Models\Animals\Arara\PLACEHOLDER_Arara.fbx")
)
foreach ($path in $required) {
    if (-not (Test-Path -LiteralPath $path) -or (Get-Item -LiteralPath $path).Length -le 0) {
        throw "Saída obrigatória ausente ou vazia: $path"
    }
}
