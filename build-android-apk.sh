#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_APK="${1:-${PROJECT_DIR}/Builds/android/RA-debug.apk}"
UNITY_BIN="${UNITY_BIN:-}"

if [[ -z "${UNITY_BIN}" ]]; then
  if command -v unity-editor >/dev/null 2>&1; then
    UNITY_BIN="$(command -v unity-editor)"
  elif command -v Unity >/dev/null 2>&1; then
    UNITY_BIN="$(command -v Unity)"
  elif command -v unity >/dev/null 2>&1; then
    UNITY_BIN="$(command -v unity)"
  else
    echo "Erro: Unity não encontrado. Defina UNITY_BIN com o caminho do editor Unity." >&2
    exit 1
  fi
fi

mkdir -p "$(dirname "${OUTPUT_APK}")"

"${UNITY_BIN}" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "${PROJECT_DIR}" \
  -buildTarget Android \
  -executeMethod BuildScript.BuildAndroidAPK \
  -customBuildPath "${OUTPUT_APK}" \
  -logFile -

echo "APK gerado em: ${OUTPUT_APK}"
