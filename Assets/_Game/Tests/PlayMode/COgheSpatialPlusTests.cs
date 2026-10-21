using System.Collections;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Spatial Plus (E01–E18, B1, B2): each level solved with real taps from its start, then Retry returns it to the start.
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator LoadPlus(string key){yield return LoadScene("COgheSpatialPlus"+key);}
  private IEnumerator SolvePlus(string key)
  {
   yield return LoadPlus(key);var rotation=game.Root.rotation;
   Capture(game.Definition.Order,"start");
   yield return new COgheSpatialScenario(game,Tap,Until).Solve();
   Assert.AreEqual(32,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.IsFalse(game.Owner.Lost);
   Assert.Less(Quaternion.Angle(rotation,game.Root.rotation),.001f);Assert.IsTrue(game.Progress.Completed.Contains(game.Definition.Id));
   game.ResetLevel();Assert.AreEqual(0,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Completed);Assert.IsFalse(game.Owner.Lost);
  }
  [UnityTest] public IEnumerator SpatialPlusE01Solve(){yield return SolvePlus("E01");}
  [UnityTest] public IEnumerator SpatialPlusE02Solve(){yield return SolvePlus("E02");}
  [UnityTest] public IEnumerator SpatialPlusE03Solve(){yield return SolvePlus("E03");}
  [UnityTest] public IEnumerator SpatialPlusE04Solve(){yield return SolvePlus("E04");}
  [UnityTest] public IEnumerator SpatialPlusE05Solve(){yield return SolvePlus("E05");}
  [UnityTest] public IEnumerator SpatialPlusE06Solve(){yield return SolvePlus("E06");}
  [UnityTest] public IEnumerator SpatialPlusE07Solve(){yield return SolvePlus("E07");}
  [UnityTest] public IEnumerator SpatialPlusB1Solve(){yield return SolvePlus("B1");}
  [UnityTest] public IEnumerator SpatialPlusE08Solve(){yield return SolvePlus("E08");}
  [UnityTest] public IEnumerator SpatialPlusE09Solve(){yield return SolvePlus("E09");}
  [UnityTest] public IEnumerator SpatialPlusE10Solve(){yield return SolvePlus("E10");}
  [UnityTest] public IEnumerator SpatialPlusE11Solve(){yield return SolvePlus("E11");}
  [UnityTest] public IEnumerator SpatialPlusE12Solve(){yield return SolvePlus("E12");}
  [UnityTest] public IEnumerator SpatialPlusE13Solve(){yield return SolvePlus("E13");}
  [UnityTest] public IEnumerator SpatialPlusE14Solve(){yield return SolvePlus("E14");}
  [UnityTest] public IEnumerator SpatialPlusE15Solve(){yield return SolvePlus("E15");}
  [UnityTest] public IEnumerator SpatialPlusE16Solve(){yield return SolvePlus("E16");}
  [UnityTest] public IEnumerator SpatialPlusE17Solve(){yield return SolvePlus("E17");}
  [UnityTest] public IEnumerator SpatialPlusE18Solve(){yield return SolvePlus("E18");}
  [UnityTest] public IEnumerator SpatialPlusB2Solve(){yield return SolvePlus("B2");}
  // Chapter 2 rebuilt (05/10/2026). N18's route asserts a half gives up on the 100% crate before the whole body moves it.
  [UnityTest] public IEnumerator SpatialPlusN13Solve(){yield return SolvePlus("N13");}
  [UnityTest] public IEnumerator SpatialPlusN18Solve(){yield return SolvePlus("N18");}
  [UnityTest] public IEnumerator SpatialPlusN15Solve(){yield return SolvePlus("N15");}
  // The glass-crawl levels (12/10/2026).
  [UnityTest] public IEnumerator SpatialPlusG1Solve(){yield return SolvePlus("G1");}
  // Glass roads, prototypes 66–68 (Mrk 18/10/2026).
  [UnityTest] public IEnumerator SpatialPlusG6Solve(){yield return SolvePlus("G6");}
  [UnityTest] public IEnumerator SpatialPlusG7Solve(){yield return SolvePlus("G7");}
  [UnityTest] public IEnumerator SpatialPlusG8Solve(){yield return SolvePlus("G8");}
  [UnityTest] public IEnumerator SpatialPlusG9Solve(){yield return SolvePlus("G9");}
  [UnityTest] public IEnumerator SpatialPlusG10Solve(){yield return SolvePlus("G10");}
  [UnityTest] public IEnumerator SpatialPlusG11Solve(){yield return SolvePlus("G11");}
  [UnityTest] public IEnumerator SpatialPlusF1Solve(){yield return SolvePlus("F1");}
  [UnityTest] public IEnumerator SpatialPlusF2Solve(){yield return SolvePlus("F2");}
  [UnityTest] public IEnumerator SpatialPlusF3Solve(){yield return SolvePlus("F3");}
  [UnityTest] public IEnumerator SpatialPlusG2Solve(){yield return SolvePlus("G2");}
  [UnityTest] public IEnumerator SpatialPlusG3Solve(){yield return SolvePlus("G3");}
  [UnityTest] public IEnumerator SpatialPlusG4Solve(){yield return SolvePlus("G4");}
  [UnityTest] public IEnumerator SpatialPlusG5Solve(){yield return SolvePlus("G5");}
  // The crate levels K01–K10 (06/10/2026).
  [UnityTest] public IEnumerator SpatialPlusK01Solve(){yield return SolvePlus("K01");}
  [UnityTest] public IEnumerator SpatialPlusK02Solve(){yield return SolvePlus("K02");}
  [UnityTest] public IEnumerator SpatialPlusK03Solve(){yield return SolvePlus("K03");}
  [UnityTest] public IEnumerator SpatialPlusK04Solve(){yield return SolvePlus("K04");}
  [UnityTest] public IEnumerator SpatialPlusK05Solve(){yield return SolvePlus("K05");}
  [UnityTest] public IEnumerator SpatialPlusK06Solve(){yield return SolvePlus("K06");}
  [UnityTest] public IEnumerator SpatialPlusK07Solve(){yield return SolvePlus("K07");}
  [UnityTest] public IEnumerator SpatialPlusK08Solve(){yield return SolvePlus("K08");}
  [UnityTest] public IEnumerator SpatialPlusK09Solve(){yield return SolvePlus("K09");}
  [UnityTest] public IEnumerator SpatialPlusK10Solve(){yield return SolvePlus("K10");}
  [UnityTest] public IEnumerator SpatialPlusN19Solve(){yield return SolvePlus("N19");}
  // Chapter 3 rebuilt (06/10/2026).
  [UnityTest] public IEnumerator SpatialPlusN22Solve(){yield return SolvePlus("N22");}
  [UnityTest] public IEnumerator SpatialPlusN29Solve(){yield return SolvePlus("N29");}
  [UnityTest] public IEnumerator SpatialPlusN23Solve(){yield return SolvePlus("N23");}
  [UnityTest] public IEnumerator SpatialPlusN25Solve(){yield return SolvePlus("N25");}
  [UnityTest] public IEnumerator SpatialPlusN26Solve(){yield return SolvePlus("N26");}
  [UnityTest] public IEnumerator SpatialPlusN31Solve(){yield return SolvePlus("N31");}
  [UnityTest] public IEnumerator SpatialPlusN33Solve(){yield return SolvePlus("N33");}
  [UnityTest] public IEnumerator SpatialPlusN35Solve(){yield return SolvePlus("N35");}
  [UnityTest] public IEnumerator SpatialPlusN32Solve(){yield return SolvePlus("N32");}
  // Level 58 (21/10/2026): back to B's handle in its bay, the route ran over B along its slick edge; the body straddled the
  // edge and stuck (fed at a 2 % slower or a 0.5 % faster walk, and with stops for breath). B's handle is reached round B.
  [UnityTest] public IEnumerator SpatialPlusN32ReachesBRoundTheBlockAtOtherPaces()
  {
   foreach(var (speed,hunger) in new[]{(.98f,COgheHunger.Fed),(.995f,COgheHunger.Fed),(1.005f,COgheHunger.Fed),(1f,COgheHunger.Exhausted)})
   {
    yield return LoadPlus("N32");game.Motion.SpeedScale=speed;COgheEnergy.ApplyRests(game.Motion,hunger);
    var b=System.Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),t=>t.Label=="B");bool over=false;
    System.Func<float,System.Func<bool>,string,IEnumerator> until=(seconds,done,reason)=>Until(seconds,()=>
    {
     var o=b.Busy?game.Motion.Get(b.Actor):null;
     if(o!=null&&b.Phase==COgheTapRail.TaskPhase.Approaching&&b.Rail.Position<.01f)
      foreach(var p in o.Path)if(p.y>-.265f)over=true;   // a waypoint on B's top (floor waypoints are at -.28)
     return done();
    },reason);
    try{yield return new COgheSpatialScenario(game,Tap,until).Solve();}
    finally{game.Motion.SpeedScale=1;COgheEnergy.ApplyRests(game.Motion,COgheHunger.Fed);}
    Assert.IsTrue(game.Owner.Completed,$"Level 58 solved at x{speed} {hunger}");
    Assert.IsFalse(over,$"At x{speed} {hunger} the walk to B's handle in its bay went over B");
   }
  }
  // Levels 53 and 41 (21/10/2026): from the balcony handle (C; A on 41) to the balcony tube mouth the route ran over the
  // handle's carriage, and the body was pinned on its front face (fed at a 2 % slower or faster walk; level 53 also in the
  // gift-box test after other tests). The walk to the mouth goes round the carriage.
  private IEnumerator BalconyMouthRoundTheCarriage(string scene,params float[] speeds)
  {
   foreach(float speed in speeds)
   {
    yield return LoadScene(scene);game.Motion.SpeedScale=speed;
    var tube=game.Owner.Apparatus.GetComponentInChildren<COgheTubeNetwork>();bool over=false;
    System.Func<float,System.Func<bool>,string,IEnumerator> until=(seconds,done,reason)=>Until(seconds,()=>
    {
     int actor=game.Motion.Selected;var o=game.Motion.Get(actor);
     if(o!=null&&tube.IsApproachingEntry(actor,2))foreach(var p in o.Path)if(p.y>-.08f)over=true;   // on the carriage (the balcony's waypoints are at -.10)
     return done();
    },reason);
    try{yield return new COgheSpatialScenario(game,Tap,until).Solve();}
    finally{game.Motion.SpeedScale=1;}
    Assert.IsTrue(game.Owner.Completed,$"{scene} solved at x{speed}");
    Assert.IsFalse(over,$"{scene} at x{speed}: the walk to the balcony mouth went over the handle's carriage");
   }
  }
  [UnityTest] public IEnumerator SpatialPlusN43ReachesTheBalconyMouthRoundC(){yield return BalconyMouthRoundTheCarriage("COgheSpatialPlusN43",.97f,.98f,1.01f,1.02f,1.7f);}
  [UnityTest] public IEnumerator Spatial15ReachesTheBalconyMouthRoundA(){yield return BalconyMouthRoundTheCarriage("COgheSpatial15",.98f,.85f,1.1f,1.5f);}
  // The same level with a frame every 2 or 4 physics steps (a 60 or 30 fps phone), as the glass-crawl levels.
  [UnityTest,Timeout(900000)] public IEnumerator SpatialPlusN32SolveAt60And30Fps()
  {
   yield return GlassCrawlSolveAtFrameRate("N32");yield return GlassCrawlSolveAtFrameRate("N32",30);
  }
  [UnityTest] public IEnumerator SpatialPlusN40Solve(){yield return SolvePlus("N40");}
  [UnityTest] public IEnumerator SpatialPlusN34Solve(){yield return SolvePlus("N34");}
  [UnityTest] public IEnumerator SpatialPlusN44Solve(){yield return SolvePlus("N44");}
  [UnityTest] public IEnumerator SpatialPlusN45Solve(){yield return SolvePlus("N45");}
  // Chapter 5, part 2 (06/10/2026). N49HalfStalls: the wrong split (half on the lock pad) stalls the door part way.
  [UnityTest] public IEnumerator SpatialPlusN41Solve(){yield return SolvePlus("N41");}
  [UnityTest] public IEnumerator SpatialPlusN42Solve(){yield return SolvePlus("N42");}
  [UnityTest] public IEnumerator SpatialPlusN47Solve(){yield return SolvePlus("N47");}
  [UnityTest] public IEnumerator SpatialPlusN49Solve(){yield return SolvePlus("N49");}
  [UnityTest] public IEnumerator SpatialPlusN50Solve(){yield return SolvePlus("N50");}
  [UnityTest] public IEnumerator SpatialPlusN48Solve(){yield return SolvePlus("N48");}
  [UnityTest] public IEnumerator SpatialPlusN46Solve(){yield return SolvePlus("N46");}
  [UnityTest] public IEnumerator SpatialPlusN43Solve(){yield return SolvePlus("N43");}
  [UnityTest] public IEnumerator SpatialPlusN49HalfStalls(){yield return LoadPlus("N49");yield return new COgheSpatialPlusScenario(game,Tap,Until).N49HalfStalls();}
 }
}
