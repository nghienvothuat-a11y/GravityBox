using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // COghe's character: acts only change the skin, the tantrum holds the controls and hands them back, idle acts wait for
 // a quiet, whole body.
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator Frames(int frames,int ticksPerFrame=4){for(int f=0;f<frames;f++){for(int i=0;i<ticksPerFrame;i++)Tick();yield return null;}}

  private Vector3[] BodySnapshot(){var a=new Vector3[32];for(int i=0;i<32;i++)a[i]=game.Matter.Bodies[i].position;return a;}
  private Vector2 FloorTapNear(float dx)
  {
   var c=game.Motion.Centre(0);var world=c+game.Root.right*dx;
   return game.Owner.View.WorldToScreenPoint(world);
  }
  [UnityTest] public IEnumerator PersonalityActsNeverMoveTheBody()
  {
   yield return Load(1);yield return Frames(45);
   var p=game.Personality;p.ResetState();
   yield return Frames(120);var control=BodySnapshot();
   foreach(var act in new[]{COgheAct.Tantrum,COgheAct.Wave,COgheAct.Shape,COgheAct.Melt,COgheAct.Monster})
   {
    yield return Load(1);yield return Frames(45);game.Personality.ResetState();
    game.Personality.Force(act,COgheShape.Star);
    yield return Frames(120);var performed=BodySnapshot();
    for(int i=0;i<32;i++)Assert.AreEqual(control[i],performed[i],$"{act} moved particle {i}: acts are skin only");
   }
  }
  [UnityTest] public IEnumerator TantrumTakesTheControlsAndGivesThemBack()
  {
   yield return Load(1);yield return Frames(45);
   var p=game.Personality;p.ResetState();var rest=game.Motion.Centre(0);
   for(int i=0;i<6;i++){p.NoteTap();yield return Frames(3);}   // six real taps in about two seconds
   yield return Frames(2);
   Assert.AreEqual(COgheAct.Tantrum,p.Act,"A burst of taps makes it throw a fit");Assert.IsTrue(game.InputLocked,"No steering while it sulks");
   int commands=game.Feedback.CommandCount;
   game.TouchPoint(FloorTapNear(.12f));
   Assert.AreEqual(commands,game.Feedback.CommandCount,"Taps are ignored during the tantrum");
   yield return Frames(Mathf.CeilToInt(COghePersonality.TantrumLength*30)+3);
   Assert.AreEqual(COgheAct.None,p.Act);Assert.IsFalse(game.InputLocked,"Controls come back after the act");
   Assert.Less(Vector3.Distance(rest,game.Motion.Centre(0)),.003f,"It is back where it was");
   game.TouchPoint(FloorTapNear(.12f));
   Assert.AreEqual(commands+1,game.Feedback.CommandCount,"And it takes commands again");
   for(int i=0;i<6;i++){p.NoteTap();yield return Frames(1);}
   yield return Frames(2);
   Assert.AreNotEqual(COgheAct.Tantrum,p.Act,"One tantrum, then a cooldown");
  }
  [UnityTest] public IEnumerator IdleActsWaitForQuietThenStopForTheGame()
  {
   yield return Load(1);yield return Frames(45);
   var p=game.Personality;p.ResetState();
   yield return Frames(Mathf.CeilToInt((COghePersonality.IdleBeforeFirstAct-1)*30));
   Assert.AreEqual(COgheAct.None,p.Act,"Nothing before it has been left alone a while");
   yield return Frames(45);
   Assert.AreNotEqual(COgheAct.None,p.Act,"Left alone, it starts an act of its own (a wave when there is room)");Assert.AreNotEqual(COgheAct.Tantrum,p.Act);
   Assert.IsFalse(game.InputLocked,"Idle acts never hold the controls");
   game.TouchPoint(FloorTapNear(.12f));
   yield return Frames(8);
   Assert.AreEqual(COgheAct.None,p.Act,"A command ends the act at once");
  }

  // The monster (Mrk 02/10): it turns to show its profile with its tongue out, then looks at the camera with its head
  // tilted; its face comes with it and goes with it. Close stills for review: Artifacts/Monster.
  [UnityTest] public IEnumerator MonsterGrowsAFaceAndLosesIt()
  {
   yield return Load(1);yield return Frames(60);
   var p=game.Personality;var life=game.Matter.GetComponent<VenomLifeAnimation>();var toCamera=-game.Owner.View.transform.forward;
   p.Force(COgheAct.Monster);
   yield return Frames(Mathf.RoundToInt(3.3f*30));
   Assert.AreEqual(COgheAct.Monster,p.Act);
   var rig=life.Monster;Assert.Greater(rig.Amount,.9f,"Fully formed");Assert.Greater(rig.Jaw,.7f,"mouth open");Assert.Greater(rig.Tongue,.8f,"tongue out");
   Assert.Less(Mathf.Abs(Vector3.Dot(rig.Forward,toCamera)),.6f,"seen in profile");
   var face=game.Matter.GetComponent<COgheMonsterFace>();Assert.IsNotNull(face);
   foreach(var name in new[]{"Monster eyes","Monster mouth","Monster teeth","Monster tongue"})
   {var part=face.transform.Find(name);Assert.IsTrue(part.gameObject.activeSelf,name);Assert.Greater(part.GetComponent<MeshFilter>().sharedMesh.vertexCount,10,name);Assert.IsNull(part.GetComponent<Collider>(),"Presentation only");}
   Assert.Greater(rig.Head.y-game.Motion.Centre(0).y,.07f,"It stands far taller than COghe");
   yield return Frames(Mathf.RoundToInt((5.8f-3.3f)*30));
   rig=life.Monster;
   Assert.Greater(Vector3.Dot(rig.Forward,toCamera),.6f,"Then it looks at the camera");Assert.Greater(Mathf.Abs(Vector3.Dot(rig.Right,Vector3.up)),.2f,"its head tilted");
   yield return Frames(Mathf.RoundToInt((COghePersonality.MonsterLength-5.8f)*30)+6);
   Assert.AreNotEqual(COgheAct.Monster,p.Act);Assert.AreEqual(0,life.Monster.Amount);
   Assert.IsFalse(face.transform.Find("Monster eyes").gameObject.activeSelf,"The face goes with it");
  }
  [Explicit("Renders monster review stills")]
  [UnityTest] public IEnumerator RenderMonsterStills()
  {
   yield return Load(1);yield return Frames(60);
   var p=game.Personality;var view=game.Owner.View;
   var close=new GameObject("Monster camera").AddComponent<Camera>();close.CopyFrom(view);close.enabled=false;close.aspect=540f/720;close.orthographic=true;
   var rt=RenderTexture.GetTemporary(540,720,24);var tex=new Texture2D(540,720,TextureFormat.RGB24,false);
   string root="Artifacts/Monster";if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
   try
   {
    p.Force(COgheAct.Monster);var rest=game.Motion.Centre(0);
    int frames=Mathf.CeilToInt(COghePersonality.MonsterLength*30);
    for(int f=0;f<=frames;f++)
    {
     yield return Frames(1);
     if(f%3!=0)continue;
     close.transform.rotation=view.transform.rotation;close.orthographicSize=.11f;   // the act faces the game camera
     close.transform.position=rest+Vector3.up*.065f-close.transform.forward*1.2f;
     Grab(close,rt,tex,$"{root}/close_{f:000}.png");
     if(f==99||f==170)
     {
      // from the side: the jaw and the face parts in profile
      var rig=game.Matter.GetComponent<VenomLifeAnimation>().Monster;
      close.transform.rotation=Quaternion.LookRotation(-rig.Right,Vector3.up);close.transform.position=rest+Vector3.up*.065f-close.transform.forward*1.2f;
      Grab(close,rt,tex,$"{root}/side_{f:000}.png");
      File.WriteAllText($"{root}/rig_{f:000}.txt",$"jaw {rig.Jaw} size {rig.Size} head {rig.Head} jaw head {rig.JawHead} fwd {rig.Forward} up {rig.Up}");
     }
    }
   }
   finally{Object.Destroy(close.gameObject);RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
  }

  // Preview frames of every act: Artifacts/Personality/<act>/frame_<n>.png (close camera) and wide_<n>.png.
  [Explicit("Renders personality act previews")]
  [UnityTest] public IEnumerator RenderPersonalityActs()
  {
   yield return Load(1);yield return Frames(60);
   var p=game.Personality;Assert.IsNotNull(p,"Spatial levels have a character");
   var view=game.Owner.View;
   var close=new GameObject("Preview camera").AddComponent<Camera>();close.CopyFrom(view);close.enabled=false;close.aspect=540f/720;
   var rt=RenderTexture.GetTemporary(540,720,24);var tex=new Texture2D(540,720,TextureFormat.RGB24,false);
   var wideRt=RenderTexture.GetTemporary(540,960,24);var wideTex=new Texture2D(540,960,TextureFormat.RGB24,false);
   string root="Artifacts/Personality";if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
   var log=new System.Text.StringBuilder();int reelFrame=0;
   try
   {
    var acts=new System.Collections.Generic.List<(COgheAct act,COgheShape shape,string name,float seconds)>{(COgheAct.Wave,0,"wave",2.9f)};
    foreach(COgheShape s in System.Enum.GetValues(typeof(COgheShape)))acts.Add((COgheAct.Shape,s,"shape_"+s,2.9f));
    acts.Add((COgheAct.Melt,0,"melt",2.3f));acts.Add((COgheAct.GlassTap,0,"glass_tap",2.1f));acts.Add((COgheAct.Doze,0,"doze",6f));acts.Add((COgheAct.Tantrum,0,"tantrum",3.1f));
    foreach(var a in acts)
    {
     string dir=$"{root}/{a.name}";Directory.CreateDirectory(dir);
     p.Force(a.act,a.shape);
     int frames=Mathf.CeilToInt(a.seconds*30);
     log.AppendLine($"{a.name} {a.act} {reelFrame} {p.HasWall}");
     for(int f=0;f<=frames;f++)
     {
      yield return Frames(1);
      Vector3 c=game.Motion.Centre(0),fwd=view.transform.forward;
      close.transform.rotation=view.transform.rotation;close.transform.position=c-fwd*Vector3.Dot(c-view.transform.position,fwd)+view.transform.up*.035f;
      close.orthographicSize=a.act==COgheAct.Tantrum?.16f:.095f;if(a.act==COgheAct.Tantrum)close.transform.position+=view.transform.up*.04f;
      Grab(close,rt,tex,$"{dir}/frame_{f:000}.png");reelFrame++;
      if(f%6==0)Grab(view,wideRt,wideTex,$"{dir}/wide_{f:000}.png");
     }
     p.Force(COgheAct.None);yield return Frames(20);
    }
    File.WriteAllText($"{root}/acts.txt",log.ToString());
   }
   finally{Object.Destroy(close.gameObject);RenderTexture.ReleaseTemporary(rt);RenderTexture.ReleaseTemporary(wideRt);Object.Destroy(tex);Object.Destroy(wideTex);}
  }
  private static void Grab(Camera cam,RenderTexture rt,Texture2D tex,string path)
  {
   var old=cam.targetTexture;cam.targetTexture=rt;cam.Render();cam.targetTexture=old;
   RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();RenderTexture.active=null;
   File.WriteAllBytes(path,tex.EncodeToPNG());
  }
 }
}
