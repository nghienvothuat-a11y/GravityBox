using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using GravityBox.Venom;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
 // Blockout pictures of the five glass-crawl designs (Mrk 12/10/2026: five levels where COghe may crawl on the glass but
 // still has to work the mechanisms to reach the exit). Explicit, presentation only: level 1's box with simple parts
 // placed for each design (tape, shelves, handles, pads, plates, lift, divider, Q, shutters). Writes
 // Artifacts/GlassCrawl/mock-G#.png and route-G#.txt (screen points of the planned route, for the annotated sheet).
 public sealed partial class COgheSpatialCampaignTests
 {
  private Transform mockRoot;private Material mTape,mSlick,mIvory,mBlue,mCoral,mPink,mMint,mRail;
  private Transform MockBox(string name,Vector3 c,Vector3 size,Material m)
  {
   var go=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.DestroyImmediate(go.GetComponent<Collider>());go.name=name;
   go.transform.SetParent(mockRoot,false);go.transform.localPosition=c;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=m;return go.transform;
  }
  private Transform MockCyl(string name,Vector3 c,float diameter,float height,Material m)
  {
   var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);Object.DestroyImmediate(go.GetComponent<Collider>());go.name=name;
   go.transform.SetParent(mockRoot,false);go.transform.localPosition=c;go.transform.localScale=new Vector3(diameter,height*.5f,diameter);go.GetComponent<Renderer>().sharedMaterial=m;return go.transform;
  }
  // tape on a face of the box: "L","R" (x=∓.40: a,b along z), "F","B" (z=∓.30: a,b along x), "T" roof (a along x, b along z)
  private void MockTape(string face,float a0,float a1,float b0,float b1)
  {
   const float t=.0008f,inset=.0015f;Vector3 c,s;
   switch(face)
   {
    case "L":c=new Vector3(-.40f+inset,(b0+b1)*.5f,(a0+a1)*.5f);s=new Vector3(t,b1-b0,a1-a0);break;
    case "R":c=new Vector3(.40f-inset,(b0+b1)*.5f,(a0+a1)*.5f);s=new Vector3(t,b1-b0,a1-a0);break;
    case "F":c=new Vector3((a0+a1)*.5f,(b0+b1)*.5f,-.30f+inset);s=new Vector3(a1-a0,b1-b0,t);break;
    case "B":c=new Vector3((a0+a1)*.5f,(b0+b1)*.5f,.30f-inset);s=new Vector3(a1-a0,b1-b0,t);break;
    default:c=new Vector3((a0+a1)*.5f,.10f-inset,(b0+b1)*.5f);s=new Vector3(a1-a0,t,b1-b0);break;
   }
   MockBox("Mock tape "+face,c,s,mTape);
  }
  private void MockBand(string face,float a0,float a1)=>MockTape(face,a0,a1,-.30f,-.23f);   // the 7 cm band at the foot
  private void MockRing(Vector3 centre,Vector3 normal,float radius=.039f)
  {
   var go=new GameObject("Mock exit",typeof(LineRenderer));go.transform.SetParent(mockRoot,false);var l=go.GetComponent<LineRenderer>();
   l.sharedMaterial=mMint;l.useWorldSpace=false;l.startWidth=l.endWidth=.0045f;l.positionCount=65;var rot=Quaternion.LookRotation(normal);
   for(int i=0;i<=64;i++){float a=i*Mathf.PI*2/64;l.SetPosition(i,centre+rot*new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));}
  }
  // a shutter over an exit on a wall: a slick plate with a band of its opener's colour
  private void MockShutter(Vector3 exit,Vector3 inward,Material opener)
  {
   var rot=Quaternion.LookRotation(inward);var p=MockBox("Mock shutter",exit+inward*.006f,Vector3.one,mSlick);p.localRotation=rot;p.localScale=new Vector3(.11f,.11f,.008f);
   var band=MockBox("Mock shutter band",exit+inward*.0105f+rot*new Vector3(0,.04f,0),Vector3.one,opener);band.localRotation=rot;band.localScale=new Vector3(.11f,.012f,.002f);
  }
  private void MockHandle(Vector3 from,Vector3 to,Material m)
  {
   var d=to-from;var rail=MockBox("Mock rail",(from+to)*.5f,Vector3.one,mRail);rail.localRotation=Quaternion.LookRotation(d);rail.localScale=new Vector3(.006f,.004f,d.magnitude);
   MockBox("Mock handle",from+Vector3.up*.012f,new Vector3(.026f,.022f,.026f),m);
  }
  private void MockPad(Vector3 at)=>MockCyl("Mock pad",at+Vector3.up*.004f,.07f,.008f,mPink);
  private void MockCOghe(Vector3 local)
  {
   var c=game.Root.InverseTransformPoint(game.Motion.Centre(0));var d=game.Root.TransformVector(local-c);
   foreach(var b in game.Matter.Bodies){b.position+=d;b.linearVelocity=Vector3.zero;b.transform.position+=d;}
  }

  [UnityTest,Explicit,Timeout(900000)] public IEnumerator GlassCrawlMocks()
  {
#if UNITY_EDITOR
   Material L(string p)=>UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(p);
   const string S="Assets/_Game/Venom/SpatialCampaign/";
   mTape=L("Assets/_Game/Venom/Art/SoftEdges/Slick tape.mat");mSlick=L(S+"Lavender slippery.mat");mIvory=L(S+"Ivory bodies.mat");
   mBlue=L(S+"Circuit A blue.mat");mCoral=L(S+"Circuit B coral.mat");mPink=L(S+"Circuit E pink.mat");mMint=L("Assets/_Game/Venom/Art/DayLab/Quiet mint light.mat");
   mRail=L(S+"Climb board rib.mat");
#endif
   Directory.CreateDirectory("Artifacts/GlassCrawl");
   foreach(var design in new[]{"G1","G2","G3","G4","G5"})
   {
    yield return LoadScene("COgheSpatial01");yield return Wait(.5f);
    // the level's own tape, exit ring and number plate go: each design draws its own
    foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
     if(r.name.StartsWith("COghe soft edges · tape")||r.name=="Fine rim"||r.transform.parent!=null&&(r.transform.parent.name=="Glass-mounted plaque"||r.transform.parent.name=="COghe specimen number"))r.enabled=false;
    mockRoot=new GameObject("Glass crawl mock").transform;mockRoot.SetParent(game.Root,false);
    var route=new List<(int step,Vector3 p)>();void R(int step,params Vector3[] ps){foreach(var p in ps)route.Add((step,p));}
    switch(design)
    {
     case "G1":   // Leo kính lấy chìa: the left wall has no tape; shelf with A; the exit on the right wall is shut until A
     {
      MockBand("F",-.40f,.40f);MockBand("B",-.40f,.40f);MockBand("R",-.30f,-.01f);MockBand("R",.11f,.30f);
      MockBox("Mock shelf",new Vector3(-.31f,-.13f,.19f),new Vector3(.18f,.02f,.20f),mIvory);
      MockHandle(new Vector3(-.31f,-.12f,.12f),new Vector3(-.31f,-.12f,.27f),mBlue);
      var exit=new Vector3(.40f,-.245f,.05f);MockRing(exit,Vector3.left);MockShutter(exit,Vector3.left,mBlue);
      MockCOghe(new Vector3(-.05f,-.27f,-.15f));
      R(1,new Vector3(-.05f,-.28f,-.15f),new Vector3(-.39f,-.27f,-.06f),new Vector3(-.395f,-.16f,.06f),new Vector3(-.31f,-.11f,.15f));
      R(2,new Vector3(-.31f,-.11f,.15f),new Vector3(-.31f,-.11f,.26f));
      R(3,new Vector3(-.31f,-.11f,.26f),new Vector3(-.395f,-.18f,.05f),new Vector3(-.36f,-.28f,-.02f),new Vector3(.10f,-.28f,.02f),new Vector3(.37f,-.26f,.05f));
      break;
     }
     case "G2":   // Kệ cao: A uncovers the gap in the left band; the shelf with P is boxed in tape below, reached from above
     {
      MockBand("F",-.40f,.40f);MockBand("R",-.30f,-.16f);MockBand("R",-.04f,.30f);MockBand("L",-.10f,.30f);MockBand("B",-.40f,-.06f);
      MockTape("B",-.06f,.30f,-.30f,-.03f);                                                    // under the shelf, the back glass is slick
      MockBox("Mock lane cover",new Vector3(-.3965f,-.265f,-.19f),new Vector3(.004f,.07f,.14f),mSlick);   // A slides it back along the wall
      MockHandle(new Vector3(-.30f,-.29f,-.24f),new Vector3(-.30f,-.29f,-.10f),mBlue);
      MockBox("Mock shelf",new Vector3(.12f,-.04f,.24f),new Vector3(.26f,.02f,.12f),mIvory);MockPad(new Vector3(.12f,-.03f,.24f));
      var exit=new Vector3(.40f,-.245f,-.10f);MockRing(exit,Vector3.left);MockShutter(exit,Vector3.left,mPink);
      MockCOghe(new Vector3(-.05f,-.27f,-.05f));
      R(1,new Vector3(-.05f,-.28f,-.05f),new Vector3(-.29f,-.27f,-.22f));
      R(2,new Vector3(-.29f,-.27f,-.22f),new Vector3(-.39f,-.25f,-.19f),new Vector3(-.395f,.0f,-.08f),new Vector3(-.25f,.05f,.295f),new Vector3(.10f,.04f,.295f),new Vector3(.12f,-.02f,.23f));
      R(3,new Vector3(.12f,-.02f,.23f),new Vector3(.30f,.03f,.295f),new Vector3(.395f,-.06f,.08f),new Vector3(.30f,-.28f,-.06f),new Vector3(.37f,-.26f,-.10f));
      break;
     }
     case "G3":   // Qua trần nhà: a slick divider; B slides a grippy plate over the roof tape; the lift (locked by B) lifts COghe past the band
     {
      foreach(var f in new[]{"F","B"}){MockBand(f,-.40f,.40f);MockTape(f,-.03f,.03f,.0f,.10f);}
      MockBand("L",-.30f,.30f);MockBand("R",-.30f,.03f);MockBand("R",.17f,.30f);
      MockBox("Mock divider",new Vector3(0,-.15f,0),new Vector3(.02f,.30f,.60f),mSlick);
      MockTape("T",-.15f,.02f,-.30f,.30f);
      MockBox("Mock roof plate",new Vector3(-.24f,.094f,0),new Vector3(.18f,.006f,.56f),mIvory);MockBox("Mock plate edge",new Vector3(-.15f,.094f,0),new Vector3(.006f,.008f,.56f),mCoral);
      MockHandle(new Vector3(-.30f,-.29f,-.20f),new Vector3(-.14f,-.29f,-.20f),mCoral);
      MockBox("Mock lift deck",new Vector3(-.20f,-.29f,.21f),new Vector3(.16f,.02f,.16f),mIvory);
      foreach(float x in new[]{-.29f,-.11f})MockBox("Mock lift guide",new Vector3(x,-.22f,.285f),new Vector3(.006f,.16f,.006f),mRail);
      MockBox("Mock lift button",new Vector3(-.13f,-.275f,.27f),new Vector3(.02f,.012f,.02f),mCoral);
      var exit=new Vector3(.40f,-.245f,.10f);MockRing(exit,Vector3.left);
      MockCOghe(new Vector3(-.25f,-.27f,-.12f));
      R(1,new Vector3(-.25f,-.28f,-.12f),new Vector3(-.28f,-.28f,-.20f));
      R(2,new Vector3(-.28f,-.28f,-.20f),new Vector3(-.20f,-.27f,.20f),new Vector3(-.20f,-.14f,.22f));
      R(3,new Vector3(-.20f,-.14f,.22f),new Vector3(-.20f,.0f,.295f),new Vector3(-.12f,.095f,.18f),new Vector3(.06f,.095f,.10f),new Vector3(.26f,.095f,.06f),new Vector3(.395f,-.05f,.08f),new Vector3(.30f,-.28f,.09f),new Vector3(.37f,-.26f,.10f));
      break;
     }
     case "G4":   // Ba kệ: A opens the gap in the left band; P on the back shelf keeps a roof plate over the roof tape (latched);
                  // B on the right shelf opens the exit
     {
      foreach(var f in new[]{"F","B"}){MockBand(f,-.40f,.40f);MockTape(f,.0f,.14f,-.23f,.10f);}
      MockBand("L",-.10f,.30f);MockBand("R",-.30f,-.22f);MockBand("R",-.08f,.30f);
      MockBox("Mock lane cover",new Vector3(-.3965f,-.265f,-.19f),new Vector3(.004f,.07f,.14f),mSlick);
      MockHandle(new Vector3(-.30f,-.29f,-.24f),new Vector3(-.30f,-.29f,-.10f),mBlue);
      MockTape("B",-.40f,.0f,-.23f,-.04f);
      MockBox("Mock back shelf",new Vector3(-.22f,-.03f,.24f),new Vector3(.22f,.02f,.12f),mIvory);MockPad(new Vector3(-.22f,-.02f,.24f));
      MockTape("T",.0f,.14f,-.30f,.30f);
      MockBox("Mock roof plate",new Vector3(-.09f,.094f,.05f),new Vector3(.16f,.006f,.20f),mIvory);MockBox("Mock plate edge",new Vector3(-.01f,.094f,.05f),new Vector3(.006f,.008f,.20f),mPink);
      MockTape("R",-.30f,.30f,-.23f,-.05f);
      MockBox("Mock right shelf",new Vector3(.33f,-.03f,.12f),new Vector3(.14f,.02f,.20f),mIvory);MockHandle(new Vector3(.33f,-.02f,.04f),new Vector3(.33f,-.02f,.19f),mCoral);
      var exit=new Vector3(.40f,-.245f,-.15f);MockRing(exit,Vector3.left);MockShutter(exit,Vector3.left,mCoral);
      MockCOghe(new Vector3(-.10f,-.27f,-.10f));
      R(1,new Vector3(-.10f,-.28f,-.10f),new Vector3(-.29f,-.27f,-.22f));
      R(2,new Vector3(-.29f,-.27f,-.22f),new Vector3(-.395f,-.10f,-.19f),new Vector3(-.395f,.02f,.10f),new Vector3(-.30f,.04f,.295f),new Vector3(-.22f,-.01f,.24f));
      R(3,new Vector3(-.22f,-.01f,.24f),new Vector3(-.20f,.06f,.295f),new Vector3(-.10f,.095f,.10f),new Vector3(.20f,.095f,.08f),new Vector3(.395f,.04f,.10f),new Vector3(.33f,-.01f,.10f));
      R(4,new Vector3(.33f,-.01f,.10f),new Vector3(.395f,-.12f,-.02f),new Vector3(.30f,-.28f,-.12f),new Vector3(.37f,-.26f,-.15f));
      break;
     }
     case "G5":   // Hai nửa trên trần (Boss): Q halves COghe; one half holds P (the roof plate stays out only while held), the other
                  // climbs the back-left lane, crosses the roof, pulls B on the right shelf (exit opens); then they merge and leave
     {
      foreach(var f in new[]{"F","B"}){MockBand(f,-.40f,.40f);MockTape(f,-.05f,.09f,-.23f,.10f);}
      MockBand("L",-.30f,.10f);MockBand("R",-.30f,.30f);
      MockTape("R",-.30f,.30f,-.23f,-.05f);MockTape("T",-.05f,.09f,-.30f,.30f);
      MockBox("Mock roof plate",new Vector3(-.13f,.094f,.12f),new Vector3(.16f,.006f,.20f),mIvory);MockBox("Mock plate edge",new Vector3(-.05f,.094f,.12f),new Vector3(.006f,.008f,.20f),mPink);
      MockBox("Mock Q",new Vector3(-.25f,-.255f,-.16f),new Vector3(.10f,.09f,.08f),mIvory);
      var q=new GameObject("Mock Q mark",typeof(TextMesh)).GetComponent<TextMesh>();q.transform.SetParent(mockRoot,false);q.transform.localPosition=new Vector3(-.25f,-.255f,-.2015f);
      q.text="Q";q.anchor=TextAnchor.MiddleCenter;q.characterSize=.012f;q.fontSize=60;q.color=new Color(.22f,.40f,.55f);
      MockPad(new Vector3(-.08f,-.30f,-.20f));
      MockBox("Mock right shelf",new Vector3(.33f,-.03f,.12f),new Vector3(.14f,.02f,.20f),mIvory);MockHandle(new Vector3(.33f,-.02f,.04f),new Vector3(.33f,-.02f,.19f),mCoral);
      var exit=new Vector3(-.40f,-.245f,-.15f);MockRing(exit,Vector3.right);MockShutter(exit,Vector3.right,mCoral);
      MockCOghe(new Vector3(-.12f,-.27f,.0f));
      R(1,new Vector3(-.12f,-.28f,.0f),new Vector3(-.25f,-.28f,-.10f));
      R(2,new Vector3(-.25f,-.28f,-.10f),new Vector3(-.08f,-.285f,-.20f));
      R(3,new Vector3(-.25f,-.28f,-.10f),new Vector3(-.38f,-.26f,.24f),new Vector3(-.395f,.02f,.22f),new Vector3(-.20f,.095f,.15f),new Vector3(.20f,.095f,.12f),new Vector3(.395f,.04f,.12f),new Vector3(.33f,-.01f,.10f));
      R(4,new Vector3(.33f,-.01f,.10f),new Vector3(.395f,-.12f,.0f),new Vector3(.20f,-.28f,-.12f),new Vector3(-.08f,-.28f,-.18f),new Vector3(-.37f,-.26f,-.15f));
      break;
     }
    }
    yield return Wait(.6f);
    var cam=Object.Instantiate(game.Owner.View);cam.name="Mock camera";cam.enabled=false;cam.orthographic=true;cam.orthographicSize=.37f;cam.aspect=1.5f;
    cam.transform.rotation=game.Root.rotation*Quaternion.Euler(24,-32,0);cam.transform.position=game.Root.TransformPoint(new Vector3(0,-.09f,0))-cam.transform.forward*3f;
    SoftShot(cam,$"Artifacts/GlassCrawl/mock-{design}.png",1500,1000);
    var sb=new System.Text.StringBuilder();
    foreach(var (step,p) in route){var v=cam.WorldToViewportPoint(game.Root.TransformPoint(p));sb.AppendLine($"{step} {v.x*1500:F1} {(1-v.y)*1000:F1}");}
    File.WriteAllText($"Artifacts/GlassCrawl/route-{design}.txt",sb.ToString());
    Object.Destroy(cam.gameObject);
   }
  }
 }
}
