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
    /// <summary>Full 39/40 solutions: player commands, real cuts, forces, collision and escape.</summary>
    public sealed class COgheCampaign40BossTests
    {
        private const float Dt = 1f / 120;
        private SimulationMode previousMode;
        private VenomCampaign game;
        private readonly COgheChapterScreenInput input = new COgheChapterScreenInput();
        private COgheTwoStageWinch machine;

        [SetUp] public void Before()
        { input.Width=480;input.Height=800;previousMode = Physics.simulationMode; Physics.simulationMode = SimulationMode.Script; VenomCampaignSave.PersistenceEnabled = false; }
        [TearDown] public void After()
        { Physics.simulationMode = previousMode; VenomCampaignSave.PersistenceEnabled = true; Time.timeScale = 1; }

        private IEnumerator Load(int level)
        {
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                game = Object.FindFirstObjectByType<VenomCampaign>();
                if (game == null) return;
                game.AutoAdvance = false; game.Owner.enabled = false; game.Owner.Rotation.enabled = false;
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync($"COgheOrigin{level:00}"); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            Assert.NotNull(game); machine = game.Owner.Apparatus.GetComponentInChildren<COgheTwoStageWinch>(); Assert.NotNull(machine);
            yield return null;
        }

        private void Tick() { game.Owner.Step(Dt); game.Owner.Rotation.Step(Dt); Physics.Simulate(Dt); }
        private List<int> Anchors()
        {
            var result = new List<int>(); var groups = new HashSet<int>();
            for (int i = 0; i < 32; i++) if (!game.Matter.Escaped[i] && groups.Add(game.Matter.Groups[i])) result.Add(i);
            return result;
        }
        private string State()
        {
            var parts = new List<string>();
            foreach (int anchor in Anchors()) parts.Add($"{anchor}@{game.Root.InverseTransformPoint(game.Motion.Centre(anchor)):F3}");
            var rails = new List<string>();
            foreach (var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>()) rails.Add($"{rail.name}={rail.Position:F4}/{rail.Travel:F3} F={rail.Effort:F3} lock={rail.Locked}");
            return $"failure={game.Failure}; parts={string.Join("; ", parts)}; A={machine.Input.Load:F3}; B={machine.Output.Load:F3}; {string.Join("; ", rails)}";
        }
        private IEnumerator Wait(float seconds, Func<bool> condition, string reason, Action refresh = null)
        {
            for (int i = 0; i < Mathf.CeilToInt(seconds / Dt) && !condition() && !game.Owner.Lost; i++)
            { if (i % 120 == 0) refresh?.Invoke(); Tick(); if (i % 240 == 0) yield return null; }
            if (!condition()) COgheExpansionIntegrationTests.Capture(game, $"{game.Definition.Order:00}-failed-two-stage");
            Assert.IsTrue(condition(), reason + "; " + State());
        }
        private IEnumerator Walk(int anchor, Vector3 local, float tolerance = .035f)
        {
            game.SelectFragment(anchor); Vector3 world = game.Root.TransformPoint(local); input.Tap(game, world - game.Root.up * .026f);
            yield return Wait(35, () => Vector3.Distance(game.Motion.Centre(anchor), world) < tolerance, "Walk to " + local);
        }
        private IEnumerator Grasp(int anchor, COgheRailSlider rail)
        {
            game.SelectFragment(anchor); input.Tap(game, rail.GetComponent<VenomMovableProp>().ManipulationGrip.position);
            yield return Wait(35, () => game.Attached, "Reach and grasp " + rail.name);
        }
        private Vector3 Target(COgheRailSlider rail, float direction) => rail.Frame.TransformPoint(rail.Start + rail.Axis * (direction > 0 ? rail.Travel + .12f : -.12f));
        private IEnumerator Pull(int anchor, COgheRailSlider rail, float direction, Func<bool> finished)
        {
            yield return Grasp(anchor, rail); Vector3 target = Target(rail, direction); input.Tap(game, target, false);
            yield return Wait(35, finished, "Operate " + rail.name, () => input.Tap(game, target, false));
            game.ReleaseProp();
        }
        private IEnumerator Pad(int anchor, COgheTissueSensor pad)
        {
            game.SelectFragment(anchor); input.Tap(game, pad.transform.position);
            yield return Wait(10, () => pad.Active, "Measured tissue load on " + pad.name);
        }
        private IEnumerator Pipe(int anchor, COgheTubeNetwork pipe)
        {
            game.SelectFragment(anchor); input.Tap(game, game.Root.TransformPoint(pipe.Nodes[0].LocalPosition));
            yield return Wait(24, () => pipe.LastReachedNode == 1 && !pipe.IsParticleInside(anchor), "Transfer whole fragment; " + pipe.DebugState(anchor));
            // Next visible target is reachable from the receiving pedestal.
        }

        [UnityTest] public IEnumerator Campaign39TwoHeldOutputsReuniteAndEscape() { yield return FullSolution(39); }
        [UnityTest] public IEnumerator Campaign40PreparesBeforeCutsThenUnlocksAndPullsFinalCap() { yield return FullSolution(40); }

        [UnityTest] public IEnumerator Audit39ReleaseAAfterStageOneThenRecoverTallPortrait() {yield return FullSolution(39,true);}
        [UnityTest] public IEnumerator Audit40ReleaseAAfterStageOneThenRecoverTallPortrait() {yield return FullSolution(40,true);}
        private IEnumerator FullSolution(int level,bool recovery=false)
        {
            if(recovery){input.Width=720;input.Height=1612;}
            yield return Load(level);
            Assert.IsFalse(game.Definition.CanRotate);
            Assert.AreEqual(level == 40, game.Definition.Boss);
            if (level == 40)
            {
                Assert.IsEmpty(game.Definition.Lesson, "Boss contains no solution instructions.");
                Assert.IsFalse(machine.Prepared); Assert.IsFalse(machine.ExitUnlocked);
                game.SelectProp(machine.PreparationPin.GetComponent<VenomMovableProp>());
                for (int i = 0; i < 480 && !game.Attached; i++) Tick();
                Assert.IsFalse(game.Attached, "P cannot be grasped through Q or around the sealed alcove.");
                Assert.Less(machine.PreparationPin.Position, .003f); game.ReleaseProp();
                yield return Pull(0, machine.PreparationCarriage, -1, () => machine.PreparationCarriage.Position < .004f);
                yield return Pull(0, machine.PreparationPin, 1, () => machine.PreparationPin.AtEnd);
                yield return Pull(0, machine.PreparationCarriage, 1, () => machine.Prepared);
                yield return Wait(2, () => machine.PreparationTransmission.Meshed, "Q actually meshes with the output gear at its dock.");
                Assert.AreEqual(1, game.Matter.TotalFragmentCount, "Preparation was performed with the actual complete body.");
                yield return Walk(0, new Vector3(-.55f, -.274f, -.10f));
            }
            var knives = game.Owner.Apparatus.GetComponentsInChildren<COgheGuillotine>();
            Array.Sort(knives, (a, b) => a.Sensor.position.x.CompareTo(b.Sensor.position.x));
            var pipes = game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>();
            Array.Sort(pipes, (a, b) => a.Nodes[0].LocalPosition.x.CompareTo(b.Nodes[0].LocalPosition.x));
            game.SelectFragment(0); input.Tap(game, knives[0].Rail.Body.position);
            yield return Wait(25, () => game.Matter.CutCount > 0 && game.Matter.TotalFragmentCount > 1, "First real blade cut");
            for (int i = 0; i < 60; i++) Tick();
            var parts = Anchors(); Assert.AreEqual(2, parts.Count, State());
            parts.Sort((a, b) => game.Motion.Centre(a).x.CompareTo(game.Motion.Centre(b).x));
            int holdA = parts[0], worker = parts[1];
            yield return Pad(holdA, machine.Input);
            Assert.AreEqual(2, game.Matter.TotalFragmentCount, "A's approach must preserve both actual cut parts; " + State());
            Assert.AreNotEqual(game.Matter.Groups[holdA], game.Matter.Groups[worker], "The holder must not cross and fuse with its waiting worker; " + State());
            yield return Pipe(worker, pipes[0]);
            Assert.AreEqual(2, game.Matter.TotalFragmentCount, "The first transfer preserves the independent A holder; " + State());
            Assert.IsTrue(machine.Input.Active, "A remains physically loaded after the worker enters the central room; " + State());
            var workerParticles = new HashSet<int>(); int workerGroup = game.Matter.Groups[worker];
            for (int i = 0; i < 32; i++) if (game.Matter.Groups[i] == workerGroup) workerParticles.Add(i);
            int cuts = game.Matter.CutCount; game.SelectFragment(worker); input.Tap(game, knives[1].Rail.Body.position);
            yield return Wait(25, () => game.Matter.CutCount > cuts && game.Matter.TotalFragmentCount >= 3, "Second real blade cut");
            for (int i = 0; i < 60; i++) Tick();
            var working = new List<int>(); foreach (int anchor in Anchors()) if (workerParticles.Contains(anchor)) working.Add(anchor);
            working.Sort((a, b) => game.Motion.Centre(a).x.CompareTo(game.Motion.Centre(b).x)); Assert.AreEqual(2, working.Count, State());
            int holdB = working[0], pullC = working[1];
            yield return Pipe(pullC, pipes[1]);
            yield return Pad(holdB, machine.Output);
            Assert.IsTrue(machine.Input.Active, "A remains at its task while other parts are selected.");

            // A wrong-order pull must hit the actual stop and leave every
            // output closed, with a recoverable route back to the I endpoint.
            yield return Grasp(pullC, machine.Handle); Vector3 wrong = Target(machine.Handle, 1); input.Tap(game, wrong, false);
            for (int i = 0; i < 240; i++) { if (i % 120 == 0) input.Tap(game, wrong, false); Tick(); }
            Assert.Less(machine.Handle.Position, machine.Handle.Travel - machine.SelectorBand, "Real stop blocks II before I.");
            Assert.Less(machine.SecondOutput.Position, .003f); Assert.IsFalse(machine.ReunionComplete); game.ReleaseProp();

            yield return Pull(pullC, machine.Handle, -1, () => machine.ReunionComplete);
            yield return Wait(8, () => machine.StageTwoEnabled, "Both door catches retract the physical selector stop");
            float first = machine.SecondOutput.Position;
            for (int i = 0; i < 240; i++) Tick();
            Assert.IsFalse(machine.Engaged, "C released means no unattended motor operation.");
            Assert.That(machine.SecondOutput.Position, Is.EqualTo(first).Within(.003f));
            Assert.IsTrue(machine.Input.Active && machine.Output.Active, "Both holders remain physically loaded between outputs.");
            if(recovery)
            {
                yield return Walk(holdA,new Vector3(level==40?-.70f:-.48f,-.274f,0));
                Assert.IsFalse(machine.Input.Active,"A operator has left the clutch");
                yield return Grasp(pullC,machine.Handle);
                Vector3 unpowered=Target(machine.Handle,1);input.Tap(game,unpowered,false);
                for(int i=0;i<240;i++)Tick();
                Assert.IsFalse(machine.Engaged||machine.Complete,"C cannot operate II without A");
                Assert.That(machine.SecondOutput.Position,Is.EqualTo(first).Within(.003f));game.ReleaseProp();
                yield return Pad(holdA,machine.Input);
            }
            yield return Pull(pullC, machine.Handle, 1, () => machine.Complete);
            if (level == 40) Assert.IsFalse(machine.ExitUnlocked, "Unlocking H must not open H itself.");

            Vector3 reunion = game.Root.TransformPoint(new Vector3(level == 40 ? .58f : .42f, -.274f, .12f));
            foreach (int anchor in Anchors()) { game.SelectFragment(anchor); input.Tap(game, reunion - game.Root.up * .026f); }
            yield return Wait(45, () => game.Matter.TotalFragmentCount == 1, "Every piece physically returns through D1 and D2 and reunites");
            int merged = Anchors()[0];
            if (level == 40) yield return Pull(merged, machine.FinalCap, 1, () => machine.FinalCap.AtEnd);
            game.SelectFragment(merged); COgheExpansionIntegrationTests.Capture(game, "before-exit");
            input.Tap(game, game.Owner.Outlet.position);
            Assert.IsTrue(game.Motion.Get(merged)?.Exit ?? false, "Opened outlet accepts screen tap");
            yield return Wait(40, () => game.Owner.Completed, "Merged body exits through the final physical aperture");
            Assert.AreEqual(32, game.Matter.EscapedCount); Assert.IsFalse(game.Owner.Lost);
            COgheExpansionIntegrationTests.Capture(game, $"{level:00}-completed");
            game.ResetLevel();
            Assert.IsFalse(machine.Complete || machine.ReunionComplete || machine.StageTwoEnabled || machine.Engaged);
            Assert.AreEqual(1, game.Matter.TotalFragmentCount); Assert.AreEqual(0, game.Matter.EscapedCount);
            foreach (var door in machine.ReunionDoors) Assert.Less(door.Position, .003f, "Retry restores each door and catch.");
            Assert.Less(machine.SecondOutput.Position, .003f); Assert.Less(machine.SelectorStop.Position, .003f);
            if (level == 40) { Assert.IsFalse(machine.Prepared); Assert.Less(machine.FinalCap.Position, .003f); }
        }
    }
}
