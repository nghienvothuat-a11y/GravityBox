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
                if(Centre.y>.20f && Centre.x<.14f){ceiling=true;break;}
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
        [Test] public void EveryNodeClimbsAndTraversesTheCeilingExit()
        {
            Steps(120);
            Assert.That(level.Outlet.localPosition,Is.EqualTo(new Vector3(0,.253f,0)));
            Assert.That(Vector3.Dot(level.Outlet.forward,Vector3.up),Is.GreaterThan(.99f));
            Assert.That(level.CrawlFaces[0].Raycast(new Ray(new Vector3(0,-.18f,0),Vector3.down),out _, .1f),Is.True,"The old floor opening must be physically sealed.");
            Assert.That(level.CrawlFaces[5].Raycast(new Ray(new Vector3(0,.18f,0),Vector3.up),out _, .1f),Is.False,"The centre of the ceiling must really be open.");
            Assert.That(level.CrawlFaces[5].Raycast(new Ray(new Vector3(.10f,.18f,0),Vector3.up),out _, .1f),Is.True);
            level.Locomotion.SetInput(Vector3.right);
            for(int i=0;i<3000 && !(Centre.y>.20f && Centre.x<.14f);i++)Steps(1);
            Assert.That(Centre.y,Is.GreaterThan(.20f));
            Assert.That(level.Climbing.VisitedMask&(1<<2),Is.Not.Zero,"The route must first climb the right wall.");
            for(int i=0;i<2600&&!level.Completed&&!level.Lost;i++)
            {
                if(level.Locomotion.Selected!=null)
                    level.Locomotion.SetInput(Vector3.ClampMagnitude(Vector3.ProjectOnPlane(level.Outlet.position-Centre,Vector3.up)/.025f,1));
                Steps(1);
            }
            Debug.Log($"CEILING EXIT {level.Organism.EscapedCount}/32 lost={level.Lost}");
            if(!level.Completed)Debug.Log(string.Join("; ",level.Organism.Bodies.Select((body,i)=>$"{i}:out={level.Organism.Escaped[i]} p={body.position:F4}")));
            Assert.That(level.Completed,Is.True);Assert.That(level.Organism.EscapedCount,Is.EqualTo(32));
            Steps(360);
            Assert.That(level.Organism.Bodies.All(body=>body.position.y>.265f),Is.True,"Escaped tissue stays visibly outside the upward-facing hole.");
            Assert.That(level.Organism.TotalMass,Is.EqualTo(.096f).Within(.000001f));
            Capture("04-ceiling-escaped");
        }
        [Test] public void ZoomFollowsMovingCreatureAndReturnsToOverview()
        {
            Steps(120);var hud=level.GetComponent<VenomHud>();
            hud.FrameChamber(540,960,.02f);Vector3 overview=level.View.transform.position;float wide=level.View.orthographicSize;
            Vector3[] particles=level.Organism.Bodies.Select(b=>b.position).ToArray();Quaternion heading=level.View.transform.rotation;
            level.ToggleZoom();hud.FrameChamber(540,960,1f/60);
            Assert.That(Vector3.Distance(level.View.transform.position,overview),Is.LessThan(.03f),"Zoom eases in, rather than cutting instantly.");
            for(int i=0;i<90;i++)hud.FrameChamber(540,960,1f/60);
            Assert.That(level.FollowView.Zoomed,Is.True);Assert.That(level.View.orthographicSize,Is.LessThan(wide*.4f));
            Assert.That(Vector3.Distance(level.View.transform.position,Centre),Is.LessThan(.5f),"The camera physically approaches the creature.");
            Assert.That(Quaternion.Angle(level.View.transform.rotation,heading),Is.LessThan(.01f));
            for(int i=0;i<32;i++)Assert.That(level.Organism.Bodies[i].position,Is.EqualTo(particles[i]),"Camera changes cannot move physics bodies.");
            Capture("04-zoom-floor");
            Vector3 start=level.View.transform.position;
            level.Locomotion.SetInput(Vector3.forward);
            for(int i=0;i<180;i++){Steps(1);hud.FrameChamber(540,960,Dt);}
            Assert.That(Vector3.Distance(level.View.transform.position,start),Is.GreaterThan(.15f));
            Vector3 viewport=level.View.WorldToViewportPoint(Centre);
            Assert.That(viewport.x,Is.InRange(.35f,.65f));Assert.That(viewport.y,Is.InRange(.42f,.68f));
            Assert.That(Quaternion.Angle(level.View.transform.rotation,heading),Is.LessThan(.01f));
            level.Locomotion.SetInput(Vector3.zero);Capture("04-zoom-wall");
            level.TogglePause();level.ToggleZoom();for(int i=0;i<90;i++)hud.FrameChamber(540,960,1f/60);
            Assert.That(level.View.orthographicSize,Is.EqualTo(wide).Within(.0001f));
            Assert.That(Vector3.Distance(level.View.transform.position,overview),Is.LessThan(.0001f));
            level.ToggleZoom();level.ResetExperiment();Assert.That(level.FollowView.Zoomed,Is.False);
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
