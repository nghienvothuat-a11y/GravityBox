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
 }
}
