using System;
using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // Physics props 66–68 (Mrk 19/10/2026): each level's new object does its job only the intended way.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF1SolveAtFrameRate(){yield return GlassCrawlSolveAtFrameRate("F1");}
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF2SolveAtFrameRate(){yield return GlassCrawlSolveAtFrameRate("F2");}
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF3SolveAtFrameRate(){yield return GlassCrawlSolveAtFrameRate("F3");}
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF3SolveAt30Fps(){yield return GlassCrawlSolveAtFrameRate("F3",30);}

  private IEnumerator Ticks(float seconds){for(int i=0;i<seconds/Dt;i++){Tick();if(i%60==0)yield return null;}}

  // 66: COghe on A lifts the door, but it drops as soon as COghe steps off, so the whole body cannot hold it and pass.
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF1DoorNeedsTheBall()
  {
   yield return LoadPlus("F1");yield return Wait(.5f);
   var door=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="A door");
   yield return Tap(game.Root.TransformPoint(new Vector3(-.35f,-.298f,-.235f)));yield return Ticks(12);
   Assert.Greater(door.Position,.05f,"COghe on A lifts the door");
   yield return Tap(game.Root.TransformPoint(new Vector3(.16f,-.30f,-.20f)));yield return Ticks(20);
   Assert.Less(game.Root.InverseTransformPoint(game.Motion.Centre(0)).x,.05f,"Stepping off A drops the door before COghe is through");
   Assert.IsFalse(game.Owner.Completed);
  }

  // 67: COghe pushing the pillar from either side never knocks it over; only the ball does.
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF2PillarIgnoresCOghe()
  {
   yield return LoadPlus("F2");yield return Wait(.5f);
   var pillar=game.Owner.Apparatus.GetComponentInChildren<COgheTopplePillar>();
   foreach(var spot in new[]{new Vector3(.20f,-.30f,-.16f),new Vector3(.20f,-.30f,-.02f),new Vector3(.20f,-.24f,.20f)})
   {yield return Tap(game.Root.TransformPoint(spot));yield return Ticks(10);}
   Assert.IsFalse(pillar.Down||pillar.Falling,"COghe against the pillar does not knock it over");
   Assert.IsFalse(game.Owner.Completed,"No way onto the 6 cm platform while the pillar stands");
  }

  // 68: one pad keeps the ball (one body cannot load both, 43 cm apart); COghe alone on the glass cracks it, no more.
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF3OnePadKeepsTheBall()
  {
   yield return LoadPlus("F3");yield return Wait(.5f);
   var s=new GravityBox.Venom.ChapterProof.COgheSpatialNextScenario(game,Tap,Until);
   var glass=s.Find<COgheBreakableGlass>();var stop=s.Slider("Ball chute stop");
   Vector3 W(float x,float y,float z)=>game.Root.TransformPoint(new Vector3(x,y,z));
   yield return s.Go(W(.33f,-.298f,-.24f),"The whole body on the right pad A");yield return Ticks(5);
   Assert.Less(stop.Position,.01f,"One pad alone does not lift the stop");
   yield return s.Go(W(-.10f,-.298f,-.25f),"The whole body on the left pad A");yield return Ticks(5);
   Assert.Less(stop.Position,.01f,"The other pad alone does not lift it either");
   yield return s.Go(W(-.06f,-.25f,-.02f),"Up the stairs");
   yield return s.Go(W(-.29f,-.19f,.19f),"The whole body onto the glass");yield return Ticks(10);
   Assert.IsTrue(glass.Cracked,"COghe on the glass cracks it");Assert.IsFalse(glass.Broken,"COghe alone does not break it");
   game.ResetLevel();yield return Wait(.5f);
   Assert.IsFalse(glass.Broken||glass.Cracked,"Retry mends the glass");
  }

  // 68: with the ball on the glass, half a body on it only cracks the glass: the halves must merge first. (A half is set
  // down on the glass: whether a half can climb the stairs is beside the point, its weight is.)
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF3HalfAndBallOnlyCrack()
  {
   yield return LoadPlus("F3");yield return Wait(.5f);
   var s=new GravityBox.Venom.ChapterProof.COgheSpatialNextScenario(game,Tap,Until);
   var q=s.Find<COgheQuantumSplitter>();var glass=s.Find<COgheBreakableGlass>();var stop=s.Slider("Ball chute stop");
   Vector3 W(float x,float y,float z)=>game.Root.TransformPoint(new Vector3(x,y,z));
   yield return s.Split(q,game.Motion.Selected);int one=q.LastLeft,two=q.LastRight;
   yield return s.Walk(two,W(.33f,-.298f,-.24f),"One half on the right pad A");
   yield return s.Walk(one,W(-.10f,-.298f,-.25f),"The other half on the left pad A");
   yield return Until(10,()=>stop.AtEnd,"Both pads lift the stop");
   yield return Until(10,()=>glass.Sensor.Load>=.04f,"The ball rolls onto the glass");yield return Ticks(3);
   Assert.IsFalse(glass.Broken,"The ball alone does not break the glass");
   Vector3 delta=W(-.29f,-.165f,.19f)-game.Motion.Centre(one);int group=game.Matter.Groups[one];
   for(int i=0;i<game.Matter.Bodies.Length;i++)
   {
    if(game.Matter.Groups[i]!=group)continue;var body=game.Matter.Bodies[i];
    body.position+=delta;body.linearVelocity=body.angularVelocity=Vector3.zero;body.transform.position=body.position;
   }
   Physics.SyncTransforms();yield return Ticks(4);
   Assert.Greater(glass.Sensor.Load,.08f,"Half a body and the ball both weigh on the glass");
   Assert.IsTrue(glass.Cracked,"Half a body and the ball crack the glass");
   Assert.IsFalse(glass.Broken,$"Half a body and the ball do not break it (load {glass.Sensor.Load:F3})");
  }
 }
}
