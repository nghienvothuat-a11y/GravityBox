using System.Collections;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed partial class COgheViewExpansionTests
    {
        private COgheViewExpansionScenario Route => new COgheViewExpansionScenario(game, Tap, Until);
        private T Mechanism<T>() where T : Component => game.Owner.Apparatus.GetComponentInChildren<T>();
        private COgheRailSlider NamedRail(string name) => game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>().Single(r => r.name == name);
        private int Side(bool left) => Enumerable.Range(0,32).OrderBy(i => (left ? 1 : -1) * game.Motion.Centre(i).x).First();
        private void IntactInside()
        {Assert.IsFalse(game.Owner.Lost);Assert.AreEqual(0,game.Matter.EscapedCount);Assert.AreEqual(32,game.Matter.Bodies.Length);}

        [UnityTest] public IEnumerator View11ParkedBridgeHidesExitUntilActuallyDocked()
        {
            yield return Load(11);Assert.IsFalse(game.FinalExitAvailable);
            var rail=Route.Task("A").Rail;var cover=rail.GetComponentsInChildren<VenomSurfacePatch>().Single(s=>s.name=="Bridge underside pocket cover");
            Assert.IsTrue(cover.Shape.Raycast(new Ray(game.Owner.Outlet.position+Vector3.up*.2f,Vector3.down),out _,.25f));
            yield return Route.Operate("A");Assert.IsTrue(game.FinalExitAvailable);
            Assert.IsFalse(cover.Shape.Raycast(new Ray(game.Owner.Outlet.position+Vector3.up*.2f,Vector3.down),out _,.25f));IntactInside();
        }
        [UnityTest] public IEnumerator View12LidReversesAndReopensWithoutReset()
        {
            yield return Load(12);var lid=NamedRail("Passage lid");
            yield return Route.Operate("A");yield return Until(10,()=>lid.AtEnd,"Open the actual lid");
            yield return Route.Operate("A");yield return Until(10,()=>lid.Position<.002f,"Close again");
            yield return Route.Operate("A");yield return Until(10,()=>lid.AtEnd,"Reopen after wrong return");IntactInside();
        }
        [UnityTest] public IEnumerator View13CoveredBridgeCannotBeCommandedUntilCoverClears()
        {
            yield return Load(13);var b=Route.Task("B");yield return Tap(b.HandPoint);yield return Advance(.5f);
            Assert.IsFalse(b.Busy);Assert.AreEqual(0,b.CompletedJourneys);Assert.Less(b.Rail.Position,.002f);
            Assert.IsFalse(b.Request(0),"The physical cover also owns the terminal interlock");
            yield return Route.Operate("A");yield return Route.Operate("B");Assert.IsTrue(b.AtEnd);IntactInside();
        }
        [UnityTest] public IEnumerator View14UncaughtOutputReturnsWhenSourceDisengages()
        {
            yield return Load(14);var drive=Mechanism<COgheViewTransmission>();
            yield return Route.Operate("A");yield return Until(10,()=>drive.First.AtEnd,"Output actually reaches end");
            yield return Advance(.03f);Assert.IsFalse(drive.FirstCaught);Assert.AreEqual(drive.Ready,drive.FirstLamp.sharedMaterial);
            yield return Route.Operate("A");yield return Until(10,()=>drive.First.Position<.002f,"Unlatched output follows disengagement");
            Assert.IsFalse(game.FinalExitAvailable);Assert.AreEqual(drive.Waiting,drive.FirstLamp.sharedMaterial);IntactInside();
        }
        [UnityTest] public IEnumerator View15WrongStationFirstCanBeRecoveredAndBothCatchesPersist()
        {
            yield return Load(15);var drive=Mechanism<COgheViewTransmission>();
            yield return Route.Operate("G");yield return Route.Operate("P");
            yield return Until(12,()=>drive.SecondCaught,"Second station can catch first");Assert.IsFalse(drive.FirstCaught);Assert.Less(drive.First.Position,.002f);
            yield return Route.Operate("P");yield return Route.Operate("G");yield return Route.Operate("P");
            yield return Until(12,()=>drive.FirstCaught,"Return source to the missed station");
            yield return Route.Operate("P");yield return Advance(2);
            Assert.IsTrue(drive.First.AtEnd&&drive.Second.AtEnd);Assert.IsTrue(drive.FirstCaught&&drive.SecondCaught);IntactInside();
            game.ResetLevel();yield return Advance(.5f);Assert.IsFalse(drive.FirstCaught||drive.SecondCaught);
        }
        [UnityTest] public IEnumerator View16SecondDeckHitsRealDogThenRecoversAfterFirstMoves()
        {
            yield return Load(16);var b=Route.Task("B");yield return Tap(b.HandPoint);
            yield return Until(15,()=>b.Phase==COgheTapRail.TaskPhase.Operating,"Reach B handle");
            yield return Until(10,()=>!b.Busy,"Physical dog stalls the wrong first move");
            Assert.AreEqual(0,b.CompletedJourneys);Assert.Less(b.Rail.Position,.035f);
            yield return Route.Operate("A");yield return Route.Operate("B");Assert.IsTrue(b.AtEnd);IntactInside();
        }
        [UnityTest] public IEnumerator View17TransferWorksBothWaysWithoutEscaping()
        {
            yield return Load(17);var tube=Mechanism<COgheTubeNetwork>();
            yield return Route.Tube(tube);IntactInside();yield return Route.Tube(tube,0);IntactInside();
            Assert.Less(game.Motion.Centre(game.Motion.Selected).z,0);
        }
        [UnityTest] public IEnumerator View18WrongBranchReturnsAndOccupiedTubeLocksSelector()
        {
            yield return Load(18);var tube=Mechanism<COgheTubeNetwork>();var a=Route.Task("A");
            yield return Tap(game.Root.TransformPoint(tube.Nodes[0].LocalPosition));
            yield return Until(20,()=>tube.IsParticleInside(0),"Enter initially open main branch route");yield return Advance(.03f);
            Assert.IsTrue(a.Clearance.Blocked);Assert.IsFalse(a.Request(0),"Selector cannot sweep occupied tubing");
            yield return Until(30,()=>tube.AnyWaiting,"Reach real Y junction");
            yield return Tap(game.Root.TransformPoint(Vector3.Lerp(tube.Nodes[1].LocalPosition,tube.Nodes[3].LocalPosition,.35f)));
            yield return Until(35,()=>!tube.IsParticleInside(0),"Wrong branch reaches safe landing");IntactInside();Assert.IsFalse(game.FinalExitAvailable);
            yield return Route.Branch(3,0);yield return Route.Operate("A");yield return Route.Branch(0,2);yield return Route.Operate("B");
            yield return Until(12,()=>Mechanism<COgheViewTransmission>().FirstCaught,"Auxiliary route supplies retained final release");IntactInside();
        }
        [UnityTest] public IEnumerator View19ReturningSelectorWithoutBridgeNeverCreatesOne()
        {
            yield return Load(19);var bridge=NamedRail("Latched return bridge");
            yield return Route.Operate("A");yield return Until(10,()=>NamedRail("Side access shutter").AtEnd,"Side door opens");
            yield return Route.Operate("A");yield return Until(10,()=>NamedRail("Main access shutter").AtEnd,"Main door opens again");
            yield return Until(5,()=>NamedRail("Side access shutter").Position<.002f,"Other physical shutter also finishes returning");Assert.Less(bridge.Position,.002f);Assert.IsFalse(Mechanism<COgheViewTransmission>().FirstCaught);IntactInside();
        }
        [UnityTest] public IEnumerator View20CoverBlocksPowerAndPipeMouthOnlyOpensAfterPower()
        {
            yield return Load(20);var b=Route.Task("B");var inlet=NamedRail("Tube inlet shutter");
            yield return Tap(b.HandPoint);yield return Advance(.5f);Assert.IsFalse(b.Busy);Assert.Less(inlet.Position,.002f);
            yield return Route.Operate("A");Assert.Less(inlet.Position,.002f,"Lid alone does not open pipe");
            yield return Route.Operate("B");yield return Until(12,()=>inlet.AtEnd,"Source drives a finite real mouth shutter");IntactInside();
        }
        [UnityTest] public IEnumerator View21OneJourneyMovesBridgeAndSeparateExitCover()
        {
            yield return Load(21);var cover=NamedRail("Final floor shutter");Assert.IsFalse(game.FinalExitAvailable);
            yield return Route.Operate("A");yield return Until(10,()=>cover.AtEnd,"Linked exit cover follows bridge");
            Assert.IsTrue(Route.Task("A").AtEnd);Assert.IsTrue(game.FinalExitAvailable);IntactInside();
        }
        [UnityTest] public IEnumerator View22UnmergedExitFailsAndRetryRemovesOldCommands()
        {
            yield return Load(22);yield return Route.Cut();int worker=Side(false),holder=Side(true);
            game.SelectFragment(holder);yield return Tap(new Vector3(-.47f,-.30f,.08f));game.SelectFragment(worker);
            yield return Route.Walk(new Vector3(.10f,-.30f,-.12f),"Separate worker from holder");
            Assert.Greater(game.Matter.TotalFragmentCount,1);yield return Tap(game.Owner.Outlet.position);
            yield return Until(30,()=>game.Owner.Lost,"Unmerged real exit is a failure");
            Assert.AreEqual(VenomCampaign.MergeFailure,game.Failure);Assert.IsFalse(game.Owner.Completed);
            game.ResetLevel();yield return Advance(.5f);AssertReset();Assert.IsNull(game.Motion.Get(0));
        }
        [UnityTest] public IEnumerator View11DirectExitCommandCannotSkipFarBank()
        {
            yield return Load(11);yield return Route.Operate("A");float farX=game.Motion.Centre(0).x,farZ=game.Motion.Centre(0).z;
            for(int view=0;view<4;view++)
            {
                yield return Tap(game.Owner.Outlet.position);
                if(game.Motion.Get(0)?.Exit==true)break;
                game.CameraRig.Orbit(-270,720);
            }
            Assert.IsTrue(game.Motion.Get(0)?.Exit==true,"A visible final aperture accepts the direct command");
            for(int tick=0;tick<35/Dt&&!game.Owner.Completed&&!game.Owner.Lost;tick++)
            {
                Tick();farX=Mathf.Max(farX,game.Motion.Centre(0).x);farZ=Mathf.Max(farZ,game.Motion.Centre(0).z);
                if(game.Matter.EscapedCount>0){Assert.Greater(farX,.14f,"Pocket requires the far-bank approach");Assert.Greater(farZ,.10f,"Pocket requires the bridge dock");}
                if(tick%240==0)yield return null;
            }
            Assert.IsFalse(game.Owner.Lost);
        }
        [UnityTest] public IEnumerator View17RetryDuringTransitClearsOwnershipAndRestoresWholeBody()
        {
            yield return Load(17);var tube=Mechanism<COgheTubeNetwork>();yield return Tap(game.Root.TransformPoint(tube.Nodes[0].LocalPosition));
            yield return Until(20,()=>tube.IsParticleInside(0),"Start real transit before Retry");
            game.ResetLevel();yield return Advance(.5f);AssertReset();
            for(int i=0;i<32;i++)Assert.IsFalse(tube.IsParticleInside(i));
            yield return Route.Tube(tube);IntactInside();
        }
        [UnityTest] public IEnumerator View22OffCentreApproachMakesUnequalCuts()
        {
            yield return Load(22);yield return Tap(new Vector3(-.34f,-.30f,-.19f));yield return Until(20,()=>game.Matter.TotalFragmentCount>1,"Actual off-centre approach triggers the blade");var sizes=game.Matter.Groups.GroupBy(g=>g).Select(g=>g.Count()).ToArray();
            Assert.AreEqual(32,sizes.Sum());Assert.AreEqual(2,sizes.Length);Assert.IsTrue(sizes.Any(n=>n!=16),"This off-centre approach must not be normalized to equal halves");
            IntactInside();
        }
        [UnityTest] public IEnumerator View29ObliqueIntakeClearsTheActualDistalMouth()
        {
            yield return Load(29);var coop=Coop;yield return coop.Split();yield return coop.Hold("A",coop.Holder);game.SelectFragment(coop.Worker);
            var tube=Mechanism<COgheTubeNetwork>();int actor=game.Motion.Selected;
            yield return Route.AimTube(tube,0,actor);
            yield return Until(35,()=>tube.IsParticleInside(actor),"One request enters from the oblique floor approach");
            yield return Until(40,()=>!tube.IsParticleInside(actor),"All worker tissue clears receiving mouth");IntactInside();
            Assert.AreEqual(1,tube.LastReachedNode);Assert.Greater(game.Motion.Centre(actor).x,.30f);
        }
        [UnityTest] public IEnumerator View30NewCommandCancelsPendingPipeApproach()
        {
            yield return Load(30);var coop=Coop;yield return coop.Split();yield return coop.Hold("A",coop.Holder);game.SelectFragment(coop.Worker);
            var tube=Mechanism<COgheTubeNetwork>();int actor=game.Motion.Selected;
            yield return Route.AimTube(tube,0,actor);Assert.IsTrue(tube.IsApproachingEntry(actor,0));yield return Advance(.15f);
            yield return Route.Walk(new Vector3(-.34f,-.30f,-.33f),"A new surface command replaces pending pipe intake",.025f);
            yield return Advance(2);Assert.IsFalse(tube.IsParticleInside(actor));Assert.IsFalse(tube.AnyApproaching);IntactInside();
            yield return Route.Tube(tube);IntactInside();
        }


        [UnityTest] public IEnumerator View30DistantExitPlaneCannotFinishIntakeAtSource()
        {
            yield return Load(30);var coop=Coop;yield return coop.Split();yield return coop.Hold("A",coop.Holder);game.SelectFragment(coop.Worker);
            var tube=Mechanism<COgheTubeNetwork>();int actor=game.Motion.Selected;
            // Isolated bent-tube regression fixture: the source is already past the distant mouth's
            // infinite plane. Preserve this case even though R2's authored mouth now points along +X.
            var path=new[]{new Vector3(-.21f,-.255f,-.20f),new Vector3(-.10f,-.19f,-.20f),
                new Vector3(0,-.12f,-.10f),new Vector3(.46f,-.12f,.12f),
                new Vector3(.46f,-.255f,.04f),new Vector3(.46f,-.255f,-.02f),new Vector3(.46f,-.255f,-.08f)};
            var edge=tube.Edges[0];edge.ControlPoints=path;tube.Nodes[1].LocalPosition=path[path.Length-1];
            var mesh=new Mesh();COgheTubeNetwork.BuildSweptMesh(COgheTubeNetwork.SampleCurve(path),tube.Radius,false,mesh);
            edge.Filter.sharedMesh=mesh;edge.Collider.sharedMesh=mesh;tube.Configure(tube.Nodes,tube.Edges,tube.Radius);
            Physics.SyncTransforms();game.Motion.BuildGraph(true);
            Assert.Greater(Vector3.Dot(tube.Nodes[0].LocalPosition-tube.Nodes[1].LocalPosition,Vector3.back),.05f);
            yield return Route.AimTube(tube,0,actor);yield return Until(35,()=>tube.IsParticleInside(actor),"Intake remains assigned to its actual tube");
            yield return Advance(.1f);Assert.IsTrue(tube.IsParticleInside(actor));Assert.Less(game.Motion.Centre(actor).x,0);
            yield return Until(40,()=>!tube.IsParticleInside(actor),"Traverse the complete bent path");
            Assert.Greater(game.Motion.Centre(actor).x,.35f);Assert.AreEqual(1,tube.LastReachedNode);IntactInside();
            Object.Destroy(mesh);
        }

        [UnityTest] public IEnumerator EveryNewKnifeWaitsAsPhysicalBarrierWhilePlayerChooses()
        {
            for(int level=22;level<=30;level++)
            {
                yield return Load(level);yield return Route.Cut();int count=game.Matter.TotalFragmentCount;
                yield return Advance(2);
                Assert.AreEqual(count,game.Matter.TotalFragmentCount,"No timing race at knife in "+level);
                IntactInside();
            }
        }
    }
}
