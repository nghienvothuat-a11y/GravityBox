#if DEVELOPMENT_BUILD && !UNITY_EDITOR
// Generated from Assets/_Game/Tests/PlayMode/COgheCampaign40CooperationTests.cs
// SHA256 f52515732c7bb40d829441ff566b3dd2e041cbfe8be02d470487b1ece5ac94e4
// Do not edit; regenerate after test changes.
using System;
using System.Collections;
using System.Collections.Generic;
using GravityBox.Venom;

using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace GravityBox.Venom.ChapterProof
{
    /// <summary>Physical solution and recovery evidence: commands match touch input; no tissue/door pose or solved state is assigned.</summary>
    public sealed class COgheCampaign40CooperationReplay
    {
        private const float Dt = 1f / 120f;
        private SimulationMode previousMode;
        private bool previousPersistence;
        private VenomCampaign game;

        public IEnumerator Before()
        {
            previousMode = Physics.simulationMode; Physics.simulationMode = SimulationMode.Script;
            previousPersistence = VenomCampaignSave.PersistenceEnabled;
            VenomCampaignSave.PersistenceEnabled = false; yield return null;
        }
        public IEnumerator After()
        {
            Time.timeScale = 1; Physics.simulationMode = previousMode;
            VenomCampaignSave.PersistenceEnabled = false; yield return null;
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
            Assert.NotNull(game); yield return null;
        }
        private void Tick()
        {
            game.Owner.Step(Dt); game.Owner.Rotation.Step(Dt); Physics.Simulate(Dt);
        }
        private List<int> Parts()
        {
            var parts = new List<int>(); var groups = new HashSet<int>();
            for (int i = 0; i < 32; i++) if (!game.Matter.Escaped[i] && groups.Add(game.Matter.Groups[i])) parts.Add(i);
            return parts;
        }
        private T Mechanism<T>(string name = null) where T : COgheMechanism
        {
            foreach (var mechanism in game.Mechanisms)
                if (mechanism is T result && (name == null || mechanism.name == name)) return result;
            Assert.Fail($"Missing {typeof(T).Name}: {name}"); return null;
        }
        private string State()
        {
            var result = new List<string> { $"failure={game.Failure}; fragments={game.Matter.TotalFragmentCount}; escaped={game.Matter.EscapedCount}" };
            foreach (int anchor in Parts()) result.Add($"part {anchor}={game.Root.InverseTransformPoint(game.Motion.Centre(anchor)):F3}");
            foreach (var mechanism in game.Mechanisms)
                if (mechanism is COgheRailSlider rail) result.Add($"{rail.name}={rail.Position:F4}/{rail.Travel:F3} locked={rail.Locked} effort={rail.Effort:F3}");
            return string.Join("; ", result);
        }
        private IEnumerator WaitFor(float seconds, Func<bool> condition, string why)
        {
            for (int i = 0; i < Mathf.CeilToInt(seconds / Dt) && !condition() && !game.Owner.Lost; i++)
            {
                Tick(); if (i % 240 == 0) yield return null;
            }
            if (!condition()) yield return COgheChapterProofCapture.Request(game, $"campaign40-{game.Definition.Order:00}-failed");
            Assert.IsTrue(condition(), why + "; " + State()); Assert.IsFalse(game.Owner.Lost, State());
        }
        private IEnumerator For(float seconds)
        {
            for (int i = 0; i < Mathf.CeilToInt(seconds / Dt); i++) { Tick(); if (i % 240 == 0) yield return null; }
            Assert.IsFalse(game.Owner.Lost, State());
        }
        private IEnumerator Walk(int anchor, Vector3 point, float tolerance = .038f)
        {
            game.SelectFragment(anchor); game.Motion.Move(anchor, point, true);
            yield return WaitFor(36, () => Vector3.Distance(game.Motion.Centre(anchor), point) < tolerance, "Walk to " + point);
        }
        private IEnumerator Hold(int anchor, COgheTissueSensor pad)
        {
            yield return Walk(anchor, pad.transform.position + Vector3.up * .018f);
            yield return WaitFor(10, () => pad.Active, "Actual tissue must hold " + pad.name);
        }
        private IEnumerator Split(COgheGuillotine knife)
        {
            Assert.AreEqual(1, game.Matter.TotalFragmentCount, "Begin the cut with reunited tissue");
            int anchor = Parts()[0]; game.SelectFragment(anchor); game.Motion.Move(anchor, knife.Sensor.position, true);
            yield return WaitFor(30, () => game.Matter.TotalFragmentCount > 1, "Actual knife cuts bonds");
        }
        private void Roles(out int holder, out int worker)
        {
            var parts = Parts(); parts.Sort((a, b) => game.Motion.Centre(a).x.CompareTo(game.Motion.Centre(b).x));
            holder = parts[0]; worker = parts[parts.Count - 1]; Assert.AreNotEqual(holder, worker);
        }
        private IEnumerator Pull(int anchor, COgheRailSlider rail, float direction, Func<bool> finished, float seconds = 35)
        {
            Vector3 target = rail.Frame.TransformPoint(rail.Start + rail.Axis * (direction > 0 ? rail.Travel + .12f : -.12f));
            game.SelectFragment(anchor); game.SelectProp(rail.GetComponent<VenomMovableProp>());
            yield return WaitFor(30, () => game.Attached || finished(), "Grasp " + rail.name);
            // A single grasp with continued input must execute the whole
            // stroke. Repeated reattachment would hide an unstable stance.
            for (int i = 0; i < Mathf.CeilToInt(seconds / Dt) && !finished() && !game.Owner.Lost; i++)
            {
                if (!game.Attached)
                {
                    yield return COgheChapterProofCapture.Request(game, $"campaign40-{game.Definition.Order:00}-grasp-failed");
                    Assert.Fail($"Lost the single grasp on {rail.name} after {i * Dt:F2}s; {State()}");
                }
                if (i % 90 == 0) game.SetPropTarget(target);
                Tick(); if (i % 240 == 0) yield return null;
            }
            if (!finished()) yield return COgheChapterProofCapture.Request(game, $"campaign40-{game.Definition.Order:00}-pull-failed");
            game.ReleaseProp();
            Assert.IsTrue(finished(), $"Operate {rail.name} with one grasp; {State()}");
        }
        private IEnumerator Reunion(Vector3 local)
        {
            Vector3 point = game.Root.TransformPoint(local);
            foreach (int anchor in Parts()) game.Motion.Move(anchor, point, true);
            yield return WaitFor(50, () => game.Matter.TotalFragmentCount == 1, "All parts physically reunite");
        }
        private IEnumerator Leave()
        {
            Assert.AreEqual(1, game.Matter.TotalFragmentCount); Assert.IsTrue(game.FinalExitAvailable);
            yield return COgheChapterProofCapture.Request(game, $"campaign40-{game.Definition.Order:00}-solution-before-exit");
            int anchor = Parts()[0]; game.SelectFragment(anchor);
            yield return COgheChapterProofCapture.Request(game, "before-exit");
            game.Motion.Move(anchor, game.Owner.Outlet.position - game.Owner.Outlet.forward * .024f, false, true);
            yield return WaitFor(45, () => game.Owner.Completed, "All tissue exits through the authored aperture");
            Assert.AreEqual(32, game.Matter.EscapedCount);
            yield return COgheChapterProofCapture.Request(game, $"campaign40-{game.Definition.Order:00}-solution-complete");
            game.ResetLevel(); yield return For(1);
            Assert.IsFalse(game.FinalExitAvailable); Assert.AreEqual(1, game.Matter.TotalFragmentCount); Assert.AreEqual(0, game.Matter.EscapedCount);
            foreach (var mechanism in game.Mechanisms)
                if (mechanism is COgheRailSlider rail)
                    Assert.That(rail.Position, Is.EqualTo(rail.InitialTravel).Within(.005f), "Retry restores " + rail.name);
        }

        public IEnumerator Level35ReturnsWhenAIsReleasedThenCatchesDReunitesPullsEAndExits()
        {
            yield return Load(35);
            var spring = Mechanism<COgheSpringAccessDoor>();
            // A single whole body cannot leave A and inherit a permanently
            // open door. Return is automatic, not a reset-driven recovery.
            yield return Hold(0, spring.Input);
            yield return WaitFor(10, () => spring.Door.AtEnd, "A opens D temporarily");
            yield return Walk(0, game.Root.TransformPoint(new Vector3(-.36f, -.278f, -.02f)));
            yield return WaitFor(6, () => spring.Door.Position < .003f, "D must return when A loses load");
            Assert.IsFalse(spring.Caught); Assert.IsFalse(game.FinalExitAvailable);
            yield return Split(Mechanism<COgheGuillotine>()); Roles(out int holder, out int worker);
            yield return Hold(holder, spring.Input);
            yield return WaitFor(10, () => spring.Door.AtEnd, "A reopens D after recoverable release");
            yield return Walk(worker, game.Root.TransformPoint(new Vector3(.13f, -.278f, .14f)));
            yield return Pull(worker, spring.LatchHandle, 1, () => spring.Caught);
            yield return Reunion(new Vector3(.29f, -.278f, .04f));
            Assert.IsFalse(spring.Input.Active); Assert.IsTrue(spring.Door.AtEnd, "The physical catch releases the A operator");
            yield return Pull(Parts()[0], spring.FinalCover, 1, () => spring.FinalCover.AtEnd);
            yield return Leave();
        }

        public IEnumerator Level36RevealsBChangesRolesAndCompletesWithRealWinchEffort()
        {
            yield return Load(36);
            var spring = Mechanism<COgheSpringAccessDoor>(); var winch = Mechanism<COgheCooperativeWinch>();
            Assert.IsFalse(winch.Input.Active); Assert.IsFalse(game.FinalExitAvailable);
            yield return Split(Mechanism<COgheGuillotine>()); Roles(out int holder, out int worker);
            yield return Hold(holder, spring.Input);
            yield return WaitFor(10, () => spring.Door.AtEnd, "Held A lifts temporary D");
            yield return Walk(worker, game.Root.TransformPoint(new Vector3(.13f, -.278f, .14f)));
            yield return Pull(worker, spring.LatchHandle, 1, () => spring.Caught);
            Assert.IsTrue(spring.LatchHandle.AtEnd, "Solid B cover is fully removed");
            yield return Hold(worker, winch.Input);
            yield return Pull(holder, winch.Handle, 1, () => winch.Complete);
            Assert.IsTrue(spring.Caught); Assert.IsFalse(spring.Input.Active, "Old A role is relinquished");
            yield return Reunion(new Vector3(.24f, -.278f, -.07f));
            yield return Leave();
        }

        public IEnumerator Level37UsesOneGearAtBothStationsWhileAIsHeldThenReunitesAndExits()
        {
            yield return Load(37);
            var dock = Mechanism<COgheCooperativeDockTransmission>();
            // Docking alone never energizes the motor.
            yield return Pull(0, dock.Carriage, -1, () => dock.AtI && dock.Carriage.Latched);
            yield return For(1); Assert.Less(dock.AccessDoor.Position, .004f); Assert.IsFalse(dock.AccessCaught);
            yield return Split(Mechanism<COgheGuillotine>()); Roles(out int holder, out int worker);
            yield return Hold(holder, dock.Input);
            yield return WaitFor(12, () => dock.AccessCaught, "Actual pitch contact at I plus A opens D");
            yield return Pull(worker, dock.Carriage, 1, () => dock.AtII && dock.Carriage.Latched, 40);
            yield return Pull(worker, dock.Handle, 1, () => dock.Complete);
            yield return Reunion(new Vector3(.33f, -.278f, .04f));
            yield return Leave();
        }

        public IEnumerator Level38SplitsOpensDReunitesMovesHeavyQSplitsAgainAndExits()
        {
            yield return Load(38);
            var first = Mechanism<COgheCooperativeWinch>("A K first-stage winch");
            var second = Mechanism<COgheCooperativeWinch>("Q B C second-stage winch");
            var q = Mechanism<COgheRailSlider>("Q heavy transmission carriage");
            Assert.Greater(q.Resistance, 16 * game.Matter.Profile.ParticleMass * 7, "Balanced half-body cannot overpower Q");
            Assert.Less(q.Resistance, 32 * game.Matter.Profile.ParticleMass * 7, "Whole body has enough force");
            yield return Split(Mechanism<COgheGuillotine>("I first gravity knife controller")); Roles(out int holder, out int worker);
            yield return Hold(holder, first.Input);
            yield return Pull(worker, first.Handle, 1, () => first.Complete);
            yield return Reunion(new Vector3(.01f, -.278f, -.12f));
            yield return Pull(Parts()[0], q, 1, () => q.AtEnd && q.Latched, 45);
            Assert.IsTrue(q.Latched, "Q's physical end catch retains the transmission");
            yield return Split(Mechanism<COgheGuillotine>("II second gravity knife controller")); Roles(out holder, out worker);
            yield return Hold(holder, second.Input);
            yield return Pull(worker, second.Handle, 1, () => second.Complete);
            yield return Reunion(new Vector3(.48f, -.278f, .03f));
            yield return Leave();
        }

        public IEnumerator CooperationScenesResetClosedWithoutResidualEffortOrPrematureExit()
        {
            foreach (int slot in new[] { 35, 36, 37, 38 })
            {
                yield return Load(slot); yield return For(.3f);
                Assert.IsFalse(game.FinalExitAvailable);
                game.ResetLevel(); Physics.SyncTransforms(); yield return For(.3f);
                Assert.AreEqual(1, game.Matter.TotalFragmentCount); Assert.AreEqual(0, game.Matter.EscapedCount);
                foreach (var mechanism in game.Mechanisms)
                    if (mechanism is COgheRailSlider rail && !rail.name.Contains("knife"))
                        Assert.That(rail.Position, Is.EqualTo(rail.InitialTravel).Within(.004f), rail.name + " resets to its authored pose");
                Assert.IsFalse(game.FinalExitAvailable);
            }
        }

        public IEnumerator CooperationDividersHaveSolidTwoSidedWallsOutsideTheAuthoredGroundDoor()
        {
            foreach (int slot in new[] { 35, 36, 37, 38 })
            {
                yield return Load(slot);
                float x = slot == 38 ? -.13f : 0;
                foreach (float side in new[] { -1f, 1f })
                foreach (var yz in new[] { new Vector2(.20f, .14f), new Vector2(-.22f, -.24f), new Vector2(-.22f, .285f) })
                {
                    var ray = new Ray(game.Root.TransformPoint(new Vector3(x + side * .08f, yz.x, yz.y)), game.Root.TransformDirection(Vector3.left * side));
                    bool blocked = false;
                    foreach (var surface in game.Surfaces)
                        if (surface.name.StartsWith("D ") && surface.Shape.Raycast(ray, out _, .12f)) { blocked = true; break; }
                    Assert.IsTrue(blocked, $"Level {slot} must seal {yz} from side {side}; climbing around D must not bypass cooperation.");
                }
            }
        }
    }
}

#endif
