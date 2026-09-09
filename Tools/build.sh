#!/bin/bash
set -euo pipefail
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
PLATFORM="${1:-macOS}"
case "$PLATFORM" in
  macOS) METHOD="BuildMac" ;;
  Android) METHOD="BuildAndroid" ;;
  iOS) METHOD="BuildIOS" ;;
  iOS-Simulator) METHOD="BuildIOSSimulator" ;;
  *) printf '%s\n' 'Usage: Tools/build.sh [macOS|Android|iOS|iOS-Simulator]' >&2; exit 2 ;;
esac
BUILDER="GravityBox.Editor.PrototypeBuilder"
if [[ "$PLATFORM" == "macOS" ]]; then BUILDER="GravityBox.Editor.CampaignBuilder"; fi
mkdir -p "$PROJECT_ROOT/Artifacts"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PROJECT_ROOT" -executeMethod "$BUILDER.$METHOD" -quit -logFile "$PROJECT_ROOT/Artifacts/build-$PLATFORM.log"
