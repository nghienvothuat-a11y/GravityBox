#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Rendering;

namespace GravityBox.Tests
{
    public sealed class VenomPrototypeTests
    {
        private VenomLevelController level;
        private SimulationMode mode; private Vector3 gravity;
        private const float Dt=1f/120;
        [UnitySetUp] public IEnumerator Setup()
        {
            mode=Physics.simulationMode;gravity=Physics.gravity;Physics.simulationMode=SimulationMode.Script;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Venom/Venom01.unity",new LoadSceneParameters(LoadSceneMode.Single));
            level=Object.FindFirstObjectByType<VenomLevelController>();Assert.That(level,Is.Not.Null);
            level.enabled=false;level.Rotation.enabled=false;level.GetComponent<VenomInput>().enabled=false;
            level.ResetExperiment();
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;Physics.simulationMode=mode;Physics.gravity=gravity;Time.timeScale=1;
        }
        private void Steps(int count)
        {
            for(int i=0;i<count;i++){level.Step(Dt);level.Rotation.Step(Dt);Physics.Simulate(Dt);}
        }
        [Test] public void MatterKeepsMassAndCohesionAtRest()
        {
            Steps(360);var matter=level.Organism;
            Debug.Log($"VENOM REST velocity={matter.Bodies.Max(b=>b.linearVelocity.magnitude)} y={matter.Bodies.Min(b=>b.position.y)}");
            Assert.That(matter.TotalMass,Is.EqualTo(.096f).Within(.00001f));
            Assert.That(matter.FragmentCount,Is.EqualTo(1));Assert.That(matter.EscapedCount,Is.Zero);
            Assert.That(matter.Bodies.All(b=>!b.isKinematic),Is.True);
            Assert.That(matter.Bodies.Max(b=>b.linearVelocity.magnitude),Is.LessThan(.08f));
            Assert.That(matter.Bodies.Min(b=>b.position.y),Is.GreaterThan(-.069f));
            matter.GetComponent<VenomSurface>().Rebuild();Assert.That(matter.GetComponent<VenomSurface>().VertexCount,Is.GreaterThan(200));
        }
        [Test] public void FastFragmentsCannotCrossSolidFloor()
        {
            // Both game-scale falls and impacts beyond the energy of one box-height
            // drop. Initial conditions only; subsequent movement is entirely PhysX.
            foreach(float speed in new[]{2f,5f,10f})
            {
                level.ResetExperiment();
                var matter=level.Organism;var box=level.Rotation.transform;
                matter.Cut(level.Spawn,new Vector3(.003f,.06f,.055f));
                Vector3 centre=matter.Bodies.Aggregate(Vector3.zero,(sum,b)=>sum+b.position)/32;
                foreach(var body in matter.Bodies)
                {
                    body.position+=new Vector3(.115f,.018f,-.015f)-centre;
                    body.linearVelocity=Vector3.down*speed;
                }
                Physics.SyncTransforms();
                for(int frame=0;frame<120;frame++)
                {
                    Steps(1);
                    for(int i=0;i<32;i++)
                    {
                        Vector3 p=box.InverseTransformPoint(matter.Bodies[i].position);
                        Assert.That(p.y,Is.GreaterThan(-.073f),$"Floor crossed at {speed} m/s, step {frame}, particle {i}, p={p:F5}");
                    }
                    if(frame==20)
                    {
                        matter.GetComponent<VenomSurface>().Rebuild(false);
                        var skin=matter.GetComponentsInChildren<MeshFilter>().First(m=>m.name=="Continuous wet skin");
                        float lowestSkin=skin.sharedMesh.vertices.Min(v=>box.InverseTransformPoint(skin.transform.TransformPoint(v)).y);
                        Debug.Log($"IMPACT {speed}m/s: lowest particle={matter.Bodies.Min(b=>box.InverseTransformPoint(b.position).y):F5}, skin={lowestSkin:F5}");
                        Capture($"impact-{speed}");
                        Assert.That(lowestSkin,Is.GreaterThanOrEqualTo(-.0671f),"The visible skin must stay above solid floor as well as the physical nodes.");
                    }
                }
            }
        }
        [Test] public void RepeatedFullBoxTurnsKeepMatterInsideShell()
        {
            var matter=level.Organism;var box=level.Rotation.transform;
            var orientations=new[]{new Vector3(180,0,0),Vector3.zero,new Vector3(0,0,180),Vector3.zero,
                new Vector3(180,90,90),new Vector3(0,180,-90),new Vector3(180,0,-90),Vector3.zero};
            for(int tick=0;tick<orientations.Length*480;tick++)
            {
                level.Rotation.SetTargetOrientation(Quaternion.Euler(orientations[tick/480]));
                Steps(1);
                for(int i=0;i<32;i++)
                {
                    Vector3 p=box.InverseTransformPoint(matter.Bodies[i].position);
                    Assert.That(Mathf.Abs(p.x),Is.LessThan(.268f),$"Side crossed: tick {tick}, particle {i}, p={p:F5}");
                    Assert.That(Mathf.Abs(p.z),Is.LessThan(.338f),$"End crossed: tick {tick}, particle {i}, p={p:F5}");
                    Assert.That(p.y,Is.InRange(-.073f,.085f),$"Floor/cover crossed: tick {tick}, particle {i}, p={p:F5}");
                }
            }
        }
        [Test] public void ClosedMechanismsStopAboveFloor()
        {
            Steps(60);level.Rotation.SetTargetOrientation(Quaternion.Euler(-16,0,0));
            for(int tick=0;tick<360;tick++)
            {
                Steps(1);
                if(tick<60)continue;
                foreach(var mechanism in new[]{level.Blade,level.Gate})
                {
                    var shape=mechanism.GetComponentInChildren<BoxCollider>();
                    for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                    {
                        Vector3 corner=shape.center+Vector3.Scale(shape.size,new Vector3(x,-1,z))*.5f;
                        Vector3 local=level.Rotation.transform.InverseTransformPoint(shape.transform.TransformPoint(corner));
                        Assert.That(local.y,Is.GreaterThan(-.0675f),$"{mechanism.name} must not press through the floor: {local.y:F5}");
                    }
                }
            }
            Assert.That(level.BladeReleased,Is.True,"Exercise the dropped knife as well as its raised waiting position.");
        }
        [TestCase(0f,12f)]
        [TestCase(-.018f,14f)]
        [TestCase(.018f,14f)]
        [TestCase(-.025f,16f)]
        [TestCase(.025f,16f)]
        public void GentleTiltCutsWithoutPreciseAlignment(float offset,float angle)
        {
            var matter=level.Organism;
            foreach(var body in matter.Bodies)body.position+=Vector3.right*offset;
            Physics.SyncTransforms();Steps(60);
            level.Rotation.SetTargetOrientation(Quaternion.Euler(-angle,0,0));
            int peak=1;
            for(int tick=0;tick<360;tick++)
            {
                Steps(1);peak=Mathf.Max(peak,matter.FragmentCount);
                if(matter.CutCount>0&&matter.FragmentCount>1)
                {
                    Debug.Log($"GENTLE CUT offset={offset:F3} tilt={angle} time={tick*Dt:F2} groups={string.Join(",",matter.Groups.GroupBy(g=>g).Select(g=>g.Count()))}");
                    break;
                }
            }
            Assert.That(peak,Is.GreaterThan(1),$"A {angle} degree tilt, offset {offset*1000} mm, should divide matter at the knife without rocking the box repeatedly.");
            Assert.That(matter.CutCount,Is.GreaterThan(0));
            Assert.That(matter.TotalMass,Is.EqualTo(.096f).Within(.000001f));
            Assert.That(level.Lost,Is.False);
        }
        [Test] public void TiltKnifeWaitsForMatterAndRearmsAfterAMissedPass()
        {
            Steps(180);
            Assert.That(level.BladeReleased,Is.False,"Do not drop the blade before the player approaches.");
            Assert.That(level.Blade.position.y-.06f,Is.GreaterThan(0),"The raised knife must leave room to slide beneath it.");
            Assert.That(level.Organism.CutCount,Is.Zero);

            // A glancing pass lies entirely on one side of the cutting plane.
            // It may release the knife, but must not split matter at a distance.
            var matter=level.Organism;
            level.ResetExperiment();
            foreach(var body in matter.Bodies)body.position+=new Vector3(.037f,0,-.08f);
            Physics.SyncTransforms();Steps(1);
            Assert.That(level.BladeReleased,Is.True);
            foreach(var body in matter.Bodies)
            {body.position+=new Vector3(.09f,0,.08f);body.linearVelocity=Vector3.zero;}
            Physics.SyncTransforms();Steps(360);
            Assert.That(matter.FragmentCount,Is.EqualTo(1));Assert.That(matter.CutCount,Is.Zero);
            Assert.That(level.BladeReleased,Is.False,"A missed pass should leave another chance without resetting the level.");
            Assert.That(level.Blade.position.y-.06f,Is.GreaterThan(0));
        }
        [Test] public void NaturalTiltCutsOpensMergesAndEscapes()
        {
            Steps(60);Capture("01-intact");level.Rotation.SetTargetOrientation(Quaternion.Euler(-16,0,0));
            int peak=1;bool capturedSplit=false,capturedGate=false,capturedMerge=false,capturedExit=false;
            for(int s=0;s<40&&!level.Completed;s++)
            {
                for(int tick=0;tick<120&&!level.Completed;tick++)
                {
                    if(level.GateLatched)
                    {
                        var box=level.Rotation.transform;
                        var active=level.Organism.Bodies.Where((b,i)=>!level.Organism.Escaped[i]).ToArray();
                        var p=active.Aggregate(Vector3.zero,(sum,b)=>sum+box.InverseTransformPoint(b.position))/active.Length;
                        var root=level.Rotation.GetComponent<Rigidbody>();
                        var v=active.Aggregate(Vector3.zero,(sum,b)=>sum+box.InverseTransformDirection(b.linearVelocity-root.GetPointVelocity(b.position)))/active.Length;
                        float targetZ=level.Organism.MergeCount==0?-.205f:-.235f;
                        float pitch=level.Organism.MergeCount==0?-20:Mathf.Clamp((targetZ-p.z)*100-v.z*45,-16,16);
                        // Keep tilting through the reunion cheeks. A proportional
                        // angle alone decays below static friction before the
                        // whole body reaches the short-range aperture assist.
                        if(level.Organism.MergeCount>0&&p.z>-.205f) pitch=Mathf.Min(pitch,-26);
                        level.Rotation.SetTargetOrientation(Quaternion.Euler(pitch,0,Mathf.Clamp(p.x*80+v.x*40,-14,14)));
                    }
                    Steps(1);peak=Mathf.Max(peak,level.Organism.FragmentCount);
                    if(!capturedSplit&&level.Organism.FragmentCount==2){capturedSplit=true;Capture("02-cut");}
                    if(!capturedGate&&level.GateOpening>.11f){capturedGate=true;Capture("03-gate");}
                    if(!capturedMerge&&level.Organism.MergeCount>0){capturedMerge=true;Capture("04-fusion");}
                    if(!capturedExit&&level.Organism.EscapedCount>10){capturedExit=true;Capture("05-exiting");}
                }
                var centre=level.Organism.Bodies.Aggregate(Vector3.zero,(sum,b)=>sum+level.Rotation.transform.InverseTransformPoint(b.position))/32;
                Debug.Log($"VENOM ROUTE {s}: fragments={level.Organism.FragmentCount} cuts={level.Organism.CutCount} merges={level.Organism.MergeCount} pads={level.LeftPad.Load:F3}/{level.RightPad.Load:F3} gate={level.GateLatched} opening={level.GateOpening:F3} escaped={level.Organism.EscapedCount} centre={centre:F3}");
            }
            Assert.That(peak,Is.GreaterThanOrEqualTo(2),"The physical blade must divide the body.");
            Assert.That(level.GateLatched,Is.True,"Two independently moving fragments must hold both contact stations.");
            Assert.That(level.Organism.MergeCount,Is.GreaterThan(0),"Fragments should fuse after the divider.");
            Assert.That(level.Completed,Is.True,"All material must traverse the real round aperture.");
            Capture("06-escaped");
            level.Organism.GetComponent<VenomSurface>().Rebuild(false);
            var escapedSkin=level.Organism.GetComponentsInChildren<MeshFilter>().First(m=>m.name=="Continuous wet skin");
            Assert.That(escapedSkin.sharedMesh.vertices.Min(v=>level.Rotation.transform.InverseTransformPoint(escapedSkin.transform.TransformPoint(v)).y),
                Is.LessThan(-.083f),"Floor skin constraints must still show material emerging beneath the real aperture.");
        }
        private void Capture(string name)
        {
            string directory=System.Environment.GetEnvironmentVariable("VENOM_CAPTURE_DIR");
            if(string.IsNullOrEmpty(directory)||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            Directory.CreateDirectory(directory);level.Organism.GetComponent<VenomSurface>().Rebuild(false);
            int width=name.Contains("motion-")?360:720,height=name.Contains("motion-")?640:1280;
            var target=new RenderTexture(width,height,24){antiAliasing=4};target.Create();
            var old=RenderTexture.active;float oldAspect=level.View.aspect;level.View.aspect=width/(float)height;
            float oldSize=level.View.orthographicSize;Vector3 oldPosition=level.View.transform.position;
            if(name.StartsWith("life-"))
            {
                Vector3 centre=level.Organism.Bodies.Aggregate(Vector3.zero,(sum,b)=>sum+b.position)/32;
                level.View.orthographicSize=.17f;level.View.transform.position=centre-level.View.transform.forward*.5f;
            }
            var picture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};
                RenderPipeline.SubmitRenderRequest(level.View,request);RenderPipeline.SubmitRenderRequest(level.View,request);
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,width,height),0,0);picture.Apply();
                File.WriteAllBytes(Path.Combine(directory,name+".png"),picture.EncodeToPNG());
                Debug.Log($"VENOM CAPTURE {name}: vertices={level.Organism.GetComponent<VenomSurface>().VertexCount}");
            }
            finally{RenderTexture.active=old;level.View.aspect=oldAspect;level.View.orthographicSize=oldSize;level.View.transform.position=oldPosition;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }
        [Test] public void ResetRestoresParticlesConnectionsMechanismsAndMass()
        {
            Steps(60);level.Rotation.SetTargetOrientation(Quaternion.Euler(-20,0,0));Steps(600);
            level.ResetExperiment();Steps(2);
            Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));Assert.That(level.Organism.CutCount,Is.Zero);
            Assert.That(level.Organism.EscapedCount,Is.Zero);Assert.That(level.GateLatched,Is.False);Assert.That(level.Completed,Is.False);
            Assert.That(level.Organism.Bodies.Sum(b=>b.mass),Is.EqualTo(.096f).Within(.000001f));
            Assert.That(Object.FindObjectsByType<CohesiveOrganism>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
        }
        [Test] public void BladeCutsOnlyIntersectedBondsWithoutReplacingOrAcceleratingMatter()
        {
            var matter=level.Organism;
            var identities=matter.Bodies.Select(b=>b.GetInstanceID()).ToArray();
            var positions=matter.Bodies.Select(b=>b.position).ToArray();
            var velocities=matter.Bodies.Select(b=>b.linearVelocity).ToArray();
            var cutter=new GameObject("Test cutting plane");
            try
            {
                cutter.transform.position=Vector3.one;
                Assert.That(matter.Cut(cutter.transform,new Vector3(.003f,.06f,.06f)),Is.Zero);
                cutter.transform.position=level.Spawn.position;
                Assert.That(matter.Cut(cutter.transform,new Vector3(.003f,.06f,.06f)),Is.GreaterThan(0));
                Assert.That(matter.FragmentCount,Is.EqualTo(2));
                CollectionAssert.AreEqual(identities,matter.Bodies.Select(b=>b.GetInstanceID()).ToArray());
                CollectionAssert.AreEqual(positions,matter.Bodies.Select(b=>b.position).ToArray());
                CollectionAssert.AreEqual(velocities,matter.Bodies.Select(b=>b.linearVelocity).ToArray());
                Assert.That(matter.Bodies.Sum(b=>b.mass),Is.EqualTo(.096f).Within(.000001f));
                // Freshly cut, adjacent faces must not immediately heal before they can separate.
                matter.Step(Dt);Assert.That(matter.FragmentCount,Is.EqualTo(2));
            }
            finally{Object.DestroyImmediate(cutter);}
        }
        [Test] public void AConnectedBodyCannotUnlockTheTwoFragmentGate()
        {
            var matter=level.Organism;
            matter.Cut(level.Spawn,new Vector3(.003f,.06f,.06f));
            Assert.That(matter.CutCount,Is.GreaterThan(0));
            var sameGroup=Enumerable.Range(0,32).Where(i=>matter.Groups[i]==matter.Groups[0]).Take(8).ToArray();
            for(int step=0;step<60;step++)
            {
                for(int p=0;p<4;p++){level.LeftPad.Touch(sameGroup[p],matter.SimulationTime);level.RightPad.Touch(sameGroup[p+4],matter.SimulationTime);}
                level.Step(Dt);
            }
            Assert.That(level.LeftPad.Pressed&&level.RightPad.Pressed,Is.True);
            Assert.That(level.GateLatched,Is.False,"Both switches touched by one connected organism must not solve cooperation.");
        }
        [Test] public void PartialExitIsNotVictoryAndSolidSpineBlocksFusion()
        {
            var box=level.Rotation.transform;
            Assert.That(level.SegmentBlocked(box.TransformPoint(new Vector3(-.012f,0,0)),box.TransformPoint(new Vector3(.012f,0,0))),Is.True);
            for(int i=0;i<31;i++)level.Organism.RecordEscape(i);
            level.Step(Dt);Assert.That(level.Completed,Is.False);
        }

        [Test] public void IdleHeadLooksAroundThenRetractsWithoutMovingPhysics()
        {
            var matter=level.Organism;var surface=matter.GetComponent<VenomSurface>();var life=matter.GetComponent<VenomLifeAnimation>();
            bool raised=false,retracted=false;float peak=0,dance=0;int raisedArms=0;bool capturedDance=false;
            for(int frame=0;frame<300;frame++)
            {
                Steps(4);
                var positions=matter.Bodies.Select(b=>b.position).ToArray();
                var velocities=matter.Bodies.Select(b=>b.linearVelocity).ToArray();
                surface.Rebuild(false);
                dance=Mathf.Max(dance,life.DanceAmount);raisedArms=Mathf.Max(raisedArms,life.RaisedTendrilCount);
                if(!capturedDance && life.DanceAmount>.9f && life.HeadHeight>.05f)
                { Capture("life-03-dancing");capturedDance=true; }
                CollectionAssert.AreEqual(positions,matter.Bodies.Select(b=>b.position).ToArray(),"Curiosity must not move physical nodes.");
                CollectionAssert.AreEqual(velocities,matter.Bodies.Select(b=>b.linearVelocity).ToArray(),"No decorative animation force.");
                if(life.HeadHeight>peak)
                {
                    peak=life.HeadHeight;
                    if(peak>.030f&&!raised){raised=true;Capture("life-01-curious");}
                }
                if(raised&&life.HeadAmount<.01f)retracted=true;
                if(frame%3==0&&System.Environment.GetEnvironmentVariable("VENOM_CAPTURE_MOTION")=="1")
                    Capture($"life-motion-idle-{frame/3:D3}");
            }
            Assert.That(peak,Is.GreaterThan(.025f),"An idle creature should visibly lift a small head.");
            Assert.That(retracted,Is.True,"The head must return to the body between looks.");
            Assert.That(dance,Is.GreaterThan(.9f),"Idle performance should include a visible stretch and dance.");
            Assert.That(raisedArms,Is.GreaterThanOrEqualTo(2),"Several upper tendrils should wave above the body.");
            Assert.That(capturedDance,Is.True);
            Assert.That(matter.FragmentCount,Is.EqualTo(1));Assert.That(matter.TotalMass,Is.EqualTo(.096f).Within(.000001f));
        }

        [Test] public void MovingTendrilsPlantOnRealSurfacesAndReleaseInAir()
        {
            var matter=level.Organism;var surface=matter.GetComponent<VenomSurface>();var life=matter.GetComponent<VenomLifeAnimation>();
            Steps(120);surface.Rebuild(false);level.Rotation.SetTargetOrientation(Quaternion.Euler(-18,0,0));
            int peakFeet=0;bool captured=false;float peakCrawl=0;
            var previousPlants=new System.Collections.Generic.Dictionary<(int,int),VenomLifeAnimation.Plant>();
            int retained=0;
            for(int frame=0;frame<100;frame++)
            {
                Steps(4);
                var positions=matter.Bodies.Select(b=>b.position).ToArray();
                var velocities=matter.Bodies.Select(b=>b.linearVelocity).ToArray();
                surface.Rebuild(false);peakFeet=Mathf.Max(peakFeet,life.PlantedFeet.Count);
                CollectionAssert.AreEqual(positions,matter.Bodies.Select(b=>b.position).ToArray(),"Flowing skin must not steer physical matter.");
                CollectionAssert.AreEqual(velocities,matter.Bodies.Select(b=>b.linearVelocity).ToArray());
                peakCrawl=Mathf.Max(peakCrawl,life.CrawlAmount);
                foreach(var plant in life.PlantedFeet)
                {
                    Assert.That(Vector3.Distance(plant.Tip,plant.Surface.ClosestPoint(plant.Tip)),Is.LessThan(.002f),"Planted tips must touch geometry.");
                    var key=(plant.Fragment,plant.Limb);
                    if(previousPlants.TryGetValue(key,out var old)&&old.Id==plant.Id&&old.Surface==plant.Surface)
                    {Assert.That(Vector3.Distance(old.LocalPoint,plant.LocalPoint),Is.LessThan(.00001f));retained++;}
                    previousPlants[key]=plant;
                }
                if(life.TendrilCount>=4&&life.CrawlAmount>.7f&&!captured){captured=true;Capture("life-02-gripping");}
                if(frame%3==0&&System.Environment.GetEnvironmentVariable("VENOM_CAPTURE_MOTION")=="1")
                    Capture($"life-motion-moving-{frame/3:D3}");
            }
            Assert.That(peakFeet,Is.GreaterThan(0));Assert.That(retained,Is.GreaterThan(0),"Feet should hold a local surface anchor across frames.");
            Debug.Log($"VENOM LIFE peak crawl={peakCrawl:F3}, planted={peakFeet}");
            Assert.That(captured,Is.True,"Exercise fully developed tendrils, not just their first growth frame.");
            // A separation from every box surface must remove the ground attachments.
            foreach(var body in matter.Bodies){body.position+=Vector3.up;body.linearVelocity=Vector3.zero;}
            Physics.SyncTransforms();
            for(int frame=0;frame<12;frame++){Steps(4);surface.Rebuild(false);}
            Assert.That(life.PlantedFeet.Count,Is.Zero);Assert.That(life.TendrilCount,Is.Zero);
        }

        [Test] public void LifeAnimationFreezesWithSimulationAndClearsOnRetry()
        {
            var surface=level.Organism.GetComponent<VenomSurface>();var life=level.Organism.GetComponent<VenomLifeAnimation>();
            for(int frame=0;frame<180&&life.HeadAmount<.9f;frame++){Steps(4);surface.Rebuild(false);}
            Assert.That(life.HeadAmount,Is.GreaterThan(.8f));
            level.TogglePause();float head=life.HeadAmount,height=life.HeadHeight;
            var meshes=level.Organism.GetComponentsInChildren<MeshFilter>();
            var saved=meshes.Select(m=>m.sharedMesh.vertices).ToArray();
            Vector3 cameraPosition=level.View.transform.position;
            level.View.transform.position+=Vector3.right;
            for(int frame=0;frame<10;frame++)surface.Rebuild(false);
            Assert.That(life.HeadAmount,Is.EqualTo(head));Assert.That(life.HeadHeight,Is.EqualTo(height));
            for(int m=0;m<meshes.Length;m++)
            {
                var actual=meshes[m].sharedMesh.vertices;
                Assert.That(actual.Length,Is.EqualTo(saved[m].Length));
                for(int v=0;v<actual.Length;v++)
                    Assert.That(Vector3.Distance(actual[v],saved[m][v]),Is.LessThan(.000001f),"Pause freezes the whole skin; camera movement must not steer curiosity.");
            }
            level.View.transform.position=cameraPosition;
            level.ResetExperiment();surface.Rebuild(false);
            Assert.That(life.HeadAmount,Is.Zero);Assert.That(life.TendrilCount,Is.Zero);Assert.That(life.PlantedFeet.Count,Is.Zero);
            Assert.That(life.DanceAmount,Is.Zero);Assert.That(life.RaisedTendrilCount,Is.Zero);
        }
    }
}
#endif
