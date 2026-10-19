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

  private IEnumerator Ticks(float seconds){for(int i=0;i<seconds/Dt;i++){Tick();if(i%60==0)yield return null;}}
  private IEnumerator PullHandle(string label)
  {
   var task=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),t=>t.Label==label);
   int before=task.CompletedJourneys;
   yield return Tap(task.Handle.position);
   for(int i=0;i<30/Dt&&task.CompletedJourneys==before;i++){Tick();if(i%60==0)yield return null;}
   Assert.Greater(task.CompletedJourneys,before,"Handle "+label+" pulled");
  }

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

  // 68: the glass holds COghe alone (it cracks) and the ball alone; only the two together break it.
  [UnityTest,Timeout(900000)] public IEnumerator PhysicsF3GlassHoldsEachAlone()
  {
   yield return LoadPlus("F3");yield return Wait(.5f);
   var glass=game.Owner.Apparatus.GetComponentInChildren<COgheBreakableGlass>();
   yield return Tap(game.Root.TransformPoint(new Vector3(-.25f,-.19f,.10f)));yield return Ticks(15);
   Assert.IsTrue(glass.Cracked,"COghe on the glass cracks it");Assert.IsFalse(glass.Broken,"COghe alone does not break it");
   game.ResetLevel();yield return Wait(.5f);
   Assert.IsFalse(glass.Broken||glass.Cracked,"Retry mends the glass");
   yield return PullHandle("D");yield return Ticks(10);
   Assert.Greater(glass.Sensor.Load,.04f,"The ball rests on the glass");Assert.IsFalse(glass.Broken,"The ball alone does not break it");
  }
 }
}
