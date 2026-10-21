using System.Collections;
using System.IO;
using System.Text;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    // Mystery gift box placement aid (run on request): solve each box level and record the merged body's way to the exit
    // (root space, every 0.25 s), the exit, the camera yaw and the surface under COghe, to choose each level's box spot.
    public sealed partial class COgheSpatialCampaignTests
    {
        [UnityTest, Explicit, Timeout(1800000)] public IEnumerator MysteryBoxRoutes()
        {
            var log = new StringBuilder(); Directory.CreateDirectory("Artifacts/MysteryBox");
            yield return Load(1); var sequence = game.Definition.SceneSequence;
            foreach (int order in new[] { 8, 13, 18, 23, 28, 33, 38, 44, 48, 53, 58, 63 })
            {
                yield return LoadScene(sequence[order - 1]);
                var root = game.Root; int ticks = 0; var samples = new StringBuilder();
                trace = () =>
                {
                    if (++ticks % 30 != 0 || game.Matter.EscapedCount > 0) return;
                    Vector3 c = game.Motion.Centre(game.Motion.Selected);
                    samples.Append($"  t={ticks * Dt:F2} frag={game.Matter.FragmentCount} c={root.InverseTransformPoint(c):F3} dExit={Vector3.Distance(c, game.Owner.Outlet.position):F3} yaw={game.CameraRig.OrbitYaw:F0}\n");
                };
                string result = "solved";
                var solve = new COgheSpatialScenario(game, Tap, Until).Solve();
                while (true)
                {
                    bool more;
                    try { more = solve.MoveNext(); } catch (System.Exception e) { result = "FAILED " + e.Message.Split('\n')[0]; break; }
                    if (!more) break; yield return solve.Current;
                }
                trace = null;
                var outlet = game.Owner.Outlet;
                log.Append($"== display {order} id={game.Definition.Id} viewOnly={game.Definition.ViewOnly} camera={game.Definition.CameraEuler} {result}\n");
                log.Append($"  exit={root.InverseTransformPoint(outlet.position):F3} outward={root.InverseTransformDirection(outlet.forward):F2}\n");
                var lines = samples.ToString().Split('\n'); int from = Mathf.Max(0, lines.Length - 26);
                for (int i = from; i < lines.Length; i++) if (lines[i].Length > 0) log.Append(lines[i]).Append('\n');
                File.WriteAllText("Artifacts/MysteryBox/routes.txt", log.ToString());
            }
            Debug.Log("MYSTERY BOX ROUTES\n" + log);
        }
    }
}
