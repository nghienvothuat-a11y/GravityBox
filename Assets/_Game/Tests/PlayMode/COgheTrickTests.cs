using System;
using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // Emotion tricks (ability cards, Mrk 16/10/2026): each plays on its own trigger in a level, as presentation only.
 public sealed partial class COgheSpatialCampaignTests
 {
  // a frame every two physics steps, so COghe's character (which runs per frame) keeps up
  private IEnumerator TickUntil(Func<bool> done,float seconds,string why)
  {
   for(int i=0;i<seconds/Dt&&!done();i++){Tick();if(i%2==1)yield return null;}
   var p=game.Personality;
   Assert.IsTrue(done(),why+$"; act {p?.Act} {p?.Trick} body {game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3}");
  }
  private IEnumerator Settle(float seconds){for(int i=0;i<seconds/Dt;i++){Tick();if(i%2==1)yield return null;}}

  [UnityTest,Timeout(900000)] public IEnumerator EmotionTricksFollowTheirTriggers()
  {
   COgheTricks.AllForTests=true;
   try
   {
    yield return LoadPlus("G2");yield return Settle(1.5f);
    var p=game.Personality;Assert.IsNotNull(p,"COghe's character runs in G2");
    var particles=game.Matter.Bodies.Select(b=>b.position).ToArray();
    // a tap on COghe itself: a wink
    yield return Tap(game.Motion.Centre(0)+game.Root.up*.01f);
    yield return TickUntil(()=>p.Act==COgheAct.Trick&&p.Trick==COgheTrick.Wink,8,"A tap on COghe: it winks");
    yield return TickUntil(()=>p.Act==COgheAct.None,6,"The wink ends");
    // slick tape it cannot climb (the foot of the right glass): a raspberry
    yield return Settle(8);
    yield return Tap(game.Root.TransformPoint(new Vector3(.40f,-.265f,.15f)));
    yield return TickUntil(()=>p.Act==COgheAct.Trick&&p.Trick==COgheTrick.Raspberry,10,"Sent onto slick glass: a raspberry");
    yield return TickUntil(()=>p.Act==COgheAct.None,6,"The raspberry ends");
    // a device it worked: a clap once it is done
    yield return Settle(8);
    var a=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().First(r=>r.Label=="A");
    yield return Tap(a.HandPoint);
    yield return TickUntil(()=>a.CompletedJourneys>0,30,"A is pulled");
    yield return TickUntil(()=>p.Act==COgheAct.Trick&&p.Trick==COgheTrick.Clap,8,"A device done: a clap");
    yield return TickUntil(()=>p.Act==COgheAct.None,6,"The clap ends");
    // up to the high shelf (A opened the lane), then down off it over the slick glass: a fall, and it puffs up
    yield return Settle(8);
    yield return Tap(game.Root.TransformPoint(new Vector3(.10f,-.03f,.20f)));
    yield return TickUntil(()=>game.Root.InverseTransformPoint(game.Motion.Centre(0)).y>-.08f,40,"Up on the high shelf");
    yield return Settle(9);
    yield return Tap(game.Root.TransformPoint(new Vector3(.10f,-.30f,.02f)));
    yield return TickUntil(()=>p.Act==COgheAct.Trick&&p.Trick==COgheTrick.PuffUp,15,"Off the shelf, a fall: it puffs up");
    yield return TickUntil(()=>p.Act==COgheAct.None,6,"The puff ends");
    Assert.IsFalse(game.Owner.Lost);
   }
   finally{COgheTricks.AllForTests=false;}
  }
 }
}
