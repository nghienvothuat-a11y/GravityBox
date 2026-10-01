using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Accessory test (Mrk 01/10): a hat on COghe's crest, or little fish and star bits floating inside a clear body.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator HatSitsOnTheBiggestPieceAndTouchesNothing()
  {
   yield return Load(1);yield return Frames(30);
   var a=COgheAccessories.Attach(game);a.Dress("HAT_BEANIE",new string[0]);yield return Frames(30);
   Assert.IsTrue(a.HatWorn,"Worn on the crest");
   float top=float.NegativeInfinity;foreach(var b in game.Matter.Bodies)top=Mathf.Max(top,b.position.y);
   Assert.That(a.HatPosition.y-top,Is.InRange(-.01f,.05f),"Just over the highest particle");
   Assert.IsEmpty(a.Holder.GetComponentsInChildren<Collider>(true),"Presentation only: no colliders");Assert.IsEmpty(a.Holder.GetComponentsInChildren<Rigidbody>(true));
   // split 22 / 10: the hat stays on the bigger piece
   var first=new bool[32];for(int i=0;i<22;i++)first[i]=true;game.Matter.Partition(game.Matter.Groups[0],first);
   yield return Frames(30);
   Vector3 big=Vector3.zero;for(int i=0;i<22;i++)big+=game.Matter.Bodies[i].position;big/=22;
   Assert.Less(new Vector2(a.HatPosition.x-big.x,a.HatPosition.z-big.z).magnitude,.06f,"On the bigger piece");
   Object.Destroy(a);
  }

  // Comparison reels: Artifacts/Accessories/<hats|inclusions>/frame_#####.png + sounds.txt.
  [Explicit("Renders the accessory comparison reels")]
  [UnityTest] public IEnumerator RenderAccessoryReels()
  {
   foreach(var reel in new[]{"hats","inclusions"})
   {
    string root="Artifacts/Accessories/"+reel;if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
    var sounds=new System.Text.StringBuilder();int frame=0;
    System.Action<string,float> heard=(clip,volume)=>sounds.AppendLine($"{frame/30f:F3} {clip} {volume:F2}");
    Camera cam=null;var rt=RenderTexture.GetTemporary(540,1170,24);var tex=new Texture2D(540,1170,TextureFormat.RGB24,false);
    try
    {
     bool clear=reel=="inclusions";
     COgheAccessories acc=null;Vector3 focus=Vector3.zero;float size=.2f;bool placed=false;
     IEnumerator Roll(float seconds,float targetSize,Quaternion view)
     {
      for(int f=0;f<seconds*30;f++)
      {
       yield return Frames(1);
       var p=game.Personality;Vector3 target=(p!=null&&game.Home?p.SkinCentre:game.Motion.Centre(0))+Vector3.up*.02f;
       if(!placed){focus=target;size=targetSize;placed=true;}
       float k=1-Mathf.Exp(-3f/30);focus=Vector3.Lerp(focus,target,k);size=Mathf.Lerp(size,targetSize,k);
       cam.transform.rotation=view;cam.orthographicSize=size;cam.transform.position=focus-cam.transform.forward*1.2f;
       Grab(cam,rt,tex,$"{root}/frame_{frame:00000}.png");frame++;
      }
     }
     // Home: life, acts and touches
     yield return EnterFurnishedHome(26);yield return Frames(200);
     COgheAudio.Heard+=heard;
     cam=new GameObject("Accessory reel camera").AddComponent<Camera>();cam.CopyFrom(game.Owner.View);cam.enabled=false;cam.orthographic=true;cam.aspect=540f/1170;cam.nearClipPlane=.01f;cam.farClipPlane=30;
     var home=Quaternion.Euler(30,-12,0);var room=game.HomeRoom;var pers=game.Personality;
     acc=COgheAccessories.Attach(game);acc.Dress(clear?"":"HAT_BEANIE",clear?new[]{"FLOAT_FISH","FLOAT_STARS"}:new string[0]);
     yield return Roll(3.5f,.17f,home);
     pers.TouchedInHome(game.Motion.Centre(0)+Vector3.right*.02f);yield return Roll(1.8f,.17f,home);
     if(!clear)acc.Dress("HAT_PARTY",new string[0]);
     pers.PlayNow(room.Find(clear?"BALL":"TRAMPOLINE"));
     for(int g=0;g<30*8&&pers.Playing!=null;g++)yield return Roll(1f/30,clear?.24f:.26f,home);
     yield return Roll(.6f,.17f,home);
     if(!clear){acc.Dress("HAT_FLOWER",new string[0]);pers.PlayNow(room.Find("SLIDE"));for(int g=0;g<30*6&&pers.Playing!=null;g++)yield return Roll(1f/30,.26f,home);yield return Roll(1f,.17f,home);}
     else{pers.TouchedInHome(game.Motion.Centre(0)+Vector3.left*.02f);yield return Roll(1.8f,.17f,home);}
     COgheAudio.Heard-=heard;Object.Destroy(cam.gameObject);cam=null;
     // a level: walking, then split in two
     COgheHomeRoom.UnlockedLevelOverride=null;
     yield return Load(2);yield return Frames(20);
     COgheAudio.Heard+=heard;
     cam=new GameObject("Accessory reel camera").AddComponent<Camera>();cam.CopyFrom(game.Owner.View);cam.enabled=false;cam.orthographic=true;cam.aspect=540f/1170;cam.nearClipPlane=.01f;cam.farClipPlane=30;
     var level=game.Owner.View.transform.rotation;placed=false;
     acc=COgheAccessories.Attach(game);acc.Dress(clear?"":"HAT_BEANIE",clear?new[]{"FLOAT_FISH","FLOAT_STARS"}:new string[0]);
     var start=game.Motion.Centre(0);
     yield return Roll(1f,.2f,level);
     game.Motion.Move(0,start+new Vector3(.16f,0,.02f));yield return Roll(3.2f,.2f,level);
     var first=new bool[32];for(int i=0;i<20;i++)first[i]=true;game.Matter.Partition(game.Matter.Groups[0],first);
     yield return Roll(3f,.22f,level);
     File.WriteAllText($"{root}/sounds.txt",sounds.ToString());
    }
    finally{COgheAudio.Heard-=heard;COgheHomeRoom.UnlockedLevelOverride=null;if(cam!=null)Object.Destroy(cam.gameObject);RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
   }
  }
 }
}
