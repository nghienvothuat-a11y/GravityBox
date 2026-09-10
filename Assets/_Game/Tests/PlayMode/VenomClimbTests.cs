#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class VenomClimbTests
    {
        private VenomLevelController level;
        private SimulationMode previousMode;
        private const float Dt=1f/120;
        private Vector3 Centre=>level.Locomotion.Selected.Centre;
        [UnitySetUp] public IEnumerator Setup()
        {
            Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return null;yield return null;
            previousMode=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Venom/Venom04.unity",new LoadSceneParameters(LoadSceneMode.Single));
            level=Object.FindFirstObjectByType<VenomLevelController>();
            level.enabled=false;level.Rotation.enabled=false;level.GetComponent<VenomInput>().enabled=false;level.ResetExperiment();
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;Physics.simulationMode=previousMode;Time.timeScale=1;
        }
        private void Steps(int n)
        {
            for(int i=0;i<n;i++){level.Step(Dt);level.Rotation.Step(Dt);Physics.Simulate(Dt);}
        }
        private void Contained()
        {
            Assert.That(level.Lost,Is.False);
            Assert.That(level.Organism.FragmentCount,Is.EqualTo(1));
            foreach(var body in level.Organism.Bodies)
            {
                Vector3 p=level.Rotation.transform.InverseTransformPoint(body.position);
                Assert.That(Mathf.Max(Mathf.Abs(p.x),Mathf.Abs(p.y),Mathf.Abs(p.z)),Is.LessThan(.247f),"All physical nodes stay inside the real cube.");
                Assert.That(body.isKinematic,Is.False);
            }
        }
        [Test] public void CrawlTurnsCornersAndHoldsCeiling()
        {
            Steps(120);Capture("04-floor");Vector3 start=Centre;
            Vector2 stick=new Vector2(Vector3.Dot(level.View.transform.right,Vector3.right),Vector3.Dot(level.View.transform.up,Vector3.right)).normalized;
            bool wall=false,ceiling=false;
            for(int i=0;i<4200;i++)
            {
                level.Locomotion.SetInput(level.Climbing.ScreenDirection(stick));Steps(1);Contained();
                if(i%600==0)Debug.Log($"CLIMB {i*Dt:F1}s centre={Centre:F3} face={level.Climbing.SurfaceName} mask={level.Climbing.VisitedMask}");
                if(!wall && Centre.y>-.08f && Centre.x>.19f){wall=true;Capture("04-wall");}
                if(Centre.y>.19f && Centre.x<.08f){ceiling=true;break;}
            }
            Assert.That(wall,Is.True,"Body must climb the wall from the floor without repositioning.");
            Assert.That(ceiling,Is.True,"Held input must wrap around the wall-ceiling corner.");
            level.Locomotion.SetInput(level.Climbing.ScreenDirection(Vector2.zero));Steps(180);Vector3 parked=Centre;Capture("04-ceiling");Steps(360);
            Assert.That(Vector3.Distance(Centre,parked),Is.LessThan(.035f));
            Assert.That(Centre.y,Is.GreaterThan(.19f));Contained();
            Quaternion target=Quaternion.Euler(105,40,20);Vector3 local=level.Rotation.transform.InverseTransformPoint(Centre);
            for(int i=0;i<1200;i++){level.Rotation.SetTargetOrientation(target);Steps(1);Contained();}
            Capture("04-rotated");
            Debug.Log($"ROTATE drift={Vector3.Distance(level.Rotation.transform.InverseTransformPoint(Centre),local):F4}");
            Assert.That(Quaternion.Angle(level.Rotation.Orientation,target),Is.LessThan(1));
            Assert.That(Vector3.Distance(level.Rotation.transform.InverseTransformPoint(Centre),local),Is.LessThan(.06f),"Idle adhesion follows the moving box in local space.");
            Assert.That(level.Climbing.VisitedCount,Is.GreaterThanOrEqualTo(3));
        }
        [Test] public void UnsupportedInputCannotFlyAndResetClearsGrip()
        {
            foreach(var body in level.Organism.Bodies){body.position+=Vector3.up*.2f;body.linearVelocity=Vector3.zero;}
            Physics.SyncTransforms();level.Locomotion.SetInput(Vector3.right);Steps(10);
            Vector3 velocity=level.Organism.Bodies.Aggregate(Vector3.zero,(v,b)=>v+b.linearVelocity)/32;
            Assert.That(Mathf.Abs(velocity.x),Is.LessThan(.001f));Assert.That(velocity.y,Is.LessThan(-.7f));
            level.ResetExperiment();Steps(120);Vector3 start=Centre;
            level.Locomotion.SetInput(Vector3.right);Steps(100);Assert.That(Centre.x,Is.GreaterThan(start.x+.04f));
            level.TogglePause();float time=level.Organism.SimulationTime;Steps(60);Assert.That(level.Organism.SimulationTime,Is.EqualTo(time));
            level.TogglePause();Assert.That(level.Rotation.InputEnabled,Is.True);
            level.ResetExperiment();Assert.That(level.Climbing.VisitedCount,Is.Zero);Assert.That(level.Locomotion.Input,Is.EqualTo(Vector3.zero));
        }
        [Test] public void FingerTransitionsSeparateCrawlFromRotation()
        {
            Steps(120);var input=level.GetComponent<VenomInput>();
            Vector2 a=new Vector2(Screen.width*.4f,Screen.height*.5f),b=a+Vector2.right*Screen.width*.16f;
            input.ApplyClimbTouch(1,a,Vector2.zero);input.ApplyClimbTouch(1,b,Vector2.zero);
            Assert.That(level.Locomotion.Input.magnitude,Is.GreaterThan(.1f));Assert.That(input.Holding,Is.True);
            input.ApplyClimbTouch(2,b,a);Assert.That(level.Locomotion.Input,Is.EqualTo(Vector3.zero));Assert.That(input.Rotating,Is.True);
            input.ApplyClimbTouch(2,b+Vector2.up*40,a+Vector2.up*40);Steps(100);
            Assert.That(Quaternion.Angle(level.Rotation.Orientation,Quaternion.identity),Is.GreaterThan(1));
            input.ApplyClimbTouch(1,a,Vector2.zero);input.ApplyClimbTouch(1,b,Vector2.zero);
            Assert.That(level.Locomotion.Input,Is.EqualTo(Vector3.zero));Assert.That(input.Holding,Is.False);
            input.ApplyClimbTouch(0,Vector2.zero,Vector2.zero);
            input.ApplyClimbTouch(1,a,Vector2.zero);input.ApplyClimbTouch(1,b,Vector2.zero);
            Assert.That(input.Holding,Is.True);Assert.That(level.Locomotion.Input.magnitude,Is.GreaterThan(.1f));
            input.ApplyClimbTouch(0,Vector2.zero,Vector2.zero);
        }
        [Test] public void EveryNodeTraversesTheRealExit()
        {
            Steps(120);
            for(int i=0;i<2600&&!level.Completed&&!level.Lost;i++)
            {
                if(level.Locomotion.Selected!=null)
                    level.Locomotion.SetInput(Vector3.ClampMagnitude(Vector3.ProjectOnPlane(new Vector3(0,0,-.12f)-Centre,Vector3.up)/.025f,1));
                Steps(1);
            }
            Debug.Log($"CLIMB EXIT {level.Organism.EscapedCount}/32 lost={level.Lost}");
            Assert.That(level.Completed,Is.True);Assert.That(level.Organism.EscapedCount,Is.EqualTo(32));
            Assert.That(level.Organism.TotalMass,Is.EqualTo(.096f).Within(.000001f));
        }
        private void Capture(string name)
        {
            string folder=Environment.GetEnvironmentVariable("VENOM_CAPTURE_DIR");
            if(string.IsNullOrEmpty(folder)||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            Directory.CreateDirectory(folder);level.Organism.GetComponent<VenomSurface>().Rebuild(false);
            level.GetComponent<VenomGlassVisibility>().Refresh();
            level.GetComponent<VenomHud>().RefreshSelection();level.View.aspect=720f/1280;
            level.GetComponent<VenomHud>().FrameChamber(720,1280);
            var target=new RenderTexture(720,1280,24){antiAliasing=4};target.Create();var old=RenderTexture.active;
            var picture=new Texture2D(720,1280,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};
                RenderPipeline.SubmitRenderRequest(level.View,request);RenderPipeline.SubmitRenderRequest(level.View,request);
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,720,1280),0,0);picture.Apply();
                File.WriteAllBytes(Path.Combine(folder,name+".png"),picture.EncodeToPNG());
            }
            finally{RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }
    }
}
#endif
