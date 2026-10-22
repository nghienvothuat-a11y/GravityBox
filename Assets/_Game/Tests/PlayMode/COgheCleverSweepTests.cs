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
 public sealed partial class COgheSpatialCampaignTests
 {
  // Clever at Lv 10 across the catalog (Explicit): each level's reference solve runs, but before the first scripted tap
  // after a step is done the player pauses, and Clever must walk COghe to the next step of the lesson (beside a handle,
  // beside a pad, up to a device), never working it itself. COGHE_CLEVER_LEVELS (positions, default all).
  // Report: Artifacts/Clever/sweep.txt, one line per level and one per guided step.
  [UnityTest,Explicit,Timeout(10800000)] public IEnumerator CleverLeadsEveryLevelStepByStep()
  {
   COgheIntelligence.ForceForTests=true;COgheIntelligence.LevelForTests=COgheAbilities.Max;
   var failures=new List<string>();
   try
   {
    yield return Load(1);var order=game.Definition.SceneSequence;
    string only=Environment.GetEnvironmentVariable("COGHE_CLEVER_LEVELS");
    var levels=string.IsNullOrEmpty(only)?Enumerable.Range(1,order.Length).ToList():only.Split(',').Select(int.Parse).ToList();
    Directory.CreateDirectory("Artifacts/Clever");string log="Artifacts/Clever/sweep.txt";File.WriteAllText(log,"");
    foreach(int n in levels)
    {
     yield return LoadScene(order[n-1]);
     var intel=game.Intelligence;var lines=new List<string>();var problems=new List<string>();
     var recorded=COgheSolutionSteps.For(game.Definition.Id)??new List<COgheSolutionSteps.Step>();
     var targets=recorded.Select(s=>COgheSolutionSteps.Resolve(game.Owner.Apparatus,s)).Where(t=>t!=null).ToList();
     if(intel==null||intel.StepCount==0||intel.StepCount!=targets.Count)problems.Add($"lesson: {intel?.StepCount??-1} steps, {targets.Count} resolved, {recorded.Count} recorded");
     int probed=-1;
     IEnumerator Probe()
     {
      int step=intel.Progress;probed=step;var target=targets[step];
      float start=game.Matter.SimulationTime;
      var rails=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();var journeys=rails.Select(r=>r.CompletedJourneys).ToArray();
      // the player's hands are off: past the pause Clever looks for a second, then walks
      bool Arrived()=>game.Matter.SimulationTime-game.LastInputTime>COgheIntelligence.IdleDelay+COgheIntelligence.LookTime+.2f&&
       intel.GuidedStep==step&&!intel.Looking&&game.Motion.Get(game.Motion.Selected)==null;
      bool tracing=Environment.GetEnvironmentVariable("COGHE_CLEVER_TRACE")=="1";
      for(int i=0;i<45/Dt&&!Arrived()&&!game.Owner.Lost;i++)
      {
       Tick();if(i%240==0)yield return null;
       if(tracing&&i%360==0)
       {
        var o=game.Motion.Get(game.Motion.Selected);
        lines.Add($"       probe {i*Dt:F0}s at {game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected)):F2} guided {intel.GuidedStep+1} look {intel.Looking} activity {game.Activity} order {(o==null?"none":$"{o.Cursor}/{o.Path.Count} to {game.Root.InverseTransformPoint(o.Target):F2}")} tube {game.InTube} attached {game.Attached}");
       }
      }
      Vector3 at=game.Motion.Centre(game.Motion.Selected);
      string kind=target is COgheTapRail?"rail":target is COgheTissueSensor?"pad":"device";
      // a crate is worked from the nearest face with room (where a tap with no face chosen sends COghe)
      Vector3 aim=target is COgheTapRail r?(r.CrateFaces?r.GuideStandPoint(game.Motion.Selected):r.StandPoint):target.transform.position;
      // a tube's mouth, a swing's ring stand (where Clever leads; the device origin can be mid-air)
      if(!(target is COgheTapRail)&&!(target is COgheTissueSensor)&&target is COgheMechanism device&&device.GuidePoint(game,game.Motion.Selected,out Vector3 guide))aim=guide;
      float miss=Vector3.Distance(at,aim);
      // beside a pad stands ~9 cm off its centre; a device's own centre cannot be stood on
      float allowed=kind=="rail"?.08f:kind=="pad"?.16f:.20f;
      // why Clever held back: an order still running (held?), a busy rail, COghe's own act
      var order=game.Motion.Get(game.Motion.Selected);
      string why=$" [order {(order==null?"none":order.Holding?"holding":"walking")}, busy rails {string.Join(",",rails.Where(r=>r.Busy).Select(r=>r.Label+":"+r.Phase+(r.Holding?"/held":"")))}, act {game.Personality?.Act}, occupied {string.Join(",",game.Mechanisms.Where(m=>m.Occupies(game,game.Motion.Selected)).Select(m=>m.name))}, on pads {string.Join(",",game.Owner.Apparatus.GetComponentsInChildren<COgheTissueSensor>().Where(p=>p.Active&&Enumerable.Range(0,32).Any(i=>game.Matter.Groups[i]==game.Matter.Groups[game.Motion.Selected]&&p.Holds(game.Matter.Bodies[i].position))).Select(p=>p.name))}, tube {game.InTube} attached {game.Attached}]";
      bool worked=rails.Where((r,i)=>r.CompletedJourneys>journeys[i]).Any();
      string state=miss<=allowed&&!worked?"ok":intel.GuidedStep!=step?"NOT GUIDED"+why:miss>allowed?"FAR"+why:"WORKED A RAIL ITSELF";
      if(game.Owner.Lost)state="LOST";
      lines.Add($"   step {step+1}/{targets.Count} {kind} {target.name}: {state} miss {miss:F3} m in {game.Matter.SimulationTime-start:F1}s");
      if(state!="ok")Capture(game.Definition.Order,$"clever-step{step+1}");
      if(state!="ok")problems.Add($"step {step+1} {kind} {target.name} {state} miss {miss:F3}");
     }
     IEnumerator CleverTap(Vector3 point)
     {
      // the script's own wait can end a tick before Clever counts the step done
      for(int i=0;i<3;i++)Tick();
      if(intel!=null&&intel.Progress<targets.Count&&intel.Progress>probed&&intel.Progress<intel.StepCount)
      {
       // split: Clever is off (Mrk 22/10/2026), nothing to wait for
       if(intel.Split){probed=intel.Progress;lines.Add($"   step {probed+1}/{targets.Count}: split, Clever off");}
       else yield return Probe();
      }
      yield return Tap(point);
     }
     string error="";float begin=game.Matter.SimulationTime;
     // a timeline of the lesson: steps done, and every walk Clever starts on its own
     int lastProgress=intel?.Progress??0,lastGuided=-1;float lastInput=game.LastInputTime;
     bool tracing=Environment.GetEnvironmentVariable("COGHE_CLEVER_TRACE")=="1";int walks=intel?.Walks??0,splitWalks=0;
     trace=()=>
     {
      if(intel==null)return;float t=game.Matter.SimulationTime-begin;
      if(intel.Walks!=walks){if(intel.Split)splitWalks++;walks=intel.Walks;}
      if(!tracing)return;
      if(intel.Progress!=lastProgress){lines.Add($"     t={t:F1} progress {lastProgress}->{intel.Progress}");lastProgress=intel.Progress;}
      if(intel.GuidedStep!=lastGuided){lines.Add($"     t={t:F1} Clever walks to step {intel.GuidedStep+1} from {game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected)):F2}");lastGuided=intel.GuidedStep;}
      if(game.LastInputTime!=lastInput){lines.Add($"     t={t:F1} tap");lastInput=game.LastInputTime;}
     };
     yield return Flat(new COgheSpatialScenario(game,CleverTap,Until).Solve(),e=>error=e);
     trace=null;
     if(splitWalks>0)problems.Add($"Clever walked {splitWalks}× while COghe was split");
     bool solved=game.Owner.Completed&&string.IsNullOrEmpty(error);
     string head=$"{n:00} {Path.GetFileNameWithoutExtension(order[n-1])} {game.Definition.Id}: {(solved?"solved":"UNSOLVED")} in {game.Matter.SimulationTime-begin:F1}s, steps {intel?.StepCount} guided {probed+1} {(solved?"":error)}";
     File.AppendAllText(log,head+"\n"+string.Join("\n",lines)+(lines.Count>0?"\n":""));
     if(!solved)problems.Add("unsolved: "+error);
     if(problems.Count>0)failures.Add($"{n:00} {game.Definition.Id}: "+string.Join("; ",problems));
    }
   }
   finally{COgheIntelligence.ForceForTests=false;COgheIntelligence.LevelForTests=null;}
   Assert.IsEmpty(failures,string.Join("\n",failures));
  }
 }
}
