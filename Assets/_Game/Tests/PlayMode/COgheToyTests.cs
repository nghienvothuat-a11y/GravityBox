using System.Collections;
using System.Collections.Generic;
using System.IO;
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

  // The ball game: passed back and forth three times. Kept for later chapters; chapter 1 stays as it was (Mrk 17/10/2026).
  [UnityTest] public IEnumerator BallGameIsPassedBackThreeTimes()
  {
   try
   {
    yield return EnterFurnishedHome(65);yield return Frames(20);
    var p=game.Personality;var part=game.HomeRoom.Find("BALL").Part("Ball");Vector3 rest=part.localPosition,restWorld=part.position;
    p.BeginBonus(new[]{COgheBonusRound.Game("BALL")});
    yield return BonusUntil(()=>p.ToyListening,15,"The ball word, the ball rolls over, and COghe passes it to the player");
    Assert.IsTrue(p.ToyIsBall);Assert.Greater(Vector3.Distance(part.position,restWorld),.15f,"The ball waits at the player's spot");
    // a touch on COghe instead: it giggles and points at the ball, which still waits for the player
    p.TouchedInHome(p.SkinCentre);Assert.AreEqual(COgheGag.GigglePoint,p.Gag);
    yield return BonusUntil(()=>p.ToyListening,8,"Still the player's turn");
    for(int pass=0;pass<COghePersonality.BallPasses;pass++)
    {
     yield return BonusUntil(()=>p.ToyListening,12,"Pass "+(pass+1));
     game.TouchPoint(game.Owner.View.WorldToScreenPoint(part.position));yield return Frames(1);
     yield return BonusUntil(()=>!p.ToyListening,4,"A tap on the ball sends it back");
    }
    yield return BonusUntil(()=>p.BonusThanking||p.BonusFinished,20,"The header show, joy, then the thank-you");
    Assert.Less(Vector3.Distance(rest,part.localPosition),1e-3f,"The ball is back home");Assert.AreEqual(1,p.BonusMisses);
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;}
  }

  // From chapter 3 the drum is a word: in order, counted, and a gag when it is tapped out of turn.
  [UnityTest] public IEnumerator TheDrumIsAWordFromChapterThree()
  {
   try
   {
    yield return HomeWithoutTheDrum();COgheToys.Give(COgheToys.Drum);game.HomeRoom.Refresh();   // chapter 2's game won
    var p=game.Personality;var drum=game.HomeRoom.Find(COgheToys.Drum);Assert.IsTrue(drum.Root.activeSelf);
    // chapter 3, question 2: a cuddle, the drum, food
    p.BeginBonus(COgheBonus.Rounds(3),1);
    yield return BonusUntil(()=>p.BonusAsking,10,"It asks");
    Assert.AreEqual(COgheBonusAnswer.Touch,p.BonusAsk.Value.Answer);
    p.PlayWith(drum);Assert.AreEqual(COgheGag.Startle,p.Gag,"The drum out of turn: COghe jumps at the noise");
    yield return BonusUntil(()=>p.BonusAsking,6,"It asks again");
    p.TouchedInHome(p.SkinCentre);
    yield return BonusUntil(()=>p.BonusAsking&&p.BonusWordsDone==1,6,"A cuddle: on to the drum");
    Assert.AreEqual(COgheShape.Drum,p.BonusAsk.Value.Shape);Assert.AreEqual(COgheToys.Drum,p.BonusAsk.Value.Item);
    p.PlayWith(drum);
    yield return BonusUntil(()=>p.Playing==drum,10,"It goes and plays the drum");
    yield return BonusUntil(()=>p.BonusAsking&&p.BonusWordsDone==2,15,"then food");
    p.BonusFeed();
    yield return BonusUntil(()=>p.BonusRound==2,10,"The whole sentence: the next question");
    p.EndBonus();
    // chapter 4, question 2: exactly three strikes
    p.BeginBonus(COgheBonus.Rounds(4),1);
    yield return BonusUntil(()=>p.BonusAsking,10,"It asks");
    Assert.AreEqual(3,p.BonusAsk.Value.Count);Assert.AreEqual(COgheToys.Drum,p.BonusAsk.Value.Item);
    for(int i=0;i<3;i++){p.PlayWith(drum);yield return Frames(6);}
    yield return BonusUntil(()=>p.BonusRound==2,12,"Three strikes, checked: the next question");
    Assert.AreEqual(0,p.BonusMisses);
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  private IEnumerator TapToy(Vector3 world){game.TouchPoint(game.Owner.View.WorldToScreenPoint(world));yield return Frames(1);}

  // Chapter 3's last question: the horn, COghe's tune played back in order (Mrk 17/10/2026).
  [UnityTest] public IEnumerator HornGameIsPlayedBackInOrder()
  {
   try
   {
    yield return HomeWithoutTheDrum();
    var p=game.Personality;var horn=game.HomeRoom.Find(COgheToys.Horn);Assert.IsFalse(horn.Root.activeSelf,"Not the player's yet");
    p.BeginBonus(COgheBonus.Rounds(3),2);
    yield return BonusUntil(()=>p.ToyWaitingForPlayer,15,"The horn pops out and COghe plays its first tune");
    Assert.IsTrue(horn.Root.activeSelf);Assert.AreEqual(2,p.ToyBeats);
    // the wrong button first: a honk, and the tune again
    int wrong=(p.ToyTune[0]+1)%3;yield return TapToy(horn.Part("Key"+wrong).position);
    Assert.AreEqual(COgheGag.Blooper,p.Gag);
    for(int tune=0;tune<3;tune++)
    {
     yield return BonusUntil(()=>p.ToyWaitingForPlayer&&p.Gag==COgheGag.None,15,"Tune "+(tune+1));
     foreach(int key in p.ToyTune){yield return TapToy(horn.Part("Key"+key).position);yield return Frames(8);}
     yield return BonusUntil(()=>!p.ToyListening,4,"Played back in order");
    }
    yield return BonusUntil(()=>p.ToyJamming,8,"The fanfare together");
    yield return BonusUntil(()=>p.BonusThanking||p.BonusFinished,15,"Won");
    Assert.IsTrue(COgheToys.Owns(COgheToys.Horn),"The horn is the player's");Assert.AreEqual(1,p.BonusMisses);
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  // Chapter 4's last question: the balloons, just as many puffs as COghe shows (Mrk 17/10/2026).
  [UnityTest] public IEnumerator BalloonGameCountsThePuffs()
  {
   try
   {
    yield return HomeWithoutTheDrum();
    var p=game.Personality;var bunch=game.HomeRoom.Find(COgheToys.Balloon);
    p.BeginBonus(COgheBonus.Rounds(4),2);
    yield return BonusUntil(()=>p.ToyWaitingForPlayer,15,"A limp balloon, and the count on COghe's tendrils");
    Assert.AreEqual(2,p.ToyBeats);
    // one puff too many: it pops in COghe's face, and a new balloon comes
    for(int i=0;i<3;i++){yield return TapToy(p.ToyTapPoint);yield return Frames(6);}
    Assert.AreEqual(COgheGag.Pop,p.Gag);
    for(int b=0;b<3;b++)
    {
     yield return BonusUntil(()=>p.ToyWaitingForPlayer&&p.Gag==COgheGag.None,15,"Balloon "+(b+1));
     int n=p.ToyBeats;for(int i=0;i<n;i++){yield return TapToy(p.ToyTapPoint);yield return Frames(8);}
     yield return BonusUntil(()=>!p.ToyListening,4,"Just enough: tied, it floats to the bunch");
    }
    yield return BonusUntil(()=>p.ToyJamming,8,"Up with the balloons");
    yield return TapToy(p.ToyTapPoint);
    yield return BonusUntil(()=>p.BonusThanking||p.BonusFinished,15,"Won");
    Assert.IsTrue(COgheToys.Owns(COgheToys.Balloon),"The bunch is the player's");Assert.AreEqual(1,p.BonusMisses);
    for(int i=0;i<3;i++)Assert.Less(Vector3.Distance(bunch.Part("Balloon"+i).localPosition,COgheHomeItems.BalloonSpots[i]),1e-3f,"Back in the bunch");
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  // Chapter 5's last question: the spinning top, kept spinning with a tap whenever it wobbles (Mrk 17/10/2026).
  [UnityTest] public IEnumerator TopGameKeepsTheTopSpinning()
  {
   try
   {
    yield return HomeWithoutTheDrum();
    var p=game.Personality;var top=game.HomeRoom.Find(COgheToys.Top);
    p.BeginBonus(COgheBonus.Rounds(5),2);
    yield return BonusUntil(()=>p.ToyWaitingForPlayer,15,"COghe pulls the top into a spin");
    Assert.IsTrue(top.Root.activeSelf);
    // left alone it wobbles and falls: a gag, then COghe stands it up and spins it again
    yield return BonusUntil(()=>p.Gag==COgheGag.TopFall,8,"Left alone, it falls");
    for(int r=0;r<COghePersonality.TopRescues;r++)
    {
     yield return BonusUntil(()=>p.ToyWaitingForPlayer&&p.ToyWobbling&&p.Gag==COgheGag.None,12,"It wobbles ("+(r+1)+")");
     yield return TapToy(p.ToyTapPoint);
     yield return BonusUntil(()=>!p.ToyListening,4,"A tap: it spins up straight again");
    }
    yield return BonusUntil(()=>p.ToyJamming,8,"COghe twirls beside it");
    yield return BonusUntil(()=>p.BonusThanking||p.BonusFinished,15,"Won");
    Assert.IsTrue(COgheToys.Owns(COgheToys.Top),"The top is the player's");Assert.AreEqual(1,p.BonusMisses);
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  // Chapter 6's last question: the music box, three tunes wound by the player (Mrk 17/10/2026).
  [UnityTest] public IEnumerator MusicBoxGamePlaysThreeTunes()
  {
   try
   {
    yield return HomeWithoutTheDrum();
    var p=game.Personality;
    p.BeginBonus(COgheBonus.Rounds(6),2);
    yield return BonusUntil(()=>p.ToyWaitingForPlayer,15,"COghe opens the box and turns the crank once");
    Assert.AreEqual(3,p.ToyBeats);
    // a touch on COghe instead: it giggles and points at the box, then the tune starts again
    p.TouchedInHome(p.SkinCentre);Assert.AreEqual(COgheGag.GigglePoint,p.Gag);
    for(int tune=0;tune<3;tune++)
    {
     yield return BonusUntil(()=>p.ToyWaitingForPlayer&&p.Gag==COgheGag.None,12,"Tune "+(tune+1));
     for(int i=0;i<3;i++){yield return TapToy(p.ToyTapPoint);yield return Frames(10);}
     yield return BonusUntil(()=>!p.ToyListening,4,"The tune complete: a flourish");
    }
    yield return BonusUntil(()=>p.ToyJamming,8,"The box plays on; they dance");
    yield return BonusUntil(()=>p.BonusThanking||p.BonusFinished,15,"Won");
    Assert.IsTrue(COgheToys.Owns(COgheToys.MusicBox),"The box is the player's");Assert.AreEqual(1,p.BonusMisses);
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  // From chapter 7 (Mrk 18/10/2026, up to 300 levels): made from the chapter number, from what every player has; the toy
  // games in turn, harder every five chapters.
  [Test] public void ChaptersFromSevenAreMadeFromWhatEveryPlayerHas()
  {
   for(int c=7;c<=COgheBonus.MaxChapters;c++)
   {
    var r=COgheBonus.Rounds(c);Assert.IsNotNull(r,"Chapter "+c);Assert.AreEqual(3,r.Length);
    Assert.AreEqual(3,r[0].Words.Length);Assert.AreEqual(4,r[1].Words.Length);
    for(int q=0;q<2;q++)
    {
     Assert.IsTrue(r[q].Ordered);var seen=new HashSet<string>();int numbers=0;
     foreach(var w in r[q].Words)
     {
      Assert.That(w.Count,Is.InRange(1,3),"A number COghe can show");if(w.Count>1)numbers++;
      string id=w.Answer==COgheBonusAnswer.Item?w.Item:w.Answer.ToString();Assert.IsTrue(seen.Add(id),"Each word once in a sentence");
      if(w.Answer==COgheBonusAnswer.Item)Assert.IsTrue(w.Item=="BALL"||COgheToys.Is(w.Item),"Only the gift ball and the toys");
     }
     Assert.AreEqual(q==1&&c>=10?2:1,numbers);
    }
    Assert.IsTrue(r[2].IsGame);Assert.AreEqual(COgheBonus.GameCycle[(c-7)%5],r[2].Toy);Assert.AreEqual(1+(c-7)/5,r[2].Level);
    Assert.AreSame(r,COgheBonus.Rounds(c),"The same questions every time");
   }
   Assert.IsNull(COgheBonus.Rounds(COgheBonus.MaxChapters+1));
  }

  // A toy is a word too (from chapter 7, the drum from 3): in its turn COghe goes and plays it; out of turn, a fright.
  [UnityTest] public IEnumerator ANewToyWordSendsCOgheToPlayIt()
  {
   try
   {
    yield return HomeWithoutTheDrum();foreach(var t in COgheToys.Catalog)COgheToys.Give(t.id);game.HomeRoom.Refresh();
    var p=game.Personality;var room=game.HomeRoom;var horn=room.Find(COgheToys.Horn);
    p.BeginBonus(new[]{new COgheBonusRound(true,COgheBonus.ToyWord(COgheToys.Horn))});
    yield return BonusUntil(()=>p.BonusAsking,10,"It asks");
    Assert.AreEqual(COgheShape.Horn,p.BonusAsk.Value.Shape);
    p.PlayWith(room.Find(COgheToys.Top));Assert.AreEqual(COgheGag.Startle,p.Gag,"Another toy: a fright");
    yield return BonusUntil(()=>p.BonusAsking,6,"It asks again");
    p.PlayWith(horn);
    yield return BonusUntil(()=>p.Playing==horn,10,"The horn: COghe goes and plays it");
    yield return BonusUntil(()=>p.BonusThanking||p.BonusFinished,20,"Understood");
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  // Later chapters play the toy games harder: a longer beat, a longer tune, more rescues of the top.
  [UnityTest] public IEnumerator LaterToyGamesAreHarder()
  {
   try
   {
    yield return HomeWithoutTheDrum();var p=game.Personality;
    p.BeginBonus(new[]{COgheBonusRound.Game(COgheToys.Drum,1)});
    yield return BonusUntil(()=>p.ToyWaitingForPlayer,15,"The drum, level 1");Assert.AreEqual(3,p.ToyBeats,"One strike more");
    p.EndBonus();
    p.BeginBonus(new[]{COgheBonusRound.Game(COgheToys.Horn,2)});
    yield return BonusUntil(()=>p.ToyWaitingForPlayer,15,"The horn, level 2");Assert.AreEqual(4,p.ToyBeats,"Two notes more");
    p.EndBonus();
    p.BeginBonus(new[]{COgheBonusRound.Game(COgheToys.Top,1)});
    yield return BonusUntil(()=>p.ToyWaitingForPlayer,15,"The top, level 1");Assert.AreEqual(COghePersonality.TopRescues+1,p.ToyBeats,"One rescue more");
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  // The toys' icons for the Items menu (Explicit): each toy rendered from its own model, in the furniture icons' three-quarter
  // view, 256 px on a clear background (rendered on black and on white: the difference is the alpha). Written to
  // Resources/COgheHome/Icons/<id>.png.
  [UnityTest,Explicit] public IEnumerator RenderToyIcons()
  {
   try
   {
    yield return HomeWithoutTheDrum();foreach(var t in COgheToys.Catalog)COgheToys.Give(t.id);game.HomeRoom.Refresh();yield return Frames(40);
    var room=game.HomeRoom;const int layer=30,size=512,half=256;
    var cam=new GameObject("Icon camera").AddComponent<Camera>();cam.orthographic=true;cam.clearFlags=CameraClearFlags.SolidColor;cam.cullingMask=1<<layer;cam.enabled=false;
    var rt=new RenderTexture(size,size,24,RenderTextureFormat.ARGB32){antiAliasing=4};cam.targetTexture=rt;
    Texture2D Shot(Color background){cam.backgroundColor=background;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,size,size),0,0);tex.Apply();RenderTexture.active=null;return tex;}
    foreach(var t in COgheToys.Catalog)
    {
     var item=room.Find(t.id);
     var box=item.Part("Lid");if(box!=null){box.localRotation=Quaternion.Euler(55,0,0);var dancer=item.Part("Dancer");dancer.localScale=Vector3.one;}   // the music box open, its dancer up
     var renderers=item.Root.GetComponentsInChildren<Renderer>();var old=new int[renderers.Length];
     for(int i=0;i<renderers.Length;i++){old[i]=renderers[i].gameObject.layer;renderers[i].gameObject.layer=layer;}
     var b=new Bounds(renderers[0].bounds.center,Vector3.zero);foreach(var r in renderers)b.Encapsulate(r.bounds);
     cam.transform.rotation=item.Root.transform.rotation*Quaternion.Euler(28,-24,0);
     cam.transform.position=b.center-cam.transform.forward*1f;cam.nearClipPlane=.01f;cam.farClipPlane=3;
     cam.orthographicSize=Mathf.Max(b.extents.x,b.extents.y,b.extents.z)*1.12f;
     var black=Shot(Color.black);var white=Shot(Color.white);
     var pb=black.GetPixels();var pw=white.GetPixels();var full=new Color[pb.Length];
     for(int i=0;i<pb.Length;i++)
     {   // premultiplied on black, alpha from the black/white difference
      float a=Mathf.Clamp01(1-((pw[i].r-pb[i].r)+(pw[i].g-pb[i].g)+(pw[i].b-pb[i].b))/3f);
      full[i]=new Color(pb[i].r,pb[i].g,pb[i].b,a);
     }
     var po=new Color[half*half];
     for(int y=0;y<half;y++)for(int x=0;x<half;x++)
     {   // 2×2 down to 256 px, then un-premultiply
      Color c=(full[2*y*size+2*x]+full[2*y*size+2*x+1]+full[(2*y+1)*size+2*x]+full[(2*y+1)*size+2*x+1])/4f;
      po[y*half+x]=c.a>.003f?new Color(c.r/c.a,c.g/c.a,c.b/c.a,c.a):new Color(0,0,0,0);
     }
     var icon=new Texture2D(half,half,TextureFormat.RGBA32,false);icon.SetPixels(po);icon.Apply();
     File.WriteAllBytes("Assets/_Game/Venom/Resources/COgheHome/Icons/"+t.id+".png",icon.EncodeToPNG());
     for(int i=0;i<renderers.Length;i++)renderers[i].gameObject.layer=old[i];
    }
    Object.Destroy(cam.gameObject);rt.Release();
   }
   finally{game.Personality?.EndBonus();COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.ResetForTests();}
  }

  [Test] public void OldSavesThatFinishedChapterTwoGetTheDrum()
  {
   try
   {
    COgheShop.ResetForTests(new COgheShop.State{Migrated=true});COgheShop.TestsOwnUnlocked=false;
    COgheToys.GrantEarned();Assert.IsFalse(COgheToys.Owns(COgheToys.Drum),"Not before chapter 2 is understood");
    COgheShop.Understand(COgheBonus.Key(2));COgheToys.GrantEarned();
    Assert.IsTrue(COgheToys.Owns(COgheToys.Drum),"Chapter 2 understood before the drum existed: it is theirs (later questions use it)");
   }
   finally{COgheShop.ResetForTests();}
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
