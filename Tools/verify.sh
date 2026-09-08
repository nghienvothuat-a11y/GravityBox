#!/bin/bash
set -euo pipefail
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
mkdir -p "$PROJECT_ROOT/Artifacts"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PROJECT_ROOT" -runTests -testPlatform EditMode -testResults "$PROJECT_ROOT/Artifacts/editmode-results.xml" -logFile "$PROJECT_ROOT/Artifacts/editmode.log"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PROJECT_ROOT" -runTests -testPlatform PlayMode -testResults "$PROJECT_ROOT/Artifacts/playmode-results.xml" -logFile "$PROJECT_ROOT/Artifacts/playmode.log"
