#!/bin/zsh
# Spatial Plus design plates: render the 20 Spatial Plus levels in PlayMode (game view + plan + projected notes into
# Artifacts/SpatialPlusDesign). Usage: Tools/render-spatial-plus-designs.sh [KEYS] — KEYS rebuilds those levels first.
# KEYS: comma list (E01,B2…) or omit for all. Requires the Unity editor used by the project (see ProjectVersion.txt).
set -e
P=$(cd "$(dirname "$0")/.." && pwd)
U=${UNITY:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}
A=$P/Artifacts/SpatialPlusDesign; mkdir -p $A
if [[ -n "$1" ]]; then "$U" -batchmode -nographics -projectPath "$P" -executeMethod GravityBox.Editor.VenomCampaignBuilder.GenerateSpatialPlus -coghe-plus-levels "$1" -quit -logFile $A/gen.log; fi
"$U" -batchmode -projectPath "$P" -runTests -testPlatform PlayMode -testFilter "GravityBox.Tests.COgheSpatialPlusDesignRender" -testResults $A/render.xml -logFile $A/render.log || true
# PlayMode tests toggle the shared mint material's emission keyword; keep the committed asset.
git -C "$P" checkout -- "Assets/_Game/Venom/Art/DayLab/Quiet mint light.mat" 2>/dev/null || true
echo "renders in $A (KEY-game.png, KEY-plan.png, KEY.proj)"
