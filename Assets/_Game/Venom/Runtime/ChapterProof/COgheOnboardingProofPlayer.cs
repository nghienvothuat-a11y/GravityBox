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
    /// <summary>Opt-in real-time replay. No commands are issued in ordinary player sessions.</summary>
    public sealed class COgheOnboardingProofPlayer : MonoBehaviour
    {
        [Serializable] private sealed class TapObservation {public Vector3 intended,accepted,propPosition;public string stage;public bool attached,hasPropTarget;}
        [Serializable] private sealed class Result {public List<TapObservation> inputs=new List<TapObservation>();public int level,escaped,fragments,taps;public bool passed;public float seconds;public string error;}
        [Serializable] private sealed class Report
        {
            public string startedUtc,endedUtc,unity,buildGuid,device,gpu;
            public string execution="Normal FixedUpdate; InputSystem mouse taps; public navigation/release commands; framebuffer including HUD. Native IMGUI buttons and novice/phone understanding require separate playtests.";
            public int width,height,passed,failed;public List<Result> levels=new List<Result>();public List<string> errors=new List<string>();
        }
        private static string requested;
        private string directory,lastStage;
        private VenomCampaign game;
        private Mouse mouse;
        private Report report;
        private Result current;
        private int captureIndex;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Prepare()
        {
            var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-onboarding-proof")requested=args[i+1];
            if(!string.IsNullOrEmpty(requested))VenomCampaignSave.PersistenceEnabled=false;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Begin()
        {if(string.IsNullOrEmpty(requested))return;var go=new GameObject("Explicit onboarding replay");DontDestroyOnLoad(go);go.AddComponent<COgheOnboardingProofPlayer>();}
        private void OnEnable()=>Application.logMessageReceived+=Observe;
        private void OnDestroy()=>Application.logMessageReceived-=Observe;
        private void Observe(string message,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report?.errors.Add(message+"\n"+trace);}
        private IEnumerator Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=60;
            directory=Path.Combine(Path.GetFullPath(requested),DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ"));Directory.CreateDirectory(directory);
            report=new Report{startedUtc=DateTime.UtcNow.ToString("O"),unity=Application.unityVersion,buildGuid=Application.buildGUID,device=SystemInfo.deviceModel,gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height};
            mouse=Mouse.current??InputSystem.AddDevice<Mouse>();
            for(int slot=1;slot<=10;slot++)
            {
                current=new Result{level=slot};report.levels.Add(current);lastStage=null;
                yield return SceneManager.LoadSceneAsync($"COgheLearn{slot:00}");yield return null;
                game=FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
                yield return new WaitForSeconds(1);yield return Capture("start");
                float started=Time.realtimeSinceStartup;
                yield return RunGuarded(new COgheOnboardingScenario(game,Tap,Until,Delay,Release).Solve());
                current.escaped=game.Matter.EscapedCount;current.fragments=game.Matter.TotalFragmentCount;
                current.passed=current.error==null&&game.Owner.Completed&&!game.Owner.Lost&&current.escaped==32&&current.fragments==1;
                current.seconds=Time.realtimeSinceStartup-started;
                if(current.passed)report.passed++;else report.failed++;
                yield return Capture(current.passed?"won":"failed");Save();
                Debug.Log($"COGHE_ONBOARDING_LEVEL {slot} passed={current.passed} escaped={current.escaped} error={current.error}");
            }
            report.endedUtc=DateTime.UtcNow.ToString("O");Save();
            Debug.Log($"COGHE_ONBOARDING_DONE passed={report.passed} failed={report.failed} directory={directory}");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-proof-quit")>=0)Application.Quit(report.failed==0&&report.errors.Count==0?0:1);
        }
        private IEnumerator RunGuarded(IEnumerator scenario)
        {
            var stack=new Stack<IEnumerator>();stack.Push(scenario);
            while(stack.Count>0)
            {
                bool next=false;object value=null;
                try{next=stack.Peek().MoveNext();if(next)value=stack.Peek().Current;}
                catch(Exception error){current.error=error.ToString();break;}
                if(!next){stack.Pop();continue;}
                if(value is IEnumerator nested)stack.Push(nested);else yield return value;
            }
        }
        private IEnumerator Tap(Vector3 point)
        {
            game.CameraRig.Frame(Screen.width,Screen.height,0,true,Screen.safeArea);
            var p=(Vector2)game.Owner.View.WorldToScreenPoint(point);
            if(!game.CameraRig.AllowsPointer(p,Screen.width,Screen.height))throw new InvalidOperationException("Target lies under HUD: "+p);
            if(lastStage!=game.Onboarding.Stage){lastStage=game.Onboarding.Stage;yield return Capture(lastStage);}
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p}.WithButton(MouseButton.Left));yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;
            current.taps++;
            current.inputs.Add(new TapObservation{intended=game.Root.InverseTransformPoint(point),
                accepted=game.Root.InverseTransformPoint(game.Feedback.CommandPoint),
                propPosition=game.Props.Length>0?game.Root.InverseTransformPoint(game.Props[0].Body.position):Vector3.zero,
                stage=game.Onboarding.Stage,attached=game.Attached,hasPropTarget=game.HasPropTarget});
        }
        private IEnumerator Until(float seconds,Func<bool> done,string message)
        {
            float deadline=Time.realtimeSinceStartup+seconds;
            while(!done()&&!game.Owner.Lost&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!done())throw new InvalidOperationException(message+"; "+game.Activity+"; "+game.Failure);
        }
        private IEnumerator Delay(float seconds){yield return new WaitForSeconds(seconds);}
        private IEnumerator Release(){yield return Capture("release");game.ReleaseProp();yield return null;}
        private IEnumerator Capture(string label)
        {
            yield return new WaitForEndOfFrame();var picture=ScreenCapture.CaptureScreenshotAsTexture();
            try{File.WriteAllBytes(Path.Combine(directory,$"{current.level:00}-{captureIndex++:000}-{label}.png"),picture.EncodeToPNG());}
            finally{Destroy(picture);}
        }
        private void Save()=>File.WriteAllText(Path.Combine(directory,"run.json"),JsonUtility.ToJson(report,true));
    }
}
#endif
