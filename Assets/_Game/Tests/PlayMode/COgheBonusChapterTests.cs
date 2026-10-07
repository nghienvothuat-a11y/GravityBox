using System;
using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // The bonus "Hiểu ra", chapters 2–5 (Mrk 07/10/2026): two things in any order, an order, an exact number, all of it.
 // COghe is driven through its own answer paths (a touch on it, Feed, an item); Frames(1) is 1/30 s of simulation.
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator BonusUntil(Func<bool> done,float seconds,string what)
  {
   for(int i=0;i<seconds*30&&!done();i++)yield return Frames(1);
   Assert.IsTrue(done(),what);
  }
  private IEnumerator StartBonusInHome(int chapter)
  {
   yield return EnterFurnishedHome(60);yield return Frames(20);
   game.Personality.BeginBonus(COgheBonus.Rounds(chapter));
   yield return BonusUntil(()=>game.Personality.BonusAsking,10,"COghe walks to the middle and asks");
  }

  [Test] public void EveryChapterHasABonusMadeOfWhatEveryPlayerHas()
  {
   for(int chapter=1;chapter<=5;chapter++)
   {
    var rounds=COgheBonus.Rounds(chapter);Assert.IsNotNull(rounds,"Chapter "+chapter);Assert.AreEqual(3,rounds.Length);
    foreach(var round in rounds)foreach(var word in round.Words)
    {
     Assert.That(word.Count,Is.InRange(1,3),"A number COghe can show on its tendrils");
     if(word.Answer==COgheBonusAnswer.Item)Assert.AreEqual("BALL",word.Item,"Only the gift ball: no bonus depends on what was bought");
    }
   }
   Assert.IsNull(COgheBonus.Rounds(6));
  }

  [UnityTest] public IEnumerator BonusTwoThingsAreTakenInAnyOrder()
  {
   try
   {
    yield return StartBonusInHome(2);var p=game.Personality;
    // cuddle and food: food first is fine
    int eaten=p.BallsEaten;p.BonusFeed();
    yield return BonusUntil(()=>p.BallsEaten>eaten&&p.BonusAsking,10,"It eats and asks for the rest");
    Assert.AreEqual(0,p.BonusRound);Assert.AreEqual(1,p.BonusWordsDone);Assert.AreEqual(COgheBonusAnswer.Touch,p.BonusAsk.Value.Answer);
    p.TouchedInHome(p.SkinCentre);
    yield return BonusUntil(()=>p.BonusRound==1,10,"Both done: the next question");
    Assert.AreEqual(0,p.BonusMisses);
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;}
  }

  [UnityTest] public IEnumerator BonusOrderMatters()
  {
   try
   {
    yield return StartBonusInHome(3);var p=game.Personality;
    // food, then a cuddle: the cuddle first is the wrong order
    p.TouchedInHome(p.SkinCentre);
    yield return BonusUntil(()=>p.BonusMisses==1&&p.BonusAsking,5,"Wrong order: a gentle no, and it asks again");
    Assert.AreEqual(0,p.BonusWordsDone);
    int eaten=p.BallsEaten;p.BonusFeed();
    yield return BonusUntil(()=>p.BallsEaten>eaten&&p.BonusAsking&&p.BonusWordsDone==1,10,"Food first, then");
    p.TouchedInHome(p.SkinCentre);
    yield return BonusUntil(()=>p.BonusRound==1,10,"In order: the next question");
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;}
  }

  [UnityTest] public IEnumerator BonusCountsExactly()
  {
   try
   {
    yield return StartBonusInHome(4);var p=game.Personality;
    Assert.AreEqual(2,p.BonusAsk.Value.Count,"Two raised tendrils, then a mushroom");
    // three is one too many: a shake, and the count starts again
    int eaten=p.BallsEaten;p.BonusFeed();p.BonusFeed();p.BonusFeed();
    Assert.AreEqual(1,p.BonusMisses);Assert.AreEqual(0,p.BonusCounted);
    yield return BonusUntil(()=>p.BonusAsking,10,"It eats the two, shakes, asks again");
    Assert.AreEqual(eaten+2,p.BallsEaten);Assert.AreEqual(0,p.BonusRound);
    // exactly two, then waiting: it checks, and it is right
    p.BonusFeed();p.BonusFeed();
    yield return BonusUntil(()=>p.BonusRound==1,15,"Exactly two: the next question");
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;}
  }

  [UnityTest] public IEnumerator BonusSentenceOfANumberAndAThing()
  {
   try
   {
    yield return StartBonusInHome(5);var p=game.Personality;
    // two cuddles, then the ball
    Assert.AreEqual(COgheBonusAnswer.Touch,p.BonusAsk.Value.Answer);Assert.AreEqual(2,p.BonusAsk.Value.Count);
    p.TouchedInHome(p.SkinCentre);yield return Frames(5);p.TouchedInHome(p.SkinCentre);
    yield return BonusUntil(()=>p.BonusAsking&&p.BonusWordsDone==1,6,"Two cuddles, checked: on to the ball");
    Assert.AreEqual(COgheBonusAnswer.Item,p.BonusAsk.Value.Answer);
    p.PlayWith(game.HomeRoom.Find("BALL"));
    yield return BonusUntil(()=>p.Playing!=null,10,"It plays ball");
    yield return BonusUntil(()=>p.BonusRound==1,25,"The whole sentence: the next question");
    Assert.AreEqual(0,p.BonusMisses);
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;}
  }
 }
}
