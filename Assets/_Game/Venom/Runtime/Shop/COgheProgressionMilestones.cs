using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    // One-time migration of unlock positions, not puzzle wins. Keeping the old catalog here is intentional:
    // it must never follow a future reorder, or an update could hide a previously earned ink or Home item.
    internal static class COgheProgressionMilestones
    {
        private static readonly string[] Before15October = {
            "01","02","03","04","05","K01","06","07","08","10",
            "N22","G1","K02","G2","14","13","09","16","N15","N13","B1",
            "N41","K03","N46","G3","N29","12","N44","17","G4","22","G5",
            "18","E10","N23","21","N25","N47","N48","K04","15","E11","20",
            "K05","N42","K06","N26","N31","26","K07","N33","N35","N43","N40",
            "N45","N49","N18","N32","K08","N34","K09","29","K10","N19","N50"
        };
        internal static void Capture(VenomCampaign game, List<int> reached)
        {
            var catalog = Resources.Load<COgheProductCatalog>("COgheUI/Catalog");
            if (catalog == null) return;
            foreach (var level in catalog.Levels)
            {
                if (level == null || !game.Progress.Completed.Contains(level.Id) || level.Order < 1 || level.Order > level.SceneSequence.Length) continue;
                string key = level.SceneSequence[level.Order - 1].Replace("COgheSpatialPlus", "").Replace("COgheSpatial", "");
                int oldPosition = Array.IndexOf(Before15October, key) + 1;
                if (oldPosition > 0 && !reached.Contains(oldPosition)) reached.Add(oldPosition);
            }
        }
    }
}
