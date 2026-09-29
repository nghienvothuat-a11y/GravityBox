#!/bin/zsh
# Spatial Plus design plates: build the 20 greyboxes, render them in PlayMode (game view + plan + projected notes into
# Artifacts/SpatialPlusDesign), then delete every temporary asset. Usage: Tools/render-spatial-plus-designs.sh [KEYS]
# KEYS: comma list (E01,B2…) or omit for all. Requires the Unity editor used by the project (see ProjectVersion.txt).
set -e
P=$(cd "$(dirname "$0")/.." && pwd)
U=${UNITY:-/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity}
A=$P/Artifacts/SpatialPlusDesign; mkdir -p $A
SEL=(); [[ -n "$1" ]] && SEL=(-coghe-plus-designs "$1")
"$U" -batchmode -nographics -projectPath "$P" -executeMethod GravityBox.Editor.VenomCampaignBuilder.GeneratePlusDesigns $SEL -quit -logFile $A/gen.log
"$U" -batchmode -projectPath "$P" -runTests -testPlatform PlayMode -testFilter "GravityBox.Tests.COgheSpatialPlusDesignRender" -testResults $A/render.xml -logFile $A/render.log || true
"$U" -batchmode -nographics -projectPath "$P" -executeMethod GravityBox.Editor.VenomCampaignBuilder.CleanPlusDesigns -quit -logFile $A/clean.log
# PlayMode tests toggle the shared mint material's emission keyword; keep the committed asset.
git -C "$P" checkout -- "Assets/_Game/Venom/Art/DayLab/Quiet mint light.mat" 2>/dev/null || true
echo "renders in $A (KEY-game.png, KEY-plan.png, KEY.proj)"
