using System.Collections;
using System.Collections.Generic;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Layer A of the hook plan (Mrk, 05/10/2026): a button reads as a button, and a refused tap says why.
 public sealed partial class COgheSpatialCampaignTests
 {
  private static void InOrder<T>(IList<T> seen,params T[] expected)
  {
   int k=0;foreach(var s in seen)if(k<expected.Length&&EqualityComparer<T>.Default.Equals(s,expected[k]))k++;
   Assert.AreEqual(expected.Length,k,"Expected in order: "+string.Join(" → ",expected)+"; saw: "+string.Join(" → ",seen));
  }

  [UnityTest] public IEnumerator LiftButtonPressesHoldsAndPopsUp()
  {
   yield return Load(8);var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();
   var seen=new List<COghePassengerLift.ButtonState>();float movingDepth=1;
   trace=()=>{if(seen.Count==0||seen[seen.Count-1]!=lift.Button)seen.Add(lift.Button);if(lift.Moving)movingDepth=Mathf.Min(movingDepth,lift.PanelDepth);};
   Assert.AreEqual(1,lift.NextDirection,"At the bottom the button sends the tray up");
   yield return Tap(lift.Panel.position);
   yield return Until(35,()=>lift.Trips==1,"Ride up");
   yield return Until(1,()=>lift.Button==COghePassengerLift.ButtonState.Ready,"The button pops back up on arrival");
   InOrder(seen,COghePassengerLift.ButtonState.Armed,COghePassengerLift.ButtonState.Pressing,COghePassengerLift.ButtonState.Latched,
    COghePassengerLift.ButtonState.Releasing,COghePassengerLift.ButtonState.Ready);
   Assert.GreaterOrEqual(movingDepth,.95f,"The button stays down while the tray travels");
   Assert.Less(lift.PanelDepth,.05f,"Up again, ready for the next press");
   Assert.AreEqual(-1,lift.NextDirection,"At the top the button sends the tray down");
   seen.Clear();
   yield return Tap(lift.Panel.position);yield return Until(35,()=>lift.Trips==2,"The same button rides back down");
   trace=null;
   InOrder(seen,COghePassengerLift.ButtonState.Pressing,COghePassengerLift.ButtonState.Latched,COghePassengerLift.ButtonState.Releasing);
   game.ResetLevel();Assert.AreEqual(COghePassengerLift.ButtonState.Ready,lift.Button);
  }

  [UnityTest] public IEnumerator UnpoweredLiftRefusesWithAReason()
  {
   yield return Load(10);var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();
   Assert.IsFalse(lift.Powered);int before=lift.Refusals;
   yield return Tap(lift.Panel.position);
   Assert.AreEqual(before+1,lift.Refusals,"The tap is refused, not swallowed");
   Assert.AreEqual(COgheMechanism.Refusal.NoPower,lift.LastRefusal);Assert.IsFalse(lift.Boarding);
   game.Feedback.Refresh();Assert.IsTrue(game.Feedback.DeniedVisible,"The refusal is marked where it happened");
   Assert.IsNotEmpty(COgheProductUI.RefusalText(lift.LastRefusal),"The product toast has a reason");
  }

  [UnityTest] public IEnumerator LockedHandleRefusesWithAReason()
  {
   // 45 (content 29): winch B stays locked until both bridge pieces are seated.
   yield return Load(29);COgheTapRail b=null;foreach(var t in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())if(t.Label=="B")b=t;
   Assert.IsFalse(b.InterlockOpen);int before=b.Refusals;
   Assert.IsFalse(b.Request(game.Motion.Selected),"A locked handle refuses");
   Assert.AreEqual(before+1,b.Refusals);Assert.AreEqual(COgheMechanism.Refusal.Locked,b.LastRefusal);Assert.IsFalse(b.Busy);
   game.Feedback.Refresh();Assert.IsTrue(game.Feedback.DeniedVisible,"The refusal is marked at the handle");
  }

  // Holding the crate, a tap on the lift button lets the crate go and presses the button (the "dead" button of 13).
  [UnityTest] public IEnumerator LiftButtonTapReleasesAHeldCrate()
  {
   yield return LoadPlus("E01");var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();
   VenomMovableProp crate=null;foreach(var p in game.Props)if(p.GetComponent<COgheRailSlider>()==null)crate=p;
   yield return Tap(crate.Body.position+Vector3.up*.02f);
   yield return Until(15,()=>game.Attached,"COghe takes hold of the crate");
   yield return Tap(lift.Panel.position);
   Assert.IsFalse(game.Attached,"The crate is let go");
   Assert.IsTrue(lift.Boarding,"The button tap boards the lift");
  }

  // Point 8: a tap on a tube's body, not only its mouth, takes COghe through it.
  [UnityTest] public IEnumerator TubeBodyTapGoesThrough()
  {
   yield return Load(14);var s=new GravityBox.Venom.ChapterProof.COgheSpatialNextScenario(game,Tap,Until);var tube=game.Owner.Apparatus.GetComponentInChildren<COgheTubeNetwork>();
   yield return s.Operate("A");
   int far=-1;float distance=0;Vector3 centre=game.Motion.Centre(game.Motion.Selected);
   for(int n=0;n<tube.Nodes.Length;n++)if(tube.Nodes[n].Terminal==COgheTubeNetwork.TerminalKind.Entry)
   {float d=Vector3.Distance(game.Root.TransformPoint(tube.Nodes[n].LocalPosition),centre);if(d>distance){distance=d;far=n;}}
   var edge=tube.Edges[0];var middle=game.Root.TransformPoint(edge.ControlPoints[edge.ControlPoints.Length/2]);
   yield return Tap(middle);
   yield return Until(45,()=>tube.LastReachedNode==far&&!tube.AnyTravelling,"One tap on the tube's arch: in at the near mouth, out at the far one");
  }

  // ... and at a junction a tap anywhere on a branch takes it, even tapped before COghe reaches the junction.
  [UnityTest] public IEnumerator TubeBranchTapRoutesThroughTheJunction()
  {
   yield return Load(15);var tube=game.Owner.Apparatus.GetComponentInChildren<COgheTubeNetwork>();
   int balcony=System.Array.FindIndex(tube.Nodes,n=>n.Name=="Ban công");Assert.GreaterOrEqual(balcony,0);
   int branch=System.Array.FindIndex(tube.Edges,e=>e.A==balcony||e.B==balcony);
   var edge=tube.Edges[branch];var point=game.Root.TransformPoint(edge.ControlPoints[edge.ControlPoints.Length/2]);
   yield return Tap(point);
   yield return Until(45,()=>tube.LastReachedNode==balcony&&!tube.AnyTravelling,"One tap on the balcony branch: in, through the Y, out on the balcony");
  }
 }
}
