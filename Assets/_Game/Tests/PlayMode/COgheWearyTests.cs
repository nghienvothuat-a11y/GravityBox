using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        // Exhausted (Mrk 21/10/2026): a tapped device gets a sigh and a sag of the body, but COghe sets off at once and works
        // it; taps in a row get one sigh; fed, there is none.
        [UnityTest, Timeout(300000)] public IEnumerator AnExhaustedCOgheSighsAtADeviceButGoesStraightToWork()
        {
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, InkGuideStep = 4, EnergyUsed = 80 });
            try
            {
                yield return LoadDisplay(4); game.Progress.HomeUnlocked = true;
                Assert.AreEqual(COgheHunger.Exhausted, COgheEnergy.State);
                var p = game.Personality; Assert.IsNotNull(p, "COghe's character runs in product levels");
                var rail = game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().First(r => r.Label == "A");
                float end = Time.realtimeSinceStartup + 10;
                while (!rail.InterlockOpen && Time.realtimeSinceStartup < end) yield return null;
                void TapRail() { game.CameraRig.Frame(720, 1280, 0, true); game.TouchPoint(game.Owner.View.WorldToScreenPoint(rail.HandPoint + Vector3.up * .004f)); }

                Vector3 start = game.Motion.Centre(game.Motion.Selected); float tappedAt = game.Matter.SimulationTime;
                TapRail();
                Assert.IsTrue(rail.Busy, "The tap is taken");
                Assert.AreEqual(1, p.Sighs, "Worn out, COghe sighs at the device");
                Assert.IsNotNull(game.Motion.Get(game.Motion.Selected), "…and sets off at once");
                float deepest = 0, movedDuringSigh = 0;
                end = Time.realtimeSinceStartup + 10;
                while (game.Matter.SimulationTime - tappedAt < COghePersonality.SighLength + .2f && Time.realtimeSinceStartup < end)
                {
                    deepest = Mathf.Max(deepest, p.Sag);
                    if (p.Sag > 0) movedDuringSigh = Vector3.Distance(game.Motion.Centre(game.Motion.Selected), start);
                    yield return null;
                }
                Assert.Greater(deepest, .8f, "The body sags");
                Assert.AreEqual(0, p.Sag, "…and rises again");
                Assert.Greater(movedDuringSigh, .01f, "It walks while it sighs: no wait");
                Assert.IsTrue(rail.Busy || rail.CompletedJourneys > 0, "Still on its way to the handle");

                TapRail();   // again, straight after: one sigh is enough
                Assert.AreEqual(1, p.Sighs, "Taps in a row get one sigh");

                end = Time.realtimeSinceStartup + 40;
                while (rail.CompletedJourneys == 0 && Time.realtimeSinceStartup < end) yield return null;
                Assert.Greater(rail.CompletedJourneys, 0, "Worn out, it still works the device");

                COgheShop.SetEnergyUsed(0); game.ResetLevel();
                end = Time.realtimeSinceStartup + 2;
                while (Time.realtimeSinceStartup < end) yield return null;
                int before = p.Sighs; TapRail();
                Assert.IsTrue(rail.Busy, "The tap is taken");
                Assert.AreEqual(before, p.Sighs, "Fed: no sigh");
            }
            finally { EndShop(); }
        }

        // Review film for Mrk (run on request): an exhausted COghe in level 4; the device tapped, it sighs and sags while it
        // sets off, then works the handle. 720×1280, 30 fps, UI included, frames in Artifacts/Weary/frames; the frame of each
        // sigh in Artifacts/Weary/sighs.txt (for the sound track).
        [UnityTest, Explicit, Timeout(900000)] public IEnumerator WearySighFilm()
        {
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, Drops = 120, InkGuideStep = 4, EnergyUsed = 80 });
            string dir = "Artifacts/Weary/frames"; if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true); System.IO.Directory.CreateDirectory(dir);
            int frame = 0; var rt = new RenderTexture(720, 1280, 24) { antiAliasing = 4 }; var tex = new Texture2D(720, 1280, TextureFormat.RGB24, false);
            var sighs = new System.Collections.Generic.List<int>(); int seen = 0;
            Time.captureFramerate = 30;
            float paced = Time.realtimeSinceStartup;
            IEnumerator Film(float seconds, System.Func<bool> until = null)
            {
                for (int guard = 0; guard < seconds * 30 + 2 && (until == null || !until()); guard++)
                {
                    while (Time.realtimeSinceStartup - paced < 1f / 30) { }
                    paced = Time.realtimeSinceStartup;
                    yield return null;   // (WaitForEndOfFrame never resumes in batch mode)
                    if (game.Personality != null && game.Personality.Sighs > seen) { seen = game.Personality.Sighs; sighs.Add(frame); }
                    var canvas = ProductCanvas; var camera = game.Owner.View;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = camera.nearClipPlane + .01f;
                    game.CameraRig.Spotlight = game.Motion.Centre(game.Motion.Selected); game.CameraRig.SpotlightHalf = .11f;   // close on COghe
                    game.CameraRig.Frame(720, 1280, 1f / 30); camera.targetTexture = rt; camera.Render(); camera.targetTexture = null;
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    var prev = RenderTexture.active; RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); tex.Apply(); RenderTexture.active = prev;
                    System.IO.File.WriteAllBytes($"{dir}/{frame++:0000}.jpg", tex.EncodeToJPG(92));
                }
            }
            try
            {
                yield return LoadDisplay(4); yield return null; game.Progress.HomeUnlocked = true;
                COgheProductUI.LayoutAreaForTests = new Rect(0, 0, 720, 1280); ui.RelayoutForTests();
                var rail = game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().First(r => r.Label == "A");
                yield return Film(2.5f);                                            // worn out where it stands
                game.CameraRig.Frame(720, 1280, 0, true); game.TouchPoint(game.Owner.View.WorldToScreenPoint(rail.HandPoint + Vector3.up * .004f));
                yield return Film(30, () => rail.CompletedJourneys > 0);            // a sigh, a sag, and off it goes
                yield return Film(2.5f);
                System.IO.File.WriteAllText("Artifacts/Weary/sighs.txt", string.Join("\n", sighs));
                Debug.Log($"WEARY FILM frames {frame}, sighs at {string.Join(",", sighs)}, speed x{game.Motion.SpeedScale:F2}");
            }
            finally { Time.captureFramerate = 0; game.CameraRig.Spotlight = null; Object.DestroyImmediate(tex); rt.Release(); COgheProductUI.LayoutAreaForTests = null; ui.RelayoutForTests(); EndShop(); }
        }
    }
}
