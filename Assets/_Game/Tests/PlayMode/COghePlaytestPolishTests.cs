using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        [UnityTest] public IEnumerator VictoryRewardOfferWaitsAndSurvivesCancelledAd()
        {
            try
            {
                FreshShop(false); COgheEconomy.Set("victory_triple_ms", 1000);
                var ads = new CountingAds { Earn = false }; COgheAds.Provider = ads;
                ui.ShowVictoryForTests(game.Definition.Id, 1); yield return null;
                yield return new WaitForSecondsRealtime(6);
                Assert.IsTrue(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Triple drops"));
                int balance = COgheShop.Drops;
                yield return Click("Triple drops"); yield return null;
                Assert.AreEqual(balance, COgheShop.Drops);
                Assert.IsTrue(ui.GetComponentsInChildren<Button>().Any(b => b.name == "Triple drops"),
                    "Cancelling an ad must not erase the offer");
                ads.Earn = true; yield return Click("Triple drops"); yield return null;
                Assert.Greater(COgheShop.Drops, balance);
                var claimed = ui.GetComponentsInChildren<Button>().Single(b => b.name == "Triple drops");
                Assert.IsFalse(claimed.interactable, "Keep a disabled receipt until Next");
                int paid = COgheShop.Drops;
                claimed.onClick.Invoke(); yield return null;
                Assert.AreEqual(paid, COgheShop.Drops, "A successfully claimed reward cannot be claimed twice");
            }
            finally { ui.ShowMenu(); EndShop(); }
        }

        [Test] public void MintGiftMigrationPreservesOceanAndFinishedLessons()
        {
            FreshShop(false);
            COgheShop.MigrateInkGift(false);
            Assert.IsTrue(COgheShop.Owns("INK_MINT")); Assert.IsFalse(COgheShop.Owns("INK_OCEAN"));
            COgheShop.ResetForTests(new COgheShop.State { InkGuideStep = 4 }); COgheShop.TestsOwnUnlocked = false;
            COgheShop.MigrateInkGift(true);
            Assert.IsTrue(COgheShop.Owns("INK_OCEAN")); Assert.AreEqual(4, COgheShop.Current.InkGuideStep);
            int count = COgheShop.Current.Owned.Count;
            COgheShop.MigrateInkGift(true); Assert.AreEqual(count, COgheShop.Current.Owned.Count);
            EndShop();
        }

        [UnityTest] public IEnumerator AbilityAccentsAreSparseAndNeverBlockTouch()
        {
            COgheShop.Current.Emotion = 1; COgheShop.Current.LastAbility = 0;
            ui.Play(); yield return null;
            var fx = ui.GetComponentInChildren<COgheAbilityFx>();
            Assert.NotNull(fx); Assert.IsFalse(fx.raycastTarget);
            yield return new WaitForSecondsRealtime(3);
            Assert.Greater(fx.LiveCount, 0, "The chosen card is visible soon after entering a level");
            int bursts = fx.Bursts;
            yield return new WaitForSecondsRealtime(4);
            Assert.AreEqual(0, fx.LiveCount); Assert.AreEqual(bursts, fx.Bursts, "No continuous confetti");
            ui.ShowMenu(); yield return null; Assert.AreEqual(0, fx.LiveCount);
            ui.Play(); yield return new WaitForSecondsRealtime(3);
            Assert.AreEqual(bursts, fx.Bursts, "Pausing or returning must not restart a burst every time");
        }

        [UnityTest] public IEnumerator SpeedyAccentsWaitForActualMovementAndSupport()
        {
            COgheShop.Current.Agility = 1; COgheShop.Current.LastAbility = 1;
            ui.Play(); yield return null;
            var fx = ui.GetComponentInChildren<COgheAbilityFx>();
            yield return new WaitForSecondsRealtime(3);
            Assert.AreEqual(0, fx.Bursts, "No fake trail while COghe rests");
            game.Motion.Move(0, game.Root.TransformPoint(new Vector3(.23f, -.3f, .10f)));
            float deadline = Time.realtimeSinceStartup + 3;
            while (fx.LiveCount == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.Greater(fx.LiveCount, 0, "Walking emits from actual support contacts");
            ui.ShowMenu(); yield return null; Assert.AreEqual(0, fx.LiveCount);
        }
    }

    public sealed partial class COgheSpatialCampaignTests
    {
        [UnityTest] public IEnumerator Boss10TrayButtonCannotCallTheLiftFromTheFloor()
        {
            yield return Load(10);
            var lift = game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();
            yield return Tap(lift.Panel.position);
            yield return Until(45, () => lift.Trips == 1 && !lift.Moving, "Ride up");
            yield return Tap(game.Root.TransformPoint(new Vector3(-.3f, -.3f, -.22f)));
            yield return Until(25, () => game.Motion.Centre(0).y < lift.Deck.transform.position.y - .08f, "Walk off to the floor");
            float height = lift.Rail.Position;
            yield return Tap(lift.Panel.position); yield return Wait(.6f);
            Assert.IsFalse(lift.Moving, "No invisible remote button when the rider is below the tray");
            Assert.AreEqual(height, lift.Rail.Position, .008f);
            Assert.AreEqual(1, lift.Trips);
            game.ResetLevel(); yield return Wait(1);
            yield return new GravityBox.Venom.ChapterProof.COgheSpatialScenario(game, Tap, Until).Solve();
            Assert.AreEqual(32, game.Matter.EscapedCount, "Boss 10 still solves with the real passenger sequence");
        }

        [UnityTest] public IEnumerator IntroFinalShotIsTheActualLevelAtPhoneAndTabletAspects()
        {
            yield return Load(1); yield return StartIntro(0);
            var intro = Object.FindFirstObjectByType<COgheIntro>();
            foreach (var size in new[] { new Vector2Int(540, 1170), new Vector2Int(768, 1024) })
            {
                var camera = game.Owner.View; var rt = RenderTexture.GetTemporary(size.x, size.y, 24);
                var tex = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                camera.targetTexture = rt; game.CameraRig.Frame(size.x, size.y, 0, true);
                try
                {
                    camera.Render(); RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); tex.Apply();
                    var before = tex.GetPixels32();
                    intro.Clock = 19.8f;
                    GL.PushMatrix(); GL.LoadPixelMatrix(0, size.x, size.y, 0); intro.Draw(size.x, size.y); GL.PopMatrix();
                    tex.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); tex.Apply();
                    CollectionAssert.AreEqual(before, tex.GetPixels32(), "No painted box, duplicate body or exit at handover");
                    System.IO.Directory.CreateDirectory("Artifacts/COghePlaytestPolish");
                    System.IO.File.WriteAllBytes($"Artifacts/COghePlaytestPolish/intro-live-{size.x}.png", tex.EncodeToPNG());
                }
                finally { camera.targetTexture = null; RenderTexture.active = null; RenderTexture.ReleaseTemporary(rt); Object.Destroy(tex); }
            }
            intro.Skip(); yield return null;
        }
    }
}
