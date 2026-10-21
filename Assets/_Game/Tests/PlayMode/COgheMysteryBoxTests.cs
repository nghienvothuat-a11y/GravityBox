using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    // Mystery gift box (Mrk 21/10/2026): display 3 and every 5 levels after, a box drops near the exit; COghe walks to it,
    // the camera closes in, an ad opens it without saying what is inside. Presentation only: the level still ends normally.
    public partial class COgheProductUITests
    {
        [Test] public void MysteryBoxComesOnLevelThreeThenEveryFifthLevelAndABossPassesItOn()
        {
            bool Boss(int n) => n == 10 || n == 21 || n == 32 || n == 43 || n == 54 || n == 65;
            var levels = Enumerable.Range(1, 67).Where(n => COgheProductUI.BoxScheduled(n, Boss)).ToArray();
            CollectionAssert.AreEqual(new[] { 3, 8, 13, 18, 23, 28, 33, 38, 44, 48, 53, 58, 63 }, levels);
        }

        /// <summary>A 720×1280 still with the UI, for review (Artifacts/MysteryBox).</summary>
        private void BoxSnapshot(string name)
        {
            System.IO.Directory.CreateDirectory("Artifacts/MysteryBox");
            var rt = new RenderTexture(720, 1280, 24) { antiAliasing = 4 }; var tex = new Texture2D(720, 1280, TextureFormat.RGB24, false);
            var canvas = ui.GetComponentInChildren<Canvas>(); var camera = game.Owner.View;
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = camera.nearClipPlane + .01f;
                game.CameraRig.Frame(720, 1280, 1, true); camera.targetTexture = rt; camera.Render();
                var prev = RenderTexture.active; RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); tex.Apply(); RenderTexture.active = prev;
                System.IO.File.WriteAllBytes($"Artifacts/MysteryBox/{name}.jpg", tex.EncodeToJPG(90));
            }
            finally { camera.targetTexture = null; canvas.renderMode = RenderMode.ScreenSpaceOverlay; Object.DestroyImmediate(tex); rt.Release(); }
        }

        private IEnumerator LoadDisplay(int order)
        {
            ui.LoadForTest(order);
            float end = Time.realtimeSinceStartup + 10;
            while (Time.realtimeSinceStartup < end)
            {
                var g = Object.FindFirstObjectByType<VenomCampaign>();
                if (g != null && g.Definition != null && g.Definition.Order == order && g.ProductUI != null) { game = g; ui = g.ProductUI; break; }
                yield return null;
            }
            Assert.AreEqual(order, game.Definition.Order);
        }
        // Level 3's route: round the observation wall, then the floor behind it, where the box waits.
        private IEnumerator WalkToBehindTheWall()
        {
            game.Motion.Move(game.Motion.Selected, game.Root.TransformPoint(new Vector3(-.23f, -.281f, .23f)));
            float end = Time.realtimeSinceStartup + 25;
            while (game.Motion.Get(game.Motion.Selected) != null && Time.realtimeSinceStartup < end) yield return null;
            game.Motion.Move(game.Motion.Selected, game.Root.TransformPoint(new Vector3(.20f, -.281f, .25f)));
            end = Time.realtimeSinceStartup + 25;
            while (ui.BoxPhase == COgheProductUI.MysteryBoxPhase.Waiting && Time.realtimeSinceStartup < end) yield return null;
        }
        private IEnumerator ExitLevel()
        {
            game.Motion.Move(game.Motion.Selected, game.Owner.Outlet.position - game.Owner.Outlet.forward * .024f, false, true);
            float end = Time.realtimeSinceStartup + 40;
            while (!game.Owner.Completed && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsTrue(game.Owner.Completed, "After the box the level still ends at its exit");
        }

        [UnityTest] public IEnumerator LevelThreeBoxOpensWithAnAdAndCOgheWearsTheBeeHat()
        {
            var ads = new CountingAds(); COgheAds.Provider = ads; COgheStyle.ResetForTests(new COgheStyle());
            try
            {
                yield return LoadDisplay(3);
                float end = Time.realtimeSinceStartup + 5;
                while ((ui.MysteryBox == null || !ui.MysteryBox.Landed) && Time.realtimeSinceStartup < end) yield return null;
                Assert.IsNotNull(ui.MysteryBox, "The box drops near the exit");Assert.IsTrue(ui.MysteryBox.Landed);
                Assert.AreEqual(0, ui.MysteryBox.GetComponentsInChildren<Collider>().Length, "Presentation only: no collider");
                Assert.AreEqual(0, ui.MysteryBox.GetComponentsInChildren<Rigidbody>().Length);
                yield return WalkToBehindTheWall();
                end = Time.realtimeSinceStartup + 6;
                while (ui.Popup != COgheProductPopup.MysteryBox && Time.realtimeSinceStartup < end) yield return null;
                Assert.AreEqual(COgheProductPopup.MysteryBox, ui.Popup, "COghe walked to the box and the offer opened");
                Assert.IsTrue(game.CameraRig.Spotlight.HasValue, "The camera closes in on the box");
                Assert.Less(Vector3.Distance(game.Motion.Centre(game.Motion.Selected), ui.MysteryBox.Rest), .12f, "COghe stands by the box");
                Assert.IsFalse(COgheShop.Owns("HAT_BEE"));
                yield return Click("Open mystery box");
                Assert.AreEqual(1, ads.Rewardeds);
                Assert.AreEqual(COgheProductPopup.MysteryReveal, ui.Popup);
                Assert.AreEqual("HAT_BEE", ui.BoxItem, "The first box is the Bee hat");
                Assert.IsTrue(COgheShop.Owns("HAT_BEE")); Assert.AreEqual("HAT_BEE", COgheStyle.Current.Hat, "Worn at once");
                Assert.IsTrue(COgheShop.Paid("box:" + game.Definition.Id));
                yield return new WaitForSecondsRealtime(1.2f);
                Assert.IsTrue(ui.MysteryBox == null, "The box opened and is gone");
                yield return Click("Mystery box continue");
                Assert.AreEqual(COgheProductPopup.None, ui.Popup); Assert.IsFalse(game.CameraRig.Spotlight.HasValue);
                Assert.AreEqual(COgheProductUI.MysteryBoxPhase.Done, ui.BoxPhase);
                yield return ExitLevel();
                Assert.IsFalse(ui.MysteryBoxHere, "Opened once: no second box on this level");
            }
            finally { EndShop(); }
        }

        [UnityTest] public IEnumerator SkippedBoxFliesAwayWithoutAPrize()
        {
            var ads = new CountingAds(); COgheAds.Provider = ads; COgheStyle.ResetForTests(new COgheStyle());
            try
            {
                yield return LoadDisplay(3);
                yield return WalkToBehindTheWall();
                float end = Time.realtimeSinceStartup + 6;
                while (ui.Popup != COgheProductPopup.MysteryBox && Time.realtimeSinceStartup < end) yield return null;
                Assert.AreEqual(COgheProductPopup.MysteryBox, ui.Popup);
                int drops = COgheShop.Drops;
                yield return Click("Skip mystery box");
                Assert.AreEqual(0, ads.Rewardeds); Assert.AreEqual(drops, COgheShop.Drops); Assert.IsFalse(COgheShop.Owns("HAT_BEE"));
                Assert.AreEqual(COgheProductPopup.None, ui.Popup); Assert.IsFalse(game.CameraRig.Spotlight.HasValue);
                yield return new WaitForSecondsRealtime(1.2f);
                Assert.IsTrue(ui.MysteryBox == null, "It flew away");
                yield return ExitLevel();
            }
            finally { EndShop(); }
        }

        // Review film for Mrk (run on request): level 3 at 720×1280, 30 fps, UI included, frames in Artifacts/MysteryBox.
        [UnityTest, Explicit, Timeout(600000)] public IEnumerator MysteryBoxFilm()
        {
            var ads = new CountingAds(); COgheAds.Provider = ads; COgheStyle.ResetForTests(new COgheStyle());
            Screen.SetResolution(720, 1280, FullScreenMode.Windowed); yield return null; yield return null;
            string dir = "Artifacts/MysteryBox/frames"; if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true); System.IO.Directory.CreateDirectory(dir);
            int frame = 0; var rt = new RenderTexture(720, 1280, 24) { antiAliasing = 4 }; var tex = new Texture2D(720, 1280, TextureFormat.RGB24, false);
            Time.captureFramerate = 30;
            IEnumerator Film(float seconds, System.Func<bool> until = null)
            {
                for (int guard = 0; guard < seconds * 30 + 2 && (until == null || !until()); guard++)
                {
                    yield return null;   // (WaitForEndOfFrame never resumes in batch mode)
                    var canvas = ui.GetComponentInChildren<Canvas>(); var camera = game.Owner.View;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = camera.nearClipPlane + .01f;
                    game.CameraRig.Frame(720, 1280, 1f / 30); camera.targetTexture = rt; camera.Render(); camera.targetTexture = null;   // phone framing
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    var prev = RenderTexture.active; RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); tex.Apply(); RenderTexture.active = prev;
                    System.IO.File.WriteAllBytes($"{dir}/{frame++:0000}.png", tex.EncodeToJPG(92));
                }
            }
            try
            {
                yield return LoadDisplay(3); yield return null;
                yield return Film(2.2f);                                                       // the box drops and hops
                game.Motion.Move(game.Motion.Selected, game.Root.TransformPoint(new Vector3(-.23f, -.281f, .23f)));
                yield return Film(14, () => game.Motion.Get(game.Motion.Selected) == null);
                game.Motion.Move(game.Motion.Selected, game.Root.TransformPoint(new Vector3(.20f, -.281f, .25f)));
                yield return Film(14, () => ui.Popup == COgheProductPopup.MysteryBox);       // COghe walks to it, camera closes in
                yield return Film(1.6f);
                ui.OpenMysteryBox(); yield return Film(3.2f);                                  // the lid pops: the Bee hat
                ui.Resume(); typeof(COgheProductUI).GetMethod("EndMysteryBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(ui, null);
                yield return Film(1.5f);
                game.Motion.Move(game.Motion.Selected, game.Owner.Outlet.position - game.Owner.Outlet.forward * .024f, false, true);
                yield return Film(12, () => game.Owner.Completed); yield return Film(2.5f);      // wearing it out of the exit
                Debug.Log("MYSTERY BOX FILM frames " + frame);
            }
            finally { Time.captureFramerate = 0; Object.DestroyImmediate(tex); rt.Release(); EndShop(); }
        }

        // Levels 8 and 13: the box lands on the floor COghe crosses before the exit (routes.txt) and the offer opens there.
        [UnityTest] public IEnumerator BoxesOnLevelsEightAndThirteenLandOnTheRouteAndOpenTheOffer()
        {
            var ads = new CountingAds(); COgheAds.Provider = ads; COgheStyle.ResetForTests(new COgheStyle());
            try
            {
                foreach (var (order, route) in new[] { (8, new Vector3(.282f, -.281f, .259f)), (13, new Vector3(.32f, -.281f, .13f)) })
                {
                    yield return LoadDisplay(order);
                    float end = Time.realtimeSinceStartup + 5;
                    while ((ui.MysteryBox == null || !ui.MysteryBox.Landed) && Time.realtimeSinceStartup < end) yield return null;
                    Assert.IsNotNull(ui.MysteryBox, $"Display {order}: the box drops"); Assert.IsTrue(ui.MysteryBox.Landed);
                    Assert.Less(Mathf.Abs(game.Root.InverseTransformPoint(ui.MysteryBox.Rest).y + .30f), .005f, $"Display {order}: on the floor");
                    game.Motion.Move(game.Motion.Selected, game.Root.TransformPoint(route));
                    end = Time.realtimeSinceStartup + 30;
                    while (ui.Popup != COgheProductPopup.MysteryBox && Time.realtimeSinceStartup < end) yield return null;
                    Assert.AreEqual(COgheProductPopup.MysteryBox, ui.Popup, $"Display {order}: COghe reached the box; centre {game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected)):F3}");
                    Assert.Less(Vector3.Distance(game.Motion.Centre(game.Motion.Selected), ui.MysteryBox.Rest), .14f, $"Display {order}: by the box");
                    yield return new WaitForSecondsRealtime(1.2f); BoxSnapshot($"L{order:00}-offer");
                    yield return Click("Skip mystery box");
                    Assert.AreEqual(COgheProductUI.MysteryBoxPhase.Done, ui.BoxPhase);
                }
            }
            finally { EndShop(); }
        }

        [Test] public void LaterBoxesGiveDropsOrAnItemNotOwnedAndTheSameForALevel()
        {
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true });
            try
            {
                for (int order = 8; order <= 63; order += 5)
                {
                    string id = "coghe.test.box." + order;
                    var a = COgheProductUI.MysteryPrize(id, order); var b = COgheProductUI.MysteryPrize(id, order);
                    Assert.AreEqual(a.ItemId, b.ItemId); Assert.AreEqual(a.Amount, b.Amount);
                    if (a.ItemId == null) CollectionAssert.Contains(new[] { 30, 50, 80 }, a.Amount);
                    else { Assert.IsFalse(COgheEconomy.IsGift(a.ItemId)); Assert.LessOrEqual(COgheEconomy.Find(a.ItemId).Level, order + 10); }
                }
            }
            finally { COgheShop.ResetForTests(); }
        }
    }
}
