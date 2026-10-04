#!/bin/bash
set -euo pipefail
COGHE_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
export DEVELOPER_DIR="${DEVELOPER_DIR:-/Applications/Xcode.app/Contents/Developer}"
python3 "$COGHE_ROOT/Tools/setup-google-sdks.py"
mkdir -p "$COGHE_ROOT/Artifacts/COgheIOS"
COGHE_IOS_OUTPUT="$COGHE_ROOT/Builds/Venom/iOS"
COGHE_IOS_METHOD=GravityBox.Editor.COgheIOSBuilder.Build
COGHE_IOS_NATIVE_ONLY=0
for arg in "$@"; do
  case "$arg" in
    --spatial) COGHE_IOS_OUTPUT="$COGHE_ROOT/Builds/SpatialLab/iOS"; COGHE_IOS_METHOD=GravityBox.Editor.COgheIOSBuilder.BuildSpatial ;;
    --native-only) COGHE_IOS_NATIVE_ONLY=1 ;;
    *) echo "Usage: bash Tools/build-venom-ios.sh [--spatial] [--native-only]" >&2; exit 2 ;;
  esac
done
if [[ "$COGHE_IOS_NATIVE_ONLY" == 0 ]]; then
    "$UNITY_EDITOR" -batchmode -nographics -projectPath "$COGHE_ROOT" \
      -buildTarget iOS -executeMethod "$COGHE_IOS_METHOD" \
      -quit -logFile "$COGHE_ROOT/Artifacts/COgheIOS/unity-export.log"
else
  test -d "$COGHE_IOS_OUTPUT/Unity-iPhone.xcodeproj"
fi

if [[ -z "${COGHE_IOS_TEAM:-}" ]]; then
  echo "Export ready: $COGHE_IOS_OUTPUT. Configure signing in Xcode to install."
  exit 0
fi

COGHE_SIGNING=("DEVELOPMENT_TEAM=$COGHE_IOS_TEAM")
COGHE_DESTINATION='generic/platform=iOS'
if [[ -z "${COGHE_IOS_PROFILE:-}" ]]; then
  COGHE_SIGNING+=(-allowProvisioningUpdates CODE_SIGN_STYLE=Automatic PROVISIONING_PROFILE_SPECIFIER=)
  if [[ -n "${COGHE_IOS_DEVICE:-}" ]]; then
    COGHE_SIGNING+=(-allowProvisioningDeviceRegistration)
    # CoreDevice identifiers differ from the hardware UDIDs Xcode expects.
    xcrun devicectl device info details --device "$COGHE_IOS_DEVICE" --quiet \
      --json-output "$COGHE_ROOT/Artifacts/COgheIOS/device.json"
    COGHE_UDID="$(plutil -extract result.hardwareProperties.udid raw \
      "$COGHE_ROOT/Artifacts/COgheIOS/device.json")"
    COGHE_DESTINATION="id=$COGHE_UDID"
  fi
fi
COGHE_IOS_PROJECT=(-project "$COGHE_IOS_OUTPUT/Unity-iPhone.xcodeproj")
if [[ -d "$COGHE_IOS_OUTPUT/Unity-iPhone.xcworkspace" ]]; then
  COGHE_IOS_PROJECT=(-workspace "$COGHE_IOS_OUTPUT/Unity-iPhone.xcworkspace")
fi
if ! xcodebuild "${COGHE_IOS_PROJECT[@]}" \
  -scheme Unity-iPhone -configuration Release -sdk iphoneos \
  -destination "$COGHE_DESTINATION" -derivedDataPath "${COGHE_IOS_OUTPUT}-DerivedData" \
  "${COGHE_SIGNING[@]}" \
  build > "$COGHE_ROOT/Artifacts/COgheIOS/xcode-build.log" 2>&1; then
  tail -n 35 "$COGHE_ROOT/Artifacts/COgheIOS/xcode-build.log" >&2
  exit 1
fi
echo "COGHE IOS BUILD SUCCESS"

if [[ -n "${COGHE_IOS_DEVICE:-}" ]]; then
  xcrun devicectl device install app --device "$COGHE_IOS_DEVICE" \
    "${COGHE_IOS_OUTPUT}-DerivedData/Build/Products/Release-iphoneos/COghe.app"
  xcrun devicectl device process launch --device "$COGHE_IOS_DEVICE" \
    --terminate-existing com.gravityboxlab.venom
fi
