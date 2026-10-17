using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // "Hiểu ra" toys and gags (Mrk 17/10/2026): chapter 2 ends with a drum game the player keeps; a wrong answer is a little
 // comedy about what COghe got instead. Driven through the player's own paths (taps on the screen, Feed, an item).
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator TapDrum()
  {
   var room=game.HomeRoom;var b=room.BoundsOf(room.Find(COgheToys.Drum));
   game.TouchPoint(game.Owner.View.WorldToScreenPoint(b.center+room.Root.up*.015f));
   yield return Frames(1);
  }
  private IEnumerator HomeWithoutTheDrum()
  {
   COgheShop.ResetForTests(new COgheShop.State{Migrated=true});COgheShop.TestsOwnUnlocked=false;
   yield return EnterFurnishedHome(65);yield return Frames(20);
  }

  [Test] public void ChapterTwoEndsWithTheDrumGame()
  {
   var rounds=COgheBonus.Rounds(2);
   Assert.IsTrue(rounds[2].IsGame,"The last question is a game");Assert.AreEqual(COgheToys.Drum,rounds[2].Toy);
   Assert.AreEqual(2,COgheToys.Catalog[0].chapter);Assert.AreEqual(COgheToys.Drum,COgheToys.OfChapter(2));
   for(int c=1;c<=6;c++)for(int r=0;r<2;r++)Assert.IsFalse(COgheBonus.Rounds(c)[r].IsGame,"Only a last question is a game");
   foreach(var item in COgheEconomy.Items)Assert.IsFalse(COgheToys.Is(item.Id),"Toys are never sold");
  }

  [UnityTest] public IEnumerator DrumGameIsPlayedBackAndTheDrumIsKept()
  {
   try
   {
    yield return HomeWithoutTheDrum();
    var p=game.Personality;var room=game.HomeRoom;var drum=room.Find(COgheToys.Drum);
    Assert.IsNotNull(drum);Assert.IsFalse(drum.Root.activeSelf,"Not the player's yet: not in the room");
    p.BeginBonus(COgheBonus.Rounds(2),2);
    yield return BonusUntil(()=>p.ToyListening,15,"COghe brings the drum out and plays the first beat");
    Assert.IsTrue(drum.Root.activeSelf,"Out for the game");Assert.AreEqual(2,p.ToyBeats);Assert.AreEqual(0,p.ToyPattern);
    // a touch on COghe instead: it giggles and points at the drum, then plays the beat again
    p.TouchedInHome(p.SkinCentre);Assert.AreEqual(COgheGag.GigglePoint,p.Gag);
    yield return BonusUntil(()=>p.ToyListening,10,"The beat again");
    // played back by taps on the drum on screen (only the number counts)
    yield return TapDrum();yield return Frames(8);yield return TapDrum();
    Assert.AreEqual(2,p.ToyTaps);
    yield return BonusUntil(()=>p.ToyListening&&p.ToyPattern==1,10,"Played back: the next beat");
    Assert.AreEqual(3,p.ToyBeats);
    // one strike too many: it covers its ears, and plays the same beat again
    for(int i=0;i<4;i++){yield return TapDrum();yield return Frames(4);}
    Assert.AreEqual(COgheGag.CoverEars,p.Gag);Assert.AreEqual(1,p.ToyPattern);
    yield return BonusUntil(()=>p.ToyListening,10,"The same beat again");
    Assert.AreEqual(0,p.ToyTaps);
    for(int i=0;i<3;i++){yield return TapDrum();yield return Frames(8);}
    yield return BonusUntil(()=>p.ToyListening&&p.ToyPattern==2,10,"The last beat");
    Assert.AreEqual(4,p.ToyBeats);
    for(int i=0;i<4;i++){yield return TapDrum();yield return Frames(8);}
    yield return BonusUntil(()=>p.ToyJamming,10,"They jam together");
    yield return TapDrum();
    yield return BonusUntil(()=>p.BonusThanking||p.BonusFinished,15,"Won: COghe's joy, then its thank-you");
    Assert.IsTrue(COgheToys.Owns(COgheToys.Drum),"The drum is the player's");Assert.IsTrue(COgheShop.Current.Owned.Contains(COgheToys.Drum));
    yield return BonusUntil(()=>p.BonusFinished,15,"The thank-you");
    p.EndBonus();Assert.IsTrue(drum.Root.activeSelf,"It stays in the room");
    // at Home, a tap on it: COghe goes over and plays it
    p.PlayWith(drum);
    yield return BonusUntil(()=>p.Playing==drum,10,"It goes to play the drum");
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  [UnityTest] public IEnumerator LeavingTheDrumGamePutsTheDrumAway()
  {
   try
   {
    yield return HomeWithoutTheDrum();
    var p=game.Personality;var drum=game.HomeRoom.Find(COgheToys.Drum);
    p.BeginBonus(COgheBonus.Rounds(2),2);
    yield return BonusUntil(()=>p.ToyListening,15,"The first beat");
    p.EndBonus();
    Assert.IsFalse(drum.Root.activeSelf,"Left before it was won: the drum goes away");Assert.IsFalse(COgheToys.Owns(COgheToys.Drum));
    // and the game resumes from its question
    p.BeginBonus(COgheBonus.Rounds(2),2);
    yield return BonusUntil(()=>p.ToyListening&&drum.Root.activeSelf,15,"Back to the drum game");
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  [UnityTest] public IEnumerator WrongAnswersAreGagsAboutWhatCOgheGot()
  {
   try
   {
    yield return StartBonusInHome(1);var p=game.Personality;var room=game.HomeRoom;
    var ball=room.Find("BALL");var part=ball.Part("Ball");Vector3 rest=part.localPosition;
    // a heart (a cuddle): food instead → it catches the ball, puffs its cheeks and spits it out; no meal is thrown
    p.BonusFeed();Assert.AreEqual(COgheGag.Spit,p.Gag);Assert.AreEqual(1,game.HomeFeedBalls.Balls.Count,"Only the ball it spits out: a wrong Feed throws no meal");
    yield return BonusUntil(()=>p.BonusAsking,6,"Spat out: it asks again");
    Assert.IsFalse(game.HomeFeedBalls.Active,"The spat ball is gone");
    // the ball instead → it bonks COghe on the head and rolls back to its spot
    p.PlayWith(ball);Assert.AreEqual(COgheGag.Bonk,p.Gag);
    yield return BonusUntil(()=>p.BonusAsking,6,"Bonked: it asks again");
    Assert.Less(Vector3.Distance(rest,part.localPosition),1e-3f,"The ball is back on its spot");
    Assert.AreEqual(0,p.BonusRound);Assert.AreEqual(2,p.BonusMisses);
    p.TouchedInHome(p.SkinCentre);
    yield return BonusUntil(()=>p.BonusRound==1&&p.BonusAsking,10,"Right: the next question (the ball)");
    // a touch when it wants the ball → it giggles, then points at the ball
    p.TouchedInHome(p.SkinCentre);Assert.AreEqual(COgheGag.GigglePoint,p.Gag);
    yield return BonusUntil(()=>p.BonusAsking,6,"It asks again");
    Assert.AreEqual(3,p.GagsPlayed);
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;}
  }
 }
}
