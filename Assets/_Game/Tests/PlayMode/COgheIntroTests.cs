using System.Collections;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // The opening comic: plays over level 1 with the level paused, hands over to a running level, skips to the dissolve,
 // never starts by itself in tests, and has every layer it names.
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator StartIntro(float speed)
  {
   var intro=COgheIntro.Play(game);intro.Speed=speed;
   for(int i=0;i<30&&!intro.Started;i++)yield return null;
   Assert.IsTrue(intro.Started,"The intro starts once the level is up");
   yield return null;   // its first frame starts picture and score together
  }
  [UnityTest] public IEnumerator IntroNeverStartsByItselfInTests()
  {
   yield return Load(1);for(int i=0;i<10;i++)yield return null;
   Assert.IsFalse(COgheIntro.Playing,"No save, no intro: tests and the proof run start straight in the level");
   Assert.IsFalse(game.Owner.Paused);
  }
  [UnityTest] public IEnumerator IntroHasEveryLayer()
  {
   foreach(var name in COgheIntro.LayerNames)Assert.IsNotNull(Resources.Load<Texture2D>("COgheIntro/"+name),"Layer "+name);
   var layout=Resources.Load<TextAsset>("COgheIntro/layout");Assert.IsNotNull(layout);
   foreach(var name in COgheIntro.LayerNames)StringAssert.Contains(name+" ",layout.text,"Layout for "+name);
   Assert.IsNotNull(Resources.Load<Shader>("COgheIntro/IntroLayer"));Assert.IsNotNull(Resources.Load<AudioClip>("COgheAudio/intro_score"));
   yield return null;
  }
  [UnityTest] public IEnumerator IntroPlaysOverLevelOneThenHandsOver()
  {
   yield return Load(1);
   Assert.IsTrue(COgheIntro.Fits(game));bool seen=COgheIntro.Seen;
   yield return StartIntro(8);
   Assert.IsTrue(COgheIntro.Playing);Assert.IsTrue(game.Owner.Paused,"The level waits under the comic");Assert.AreEqual(0,Time.timeScale);
   yield return new WaitForSecondsRealtime(.5f);   // the music fades out in real time, whatever the frame rate
   if(COgheAudio.Instance!=null)Assert.Less(COgheAudio.Instance.Music.volume,.3f,"The game's music gives way to the intro score");
   float start=Time.realtimeSinceStartup;
   while(COgheIntro.Playing&&Time.realtimeSinceStartup-start<10)yield return null;
   Assert.IsFalse(COgheIntro.Playing,"It ends by itself");
   Assert.IsFalse(game.Owner.Paused,"The level runs after the dissolve");Assert.AreEqual(1,Time.timeScale);
   Assert.AreEqual(seen,COgheIntro.Seen,"Without a save the seen flag is untouched");
  }
  [UnityTest] public IEnumerator IntroSkipLandsOnTheDissolveAndReplaysFromPause()
  {
   yield return Load(1);
   game.Owner.TogglePause();   // replayed from the pause screen
   yield return StartIntro(1);
   var intro=Object.FindFirstObjectByType<COgheIntro>();
   intro.Skip();Assert.AreEqual(COgheIntro.SkipTo,intro.Clock,.01f);
   intro.Speed=4;
   float start=Time.realtimeSinceStartup;
   while(COgheIntro.Playing&&Time.realtimeSinceStartup-start<5)yield return null;
   Assert.IsFalse(COgheIntro.Playing);Assert.IsFalse(game.Owner.Paused,"A replay also hands over to a running level");
  }
  [UnityTest] public IEnumerator IntroOnlyFitsLevelOne()
  {
   yield return Load(2);
   Assert.IsFalse(COgheIntro.Fits(game),"The last panel is level 1's box");
  }

  // Preview frames for review: Artifacts/Intro/<size>/frame_<ms>.png, the live level rendered under the comic.
  [Explicit("Renders intro preview frames")]
  [UnityTest] public IEnumerator RenderIntroFrames()
  {
   yield return Load(1);
   yield return StartIntro(0);
   var intro=Object.FindFirstObjectByType<COgheIntro>();
   string steps=System.Environment.GetEnvironmentVariable("COGHE_INTRO_FPS");
   var times=new List<float>();
   if(!string.IsNullOrEmpty(steps)){float fps=float.Parse(steps);for(float t=0;t<COgheIntro.Length;t+=1f/fps)times.Add(t);}
   else times.AddRange(new[]{.9f,1.75f,1.9f,2.4f,3.8f,5.0f,6.3f,7.05f,7.9f,8.8f,10.0f,10.45f,11.0f,12.1f,14.0f,14.75f,15.5f,16.2f,16.5f,17.5f,18.4f,18.9f,19.3f,19.8f});
   var sizes=new List<Vector2Int>{new Vector2Int(1080,2340),new Vector2Int(1080,1920)};
   if(!string.IsNullOrEmpty(steps))sizes=new List<Vector2Int>{new Vector2Int(540,1170)};
   foreach(var size in sizes)
   {
    string dir=$"Artifacts/Intro/{size.x}x{size.y}";if(Directory.Exists(dir))Directory.Delete(dir,true);Directory.CreateDirectory(dir);
    var cam=game.Owner.View;var rt=RenderTexture.GetTemporary(size.x,size.y,24);var tex=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
    cam.targetTexture=rt;game.CameraRig.Frame(size.x,size.y,0,true);
    try
    {
     foreach(float t in times)
     {
      intro.Clock=t;cam.Render();
      RenderTexture.active=rt;GL.PushMatrix();GL.LoadPixelMatrix(0,size.x,size.y,0);intro.Draw(size.x,size.y);GL.PopMatrix();
      tex.ReadPixels(new Rect(0,0,size.x,size.y),0,0);tex.Apply();RenderTexture.active=null;
      File.WriteAllBytes($"{dir}/frame_{Mathf.RoundToInt(t*1000):00000}.png",tex.EncodeToPNG());
     }
     // the live level alone, for the match-cut check
     cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,size.x,size.y),0,0);tex.Apply();RenderTexture.active=null;
     File.WriteAllBytes($"{dir}/level.png",tex.EncodeToPNG());
    }
    finally{cam.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
    yield return null;
   }
   intro.Speed=100;
   while(COgheIntro.Playing)yield return null;
  }
 }
}
