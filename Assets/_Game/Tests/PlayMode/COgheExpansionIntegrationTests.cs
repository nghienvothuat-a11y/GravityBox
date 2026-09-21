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
    public sealed class COgheExpansionIntegrationTests
    {
        private VenomCampaign game;
        private SimulationMode previous;
        [UnitySetUp] public IEnumerator Before()
        {previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Time.timeScale=1;Physics.simulationMode=previous;VenomCampaignSave.PersistenceEnabled=true;yield return null;}

        [UnityTest] public IEnumerator LegacyTwentyScenesKeepTheirIdentityAndExpansionCanIdleResetAndRender()
        {
            Assert.AreEqual(40,VenomCampaign.LevelCount);
            var idleFailures=new System.Collections.Generic.List<string>();
            for(int n=1;n<=20;n++)
            {
                void Loaded(Scene scene,LoadSceneMode mode)
                {
                    game=Object.FindFirstObjectByType<VenomCampaign>();
                    if(game==null)return;
                    game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
                }
                SceneManager.sceneLoaded+=Loaded;
                try{yield return SceneManager.LoadSceneAsync($"VenomOrigin{n:00}");}
                finally{SceneManager.sceneLoaded-=Loaded;}
                Assert.NotNull(game,$"Scene {n}");
                foreach(var sceneRoot in SceneManager.GetActiveScene().GetRootGameObjects())
                    foreach(var component in sceneRoot.GetComponentsInChildren<MonoBehaviour>(true))
                        Assert.IsNotNull(component,$"Missing behaviour in scene {n}, root {sceneRoot.name}");
                Assert.AreEqual($"venom.origin.{n:00}",game.Definition.Id);
                Assert.AreEqual(n,game.Definition.Order);Assert.AreEqual(n%10==0,game.Definition.Boss);
                Assert.AreEqual((n-1)/10,game.LevelPage);
                foreach(var surface in game.Surfaces)
                {
                    if(surface.Slippery||surface.HasSlipRegion)
                        Assert.IsTrue(surface.Selectable,$"Level {n}: slick surface {surface.name} must accept commands.");
                    if(surface.name!="Glass face 5"&&surface.name!="Laboratory ceiling")continue;
                    // Approved 18/09 exception: source 17 is displayed as 24.
                    // Its roof passes taps to the high bearing; it still has
                    // physical collision and grip. See gear-selection-24.md.
                    if(n==17&&surface.name=="Laboratory ceiling")
                    {
                        Assert.IsFalse(surface.Selectable,"The gear lesson roof must pass taps through to its bearing.");
                        Assert.IsTrue(surface.Shape.enabled);
                        Assert.IsTrue(surface.Grip(surface.transform.position));
                        continue;
                    }
                    Assert.IsTrue(surface.Selectable&&surface.InterceptExterior,$"Level {n}: roof must remain pickable even without rotation.");
                    bool selected=false;
                    foreach(float x in new[]{-.3f,0f,.3f})foreach(float y in new[]{-.3f,0f,.3f})
                    {
                        game.Feedback.ResetFeedback();
                        var point=surface.transform.TransformPoint(new Vector3(x*surface.Size.x,y*surface.Size.y,0));
                        game.TouchPoint(game.Owner.View.WorldToScreenPoint(point));
                        selected|=game.Feedback.CommandSurface==surface;
                    }
                    Assert.IsTrue(selected,$"Level {n}: visible roof {surface.name} must accept an actual screen tap.");
                }
                game.ResetLevel();Physics.SyncTransforms();
                if(n<11)continue;
                for(int tick=0;tick<600;tick++)
                {game.Owner.Step(1f/120);game.Owner.Rotation.Step(1f/120);Physics.Simulate(1f/120);if(tick%240==0)yield return null;}
                Capture(game,$"{n:00}-overview");
                if(game.Owner.Lost)idleFailures.Add($"Idle spawn {n}: {game.Failure}");
                if(game.Owner.Completed)idleFailures.Add($"Idle must not solve {n}");
                game.ResetLevel();Physics.SyncTransforms();
                Assert.IsFalse(game.Owner.Completed);Assert.IsFalse(game.Owner.Lost);
                Assert.AreEqual(0,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
                Assert.AreEqual(game.Definition.CanRotate,game.Owner.Rotation.InputEnabled);
            }
            Assert.IsEmpty(idleFailures,string.Join("\n",idleFailures));
        }

        public static void Capture(VenomCampaign game,string name,int width=720,int height=1280)
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            string dir="Artifacts/COgheExpansion/Frames";Directory.CreateDirectory(dir);
            var camera=game.Owner.View;camera.aspect=width/(float)height;
            game.CameraRig.Frame(width,height,0,true);
            game.Matter.GetComponent<VenomSurface>().Rebuild(false);game.Feedback?.Refresh();
            var target=new RenderTexture(width,height,24){antiAliasing=4};target.Create();
            var old=RenderTexture.active;var picture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};
                RenderPipeline.SubmitRenderRequest(camera,request);RenderPipeline.SubmitRenderRequest(camera,request);
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,width,height),0,0);picture.Apply();
                File.WriteAllBytes(Path.Combine(dir,name+".png"),picture.EncodeToPNG());
            }
            finally{RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }
    }
}
