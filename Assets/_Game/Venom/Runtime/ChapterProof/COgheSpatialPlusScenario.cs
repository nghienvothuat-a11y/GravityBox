#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GravityBox.Venom.ChapterProof
{
 // Author routes for Spatial Plus (E01–E18, B1, B2). Every movement, task, split and exit goes through a visible screen
 // tap (fragment selection uses the public selection API). No pose, topology, gate or win state is assigned.
 public sealed class COgheSpatialPlusScenario
 {
  readonly VenomCampaign game;readonly Func<Vector3,IEnumerator> tap;readonly Func<float,Func<bool>,string,IEnumerator> until;readonly COgheSpatialNextScenario s;
  public COgheSpatialPlusScenario(VenomCampaign g,Func<Vector3,IEnumerator> t,Func<float,Func<bool>,string,IEnumerator> u,Func<float,IEnumerator> o=null){game=g;tap=t;until=u;s=new COgheSpatialNextScenario(g,t,u,o);}
  Vector3 W(float x,float y,float z)=>game.Root.TransformPoint(new Vector3(x,y,z));
  int Selected=>game.Motion.Selected;
  T Find<T>() where T:Component=>s.Find<T>();
  COgheTubeNetwork Tube(string name)=>Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>(),t=>t.name==name)??throw new InvalidOperationException("Missing tube "+name);
  COgheSwingTransfer Rope(string label)=>Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheSwingTransfer>(),r=>r.Label==label)??throw new InvalidOperationException("Missing rope "+label);
  COgheGearTrain Train(string name)=>Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheGearTrain>(),t=>t.name==name)??throw new InvalidOperationException("Missing train "+name);
  VenomMovableProp Prop(string name)=>Array.Find(game.Props,p=>p.name==name)??throw new InvalidOperationException("Missing prop "+name);
  // Operate a handle and wait for its rail to reach the end.
  IEnumerator Pull(string label,string why,float seconds=40){yield return s.Operate(label,seconds);var t=s.Task(label);yield return until(8,()=>t.Rail.AtEnd,why);}

  public IEnumerator Solve()
  {
   switch(COgheSpatialNextScenario.ContentKey(game))
   {
    case "E01":yield return E01();break;case "E02":yield return E02();break;case "E03":yield return E03();break;case "E04":yield return E04();break;
    case "E05":yield return E05();break;case "E06":yield return E06();break;case "E07":yield return E07();break;case "B1":yield return B1();break;
    case "E08":yield return E08();break;case "E09":yield return E09();break;case "E10":yield return E10();break;case "E11":yield return E11();break;
    case "E12":yield return E12();break;case "E13":yield return E13();break;case "E14":yield return E14();break;case "E15":yield return E15();break;
    case "E16":yield return E16();break;case "E17":yield return E17();break;case "E18":yield return E18();break;case "B2":yield return B2();break;
    case "K01":case "K02":case "K03":case "K04":case "K05":case "K06":case "K07":case "K08":case "K09":case "K10":yield return Crates(COgheSpatialNextScenario.ContentKey(game));break;
    case "N22":yield return N22();break;case "N29":yield return N29();break;case "N26":yield return N26();break;case "N25":yield return N25();break;case "N23":yield return N23();break;
    case "N13":yield return N13();break;case "N15":yield return N15();break;case "N18":yield return N18();break;case "N19":yield return N19();break;
    case "N31":yield return N31();break;case "N33":yield return N33();break;case "N35":yield return N35();break;case "N32":yield return N32();break;case "N40":yield return N40();break;case "N34":yield return N34();break;case "N44":yield return N44();break;case "N45":yield return N45();break;
    case "N41":yield return N41();break;case "N42":yield return N42();break;case "N47":yield return N47();break;case "N49":yield return N49();break;case "N50":yield return N50();break;case "N48":yield return N48();break;case "N46":yield return N46();break;case "N43":yield return N43();break;
    default:throw new NotImplementedException("Spatial Plus route "+COgheSpatialNextScenario.ContentKey(game));
   }
   yield return s.Exit();
  }

  IEnumerator E01()
  {
   var lift=Find<COghePassengerLift>();var crate=Prop("A crate");var socket=Find<COghePropSocket>();
   yield return tap(lift.Panel.position);yield return until(45,()=>lift.Trips==1&&lift.Rail.AtEnd,"Board beside the crate and ride up");
   // Step back to the far corner of the tray first: grasping starts from a clear approach, not from against the crate.
   yield return s.Go(W(.075f,-.07f,.22f),"Step back on the tray",.06f);
   // Grasp where the crate shows: at some framings the body on the tray covers the middle of its top.
   foreach(var o in new[]{Vector3.zero,new Vector3(-.035f,0,0),new Vector3(0,0,.035f),new Vector3(.035f,0,0),new Vector3(0,0,-.035f)})
   {
    if(game.Attached)break;
    yield return tap(crate.Body.position+crate.transform.TransformDirection(o)+Vector3.up*.012f);
    float t=game.Matter.SimulationTime;yield return until(20,()=>game.Attached||game.Matter.SimulationTime-t>5,"Grasp the crate");
   }
   for(int attempt=0;attempt<3&&!socket.Seated;attempt++){yield return s.Push(crate,W(-.214f,-.0565f,.16f));float t0=game.Matter.SimulationTime;yield return until(8,()=>socket.Seated||game.Matter.SimulationTime-t0>3,"Crate rests on the socket");}
   yield return until(6,()=>socket.Seated,"Socket pawl catches the crate");
   yield return s.Go(W(-.33f,-.01f,.18f),"Climb the crate to the exit bench");
  }
  IEnumerator E02()
  {
   var up=Tube("Balcony tube");var exit=Tube("Exit tube");
   yield return s.EnterTube(up,0);yield return s.LeaveTube(up);
   yield return s.Operate("A");yield return until(8,()=>exit.IsEntryOpen(0),"A lifts the cap of the exit tube");
   yield return s.EnterTube(up,1);yield return s.LeaveTube(up);
   yield return s.EnterTube(exit,0);yield return s.LeaveTube(exit);
   // The outlet pane is slick: approach it over the floor below, not along the glass from the tube's mouth.
   yield return s.Go(W(.34f,-.30f,.12f),"Floor under the exit");
  }
  IEnumerator E03()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("A door");
   yield return s.Split(q,Selected);int holder=q.LastLeft,worker=q.LastRight;
   yield return s.Walk(holder,W(-.32f,-.298f,-.20f),"Holder loads pad A");
   yield return until(10,()=>door.AtEnd,"Pad A raises the door");
   yield return s.Walk(worker,W(.16f,-.30f,-.20f),"Through the door");
   game.SelectFragment(worker);yield return Pull("B","B latches the door open");
   yield return s.Walk(holder,W(.14f,-.30f,-.22f),"Holder leaves A and follows");
   yield return s.Merge(W(.24f,-.30f,.02f));
  }
  // N13: the holder pulls the spring block B into its bay and holds it; the pusher slides A to the island; letting go,
  // B springs back across the lane as the low step. Merged, the body climbs B, A, the island.
  IEnumerator N13()
  {
   var q=Find<COgheQuantumSplitter>();var b=s.Task("B");var a=s.Task("A");
   yield return s.Split(q,Selected);int holder=q.LastLeft,pusher=q.LastRight;
   game.SelectFragment(holder);
   for(int attempt=0;attempt<3&&!b.Busy;attempt++){yield return tap(b.HandPoint+Vector3.up*.004f);if(!b.Busy){yield return tap(b.StandPoint);yield return until(20,()=>Vector3.Distance(game.Motion.Centre(holder),b.StandPoint)<.06f,"Holder beside the spring block");}}
   yield return until(40,()=>b.Holding&&b.Rail.AtEnd,"The holder pulls B into its bay and holds it");
   game.SelectFragment(pusher);yield return Pull("A","The pusher slides the tall block to the island");
   yield return s.Walk(holder,W(-.22f,-.30f,-.06f),"The holder lets go");
   yield return until(15,()=>b.Rail.Position<=b.Rail.CatchTolerance,"B springs back beside the tall block");
   yield return s.Merge(W(-.12f,-.30f,-.02f));
   yield return s.Go(W(-.028f,-.27f,.14f),"Mount the low block");
   yield return s.Go(W(.096f,-.24f,.14f),"Mount the tall block");
   yield return s.Go(W(.27f,-.21f,.20f),"Reach the exit island");
  }
  // Crate levels: the shortest route (Tools/crate_puzzles/logic.py), played as a player does since 07/10/2026 (Mrk): a tap
  // on the crate's face at the end COghe works from (behind it to push, the face it moves toward to pull: the route's floor
  // cell); COghe walks there itself and the crate slides to its other stop. Last, the red crate leaves the exit.
  IEnumerator Crates(string key)
  {
   var tasks=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();
   COgheTapRail Crate(int i)=>Array.Find(tasks,t=>t.Rail.name==(i==0?"Red crate":"Crate "+i))??throw new InvalidOperationException("Missing crate "+i);
   Vector3 Cell(int x,int z)=>W(COgheCrateRoutes.GridX+(x+.5f)*COgheCrateRoutes.Cell,-.30f,COgheCrateRoutes.GridZ+(z+.5f)*COgheCrateRoutes.Cell);
   int n=0;
   foreach(var (index,x,z) in COgheCrateRoutes.Steps[key])
   {
    n++;var t=Crate(index);float goal=t.Rail.Position<=t.Rail.CatchTolerance?t.Rail.Travel:0;int before=t.CompletedJourneys;
    // Done when the pull itself has finished (the crate seated at its stop and COghe let go), so the next tap is not
    // swallowed by a pull still closing its last millimetres. The red crate is the last pull: once the exit is open,
    // COghe may already be dropping through it.
    bool Done()=>t.CompletedJourneys>before||index==0&&(game.Owner.Completed||game.Root.InverseTransformPoint(game.Motion.Centre(0)).y<-.32f||!t.Busy&&Mathf.Abs(t.Rail.Position-goal)<.012f);
    for(int attempt=0;attempt<3&&!Done();attempt++)
    {
     // The face COghe works from is the side of the crate its route cell is on: an end (push or pull) or a long side
     // (grip and walk along, Mrk 08/10/2026). Tap the crate's top near that face's edge, else the face itself, only where
     // the camera's ray meets this crate first.
     Vector3 body=t.Rail.Body.position,me=game.Motion.Centre(Selected),along=t.Rail.WorldAxis,up=game.Root.up,across=Vector3.Cross(up,along).normalized;
     Vector3 size=t.CrateSize,toCell=Cell(x,z)-body;
     float Half(Vector3 d){var l=t.Rail.Frame.InverseTransformDirection(d);return Mathf.Abs(l.x)*size.x*.5f+Mathf.Abs(l.z)*size.z*.5f;}
     float onAxis=Vector3.Dot(toCell,along);bool byEnd=Mathf.Abs(onAxis)>=Half(along)-.01f;
     Vector3 face=byEnd?along*Mathf.Sign(onAxis):across*Mathf.Sign(Vector3.Dot(toCell,across));float half=Half(face);
     var spots=new[]{body+face*half*.78f+up*size.y*.5f,body+face*half*.66f+up*size.y*.5f,body+face*half+up*size.y*.1f};
     bool Hits(Vector3 p){var cam=game.Owner.View;var r=cam.ScreenPointToRay(cam.WorldToScreenPoint(p));
      foreach(var h in System.Linq.Enumerable.OrderBy(Physics.RaycastAll(r,10),h=>h.distance)){var pane=h.collider.GetComponent<VenomSurfacePatch>();if(h.collider.isTrigger||h.collider.GetComponent<VenomContact>()!=null||pane!=null&&pane.ExteriorGlass)continue;return h.rigidbody==t.Rail.Body;}return false;}   // a tap passes through the glass box
     for(int k=0;k<spots.Length&&!t.Busy;k++)if(Hits(spots[k]))yield return this.tap(spots[k]);
     if(!t.Busy)
     {
      // Diagnose: what does a ray from the camera to the crate's middle hit first?
      var cam=game.Owner.View;var ray=cam.ScreenPointToRay(cam.WorldToScreenPoint(body+Vector3.up*.02f));
      var hits=Physics.RaycastAll(ray,10);System.Array.Sort(hits,(a,b2)=>a.distance.CompareTo(b2.distance));
      string first=string.Join(" | ",System.Array.ConvertAll(hits,h=>$"{h.collider.name}@{h.distance:F3}{(h.collider.isTrigger?"(trig)":"")}"));
      string rails=string.Join(" ",System.Array.ConvertAll(tasks,r=>$"{r.Rail.name}:{r.Phase}/{(r.Busy?"busy":"-")}/pos{r.Rail.Position:F4}/lat{r.Rail.Latched}"));
      bool direct=t.TryTouch(game,ray,100f);
      first+=$" | canControl={game.Owner.CanControl} selected={Selected} home={game.Home} rails: {rails}";
      throw new InvalidOperationException($"Crate {index} refused pull {n}: {t.LastFailure} {t.LastRefusal}; direct={direct} busy={t.Busy} body={game.Root.InverseTransformPoint(body):F3} me={game.Root.InverseTransformPoint(me):F3} hits: {first}");
     }
     // A pull that loses its footing stops; the player taps again from beside the crate.
     yield return until(40,()=>Done()||!t.Busy,$"Pull {n}: crate {index} slides");
     // A pull that opens the exit can end with COghe dropping into it: give the fall a moment before trying again.
     if(index==0&&!Done()){float t0=game.Matter.SimulationTime;yield return until(5,()=>Done()||game.Matter.SimulationTime-t0>2,"settle");}
    }
    yield return until(5,Done,$"Pull {n}: crate {index} reaches its other stop");
   }
  }
  // N15: push the step through the slot from this side first, then take the tube; climb the step to the shelf.
  IEnumerator N15()
  {
   var tube=Find<COgheTubeNetwork>();var step=s.Slider("A step");
   yield return Pull("A","Handle A pushes the step through the slot");
   yield return until(10,()=>step.AtEnd,"The step docks against the shelf");
   yield return s.EnterTube(tube,0);yield return s.LeaveTube(tube);
   yield return s.Go(W(.196f,-.27f,.14f),"Onto the step");
   yield return s.Go(W(.33f,-.24f,.14f),"Onto the shelf");
  }
  // N18: as E03, but first the worker tries the 100% crate and gives up (too heavy for a half); after the door is
  // latched and the halves merge, the whole body pushes it to the shelf and climbs.
  IEnumerator N18()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("A door");var crate=s.Task("C");
   yield return s.Split(q,Selected);int holder=q.LastLeft,worker=q.LastRight;
   yield return s.Walk(holder,W(-.32f,-.298f,-.20f),"Holder loads pad A");
   yield return until(10,()=>door.AtEnd,"Pad A raises the door");
   yield return s.Walk(worker,W(.16f,-.30f,-.20f),"Through the door");
   int gave=crate.GaveUp;game.SelectFragment(worker);
   for(int attempt=0;attempt<3&&!crate.Busy;attempt++){yield return tap(crate.HandPoint+Vector3.up*.004f);if(!crate.Busy){yield return tap(crate.StandPoint);yield return until(20,()=>Vector3.Distance(game.Motion.Centre(worker),crate.StandPoint)<.06f,"Worker beside the crate");}}
   yield return until(30,()=>crate.GaveUp>gave,"Half a body strains at the 100% crate and lets go");
   if(crate.Rail.Position>.01f)throw new InvalidOperationException("Half a body moved the 100% crate");
   game.SelectFragment(worker);yield return Pull("B","B latches the door open");
   yield return s.Walk(holder,W(.14f,-.30f,-.22f),"Holder leaves A and follows");
   yield return s.Merge(W(.18f,-.30f,-.12f));
   yield return Pull("C","The whole body pushes the crate against the shelf");
   yield return s.Go(W(.31f,-.24f,.03f),"Climb the crate");
   yield return s.Go(W(.31f,-.21f,.20f),"Onto the shelf");
  }
  // N19: E03's door, then the freed holder's second job: hold spring handle C (bolt C out of D's lane) while the
  // other half pushes D to the shelf. Released, the bolt springs back; the holder follows, they merge and climb.
  IEnumerator N19()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("A door");var c=s.Task("C");var bolt=s.Slider("C bolt");
   yield return s.Split(q,Selected);int holder=q.LastRight,worker=q.LastLeft;
   yield return s.Walk(holder,W(.32f,-.298f,-.20f),"Holder loads pad A");
   yield return until(10,()=>door.AtEnd,"Pad A raises the door");
   yield return s.Walk(worker,W(-.16f,-.30f,-.20f),"Through the door");
   game.SelectFragment(worker);yield return Pull("B","B latches the door open");
   // The freed holder steps off pad A to spring handle C, then takes it.
   yield return s.Walk(holder,W(.09f,-.30f,-.19f),"The freed holder walks to spring handle C");
   game.SelectFragment(holder);
   for(int attempt=0;attempt<3&&!c.Busy;attempt++){game.SelectFragment(holder);yield return tap(c.HandPoint+Vector3.up*.004f);}
   if(!c.Busy)throw new InvalidOperationException("Spring handle C rejected: "+c.LastFailure);
   yield return until(40,()=>c.Holding&&bolt.AtEnd,"The freed holder holds C: the bolt is drawn out of D's lane");
   game.SelectFragment(worker);yield return Pull("D","The other half pushes D against the shelf");
   yield return s.Walk(holder,W(-.14f,-.30f,-.22f),"The holder lets go and follows through the door");
   yield return until(10,()=>bolt.Position<=bolt.CatchTolerance,"Bolt C springs back");
   yield return s.Merge(W(-.12f,-.30f,-.22f));
   yield return s.Go(W(-.31f,-.24f,.03f),"Climb D");
   yield return s.Go(W(-.31f,-.21f,.20f),"Onto the shelf");
  }
  IEnumerator E04()
  {
   yield return Pull("A","Cart A against the shelf");
   yield return Pull("B","Crate B across the cart beside the shelf");
   yield return s.Go(W(.05f,-.18f,.18f),"Climb the crate");
   yield return s.Go(W(.26f,-.18f,.17f),"Onto the exit shelf");
  }
  // N22: load first. B across the cart while the cart is still away; then the cart to the shelf; climb the crate.
  IEnumerator N22()
  {
   yield return Pull("B","Crate B across the cart before it sails");
   yield return Pull("A","Cart A, loaded, against the shelf");
   yield return s.Go(W(.05f,-.18f,.18f),"Climb the crate");
   yield return s.Go(W(.26f,-.18f,.17f),"Onto the exit shelf");
  }
  // N29: top tier first. Climb A where it stands, push B to A's far end, then pull A over and climb the stack.
  IEnumerator N29()
  {
   yield return s.Go(W(-.30f,-.21f,.14f),"Climb block A at its start");
   yield return Pull("B","Block B to A's far end");
   yield return Pull("A","Block A, with B, against the shelf");
   yield return s.Go(W(-.10f,-.21f,.14f),"Climb block A");
   yield return s.Go(W(.07f,-.12f,.14f),"Climb block B");
   yield return s.Go(W(.26f,-.12f,.17f),"Onto the shelf");
  }
  // N26: split, one half on each pad A: the door opens and stays open. Half a body past the axle: the heavy plank only
  // creaks. Back, merge, and the whole body tips it down into the exit room.
  // N23: lever B, far at the front-right, brings the landing into the rope's arc; up the stairs, grip, swing across.
  IEnumerator N23()
  {
   var swing=Find<COgheSwingTransfer>();var tray=s.Slider("B landing tray");
   yield return s.Operate("B");yield return until(10,()=>tray.AtEnd,"B brings the landing tray into the arc");
   yield return s.Go(W(-.28f,-.16f,.05f),"Climb the stairs onto the start bank");
   yield return s.Grip(swing);
   yield return s.Swing(swing,1);
  }
  // N25: the light crate onto the tray lifts the plank only part way; COghe climbs onto the loaded tray, the plank comes
  // level and the pawl catches; out of the pit and across.
  IEnumerator N25()
  {
   var seesaw=Find<COgheSeesawBridge>();var crate=Prop("A crate");
   yield return s.Push(crate,W(-.28f,-.187f,.075f),.012f);
   float t0=game.Matter.SimulationTime;yield return until(8,()=>game.Matter.SimulationTime-t0>4,"The crate's weight settles");
   Debug.Log($"N25 crate alone: plank {seesaw.AngleToLevel:F1} degrees from level, caught={seesaw.Caught}, tension={seesaw.Tension:F3}");
   if(seesaw.Caught)throw new InvalidOperationException("The crate alone levelled the plank");
   yield return s.Go(W(-.28f,-.215f,.20f),"COghe climbs onto the far end of the loaded tray",.08f);
   yield return until(20,()=>seesaw.Caught,"With COghe on the tray the plank comes level; the pawl catches");
   yield return s.Go(W(-.19f,-.20f,.10f),"Out of the pit");
   yield return s.Go(W(0,-.20f,.10f),"Onto the level plank");
   yield return s.Go(W(.25f,-.20f,.12f),"Across to the exit platform");
  }
  IEnumerator N26()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("A door");var plank=Prop("B seesaw plank");
   bool Tipped()=>plank.transform.TransformPoint(Vector3.right*.17f).y<plank.transform.position.y;
   yield return s.Split(q,Selected);int one=q.LastLeft,two=q.LastRight;
   yield return s.Walk(one,W(-.34f,-.298f,-.21f),"One half on the front pad A");
   // Round the Q by the partition and the back wall (a straight tap sends it over the Q's slick casing).
   yield return s.Walk(two,W(-.06f,-.30f,.20f),"The other half along the partition");
   yield return s.Walk(two,W(-.34f,-.298f,.21f),"The other half on the back pad A");
   yield return until(10,()=>door.AtEnd,"Both pads open the door");
   yield return s.Walk(one,W(.08f,-.30f,-.20f),"Through the door");
   yield return s.Walk(one,plank.transform.TransformPoint(new Vector3(-.12f,.011f,0)),"Half a body onto the plank's foot",.07f);
   yield return s.Walk(one,plank.transform.TransformPoint(new Vector3(.10f,.011f,0)),"Half a body past the axle",.08f);
   float t0=game.Matter.SimulationTime;yield return until(5,()=>game.Matter.SimulationTime-t0>2||Tipped(),"The plank creaks");
   if(Tipped())throw new InvalidOperationException("Half a body tipped the heavy plank");
   yield return s.Walk(one,W(.08f,-.30f,-.10f),"Back off the plank");
   yield return s.Walk(two,W(.08f,-.30f,-.22f),"The other half through the door");
   yield return s.Merge(W(.08f,-.30f,-.15f));
   yield return s.Go(plank.transform.TransformPoint(new Vector3(-.12f,.011f,0)),"The whole body onto the plank's foot",.07f);
   yield return tap(plank.transform.TransformPoint(new Vector3(.12f,.011f,0)));
   yield return until(25,Tipped,"The whole body tips the plank");
   // Tipped, the plank can carry the body straight down and out through the low exit.
   float t1=game.Matter.SimulationTime;yield return until(5,()=>game.Owner.Completed||game.Matter.SimulationTime-t1>1.5f,"Down the plank");
   if(!game.Owner.Completed&&game.Root.InverseTransformPoint(game.Motion.Centre(0)).x<.40f)
   {
    // The body may roll on through the low exit while walking down: done either way.
    var foot=W(.33f,-.30f,-.06f);yield return tap(foot);
    yield return until(15,()=>game.Owner.Completed||game.Root.InverseTransformPoint(game.Motion.Centre(0)).x>.40f||Vector3.Distance(game.Motion.Centre(0),foot+Vector3.up*.02f)<.08f,"Down the plank into the exit room");
   }
  }
  string Gauges()=>string.Join(", ",Array.ConvertAll(game.Owner.Apparatus.GetComponentsInChildren<COgheTissueSensor>(),
   p=>$"{p.name.Split(' ')[0]}@{p.transform.localPosition.x:F2},{p.transform.localPosition.z:F2}={p.Load:F3}/{p.Threshold:F3} lit {(p.GaugeTiles==null?0:Array.FindAll(p.GaugeTiles,t=>t.enabled).Length)}/{p.GaugeTiles?.Length??0}"));
  // N31: split; the half takes the pad at the door (two tiles), the other half splits again and its quarters take the two
  // back pads (one tile each). The door opens for good; all merge and go through.
  IEnumerator N31()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("A door");
   yield return s.Split(q,Selected);int half=q.LastRight,other=q.LastLeft;
   yield return s.Walk(half,W(-.06f,-.298f,-.21f),"Half onto the pad at the door");
   yield return s.Split(q,other);int q1=q.LastLeft,q2=q.LastRight;
   yield return s.Walk(q1,W(-.34f,-.298f,.21f),"Quarter onto the back-left pad");
   yield return s.Walk(q2,W(-.06f,-.298f,.21f),"Quarter onto the back-right pad");
   yield return until(10,()=>door.AtEnd,"Three loaded pads open the door");
   Debug.Log("N31 GAUGES "+Gauges());
   yield return s.Merge(W(-.04f,-.30f,-.11f));
   yield return s.Go(W(.20f,-.30f,-.10f),"Through the door");
  }
  // N32: the whole body pushes heavy B aside first; then split: one half on pad A (A's bolt drawn), the other pushes A to its
  // parking place; merged, the whole body pushes B back into the crossing and climbs B, A, the island.
  IEnumerator N32()
  {
   var q=Find<COgheQuantumSplitter>();var a=s.Task("A");var b=s.Task("B");var bolt=s.Slider("A lock bolt");
   yield return s.Operate("B");yield return until(8,()=>b.Rail.Position<=b.Rail.CatchTolerance,"The whole body pushes heavy B aside");
   yield return s.Split(q,Selected);int holder=q.LastLeft,worker=q.LastRight;
   yield return s.Walk(holder,W(-.33f,-.298f,-.03f),"Half onto pad A");
   yield return until(10,()=>bolt.AtEnd,"Pad A draws A's bolt");
   game.SelectFragment(worker);yield return Pull("A","Half a body pushes A to its parking place");
   yield return s.Merge(W(-.12f,-.30f,-.02f));
   yield return Pull("B","The whole body pushes B back into the crossing");
   yield return s.Go(W(-.028f,-.27f,.14f),"Mount the low block");
   yield return s.Go(W(.096f,-.24f,.14f),"Mount the tall block");
   yield return s.Go(W(.27f,-.21f,.20f),"Reach the exit island");
  }
  // N40 (boss): the whole body on the pan is too heavy; split; half holds door pad A; the other half splits, a quarter walks
  // to pad B in the side room (axle pin drawn); the half alone on the pan is too light; the last quarter joins it: three
  // quarters level the beam, the exit step slides out and the door stays open; all merge in the side room and climb.
  IEnumerator N40()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("A door");var shutter=s.Slider("Step bridge");
   var scale=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTissueSensor>(),p=>p.Column>0);
   Vector3 pan=W(-.30f,-.298f,.20f);
   yield return s.Go(pan,"The whole body onto the pan");
   float t0=game.Matter.SimulationTime;yield return until(4,()=>game.Matter.SimulationTime-t0>1.5f,"The scale weighs the whole body");
   Debug.Log("N40 GAUGES whole on the pan: "+Gauges());
   if(!scale.TooHeavy)throw new InvalidOperationException("The whole body did not tip the scale too far");
   yield return s.Go(W(-.18f,-.30f,-.16f),"Back round to Q's mouth"); // from behind, a route into Q runs up a tray lane and stalls
   yield return s.Split(q,Selected);int half=q.LastLeft,other=q.LastRight;
   yield return s.Walk(half,W(-.32f,-.298f,-.20f),"Half onto door pad A");
   yield return until(10,()=>door.AtEnd,"Half a body raises the door");
   yield return s.Split(q,other);int q1=q.LastRight,q2=q.LastLeft;
   yield return s.Walk(q1,W(.32f,-.298f,-.22f),"A quarter to pad B in the side room");
   yield return s.Walk(q2,W(-.12f,-.30f,.22f),"The other quarter waits clear of the pan");
   yield return s.Walk(half,pan,"The half leaves A for the pan");
   t0=game.Matter.SimulationTime;yield return until(4,()=>game.Matter.SimulationTime-t0>1.5f,"The scale weighs half a body");
   Debug.Log("N40 GAUGES half on the pan: "+Gauges());
   if(scale.Active)throw new InvalidOperationException("Half a body balanced the scale");
   yield return s.MergeParts(new List<int>{q2,half},pan,2);
   t0=game.Matter.SimulationTime;yield return until(8,()=>shutter.AtEnd||game.Matter.SimulationTime-t0>4,"Three quarters settle on the pan");
   Debug.Log("N40 GAUGES merged by the pan: "+Gauges());
   // Merged off-centre: nudge the part onto the pan (a tap on the part itself would only select it).
   if(!shutter.AtEnd)yield return s.CommandAny(half,new List<Vector3>{W(-.30f,-.298f,.23f),W(-.33f,-.298f,.20f),W(-.30f,-.298f,.17f)});
   yield return until(15,()=>shutter.AtEnd,"Three quarters level the beam; the exit step slides out");
   Debug.Log("N40 GAUGES three quarters on the pan: "+Gauges());
   yield return until(10,()=>door.AtEnd,"The door stays open");
   yield return s.Walk(half,W(.16f,-.30f,-.20f),"Three quarters through the door");
   yield return s.Merge(W(.24f,-.30f,-.06f));
   yield return s.Go(W(.23f,-.27f,.08f),"Onto the exit step");
   yield return s.Go(W(.23f,-.24f,.22f),"Onto the exit platform");
  }
  // N34: half with half balances (nothing moves); then a quarter rides the lift, the half on the counter tray lifts it to
  // the bank; it pulls B (exit step out), steps back on the raised tray and rides down when the counterweight steps off.
  IEnumerator N34()
  {
   var q=Find<COgheQuantumSplitter>();var lift=Find<COgheBalanceLift>();var bridge=s.Slider("Step bridge");
   Vector3 tray=W(.06f,-.286f,-.16f),counter=W(-.12f,-.298f,-.16f);
   yield return s.Split(q,Selected);int weight=q.LastLeft,other=q.LastRight;
   yield return s.Walk(other,tray,"Half onto the lift tray");
   yield return s.Walk(weight,counter,"Half onto the counter tray");
   float t0=game.Matter.SimulationTime;yield return until(5,()=>game.Matter.SimulationTime-t0>2,"Half with half");
   Debug.Log($"N34 BALANCE half/half counter={lift.CounterLoad.Load:F3} rider={lift.RisingLoad.Load:F3} lift={lift.Rising.Position:F3}");
   if(lift.Rising.Position>.01f)throw new InvalidOperationException("Half lifted half");
   yield return s.Walk(other,W(.06f,-.30f,.04f),"The other half steps off the tray, backwards");
   yield return s.Walk(weight,W(-.33f,-.30f,-.25f),"The counterweight steps off");
   yield return s.Walk(other,W(-.22f,-.30f,-.02f),"The other half back to Q");
   yield return s.Split(q,other);int q1=q.LastRight,q2=q.LastLeft;
   yield return s.Command(q1,tray);yield return until(20,()=>lift.RisingLoad.Load>=.02f,"A quarter onto the lift tray");
   yield return s.Walk(weight,counter,"Half onto the counter tray");
   yield return until(10,()=>lift.Rising.AtEnd,"Half a body lifts the quarter to the bank");
   Debug.Log($"N34 BALANCE half/quarter counter={lift.CounterLoad.Load:F3} rider={lift.RisingLoad.Load:F3} lift={lift.Rising.Position:F3}");
   yield return s.Walk(q1,W(.19f,-.18f,-.23f),"The quarter onto the bank");
   game.SelectFragment(q1);yield return Pull("B","The quarter pulls B home");
   yield return until(10,()=>bridge.AtEnd,"B slides the exit step out");
   yield return s.Walk(q1,W(.06f,-.178f,-.16f),"Back onto the raised tray");
   yield return s.Walk(weight,W(-.33f,-.30f,-.25f),"The counterweight steps off");
   yield return until(10,()=>lift.Rising.Position<.01f,"The quarter rides down");
   yield return s.Merge(W(-.05f,-.30f,.0f));
   yield return s.Go(W(.23f,-.27f,.08f),"Onto the exit step");
   yield return s.Go(W(.23f,-.24f,.22f),"Onto the exit platform");
  }
  // N44: on P with straight gear A in, the step runs back; draw A out, push the idler pair B in; on P the step runs out.
  IEnumerator N44()
  {
   var bridge=s.Slider("Step bridge");var a=s.Task("A");Vector3 pad=W(-.32f,-.298f,-.16f);
   yield return s.Go(pad,"Onto P");
   yield return until(10,()=>bridge.Position<=bridge.CatchTolerance,"Three wheels: the step runs back in");
   yield return s.Operate("A");yield return until(8,()=>a.Rail.Position<=a.Rail.CatchTolerance,"Draw straight gear A out");
   yield return Pull("B","Push the idler pair in");
   yield return s.Go(pad,"Back onto P");
   yield return until(15,()=>bridge.AtEnd,"Four wheels: the step runs out");
   yield return s.Go(W(.20f,-.27f,.07f),"Onto the step");
   yield return s.Go(W(.20f,-.24f,.21f),"Onto the exit platform");
  }
  // N45: half drives half: the lift stalls half way. The driver steps off (the lift comes down), the rider splits at Q:
  // a quarter rides, the other quarter joins the driver on P (75 %): the lift reaches the top. Then as E15.
  IEnumerator N45()
  {
   var q=Find<COgheQuantumSplitter>();var lift=s.Slider("Gear lift");var top=s.Slider("C top step");Vector3 pad=W(-.30f,-.298f,-.20f);
   yield return s.Split(q,Selected);int driver=q.LastLeft,rider=q.LastRight;
   yield return s.Walk(rider,W(.13f,-.30f,-.02f),"Rider steps wide of Q");
   yield return s.Walk(rider,W(-.01f,-.288f,.16f),"Rider onto the lift");
   yield return s.Walk(driver,pad,"Driver onto pad P");
   float t0=game.Matter.SimulationTime;yield return until(15,()=>game.Matter.SimulationTime-t0>6,"Half drives half");
   Debug.Log($"N45 LIFT half/half pos={lift.Position:F3} {Gauges()}");
   if(lift.AtEnd||lift.Position<.02f)throw new InvalidOperationException($"Half a body should lift half a body part way: {lift.Position:F3}");
   yield return s.Walk(driver,W(-.30f,-.30f,-.06f),"The driver steps off P");
   yield return until(20,()=>lift.Position<.01f,"The lift comes back down");
   yield return s.Walk(rider,W(.13f,-.30f,-.02f),"The rider round Q");
   yield return s.Walk(rider,W(.10f,-.30f,-.27f),"The rider to Q's mouth");
   yield return s.Split(q,rider);int r1=q.LastRight,r2=q.LastLeft;
   yield return s.Walk(r1,W(-.01f,-.288f,.16f),"A quarter onto the lift");
   yield return s.MergeParts(new List<int>{r2,driver},pad,2);
   yield return s.Walk(driver,pad,"Three quarters on P");
   yield return until(30,()=>lift.AtEnd,"Three quarters lift a quarter to the top");
   Debug.Log($"N45 LIFT 75/25 pos={lift.Position:F3} {Gauges()}");
   yield return s.Walk(r1,W(.18f,-.12f,.20f),"The quarter onto the high deck");
   game.SelectFragment(r1);yield return s.Operate("C");yield return until(10,()=>top.AtEnd,"C raises the top stair step");
   yield return s.Walk(driver,W(.25f,-.24f,-.07f),"The driver leaves P for the stair");
   yield return s.Walk(driver,W(.25f,-.12f,.045f),"The driver climbs to the high deck");
   yield return s.Merge(W(.25f,-.12f,.20f));
  }
  // N41: on P with the gap open only the motor turns and the lamp stays dark; A brings G into the ring (the lamp lights);
  // back on P the step runs out.
  IEnumerator N41()
  {
   var drawer=s.Slider("Step bridge");var train=Find<COgheGearTrain>();Vector3 pad=W(-.32f,-.298f,-.16f);
   yield return s.Go(pad,"Onto P");
   float t0=game.Matter.SimulationTime;yield return until(5,()=>game.Matter.SimulationTime-t0>1.5f,"Only the motor turns");
   if(train.Meshed||drawer.Position>.005f)throw new InvalidOperationException("The train ran with the gap open");
   yield return Pull("A","Gear G into the ring");
   yield return until(5,()=>train.Meshed,"Every gear meshes");
   if(train.MeshLamp.sharedMaterial!=train.MeshLampOn)throw new InvalidOperationException("The mesh lamp did not light");
   yield return s.Go(pad,"Back onto P");
   yield return until(20,()=>drawer.AtEnd,"The train draws the step out");
   yield return s.Go(W(.20f,-.27f,.07f),"Onto the step");yield return s.Go(W(.20f,-.24f,.21f),"Onto the exit platform");
  }
  // N42: B's gear sits in A's lane, so A refuses. Draw B out, push A in (its gear passes B's slot), push B back; P.
  IEnumerator N42()
  {
   var a=s.Task("A");var b=s.Task("B");var drawer=s.Slider("Step bridge");var train=Find<COgheGearTrain>();
   yield return tap(a.HandPoint+Vector3.up*.004f);
   if(a.Busy)throw new InvalidOperationException("A set off with B's gear in its lane");
   Debug.Log("N42 A refused: "+a.LastFailure);
   yield return s.Operate("B");yield return until(8,()=>b.Rail.Position<=b.Rail.CatchTolerance,"Draw B out of A's lane");
   yield return Pull("A","A's gear through B's slot into its own");
   yield return Pull("B","B back into its slot");
   yield return until(5,()=>train.Meshed,"Every gear meshes");
   yield return s.Go(W(-.33f,-.298f,-.20f),"Onto P");
   yield return until(20,()=>drawer.AtEnd,"The train draws the step out");
   yield return s.Go(W(.20f,-.27f,.07f),"Onto the step");yield return s.Go(W(.20f,-.24f,.21f),"Onto the exit platform");
  }
  // N47: G refuses (the gate is down). P1: machine 1 lifts the gate, which latches. G to machine 2; P2: the step runs out.
  IEnumerator N47()
  {
   var g=s.Task("G");var gate=s.Slider("A gate");var drawer=s.Slider("Step bridge");
   yield return tap(g.HandPoint+Vector3.up*.004f);
   if(g.Busy)throw new InvalidOperationException("G left machine 1 through the closed gate");
   Debug.Log("N47 G refused: "+g.LastFailure);
   yield return s.Go(W(-.33f,-.298f,-.10f),"Onto P1");
   yield return until(15,()=>gate.AtEnd,"Machine 1 lifts the gate");
   yield return Pull("G","G borrowed for machine 2");
   if(!gate.AtEnd)throw new InvalidOperationException("The gate dropped when G left machine 1");
   yield return s.Go(W(-.33f,-.298f,.12f),"Onto P2");
   yield return until(20,()=>drawer.AtEnd,"Machine 2 draws the step out");
   yield return s.Go(W(.20f,-.27f,.07f),"Onto the step");
   // Tap the platform beside the body: from this framing a tap straight behind it passes over the body (a selection).
   yield return s.Go(W(.16f,-.24f,.22f),"Onto the exit platform");
  }
  // N49: half on P2, a quarter on C (the lock bolt slides out), a quarter on P1: 75 % on the motors opens the door.
  IEnumerator N49()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("Exit door");var bolt=s.Slider("C gear lock");
   yield return s.Split(q,Selected);int half=q.LastLeft,other=q.LastRight;
   // Out of Q's tray lanes by its left side first: from a lane, a route round Q stalls (see N40, N45).
   yield return s.Walk(half,W(-.36f,-.30f,-.05f),"Half out past Q's left side");
   yield return s.Walk(half,W(.10f,-.298f,.04f),"Half round Q onto motor pad P2");
   yield return s.Split(q,other);int q1=q.LastLeft,q2=q.LastRight;
   yield return s.Walk(q2,W(.30f,-.298f,-.20f),"A quarter onto C");
   yield return until(10,()=>bolt.AtEnd,"C draws the lock bolt");
   yield return s.Walk(q1,W(-.36f,-.30f,-.05f),"A quarter out past Q's left side");
   yield return s.Walk(q1,W(-.10f,-.298f,.04f),"A quarter onto motor pad P1");
   yield return until(20,()=>door.AtEnd,"Three quarters on the motors open the door");
   Debug.Log($"N49 DOOR 50+25 pos={door.Position:F3} {Gauges()}");
   yield return s.Merge(W(.15f,-.30f,-.05f));
  }
  // N49, the wrong split: half on C, a quarter on each motor. Half a body's pull stalls the door part way.
  public IEnumerator N49HalfStalls()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("Exit door");
   yield return s.Split(q,Selected);int half=q.LastLeft,other=q.LastRight;
   yield return s.Walk(half,W(.30f,-.298f,-.20f),"Half round Q onto C");
   yield return s.Split(q,other);int q1=q.LastLeft,q2=q.LastRight;
   yield return s.Walk(q1,W(-.36f,-.30f,-.05f),"A quarter out past Q's left side");
   yield return s.Walk(q1,W(-.10f,-.298f,.04f),"A quarter onto P1");
   yield return s.Walk(q2,W(.03f,-.30f,-.14f),"A quarter out past Q's right side");
   yield return s.Walk(q2,W(.10f,-.298f,.04f),"A quarter onto P2");
   float t0=game.Matter.SimulationTime;yield return until(15,()=>game.Matter.SimulationTime-t0>6,"Half a body pulls");
   Debug.Log($"N49 DOOR 25+25 pos={door.Position:F3} {Gauges()}");
   if(door.AtEnd||door.Position<.01f||door.Position>.08f)throw new InvalidOperationException($"Half a body should stall the door part way: {door.Position:F3}");
  }
  // N43: the route starts on the right branch (to a shut exit). G to the left row, P: the switch turns to the balcony. Up the
  // tube to the balcony, C opens the exit shutter; back down the tube. G to the right row, P: the switch turns to the
  // landing; up the tube to the landing and out.
  IEnumerator N43()
  {
   var tube=Find<COgheTubeNetwork>();var shutter=s.Slider("C exit shutter");var g=s.Task("G");Vector3 pad=W(.04f,-.298f,-.04f);
   if(!tube.Edges[2].Open||tube.Edges[1].Open)throw new InvalidOperationException("The route should start on the right branch only");
   yield return s.Operate("G");yield return until(8,()=>g.Rail.Position<=g.Rail.CatchTolerance,"G into the left row");
   yield return s.Go(pad,"Onto P");
   yield return until(10,()=>tube.Edges[1].Open&&!tube.Edges[2].Open,"The left row turns the switch to the balcony");
   yield return s.EnterTube(tube,0);yield return s.Choose(tube,1,1);yield return s.LeaveTube(tube);
   yield return s.Operate("C");yield return until(8,()=>shutter.AtEnd,"C raises the exit shutter");
   yield return s.EnterTube(tube,2);yield return s.Choose(tube,1,0);yield return s.LeaveTube(tube);
   yield return s.Go(W(-.04f,-.30f,-.25f),"Out from the tube's floor mouth");
   yield return Pull("G","G back into the right row");
   yield return s.Go(pad,"Onto P");
   yield return until(10,()=>tube.Edges[2].Open&&!tube.Edges[1].Open,"The right row turns the switch to the landing");
   yield return s.EnterTube(tube,0);yield return s.Choose(tube,1,2);yield return s.LeaveTube(tube);
  }
  // N46: push the crate to the deck's far end, step off, P: the table turns the crate against the ledge; deck, crate, ledge.
  IEnumerator N46()
  {
   var table=Find<COgheTurntable>();
   yield return Pull("A","Push the crate to the deck's far end");
   yield return s.Go(W(-.25f,-.298f,-.24f),"Off the table onto P");
   yield return until(20,()=>table.Caught,"The table turns a quarter, crate and all");
   yield return s.Go(W(-.12f,-.27f,.10f),"Onto the deck");
   yield return s.Go(W(.14f,-.207f,.10f),"Onto the crate");
   yield return s.Go(W(.30f,-.21f,.10f),"Onto the exit ledge");
  }
  // N48: on P with A's gap open nothing turns; A in, P: the floor layer screws C up into the upper layer, the column turns
  // it and the bridge rises; up the stairs, across the mid deck and the bridge.
  IEnumerator N48()
  {
   var lift=s.Slider("C screw lift");var bridge=s.Slider("Upper bridge");Vector3 pad=W(-.12f,-.298f,-.25f);
   yield return s.Go(pad,"Onto P");
   float t0=game.Matter.SimulationTime;yield return until(5,()=>game.Matter.SimulationTime-t0>1.5f,"Only the motor turns");
   if(lift.Position>.005f)throw new InvalidOperationException("The screw turned with A's gap open");
   yield return Pull("A","Gear A into the floor gap");
   yield return s.Go(pad,"Back onto P");
   yield return until(20,()=>lift.AtEnd,"The floor layer screws C up into the upper layer");
   yield return until(20,()=>bridge.AtEnd,"The column turns the upper layer: the bridge rises");
   yield return s.Go(W(-.37f,-.12f,-.16f),"Up the stairs");yield return s.Go(W(-.20f,-.12f,.20f),"Across the mid deck");
   yield return s.Go(W(.025f,-.12f,.20f),"Onto the bridge");yield return s.Go(W(.28f,-.12f,.20f),"Onto the exit ledge");
  }
  // N50 (boss): A in, P: the stair gate drops back. A out, B in, P: the gate rises for good. B out, A in. Split: a half
  // up the stairs onto C (the door bolt slides out), a half on P: the door strains half way and drops back. The half on C
  // comes down to Q and splits: a quarter back up to C, a quarter joins the driver: three quarters open the door. All up.
  IEnumerator N50()
  {
   var q=Find<COgheQuantumSplitter>();var gate=s.Slider("Stair gate");var door=s.Slider("Exit door");var bolt=s.Slider("C door bolt");var a=s.Task("A");var b=s.Task("B");
   Vector3 pad=W(-.32f,-.298f,-.04f),foot=W(.16f,-.30f,-.04f),top=W(.37f,-.12f,-.04f),pc=W(.22f,-.118f,.24f);
   yield return s.Go(pad,"The whole body onto P");
   yield return until(10,()=>gate.Position<=gate.CatchTolerance,"Straight gear A: the stair gate drops back");
   yield return s.Operate("A");yield return until(8,()=>a.Rail.Position<=a.Rail.CatchTolerance,"Draw A out");
   yield return Pull("B","Push the idler pair in");
   yield return s.Go(pad,"Back onto P");
   yield return until(20,()=>gate.AtEnd,"The idler reverses the train: the stair gate rises");
   yield return s.Operate("B");yield return until(8,()=>b.Rail.Position<=b.Rail.CatchTolerance,"Draw the idler out again");
   yield return Pull("A","A back in");
   yield return s.Split(q,Selected);int climber=q.LastRight,driver=q.LastLeft;
   yield return s.Walk(climber,foot,"A half to the foot of the stairs");
   yield return s.Walk(climber,top,"Up the stairs");
   yield return s.Walk(climber,pc,"Onto C: the door bolt slides out");
   yield return until(10,()=>bolt.AtEnd,"C draws the door bolt");
   yield return s.Walk(driver,W(-.36f,-.30f,.02f),"The other half out past Q's left side");
   yield return s.Walk(driver,pad,"The other half onto P");
   float t0=game.Matter.SimulationTime;yield return until(15,()=>game.Matter.SimulationTime-t0>6,"Half a body drives");
   Debug.Log($"N50 DOOR half/half pos={door.Position:F3} {Gauges()}");
   if(door.AtEnd||door.Position<.01f||door.Position>.08f)throw new InvalidOperationException($"Half a body should stall the door part way: {door.Position:F3}");
   yield return s.Walk(climber,top,"The climber leaves C");
   yield return s.Walk(climber,foot,"Down the stairs");
   yield return s.Walk(climber,W(-.20f,-.30f,-.01f),"In front of Q");
   yield return s.Split(q,climber);int q1=q.LastRight,q2=q.LastLeft;
   yield return s.Walk(q1,foot,"A quarter to the stairs");
   yield return s.Walk(q1,top,"Up the stairs");
   yield return s.Walk(q1,pc,"The quarter onto C");
   yield return until(10,()=>bolt.AtEnd,"C draws the bolt again");
   yield return s.Walk(q2,W(-.36f,-.30f,.02f),"The other quarter out past Q's left side");
   yield return s.MergeParts(new List<int>{q2,driver},pad,2);
   yield return s.Walk(driver,pad,"Three quarters on P");
   yield return until(20,()=>door.AtEnd,"Three quarters open the exit door");
   Debug.Log($"N50 DOOR 75/25 pos={door.Position:F3} {Gauges()}");
   yield return s.Walk(driver,foot,"The driver to the stairs");
   yield return s.Walk(driver,top,"Up the stairs");
   yield return s.Merge(W(.28f,-.12f,.14f));
  }
  // N33: half on A holds the door; the quarters load B1 and B2 (C's cover lifts for good); one quarter alone on C lights one
  // tile of two and the pin stays put; the quarters merge on C, the pin slides, the holder follows, all merge.
  IEnumerator N33()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("A door");var cover=s.Slider("B cover");var pin=s.Slider("C door pin");
   yield return s.Split(q,Selected);int half=q.LastLeft,other=q.LastRight;
   yield return s.Walk(half,W(-.32f,-.298f,-.20f),"Half onto the heavy pad A");
   yield return until(10,()=>door.AtEnd,"Half a body raises the door");
   yield return s.Split(q,other);int q1=q.LastLeft,q2=q.LastRight;
   yield return s.Walk(q2,W(.14f,-.298f,.22f),"Quarter onto B1");
   yield return s.Walk(q1,W(.32f,-.298f,-.22f),"Quarter onto B2");
   yield return until(10,()=>cover.AtEnd,"B1 and B2 lift the cover off C");
   yield return s.Walk(q1,W(.32f,-.298f,.02f),"One quarter onto C");
   float t0=game.Matter.SimulationTime;yield return until(4,()=>game.Matter.SimulationTime-t0>1.5f,"C weighs one quarter");
   Debug.Log("N33 GAUGES quarter on C: "+Gauges());
   if(pin.Position>.005f)throw new InvalidOperationException("A quarter slid the door pin");
   yield return s.MergeParts(new List<int>{q1,q2},W(.32f,-.298f,.02f),2);
   yield return s.Walk(q1,W(.32f,-.298f,.02f),"The merged half onto C");
   yield return until(10,()=>pin.AtEnd,"Half a body on C slides the pin");
   Debug.Log("N33 GAUGES half on C: "+Gauges());
   yield return s.Walk(half,W(.14f,-.30f,-.20f),"The holder leaves A and follows");
   yield return s.Merge(W(.20f,-.30f,-.02f));
  }
  // N35: a half holds A1 and the other half strains at B (75 %) and lets go; the holder splits again: one quarter takes A1,
  // the other joins the half. Three quarters push B into its socket; all merge and climb.
  IEnumerator N35()
  {
   var q=Find<COgheQuantumSplitter>();var bolt=s.Slider("A lock bolt");var b=s.Task("B");
   yield return s.Split(q,Selected);int holder=q.LastLeft,big=q.LastRight;
   yield return s.Walk(big,W(.12f,-.30f,-.06f),"Half waits off the tray");
   yield return s.Walk(holder,W(-.31f,-.298f,.16f),"Half onto A1");
   yield return until(10,()=>bolt.AtEnd,"A1 draws the bolt");
   int gave=b.GaveUp;game.SelectFragment(big);
   for(int attempt=0;attempt<3&&!b.Busy;attempt++){yield return tap(b.HandPoint+Vector3.up*.004f);if(!b.Busy){yield return tap(b.StandPoint);yield return until(20,()=>Vector3.Distance(game.Motion.Centre(big),b.StandPoint)<.06f,"Half beside B");}}
   yield return until(30,()=>b.GaveUp>gave,"Half a body strains at the 75% block and lets go");
   if(b.Rail.Position>.01f)throw new InvalidOperationException("Half a body moved the 75% block");
   // Back round to Q's mouth (from behind, a route into Q runs up a tray lane and stalls at its gate).
   yield return s.Walk(holder,W(-.185f,-.30f,-.275f),"The holder back in front of Q");
   yield return s.Split(q,holder);int s1=q.LastLeft,s2=q.LastRight;
   yield return s.Walk(s1,W(-.31f,-.298f,.16f),"Quarter onto A1");
   yield return until(10,()=>bolt.AtEnd,"A1 draws the bolt again");
   yield return s.MergeParts(new List<int>{s2,big},game.Motion.Centre(big),2);
   game.SelectFragment(big);yield return Pull("B","Three quarters push B into its socket");
   yield return s.Merge(W(-.05f,-.30f,-.02f));
   yield return s.Go(W(.17f,-.27f,.005f),"Onto the fixed step");
   yield return s.Go(W(.17f,-.24f,.12f),"Onto block B");
   yield return s.Go(W(.31f,-.21f,.15f),"Onto the exit platform");
  }
  IEnumerator E05()
  {
   var a=Rope("A");var b=Rope("B");
   yield return s.Grip(a);yield return s.Swing(a,0);
   yield return s.Grip(b);yield return s.Swing(b,0);
  }
  IEnumerator E06()
  {
   // Each plank: walk onto its foot, then up past the axle; it tips and the body walks down into the next room.
   // (Tapped from beside the plank, the route can meet the plank's edge in mid-air.)
   foreach(var label in new[]{"A","B"})
   {
    var plank=Prop(label+" seesaw plank");
    yield return s.Go(plank.transform.TransformPoint(new Vector3(-.17f,.011f,0)),"Onto the foot of plank "+label,.07f);
    yield return tap(plank.transform.TransformPoint(new Vector3(.07f,.011f,0)));
    yield return until(25,()=>plank.transform.TransformPoint(Vector3.right*.2f).y<plank.transform.position.y,"Plank "+label+" tips under the body");
   }
   yield return s.Go(W(.30f,-.30f,-.06f),"Down plank B into the exit room",.07f);
  }
  IEnumerator E07()
  {
   yield return Pull("A","Block A against the shelf");
   yield return s.Go(W(-.13f,-.21f,.14f),"Climb block A");
   yield return Pull("B","Block B to the shelf edge");
   yield return s.Go(W(.10f,-.12f,.14f),"Climb block B");
   yield return s.Go(W(.26f,-.12f,.17f),"Onto the shelf");
  }
  IEnumerator B1()
  {
   var q=Find<COgheQuantumSplitter>();var latch=Find<COgheLoadLatch>();
   yield return s.Split(q,Selected);int climber=q.LastLeft,holder=q.LastRight;
   yield return s.Walk(holder,W(.30f,-.298f,-.20f),"One half loads pad A");
   yield return s.Walk(climber,W(.02f,-.21f,.06f),"The other climbs C1's exposed end");
   yield return s.Walk(climber,W(-.28f,-.12f,.16f),"From C1 onto the lever shelf");
   game.SelectFragment(climber);yield return Pull("B","Lever B");
   yield return until(10,()=>latch.Caught,"Pad A and lever B catch C1's bolt");
   game.SelectFragment(climber);yield return Pull("C2","From the shelf, C2 across to C1's tower end");
   yield return s.Walk(climber,W(-.10f,-.12f,.06f),"Onto C2's free strip");
   game.SelectFragment(climber);yield return Pull("C3","On C2, C3 back");
   yield return s.Merge(W(-.10f,-.30f,-.08f));
   yield return Pull("C1","The whole body hauls C1 against the tower",60);
   yield return s.Go(W(-.04f,-.21f,.08f),"Onto C1");yield return s.Go(W(.06f,-.12f,.08f),"Onto C2");
   yield return s.Go(W(.17f,-.03f,.21f),"Onto C3");yield return s.Go(W(.31f,-.03f,.15f),"Onto the tower");
  }
  IEnumerator E08()
  {
   var deck=s.Slider("A rising deck");
   yield return s.Go(W(-.12f,-.30f,-.22f),"Stand on pad A");
   yield return until(20,()=>deck.AtEnd,"The gears raise the deck");
   yield return s.Go(W(-.28f,-.18f,.10f),"Up the stairs onto the start ledge");
   yield return s.Go(W(.28f,-.18f,.13f),"Across the deck to the exit ledge");
  }
  IEnumerator E09()
  {
   var q=Find<COgheQuantumSplitter>();var door=s.Slider("A door");var pin=s.Slider("B door pin");
   yield return s.Split(q,Selected);int half=q.LastLeft,other=q.LastRight;
   yield return s.Walk(half,W(-.32f,-.298f,-.20f),"Half onto the heavy pad A");
   yield return until(10,()=>door.AtEnd,"Half a body opens the door");
   yield return s.Split(q,other);int q1=q.LastLeft,q2=q.LastRight;
   yield return s.Walk(q2,W(.22f,-.298f,.12f),"Quarter onto B1");
   yield return s.Walk(q1,W(.32f,-.298f,-.10f),"Quarter onto B2");
   yield return until(10,()=>pin.AtEnd,"B1 and B2 catch the door pin");
   yield return s.Merge(W(.24f,-.30f,.0f));
  }
  IEnumerator E10()
  {
   var a=Rope("A");var tube=Tube("Pen tube");
   yield return s.Grip(a);yield return s.Swing(a,0);
   yield return s.EnterTube(tube,0);yield return s.LeaveTube(tube);
  }
  IEnumerator E11()
  {
   var q=Find<COgheQuantumSplitter>();var lift=Find<COghePassengerLift>();var pin=s.Slider("Lift enable pin");var c=s.Task("C");
   yield return s.Split(q,Selected);int holder=q.LastLeft,rider=q.LastRight;
   yield return s.Walk(rider,W(.20f,-.30f,-.08f),"Rider steps wide of Q");
   yield return s.Walk(rider,W(-.06f,-.30f,.03f),"Rider waits in front of the lift");
   game.SelectFragment(holder);yield return s.Hold("B");
   yield return until(10,()=>pin.AtEnd,"Holding B powers the lift");
   game.SelectFragment(rider);yield return tap(lift.Panel.position);
   yield return until(45,()=>lift.Trips>=1&&lift.Rail.AtEnd,"The rider goes up");
   game.SelectFragment(rider);yield return s.Operate("C");yield return until(5,()=>c.Rail.AtEnd,"C keeps the lift powered");
   game.SelectFragment(holder);yield return tap(lift.CallPanels[0].position);
   yield return until(45,()=>lift.Rail.Position<=lift.Rail.CatchTolerance*2&&!lift.Moving,"Call the lift down");
   game.SelectFragment(holder);yield return tap(lift.Panel.position);
   yield return until(60,()=>lift.Trips>=3&&lift.Rail.AtEnd,"The holder rides up");
   yield return s.Merge(W(.20f,-.06f,.22f));
  }
  IEnumerator E12()
  {
   var drawer=s.Slider("Step bridge");
   yield return Pull("A","Gear G into the gap");
   yield return s.Go(W(-.32f,-.30f,-.16f),"Stand on pad P");
   yield return until(20,()=>drawer.AtEnd,"The train draws the step out");
   yield return s.Go(W(.20f,-.27f,.07f),"Onto the step");yield return s.Go(W(.20f,-.24f,.21f),"Onto the exit platform");
  }
  IEnumerator E13()
  {
   var q=Find<COgheQuantumSplitter>();var left=Tube("Left tube");var right=Tube("Right tube");var latch=Find<COgheLoadLatch>();
   yield return s.Split(q,Selected);int a=q.LastLeft,b=q.LastRight;
   game.SelectFragment(a);yield return s.EnterTube(left,0);yield return s.LeaveTube(left);
   yield return s.Walk(a,W(-.345f,-.098f,.245f),"Left half onto A1");
   game.SelectFragment(b);yield return s.EnterTube(right,0);yield return s.LeaveTube(right);
   yield return s.Walk(b,W(.345f,-.098f,.245f),"Right half onto A2");
   yield return until(20,()=>latch.Caught,"Both pads slide the step out; its pawl catches");
   game.SelectFragment(a);yield return s.EnterTube(left,1);yield return s.LeaveTube(left);
   game.SelectFragment(b);yield return s.EnterTube(right,1);yield return s.LeaveTube(right);
   yield return s.Merge(W(.15f,-.30f,0)); // open floor between Q and the step: not beside the step (boxed in), not near Q's tray
   yield return s.Go(W(0,-.30f,0),"Square in front of the step"); // not onto its side edge
   yield return s.Go(W(0,-.27f,.08f),"Onto the step");yield return s.Go(W(0,-.24f,.21f),"Onto the exit platform");
  }
  IEnumerator E14()
  {
   var gate=s.Slider("Stair gate");var bridge=s.Slider("Upper bridge");
   yield return Pull("A","Floor layer closed");
   yield return s.Go(W(-.12f,-.30f,-.25f),"Stand on pad P");
   yield return until(20,()=>gate.AtEnd,"The floor layer lifts the stair gate");
   yield return s.Go(W(-.37f,-.12f,-.16f),"Up the stairs");yield return s.Go(W(-.22f,-.12f,-.05f),"Onto the mid deck");
   yield return Pull("B","Mid layer closed");
   yield return s.Go(W(-.12f,-.30f,-.25f),"Back down onto pad P");
   yield return until(20,()=>bridge.AtEnd,"The shaft turns the mid layer; the bridge rises");
   yield return s.Go(W(-.37f,-.12f,-.16f),"Up the stairs again");yield return s.Go(W(-.20f,-.12f,.20f),"Across the mid deck");
   yield return s.Go(W(.025f,-.12f,.20f),"Onto the bridge");yield return s.Go(W(.28f,-.12f,.20f),"Onto the exit ledge");
  }
  IEnumerator E15()
  {
   var q=Find<COgheQuantumSplitter>();var lift=s.Slider("Gear lift");var top=s.Slider("C top step");
   yield return s.Split(q,Selected);int driver=q.LastLeft,rider=q.LastRight;
   yield return s.Walk(rider,W(.13f,-.30f,-.02f),"Rider steps wide of Q");
   yield return s.Walk(rider,W(-.01f,-.27f,.16f),"Rider onto the lift");
   yield return s.Walk(driver,W(-.30f,-.298f,-.20f),"Driver onto pad P");
   yield return until(30,()=>lift.AtEnd,"The lift rises while P is loaded");
   yield return s.Walk(rider,W(.18f,-.12f,.20f),"Rider onto the high deck");
   game.SelectFragment(rider);yield return s.Operate("C");yield return until(10,()=>top.AtEnd,"C raises the top stair step");
   yield return s.Walk(driver,W(.25f,-.24f,-.07f),"Driver leaves P for the stair");
   yield return s.Walk(driver,W(.25f,-.12f,.045f),"Driver climbs to the high deck"); // front edge: visible below the HUD on short screens
   yield return s.Merge(W(.25f,-.12f,.20f));
  }
  IEnumerator E16()
  {
   var table=Find<COgheTurntable>();
   yield return s.Go(W(-.16f,-.30f,-.24f),"Stand on pad P");
   yield return until(20,()=>table.Caught,"The gears turn the table a quarter turn");
   yield return s.Go(W(-.30f,-.18f,.08f),"Up the stairs onto the start ledge");
   yield return s.Go(W(.28f,-.18f,.10f),"Across the turntable to the exit ledge");
  }
  IEnumerator E17()
  {
   var g=s.Slider("G carriage");var drawer=s.Slider("Step bridge");
   yield return s.Go(W(-.32f,-.30f,-.20f),"Stand on P1");
   yield return until(20,()=>g.AtEnd,"Machine 1 pushes gear G into machine 2");
   yield return s.Go(W(.02f,-.30f,-.20f),"Stand on P2");
   yield return until(20,()=>drawer.AtEnd,"Machine 2 draws the step out");
   yield return s.Go(W(.24f,-.27f,.07f),"Onto the step");yield return s.Go(W(.24f,-.24f,.21f),"Onto the exit platform");
  }
  IEnumerator E18()
  {
   var q=Find<COgheQuantumSplitter>();var gate=s.Slider("Stair gate");var step=s.Slider("Step to the top deck");var shutter=s.Slider("Exit shutter");
   yield return s.Split(q,Selected);int fitter=q.LastLeft,driver=q.LastRight;
   yield return s.Walk(driver,W(.355f,-.298f,.035f),"Driver up the right side onto pad P");
   game.SelectFragment(fitter);yield return Pull("A","Floor layer closed");
   yield return until(20,()=>gate.AtEnd,"The floor layer lifts the stair gate");
   yield return s.Walk(fitter,W(-.37f,-.12f,-.16f),"Fitter up the stairs");yield return s.Walk(fitter,W(-.22f,-.12f,-.05f),"Onto the mid deck");
   game.SelectFragment(fitter);yield return Pull("B","Mid layer closed");
   yield return until(20,()=>step.AtEnd,"The mid layer slides the step to the top deck");
   yield return s.Walk(fitter,W(-.14f,-.03f,.245f),"Fitter onto the step");yield return s.Walk(fitter,W(.14f,-.03f,.22f),"Onto the top deck");
   game.SelectFragment(fitter);yield return Pull("C","Top layer closed");
   yield return until(20,()=>shutter.AtEnd,"The top layer opens the exit shutter");
   // Carriage A now closes the way behind Q: down the right side and along the front.
   yield return s.Walk(driver,W(-.18f,-.30f,-.22f),"Driver leaves P for the stairs by the front");
   yield return s.Walk(driver,W(-.37f,-.12f,-.16f),"Up the stairs");
   yield return s.Walk(driver,W(-.20f,-.12f,.20f),"Across the mid deck");yield return s.Walk(driver,W(-.14f,-.03f,.245f),"Onto the step");
   yield return s.Merge(W(.14f,-.03f,.22f));
  }
  IEnumerator B2()
  {
   // 50 % + 25 % + 25 %: the half drives P2, one quarter drives P1, the other rides the lift and fits B (which opens the stairs).
   var q=Find<COgheQuantumSplitter>();var gate=s.Slider("Stair gate");var lift=s.Slider("Held lift");var step=s.Slider("Step to the top deck");var shutter=s.Slider("Exit shutter");var c=s.Slider("C carriage");
   // The left tray's half goes straight along the front to the stairs; the right tray's half goes back through Q.
   yield return s.Split(q,Selected);int half=q.LastLeft,l=q.LastRight;
   yield return s.Walk(half,W(-.17f,-.30f,-.25f),"The half waits right of the stairs");
   yield return s.Split(q,l);int fitter=q.LastLeft,driver=q.LastRight;
   // As the game prompts, lead the left quarter off Q's tray (out the front); straight at A it meets the casing side.
   yield return s.Walk(fitter,W(.06f,-.30f,-.25f),"Fitter off Q's left tray");
   game.SelectFragment(fitter);yield return Pull("A","Floor layer closed");
   yield return s.Walk(fitter,W(-.15f,-.27f,-.155f),"Fitter onto the held lift");
   yield return s.Walk(driver,W(.355f,-.298f,.035f),"Driver up the right side onto P1");
   yield return until(30,()=>lift.AtEnd,"P1 runs the lift up with the fitter");
   yield return s.Walk(fitter,W(-.20f,-.12f,-.06f),"Fitter onto the mid deck");
   game.SelectFragment(fitter);yield return Pull("B","Mid layer closed");
   yield return until(10,()=>gate.AtEnd,"B's linkage lifts the stair gate");
   yield return s.Walk(half,W(-.37f,-.12f,-.16f),"The half up the stairs");
   yield return s.Walk(half,W(-.25f,-.118f,.14f),"The half onto P2");
   yield return until(30,()=>c.AtEnd&&step.AtEnd,"P2 pushes C into the top layer and slides the step");
   yield return until(30,()=>shutter.AtEnd,"P1 and P2 together open the exit shutter");
   yield return s.Walk(fitter,W(-.14f,-.03f,.245f),"Fitter onto the step");yield return s.Walk(fitter,W(.14f,-.03f,.22f),"Onto the top deck");
   // The shutter has latched open: the driver leaves P1 by the front (carriage A closes the way behind Q), climbs the
   // stairs, and all three parts meet on the top deck.
   yield return s.Walk(driver,W(-.18f,-.30f,-.22f),"Driver leaves P1 for the stairs by the front");
   yield return s.Walk(driver,W(-.37f,-.12f,-.16f),"Driver up the stairs");
   // The fitter steps toward the exit so taps at the meeting point land on the deck, not on the fitter.
   yield return s.Walk(fitter,W(.30f,-.03f,.20f),"Fitter waits by the exit");
   yield return s.MergeParts(new List<int>{driver,half,fitter},W(.14f,-.03f,.22f),1);
  }
 }
}
#endif
