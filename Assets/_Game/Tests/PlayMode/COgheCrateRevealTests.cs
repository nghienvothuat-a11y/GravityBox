using System;
using System.Collections;
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
  [UnityTest] public IEnumerator CrateRevealExposesFloorExitAndResets()
  {
   yield return LoadPlus("K01");
   var root=game.Owner.Apparatus;
   var tasks=root.GetComponentsInChildren<COgheTapRail>();
   Assert.IsTrue(tasks.All(t=>t.CrateFaces),"Display 6 has crates only, no switch rail");
   Assert.IsEmpty(root.GetComponentsInChildren<COgheTissueSensor>());
   Assert.IsEmpty(root.GetComponentsInChildren<COgheTapPad>());
   Assert.IsEmpty(root.GetComponentsInChildren<COgheLoadLatch>());
   var red=tasks.Single(t=>t.Rail.name=="Red crate");
   var surfaces=root.GetComponentsInChildren<VenomSurfacePatch>();
   var floor=surfaces.Single(p=>p.name=="Laboratory floor");
   Assert.IsTrue(floor.Hole);
   Assert.Less(Vector3.Distance(floor.transform.TransformPoint(new Vector3(floor.HoleCentre.x,floor.HoleCentre.y,0)),game.Owner.Outlet.position),.001f);
   Assert.Greater(Vector3.Dot(game.Owner.Outlet.forward,-game.Root.up),.999f);
   Assert.IsTrue(surfaces.Where(p=>p.ExteriorGlass).All(p=>!p.Hole&&!p.Slippery),"Outer glass stays intact and grippy");
   Vector3 start=red.Rail.Body.position;
   bool Covered()
   {
    var local=red.Rail.Body.transform.InverseTransformPoint(game.Owner.Outlet.position);
    float r=game.Owner.ApertureRadius;
    return Mathf.Abs(local.x)+r<red.CrateSize.x*.5f&&Mathf.Abs(local.z)+r<red.CrateSize.z*.5f;
   }
   Assert.IsTrue(Covered(),"The red crate fully covers the floor hole at spawn");
   Capture(6,"start");
   // Touching the hidden exit must not let the creature pass through the red crate.
   yield return Tap(game.Owner.Outlet.position);yield return Wait(4);
   Assert.AreEqual(0,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Completed);
   game.ResetLevel();yield return Wait(1);
   bool revealed=false;
   IEnumerator Observe(float seconds,Func<bool> done,string reason)
   {
    yield return Until(seconds,done,reason);
    if(reason=="Pull 2: crate 0 reaches its other stop")
    {
     revealed=true;Assert.IsFalse(Covered());Assert.Greater(Vector3.Distance(start,red.Rail.Body.position),.20f);
     Capture(6,"revealed");
    }
   }
   yield return new COgheSpatialScenario(game,Tap,Observe).Solve();
   Assert.IsTrue(revealed);Assert.AreEqual(32,game.Matter.EscapedCount);
   Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.IsFalse(game.Owner.Lost);
   Capture(6,"won");
   game.ResetLevel();yield return Wait(.5f);
   Assert.Less(Vector3.Distance(start,red.Rail.Body.position),.003f);Assert.IsTrue(Covered());
   Assert.AreEqual(0,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Completed);
  }
 }
}
