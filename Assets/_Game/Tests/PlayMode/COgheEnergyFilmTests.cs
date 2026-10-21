using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        // Review film for Mrk (run on request): an exhausted COghe in level 2, the win's "Hungry · Feed", Home on the Food
        // tray, a toy turned down, then the five snacks with their reactions as the energy bar fills. 720×1280, 30 fps, UI
        // included, frames in Artifacts/Energy/frames.
        [UnityTest, Explicit, Timeout(900000)] public IEnumerator EnergyFilm()
        {
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true, Drops = 300, InkGuideStep = 4, EnergyUsed = 80 });
            COgheAds.Provider = new CountingAds(); COgheStyle.ResetForTests(new COgheStyle());
            COgheHomeRoom.UnlockedLevelOverride = 32; game.Progress.HomeUnlocked = true;
            string dir = "Artifacts/Energy/frames"; if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true); System.IO.Directory.CreateDirectory(dir);
            int frame = 0; var rt = new RenderTexture(720, 1280, 24) { antiAliasing = 4 }; var tex = new Texture2D(720, 1280, TextureFormat.RGB24, false);
            Time.captureFramerate = 30;
            float paced = Time.realtimeSinceStartup;
            int pants = 0, sighs = 0; var sounds = new System.Text.StringBuilder();
            IEnumerator Film(float seconds, System.Func<bool> until = null)
            {
                for (int guard = 0; guard < seconds * 30 + 2 && (until == null || !until()); guard++)
                {
                    while (Time.realtimeSinceStartup - paced < 1f / 30) { }
                    paced = Time.realtimeSinceStartup;
                    yield return null;   // (WaitForEndOfFrame never resumes in batch mode)
                    var canvas = ProductCanvas; var camera = game.Owner.View;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = camera.nearClipPlane + .01f;
                    game.CameraRig.Frame(720, 1280, 1f / 30); camera.targetTexture = rt; camera.Render(); camera.targetTexture = null;
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    var prev = RenderTexture.active; RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); tex.Apply(); RenderTexture.active = prev;
                    System.IO.File.WriteAllBytes($"{dir}/{frame++:0000}.jpg", tex.EncodeToJPG(92));
                    var p = game.Personality;   // the frames of the sounds, for the review clip's audio
                    if (p != null && p.Pants != pants) { pants = p.Pants; sounds.Append($" pant@{frame - 1}"); }
                    if (p != null && p.Sighs != sighs) { sighs = p.Sighs; sounds.Append($" sigh@{frame - 1}"); }
                }
            }
            void Press(string name) => ui.GetComponentsInChildren<Button>().First(b => b.name == name).onClick.Invoke();
            IEnumerator Walk(Vector3 local, float seconds, bool exit = false)
            {
                var to = exit ? game.Owner.Outlet.position - game.Owner.Outlet.forward * .024f : game.Root.TransformPoint(local);
                game.Motion.Move(game.Motion.Selected, to, false, exit);
                yield return Film(seconds, () => exit ? game.Owner.Completed : game.Motion.Get(game.Motion.Selected) == null);
            }
            try
            {
                yield return LoadDisplay(2); yield return null; game.Progress.HomeUnlocked = true;   // the loaded level has its own progress
                COgheProductUI.LayoutAreaForTests = new Rect(0, 0, 720, 1280); ui.RelayoutForTests();
                Debug.Log($"ENERGY FILM level speed x{game.Motion.SpeedScale:F2} energy {COgheEnergy.Value}");
                yield return Film(1.5f);
                yield return Walk(new Vector3(-.12f, -.22f, .10f), 14);   // drooping, heavy-eyed, panting
                yield return Film(5);                                      // stops: worn out, it flops
                yield return Walk(new Vector3(.15f, -.14f, .18f), 14);
                yield return Film(3);
                yield return Walk(default, 14, true); yield return Film(3.5f);   // the win: "Hungry · Feed"
                Debug.Log($"ENERGY FILM level part ends at frame {frame}; sounds{sounds}");
                Press(COgheProductUI.FeedAfterVictoryButton);
                yield return Film(12, () => ui.Page == COgheProductPage.Home && ui.Popup == COgheProductPopup.Food);
                ui.RelayoutForTests(); yield return Film(2.5f);           // Home, the Food tray open
                ui.Resume(); ui.ToggleHomeZoom(); yield return Film(2);
                var room = game.HomeRoom; var toy = room.Items.FirstOrDefault(i => i.Id == "BALL" && room.Present(i));
                if (toy != null) { game.Personality.PlayWith(toy); yield return Film(3.4f); }   // a toy? food first
                foreach (var snack in COgheSnacks.Catalog)
                {
                    ui.ShowPopup(COgheProductPopup.Food); yield return Film(1.1f);
                    Press(COgheProductUI.SnackButton(snack.Id)); yield return Film(1, () => game.Personality.Snack != null);
                    yield return Film(6, () => game.Personality.Snack == null); yield return Film(.6f);
                }
                yield return Film(2);
                Debug.Log($"ENERGY FILM frames {frame}, energy {COgheEnergy.Value}");
            }
            finally { Time.captureFramerate = 0; Object.DestroyImmediate(tex); rt.Release(); COgheProductUI.LayoutAreaForTests = null; ui.RelayoutForTests(); EndShop(); }
        }
    }
}
