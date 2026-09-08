#!/bin/bash
set -euo pipefail
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SIMULATOR_ID="${1:-booted}"
XCODE_PATH="${DEVELOPER_DIR:-/Applications/Xcode.app/Contents/Developer}"
export DEVELOPER_DIR="$XCODE_PATH"
bash "$PROJECT_ROOT/Tools/build.sh" iOS-Simulator
xcodebuild -project "$PROJECT_ROOT/Builds/iOS-Simulator/Unity-iPhone.xcodeproj" -scheme Unity-iPhone -configuration Debug -sdk iphonesimulator -arch arm64 -derivedDataPath "$PROJECT_ROOT/Builds/iOS-DerivedData" CODE_SIGNING_ALLOWED=NO build > "$PROJECT_ROOT/Artifacts/xcode-simulator.log" 2>&1
xcrun simctl install "$SIMULATOR_ID" "$PROJECT_ROOT/Builds/iOS-DerivedData/Build/Products/Debug-iphonesimulator/GravityBox.app"
xcrun simctl launch --terminate-running-process "$SIMULATOR_ID" com.gravityboxlab.prototype
