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
  IEnumerator E04()
  {
   yield return Pull("A","Cart A against the shelf");
   yield return Pull("B","Crate B across the cart beside the shelf");
   yield return s.Go(W(.05f,-.18f,.18f),"Climb the crate");
   yield return s.Go(W(.26f,-.18f,.17f),"Onto the exit shelf");
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
