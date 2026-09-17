using System.Diagnostics;
using UnityEngine;
#if COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
#endif

namespace GravityBox.Venom
{
    // Opt-in, release-IL2CPP instrumentation. Absent from ordinary player builds.
    public static class COgheMobileMetrics
    {
        public static readonly double[] Milliseconds=new double[4];
        private static readonly long[] Starts=new long[4];
        public static bool Enabled;
        [Conditional("COGHE_MOBILE_BENCHMARK"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Begin(int area) { if(Enabled)Starts[area]=Stopwatch.GetTimestamp(); }
        [Conditional("COGHE_MOBILE_BENCHMARK"), Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void End(int area) { if(Enabled)Milliseconds[area]+=(Stopwatch.GetTimestamp()-Starts[area])*1000.0/Stopwatch.Frequency; }
    }
#if COGHE_MOBILE_BENCHMARK
    public sealed class COgheMobileBenchmark : MonoBehaviour
    {
        [Serializable] private sealed class Sample
        {
            public string run,phase,device; public int level,frames,vertices,surfaces,graphBuilds;
            public float seconds,fps,p50Ms,p95Ms,p99Ms,maxMs,over33Percent,over50Percent;
            public double skinMs,gameStepMs,graphMs; public bool lost; public string failure;
        }
        private readonly List<float> times=new List<float>(4096);
        private string run; private string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfRequested()
        {
            string label=null;
#if UNITY_ANDROID && !UNITY_EDITOR
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
            using(var intent=activity.Call<AndroidJavaObject>("getIntent"))
                label=intent.Call<string>("getStringExtra","coghe_benchmark");
#endif
            if(string.IsNullOrEmpty(label))return;
            var go=new GameObject("COghe opt-in hardware benchmark");DontDestroyOnLoad(go);
            go.AddComponent<COgheMobileBenchmark>().run=label;
        }
        private IEnumerator Start()
        {
            COgheMobileMetrics.Enabled=true;
            output=Path.Combine(Application.persistentDataPath,"benchmark-"+run+".jsonl");
            File.WriteAllText(output,"");VenomCampaignSave.PersistenceEnabled=false;
            for(int number=11;number<=20;number++)
            {
                yield return SceneManager.LoadSceneAsync("VenomOrigin"+number.ToString("00"));
                var game=FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;
                yield return new WaitForSecondsRealtime(2);
                yield return Measure(game,"idle",5);
                game.Motion.Move(0,game.Root.TransformPoint(new Vector3(.12f,-.275f,-.08f)));
                yield return Measure(game,"move",8);
                if(game.Definition.CanRotate)yield return Measure(game,"rotate",10);
            }
            UnityEngine.Debug.Log("COGHE_BENCHMARK_DONE "+output);
            VenomCampaignSave.PersistenceEnabled=true;COgheMobileMetrics.Enabled=false;Destroy(gameObject);
        }
        private IEnumerator Measure(VenomCampaign game,string phase,float duration)
        {
            times.Clear();Array.Clear(COgheMobileMetrics.Milliseconds,0,3);
            double skin=0,step=0,graph=0;int builds=0;
            float start=Time.realtimeSinceStartup,nextTurn=start,elapsed=0;int turn=0;
            var surface=game.Matter.GetComponent<VenomSurface>();
            while((elapsed=Time.realtimeSinceStartup-start)<duration)
            {
                if(phase=="rotate"&&Time.realtimeSinceStartup>=nextTurn)
                {
                    game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(turn%2==0?80:-65,turn*35,0));
                    turn++;nextTurn+=2.5f;
                }
                yield return new WaitForEndOfFrame();
                times.Add(Time.unscaledDeltaTime*1000);
                skin+=COgheMobileMetrics.Milliseconds[0];step+=COgheMobileMetrics.Milliseconds[1];
                graph+=COgheMobileMetrics.Milliseconds[2];if(COgheMobileMetrics.Milliseconds[2]>0)builds++;
                Array.Clear(COgheMobileMetrics.Milliseconds,0,3);
            }
            times.Sort();int n=times.Count,over33=0,over50=0;
            foreach(float t in times){if(t>33.8f)over33++;if(t>50.5f)over50++;}
            var s=new Sample{run=run,phase=phase,device=SystemInfo.deviceModel,level=game.Definition.Order,
                frames=n,seconds=elapsed,fps=n/elapsed,p50Ms=times[n/2],p95Ms=times[Mathf.Min(n-1,(int)(n*.95f))],
                p99Ms=times[Mathf.Min(n-1,(int)(n*.99f))],maxMs=times[n-1],over33Percent=100f*over33/n,over50Percent=100f*over50/n,
                skinMs=skin/n,gameStepMs=step/n,graphMs=graph/n,graphBuilds=builds,vertices=surface.VertexCount,
                surfaces=game.Surfaces.Length,lost=game.Owner.Lost,failure=game.Failure};
            string json=JsonUtility.ToJson(s);File.AppendAllText(output,json+"\n");UnityEngine.Debug.Log("COGHE_BENCHMARK "+json);
        }
    }
#endif
}
