using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Snacks at Home (Mrk 21/10/2026, five to test): Codex's models, eaten at once with a reaction each, nothing left behind.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator SnackModelsAreCodexsSmallLowPolyFoodInTheirOwnColours()
  {
   try
   {
    yield return EnterFurnishedHome(65);
    foreach(var snack in COgheSnacks.Catalog)
    {
     Assert.IsNotNull(Resources.Load<GameObject>("COgheSnacks/"+snack.Id),snack.Id+": Codex's FBX");
     var go=COgheSnacks.Build(snack,game.HomeRoom.Root,game.HomeRoom.Materials);
     try
     {
      int triangles=0;foreach(var f in go.GetComponentsInChildren<MeshFilter>())for(int s=0;s<f.sharedMesh.subMeshCount;s++)triangles+=(int)f.sharedMesh.GetIndexCount(s)/3;   // not readable: no .triangles
      Assert.Less(triangles,300,snack.Id+": under 300 triangles");
      Assert.AreEqual(0,go.GetComponentsInChildren<Collider>().Length,snack.Id+": no collider (it never pushes COghe)");
      var b=new Bounds(go.transform.position,Vector3.zero);foreach(var r in go.GetComponentsInChildren<Renderer>())b.Encapsulate(r.bounds);
      Assert.LessOrEqual(b.size.y,.041f,snack.Id+": at most 4 cm tall");Assert.LessOrEqual(Mathf.Max(b.size.x,b.size.z),.036f,snack.Id+": about 3 cm across");
      Assert.AreEqual(0,b.min.y-go.transform.position.y,.002f,snack.Id+": stands on its base");
      var colours=new System.Collections.Generic.HashSet<Color>();
      foreach(var r in go.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials){Assert.IsNotNull(m,snack.Id+": every slot has the room's material");colours.Add(m.color);}
      Assert.GreaterOrEqual(colours.Count,3,snack.Id+": its colours read from Codex's material names");
     }
     finally{Object.Destroy(go);}
    }
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;}
  }

  [UnityTest] public IEnumerator EverySnackIsEatenWithItsOwnReactionThenCOgheGoesBackToItsDay()
  {
   try
   {
    yield return EnterFurnishedHome(65);yield return Frames(200);   // the furniture pops in first
    var p=game.Personality;var room=game.HomeRoom;
    COgheShop.SetEnergyUsed(COgheEnergy.Max);   // empty, so each snack's energy shows
    foreach(var snack in COgheSnacks.Catalog)
    {
     int eaten=p.SnacksEaten,energy=COgheEnergy.Value;
     Assert.IsTrue(p.EatSnack(snack),snack.Id+": eaten at once");Assert.AreEqual(snack,p.Snack);Assert.IsFalse(p.CanEatSnack,"One snack at a time");
     float length=COghePersonality.SnackGulp+COghePersonality.ReactionLength(snack.Kind);bool fx=false;
     var start=p.Pose;float moved=0;
     for(int f=0;f<30*(length+1)&&p.Snack!=null;f++)
     {
      yield return Frames(1);
      fx|=p.SnackFx!=null&&p.SnackFx.Live>0;
      if(p.Snack!=null)moved=Mathf.Max(moved,Vector3.Distance(p.Pose.Centre,start.Centre),Mathf.Abs(p.Pose.Squash-1));
      var local=room.Root.InverseTransformPoint(game.Motion.Centre(0));
      Assert.Less(Mathf.Abs(local.x),COgheHomeRoom.HalfWidth,"Stays on the floor");Assert.Less(Mathf.Abs(local.z),COgheHomeRoom.HalfDepth,"Stays on the floor");
     }
     Assert.IsNull(p.Snack,snack.Id+": the show ends");Assert.AreEqual(eaten+1,p.SnacksEaten,snack.Id+": swallowed");
     Assert.AreEqual(Mathf.Min(COgheEnergy.Max,energy+snack.Energy),COgheEnergy.Value,snack.Id+": its energy");
     Assert.IsTrue(p.CanEatSnack,snack.Id+": ready for another");Assert.Greater(moved,.005f,snack.Id+": its body reacts");
     if(snack.Kind!=COgheTaste.Yummy&&snack.Kind!=COgheTaste.Sour)Assert.IsTrue(fx,snack.Id+": flames, snow or confetti");
     foreach(Transform t in room.Root)Assert.IsFalse(t.name==snack.Id,snack.Id+": nothing left in the room");
     for(int f=0;f<30*5&&p.SnackFx.Live>0;f++)yield return Frames(1);
     Assert.AreEqual(0,p.SnackFx.Live,snack.Id+": the bits fade away");
    }
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.SetEnergyUsed(0);}
  }

  // Hungry (Mrk 21/10/2026): a toy offered is turned down (a head shake, an apple of itself) and COghe does not go and
  // play; it rests with a sad look and asks for food. Fed again, it plays as before. Snacks are worth more than balls.
  [UnityTest] public IEnumerator AHungryCOgheTurnsItsToysDownUntilItIsFed()
  {
   try
   {
    foreach(var snack in COgheSnacks.Catalog)Assert.Greater(snack.Energy,COgheEnergy.PerBall*COgheFeedBalls.PerFeed,snack.Id+": more than a serving of balls");
    yield return EnterFurnishedHome(65);yield return Frames(200);
    var p=game.Personality;var room=game.HomeRoom;
    COgheHomeItem toy=null;foreach(var item in room.Items)if(room.Present(item)&&item.Id=="BALL")toy=item;
    Assert.IsNotNull(toy,"The ball is out");
    COgheShop.SetEnergyUsed(60);Assert.AreEqual(COgheHunger.Hungry,COgheEnergy.State);
    p.TakeHungryRefusal();p.PlayWith(toy);
    Assert.IsTrue(p.TakeHungryRefusal(),"It says no (the UI tells the player why)");
    for(int f=0;f<30*4;f++){yield return Frames(1);Assert.IsNull(p.Playing,"Hungry: no games");}
    Assert.AreNotEqual(COgheMood.Happy,p.EyeMood(0,out _),"Not a happy look");

    COgheShop.SetEnergyUsed(0);p.PlayWith(toy);Assert.IsFalse(p.TakeHungryRefusal());
    bool played=false;for(int f=0;f<30*12&&!played;f++){yield return Frames(1);played=p.Playing==toy;}
    Assert.IsTrue(played,"Fed: off to play");
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;COgheShop.SetEnergyUsed(0);}
  }

  // The snacks' icons for the Food tray (Explicit): each snack from its own model, three-quarter view, 256 px on a clear
  // background (black and white renders: the difference is the alpha), as RenderToyIcons. Resources/COgheHome/Icons/<id>.png.
  [UnityTest,Explicit] public IEnumerator RenderSnackIcons()
  {
   try
   {
    yield return EnterFurnishedHome(65);yield return Frames(40);
    var room=game.HomeRoom;const int layer=30,size=512,half=256;
    var cam=new GameObject("Icon camera").AddComponent<Camera>();cam.orthographic=true;cam.clearFlags=CameraClearFlags.SolidColor;cam.cullingMask=1<<layer;cam.enabled=false;
    var rt=new RenderTexture(size,size,24,RenderTextureFormat.ARGB32){antiAliasing=4};cam.targetTexture=rt;
    Texture2D Shot(Color background){cam.backgroundColor=background;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,size,size),0,0);tex.Apply();RenderTexture.active=null;return tex;}
    foreach(var snack in COgheSnacks.Catalog)
    {
     var go=COgheSnacks.Build(snack,room.Root,room.Materials);go.transform.localPosition=new Vector3(0,.5f,0);
     var renderers=go.GetComponentsInChildren<Renderer>();foreach(var r in renderers)r.gameObject.layer=layer;
     var b=new Bounds(renderers[0].bounds.center,Vector3.zero);foreach(var r in renderers)b.Encapsulate(r.bounds);
     cam.transform.rotation=room.Root.rotation*Quaternion.Euler(28,-24+180,0);   // Codex's fronts face -Z
     cam.transform.position=b.center-cam.transform.forward*1f;cam.nearClipPlane=.01f;cam.farClipPlane=3;
     cam.orthographicSize=Mathf.Max(b.extents.x,b.extents.y,b.extents.z)*1.5f;
     var black=Shot(Color.black);var white=Shot(Color.white);
     var pb=black.GetPixels();var pw=white.GetPixels();var full=new Color[pb.Length];
     for(int i=0;i<pb.Length;i++)
     {
      float a=Mathf.Clamp01(1-((pw[i].r-pb[i].r)+(pw[i].g-pb[i].g)+(pw[i].b-pb[i].b))/3f);
      full[i]=new Color(pb[i].r,pb[i].g,pb[i].b,a);
     }
     var po=new Color[half*half];
     for(int y=0;y<half;y++)for(int x=0;x<half;x++)
     {
      Color c=(full[2*y*size+2*x]+full[2*y*size+2*x+1]+full[(2*y+1)*size+2*x]+full[(2*y+1)*size+2*x+1])/4f;
      po[y*half+x]=c.a>.003f?new Color(c.r/c.a,c.g/c.a,c.b/c.a,c.a):new Color(0,0,0,0);
     }
     var icon=new Texture2D(half,half,TextureFormat.RGBA32,false);icon.SetPixels(po);icon.Apply();
     File.WriteAllBytes("Assets/_Game/Venom/Resources/COgheHome/Icons/"+snack.Id+".png",icon.EncodeToPNG());
     go.SetActive(false);Object.Destroy(go);   // Destroy waits for the frame's end: out of the next snack's shot now
    }
    Object.Destroy(cam.gameObject);rt.Release();
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;}
  }
 }
}
