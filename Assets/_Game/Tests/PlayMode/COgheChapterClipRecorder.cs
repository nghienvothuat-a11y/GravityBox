using System;
using System.Collections;
using System.IO;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace GravityBox.Tests
{
 // Review clips (Explicit): plays the reference solution of each chosen level and saves a 30 fps frame sequence
 // (720×1280) to Artifacts/Clips/NN, presentation included (skin, tendrils, button rings, refusal marks).
 // COGHE_CLIP_LEVELS picks play positions 1–60, e.g. "5,8,10"; default 1–10. COGHE_CLIP_SIZE sets the frame size
 // (e.g. "1080x1920"; default 720x1280). Each folder also gets taps.txt (frame, x, y in pixels from the bottom-left: where
 // each tap landed, for a finger in an edit) and sounds.txt (seconds, clip, volume). Encode with ffmpeg afterwards.
 public sealed partial class COgheSpatialCampaignTests
 {
  private int clipFrame;private string clipDirectory;private int clipWidth=720,clipHeight=1280;
  private System.Text.StringBuilder clipTaps,clipSounds;
  private IEnumerator Shot()
  {
   yield return null;game.CameraRig.Frame(720,1280,0,true);   // after the frame's own camera fit, so the shot keeps 9:16
   var camera=game.Owner.View;var target=RenderTexture.GetTemporary(clipWidth,clipHeight,24);target.antiAliasing=4;var previous=RenderTexture.active;var texture=new Texture2D(clipWidth,clipHeight,TextureFormat.RGB24,false);
   try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,clipWidth,clipHeight),0,0);texture.Apply();File.WriteAllBytes($"{clipDirectory}/{clipFrame++:00000}.png",texture.EncodeToPNG());}
   finally{camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.DestroyImmediate(texture);}
  }
  private IEnumerator Hold(float seconds){for(int i=0;i<seconds*30;i++){for(int k=0;k<4;k++)Tick();yield return Shot();}}
  private IEnumerator RecordUntil(float seconds,Func<bool> done,string reason)
  {
   for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++){Tick();if(i%4==3)yield return Shot();}
   Assert.IsTrue(done(),reason+$"; level={game.Definition.Order}");
  }
  private IEnumerator RecordTap(Vector3 point)
  {
   game.CameraRig.Frame(720,1280,0,true);var v=game.Owner.View.WorldToViewportPoint(point);
   clipTaps?.AppendLine($"{clipFrame} {v.x*clipWidth:F0} {v.y*clipHeight:F0}");
   yield return Tap(point);yield return Shot();
  }
  private IEnumerator RecordOrbit(float degrees)
  {
   // A visible turn of the view: 1 s, the same total as a drag of that many degrees.
   for(int i=0;i<30;i++){game.CameraRig.Orbit(-degrees/30f/240f*720f,720);for(int k=0;k<4;k++)Tick();yield return Shot();}
  }

  [UnityTest,Explicit,Timeout(3600000)] public IEnumerator RecordReviewClips()
  {
   string only=Environment.GetEnvironmentVariable("COGHE_CLIP_LEVELS"),size=Environment.GetEnvironmentVariable("COGHE_CLIP_SIZE");
   if(!string.IsNullOrEmpty(size)){var wh=size.Split('x');clipWidth=int.Parse(wh[0]);clipHeight=int.Parse(wh[1]);}
   System.Action<string,float> heard=(clip,volume)=>clipSounds?.AppendLine($"{clipFrame/30f:F3} {clip} {volume:F2}");COgheAudio.Heard+=heard;
   try{
   var levels=string.IsNullOrEmpty(only)?new[]{1,2,3,4,5,6,7,8,9,10}:Array.ConvertAll(only.Split(','),int.Parse);
   // Levels are play positions 1–50: the catalog's scene sequence names each one's scene (pilot, 11–30 or Plus).
   yield return Load(1);var order=game.Definition.SceneSequence;
   foreach(int n in levels)
   {
    clipDirectory=$"Artifacts/Clips/{n:00}";if(Directory.Exists(clipDirectory))Directory.Delete(clipDirectory,true);Directory.CreateDirectory(clipDirectory);clipFrame=0;
    clipTaps=new System.Text.StringBuilder();clipSounds=new System.Text.StringBuilder();
    yield return LoadScene(order[n-1]);yield return Hold(1);
    yield return new COgheSpatialScenario(game,RecordTap,RecordUntil,RecordOrbit).Solve();
    yield return Hold(1.2f);
    File.WriteAllText($"{clipDirectory}/taps.txt",clipTaps.ToString());File.WriteAllText($"{clipDirectory}/sounds.txt",clipSounds.ToString());
   }
   }finally{COgheAudio.Heard-=heard;}
  }
 }
}
