#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace GravityBox.Venom.ChapterProof
{
    /// <summary>Explicit opt-in evidence: normal player physics, queued mouse press/release, real framebuffer.</summary>
    public sealed class COgheTapProofPlayer : MonoBehaviour
    {
        [Serializable] private sealed class LevelResult
        { public int level; public bool passed; public string error; public int taps, escaped, fragments; public bool homeUnlocked; }
        [Serializable] private sealed class Report
        {
            public string startedUtc, endedUtc, unity, buildGuid, device, gpu;
            public string execution="Normal player FixedUpdate; InputSystem gameplay mouse press/release; public navigation/release commands; framebuffer including HUD; isolated save. Native HUD is outside this automated replay.";
            public int passed, failed;
            public List<LevelResult> levels=new List<LevelResult>();
            public List<string> errors=new List<string>();
        }
        private static string requested;
        private string directory;
        private VenomCampaign game;
        private Report report;
        private LevelResult current;
        private Mouse mouse;
        private bool recording;
        private bool integrated;
        private readonly List<string> videoFrames = new List<string>();
        private readonly List<float> videoTimes = new List<float>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Prepare()
        {
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-tap-proof-directory")requested=args[i+1];
            if(!string.IsNullOrEmpty(requested))VenomCampaignSave.PersistenceEnabled=false;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Begin()
        {
            if(string.IsNullOrEmpty(requested))return;
            var go=new GameObject("COghe opt-in tap playthrough evidence");DontDestroyOnLoad(go);go.AddComponent<COgheTapProofPlayer>();
        }
        private void OnEnable()=>Application.logMessageReceived+=Observe;
        private void OnDestroy()=>Application.logMessageReceived-=Observe;
        private void Observe(string message,string trace,LogType type)
        {
            if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
            report?.errors.Add(type+": "+message+"\n"+trace);
            if(current!=null&&current.error==null)current.error=message;
        }
        private IEnumerator Start()
        {
            Application.runInBackground=true;
            directory=Path.Combine(Path.GetFullPath(requested),DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ"));Directory.CreateDirectory(directory);
            report=new Report{startedUtc=DateTime.UtcNow.ToString("O"),unity=Application.unityVersion,buildGuid=Application.buildGUID,device=SystemInfo.deviceModel,gpu=SystemInfo.graphicsDeviceName};
            mouse=Mouse.current??InputSystem.AddDevice<Mouse>();
            integrated=Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-campaign55-proof")>=0;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-tap-proof-video")>=0)
            {recording=true;Directory.CreateDirectory(Path.Combine(directory,"video"));StartCoroutine(RecordVideo());}
            for(int level=integrated?41:1;level<=(integrated?55:10);level++)
            {
                current=new LevelResult{level=level};report.levels.Add(current);
                yield return Execute(Play(level));
                current.passed=current.error==null&&game!=null&&game.Owner.Completed&&!game.Owner.Lost&&game.Matter.EscapedCount==32;
                if(game!=null){current.escaped=game.Matter.EscapedCount;current.fragments=game.Matter.TotalFragmentCount;current.homeUnlocked=game.Progress.HomeUnlocked;}
                if(current.passed)report.passed++;else{report.failed++;yield return Capture("failed");}
                Save();Debug.Log("COGHE_TAP_PROOF_LEVEL "+level+" passed="+current.passed+" error="+current.error);
            }
            recording=false;WriteVideoIndex();report.endedUtc=DateTime.UtcNow.ToString("O");Save();
            Debug.Log("COGHE_TAP_PROOF_DONE passed="+report.passed+" failed="+report.failed+" directory="+directory);
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-tap-proof-quit")>=0)Application.Quit(report.failed==0?0:1);
        }
        private void Save()=>File.WriteAllText(Path.Combine(directory,"run.json"),JsonUtility.ToJson(report,true));
        private IEnumerator RecordVideo()
        {
            while(recording)
            {
                yield return new WaitForEndOfFrame();
                if(!recording)yield break;
                var frame=ScreenCapture.CaptureScreenshotAsTexture();
                string name=$"video/frame-{videoFrames.Count:00000}.png";
                try{File.WriteAllBytes(Path.Combine(directory,name),frame.EncodeToPNG());videoFrames.Add(name);videoTimes.Add(Time.realtimeSinceStartup);}
                finally{Destroy(frame);}
                yield return new WaitForSecondsRealtime(.20f);
            }
        }
        private void WriteVideoIndex()
        {
            if(videoFrames.Count==0)return;
            var lines=new List<string>{"ffconcat version 1.0"};
            for(int i=0;i<videoFrames.Count;i++)
            {
                lines.Add("file '"+videoFrames[i]+"'");
                float duration=i+1<videoTimes.Count?videoTimes[i+1]-videoTimes[i]:.20f;
                lines.Add("duration "+duration.ToString("F6",System.Globalization.CultureInfo.InvariantCulture));
            }
            lines.Add("file '"+videoFrames[videoFrames.Count-1]+"'");
            File.WriteAllLines(Path.Combine(directory,"playthrough.ffconcat"),lines);
        }
        private IEnumerator Execute(IEnumerator action)
        {
            var stack=new Stack<IEnumerator>();stack.Push(action);
            while(stack.Count>0)
            {
                bool more=false;object value=null;Exception error=null;
                try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;}
                catch(Exception caught){error=caught;}
                if(error!=null)
                {
                    current.error=error.ToString();
                    while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();
                    yield break;
                }
                if(!more){(stack.Pop() as IDisposable)?.Dispose();continue;}
                if(value is IEnumerator nested){stack.Push(nested);continue;}
                yield return value;
            }
        }
        private string State()
        {
            if(game==null)return "no game";
            string state=$"{game.Definition.Id}, selected={game.Motion.Selected}, centre={game.Motion.Centre(game.Motion.Selected):F3}, fragments={game.Matter.TotalFragmentCount}, loss={game.Failure}";
            foreach(var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())state+=$"; {rail.Label}:{rail.Phase} pos={rail.Rail.Position:F4} fail={rail.LastFailure}";
            return state;
        }
        private IEnumerator Wait(Func<bool> done,float seconds,string reason)
        {
            float until=Time.realtimeSinceStartup+seconds;
            while(!done()&&Time.realtimeSinceStartup<until&&!game.Owner.Lost)yield return null;
            if(!done())throw new InvalidOperationException(reason+"; "+State());
        }
        private IEnumerator Click(Vector3 world)
        {
            yield return null;
            Vector2 p=game.Owner.View.WorldToScreenPoint(world);
            if(!game.CameraRig.AllowsPointer(p,Screen.width,Screen.height))throw new InvalidOperationException("Control is outside play area: "+p);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p}.WithButton(MouseButton.Left));yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;
            current.taps++;
        }
        private IEnumerator Select(int actor)
        {
            if(game.Matter.Groups[actor]==game.Matter.Groups[game.Motion.Selected])yield break;
            yield return Click(game.Motion.Centre(actor));
            if(game.Matter.Groups[actor]!=game.Matter.Groups[game.Motion.Selected])throw new InvalidOperationException("Fragment click did not select intended body");
        }
        private COgheTapRail Rail(string label)=>Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),x=>x.Label==label);
        private COgheGearTrain Train=>FindFirstObjectByType<COgheGearTrain>();
        private IEnumerator Operate(string label)
        {
            var rail=Rail(label);int before=rail.CompletedJourneys;
            yield return Click(rail.HandPoint);
            yield return Wait(()=>rail.CompletedJourneys==before+1&&!rail.Busy,25,"Single mouse click completes "+label);
        }
        private int Extreme(bool left)
        {
            int found=0;float best=left?float.PositiveInfinity:float.NegativeInfinity;
            for(int i=0;i<32;i++){float x=game.Motion.Centre(i).x;if(left?x<best:x>best){best=x;found=i;}}
            return found;
        }
        private IEnumerator Play(int slot)
        {
            VenomCampaignSave.PersistenceEnabled=false;
            int level=integrated?slot-40:slot;
            if(integrated) yield return ChooseIntegratedSlot(slot);
            else { yield return SceneManager.LoadSceneAsync($"COgheTap{level:00}");yield return null; }
            game=FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
            if(game.Owner.Paused)game.Owner.TogglePause();
            yield return new WaitForSeconds(.5f);yield return Capture("start");
            if(integrated&&slot>=51)
            {
                yield return AssembleBridge();
            }
            else if(level<=3)
            {
                yield return Operate("A");
                if(level>1)yield return Operate("B");
                if(level==2)yield return Operate("A");
                yield return Wait(()=>Train.Rack.AtEnd,8,"Connected gears open the physical door");
                if(level==1)
                {
                    yield return Capture("open");
                    var returning=Rail("A");int journeys=returning.CompletedJourneys;yield return Click(returning.HandPoint);
                    yield return Wait(()=>!Train.Meshed&&Train.Rack.Position<.13f,12,"Physical door starts returning");
                    yield return Capture("returning");
                    yield return Wait(()=>returning.CompletedJourneys==journeys+1&&!returning.Busy,20,"Reverse journey stops at its catch");
                    yield return Wait(()=>Train.Rack.Position<.006f,8,"Return closes door");yield return Capture("closed-again");
                    yield return Operate("A");yield return Wait(()=>Train.Rack.AtEnd,8,"Reopen");
                }
            }
            else if(level==7)
            {
                yield return Operate("A");yield return Operate("B");
                yield return Operate("A");yield return Capture("third-stop");yield return Operate("A");
                yield return Wait(()=>Train.Rack.AtEnd,8,"Four-stop transmission opens");
            }
            else if(level==9)
            {
                yield return Operate("A");yield return Operate("B");yield return Operate("A");
                yield return Wait(()=>Train.Rack.AtEnd,8,"Returning gear reconnects");
            }
            else
            {
                var knife=FindFirstObjectByType<COgheGuillotine>();yield return Click(knife.Rail.Body.position);
                yield return Wait(()=>game.Matter.TotalFragmentCount>1,18,"Actual blade divides the body");
                int left=Extreme(true),right=Extreme(false);
                yield return Select(right);yield return Click(game.Root.TransformPoint(new Vector3(.16f,-.30f,-.18f)));
                yield return Select(left);
                if(level>=5)
                {
                    var pad=FindFirstObjectByType<COgheTapPad>();yield return Click(pad.Sensor.transform.position);
                    yield return Wait(()=>pad.Sensor.Active,18,"Left part holds A");
                    yield return Select(right);
                    if(level==8||level==10)
                    {
                        yield return Operate("A");yield return Capture("second-stop");yield return Operate("B");
                        if(level==10)yield return Operate("C");
                        yield return Operate("A");yield return Operate("A");
                    }
                    else {yield return Operate("B");if(level==6)yield return Operate("C");}
                    yield return Wait(()=>Train.Rack.AtEnd,8,"Cooperation catches door");
                    yield return Capture("two-parts-open");
                    yield return Select(left);yield return Click(pad.Sensor.transform.position);
                    yield return Wait(()=>!pad.Sensor.Active,12,"Step off A");
                }
                else yield return Click(game.Root.TransformPoint(new Vector3(-.28f,-.30f,.05f)));
                var groups=new HashSet<int>();var actors=new List<int>();
                for(int i=0;i<32;i++)if(groups.Add(game.Matter.Groups[i]))actors.Add(i);
                foreach(int actor in actors){yield return Select(actor);yield return Click(game.Root.TransformPoint(new Vector3(.22f,-.30f,-.15f)));}
                yield return Wait(()=>game.Matter.TotalFragmentCount==1,25,"All fragments physically reunite");
            }
            yield return Capture("before-exit");yield return Click(game.Owner.Outlet.position);
            yield return Wait(()=>game.Owner.Completed,30,"Merged creature exits");
            yield return new WaitForSeconds(1.5f);yield return Capture("won");
        }
        private IEnumerator ChooseIntegratedSlot(int slot)
        {
            if(game==null)
            {
                yield return SceneManager.LoadSceneAsync("COgheOrigin40");yield return null;
                game=FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
            }
            // IMGUI uses native window events, not InputSystem synthetic mouse states.
            // Exercise its shared scene command here; validate HUD clicks separately on the native app.
            game.Load(slot);yield return null;
            float until=Time.realtimeSinceStartup+10;
            while(SceneManager.GetActiveScene().name!=$"COgheOrigin{slot:00}"&&Time.realtimeSinceStartup<until)yield return null;
            if(SceneManager.GetActiveScene().name!=$"COgheOrigin{slot:00}")throw new InvalidOperationException("Campaign navigation failed to load "+slot);
            yield return null;
        }
        private IEnumerator AssembleBridge()
        {
            var bridge=FindFirstObjectByType<COgheAssemblyBridge>();
            for(int i=0;i<bridge.Rails.Length;i++)
            {
                var rail=bridge.Rails[i];var prop=rail.GetComponent<VenomMovableProp>();
                yield return Click(prop.ManipulationGrip.position);
                yield return Wait(()=>game.Attached,25,"Approach bridge handle "+i);
                var target=rail.Start+rail.Axis*(rail.Travel+.05f);target.y=-.10f;
                float until=Time.realtimeSinceStartup+25,refresh=0;
                while(!(rail.AtEnd&&rail.Latched)&&Time.realtimeSinceStartup<until)
                {
                    if(Time.realtimeSinceStartup>=refresh)
                    {yield return Click(game.Root.TransformPoint(target));refresh=Time.realtimeSinceStartup+1;}
                    yield return null;
                }
                if(!rail.AtEnd||!rail.Latched)throw new InvalidOperationException("Bridge failed to seat "+i+" pos="+rail.Position+"; "+State());
                game.ReleaseProp();yield return null;
                if(game.Attached)throw new InvalidOperationException("Release command did not let go");
            }
            yield return Capture("assembled");
            foreach(var p in new[]{new Vector3(-.255f,-.30f,-.24f),new Vector3(-.255f,-.19f,-.18f),
                new Vector3(-.255f,-.10f,-.12f),new Vector3(-.255f,-.10f,0)})
            {
                yield return Click(game.Root.TransformPoint(p));
                yield return Wait(()=>Vector3.Distance(game.Motion.Centre(0),game.Root.TransformPoint(p))<.045f,25,"Walk to bank "+p);
            }
        }
        private IEnumerator Capture(string stage)
        {
            if(game==null)yield break;
            yield return null;yield return new WaitForEndOfFrame();
            var frame=ScreenCapture.CaptureScreenshotAsTexture();
            try{File.WriteAllBytes(Path.Combine(directory,$"{current.level:00}-{stage}.png"),frame.EncodeToPNG());}
            finally{Destroy(frame);}
        }
    }
}
#endif
