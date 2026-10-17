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

  // three retries in a row of the same level: COghe turns its back and sulks once the level is running again
  [UnityTest,Timeout(600000)] public IEnumerator ThreeRetriesInARowMakeCOgheSulk()
  {
   COgheTricks.AllForTests=true;
   try
   {
    yield return LoadPlus("G2");yield return Settle(1.5f);
    var p=game.Personality;
    for(int k=0;k<2;k++){p.NoteRetry();game.ResetLevel();yield return Settle(2.5f);}
    Assert.AreNotEqual(COgheTrick.BackTurnSulk,p.Trick,"Two retries: nothing yet");
    p.NoteRetry();game.ResetLevel();
    yield return TickUntil(()=>p.Act==COgheAct.Trick&&p.Trick==COgheTrick.BackTurnSulk,8,"The third retry: it turns its back");
   }
   finally{COgheTricks.AllForTests=false;}
  }

  // Batch 3 (Mrk 17/10/2026). A heavy job (K01's first push: crate 1 toward cell 2,2) ends in a flex, not a clap.
  [UnityTest,Timeout(600000)] public IEnumerator HeavyJobMakesCOgheFlex()
  {
   COgheTricks.AllForTests=true;
   try
   {
    yield return LoadPlus("K01");yield return Settle(1.5f);
    var p=game.Personality;
    var t=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().First(r=>r.Rail.name=="Crate 1");
    Assert.IsTrue(COgheTricks.Heavy(t),"A crate is heavy work");
    int before=t.CompletedJourneys;
    // the crate's top by the face toward the route cell, as the solve taps it
    Vector3 cell=game.Root.TransformPoint(new Vector3(COgheCrateRoutes.GridX+2.5f*COgheCrateRoutes.Cell,-.30f,COgheCrateRoutes.GridZ+2.5f*COgheCrateRoutes.Cell));
    Vector3 body=t.Rail.Body.position,along=t.Rail.WorldAxis,up=game.Root.up,across=Vector3.Cross(up,along).normalized,size=t.CrateSize,toCell=cell-body;
    float Half(Vector3 d){var l=t.Rail.Frame.InverseTransformDirection(d);return Mathf.Abs(l.x)*size.x*.5f+Mathf.Abs(l.z)*size.z*.5f;}
    float onAxis=Vector3.Dot(toCell,along);bool byEnd=Mathf.Abs(onAxis)>=Half(along)-.01f;
    Vector3 face=byEnd?along*Mathf.Sign(onAxis):across*Mathf.Sign(Vector3.Dot(toCell,across));float half=Half(face);
    foreach(var spot in new[]{body+face*half*.78f+up*size.y*.5f,body+face*half*.66f+up*size.y*.5f,body+face*half+up*size.y*.1f})
    {if(t.Busy)break;yield return Tap(spot);yield return Settle(.2f);}
    Assert.IsTrue(t.Busy,"COghe takes the crate");
    yield return TickUntil(()=>t.CompletedJourneys>before,40,"Crate 1 pushed");
    yield return TickUntil(()=>p.Act==COgheAct.Trick&&p.Trick==COgheTrick.Flex,8,"A flex after the push");
    Assert.AreEqual(0,p.TrickPlays[(int)COgheTrick.Clap],"Not a clap");
   }
   finally{COgheTricks.AllForTests=false;}
  }

  // A slide on the slick tape (G2: on its way to B COghe slips at the taped foot of the glass) ends in a skater's glide
  // once it stops, not in puffing up.
  [UnityTest,Timeout(600000)] public IEnumerator SlideOnTapeEndsInASkatersGlide()
  {
   COgheTricks.AllForTests=true;
   try
   {
    yield return LoadPlus("G2");yield return Settle(1.5f);
    var p=game.Personality;
    yield return Tap(game.Root.TransformPoint(new Vector3(.10f,-.03f,.20f)));
    yield return TickUntil(()=>p.SlideSeconds>.25f,10,"It slides on the tape");
    yield return TickUntil(()=>p.Act==COgheAct.Trick&&p.Trick==COgheTrick.Skater,20,"A skater's glide once it stops");
    Assert.AreEqual(0,p.TrickPlays[(int)COgheTrick.PuffUp],"Not puffing up");
   }
   finally{COgheTricks.AllForTests=false;}
  }

  // Home: with the yuck face COghe pulls a face at its food and shakes its head, but every ball is still eaten.
  [UnityTest,Timeout(600000)] public IEnumerator YuckFaceStillEatsEveryBall()
  {
   COgheTricks.AllForTests=true;COghePersonality.YuckForTests=true;
   try
   {
    yield return EnterFurnishedHome(65);yield return Frames(20);
    var p=game.Personality;game.FeedHome();
    for(int f=0;f<30*45&&game.HomeFeeding;f++)yield return Frames(1);
    Assert.IsFalse(game.HomeFeeding,"The meal is over");
    Assert.AreEqual(COgheFeedBalls.PerFeed,p.BallsEaten,"Every ball eaten, even the ones it pulled a face at");
    Assert.AreEqual(COgheFeedBalls.PerFeed-1,p.TrickPlays[(int)COgheTrick.YuckFace],"A face at every ball but the last");
   }
   finally{COgheTricks.AllForTests=false;COghePersonality.YuckForTests=false;COgheHomeRoom.UnlockedLevelOverride=null;}
  }

  // Home: the tail chase (Mrk 17/10/2026, instead of the screen lick) spins after the tail, eyes on it all the way round,
  // and leaves COghe where it was.
  [UnityTest,Timeout(600000)] public IEnumerator TailChaseSpinsAfterTheTail()
  {
   COgheTricks.AllForTests=true;
   try
   {
    yield return EnterFurnishedHome(65);yield return Frames(20);
    var p=game.Personality;var life=game.Matter.GetComponent<VenomLifeAnimation>();var start=game.Motion.Centre(0);
    p.PlayTrick(COgheTrick.TailChase);
    float turned=0;Vector3? last=null;
    for(int f=0;f<30*5&&p.Act==COgheAct.Trick&&p.Trick==COgheTrick.TailChase;f++)
    {
     yield return Frames(1);
     if(p.Time<.5f||p.Time>2.35f)continue;
     p.EyeMood(game.Matter.Groups[0],out var look);Assert.IsTrue(look.HasValue,"Eyes on the tail");
     var d=Vector3.ProjectOnPlane(life.TrickFocus-game.Motion.Centre(0),game.HomeRoom.Root.up);
     if(last.HasValue)turned+=Mathf.Abs(Vector3.SignedAngle(last.Value,d,game.HomeRoom.Root.up));last=d;
    }
    Assert.Greater(turned,540,"Round and round after the tail");
    Assert.Less(Vector3.Distance(start,game.Motion.Centre(0)),.03f,"Presentation only: the body stays put");
   }
   finally{COgheTricks.AllForTests=false;COgheHomeRoom.UnlockedLevelOverride=null;}
  }
 }
}
