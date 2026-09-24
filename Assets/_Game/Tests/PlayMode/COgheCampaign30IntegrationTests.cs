using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class COgheCampaign30IntegrationTests
    {
        private static readonly int[] Content =
        {
            1,2,3,4,21,5,6,7,23,30,
            22,8,9,24,11,25,13,26,16,10,
            27,12,28,17,14,29,18,15,19,20
        };
        private static readonly int[] NewSlots = {5,9,10,11,14,16,18,21,23,26};
        private VenomCampaign game;
        private SimulationMode previous;

        [UnitySetUp] public IEnumerator Before()
        {
            previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            VenomCampaignSave.PersistenceEnabled=false;yield return null;
        }
        [UnityTearDown] public IEnumerator After()
        {
            Time.timeScale=1;Physics.simulationMode=previous;VenomCampaignSave.PersistenceEnabled=true;yield return null;
        }

        private IEnumerator Load(int slot)
        {
            void Loaded(Scene scene,LoadSceneMode mode)
            {
                game=Object.FindFirstObjectByType<VenomCampaign>();
                if(game==null)return;game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            }
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"COgheOrigin{slot:00}");}
            finally{SceneManager.sceneLoaded-=Loaded;}
            Assert.NotNull(game,$"Integrated scene {slot:00}");
        }

        private void Steps(int count)
        {
            for(int i=0;i<count;i++)
            {game.Owner.Step(1f/120);game.Owner.Rotation.Step(1f/120);Physics.Simulate(1f/120);}
        }

        [UnityTest] public IEnumerator GearHandleKeepsItsGripThroughOneUpwardCommand()
        {
            yield return Load(9);Steps(60);
            game.Owner.View.aspect=720f/1280;game.CameraRig.Frame(720,1280,0,true);
            var prop=Array.Find(game.Props,p=>p.name=="G sliding gear");
            var rail=prop.GetComponent<COgheRailSlider>();
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(prop.ManipulationGrip.position));
            for(int t=0;t<3000&&!game.Attached;t++){Steps(1);if(t%240==0)yield return null;}
            Debug.Log($"GEAR APPROACH held={game.Attached} body={game.Motion.Centre(0):F4}");
            COgheExpansionIntegrationTests.Capture(game,"gear09-approach");
            Assert.IsTrue(game.Attached,"A real tap must reach and grasp G.");
            Vector3 target=rail.Frame.TransformPoint(rail.Start+rail.Axis*(rail.Travel+.10f));
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(target));
            bool released=false;float maxSeparation=0,maxSpeed=0;int backwards=0;
            for(int t=0;t<330;t++)
            {
                Steps(1);released|=!game.Attached;
                Vector3 velocity=Vector3.zero;int feet=0;
                for(int i=0;i<32;i++){velocity+=game.Matter.Bodies[i].linearVelocity/32;if(game.Motion.HasGrip(i))feet++;}
                maxSeparation=Mathf.Max(maxSeparation,Vector3.Distance(game.Motion.Centre(0),prop.ManipulationGrip.position));
                maxSpeed=Mathf.Max(maxSpeed,velocity.magnitude);
                if(t>30&&Vector3.Dot(velocity,rail.WorldAxis)<-.06f)backwards++;
                if(t%60==0)Debug.Log($"GEAR GRASP t={t} held={game.Attached} pos={rail.Position:F4} body={game.Motion.Centre(0):F4} velocity={velocity:F4} feet={feet}");
                if(t%120==0)yield return null;
            }
            Debug.Log($"GEAR GRASP RESULT released={released} rail={rail.Position:F4} separation={maxSeparation:F4} speed={maxSpeed:F4} backwards={backwards}");
            COgheExpansionIntegrationTests.Capture(game,"gear09-grasp");
            Assert.IsFalse(released,"One upward command must keep the handle, without reselecting after a fall.");
            Assert.IsTrue(rail.AtEnd,"A .16 m rail must finish a single stroke before the 3 s idle release.");
            Assert.Less(backwards,12,"The body must not repeatedly drop while pushing up.");
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            game.SetPropTarget(rail.Frame.TransformPoint(rail.Start-rail.Axis*.10f));
            for(int t=0;t<330;t++){Steps(1);Assert.IsTrue(game.Attached,"Reverse pull retains the same handhold.");}
            Assert.Less(rail.Position,.004f,"A reverse instruction releases the catch and pulls G back down.");
            Steps(40);Assert.IsFalse(game.Attached,"Three seconds without another instruction release the hand.");
        }

        [UnityTest] public IEnumerator OtherGearLevelUsesTheSameStablePushAndPull()
        {
            foreach(string name in new[]{"A sliding bearing","B sliding bearing"})
            {
                yield return Load(24);Steps(60);
                var prop=Array.Find(game.Props,p=>p.name==name);var rail=prop.GetComponent<COgheRailSlider>();
                if(name.StartsWith("B "))
                {
                    // The two guarded bearings have separate access aisles.
                    // Use the player's floor waypoint to reach B's right side,
                    // as in the authored full solution, without moving tissue.
                    Vector3 aisle=game.Root.TransformPoint(new Vector3(.129f,-.277f,-.11f));
                    game.Motion.Move(0,aisle);
                    for(int t=0;t<3600&&Vector3.Distance(game.Motion.Centre(0),aisle)>.04f;t++){Steps(1);if(t%240==0)yield return null;}
                    Assert.Less(Vector3.Distance(game.Motion.Centre(0),aisle),.04f,"Walk around the guard to B's aisle.");
                    Vector3 cheek=game.Root.TransformPoint(new Vector3(.129f,rail.Start.y+rail.InitialTravel,.068f));
                    game.Motion.Move(0,cheek);
                    for(int t=0;t<3600&&Vector3.Distance(game.Motion.Centre(0),cheek)>.032f;t++){Steps(1);if(t%240==0)yield return null;}
                    Assert.Less(Vector3.Distance(game.Motion.Centre(0),cheek),.032f,"Climb B's exposed grip cheek before grasping.");
                }
                game.SelectProp(prop);
                for(int t=0;t<3600&&!game.Attached;t++){Steps(1);if(t%240==0)yield return null;}
                Debug.Log($"GEAR OTHER {name} held={game.Attached} body={game.Motion.Centre(0):F4}");
                Assert.IsTrue(game.Attached,"Approach "+name);
                bool upward=name.StartsWith("A ");
                game.SetPropTarget(rail.Frame.TransformPoint(rail.Start+rail.Axis*(upward?rail.Travel+.10f:-.10f)));
                for(int t=0;t<330;t++)
                {
                    Steps(1);Assert.IsTrue(game.Attached,"One continuous stroke without falling/re-grasping: "+name);
                    if(t%120==0)yield return null;
                }
                Assert.That(rail.Position,Is.EqualTo(upward?rail.Travel:0).Within(.004f),"Finish "+name);
                Assert.AreEqual(1,game.Matter.TotalFragmentCount);
                game.ResetLevel();Steps(60);Assert.IsFalse(game.Attached,"Reset clears the shared grasp state.");
            }
        }

        [UnityTest] public IEnumerator SlotSixInvertedSlipperyFloorKeepsTheTappedDestination()
        {
            yield return Load(6);Steps(60);
            var slick=Array.Find(game.Surfaces,p=>p.Slippery);
            Assert.NotNull(slick);
            game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(0,0,180));Steps(600);
            game.Owner.View.aspect=720f/1280;game.CameraRig.Frame(720,1280,0,true);
            Vector3 hit=slick.transform.TransformPoint(new Vector3(.13f,-.12f,0));
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(hit));
            Assert.AreSame(slick,game.Feedback.CommandSurface,"The flipped slippery roof is the floor the user tapped.");
            var command=game.Motion.Get(0);Assert.NotNull(command);
            Vector3 goal=game.Root.TransformPoint(command.Target);
            Assert.Less(Vector3.Distance(goal,hit+slick.Normal*.019f),.002f);
            var cached=new List<Vector3>();var rebuilt=new List<Vector3>();
            int builds=game.Motion.GraphBuildCount;
            game.Motion.FindPath(game.Motion.Centre(0),goal,cached,true);
            Assert.AreEqual(builds,game.Motion.GraphBuildCount,"Rotation must reuse the local surface graph.");
            game.Motion.BuildGraph(true);
            game.Motion.FindPath(game.Motion.Centre(0),goal,rebuilt,true);
            Assert.AreEqual(rebuilt.Count,cached.Count,"Cached routing after inversion must choose the same approach as fresh geometry, not the original world-space floor.");
            for(int i=0;i<cached.Count;i++)Assert.Less(Vector3.Distance(cached[i],rebuilt[i]),.0001f,$"Rotated route waypoint {i}");
            bool reachedSlick=false;
            for(int tick=0;tick<2400&&!reachedSlick;tick++)
            {
                Steps(1);
                for(int particle=0;particle<32;particle++)
                    if(game.Motion.Support(particle,out var shape,out _,out _)&&shape==slick.Shape)reachedSlick=true;
                if(tick%240==0)yield return null;
            }
            Assert.IsTrue(reachedSlick,"After the tap, the body must actually approach the selected slippery floor from its wall.");
            COgheExpansionIntegrationTests.Capture(game,"campaign30-06-inverted-floor-command");
        }

        [UnityTest] public IEnumerator CatalogUsesStableContentIdsAndBossesAtTenTwentyThirty()
        {
            Assert.AreEqual(60,VenomCampaign.LevelCount);
            Assert.AreEqual(6,COgheDayLabPresentation.LevelPageCount,"The Day Lab selector must expose all six campaign pages");
            var seen=new HashSet<string>();
            for(int slot=1;slot<=30;slot++)
            {
                yield return Load(slot);
                Assert.AreEqual(slot,game.Definition.Order,$"display order {slot}");
                Assert.AreEqual($"venom.origin.{Content[slot-1]:00}",game.Definition.Id,$"stable content id {slot}");
                Assert.AreEqual(slot%10==0,game.Definition.Boss,$"boss position {slot}");
                Assert.IsTrue(seen.Add(game.Definition.Id),$"duplicate id {game.Definition.Id}");
                Assert.AreEqual((slot-1)/10,game.LevelPage,$"page {slot}");
            }
        }

        [UnityTest] public IEnumerator FirstTwoCampaignSlotsShowTheExitLessonArrow()
        {
            foreach(int slot in new[]{1,2,3})
            {
                yield return Load(slot);yield return null;game.Feedback.Refresh();
                var hint=Array.Find(game.GetComponentsInChildren<LineRenderer>(true),line=>line.name=="Lesson arrow");
                Assert.NotNull(hint,$"Campaign slot {slot} must contain the shared lesson arrow renderer.");
                Assert.AreEqual(slot<=2,hint.enabled,$"Campaign slot {slot} exit lesson arrow visibility.");
            }
        }

        [UnityTest] public IEnumerator AllCampaignExitOutlinesUseTheFlushMintStyleWithoutCollision()
        {
            for(int slot=1;slot<=30;slot++)
            {
                yield return Load(slot);yield return null;
                var owner=game.GetComponent<VenomLevelController>();
                var rim=Array.Find(owner.Outlet.GetComponentsInChildren<LineRenderer>(true),line=>line.name=="Fine rim");
                Assert.NotNull(rim,$"Campaign slot {slot} must retain its authored circular exit rim.");
                Assert.IsTrue(rim.enabled,$"Campaign slot {slot} exit outline must be visible.");
                Assert.AreSame(owner.IndicatorMaterial,rim.sharedMaterial,$"Campaign slot {slot} exit outline must use quiet mint.");
                Assert.AreEqual(COgheDayLabPresentation.ExitOutlineWidth,rim.startWidth,.00001f,$"Campaign slot {slot} exit outline width.");
                Assert.AreEqual(COgheDayLabPresentation.ExitOutlineWidth,rim.endWidth,.00001f,$"Campaign slot {slot} exit outline width.");
                Assert.IsEmpty(rim.GetComponents<Collider>(),$"Campaign slot {slot} exit outline must remain presentation-only.");
                Assert.AreEqual(ShadowCastingMode.Off,rim.shadowCastingMode,$"Campaign slot {slot} exit outline must not add a shadow pass.");
                if(slot==2)COgheExpansionIntegrationTests.Capture(game,"exit-outline-02");
            }
        }

        [UnityTest] public IEnumerator SlotFourFirstHoleTapTeachesSlipBeforeTheSafeDetour()
        {
            yield return Load(4);yield return null;Steps(60);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            var order=game.Motion.Get(0);Assert.NotNull(order);Assert.IsTrue(order.Exit);Assert.IsFalse(order.AvoidSlippery);
            bool touched=false,fell=false;float highest=float.NegativeInfinity;
            for(int t=0;t<2400&&!game.Owner.Completed&&!game.Owner.Lost;t++)
            {
                Steps(1);int slick=0;
                for(int i=0;i<32;i++)
                    if(game.Motion.Support(i,out var collider,out var point,out _)&&
                       collider.GetComponent<VenomSurfacePatch>() is VenomSurfacePatch patch&&!patch.Grip(point))slick++;
                if(slick>2)touched=true;
                float y=game.Motion.Centre(0).y;if(touched){highest=Mathf.Max(highest,y);fell|=highest-y>.09f;}
                if(t%240==0)yield return null;if(fell)break;
            }
            Assert.IsTrue(touched,"Shipping slot 04 must enter the visible slippery coating after the first hole tap.");
            Assert.IsTrue(fell,"Shipping slot 04 must visibly fall before the player chooses a dry detour.");
            Assert.IsFalse(game.Owner.Completed,"The teaching attempt must not finish slot 04.");
            COgheExpansionIntegrationTests.Capture(game,"campaign30-04-first-tap-slip");
        }

        [UnityTest] public IEnumerator NewLevelsLoadResetIdleAndExposeTheirAuthoredMechanisms()
        {
            foreach(int slot in NewSlots)
            {
                yield return Load(slot);
                foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())
                    foreach(var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                        Assert.NotNull(behaviour,$"Missing script in slot {slot}: {root.name}");
                Assert.Greater(game.Surfaces.Length,5,$"authored geometry {slot}");
                foreach(var surface in game.Surfaces)
                {
                    Assert.NotNull(surface.Shape,$"collider {slot}/{surface.name}");
                    if(surface.Slippery||surface.HasSlipRegion)Assert.IsTrue(surface.Selectable,$"slick target {slot}/{surface.name}");
                }
                game.ResetLevel();Physics.SyncTransforms();Steps(180);
                Assert.IsFalse(game.Owner.Completed,$"idle solution {slot}");
                Assert.IsFalse(game.Owner.Lost,$"idle failure {slot}: {game.Failure}");
                Assert.AreEqual(0,game.Matter.EscapedCount,$"escaped at idle {slot}");
                Assert.AreEqual(1,game.Matter.TotalFragmentCount,$"fragment at idle {slot}");
                COgheExpansionIntegrationTests.Capture(game,$"campaign30-{slot:00}-overview");
            }
        }

        [UnityTest] public IEnumerator NewLevelMechanismContractsMatchTheApprovedDescriptions()
        {
            yield return Load(5);Assert.IsTrue(game.Definition.CanRotate);Assert.AreEqual(0,game.Mechanisms.Length);
            yield return Load(9);Assert.NotNull(Object.FindFirstObjectByType<COgheGearTrain>());Assert.IsFalse(game.FinalExitAvailable);
            yield return Load(10);Assert.IsTrue(game.Definition.Boss);Assert.NotNull(Object.FindFirstObjectByType<COgheSequentialWinch>());Assert.NotNull(Object.FindFirstObjectByType<COgheGearTrain>());
            yield return Load(11);Assert.NotNull(game.Tube);Assert.IsTrue(game.Tube.AutoEnterOnContact);Assert.IsFalse(game.Definition.CanRotate);
            yield return Load(14);Assert.NotNull(Object.FindFirstObjectByType<COgheExitRailLock>());Assert.IsFalse(game.FinalExitAvailable);
            yield return Load(16);Assert.NotNull(Object.FindFirstObjectByType<COgheSequentialWinch>());Assert.IsFalse(game.FinalExitAvailable);
            yield return Load(18);Assert.NotNull(Object.FindFirstObjectByType<COgheDockedBridgeDeck>());Assert.IsNull(Object.FindFirstObjectByType<COgheExitRailLock>());Assert.IsTrue(game.FinalExitAvailable,"The partition and alignment plate physically close this route.");
            yield return Load(21);Assert.NotNull(Object.FindFirstObjectByType<COgheDualDockTransmission>());Assert.IsFalse(game.FinalExitAvailable);
            yield return Load(23);Assert.NotNull(Array.Find(game.Props,p=>p.name.Contains("blocking crate")));Assert.NotNull(Object.FindFirstObjectByType<COgheGearTrain>());
            yield return Load(26);Assert.NotNull(Object.FindFirstObjectByType<COgheGuillotine>());Assert.NotNull(Object.FindFirstObjectByType<COgheCooperativeWinch>());Assert.IsFalse(game.FinalExitAvailable);
        }

        [UnityTest] public IEnumerator RailAndSequentialMachinesReachVisiblePhysicalSolvedStatesAndReset()
        {
            foreach(int slot in new[]{14})
            {
                yield return Load(slot);var lockMechanism=Object.FindFirstObjectByType<COgheExitRailLock>();var rail=lockMechanism.Rail;
                rail.Body.position=rail.Frame.TransformPoint(rail.Start+rail.Axis.normalized*rail.Travel);Physics.SyncTransforms();Steps(3);
                Assert.IsTrue(lockMechanism.ExitUnlocked,$"rail unlock {slot}");Assert.IsTrue(game.FinalExitAvailable,$"exit unlock {slot}");
                game.ResetLevel();Physics.SyncTransforms();Assert.IsFalse(lockMechanism.ExitUnlocked,$"rail reset {slot}");
            }
            foreach(int slot in new[]{16,10})
            {
                yield return Load(slot);var sequence=Object.FindFirstObjectByType<COgheSequentialWinch>();
                sequence.Access.Body.position=sequence.Access.Frame.TransformPoint(sequence.Access.Start+sequence.Access.Axis.normalized*sequence.Access.Travel);
                sequence.Door.Body.position=sequence.Door.Frame.TransformPoint(sequence.Door.Start+sequence.Door.Axis.normalized*sequence.Door.Travel);
                Physics.SyncTransforms();Steps(3);
                Assert.IsTrue(sequence.AccessOpen,$"access {slot}");Assert.IsTrue(sequence.Complete,$"door catch {slot}");Assert.IsTrue(game.FinalExitAvailable,$"full exit {slot}");
                game.ResetLevel();Physics.SyncTransforms();Assert.IsFalse(sequence.Complete,$"sequence reset {slot}");
            }
        }

        [UnityTest] public IEnumerator IntendedFirstControlsAreReachableByRealScreenTaps()
        {
            var expected=new Dictionary<int,string>
            {
                {9,"G sliding gear"},{10,"G factory carriage"},{14,"A sliding exit cover"},
                {16,"A access cover"},{18,"Single bridge module"},{21,"G reusable carriage"},
                {23,"Amber blocking crate"}
            };
            var approach=typeof(VenomCampaign).GetField("approachProp",BindingFlags.Instance|BindingFlags.NonPublic);
            foreach(var pair in expected)
            {
                yield return Load(pair.Key);var prop=Array.Find(game.Props,p=>p.name==pair.Value);Assert.NotNull(prop,$"first control {pair.Key}");
                bool reached=false;
                foreach(var shape in prop.CollisionShapes)
                {
                    Vector3 screen=game.Owner.View.WorldToScreenPoint(shape.bounds.center);if(screen.z<=0)continue;
                    game.TouchPoint(screen);
                    if(ReferenceEquals(approach.GetValue(game),prop)){reached=true;break;}
                    game.ReleaseProp();
                }
                Assert.IsTrue(reached,$"Slot {pair.Key}: first required control {pair.Value} must be selectable from the overview camera");
            }
        }

        [UnityTest] public IEnumerator GearAndDualDockSolutionsDriveTheirRealShuttersToCatches()
        {
            foreach(int slot in new[]{9,23})
            {
                yield return Load(slot);var train=Object.FindFirstObjectByType<COgheGearTrain>();
                var carriage=Array.Find(game.Props,p=>p.name.Contains("G ")).GetComponent<COgheRailSlider>();
                carriage.Body.position=carriage.Frame.TransformPoint(carriage.Start+carriage.Axis.normalized*carriage.Travel);Physics.SyncTransforms();
                Steps(520);Assert.IsTrue(train.Meshed,$"gear contact {slot}");Assert.IsTrue(train.Rack.AtEnd,$"rack catch {slot}");Assert.IsTrue(game.FinalExitAvailable,$"gear solution {slot}");
            }
            yield return Load(21);var dual=Object.FindFirstObjectByType<COgheDualDockTransmission>();
            dual.Carriage.Body.position=dual.Carriage.Frame.TransformPoint(dual.Carriage.Start);Physics.SyncTransforms();Steps(650);
            Assert.IsTrue(dual.StageAComplete,"station A must physically clear the rail");
            dual.Carriage.Body.position=dual.Carriage.Frame.TransformPoint(dual.Carriage.Start+dual.Carriage.Axis.normalized*dual.Carriage.Travel);Physics.SyncTransforms();Steps(520);
            Assert.IsTrue(dual.StageBComplete,"station B must physically open the exit");Assert.IsTrue(game.FinalExitAvailable);
        }

        [UnityTest] public IEnumerator DualDockRequiresCoplanarPitchContactAndKeepsTeethInPhase()
        {
            yield return Load(21);var dual=Object.FindFirstObjectByType<COgheDualDockTransmission>();
            Quaternion initialG=dual.CarriageWheel.localRotation;
            var wheel=dual.StationAWheels[0];Vector3 authored=wheel.localPosition;Quaternion rotation=wheel.localRotation;
            dual.Carriage.Body.position=dual.Carriage.Frame.TransformPoint(dual.Carriage.Start);Physics.SyncTransforms();
            wheel.localPosition+=Vector3.forward*.02f;Steps(120);
            Assert.IsFalse(dual.AtA,"A rail stop alone cannot transmit across an axial gap");
            Assert.IsFalse(dual.StageAComplete);Assert.Less(dual.AccessGate.Position,.002f);
            wheel.localPosition=authored;wheel.localRotation=Quaternion.Euler(25,0,0);Steps(120);
            Assert.IsFalse(dual.AtA,"Nonparallel shafts cannot transmit");
            wheel.localRotation=rotation;Physics.SyncTransforms();
            for(int step=0;step<240;step++)
            {
                Steps(1);Assert.IsTrue(dual.AtA);
                foreach(var fixedWheel in dual.StationAWheels)
                {
                    Assert.IsTrue(COgheGearTrain.PitchContact(dual.CarriageWheel.position,fixedWheel.position,dual.CarriageWheel.forward,dual.PitchRadius,dual.PitchRadius,dual.MeshTolerance));
                    Vector3 line=dual.CarriageWheel.InverseTransformPoint(fixedWheel.position);
                    float pitch=360f/dual.ToothCount;
                    float relative=(Quaternion.Inverse(dual.CarriageWheel.rotation)*fixedWheel.rotation).eulerAngles.z;
                    float phase=2*Mathf.Atan2(line.y,line.x)*Mathf.Rad2Deg+180-pitch*.5f;
                    Assert.Less(Mathf.Abs(Mathf.Repeat(relative-phase+pitch*.5f,pitch)-pitch*.5f),.1f,"Both fixed gears must face a tooth gap on G throughout rotation");
                }
            }
            float raised=dual.AccessGate.Position;
            Assert.Greater(raised,.035f);Assert.IsFalse(dual.StageAComplete);
            dual.Carriage.Body.position=dual.Carriage.Frame.TransformPoint(dual.Carriage.Start+Vector3.right*.045f);Physics.SyncTransforms();Steps(90);
            Assert.IsFalse(dual.AtA);Assert.IsFalse(dual.AtB);Assert.IsFalse(dual.StageAComplete);
            Assert.Less(Mathf.Abs(dual.AccessGate.Position-raised),.004f,"Unmeshing brakes the partially raised load");
            game.ResetLevel();Assert.Less(Quaternion.Angle(initialG,dual.CarriageWheel.localRotation),.001f);
            Assert.IsFalse(dual.StageAComplete);Assert.IsFalse(dual.StageBComplete);
        }

        [UnityTest] public IEnumerator NewLevelSimulationBudgetHasNoManagedAllocationGrowthOrRunawayFrameCost()
        {
            foreach(int slot in NewSlots)
            {
                yield return Load(slot);game.ResetLevel();Physics.SyncTransforms();Steps(60);
                long before=GC.GetTotalMemory(false);var watch=Stopwatch.StartNew();Steps(600);watch.Stop();long after=GC.GetTotalMemory(false);
                double milliseconds=watch.Elapsed.TotalMilliseconds/600.0;
                Debug.Log($"COGHE30 PERF slot={slot:00} content={game.Definition.Id} avgStepMs={milliseconds:F4} managedDelta={after-before} surfaces={game.Surfaces.Length} props={game.Props.Length} mechanisms={game.Mechanisms.Length}");
                Assert.Less(milliseconds,2.5,$"simulation CPU regression in slot {slot}");
                Assert.Less(after-before,2_000_000,$"runaway managed growth in slot {slot}");
                yield return null;
            }
        }
    }
}
