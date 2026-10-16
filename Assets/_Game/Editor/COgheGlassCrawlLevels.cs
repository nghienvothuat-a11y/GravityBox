using System.Linq;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
 /// <summary>
 /// Five levels where COghe may crawl on the glass, walls and roof, but the mechanisms still decide the way to the exit
 /// (Mrk 12/10/2026; designs in Docs/LevelDesign/COghe/GlassCrawl5). The glass grips; the slick tape is real slick
 /// glass (VenomSurfacePatch.SlipRegions), drawn by COgheSoftEdges where it is. The player taps where COghe should go
 /// (a shelf, the far room); the route runs up the glass and over the roof by itself. The exit is locked by the last mechanism
 /// (a slick shutter linked to it), except G3, where the roof plate is the only way across. Mechanisms stand on floors and
 /// shelves; walls and roof are the way between them.
 /// </summary>
 public static partial class VenomCampaignBuilder
 {
  // The slick tape at the foot of the glass. COghe stands about 5 cm and reaches about 6 cm up a slick face.
  const float GlassBand=.07f;
  // NextShell's panes: 0 roof, 1 front (z −.30), 2 back (z +.30), 3 left (x −.40), 4 right (x +.40).
  static VenomSurfacePatch Pane(ExpansionContext c,int i)=>c.Surfaces.First(s=>s.name=="Outer pane "+i);
  /// <summary>Slick glass on pane <paramref name="pane"/>, given in root coordinates: u along the pane (x on front, back
  /// and roof, z on left and right), v up the pane (y; z on the roof).</summary>
  static void Tape(ExpansionContext c,int pane,float u0,float u1,float v0,float v1)
  {
   Vector3 a,b;
   switch(pane)
   {
    case 0:a=new Vector3(u0,.10f,v0);b=new Vector3(u1,.10f,v1);break;
    case 1:a=new Vector3(u0,v0,-.30f);b=new Vector3(u1,v1,-.30f);break;
    case 2:a=new Vector3(u0,v0,.30f);b=new Vector3(u1,v1,.30f);break;
    case 3:a=new Vector3(-.40f,v0,u0);b=new Vector3(-.40f,v1,u1);break;
    default:a=new Vector3(.40f,v0,u0);b=new Vector3(.40f,v1,u1);break;
   }
   var p=Pane(c,pane);Vector3 la=p.transform.InverseTransformPoint(c.Root.TransformPoint(a)),lb=p.transform.InverseTransformPoint(c.Root.TransformPoint(b));
   var r=Rect.MinMaxRect(Mathf.Min(la.x,lb.x),Mathf.Min(la.y,lb.y),Mathf.Max(la.x,lb.x),Mathf.Max(la.y,lb.y));
   p.SlipRegions=(p.SlipRegions??new Rect[0]).Concat(new[]{r}).ToArray();
  }
  static void Band(ExpansionContext c,int pane,float u0,float u1)=>Tape(c,pane,u0,u1,-.30f,-.30f+GlassBand);
  /// <summary>A plate under the roof that a link slides along: only its underside grips (COghe hangs from it).</summary>
  static COgheRailSlider RoofPlate(ExpansionContext c,string name,Vector3 start,Vector3 axis,float travel,Vector3 size)
  {
   var rail=ExpansionRail(c,name,start,axis,travel,0,size,.02f,.004f,false,false);rail.CatchTolerance=.004f;rail.LatchAtEnd=true;
   rail.GetComponent<VenomMovableProp>().Manipulable=false;
   foreach(var f in rail.GetComponentsInChildren<VenomSurfacePatch>(true)){f.MotionFrame=rail.Body;f.Slippery=f.Normal.y>-.9f;}
   return rail;
  }
  /// <summary>A thin slick shutter inside the exit, the last mechanism's lock. It slides along the wall, back from the
  /// camera's line to the hole: raised, it stood between the view and the hole and caught the exit tap.</summary>
  static COgheRailSlider ExitShutter(ExpansionContext c,COgheRailSlider key,string name,Vector3? slide=null,float travel=.12f)
  {
   var inward=-c.Outward;var at=c.Exit+inward*.005f;bool side=Mathf.Abs(c.Outward.x)>.5f;
   var size=side?new Vector3(.006f,.10f,.10f):new Vector3(.10f,.10f,.006f);
   var shutter=PlusGate(c,name,at,slide??(side?Vector3.forward:Vector3.right),travel,size);
   // taps pass through it to the exit, from any view (a turned camera can have it between the eye and the hole)
   foreach(var f in shutter.GetComponentsInChildren<VenomSurfacePatch>(true))f.Selectable=false;
   ViewLink(c,key,shutter,true,ViewExitSurface(c));
   return shutter;
  }

  // G1 · Leo kính lấy chìa (Mrk's glass crawl, the lesson): the left wall has no tape, the way up to a shelf with handle
  // A; the exit at the foot of the right wall is shut until A. The other walls are taped at the foot.
  static void PlusG1(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.245f,-.10f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.05f,-.25f,-.15f);NextShell(c,.10f,false);
   Band(c,1,-.40f,.40f);Band(c,2,-.40f,.40f);Band(c,4,-.30f,.30f);   // the left wall, bare, is the way up
   // grippy all round (not a ledge with slick sides): COghe comes up the wall beside it and crawls over its edge
   int top=c.Surfaces.Count;ViewBlock(c,"Glass shelf",new Vector3(-.30f,-.13f,.14f),new Vector3(.20f,.02f,.28f));var shelf=c.Surfaces[top];
   var a=ViewTask(c,"A",new Vector3(-.27f,-.107f,.16f),Vector3.forward,.08f,shelf);
   ExitShutter(c,a.Rail,"A exit shutter");
  }

  // G2 · Kệ cao: a slick cover hides the gap in the left wall's tape; A (floor) slides it away. The high shelf with B is
  // boxed in slick glass below, so COghe comes down onto it from above. B opens the exit.
  static void PlusG2(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.245f,-.10f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.05f,-.25f,-.05f);NextShell(c,.10f,false);var floor=c.Surfaces[0];
   Band(c,1,-.40f,.40f);Band(c,2,-.40f,-.06f);Band(c,4,-.30f,.30f);Band(c,3,-.30f,-.26f);Band(c,3,-.12f,.30f);
   Tape(c,2,-.06f,.30f,-.30f,-.03f);Band(c,2,.30f,.40f);               // under the high shelf the back glass is slick
   var cover=PlusGate(c,"Lane cover",new Vector3(-.394f,-.265f,-.19f),Vector3.forward,.16f,new Vector3(.008f,.07f,.14f));
   var a=ViewTask(c,"A",new Vector3(-.26f,-.277f,-.17f),Vector3.right,.08f,floor);
   ViewLink(c,a.Rail,cover,false,null);
   var shelf=ChapterLedge(c,"High shelf",new Vector3(.12f,-.05f,.22f),new Vector3(.26f,.02f,.16f));
   var b=ViewTask(c,"B",new Vector3(.08f,-.027f,.26f),Vector3.right,.08f,shelf);
   ExitShutter(c,b.Rail,"B exit shutter");
  }

  // G3 · Qua trần nhà: a slick divider splits the box 10 cm under the roof; the glass and roof over it are slick. B (left
  // room) slides a plate that grips under the roof across the strip; the lift (locked by B) carries COghe above the tape.
  // Over the roof, down the right room's glass, out.
  static void PlusG3(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.245f,.10f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.25f,-.25f,-.12f);NextShell(c,.10f,false);var floor=c.Surfaces[0];
   Band(c,1,-.40f,.40f);Band(c,2,-.40f,.40f);Band(c,3,-.30f,.30f);Band(c,4,-.30f,.30f);
   Tape(c,2,-.34f,-.06f,-.30f,-.17f);                                  // behind the lift: out of reach from the lowered deck
   ChapterParapet(c,"Divider",new Vector3(0,-.15f,0),new Vector3(.02f,.30f,.60f));
   foreach(int pane in new[]{1,2})Tape(c,pane,-.09f,.09f,-.23f,.10f);   // the glass over the divider is slick: the way is the roof
   Tape(c,0,-.15f,.03f,-.30f,.30f);                                    // and a strip of the roof above it
   var plate=RoofPlate(c,"Roof plate",new Vector3(-.24f,.094f,0),Vector3.right,.18f,new Vector3(.18f,.006f,.56f));
   var b=ViewTask(c,"B",new Vector3(-.24f,-.277f,-.06f),Vector3.right,.08f,floor);   // clear of the walls: no step up to the glass
   ViewLink(c,b.Rail,plate,false,null);
   ChapterLift(c,new Vector3(-.20f,-.282f,.165f),.15f,b.Rail,"L");
  }

  // G4 · Ba kệ: A (floor) uncovers the gap in the left wall's tape; the back shelf's pad P latches a roof plate across the
  // slick strip that splits the box; the right shelf, slick glass below it, has B, which opens the exit.
  static void PlusG4(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.245f,-.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.10f,-.25f,-.10f);NextShell(c,.10f,false);var floor=c.Surfaces[0];
   Band(c,1,-.40f,.40f);Band(c,2,-.40f,.40f);Band(c,4,-.30f,.30f);Band(c,3,-.30f,-.26f);Band(c,3,-.12f,.30f);
   foreach(int pane in new[]{1,2})Tape(c,pane,.0f,.18f,-.23f,.10f);    // the glass and roof along x 0….18 are slick
   Tape(c,0,.0f,.18f,-.30f,.30f);
   Tape(c,4,-.30f,.30f,-.23f,-.05f);Tape(c,2,.18f,.40f,-.23f,-.05f);Tape(c,1,.18f,.40f,-.23f,-.05f);   // the right half: slick to the shelf
   var cover=PlusGate(c,"Lane cover",new Vector3(-.394f,-.265f,-.19f),Vector3.forward,.16f,new Vector3(.008f,.07f,.14f));
   var a=ViewTask(c,"A",new Vector3(-.26f,-.277f,-.17f),Vector3.right,.08f,floor);
   ViewLink(c,a.Rail,cover,false,null);
   var back=ChapterLedge(c,"Back shelf",new Vector3(-.22f,-.04f,.22f),new Vector3(.24f,.02f,.16f));
   var pad=ExpansionPad(c,"P",new Vector3(-.22f,-.028f,.22f),.009f,.10f);
   var plate=RoofPlate(c,"Roof plate",new Vector3(-.13f,.094f,0),Vector3.right,.22f,new Vector3(.20f,.006f,.56f));   // full depth: it meets the back glass
   PlusLatch(c,"P holds the roof plate out",new[]{pad},plate,false,true);
   var right=ChapterLedge(c,"Right shelf",new Vector3(.33f,-.03f,.12f),new Vector3(.14f,.02f,.24f));
   var b=ViewTask(c,"B",new Vector3(.33f,-.007f,.12f),Vector3.forward,.08f,right);
   ExitShutter(c,b.Rail,"B exit shutter",Vector3.back,.10f);   // toward the front corner: COghe comes down the right glass behind the exit
  }

  // G5 · Two-stage relay boss: Q halves COghe; P supports the first crossing. B + D release C,
  // then C releases A on the back glass. A secures the roof and opens the exit; both halves return and merge.
  static void PlusG5(ExpansionContext c)
  {
   // Keep the final mouth away from A's descent: opening it must not suck in the working half before reunion.
   c.Exit=new Vector3(.26f,-.245f,.30f);c.Outward=Vector3.forward;c.Spawn=new Vector3(-.16f,-.25f,-.255f);NextShell(c,.10f,false);   // in front of Q: its back is closed
   Band(c,1,-.40f,.40f);Band(c,2,-.40f,.40f);Band(c,4,-.30f,.30f);Band(c,3,-.30f,.10f);   // the left wall's back corner is bare
   foreach(int pane in new[]{1,2})Tape(c,pane,-.05f,.13f,-.23f,.10f);
   Tape(c,0,-.05f,.13f,-.30f,.30f);
   Tape(c,4,-.30f,.30f,-.23f,-.05f);Tape(c,2,.13f,.40f,-.23f,-.05f);Tape(c,1,.13f,.40f,-.23f,-.05f);
   NextQuantum(c,new Vector3(-.16f,-.30f,-.16f));
   var pad=ExpansionPad(c,"P",new Vector3(.20f,-.298f,-.16f),.009f,.10f);   // clear of Q's trays and of the climber's corner
   var plate=RoofPlate(c,"Roof plate",new Vector3(-.17f,.094f,0),Vector3.right,.21f,new Vector3(.20f,.006f,.56f));
   // Relay: the first half holds P, the second crosses and operates B, then takes the remote pad D.
   // D takes over the roof and, together with B, releases C on the starting floor. The first half can now leave P,
   // pull C and climb to A on the glass. A secures the roof and opens the final exit. Both halves must return and merge inside.
   var right=ChapterLedge(c,"Right shelf",new Vector3(.33f,-.03f,.05f),new Vector3(.14f,.02f,.50f));
   var b=ViewTask(c,"B",new Vector3(.33f,-.007f,.12f),Vector3.forward,.06f,right);
   var farPad=ExpansionPad(c,"D",new Vector3(.33f,-.018f,-.13f),.009f,.095f);
   var relayPin=PlusGate(c,"D relay pin",new Vector3(.11f,-.285f,.04f),Vector3.up,.035f,new Vector3(.018f,.025f,.018f));
   var relay=PlusLatch(c,"B and D release C",new[]{farPad},relayPin,false,false);relay.Rails=new[]{b.Rail};
   var relayControl=ViewTask(c,"C",new Vector3(.02f,-.277f,.04f),Vector3.right,.06f,c.Surfaces[0]);
   relayControl.RequiredRail=relayPin;relayControl.RequiredEnd=true;
   var a=ViewTask(c,"A",new Vector3(-.25f,.02f,.268f),Vector3.right,.06f,Pane(c,2));
   a.StandOffset=new Vector3(0,-.068f,0);a.TouchSize=new Vector3(.10f,.08f,.07f);
   var pin=PlusGate(c,"C upper pin",new Vector3(-.15f,.02f,.274f),Vector3.up,.035f,new Vector3(.018f,.025f,.018f));
   ViewLink(c,relayControl.Rail,pin,false,null);a.RequiredRail=pin;a.RequiredEnd=true;
   var hold=PlusLatch(c,"P or D or A holds the roof plate",new[]{pad,farPad},plate,true,false);hold.Rails=new[]{a.Rail};
   ExitShutter(c,a.Rail,"A exit shutter",Vector3.left); // park inward, clear of the right enclosure pane
   NextTrace("D",new Vector3(.33f,-.0192f,-.13f),new Vector3(.39f,-.0192f,-.13f),new Vector3(.399f,-.2992f,-.13f),new Vector3(.11f,-.2992f,.04f));
   PlusStep(1,c.Spawn);PlusStep(2,new Vector3(.20f,-.30f,-.16f));PlusStep(3,new Vector3(.33f,-.02f,-.04f));
   PlusStep(4,new Vector3(.33f,-.02f,-.13f));PlusStep(5,new Vector3(.02f,-.30f,.04f));PlusStep(6,new Vector3(-.25f,-.05f,.30f));

  }
 }
}
