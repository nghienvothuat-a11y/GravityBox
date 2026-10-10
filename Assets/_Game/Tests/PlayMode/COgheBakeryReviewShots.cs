using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace GravityBox.Tests
{
 // Bakery review level (Mrk 10/10/2026): stills of the dressed level 49 (N41) for review. Explicit; Artifacts/Bakery/.
 // "game" is the gameplay camera's framing; "concept" a three-quarter view like Codex's concept boards.
 public sealed partial class COgheSpatialCampaignTests
 {
  private void BakeryShot(Camera camera,string name,int w,int h)
  {
   var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.antiAliasing=4;var tex=new Texture2D(w,h,TextureFormat.RGB24,false);var prev=RenderTexture.active;
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes($"Artifacts/Bakery/{name}.png",tex.EncodeToPNG());}
   finally{camera.targetTexture=null;RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);}
  }
  [UnityTest,Explicit,Timeout(600000)] public IEnumerator BakeryReviewShots()
  {
   Directory.CreateDirectory("Artifacts/Bakery");
   yield return LoadScene("COgheSpatialPlusN41");yield return Wait(1);
   game.CameraRig.Frame(1080,1920,0,true);BakeryShot(game.Owner.View,"n41-game",1080,1920);
   var low=new System.Text.StringBuilder();
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {var b=r.bounds;var c=game.Root.InverseTransformPoint(b.center);if(r.enabled&&r.gameObject.activeInHierarchy&&b.size.x>.4f&&b.size.x<3f)low.AppendLine($"{r.name} | {(r.transform.parent!=null?r.transform.parent.name:"-")} | {(r.sharedMaterial!=null?r.sharedMaterial.name:"-")} | c {c:F3} size {b.size:F3}");}
   File.WriteAllText("Artifacts/Bakery/large-renderers.txt",low.ToString());
   var cam=Object.Instantiate(game.Owner.View);cam.name="Concept camera";cam.enabled=false;
   cam.transform.rotation=game.Root.rotation*Quaternion.Euler(38,-32,0);
   cam.orthographic=true;cam.orthographicSize=.40f;cam.aspect=1.5f;
   cam.transform.position=game.Root.TransformPoint(new Vector3(0,-.24f,0))-cam.transform.forward*3f;
   BakeryShot(cam,"n41-concept",1536,1024);
   Object.Destroy(cam.gameObject);
  }
 }
}
