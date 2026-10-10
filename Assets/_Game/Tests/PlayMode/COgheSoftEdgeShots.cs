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

  // Slick looks for review (Mrk 10/10/2026: another colour or shader that shows slipperiness better). Each look is a pair
  // of COghe/Slick materials, the tape (edges marked) and the slick faces, swapped in for "Slick tape" and "Lavender
  // slippery" while rendering. Stills per scene and look, and with COGHE_SLICK_CLIP=1 four seconds of frames per look
  // (Artifacts/Soft/clip-<look>) on the first scene, so the sliding shine shows.
  private static Material SlickLook(string look,bool tape)
  {
   var m=new Material(Shader.Find("COghe/Slick"));
   void C(string p,float r,float g,float b)=>m.SetColor(p,new Color(r,g,b));
   switch(look)
   {
    case "lilac":   // the slick colour kept, glossy, wave lines ≈
     C("_BaseColor",.62f,.54f,.92f);C("_PatternColor",.52f,.44f,.86f);C("_EdgeColor",.44f,.37f,.80f);m.SetFloat("_Pattern",2);m.SetFloat("_PatternScale",tape?.011f:.022f);
     m.SetFloat("_Gloss",220);m.SetFloat("_Spec",1.4f);m.SetFloat("_Reflect",.6f);m.SetFloat("_Fresnel",.35f);m.SetFloat("_SheenStrength",.45f);break;
    case "ice":     // pale ice, very glossy, sparkle
     C("_BaseColor",.80f,.91f,.98f);C("_PatternColor",.68f,.84f,.95f);C("_EdgeColor",.52f,.73f,.90f);m.SetFloat("_Pattern",1);m.SetFloat("_PatternScale",tape?.014f:.03f);
     m.SetFloat("_Gloss",420);m.SetFloat("_Spec",1.8f);m.SetFloat("_Reflect",.9f);m.SetFloat("_Fresnel",.55f);m.SetFloat("_SheenStrength",.6f);m.SetFloat("_Sparkle",1.4f);m.SetFloat("_Iridescence",.12f);break;
    case "pearl":   // soap: pearl with a rainbow film and bubble rings
     C("_BaseColor",.86f,.80f,.97f);C("_PatternColor",.68f,.60f,.92f);C("_EdgeColor",.62f,.53f,.88f);m.SetFloat("_Pattern",3);m.SetFloat("_PatternScale",tape?.012f:.025f);
     m.SetFloat("_Gloss",260);m.SetFloat("_Spec",1.4f);m.SetFloat("_Reflect",.7f);m.SetFloat("_Fresnel",.5f);m.SetFloat("_SheenStrength",.4f);m.SetFloat("_Iridescence",1);break;
    case "gel":     // wet gel: clear rippling coat flowing slowly over lilac, droplets standing on it
     C("_BaseColor",.58f,.50f,.90f);C("_EdgeColor",.44f,.37f,.80f);m.SetFloat("_Pattern",0);m.SetFloat("_Mode",1);
     m.SetFloat("_Ripple",1.1f);m.SetFloat("_RippleScale",22);m.SetFloat("_Drops",tape?.35f:.4f);
     m.SetFloat("_Gloss",320);m.SetFloat("_Spec",1.7f);m.SetFloat("_Reflect",.9f);m.SetFloat("_Fresnel",.4f);m.SetFloat("_SheenStrength",.25f);break;
    case "oil":     // pastel holographic film drifting over lilac
     C("_BaseColor",.60f,.52f,.91f);C("_EdgeColor",.44f,.37f,.80f);m.SetFloat("_Pattern",0);m.SetFloat("_Mode",2);m.SetFloat("_Film",1);
     m.SetFloat("_Gloss",300);m.SetFloat("_Spec",1.5f);m.SetFloat("_Reflect",.75f);m.SetFloat("_Fresnel",.45f);m.SetFloat("_SheenStrength",.3f);break;
    case "glitter": // glitter enamel: deep lavender coat with twinkling flakes
     C("_BaseColor",.54f,.43f,.88f);C("_EdgeColor",.40f,.31f,.74f);m.SetFloat("_Pattern",0);m.SetFloat("_Mode",3);m.SetFloat("_Glitter",2.5f);
     m.SetFloat("_Gloss",260);m.SetFloat("_Spec",1.5f);m.SetFloat("_Reflect",.7f);m.SetFloat("_Fresnel",.4f);m.SetFloat("_SheenStrength",.3f);break;
   }
   m.SetFloat("_EdgeBand",tape?.12f:0);m.SetFloat("_SheenSpeed",.25f);m.SetFloat("_SheenWidth",.05f);m.name=$"Slick {look}{(tape?" tape":"")}";
   return m;
  }
  [UnityTest,Explicit,Timeout(1800000)] public IEnumerator SlickLooks()
  {
   Directory.CreateDirectory("Artifacts/Soft");
   string scenes=Environment.GetEnvironmentVariable("COGHE_SOFT_SCENES")??"COgheSpatial02";bool clip=Environment.GetEnvironmentVariable("COGHE_SLICK_CLIP")=="1";
   var looks=(Environment.GetEnvironmentVariable("COGHE_SLICK_LOOKS")??"current,lilac,ice,pearl").Split(',');
   int frames=int.TryParse(Environment.GetEnvironmentVariable("COGHE_SLICK_FRAMES"),out int fr)?fr:120;
   Time.captureFramerate=30;
   try{
   bool first=true;
   foreach(var name in scenes.Split(','))
   {
    yield return LoadScene(name.Trim());yield return Wait(1);
    // the slots that are slick: per renderer, its original materials and which slots to swap
    var slots=new System.Collections.Generic.List<(Renderer r,Material[] original,bool[] tape,bool[] face)>();
    foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
    {
     var mats=r.sharedMaterials;var t=new bool[mats.Length];var f=new bool[mats.Length];bool any=false;
     for(int i=0;i<mats.Length;i++){if(mats[i]==null)continue;t[i]=mats[i].name.StartsWith("Slick tape");f[i]=mats[i].name.StartsWith("Lavender slippery");any|=t[i]||f[i];}
     if(any)slots.Add((r,mats,t,f));
    }
    var pairs=new System.Collections.Generic.Dictionary<string,(Material tape,Material face)>();
    foreach(var look in looks)if(look!="current")pairs[look]=(SlickLook(look,true),SlickLook(look,false));
    void Use(string look)
    {
     foreach(var (r,original,t,f) in slots)
     {
      var mats=(Material[])original.Clone();
      if(look!="current")for(int i=0;i<mats.Length;i++){if(t[i])mats[i]=pairs[look].tape;else if(f[i])mats[i]=pairs[look].face;}
      r.sharedMaterials=mats;
     }
    }
    var cam=Object.Instantiate(game.Owner.View);cam.name="Slick review camera";cam.enabled=false;
    cam.transform.rotation=game.Root.rotation*Quaternion.Euler(26,-34,0);cam.orthographic=true;cam.orthographicSize=.27f;cam.aspect=1.5f;
    cam.transform.position=game.Root.TransformPoint(new Vector3(0,-.20f,0))-cam.transform.forward*3f;
    foreach(var look in looks)
    {
     Use(look);
     game.CameraRig.Frame(1080,1920,0,true);SoftShot(game.Owner.View,$"Artifacts/Soft/slick-{look}-{name}-game.png",1080,1920);
     SoftShot(cam,$"Artifacts/Soft/slick-{look}-{name}-close.png",1536,1024);
    }
    if(clip&&first)
    {
     foreach(var look in looks)Directory.CreateDirectory($"Artifacts/Soft/clip-{look}");
     for(int frame=0;frame<frames;frame++)
     {
      foreach(var look in looks){Use(look);SoftShot(cam,$"Artifacts/Soft/clip-{look}/{frame:00000}.png",768,512);}
      yield return null;
     }
    }
    Use("current");first=false;
    Object.Destroy(cam.gameObject);
   }
   }finally{Time.captureFramerate=0;}
  }
 }
}
