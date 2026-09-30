using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // COghe's home: the furnished room, item unlocks and ghosts, the creature living in it.
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator EnterFurnishedHome(int unlockedLevel)
  {
   COgheHomeRoom.UnlockedLevelOverride=unlockedLevel;
   yield return Load(1);yield return Frames(10);
   game.Progress.HomeUnlocked=true;game.EnterHome();yield return Frames(10);
   Assert.IsNotNull(game.HomeRoom,"The Spatial Home is the furnished room");
  }
  [UnityTest] public IEnumerator HomeShowsEarnedItemsAndGhostsLockedOnes()
  {
   try
   {
    yield return EnterFurnishedHome(14);
    var room=game.HomeRoom;Assert.AreEqual(14,room.Items.Count);
    foreach(var item in room.Items)Assert.AreEqual(item.UnlockLevel<=14,item.Root.activeSelf,item.Id+" shows only once earned");
    var mirror=room.Find("MIRROR");room.ShowGhost(mirror);
    Assert.IsTrue(mirror.Root.activeSelf,"A locked item can be previewed");
    foreach(var r in mirror.Root.GetComponentsInChildren<Renderer>())Assert.AreEqual("Home ghost",r.sharedMaterial.name);
    room.HideGhost();Assert.IsFalse(mirror.Root.activeSelf);
    foreach(var r in room.Find("BED").Root.GetComponentsInChildren<Renderer>())Assert.AreNotEqual("Home ghost",r.sharedMaterial.name);
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;}
  }
  [UnityTest] public IEnumerator HomeCreatureLivesInTheRoom()
  {
   try
   {
    yield return EnterFurnishedHome(50);
    var p=game.Personality;var room=game.HomeRoom;bool played=false,moved=false;var start=game.Motion.Centre(0);
    for(int f=0;f<30*45;f++)   // 45 s of life
    {
     yield return Frames(1);
     played|=p.Playing!=null;moved|=Vector3.Distance(start,game.Motion.Centre(0))>.08f;
     var local=room.Root.InverseTransformPoint(game.Motion.Centre(0));
     Assert.Less(Mathf.Abs(local.x),COgheHomeRoom.HalfWidth,"Stays on the floor");Assert.Less(Mathf.Abs(local.z),COgheHomeRoom.HalfDepth,"Stays on the floor");
     Assert.Greater(local.y,-.02f,"Never falls through");
    }
    Assert.IsTrue(moved,"It wanders by itself");Assert.IsTrue(played,"It plays with its furniture");
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;}
  }
  [UnityTest] public IEnumerator HomeTouchesGetReactionsThenASulk()
  {
   try
   {
    yield return EnterFurnishedHome(10);yield return Frames(20);
    var p=game.Personality;
    p.TouchedInHome(game.Motion.Centre(0)+Vector3.right*.02f);yield return Frames(2);
    Assert.AreEqual(COgheAct.Home,p.Act,"A touch gets a reaction");
    for(int i=0;i<5;i++){p.TouchedInHome(game.Motion.Centre(0));yield return Frames(2);}
    Assert.IsTrue(p.Sulking,"Poked too much, it sulks");
    for(int f=0;f<30*12&&p.Act!=COgheAct.Home;f++)yield return Frames(1);
    Assert.AreEqual(COgheAct.Home,p.Act,"In its corner, turned away");Assert.IsTrue(p.Sulking);
    for(int f=0;f<30*8&&p.Sulking;f++)yield return Frames(1);
    Assert.IsFalse(p.Sulking,"And it gets over it");
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;}
  }

  // A 30 fps reel of life at Home (Artifacts/HomeReel/frame_#####.png + sounds.txt with the one-shots heard).
  [Explicit("Renders the Home reel")]
  [UnityTest] public IEnumerator RenderHomeReel()
  {
   string root="Artifacts/HomeReel";if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
   var sounds=new System.Text.StringBuilder();int frame=0;
   System.Action<string,float> heard=(clip,volume)=>sounds.AppendLine($"{frame/30f:F3} {clip} {volume:F2}");
   COgheAudio.Heard+=heard;
   Camera cam=null;
   var rt=RenderTexture.GetTemporary(540,1170,24);var tex=new Texture2D(540,1170,TextureFormat.RGB24,false);
   try
   {
    yield return EnterFurnishedHome(50);   // loads the scene: make the camera afterwards
    cam=new GameObject("Reel camera").AddComponent<Camera>();cam.CopyFrom(game.Owner.View);cam.enabled=false;cam.orthographic=true;cam.aspect=540f/1170;cam.transform.rotation=Quaternion.Euler(42,-6,0);
    var room=game.HomeRoom;var p=game.Personality;
    float overview=Mathf.Max((COgheHomeRoom.HalfWidth*2+.22f)*.5f/(540f/1170),1.08f);
    Vector3 focus=room.Root.position+Vector3.up*.05f;float size=overview;
    IEnumerator Roll(float seconds,bool follow)
    {
     for(int f=0;f<seconds*30;f++)
     {
      yield return Frames(1);
      Vector3 target=room.Root.position+Vector3.up*.05f;float targetSize=overview;
      var item=p.Playing;
      if(item!=null){var b=room.BoundsOf(item);target=Vector3.Lerp(b.center,game.Motion.Centre(0),.35f);targetSize=.34f;}
      else if(follow){target=game.Motion.Centre(0)+Vector3.up*.03f;targetSize=.32f;}
      float k=1-Mathf.Exp(-2.2f/30);focus=Vector3.Lerp(focus,target,k);size=Mathf.Lerp(size,targetSize,k);
      cam.orthographicSize=size;cam.transform.position=focus-cam.transform.forward*1.2f;
      Grab(cam,rt,tex,$"{root}/frame_{frame:00000}.png");frame++;
     }
    }
    yield return Roll(7,false);   // the furniture pops in; COghe heads for the newest
    foreach(var id in new[]{"BED","DUMBBELL","SWING","SLIDE","TRAMPOLINE","XYLOPHONE","WHEEL","MIRROR"})
    {
     p.PlayNow(room.Find(id));
     for(int guard=0;guard<30*9&&p.Playing!=null;guard+=1)yield return Roll(1f/30,false);
     yield return Roll(.6f,false);
    }
    // touches: a few reactions, then too many
    for(int i=0;i<3;i++){p.TouchedInHome(game.Motion.Centre(0)+Vector3.right*.02f);yield return Roll(1.7f,true);}
    for(int i=0;i<5;i++){p.TouchedInHome(game.Motion.Centre(0));yield return Roll(.1f,true);}
    for(int guard=0;guard<30*14&&p.Sulking;guard++)yield return Roll(1f/30,true);
    yield return Roll(1.5f,false);
    File.WriteAllText($"{root}/sounds.txt",sounds.ToString());
   }
   finally{COgheAudio.Heard-=heard;COgheHomeRoom.UnlockedLevelOverride=null;if(cam!=null)Object.Destroy(cam.gameObject);RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
  }

  // Previews: Artifacts/Home/overview.png, top.png, and <ITEM>/frame_<n>.png for every item's game.
  [Explicit("Renders Home previews")]
  [UnityTest] public IEnumerator RenderHome()
  {
   try
   {
    yield return EnterFurnishedHome(50);yield return Frames(190);   // the new furniture pops in first
    var room=game.HomeRoom;var p=game.Personality;
    string root="Artifacts/Home";if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
    var cam=new GameObject("Home preview camera").AddComponent<Camera>();cam.CopyFrom(game.Owner.View);cam.enabled=false;cam.orthographic=true;
    var rt=RenderTexture.GetTemporary(540,1170,24);var tex=new Texture2D(540,1170,TextureFormat.RGB24,false);
    void Shot(Vector3 focus,float size,Quaternion rotation,string file){cam.aspect=540f/1170;cam.transform.rotation=rotation;cam.orthographicSize=size;cam.transform.position=focus-cam.transform.forward*1.2f;Grab(cam,rt,tex,file);}
    var overviewRotation=Quaternion.Euler(42,-6,0);Vector3 centre=room.Root.position+Vector3.up*.05f;
    float fit=Mathf.Max((COgheHomeRoom.HalfWidth*2+.22f)*.5f/(540f/1170),1.08f);
    Shot(centre,fit,overviewRotation,$"{root}/overview.png");
    Shot(centre,.92f,Quaternion.Euler(90,0,0),$"{root}/top.png");
    foreach(var item in room.Items)
    {
     string dir=$"{root}/{item.Id}";Directory.CreateDirectory(dir);
     p.PlayNow(item);
     for(int f=0;f<48;f++)
     {
      yield return Frames(4);   // 0.13 s steps
      var b=room.BoundsOf(item);Shot(Vector3.Lerp(b.center,game.Motion.Centre(0),.35f),.3f,overviewRotation,$"{dir}/frame_{f:00}.png");
      if(p.Playing==null)break;
     }
    }
    Object.Destroy(cam.gameObject);RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);
   }
   finally{COgheHomeRoom.UnlockedLevelOverride=null;}
  }
 }
}
