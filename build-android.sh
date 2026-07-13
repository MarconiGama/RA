#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROFILE="${1:-development}"
CUSTOM_OUTPUT="${2:-}"
UNITY_BIN="${UNITY_BIN:-}"

case "${PROFILE}" in
  development|dev|apk)
    METHOD="AndroidBuildPipeline.BuildDevelopmentApk"
    DEFAULT_OUTPUT="${PROJECT_DIR}/Builds/Android/RealidadeA-development.apk"
    ;;
  production|prod|aab)
    METHOD="AndroidBuildPipeline.BuildProductionAab"
    DEFAULT_OUTPUT="${PROJECT_DIR}/Builds/Android/RealidadeA-production.aab"
    ;;
  *)
    echo "Uso: $0 [development|production] [caminho-de-saida]" >&2
    exit 2
    ;;
esac

OUTPUT_PATH="${CUSTOM_OUTPUT:-${DEFAULT_OUTPUT}}"

if [[ -z "${UNITY_BIN}" ]]; then
  for candidate in \
    "/opt/unity/Editor/Unity" \
    "/Applications/Unity/Hub/Editor/2019.4.20f1/Unity.app/Contents/MacOS/Unity"; do
    if [[ -x "${candidate}" ]]; then
      UNITY_BIN="${candidate}"
      break
    fi
  done
fi

if [[ -z "${UNITY_BIN}" ]]; then
  if command -v Unity >/dev/null 2>&1; then
    UNITY_BIN="$(command -v Unity)"
  elif command -v unity-editor >/dev/null 2>&1; then
    UNITY_BIN="$(command -v unity-editor)"
  else
    echo "Unity não encontrado. Defina UNITY_BIN com o executável do Unity 2019.4.20f1." >&2
    exit 1
  fi
fi

mkdir -p "$(dirname "${OUTPUT_PATH}")"

"${UNITY_BIN}" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "${PROJECT_DIR}" \
  -buildTarget Android \
  -executeMethod "${METHOD}" \
  -customBuildPath "${OUTPUT_PATH}" \
  -logFile -

echo "Build gerado em: ${OUTPUT_PATH}"
