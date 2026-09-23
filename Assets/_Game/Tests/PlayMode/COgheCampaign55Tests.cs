using System;
using System.Collections;
using System.Collections.Generic;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class COgheCampaign55Tests
    {
        const float Dt = 1f / 120;
        VenomCampaign game;
        COgheAssemblyBridge bridge;
        SimulationMode previousMode;
        bool previousPersistence;
        float previousTime;
        int height = 1280;
        [UnitySetUp] public IEnumerator Before()
        {
            previousMode = Physics.simulationMode; previousPersistence = VenomCampaignSave.PersistenceEnabled; previousTime = Time.timeScale;
            Physics.simulationMode = SimulationMode.Script; VenomCampaignSave.PersistenceEnabled = false; Time.timeScale = 1;
            yield return null;
        }
        [UnityTearDown] public IEnumerator After()
        {
            Physics.simulationMode = previousMode; VenomCampaignSave.PersistenceEnabled = previousPersistence; Time.timeScale = previousTime;
            yield return null;
        }
        IEnumerator Load(int slot)
        {
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                game = Object.FindFirstObjectByType<VenomCampaign>();
                game.AutoAdvance = false; game.Owner.enabled = false; game.Owner.Rotation.enabled = false;
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync($"COgheOrigin{slot:00}"); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            bridge = Object.FindFirstObjectByType<COgheAssemblyBridge>(); Steps(120); Frame();
        }
        void Steps(int count)
        { for (int i = 0; i < count; i++) { game.Owner.Step(Dt); game.Owner.Rotation.Step(Dt); if (!game.Owner.Paused) Physics.Simulate(Dt); } }
        void Frame() => game.CameraRig.Frame(720, height, 0, true);
        void Tap(Vector3 point) { Frame(); game.TouchPoint(game.Owner.View.WorldToScreenPoint(point)); }
        string State => $"slot={game.Definition.Order}, centre={game.Motion.Centre(0):F4}, attached={game.Attached}, loss={game.Failure}, picked={game.Feedback.CommandSurface?.name}";
        IEnumerator Wait(float seconds, Func<bool> done, string reason, Action refresh = null)
        {
            for (int i = 0; i < seconds / Dt && !done() && !game.Owner.Lost; i++)
            { if (i % 120 == 0) refresh?.Invoke(); Steps(1); if (i % 240 == 0) yield return null; }
            if (!done()) COgheExpansionIntegrationTests.Capture(game, "campaign55-failed-" + TestContext.CurrentContext.Test.Name);
            Assert.IsTrue(done(), reason + "; " + State);
        }
        IEnumerator Move(Vector3 point)
        {
            game.Motion.Move(0, game.Root.TransformPoint(point));
            yield return Wait(20, () => Vector3.Distance(game.Motion.Centre(0), game.Root.TransformPoint(point)) < .035f, "Walk/climb " + point);
        }
        IEnumerator Seat(int index, bool reverse = false)
        {
            var rail = bridge.Rails[index]; var prop = rail.GetComponent<VenomMovableProp>();
            Tap(prop.ManipulationGrip.position);
            yield return Wait(20, () => game.Attached, "Grasp " + prop.name);
            Vector3 target = rail.Start + rail.Axis * (reverse ? -.05f : rail.Travel + .05f); target.y = -.10f;
            Action command = () => Tap(game.Root.TransformPoint(target)); command();
            yield return Wait(20, () => reverse ? rail.Position < .015f : rail.AtEnd && rail.Latched,
                (reverse ? "Unseat " : "Seat ") + index + " position=" + rail.Position, command);
            game.ReleaseProp(); Steps(60);
        }
        IEnumerator Solution(int slot)
        {
            yield return Load(slot); Assert.NotNull(bridge); Assert.IsFalse(bridge.Ready);
            // A different assembly order remains a valid physical solution.
            for (int i = bridge.Rails.Length - 1; i >= 0; i--) yield return Seat(i);
            Assert.IsTrue(bridge.Ready); Assert.AreEqual(bridge.Rails.Length, bridge.SeatedCount);
            COgheExpansionIntegrationTests.Capture(game, $"campaign55-{slot}-assembled");
            foreach (var point in new[] { new Vector3(-.255f, -.278f, -.24f), new Vector3(-.255f, -.19f, -.202f),
                new Vector3(-.255f, -.078f, -.12f), new Vector3(-.255f, -.078f, 0) }) yield return Move(point);
            Tap(game.Owner.Outlet.position);
            yield return Wait(35, () => game.Owner.Completed, "Traverse the actual decks to the final aperture");
            Assert.AreEqual(32, game.Matter.EscapedCount); Assert.AreEqual(1, game.Matter.TotalFragmentCount); Assert.IsFalse(game.Owner.Lost);
            COgheExpansionIntegrationTests.Capture(game, $"campaign55-{slot}-won");
            game.ResetLevel(); Steps(120); Assert.IsFalse(bridge.Ready); Assert.AreEqual(0, game.Matter.EscapedCount);
            foreach (var rail in bridge.Rails) { Assert.Less(rail.Position, .004f); Assert.IsFalse(rail.Latched); }
        }
        [UnityTest] public IEnumerator Assembly51TwoWideBlocks() { yield return Solution(51); }
        [UnityTest] public IEnumerator Assembly52StaggeredThreeDecks() { yield return Solution(52); }
        [UnityTest] public IEnumerator Assembly53PullsThreeBlocksBack() { yield return Solution(53); }
        [UnityTest] public IEnumerator Assembly54CombinesPushAndPull() { yield return Solution(54); }
        [UnityTest] public IEnumerator Assembly55FourStaggeredBlocks() { height = 1612; yield return Solution(55); }

        [UnityTest] public IEnumerator EmptyGapsCannotBeBypassedByTappingExit()
        {
            for (int slot = 51; slot <= 55; slot++)
            {
                yield return Load(slot); Tap(game.Owner.Outlet.position); Steps(3600);
                Assert.IsFalse(game.Owner.Completed, State); Assert.AreEqual(0, game.Matter.EscapedCount, State);
                Assert.IsFalse(game.Owner.Lost, State); Assert.IsFalse(bridge.Ready);
            }
        }
        [UnityTest] public IEnumerator ReversePauseAndRetryRecoverThePhysicalBridge()
        {
            yield return Load(54); yield return Seat(0);
            game.Owner.TogglePause(); float position = bridge.Rails[0].Position; Steps(600);
            Assert.AreEqual(position, bridge.Rails[0].Position); game.Owner.TogglePause();
            yield return Seat(0, true); Assert.IsFalse(bridge.Rails[0].Latched); Assert.IsFalse(bridge.Ready);
            yield return Seat(0); game.ResetLevel(); Steps(120);
            Assert.AreEqual(0, bridge.SeatedCount); Assert.AreEqual(1, game.Matter.TotalFragmentCount);
            yield return Seat(1); Assert.IsTrue(bridge.Rails[1].Latched);
        }
        [UnityTest] public IEnumerator IntegratedCatalogUsesStableIdsAndRealNavigation()
        {
            var ids = new HashSet<string>();
            for (int slot = 41; slot <= 55; slot++)
            {
                yield return Load(slot); Assert.AreEqual(slot, game.Definition.Order);
                Assert.AreEqual(slot <= 50 ? $"coghe.tap.v1.{slot - 40:00}" : $"venom.origin.{slot:00}", game.Definition.Id);
                Assert.IsTrue(ids.Add(game.Definition.Id)); Assert.AreEqual(55, game.PlayableLevelCount);
                Assert.IsEmpty(game.Definition.SceneSequence); Assert.AreEqual(slot == 50, game.Definition.Boss);
                Assert.IsFalse(game.Definition.CanRotate); Assert.AreEqual((slot - 1) / 10, game.LevelPage);
                if (slot == 50) Assert.IsEmpty(game.GetComponent<COgheTapLesson>().Hint);
            }
            foreach (int slot in new[] { 40, 41, 50, 51, 55, 1 })
            {
                game.Load(slot); yield return null;
                game = Object.FindFirstObjectByType<VenomCampaign>(); game.AutoAdvance = false; game.Owner.enabled = false; game.Owner.Rotation.enabled = false;
                Assert.AreEqual($"COgheOrigin{slot:00}", SceneManager.GetActiveScene().name);
            }
            game.Load(56); yield return null; Assert.AreEqual("COgheOrigin01", SceneManager.GetActiveScene().name);
        }
        [UnityTest] public IEnumerator EveryAssemblyHandleAcceptsBothPortraitSizes()
        {
            for (int slot = 51; slot <= 55; slot++) foreach (int viewport in new[] { 1280, 1612 })
            {
                yield return Load(slot); height = viewport;
                var old = game.Owner.View.targetTexture; var texture = new RenderTexture(720, height, 24); game.Owner.View.targetTexture = texture;
                try
                {
                    foreach (var rail in bridge.Rails)
                    {
                        var prop = rail.GetComponent<VenomMovableProp>(); Frame(); var screen = game.Owner.View.WorldToScreenPoint(prop.ManipulationGrip.position);
                        Assert.IsTrue(game.CameraRig.UsableRect(720, height, new Rect(0, 0, 720, height)).Contains(screen), prop.name);
                        Tap(prop.ManipulationGrip.position);
                        yield return Wait(20, () => game.Attached, "Screen tap approaches handle " + prop.name);
                        Assert.That(Vector3.Distance(game.PropContact, prop.ManipulationGrip.position), Is.LessThan(.04f), prop.name);
                        game.ReleaseProp();
                    }
                    COgheExpansionIntegrationTests.Capture(game, $"campaign55-{slot}-start-{height}", 720, height);
                }
                finally { game.Owner.View.targetTexture = old; Object.DestroyImmediate(texture); }
            }
        }
        [UnityTest] public IEnumerator ExistingCompletionSurvivesTapReorderingWithoutDuplicateRewards()
        {
            yield return Load(41);
            var save=JsonUtility.FromJson<VenomCampaignSave>("{\"Version\":2,\"Completed\":[\"venom.origin.01\",\"venom.origin.40\",\"coghe.tap.v1.01\"],\"HomeUnlocked\":true,\"RevealHome\":false}");
            save.Win(game.Definition);
            Assert.AreEqual(3,save.Completed.Count);Assert.IsTrue(save.HomeUnlocked);Assert.IsFalse(save.RevealHome);
            yield return Load(51);save.Win(game.Definition);
            var restored=JsonUtility.FromJson<VenomCampaignSave>(JsonUtility.ToJson(save));
            Assert.AreEqual(2,restored.Version);Assert.AreEqual(4,restored.Completed.Count);
            Assert.Contains("venom.origin.01",restored.Completed);Assert.Contains("venom.origin.40",restored.Completed);
            Assert.Contains("coghe.tap.v1.01",restored.Completed);Assert.Contains("venom.origin.51",restored.Completed);
            Assert.IsTrue(restored.HomeUnlocked);Assert.IsFalse(restored.RevealHome);
        }
    }
}
