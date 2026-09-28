using System;
using System.Collections;
using System.IO;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed partial class COgheViewExpansionTests
    {
        private VenomCampaign game;
        private SimulationMode previous;
        private bool persistence;
        private const float Dt = 1f / 120;
        [UnitySetUp] public IEnumerator Before()
        { previous = Physics.simulationMode; persistence = VenomCampaignSave.PersistenceEnabled; Physics.simulationMode = SimulationMode.Script; VenomCampaignSave.PersistenceEnabled = false; yield return null; }
        [UnityTearDown] public IEnumerator After()
        { Time.timeScale = 1; Physics.simulationMode = previous; VenomCampaignSave.PersistenceEnabled = persistence; yield return null; }
        private IEnumerator Load(int level)
        {
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                game = Object.FindFirstObjectByType<VenomCampaign>(); game.AutoAdvance = false; game.Owner.enabled = game.Owner.Rotation.enabled = false;
                foreach (var body in game.Matter.Bodies) Assert.Greater(body.position.y - game.Matter.Profile.ParticleRadius, -.299f, "Spawn clears floor with particle radius");
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync($"COgheView{level:00}"); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            yield return Advance(1); game.CameraRig.Frame(720, 1280, 0, true);
        }
        private void Tick() { game.Owner.Step(Dt); game.Owner.Rotation.Step(Dt); if (!game.Owner.Paused) Physics.Simulate(Dt); }
        private IEnumerator Advance(float seconds) { for (int i = 0; i < seconds / Dt; i++) { Tick(); if (i % 240 == 0) yield return null; } }
        private IEnumerator Until(float seconds, Func<bool> done, string reason)
        {
            for (int i = 0; i < seconds / Dt && !done() && !game.Owner.Lost; i++) { Tick(); if (i % 240 == 0) yield return null; }
            if (!done()) Capture("failure");
            string detail = $"; level={game.Definition.Order}; centre={game.Motion.Centre(game.Motion.Selected):F4}; {game.Activity}; {game.Failure}";
            foreach (var task in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>()) detail += $"; {task.Label}={task.Rail.Position:F4}/{task.Rail.Travel} {task.Phase} {task.LastFailure}";
            foreach (var tube in game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>()) { detail += "; " + tube.DebugState(game.Motion.Selected); for(int n=0;n<tube.Nodes.Length;n++) if(tube.Nodes[n].Terminal==COgheTubeNetwork.TerminalKind.Entry)detail+="; "+tube.DebugEntryState(game.Motion.Selected,n); }
            for(int i=0;i<32;i++) if(i==0||game.Matter.Groups[i]!=game.Matter.Groups[0]) { detail+=$"; actor={i} group={game.Matter.Groups[i]} target={game.Motion.Get(i)?.Target}"; if(i>0)break; }
            foreach(var pad in game.Owner.Apparatus.GetComponentsInChildren<COgheTapPad>()) detail+=$"; pad actor={pad.Actor} load={pad.Sensor.Load}";
            Assert.IsTrue(done(), reason + detail);
        }
        private IEnumerator Tap(Vector3 point)
        { game.CameraRig.Frame(720, 1280, 0, true); game.TouchPoint(game.Owner.View.WorldToScreenPoint(point)); yield return null; }
        private void Capture(string state)
        {
            string directory = "Artifacts/COgheViewExpansion/TestFrames"; Directory.CreateDirectory(directory);
            var camera = game.Owner.View; var previous = RenderTexture.active; var target = RenderTexture.GetTemporary(720, 1280, 24);
            var texture = new Texture2D(720, 1280, TextureFormat.RGB24, false);
            try { camera.targetTexture = target; camera.Render(); RenderTexture.active = target; texture.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); texture.Apply(); File.WriteAllBytes($"{directory}/{game.Definition.Order:00}-{state}.png", texture.EncodeToPNG()); }
            finally { camera.targetTexture = null; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); Object.DestroyImmediate(texture); }
        }
        private IEnumerator Solve(int level)
        {
            yield return Load(level); Capture("start");
            yield return new COgheViewExpansionScenario(game, Tap, Until).Solve();
            Assert.AreEqual(32, game.Matter.EscapedCount); Assert.AreEqual(1, game.Matter.TotalFragmentCount);
            Assert.IsFalse(game.Owner.Lost); Assert.IsTrue(game.Progress.Completed.Contains(game.Definition.Id));
            Assert.AreEqual(Quaternion.identity, game.Root.rotation); Capture("won");
            game.ResetLevel(); yield return Advance(.5f); AssertReset();
        }
        private void AssertReset()
        {
            Assert.AreEqual(0, game.Matter.EscapedCount); Assert.AreEqual(1, game.Matter.TotalFragmentCount);
            Assert.IsFalse(game.Owner.Lost); Assert.IsFalse(game.Owner.Completed); Assert.AreEqual(0, game.CameraRig.OrbitYaw); Assert.AreEqual(1, game.CameraRig.ZoomScale);
            foreach (var task in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>()) { Assert.IsFalse(task.Busy); Assert.AreEqual(0, task.CompletedJourneys); }
            foreach (var door in game.Owner.Apparatus.GetComponentsInChildren<COgheSpringAccessDoor>()) Assert.IsFalse(door.Caught);
        }
        private IEnumerator Recovery(int level)
        {
            yield return Load(level);
            Assert.AreEqual(30, game.PlayableLevelCount); Assert.AreEqual($"coghe.view.v2.{level:00}", game.Definition.Id);
            Assert.IsTrue(game.Definition.ViewOnly); Assert.IsFalse(game.Definition.CanRotate); Assert.AreEqual("", game.Definition.ProgressKey);
            if (level % 10 == 0) Assert.IsEmpty(game.Definition.Lesson);
            foreach (var task in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
                if (task.RequiredLoad != null) { Assert.IsFalse(task.Request(0), "Absent load cannot start a remote task"); Assert.AreEqual(0, task.CompletedJourneys); }
            var tasks = game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();
            COgheTapRail first = Array.Find(tasks, t => t.InterlockOpen);
            if (first != null)
            {
                yield return Tap(first.HandPoint);
                yield return Advance(.2f);
                int journeys = first.CompletedJourneys;
                for (int i = 0; i < 4; i++) yield return Tap(first.HandPoint);
                float time = game.Matter.SimulationTime, position = first.Rail.Position;
                game.Owner.TogglePause(); yield return Advance(2);
                Assert.AreEqual(time, game.Matter.SimulationTime); Assert.AreEqual(position, first.Rail.Position);
                game.Owner.TogglePause();
                Assert.LessOrEqual(first.CompletedJourneys, journeys + 1, "Repeated input never queues an extra journey");
            }
            game.CameraRig.Orbit(180, 720); game.CameraRig.Pinch(1.4f);
            game.ResetLevel(); yield return Advance(.5f); AssertReset();
            Assert.IsTrue(game.Owner.CanControl, "Retry must release all stale ownership/input");
            foreach (var tube in game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>())
                for (int i = 0; i < 32; i++) Assert.IsFalse(tube.IsParticleInside(i), "Retry clears transit ownership");
            foreach (int height in new[] { 1280, 1612 })
            {
                var safe = new Rect(0, 40, 720, height - 120); game.CameraRig.Frame(720, height, 0, true, safe);
                var usable = game.CameraRig.UsableRect(720, height, safe);
                for (int i = 0; i < 8; i++)
                {
                    var p = game.Owner.View.WorldToViewportPoint(game.Root.TransformPoint(VenomCampaignCamera.Corner(game.CameraRig.OverviewBounds, i)));
                    Assert.That(p.x * 720, Is.InRange(usable.xMin - 1, usable.xMax + 1));
                    Assert.That(p.y * height, Is.InRange(usable.yMin - 1, usable.yMax + 1));
                }
            }
        }
        [UnityTest] public IEnumerator View11FullSolution() { yield return Solve(11); }
        [UnityTest] public IEnumerator View11RecoveryAndPortrait() { yield return Recovery(11); }
        [UnityTest] public IEnumerator View12FullSolution() { yield return Solve(12); }
        [UnityTest] public IEnumerator View12RecoveryAndPortrait() { yield return Recovery(12); }
        [UnityTest] public IEnumerator View13FullSolution() { yield return Solve(13); }
        [UnityTest] public IEnumerator View13RecoveryAndPortrait() { yield return Recovery(13); }
        [UnityTest] public IEnumerator View14FullSolution() { yield return Solve(14); }
        [UnityTest] public IEnumerator View14RecoveryAndPortrait() { yield return Recovery(14); }
        [UnityTest] public IEnumerator View15FullSolution() { yield return Solve(15); }
        [UnityTest] public IEnumerator View15RecoveryAndPortrait() { yield return Recovery(15); }
        [UnityTest] public IEnumerator View16FullSolution() { yield return Solve(16); }
        [UnityTest] public IEnumerator View16RecoveryAndPortrait() { yield return Recovery(16); }
        [UnityTest] public IEnumerator View17FullSolution() { yield return Solve(17); }
        [UnityTest] public IEnumerator View17RecoveryAndPortrait() { yield return Recovery(17); }
        [UnityTest] public IEnumerator View18FullSolution() { yield return Solve(18); }
        [UnityTest] public IEnumerator View18RecoveryAndPortrait() { yield return Recovery(18); }
        [UnityTest] public IEnumerator View19FullSolution() { yield return Solve(19); }
        [UnityTest] public IEnumerator View19RecoveryAndPortrait() { yield return Recovery(19); }
        [UnityTest] public IEnumerator View20FullSolution() { yield return Solve(20); }
        [UnityTest] public IEnumerator View20RecoveryAndPortrait() { yield return Recovery(20); }
        [UnityTest] public IEnumerator View21FullSolution() { yield return Solve(21); }
        [UnityTest] public IEnumerator View21RecoveryAndPortrait() { yield return Recovery(21); }
        [UnityTest] public IEnumerator View22FullSolution() { yield return Solve(22); }
        [UnityTest] public IEnumerator View22RecoveryAndPortrait() { yield return Recovery(22); }
        [UnityTest] public IEnumerator View23FullSolution() { yield return Solve(23); }
        [UnityTest] public IEnumerator View23RecoveryAndPortrait() { yield return Recovery(23); }
        [UnityTest] public IEnumerator View24FullSolution() { yield return Solve(24); }
        [UnityTest] public IEnumerator View24RecoveryAndPortrait() { yield return Recovery(24); }
        [UnityTest] public IEnumerator View25FullSolution() { yield return Solve(25); }
        [UnityTest] public IEnumerator View25RecoveryAndPortrait() { yield return Recovery(25); }
        [UnityTest] public IEnumerator View26FullSolution() { yield return Solve(26); }
        [UnityTest] public IEnumerator View26RecoveryAndPortrait() { yield return Recovery(26); }
        [UnityTest] public IEnumerator View27FullSolution() { yield return Solve(27); }
        [UnityTest] public IEnumerator View27RecoveryAndPortrait() { yield return Recovery(27); }
        [UnityTest] public IEnumerator View28FullSolution() { yield return Solve(28); }
        [UnityTest] public IEnumerator View28RecoveryAndPortrait() { yield return Recovery(28); }
        [UnityTest] public IEnumerator View29FullSolution() { yield return Solve(29); }
        [UnityTest] public IEnumerator View29RecoveryAndPortrait() { yield return Recovery(29); }
        [UnityTest] public IEnumerator View30FullSolution() { yield return Solve(30); }
        [UnityTest] public IEnumerator View30RecoveryAndPortrait() { yield return Recovery(30); }
    }
}
