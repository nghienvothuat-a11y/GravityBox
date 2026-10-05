#!/bin/bash
# Run a filtered PlayMode test set, then restore the material files the suite rewrites.
# Usage: bash Tools/run-coghe-tests.sh <name> [testFilter]
set -uo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}"
OUT="$ROOT/Artifacts/Tests"; mkdir -p "$OUT"
NAME="$1"; FILTER="${2:-}"
ARGS=(-batchmode -projectPath "$ROOT" -runTests -testPlatform PlayMode -testResults "$OUT/$NAME.xml" -logFile "$OUT/$NAME.log")
[[ -n "$FILTER" ]] && ARGS+=(-testFilter "$FILTER")
"$UNITY" "${ARGS[@]}"; code=$?
git -C "$ROOT" checkout -q -- "Assets/_Game/PhysicsLab/Materials" "Assets/_Game/Venom/Art/DayLab/Quiet mint light.mat" 2>/dev/null
rm -f "$ROOT"/Assets/Resources/PerformanceTestRun*.json*
python3 - "$OUT/$NAME.xml" <<'PY'
import sys, xml.etree.ElementTree as ET
try: r = ET.parse(sys.argv[1]).getroot()
except Exception as e: print('no results:', e); raise SystemExit(1)
print(f"{r.get('result')}: {r.get('passed')} passed / {r.get('total')} total; {r.get('failed')} failed; {r.get('skipped')} skipped; {float(r.get('duration',0)):.0f}s")
for c in r.iter('test-case'):
    if c.get('result') not in ('Passed', 'Skipped'):
        print('FAIL', c.get('fullname'), (c.findtext('failure/message') or '')[:600].replace('\n', ' | '))
PY
exit $code
