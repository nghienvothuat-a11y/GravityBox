using System;
using System.Collections;
using System.IO;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed partial class COgheViewCampaignTests
    {
        private VenomCampaign game;
        private SimulationMode simulation;
        private bool persistence;
        private InputSettings.BackgroundBehavior background;
        private InputSettings.EditorInputBehaviorInPlayMode editorInput;
        private const float Dt=1f/120;
        [UnitySetUp] public IEnumerator Before()
        {simulation=Physics.simulationMode;persistence=VenomCampaignSave.PersistenceEnabled;background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Time.timeScale=1;Physics.simulationMode=simulation;VenomCampaignSave.PersistenceEnabled=persistence;InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;yield return null;}
        private IEnumerator Load(int n)
        {
            void Loaded(Scene s,LoadSceneMode m){game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;foreach(var body in game.Matter.Bodies)Assert.Greater(body.position.y-game.Matter.Profile.ParticleRadius,-.299f,"Every spawn particle must start above the slab, not merely the body centre");for(int a=0;a<game.Props.Length;a++)for(int b=a+1;b<game.Props.Length;b++)foreach(var first in game.Props[a].CollisionShapes)foreach(var second in game.Props[b].CollisionShapes)if(Physics.ComputePenetration(first,first.transform.position,first.transform.rotation,second,second.transform.position,second.transform.rotation,out _,out float overlap))Assert.Less(overlap,.0005f,$"Initial props overlap: {game.Props[a].name} / {game.Props[b].name}");}
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"COgheView{n:00}");}finally{SceneManager.sceneLoaded-=Loaded;}
            yield return Wait(1);game.CameraRig.Frame(720,1280,0,true);
        }
        private void Tick(){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);if(!game.Owner.Paused)Physics.Simulate(Dt);}
        private IEnumerator Wait(float seconds){for(int i=0;i<seconds/Dt;i++){Tick();if(i%240==0)yield return null;}}
        private IEnumerator Until(float seconds,Func<bool> done,string reason)
        {
            for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++){Tick();if(i%240==0)yield return null;}
            string tasks="";foreach(var t in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())tasks+=$"; {t.Label} pos={t.Rail.Position} phase={t.Phase} last={t.LastFailure} stand={t.StandPoint} hand={t.HandPoint}";
            Assert.IsTrue(done(),reason+$"; level={game.Definition.Order} centre={game.Motion.Centre(0)} activity={game.Activity} failure={game.Failure}"+tasks);
        }
        private IEnumerator Tap(Vector3 point)
        {game.CameraRig.Frame(720,1280,0,true);game.TouchPoint(game.Owner.View.WorldToScreenPoint(point));yield return null;}
        private IEnumerator Solve(int n)
        {
            yield return Load(n);var rotation=game.Root.rotation;
            Capture(n,"start");
            yield return new COgheViewScenario(game,Tap,Until).Solve();
            Assert.AreEqual(32,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.IsFalse(game.Owner.Lost);
            Assert.Less(Quaternion.Angle(rotation,game.Root.rotation),.001f);Assert.IsTrue(game.Progress.Completed.Contains(game.Definition.Id));
            if(n==10)Assert.IsTrue(game.Progress.HomeUnlocked);
            game.ResetLevel();Assert.AreEqual(0,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Completed);Assert.IsFalse(game.Owner.Lost);
        }
        private void Capture(int n,string state)
        {
            string directory="Artifacts/COgheViewV2/Frames";Directory.CreateDirectory(directory);
            var camera=game.Owner.View;var target=RenderTexture.GetTemporary(720,1280,24);var previous=RenderTexture.active;
            var texture=new Texture2D(720,1280,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,720,1280),0,0);texture.Apply();File.WriteAllBytes($"{directory}/{n:00}-{state}.png",texture.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.DestroyImmediate(texture);}
        }
        [UnityTest] public IEnumerator View01Tap(){yield return Solve(1);}
        [UnityTest] public IEnumerator View02Climb(){yield return Solve(2);}
        [UnityTest] public IEnumerator View03Orbit(){yield return Solve(3);}
        [UnityTest] public IEnumerator View04Zoom(){yield return Solve(4);}
        [UnityTest] public IEnumerator View05Slider(){yield return Solve(5);}
        [UnityTest] public IEnumerator View06Practice(){yield return Solve(6);}
        [UnityTest] public IEnumerator View07Reverse(){yield return Solve(7);}
        [UnityTest] public IEnumerator View08Bridge(){yield return Solve(8);}
        [UnityTest] public IEnumerator View09Cover(){yield return Solve(9);}
        [UnityTest] public IEnumerator View10Boss(){yield return Solve(10);}
        [UnityTest] public IEnumerator OrbitAndZoomNeverRotateChamberAndRetryRestoresView()
        {
            yield return Load(5);var root=game.Root.rotation;var gravity=Physics.gravity;var rail=game.Props[0].Body.position;
            game.CameraRig.Orbit(220,720);game.CameraRig.Pinch(1.6f);game.CameraRig.Frame(720,1612,0,true,new Rect(0,40,720,1530));
            Assert.AreEqual(root,game.Root.rotation);Assert.AreEqual(gravity,Physics.gravity);Assert.AreEqual(rail,game.Props[0].Body.position);
            Assert.Less(game.CameraRig.ZoomScale,1);Assert.IsNull(game.Motion.Get(0));
            game.ResetLevel();Assert.AreEqual(1,game.CameraRig.ZoomScale);Assert.AreEqual(0,game.CameraRig.OrbitYaw);
        }
        [UnityTest] public IEnumerator RealPinchReleaseDoesNotIssueCommand()
        {
            yield return Load(1);var touch=InputSystem.AddDevice<Touchscreen>();
            try
            {
                yield return null;
                void Contact(int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 p)=>InputSystem.QueueStateEvent(touch,new TouchState{touchId=id,phase=phase,position=p});
                Vector2 p=new Vector2(Screen.width*.4f,Screen.height*.5f),q=new Vector2(Screen.width*.6f,Screen.height*.5f);
                Contact(1,UnityEngine.InputSystem.TouchPhase.Began,p);yield return null;
                Contact(2,UnityEngine.InputSystem.TouchPhase.Began,q);yield return null;
                Contact(2,UnityEngine.InputSystem.TouchPhase.Moved,q+Vector2.right*80);yield return null;
                Contact(2,UnityEngine.InputSystem.TouchPhase.Ended,q+Vector2.right*80);yield return null;
                Contact(1,UnityEngine.InputSystem.TouchPhase.Ended,p);yield return null;yield return null;
                Assert.IsNull(game.Motion.Get(0));Assert.Less(game.CameraRig.ZoomScale,1);Assert.AreEqual(Quaternion.identity,game.Root.rotation);
            }
            finally{InputSystem.RemoveDevice(touch);}
        }
        [UnityTest] public IEnumerator ClosedLidBlocksHandleAndEverySceneUsesNewCatalog()
        {
            yield return Load(9);var tasks=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();var b=Array.Find(tasks,t=>t.Label=="B");
            yield return Tap(b.HandPoint);Assert.IsFalse(b.Busy);Assert.IsFalse(b.InterlockOpen);
            for(int n=1;n<=10;n++)
            {yield return Load(n);Assert.IsTrue(game.Definition.ViewOnly);Assert.IsFalse(game.Definition.CanRotate);Assert.AreEqual(30,game.PlayableLevelCount);Assert.AreEqual($"coghe.view.v2.{n:00}",game.Definition.Id);Assert.AreEqual("",game.Definition.ProgressKey);if(n==10)Assert.IsEmpty(game.Definition.Lesson);}
        }

        [UnityTest] public IEnumerator DragAndThirdFingerDoNotBecomeMovementCommands()
        {
            yield return Load(1);var touch=InputSystem.AddDevice<Touchscreen>();
            void Contact(int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 point)=>InputSystem.QueueStateEvent(touch,new TouchState{touchId=id,phase=phase,position=point});
            Vector2 p=new Vector2(Screen.width*.35f,Screen.height*.5f);
            try
            {
                yield return null;Contact(1,UnityEngine.InputSystem.TouchPhase.Began,p);yield return null;
                Contact(1,UnityEngine.InputSystem.TouchPhase.Moved,p+Vector2.right*100);yield return null;
                Contact(1,UnityEngine.InputSystem.TouchPhase.Ended,p+Vector2.right*100);yield return null;yield return null;
                Assert.IsNull(game.Motion.Get(0));Assert.AreNotEqual(0,game.CameraRig.OrbitYaw);Assert.AreEqual(Quaternion.identity,game.Root.rotation);
                game.CameraRig.Overview();
                Contact(1,UnityEngine.InputSystem.TouchPhase.Began,p);yield return null;
                Contact(2,UnityEngine.InputSystem.TouchPhase.Began,p+Vector2.right*80);yield return null;
                Contact(3,UnityEngine.InputSystem.TouchPhase.Began,p+Vector2.up*40);yield return null;
                Contact(3,UnityEngine.InputSystem.TouchPhase.Ended,p+Vector2.up*40);yield return null;
                Contact(2,UnityEngine.InputSystem.TouchPhase.Moved,p+Vector2.right*150);yield return null;
                Contact(2,UnityEngine.InputSystem.TouchPhase.Ended,p+Vector2.right*150);yield return null;
                Contact(1,UnityEngine.InputSystem.TouchPhase.Ended,p);yield return null;yield return null;
                Assert.AreEqual(1,game.CameraRig.ZoomScale);Assert.IsNull(game.Motion.Get(0));
            }
            finally{InputSystem.RemoveDevice(touch);}
        }

        [UnityTest] public IEnumerator RetryDuringPinchRequiresAllFingersToRelease()
        {
            yield return Load(1);var touch=InputSystem.AddDevice<Touchscreen>();
            Vector2 p=new Vector2(Screen.width*.4f,Screen.height*.5f),q=new Vector2(Screen.width*.6f,Screen.height*.5f);
            void Contact(int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 point)=>InputSystem.QueueStateEvent(touch,new TouchState{touchId=id,phase=phase,position=point});
            try
            {
                yield return null;Contact(1,UnityEngine.InputSystem.TouchPhase.Began,p);yield return null;
                Contact(2,UnityEngine.InputSystem.TouchPhase.Began,q);yield return null;
                game.ResetLevel();
                Contact(2,UnityEngine.InputSystem.TouchPhase.Moved,q+Vector2.right*80);yield return null;
                Contact(2,UnityEngine.InputSystem.TouchPhase.Ended,q+Vector2.right*80);yield return null;
                Contact(1,UnityEngine.InputSystem.TouchPhase.Ended,p);yield return null;yield return null;
                Assert.AreEqual(1,game.CameraRig.ZoomScale);Assert.IsNull(game.Motion.Get(0));
                Contact(1,UnityEngine.InputSystem.TouchPhase.Began,p);yield return null;
                Contact(1,UnityEngine.InputSystem.TouchPhase.Moved,p+Vector2.right*80);yield return null;
                Contact(1,UnityEngine.InputSystem.TouchPhase.Ended,p+Vector2.right*80);yield return null;yield return null;
                Assert.AreNotEqual(0,game.CameraRig.OrbitYaw,"Fresh gestures work after the old contacts end");
            }
            finally{InputSystem.RemoveDevice(touch);}
        }

        [UnityTest] public IEnumerator FocusLossAndPauseDropPendingTap()
        {
            yield return Load(1);var touch=InputSystem.AddDevice<Touchscreen>();
            Vector2 p=new Vector2(Screen.width*.5f,Screen.height*.5f);
            void Contact(UnityEngine.InputSystem.TouchPhase phase)=>InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=phase,position=p});
            try
            {
                yield return null;Contact(UnityEngine.InputSystem.TouchPhase.Began);yield return null;
                game.SendMessage("OnApplicationFocus",false);game.SendMessage("OnApplicationFocus",true);
                Contact(UnityEngine.InputSystem.TouchPhase.Ended);yield return null;yield return null;
                Assert.IsNull(game.Motion.Get(0));
                Contact(UnityEngine.InputSystem.TouchPhase.Began);yield return null;
                game.Owner.TogglePause();yield return null;game.Owner.TogglePause();
                Contact(UnityEngine.InputSystem.TouchPhase.Ended);yield return null;yield return null;
                Assert.IsNull(game.Motion.Get(0));
            }
            finally{InputSystem.RemoveDevice(touch);}
        }

        [UnityTest] public IEnumerator PinchKeepsMidpointAndOverviewFitsTallSafeArea()
        {
            yield return Load(1);game.CameraRig.Frame(Screen.width,Screen.height,0,true,Screen.safeArea);
            var camera=game.Owner.View;var plane=new Plane(camera.transform.forward,game.CameraRig.OverviewBounds.center);
            Vector2 point=new Vector2(Screen.width*.60f,Screen.height*.51f);var ray=camera.ScreenPointToRay(point);
            Assert.IsTrue(plane.Raycast(ray,out float distance));Vector3 world=ray.GetPoint(distance);
            game.CameraRig.PinchAt(1.35f,point);
            Assert.Less(Vector2.Distance(point,camera.WorldToScreenPoint(world)),2f,"Zoom stays around the fingers, without following the organism");
            game.CameraRig.Overview();
            foreach(int height in new[]{1280,1612})foreach(float yaw in new[]{0f,90f,180f,270f})
            {
                game.CameraRig.Overview();game.CameraRig.Orbit(-yaw*3,720);
                var safe=new Rect(0,40,720,height-120);game.CameraRig.Frame(720,height,0,true,safe);
                Rect usable=game.CameraRig.UsableRect(720,height,safe);
                for(int corner=0;corner<8;corner++)
                {
                    Vector3 v=camera.WorldToViewportPoint(game.Root.TransformPoint(VenomCampaignCamera.Corner(game.CameraRig.OverviewBounds,corner)));
                    Assert.That(v.x*720,Is.InRange(usable.xMin-1,usable.xMax+1));Assert.That(v.y*height,Is.InRange(usable.yMin-1,usable.yMax+1));
                }
            }
        }

        [UnityTest] public IEnumerator OneTapPauseReverseAndRetryUsePhysicalGateState()
        {
            yield return Load(5);var task=game.Owner.Apparatus.GetComponentInChildren<COgheTapRail>();
            Assert.IsFalse(game.FinalExitAvailable);yield return Tap(task.HandPoint);
            yield return Until(12,()=>task.Phase==COgheTapRail.TaskPhase.Operating,"Reach handle");
            for(int i=0;i<4;i++)yield return Tap(task.HandPoint);
            float time=game.Matter.SimulationTime,position=task.Rail.Position;game.Owner.TogglePause();yield return Wait(3);
            Assert.AreEqual(time,game.Matter.SimulationTime);Assert.AreEqual(position,task.Rail.Position);
            game.Owner.TogglePause();yield return Until(20,()=>task.CompletedJourneys==1&&game.FinalExitAvailable,"Complete original task and open real door");
            yield return Tap(task.HandPoint);yield return Until(20,()=>task.CompletedJourneys==2&&!game.FinalExitAvailable,"Reverse closes the real door");
            game.ResetLevel();Assert.IsFalse(task.Busy);Assert.AreEqual(0,task.CompletedJourneys);Assert.IsFalse(game.FinalExitAvailable);Assert.AreEqual(0,game.Matter.EscapedCount);
        }

        [UnityTest] public IEnumerator NearPanesPassInputAndInteriorWallStillOccludes()
        {
            yield return Load(1);yield return Tap(new Vector3(.2f,-.3f,-.1f));
            Assert.AreEqual("Laboratory floor",game.Feedback.CommandSurface.name);
            Assert.That(game.Motion.Get(0).Target.x,Is.EqualTo(.2f).Within(.01f));
            yield return Load(3);yield return Tap(game.Owner.Outlet.position);
            Assert.IsFalse(game.Motion.Get(0)?.Exit??false,"An opaque interior wall cannot be tapped through");
            Assert.IsFalse(game.Owner.Completed);Assert.IsFalse(game.Feedback.CommandSurface.ExteriorGlass);
        }

        [UnityTest] public IEnumerator V2CompletionRetainsLegacyProgressAndHome()
        {
            yield return Load(1);Assert.AreEqual("",game.Definition.ProgressKey);
            string fixture="coghe.view.test."+Guid.NewGuid().ToString("N"),before=PlayerPrefs.GetString(VenomCampaignSave.Key,"");
            try
            {
                VenomCampaignSave.PersistenceEnabled=true;
                PlayerPrefs.SetString(fixture,"{\"Version\":2,\"Completed\":[\"venom.origin.01\",\"venom.origin.10\"],\"HomeUnlocked\":true,\"RevealHome\":false}");
                var save=VenomCampaignSave.Read(fixture);save.Win(game.Definition);var loaded=VenomCampaignSave.Read(fixture);
                CollectionAssert.AreEquivalent(new[]{"venom.origin.01","venom.origin.10","coghe.view.v2.01"},loaded.Completed);
                Assert.IsTrue(loaded.HomeUnlocked);Assert.IsFalse(loaded.RevealHome);Assert.AreEqual(before,PlayerPrefs.GetString(VenomCampaignSave.Key,""));
            }
            finally{VenomCampaignSave.PersistenceEnabled=false;PlayerPrefs.DeleteKey(fixture);PlayerPrefs.Save();}
        }
    }
}
