using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
 // COghe's eyes (Mrk 07/10/2026), for reviews: close stills of each mood in Home and of a level.
 // Artifacts/Eyes/<name>.png.
 public sealed partial class COgheSpatialCampaignTests
 {
  [Explicit("Renders the eye prototype")]
  [UnityTest] public IEnumerator RenderEyesStills()
  {
   bool was=COgheEyes.Enabled;COgheEyes.Enabled=true;
   string root="Artifacts/Eyes";if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
   var rt=RenderTexture.GetTemporary(600,800,24);var tex=new Texture2D(600,800,TextureFormat.RGB24,false);Camera close=null;
   void Shot(string name,float size=.075f)
   {
    var view=game.Owner.View;if(close==null){close=new GameObject("Eyes camera").AddComponent<Camera>();close.enabled=false;}
    close.CopyFrom(view);close.aspect=600f/800;close.orthographic=true;close.orthographicSize=size;
    Vector3 c=game.Personality!=null&&game.Home?game.Personality.SkinCentre:game.Motion.Centre(0),fwd=view.transform.forward;
    close.transform.rotation=view.transform.rotation;close.transform.position=c-fwd*.6f+view.transform.up*.012f;
    Grab(close,rt,tex,$"{root}/{name}.png");
   }
   try
   {
    // a level: at rest, then split in two by Q (level 13: "16" content)
    yield return Load(1);yield return Frames(90);Shot("level-rest");Shot("level-wide",.25f);
    // Home: moods
    yield return EnterFurnishedHome(60);yield return Frames(200);var p=game.Personality;
    Shot("home-rest");
    p.TouchedInHome(game.Motion.Centre(0));yield return Frames(15);Shot("home-touch");
    yield return Frames(60);p.TouchedInHome(game.Motion.Centre(0));yield return Frames(15);Shot("home-touch2");
    yield return Frames(40);p.TouchedInHome(game.Motion.Centre(0));yield return Frames(22);Shot("home-hug",.09f);
    yield return Frames(90);
    for(int i=0;i<5;i++){p.TouchedInHome(game.Motion.Centre(0));yield return Frames(2);}
    for(int f=0;f<30*8&&p.Act!=COgheAct.Home;f++)yield return Frames(1);yield return Frames(20);Shot("home-sulk");
    for(int f=0;f<30*10&&p.Sulking;f++)yield return Frames(1);
    p.PlayNow(game.HomeRoom.Find("BED"));yield return Frames(90);Shot("home-sleep",.1f);
    yield return Frames(30*7);
    game.FeedHome();for(int f=0;f<30*12&&!p.Eating;f++)yield return Frames(1);yield return Frames(20);Shot("home-eat");
    // the bonus: a wrong answer (sad), a right one (happy)
    p.BeginBonus(COgheBonus.Rounds(1));for(int f=0;f<30*10&&!p.BonusAsking;f++)yield return Frames(1);yield return Frames(80);
    p.BonusFeed();yield return Frames(10);Shot("bonus-sad");
    for(int f=0;f<30*3&&!p.BonusAsking;f++)yield return Frames(1);p.TouchedInHome(p.SkinCentre);yield return Frames(4);Shot("bonus-happy");
    p.EndBonus();
   }
   finally{COgheEyes.Enabled=was;COgheHomeRoom.UnlockedLevelOverride=null;if(close!=null)Object.Destroy(close.gameObject);RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
  }

  // The eyes in Home, for a review: a close camera following COghe through each mood (rest and blinks, touches, a hug,
  // a sulk, the ball, the bed, a meal, the chapter 1 bonus, a light ink), each frame saved with the eyes (eyes/) and the
  // same frame without them (plain/). marks.txt: frame, case; taps.txt: frame, x, y (pixels from the bottom-left).
  [UnityTest,Explicit,Timeout(3600000)] public IEnumerator RecordEyesHome()
  {
   bool was=COgheEyes.Enabled;COgheEyes.Enabled=COgheEyes.Trace=true;
   const int W=600,H=800;string root="Artifacts/Clips/eyes-home";
   if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root+"/eyes");Directory.CreateDirectory(root+"/plain");
   var rt=RenderTexture.GetTemporary(W,H,24);rt.antiAliasing=4;var tex=new Texture2D(W,H,TextureFormat.RGB24,false);Camera close=null;
   var marks=new System.Text.StringBuilder();var taps=new System.Text.StringBuilder();var diag=new System.Text.StringBuilder();int frame=0;Vector3 focus=default;bool framed=false;
   void Twin()
   {
    var view=game.Owner.View;if(close==null){close=new GameObject("Eyes clip camera").AddComponent<Camera>();close.enabled=false;}
    close.CopyFrom(view);close.aspect=(float)W/H;close.orthographic=true;close.orthographicSize=.14f;
    var c=game.Personality.SkinCentre;focus=framed?Vector3.Lerp(focus,c,.12f):c;framed=true;
    close.transform.rotation=view.transform.rotation;close.transform.position=focus-view.transform.forward*.8f+view.transform.up*.012f;
    Grab(close,rt,tex,$"{root}/eyes/{frame:00000}.png");
    var eyesView=game.Matter.transform.Find("COghe eyes");bool shown=eyesView!=null&&eyesView.gameObject.activeSelf;
    if(shown)eyesView.gameObject.SetActive(false);
    Grab(close,rt,tex,$"{root}/plain/{frame:00000}.png");
    if(shown)eyesView.gameObject.SetActive(true);
    var eyes=game.Matter.GetComponent<COgheEyes>();diag.AppendLine($"{frame} {game.Personality.Act} {(eyes!=null?eyes.LastState:"-")}");
    frame++;
   }
   IEnumerator Rec(float seconds,System.Func<bool> until=null)
   {
    for(int i=0;i<seconds*30&&(until==null||!until());i++){for(int k=0;k<4;k++)Tick();yield return null;Twin();}
   }
   void Mark(string what)=>marks.AppendLine($"{frame} {what}");
   void Touch(Vector3 at){if(close!=null){var v=close.WorldToViewportPoint(at);taps.AppendLine($"{frame} {v.x*W:F0} {v.y*H:F0}");}game.Personality.TouchedInHome(at);}
   try
   {
    yield return EnterFurnishedHome(60);yield return Frames(150);var p=game.Personality;var room=game.HomeRoom;
    Mark("rest");yield return Rec(5);
    Mark("touch");Touch(p.SkinCentre);yield return Rec(2.4f);
    Touch(p.SkinCentre);yield return Rec(2f);
    Mark("hug");Touch(p.SkinCentre);yield return Rec(3f);
    yield return Rec(1.5f);
    Mark("sulk");for(int i=0;i<5;i++){Touch(p.SkinCentre);yield return Rec(.07f);}
    yield return Rec(8,()=>p.Sulking);yield return Rec(4.5f);
    yield return Frames(30*10);for(int f=0;f<30*10&&p.Sulking;f++)yield return Frames(1);
    Mark("ball");p.PlayNow(room.Find("BALL"));yield return Rec(6);
    Mark("bed");p.PlayNow(room.Find("BED"));yield return Rec(5);
    yield return Frames(30*7);
    Mark("eat");game.FeedHome();yield return Rec(14,()=>p.Eating);yield return Rec(4.5f);
    yield return Frames(30*4);
    // chapter 1's bonus: a wrong answer, then each right one, then the finale
    p.BeginBonus(COgheBonus.Rounds(1));for(int f=0;f<30*10&&!p.BonusAsking;f++)yield return Frames(1);
    Mark("bonus-ask");yield return Rec(2.2f);
    Mark("bonus-wrong");p.BonusFeed();yield return Rec(3,()=>p.BonusAsking);yield return Rec(.6f);
    Mark("bonus-right");Touch(p.SkinCentre+game.Root.up*.04f);yield return Rec(10,()=>p.BonusRound==1&&p.BonusAsking);yield return Rec(.8f);
    // the ball: the start of the game, then (unrecorded) until it has played and asks the last question
    Mark("bonus-right");p.PlayWith(room.Find("BALL"));yield return Rec(6,()=>p.BonusRound==2&&p.BonusAsking);
    for(int f=0;f<30*40&&!(p.BonusRound==2&&p.BonusAsking);f++)yield return Frames(1);
    Assert.IsTrue(p.BonusRound==2&&p.BonusAsking,"The last question");
    Mark("bonus-ask");yield return Rec(1.5f);
    Mark("bonus-right");p.BonusFeed();yield return Rec(20,()=>p.BonusThanking);
    Mark("bonus-finale");yield return Rec(14,()=>p.BonusFinished);yield return Rec(1);
    p.EndBonus();yield return Frames(60);
    // a light ink on the whole body: the eyes still read
    foreach(var id in new[]{"INK_PEARL","INK_PRISM"})
    {
     var style=new COgheStyle{Seed=3};style.Inks[0]=id;for(int i=0;i<style.Amount.Length;i++)style.Amount[i]=new Vector4(1,0,0,0);
     COgheStyle.ResetForTests(style);style.ApplyTo(game);for(int f=0;f<30*6&&p.Act!=COgheAct.Home&&p.Act!=COgheAct.None;f++)yield return Frames(1);yield return Frames(30);
     Mark("ink-"+id);yield return Rec(3f);Touch(p.SkinCentre);yield return Rec(3f);
    }
    Mark("end");
    File.WriteAllText($"{root}/marks.txt",marks.ToString());File.WriteAllText($"{root}/taps.txt",taps.ToString());File.WriteAllText($"{root}/diag.txt",diag.ToString());
   }
   finally{COgheEyes.Enabled=was;COgheEyes.Trace=false;COgheStyle.ResetForTests();COgheHomeRoom.UnlockedLevelOverride=null;if(close!=null)Object.Destroy(close.gameObject);RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
  }
 }
}
