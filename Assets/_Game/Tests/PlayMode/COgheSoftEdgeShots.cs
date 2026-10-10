using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
 // Review stills for the soft edges (Mrk 10/10/2026: tape round the foot of the glass, rounded blocks). Explicit.
 // COGHE_SOFT_SCENES lists scene names (e.g. "COgheSpatial01,COgheSpatialPlusK01"); COGHE_SOFT_TAG names the set
 // ("before"/"after"). Each scene: the game's portrait view and a closer three-quarter view, in Artifacts/Soft.
 public sealed partial class COgheSpatialCampaignTests
 {
  private static void SoftShot(Camera camera,string path,int w,int h)
  {
   var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.antiAliasing=4;var tex=new Texture2D(w,h,TextureFormat.RGB24,false);var prev=RenderTexture.active;
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
   finally{camera.targetTexture=null;RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);}
  }
  [UnityTest,Explicit,Timeout(900000)] public IEnumerator SoftEdgeShots()
  {
   Directory.CreateDirectory("Artifacts/Soft");
   string tag=Environment.GetEnvironmentVariable("COGHE_SOFT_TAG")??"shot";
   string scenes=Environment.GetEnvironmentVariable("COGHE_SOFT_SCENES")??"COgheSpatial01";
   foreach(var name in scenes.Split(','))
   {
    yield return LoadScene(name.Trim());yield return Wait(1);
    game.CameraRig.Frame(1080,1920,0,true);SoftShot(game.Owner.View,$"Artifacts/Soft/{tag}-{name}-game.png",1080,1920);
    // closer, lower and from the front left: the foot of the glass and the blocks' edges
    var cam=Object.Instantiate(game.Owner.View);cam.name="Soft review camera";cam.enabled=false;
    cam.transform.rotation=game.Root.rotation*Quaternion.Euler(26,-34,0);
    cam.orthographic=true;cam.orthographicSize=.27f;cam.aspect=1.5f;
    cam.transform.position=game.Root.TransformPoint(new Vector3(0,-.20f,0))-cam.transform.forward*3f;
    SoftShot(cam,$"Artifacts/Soft/{tag}-{name}-close.png",1536,1024);
    Object.Destroy(cam.gameObject);
   }
  }
 }
}
