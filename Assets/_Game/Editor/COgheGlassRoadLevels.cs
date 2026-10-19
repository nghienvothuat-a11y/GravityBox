using System.Collections.Generic;
using System.Linq;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
 /// <summary>
 /// Glass roads (Mrk 18/10/2026: "kính là bản đồ"): the glass itself is the level. Roads of bare glass run over the four walls
 /// and turn their corners; a lavender fence wider than COghe's reach lines every road, and a band runs along the foot of
 /// every wall, so a road is a clear lane between two stripes and nothing else on the glass can be reached. The rest of the
 /// glass stays clear (unreachable), the roof too: roads end 12 cm below it. Mechanisms open roads (a slick cover slides off
 /// a road's foot), stand on wall shelves or hang on the glass above a road, worked from the road beneath. Only the camera
 /// turns; the box stays still; nothing is on the roof (it cannot be tapped). Prototypes 66–68: easy G6, medium G7, hard G8.
 /// </summary>
 public static partial class VenomCampaignBuilder
 {
  // COghe is ~7.4 cm wide and a node grips glass 3.4 cm away: a fence or band this wide is out of its reach.
  const float RoadFence=.12f;

  /// <summary>Bare roads, given as in <see cref="Tape"/>: (pane, u0, u1, v0, v1) in root coordinates (u along the pane: x on
  /// front and back, z on left and right; v up the pane). Slick: a band along the foot of every wall (but where a road starts
  /// on the floor) and a fence round every road, carried round the corner onto the next wall; never over a road.</summary>
  static void GlassRoads(ExpansionContext c,params (int pane,float u0,float u1,float v0,float v1)[] roads)=>GlassRoads(c,.10f,roads);
  static void GlassRoads(ExpansionContext c,float roof,params (int pane,float u0,float u1,float v0,float v1)[] roads)
  {
   for(int pane=1;pane<=4;pane++)
   {
    Pane(c,pane).NoSlickClimb=true;   // fences are walls: route planning never tries to climb them
    float uMax=pane<=2?.40f:.30f;
    var slick=new List<Rect>{Rect.MinMaxRect(-uMax,-.30f,uMax,-.30f+RoadFence)};
    var bare=roads.Where(r=>r.pane==pane).Select(r=>Rect.MinMaxRect(r.u0,r.v0,r.u1,r.v1)).ToList();
    foreach(var r in bare)slick.Add(Rect.MinMaxRect(r.xMin-RoadFence,r.yMin-RoadFence,r.xMax+RoadFence,r.yMax+RoadFence));
    // a road that meets a corner: its fence goes on round the corner (unless a road carries on there)
    foreach(var r in roads.Where(r=>r.pane!=pane))
     foreach(float edge in new[]{r.u0,r.u1})
      if(Corner(r.pane,edge,out int next,out float at)&&next==pane)
       slick.Add(Rect.MinMaxRect(at<0?at:at-RoadFence,r.v0-RoadFence,at<0?at+RoadFence:at,r.v1+RoadFence));
    // a grid of every edge: a cell is slick when a slick rect covers it and no road does
    var us=new SortedSet<float>{-uMax,uMax};var vs=new SortedSet<float>{-.30f,roof};
    foreach(var r in slick.Concat(bare)){us.Add(Mathf.Clamp(r.xMin,-uMax,uMax));us.Add(Mathf.Clamp(r.xMax,-uMax,uMax));vs.Add(Mathf.Clamp(r.yMin,-.30f,roof));vs.Add(Mathf.Clamp(r.yMax,-.30f,roof));}
    var ul=us.ToList();var vl=vs.ToList();
    for(int i=0;i+1<ul.Count;i++)
    {
     float ua=ul[i],ub=ul[i+1];if(ub-ua<.001f)continue;float um=(ua+ub)*.5f;float? from=null;
     for(int j=0;j+1<vl.Count;j++)
     {
      float va=vl[j],vb=vl[j+1];float vm=(va+vb)*.5f;
      bool coat=vb-va>.0005f&&slick.Any(r=>r.Contains(new Vector2(um,vm)))&&!bare.Any(r=>r.Contains(new Vector2(um,vm)));
      if(coat&&from==null)from=va;
      if(!coat&&from!=null){Tape(c,pane,ua,ub,from.Value,va);from=null;}
     }
     if(from!=null)Tape(c,pane,ua,ub,from.Value,roof);
    }
   }
  }
  /// <summary>The pane on the other side of a vertical corner, and the corner's u there.</summary>
  static bool Corner(int pane,float u,out int next,out float at)
  {
   next=0;at=0;
   switch(pane)
   {
    case 1:if(u<=-.399f){next=3;at=-.30f;}else if(u>=.399f){next=4;at=-.30f;}break;
    case 2:if(u<=-.399f){next=3;at=.30f;}else if(u>=.399f){next=4;at=.30f;}break;
    case 3:if(u<=-.299f){next=1;at=-.40f;}else if(u>=.299f){next=2;at=-.40f;}break;
    case 4:if(u<=-.299f){next=1;at=.40f;}else if(u>=.299f){next=2;at=.40f;}break;
   }
   return next!=0;
  }
  /// <summary>A handle hung on the glass of a wall, just above a road, worked by COghe on the road beneath (as G5's A). Its
  /// carriage lies flat along the pane (5 mm off it), so the road passes under it.</summary>
  static COgheTapRail GlassHandle(ExpansionContext c,string key,Vector3 at,Vector3 axis,int pane)
  {
   bool side=pane>=3;const float travel=.06f;
   var rail=ExpansionRail(c,key+" carriage",at,axis,travel,0,side?new Vector3(.054f,.026f,.068f):new Vector3(.068f,.026f,.054f),.035f,.010f,false,true);
   rail.LatchAtStart=rail.LatchAtEnd=true;
   var prop=rail.GetComponent<VenomMovableProp>();prop.Manipulable=false;
   var task=rail.gameObject.AddComponent<COgheTapRail>();task.Rail=rail;task.Handle=prop.ManipulationGrip;task.WorkingSurface=Pane(c,pane);task.TouchControlBody=true;
   task.Label=key;task.PickHandleOnly=true;task.TrackStandPoint=true;task.StandOffset=new Vector3(0,-.068f,0);
   task.TouchSize=side?new Vector3(.07f,.08f,.10f):new Vector3(.10f,.08f,.07f);
   // slick all round: worked from the road beneath, never climbed (on top, COghe would reach the bare roof)
   foreach(var face in rail.GetComponentsInChildren<VenomSurfacePatch>(true)){face.Slippery=true;face.NoSlickClimb=true;}
   TapLabel(rail.transform,key,new Vector3(0,.038f,side?0:-.030f));
   foreach(float end in new[]{0f,travel})
    MechanismVisual(c.Root,key+" visible socket",at+axis*end+Vector3.down*.011f,side?new Vector3(.065f,.005f,.081f):new Vector3(.081f,.005f,.065f),metal);
   return task;
  }

  // G6 · Đường trên kính (66, easy): one clear road. Its foot at the back-right is under a slick cover; A on the floor slides
  // the cover away. The road climbs the back glass, turns the corner and runs along the right glass to the open exit.
  static void PlusG6(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.07f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.25f,-.25f,-.15f);NextShell(c,.10f,false);var floor=c.Surfaces[0];
   GlassRoads(c,(2,.26f,.40f,-.30f,-.02f),(4,.02f,.30f,-.14f,-.02f));
   var cover=PlusGate(c,"Road cover",new Vector3(.33f,-.21f,.294f),Vector3.left,.16f,new Vector3(.135f,.18f,.008f));   // clear of the right glass
   var a=ViewTask(c,"A",new Vector3(-.05f,-.277f,.05f),Vector3.right,.08f,floor);
   ViewLink(c,a.Rail,cover,false,null);
   PlusStep(1,new Vector3(-.05f,-.30f,.0f));PlusStep(2,new Vector3(.33f,-.15f,.30f));PlusStep(3,new Vector3(.40f,-.07f,.22f));
  }

  // G7 · Sau tấm chắn (67, medium): a slick screen stands before the back glass; the road starts behind it, under a cover that
  // A slides away (turn the camera to see it). Up the back glass, round the back-left corner, along the left glass to B, which
  // hangs on the glass and opens the exit; on round the front-left corner to the exit on the front glass.
  static void PlusG7(ExpansionContext c)
  {
   c.Exit=new Vector3(-.26f,-.07f,-.30f);c.Outward=Vector3.back;c.Spawn=new Vector3(.22f,-.25f,-.18f);NextShell(c,.10f,false);var floor=c.Surfaces[0];
   GlassRoads(c,(2,-.21f,-.07f,-.30f,-.06f),(2,-.40f,-.07f,-.12f,-.02f),(3,-.30f,.30f,-.12f,-.02f),(1,-.40f,-.12f,-.12f,-.02f));
   ChapterParapet(c,"Screen",new Vector3(-.08f,-.16f,.15f),new Vector3(.34f,.28f,.04f));
   var cover=PlusGate(c,"Road cover",new Vector3(-.14f,-.21f,.294f),Vector3.right,.16f,new Vector3(.16f,.18f,.008f));
   var a=ViewTask(c,"A",new Vector3(.25f,-.277f,.10f),Vector3.right,.08f,floor);
   ViewLink(c,a.Rail,cover,false,null);
   var b=GlassHandle(c,"B",new Vector3(-.368f,-.007f,.06f),Vector3.forward,3);
   ExitShutter(c,b.Rail,"B exit shutter");
   PlusStep(1,new Vector3(.25f,-.30f,.10f));PlusStep(2,new Vector3(-.14f,-.15f,.30f));PlusStep(3,new Vector3(-.40f,-.05f,.08f));PlusStep(4,c.Exit);
  }

  // G8 · Bốn mặt kính (68, hard): a road over all four walls, opened in three steps, with a dead end. A (floor) uncovers the
  // front road's foot; up and round the front-left corner, along the left glass onto a shelf with B, at the back corner; B
  // lowers a cover across the back road; along the back glass, round the back-right corner to C on the right glass (beside
  // a slick pillar), which opens the exit along the same road. The front road also runs right, toward the exit, and stops short.
  static void PlusG8(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.07f,-.18f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.12f);NextShell(c,.10f,false);var floor=c.Surfaces[0];
   GlassRoads(c,(1,-.30f,-.18f,-.30f,-.02f),(1,-.40f,.20f,-.12f,-.02f),   // the front: up, then left to the corner (and right to a dead end)
    (3,-.30f,.08f,-.12f,-.02f),                                          // the left: from the front corner to the shelf
    (2,-.40f,.40f,-.12f,-.02f),                                          // the back: corner to corner
    (4,-.24f,.30f,-.12f,-.02f));                                         // the right: from the back corner past C to the exit
   var front=PlusGate(c,"Front road cover",new Vector3(-.24f,-.21f,-.294f),Vector3.right,.16f,new Vector3(.14f,.18f,.008f));
   var a=ViewTask(c,"A",new Vector3(.10f,-.277f,.0f),Vector3.right,.08f,floor);
   ViewLink(c,a.Rail,front,false,null);
   // the shelf meets the back glass and runs along it: the back road starts at its back edge, at its height, and the way
   // onto it (the route finder links points 10 cm apart) is over the shelf, never over the drop beside it
   var shelf=ChapterLedge(c,"Left shelf",new Vector3(-.29f,-.13f,.19f),new Vector3(.22f,.02f,.22f));
   var b=ViewTask(c,"B",new Vector3(-.30f,-.107f,.19f),Vector3.forward,.05f,shelf);
   // a cover across the back road, away from the shelf: B lowers it under the road
   var gate=PlusGate(c,"Back road cover",new Vector3(-.02f,-.07f,.294f),Vector3.down,.12f,new Vector3(.16f,.14f,.008f));
   ViewLink(c,b.Rail,gate,false,null);
   ChapterParapet(c,"Pillar",new Vector3(.25f,-.16f,.12f),new Vector3(.06f,.28f,.10f));
   var cHandle=GlassHandle(c,"C",new Vector3(.368f,-.007f,.12f),Vector3.forward,4);
   ExitShutter(c,cHandle.Rail,"C exit shutter",Vector3.down,.12f);   // down, under the road: back or forward it would cover the road
   PlusStep(1,new Vector3(.10f,-.30f,.0f));PlusStep(2,new Vector3(-.30f,-.10f,.15f));PlusStep(3,new Vector3(.40f,-.05f,.12f));PlusStep(4,c.Exit);
  }

  // G9 · Hai nửa, bốn vách (69, very hard; Mrk 18/10/2026: "cực khó, các cơ quan kết hợp"): the roads of G8, with a gate across
  // the back road that only stands open while a pad is weighed: P1 on the floor or P2 on the right shelf. The exit is on the
  // right wall and needs the whole body, so COghe splits at Q: one half holds P1 while the other climbs the front and left
  // roads and crosses the back glass to P2; P2 takes over, and the first half climbs and crosses too, pulls C (hung on the
  // right glass, it opens the exit) and comes back; the halves merge on the right shelf and leave. A alone opens the way up
  // the front glass; the front road runs right to a dead end. (P1 is off the climbers' way: halves that pass close merge.)
  static void PlusG9(ExpansionContext c)
  {
   // the exit ~19 cm past C's stand: opened, its pull must not take the half that worked C before the halves merge
   c.Exit=new Vector3(.40f,-.07f,-.21f);c.Outward=Vector3.right;c.Spawn=new Vector3(.12f,-.25f,-.235f);NextShell(c,.10f,false);var floor=c.Surfaces[0];
   GlassRoads(c,(1,-.30f,-.18f,-.30f,-.02f),(1,-.40f,.22f,-.12f,-.02f),   // the front: up, left to the corner (right: a dead end)
    (3,-.30f,.02f,-.12f,-.02f),                                          // the left: from the front corner to the left shelf
    (2,-.40f,.40f,-.12f,-.02f),                                          // the back: corner to corner, a gate halfway
    (4,-.27f,.30f,-.12f,-.02f));                                         // the right: the right shelf, C and the exit
   NextQuantum(c,new Vector3(.12f,-.30f,-.12f));                       // its back is closed: start in front of it
   var front=PlusGate(c,"Front road cover",new Vector3(-.24f,-.21f,-.294f),Vector3.right,.16f,new Vector3(.14f,.18f,.008f));
   var a=ViewTask(c,"A",new Vector3(-.10f,-.277f,.06f),Vector3.right,.08f,floor);a.OneWay=true;
   ViewLink(c,a.Rail,front,false,null);
   // both shelves meet the back glass and run along it (the way onto the back road is over a shelf)
   var left=ChapterLedge(c,"Left shelf",new Vector3(-.29f,-.13f,.16f),new Vector3(.22f,.02f,.28f));
   var right=ChapterLedge(c,"Right shelf",new Vector3(.26f,-.13f,.20f),new Vector3(.28f,.02f,.20f));   // from just past the gate
   var p1=ExpansionPad(c,"P1",new Vector3(.28f,-.298f,.12f),.009f,.09f);   // on the floor, clear of Q's trays and the roads
   var p2=ExpansionPad(c,"P2",new Vector3(.34f,-.108f,.25f),.009f,.09f);   // the right shelf's back corner: the other half arrives clear of it
   var gate=PlusGate(c,"Back road gate",new Vector3(.03f,-.07f,.294f),Vector3.down,.12f,new Vector3(.16f,.14f,.008f));
   PlusLatch(c,"P1 or P2 holds the back road gate",new[]{p1,p2},gate,true,false);
   var cHandle=GlassHandle(c,"C",new Vector3(.368f,-.007f,.04f),Vector3.back,4);cHandle.OneWay=true;   // just before the right shelf
   ExitShutter(c,cHandle.Rail,"C exit shutter",Vector3.down,.12f);
   PlusStep(1,new Vector3(-.10f,-.30f,.06f));PlusStep(2,new Vector3(.12f,-.30f,-.12f));PlusStep(3,new Vector3(.28f,-.30f,.12f));
   PlusStep(4,new Vector3(.34f,-.12f,.25f));PlusStep(5,new Vector3(.40f,-.05f,.04f));PlusStep(6,c.Exit);
  }

  // G10 · Thang máy giữa hai đường (planned for 40; Mrk 19/10/2026: replace a repeated level): a low road and a high road on
  // different walls. Up the low road on the left glass to B, which hangs there and unlocks the lift in the back-right corner;
  // down again, ride the lift up: its deck stops at the foot of the high road, which runs along the back and right glass to
  // the exit. The high road is 14 cm above the lowered deck: out of reach until the lift rises.
  static void PlusG10(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.07f,-.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.10f,-.25f,-.15f);NextShell(c,.10f,false);
   // roads end 12 cm under the roof, so their fences reach it: from B's carriage nothing bare is in reach
   GlassRoads(c,(3,-.22f,-.10f,-.30f,-.02f),(3,-.22f,.20f,-.12f,-.02f),   // the low road: up the left glass, then along it
    (2,.115f,.40f,-.12f,-.02f),(4,-.20f,.30f,-.12f,-.02f));              // the high road: from the raised deck, back then right glass
   var b=GlassHandle(c,"B",new Vector3(-.368f,-.007f,.12f),Vector3.forward,3);
   ChapterLift(c,new Vector3(.25f,-.282f,.15f),.18f,b.Rail,"L");
   PlusStep(1,new Vector3(-.40f,-.08f,-.16f));PlusStep(2,new Vector3(-.40f,-.03f,.12f));PlusStep(3,new Vector3(.25f,-.26f,.15f));PlusStep(4,c.Exit);
  }

  // G11 · Bánh răng và dây đu (planned for 52; Mrk 19/10/2026: "giống 69 quá, bố trí lại đường đi, thêm bánh răng, đu dây").
  // A taller box (roof .30) for a rope swing. Three gears stand on the back glass with the middle one missing: A, hung on the
  // glass above the back road, slides gear G into the gap. On pad P the train turns and pushes the landing deck along the right
  // glass under the rope (without G only the motor gear turns). Up the left road onto the left shelf, grip ring B, tap the
  // deck: COghe swings across (a miss swings back to the hook). From the deck the right road climbs to the exit. A road up the
  // front glass and round onto the right glass is a dead end below the exit's height.
  static void PlusG11(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,.08f,.13f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.10f,-.25f,-.20f);NextShell(c,.30f,false);var floor=c.Surfaces[0];
   plusPlaqueRight=true;   // top-left, the camera's line through the plaque lands on the shelf where COghe grips the ring
   // the left: up from the floor beside the shelf, then along the glass at the shelf's height (as 69: a climb that ended
   // at the shelf's corner made COghe step off the glass over the air there, and fall)
   GlassRoads(c,.30f,(3,-.16f,-.04f,-.30f,-.04f),(3,-.04f,.18f,-.12f,-.04f),
    (2,.00f,.12f,-.30f,-.13f),                                              // the back: up to A
    (4,.08f,.18f,-.14f,.14f),                                               // the right: from the landing up to the exit
    (1,.26f,.40f,-.30f,.02f),(4,-.30f,-.22f,-.06f,.10f));                   // the front and round the corner: a dead end
   // the landing starts 8 cm short of the right road and 6 cm behind the front glass, 8 cm below the dead end: out of reach
   // the shelf and the landing end 12 cm before the back glass: from the back road they are out of reach
   var shelf=ChapterLedge(c,"Left shelf",new Vector3(-.28f,-.11f,.07f),new Vector3(.24f,.02f,.22f));
   var deck=ExpansionRail(c,"B landing deck",new Vector3(.255f,-.15f,-.08f),Vector3.forward,.18f,0,new Vector3(.28f,.02f,.16f),.05f,.01f,false,false);
   deck.GetComponent<VenomMovableProp>().Manipulable=false;deck.LatchAtEnd=true;TrimSideSlabs(deck);
   foreach(var f in deck.GetComponentsInChildren<VenomSurfacePatch>()){f.MotionFrame=deck.Body;if(f.Normal.y<.9f)f.Slippery=true;}
   var landing=deck.GetComponentsInChildren<VenomSurfacePatch>().First(f=>f.Normal.y>.9f);
   NextSwing(c,"B",new Vector3(0,.20f,.10f),.28f,40f,new Vector3(-.23f,-.08f,.10f),shelf,new[]{landing},new[]{new Vector3(.25f,-.12f,.10f)},floor);
   // the gears on the back glass (axles through the glass); G rides A's carriage into the gap between them
   var motor=MeshingStationWheel(c.Root,"P motor gear",new Vector3(.24f,-.22f,.29f),.04f,18,0);
   var output=MeshingStationWheel(c.Root,"Landing gear",new Vector3(.24f,-.06f,.29f),.04f,18,0);
   var a=GlassHandle(c,"A",new Vector3(.04f,-.12f,.268f),Vector3.right,2);a.OneWay=true;
   var gear=MeshingStationWheel(a.Rail.transform,"A carried gear G",new Vector3(.14f,-.02f,.022f),.04f,18,0);
   MechanismVisual(a.Rail.transform,"A gear arm",new Vector3(.07f,-.02f,.022f),new Vector3(.14f,.008f,.004f),metal);
   foreach(var w in new[]{motor,output})MechanismVisual(c.Root,w.name+" axle",w.localPosition+Vector3.back*.002f,new Vector3(.014f,.004f,.014f),metal,PrimitiveType.Cylinder).localRotation=Quaternion.Euler(90,0,0);
   MechanismVisual(c.Root,"Landing drive shaft",new Vector3(.32f,-.06f,.292f),new Vector3(.16f,.006f,.006f),metal);
   var p=ExpansionPad(c,"P",new Vector3(.02f,-.298f,-.04f),.009f,.10f);
   PlusTrain(c,"G completes the train",p,deck,false,null,motor,gear,output);
   NextTrace("P",new Vector3(.02f,-.2992f,.01f),new Vector3(.02f,-.2992f,.24f),new Vector3(.24f,-.2992f,.24f),new Vector3(.24f,-.2992f,.295f));
   PlusStep(1,new Vector3(.07f,-.19f,.29f));PlusStep(2,new Vector3(.02f,-.30f,-.04f));PlusStep(3,new Vector3(-.23f,-.10f,.10f));
   PlusStep(4,new Vector3(.25f,-.14f,.10f));PlusStep(5,c.Exit);   // the right road: z .08–.18 on the landing
   PlusGhost("G",new Vector3(.24f,-.14f,.29f));
  }
 }
}
