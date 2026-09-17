#!/bin/bash
set -euo pipefail
COGHE_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$COGHE_ROOT/Artifacts/COgheAndroid"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$COGHE_ROOT" \
  -buildTarget Android -executeMethod GravityBox.Editor.COgheAndroidBuilder.Build \
  -quit -logFile "$COGHE_ROOT/Artifacts/COgheAndroid/unity-build.log"
echo "APK: $COGHE_ROOT/Builds/Venom/Android/COghe.apk"
