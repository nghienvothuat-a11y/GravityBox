using System.Collections;
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
  // Crate levels 51–60 (06/10/2026).
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
