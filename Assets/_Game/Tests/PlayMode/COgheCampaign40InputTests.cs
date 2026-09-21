using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    /// <summary>
    /// Screen-picking contract for the untouched opening state. Every visible
    /// handle/pad must accept its centre tap from overview or its authored room
    /// view, at both portrait ratios. Intentionally covered controls are checked
    /// as covered. This does not substitute for the physical solution suites.
    /// </summary>
    public sealed class COgheCampaign40InputTests
    {
        private static readonly FieldInfo Approach = typeof(VenomCampaign).GetField("approachProp", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo Held = typeof(VenomCampaign).GetField("heldProp", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly int[] HandleCounts = { 2, 2, 2, 3, 2, 2, 2, 3, 1, 4 };
        private static readonly int[] PadCounts = { 0, 0, 0, 0, 1, 2, 1, 2, 2, 2 };
        private SimulationMode previousMode;
        private bool previousPersistence;
        private float previousTimeScale;
        private VenomCampaign game;

        [UnitySetUp] public IEnumerator Before()
        {
            previousMode = Physics.simulationMode;
            previousPersistence = VenomCampaignSave.PersistenceEnabled;
            previousTimeScale = Time.timeScale;
            Physics.simulationMode = SimulationMode.Script;
            VenomCampaignSave.PersistenceEnabled = false;
            Assert.NotNull(Approach); Assert.NotNull(Held);
            yield return null;
        }
        [UnityTearDown] public IEnumerator After()
        {
            Physics.simulationMode = previousMode;
            VenomCampaignSave.PersistenceEnabled = previousPersistence;
            Time.timeScale = previousTimeScale;
            yield return null;
        }
        private IEnumerator Load(int slot)
        {
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                game = Object.FindFirstObjectByType<VenomCampaign>();
                if (game == null) return;
                game.AutoAdvance = false; game.Owner.enabled = false; game.Owner.Rotation.enabled = false;
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync($"COgheOrigin{slot:00}"); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            Assert.NotNull(game);
            yield return null;
            Physics.SyncTransforms();
        }

        private List<int> ViewsFor(Vector3 point)
        {
            var views = new List<int> { -1 };
            Vector3 local = game.Root.InverseTransformPoint(point);
            int nearest = -1; float distance = float.PositiveInfinity;
            for (int zone = 0; zone < game.CameraRig.ZoneCount; zone++)
            {
                Bounds bounds = game.Definition.CameraZones[zone].LocalBounds;
                if (bounds.Contains(local)) views.Add(zone);
                float d = (bounds.ClosestPoint(local) - local).sqrMagnitude;
                if (d < distance) { distance = d; nearest = zone; }
            }
            if (views.Count == 1 && nearest >= 0) views.Add(nearest);
            return views;
        }

        private bool TapAt(Vector3 point, int zone, int height, out string diagnostic)
        {
            game.ReleaseProp(); game.Motion.StopAll(); game.Feedback.ResetFeedback();
            game.CameraRig.SelectZone(zone);
            var safeArea = new Rect(0, 24, 720, height - 52);
            game.CameraRig.Frame(720, height, 0, true, safeArea);
            Vector3 screen = game.Owner.View.WorldToScreenPoint(point);
            bool usable = screen.z > 0 && game.CameraRig.UsableRect(720, height, safeArea).Contains(screen) &&
                game.CameraRig.AllowsPointer(screen, 720, height);
            if (usable) game.TouchPoint(screen);
            var approach = Approach.GetValue(game) as VenomMovableProp;
            var held = Held.GetValue(game) as VenomMovableProp;
            diagnostic = $"view={zone}, screen={screen:F1}, usable={usable}, approach={approach?.name ?? "none"}, held={held?.name ?? "none"}, face={game.Feedback.CommandSurface?.name ?? "none"}";
            return usable;
        }

        private static bool CoveredHandle(int slot, VenomMovableProp prop) =>
            (slot == 31 && prop.name == "P retracting pin") || (slot == 40 && prop.name == "P preparation pin");
        private static bool CoveredPad(int slot, COgheTissueSensor pad) => slot == 36 && pad.name.StartsWith("B ");

        private IEnumerator Verify(int slot)
        {
            yield return Load(slot);
            var handles = Array.FindAll(game.Props, p => p.Manipulable);
            var pads = new List<COgheTissueSensor>();
            var rails = new List<COgheRailSlider>();
            foreach (var mechanism in game.Mechanisms)
            {
                if (mechanism is COgheTissueSensor pad) pads.Add(pad);
                if (mechanism is COgheRailSlider rail) rails.Add(rail);
            }
            Assert.AreEqual(HandleCounts[slot - 31], handles.Length, "No authored handle is silently excluded from picking coverage");
            Assert.AreEqual(PadCounts[slot - 31], pads.Count, "No authored pressure pad is silently excluded from picking coverage");
            var bodyPositions = new Vector3[32];
            for (int i = 0; i < 32; i++) bodyPositions[i] = game.Matter.Bodies[i].position;
            var railPositions = new float[rails.Count];
            for (int i = 0; i < rails.Count; i++) railPositions[i] = rails[i].Position;
            int fragments = game.Matter.TotalFragmentCount;
            bool exit = game.FinalExitAvailable;
            var failures = new List<string>();
            var camera = game.Owner.View; var previousTarget = camera.targetTexture;

            foreach (int height in new[] { 1280, 1612 })
            {
                // Screen coordinates must use the real requested portrait
                // pixel size, rather than the Editor's arbitrary Game view.
                var target = new RenderTexture(720, height, 24);
                camera.targetTexture = target;
                try
                {
                    foreach (var prop in handles)
                    {
                        if (prop.ManipulationGrip == null) { failures.Add($"{prop.name}: missing visible handle"); continue; }
                        bool selected = false; var diagnostics = new List<string>();
                        foreach (int zone in ViewsFor(prop.ManipulationGrip.position))
                        {
                            bool usable = TapAt(prop.ManipulationGrip.position, zone, height, out string detail);
                            bool hit = usable && (ReferenceEquals(Approach.GetValue(game), prop) || ReferenceEquals(Held.GetValue(game), prop));
                            diagnostics.Add(detail); selected |= hit;
                            // Deliberately covered P must remain blocked in
                            // every available view; camera inspection is not a
                            // physical access bypass.
                            if (hit && !CoveredHandle(slot, prop)) break;
                        }
                        if (CoveredHandle(slot, prop) ? selected : !selected)
                            failures.Add($"{height}px {prop.name}: expected {(CoveredHandle(slot, prop) ? "covered initially" : "one centre tap selects the actual prop")}; {string.Join(" | ", diagnostics)}");
                    }
                    foreach (var pad in pads)
                    {
                        var surface = Array.Find(game.Surfaces, s => s.name.EndsWith("sensing surface") && Vector3.Distance(s.transform.position, pad.transform.position) < .001f);
                        Assert.NotNull(surface, pad.name + " has a real sensing surface");
                        bool selected = false; var diagnostics = new List<string>();
                        foreach (int zone in ViewsFor(pad.transform.position))
                        {
                            bool usable = TapAt(pad.transform.position, zone, height, out string detail);
                            var order = game.Motion.Get(game.Motion.Selected);
                            Vector3 expected = game.Root.InverseTransformPoint(pad.transform.position + pad.transform.up * .019f);
                            bool hit = usable && game.Feedback.CommandSurface == surface && order != null && Vector3.Distance(order.Target, expected) < .004f;
                            diagnostics.Add(detail + $", target={order?.Target}"); selected |= hit;
                            if (hit && !CoveredPad(slot, pad)) break;
                        }
                        if (CoveredPad(slot, pad) ? selected : !selected)
                            failures.Add($"{height}px {pad.name}: expected {(CoveredPad(slot, pad) ? "blocked by solid L cover" : "tap commands the pad centre")}; {string.Join(" | ", diagnostics)}");
                    }
                    if (failures.Count > 0)
                    {
                        game.CameraRig.SelectZone(-1);
                        COgheExpansionIntegrationTests.Capture(game, $"campaign40-{slot:00}-input-{height}-failed", 720, height);
                    }
                }
                finally { camera.targetTexture = previousTarget; Object.DestroyImmediate(target); }
            }
            game.ReleaseProp(); game.Motion.StopAll();
            for (int i = 0; i < 32; i++) Assert.AreEqual(bodyPositions[i], game.Matter.Bodies[i].position, "Picking must not move tissue without simulation");
            for (int i = 0; i < rails.Count; i++) Assert.AreEqual(railPositions[i], rails[i].Position, rails[i].name + " was not moved to make it pickable");
            Assert.AreEqual(fragments, game.Matter.TotalFragmentCount); Assert.AreEqual(exit, game.FinalExitAvailable);
            Assert.IsEmpty(failures, $"Level {slot} screen-input contract:\n" + string.Join("\n", failures));
        }

        [UnityTest] public IEnumerator Level31VisibleControlsAcceptScreenTaps() { yield return Verify(31); }
        [UnityTest] public IEnumerator Level32VisibleControlsAcceptScreenTaps() { yield return Verify(32); }
        [UnityTest] public IEnumerator Level33VisibleControlsAcceptScreenTaps() { yield return Verify(33); }
        [UnityTest] public IEnumerator Level34VisibleControlsAcceptScreenTaps() { yield return Verify(34); }
        [UnityTest] public IEnumerator Level35VisibleControlsAcceptScreenTaps() { yield return Verify(35); }
        [UnityTest] public IEnumerator Level36VisibleControlsAcceptScreenTaps() { yield return Verify(36); }
        [UnityTest] public IEnumerator Level37VisibleControlsAcceptScreenTaps() { yield return Verify(37); }
        [UnityTest] public IEnumerator Level38VisibleControlsAcceptScreenTaps() { yield return Verify(38); }
        [UnityTest] public IEnumerator Level39VisibleControlsAcceptScreenTaps() { yield return Verify(39); }
        [UnityTest] public IEnumerator Level40VisibleControlsAcceptScreenTaps() { yield return Verify(40); }
    }
}
