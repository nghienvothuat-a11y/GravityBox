using System.Linq;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
 /// <summary>
 /// Physics props (Mrk 19/10/2026: "những ý tưởng về vật lý này phải kết hợp được những cơ quan đã có. mày nhặt 3 levels đã
 /// có rồi biến đổi, thêm những yếu tố vật lý ... tạo ra 3 màn. 66, 67, 68"). Three existing levels, each reworked around one
 /// physical object: F1 = N18 with a steel ball that holds the door pad; F2 = N41 whose gears release the ball, which knocks a
 /// pillar flat into a step; F3 = N25 whose counterweight tray lies under thin glass that only the ball and COghe together
 /// break. The ball is a loose prop (gravity and Retry as every prop); it rolls only in slick chutes, so it lands the same way.
 /// </summary>
 public static partial class VenomCampaignBuilder
 {
  const float BallRadius=.025f,BallMass=.05f;

  /// <summary>A steel ball: a loose sphere (not a surface: COghe never stands on it), damped so it settles where it stops.</summary>
  static VenomMovableProp SteelBall(ExpansionContext c,string name,Vector3 centre)
  {
   var go=new GameObject(name,typeof(Rigidbody),typeof(SphereCollider),typeof(VenomMovableProp));go.transform.SetParent(c.Root,false);go.transform.localPosition=centre;
   var rb=go.GetComponent<Rigidbody>();rb.mass=BallMass;rb.useGravity=false;rb.interpolation=RigidbodyInterpolation.None;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
   rb.solverIterations=20;rb.solverVelocityIterations=8;rb.linearDamping=.3f;rb.angularDamping=1.5f;rb.maxDepenetrationVelocity=.2f;
   go.GetComponent<SphereCollider>().radius=BallRadius;
   var prop=go.GetComponent<VenomMovableProp>();prop.Body=rb;prop.Manipulable=false;
   MechanismVisual(go.transform,name+" steel",Vector3.zero,Vector3.one*BallRadius*2,metal,PrimitiveType.Sphere);
   c.Props.Add(prop);return prop;
  }

  /// <summary>A slick chute running straight from <paramref name="high"/> down to <paramref name="low"/> (both on its bed's
  /// centre line, axis-aligned): bed, closed high end and side walls to the floor that stand <paramref name="lip"/> above the
  /// bed's top. A side on the glass is left out (<paramref name="openSide"/>: +1 right of the run, -1 left, 0 none). A stop
  /// across the bed at <paramref name="stopAt"/> (metres down the run from the high end) holds the ball, which is returned.
  /// The walls stand on <paramref name="baseY"/> (the floor, or a deck).</summary>
  static VenomMovableProp BallChute(ExpansionContext c,string name,Vector3 high,Vector3 low,float width,float lip,int openSide,float stopAt,out COgheRailSlider stop,float baseY=-.30f)
  {
   Vector3 run=Vector3.ProjectOnPlane(low-high,Vector3.up);float length=run.magnitude;run/=length;Vector3 side=Vector3.Cross(Vector3.up,run);
   float drop=high.y-low.y,slope=Mathf.Atan2(drop,length);
   var bed=NextRamp(c,name+" bed",high,low,width);bed.Slippery=true;
   float wallTop=high.y+lip,wallHeight=wallTop-baseY;
   foreach(int s in new[]{-1,1})
   {
    if(s==openSide)continue;
    Vector3 at=(high+low)*.5f+side*(s*width*.5f);at.y=(wallTop+baseY)*.5f;
    // faces inward: a panel's 8 mm solid lies behind its face, outside the chute (facing out, it narrowed the bed to 4.4 cm
    // and wedged the 5 cm ball and the stop)
    Panel(c.Root,name+" wall",at,-side*s,new Vector2(length,wallHeight),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   }
   Vector3 end=high-run*.002f;end.y=(wallTop+baseY)*.5f;
   Panel(c.Root,name+" end",end,run,new Vector2(width,wallHeight),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   // the stop: a slick gate across the bed that lifts clear of the ball
   Vector3 onBed=high+run*stopAt+Vector3.down*(drop*stopAt/length);
   // lifts 10 cm: on a steep bed the ball's top uphill of the stop stands higher than at it (6 cm caught it on 67's 32°)
   stop=PlusGate(c,name+" stop",onBed+Vector3.up*.018f,Vector3.up,.10f,Mathf.Abs(run.x)>.5f?new Vector3(.008f,.03f,width-.006f):new Vector3(width-.006f,.03f,.008f));
   // the ball rests on the bed just uphill of the stop, 2 mm clear of it
   float back=stopAt-.004f-BallRadius-.002f;
   Vector3 rest=high+run*back+Vector3.down*(drop*back/length)+Vector3.up*(BallRadius/Mathf.Cos(slope)+.001f);
   return SteelBall(c,"Steel ball",rest);
  }

  // F1 · Bi giữ cửa (66, from N18 "Nửa thân không đủ sức"): no Q. Pull D: the stop lifts, the steel ball rolls down the
  // chute along the left glass and comes to rest on pad A against the front glass; A holds the spring door up for as long
  // as the ball lies there. The whole body goes through, pushes the 100% crate C to the shelf (it needs all of COghe: a
  // split would not), climbs it and leaves. COghe on A opens the door too, but it shuts as soon as COghe steps off.
  static void PlusF1(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.163f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   var door=PlusGate(c,"A door",new Vector3(.05f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
   var a=ExpansionPad(c,"A",new Vector3(-.35f,-.298f,-.235f),.009f,.10f);
   var ball=BallChute(c,"Ball chute",new Vector3(-.37f,-.19f,.16f),new Vector3(-.37f,-.30f,-.10f),.06f,.02f,1,.06f,out var stop);   // the left glass is its outer wall
   a.Weights=new[]{ball.Body};
   var d=ViewTask(c,"D",new Vector3(-.12f,-.277f,.16f),Vector3.right,.05f,floor);d.OneWay=true;
   ViewLink(c,d.Rail,stop,false,null);
   var safe=new GameObject("Door safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(.05f,-.25f,-.20f);safe.Size=new Vector3(.06f,.10f,.12f);
   var hold=new GameObject("A holds the door",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a};hold.Output=door;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   NextPlinth(c,"Exit shelf",new Vector3(.31f,-.255f,.20f),new Vector3(.18f,.09f,.20f));
   var crate=NextCrate(c,"C",new Vector3(.31f,-.27f,-.12f),Vector3.forward,.156f,new Vector3(.14f,.06f,.12f),floor,new Vector3(0,0,-.068f),new Vector3(0,0,-.052f),.08f,.45f);
   crate.CompensateLoad=true;crate.LoadShare=1;
   TapLabel(c.Root,"100%",new Vector3(.31f,-.2985f,-.215f));
   NextOutline(c,"C shelf step outline",new Vector3(.31f,-.2995f,.036f),new Vector2(.145f,.125f));
   NextTrace("A",new Vector3(-.30f,-.2992f,-.235f),new Vector3(-.05f,-.2992f,-.27f),new Vector3(.03f,-.2992f,-.27f));
   NextTrace("D",new Vector3(-.17f,-.2992f,.16f),new Vector3(-.33f,-.2992f,.16f));
   PlusStep(1,new Vector3(-.12f,-.28f,.11f));PlusStep(2,new Vector3(-.35f,-.29f,-.235f));PlusStep(3,new Vector3(.16f,-.28f,-.20f));PlusStep(4,new Vector3(.31f,-.28f,-.25f));PlusStep(5,new Vector3(.31f,-.24f,.04f));
  }

  // F2 · Bánh răng thả bi (67, from N41 "Bánh răng đầu tiên"): the train's output is the stop holding the steel ball at the top
  // of a chute. Pull A (gear G into the gap), stand on P: the gears lift the stop, the ball rolls down and knocks the pillar
  // standing before the exit platform flat. Lying down it is a 3 cm step to the 6 cm slick platform (3 cm risers climb,
  // 6 cm does not), and COghe walks up to the exit. Pushing the pillar does nothing; only the ball knocks it over.
  static void PlusF2(ExpansionContext c)
  {
   c.Exit=new Vector3(.20f,-.192f,.30f);c.Spawn=new Vector3(-.28f,-.25f,-.22f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit platform",new Vector3(.235f,-.27f,.21f),new Vector3(.33f,.06f,.18f));
   float y=PlusGearTable(c,"Gear table",new Vector3(-.12f,0,.08f),new Vector2(.30f,.10f));
   var pad=ExpansionPad(c,"P",new Vector3(-.32f,-.298f,-.16f),.009f,.10f);
   var g0=PlusGear(c.Root,"Motor gear",new Vector3(-.22f,y,.08f));var g2=PlusGear(c.Root,"Output gear",new Vector3(-.06f,y,.08f));
   PlusGearCarriage(c,"A",new Vector3(-.14f,-.277f,-.20f),Vector3.forward,.19f,floor,y,.09f,out var gear);
   var ball=BallChute(c,"Ball chute",new Vector3(.20f,-.20f,-.298f),new Vector3(.20f,-.30f,-.14f),.06f,.02f,0,.06f,out var stop);   // flush with the front glass
   var train=PlusTrain(c,"G lifts the ball stop",pad,stop,false,null,g0,gear,g2);
   PlusSlotRing(c,"G slot ring",new Vector3(-.14f,y-.0075f,.08f),.047f);
   PlusMeshLamp(c,train,new Vector3(.005f,y,.05f));
   // the pillar: 22 cm tall, foot hinged on its back edge 10 cm before the platform; it falls back, flat against the platform
   var pillar=new GameObject("Pillar",typeof(Rigidbody)).GetComponent<Rigidbody>();pillar.transform.SetParent(c.Root,false);pillar.transform.localPosition=new Vector3(.20f,-.19f,-.115f);
   pillar.isKinematic=true;pillar.useGravity=false;
   var faces=new System.Collections.Generic.List<VenomSurfacePatch>();Vector3 size=new Vector3(.10f,.22f,.03f);
   foreach(var n in new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back})
   {
    float half=Mathf.Abs(n.x)*size.x+Mathf.Abs(n.y)*size.y+Mathf.Abs(n.z)*size.z;
    Vector2 s=Mathf.Abs(n.y)>.9f?new Vector2(size.x,size.z):Mathf.Abs(n.x)>.9f?new Vector2(size.z,size.y):new Vector2(size.x,size.y);
    var face=DynamicFace(pillar.transform,"Pillar surface",n*half*.5f,n,s,stone,c.Surfaces);face.Slippery=true;face.MotionFrame=pillar;faces.Add(face);
   }
   var topple=new GameObject("Pillar falls",typeof(COgheTopplePillar)).GetComponent<COgheTopplePillar>();topple.transform.SetParent(c.Root,false);
   topple.Body=pillar;topple.HingeLocal=new Vector3(.20f,-.30f,-.10f);topple.AxisLocal=Vector3.right;topple.Length=.22f;topple.FallAngle=90f;
   topple.Triggers=new[]{ball.Body};topple.WalkWhenDown=new[]{faces[5]};   // the face toward the ball is on top once down
   NextTrace("P",new Vector3(-.06f,-.2992f,.03f),new Vector3(-.06f,-.2992f,-.05f),new Vector3(.20f,-.2992f,-.05f),new Vector3(.20f,-.2992f,-.10f));
   PlusStep(1,new Vector3(-.14f,-.29f,-.24f));PlusStep(2,new Vector3(-.32f,-.29f,-.16f));PlusStep(3,new Vector3(.20f,-.27f,.0f));PlusStep(4,new Vector3(.20f,-.24f,.20f));
   PlusGhost("G",new Vector3(-.14f,y,.08f));
  }

  // F3 · Kính vỡ, đối trọng (68, from N25 "Chưa đủ nặng"; Mrk 19/10/2026: "thiết kế thêm nhiều bước nữa"). No crate. Thin
  // glass covers the pit, 8 mm above the deck, with the counterweight tray just under it. COghe starts on the floor by Q.
  // The steel ball waits in its trough on the back glass, 11 cm over the glass; two pads A on the floor, 43 cm apart, lift
  // its stop together, so COghe splits and a half stands on each. The ball drops onto the glass and rolls to the lip at its
  // front edge; the glass holds it. A half with the ball only cracks it (.05 + .048 < .12): the halves merge, the whole
  // body climbs the stairs and with the ball breaks the glass (.146). Both drop onto the tray, the counterweight levels the
  // plank and the pawl catches. Out of the pit, across the plank, to the exit.
  static void PlusF3(ExpansionContext c)
  {
   // Q stands 9 cm behind the front glass, so the whole body passes in front of it; the stairs leave the deck behind Q's
   // left tray (at the front they would wall Q in). Two slick blocks close the notch between Q's rear housing and the exit
   // platform. Taps onto the glass aim at its back half: the ball rests at the front lip.
   PlusN25Room(c,-.02f);c.Spawn=new Vector3(.10f,-.25f,-.25f);
   NextQuantum(c,new Vector3(.10f,-.30f,-.16f),.025f,.13f);
   NextPlinth(c,"Q nook filler",new Vector3(.1385f,-.25f,-.04f),new Vector3(.023f,.10f,.10f));
   NextPlinth(c,"Q nook filler",new Vector3(.1725f,-.25f,-.07f),new Vector3(.045f,.10f,.04f));
   var crate=c.Props.Last(p=>p.name=="A crate");
   foreach(var f in crate.GetComponentsInChildren<VenomSurfacePatch>(true))c.Surfaces.Remove(f);
   c.Props.Remove(crate);Object.DestroyImmediate(crate.gameObject);
   var hinge=c.Props.Last(p=>p.name=="Seesaw plank").GetComponent<HingeJoint>();
   hinge.useSpring=true;hinge.spring=new JointSpring{spring=PlankSpring,damper=PlankSpring*.05f,targetPosition=0};
   // the glass: its own collider and walking surface (drawn by a glass sheet; the plain face is hidden)
   var pane=Panel(c.Root,"Thin glass",new Vector3(-.28f,-.192f,.14f),Vector3.up,new Vector2(.12f,.24f),glass,false,Vector2.zero,0,c.Surfaces);
   var look=pane.GetComponent<Renderer>();if(look!=null)look.enabled=false;
   // a pale sheet in a thin metal frame: the box's own glass material left the lid, its cracks and shards near invisible
   var sheet=SpatialMaterial("Thin glass",new Color(.72f,.87f,.93f));var crackInk=SpatialMaterial("Thin glass crack",new Color(.20f,.28f,.36f));
   MechanismVisual(pane.transform,"Thin glass sheet",new Vector3(0,0,-.002f),new Vector3(.12f,.24f,.004f),sheet);
   foreach(var (at,size) in new[]{(new Vector3(-.28f,-.194f,.2625f),new Vector3(.125f,.004f,.005f)),(new Vector3(-.28f,-.194f,.0175f),new Vector3(.125f,.004f,.005f)),
    (new Vector3(-.2175f,-.194f,.14f),new Vector3(.005f,.004f,.25f)),(new Vector3(-.3425f,-.194f,.14f),new Vector3(.005f,.004f,.25f))})
    MechanismVisual(c.Root,"Thin glass frame",at,size,metal);   // the frame stays when the pane breaks
   var sensor=new GameObject("Thin glass load",typeof(COgheTissueSensor)).GetComponent<COgheTissueSensor>();sensor.transform.SetParent(c.Root,false);
   sensor.transform.localPosition=new Vector3(-.28f,-.192f,.14f);sensor.Size=new Vector2(.12f,.24f);sensor.Threshold=1;
   sensor.Column=.05f;   // a part on the glass weighs whole (the contact layer alone read a whole COghe light)
   var cracks=new System.Collections.Generic.List<Renderer>();
   foreach(var (at,len,turn) in new[]{(new Vector2(-.01f,.03f),.09f,25f),(new Vector2(.02f,-.04f),.11f,-35f),(new Vector2(-.02f,-.01f),.07f,80f)})
   {var line=MechanismVisual(pane.transform,"Thin glass crack",new Vector3(at.x,at.y,.0008f),new Vector3(.003f,len,.0012f),crackInk);line.localRotation=Quaternion.Euler(0,0,turn);var r=line.GetComponent<Renderer>();r.enabled=false;cracks.Add(r);}
   var shards=new System.Collections.Generic.List<Transform>();
   for(int i=0;i<8;i++){var shard=MechanismVisual(c.Root,"Thin glass shard",new Vector3(-.28f+(i%2-.5f)*.06f,-.192f,.14f+(i/2-1.5f)*.06f),new Vector3(.03f,.003f,.04f),sheet);shard.gameObject.SetActive(false);shards.Add(shard);}
   var ball=BallChute(c,"Ball chute",new Vector3(-.28f,-.065f,.298f),new Vector3(-.28f,-.08f,.20f),.06f,.02f,0,.06f,out var stop,-.09f);
   sensor.Weights=new[]{ball.Body};
   NextPlinth(c,"Ball lip",new Vector3(-.28f,-.1925f,.015f),new Vector3(.12f,.015f,.01f));   // the glass's front edge: the ball stops on it
   var breaker=new GameObject("Thin glass breaks",typeof(COgheBreakableGlass)).GetComponent<COgheBreakableGlass>();breaker.transform.SetParent(c.Root,false);
   breaker.Sensor=sensor;breaker.Pane=pane.gameObject;breaker.BreakLoad=.12f;breaker.CrackLoad=.03f;breaker.Cracks=cracks.ToArray();breaker.Shards=shards.ToArray();
   c.Root.GetComponentInChildren<COgheSeesawBridge>().RequiredLoad=ball.Body;
   // Two pads A on the floor, 43 cm apart either side of Q: one body cannot load both, so the stop needs a split. Once both
   // have pressed it stays up.
   var rightPad=ExpansionPad(c,"A",new Vector3(.33f,-.298f,-.24f),.009f,.10f);
   var leftPad=ExpansionPad(c,"A",new Vector3(-.10f,-.298f,-.25f),.009f,.10f);
   var both=new GameObject("Both A pads lift the ball stop",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();both.transform.SetParent(c.Root,false);
   both.Inputs=new[]{rightPad,leftPad};both.Rails=new COgheRailSlider[0];both.Output=stop;both.Any=false;both.Retain=true;
   NextTrace("A",new Vector3(.28f,-.2992f,-.285f),new Vector3(-.05f,-.2992f,-.285f));
   NextTrace("A",new Vector3(-.185f,-.1992f,-.27f),new Vector3(-.185f,-.1992f,.28f),new Vector3(-.25f,-.1992f,.28f));
   PlusStep(1,new Vector3(.10f,-.19f,-.16f));PlusStep(2,new Vector3(.33f,-.29f,-.24f));PlusStep(3,new Vector3(-.10f,-.29f,-.25f));PlusStep(4,new Vector3(-.06f,-.24f,-.02f));
   PlusStep(5,new Vector3(-.29f,-.18f,.19f));PlusStep(6,new Vector3(-.19f,-.20f,.10f));PlusStep(7,new Vector3(.25f,-.20f,.12f));
  }
 }
}
