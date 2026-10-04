#!/bin/bash
set -euo pipefail
COGHE_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
python3 "$COGHE_ROOT/Tools/setup-google-sdks.py"
COGHE_ANDROID_METHOD="GravityBox.Editor.COgheAndroidBuilder.Build"
COGHE_ANDROID_OUTPUT="Builds/Venom/Android/COghe.apk"
if [[ "${1:-}" == "--tap" ]]; then
  COGHE_ANDROID_METHOD="GravityBox.Editor.COgheAndroidBuilder.BuildTap"
  COGHE_ANDROID_OUTPUT="Builds/COgheTapChapter/Android/COghe.apk"
elif [[ "${1:-}" == "--onboarding" ]]; then
  COGHE_ANDROID_METHOD="GravityBox.Editor.COgheAndroidBuilder.BuildOnboarding"
  COGHE_ANDROID_OUTPUT="Builds/COgheOnboarding/Android/COghe-Learn.apk"
elif [[ "${1:-}" == "--spatial" ]]; then
  COGHE_ANDROID_METHOD="GravityBox.Editor.COgheAndroidBuilder.BuildSpatial"
  COGHE_ANDROID_OUTPUT="Builds/SpatialLab/Android/COghe-Spatial.apk"
elif [[ -n "${1:-}" ]]; then
  echo "Usage: bash Tools/build-venom-android.sh [--tap|--onboarding|--spatial]" >&2
  exit 2
fi
mkdir -p "$COGHE_ROOT/Artifacts/COgheAndroid"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$COGHE_ROOT" \
  -buildTarget Android -executeMethod "$COGHE_ANDROID_METHOD" \
  -quit -logFile "$COGHE_ROOT/Artifacts/COgheAndroid/unity-build.log"
echo "APK: $COGHE_ROOT/$COGHE_ANDROID_OUTPUT"
