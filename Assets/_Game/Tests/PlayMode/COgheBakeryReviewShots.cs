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
   string sceneName=Environment.GetEnvironmentVariable("COGHE_BAKERY_SCENE")??"COgheSpatialPlusN41";   // e.g. COgheSpatial01
   yield return LoadScene(sceneName);yield return Wait(1);
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
 
  // The cherry cannot be taken without the gears (no exit pull any more): from the start, taps on the cherry and walks to the
  // foot of the exit platform from every side, 90 s in all, never win; then the reference solution still does.
  [UnityTest,Explicit,Timeout(900000)] public IEnumerator BakeryCherryNoShortcut()
  {
   yield return LoadScene("COgheSpatialPlusN41");yield return Wait(1);
   var goal=game.CherryGoal;Assert.IsNotNull(goal,"N41 has a cherry goal");var cherry=goal.Cherry.position;
   var spots=new[]{new Vector3(.20f,-.30f,.08f),new Vector3(.05f,-.30f,.20f),new Vector3(.36f,-.30f,.20f),new Vector3(.36f,-.30f,.10f),new Vector3(.05f,-.30f,.10f)};
   for(int k=0;k<spots.Length&&!game.Owner.Completed;k++)
   {
    game.Motion.Move(game.Motion.Selected,game.Root.TransformPoint(spots[k]+Vector3.up*.02f));yield return Wait(8);
    for(int t=0;t<3;t++){yield return Tap(cherry+game.Root.up*.01f);yield return Wait(3);}
    Assert.IsFalse(game.Owner.Completed,$"Won without the gears after walking to {spots[k]} and tapping the cherry");
   }
   Assert.IsFalse(game.Owner.Lost,"Lost while trying: "+game.Failure);
  }
 
  // Diagnosis for BakeryCherryNoShortcut: the same first walk and cherry taps, logging the body each half second and frames.
  [UnityTest,Explicit,Timeout(900000)] public IEnumerator BakeryShortcutTrace()
  {
   Directory.CreateDirectory("Artifacts/Bakery/trace");probeLog="Artifacts/Bakery/trace/log.txt";File.WriteAllText(probeLog,"");
   yield return LoadScene("COgheSpatialPlusN41");yield return Wait(1);var cherry=game.CherryGoal.Cherry.position;int f=0;
   game.Motion.Move(game.Motion.Selected,game.Root.TransformPoint(new Vector3(.20f,-.28f,.08f)));
   for(int s=0;s<16;s++){yield return Wait(.5f);Note($"walk t={s*.5f:F1} {Where()}");}
   for(int t=0;t<3&&!game.Owner.Completed;t++)
   {
    yield return Tap(cherry+game.Root.up*.01f);var o=game.Motion.Get(0);
    Note($"tap {t}: path={(o!=null?string.Join(" ",o.Path.ConvertAll(q=>q.ToString("F2"))):"none")} exit={(o!=null&&o.Exit)}");
    for(int s=0;s<12&&!game.Owner.Completed;s++){yield return Wait(.25f);Note($"  t={s*.25f:F2} {Where()}");ShotClose($"../Bakery/trace/f{f++:000}");}
   }
   Note("completed="+game.Owner.Completed);
  }
 }
}
