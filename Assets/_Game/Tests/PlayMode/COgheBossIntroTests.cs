using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Boss levels: the opening tour holds the level paused, flies the camera and hands it back exactly to the game's view.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator BossIntroToursThenHandsTheViewBack()
  {
   yield return Load(10);
   Assert.IsTrue(game.Definition.Boss,"Position 10 is a Boss level");
   game.CameraRig.Frame(720,1280,0,true);var view=game.Owner.View;
   COgheBossIntro.PreviewSize=new Vector2Int(720,1280);
   try
   {
    bool done=false;var intro=COgheBossIntro.Play(game,()=>done=true);
    yield return null;yield return null;
    Assert.IsTrue(game.Owner.Paused,"The level waits under the tour");Assert.IsTrue(COgheBossIntro.OwnsCamera);
    Assert.IsFalse(view.orthographic,"The tour looks through a perspective lens");
    var before=game.Motion.Centre(0);
    intro.Speed=6;float start=Time.realtimeSinceStartup;
    while(!done&&Time.realtimeSinceStartup-start<10)yield return null;
    Assert.IsTrue(done,"It ends by itself");Assert.IsFalse(game.Owner.Paused,"Then the level starts");
    Assert.IsTrue(view.orthographic,"Back to the game's view");Assert.IsFalse(COgheBossIntro.OwnsCamera);
    Assert.AreEqual(before,game.Motion.Centre(0),"Nothing in the box moved");
   }
   finally{COgheBossIntro.PreviewSize=null;}
  }

  // Frames of the whole intro at 30 fps: Artifacts/BossIntro/frame_#####.png
  [Explicit("Renders the Boss intro")]
  [UnityTest] public IEnumerator RenderBossIntro()
  {
   yield return Load(10);
   COgheBossIntro.PreviewSize=new Vector2Int(540,1170);
   string root="Artifacts/BossIntro";if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
   var rt=RenderTexture.GetTemporary(540,1170,24);var tex=new Texture2D(540,1170,TextureFormat.RGB24,false);
   try
   {
    var view=game.Owner.View;view.aspect=540f/1170;
    var intro=COgheBossIntro.Play(game,null);intro.Speed=0;yield return null;yield return null;view.aspect=540f/1170;
    float end=intro.TourLength+COgheBossIntro.PullBack+COgheBossIntro.Warning;int frame=0;
    for(float t=0;t<end;t+=1f/30)
    {
     intro.Seek(t);view.aspect=540f/1170;Canvas.ForceUpdateCanvases();
     Grab(view,rt,tex,$"{root}/frame_{frame++:00000}.png");
     if(frame%30==0)yield return null;
    }
    File.WriteAllText($"{root}/timing.txt",$"tour {intro.TourLength:F2} pull {COgheBossIntro.PullBack} warning {COgheBossIntro.Warning}");
    intro.Seek(end+1);
   }
   finally{COgheBossIntro.PreviewSize=null;RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
  }
 }
}
