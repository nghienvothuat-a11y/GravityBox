#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using UnityEngine;
namespace GravityBox.Venom.ChapterProof
{
 // Author routes for Spatial 11–30. Every movement, task, split and exit goes through a visible screen tap
 // (fragment selection uses the public selection API). No pose, topology, gate or win state is assigned.
 public sealed class COgheSpatialNextScenario
 {
  readonly VenomCampaign game;readonly Func<Vector3,IEnumerator> tap;readonly Func<float,Func<bool>,string,IEnumerator> until;readonly Func<float,IEnumerator> orbit;
  public COgheSpatialNextScenario(VenomCampaign g,Func<Vector3,IEnumerator> t,Func<float,Func<bool>,string,IEnumerator> u,Func<float,IEnumerator> o=null){game=g;tap=t;until=u;orbit=o;}
  Transform Root=>game.Root;
  Vector3 W(float x,float y,float z)=>Root.TransformPoint(new Vector3(x,y,z));
  int Selected=>game.Motion.Selected;
  public COgheTapRail Task(string label)=>Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),x=>x.Label==label)??throw new InvalidOperationException("Missing task "+label);
  public T Find<T>() where T:Component=>game.Owner.Apparatus.GetComponentInChildren<T>()??throw new InvalidOperationException("Missing "+typeof(T).Name);
  // Tap a surface point for the selected fragment and wait until its centre arrives.
  public IEnumerator Go(Vector3 world,string why,float tolerance=.05f)
  {
   int actor=Selected;bool Arrived()=>Vector3.Distance(game.Motion.Centre(actor),world+Vector3.up*.02f)<tolerance;
   yield return tap(world);
   // As in Walk: a body that stops short (a climb that gave up) is sent again, as a player taps the spot a second time.
   for(int retry=0;retry<2&&!Arrived();retry++)
   {
    float start=game.Matter.SimulationTime;
    yield return until(40,()=>Arrived()||game.Motion.Get(actor)==null||game.Matter.SimulationTime-start>20,why);
    if(!Arrived()){game.SelectFragment(actor);yield return tap(world);}
   }
   yield return until(40,Arrived,why);
  }
  public IEnumerator Operate(string label,float seconds=40)
  {
   var t=Task(label);int before=t.CompletedJourneys;
   for(int attempt=0;attempt<3&&!t.Busy;attempt++)
   {
    yield return tap(t.HandPoint+Vector3.up*.004f);
    // The handle may be hidden behind the selected body itself; step beside it first, as a player would.
    if(!t.Busy&&attempt==0){yield return tap(t.StandPoint);yield return until(20,()=>Vector3.Distance(game.Motion.Centre(Selected),t.StandPoint)<.06f,"Step beside handle "+label);}
   }
   if(!t.Busy)throw new InvalidOperationException("Handle "+label+" rejected: "+t.LastFailure);
   // The game asks for a fresh tap when a pull loses its footing; a player taps the handle again.
   for(int retry=0;retry<2;retry++)
   {
    yield return until(seconds,()=>t.CompletedJourneys>before||!t.Busy,"Operate "+label);
    if(t.CompletedJourneys>before||t.LastFailure==null||!t.LastFailure.StartsWith("Mất điểm bám"))break;
    yield return tap(t.HandPoint+Vector3.up*.004f);
   }
   yield return until(seconds,()=>t.CompletedJourneys>before,"Operate "+label);
  }
  public IEnumerator Until(float seconds,Func<bool> done,string why){yield return until(seconds,done,why);}
  // A dead-man handle: approach, pull to its end and keep bracing until another command releases it.
  public IEnumerator Hold(string label)
  {
   var t=Task(label);
   for(int attempt=0;attempt<3&&!t.Busy;attempt++)
   {
    yield return tap(t.HandPoint+Vector3.up*.004f);
    if(!t.Busy&&attempt==0){yield return tap(t.StandPoint);yield return until(20,()=>Vector3.Distance(game.Motion.Centre(Selected),t.StandPoint)<.06f,"Step beside handle "+label);}
   }
   if(!t.Busy)throw new InvalidOperationException("Handle "+label+" rejected: "+t.LastFailure);
   yield return until(40,()=>t.Holding,"Hold "+label);
  }
  public COgheRailSlider Slider(string name)=>Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name==name)??throw new InvalidOperationException("Missing rail "+name);
  // Tube travel through visible mouths and junction branches.
  public IEnumerator EnterTube(COgheTubeNetwork tube,int node)
  {
   int actor=Selected;
   for(int attempt=0;attempt<3&&!tube.IsParticleInside(actor)&&!tube.IsApproachingEntry(actor,node);attempt++)yield return tap(Root.TransformPoint(tube.Nodes[node].LocalPosition));
   yield return until(35,()=>tube.IsParticleInside(actor),"Enter tube at "+tube.Nodes[node].Name);
  }
  public IEnumerator Choose(COgheTubeNetwork tube,int junction,int edge)
  {
   int actor=Selected;
   yield return until(35,()=>tube.LastReachedNode==junction&&tube.AnyWaiting,"Gather in junction "+tube.Nodes[junction].Name);
   var e=tube.Edges[edge];Vector3 point=Root.TransformPoint(tube.Edges[edge].ControlPoints[e.A==junction?1:e.ControlPoints.Length-2]);
   for(int attempt=0;attempt<3&&tube.LastChosenEdge!=edge;attempt++)yield return tap(point);
   if(tube.LastChosenEdge!=edge)throw new InvalidOperationException("Branch not chosen "+e.Name);
  }
  public IEnumerator LeaveTube(COgheTubeNetwork tube){int actor=Selected;yield return until(40,()=>!tube.IsParticleInside(actor),"Leave tube");}
  // Loose props use the shared push/pull: tap the prop, then tap where it should slide.
  public IEnumerator Push(VenomMovableProp prop,Vector3 target,float tolerance=.012f)
  {
   // Already held (a caller grasped it at a visible spot): go straight to sliding.
   if(!game.Attached){yield return tap(prop.Body.position+Vector3.up*.012f);yield return until(20,()=>game.Attached,"Grasp "+prop.name);}
   Vector3 direction=Vector3.ProjectOnPlane(target-prop.Body.position,Vector3.up).normalized;
   for(int attempt=0;attempt<6&&Flat(prop.Body.position-target)>tolerance;attempt++)
   {
    // Re-taps aim at the floor just beyond the crate, so the ray never lands on the crate's own top.
    yield return tap(attempt==0?target:target+direction*.05f);
    float start=game.Matter.SimulationTime;
    yield return until(12,()=>Flat(prop.Body.position-target)<=tolerance||game.Matter.SimulationTime-start>2.5f||!game.Attached,"Slide "+prop.name);
    if(!game.Attached&&Flat(prop.Body.position-target)>tolerance){yield return tap(prop.Body.position+Vector3.up*.012f);yield return until(20,()=>game.Attached,"Regrasp "+prop.name);}
   }
   yield return until(1,()=>Flat(prop.Body.position-target)<=tolerance,"Place "+prop.name);
   yield return until(8,()=>!game.Attached,"Let go of "+prop.name);
  }
  static float Flat(Vector3 v){v.y=0;return v.magnitude;}
  int Count(int anchor){int n=0;for(int i=0;i<32;i++)if(!game.Matter.Escaped[i]&&game.Matter.Groups[i]==game.Matter.Groups[anchor])n++;return n;}
  // Walk the selected fragment into Q; the machine itself gathers, separates and delivers two equal lobes.
  public IEnumerator Split(COgheQuantumSplitter q,int actor)
  {
   int before=q.Splits,mass=Count(actor);game.SelectFragment(actor);
   yield return tap(q.transform.position+q.transform.rotation*new Vector3(0,.1f,-.02f));
   yield return until(45,()=>q.Splits>before&&q.Phase==COgheQuantumSplitter.SplitPhase.Idle,"Q splits and clears both trays");
   if(Count(q.LastLeft)!=mass/2||Count(q.LastRight)!=mass/2||game.Matter.Groups[q.LastLeft]==game.Matter.Groups[q.LastRight])
    throw new InvalidOperationException($"Q split was not exact: {mass} -> {Count(q.LastLeft)}/{Count(q.LastRight)}");
  }
  public IEnumerator Walk(int actor,Vector3 world,string why,float tolerance=.05f)
  {
   bool Arrived()=>Vector3.Distance(game.Motion.Centre(actor),world+Vector3.up*.02f)<tolerance;
   yield return Command(actor,world);
   // A part that stalls on the way (or drops its order) is sent again, as a player taps the spot a second time.
   for(int retry=0;retry<2&&!Arrived();retry++)
   {
    float start=game.Matter.SimulationTime;
    yield return until(40,()=>Arrived()||game.Motion.Get(actor)==null||game.Matter.SimulationTime-start>20,why);
    if(!Arrived())yield return Command(actor,world);
   }
   yield return until(40,Arrived,why);
  }
  // A tap whose ray passes over another part selects that part (game rule); try nearby visible floor points.
  public IEnumerator Command(int actor,Vector3 world)
  {
   Vector3[] offsets={Vector3.zero,new Vector3(.03f,0,0),new Vector3(-.03f,0,0),new Vector3(0,0,.03f),new Vector3(0,0,-.03f)};
   foreach(var offset in offsets)
   {
    game.SelectFragment(actor);yield return tap(world+Root.TransformDirection(offset));
    var o=game.Motion.Get(actor);
    if(o!=null&&Vector3.Distance(Root.TransformPoint(o.Target),world+Root.TransformDirection(offset))<.06f)yield break;
   }
   throw new InvalidOperationException($"No visible floor command for part {actor} at {Root.InverseTransformPoint(world):F3}; activity={game.Activity}");
  }
  // Like Command, but tries several destinations and never throws (used for re-taps).
  public bool CommandOk;
  public IEnumerator CommandAny(int actor,System.Collections.Generic.List<Vector3> worlds)
  {
   CommandOk=false;
   foreach(var world in worlds)
   {
    game.SelectFragment(actor);yield return tap(world);
    var o=game.Motion.Get(actor);
    // A tap a handle claims is not a floor command; the next candidate's tap cancels that approach before it pulls.
    bool claimed=Array.Exists(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),t=>t.Owns(actor));
    if(!claimed&&o!=null&&Vector3.Distance(Root.TransformPoint(o.Target),world)<.06f){CommandOk=true;yield break;}
   }
  }
  public IEnumerator Merge(Vector3 world)
  {
   var anchors=new System.Collections.Generic.List<int>();var seen=new System.Collections.Generic.HashSet<int>();
   for(int i=0;i<32;i++)if(!game.Matter.Escaped[i]&&seen.Add(game.Matter.Groups[i]))anchors.Add(i);
   yield return MergeParts(anchors,world,1);
  }
  // Tap points around a spot (the spot itself may sit behind a part or scenery at another screen framing).
  System.Collections.Generic.List<Vector3> Around(Vector3 world)
  {
   var local=Root.InverseTransformPoint(world);var list=new System.Collections.Generic.List<Vector3>();
   foreach(var o in new[]{Vector3.zero,new Vector3(0,0,-.04f),new Vector3(.04f,0,0),new Vector3(-.04f,0,0),new Vector3(0,0,.04f),new Vector3(.04f,0,-.04f),new Vector3(-.04f,0,-.04f),new Vector3(0,0,-.08f)})list.Add(Root.TransformPoint(local+o));
   return list;
  }
  // Brings the given parts together until only `remaining` bodies exist in the box.
  public IEnumerator MergeParts(System.Collections.Generic.List<int> anchors,Vector3 world,int remaining)
  {
   var groups=new System.Collections.Generic.HashSet<int>();
   foreach(int a in anchors)
   {
    if(game.Matter.TotalFragmentCount<=remaining)break;
    if(!groups.Add(game.Matter.Groups[a]))continue;
    yield return CommandAny(a,Around(world));
   }
   // Parts that stall a hand's width apart are called together again, as a player would re-tap between them.
   for(int round=0;round<3&&game.Matter.TotalFragmentCount>remaining;round++)
   {
    float start=game.Matter.SimulationTime;
    yield return until(45,()=>game.Matter.TotalFragmentCount<=remaining||game.Matter.SimulationTime-start>15&&Stalled(),"All parts meet and fuse");
    if(game.Matter.TotalFragmentCount<=remaining)break;
    Vector3 middle=Vector3.zero;int count=0;groups.Clear();
    foreach(int a in anchors)if(!game.Matter.Escaped[a]&&groups.Add(game.Matter.Groups[a])){middle+=game.Motion.Centre(a);count++;}
    var local=Root.InverseTransformPoint(middle/count);local.y=Root.InverseTransformPoint(world).y;
    var candidates=Around(Root.TransformPoint(local));groups.Clear();
    foreach(int a in anchors)if(!game.Matter.Escaped[a]&&groups.Add(game.Matter.Groups[a]))yield return CommandAny(a,candidates);
   }
   yield return until(45,()=>game.Matter.TotalFragmentCount<=remaining,"All parts meet and fuse");
  }
  bool Stalled(){for(int i=0;i<32;i++)if(!game.Matter.Escaped[i]&&game.Matter.Bodies[i].linearVelocity.magnitude>.01f)return false;return true;}
  // Rope swing: tap the ring (grip), then tap the chosen landing; gravity swings the body across.
  public IEnumerator Grip(COgheSwingTransfer swing)
  {
   // If the approach gives up ("cannot reach ring"), the player taps the ring again from where the part now is.
   for(int attempt=0;attempt<2&&swing.Phase!=COgheSwingTransfer.SwingPhase.Ready;attempt++)
   {
    yield return tap(swing.Ring.position);
    yield return until(40,()=>swing.Phase==COgheSwingTransfer.SwingPhase.Ready||swing.Phase==COgheSwingTransfer.SwingPhase.Idle&&swing.Actor<0,"Grip ring "+swing.Label);
   }
   yield return until(40,()=>swing.Phase==COgheSwingTransfer.SwingPhase.Ready,"Grip ring "+swing.Label);
  }
  public IEnumerator Swing(COgheSwingTransfer swing,int dock)
  {
   int landings=swing.Landings;var patch=swing.Docks[dock];
   if(!patch.isActiveAndEnabled)patch=Array.Find(swing.Docks,d=>d.isActiveAndEnabled);
   yield return tap(patch.Closest(swing.DockTargets[dock].position));
   yield return until(20,()=>swing.Landings>landings||swing.Misses>0,"Swing to landing "+dock);
   if(swing.Landings==landings)throw new InvalidOperationException("Swing missed the landing");
   yield return until(20,()=>game.Motion.Get(Selected)==null||Vector3.Distance(game.Motion.Centre(Selected),swing.DockTargets[dock].position)<.05f,"Crawl onto the landing");
  }
  public IEnumerator Exit()
  {
   yield return until(15,()=>game.FinalExitAvailable,"Exit physically unlocked");
   // A tall crate (crate levels, 08/10/2026) can stand between the camera and the hole: tap a part of the hole the camera
   // sees, as a player does, else turn the view a quarter and look again.
   for(int turn=0;turn<4;turn++)
   {
    var spot=VisibleExitSpot();
    if(spot.HasValue){yield return tap(spot.Value);break;}
    game.CameraRig.Orbit(270,720);yield return null;
   }
   yield return until(45,()=>game.Owner.Completed,"All tissue through the final exit");
  }
  Vector3? VisibleExitSpot()
  {
   var outlet=game.Owner.Outlet;var cam=game.Owner.View;float r=game.Owner.ApertureRadius*.6f;
   foreach(var o in new[]{Vector2.zero,new Vector2(r,0),new Vector2(-r,0),new Vector2(0,r),new Vector2(0,-r)})
   {
    var p=outlet.position+outlet.right*o.x+outlet.up*o.y;var ray=cam.ScreenPointToRay(cam.WorldToScreenPoint(p));
    bool covered=false;
    foreach(var h in System.Linq.Enumerable.OrderBy(Physics.RaycastAll(ray,10),h=>h.distance))
    {
     var pane=h.collider.GetComponent<VenomSurfacePatch>();
     if(h.collider.isTrigger||h.collider.GetComponent<VenomContact>()!=null||pane!=null&&pane.ExteriorGlass)continue;
     covered=h.rigidbody!=null&&h.rigidbody.GetComponent<COgheTapRail>()!=null&&Vector3.Distance(h.point,p)>.01f;break;
    }
    if(!covered)return p;
   }
   return null;
  }
  // The level's content, from its ID ("coghe.spatial.next.29" → 29; "coghe.spatial.plus.e04" → "E04"): the campaign
  // order (Definition.Order) moves levels around, their author routes stay with the content.
  public static string ContentKey(VenomCampaign g){string id=g.Definition.Id??string.Empty,tail=id.Substring(id.LastIndexOf('.')+1);return id.Contains(".plus.")?tail.ToUpperInvariant():tail;}
  public IEnumerator Solve()
  {
   int n=int.Parse(ContentKey(game));
   if(n==11)
   {
    yield return Operate("A");
    yield return until(5,()=>Task("A").Rail.AtEnd,"Crate parked in its socket");
    yield return Go(W(-.004f,-.14f,.10f),"Climb onto the parked crate");
    yield return Go(W(.20f,-.14f,.20f),"Step across onto the plinth");
   }
   else if(n==12)
   {
    var a=Task("A").Rail;var b=Task("B").Rail;
    yield return Operate("B");yield return until(5,()=>b.Position<=b.CatchTolerance,"Low block waits aside");
    yield return Operate("A");yield return until(5,()=>a.AtEnd,"Tall block reaches the island");
    yield return Operate("B");yield return until(5,()=>b.AtEnd,"Low block returns to the crossing");
    yield return Go(W(-.028f,-.27f,.14f),"Mount the low block");
    yield return Go(W(.096f,-.24f,.14f),"Mount the tall block");
    yield return Go(W(.27f,-.21f,.20f),"Reach the exit island");
   }
   else if(n==13)
   {
    var lift=Find<COghePassengerLift>();var crate=Array.Find(game.Props,p=>p.name=="A crate");
    yield return Push(crate,W(.07f,-.2545f,.16f),.03f); // anywhere well on the tray deck; the socket push below stays exact
    yield return tap(lift.Panel.position);yield return until(45,()=>lift.Trips==1&&lift.Rail.AtEnd,"Ride up with the crate");
    var socket=Find<COghePropSocket>();
    // If the crate settles off its seat after letting go, push it home again (the pawl only catches a resting crate).
    for(int attempt=0;attempt<3&&!socket.Seated;attempt++){yield return Push(crate,W(.214f,-.0565f,.16f));float t0=game.Matter.SimulationTime;yield return until(8,()=>socket.Seated||game.Matter.SimulationTime-t0>3,"Crate rests on the socket");}
    yield return until(6,()=>socket.Seated,"Socket pawl catches the resting crate");
    yield return Go(W(.33f,-.01f,.18f),"Climb the seated crate to the exit bench");
   }
   else if(n==14)
   {
    var tube=Find<COgheTubeNetwork>();
    yield return Operate("A");yield return until(8,()=>tube.IsEntryOpen(0),"Cap lifted from the mouth");
    yield return EnterTube(tube,0);yield return LeaveTube(tube);
   }
   else if(n==15)
   {
    var tube=Find<COgheTubeNetwork>();
    yield return EnterTube(tube,0);yield return Choose(tube,1,1);yield return LeaveTube(tube);
    yield return Operate("A");yield return until(8,()=>tube.Edges[2].Open,"Right branch door latched open");
    yield return EnterTube(tube,2);yield return Choose(tube,1,2);yield return LeaveTube(tube);
   }
   else if(n==16)
   {
    var q=Find<COgheQuantumSplitter>();var latch=Find<COgheLoadLatch>();
    yield return Split(q,Selected);int left=q.LastLeft,right=q.LastRight;
    yield return Walk(left,W(-.27f,-.298f,.02f),"Left half loads pad A");
    yield return Walk(right,W(.27f,-.298f,.02f),"Right half loads pad B");
    yield return until(20,()=>latch.Caught,"Both loads slide the bridge out; its pawl catches");
    yield return Merge(W(.10f,-.30f,.04f));
    yield return Go(W(0,-.27f,.08f),"Onto the step bridge");
    yield return Go(W(0,-.24f,.21f),"Onto the exit platform");
   }
   else if(n==17)
   {
    var q=Find<COgheQuantumSplitter>();var tube=Find<COgheTubeNetwork>();var door=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="B return door");
    yield return Split(q,Selected);int holder=q.LastLeft,worker=q.LastRight;
    yield return Walk(holder,W(-.30f,-.298f,.17f),"Holder loads pad A");
    yield return until(10,()=>tube.IsEntryOpen(0),"Pad A holds the tube cap open");
    game.SelectFragment(worker);yield return EnterTube(tube,0);yield return LeaveTube(tube);
    yield return Operate("B");yield return until(10,()=>door.AtEnd,"B latches the return door open");
    yield return Walk(holder,W(.20f,-.30f,-.20f),"Holder leaves A and walks through the return door");
    // Meet in the open middle of the right room, so the halves converge from two sides instead of queueing in a corner.
    yield return Merge(W(.24f,-.30f,.02f));
   }
   else if(n==18)
   {
    var swing=Find<COgheSwingTransfer>();
    yield return Grip(swing);
    yield return Swing(swing,0);
   }
   else if(n==19)
   {
    var swing=Find<COgheSwingTransfer>();var tray=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="B landing tray");
    yield return Operate("B");yield return until(10,()=>tray.AtEnd,"B brings the landing tray into the arc");
    yield return Go(W(-.28f,-.16f,.05f),"Climb the stairs onto the start bank");
    yield return Grip(swing);
    yield return Swing(swing,1);
   }
   else if(n==20)
   {
    var q=Find<COgheQuantumSplitter>();var tube=Find<COgheTubeNetwork>();
    var gate=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="B return gate");
    var span=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="Winch lifting span");
    yield return Split(q,Selected);int left=q.LastLeft,right=q.LastRight;
    yield return Walk(right,W(.13f,-.298f,-.25f),"One half loads pad A");
    yield return until(10,()=>tube.IsEntryOpen(0),"Pad A holds the tube cap open");
    // Into the corridor behind Q first; a straight line to the mouth hugs Q's back corner.
    yield return Walk(left,W(-.24f,-.30f,.03f),"Round Q's back corner into the corridor");
    game.SelectFragment(left);yield return EnterTube(tube,0);yield return LeaveTube(tube);
    yield return Operate("B");yield return until(10,()=>gate.AtEnd,"B lifts the return gate and latches the cap");
    yield return Walk(left,W(.36f,-.29f,-.14f),"Down the return ramp",.07f);
    yield return Merge(W(.24f,-.30f,-.15f));
    yield return Go(W(.05f,-.30f,.05f),"Walk round behind Q");yield return Go(W(-.24f,-.30f,.05f),"Along the corridor");yield return Go(W(-.24f,-.30f,-.10f),"Beside the crate lane");
    yield return Operate("C");yield return until(5,()=>Task("C").Rail.AtEnd,"Crate parked at the bench");
    yield return Go(W(-.30f,-.21f,.17f),"Climb crate and winch bench");
    yield return Operate("D",60);yield return until(15,()=>span.AtEnd,"Full body winds the span up");
    yield return Go(W(-.02f,-.21f,.23f),"Cross the span to the exit platform");
   }
   else if(n==21)
   {
    var seesaw=Find<COgheSeesawBridge>();var crate=Array.Find(game.Props,p=>p.name=="A crate");
    yield return Push(crate,W(-.28f,-.187f,.108f),.012f);
    yield return until(20,()=>seesaw.Caught,"Loaded tray sinks; rope lifts the plank level; pawl catches");
    yield return Go(W(0,-.20f,.10f),"Onto the level plank");
    yield return Go(W(.25f,-.20f,.12f),"Across to the exit platform");
   }
   else if(n==22)
   {
    var a=Task("A").Rail;var b=Task("B").Rail;var span=Task("C").Rail;
    yield return Operate("B");yield return until(5,()=>b.Position<=b.CatchTolerance,"Tall block waits aside");
    yield return Operate("C");yield return until(5,()=>span.AtEnd,"Span reaches its far bearers");
    yield return Operate("B");yield return until(5,()=>b.AtEnd,"Tall block into its socket");
    yield return Operate("A");yield return until(5,()=>a.AtEnd,"Low block into its socket");
    yield return Go(W(-.12f,-.27f,.08f),"Mount the low block");
    yield return Go(W(-.016f,-.24f,.08f),"Mount the tall block");
    yield return Go(W(.095f,-.24f,.08f),"Onto the span");
    yield return Go(W(.27f,-.24f,.12f),"Onto the exit platform");
   }
   else if(n==23)
   {
    var tube=Find<COgheTubeNetwork>();
    yield return EnterTube(tube,0);yield return Choose(tube,1,1);yield return LeaveTube(tube);
    yield return Operate("A");yield return until(8,()=>tube.Edges[2].Open,"A turns the junction valve onto the upper route");
    yield return EnterTube(tube,2);yield return Choose(tube,1,2);yield return LeaveTube(tube);
   }
   else if(n==24)
   {
    var q=Find<COgheQuantumSplitter>();var latch=Find<COgheLoadLatch>();
    yield return Split(q,Selected);int l=q.LastLeft,r=q.LastRight;
    yield return Walk(r,W(.20f,-.30f,-.02f),"Right half waits off the tray");
    yield return Split(q,l);int l1=q.LastLeft,l2=q.LastRight;
    yield return Walk(l1,W(-.33f,-.298f,-.20f),"25 % onto A1");
    yield return Walk(l2,W(.33f,-.298f,-.20f),"25 % onto B1");
    yield return Split(q,r);int r1=q.LastLeft,r2=q.LastRight;
    yield return Walk(r1,W(-.33f,-.298f,.14f),"25 % onto A2");
    yield return Walk(r2,W(.33f,-.298f,.14f),"25 % onto B2");
    yield return until(20,()=>latch.Caught,"Four loads slide the step out; pawl catches");
    yield return Merge(W(.12f,-.30f,-.02f));
    yield return Go(W(0,-.27f,.07f),"Onto the step");
    yield return Go(W(0,-.24f,.21f),"Onto the exit platform");
   }
   else if(n==25)
   {
    var q=Find<COgheQuantumSplitter>();var bolt=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="A lock bolt");var b=Task("B");
    yield return Split(q,Selected);int l=q.LastLeft,big=q.LastRight;
    yield return Walk(big,W(.12f,-.30f,-.06f),"50 % waits off the tray");
    yield return Split(q,l);int s1=q.LastLeft,s2=q.LastRight;
    yield return Walk(s1,W(-.31f,-.298f,.16f),"25 % onto A1");
    yield return Walk(s2,W(.06f,-.298f,-.22f),"25 % onto A2");
    yield return until(10,()=>bolt.AtEnd,"Both small loads draw the bolt");
    game.SelectFragment(big);yield return Operate("B",40);yield return until(5,()=>b.Rail.AtEnd,"50 % pushes heavy B into its socket");
    yield return Merge(W(-.05f,-.30f,-.02f));
    yield return Go(W(.17f,-.27f,.005f),"Onto the fixed step");
    yield return Go(W(.17f,-.24f,.12f),"Onto block B");
    yield return Go(W(.31f,-.21f,.15f),"Onto the exit platform");
   }
   else if(n==26)
   {
    var q=Find<COgheQuantumSplitter>();var tube=Find<COgheTubeNetwork>();var swing=Find<COgheSwingTransfer>();
    var flap=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="B landing tray");
    yield return Split(q,Selected);int holder=q.LastLeft,worker=q.LastRight;
    yield return Walk(holder,W(-.34f,-.158f,.24f),"Holder loads pad A");
    yield return until(10,()=>tube.IsEntryOpen(0),"Pad A holds the tube cap open");
    game.SelectFragment(worker);yield return EnterTube(tube,0);yield return LeaveTube(tube);
    yield return Operate("B");yield return until(10,()=>flap.AtEnd,"B slides the landing tray into the arc and latches the cap");
    game.SelectFragment(holder);yield return Grip(swing);
    yield return Swing(swing,1);
    yield return Merge(W(.30f,-.188f,.10f));
   }
   else if(n==27)
   {
    var q=Find<COgheQuantumSplitter>();var lift=Find<COghePassengerLift>();var bolt=Slider("A lock bolt");var pin=Slider("Lift enable pin");var latch=Task("C");
    yield return Split(q,Selected);int l=q.LastLeft,r=q.LastRight;
    yield return Walk(r,W(-.20f,-.30f,-.01f),"50 % waits behind Q");
    yield return Split(q,l);int a1=q.LastLeft,a2=q.LastRight;
    yield return Walk(a1,W(-.33f,-.298f,-.22f),"25 % onto A1");
    yield return Walk(a2,W(.33f,-.298f,-.22f),"25 % onto A2");
    yield return until(10,()=>bolt.AtEnd,"Both pads draw the bolt from lever B");
    yield return Split(q,r);int holder=q.LastLeft,rider=q.LastRight;
    // Wide of Q's back-right corner first (floor taps straight behind Q pass through Q's own touch zone).
    yield return Walk(rider,W(.20f,-.30f,-.08f),"Rider steps wide of Q's back-right corner");
    yield return Walk(rider,W(-.06f,-.30f,.03f),"Rider waits in front of the lift");
    game.SelectFragment(holder);yield return Hold("B");
    yield return until(10,()=>pin.AtEnd,"Holding B powers the lift");
    game.SelectFragment(rider);yield return tap(lift.Panel.position);
    yield return until(45,()=>lift.Trips>=1&&lift.Rail.AtEnd,"B's power lifts the fourth part to the high platform");
    game.SelectFragment(rider);yield return Operate("C");yield return until(5,()=>latch.Rail.AtEnd,"C latches the lift power");
    // The holder lets go of B; with the rider's latch C the lift stays powered. The three on the floor merge.
    yield return MergeParts(new System.Collections.Generic.List<int>{holder,a1,a2},W(-.08f,-.30f,0f),2);
    yield return until(5,()=>pin.AtEnd&&lift.Rail.AtEnd,"Lift stays powered at the top");
    int group=holder;
    yield return tap(lift.CallPanels[0].position);yield return until(45,()=>lift.Rail.Position<=lift.Rail.CatchTolerance*2&&!lift.Moving,"Call the lift down");
    game.SelectFragment(group);yield return tap(lift.Panel.position);
    yield return until(60,()=>lift.Trips>=3&&lift.Rail.AtEnd,"75 % rides up");
    yield return Merge(W(.20f,-.06f,.22f));
   }
   else if(n==28)
   {
    var q=Find<COgheQuantumSplitter>();var tube=Find<COgheTubeNetwork>();var gate=Slider("D climb gate");
    yield return Split(q,Selected);int outer=q.LastLeft,inner=q.LastRight;
    game.SelectFragment(outer);yield return Hold("A");
    yield return until(10,()=>tube.Edges[3].Open,"Holding A opens J2's maintenance branch");
    game.SelectFragment(inner);yield return EnterTube(tube,0);yield return Choose(tube,1,2);yield return Choose(tube,3,3);yield return LeaveTube(tube);
    yield return Operate("C");yield return until(5,()=>Task("C").Rail.AtEnd,"C latches the maintenance route");
    yield return Walk(outer,W(-.25f,.02f,.20f),"Outer half leaves A and climbs to station B");
    game.SelectFragment(outer);yield return Operate("B");yield return until(5,()=>tube.Edges[5].Open,"B turns J3 onto the upper balcony");
    game.SelectFragment(inner);yield return EnterTube(tube,4);yield return Choose(tube,3,4);yield return Choose(tube,5,5);yield return LeaveTube(tube);
    yield return Operate("D");yield return until(8,()=>gate.AtEnd,"D lifts the gate at the head of the outer bridge");
    // Meet at the head of the outer bridge (back-left of the upper balcony): far from its front edge, and a tap there
    // stays clear of the tube mouth's and handle D's pick zones. The merged body then walks along the back to the exit.
    yield return Merge(W(.18f,.08f,.24f));
   }
   else if(n==29)
   {
    var q=Find<COgheQuantumSplitter>();var bolt=Slider("A piece bolt");var frame=Find<COgheSeesawBridge>();var frameLock=Slider("Frame lock bolt");
    // The left half is the worker: its way to the stairs never passes the other half's tray (touching parts fuse).
    yield return Split(q,Selected);int worker=q.LastLeft,r=q.LastRight;
    yield return Walk(worker,W(-.33f,-.30f,-.25f),"50 % waits at the foot of the stairs");
    yield return Split(q,r);int s1=q.LastLeft,s2=q.LastRight;
    yield return Walk(s2,W(.34f,-.298f,-.20f),"25 % onto A1");
    // Behind Q, well clear of the part already standing on A1 (touching parts fuse).
    yield return Walk(s1,W(-.05f,-.30f,-.05f),"25 % steps north of the left tray");yield return Walk(s1,W(.22f,-.30f,.0f),"25 % goes round behind Q");yield return Walk(s1,W(.34f,-.298f,-.065f),"25 % onto A2");
    yield return until(10,()=>bolt.AtEnd,"Both pads draw the piece bolt");
    game.SelectFragment(worker);yield return Operate("C");yield return until(5,()=>Task("C").Rail.AtEnd,"Far piece C slides down the frame through the near seat into the far one");
    game.SelectFragment(worker);yield return Operate("D");yield return until(5,()=>Task("D").Rail.AtEnd,"Near piece D slides in from its bay");
    yield return until(10,()=>frameLock.AtEnd,"Both seats caught: the frame lock frees B");
    // Meet on the floor in front of the frame, left of Q's tray and clear of B's handle.
    yield return Merge(W(-.14f,-.30f,-.20f));
    yield return Operate("B",60);yield return until(15,()=>frame.Caught,"Whole body winds B; the frame rises level with both pieces and its pawl catches");
    yield return Go(W(-.34f,-.18f,.13f),"Up the stairs onto the landing");
    yield return Go(W(.02f,-.18f,.20f),"Across the lifted bridge onto the exit platform");
   }
   else if(n==30)
   {
    var q=Find<COgheQuantumSplitter>();var tube=Find<COgheTubeNetwork>();var swing=Find<COgheSwingTransfer>();
    var boltA=Slider("A bolt");var boltB=Slider("B bolt");var gate=Slider("B return gate");
    var spans=Array.FindAll(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="E lifting span");
    yield return Split(q,Selected);int l=q.LastLeft,r=q.LastRight;
    yield return Walk(r,W(-.08f,-.30f,.0f),"50 % waits in the corridor right of Q");
    yield return Split(q,l);int holderA=q.LastLeft,holderB=q.LastRight;
    yield return Walk(holderA,W(-.35f,-.298f,-.02f),"25 % holds pad A");
    yield return Walk(holderB,W(.12f,-.30f,-.25f),"Round the front of the blocks");yield return Walk(holderB,W(.285f,-.298f,.045f),"25 % holds pad B");
    yield return until(10,()=>boltA.AtEnd&&boltB.AtEnd,"Both pads draw their bolts");
    // The tube worker leaves from the right tray (behind Q, clear of pad A); the swinger from the left tray along the front.
    yield return Split(q,r);int tubeWorker=q.LastRight,swinger=q.LastLeft;
    yield return Walk(tubeWorker,W(-.08f,-.30f,.02f),"Tube worker up the corridor right of Q");
    game.SelectFragment(tubeWorker);yield return EnterTube(tube,0);yield return LeaveTube(tube);
    yield return Operate("A");yield return until(5,()=>Task("A").Rail.AtEnd,"Latch A catches bolt A");
    yield return Walk(swinger,W(.12f,-.30f,-.25f),"Swinger goes round the front of the blocks");
    yield return Walk(swinger,W(.22f,-.30f,-.04f),"Swinger at the foot of the bank's climbing face, on the open rescue floor");
    game.SelectFragment(swinger);yield return Grip(swing);yield return Swing(swing,0);
    yield return Operate("B");yield return until(8,()=>Task("B").Rail.AtEnd&&gate.AtEnd,"Latch B catches bolt B and opens the ramp home");
    // Both holders are free. The swinger comes home down the ramp and pairs with pad B's holder; that 50 % pair sets
    // the far block C, then the near block D. The tube worker returns through the tube; then all four merge.
    game.SelectFragment(tubeWorker);yield return EnterTube(tube,1);yield return LeaveTube(tube);
    yield return Command(swinger,W(.285f,-.30f,-.05f));yield return Command(holderB,W(.285f,-.30f,-.05f));
    yield return until(40,()=>game.Matter.Groups[swinger]==game.Matter.Groups[holderB],"Right pair merges");
    game.SelectFragment(holderB);yield return Operate("C");yield return until(5,()=>Task("C").Rail.AtEnd,"Far block C goes in first");
    game.SelectFragment(holderB);yield return Operate("D");yield return until(5,()=>Task("D").Rail.AtEnd,"Near block D follows");
    yield return Merge(W(.14f,-.30f,-.22f));
    yield return Operate("E",60);yield return until(15,()=>Array.TrueForAll(spans,s=>s.AtEnd),"Whole body winds E; both spans rise and latch");
    yield return Go(W(-.02f,-.21f,.26f),"Across the spans to the exit strip");
   }
   else throw new NotImplementedException("Spatial "+n+" route");
   yield return Exit();
  }
 }
}
#endif
