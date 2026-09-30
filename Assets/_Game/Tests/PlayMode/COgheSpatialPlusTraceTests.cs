using System.Collections;
using System.IO;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Records what a rail body touches during the last physics step (diagnostics only).
 public sealed class PlusContactProbe:MonoBehaviour
 {
  public static Transform Frame;
  public readonly System.Collections.Generic.List<string> Now=new System.Collections.Generic.List<string>();
  void OnCollisionStay(Collision c)
  {
   var p=c.GetContact(0);
   Now.Add($"{c.collider.name}({(c.rigidbody!=null?c.rigidbody.name:"static")}) at {Frame.InverseTransformPoint(p.point):F3} n={p.normal:F2} sep={p.separation:F4} imp={c.impulse.magnitude:F5}");
  }
 }
 // Diagnostics for Spatial Plus: a solve with a running log (taps, waits, walk orders and paths, parts, stalled rails with
 // their contacts, parts that stopped with what is around them).
 public sealed partial class COgheSpatialCampaignTests
 {
  Vector3 L(Vector3 world)=>game.Root.InverseTransformPoint(world);
  string Parts(){string t="";var seen=new System.Collections.Generic.HashSet<int>();for(int i=0;i<32;i++)if(!game.Matter.Escaped[i]&&seen.Add(game.Matter.Groups[i])){int n=0;for(int j=0;j<32;j++)if(game.Matter.Groups[j]==game.Matter.Groups[i])n++;t+=$" p{i}×{n}@{L(game.Motion.Centre(i)):F3}";}return t;}
  // A solve with a running log: every tap, every wait, each new walk order with its path, and the parts twice a second.
  System.Action trace;string traceFile;
  void T(string line){File.AppendAllText(traceFile,line+"\n");}
  IEnumerator TracePlus(string key)
  {
   Directory.CreateDirectory("Artifacts/SpatialPlus");traceFile=$"Artifacts/SpatialPlus/trace-{key}.txt";File.WriteAllText(traceFile,"");
   yield return LoadPlus(key);int ticks=0;var seen=new System.Collections.Generic.HashSet<int>();
   var lastCentre=new System.Collections.Generic.Dictionary<int,Vector3>();
   PlusContactProbe.Frame=game.Root;var rails=game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>();var contacts=new PlusContactProbe[rails.Length];
   for(int i=0;i<rails.Length;i++)contacts[i]=rails[i].Body.gameObject.AddComponent<PlusContactProbe>();
   trace=()=>
   {
    ticks++;
    for(int i=0;i<32;i++){var o=game.Motion.Get(i);if(o!=null&&seen.Add(o.CommandId)){string p="";foreach(var q in o.Path)p+=q.ToString("F2");T($"  order p{i} #{o.CommandId} exit={o.Exit} target={o.Target:F3} path {p}");}}
    if(ticks%60==0)
    {
     T($"t={ticks*Dt:F1}{Parts()} act={game.Activity} fail={game.Failure}");
     // A part with a walk order that has not moved: name what is around it.
     var bodies=new System.Collections.Generic.HashSet<Rigidbody>(game.Matter.Bodies);
     for(int i=0;i<32;i++)
     {
      if(game.Matter.Escaped[i]||game.Motion.Get(i)==null||game.Motion.Get(i).Anchor!=i)continue;
      var here=game.Motion.Centre(i);bool still=lastCentre.TryGetValue(i,out var was)&&Vector3.Distance(was,here)<.003f;lastCentre[i]=here;
      if(!still)continue;var near=new System.Collections.Generic.List<string>();
      foreach(var col in Physics.OverlapSphere(here,.045f))if(col.attachedRigidbody==null||!bodies.Contains(col.attachedRigidbody))near.Add(col.name+"@"+L(col.ClosestPoint(here)).ToString("F3"));
      T($"  still p{i} at {L(here):F3} near: {string.Join(", ",near)}");
     }
     // A rail pushed but not moving: name what it is pressing against.
     for(int i=0;i<rails.Length;i++)if(Mathf.Abs(rails[i].Effort)>.02f&&rails[i].Body.linearVelocity.magnitude<.002f&&!rails[i].Locked&&!rails[i].Latched&&!(rails[i].Effort<0&&rails[i].Position<=rails[i].CatchTolerance)&&!(rails[i].Effort>0&&rails[i].AtEnd))
      T($"  stalled {rails[i].name} pos={rails[i].Position:F4} effort={rails[i].Effort:F3} contacts: {string.Join(" | ",contacts[i].Now)}");
    }
    foreach(var c in contacts)c.Now.Clear();
   };
   System.Func<Vector3,IEnumerator> tap=p=>{T($"TAP {L(p):F3} selected={game.Motion.Selected}");return Tap(p);};
   System.Func<float,System.Func<bool>,string,IEnumerator> until=(sec,c,r)=>{T($"UNTIL {r}");return Until(sec,c,r);};
   try{yield return new COgheSpatialScenario(game,tap,until).Solve();T("SOLVED");}
   finally{trace=null;}
  }

  // Run by name with COGHE_PLUS_TRACE=<key> (e.g. E07) to write Artifacts/SpatialPlus/trace-<key>.txt; skipped otherwise.
  [UnityTest,Explicit("diagnostic")] public IEnumerator TracePlusLevel()
  {
   string key=System.Environment.GetEnvironmentVariable("COGHE_PLUS_TRACE");
   if(string.IsNullOrEmpty(key))Assert.Ignore("Set COGHE_PLUS_TRACE to a Spatial Plus key (E01…E18, B1, B2).");
   yield return TracePlus(key);
  }
 }
}
