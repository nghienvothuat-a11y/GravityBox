using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEngine;
namespace GravityBox.Editor
{
 /// <summary>
 /// Chapter 1 rebuilt for the hook plan (Mrk, 05/10/2026; PLANS/COGHE_LEVEL_HOOK_PLAN.md §5.1). Every level adds one step
 /// or one new kind of dependency over the one before, and every lock shows where it is opened from:
 /// 2 lavender cannot be climbed · 3 look around · 4 handle → door · 5 a floor lever lifts the glass cover off the wall
 /// handle · 6 climb to the wall handle that opens the box around the winch · 7 the bridge's stop pin is worked from the
 /// pit · 8 ride up, pull, press the popped-up button to ride down · 9 one bridge both blocks the lift and is the way ·
 /// 10 BOSS: two locks on the last winch, one hidden from the first view, both opened from the balcony the lift reaches.
 /// </summary>
 public static partial class VenomCampaignBuilder
 {
  const string ClearCover="Assets/_Game/Venom/Art/DayLab/Loose cover glass.mat";

  /// <summary>A raised block that can only be stood on: its sides are lavender (slick).</summary>
  static VenomSurfacePatch ChapterLedge(ExpansionContext c,string name,Vector3 centre,Vector3 size)
  {
   int first=c.Surfaces.Count;ViewBlock(c,name,centre,size);
   for(int i=first+1;i<c.Surfaces.Count;i++)c.Surfaces[i].Slippery=true;
   return c.Surfaces[first];
  }
  /// <summary>A low slick parapet: a slick riser over 5 cm cannot be climbed, so COghe does not drop off an edge.</summary>
  static void ChapterParapet(ExpansionContext c,string name,Vector3 centre,Vector3 size)
  {int first=c.Surfaces.Count;ViewBlock(c,name,centre,size);for(int i=first;i<c.Surfaces.Count;i++)c.Surfaces[i].Slippery=true;}

  /// <summary>A lift whose button stands at the deck's back-right corner; COghe boards in the far half, so it never
  /// covers the button and a tendril reaches over to press it.</summary>
  static COghePassengerLift ChapterLift(ExpansionContext c,Vector3 centre,float travel,COgheRailSlider required,string label)
  {
   var rail=ExpansionRail(c,"Passenger elevator",centre,Vector3.up,travel,0,new Vector3(.27f,.036f,.27f),.04f,.004f,true,false);rail.GetComponent<VenomMovableProp>().Manipulable=false;
   var lift=rail.gameObject.AddComponent<COghePassengerLift>();lift.Rail=rail;foreach(var face in rail.GetComponentsInChildren<VenomSurfacePatch>())face.MotionFrame=rail.Body;
   lift.RequiredRail=required;lift.DeckSize=new Vector2(.27f,.27f);lift.DeckHeight=.018f;lift.Deck=rail.GetComponentsInChildren<VenomSurfacePatch>().First(p=>p.Normal.y>.9f);
   // Back-right corner: seen from the start view both down below (a ledge beside the shaft would hide a left one)
   // and up at the landing (a front guard would hide a front one).
   lift.Panel=MechanismVisual(rail.transform,label+" lift panel",new Vector3(.09f,.024f,.09f),new Vector3(.05f,.013f,.05f),metal,PrimitiveType.Cylinder);
   lift.BoardPoint=new GameObject("Rider stance").transform;lift.BoardPoint.SetParent(rail.transform,false);lift.BoardPoint.localPosition=new Vector3(-.04f,.031f,-.04f);
   foreach(float dx in new[]{-.155f,.155f})MechanismVisual(c.Root,"Lift upright",centre+new Vector3(dx,travel*.5f,.095f),new Vector3(.014f,travel+.10f,.018f),metal);
   return lift;
  }

