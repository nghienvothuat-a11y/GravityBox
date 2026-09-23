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
    public sealed class COgheTapCampaignTests
    {
        const float Dt = 1f / 120;
        VenomCampaign game;
        SimulationMode previousMode;
        bool previousPersistence;
        float previousTime;
        int viewportHeight=1280;
        bool integratedCampaign;
        [UnitySetUp] public IEnumerator Before()
        {
            previousMode = Physics.simulationMode; previousPersistence = VenomCampaignSave.PersistenceEnabled; previousTime = Time.timeScale;
            Physics.simulationMode = SimulationMode.Script; VenomCampaignSave.PersistenceEnabled = false; Time.timeScale = 1;
            yield return null;
        }
        [UnityTearDown] public IEnumerator After()
        {
            if (game != null) { game.Motion.StopAll(); game.AutoAdvance = false; }
            Physics.simulationMode = previousMode; VenomCampaignSave.PersistenceEnabled = previousPersistence; Time.timeScale = previousTime;
            yield return null;
        }
        IEnumerator Load(int n)
        {
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                game = Object.FindFirstObjectByType<VenomCampaign>(); Assert.NotNull(game);
                game.AutoAdvance = false; game.Owner.enabled = false; game.Owner.Rotation.enabled = false;
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync(integratedCampaign ? $"COgheOrigin{n + 40:00}" : $"COgheTap{n:00}"); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            Steps(120); Frame();
        }
        void Steps(int count)
        {
            for (int i = 0; i < count; i++)
            { game.Owner.Step(Dt); game.Owner.Rotation.Step(Dt); if (!game.Owner.Paused) Physics.Simulate(Dt); }
        }
        void Frame() => game.CameraRig.Frame(720, viewportHeight, 0, true);
        void Tap(Vector3 point) { Frame(); game.TouchPoint(game.Owner.View.WorldToScreenPoint(point)); }
        COgheTapRail Rail(string label) => Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(), x => x.Label == label);
        COgheGearTrain Train => Object.FindFirstObjectByType<COgheGearTrain>();
        string State
        {
            get
            {
                string s = $"centre={game.Motion.Centre(game.Motion.Selected):F3}, loss={game.Failure}, fragments={game.Matter.TotalFragmentCount}, selected={game.Motion.Selected}, order={game.Motion.Get(game.Motion.Selected)?.Target}, exit={game.Motion.Get(game.Motion.Selected)?.Exit}, picked={game.Feedback.CommandSurface?.name}";
                foreach (var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
                    s += $"; {rail.Label}:{rail.Phase}, x={rail.Rail.Position:F4}, end={rail.AtEnd}, lock={rail.InterlockOpen}, failure={rail.LastFailure}";
                var knife=Object.FindFirstObjectByType<COgheGuillotine>();
                if(knife!=null)s+=$"; knife={knife.Phase}, blade={knife.Rail.Body.position:F4}, cuts={game.Matter.CutCount}";
                return s;
            }
        }
        IEnumerator Wait(float seconds, Func<bool> complete, string message)
        {
            for (int i = 0; i < seconds / Dt && !complete() && !game.Owner.Lost; i++)
            { Steps(1); if (i % 240 == 0) yield return null; }
            if (!complete()) COgheExpansionIntegrationTests.Capture(game, "tap-failed-" + TestContext.CurrentContext.Test.Name);
            Assert.IsTrue(complete(), message + "; " + State);
        }
        IEnumerator Operate(string label)
        {
            var rail = Rail(label); int before = rail.CompletedJourneys;
            Tap(rail.HandPoint);
            if(!rail.Busy)COgheExpansionIntegrationTests.Capture(game,"tap-pick-failed-"+TestContext.CurrentContext.Test.Name);
            Assert.IsTrue(rail.Busy, "One screen tap must start " + label + "; " + State);
            yield return Wait(24, () => rail.CompletedJourneys == before + 1 && !rail.Busy, "One tap completes " + label);
        }
        IEnumerator Exit()
        {
            Tap(game.Owner.Outlet.position);
            yield return Wait(24, () => game.Owner.Completed, "Whole body exits through the real aperture");
            Assert.AreEqual(32, game.Matter.EscapedCount); Assert.IsFalse(game.Owner.Lost);
        }

        [UnityTest] public IEnumerator OneTapSeatsStopsReversesAndActuallyOpensAndClosesDoor()
        {
            yield return Load(1); var rail = Rail("A");
            Assert.IsFalse(Train.Powered); Assert.IsFalse(game.FinalExitAvailable);
            yield return Operate("A");
            yield return Wait(6, () => Train.Rack.AtEnd, "Measured gear train opens rack");
            float seated = rail.Rail.Position; Steps(600);
            Assert.That(rail.Rail.Position, Is.EqualTo(seated).Within(.002f)); Assert.IsFalse(rail.Busy);
            COgheExpansionIntegrationTests.Capture(game, "tap-01-open");
            yield return Operate("A");
            yield return Wait(6, () => Train.Rack.Position < .006f, "Disconnecting lets the return spring close the actual shutter");
            Assert.IsFalse(game.FinalExitAvailable);
            yield return Operate("A"); yield return Wait(6, () => Train.Rack.AtEnd, "Reopen after reverse");
            yield return Exit();
        }
        [UnityTest] public IEnumerator RepeatedTapsDoNotQueueTogglesAndPauseDoesNotExpireJourney()
        {
            yield return Load(1); var rail = Rail("A"); Tap(rail.HandPoint);
            for (int i = 0; i < 12; i++) Tap(rail.HandPoint);
            yield return Wait(15, () => rail.Phase == COgheTapRail.TaskPhase.Operating, "Reach real working contact");
            float at = rail.Rail.Position, clock = game.Matter.SimulationTime;
            game.Owner.TogglePause(); Steps(1200);
            Assert.AreEqual(clock, game.Matter.SimulationTime); Assert.AreEqual(at, rail.Rail.Position);
            Assert.IsFalse(rail.Request(0), "Paused input cannot enqueue an action");
            game.Owner.TogglePause();
            yield return Wait(12, () => rail.CompletedJourneys == 1, "Resume finishes the original action");
            Steps(720); Assert.AreEqual(1, rail.CompletedJourneys); Assert.IsTrue(rail.AtEnd);
        }
        [UnityTest] public IEnumerator OpeningHintFollowsRealProgressAndReturnsOnRetry()
        {
            yield return Load(1);var lesson=game.GetComponent<COgheTapLesson>();Assert.NotNull(lesson);
            StringAssert.Contains("bánh răng A",lesson.Hint);
            Tap(Rail("A").HandPoint);StringAssert.Contains("đang thao tác",lesson.Hint);
            yield return Wait(20,()=>Rail("A").CompletedJourneys==1&&Train.Rack.AtEnd,"Complete the observed action");
            StringAssert.Contains("lỗ sáng",lesson.Hint);
            game.ResetLevel();Steps(120);StringAssert.Contains("bánh răng A",lesson.Hint);
        }
        [UnityTest] public IEnumerator ClosingDoorBackDrivesItsOutputGearAcrossNoDisconnectedGap()
        {
            yield return Load(1);yield return Operate("A");yield return Wait(8,()=>Train.Rack.AtEnd,"Open door first");
            Tap(Rail("A").HandPoint);
            yield return Wait(12,()=>!Train.Meshed&&Train.Rack.Position<.13f&&Train.Rack.Body.linearVelocity.y<-.03f,"Observe actual return motion");
            int last=Train.Wheels.Length-1;var before=Train.Wheels[last].rotation;float rack=Train.Rack.Position;
            Steps(6);
            Assert.IsFalse(Train.Powered);Assert.Less(Train.Rack.Position,rack);
            Assert.Less(Train.AngularSpeeds[last],0,"Output gear must turn with the returning rack");
            Assert.Greater(Quaternion.Angle(before,Train.Wheels[last].rotation),2,"Visible output cannot stay still during physical return");
            Assert.That(Train.AngularSpeeds[0],Is.EqualTo(-Train.MotorSpeed).Within(.001f),"Disconnected source keeps its own motor motion");
        }
        [UnityTest] public IEnumerator RetryDuringApproachAndOperationClearsActorForcesAndOutput()
        {
            yield return Load(1); var rail = Rail("A"); Tap(rail.HandPoint); Steps(30); game.ResetLevel();
            Assert.IsFalse(rail.Busy); Assert.AreEqual(-1, rail.Actor);
            Steps(120); Tap(rail.HandPoint);
            yield return Wait(15, () => rail.Phase == COgheTapRail.TaskPhase.Operating && rail.Rail.Position > .025f, "Reset while moving");
            game.ResetLevel(); Steps(360);
            Assert.IsFalse(rail.Busy); Assert.AreEqual(0, rail.CompletedJourneys);
            Assert.Less(rail.Rail.Position, .004f); Assert.Less(Train.Rack.Position, .004f);
            Assert.IsFalse(game.Owner.Lost); Assert.AreEqual(1, game.Matter.TotalFragmentCount);
            yield return Operate("A");
        }
        [UnityTest] public IEnumerator ANewWalkCancelsOnlyTheApproach()
        {
            yield return Load(1); var rail = Rail("A"); Tap(rail.HandPoint);
            Tap(game.Root.TransformPoint(new Vector3(-.30f, -.30f, -.20f))); Steps(120);
            Assert.IsFalse(rail.Busy); Assert.Less(rail.Rail.Position, .004f);
            yield return Operate("A");
        }
        [UnityTest] public IEnumerator APhysicalBlockCannotBeAnimatedThroughOrReportedAsSuccess()
        {
            yield return Load(1); var rail = Rail("A");
            var blocker = new GameObject("Test obstruction"); var box = blocker.AddComponent<BoxCollider>();
            blocker.transform.position = rail.Rail.Frame.TransformPoint(rail.Rail.Start + rail.Rail.Axis * .065f);
            box.size = new Vector3(.12f, .075f, .025f);
            try
            {
                Physics.SyncTransforms(); game.Motion.BuildGraph(true); Tap(rail.HandPoint);
                Steps(2400);
                Assert.AreEqual(0, rail.CompletedJourneys); Assert.IsFalse(rail.Busy);
                Assert.Less(rail.Rail.Position, .05f); Assert.IsFalse(Train.Powered); Assert.IsFalse(game.Owner.Completed);
            }
            finally { Object.DestroyImmediate(blocker); game.ResetLevel(); }
            Steps(120); yield return Operate("A");
        }
        [UnityTest] public IEnumerator LessonTwoNeedsOpenPowerThenReturnAndCanBeReplayed()
        {
            yield return Load(2); Tap(Rail("B").HandPoint); Steps(30);
            Assert.IsFalse(Rail("B").Busy); Assert.IsFalse(game.FinalExitAvailable);
            yield return Operate("A"); yield return Operate("B");
            Assert.IsFalse(Train.Powered, "Power alone cannot bridge the disconnected gear");
            yield return Operate("A"); yield return Wait(8, () => Train.Rack.AtEnd, "Returning A reconnects powered wheels");
            yield return Exit();
        }
        [UnityTest] public IEnumerator LessonThreeRequiresTwoConnectedGearsInOrder()
        {
            yield return Load(3); Tap(Rail("B").HandPoint); Assert.IsFalse(Rail("B").Busy);
            yield return Operate("A"); Assert.IsFalse(Train.Powered);
            yield return Operate("B"); yield return Wait(8, () => Train.Rack.AtEnd, "Both measured contacts transmit motion");
            yield return Exit();
        }
        IEnumerator Cut()
        {
            var knife = Object.FindFirstObjectByType<COgheGuillotine>(); Assert.NotNull(knife);
            Tap(knife.Rail.Body.position);
            yield return Wait(14, () => game.Matter.TotalFragmentCount > 1, "A screen tap approaches a real blade and cuts tissue");
            Assert.AreEqual(32, game.Matter.Bodies.Length); Assert.AreEqual(0, game.Matter.EscapedCount);
        }
        int Extreme(bool left)
        {
            int anchor = 0; float value = left ? float.PositiveInfinity : float.NegativeInfinity;
            for (int i = 0; i < 32; i++)
            {
                float x = game.Motion.Centre(i).x;
                if (left ? x < value : x > value) { value = x; anchor = i; }
            }
            return anchor;
        }
        void Select(int anchor) => game.SelectFragment(anchor);
        [UnityTest] public IEnumerator Integrated41Through50RetainAllTenPlayableSolutions()
        {
            integratedCampaign=true;
            try
            {
                yield return OneTapSeatsStopsReversesAndActuallyOpensAndClosesDoor();
                yield return LessonTwoNeedsOpenPowerThenReturnAndCanBeReplayed();
                yield return LessonThreeRequiresTwoConnectedGearsInOrder();
                yield return LessonFourCutsSelectsAndReunitesWithoutRotating();
                yield return LessonFiveTwoFragmentsHoldOperateReleaseReuniteAndWin();
                yield return ChapterSixAssemblesTwoGearsThenRetrievesBothParts();
                yield return ChapterSevenFourStopsPowerConnectionAndWrapAround();
                yield return ChapterEightCoordinatesLoadAndFourStopSelector();
                yield return ChapterNineWithdrawsThenRejoinsTheTwoGearTrain();
                yield return ChapterTenCombinesKnownRulesWithoutHintsAndUnlocksHomeOnce();
            }
            finally { integratedCampaign=false; }
        }
        string GroupState()
        {
            var groups=new HashSet<int>();string result="";
            for(int i=0;i<32;i++)if(groups.Add(game.Matter.Groups[i]))
            {
                var order=game.Motion.Get(i);
                result+=$" g{game.Matter.Groups[i]} anchor={i} centre={game.Motion.Centre(i):F4} target={order?.Target} path=";
                if(order!=null)for(int j=order.Cursor;j<Math.Min(order.Path.Count,order.Cursor+3);j++)result+=order.Path[j].ToString("F4");
            }
            return result;
        }
        IEnumerator MergeAndExit()
        {
            Vector3 meeting = game.Root.TransformPoint(new Vector3(.22f, -.30f, -.15f));
            var groups = new HashSet<int>();
            for (int i = 0; i < 32; i++) if (groups.Add(game.Matter.Groups[i])) { Select(i); Tap(meeting); }
            yield return Wait(24, () => game.Matter.TotalFragmentCount == 1, "Fragments walk back and fuse inside");
            yield return Exit();
        }
        [UnityTest] public IEnumerator LessonFourCutsSelectsAndReunitesWithoutRotating()
        {
            yield return Load(4); yield return Cut();
            StringAssert.Contains("từng phần",game.GetComponent<COgheTapLesson>().Hint);
            int left = Extreme(true), right = Extreme(false);
            string trace="After cut "+State+GroupState();
            Select(left); Tap(game.Root.TransformPoint(new Vector3(-.28f, -.30f, .05f)));
            trace+="\nAfter left tap "+State+GroupState();
            Select(right); Tap(game.Root.TransformPoint(new Vector3(.20f, -.30f, -.14f)));
            trace+="\nAfter right tap "+State+GroupState();
            for(int i=0;i<6;i++){Steps(30);trace+="\nAt "+(i+1)*.25f+" "+State+GroupState();}
            Assert.Greater(game.Matter.TotalFragmentCount, 1,trace);
            yield return MergeAndExit();
        }
        [UnityTest] public IEnumerator LessonFiveOneBodyCannotOperateTheLockedGear()
        {
            yield return Load(5); var task = Rail("B");
            Tap(task.HandPoint); Steps(1200);
            Assert.IsFalse(task.Busy); Assert.IsFalse(game.FinalExitAvailable); Assert.Less(task.Rail.Position, .004f);
            var pad = Object.FindFirstObjectByType<COgheTapPad>();
            Tap(pad.Sensor.transform.position);
            yield return Wait(15, () => pad.Sensor.Active, "One body can reach the pad");
            Tap(task.HandPoint); Steps(2400);
            Assert.IsFalse(game.FinalExitAvailable, "Leaving the pad must relock before one body can operate B");
            Assert.AreEqual(0, task.CompletedJourneys); Assert.IsFalse(task.Busy);
        }
        [UnityTest] public IEnumerator LessonFiveTwoFragmentsHoldOperateReleaseReuniteAndWin()
        {
            yield return Load(5); yield return Cut();
            var pad = Object.FindFirstObjectByType<COgheTapPad>();
            int left = Extreme(true), right = Extreme(false);
            Select(right); Tap(game.Root.TransformPoint(new Vector3(.16f, -.30f, -.18f)));
            Select(left); Tap(pad.Sensor.transform.position);
            yield return Wait(18, () => pad.Sensor.Active, "Left part holds the actual load sensor");
            StringAssert.Contains("phần khác",game.GetComponent<COgheTapLesson>().Hint);
            Select(right); yield return Operate("B");
            Assert.IsTrue(pad.Sensor.Active, "Changing selected fragment preserves pad occupancy");
            yield return Wait(8, () => Train.Rack.AtEnd, "Real transmission opens and catches the gate");
            StringAssert.Contains("chạm A lần nữa",game.GetComponent<COgheTapLesson>().Hint);
            Select(left); Tap(pad.Sensor.transform.position);
            yield return Wait(12, () => !pad.Sensor.Active, "Second pad tap steps its actor off");
            StringAssert.Contains("hợp thể",game.GetComponent<COgheTapLesson>().Hint);
            Steps(180); Assert.IsTrue(Train.Rack.AtEnd, "Visible output catch permits reunion after releasing A");
            COgheExpansionIntegrationTests.Capture(game, "tap-05-open-two-parts");
            yield return MergeAndExit();
        }
        [UnityTest] public IEnumerator EveryNewSceneHasIndependentIdentityAndPortraitTargets()
        {
            for (int n = 1; n <= 10; n++)
            {
                yield return Load(n);
                Assert.AreEqual($"coghe.tap.v1.{n:00}", game.Definition.Id);
                Assert.AreEqual(10, game.PlayableLevelCount); Assert.IsFalse(game.Definition.CanRotate); Assert.IsFalse(game.Owner.Completed);
                var old = game.Owner.View.targetTexture;
                foreach (int height in new[] { 1280, 1612 })
                {
                    var texture = new RenderTexture(720, height, 24); game.Owner.View.targetTexture = texture;
                    try
                    {
                        game.CameraRig.Frame(720, height, 0, true);
                        foreach (var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
                        {
                            var screen = game.Owner.View.WorldToScreenPoint(rail.HandPoint);
                            Assert.IsTrue(game.CameraRig.UsableRect(720, height, new Rect(0, 0, 720, height)).Contains(screen), rail.name);
                            if (rail.InterlockOpen) { game.TouchPoint(screen); Assert.IsTrue(rail.Busy, rail.name); rail.CancelTask(); }
                        }
                    }
                    finally { game.Owner.View.targetTexture = old; Object.DestroyImmediate(texture); }
                }
                game.ResetLevel(); Steps(120); COgheExpansionIntegrationTests.Capture(game, $"tap-{n:00}-start");
                Assert.IsFalse(game.Owner.Lost); Assert.AreEqual(0, game.Matter.EscapedCount);
            }
        }
        [UnityTest] public IEnumerator DisabledActuatorReleasesItsOwnedOrderAndCanBeUsedAgain()
        {
            yield return Load(1);var rail=Rail("A");Tap(rail.HandPoint);
            yield return Wait(15,()=>rail.Phase==COgheTapRail.TaskPhase.Operating,"Reach hand contact");
            rail.enabled=false;Assert.IsFalse(rail.Busy);Assert.IsNull(game.Motion.Get(0));
            Steps(120);rail.enabled=true;yield return Operate("A");
        }
        [UnityTest] public IEnumerator PrototypeSceneNavigationStaysInsideItsCatalog()
        {
            yield return Load(10);game.Load(11);yield return null;
            Assert.AreEqual("COgheTap10",SceneManager.GetActiveScene().name);
            game.Load(1);yield return null;
            game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            Assert.AreEqual("COgheTap01",SceneManager.GetActiveScene().name);Assert.AreEqual("coghe.tap.v1.01",game.Definition.Id);
            Assert.AreEqual(10,game.PlayableLevelCount);
        }
        [UnityTest] public IEnumerator OpaqueCoverRejectsTheEnlargedInteractionTarget()
        {
            yield return Load(1);var rail=Rail("A");
            var cover=new GameObject("Test opaque cover");var shape=cover.AddComponent<BoxCollider>();
            cover.transform.position=rail.Rail.Body.position+Vector3.up*.065f;shape.size=new Vector3(.25f,.035f,.25f);
            try{Physics.SyncTransforms();Tap(rail.HandPoint);Assert.IsFalse(rail.Busy);Steps(60);Assert.AreEqual(0,rail.CompletedJourneys);}
            finally{Object.DestroyImmediate(cover);game.ResetLevel();}
            Steps(120);yield return Operate("A");
        }
        [UnityTest] public IEnumerator UnmergedPartLeavingNewPracticeRoomLosesAndRetryRecovers()
        {
            yield return Load(4);yield return Cut();int right=Extreme(false);Select(right);Tap(game.Owner.Outlet.position);
            yield return Wait(20,()=>game.Owner.Lost,"An unmerged fragment must fail at the actual final exit");
            Assert.AreEqual(VenomCampaign.MergeFailure,game.Failure);Assert.IsFalse(game.Owner.Completed);
            game.ResetLevel();Steps(120);Assert.IsFalse(game.Owner.Lost);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            yield return Exit();
        }
        [UnityTest] public IEnumerator ReleasingLoadMidActionCancelsOnlyThatJourneyAndAllowsRecovery()
        {
            yield return Load(5);yield return Cut();var pad=Object.FindFirstObjectByType<COgheTapPad>();
            int left=Extreme(true),right=Extreme(false);Select(right);Tap(game.Root.TransformPoint(new Vector3(.16f,-.30f,-.18f)));
            Select(left);Tap(pad.Sensor.transform.position);yield return Wait(18,()=>pad.Sensor.Active,"Hold A");
            Select(right);var rail=Rail("B");Tap(rail.HandPoint);
            yield return Wait(18,()=>rail.Phase==COgheTapRail.TaskPhase.Operating,"B owns its operation");
            Assert.IsFalse(rail.Request(left),"Another fragment cannot steal a busy mechanism");Assert.AreEqual(right,rail.Actor);
            Select(left);Tap(pad.Sensor.transform.position);yield return Wait(12,()=>!pad.Sensor.Active&&!rail.Busy,"Lost load releases actor command");
            Assert.AreEqual(0,rail.CompletedJourneys);Assert.IsFalse(game.FinalExitAvailable);
            Select(left);Tap(pad.Sensor.transform.position);yield return Wait(12,()=>pad.Sensor.Active,"Return to A");
            Select(right);yield return Operate("B");yield return Wait(8,()=>Train.Rack.AtEnd,"Recovered transmission opens");
        }

        IEnumerator HoldPad()
        {
            yield return Cut();
            int left=Extreme(true),right=Extreme(false);
            Select(right);Tap(game.Root.TransformPoint(new Vector3(.16f,-.30f,-.18f)));
            Select(left);var pad=Object.FindFirstObjectByType<COgheTapPad>();Tap(pad.Sensor.transform.position);
            yield return Wait(18,()=>pad.Sensor.Active,"Hold the real pad with one fragment");
            Select(right);
        }
        IEnumerator ReleaseMergeExit()
        {
            var pad=Object.FindFirstObjectByType<COgheTapPad>();
            yield return Wait(8,()=>Train.Rack.AtEnd,"Transmission reaches output catch");
            int holder=pad.Actor;Assert.GreaterOrEqual(holder,0);
            Select(holder);Tap(pad.Sensor.transform.position);
            yield return Wait(12,()=>!pad.Sensor.Active,"Recall the load holder");
            Steps(180);Assert.IsTrue(Train.Rack.AtEnd,"Open catch must permit retrieval");
            yield return MergeAndExit();
        }
        IEnumerator SelectorPower()
        {
            Tap(Rail("B").HandPoint);Assert.IsFalse(Rail("B").Busy,"Power lever starts mechanically locked");
            yield return Operate("A");Assert.AreEqual(1,Rail("A").CurrentStop);
            yield return Operate("B");Assert.IsFalse(Train.Powered,"Socket 2 releases power but does not bridge the exit gears");
        }
        [UnityTest] public IEnumerator ChapterSixAssemblesTwoGearsThenRetrievesBothParts()
        {
            yield return Load(6);yield return HoldPad();
            Tap(Rail("C").HandPoint);Assert.IsFalse(Rail("C").Busy);
            yield return Operate("B");Assert.IsFalse(game.FinalExitAvailable);
            yield return Operate("C");yield return ReleaseMergeExit();
        }
        [UnityTest] public IEnumerator ChapterSevenFourStopsPowerConnectionAndWrapAround()
        {
            yield return Load(7);yield return SelectorPower();
            yield return Operate("A");Assert.AreEqual(2,Rail("A").CurrentStop);Assert.IsFalse(Train.Powered);
            yield return Operate("A");Assert.AreEqual(3,Rail("A").CurrentStop);
            yield return Wait(8,()=>Train.Rack.AtEnd,"Fourth socket transmits to exit");
            yield return Operate("A");Assert.AreEqual(0,Rail("A").CurrentStop);
            yield return Wait(8,()=>Train.Rack.Position<.006f,"Wrap removes gear and closes output");
            for(int i=0;i<3;i++)yield return Operate("A");
            yield return Wait(8,()=>Train.Rack.AtEnd,"Reopen after complete physical cycle");yield return Exit();
        }
        [UnityTest] public IEnumerator ChapterEightCoordinatesLoadAndFourStopSelector()
        {
            yield return Load(8);yield return HoldPad();yield return SelectorPower();
            yield return Operate("A");yield return Operate("A");yield return ReleaseMergeExit();
        }
        [UnityTest] public IEnumerator ChapterNineWithdrawsThenRejoinsTheTwoGearTrain()
        {
            yield return Load(9);Tap(Rail("B").HandPoint);Assert.IsFalse(Rail("B").Busy);
            yield return Operate("A");yield return Operate("B");Assert.IsFalse(Train.Powered);
            yield return Operate("A");yield return Wait(8,()=>Train.Rack.AtEnd,"Returning gear bridges both contacts");yield return Exit();
        }
        [UnityTest] public IEnumerator ChapterTenCombinesKnownRulesWithoutHintsAndUnlocksHomeOnce()
        {
            yield return Load(10);Assert.IsTrue(game.Definition.Boss);Assert.IsEmpty(game.GetComponent<COgheTapLesson>().Hint);
            Assert.IsFalse(game.Progress.HomeUnlocked);yield return HoldPad();
            Tap(Rail("C").HandPoint);Assert.IsFalse(Rail("C").Busy);
            yield return SelectorPower();yield return Operate("C");
            yield return Operate("A");yield return Operate("A");
            Assert.IsEmpty(game.GetComponent<COgheTapLesson>().Hint);yield return ReleaseMergeExit();
            Assert.IsTrue(game.Progress.HomeUnlocked);Assert.IsTrue(game.Progress.RevealHome);
            Assert.Contains(game.Definition.Id,game.Progress.Completed);
            game.Progress.RevealHome=false;game.Progress.Win(game.Definition);
            Assert.IsFalse(game.Progress.RevealHome);Assert.AreEqual(1,game.Progress.Completed.Count);
            game.EnterHome();Assert.IsTrue(game.Home);
        }
        [UnityTest] public IEnumerator FourStopBusyTapsPauseCancelAndRetryNeverSkipADetent()
        {
            yield return Load(7);var rail=Rail("A");Tap(rail.HandPoint);
            yield return Wait(20,()=>rail.Phase==COgheTapRail.TaskPhase.Operating&&rail.Rail.Position>.025f,"Pause mid-detent travel");
            for(int i=0;i<8;i++)Tap(rail.HandPoint);
            game.Owner.TogglePause();float pos=rail.Rail.Position;Steps(600);Assert.AreEqual(pos,rail.Rail.Position);game.Owner.TogglePause();
            rail.enabled=false;Steps(60);Assert.AreEqual(0,rail.CompletedJourneys);rail.enabled=true;
            yield return Operate("A");Assert.AreEqual(1,rail.CurrentStop);Steps(600);Assert.AreEqual(1,rail.CompletedJourneys);
            Assert.That(rail.Rail.Position,Is.EqualTo(.08f).Within(rail.Rail.CatchTolerance));
            Tap(rail.HandPoint);Steps(20);game.ResetLevel();Steps(120);
            Assert.AreEqual(0,rail.CurrentStop);Assert.AreEqual(0,rail.CompletedJourneys);Assert.AreEqual(1,rail.NextStop);
            yield return Operate("A");Assert.AreEqual(1,rail.CurrentStop);
        }
        [UnityTest] public IEnumerator SkippingPowerAtTheCamCanBeRecoveredByCycling()
        {
            yield return Load(7);for(int i=0;i<3;i++)yield return Operate("A");
            Assert.IsFalse(game.FinalExitAvailable);Assert.IsFalse(Rail("B").InterlockOpen);
            yield return Operate("A");yield return SelectorPower();
            yield return Operate("A");yield return Operate("A");yield return Wait(8,()=>Train.Rack.AtEnd,"Recovered missed power operation");yield return Exit();
        }
        [UnityTest] public IEnumerator SharedSelectorCannotBeSolvedByWalkingOneBodyOffThePad()
        {
            foreach(int n in new[]{8,10})
            {
                yield return Load(n);yield return Operate("A");
                var pad=Object.FindFirstObjectByType<COgheTapPad>();Tap(pad.Sensor.transform.position);
                yield return Wait(18,()=>pad.Sensor.Active,"Whole body reaches pad");
                Tap(Rail("B").HandPoint);Steps(3000);
                Assert.IsFalse(Rail("B").Busy);Assert.AreEqual(0,Rail("B").CompletedJourneys);
                Assert.IsFalse(game.FinalExitAvailable);Assert.IsFalse(game.Progress.HomeUnlocked);
            }
        }
        [UnityTest] public IEnumerator SelectorHandleStaysPickableAtEveryStopBesideKnifeInPortrait()
        {
            foreach(int n in new[]{8,10})foreach(int height in new[]{1280,1612})
            {
                yield return Load(n);var previous=game.Owner.View.targetTexture;
                var target=new RenderTexture(720,height,24);game.Owner.View.targetTexture=target;viewportHeight=height;
                try
                {
                    for(int stop=1;stop<=4;stop++)
                    {
                        yield return Operate("A");Assert.AreEqual(stop%4,Rail("A").CurrentStop);
                        Assert.IsFalse(game.FinalExitAvailable,"Cycling without the source cannot open the exit");
                    }
                }
                finally{viewportHeight=1280;game.Owner.View.targetTexture=previous;Object.DestroyImmediate(target);}
            }
        }
    }
}
