using System.Collections.Generic;
using System.Linq;

namespace GravityBox.Editor
{
    internal static class CampaignCurriculum
    {
        private static readonly Dictionary<int, string> Introductions = new Dictionary<int, string>
        {
            {1,"tilt"},{2,"brake"},{3,"contact"},{5,"corner"},{8,"clearance"},
            {11,"slider"},{12,"parking"},{15,"bridge"},{21,"drop"},{23,"cage"},{26,"spatial"},
            {31,"two-balls"},{33,"lever"},{36,"partner-release"},
            {41,"cam-stroke"},{42,"cam-return"},{45,"cam-route"},
            {51,"pendulum"},{55,"flight"},{56,"catch"},{62,"water"},{72,"mercury"}
        };

        public static string[] Introduced(int index) => Introductions.TryGetValue(index, out string skill)
            ? new[] { skill } : System.Array.Empty<string>();

        public static string[] Required(CampaignLevelSpec spec)
        {
            var skills = new HashSet<string>();
            if (spec.index > 1) skills.Add("tilt");
            if (spec.index > 2) skills.Add("brake");
            foreach (int source in spec.source_prototypes)
            {
                switch (source)
                {
                    case 2: Add("contact"); break;
                    case 4: case 5: case 6: case 7: case 8: case 15: Add("corner"); break;
                    case 9: case 10: Add("slider"); Add("parking"); break;
                    case 11: Add("drop"); break;
                    case 12: Add("spatial"); break;
                    case 13: Add("water"); break;
                    case 14: Add("mercury"); break;
                    case 16: Add("two-balls"); if (spec.index >= 36 && spec.index != 39) Add("partner-release"); break;
                    case 17: Add("parking"); Add("bridge"); break;
                    case 18: Add("two-balls"); Add("lever"); break;
                    case 19: Add("cage"); break;
                    case 20: Add("pendulum"); break;
                    case 21: Add("flight"); Add("catch"); break;
                    case 22: Add("cam-stroke"); Add("cam-return"); Add("cam-route"); break;
                    case 23: Add("two-balls"); Add("lever"); Add("partner-release"); Add("cage"); break;
                }
            }
            // Introductory variants intentionally omit later systems from their source prototype.
            return skills.OrderBy(s => s).ToArray();
            void Add(string name)
            {
                foreach (var intro in Introductions)
                    if (intro.Value == name && intro.Key < spec.index) { skills.Add(name); break; }
            }
        }
    }
}
