#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed class VenomGuidanceTests
    {
        private VenomLevelController level;
        private SimulationMode simulation;
        private bool persistence;
        private const float Dt=1f/120;
        private Vector3 Centre=>level.Rotation.transform.InverseTransformPoint(level.Locomotion.Selected.Centre);
        [UnitySetUp] public IEnumerator Setup()
        {
            simulation=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            persistence=VenomGuidanceMemory.PersistenceEnabled;VenomGuidanceMemory.PersistenceEnabled=false;
            Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return null;yield return null;
            yield return Load(7);
        }
        private IEnumerator Load(int number)
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"Assets/_Game/Venom/Venom{number:00}.unity",new LoadSceneParameters(LoadSceneMode.Single));
            level=Object.FindFirstObjectByType<VenomLevelController>();
            level.enabled=false;level.Rotation.enabled=false;level.GetComponent<VenomInput>().enabled=false;level.ResetExperiment();
            level.GetComponent<VenomHud>().FrameChamber(Screen.width,Screen.height);
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;Physics.simulationMode=simulation;Time.timeScale=1;VenomGuidanceMemory.PersistenceEnabled=persistence;
        }
        private void Steps(int count)
        {
            for(int i=0;i<count;i++){level.Step(Dt);level.Rotation.Step(Dt);Physics.Simulate(Dt);}
        }
        private void Reach(int face,Vector3 target)
        {
            Assert.That(level.Guidance.Command(face,target),Is.True);
            for(int i=0;i<3000&&!level.Guidance.Arrived&&!level.Completed;i++)
            {
                Steps(1);
                if(i%600==0)Debug.Log($"GUIDED REACH t={i*Dt:F1} face={level.Climbing.SurfaceName} centre={Centre:F3} goal={target:F3}");
            }
            Debug.Log($"GUIDED ARRIVE face={level.Climbing.SurfaceName} centre={Centre:F3} goal={target:F3}");
            Assert.That(level.Guidance.Arrived,Is.True);Assert.That(Vector3.Distance(Centre,target),Is.LessThan(.055f));
            Assert.That(level.Lost,Is.False);Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));
        }
        [Test] public void TapTargetsCrossRealFacesSurviveRotationAndExitAutomatically()
        {
            Steps(120);Capture("07-overview");
            Reach(1,new Vector3(-.25f,.08f,.08f));Capture("07-wall");
            Vector3 remembered=level.Guidance.TargetLocal;
            var orientation=Quaternion.Euler(65,30,-20);
            level.Rotation.SetTargetOrientation(orientation);Steps(840);
            Assert.That(level.Guidance.TargetLocal,Is.EqualTo(remembered));
            Assert.That(Vector3.Distance(Centre,remembered),Is.LessThan(.065f),"An arrived destination stays attached to the rotating shell.");
            Reach(5,new Vector3(.12f,.25f,-.12f));Capture("07-ceiling");
            Assert.That(level.Guidance.Memory.Visits.Count,Is.EqualTo(2));
            level.Guidance.Command(5,new Vector3(.045f,.25f,0));
            for(int i=0;i<3600&&!level.Completed;i++)Steps(1);
            Assert.That(level.Completed,Is.True,$"Automatic exit: {level.Organism.EscapedCount}/32; centre {Centre:F3}");
            Assert.That(level.Organism.TotalMass,Is.EqualTo(.096f).Within(.000001f));
            Assert.That(level.Guidance.Memory.Visits.Last().Face,Is.EqualTo(5),"A successful exit becomes part of the learned route.");Capture("07-escaped");
        }
        [TestCase(0,.12f,-.25f,.08f)]
        [TestCase(1,-.25f,.10f,.08f)]
        [TestCase(2,.25f,.10f,.08f)]
        [TestCase(3,.08f,.10f,-.25f)]
        [TestCase(4,.08f,.10f,.25f)]
        [TestCase(5,-.12f,.25f,.12f)]
        public void SingleDestinationCanReachEveryFace(int face,float x,float y,float z)
        {
            Steps(120);Reach(face,new Vector3(x,y,z));
            Assert.That(level.Climbing.FaceFor(level.Locomotion.Selected.Anchor),Is.EqualTo(face));
        }
        [Test] public void NewCommandsLeaveTheCeilingViaAnotherWall()
        {
            Steps(120);Reach(5,new Vector3(-.12f,.25f,.12f));
            Reach(2,new Vector3(.25f,.08f,.10f));Reach(3,new Vector3(.10f,.10f,-.25f));
            Reach(0,new Vector3(.08f,-.25f,.08f));
            Assert.That(level.Organism.EscapedCount,Is.Zero);
        }
        [Test] public void DemonstrationMemoryPersistsPerLevel()
        {
            const int testLevel=999;
            try
            {
                VenomGuidanceMemory.PersistenceEnabled=true;
                var memory=new VenomGuidanceMemory();memory.Remember(1,new Vector3(-.25f,.1f,.1f));memory.KnowsSwitch=true;memory.Save(testLevel);
                var restored=VenomGuidanceMemory.Load(testLevel);
                Assert.That(restored.KnowsSwitch,Is.True);Assert.That(restored.Visits.Count,Is.EqualTo(1));
                Assert.That(restored.Visits[0].Point,Is.EqualTo(new Vector3(-.25f,.1f,.1f)));
            }
            finally{PlayerPrefs.DeleteKey("venom.guidance.v1.999");PlayerPrefs.Save();VenomGuidanceMemory.PersistenceEnabled=false;}
        }
        [Test] public void ReachedInstructionsReplayAndForgetWithoutChangingPhysics()
        {
            Steps(120);Reach(0,new Vector3(.10f,-.25f,.10f));Reach(0,new Vector3(.08f,-.25f,-.10f));
            string saved=JsonUtility.ToJson(level.Guidance.Memory);
            var restored=JsonUtility.FromJson<VenomGuidanceMemory>(saved);
            Assert.That(restored.Visits.Count,Is.EqualTo(2));
            level.ResetExperiment();Steps(120);
            Assert.That(level.Guidance.Memory.Visits.Count,Is.EqualTo(2));Assert.That(level.Guidance.HasTarget,Is.False);
            Assert.That(level.Guidance.Replay(),Is.True);
            for(int i=0;i<2400&&level.Guidance.Replaying;i++)Steps(1);
            Assert.That(level.Guidance.Replaying,Is.False);Assert.That(level.Guidance.ArrivalCount,Is.EqualTo(2));
            Assert.That(Vector3.Distance(Centre,new Vector3(.08f,-.25f,-.10f)),Is.LessThan(.055f));
            var positions=level.Organism.Bodies.Select(b=>b.position).ToArray();
            level.Guidance.Forget();CollectionAssert.AreEqual(positions,level.Organism.Bodies.Select(b=>b.position));
            Assert.That(level.Guidance.Memory.Visits.Count,Is.Zero);Assert.That(level.Guidance.Replay(),Is.False);
        }
        [Test] public void PickingUsesVisibleInnerShellAndDragNeverIssuesATap()
        {
            Steps(120);var guide=level.Guidance;var input=level.GetComponent<VenomInput>();
            Assert.That(guide.PickSurface(new Ray(new Vector3(.10f,1,.1f),Vector3.down),out int face,out var local),Is.True);
            Assert.That(face,Is.Zero);Assert.That(Vector3.Distance(local,new Vector3(.1f,-.25f,.1f)),Is.LessThan(.0001f));
            Assert.That(guide.PickSurface(new Ray(Vector3.one,Vector3.up),out _,out _),Is.False);
            Vector2 p=level.View.WorldToScreenPoint(new Vector3(.10f,-.25f,.1f));
            input.BeginTap(p);input.MoveTap(p+Vector2.right*70);input.FinishTap();
            Assert.That(guide.CommandCount,Is.Zero);
            input.ApplyGuidanceTouch(1,p,Vector2.zero);input.ApplyGuidanceTouch(2,p,p+Vector2.right*45);
            input.ApplyGuidanceTouch(1,p,Vector2.zero);input.ApplyGuidanceTouch(0,Vector2.zero,Vector2.zero);
            Assert.That(guide.CommandCount,Is.Zero,"Lifting one finger after a rotation must not send the creature away.");
            input.BeginTap(p);input.FinishTap();Assert.That(guide.CommandCount,Is.EqualTo(1));
            var destination=guide.TargetLocal;
            level.TogglePause();float time=level.Organism.SimulationTime;Steps(90);
            Assert.That(guide.Command(0,Vector3.zero),Is.False);Assert.That(guide.TargetLocal,Is.EqualTo(destination));
            Assert.That(level.Organism.SimulationTime,Is.EqualTo(time));level.TogglePause();
        }
        [UnityTest] public IEnumerator PhysicalButtonOpensDoorThenCreatureExitsAndRemembersTheLesson()
        {
            yield return Load(8);
            Vector3 pad=level.Rotation.transform.InverseTransformPoint(level.Guidance.Button.Body.position);
            Assert.That(Vector2.Distance(new Vector2(pad.x,pad.z),new Vector2(level.Guidance.ButtonLocal.x,level.Guidance.ButtonLocal.z)),Is.LessThan(.001f));
            Assert.That(Mathf.Abs(pad.y-level.FloorBoundary.Top),Is.LessThan(.005f),"The button starts recessed in the floor, including the first frame after reset.");
            Steps(240);Capture("08-overview");
            Assert.That(level.GateLatched,Is.False);Assert.That(level.Guidance.DoorUnlocked,Is.False);
            Assert.That(level.Guidance.Command(5,new Vector3(0,.25f,0)),Is.False);
            Assert.That(level.SegmentBlocked(new Vector3(0,.20f,0),new Vector3(0,.27f,0)),Is.True);
            Assert.That(level.Guidance.Memory.KnowsSwitch,Is.False);
            Assert.That(level.Guidance.Touch(level.View.WorldToScreenPoint(level.Rotation.transform.TransformPoint(level.Guidance.ButtonLocal))),Is.True);
            bool pressed=false,opened=false;
            for(int i=0;i<4200&&!level.Completed;i++)
            {
                Steps(1);pressed|=level.Guidance.Button.Pressed;
                if(!opened&&level.GateLatched){opened=true;Capture("08-open");Assert.That(level.Guidance.Replay(),Is.True);Assert.That(level.Guidance.Exiting,Is.True);}
                if(i%600==0)Debug.Log($"GUIDED SWITCH t={i*Dt:F1} centre={Centre:F3} face={level.Climbing.SurfaceName} load={level.Guidance.Button.Load:F3} travel={level.Guidance.DoorTravel:F3} out={level.Organism.EscapedCount}");
            }
            Assert.That(pressed,Is.True);Assert.That(opened,Is.True);Assert.That(level.Completed,Is.True);
            Assert.That(level.Guidance.Memory.KnowsSwitch,Is.True);Capture("08-escaped");
            level.ResetExperiment();Steps(120);
            Assert.That(level.Guidance.DoorUnlocked,Is.False);Assert.That(level.Guidance.Memory.KnowsSwitch,Is.True);
            Assert.That(level.Guidance.Replay(),Is.True);
            for(int i=0;i<4200&&!level.Completed;i++){if(i==180)level.Rotation.SetTargetOrientation(Quaternion.Euler(20,25,-15));Steps(1);if(i%600==0)Debug.Log($"GUIDED REPLAY t={i*Dt:F1} centre={Centre:F3} face={level.Climbing.SurfaceName} load={level.Guidance.Button.Load:F3} door={level.Guidance.DoorTravel:F3} out={level.Organism.EscapedCount}");}
            Assert.That(level.Completed,Is.True,"A learned switch-to-exit chain should work again without a second demonstration.");
        }
        private void Capture(string name)
        {
            string directory=System.Environment.GetEnvironmentVariable("VENOM_GUIDANCE_CAPTURE_DIR")??System.Environment.GetEnvironmentVariable("VENOM_CAPTURE_DIR");
            if(string.IsNullOrEmpty(directory)||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            Directory.CreateDirectory(directory);level.GetComponent<VenomHud>().FrameChamber(720,1280);level.GetComponent<VenomHud>().RefreshSelection();level.Guidance.RefreshMarker();level.Organism.GetComponent<VenomSurface>().Rebuild(false);
            var target=new RenderTexture(720,1280,24){antiAliasing=4};target.Create();var old=RenderTexture.active;
            var picture=new Texture2D(720,1280,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};RenderPipeline.SubmitRenderRequest(level.View,request);RenderPipeline.SubmitRenderRequest(level.View,request);
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,720,1280),0,0);picture.Apply();File.WriteAllBytes(Path.Combine(directory,name+".png"),picture.EncodeToPNG());
            }
            finally{RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }
    }
}
#endif
