using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Inks injected into COghe (Mrk 01/10): carried by the particles, spread by holding, rinsed away.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator InkIsCarriedByTheParticlesAndRinsesAway()
  {
   yield return Load(1);yield return Frames(20);
   var ink=COgheInking.Attach(game);var surface=game.Matter.GetComponent<VenomSurface>();
   int target=0;float best=float.NegativeInfinity;for(int i=0;i<32;i++)if(surface.DrawnParticles[i].y>best){best=surface.DrawnParticles[i].y;target=i;}
   var at=surface.DrawnParticles[target];
   for(int f=0;f<30;f++){ink.Inject(at,"INK_OCEAN",1f/30,f/30f);yield return Frames(1);}
   Assert.AreEqual("INK_OCEAN",ink.Inks[0]);
   Assert.Greater(ink.Amount[target].x,.3f,"Ink where it went in");
   int far=0;float farthest=0;for(int i=0;i<32;i++){float d=(surface.DrawnParticles[i]-at).magnitude;if(d>farthest){farthest=d;far=i;}}
   Assert.Less(ink.Amount[far].x,ink.Amount[target].x*.5f,"Less far away");
   var mesh=surface.GetComponentInChildren<MeshFilter>().sharedMesh;var inks=new System.Collections.Generic.List<Vector4>();mesh.GetUVs(2,inks);
   Assert.AreEqual(mesh.vertexCount,inks.Count,"The skin carries ink per vertex");
   float maxSkin=0;foreach(var v in inks)maxSkin=Mathf.Max(maxSkin,v.x);Assert.Greater(maxSkin,.2f,"and shows it");
   ink.Rinse();yield return Frames(60);
   foreach(var a in ink.Amount)Assert.Less(a.x+a.y+a.z+a.w,.001f,"Rinsed");Assert.IsNull(ink.Inks[0]);
   Object.Destroy(ink);
  }

  // Preview reels (Artifacts/Inks/<singles|mixes>/frame_#####.png + sounds.txt): single syringes, then mixes.
  [Explicit("Renders the ink injection reels")]
  [UnityTest] public IEnumerator RenderInkInjections()
  {
   bool enabled=COghePersonality.Enabled;COghePersonality.Enabled=false;   // COghe holds still on the stage
   try
   {
    foreach(var reel in new[]{"singles","mixes"})
    {
     string root="Artifacts/Inks/"+reel;if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
     var sounds=new System.Text.StringBuilder();int frame=0;
     System.Action<string,float> heard=(clip,volume)=>sounds.AppendLine($"{frame/30f:F3} {clip} {volume:F2}");
     yield return EnterFurnishedHome(10);yield return Frames(120);
     COgheAudio.Heard+=heard;
     var cam=new GameObject("Ink reel camera").AddComponent<Camera>();cam.CopyFrom(game.Owner.View);cam.enabled=false;cam.orthographic=true;cam.aspect=540f/1170;cam.nearClipPlane=.01f;cam.farClipPlane=30;
     var rt=RenderTexture.GetTemporary(540,1170,24);var tex=new Texture2D(540,1170,TextureFormat.RGB24,false);
     var ink=COgheInking.Attach(game,7);var surface=game.Matter.GetComponent<VenomSurface>();
     var syringe=COgheSyringe.Create(game.transform,game.Matter.Profile.Skin);syringe.gameObject.SetActive(false);
     COgheAccessories extras=null;float yaw=-20;
     Vector3 Centre(){var c=Vector3.zero;for(int i=0;i<32;i++)c+=surface.DrawnParticles[i];return c/32;}
     try
     {
      IEnumerator Roll(float seconds,float spin=8)
      {
       for(int f=0;f<Mathf.RoundToInt(seconds*30);f++)
       {
        yield return Frames(1);yaw+=spin/30;
        cam.transform.rotation=Quaternion.Euler(26,yaw,0);cam.orthographicSize=.14f;
        cam.transform.position=Centre()+Vector3.up*.02f-cam.transform.forward*1.2f;
        Grab(cam,rt,tex,$"{root}/frame_{frame:00000}.png");frame++;
       }
      }
      // a point on the skin in a direction of the camera's frame (x right, y up, z toward the camera)
      Vector3 SkinPoint(Vector3 dir)
      {
       var c=Centre();var r=cam.transform.right;var u=Vector3.up;var toward=-Vector3.ProjectOnPlane(cam.transform.forward,Vector3.up).normalized;
       var d=(r*dir.x+u*dir.y+toward*dir.z).normalized;float reach=0;
       for(int i=0;i<32;i++)reach=Mathf.Max(reach,Vector3.Dot(surface.DrawnParticles[i]-c,d));
       return c+d*(reach+.008f);
      }
      IEnumerator Shot(string id,Vector3 dir,float hold)
      {
       var spec=COgheInks.Find(id);syringe.SetInk(spec);syringe.gameObject.SetActive(true);
       Vector3 Out(){var c=Centre();var p=SkinPoint(dir);return (p-c).normalized*.6f+Vector3.up*.25f-cam.transform.forward*.35f;}
       for(int f=0;f<14;f++){float k=f/13f;var p=SkinPoint(dir);syringe.Pose(p,Out(),Mathf.Lerp(.05f,-.005f,Mathf.SmoothStep(0,1,k)),1);if(f==10)COgheAudio.Instance?.Play("glass_tok",.4f,0,.1f);yield return Roll(1f/30);}
       int n=Mathf.RoundToInt(hold*30);
       for(int f=0;f<n;f++){var p=SkinPoint(dir);ink.Inject(p,id,1f/30,f/30f);syringe.Pose(p,Out(),-.005f,1-(f+1f)/n);if(f%9==0)COgheAudio.Instance?.Play("creature_slide",.18f,0,.2f);yield return Roll(1f/30);}
       for(int f=0;f<10;f++){var p=SkinPoint(dir);syringe.Pose(p,Out(),Mathf.Lerp(-.005f,.06f,f/9f),0);yield return Roll(1f/30);}
       syringe.gameObject.SetActive(false);COgheAudio.Instance?.Play("creature_happy",.35f,0,.3f);
      }
      IEnumerator Clear(bool inside)
      {
       if(extras!=null){Object.Destroy(extras);extras=null;yield return Frames(1);}
       if(inside)extras=COgheAccessories.Attach(game,COgheAccessory.Inclusions);
      }
      var top=new Vector3(0,1,.55f);var left=new Vector3(-1,.45f,.6f);var right=new Vector3(1,.45f,.6f);
      yield return Roll(1f);
      if(reel=="singles")
      {
       foreach(var id in new[]{"INK_OCEAN","INK_FIREFLY","INK_GOLD","INK_STARDUST","INK_AURORA","INK_LAVA","INK_GALAXY","INK_PRISM"})
       {
        bool inside=id=="INK_STARDUST"||id=="INK_GALAXY";
        yield return Shot(id,top,2.2f);
        if(inside)yield return Clear(true);
        yield return Roll(1.8f);
        ink.Rinse();yield return Roll(.9f);
        yield return Clear(false);
       }
      }
      else
      {
       yield return Shot("INK_OCEAN",left,1.8f);yield return Shot("INK_CORAL",right,1.8f);yield return Roll(2.2f);ink.Rinse();yield return Roll(.9f);
       yield return Shot("INK_FIREFLY",top,1.6f);yield return Shot("INK_GALAXY",left,1.8f);yield return Clear(true);yield return Roll(2.2f);ink.Rinse();yield return Roll(.9f);yield return Clear(false);
       yield return Shot("INK_SAKURA",top,1.8f);yield return Shot("INK_PEARL",left,1.4f);yield return Shot("INK_GOLD",right,1f);yield return Roll(2.2f);ink.Rinse();yield return Roll(.9f);
       yield return Shot("INK_LAVA",top,1.8f);yield return Shot("INK_GOLD",left,1.4f);yield return Roll(2.2f);ink.Rinse();yield return Roll(.9f);
       yield return Shot("INK_MINT",left,1.5f);yield return Shot("INK_AURORA",right,1.6f);yield return Shot("INK_PRISM",top,1f);yield return Roll(2.4f);
      }
      File.WriteAllText($"{root}/sounds.txt",sounds.ToString());
     }
     finally
     {
      COgheAudio.Heard-=heard;Object.Destroy(cam.gameObject);RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);
      if(syringe!=null)Object.Destroy(syringe.gameObject);COgheHomeRoom.UnlockedLevelOverride=null;
     }
    }
   }
   finally{COghePersonality.Enabled=enabled;}
  }
 }
}
