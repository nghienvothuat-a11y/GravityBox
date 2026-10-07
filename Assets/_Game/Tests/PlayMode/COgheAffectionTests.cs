using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // COghe's affection at Home (Mrk 07/10/2026: "tăng cường animation tương tác, thể hiện tình cảm"). Presentation only.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator HomeGentleTouchesWarmItUpToAHug()
  {
   try
   {
    yield return EnterFurnishedHome(12);yield return Frames(30*4);
    var p=game.Personality;bool hearts=false;p.TakeAffectionHearts();
    // three gentle touches, 1.2 s apart: a playful reaction, then it leans in (or a heart), then a hug with hearts
    for(int i=0;i<3;i++){p.TouchedInHome(game.Motion.Centre(0));for(int f=0;f<36;f++){yield return Frames(1);hearts|=p.TakeAffectionHearts();}}
    for(int f=0;f<60;f++){yield return Frames(1);hearts|=p.TakeAffectionHearts();}
    Assert.IsFalse(p.Sulking,"Gentle touches are not pokes");Assert.IsTrue(hearts,"The third touch: a hug, hearts flying");
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;}
  }

  [UnityTest] public IEnumerator HomeVisitStartsWithAHello()
  {
   try
   {
    yield return EnterFurnishedHome(12);var p=game.Personality;bool waved=false;
    for(int f=0;f<30*20&&!waved;f++){yield return Frames(1);waved=p.Act==COgheAct.Wave;}
    Assert.IsTrue(waved,"It comes to the front and waves hello");
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;}
  }
 }
}
