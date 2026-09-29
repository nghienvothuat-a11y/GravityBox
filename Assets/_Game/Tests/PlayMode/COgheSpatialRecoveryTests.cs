using System;
using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator RailTrip(string label)
  {
   var task=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),x=>x.Label==label);int before=task.CompletedJourneys;
   yield return Tap(task.HandPoint+Vector3.up*.004f);yield return Until(30,()=>task.CompletedJourneys>before,"Reversible rail "+label);
  }
  [UnityTest] public IEnumerator MechanismsStartClearOfFixedScenery()
  {
   for(int n=1;n<=10;n++)
   {
    yield return Load(n);game.ResetLevel();Physics.SyncTransforms();
    foreach(var prop in game.Props)foreach(var shape in prop.CollisionShapes)foreach(var patch in game.Surfaces)
    {
     if(patch.Shape.attachedRigidbody!=game.Root.GetComponent<Rigidbody>())continue;
     if(Physics.ComputePenetration(shape,shape.transform.position,shape.transform.rotation,patch.Shape,patch.transform.position,patch.transform.rotation,out _,out float depth))
      Assert.Less(depth,.0005f,$"Level {n}: {prop.name} starts in {patch.name}");
    }
   }
  }
  [UnityTest] public IEnumerator NoIdleMechanismOpensAndCatalogIsIsolated()
  {
   for(int n=1;n<=10;n++)
   {
    yield return Load(n);yield return Wait(5);Assert.IsFalse(game.Owner.Completed);Assert.IsFalse(game.Owner.Lost);
    Assert.AreEqual(10,game.PlayableLevelCount);Assert.AreEqual("coghe.spatial.pilot",game.Definition.ProgressKey);
    Assert.IsTrue(game.Definition.ViewOnly);Assert.IsFalse(game.Definition.CanRotate);
    foreach(var r in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())Assert.Less(r.Position,.009f,"No spontaneous motion: "+r.name);
    if(n==10){Assert.IsTrue(game.Definition.Boss);Assert.IsEmpty(game.Definition.Lesson);}
   }
  }
  [UnityTest] public IEnumerator PulleyCanBeReversedAndReset()
  {
   yield return Load(6);yield return RailTrip("A");var cable=game.Owner.Apparatus.GetComponentInChildren<COghePulleyDrive>();
   yield return Until(10,()=>cable.Output.AtEnd,"Raised bridge holds");yield return Wait(2);Assert.IsTrue(cable.Output.AtEnd);
   yield return RailTrip("A");yield return Until(15,()=>cable.Output.Position<.012f,"Release and lower via gravity");
   game.ResetLevel();yield return Wait(1);Assert.Less(cable.Input.Position,.004f);Assert.Less(cable.Output.Position,.004f);Assert.IsFalse(cable.Input.Locked);
  }
  [UnityTest] public IEnumerator ElevatorRoundTripCarriesAllTissue()
  {
   yield return Load(8);var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();yield return Tap(lift.Panel.position);
   yield return Until(35,()=>lift.Trips==1,"Raise passenger");Assert.Greater(game.Motion.Centre(0).y,.03f);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
   for(int i=0;i<32;i++)Assert.Greater(game.Matter.Bodies[i].position.y,lift.Rail.Body.position.y,"Every particle carried");
   yield return Tap(lift.Panel.position);yield return Until(35,()=>lift.Trips==2,"Lower passenger");Assert.Less(lift.Rail.Position,.004f);Assert.Less(game.Motion.Centre(0).y,-.20f);
   game.ResetLevel();Assert.IsFalse(lift.Moving);Assert.IsFalse(lift.Boarding);Assert.AreEqual(0,lift.Trips);
  }
  [UnityTest] public IEnumerator BoardingCanBeCancelledByAnotherTap()
  {
   yield return Load(8);var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();yield return Tap(lift.Panel.position);
   Assert.IsTrue(lift.Boarding);yield return Tap(new Vector3(-.26f,-.30f,-.10f));yield return Wait(5);
   Assert.IsFalse(lift.Boarding);Assert.IsFalse(lift.Moving);Assert.AreEqual(0,lift.Trips);Assert.Less(lift.Rail.Position,.004f);
  }
 }
}
