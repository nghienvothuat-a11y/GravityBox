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
    public sealed partial class COgheSpatialCampaignTests
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
            try{yield return SceneManager.LoadSceneAsync($"COgheSpatial{n:00}");}finally{SceneManager.sceneLoaded-=Loaded;}
            yield return Wait(1);game.CameraRig.Frame(720,1280,0,true);
        }
        private void Tick(){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);if(!game.Owner.Paused)Physics.Simulate(Dt);}
        private IEnumerator Wait(float seconds){for(int i=0;i<seconds/Dt;i++){Tick();if(i%240==0)yield return null;}}
        private IEnumerator Until(float seconds,Func<bool> done,string reason)
        {
            for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++){Tick();if(i%240==0)yield return null;}
            string tasks="";foreach(var t in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())tasks+=$"; {t.Label} pos={t.Rail.Position} phase={t.Phase} last={t.LastFailure} stand={t.StandPoint} hand={t.HandPoint}";
            foreach(var r in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())tasks+=$"; rail {r.name} pos={r.Position:F4} velocity={r.Body.linearVelocity:F4} effort={r.Effort:F3} locked={r.Locked} latch={r.Latched}";
            foreach(var t in game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>())tasks+="; tube "+t.DebugState(game.Motion.Selected)+" chosen="+t.LastChosenEdge+" reached="+t.LastReachedNode;
            foreach(var p in game.Props)if(p.GetComponent<COgheRailSlider>()==null)tasks+=$"; loose {p.name} at {game.Root.InverseTransformPoint(p.Body.position):F3} held={game.Attached}";
            Capture(game.Definition.Order,"result");
            Assert.IsTrue(done(),reason+$"; level={game.Definition.Order} centre={game.Motion.Centre(0)} activity={game.Activity} failure={game.Failure}"+tasks);
        }
        private IEnumerator Tap(Vector3 point)
        {game.CameraRig.Frame(720,1280,0,true);game.TouchPoint(game.Owner.View.WorldToScreenPoint(point));yield return null;}
        private IEnumerator Solve(int n)
        {
            yield return Load(n);var rotation=game.Root.rotation;
            Capture(n,"start");
            yield return new COgheSpatialScenario(game,Tap,Until).Solve();
            Assert.AreEqual(32,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.IsFalse(game.Owner.Lost);
            Assert.Less(Quaternion.Angle(rotation,game.Root.rotation),.001f);Assert.IsTrue(game.Progress.Completed.Contains(game.Definition.Id));
            if(n==10)Assert.IsTrue(game.Progress.HomeUnlocked);
            game.ResetLevel();Assert.AreEqual(0,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Completed);Assert.IsFalse(game.Owner.Lost);
        }
        private void Capture(int n,string state)
        {
            string directory="Artifacts/COgheSpatial/Frames";Directory.CreateDirectory(directory);
            game.CameraRig.Frame(720,1280,0,true);var camera=game.Owner.View;var target=RenderTexture.GetTemporary(720,1280,24);var previous=RenderTexture.active;
            var texture=new Texture2D(720,1280,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,720,1280),0,0);texture.Apply();File.WriteAllBytes($"{directory}/{n:00}-{state}.png",texture.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.DestroyImmediate(texture);}
        }
        [UnityTest] public IEnumerator Spatial01Solve(){yield return Solve(1);}
        [UnityTest] public IEnumerator Spatial02Solve(){yield return Solve(2);}
        [UnityTest] public IEnumerator Spatial03Solve(){yield return Solve(3);}
        [UnityTest] public IEnumerator Spatial04Solve(){yield return Solve(4);}
        [UnityTest] public IEnumerator Spatial05Solve(){yield return Solve(5);}
        [UnityTest] public IEnumerator Spatial06Solve(){yield return Solve(6);}
        [UnityTest] public IEnumerator Spatial07Solve(){yield return Solve(7);}
        [UnityTest] public IEnumerator Spatial08Solve(){yield return Solve(8);}
        [UnityTest] public IEnumerator Spatial09Solve(){yield return Solve(9);}
        [UnityTest] public IEnumerator Spatial10Solve(){yield return Solve(10);}
    }
}
