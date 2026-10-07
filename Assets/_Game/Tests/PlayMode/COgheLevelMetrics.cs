using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Level metrics (Explicit; Mrk, 06/10/2026: "vẽ biểu đồ độ khó hiện tại của 60 màn"): plays the reference solution of
 // every play position and writes one JSON line per level to Artifacts/Metrics/levels.jsonl: taps, taps a mechanism took,
 // part switches, simulated seconds to the exit, most parts at once, view turns and the mechanisms in the box.
 // COGHE_METRIC_LEVELS picks positions ("1,5,9"); default all.
 public sealed partial class COgheSpatialCampaignTests
 {
  private int metricTaps,metricParts;private float metricTurn;
  private IEnumerator MetricTap(Vector3 point){metricTaps++;yield return Tap(point);}
  private IEnumerator MetricUntil(float seconds,Func<bool> done,string reason)
  {
   for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++){Tick();metricParts=Mathf.Max(metricParts,game.Matter.TotalFragmentCount);if(i%240==0)yield return null;}
   if(!done())throw new InvalidOperationException(reason+$"; level={game.Definition.Order}");   // recorded, the run goes on
  }
  /// <summary>Runs nested coroutines here, so a level that fails is written down and the next level still runs.</summary>
  private static IEnumerator Flat(IEnumerator root,Action<string> fail)
  {
   var stack=new Stack<IEnumerator>();stack.Push(root);
   while(stack.Count>0)
   {
    var top=stack.Peek();bool more;
    try{more=top.MoveNext();}catch(Exception e){fail(e.Message.Replace('"','\'').Split('\n')[0]);yield break;}
    if(!more){stack.Pop();continue;}
    if(top.Current is IEnumerator inner)stack.Push(inner);else yield return top.Current;
   }
  }
  private IEnumerator MetricOrbit(float degrees){metricTurn+=Mathf.Abs(degrees);game.CameraRig.Orbit(-degrees*3,720);yield return null;}

  [UnityTest,Explicit,Timeout(7200000)] public IEnumerator MeasureLevelMetrics()
  {
   yield return Load(1);var order=game.Definition.SceneSequence;
   string only=Environment.GetEnvironmentVariable("COGHE_METRIC_LEVELS");
   var levels=string.IsNullOrEmpty(only)?Enumerable.Range(1,order.Length).ToArray():Array.ConvertAll(only.Split(','),int.Parse);
   Directory.CreateDirectory("Artifacts/Metrics");string path="Artifacts/Metrics/levels.jsonl";if(string.IsNullOrEmpty(only)&&File.Exists(path))File.Delete(path);
   foreach(int n in levels)
   {
    yield return LoadScene(order[n-1]);
    metricTaps=0;metricParts=1;metricTurn=0;
    float start=game.Matter.SimulationTime;string error="";
    yield return Flat(new COgheSpatialScenario(game,MetricTap,MetricUntil,MetricOrbit).Solve(),e=>error=e);
    float seconds=game.Matter.SimulationTime-start;
    var kinds=new SortedDictionary<string,int>();
    foreach(var m in game.Owner.Apparatus.GetComponentsInChildren<COgheMechanism>(true)){string k=m.GetType().Name;kinds[k]=kinds.TryGetValue(k,out int c)?c+1:1;}
    string mech=string.Join(",",kinds.Select(p=>$"\"{p.Key}\":{p.Value}"));
    string key=Path.GetFileNameWithoutExtension(order[n-1]).Replace("COgheSpatialPlus","").Replace("COgheSpatial","");
    File.AppendAllText(path,$"{{\"pos\":{n},\"key\":\"{key}\",\"title\":\"{game.Definition.Title.Replace('"','\'')}\",\"taps\":{metricTaps},\"mechanismTaps\":{game.MechanismTaps},\"partSelections\":{game.PartSelections},\"seconds\":{seconds:F1},\"maxParts\":{metricParts},\"turnDegrees\":{metricTurn:F0},\"solved\":{(game.Owner.Completed?"true":"false")},\"error\":\"{error}\",\"mechanisms\":{{{mech}}}}}\n");
   }
  }
 }
}
