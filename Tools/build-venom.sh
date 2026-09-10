#!/bin/bash
set -euo pipefail
VENOM_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$VENOM_ROOT/Artifacts/Venom01"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$VENOM_ROOT" \
  -executeMethod GravityBox.Editor.VenomPrototypeBuilder.BuildMac \
  -quit -logFile "$VENOM_ROOT/Artifacts/Venom01/build-macOS.log"
