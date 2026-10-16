using System;
using UnityEditor;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        public static void BuildDifficulty65Review()
        {
            ApplyDifficulty65(); COgheProductUIBuilder.BuildMac();
        }
        // Rebuild only the changed puzzles. All other scene edits are number plaques; saves keep stable IDs.
        [MenuItem("Gravity Box/COghe/Spatial/Apply 65 level pacing")]
        public static void ApplyDifficulty65()
        {
            PrepareCampaign30Assets();
            string old = authoredMeshFolder;
            try
            {
                authoredMeshFolder = SpatialFolder + "/Meshes";
                // Boss 10 is explicitly preserved at the user's request (15/10).
                var levels = PlusLevels(); int i = Array.FindIndex(levels, d => d.Key == "G5");
                BuildSpatialPlus(levels[i], i + 1);
            }
            finally { authoredMeshFolder = old; }
            ApplySpatialOrder(); RenumberSpatialPlaques(); COgheProductUIBuilder.Prepare();
            COgheCircuitSurfaceAudit.Run();
        }
    }
}
