param(
    [ValidateSet("development", "production")]
    [string]$Profile = "development",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$UnityBin = $env:UNITY_BIN

if ([string]::IsNullOrWhiteSpace($UnityBin)) {
    $UnityBin = "C:\Program Files\Unity\Hub\Editor\2019.4.41f1\Editor\Unity.exe"
}

if (-not (Test-Path $UnityBin)) {
    throw "Unity não encontrado em '$UnityBin'. Defina UNITY_BIN com o Unity 2019.4.41f1."
}

if ($Profile -eq "production") {
    $Method = "AndroidBuildPipeline.BuildProductionAab"
    if ([string]::IsNullOrWhiteSpace($OutputPath)) {
        $OutputPath = Join-Path $ProjectDir "Builds\Android\RealidadeA-production.aab"
    }
} else {
    $Method = "AndroidBuildPipeline.BuildDevelopmentApk"
    if ([string]::IsNullOrWhiteSpace($OutputPath)) {
        $OutputPath = Join-Path $ProjectDir "Builds\Android\RealidadeA-development.apk"
    }
}

$OutputDirectory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

& $UnityBin `
    -batchmode `
    -nographics `
    -quit `
    -projectPath $ProjectDir `
    -buildTarget Android `
    -executeMethod $Method `
    -customBuildPath $OutputPath `
    -logFile -

if ($LASTEXITCODE -ne 0) {
    throw "O Unity encerrou o build com código $LASTEXITCODE."
}

Write-Host "Build gerado em: $OutputPath"
