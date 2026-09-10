#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed class VenomJourneyTests
    {
        private VenomLevelController level;
        private SimulationMode simulation;
        private bool memory,progress;
        private string progressKey;
        private Mouse mouse;
        private Touchscreen touchscreen;
        private InputSettings.BackgroundBehavior background;
        private InputSettings.EditorInputBehaviorInPlayMode editorInput;
        private const float Dt=1f/120;
        private VenomJourney Journey=>level.Journey;
        private Vector3 Local(Vector3 p)=>level.Rotation.transform.InverseTransformPoint(p);
        [UnitySetUp] public IEnumerator Setup()
        {
            simulation=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            memory=VenomGuidanceMemory.PersistenceEnabled;progress=VenomJourneyProgress.PersistenceEnabled;
            progressKey=VenomJourneyProgress.PersistenceKey;
            background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            VenomGuidanceMemory.PersistenceEnabled=VenomJourneyProgress.PersistenceEnabled=false;
            Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return null;yield return null;
        }
        private IEnumerator Load(int chapter)
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"Assets/_Game/Venom/VenomJourney{chapter:00}.unity",new LoadSceneParameters(LoadSceneMode.Single));
            level=Object.FindFirstObjectByType<VenomLevelController>();level.enabled=false;level.Rotation.enabled=false;
            level.GetComponent<VenomInput>().enabled=false;Journey.AutoAdvance=false;level.ResetExperiment();
            level.GetComponent<VenomHud>().FrameChamber(Screen.width,Screen.height);Steps(180);
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;Physics.simulationMode=simulation;Time.timeScale=1;
            VenomGuidanceMemory.PersistenceEnabled=memory;VenomJourneyProgress.PersistenceEnabled=progress;
            PlayerPrefs.DeleteKey("venom.journey.test");PlayerPrefs.Save();VenomJourneyProgress.PersistenceKey=progressKey;
            if(mouse!=null)InputSystem.RemoveDevice(mouse);
            if(touchscreen!=null)InputSystem.RemoveDevice(touchscreen);
            InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
        }
        private void Steps(int count)
        {
            for(int i=0;i<count;i++){level.Step(Dt);level.Rotation.Step(Dt);Physics.Simulate(Dt);}
        }
        private void Until(System.Func<bool> condition,int ticks,string message)
        {
            for(int i=0;i<ticks&&!condition();i++)
            {
                Steps(1);
                if(i%900==0)Debug.Log($"JOURNEY {Journey.Chapter} {message}: {Dump()}");
            }
            Assert.That(condition(),Is.True,message+": "+Dump());
        }
        private string Dump()=>string.Join("; ",level.Locomotion.Fragments.Select(f=>$"{f.Anchor}/{f.Count} face={level.Climbing.FaceFor(f.Anchor)} intent={f.Intent:F2} at {Local(f.Centre):F3} v={f.Velocity:F2} target={Journey.TaskFor(f.Anchor)?.Target:F3} {Journey.TaskFor(f.Anchor)?.Task} {Journey.TaskFor(f.Anchor)?.Feedback}"))+$" A {Journey.MassA:F3} B {Journey.MassB:F3} gate {level.GateLatched} cut {level.Organism.CutCount} out {level.Organism.EscapedCount}";
        private void Reach(int face,Vector3 p)
        {
            Assert.That(Journey.Command(face,p),Is.True);int anchor=level.Locomotion.Selected.Anchor;
            Until(()=>Journey.TaskFor(anchor)?.Arrived==true,2400,"Reach destination");
        }
        private void ExitAll()
        {
            Journey.GuideAllOut();
            for(int tick=0;tick<5400&&!level.Completed;tick++)
            {
                Steps(1);
                if(level.Organism.EscapedCount>0)
                    for(int i=0;i<32;i++)if(!level.Organism.Escaped[i])
                        Assert.That(Journey.TaskFor(i)?.Task,Is.EqualTo(VenomTask.Exit),"The tail keeps its exit order even after its owning particle leaves.");
            }
            Assert.That(level.Completed,Is.True,"Every particle exits physically: "+Dump());Steps(2);
            Assert.That(level.Organism.EscapedCount,Is.EqualTo(32));Assert.That(level.Organism.TotalMass,Is.EqualTo(.096f).Within(.000001f));
        }
        private void Cut(float bias)
        {
            Journey.SetCutBias(bias);
            Assert.That(Journey.Touch(level.View.WorldToScreenPoint(Journey.Knife.position)),Is.True,"Tap the actual raised blade, not a hidden point on the floor.");
            Assert.That(Journey.TaskFor(level.Locomotion.Selected.Anchor).Task,Is.EqualTo(VenomTask.Cut));
            Until(()=>level.Organism.FragmentCount>=2&&!Journey.Cutting,3000,"Physical division");Steps(420);
            Assert.That(level.Organism.FragmentCount,Is.EqualTo(2),Dump());Capture($"0{Journey.Chapter}-split");
        }
        private void Hold(int anchor,bool a)
        {
            Assert.That(Journey.Select(anchor),Is.True);
            Assert.That(Journey.Command(0,a?Journey.PadALocal:Journey.PadBLocal),Is.True);
        }
        [UnityTest] public IEnumerator FirstLessonLearnsFromArrivalThenAllMatterExitsFloorHole()
        {
            yield return Load(1);Capture("01-overview");
            Assert.That(Journey.Progress.Knows(VenomSkill.Arrive),Is.False);
            Reach(0,new Vector3(-.13f,-.25f,.11f));
            Assert.That(Journey.Progress.Knows(VenomSkill.Arrive),Is.True);
            Assert.That(level.Organism.EscapedCount,Is.Zero);ExitAll();Capture("01-escaped");
            Assert.That(Journey.Progress.Completed&1,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator VictoryWaitsForEveryParticleThenFramesDancingTissueAndRestoresSceneryOnRetry()
        {
            yield return Load(1);
            var scenery=SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<Renderer>(true))
                .Where(r=>!r.transform.IsChildOf(level.Organism.transform)).ToArray();
            scenery[0].forceRenderingOff=true;
            var hidden=scenery.Select(r=>r.forceRenderingOff).ToArray();
            var colliders=level.Rotation.GetComponentsInChildren<Collider>().ToArray();
            var enabled=colliders.Select(c=>c.enabled).ToArray();
            float overview=level.View.orthographicSize;
            Journey.GuideAllOut();Until(()=>level.Organism.EscapedCount>0,2400,"First tissue crosses outlet");
            Assert.That(level.Completed,Is.False);Assert.That(level.Celebration.Active,Is.False);
            CollectionAssert.AreEqual(hidden,scenery.Select(r=>r.forceRenderingOff).ToArray());
            Until(()=>level.Completed,2400,"Tail clears outlet");Steps(150);
            var hud=level.GetComponent<VenomHud>();hud.FrameChamber(Screen.width,Screen.height);
            var skin=level.Organism.GetComponent<VenomSurface>();skin.Rebuild(false);
            var life=level.Organism.GetComponent<VenomLifeAnimation>();
            Assert.That(life.DanceAmount,Is.GreaterThan(.9f));Assert.That(life.RaisedTendrilCount,Is.GreaterThanOrEqualTo(4));
            Assert.That(scenery.All(r=>r.forceRenderingOff),Is.True,"Hide glass, opaque mechanisms and markers alike.");
            CollectionAssert.AreEqual(enabled,colliders.Select(c=>c.enabled).ToArray(),"No collision changes for the shot.");
            Assert.That(level.View.orthographicSize,Is.LessThan(overview*.6f));
            Assert.That(level.Organism.GetComponentsInChildren<Renderer>().All(r=>!r.forceRenderingOff),Is.True);
            var positions=level.Organism.Bodies.Select(b=>b.position).ToArray();skin.Rebuild(false);
            CollectionAssert.AreEqual(positions,level.Organism.Bodies.Select(b=>b.position).ToArray(),"Dance only deforms the rendered skin.");
            Capture("victory-floor-dance");
            level.TogglePause();float time=level.Celebration.Elapsed;Vector3 camera=level.View.transform.position;
            for(int i=0;i<600;i++)level.Step(Dt);
            hud.FrameChamber(Screen.width,Screen.height);
            Assert.That(level.Celebration.Elapsed,Is.EqualTo(time));Assert.That(Vector3.Distance(camera,level.View.transform.position),Is.LessThan(.00001f));
            Assert.That(level.Celebration.ReadyForNext,Is.False);
            level.ResetExperiment();hud.FrameChamber(Screen.width,Screen.height);
            Assert.That(level.Celebration.Active,Is.False);Assert.That(level.Organism.EscapedCount,Is.Zero);
            CollectionAssert.AreEqual(hidden,scenery.Select(r=>r.forceRenderingOff).ToArray());
            Assert.That(level.View.orthographicSize,Is.EqualTo(overview).Within(.001f));
        }
        [UnityTest] public IEnumerator CelebrationFinishesBeforeAutomaticAdvanceAndDoesNotLeakIntoTheNextLesson()
        {
            yield return Load(1);ExitAll();Steps(390);Journey.AutoAdvance=true;
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("VenomJourney01"),"The old three-second timer must not cut the dance short.");
            Steps(220);level.TogglePause();yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("VenomJourney01"),"Pausing on the final beat still defers the scene change.");
            level.TogglePause();yield return null;yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("VenomJourney02"));
            level=Object.FindFirstObjectByType<VenomLevelController>();level.enabled=false;level.Rotation.enabled=false;
            Assert.That(level.Celebration.Active,Is.False);Assert.That(level.Completed,Is.False);
            Assert.That(level.CrawlFaces.All(c=>!c.GetComponent<Renderer>().forceRenderingOff),Is.True);
        }
        [UnityTest] public IEnumerator SecondLessonRetainsLocalDestinationAcrossRotationAndClimbsToCeiling()
        {
            yield return Load(2);Capture("02-overview");
            Reach(1,new Vector3(-.25f,.08f,-.1f));
            Assert.That(Journey.Progress.Knows(VenomSkill.Climb),Is.True);
            level.Rotation.SetTargetOrientation(Quaternion.Euler(15,25,-12));Steps(240);
            Assert.That(Vector3.Distance(Local(level.Locomotion.Selected.Centre),new Vector3(-.225f,.08f,-.1f)),Is.LessThan(.05f));
            ExitAll();Capture("02-escaped");Steps(180);Capture("victory-rotated-ceiling-dance");
        }
        [UnityTest] public IEnumerator ThirdLessonRequiresRealSustainedContactAndDoesNotAutoSolveExit()
        {
            yield return Load(3);Capture("03-overview");
            Assert.That(Journey.Command(5,new Vector3(0,.25f,0)),Is.False);
            Assert.That(Journey.Command(0,Journey.PadALocal),Is.True);
            Until(()=>Journey.AActive,2000,"Reach A");
            Assert.That(Journey.Solved,Is.False);
            Assert.That(Journey.Command(0,new Vector3(-.15f,-.25f,-.15f)),Is.True);
            Steps(600);Assert.That(Journey.Solved,Is.False,"An abandoned hold must not complete later from an old animation callback.");
            Hold(level.Locomotion.Selected.Anchor,true);Until(()=>level.GateLatched,2600,"Sustained hold opens real cover");
            Steps(300);Assert.That(level.Organism.EscapedCount,Is.Zero);Assert.That(Journey.TaskFor(level.Locomotion.Selected.Anchor).Task,Is.EqualTo(VenomTask.HoldA));
            Assert.That(Journey.Progress.Knows(VenomSkill.Hold),Is.True);Capture("03-open");ExitAll();
            level.ResetExperiment();Steps(120);Assert.That(Journey.Solved,Is.False);Assert.That(Journey.TaskFor(0),Is.Null);
            Assert.That(Journey.Progress.Knows(VenomSkill.Hold),Is.True,"Retry resets apparatus, not learned skills.");
        }
        [UnityTest] public IEnumerator FourthLessonTwoIndependentOrdersHoldThroughRotationAndOpenTheExit()
        {
            yield return Load(4);Capture("04-overview");Cut(0);
            var pieces=level.Locomotion.Fragments.OrderBy(f=>Local(f.Centre).x).ToArray();int a=pieces[0].Anchor,b=pieces[1].Anchor;
            Assert.That(Journey.Select(b),Is.True);Assert.That(Journey.Command(0,Journey.PadBLocal),Is.False,"B is physically covered until A is held.");
            Hold(a,true);Until(()=>Journey.AActive,2600,"A holds B cover");Steps(300);
            Assert.That(Journey.TaskFor(a).Task,Is.EqualTo(VenomTask.HoldA));
            level.Rotation.SetTargetOrientation(Quaternion.Euler(12,-15,8));Steps(480);
            Assert.That(Journey.AActive,Is.True,"Rotation and selection must not release A.");
            Hold(b,false);Until(()=>level.GateLatched,3200,"B latches while another fragment holds A");
            Assert.That(Journey.Progress.Knows(VenomSkill.Cooperate),Is.True);Assert.That(level.Organism.FragmentCount,Is.EqualTo(2));
            Capture("04-cooperate");ExitAll();Capture("04-escaped");
            Steps(180);level.GetComponent<VenomHud>().FrameChamber(Screen.width,Screen.height);Capture("victory-cooperative-dance");
            foreach(var body in level.Organism.Bodies)
            {
                Vector3 p=level.View.WorldToViewportPoint(body.position);
                Assert.That(p.x,Is.InRange(.1f,.9f));Assert.That(p.y,Is.InRange(.15f,.85f));
            }
            Assert.That(level.Organism.TotalMass,Is.EqualTo(.096f).Within(.000001f));
        }
        [UnityTest] public IEnumerator FifthLessonEqualSplitFailsHeavyStationThenReunionAndUnequalSplitSolveIt()
        {
            yield return Load(5);Capture("05-overview");Cut(0);
            var pieces=level.Locomotion.Fragments.OrderBy(f=>Local(f.Centre).x).ToArray();
            Assert.That(pieces.Max(f=>f.Count),Is.LessThan(20),"The centred cut should make the heavy station lesson observable.");
            Hold(pieces[0].Anchor,true);Until(()=>Journey.AActive,2600,"A light station");Steps(240);
            Hold(pieces[1].Anchor,false);Until(()=>Journey.TaskFor(pieces[1].Anchor).Arrived,2600,"Equal half on heavy B");Steps(300);
            Assert.That(Journey.MassB,Is.LessThan(Journey.RequiredB));Assert.That(level.GateLatched,Is.False);Capture("05-too-light");
            Journey.CallTogether();Until(()=>level.Organism.FragmentCount==1,4000,"Reunite to change allocation");Steps(300);
            Cut(.018f);pieces=level.Locomotion.Fragments.OrderBy(f=>f.Count).ToArray();
            Assert.That(pieces[0].Count,Is.GreaterThanOrEqualTo(8));Assert.That(pieces[1].Count,Is.GreaterThanOrEqualTo(20));
            Hold(pieces[0].Anchor,true);Until(()=>Journey.AActive,2600,"Small part on A");Steps(240);
            Hold(pieces[1].Anchor,false);Until(()=>level.GateLatched,3200,"Large part on heavy B");
            Assert.That(Journey.Progress.Knows(VenomSkill.Allocate),Is.True);Capture("05-allocated");ExitAll();
        }
        [UnityTest] public IEnumerator GestureSelectionAndAcknowledgementNeverTurnRotationIntoACommand()
        {
            yield return Load(2);var input=level.GetComponent<VenomInput>();
            Vector2 p=level.View.WorldToScreenPoint(level.Rotation.transform.TransformPoint(new Vector3(-.13f,-.25f,.12f)));
            input.BeginTap(p);input.MoveTap(p+Vector2.right*90);input.FinishTap();Assert.That(Journey.CommandCount,Is.Zero);
            input.ApplyGuidanceTouch(1,p,Vector2.zero);input.ApplyGuidanceTouch(2,p,p+Vector2.right*40);
            input.ApplyGuidanceTouch(1,p,Vector2.zero);input.ApplyGuidanceTouch(0,Vector2.zero,Vector2.zero);Assert.That(Journey.CommandCount,Is.Zero);
            Assert.That(Journey.Command(0,new Vector3(-.13f,-.25f,.12f)),Is.True);
            Assert.That(Journey.Attention(level.Locomotion.Selected.Anchor,out _),Is.GreaterThan(0));
            level.Organism.GetComponent<VenomSurface>().Rebuild(false);
            level.TogglePause();Steps(120);Assert.That(Journey.Command(0,Vector3.zero),Is.False);level.TogglePause();
        }
        [UnityTest] public IEnumerator KnowledgeSurvivesSceneReloadButNeverReplaysAStaleTask()
        {
            VenomJourneyProgress.PersistenceKey="venom.journey.test";PlayerPrefs.DeleteKey(VenomJourneyProgress.PersistenceKey);
            VenomJourneyProgress.PersistenceEnabled=true;
            yield return Load(3);Hold(level.Locomotion.Selected.Anchor,true);Until(()=>level.GateLatched,3000,"Teach hold");
            Assert.That(Journey.Progress.Knows(VenomSkill.Hold),Is.True);
            yield return Load(4);Assert.That(Journey.Progress.Knows(VenomSkill.Hold),Is.True);
            Assert.That(Journey.TaskFor(0),Is.Null);Assert.That(Journey.Solved,Is.False);Assert.That(Journey.CommandCount,Is.Zero);
            yield return Load(3);Assert.That(Journey.Progress.Knows(VenomSkill.Hold),Is.True);
            Steps(600);Assert.That(Journey.AActive,Is.False);Assert.That(level.GateLatched,Is.False);Assert.That(Journey.TaskFor(0),Is.Null);
        }
        [UnityTest] public IEnumerator NativeMouseEventsKeepFastDragsDistinctFromTapsIncludingCursorRestoration()
        {
            yield return Load(2);level.GetComponent<VenomInput>().enabled=true;mouse=InputSystem.AddDevice<Mouse>();
            Vector2 start=new Vector2(Screen.width*.35f,Screen.height*.5f),end=start+Vector2.right*120;
            void Send(Vector2 p,bool down)=>InputSystem.QueueStateEvent(mouse,new MouseState{position=p}.WithButton(MouseButton.Left,down));
            Send(start,false);Send(start,true);Send(end,true);Send(end,false);Send(start,false);InputSystem.Update();
            yield return null;Steps(120);
            Assert.That(Journey.CommandCount,Is.Zero,"A drag completed between frames is never a destination tap.");
            Assert.That(Quaternion.Angle(level.Rotation.Orientation,Quaternion.identity),Is.GreaterThan(5));
            level.ResetExperiment();
            // Even returning to the initial point while still held remains a drag.
            Send(start,true);Send(end,true);Send(start,true);Send(start,false);InputSystem.Update();yield return null;
            Assert.That(Journey.CommandCount,Is.Zero);
            Vector2 target=level.View.WorldToScreenPoint(level.Rotation.transform.TransformPoint(new Vector3(-.14f,-.25f,.12f)));
            Send(target,false);Send(target,true);Send(target,false);InputSystem.Update();yield return null;
            Assert.That(Journey.CommandCount,Is.EqualTo(1),"A real tap remains responsive through the same event pipeline.");
        }
        [UnityTest] public IEnumerator ReleasingTheHolderClosesTheAccessCoverAndKeepsTheFinalGateLocked()
        {
            yield return Load(4);Cut(0);int anchor=level.Locomotion.Selected.Anchor;
            Hold(anchor,true);Until(()=>Journey.AActive,2400,"Hold A");Steps(220);
            Assert.That(Local(Journey.ButtonCover.position).y,Is.GreaterThan(-.15f));
            Assert.That(Journey.Command(0,new Vector3(-.14f,-.25f,-.19f)),Is.True);
            Until(()=>!Journey.AActive,1200,"Release A");Steps(220);
            Assert.That(Local(Journey.ButtonCover.position).y,Is.LessThan(-.22f));Assert.That(level.GateLatched,Is.False);
            Assert.That(Journey.Command(0,Journey.PadBLocal),Is.False);
        }
        [UnityTest] public IEnumerator TouchReleasePositionCompletesAFastSwipeInsteadOfIssuingADestination()
        {
            yield return Load(2);level.GetComponent<VenomInput>().enabled=true;touchscreen=InputSystem.AddDevice<Touchscreen>();
            Vector2 start=new Vector2(Screen.width*.35f,Screen.height*.5f),end=start+Vector2.right*120;
            InputSystem.QueueStateEvent(touchscreen,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=start});
            yield return null;yield return null;
            InputSystem.QueueStateEvent(touchscreen,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=end});
            yield return null;yield return null;Steps(120);
            Assert.That(Journey.CommandCount,Is.Zero);
            Assert.That(Quaternion.Angle(level.Rotation.Orientation,Quaternion.identity),Is.GreaterThan(5));
        }
        private void Capture(string name)
        {
            string dir=System.Environment.GetEnvironmentVariable("VENOM_JOURNEY_CAPTURE_DIR");
            if(string.IsNullOrEmpty(dir)||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            Directory.CreateDirectory(dir);level.GetComponent<VenomHud>().FrameChamber(720,1280);level.GetComponent<VenomHud>().RefreshSelection();level.Organism.GetComponent<VenomSurface>().Rebuild(false);
            var target=new RenderTexture(720,1280,24){antiAliasing=4};target.Create();var old=RenderTexture.active;
            var picture=new Texture2D(720,1280,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};RenderPipeline.SubmitRenderRequest(level.View,request);RenderPipeline.SubmitRenderRequest(level.View,request);
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,720,1280),0,0);picture.Apply();File.WriteAllBytes(Path.Combine(dir,name+".png"),picture.EncodeToPNG());
            }
            finally{RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }
    }
}
#endif
