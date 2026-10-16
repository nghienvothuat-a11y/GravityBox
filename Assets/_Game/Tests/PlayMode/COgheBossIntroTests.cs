using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // The warning must not take over the camera, move tissue or leave the level paused.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator BossWarningsKeepTheGameplayCameraStill()
  {
   string[] scenes={"COgheSpatial10","COgheSpatialPlusB1","COgheSpatialPlusG5","COgheSpatial20","COgheSpatialPlusN40","COgheSpatialPlusN50"};
   foreach(string scene in scenes)
   {
    yield return LoadScene(scene);
    Assert.IsTrue(game.Definition.Boss,scene);
    game.enabled=false; // isolate the warning from normal viewport/aspect updates
    COgheBossIntro.PreviewSize=new Vector2Int(720,1280);
    COgheBossIntro intro=null;
    try
    {
     game.CameraRig.Reset();game.CameraRig.Frame(720,1280,0,true,new Rect(0,0,720,1280));
     var view=game.Owner.View;var position=view.transform.position;var rotation=view.transform.rotation;
     float size=view.orthographicSize,near=view.nearClipPlane,far=view.farClipPlane,fov=view.fieldOfView;
     var centre=game.Motion.Centre(0);int callbacks=0;
     intro=COgheBossIntro.Play(game,()=>callbacks++);intro.Speed=6;
     yield return null;yield return null;
     Assert.IsTrue(game.Owner.Paused,"The level waits under the warning: "+scene);
     float deadline=Time.realtimeSinceStartup+5;
     while(!intro.Finished&&Time.realtimeSinceStartup<deadline)
     {
      Assert.IsTrue(view.orthographic,scene+" never switches to perspective");
      Assert.Less(Vector3.Distance(position,view.transform.position),.00001f,scene+" position");
      Assert.Less(Quaternion.Angle(rotation,view.transform.rotation),.001f,scene+" rotation");
      Assert.AreEqual(size,view.orthographicSize,.00001f,scene+" zoom");
      Assert.AreEqual(near,view.nearClipPlane);Assert.AreEqual(far,view.farClipPlane);Assert.AreEqual(fov,view.fieldOfView);
      Assert.AreEqual(centre,game.Motion.Centre(0),scene+" tissue stays put");
      yield return null;
     }
     Assert.AreEqual(1,callbacks,scene+" completes exactly once");
     Assert.IsNull(COgheBossIntro.Current);Assert.IsFalse(game.Owner.Paused,scene+" hands over to play");
     Assert.IsTrue(view.orthographic);Assert.AreEqual(size,view.orthographicSize,.00001f);
     Assert.Less(Vector3.Distance(position,view.transform.position),.00001f);
     // Player-controlled orbit/zoom remain owned by the ordinary rig after the warning.
     game.enabled=true;
    }
    finally{if(intro!=null)Object.Destroy(intro.gameObject);COgheBossIntro.PreviewSize=null;}
   }
  }

  [Explicit("Renders the static Boss warning at two portrait sizes")]
  [UnityTest] public IEnumerator RenderBossIntro()
  {
   string root="Artifacts/BossStaticCamera/Frames";Directory.CreateDirectory(root);
   foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(720,1612)})
   {
    yield return Load(10);game.enabled=false;
    COgheBossIntro.PreviewSize=size;
    var rt=RenderTexture.GetTemporary(size.x,size.y,24);var tex=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
    COgheBossIntro intro=null;
    try
    {
     var view=game.Owner.View;view.aspect=(float)size.x/size.y;
     intro=COgheBossIntro.Play(game,null);intro.Speed=0;yield return null;yield return null;
     foreach(float t in new[]{0f,1.1f,2.4f})
     {
      intro.Seek(t);Canvas.ForceUpdateCanvases();
      Grab(view,rt,tex,$"{root}/{size.x}x{size.y}-{t:F1}.png");
     }
     intro.Seek(COgheBossIntro.Warning+.1f);yield return null;
     Grab(view,rt,tex,$"{root}/{size.x}x{size.y}-play.png");
    }
    finally{if(intro!=null)Object.Destroy(intro.gameObject);COgheBossIntro.PreviewSize=null;RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
   }
  }
 }
}
