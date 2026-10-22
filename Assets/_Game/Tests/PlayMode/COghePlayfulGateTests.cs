using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 public sealed partial class COgheSpatialCampaignTests
 {
  // Playful (Emotion): a trick plays in a level only once its card level unlocks it (two a level, in enum order).
  // Emotion 0: a tap on COghe gives no wink. Emotion 1 (Groove, Wink): a wink, but no raspberry on slick tape.
  // Emotion 2: the raspberry too.
  [UnityTest,Timeout(900000)] public IEnumerator PlayfulTricksWaitForTheirEmotionLevel()
  {
   bool Saw(COgheTrick t)=>game.Personality.Act==COgheAct.Trick&&game.Personality.Trick==t;
   IEnumerator Watch(COgheTrick t,float seconds,System.Action<bool> seen)
   {bool any=false;for(int i=0;i<seconds/Dt;i++){Tick();any|=Saw(t);if(i%2==1)yield return null;}seen(any);}
   IEnumerator TapBody(){yield return Tap(game.Motion.Centre(0)+game.Root.up*.01f);}
   IEnumerator TapSlick(){yield return Tap(game.Root.TransformPoint(new Vector3(.40f,-.265f,.15f)));}
   bool wink=false,raspberry=false;
   try
   {
    foreach(int emotion in new[]{0,1,2})
    {
     COgheShop.ResetForTests(new COgheShop.State{Emotion=emotion});
     Assert.AreEqual(2*emotion,COgheAbilities.MoodsUnlocked);
     yield return LoadPlus("G2");yield return Settle(1.5f);Assert.IsNotNull(game.Personality);
     yield return TapBody();yield return Watch(COgheTrick.Wink,8,s=>wink=s);
     yield return Settle(8);
     yield return TapSlick();yield return Watch(COgheTrick.Raspberry,10,s=>raspberry=s);
     Debug.Log($"PLAYFUL emotion {emotion}: wink {wink} raspberry {raspberry}");
     Assert.AreEqual(emotion>=1,wink,$"Emotion {emotion}: wink on a body tap");
     Assert.AreEqual(emotion>=2,raspberry,$"Emotion {emotion}: raspberry on slick tape");
    }
   }
   finally{COgheShop.ResetForTests();}
  }
 }
}
