using System.Collections.Generic;

namespace GravityBox.Venom
{
    // One-time migrations of unlock positions, not puzzle wins. Keeping the old orders here is intentional:
    // they must never follow a future reorder, or an update could hide a previously earned ink or Home item.
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
        // 19/10/2026 (Mrk): the six glass-road levels took positions 8, 11, 25, 27, 59 and 63; the order they left.
        private static readonly string[] Before19October = {
            "01","02","03","04","05","K01","06","08","09","10",
            "G1","K02","G2","N22","N29","16","N13","N18","13","12","B1",
            "14","N15","K03","07","G3","N41","N44","17","G4","22","G5",
            "18","N23","E10","21","N25","N48","N47","K04","15","E11","20",
            "K05","N42","K06","N26","N31","N33","K07","26","N35","N43","N40",
            "N46","N45","N49","N32","K08","N34","K09","29","K10","N19","N50"
        };
        internal const int LatestRevision = 2;
        /// <summary>Positions the player had reached in the order of a revision (1: before 15/10, 2: before 19/10). By stable ID,
        /// not through the catalog: a level that has since left the order still counts.</summary>
        internal static void Capture(VenomCampaign game, List<int> reached, int revision)
        {
            var order = revision == 1 ? Before15October : Before19October;
            for (int i = 0; i < order.Length; i++)
                if (game.Progress.Completed.Contains(IdOf(order[i])) && !reached.Contains(i + 1)) reached.Add(i + 1);
        }
        // As the builders: pilot 01–10, Spatial 11–30, Spatial Plus by key.
        private static string IdOf(string key) => char.IsDigit(key[0])
            ? (int.Parse(key) <= 10 ? "coghe.spatial.pilot." : "coghe.spatial.next.") + key
            : "coghe.spatial.plus." + key.ToLowerInvariant();
    }
}
