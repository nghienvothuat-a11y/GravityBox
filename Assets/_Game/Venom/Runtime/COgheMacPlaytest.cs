#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GravityBox.Venom
{
    // Explicit development-only playtest. Real-time physics, unchanged quality,
    // ordinary movement commands. No body placement or puzzle-state overrides.
    public sealed partial class COgheMacPlaytest : MonoBehaviour
    {
        [Serializable] private struct Sample
        {public string phase;public float frameMs;public double skinMs,stepMs,graphMs,pathMs;public int groups;}
        [Serializable] private sealed class Summary
        {public string run,phase,device,cpu,gpu;public int frames;public float fps,p50,p95,p99,max,over33,over50;public double skinMs,stepMs,graphMs,pathMs;public int graphBuilds;}
        private readonly List<Sample> samples=new List<Sample>(24000);
        private VenomCampaign game;
        private string run,phase="warmup",output;
        private bool recording;
        private GUIStyle hudStyle;
        private bool wasBackground;
        private string hud="";
        private float nextHud;
        private static string Argument(string name)
        {var a=Environment.GetCommandLineArgs();for(int i=0;i<a.Length-1;i++)if(a[i]==name)return a[i+1];return null;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartRequested()
        {
#if !UNITY_EDITOR
            string label=Argument("-coghe-profile");if(string.IsNullOrEmpty(label))return;
            var go=new GameObject("COghe explicit Mac playtest");DontDestroyOnLoad(go);go.AddComponent<COgheMacPlaytest>().run=label;
#endif
        }
        private IEnumerator Start()
        {
            wasBackground=Application.runInBackground;Application.runInBackground=true;
            output=Path.Combine(Application.persistentDataPath,"mac-"+Path.GetFileName(run));
            VenomCampaignSave.PersistenceEnabled=false;
            yield return SceneManager.LoadSceneAsync("VenomOrigin20");game=FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
            yield return new WaitForSecondsRealtime(3);COgheMobileMetrics.Enabled=true;recording=true;
            StartCoroutine(SampleFrames());
            if(Argument("-coghe-profile-mode")=="demo"){yield return DemonstrateBoss();Finish();yield break;}
            if(Argument("-coghe-profile-mode")=="manual") {phase="manual";yield break;}
            phase="idle";yield return new WaitForSeconds(6);
            phase="commands";
            // Repeat an identical sequence on both builds; avoid the blade sensor.
            foreach(var p in new[]{new Vector3(-.53f,-.274f,.10f),new Vector3(-.31f,-.274f,.20f),new Vector3(-.53f,-.274f,.10f),new Vector3(-.31f,-.274f,.20f)})
            {game.Motion.Move(0,game.Root.TransformPoint(p),true);yield return new WaitForSeconds(3);}
            phase="gear-command";
            for(int i=0;i<4;i++)
            {game.SelectProp(game.Owner.Apparatus.GetComponentInChildren<COgheCooperativeWinch>().GearCarriage.GetComponent<VenomMovableProp>());yield return new WaitForSeconds(1);game.ReleaseProp();yield return new WaitForSeconds(1);}
            Finish();
        }
        private IEnumerator SampleFrames()
        {
            var end=new WaitForEndOfFrame();
            while(recording)
            {
                yield return end;
                if(!game.Owner.Paused)
                {
                    var m=COgheMobileMetrics.Milliseconds;
                    samples.Add(new Sample{phase=phase,frameMs=Time.unscaledDeltaTime*1000,skinMs=m[0],stepMs=m[1],graphMs=m[2],pathMs=m[3],groups=game.Matter.TotalFragmentCount});
                    if(Time.realtimeSinceStartup>=nextHud)
                    {hud=$"ĐO MAC · {phase} · {1f/Time.unscaledDeltaTime:F0} FPS · F8: lưu";nextHud=Time.realtimeSinceStartup+.5f;}
                }
                Array.Clear(COgheMobileMetrics.Milliseconds,0,4);
            }
        }
        private void Update()
        {if(recording&&Keyboard.current!=null&&Keyboard.current.f8Key.wasPressedThisFrame)Finish();}
        private void Finish()
        {
            if(!recording)return;recording=false;COgheMobileMetrics.Enabled=false;
            var csv=new System.Text.StringBuilder("phase,frame_ms,skin_ms,step_ms,graph_ms,path_ms,groups\n");
            var phases=new Dictionary<string,List<Sample>>();
            foreach(var s in samples)
            {
                csv.Append("\"").Append(s.phase.Replace("\"","\"\"")).Append("\",");
                csv.AppendLine(FormattableString.Invariant($"{s.frameMs:F4},{s.skinMs:F4},{s.stepMs:F4},{s.graphMs:F4},{s.pathMs:F4},{s.groups}"));
                if(!phases.TryGetValue(s.phase,out var list)){list=new List<Sample>();phases.Add(s.phase,list);}list.Add(s);
            }
            File.WriteAllText(output+".csv",csv.ToString());File.WriteAllText(output+".jsonl","");
            foreach(var pair in phases)
            {
                var rows=pair.Value;var times=new List<float>();float total=0,over33=0,over50=0;double skin=0,step=0,graph=0,path=0;int builds=0;
                foreach(var s in rows){times.Add(s.frameMs);total+=s.frameMs;if(s.frameMs>33.8f)over33++;if(s.frameMs>50.5f)over50++;skin+=s.skinMs;step+=s.stepMs;graph+=s.graphMs;path+=s.pathMs;if(s.graphMs>0)builds++;}
                times.Sort();int n=times.Count;
                var summary=new Summary{run=run,phase=pair.Key,device=SystemInfo.deviceModel,cpu=SystemInfo.processorType,gpu=SystemInfo.graphicsDeviceName,frames=n,fps=1000*n/total,p50=times[n/2],p95=times[Mathf.Min(n-1,(int)(n*.95f))],p99=times[Mathf.Min(n-1,(int)(n*.99f))],max=times[n-1],over33=100*over33/n,over50=100*over50/n,skinMs=skin/n,stepMs=step/n,graphMs=graph/n,pathMs=path/n,graphBuilds=builds};
                File.AppendAllText(output+".jsonl",JsonUtility.ToJson(summary)+"\n");
            }
            hud="ĐÃ LƯU SỐ ĐO MAC";Debug.Log("COGHE_MAC_PROFILE_DONE "+output);
            // Keep the entire explicit test session isolated from the save.
            // Restore persistence only when the playtest component is destroyed.
        }
        private void OnGUI()
        {
            if(string.IsNullOrEmpty(hud))return;
            float scale=Mathf.Min(Screen.width/540f,Screen.height/960f);
            if(hudStyle==null)hudStyle=new GUIStyle(GUI.skin.label){fontSize=12,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.15f,.3f,.32f)}};
            hudStyle.fontSize=Mathf.RoundToInt(12*scale);
            var rect=new Rect(0,Screen.height-28*scale,Screen.width,28*scale);
            var color=GUI.color;GUI.color=new Color(.97f,.96f,.89f);GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=color;
            GUI.Label(rect,hud,hudStyle);
        }
        private void OnDestroy(){Application.runInBackground=wasBackground;COgheMobileMetrics.Enabled=false;VenomCampaignSave.PersistenceEnabled=true;}
    }
}
#endif
