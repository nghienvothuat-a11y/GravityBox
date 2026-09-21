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
    // Every world interaction goes through the production screen-ray picker.
    // Fragment selection, release and camera zones use the same methods as HUD buttons.
    public sealed class COgheBoss30ScreenTests
    {
        private const float Dt = 1f / 120;
        private SimulationMode previousMode;
        private VenomCampaign game;
        private COgheCooperativeWinch machine;
        private int width = 480, height = 800, zone;

        [SetUp] public void Before()
        { previousMode = Physics.simulationMode; Physics.simulationMode = SimulationMode.Script; VenomCampaignSave.PersistenceEnabled = false; }
        [TearDown] public void After()
        { Physics.simulationMode = previousMode; VenomCampaignSave.PersistenceEnabled = true; Time.timeScale = 1; }

        private IEnumerator Load()
        {
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                game = Object.FindFirstObjectByType<VenomCampaign>();
                game.AutoAdvance = false; game.Owner.enabled = false; game.Owner.Rotation.enabled = false;
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync("COgheOrigin30"); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            Assert.NotNull(game);
            machine = game.Owner.Apparatus.GetComponentInChildren<COgheCooperativeWinch>();
            yield return null;
            for (int i = 0; i < 120; i++) Tick();
        }

        private void Tick() { game.Owner.Step(Dt); game.Owner.Rotation.Step(Dt); Physics.Simulate(Dt); }
        private void View(int room) { zone = room; game.CameraRig.SelectZone(room); game.CameraRig.Frame(width, height, 0, true); }
        private void Tap(Vector3 world)
        {
            View(zone);
            Vector3 screen = game.Owner.View.WorldToScreenPoint(world);
            Assert.Greater(screen.z, 0, "Target is in front of camera");
            game.TouchPoint(screen);
            Debug.Log($"BOSS30_TAP world={game.Root.InverseTransformPoint(world):F3} pixel={screen:F1} picked={game.Feedback.CommandSurface?.name} target={game.Feedback.CommandPoint:F3}");
        }
        private List<int> Anchors()
        {
            var result = new List<int>(); var groups = new HashSet<int>();
            for (int i = 0; i < 32; i++) if (!game.Matter.Escaped[i] && groups.Add(game.Matter.Groups[i])) result.Add(i);
            return result;
        }
        private string State()
        {
            var parts = new List<string>();
            foreach (int a in Anchors()) parts.Add($"{a}@{game.Root.InverseTransformPoint(game.Motion.Centre(a)):F3}");
            return $"parts={string.Join(";", parts)} A={machine.Input.Active} B={machine.Output.Active} G={machine.GearCarriage.Position:F4} C={machine.Handle.Position:F4} H={machine.FinalCap.Position:F4} attached={game.Attached} picked={game.Feedback.CommandSurface?.name} target={game.Feedback.CommandPoint:F3} activity={game.Activity} failure={game.Failure}";
        }
        private IEnumerator Wait(float seconds, Func<bool> condition, string reason, Action refresh = null)
        {
            Debug.Log("BOSS30_SCREEN_START " + reason);
            for (int i = 0; i < Mathf.CeilToInt(seconds / Dt) && !condition() && !game.Owner.Lost; i++)
            { if (i % 120 == 0) refresh?.Invoke(); Tick(); if (i % 240 == 0) yield return null; }
            if (!condition()) COgheExpansionIntegrationTests.Capture(game, "30-screen-failed");
            Assert.IsTrue(condition(), reason + "; " + State());
            Debug.Log("BOSS30_SCREEN_PASS " + reason + "; " + State());
        }
        private IEnumerator Floor(int anchor, Vector3 local, int room)
        {
            game.SelectFragment(anchor); View(room);
            Vector3 aim = local; aim.y = -.3f; Tap(game.Root.TransformPoint(aim));
            yield return Wait(35, () => Vector3.Distance(game.Motion.Centre(anchor), game.Root.TransformPoint(local)) < .04f, "Walk via floor tap " + local);
        }
        private IEnumerator Pad(int anchor, COgheTissueSensor pad, int room)
        {
            game.SelectFragment(anchor); View(room); Tap(pad.transform.position);
            yield return Wait(25, () => pad.Active, "Hold " + pad.name);
        }
        private IEnumerator Pipe(int anchor, COgheTubeNetwork pipe, int room)
        {
            game.SelectFragment(anchor); View(room); Tap(game.Root.TransformPoint(pipe.Nodes[0].LocalPosition));
            yield return Wait(35, () => pipe.LastReachedNode == 1 && !pipe.IsParticleInside(anchor), "Transfer through " + pipe.name);
            // The pedestal occludes the floor immediately under its lip.
            // The next visible handle is a supported, reachable destination.
        }
        private IEnumerator Pull(int anchor, COgheRailSlider rail, int room, Func<bool> finished)
        {
            game.SelectFragment(anchor); View(room); Tap(rail.GetComponent<VenomMovableProp>().ManipulationGrip.position);
            yield return Wait(30, () => game.Attached, "Grasp " + rail.name);
            Vector3 target = rail.Frame.TransformPoint(rail.Start + rail.Axis * (rail.Travel + .12f));
            Tap(target);
            yield return Wait(30, finished, "Pull " + rail.name, () => Tap(target));
            game.ReleaseProp();
        }

        [UnityTest] public IEnumerator Campaign30WholeSolutionUsesScreenCommands()
        { yield return FullSolution(480, 800); }

        [UnityTest] public IEnumerator Boss30TallPortraitWholeSolutionUsesScreenCommands()
        { yield return FullSolution(720, 1612); }

        private IEnumerator FullSolution(int viewportWidth, int viewportHeight)
        {
            width = viewportWidth; height = viewportHeight;
            yield return Load();
            var knives = game.Owner.Apparatus.GetComponentsInChildren<COgheGuillotine>();
            Array.Sort(knives, (a, b) => a.Sensor.position.x.CompareTo(b.Sensor.position.x));
            var pipes = game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>();
            Array.Sort(pipes, (a, b) => a.Nodes[0].LocalPosition.x.CompareTo(b.Nodes[0].LocalPosition.x));
            View(0); Tap(knives[0].Rail.Body.position);
            yield return Wait(20, () => game.Matter.CutCount > 0, "First blade tap cuts real tissue");
            for (int i = 0; i < 60; i++) Tick();
            var parts = Anchors(); Assert.AreEqual(2, parts.Count);
            parts.Sort((a, b) => game.Motion.Centre(a).x.CompareTo(game.Motion.Centre(b).x));
            int holdA = parts[0], worker = parts[1];
            yield return Pad(holdA, machine.Input, 0);
            yield return Pipe(worker, pipes[0], 0);
            yield return Pull(worker, machine.GearCarriage, 1, () => machine.GearCarriage.AtEnd);
            int group = game.Matter.Groups[worker]; var workerParticles = new HashSet<int>();
            for (int i = 0; i < 32; i++) if (game.Matter.Groups[i] == group) workerParticles.Add(i);
            int cuts = game.Matter.CutCount;
            View(1); Tap(knives[1].Rail.Body.position);
            yield return Wait(25, () => game.Matter.CutCount > cuts && game.Matter.TotalFragmentCount == 3, "Second blade tap makes B and C operators");
            for (int i = 0; i < 60; i++) Tick();
            var operators = new List<int>(); foreach (int a in Anchors()) if (workerParticles.Contains(a)) operators.Add(a);
            operators.Sort((a, b) => game.Motion.Centre(a).x.CompareTo(game.Motion.Centre(b).x));
            Assert.AreEqual(2, operators.Count); int holdB = operators[0], pullC = operators[1];
            yield return Floor(pullC, new Vector3(.13f, -.274f, -.24f), 1);
            yield return Pipe(pullC, pipes[1], 1);
            yield return Pad(holdB, machine.Output, 1);
            Assert.AreEqual(3, game.Matter.TotalFragmentCount);
            yield return Pull(pullC, machine.Handle, 2, () => machine.Complete);
            Assert.IsFalse(machine.ExitUnlocked);
            foreach (int anchor in Anchors())
            {
                game.SelectFragment(anchor); View(2); Tap(game.Root.TransformPoint(new Vector3(.42f, -.3f, .12f)));
            }
            yield return Wait(40, () => game.Matter.TotalFragmentCount == 1, "Reunite all three through the opened doors");
            yield return Pull(Anchors()[0], machine.FinalCap, 2, () => machine.FinalCap.AtEnd);
            COgheExpansionIntegrationTests.Capture(game, "30-screen-before-exit");
            View(2); Tap(game.Owner.Outlet.position);
            yield return Wait(40, () => game.Owner.Completed, "Exit with all 32 particles using the visible aperture");
            Assert.AreEqual(32, game.Matter.EscapedCount);
            Assert.IsFalse(game.Owner.Lost);
            COgheExpansionIntegrationTests.Capture(game, "30-screen-completed");
            game.ResetLevel(); for (int i = 0; i < 120; i++) Tick();
            Assert.IsFalse(game.Owner.Completed); Assert.IsFalse(machine.Complete);
            Assert.AreEqual(0, game.Matter.EscapedCount); Assert.AreEqual(1, game.Matter.TotalFragmentCount);
            Assert.IsTrue(machine.FinalCap.Locked);
        }
    }
}
