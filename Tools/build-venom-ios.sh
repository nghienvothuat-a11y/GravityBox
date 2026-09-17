#!/bin/bash
set -euo pipefail
COGHE_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
export DEVELOPER_DIR="${DEVELOPER_DIR:-/Applications/Xcode.app/Contents/Developer}"
mkdir -p "$COGHE_ROOT/Artifacts/COgheIOS"
case "${1:-}" in
  "")
    "$UNITY_EDITOR" -batchmode -nographics -projectPath "$COGHE_ROOT" \
      -buildTarget iOS -executeMethod GravityBox.Editor.COgheIOSBuilder.Build \
      -quit -logFile "$COGHE_ROOT/Artifacts/COgheIOS/unity-export.log"
    ;;
  --native-only) test -d "$COGHE_ROOT/Builds/Venom/iOS/Unity-iPhone.xcodeproj" ;;
  *) echo "Usage: bash Tools/build-venom-ios.sh [--native-only]" >&2; exit 2 ;;
esac

if [[ -z "${COGHE_IOS_TEAM:-}" ]]; then
  echo "Export ready: Builds/Venom/iOS/Unity-iPhone.xcodeproj. Configure signing in Xcode to install."
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
if ! xcodebuild -project "$COGHE_ROOT/Builds/Venom/iOS/Unity-iPhone.xcodeproj" \
  -scheme Unity-iPhone -configuration Release -sdk iphoneos \
  -destination "$COGHE_DESTINATION" -derivedDataPath "$COGHE_ROOT/Builds/Venom/iOS-DerivedData" \
  "${COGHE_SIGNING[@]}" \
  build > "$COGHE_ROOT/Artifacts/COgheIOS/xcode-build.log" 2>&1; then
  tail -n 35 "$COGHE_ROOT/Artifacts/COgheIOS/xcode-build.log" >&2
  exit 1
fi
echo "COGHE IOS BUILD SUCCESS"

if [[ -n "${COGHE_IOS_DEVICE:-}" ]]; then
  xcrun devicectl device install app --device "$COGHE_IOS_DEVICE" \
    "$COGHE_ROOT/Builds/Venom/iOS-DerivedData/Build/Products/Release-iphoneos/COghe.app"
  xcrun devicectl device process launch --device "$COGHE_IOS_DEVICE" \
    --terminate-existing com.gravityboxlab.venom
fi
