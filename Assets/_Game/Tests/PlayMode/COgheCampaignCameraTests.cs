using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed class COgheCampaignCameraTests
    {
        private VenomCampaign game;
        private SimulationMode previous;
        private const string DirectoryName="Artifacts/COgheCamera";
        [UnitySetUp] public IEnumerator Before()
        {previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Physics.simulationMode=previous;Time.timeScale=1;VenomCampaignSave.PersistenceEnabled=true;yield return null;}
        private IEnumerator Load(int number)
        {
            void Loaded(Scene scene,LoadSceneMode mode)
            {
                game=Object.FindFirstObjectByType<VenomCampaign>();
                game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            }
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"VenomOrigin{number:00}");}
            finally{SceneManager.sceneLoaded-=Loaded;}
            yield return null;
            for(int i=0;i<30;i++){game.Owner.Step(1f/120);Physics.Simulate(1f/120);}
        }
        private void AssertVisible(Bounds bounds,int width,int height,Rect safeArea)
        {
            var rect=game.CameraRig.UsableRect(width,height,safeArea);
            // The camera is projected explicitly into the same dimensions as the
            // requested render, independent of the Editor's host window size.
            for(int i=0;i<8;i++)
            {
                Vector3 world=game.Root.TransformPoint(VenomCampaignCamera.Corner(bounds,i));
                if(game.Surfaces.Length==1&&game.Surfaces[0].SphereRadius>0)
                {
                    Vector3 axis=(i&1)==0?game.Owner.View.transform.right:game.Owner.View.transform.up;
                    world=game.Root.TransformPoint(bounds.center)+axis*((i&2)==0?-bounds.extents.x:bounds.extents.x);
                }
                var p=game.Owner.View.WorldToViewportPoint(world);
                Assert.Greater(p.z,game.Owner.View.nearClipPlane);
                Assert.That(p.x*width,Is.InRange(rect.xMin-.1f,rect.xMax+.1f),$"Level {game.Definition.Order} horizontal framing");
                Assert.That(p.y*height,Is.InRange(rect.yMin-.1f,rect.yMax+.1f),$"Level {game.Definition.Order} vertical framing");
            }
        }
        private void Capture(string name,int width=720,int height=1280)
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            Directory.CreateDirectory(DirectoryName);var camera=game.Owner.View;
            game.Matter.GetComponent<VenomSurface>().Rebuild(false);game.Feedback.Refresh();
            var target=new RenderTexture(width,height,24){antiAliasing=4};target.Create();
            var old=RenderTexture.active;var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};
                RenderPipeline.SubmitRenderRequest(camera,request);RenderPipeline.SubmitRenderRequest(camera,request);
                RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(DirectoryName,name+".png"),image.EncodeToPNG());
            }
            finally{RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(image);}
        }
        [UnityTest] public IEnumerator EveryOverviewFitsPortraitAndRotatingCornersWithoutChangingPhysics()
        {
            Directory.CreateDirectory(DirectoryName);
            var report=new System.Text.StringBuilder("level,old_size,new_size,linear_enlargement\n");
            for(int n=1;n<=20;n++)
            {
                yield return Load(n);var camera=game.Owner.View;
                camera.aspect=720f/1280;camera.transform.rotation=Quaternion.Euler(game.Definition.CameraEuler);
                float oldSize=game.Definition.ViewRadius*1280/(720*.87f)*(n>=3?1.1f:1);
                camera.orthographicSize=oldSize;camera.transform.position=-camera.transform.forward*2-(n>=3?camera.transform.up*oldSize*.08f:Vector3.zero);
                Capture($"{n:00}-before");
                game.CameraRig.Frame(720,1280,0,true);float newSize=camera.orthographicSize;
                Capture($"{n:00}-after");
                report.AppendLine($"{n},{oldSize:F4},{newSize:F4},{oldSize/newSize:F3}");
                Assert.Less(newSize,oldSize,$"Level {n} should be closer");
                var savedPosition=game.Matter.Bodies[0].position;var savedRoot=game.Root.rotation;
                foreach(int height in new[]{1280,1612})
                {
                    var safeArea=new Rect(0,24,720,height-52);
                    game.CameraRig.Frame(720,height,0,true,safeArea);
                    AssertVisible(game.CameraRig.OverviewBounds,720,height,safeArea);
                    if(height==1612)Capture($"{n:00}-oppo-aspect",720,height);
                }
                Assert.AreEqual(savedPosition,game.Matter.Bodies[0].position);
                Assert.AreEqual(savedRoot,game.Root.rotation);
                if(game.Definition.CanRotate&&n!=6)
                {
                    // Deliberately rotate the test fixture, not the camera code.
                    foreach(var euler in new[]{new Vector3(70,31,22),new Vector3(180,90,0),new Vector3(38,143,95)})
                    {
                        game.Root.rotation=Quaternion.Euler(euler);Physics.SyncTransforms();
                        game.CameraRig.Frame(720,1280,1f/60);
                        AssertVisible(game.CameraRig.OverviewBounds,720,1280,new Rect(0,0,720,1280));
                    }
                    Capture($"{n:00}-rotated");game.Root.rotation=savedRoot;Physics.SyncTransforms();
                }
            }
            File.WriteAllText(Path.Combine(DirectoryName,"framing.csv"),report.ToString());
        }
        [UnityTest] public IEnumerator ZonesFollowSelectionSmoothlyAndReturnToOverviewOnReset()
        {
            foreach(int n in new[]{8,19,20})
            {
                yield return Load(n);Assert.AreEqual(n==20?3:2,game.CameraRig.ZoneCount);
                game.CameraRig.Frame(720,1280,0,true);float overview=game.Owner.View.orthographicSize;
                for(int zone=0;zone<game.CameraRig.ZoneCount;zone++)
                {
                    var before=game.Owner.View.transform.position;
                    game.CameraRig.SelectZone(zone);game.CameraRig.Frame(720,1280,1f/60);
                    Assert.Less(Vector3.Distance(before,game.Owner.View.transform.position),.20f,"No cut between viewpoints");
                    for(int i=0;i<90;i++)game.CameraRig.Frame(720,1280,1f/60);
                    AssertVisible(game.Definition.CameraZones[zone].LocalBounds,720,1280,new Rect(0,0,720,1280));
                    Assert.Less(game.Owner.View.orthographicSize,overview*.90f,$"Level {n} compartment must improve readability");
                    Capture($"{n:00}-zone-{zone+1}");
                }
                game.CameraRig.ToggleFollow();Assert.IsTrue(game.Zoom);
                for(int i=0;i<90;i++)game.CameraRig.Frame(720,1280,1f/60);
                var creature=game.Owner.View.WorldToViewportPoint(game.Motion.Centre(game.Motion.Selected));
                Assert.That(creature.x,Is.InRange(.49f,.51f));
                Capture($"{n:00}-follow");
                var point=game.Matter.Bodies[0].position;game.CameraRig.SelectZone(0);game.CameraRig.Frame(720,1280,.5f);
                Assert.AreEqual(point,game.Matter.Bodies[0].position);
                float scale=Mathf.Min(720f/540,1280f/960);
                Assert.IsFalse(game.CameraRig.AllowsPointer(new Vector2(300,1280-218*scale),720,1280),"View selector cannot issue a move or rotate command");
                game.ResetLevel();Assert.AreEqual(-1,game.CameraRig.Zone);Assert.IsFalse(game.Zoom);
                game.CameraRig.Frame(720,1280,0,true);Assert.AreEqual(overview,game.Owner.View.orthographicSize,.0001f);
            }
        }
    }
}
