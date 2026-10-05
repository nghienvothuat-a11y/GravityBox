using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Spatial Plus: the creature is moved in ways the author routes do not use, to find places it can be trapped.
 // Wander: from the start, walk to every corner and the middle of every fixed ledge, deck and floor it can reach, then
 // walk home; a trap fails at the spot. Then Retry and solve. The targeted tests cancel, overload, strand and retry.
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator WaitFor(float seconds,Func<bool> done){for(int i=0;i<seconds/Dt&&!done();i++){Tick();if(i%240==0)yield return null;}}
  private Vector3 Local(Vector3 world)=>game.Root.InverseTransformPoint(world);
  private bool TubeInFront(Vector3 target)
  {
   game.CameraRig.Frame(720,1280,0,true);var ray=game.Owner.View.ScreenPointToRay(game.Owner.View.WorldToScreenPoint(target));
   var hits=Physics.RaycastAll(ray,Mathf.Max(5,game.Owner.View.farClipPlane));Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
   foreach(var hit in hits)
   {
    if(hit.collider.GetComponent<VenomContact>()!=null)continue;
    var face=hit.collider.GetComponent<VenomSurfacePatch>();if(face!=null&&face.ExteriorGlass&&Vector3.Dot(ray.direction,face.Normal)>=-.001f)continue;
    return hit.collider.GetComponentInParent<COgheTubeNetwork>()!=null;
   }
   return false;
  }
  private IEnumerator WanderPlus(string key){yield return Wander(LoadPlus(key),key);}
  private IEnumerator Wander(IEnumerator load,string key)
  {
   yield return load;
   Directory.CreateDirectory("Artifacts/SpatialPlus");string log=$"Artifacts/SpatialPlus/wander-{key}.txt";File.WriteAllText(log,"");
   Vector3 home=game.Motion.Centre(0),homeTap=home+Vector3.down*.025f;
   var q=game.Owner.Apparatus.GetComponentInChildren<COgheQuantumSplitter>();
   var tasks=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();var outlet=Local(game.Owner.Outlet.position);
   var lifts=game.Owner.Apparatus.GetComponentsInChildren<COghePassengerLift>();
   // Every fixed, grippy, upward face: its middle and corners (3.5 cm in). Q and the handles are left alone: a tap
   // there is a split or a pull, not a walk.
   var targets=new List<Vector3>();
   foreach(var f in game.Surfaces)
   {
    if(f==null||!f.isActiveAndEnabled||f.Slippery||f.MotionFrame!=null||f.Hole||f.Normal.y<.9f)continue;
    float hx=Mathf.Max(0,f.Size.x*.5f-.035f),hy=Mathf.Max(0,f.Size.y*.5f-.035f);
    foreach(var o in new[]{Vector2.zero,new Vector2(-hx,-hy),new Vector2(hx,-hy),new Vector2(-hx,hy),new Vector2(hx,hy)})
    {
     var p=f.transform.TransformPoint(new Vector3(o.x,o.y,0));
     if(q!=null&&Vector2.Distance(new Vector2(Local(p).x,Local(p).z),new Vector2(Local(q.transform.position).x,Local(q.transform.position).z))<.14f)continue;
     if(Array.Exists(tasks,t=>Vector3.Distance(t.HandPoint,p)<.07f))continue;
     if(Array.Exists(lifts,l=>Vector3.Distance(l.Panel.position,p)<.08f))continue; // a lift button is a ride, not a walk
     if(Vector2.Distance(new Vector2(Local(p).x,Local(p).z),new Vector2(outlet.x,outlet.z))<.12f)continue; // walking out is not wandering
     if(targets.Exists(t=>Vector3.Distance(t,p)<.05f))continue;
     targets.Add(p);
    }
   }
   int reached=0,walked=0;var at=home;
   while(targets.Count>0)
   {
    // Nearest next: a walk around the box rather than back and forth across it.
    int best=0;for(int i=1;i<targets.Count;i++)if(Vector3.Distance(targets[i],at)<Vector3.Distance(targets[best],at))best=i;
    var target=targets[best];targets.RemoveAt(best);
    var before=game.Motion.Get(0);int id=before!=null?before.CommandId:-1;
    // A tap that lands on a tube body is a trip through it (Mrk, 05/10/2026), not a walk: leave it to the solve tests.
    if(TubeInFront(target)){File.AppendAllText(log,$"skip {Local(target):F3} (a tube in front)\n");continue;}
    yield return Tap(target);
    if(game.Attached){game.ReleaseProp();File.AppendAllText(log,$"skip {Local(target):F3} (grabbed a prop; let go)\n");continue;}
    var order=game.Motion.Get(0);
    if(order==null||order.CommandId==id||Array.Exists(tasks,t=>t.Owns(0))){File.AppendAllText(log,$"skip {Local(target):F3} (no walk)\n");if(Array.Exists(tasks,t=>t.Owns(0)))yield return Tap(homeTap);continue;}
    walked++;
    yield return WaitFor(15,()=>Vector3.Distance(game.Motion.Centre(0),target+Vector3.up*.02f)<.06f||game.Motion.Get(0)==null);
    yield return Wait(.5f);
    bool ok=Vector3.Distance(game.Motion.Centre(0),target+Vector3.up*.02f)<.07f;if(ok)reached++;
    at=game.Motion.Centre(0);
    File.AppendAllText(log,$"{(ok?"reach":"stop ")} {Local(target):F3} body {Local(at):F3} {game.Activity}\n");
    Assert.IsFalse(game.Owner.Lost,"Lost while wandering to "+Local(target));
    Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Split while wandering to "+Local(target));
    // Out through the exit hole (a low exit beside a wall the wander walked along): only fair where the exit is open,
    // and then the exit tap finishes the level; this wander is over.
    var inside=Local(game.Motion.Centre(0));
    if(Mathf.Abs(inside.x)>.40f||Mathf.Abs(inside.z)>.30f)
    {
     File.AppendAllText(log,$"out through the exit near {Local(target):F3}\n");
     Assert.IsTrue(game.FinalExitAvailable,"Slipped out through a closed exit while wandering to "+Local(target));
     yield return Tap(game.Owner.Outlet.position);yield return Until(40,()=>game.Owner.Completed,"The exit tap takes the body out");
     break;
    }
   }
   // Up on a lift's landing, a player presses the lift button to come down.
   foreach(var lift in lifts)if(lift.Rail.Position>lift.Rail.Travel*.5f&&!game.Owner.Completed)
   {int trips=lift.Trips;yield return Tap(lift.Panel.position);yield return WaitFor(40,()=>lift.Trips>trips&&!lift.Moving);File.AppendAllText(log,$"rode the lift down, body {Local(game.Motion.Centre(0)):F3}\n");}
   // Home again from wherever the wander ended. If a walk gives up, a player taps somewhere else first and tries
   // again: the room's open middle, then the four sides. Trapped means no tap frees the body.
   var detours=new List<Vector3>{Vector3.zero,new Vector3(-.30f,0,0),new Vector3(.30f,0,0),new Vector3(0,0,-.22f),new Vector3(0,0,.22f)};
   for(int attempt=0;attempt<=detours.Count&&!game.Owner.Completed&&Vector3.Distance(game.Motion.Centre(0),home)>.07f;attempt++)
   {
    if(attempt>0)
    {
     var d=game.Root.TransformPoint(detours[attempt-1]+Vector3.down*.30f);yield return Tap(d);
     yield return WaitFor(20,()=>Vector3.Distance(game.Motion.Centre(0),d+Vector3.up*.02f)<.07f||game.Motion.Get(0)==null);
     File.AppendAllText(log,$"detour {Local(d):F3} body {Local(game.Motion.Centre(0)):F3}\n");
    }
    yield return Tap(homeTap);yield return WaitFor(40,()=>Vector3.Distance(game.Motion.Centre(0),home)<.07f||game.Motion.Get(0)==null);
   }
   File.AppendAllText(log,$"walked {walked} reached {reached} home {Local(game.Motion.Centre(0)):F3}\n");
   if(!game.Owner.Completed)Assert.Less(Vector3.Distance(game.Motion.Centre(0),home),.07f,$"{key}: trapped after wandering at {Local(game.Motion.Centre(0)):F3} ({game.Activity}); see {log}");
   Assert.Greater(walked,3,"The wander found places to walk");
   // Retry clears the wander (a pad stood on, a plank tipped) and the level still solves.
   game.ResetLevel();yield return Wait(1);
   yield return new COgheSpatialScenario(game,Tap,Until).Solve();
   Assert.AreEqual(32,game.Matter.EscapedCount);Assert.IsTrue(game.Progress.Completed.Contains(game.Definition.Id));
  }
  // Chapter 1 rebuilt (05/10/2026): every new level gets a wander (stuck) test.
  [UnityTest] public IEnumerator Spatial02Wander(){yield return Wander(Load(2),"02");}
  [UnityTest] public IEnumerator Spatial05Wander(){yield return Wander(Load(5),"05");}
  [UnityTest] public IEnumerator Spatial06Wander(){yield return Wander(Load(6),"06");}
  [UnityTest] public IEnumerator Spatial07Wander(){yield return Wander(Load(7),"07");}
  [UnityTest] public IEnumerator Spatial08Wander(){yield return Wander(Load(8),"08");}
  [UnityTest] public IEnumerator Spatial09Wander(){yield return Wander(Load(9),"09");}
  [UnityTest] public IEnumerator Spatial10Wander(){yield return Wander(Load(10),"10");}
  [UnityTest] public IEnumerator SpatialPlusE01Wander(){yield return WanderPlus("E01");}
  [UnityTest] public IEnumerator SpatialPlusE02Wander(){yield return WanderPlus("E02");}
  [UnityTest] public IEnumerator SpatialPlusE03Wander(){yield return WanderPlus("E03");}
  [UnityTest] public IEnumerator SpatialPlusE04Wander(){yield return WanderPlus("E04");}
  [UnityTest] public IEnumerator SpatialPlusE05Wander(){yield return WanderPlus("E05");}
  [UnityTest] public IEnumerator SpatialPlusE06Wander(){yield return WanderPlus("E06");}
  [UnityTest] public IEnumerator SpatialPlusE07Wander(){yield return WanderPlus("E07");}
  [UnityTest] public IEnumerator SpatialPlusB1Wander(){yield return WanderPlus("B1");}
  [UnityTest] public IEnumerator SpatialPlusE08Wander(){yield return WanderPlus("E08");}
  [UnityTest] public IEnumerator SpatialPlusE09Wander(){yield return WanderPlus("E09");}
  [UnityTest] public IEnumerator SpatialPlusE10Wander(){yield return WanderPlus("E10");}
  [UnityTest] public IEnumerator SpatialPlusE11Wander(){yield return WanderPlus("E11");}
  [UnityTest] public IEnumerator SpatialPlusE12Wander(){yield return WanderPlus("E12");}
  [UnityTest] public IEnumerator SpatialPlusE13Wander(){yield return WanderPlus("E13");}
  [UnityTest] public IEnumerator SpatialPlusE14Wander(){yield return WanderPlus("E14");}
  [UnityTest] public IEnumerator SpatialPlusE15Wander(){yield return WanderPlus("E15");}
  [UnityTest] public IEnumerator SpatialPlusE16Wander(){yield return WanderPlus("E16");}
  [UnityTest] public IEnumerator SpatialPlusE17Wander(){yield return WanderPlus("E17");}
  [UnityTest] public IEnumerator SpatialPlusE18Wander(){yield return WanderPlus("E18");}
  [UnityTest] public IEnumerator SpatialPlusB2Wander(){yield return WanderPlus("B2");}

  // On the way to a handle another tap takes the body elsewhere and nothing moves. Once the hand is on the handle a
  // tap elsewhere is refused (the pull is not interruptible) and the pull finishes; pulling again runs it back.
  private IEnumerator CancelAndRedo(string key,string label)
  {
   yield return LoadPlus(key);var s=new COgheSpatialNextScenario(game,Tap,Until);var task=s.Task(label);
   var away=game.Root.TransformPoint(new Vector3(.25f,-.30f,-.25f));
   yield return Tap(task.HandPoint+Vector3.up*.004f);Assert.AreEqual(COgheTapRail.TaskPhase.Approaching,task.Phase,"Walking to "+label);
   yield return Wait(.5f);yield return Tap(away);
   Assert.AreEqual(COgheTapRail.TaskPhase.Idle,task.Phase,"A new tap ends the approach");
   yield return Until(30,()=>Vector3.Distance(game.Motion.Centre(0),away+Vector3.up*.02f)<.07f,"The body goes where it was sent instead");
   Assert.Less(task.Rail.Position,.004f,label+" did not move");
   int before=task.CompletedJourneys;yield return Tap(task.HandPoint+Vector3.up*.004f);
   yield return Until(40,()=>task.Phase==COgheTapRail.TaskPhase.Operating&&task.Rail.Position>task.Rail.Travel*.3f,"Pulling "+label);
   yield return Tap(away);Assert.AreEqual(COgheTapRail.TaskPhase.Operating,task.Phase,"A tap during the pull is refused");
   yield return Until(40,()=>task.CompletedJourneys>before,"The pull finishes");Assert.IsTrue(task.Rail.AtEnd);
   before=task.CompletedJourneys;yield return Tap(task.HandPoint+Vector3.up*.004f);
   yield return Until(40,()=>task.CompletedJourneys>before,"Pulled again, "+label+" runs back");Assert.Less(task.Rail.Position,.004f);
   game.ResetLevel();yield return Wait(1);
   yield return new COgheSpatialScenario(game,Tap,Until).Solve();Assert.AreEqual(32,game.Matter.EscapedCount);
  }
  [UnityTest] public IEnumerator SpatialPlusE04CartCancelAndRedo(){yield return CancelAndRedo("E04","A");}
  [UnityTest] public IEnumerator SpatialPlusE07BlockCancelAndRedo(){yield return CancelAndRedo("E07","A");}

  // The heavy pad (E09) takes half a body: a quarter stands on it and nothing moves; the half opens the door.
  [UnityTest] public IEnumerator SpatialPlusE09QuarterTooLightForHeavyPad()
  {
   yield return LoadPlus("E09");var s=new COgheSpatialNextScenario(game,Tap,Until);
   var q=game.Owner.Apparatus.GetComponentInChildren<COgheQuantumSplitter>();var door=s.Slider("A door");
   var pad=game.Root.TransformPoint(new Vector3(-.32f,-.298f,-.20f));
   yield return s.Split(q,game.Motion.Selected);int half=q.LastLeft,other=q.LastRight;
   yield return s.Walk(half,game.Root.TransformPoint(new Vector3(-.20f,-.30f,-.25f)),"The half off Q's tray");
   yield return s.Split(q,other);int quarter=q.LastLeft;
   yield return s.Walk(quarter,pad,"A quarter onto the heavy pad");yield return Wait(5);
   Assert.Less(door.Position,.01f,"A quarter does not open the heavy door");
   yield return s.Walk(quarter,game.Root.TransformPoint(new Vector3(-.36f,-.30f,-.10f)),"The quarter steps off");
   yield return s.Walk(half,pad,"The half onto the heavy pad");yield return Until(10,()=>door.AtEnd,"Half a body opens it");
   game.ResetLevel();yield return Wait(1);Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.Less(door.Position,.004f);
   yield return new COgheSpatialScenario(game,Tap,Until).Solve();Assert.AreEqual(32,game.Matter.EscapedCount);
  }

  // E15: the driver steps off P while the lift carries the rider; the lift sets the rider down, and rises again once
  // the driver is back.
  [UnityTest] public IEnumerator SpatialPlusE15DriverLeavesMidRide()
  {
   yield return LoadPlus("E15");var s=new COgheSpatialNextScenario(game,Tap,Until);
   var q=game.Owner.Apparatus.GetComponentInChildren<COgheQuantumSplitter>();var lift=s.Slider("Gear lift");
   var pad=game.Root.TransformPoint(new Vector3(-.30f,-.298f,-.20f));
   yield return s.Split(q,game.Motion.Selected);int driver=q.LastLeft,rider=q.LastRight;
   yield return s.Walk(rider,game.Root.TransformPoint(new Vector3(.13f,-.30f,-.02f)),"Rider steps wide of Q");
   yield return s.Walk(rider,game.Root.TransformPoint(new Vector3(-.01f,-.27f,.16f)),"Rider onto the lift");
   yield return s.Walk(driver,pad,"Driver onto P");yield return Until(20,()=>lift.Position>.06f,"The lift rises");
   yield return s.Walk(driver,game.Root.TransformPoint(new Vector3(-.18f,-.30f,-.26f)),"Driver steps off P mid-ride");
   yield return Until(25,()=>lift.Position<.01f,"Without its driver the lift comes back down");
   Assert.IsFalse(game.Owner.Lost);Assert.AreEqual(2,game.Matter.TotalFragmentCount);
   Assert.Less(Local(game.Motion.Centre(rider)).y,-.22f,"The rider is set down with the lift");
   yield return s.Walk(driver,pad,"Driver back on P");yield return Until(30,()=>lift.AtEnd,"The lift rises again");
   game.ResetLevel();yield return Wait(1);yield return new COgheSpatialScenario(game,Tap,Until).Solve();Assert.AreEqual(32,game.Matter.EscapedCount);
  }

  // E06: weight short of the axle does not tip a plank, and the body walks back off it.
  [UnityTest] public IEnumerator SpatialPlusE06NearHalfDoesNotTip()
  {
   yield return LoadPlus("E06");var s=new COgheSpatialNextScenario(game,Tap,Until);
   var plank=Array.Find(game.Props,p=>p.name=="A seesaw plank");var home=game.Motion.Centre(0);
   bool Tipped()=>plank.transform.TransformPoint(Vector3.right*.2f).y<plank.transform.position.y;
   yield return s.Go(plank.transform.TransformPoint(new Vector3(-.17f,.011f,0)),"Onto the foot of plank A",.07f);
   yield return Tap(plank.transform.TransformPoint(new Vector3(-.07f,.011f,0)));yield return Wait(6);
   Assert.IsFalse(Tipped(),"Short of the axle the plank stays down at its foot");
   yield return Tap(home+Vector3.down*.025f);yield return Until(30,()=>Vector3.Distance(game.Motion.Centre(0),home)<.07f,"Back off the plank");
   yield return Wait(2);Assert.IsFalse(Tipped());
   game.ResetLevel();yield return Wait(1);yield return new COgheSpatialScenario(game,Tap,Until).Solve();Assert.AreEqual(32,game.Matter.EscapedCount);
  }

  // B2: Retry partway (split, one part on its way) returns one whole body and every mechanism to the start.
  [UnityTest] public IEnumerator SpatialPlusB2RetryPartway()
  {
   yield return LoadPlus("B2");var s=new COgheSpatialNextScenario(game,Tap,Until);
   var q=game.Owner.Apparatus.GetComponentInChildren<COgheQuantumSplitter>();
   yield return s.Split(q,game.Motion.Selected);int half=q.LastLeft;
   yield return s.Walk(half,game.Root.TransformPoint(new Vector3(-.17f,-.30f,-.25f)),"The half toward the stairs");
   yield return s.Operate("A");
   game.ResetLevel();yield return Wait(1);
   Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.AreEqual(0,game.Matter.EscapedCount);
   foreach(var r in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())if(!r.name.StartsWith("Q "))Assert.Less(r.Position,.004f,"Retry returns "+r.name);
   yield return new COgheSpatialScenario(game,Tap,Until).Solve();Assert.AreEqual(32,game.Matter.EscapedCount);
  }
 }
}
