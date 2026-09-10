#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class VenomControlTests
    {
        private VenomLevelController level;
        private SimulationMode mode;
        private Vector3 gravity;
        private const float Dt = 1f/120;
        [UnitySetUp] public IEnumerator Setup()
        {
            mode = Physics.simulationMode; gravity = Physics.gravity; Physics.simulationMode = SimulationMode.Script;
            yield return Load(2);
        }
        private IEnumerator Load(int number)
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"Assets/_Game/Venom/Venom{number:00}.unity",new LoadSceneParameters(LoadSceneMode.Single));
            level = Object.FindFirstObjectByType<VenomLevelController>(); Assert.That(level,Is.Not.Null);
            level.enabled = false; level.Rotation.enabled = false; level.GetComponent<VenomInput>().enabled = false;
            level.ResetExperiment();
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);
            yield return null; Physics.simulationMode = mode; Physics.gravity = gravity; Time.timeScale = 1;
        }
        private void Steps(int count)
        {
            for (int i=0;i<count;i++) { level.Step(Dt); level.Rotation.Step(Dt); Physics.Simulate(Dt); }
        }
        private Vector3 Centre => level.Locomotion.Selected.Centre;
        private void Toward(Vector3 point)
        {
            level.Locomotion.SetInput(Vector3.ClampMagnitude(Vector3.ProjectOnPlane(point-Centre,Vector3.up)/.025f,1));
            Steps(1);
        }
        private void Go(float x,float z,int ticks=1200)
        {
            for (int i=0;i<ticks;i++)
            {
                Vector3 target=new Vector3(x,0,z);
                if(Vector3.ProjectOnPlane(Centre-target,Vector3.up).magnitude<.012f)break;
                Toward(target);
            }
            level.Locomotion.SetInput(Vector3.zero);Steps(60);
            Debug.Log($"CONTROL GO ({x:F3},{z:F3}): p={Centre:F3} parts={level.Organism.FragmentCount} cuts={level.Organism.CutCount} merge={level.Organism.MergeCount} gate={level.GateLatched}");
            if(Vector3.ProjectOnPlane(Centre-new Vector3(x,0,z),Vector3.up).magnitude>.035f) Debug.Log(string.Join("; ",level.Organism.Bodies.Select((b,i)=>$"{i}:g{level.Organism.Groups[i]} p={b.position:F4} support={level.Organism.TryGetSupport(i,out _,out _,out _)}")));
            Assert.That(Vector3.ProjectOnPlane(Centre-new Vector3(x,0,z),Vector3.up).magnitude,Is.LessThan(.035f),"The real creature must reach this waypoint using traction alone.");
        }
        private void ApproachBlade()
        {
            Steps(90);
            for(int i=0;i<900&&level.Organism.FragmentCount<2;i++)Toward(new Vector3(0,0,.145f));
            level.Locomotion.SetInput(Vector3.zero);Steps(120);
            Debug.Log($"BLADE {level.Blade.position:F4} released={level.BladeReleased}; CUT parts: {string.Join("; ",level.Locomotion.Fragments.Select(f=>$"{f.Anchor}:{f.Count}@{f.Centre:F3}"))}");
            Assert.That(level.Organism.CutCount,Is.GreaterThan(0));Assert.That(level.Organism.FragmentCount,Is.GreaterThanOrEqualTo(2));
            Capture($"0{level.LevelNumber}-split");
        }
        [Test] public void DirectInputMovesByForcesAndBoxStaysStill()
        {
            Steps(120);Vector3 start=Centre;Quaternion box=level.Rotation.Orientation;
            level.Locomotion.SetInput(Vector3.left);Steps(120);
            Assert.That(Centre.x,Is.LessThan(start.x-.045f));Assert.That(level.Rotation.Orientation,Is.EqualTo(box));
            Assert.That(level.Rotation.InputEnabled,Is.False);Assert.That(level.Organism.Bodies.All(b=>!b.isKinematic),Is.True);
            level.Locomotion.SetInput(Vector3.zero);Steps(120);
            Assert.That(level.Locomotion.Selected.Velocity.magnitude,Is.LessThan(.04f));
            Assert.That(level.Organism.TotalMass,Is.EqualTo(.096f).Within(.000001f));
            level.TogglePause();float time=level.Organism.SimulationTime;level.Step(Dt);
            Assert.That(level.Organism.SimulationTime,Is.EqualTo(time));
            level.TogglePause();Assert.That(level.Rotation.InputEnabled,Is.False);
            level.ResetExperiment();Assert.That(level.Locomotion.Input,Is.EqualTo(Vector3.zero));Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));
        }
        [Test] public void UnsupportedFragmentsCannotFlyUnderDirectionalInput()
        {
            foreach(var body in level.Organism.Bodies){body.position+=Vector3.up*.4f;body.linearVelocity=Vector3.zero;}
            Physics.SyncTransforms();level.Locomotion.SetInput(Vector3.right);Steps(12);
            Vector3 mean=level.Organism.Bodies.Aggregate(Vector3.zero,(p,b)=>p+b.linearVelocity)/32;
            Assert.That(Mathf.Abs(mean.x),Is.LessThan(.001f));Assert.That(mean.y,Is.LessThan(-.8f));
        }
        [Test] public void SelectedFragmentsSolveBothPlatesThenAllEscape()
        {
            ApproachBlade();
            var left=level.Locomotion.Fragments.OrderBy(f=>f.Centre.x).First();
            var right=level.Locomotion.Fragments.OrderBy(f=>f.Centre.x).Last();
            int leftId=left.Anchor,rightId=right.Anchor;
            Assert.That(level.Locomotion.Select(leftId),Is.True);
            Go(-.085f,.20f);Go(-.085f,-.033f);
            Assert.That(level.LeftPad.Pressed,Is.True);Vector3 parked=Centre;
            Assert.That(level.Locomotion.Select(rightId),Is.True);
            Go(.085f,.20f);Go(.085f,-.033f);Steps(120);
            Assert.That(level.GateLatched,Is.True,"Independent selected parts must hold two real pressure plates.");
            var parkedNow=level.Locomotion.Fragments.First(f=>f.Group==level.Organism.Groups[leftId]);
            Assert.That(Vector3.Distance(parkedNow.Centre,parked),Is.LessThan(.025f),"Unselected body keeps a bounded floor grip while the other crawls.");
            Capture("02-both-stations");
            Go(.085f,-.15f);Go(0,-.15f);
            level.Locomotion.Select(leftId);Go(-.085f,-.15f);Go(0,-.15f);Steps(180);
            Assert.That(level.Organism.MergeCount,Is.GreaterThan(0));
            Capture("02-reunion");
            ExitAll();
        }
        private void ExitAll()
        {
            for(int i=0;i<2400&&!level.Completed&&!level.Lost;i++)
                if(level.Locomotion.Selected!=null)Toward(new Vector3(0,0,-.235f));else Steps(1);
            if(!level.Completed) Debug.Log(string.Join("; ",level.Organism.Bodies.Select((b,i)=>$"{i}:out={level.Organism.Escaped[i]} p={b.position:F4}")));
            Debug.Log($"EXIT {level.LevelNumber}: {level.Organism.EscapedCount}/32 lost={level.Lost} parts={level.Organism.FragmentCount}");
            Assert.That(level.Completed,Is.True);Assert.That(level.Organism.EscapedCount,Is.EqualTo(32));
            Capture($"0{level.LevelNumber}-escaped");
        }
        [Test] public void ReunitedBodyCanReturnToCuttingStationBeforeSolving()
        {
            ApproachBlade();
            int first = level.Locomotion.Fragments[0].Anchor, second = level.Locomotion.Fragments[1].Anchor;
            level.Locomotion.Select(first);Go(-.075f,.252f);
            level.Locomotion.Select(second);Go(.075f,.252f);Go(0,.252f);
            level.Locomotion.Select(first);Go(0,.252f);Steps(240);
            Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));Assert.That(level.GateLatched,Is.False);
            Assert.That(level.BladeReleased,Is.False,"The press must re-arm after the reunited body clears its bay.");
            int cuts=level.Organism.CutCount;ApproachBlade();Assert.That(level.Organism.CutCount,Is.GreaterThan(cuts));
        }
        [UnityTest] public IEnumerator FollowerWaitsThreeSecondsAndCannotBeSelected()
        {
            yield return Load(3);
            // Controlled initial split with ample separation prevents ordinary
            // contact fusion from ending the wait before its deadline is tested.
            var cutter = new GameObject("Timer test initial cutting plane").transform;
            cutter.position = level.Spawn.position+Vector3.right*.012f;
            level.Organism.Cut(cutter,new Vector3(.003f,.2f,.2f));
            for(int i=0;i<32;i++) level.Organism.Bodies[i].position += Vector3.right*(level.Organism.Bodies[i].position.x>.012f?.07f:-.06f);
            Object.Destroy(cutter.gameObject);Physics.SyncTransforms();Steps(1);
            var motion=level.Locomotion;var follower=motion.Fragments.First(f=>!f.Selected);
            Assert.That(motion.Selected.Count,Is.GreaterThan(follower.Count));
            Assert.That(motion.Select(follower.Anchor),Is.False);
            Assert.That(follower.WaitRemaining,Is.EqualTo(3).Within(.001f));Assert.That(follower.Following,Is.False);
            int followerId=follower.Anchor;float elapsed=level.Organism.SimulationTime;
            level.TogglePause();for(int i=0;i<720;i++)level.Step(Dt);
            Assert.That(level.Organism.SimulationTime,Is.EqualTo(elapsed));
            level.TogglePause();Steps(358);
            follower=motion.Fragments.First(f=>f.Group==level.Organism.Groups[followerId]);
            Assert.That(follower.Following,Is.False);Assert.That(follower.WaitRemaining,Is.GreaterThan(0));
            Assert.That(motion.PathSearches,Is.Zero);
            Steps(3);follower=motion.Fragments.First(f=>f.Group==level.Organism.Groups[followerId]);
            Assert.That(motion.PathSearches,Is.GreaterThan(0));
            Assert.That(follower.WaitRemaining,Is.Zero);Assert.That(follower.Following,Is.True);
            level.ResetExperiment();Assert.That(motion.PathSearches,Is.Zero);Assert.That(motion.Fragments.Count,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator LargestRemainsLeaderAcrossFurtherSplitsAndEscapes()
        {
            yield return Load(3);
            var cutter = new GameObject("Topology test cutting plane").transform;
            cutter.position = level.Spawn.position+Vector3.right*.012f;
            level.Organism.Cut(cutter,new Vector3(.003f,.2f,.2f));Steps(1);
            int initial=level.Locomotion.SelectedParticle;
            Assert.That(level.Locomotion.Selected.Count,Is.EqualTo(level.Locomotion.Fragments.Max(f=>f.Count)));
            cutter.position=level.Spawn.position;cutter.rotation=Quaternion.Euler(0,90,0);
            level.Organism.Cut(cutter,new Vector3(.003f,.2f,.2f));Steps(1);
            Assert.That(level.Locomotion.Fragments.Count,Is.GreaterThanOrEqualTo(3));
            Assert.That(level.Locomotion.Selected.Count,Is.EqualTo(level.Locomotion.Fragments.Max(f=>f.Count)));
            Assert.That(level.Locomotion.SelectedParticle,Is.EqualTo(initial),"Equal-size ties keep the previous material point selected.");
            int group=level.Locomotion.Selected.Group;
            for(int i=0;i<32;i++)if(level.Organism.Groups[i]==group)level.Organism.RecordEscape(i);
            Steps(1);Assert.That(level.Organism.Escaped[level.Locomotion.SelectedParticle],Is.False);
            Assert.That(level.Locomotion.Selected.Count,Is.EqualTo(level.Locomotion.Fragments.Max(f=>f.Count)));
            Object.Destroy(cutter.gameObject);
        }
        [UnityTest] public IEnumerator FollowerFindsDetourReunitesAndTraversesExit()
        {
            yield return Load(3);ApproachBlade();
            Go(-.11f,.19f);Go(-.11f,-.018f);
            Capture("03-leader-waiting");
            for(int i=0;i<3600&&level.Organism.FragmentCount>1;i++)Steps(1);
            Debug.Log($"FOLLOW REUNION: {string.Join("; ",level.Locomotion.Fragments.Select(f=>$"{f.Anchor}:{f.Count}@{f.Centre:F3} blocked={f.Blocked} wait={f.WaitRemaining}"))} searches={level.Locomotion.PathSearches}");
            Assert.That(level.Organism.MergeCount,Is.GreaterThan(0),"Smaller fragment must actually navigate around the separating spine and touch the leader.");
            Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));Steps(180);Assert.That(level.GateLatched,Is.True);
            Capture("03-reunion");
            Go(.17f,-.025f);Go(.17f,-.145f);Go(0,-.15f);
            ExitAll();
        }
        [UnityTest] public IEnumerator NavigationRespectsWallsAndClosedGate()
        {
            yield return Load(3);Steps(120);
            var path=new List<Vector3>();var nav=level.Locomotion.Navigator;float r=level.LocomotionProfile.NavigationClearance;
            Vector3 from=new Vector3(.09f,-.042f,.05f),to=new Vector3(-.11f,-.042f,-.018f);
            Assert.That(nav.Clear(from,to,r),Is.False);
            Assert.That(nav.FindPath(from,to,r,path),Is.True);Assert.That(path.Count,Is.GreaterThan(1));
            Vector3 previous=from;
            foreach(var point in path){Assert.That(nav.Clear(previous,point,r),Is.True);previous=point;}
            Assert.That(path.Any(p=>p.z<-.01f),Is.True,"Route must go around the bottom of the spine.");
            path.Clear();Assert.That(nav.FindPath(new Vector3(.17f,-.042f,-.14f),new Vector3(0,-.042f,-.235f),r,path),Is.False,"Closed gate must block route; no straight-line fallback.");
        }
        [UnityTest] public IEnumerator WholeBodySqueezesThroughSlitThenEscapes()
        {
            yield return Load(3);ApproachBlade();
            Go(-.11f,.19f);Go(-.11f,-.018f);
            for(int i=0;i<3600&&level.Organism.FragmentCount>1;i++)Steps(1);
            Steps(180);Assert.That(level.GateLatched,Is.True);
            Go(-.215f,-.035f);
            Capture("03-slit-before");
            int crossed=0,ticks=0;float maxFlow=0;bool captured=false;
            var prior=level.Organism.Bodies.Select(b=>b.position).ToArray();
            level.Locomotion.SetInput(Vector3.back);
            for(int tick=0;tick<1800;tick++)
            {
                Steps(1);maxFlow=Mathf.Max(maxFlow,level.Locomotion.Selected.Squeeze.Amount);
                ticks++;
                crossed=0;
                for(int p=0;p<32;p++)
                {
                    Vector3 position=level.Organism.Bodies[p].position;
                    if(prior[p].z>-.105f&&position.z<=-.105f)
                        Assert.That(position.x,Is.InRange(-.242f,-.226f),"Each physical node must pass inside the actual slit.");
                    if(position.z<-.13f)crossed++;
                    Assert.That(position.y,Is.GreaterThan(level.FloorBoundary.Top+.006f),"No floor tunnelling while compressed.");
                    prior[p]=position;
                }
                if(!captured && crossed>=8) { Capture("03-slit-flow");captured=true; }
                if(crossed==32)break;
            }
            Debug.Log($"SLIT crossed={crossed}/32 in {ticks*Dt:F2}s flow={maxFlow} centre={Centre:F4}");
            Assert.That(maxFlow,Is.GreaterThan(.8f));Assert.That(crossed,Is.EqualTo(32));
            Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));
            Assert.That(level.Organism.TotalMass,Is.EqualTo(.096f).Within(.000001f));
            level.Locomotion.SetInput(Vector3.zero);Steps(240);
            Assert.That(level.Locomotion.Selected.Squeeze.Amount,Is.Zero);
            Capture("03-slit-after");
            Go(-.12f,-.235f);ExitAll();
        }
        [UnityTest] public IEnumerator SlitCanBeCancelledAndBackedOutOf()
        {
            yield return Load(3);Steps(120);Go(-.19f,.245f);Go(-.215f,-.035f);
            level.Locomotion.SetInput(Vector3.back);Steps(100);
            Assert.That(level.Locomotion.Selected.Squeeze.Amount,Is.GreaterThan(.5f));
            level.Locomotion.SetInput(Vector3.zero);Steps(180);
            Assert.That(level.Locomotion.Selected.Squeeze.Amount,Is.Zero);
            Assert.That(level.Locomotion.Selected.Velocity.magnitude,Is.LessThan(.04f));
            Go(-.2f,.045f);
            Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));Assert.That(level.Lost,Is.False);
            level.ResetExperiment();Steps(120);
            Assert.That(level.Locomotion.Selected.Squeeze.Amount,Is.Zero);
            foreach(var a in level.Organism.Bodies)foreach(var b in level.Organism.Bodies)
                if(a!=b)Assert.That(Physics.GetIgnoreCollision(a.GetComponent<Collider>(),b.GetComponent<Collider>()),Is.False);
        }
        [UnityTest] public IEnumerator SubParticleSlitRemainsSolid()
        {
            yield return Load(3);
            var wall=level.Rotation.transform.Find("Detour wall");
            wall.localPosition=new Vector3(-.069f,.002f,-.105f);wall.localScale=new Vector3(.338f,.138f,.008f);
            Physics.SyncTransforms();Steps(120);Go(-.19f,.245f);Go(-.215f,-.035f);
            level.Locomotion.SetInput(Vector3.back);Steps(600);
            Assert.That(level.Locomotion.Selected.Squeeze.Amount,Is.Zero,"A 12 mm slit cannot admit an 18 mm collision particle.");
            Assert.That(level.Organism.Bodies.All(b=>b.position.z>-.11f),Is.True);
            Assert.That(level.GateLatched,Is.False);Assert.That(level.Lost,Is.False);
        }
        [UnityTest] public IEnumerator NavigationCanUseParticleWidthSlitWithoutCrossingGate()
        {
            yield return Load(3);
            // Close only the broad route so a follower must use the real slit.
            var wall=level.Rotation.transform.Find("Detour wall");
            wall.localPosition=new Vector3(.016f,.002f,-.105f);wall.localScale=new Vector3(.468f,.138f,.008f);
            Physics.SyncTransforms();Steps(120);
            var path=new List<Vector3>();var nav=level.Locomotion.Navigator;
            Vector3 from=new Vector3(-.18f,-.042f,-.025f),to=new Vector3(-.18f,-.042f,-.145f);
            Assert.That(nav.FindPath(from,to,.029f,path),Is.False);
            Assert.That(nav.FindPath(from,to,.011f,path),Is.True);
            Vector3 previous=from;
            foreach(var p in path) { Assert.That(nav.Clear(previous,p,.011f),Is.True);previous=p; }
            Assert.That(path.Any(p=>p.x<-.229f),Is.True);
            Assert.That(nav.FindPath(from,new Vector3(0,-.042f,-.235f),.011f,path),Is.False);
        }
        [UnityTest] public IEnumerator FollowerUsesSlitWhenBroadRouteIsClosed()
        {
            yield return Load(3);
            var wall=level.Rotation.transform.Find("Detour wall");
            wall.localPosition=new Vector3(.016f,.002f,-.105f);wall.localScale=new Vector3(.468f,.138f,.008f);
            // Controlled starting state: a leader beyond the divider, a small
            // detached fragment above it, and only the narrow route between them.
            level.Gate.GetComponentInChildren<Collider>().enabled=false;
            var cutter=new GameObject("Slit test initial split").transform;
            cutter.position=level.Spawn.position+Vector3.right*.012f;
            level.Organism.Cut(cutter,new Vector3(.003f,.2f,.2f));
            var groups=Enumerable.Range(0,32).GroupBy(i=>level.Organism.Groups[i]).OrderByDescending(g=>g.Count()).ToArray();
            Assert.That(groups.Length,Is.EqualTo(2));
            for(int g=0;g<2;g++)
            {
                Vector3 centre=groups[g].Aggregate(Vector3.zero,(p,i)=>p+level.Organism.Bodies[i].position)/groups[g].Count();
                Vector3 target=new Vector3(-.18f,-.025f,g==0?-.163f:-.025f);
                foreach(int i in groups[g])level.Organism.Bodies[i].position+=target-centre;
            }
            Object.Destroy(cutter.gameObject);Physics.SyncTransforms();Steps(1);
            float flow=0;
            for(int tick=0;tick<2400 && level.Organism.FragmentCount>1;tick++)
            {
                Steps(1);
                foreach(var fragment in level.Locomotion.Fragments)
                    if(!fragment.Selected)flow=Mathf.Max(flow,fragment.Squeeze.Amount);
            }
            Debug.Log($"SLIT FOLLOWER: parts={level.Organism.FragmentCount}, flow={flow}, searches={level.Locomotion.PathSearches}");
            Assert.That(flow,Is.GreaterThan(.5f));Assert.That(level.Organism.MergeCount,Is.GreaterThan(0));
            Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));Assert.That(level.Lost,Is.False);
        }
        private void Capture(string name)
        {
            string directory=Environment.GetEnvironmentVariable("VENOM_CAPTURE_DIR");
            if(string.IsNullOrEmpty(directory)||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            Directory.CreateDirectory(directory);level.Organism.GetComponent<VenomSurface>().Rebuild(false);level.GetComponent<VenomHud>().RefreshSelection();
            var target=new RenderTexture(720,1280,24){antiAliasing=4};target.Create();var old=RenderTexture.active;
            float aspect=level.View.aspect, size=level.View.orthographicSize;Vector3 cameraPosition=level.View.transform.position;
            level.View.aspect=720f/1280;level.GetComponent<VenomHud>().FrameChamber(720,1280);
            var picture=new Texture2D(720,1280,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};
                RenderPipeline.SubmitRenderRequest(level.View,request);RenderPipeline.SubmitRenderRequest(level.View,request);
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,720,1280),0,0);picture.Apply();
                File.WriteAllBytes(Path.Combine(directory,name+".png"),picture.EncodeToPNG());
            }
            finally{level.View.aspect=aspect;level.View.orthographicSize=size;level.View.transform.position=cameraPosition;RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }
    }
}
#endif
