#!/bin/bash
set -euo pipefail
VENOM_VERIFY_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
VENOM_SCENE_BACKUP="$(mktemp -t venom-scenes)"
cp "$VENOM_VERIFY_ROOT/ProjectSettings/EditorBuildSettings.asset" "$VENOM_SCENE_BACKUP"
trap 'cp "$VENOM_SCENE_BACKUP" "$VENOM_VERIFY_ROOT/ProjectSettings/EditorBuildSettings.asset"; rm -f "$VENOM_SCENE_BACKUP"' EXIT

# Runtime scene indices are captured before entering Play Mode. Register archived
# destinations before starting Unity, then restore the ten-scene shipping list.
python3 - "$VENOM_VERIFY_ROOT" <<'PY'
from pathlib import Path
import sys
root = Path(sys.argv[1])
settings = root / 'ProjectSettings/EditorBuildSettings.asset'
text = settings.read_text()
extra = ''
for i in range(1, 6):
    path = f'Assets/_Game/Venom/VenomJourney{i:02}.unity'
    if f'path: {path}\n' in text:
        continue
    guid = next(line.split(': ', 1)[1] for line in (root / (path + '.meta')).read_text().splitlines() if line.startswith('guid:'))
    extra += f'  - enabled: 1\n    path: {path}\n    guid: {guid}\n'
settings.write_text(text.replace('  m_configObjects:', extra + '  m_configObjects:'))
PY

mkdir -p "$VENOM_VERIFY_ROOT/Artifacts/Venom01"
VENOM_TEST_FILTER="${1:-GravityBox.Tests.VenomClimbTests;GravityBox.Tests.VenomControlTests;GravityBox.Tests.VenomGuidanceTests;GravityBox.Tests.VenomJourneyTests;GravityBox.Tests.VenomOriginTests;GravityBox.Tests.VenomPrototypeTests;GravityBox.Tests.VenomVaultTests}"
"$UNITY_EDITOR" -batchmode -projectPath "$VENOM_VERIFY_ROOT" \
  -runTests -testPlatform PlayMode -testFilter "$VENOM_TEST_FILTER" \
  -testResults "$VENOM_VERIFY_ROOT/Artifacts/Venom01/origin-regression.xml" \
  -logFile "$VENOM_VERIFY_ROOT/Artifacts/Venom01/origin-regression.log"