  /// <summary>A clear cover over a handle or winch: shows what is locked and slides away when its opener is pulled.</summary>
  static COgheRailSlider ChapterCover(ExpansionContext c,string name,Vector3 start,Vector3 size,float travel)
  {
   var cover=ViewGate(c,name,start,Vector3.up,travel,size);cover.GetComponent<VenomMovableProp>().Manipulable=false;
   // Clear glass to look and tap through: no walkable faces, and a tap passes to the handle it covers.
   foreach(var patch in cover.GetComponentsInChildren<VenomSurfacePatch>()){c.Surfaces.Remove(patch);UnityEngine.Object.DestroyImmediate(patch);}
   foreach(var shape in cover.GetComponentsInChildren<Collider>())shape.gameObject.layer=2;
   // A thin frame around the pane, moving with it, so the glass reads as a cover even when nearly clear.
   bool flat=size.y<size.x&&size.y<size.z;   // a lid lies flat, a cover stands
   Vector3 u=Vector3.right,v=flat?Vector3.forward:Vector3.up,w=flat?Vector3.up:Vector3.forward;
   float su=size.x,sv=flat?size.z:size.y,sw=(flat?size.y:size.z)+.002f,t=.004f;
   foreach(float side in new[]{-1f,1f})
   {
    MechanismVisual(cover.transform,"Cover frame",v*(sv*.5f*side),u*su+v*t+w*sw,metal);
    MechanismVisual(cover.transform,"Cover frame",u*(su*.5f*side),u*t+v*sv+w*sw,metal);
   }
   return cover;
  }

