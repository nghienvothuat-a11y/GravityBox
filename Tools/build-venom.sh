#!/bin/bash
set -euo pipefail
VENOM_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$VENOM_ROOT/Artifacts/Venom01"
VENOM_BUILD_METHOD="GravityBox.Editor.VenomPrototypeBuilder.BuildJourneyMac"
if [[ "${1:-}" == "--lab" ]]; then
  VENOM_BUILD_METHOD="GravityBox.Editor.VenomPrototypeBuilder.BuildMac"
elif [[ -n "${1:-}" ]]; then
  echo "Usage: bash Tools/build-venom.sh [--lab]" >&2
  exit 2
fi
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$VENOM_ROOT" \
  -executeMethod "$VENOM_BUILD_METHOD" \
  -quit -logFile "$VENOM_ROOT/Artifacts/Venom01/build-macOS.log"
