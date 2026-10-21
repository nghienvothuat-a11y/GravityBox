using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        // Review film for Mrk (run on request): the same walk across level 1 and back, fed, hungry and exhausted, close on
        // COghe; hungry it stops now and then to catch its breath, exhausted more often and longer, flat to the floor.
        // 720×1280, 30 fps, frames in Artifacts/Rest/frames; the log line "REST FILM" has each part's first frame and the
        // frames of the pants (for the review clip's captions and sound).
        [UnityTest, Explicit, Timeout(900000)] public IEnumerator TiredRestFilm()
        {
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, Drops = 120, InkGuideStep = 4 });
            string dir = "Artifacts/Rest/frames"; if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true); System.IO.Directory.CreateDirectory(dir);
            int frame = 0; var rt = new RenderTexture(720, 1280, 24) { antiAliasing = 4 }; var tex = new Texture2D(720, 1280, TextureFormat.RGB24, false);
            Time.captureFramerate = 30;
            float paced = Time.realtimeSinceStartup;
            int pants = 0; var log = new System.Text.StringBuilder();
            IEnumerator Film(float seconds, System.Func<bool> until = null)
            {
                for (int guard = 0; guard < seconds * 30 + 2 && (until == null || !until()); guard++)
                {
                    while (Time.realtimeSinceStartup - paced < 1f / 30) { }
                    paced = Time.realtimeSinceStartup;
                    yield return null;   // (WaitForEndOfFrame never resumes in batch mode)
                    var canvas = ProductCanvas; var camera = game.Owner.View;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = camera.nearClipPlane + .01f;
                    game.CameraRig.Spotlight = game.Motion.Centre(game.Motion.Selected); game.CameraRig.SpotlightHalf = .16f;   // close on COghe
                    game.CameraRig.Frame(720, 1280, 1f / 30); camera.targetTexture = rt; camera.Render(); camera.targetTexture = null;
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    var prev = RenderTexture.active; RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); tex.Apply(); RenderTexture.active = prev;
                    System.IO.File.WriteAllBytes($"{dir}/{frame++:0000}.jpg", tex.EncodeToJPG(92));
                    var p = game.Personality;
                    if (p != null && p.Pants != pants) { pants = p.Pants; log.Append($" pant@{frame - 1}"); }
                }
            }
            IEnumerator Walk(Vector3 world) { game.Motion.Move(game.Motion.Selected, world); yield return Film(16, () => game.Motion.Get(game.Motion.Selected) == null); }
            try
            {
                yield return LoadDisplay(2); yield return LoadDisplay(1);   // (the fixture starts on level 1: reload it fresh)
                yield return null; game.Progress.HomeUnlocked = true;          // the loaded level has its own progress
                COgheProductUI.LayoutAreaForTests = new Rect(0, 0, 720, 1280); ui.RelayoutForTests();
                foreach (var (used, name) in new[] { (0, "fed"), (60, "hungry"), (80, "exhausted") })
                {
                    COgheShop.SetEnergyUsed(used); game.ResetLevel(); pants = game.Personality != null ? game.Personality.Pants : 0;
                    for (int i = 0; i < 60; i++) yield return null;   // settle where the level puts it
                    log.Append($" {name}@{frame}");
                    Assert.AreEqual(name == "fed" ? 0f : name == "hungry" ? COgheEnergy.HungryWalk : COgheEnergy.ExhaustedWalk, game.Motion.RestWalk, name);
                    Vector3 start = game.Motion.Centre(game.Motion.Selected), local = game.Root.InverseTransformPoint(start);
                    yield return Film(1);
                    yield return Walk(game.Root.TransformPoint(new Vector3(local.x > 0 ? -.30f : .30f, local.y, local.z)));
                    yield return Film(.8f);
                    yield return Walk(start);
                    yield return Film(1);
                    Debug.Log($"REST FILM {name}: {game.Motion.Rests} stops");
                }
                Debug.Log($"REST FILM frames {frame};{log}");
            }
            finally { Time.captureFramerate = 0; game.CameraRig.Spotlight = null; Object.DestroyImmediate(tex); rt.Release(); COgheProductUI.LayoutAreaForTests = null; ui.RelayoutForTests(); EndShop(); }
        }
    }
}