  /// <summary>Builds the chapter-1 content for level <paramref name="n"/>; levels 1 and 4 keep their original layout.</summary>
  static void ChapterOne(ExpansionContext c,int n,VenomSurfacePatch floor)
  {
   if(n==2)
   {
    // The faces toward the start are lavender: COghe climbs the first step from its ivory side.
    int first=c.Surfaces.Count;ViewBlock(c,"Ivory first step",new Vector3(-.12f,-.26f,.11f),new Vector3(.28f,.08f,.28f));c.Surfaces[first+5].Slippery=true;
    first=c.Surfaces.Count;ViewBlock(c,"Ivory second step",new Vector3(.20f,-.22f,.17f),new Vector3(.36f,.16f,.26f));c.Surfaces[first+5].Slippery=true;c.Surfaces[first+3].Slippery=true;
   }
   if(n==3)
   {
    // The wall now hides the whole exit ring: only looking around finds it.
    ViewBlock(c,"Observation wall",new Vector3(.14f,-.15f,.10f),new Vector3(.32f,.30f,.016f));ViewBlock(c,"Observation return",new Vector3(-.02f,-.15f,.16f),new Vector3(.016f,.30f,.12f));c.Definition.CameraEuler=new Vector3(38,0,0);
   }
   if(n==5)
   {
    var wall=c.Surfaces.Find(p=>p.name=="Outer pane 2");
    var a=ViewTask(c,"A",new Vector3(-.23f,-.04f,.268f),Vector3.right,.16f,wall);a.StandOffset=new Vector3(0,-.068f,0);a.TouchSize=new Vector3(.10f,.08f,.07f);
    var gate=ViewGate(c,"A blue shutter",c.Exit+Vector3.back*.020f,Vector3.left,.14f,new Vector3(.13f,.14f,.018f));ViewLink(c,a.Rail,gate,true,ViewExitSurface(c));
    // A clear cover in front of handle A; floor lever C lifts it. Until then A refuses, and its pin shows why.
    var cover=ChapterCover(c,"A glass cover",new Vector3(-.15f,-.035f,.222f),new Vector3(.23f,.11f,.008f),.13f);
    var lever=ViewTask(c,"C",new Vector3(-.05f,-.277f,-.13f),Vector3.right,.12f,floor);
    ViewLink(c,lever.Rail,cover,false,null);a.RequiredRail=cover;a.RequiredEnd=true;TapLock(c,a,cover.Start);
   }
   if(n==6)
   {
    var pulley=SpatialPulley(c,floor);var winch=pulley.Command;
    // An ivory service panel on the back wall above the departure plinth carries wall handle C; C lifts the clear lid
    // off winch A on the floor. Climb up first, then turn the winch.
    int first=c.Surfaces.Count;ViewBlock(c,"Service panel",new Vector3(-.25f,-.02f,.2975f),new Vector3(.26f,.16f,.005f));
    var panel=c.Surfaces[first+5];
    var handle=ViewTask(c,"C",new Vector3(-.33f,-.005f,.262f),Vector3.right,.08f,panel);handle.StandOffset=new Vector3(0,-.068f,0);handle.TouchSize=new Vector3(.10f,.08f,.07f);
    var lid=ChapterCover(c,"A glass lid",new Vector3(-.23f,-.222f,-.12f),new Vector3(.25f,.008f,.10f),.12f);
    foreach(var p in new[]{new Vector3(-.35f,-.262f,-.165f),new Vector3(-.11f,-.262f,-.165f),new Vector3(-.35f,-.262f,-.075f),new Vector3(-.11f,-.262f,-.075f)})
     MechanismVisual(c.Root,"Lid guide post",p,new Vector3(.008f,.076f,.008f),metal);
    ViewLink(c,handle.Rail,lid,false,null);winch.RequiredRail=lid;winch.RequiredEnd=true;TapLock(c,winch,lid.Start);
   }
   if(n==7)
   {
    var bridge=ViewBridge(c,floor);bridge.Label="B";
    // A stop pin holds the bridge; its lever D stands on a dry pad down in the pit, at the foot of the climb back up.
    var pin=ViewGate(c,"B stop pin",new Vector3(.145f,-.305f,-.17f),Vector3.right,.045f,new Vector3(.05f,.014f,.014f));
    pin.GetComponent<VenomMovableProp>().Manipulable=false;
    var basin=c.Surfaces.Find(p=>p.name=="Recovery basin");
    var pad=Panel(c.Root,"Pit lever pad",new Vector3(-.045f,-.3995f,.06f),Vector3.up,new Vector2(.12f,.16f),stone,false,Vector2.zero,0,c.Surfaces);
    var lever=ViewTask(c,"D",new Vector3(-.045f,-.377f,.06f),Vector3.forward,.07f,pad);lever.StandOffset=new Vector3(0,0,-.05f);
    ViewLink(c,lever.Rail,pin,false,null);bridge.RequiredRail=pin;bridge.RequiredEnd=true;TapLock(c,bridge,pin.Start);
   }
   if(n==8)
   {
    // Ride up, pull A on the upper ledge (it opens the exit down at floor level), press the popped-up button, ride down.
    var ledge=ChapterLedge(c,"Upper ledge",new Vector3(-.0045f,-.13f,.15f),new Vector3(.191f,.34f,.30f));
    ChapterParapet(c,"Ledge parapet",new Vector3(-.0045f,.07f,.004f),new Vector3(.191f,.06f,.008f));
    ChapterParapet(c,"Ledge parapet",new Vector3(-.096f,.07f,.154f),new Vector3(.008f,.06f,.292f));
    var lift=ChapterLift(c,new Vector3(.23f,-.278f,.15f),.30f,null,"A");
    // At the top the lift is fenced: the way down is the button, not a fall.
    ChapterParapet(c,"Lift guard",new Vector3(.23f,.0725f,.009f),new Vector3(.27f,.055f,.006f));
    ChapterParapet(c,"Lift guard",new Vector3(.371f,.0725f,.15f),new Vector3(.006f,.055f,.27f));
    var a=ViewTask(c,"A",new Vector3(.03f,.063f,.22f),Vector3.left,.08f,ledge);
    // The exit starts at the floor: the shutter sits 2 cm up, clear of the floor, and still covers the hole.
    var gate=ViewGate(c,"A blue shutter",c.Exit+Vector3.back*.020f+Vector3.up*.02f,Vector3.up,.14f,new Vector3(.13f,.13f,.018f));ViewLink(c,a.Rail,gate,true,ViewExitSurface(c));
   }
   if(n==9)
   {
    // One bridge does two jobs: parked over the lift shaft it stops the lift; slid across, it joins the lift's top to the
    // exit tower. Its stop pin is worked by floor lever C.
    var tower=ChapterLedge(c,"Exit tower",new Vector3(-.25f,-.13f,.17f),new Vector3(.30f,.34f,.26f));
    var bridge=ExpansionRail(c,"Sliding bridge",new Vector3(.23f,.022f,.15f),Vector3.left,.233f,0,new Vector3(.185f,.036f,.24f),.04f,.004f,false,false);
    bridge.LatchAtEnd=true;bridge.CatchTolerance=.002f;bridge.GetComponent<VenomMovableProp>().Manipulable=false;
    foreach(var face in bridge.GetComponentsInChildren<VenomSurfacePatch>())face.MotionFrame=bridge.Body;
    var lift=ChapterLift(c,new Vector3(.23f,-.278f,.15f),.30f,bridge,"A");
    var pin=ViewGate(c,"B stop pin",new Vector3(.345f,.022f,.15f),Vector3.up,.05f,new Vector3(.02f,.05f,.02f));pin.GetComponent<VenomMovableProp>().Manipulable=false;
    var b=ViewTask(c,"B",new Vector3(-.20f,-.277f,-.12f),Vector3.left,.12f,floor);
    var lever=ViewTask(c,"C",new Vector3(.02f,-.277f,-.20f),Vector3.right,.08f,floor);
    ViewLink(c,b.Rail,bridge,false,null);ViewLink(c,lever.Rail,pin,false,null);b.RequiredRail=pin;b.RequiredEnd=true;TapLock(c,b,pin.Start);
   }
   if(n==10)
   {
    // BOSS. A floating balcony at the back, reached only by the lift. The last winch A (it opens the exit under the
    // balcony) has two locks: a clear lid lifted by C, and a jam pin pulled by D. Both are on the balcony, and D is
    // hidden from the first view behind a screen: turn the view to find it. Up, unlock both, ride down, wind A, out.
    // 2 mm from the lift deck at its top stop: a wider seam caught COghe stepping across.
    // Deep enough (z .06–.30) that COghe stepping off the deck lands well inside, not along the front edge.
    var balcony=ChapterLedge(c,"Balcony",new Vector3(.1435f,.02f,.18f),new Vector3(.513f,.04f,.24f));
    // The parapet starts 2.3 cm in from the lift: too narrow to fall through, clear of COghe stepping off the deck.
    ChapterParapet(c,"Balcony parapet",new Vector3(.09f,.07f,.064f),new Vector3(.36f,.06f,.008f));
    ChapterParapet(c,"Balcony screen",new Vector3(.33f,.10f,.144f),new Vector3(.14f,.12f,.008f));
    var lift=ChapterLift(c,new Vector3(-.25f,-.278f,.15f),.30f,null,"B");
    ChapterParapet(c,"Lift guard",new Vector3(-.25f,.0725f,.009f),new Vector3(.27f,.055f,.006f));
    var lever=ViewTask(c,"C",new Vector3(.05f,.063f,.26f),Vector3.right,.06f,balcony);
    var hidden=ViewTask(c,"D",new Vector3(.30f,.063f,.27f),Vector3.right,.05f,balcony);
    var winch=ViewTask(c,"A",new Vector3(-.05f,-.277f,-.05f),Vector3.right,.12f,floor);
    var lid=ChapterCover(c,"A glass lid",new Vector3(.01f,-.222f,-.05f),new Vector3(.20f,.008f,.09f),.12f);
    var pin=ViewGate(c,"A jam pin",new Vector3(.125f,-.284f,-.05f),Vector3.up,.05f,new Vector3(.02f,.03f,.03f));pin.GetComponent<VenomMovableProp>().Manipulable=false;
    ViewLink(c,lever.Rail,lid,false,null);ViewLink(c,hidden.Rail,pin,false,null);
    winch.RequiredRail=lid;winch.RequiredEnd=true;winch.AlsoRequired=new[]{pin};TapLock(c,winch,lid.Start);
    var gate=ViewGate(c,"B coral shutter",c.Exit+Vector3.back*.020f+Vector3.up*.02f,Vector3.up,.14f,new Vector3(.13f,.13f,.018f));ViewLink(c,winch.Rail,gate,true,ViewExitSurface(c));
   }
  }

