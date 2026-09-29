using System.Collections;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Recovery: a body that falls (or walks) to the floor climbs back to where it started with ordinary taps, and
 // rope levels grip and swing again. Reported for 18/19 on 29/09/2026: the old ramps stalled at the bank's lip.
 public sealed partial class COgheSpatialCampaignTests
 {
  // Grip, launch toward the far bank and let go over the gap onto the rescue floor.
  IEnumerator FallFromRope(COgheSpatialNextScenario s,COgheSwingTransfer swing,Vector3 floorPoint,int dock)
  {
   int anchor=game.Motion.Selected;
   yield return s.Grip(swing);
   yield return Tap(swing.Docks[dock].Closest(swing.DockTargets[dock].position));
   yield return Until(3,()=>swing.Phase==COgheSwingTransfer.SwingPhase.Swinging&&Vector3.ProjectOnPlane(swing.Ring.position-swing.Pivot.position,Vector3.up).magnitude<.05f,"Over the gap");
   yield return Tap(game.Root.TransformPoint(floorPoint));
   yield return Until(20,()=>swing.Actor<0&&game.Root.InverseTransformPoint(game.Motion.Centre(anchor)).y<-.25f,"Let go onto the rescue floor");
   yield return Until(15,()=>swing.Phase==COgheSwingTransfer.SwingPhase.Idle,"Winch parks the ring again");
   Capture(game.Definition.Order,"fallen");
  }
  // After the fall: tap the start bank's top (the player's way back), then grip and swing.
  IEnumerator ClimbRegripSwing(COgheSpatialNextScenario s,COgheSwingTransfer swing,Vector3 bankTop,int dock)
  {
   yield return s.Walk(game.Motion.Selected,game.Root.TransformPoint(bankTop),"Climb back onto the start bank");
   Capture(game.Definition.Order,"climbed");
   yield return s.Grip(swing);
   Assert.AreEqual(COgheSwingTransfer.SwingPhase.Ready,swing.Phase,"Regrip after the climb");
   yield return s.Swing(swing,dock);
  }
  [UnityTest] public IEnumerator Spatial18FallRecovery()
  {
   yield return Load(18);var s=new COgheSpatialNextScenario(game,Tap,Until);var swing=s.Find<COgheSwingTransfer>();
   yield return FallFromRope(s,swing,new Vector3(0,-.30f,-.05f),0);
   yield return ClimbRegripSwing(s,swing,new Vector3(-.28f,-.16f,.05f),0);
  }
  [UnityTest] public IEnumerator Spatial19FallRecovery()
  {
   yield return Load(19);var s=new COgheSpatialNextScenario(game,Tap,Until);var swing=s.Find<COgheSwingTransfer>();var tray=s.Slider("B landing tray");
   yield return s.Operate("B");yield return Until(10,()=>tray.AtEnd,"B brings the landing tray into the arc");
   yield return s.Walk(game.Motion.Selected,game.Root.TransformPoint(new Vector3(-.28f,-.16f,.05f)),"Up the stairs onto the start bank");
   yield return FallFromRope(s,swing,new Vector3(0,-.30f,-.05f),1);
   yield return ClimbRegripSwing(s,swing,new Vector3(-.28f,-.16f,.05f),1);
  }
  // Tapping the ring straight from the rescue floor must also bring the body up to it.
  [UnityTest] public IEnumerator Spatial18RingFromFloor()
  {
   yield return Load(18);var s=new COgheSpatialNextScenario(game,Tap,Until);var swing=s.Find<COgheSwingTransfer>();
   yield return FallFromRope(s,swing,new Vector3(0,-.30f,-.05f),0);
   yield return s.Grip(swing);
   Assert.AreEqual(COgheSwingTransfer.SwingPhase.Ready,swing.Phase,"Grip the ring from the rescue floor");
  }
  // Whole body on the rope, dropped over the gap: climb back and grip again (26: stairs; 30: the bank's ivory back face).
  IEnumerator FallClimbRegrip(int n,Vector3 floorPoint,Vector3 bankTop,int dock)
  {
   yield return Load(n);var s=new COgheSpatialNextScenario(game,Tap,Until);var swing=s.Find<COgheSwingTransfer>();
   yield return FallFromRope(s,swing,floorPoint,dock);
   yield return s.Walk(game.Motion.Selected,game.Root.TransformPoint(bankTop),"Climb back onto the start bank");
   yield return s.Grip(swing);
   Assert.AreEqual(COgheSwingTransfer.SwingPhase.Ready,swing.Phase,"Regrip after the climb");
  }
  [UnityTest] public IEnumerator Spatial26FallRecovery(){yield return FallClimbRegrip(26,new Vector3(.12f,-.30f,-.15f),new Vector3(-.28f,-.16f,.14f),2);}
  [UnityTest] public IEnumerator Spatial30FallRecovery(){yield return FallClimbRegrip(30,new Vector3(.30f,-.30f,-.05f),new Vector3(.28f,-.20f,-.24f),0);}
  // A one-way latch stays at its end: in 26, once B has slid the landing tray out, tapping B's handle again neither
  // pulls it back nor starts an approach (the tap falls through to the platform behind it).
  [UnityTest] public IEnumerator Spatial26LatchIsOneWay()
  {
   yield return Load(26);var s=new COgheSpatialNextScenario(game,Tap,Until);
   var q=s.Find<COgheQuantumSplitter>();var tube=s.Find<COgheTubeNetwork>();var b=s.Task("B");var tray=s.Slider("B landing tray");
   yield return s.Split(q,game.Motion.Selected);int holder=q.LastLeft,worker=q.LastRight;
   yield return s.Walk(holder,game.Root.TransformPoint(new Vector3(-.34f,-.158f,.24f)),"Holder loads pad A");
   yield return Until(10,()=>tube.IsEntryOpen(0),"Pad A holds the tube cap open");
   game.SelectFragment(worker);yield return s.EnterTube(tube,0);yield return s.LeaveTube(tube);
   yield return s.Operate("B");yield return Until(10,()=>tray.AtEnd,"Tray out");
   game.SelectFragment(worker);yield return Tap(b.HandPoint+Vector3.up*.004f);
   Assert.IsFalse(b.Busy,"A latched one-way handle takes no new task");
   yield return Wait(3);
   Assert.IsTrue(b.Rail.AtEnd&&tray.AtEnd,"B and the tray stay latched out");
  }
  // Walk down the recovery stairs to the floor, then tap the platform top: the body must climb back.
  IEnumerator DownAndBack(int n,Vector3 floorPoint,Vector3 top)
  {
   yield return Load(n);var s=new COgheSpatialNextScenario(game,Tap,Until);int anchor=game.Motion.Selected;
   yield return s.Walk(anchor,game.Root.TransformPoint(floorPoint),"Down to the floor");
   yield return s.Walk(anchor,game.Root.TransformPoint(top),"Back up the stairs");
   Capture(n,"climbed");
  }
  [UnityTest] public IEnumerator Spatial21ClimbBack(){yield return DownAndBack(21,new Vector3(0,-.30f,-.20f),new Vector3(-.30f,-.20f,-.18f));}
  [UnityTest] public IEnumerator Spatial26ClimbBack(){yield return DownAndBack(26,new Vector3(-.045f,-.30f,.19f),new Vector3(-.05f,-.16f,-.08f));}
  [UnityTest] public IEnumerator Spatial29ClimbBack(){yield return DownAndBack(29,new Vector3(-.34f,-.30f,-.24f),new Vector3(-.34f,-.18f,.13f));}
 }
}
