#if (DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK) && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;

namespace GravityBox.Venom.ChapterProof
{
    public sealed class COgheViewProofPlayer:MonoBehaviour
    {
        [Serializable] private sealed class Result
        {
            public int level,escaped,fragments,taps,frames,over33,over50,gcCollections;public bool passed;
            public double skinMs,gameStepMs,graphMs,routeMs,maxGraphMs;
            public float seconds,fps,p50Ms,p95Ms,p99Ms,maxMs;public long peakAllocatedBytes;
            public string error;public List<float> frameMs=new List<float>();
        }
        [Serializable] private sealed class Report
        {
            public string utc,unity,buildGuid,device,os,gpu,api,execution="Normal player loop; InputSystem touch events; screenshots excluded from frame samples. Author replay, not novice evidence.";
            public int width,height,passed,failed;public List<Result> levels=new List<Result>();public List<string> errors=new List<string>();
        }
        private static string requested;private static bool previousPersistence;
        private string directory;private VenomCampaign game;private Touchscreen touchscreen;
        private Report report;private Result result;private bool measuring,fast;private int shot;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Prepare()
        {
            previousPersistence=VenomCampaignSave.PersistenceEnabled;requested=null;var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-view-proof")requested=args[i+1];
#if UNITY_ANDROID
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
            using(var intent=activity.Call<AndroidJavaObject>("getIntent"))
                if(intent.Call<bool>("hasExtra","coghe_view_proof"))requested=Path.Combine(Application.persistentDataPath,"view-proof");
#endif
            if(!string.IsNullOrEmpty(requested))VenomCampaignSave.PersistenceEnabled=false;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Begin()
        {if(string.IsNullOrEmpty(requested))return;var go=new GameObject("Requested V2 author replay");DontDestroyOnLoad(go);go.AddComponent<COgheViewProofPlayer>();}
        private void OnEnable()=>Application.logMessageReceived+=Log;
        private void OnDestroy(){Application.logMessageReceived-=Log;if(touchscreen!=null)InputSystem.RemoveDevice(touchscreen);}
        private void Log(string message,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report?.errors.Add(message+"\n"+trace);}
        private void Update()
        {
            if(!measuring||result==null)return;
            result.frameMs.Add(Time.unscaledDeltaTime*1000);
            result.peakAllocatedBytes=Math.Max(result.peakAllocatedBytes,Profiler.GetTotalAllocatedMemoryLong());
            result.skinMs+=COgheMobileMetrics.Milliseconds[0];result.gameStepMs+=COgheMobileMetrics.Milliseconds[1];result.graphMs+=COgheMobileMetrics.Milliseconds[2];result.routeMs+=COgheMobileMetrics.Milliseconds[3];result.maxGraphMs=Math.Max(result.maxGraphMs,COgheMobileMetrics.Milliseconds[2]);Array.Clear(COgheMobileMetrics.Milliseconds,0,4);
        }
        private IEnumerator Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=60;
            fast=Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-view-fast")>=0;
            directory=Path.Combine(Path.GetFullPath(requested),DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));Directory.CreateDirectory(directory);
            report=new Report{utc=DateTime.UtcNow.ToString("O"),unity=Application.unityVersion,buildGuid=Application.buildGUID,device=SystemInfo.deviceModel,os=SystemInfo.operatingSystem,gpu=SystemInfo.graphicsDeviceName,api=SystemInfo.graphicsDeviceType.ToString(),width=Screen.width,height=Screen.height};
            if(fast)report.execution="Accelerated author diagnosis: normal touch events; 120Hz scripted simulation. Not realtime performance evidence.";
            touchscreen=InputSystem.AddDevice<Touchscreen>();
            for(int n=1;n<=10;n++)
            {
                result=new Result{level=n};report.levels.Add(result);
                yield return SceneManager.LoadSceneAsync($"COgheView{n:00}");yield return null;
                game=FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
                if(fast){game.Owner.enabled=false;game.Owner.Rotation.enabled=false;Physics.simulationMode=SimulationMode.Script;for(int t=0;t<120;t++)Tick();}
                yield return new WaitForSeconds(2);yield return Capture("start");
                float started=Time.realtimeSinceStartup;int gc=GC.CollectionCount(0);COgheMobileMetrics.Enabled=!fast;Array.Clear(COgheMobileMetrics.Milliseconds,0,4);measuring=!fast;
                yield return Guarded(new COgheViewScenario(game,Tap,Until,Orbit,Pinch).Solve());
                measuring=false;COgheMobileMetrics.Enabled=false;result.gcCollections=GC.CollectionCount(0)-gc;result.seconds=Time.realtimeSinceStartup-started;
                result.escaped=game.Matter.EscapedCount;result.fragments=game.Matter.TotalFragmentCount;
                result.passed=result.error==null&&game.Owner.Completed&&!game.Owner.Lost&&result.escaped==32&&result.fragments==1;

                var samples=new List<float>(result.frameMs);samples.Sort();result.frames=samples.Count;
                if(samples.Count>0)
                {float sum=0;foreach(float t in samples){sum+=t;if(t>33.333f)result.over33++;if(t>50)result.over50++;}result.skinMs/=samples.Count;result.gameStepMs/=samples.Count;result.graphMs/=samples.Count;result.routeMs/=samples.Count;result.fps=1000*samples.Count/sum;result.p50Ms=samples[samples.Count/2];result.p95Ms=samples[Mathf.Min(samples.Count-1,(int)(samples.Count*.95f))];result.p99Ms=samples[Mathf.Min(samples.Count-1,(int)(samples.Count*.99f))];result.maxMs=samples[samples.Count-1];}
                if(result.passed)yield return Guarded(Until(6,()=>game.Owner.Celebration.ReadyForNext,"Victory becomes ready"));
                result.passed&=result.error==null;if(result.passed)report.passed++;else report.failed++;
                yield return Capture(result.passed?"won":"failed");Save();
                Debug.Log($"COGHE_VIEW_LEVEL {n} passed={result.passed} escaped={result.escaped} error={result.error}");
            }
            Save();Debug.Log($"COGHE_VIEW_DONE passed={report.passed} failed={report.failed} directory={directory}");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-proof-quit")>=0)Application.Quit(report.failed==0&&report.errors.Count==0?0:1);
            else
            {
                VenomCampaignSave.PersistenceEnabled=previousPersistence;
                yield return SceneManager.LoadSceneAsync("COgheView01");
                Destroy(gameObject);
            }
        }
        private IEnumerator Guarded(IEnumerator scenario)
        {
            var stack=new Stack<IEnumerator>();stack.Push(scenario);
            while(stack.Count>0)
            {
                bool next=false;object value=null;
                try{next=stack.Peek().MoveNext();if(next)value=stack.Peek().Current;}
                catch(Exception e){result.error=e.ToString();break;}
                if(!next){stack.Pop();continue;}if(value is IEnumerator nested)stack.Push(nested);else yield return value;
            }
        }
        private void Contact(int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 point)
            =>InputSystem.QueueStateEvent(touchscreen,new TouchState{touchId=id,phase=phase,position=point});
        private IEnumerator Orbit(float degrees)
        {
            float before=game.CameraRig.OrbitYaw;Vector2 start=new Vector2(Screen.width*.88f,Screen.height*.5f);
            Contact(1,UnityEngine.InputSystem.TouchPhase.Began,start);yield return null;
            for(int step=1;step<=6;step++){Contact(1,UnityEngine.InputSystem.TouchPhase.Moved,start-Vector2.right*(Screen.width*degrees/240*step/6));yield return null;}
            Contact(1,UnityEngine.InputSystem.TouchPhase.Ended,start-Vector2.right*(Screen.width*degrees/240));yield return null;yield return null;
            if(Mathf.Abs(Mathf.DeltaAngle(before,game.CameraRig.OrbitYaw)-Mathf.DeltaAngle(0,degrees))>1)throw new InvalidOperationException("Real drag did not orbit by the requested angle");
            if(game.Motion.Get(0)!=null)throw new InvalidOperationException("Drag issued a movement command");
        }
        private IEnumerator Pinch()
        {
            Vector2 p=new Vector2(Screen.width*.4f,Screen.height*.5f),q=new Vector2(Screen.width*.6f,Screen.height*.5f);
            Contact(1,UnityEngine.InputSystem.TouchPhase.Began,p);yield return null;Contact(2,UnityEngine.InputSystem.TouchPhase.Began,q);yield return null;
            Contact(1,UnityEngine.InputSystem.TouchPhase.Moved,p-Vector2.right*Screen.width*.05f);Contact(2,UnityEngine.InputSystem.TouchPhase.Moved,q+Vector2.right*Screen.width*.05f);yield return null;
            Contact(2,UnityEngine.InputSystem.TouchPhase.Ended,q+Vector2.right*Screen.width*.05f);yield return null;
            Contact(1,UnityEngine.InputSystem.TouchPhase.Ended,p-Vector2.right*Screen.width*.05f);yield return null;yield return null;
            if(game.CameraRig.ZoomScale>=.95f||game.Motion.Get(0)!=null)throw new InvalidOperationException("Pinch failed or its release issued a command");
            bool before=measuring;measuring=false;yield return Capture("pinch");yield return null;yield return null;Array.Clear(COgheMobileMetrics.Milliseconds,0,4);measuring=before;
        }
        private IEnumerator Tap(Vector3 point)
        {
            game.CameraRig.Frame(Screen.width,Screen.height,0,true,Screen.safeArea);
            Vector2 screen=game.Owner.View.WorldToScreenPoint(point);
            if(!game.CameraRig.AllowsPointer(screen,Screen.width,Screen.height))throw new InvalidOperationException("Target under HUD: "+screen);
            bool before=measuring;measuring=false;yield return Capture("tap"+result.taps);yield return null;yield return null;Array.Clear(COgheMobileMetrics.Milliseconds,0,4);measuring=before;
            InputSystem.QueueStateEvent(touchscreen,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=screen});yield return null;
            InputSystem.QueueStateEvent(touchscreen,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=screen});yield return null;yield return null;result.taps++;
        }
        private IEnumerator Until(float seconds,Func<bool> done,string message)
        {
            if(fast)
            {
                for(int i=0;i<seconds*120&&!done()&&!game.Owner.Lost;i++){Tick();if(i%120==0)yield return null;}
                if(!done())throw new InvalidOperationException(message+State()+Nearby());
                yield break;
            }
            float until=Time.realtimeSinceStartup+seconds;
            while(!done()&&!game.Owner.Lost&&Time.realtimeSinceStartup<until)yield return null;
            if(!done())throw new InvalidOperationException(message+State()+Nearby());
        }
        private void Tick(){game.Owner.Step(1f/120);game.Owner.Rotation.Step(1f/120);Physics.Simulate(1f/120);}
        private string State()
        {string text="; "+game.Activity+"; "+game.Failure+"; centre="+game.Motion.Centre(0);foreach(var t in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())text+=$"; {t.Label} position={t.Rail.Position} phase={t.Phase} last={t.LastFailure} stand={t.StandPoint} hand={t.HandPoint}";Vector3 intent=Vector3.zero,velocity=Vector3.zero;int feet=0;var support=new Dictionary<string,int>();for(int i=0;i<32;i++){intent+=game.Motion.Intent(i)/32;velocity+=game.Matter.Bodies[i].linearVelocity/32;if(game.Motion.HasGrip(i))feet++;if(game.Motion.Support(i,out var collider,out _,out _)){string key=collider.name;support[key]=support.TryGetValue(key,out int count)?count+1:1;}}text+=$"; intent={intent:F4} velocity={velocity:F4} feet={feet} supports=";foreach(var pair in support)text+=pair.Key+":"+pair.Value+",";var order=game.Motion.Get(0);if(order!=null){text+=$"; cursor={order.Cursor} target={order.Target} path=";foreach(var p in order.Path)text+=p+",";}return text;}
        private string Nearby()
        {
            string text="\nNearby colliders: ";foreach(var shape in Physics.OverlapSphere(game.Motion.Centre(0),.11f))if(shape.GetComponent<VenomContact>()==null)text+=$"\n{shape.name} bounds={shape.bounds} active={shape.gameObject.activeInHierarchy} enabled={shape.enabled}";
            foreach(var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())text+=$"\nRail {rail.name} pos={rail.Position:F5} target={rail.Travel:F5} end={rail.AtEnd}";
            return text;
        }
        private IEnumerator Capture(string label)
        {
            yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();
            try{File.WriteAllBytes(Path.Combine(directory,$"{result.level:00}-{shot++:000}-{label}.png"),image.EncodeToPNG());}finally{Destroy(image);}
        }
        private void Save()=>File.WriteAllText(Path.Combine(directory,"run.json"),JsonUtility.ToJson(report,true));
    }
}
#endif
