using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace GravityBox.Editor
{
 /// <summary>
 /// Chapter 1 rebuilt for the hook plan (Mrk, 05/10/2026; PLANS/COGHE_LEVEL_HOOK_PLAN.md §5.1). Every level adds one step
 /// or one new kind of dependency over the one before, and every lock shows where it is opened from:
 /// 2 lavender cannot be climbed · 3 look around · 4 handle → door · 5 a floor lever flips open the box over the wall
 /// handle · 6 climb to the wall handle that flips open the box over the winch · 7 the bridge's stop pin is worked from
 /// the pit · 8 ride up, pull, press the popped-up button to ride down · 9 one bridge both blocks the lift and is the way ·
 /// 10 BOSS: the box over the last winch also has a clip; both openers are on the balcony the lift reaches, one hidden.
 /// No letters: a control and what it works share a colour (Mrk, 05/10/2026).
 /// </summary>
 public static partial class VenomCampaignBuilder
 {
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
   // No uprights: dark posts beside the shaft read as clutter (Mrk, 05/10/2026).
   return lift;
  }

  /// <summary>The lock behind a box over a handle: a rail its opener drives through the linkage. It carries the state
  /// only; ChapterOneFinish hides it and builds the hinged box (COgheFlipCover) that shows it.</summary>
  static COgheRailSlider ChapterCover(ExpansionContext c,string name,Vector3 start,Vector3 size,float travel)
  {
   var cover=ViewGate(c,name,start,Vector3.up,travel,size);cover.GetComponent<VenomMovableProp>().Manipulable=false;
   // No walkable faces, and a tap passes through to the handle it covers.
   foreach(var patch in cover.GetComponentsInChildren<VenomSurfacePatch>()){c.Surfaces.Remove(patch);UnityEngine.Object.DestroyImmediate(patch);}
   foreach(var shape in cover.GetComponentsInChildren<Collider>())shape.gameObject.layer=2;
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
    // A box over handle A; floor lever C flips it open. Until then A refuses with a reason.
    var cover=ChapterCover(c,"A glass cover",new Vector3(-.15f,-.035f,.222f),new Vector3(.23f,.11f,.008f),.13f);
    var lever=ViewTask(c,"C",new Vector3(-.05f,-.277f,-.13f),Vector3.right,.12f,floor);
    ViewLink(c,lever.Rail,cover,false,null);a.RequiredRail=cover;a.RequiredEnd=true;
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
    ViewLink(c,handle.Rail,lid,false,null);winch.RequiredRail=lid;winch.RequiredEnd=true;
    // Start clear of the box over the winch: the way to the plinth passes beside it, not through it.
    c.Spawn=new Vector3(-.13f,-.25f,-.20f);
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
    ViewLink(c,lever.Rail,pin,false,null);bridge.RequiredRail=pin;bridge.RequiredEnd=true;
    // The pit can be fallen into anywhere and always climbed out of (Mrk's playtest, 09/10/2026: a first tap on the exit
    // left COghe stuck down there, and once it slid out of the box). The glass and the banks end at y −.30 but the pit
    // floor is 10 cm lower, so its front, back and right ends were open: slick skirts close them, and a slick face closes
    // the space under the receiving bank (it is never a way up). The receiving cliff only spans that bank, so the whole
    // pit is one floor, and the floor grips: from anywhere in it COghe crawls back to the climb up to the start bank.
    basin.Slippery=false;
    var cliff=c.Surfaces.Find(p=>p.name=="Receiving cliff");c.Surfaces.Remove(cliff);Object.DestroyImmediate(cliff.gameObject);
    ChapterParapet(c,"Receiving cliff",new Vector3(.109f,-.35f,.18f),new Vector3(.008f,.10f,.24f)); // wholly under the bank: the bridge slides 6 mm from x .105
    ChapterParapet(c,"Under receiving bank",new Vector3(.2525f,-.35f,.056f),new Vector3(.295f,.10f,.008f));
    ChapterParapet(c,"Pit skirt front",new Vector3(.1475f,-.35f,-.296f),new Vector3(.505f,.10f,.008f));
    ChapterParapet(c,"Pit skirt back",new Vector3(.1475f,-.35f,.296f),new Vector3(.505f,.10f,.008f));
    ChapterParapet(c,"Pit skirt right",new Vector3(.396f,-.35f,0),new Vector3(.008f,.10f,.60f));
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
    ViewLink(c,b.Rail,bridge,false,null);ViewLink(c,lever.Rail,pin,false,null);b.RequiredRail=pin;b.RequiredEnd=true;
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
    winch.RequiredRail=lid;winch.RequiredEnd=true;winch.AlsoRequired=new[]{pin};
    var gate=ViewGate(c,"B coral shutter",c.Exit+Vector3.back*.020f+Vector3.up*.02f,Vector3.up,.14f,new Vector3(.13f,.13f,.018f));ViewLink(c,winch.Rail,gate,true,ViewExitSurface(c));
   }
  }

  /// <summary>Colour key of a control in chapter 1: level 7's pin lever D wears C's amber, so through levels 5–9 amber
  /// always opens a lock; the boss adds a second one, green.</summary>
  internal static string ChapterOneColourKey(VenomCampaign game,string label)=>label=="D"&&(game.Definition.Id??"").EndsWith(".pilot.07")?"C":label;
  /// <summary>Locks whose trace ChapterOneFinish draws to the box or clip itself (levels 6 and 10).</summary>
  internal static bool ChapterOneOwnTrace(COgheRailSlider output)=>output.name=="A glass lid"||output.name=="A jam pin";

  /// <summary>After the art passes: dark parts removed, each lock shown as a hinged box in its opener's colour.</summary>
  static void ChapterOneFinish(VenomCampaign game)
  {
   // Long linkage rods across the room hide more than they explain; the printed traces already show the links.
   var root=game.GetComponent<VenomLevelController>().Rotation.transform;
   foreach(var rod in root.GetComponentsInChildren<Transform>(true).Where(t=>(t.name=="Visible linkage housing"||t.name=="Lock linkage")&&t.localScale.z>.15f).ToArray())UnityEngine.Object.DestroyImmediate(rod.gameObject);
   COgheDayLabBuilder.ChapterOneArt(game,int.Parse(game.Definition.Id.Substring(game.Definition.Id.Length-2)));
  }
  internal static readonly Color ChapterAmber=new Color(.89f,.66f,.17f),ChapterGreen=new Color(.40f,.66f,.29f);

 }

 public static partial class COgheDayLabBuilder
 {
  /// <summary>Locks drawn as a box (or a clip on one) instead of their rail; the rail itself is hidden.</summary>
  internal static bool ChapterOneHiddenLock(string name)=>name.StartsWith("A glass cover")||name.StartsWith("A glass lid")||name.StartsWith("A jam pin");

  /// <summary>
  /// Chapter 1 after every art pass (Mrk, 05/10/2026). A handle that another control unlocks sits under a five-sided box
  /// in that control's colour, hinged on one edge like a socket's flip cover; pulling the control swings it open.
  /// No dark parts: the lock rails behind the boxes are hidden, and the build logs anything still dark.
  /// </summary>
  internal static void ChapterOneArt(VenomCampaign game,int n)
  {
   var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
   var shell=Lit("Spatial pearl casing",new Color(.86f,.85f,.77f),.08f,.40f);
   Material Body(string name)=>AssetDatabase.LoadAssetAtPath<Material>(VenomCampaignBuilder.SpatialFolder+"/"+name+".mat");
   var amber=Body("Circuit C amber");var green=Body("Circuit D green");
   var traceC=Lit("Spatial printed C",new Color(.74f,.56f,.18f),.08f,.36f);var traceD=Lit("Spatial printed D",new Color(.36f,.58f,.27f),.08f,.36f);
   var amberBox=Tinted("Chapter box amber",VenomCampaignBuilder.ChapterAmber,.55f);
   var rails=owner.Apparatus.GetComponentsInChildren<COgheRailSlider>();
   COgheRailSlider Rail(string name)=>System.Array.Find(rails,x=>x.name==name);
   foreach(var rail in rails)
   {
    if(ChapterOneHiddenLock(rail.name))
    {
     foreach(var r in rail.GetComponentsInChildren<Renderer>(true))r.enabled=false;
     foreach(var shape in rail.GetComponentsInChildren<Collider>(true))shape.enabled=false;
    }
    // A stop pin wears the colour of the lever that pulls it.
    if(rail.name=="B stop pin")foreach(var r in rail.GetComponentsInChildren<MeshRenderer>())r.sharedMaterial=amber;
   }
   Remove(root,"Chapter one art");var art=Child(root,"Chapter one art");
   if(n==5)
   {
    // On the wall: hinged along its top edge, it swings up and out like a socket cover.
    ChapterBox(root,"Amber box over A",new Vector3(-.23f,-.008f,.292f),new Vector3(-.05f,-.064f,-.074f),new Vector3(.05f,0,.006f),Vector3.forward,Vector3.right,95,amberBox,amber,new[]{Rail("A glass cover")});
   }
   if(n==6)
   {
    // On the floor beside the left wall: hinged along its left edge, it tips over onto its side, away from the winch's pull.
    ChapterBox(root,"Amber box over A",new Vector3(-.350f,-.30f,-.128f),new Vector3(0,0,-.038f),new Vector3(.080f,.040f,.038f),Vector3.down,Vector3.forward,90,amberBox,amber,new[]{Rail("A glass lid")});
    // From the wall handle down the panel, across the plinth top, down its front and over the floor to the box.
    ChapterTrace(art,traceC,new[]{new Vector3(-.25f,-.005f,.2938f),new Vector3(-.225f,-.005f,.2938f),new Vector3(-.225f,-.0988f,.2938f),new Vector3(-.225f,-.0988f,.0388f),new Vector3(-.225f,-.2988f,.0388f),new Vector3(-.225f,-.2988f,-.128f),new Vector3(-.262f,-.2988f,-.128f)},
     new[]{Vector3.back,Vector3.back,Vector3.up,Vector3.back,Vector3.up,Vector3.up});
    SpatialPort(art,new Vector3(-.262f,-.2992f,-.128f),Vector3.up,amber,shell);
   }
   if(n==10)
   {
    // BOSS: the familiar amber box over winch A, and a green clip on it: amber C lifts the lid, green D (hidden on the
    // balcony) pulls the clip. The lid waits for both.
    var box=ChapterBox(root,"Amber box over A",new Vector3(-.05f,-.30f,-.006f),new Vector3(-.04f,0,-.092f),new Vector3(.04f,.040f,0),Vector3.down,Vector3.right,100,amberBox,amber,new[]{Rail("A glass lid"),Rail("A jam pin")});
    var clip=Slab(box.transform,"Green clip",new Vector3(0,.014f,-.092f),new Vector3(.018f,.026f,.012f),green);clip.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.On;
    var cover=box.GetComponent<COgheFlipCover>();cover.Clips=new[]{null,clip};cover.ClipTravel=new[]{Vector3.zero,Vector3.left*.054f};
    // Both traces leave the balcony down the back glass and cross the floor to the box (amber) and its clip (green).
    ChapterTrace(art,traceC,new[]{new Vector3(.11f,.0412f,.26f),new Vector3(.135f,.0412f,.26f),new Vector3(.135f,.0412f,.2975f),new Vector3(.135f,-.2988f,.2975f),new Vector3(.135f,-.2988f,-.112f),new Vector3(.002f,-.2988f,-.112f)},
     new[]{Vector3.up,Vector3.up,Vector3.back,Vector3.up,Vector3.up});
    SpatialPort(art,new Vector3(.002f,-.2992f,-.112f),Vector3.up,amber,shell);
    ChapterTrace(art,traceD,new[]{new Vector3(.35f,.0412f,.27f),new Vector3(.375f,.0412f,.27f),new Vector3(.375f,.0412f,.2975f),new Vector3(.375f,-.2988f,.2975f),new Vector3(.375f,-.2988f,-.126f),new Vector3(-.05f,-.2988f,-.126f),new Vector3(-.05f,-.2988f,-.104f)},
     new[]{Vector3.up,Vector3.up,Vector3.back,Vector3.up,Vector3.up,Vector3.up});
   }
   foreach(var r in art.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
   foreach(var r in game.GetComponentsInChildren<Renderer>())
   {
    if(!r.enabled||!r.gameObject.activeInHierarchy||r.sharedMaterial==null||!r.sharedMaterial.HasProperty("_BaseColor"))continue;
    var col=r.sharedMaterial.GetColor("_BaseColor");float lum=.3f*col.r+.59f*col.g+.11f*col.b,metallic=r.sharedMaterial.HasProperty("_Metallic")?r.sharedMaterial.GetFloat("_Metallic"):0;
    // Everything dark or metallic still showing, for review (circuit colours sit at .44–.49).
    if(lum<.5f||metallic>.4f)Debug.Log($"CHAPTER 1 AUDIT level {n}: {r.name} [{r.transform.parent?.name}] mat={r.sharedMaterial.name} lum={lum:0.00} metal={metallic:0.00} shader={r.sharedMaterial.shader.name}");
   }
  }

  /// <summary>A five-sided box (no face on <paramref name="open"/>) whose lid pivots about <paramref name="axis"/> through
  /// <paramref name="pivot"/>; <paramref name="min"/>/<paramref name="max"/> are its bounds relative to the pivot.</summary>
  static Transform ChapterBox(Transform root,string name,Vector3 pivot,Vector3 min,Vector3 max,Vector3 open,Vector3 axis,float angle,Material tint,Material trim,COgheRailSlider[] locks)
  {
   Remove(root,name);var holder=Child(root,name);holder.localPosition=pivot;var lid=Child(holder,"Lid");
   Vector3 c=(min+max)*.5f,s=max-min;const float wall=.0025f,edge=.0042f;
   foreach(var f in new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back})
   {
    if(f==open)continue;
    var size=new Vector3(Mathf.Abs(f.x)>.5f?wall:s.x,Mathf.Abs(f.y)>.5f?wall:s.y,Mathf.Abs(f.z)>.5f?wall:s.z);
    Slab(lid,"Box face",c+Vector3.Scale(f,s*.5f-Vector3.one*wall*.5f),size,tint);
   }
   // Solid coloured edges, except round the open side: they make it read as a box, not a pane.
   var axes=new[]{Vector3.right,Vector3.up,Vector3.forward};
   for(int a=0;a<3;a++)
   {
    Vector3 u=axes[(a+1)%3],v=axes[(a+2)%3];
    foreach(float su in new[]{-1f,1f})foreach(float sv in new[]{-1f,1f})
    {
     Vector3 side=u*su+v*sv;if(Vector3.Dot(side,open)>.5f)continue;
     Vector3 at=c+Vector3.Scale(u*su,s*.5f-Vector3.one*edge*.5f)+Vector3.Scale(v*sv,s*.5f-Vector3.one*edge*.5f);
     Slab(lid,"Box edge",at,axes[a]*Vector3.Dot(axes[a],s)+(u+v)*edge,trim);
    }
   }
   // The hinge barrel stays put on the mounting surface.
   float length=Mathf.Abs(Vector3.Dot(axis,s));
   var barrel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);barrel.name="Hinge barrel";barrel.transform.SetParent(holder,false);
   barrel.transform.localPosition=axis*Vector3.Dot(axis,c);
   barrel.transform.localRotation=Quaternion.FromToRotation(Vector3.up,axis);barrel.transform.localScale=new Vector3(.007f,length*.5f,.007f);
   Object.DestroyImmediate(barrel.GetComponent<Collider>());barrel.GetComponent<Renderer>().sharedMaterial=trim;
   var cover=holder.gameObject.AddComponent<COgheFlipCover>();cover.Lid=lid;cover.HingeAxis=axis;cover.OpenAngle=angle;cover.Locks=locks;
   return holder;
  }
  static Transform Slab(Transform parent,string name,Vector3 p,Vector3 size,Material mat)
  {
   var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=size;
   Object.DestroyImmediate(go.GetComponent<Collider>());var r=go.GetComponent<Renderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;return go.transform;
  }
  /// <summary>A printed trace with an explicit surface normal per segment (wall, panel, plinth front, floor).</summary>
  static void ChapterTrace(Transform parent,Material mat,Vector3[] points,Vector3[] normals)
  {
   for(int i=1;i<points.Length;i++)
   {
    var d=points[i]-points[i-1];
    var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Printed conductor";go.transform.SetParent(parent,false);
    go.transform.localPosition=(points[i]+points[i-1])*.5f;go.transform.localRotation=Quaternion.LookRotation(d,normals[i-1]);go.transform.localScale=new Vector3(.003f,.0005f,d.magnitude+.003f);
    Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;
   }
  }
  /// <summary>Tinted clear plastic (URP Lit, alpha blended).</summary>
  static Material Tinted(string name,Color color,float alpha)
  {
   var m=SaveAsset(name+".mat",()=>new Material(Shader.Find("Universal Render Pipeline/Lit")));m.shader=Shader.Find("Universal Render Pipeline/Lit");
   color.a=alpha;m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",0);m.SetFloat("_Smoothness",.62f);
   m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetFloat("_BlendModePreserveSpecular",0);m.SetFloat("_Cull",0);m.SetFloat("_ZWrite",0);
   m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_SrcBlendAlpha",(float)BlendMode.One);m.SetFloat("_DstBlendAlpha",(float)BlendMode.OneMinusSrcAlpha);
   m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.DisableKeyword("_ALPHAPREMULTIPLY_ON");m.SetOverrideTag("RenderType","Transparent");m.renderQueue=(int)RenderQueue.Transparent;
   EditorUtility.SetDirty(m);return m;
  }
 }
}
