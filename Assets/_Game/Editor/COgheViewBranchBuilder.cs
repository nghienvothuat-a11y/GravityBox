using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static void ChapterBranches(ExpansionContext c)
        {
            c.Exit = new Vector3(.44f, -.30f, .32f);
            ChapterShell(c);
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * .28f;
                foreach (float face in new[] { -1f, 1f })
                {
                    Vector3 normal = Vector3.forward * face;
                    Vector3 centre = new Vector3(x, -.10f, .09f + face * .006f);
                    Vector3 opening = new Vector3(side * .145f, -.20f, centre.z);
                    Vector3 hole = Quaternion.Inverse(Quaternion.LookRotation(normal, Vector3.up)) * (opening - centre);
                    Panel(c.Root, "Branch wall", centre, normal, new Vector2(.56f, .40f), stone, true, new Vector2(hole.x, hole.y), .058f, c.Surfaces);
                }
            }
            ViewBlock(c, "Branch separator", new Vector3(0, -.10f, .245f), new Vector3(.018f, .40f, .31f));
            var nodes = new[] {
                new COgheTubeNetwork.Node("Vào", new Vector3(0, -.255f, -.24f), COgheTubeNetwork.TerminalKind.Entry),
                new COgheTubeNetwork.Node("Y", new Vector3(0, -.20f, -.06f)),
                new COgheTubeNetwork.Node("Phụ", new Vector3(-.28f, -.255f, .25f), COgheTubeNetwork.TerminalKind.Entry),
                new COgheTubeNetwork.Node("Chính", new Vector3(.28f, -.255f, .25f), COgheTubeNetwork.TerminalKind.Entry) };
            var edges = new[] { Edge("Vào–Y", 0, 1, nodes, new Vector3(0, -.20f, -.16f)),
                Edge("Y–Phụ", 1, 2, nodes, new Vector3(-.145f, -.20f, .09f), new Vector3(-.28f, -.255f, .17f)),
                Edge("Y–Chính", 1, 3, nodes, new Vector3(.145f, -.20f, .09f), new Vector3(.28f, -.255f, .17f)) };
            var tube = TubeNetwork(c.Root, "Y transfer tube", nodes, edges, .038f, glass); tube.CaptureSurfaceCommandsWhileInside = true;
            var a = ChapterTask(c, "A", -.44f, -.12f, .12f);
            var left = ViewGate(c, "Left branch shutter", new Vector3(-.28f, -.255f, .28f), Vector3.up, .14f, new Vector3(.10f, .09f, .018f));
            var right = ViewGate(c, "Right branch shutter", new Vector3(.28f, -.255f, .28f), Vector3.up, .14f, new Vector3(.10f, .09f, .018f));
            right.InitialTravel = right.Travel; right.Body.position = c.Root.TransformPoint(right.Start + right.Axis * right.Travel);
            ViewLink(c, a.Rail, left, false, null);
            var reverse = new GameObject("Branch selector reverse linkage", typeof(COgheViewMechanism)).GetComponent<COgheViewMechanism>();
            reverse.transform.SetParent(c.Root, false); reverse.Input = a.Rail; reverse.Output = right; reverse.Reverse = true;
            edges[1].AccessGate = left; edges[2].AccessGate = right;
            var safe = new GameObject("Junction safety", typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>(); safe.transform.SetParent(c.Root, false);
            safe.Network = tube; safe.Size = Vector3.zero; a.Clearance = safe;
            // Approach from the open landing behind the pipe mouth. A front-facing
            // stance sends recently emerged tissue back across the solid bore.
            var b = ChapterTask(c, "B", -.47f, .26f, .10f);
            var grip = b.Handle.localPosition; grip.z = .035f; b.Handle.localPosition = grip;
            b.StandOffset = Vector3.forward * .05f;
            ChapterDrive(c, b.Rail, null, ChapterFinal(c));
        }
    }
}
