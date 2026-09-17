#!/bin/bash
set -euo pipefail
COGHE_VERIFY_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
COGHE_UNITY="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
COGHE_RESULTS="$COGHE_VERIFY_ROOT/Artifacts/COgheExpansion"
mkdir -p "$COGHE_RESULTS"
if [[ "${1:-}" == "--generate" ]]; then
  "$COGHE_UNITY" -batchmode -nographics -projectPath "$COGHE_VERIFY_ROOT" \
    -executeMethod GravityBox.Editor.VenomCampaignBuilder.GenerateExpansion \
    -quit -logFile "$COGHE_RESULTS/generate.log"
elif [[ -n "${1:-}" ]]; then
  echo "Usage: bash Tools/verify-coghe-expansion.sh [--generate]" >&2
  exit 2
fi
# Keep graphics enabled: solution tests save actual portrait URP frames.
"$COGHE_UNITY" -batchmode -projectPath "$COGHE_VERIFY_ROOT" \
  -runTests -testPlatform PlayMode \
  -testFilter 'GravityBox.Tests.VenomOriginTests;GravityBox.Tests.COgheExpansionIntegrationTests;GravityBox.Tests.COgheEarlyExpansionTests;GravityBox.Tests.COghePipeExpansionTests;GravityBox.Tests.COgheMechanismExpansionTests' \
  -testResults "$COGHE_RESULTS/verification.xml" \
  -logFile "$COGHE_RESULTS/verification.log"
python3 - "$COGHE_RESULTS/verification.xml" <<'PY'
import sys
import xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
print(f"COghe: {root.get('passed')} passed / {root.get('total')} total; {root.get('failed')} failed")
for case in root.iter('test-case'):
    if case.get('result') != 'Passed':
        print(case.get('name'), case.findtext('failure/message') or case.get('result'))
raise SystemExit(0 if root.get('result') == 'Passed' else 1)
PY