  /// <summary>After the art passes: covers read as clear glass; unlockers C and D are neutral, not circuit colours.</summary>
  static void ChapterOneFinish(VenomCampaign game,Material ivory)
  {
   var clear=AssetDatabase.LoadAssetAtPath<Material>(ClearCover);
   // Long linkage rods across the room hide more than they explain; the printed traces already show the links.
   var root=game.GetComponent<VenomLevelController>().Rotation.transform;
   foreach(var rod in root.GetComponentsInChildren<Transform>(true).Where(t=>(t.name=="Visible linkage housing"||t.name=="Lock linkage")&&t.localScale.z>.15f).ToArray())UnityEngine.Object.DestroyImmediate(rod.gameObject);
   var apparatus=game.GetComponent<VenomLevelController>().Apparatus;
   foreach(var rail in apparatus.GetComponentsInChildren<COgheRailSlider>())
    if(clear!=null&&(rail.name.Contains("glass cover")||rail.name.Contains("glass lid")))foreach(var r in rail.GetComponentsInChildren<Renderer>())if(r.name!="Cover frame")r.sharedMaterial=clear;
   foreach(var task in apparatus.GetComponentsInChildren<COgheTapRail>())
    if(task.Label=="C"||task.Label=="D")foreach(var r in task.Handle.GetComponentsInChildren<Renderer>())r.sharedMaterial=ivory;
  }
 }
}
