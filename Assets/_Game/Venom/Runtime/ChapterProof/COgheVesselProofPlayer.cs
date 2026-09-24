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
    /// <summary>Opt-in developer replay; no execution or save changes in normal play.</summary>
    public sealed class COgheVesselProofPlayer : MonoBehaviour
    {
        [Serializable] private sealed class Result {public int level,escaped,fragments,drags;public bool passed;public float seconds;}
        [Serializable] private sealed class Report
        {
            public string startedUtc,endedUtc,unity,buildGuid,device,gpu;
            public string execution="Normal player FixedUpdate, InputSystem mouse drag events, public scene navigation, real framebuffer. Author replay, not a novice or phone playtest.";
            public int passed,failed;public List<Result> levels=new List<Result>();public List<string> errors=new List<string>();
        }
        static string requested;
        string directory;VenomCampaign game;Mouse mouse;Report report;Result current;
        readonly List<string> frames=new List<string>();readonly List<float> times=new List<float>();bool recording;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Prepare()
        {
            var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-vessel-proof-directory")requested=args[i+1];
            if(!string.IsNullOrEmpty(requested))VenomCampaignSave.PersistenceEnabled=false;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Begin()
        {if(string.IsNullOrEmpty(requested))return;var go=new GameObject("Opt-in vessel proof");DontDestroyOnLoad(go);go.AddComponent<COgheVesselProofPlayer>();}
        void OnEnable()=>Application.logMessageReceived+=Observe;
        void OnDestroy()=>Application.logMessageReceived-=Observe;
        void Observe(string message,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report?.errors.Add(message+"\n"+trace);}
        IEnumerator Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=60;
            directory=Path.Combine(Path.GetFullPath(requested),DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ"));Directory.CreateDirectory(directory);
            report=new Report{startedUtc=DateTime.UtcNow.ToString("O"),unity=Application.unityVersion,buildGuid=Application.buildGUID,device=SystemInfo.deviceModel,gpu=SystemInfo.graphicsDeviceName};
            mouse=Mouse.current??InputSystem.AddDevice<Mouse>();
            recording=Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-vessel-proof-video")>=0;
            if(recording){Directory.CreateDirectory(Path.Combine(directory,"video"));StartCoroutine(Video());}
            for(int slot=56;slot<=60;slot++)
            {
                current=new Result{level=slot};report.levels.Add(current);float started=Time.realtimeSinceStartup;
                yield return SceneManager.LoadSceneAsync($"COgheOrigin{slot:00}");yield return null;
                game=FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
                yield return new WaitForSeconds(1);yield return Capture("start");
                game.Owner.TogglePause();yield return new WaitForSecondsRealtime(.25f);game.Owner.TogglePause();
                game.ResetLevel();yield return new WaitForSeconds(.5f);
                var vertices=game.Surfaces[0].Curved.Interior.vertices;int rings=(vertices.Length-1)/64;
                var centres=new Vector3[rings];for(int r=0;r<rings;r++)for(int a=0;a<64;a++)centres[r]+=vertices[r*64+a]/64;
                int progress=0;float until=Time.realtimeSinceStartup+100;
                while(!game.Owner.Completed&&!game.Owner.Lost&&Time.realtimeSinceStartup<until)
                {
                    Vector3 direction=slot==59?Vector3.down:Vector3.up;
                    if(slot==60)
                    {
                        var p=game.Root.InverseTransformPoint(game.Motion.Centre(0));float best=float.PositiveInfinity;int nearest=0;
                        for(int i=0;i<rings;i++){float d=(p-centres[i]).sqrMagnitude;if(d<best){best=d;nearest=i;}}
                        progress=Math.Max(progress,nearest);
                        direction=progress>=rings-3?game.Root.InverseTransformDirection(game.Owner.Outlet.forward):(centres[Math.Min(rings-1,progress+4)]-p).normalized;
                    }
                    else if(Time.realtimeSinceStartup-started>10)direction=game.Root.InverseTransformDirection(game.Owner.Outlet.forward);
                    yield return DragToward(direction);
                }
                Queue(new Vector2(Screen.width*.5f,Screen.height*.5f),false);yield return null;
                current.escaped=game.Matter.EscapedCount;current.fragments=game.Matter.TotalFragmentCount;
                current.passed=game.Owner.Completed&&!game.Owner.Lost&&current.escaped==32&&current.fragments==1;current.seconds=Time.realtimeSinceStartup-started;
                if(current.passed)report.passed++;else report.failed++;
                yield return Capture(current.passed?"won":"failed");Save();
                Debug.Log("COGHE_VESSEL_PROOF_LEVEL "+slot+" passed="+current.passed+" escaped="+current.escaped);
                yield return new WaitForSecondsRealtime(.5f);
            }
            recording=false;report.endedUtc=DateTime.UtcNow.ToString("O");Save();WriteVideo();
            Debug.Log("COGHE_VESSEL_PROOF_DONE passed="+report.passed+" failed="+report.failed+" directory="+directory);
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-vessel-proof-quit")>=0)Application.Quit(report.failed==0&&report.errors.Count==0?0:1);
        }
        void Queue(Vector2 position,bool down)=>InputSystem.QueueStateEvent(mouse,new MouseState{position=position}.WithButton(MouseButton.Left,down));
        IEnumerator DragToward(Vector3 localDown)
        {
            Vector2 p=new Vector2(Screen.width*.5f,Screen.height*.52f);Queue(p,false);yield return null;Queue(p,true);yield return null;
            // Mouse motion is fed to the same threshold, smoothing and angular limits as human input.
            for(int frame=0;frame<18&&!game.Owner.Completed;frame++)
            {
                var world=game.Root.TransformDirection(localDown).normalized;var error=Quaternion.FromToRotation(world,Vector3.down);error.ToAngleAxis(out float angle,out var axis);
                if(angle>180){angle=360-angle;axis=-axis;}var angular=axis*Mathf.Min(angle,3);
                var cam=game.Owner.View;var delta=new Vector2(-Vector3.Dot(angular,cam.transform.up),Vector3.Dot(angular,cam.transform.right));
                if(delta.magnitude<.12f&&angle>5)delta=new Vector2(.6f,.4f);
                p+=delta/200*Mathf.Min(Screen.width,Screen.height);Queue(p,true);yield return null;
            }
            Queue(p,false);current.drags++;yield return null;
        }
        void Save()=>File.WriteAllText(Path.Combine(directory,"run.json"),JsonUtility.ToJson(report,true));
        IEnumerator Capture(string label)
        {
            yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();
            try{File.WriteAllBytes(Path.Combine(directory,$"{current.level}-{label}.png"),image.EncodeToPNG());}finally{Destroy(image);}
        }
        IEnumerator Video()
        {
            while(recording)
            {
                yield return new WaitForEndOfFrame();if(!recording)yield break;var image=ScreenCapture.CaptureScreenshotAsTexture();
                string path=$"video/frame-{frames.Count:00000}.png";
                try{File.WriteAllBytes(Path.Combine(directory,path),image.EncodeToPNG());frames.Add(path);times.Add(Time.realtimeSinceStartup);}finally{Destroy(image);}
                yield return new WaitForSecondsRealtime(.2f);
            }
        }
        void WriteVideo()
        {
            if(frames.Count==0)return;var lines=new List<string>{"ffconcat version 1.0"};
            for(int i=0;i<frames.Count;i++){lines.Add("file '"+frames[i]+"'");lines.Add("duration "+(i+1<times.Count?times[i+1]-times[i]:.2f).ToString("F6",System.Globalization.CultureInfo.InvariantCulture));}
            lines.Add("file '"+frames[frames.Count-1]+"'");File.WriteAllLines(Path.Combine(directory,"playthrough.ffconcat"),lines);
        }
    }
}
#endif
