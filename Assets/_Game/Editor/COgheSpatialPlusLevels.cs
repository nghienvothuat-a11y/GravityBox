using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
 // Spatial Plus levels E01–E18, B1, B2 (see COgheSpatialPlusBuilder). Coordinates are Root-local metres: floor -0.30,
 // box ±0.40 × ±0.30. Heights follow the Spatial rules: ivory climbs, lavender is slick (≤3.5 cm riser passes, ≥5 blocks).
 public static partial class VenomCampaignBuilder
 {
  // ---- shared pieces -------------------------------------------------------------------------------------------
  // A flat gear on a vertical axle; wheels of one train must sit at one height, 8 cm apart (pitch radius 4 cm).
  static Transform PlusGear(Transform parent,string name,Vector3 local){var g=MeshingStationWheel(parent,name,local,.04f,18,0);g.localRotation=Quaternion.Euler(90,0,0);return g;}
  // A 3 cm ivory gear table; returns the height of gears standing on it.
  // Slick all over: machinery, not a walkway. An ivory top drew routes across it and the gears on it caught the body.
  static float PlusGearTable(ExpansionContext c,string name,Vector3 centre,Vector2 size,float top=-.27f){Top(NextPlinth(c,name,new Vector3(centre.x,(top-.30f)*.5f,centre.z),new Vector3(size.x,top+.30f,size.y))).Slippery=true;return top+.008f;}
  // A vertical shaft carrying gears on several layers (drawn; the train couples them through ShaftLinks).
  static void PlusShaft(ExpansionContext c,string name,Vector3 at,float bottom,float top)=>MechanismVisual(c.Root,name,new Vector3(at.x,(bottom+top)*.5f,at.z),new Vector3(.012f,(top-bottom)*.5f,.012f),metal,PrimitiveType.Cylinder);
  static COgheGearTrain PlusTrain(ExpansionContext c,string name,COgheTissueSensor clutch,COgheRailSlider rack,bool gatesExit,int[] shafts,params Transform[] wheels)
  {
   var train=new GameObject(name,typeof(COgheGearTrain)).GetComponent<COgheGearTrain>();train.transform.SetParent(c.Root,false);
   train.Wheels=wheels;train.PitchRadii=wheels.Select(_=>.04f).ToArray();train.ToothCounts=wheels.Select(_=>18).ToArray();
   train.InputClutch=clutch;train.Rack=rack;train.GatesExit=gatesExit;train.ShaftLinks=shafts??System.Array.Empty<int>();train.MeshTolerance=.004f;train.LatchOutput=true;train.MotorTorque=.06f;return train;
  }
  // A deck a gear rack lifts by `travel` (slick sides); its catch keeps it up unless the train returns it.
  static COgheRailSlider PlusRisingDeck(ExpansionContext c,string name,Vector3 start,Vector3 size,float travel,bool ivorySides=false)
  {
   var deck=ExpansionRail(c,name,start,Vector3.up,travel,0,size,.05f,.01f,false,false);deck.GetComponent<VenomMovableProp>().Manipulable=false;deck.LatchAtEnd=true;TrimSideSlabs(deck);
   foreach(var f in deck.GetComponentsInChildren<VenomSurfacePatch>()){f.MotionFrame=deck.Body;if(f.Normal.y<.9f)f.Slippery=!ivorySides;}
   return deck;
  }
  // A carriage (handle = label) that slides one flat gear, carried `arm` ahead of it along its travel, into a train.
  static COgheTapRail PlusGearCarriage(ExpansionContext c,string label,Vector3 start,Vector3 axis,float travel,VenomSurfacePatch floor,float gearY,float arm,out Transform gear)
  {
   var task=ViewTask(c,label,start,axis,travel,floor);var local=axis.normalized*arm;local.y=gearY-start.y;
   gear=PlusGear(task.Rail.transform,label+" carried gear",local);
   var bar=MechanismVisual(task.Rail.transform,label+" gear arm",new Vector3(local.x*.5f,local.y-.004f,local.z*.5f),new Vector3(Mathf.Abs(local.x)+.008f,.006f,Mathf.Abs(local.z)+.008f),metal);
   foreach(var f in task.Rail.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true; // a mechanism, not a stepping block: routes go round it
   task.Rail.LatchAtEnd=true;return task;
  }
  // A free seesaw plank on a low wall: counterweighted to rest near end (−x) down on the floor; a body that walks past
  // the axle tips it and walks down into the next room. Hinge + mass only, no motor.
  // Doors, caps, pins and shutters: slick all round. An ivory gate was a ladder (E03/E09: over the door onto the
  // partition; E02: onto the cap and down behind it into the tube mouth).
  static COgheRailSlider PlusGate(ExpansionContext c,string name,Vector3 start,Vector3 axis,float travel,Vector3 size)
  {
   var gate=ViewGate(c,name,start,axis,travel,size);
   foreach(var f in gate.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;
   return gate;
  }
  static void PlusSeesaw(ExpansionContext c,string label,Vector3 pivot,float half,float rest)
  {
   var plank=Prop(c.Root,label+" seesaw plank",pivot,new Vector3(half*2,.02f,.12f),false,plastic,c.Surfaces);c.Props.Add(plank);
   plank.Body.mass=.05f;plank.Body.centerOfMass=new Vector3(-.03f,0,0);
   foreach(var f in plank.GetComponentsInChildren<VenomSurfacePatch>()){f.MotionFrame=plank.Body;if(f.Normal.y<-.9f)f.Slippery=true;} // ivory edges: near its foot the plank is mounted from the side too
   plank.transform.localRotation=Quaternion.Euler(0,0,rest);
   var hinge=plank.gameObject.AddComponent<HingeJoint>();hinge.connectedBody=c.Root.GetComponent<Rigidbody>();hinge.autoConfigureConnectedAnchor=false;hinge.anchor=Vector3.zero;hinge.connectedAnchor=pivot;hinge.axis=Vector3.forward;
   hinge.useLimits=true;hinge.limits=new JointLimits{min=-2*rest,max=1};hinge.enableCollision=true;
   var axle=MechanismVisual(c.Root,label+" seesaw axle",pivot,new Vector3(.012f,.075f,.012f),metal,PrimitiveType.Cylinder);axle.localRotation=Quaternion.Euler(90,0,0);
   foreach(float z in new[]{-.07f,.07f})MechanismVisual(c.Root,label+" seesaw bracket",pivot+new Vector3(0,-.012f,z),new Vector3(.02f,.024f,.01f),metal);
   // Under each end that comes down, slick filler steps 2 mm below the plank: the wedge between a low plank and the
   // floor was a crawl space a body wedged into (wander test). No gap under the plank is taller than 3 cm.
   float t=Mathf.Tan(rest*Mathf.Deg2Rad),under=pivot.y-.01f/Mathf.Cos(rest*Mathf.Deg2Rad);
   foreach(float side in new[]{-1f,1f})
    foreach(var (near,far) in new[]{(.015f,.07f),(.07f,.12f)})
    {
     float top=under-t*far-.002f;
     Top(NextPlinth(c,label+" seesaw filler",new Vector3(pivot.x+side*(near+far)*.5f,(top-.30f)*.5f,pivot.z),new Vector3(far-near,top+.30f,.12f))).Slippery=true; // not a stair onto the wall
    }
  }

  // ---- chapter 2 -------------------------------------------------------------------------------------------------
  // E01 · 13 · Thang chở hàng. The crate already rides the lift tray: board beside it, send the tray up, push the crate
  // off into the socket beside the exit bench, climb. Level 13's idea without loading the crate; mirrored.
  static void PlusE01(ExpansionContext c)
  {
   c.Exit=new Vector3(-.335f,.058f,.30f);c.Spawn=new Vector3(.30f,-.22f,-.16f);NextShell(c,.30f,true);
   NextPlinth(c,"Lower landing",new Vector3(.255f,-.284f,0),new Vector3(.29f,.032f,.59f));
   NextPlinth(c,"Upper landing",new Vector3(-.203f,-.186f,.16f),new Vector3(.13f,.228f,.27f));
   NextPlinth(c,"Exit bench",new Vector3(-.3315f,-.155f,.16f),new Vector3(.127f,.29f,.27f));
   var rail=ExpansionRail(c,"Passenger tray",new Vector3(-.014f,-.283f,.16f),Vector3.up,.20f,0,new Vector3(.24f,.026f,.24f),.05f,.004f,true,false);rail.GetComponent<VenomMovableProp>().Manipulable=false;
   var lift=rail.gameObject.AddComponent<COghePassengerLift>();lift.Rail=rail;foreach(var face in rail.GetComponentsInChildren<VenomSurfacePatch>())face.MotionFrame=rail.Body;TrimSideSlabs(rail);
   lift.DeckSize=new Vector2(.24f,.24f);lift.DeckHeight=.013f;lift.Deck=rail.GetComponentsInChildren<VenomSurfacePatch>().First(p=>p.Normal.y>.9f);lift.CarriesProps=true;lift.MaximumForce=4;
   lift.BoardPoint=new GameObject("Rider stance").transform;lift.BoardPoint.SetParent(rail.transform,false);lift.BoardPoint.localPosition=new Vector3(.064f,.031f,0);
   lift.Panel=MechanismVisual(rail.transform,"A lift panel",new Vector3(.09f,.018f,-.09f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder);
   lift.CallPanels=new[]{MechanismVisual(c.Root,"A lower call panel",new Vector3(.14f,-.262f,-.04f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder),
    MechanismVisual(c.Root,"A upper call panel",new Vector3(-.20f,-.062f,.05f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder)};
   foreach(float x in new[]{.12f,-.14f})MechanismVisual(c.Root,"Lift upright",new Vector3(x,-.18f,.285f),new Vector3(.012f,.24f,.012f),metal);
   var crate=NextLooseCrate(c,"A crate",new Vector3(-.07f,-.2545f,.16f),new Vector3(.10f,.03f,.10f),.04f);
   var socket=new GameObject("A crate socket",typeof(COghePropSocket)).GetComponent<COghePropSocket>();socket.transform.SetParent(c.Root,false);socket.transform.localPosition=new Vector3(-.214f,-.057f,.16f);
   socket.Prop=crate;socket.Socket=socket.transform;socket.Tolerance=new Vector3(.016f,.012f,.035f);socket.Pawl=MechanismVisual(c.Root,"A socket pawl",new Vector3(-.266f,-.069f,.16f),new Vector3(.006f,.006f,.05f),metal); // wide along the bench face: any spot there is a step
   NextOutline(c,"Upper crate socket outline",new Vector3(-.214f,-.0715f,.16f),new Vector2(.106f,.106f));
   PlusStep(1,new Vector3(.05f,-.26f,.16f));PlusStep(2,new Vector3(.076f,-.25f,.07f));PlusStep(3,new Vector3(-.20f,-.06f,.16f));PlusStep(4,new Vector3(-.33f,-.01f,.16f));
   PlusRoute("100",c.Spawn,new Vector3(.18f,-.26f,.04f),new Vector3(.05f,-.26f,.16f));
   PlusRoute("100",new Vector3(.05f,-.06f,.16f),new Vector3(-.14f,-.06f,.16f),new Vector3(-.21f,-.04f,.16f),new Vector3(-.33f,-.01f,.20f),c.Exit);
  }

  // E02 · 15 · Hai ống, một đích. Two separate tubes: tube 1 climbs to a slick balcony where A lifts the cap of tube 2;
  // back down the same tube, then tube 2 over the partition into the exit room.
  static void PlusE02(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.225f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.12f,-.25f,-.22f);NextShell(c,.10f,true);
   NextPlinth(c,"Low partition",new Vector3(.08f,-.23f,0),new Vector3(.03f,.14f,.59f));
   var balcony=Top(NextPlinth(c,"Left balcony",new Vector3(-.28f,-.21f,.15f),new Vector3(.22f,.18f,.29f)));
   ChapterTube(c,"Balcony tube",new Vector3(-.08f,-.255f,.02f),new Vector3(-.08f,-.25f,.07f),new Vector3(-.10f,-.20f,.11f),new Vector3(-.13f,-.12f,.13f),new Vector3(-.17f,-.08f,.14f),new Vector3(-.21f,-.075f,.14f)); // a tighter bend stalled the native (frame-stepped) player
   // The exit tube hugs the partition, clear of the ways across room 1: crossing the room low, it caught bodies passing under.
   var tube=ChapterTube(c,"Exit tube",new Vector3(-.04f,-.255f,-.20f),new Vector3(-.015f,-.25f,-.20f),new Vector3(0,-.22f,-.20f),new Vector3(.02f,-.17f,-.20f),new Vector3(.045f,-.125f,-.20f),new Vector3(.07f,-.10f,-.20f),new Vector3(.095f,-.09f,-.20f),
    new Vector3(.12f,-.10f,-.20f),new Vector3(.145f,-.125f,-.20f),new Vector3(.17f,-.17f,-.20f),new Vector3(.19f,-.22f,-.20f),new Vector3(.205f,-.25f,-.20f),new Vector3(.23f,-.255f,-.20f)); // ≤ 68° like the other tubes
   var cap=PlusGate(c,"A tube cap",new Vector3(-.051f,-.253f,-.20f),Vector3.up,.12f,new Vector3(.012f,.092f,.092f));
   tube.EntryBlocker=cap.GetComponent<VenomMovableProp>().CollisionShapes[0];
   var a=ViewTask(c,"A",new Vector3(-.34f,-.097f,.08f),Vector3.right,.08f,balcony);
   ViewLink(c,a.Rail,cap,false,null);
   NextTrace("A",new Vector3(-.30f,-.1192f,.04f),new Vector3(-.30f,-.1192f,.02f),new Vector3(-.39f,-.1192f,.02f));
   PlusStep(1,new Vector3(-.08f,-.255f,.02f));PlusStep(2,new Vector3(-.30f,-.12f,.08f));PlusStep(3,new Vector3(-.21f,-.075f,.14f));PlusStep(4,new Vector3(-.04f,-.255f,-.20f));PlusStep(5,new Vector3(.33f,-.30f,.12f));
   PlusRoute("100",c.Spawn,new Vector3(-.10f,-.29f,-.05f),new Vector3(-.08f,-.255f,.02f));PlusRoute("tube",new Vector3(-.08f,-.255f,.02f),new Vector3(-.10f,-.20f,.11f),new Vector3(-.17f,-.08f,.14f),new Vector3(-.21f,-.075f,.14f));
   PlusRoute("100",new Vector3(-.21f,-.12f,.14f),new Vector3(-.30f,-.12f,.08f));
   PlusRoute("100",new Vector3(-.10f,-.29f,-.03f),new Vector3(-.10f,-.29f,-.20f),new Vector3(-.04f,-.255f,-.20f));PlusRoute("tube",new Vector3(-.04f,-.255f,-.20f),new Vector3(.095f,-.09f,-.20f),new Vector3(.23f,-.255f,-.20f));PlusRoute("100",new Vector3(.25f,-.30f,-.20f),new Vector3(.36f,-.30f,.12f),c.Exit);
  }

  // E03 · 18 · Giữ cửa cho bạn. One half holds pad A (the door rises); the other walks through and pulls B, which
  // latches the door open for good; the holder leaves the pad, follows, and they merge.
  static void PlusE03(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.225f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   var door=PlusGate(c,"A door",new Vector3(.05f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
   NextQuantum(c,new Vector3(-.18f,-.30f,-.02f),.025f,.14f);
   var a=ExpansionPad(c,"A",new Vector3(-.32f,-.298f,-.20f),.009f,.10f);
   var b=ViewTask(c,"B",new Vector3(.25f,-.277f,-.04f),Vector3.right,.08f,floor);b.OneWay=true;
   var safe=new GameObject("Door safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(.05f,-.25f,-.20f);safe.Size=new Vector3(.06f,.10f,.12f);
   var hold=new GameObject("A holds, B latches the door",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a};hold.Rails=new[]{b.Rail};hold.Output=door;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   NextTrace("A",new Vector3(-.27f,-.2992f,-.20f),new Vector3(-.05f,-.2992f,-.27f),new Vector3(.03f,-.2992f,-.27f));
   NextTrace("B",new Vector3(.25f,-.2992f,.0f),new Vector3(.25f,-.2992f,.03f),new Vector3(.07f,-.2992f,.03f),new Vector3(.07f,-.2992f,-.14f));
   PlusStep(1,new Vector3(-.18f,-.27f,-.07f));PlusStep(2,new Vector3(-.32f,-.29f,-.20f));PlusStep(3,new Vector3(.25f,-.28f,-.04f));PlusStep(4,new Vector3(.12f,-.29f,-.20f));PlusStep(5,new Vector3(.34f,-.29f,.20f));
   PlusGhost("50%",new Vector3(-.32f,-.28f,-.20f));PlusGhost("50%",new Vector3(.25f,-.28f,-.10f));
   PlusRoute("100",c.Spawn,new Vector3(-.18f,-.27f,-.06f));PlusRoute("50a",new Vector3(-.33f,-.29f,.00f),new Vector3(-.32f,-.29f,-.20f));
   PlusRoute("50b",new Vector3(-.03f,-.29f,.00f),new Vector3(-.01f,-.29f,-.20f),new Vector3(.12f,-.29f,-.20f),new Vector3(.25f,-.29f,-.10f));
   PlusRoute("50a",new Vector3(-.28f,-.29f,-.24f),new Vector3(.12f,-.29f,-.24f),new Vector3(.22f,-.29f,-.14f));PlusRoute("100",new Vector3(.25f,-.29f,-.08f),new Vector3(.34f,-.29f,.20f),c.Exit);
  }

  // ---- chapter 3 -------------------------------------------------------------------------------------------------
  // E04 · 21 · Cõng thùng. Cart A (3 cm) runs to the slick exit shelf; crate B (9 cm, ivory) runs across the cart.
  static void PlusE04(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.152f,.17f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.25f,-.25f,-.20f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit shelf",new Vector3(.26f,-.24f,.16f),new Vector3(.28f,.12f,.28f));
   var cart=NextCrate(c,"A",new Vector3(-.25f,-.285f,.15f),Vector3.right,.27f,new Vector3(.20f,.03f,.30f),floor,new Vector3(-.03f,0,-.158f),new Vector3(0,0,-.052f),.06f,.012f,0,false); // ivory, 20 cm wide: the body mounts it clear of the shelf's slick corner
   var crate=NextCrate(c,"B",new Vector3(-.21f,-.225f,.06f),Vector3.forward,.12f,new Vector3(.12f,.09f,.12f),floor,new Vector3(-.035f,0,-.068f),new Vector3(0,0,-.08f),.04f,.010f,0,false);
   MountOnCarrier(c,crate,cart.GetComponent<VenomMovableProp>()); // B's handle left of centre: push and climb clear of the shelf's slick corner
   NextOutline(c,"Cart stop outline",new Vector3(.02f,-.2995f,.15f),new Vector2(.205f,.305f));
   PlusStep(1,new Vector3(-.25f,-.29f,-.02f));PlusStep(2,new Vector3(.05f,-.26f,-.03f));PlusStep(3,new Vector3(.05f,-.18f,.18f));PlusStep(4,new Vector3(.30f,-.18f,.17f));
   PlusGhost("thùng",new Vector3(.05f,-.225f,.18f));
   PlusRoute("100",c.Spawn,new Vector3(-.25f,-.29f,-.04f),new Vector3(.04f,-.29f,-.04f));
   PlusRoute("100",new Vector3(.05f,-.29f,-.07f),new Vector3(.05f,-.26f,.04f),new Vector3(.05f,-.18f,.18f),new Vector3(.30f,-.18f,.17f),c.Exit);
  }

  // E05 · 23 · Hai nhịp dây. Rope A across from the ivory start bank to a slick island, rope B front → back from the
  // island to the low exit bank. A drop lands on the rescue floor, with stairs back to the start bank.
  static void PlusE05(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.19f,.235f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.11f,-.18f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var start=NextPlinth(c,"Start bank",new Vector3(-.28f,-.23f,-.14f),new Vector3(.24f,.14f,.32f),false);
   NextStairs(c,"Recovery stairs",new Vector3(-.28f,0,.02f),Vector3.forward,-.16f,.12f,3,.06f);
   var island=NextPlinth(c,"Middle island",new Vector3(.2425f,-.25f,-.195f),new Vector3(.315f,.10f,.21f)); // ends 13 cm short of rope B's anchor, as the start bank for A: a longer island drags the swing
   var exitBank=NextPlinth(c,"Exit bank",new Vector3(.2425f,-.27f,.2275f),new Vector3(.315f,.06f,.145f)); // edge 11.5 cm past rope B's anchor, as rope A
   NextSwing(c,"A",new Vector3(-.03f,.14f,-.14f),.28f,40f,new Vector3(-.26f,-.14f,-.14f),Top(start),new[]{Top(island)},new[]{new Vector3(.22f,-.18f,-.14f)},floor);
   NextSwing(c,"B",new Vector3(.26f,.10f,.04f),.28f,40f,new Vector3(.26f,-.18f,-.19f),Top(island),new[]{Top(exitBank)},new[]{new Vector3(.26f,-.22f,.24f)},floor,Vector3.forward);
   PlusStep(1,new Vector3(-.21f,-.075f,-.14f));PlusStep(2,new Vector3(.22f,-.20f,-.14f));PlusStep(3,new Vector3(.26f,-.115f,-.14f));PlusStep(4,new Vector3(.26f,-.24f,.24f));
   PlusRoute("100",c.Spawn,new Vector3(-.23f,-.14f,-.14f),new Vector3(.0f,-.14f,-.14f),new Vector3(.22f,-.18f,-.14f),new Vector3(.26f,-.18f,-.18f));
   PlusRoute("100",new Vector3(.26f,-.18f,-.14f),new Vector3(.26f,-.14f,.04f),new Vector3(.26f,-.22f,.24f),c.Exit);
  }

  // E06 · 26 · Bập bênh. Two free seesaw planks over two low slick walls: up the plank, past the axle it tips and lets
  // the body down into the next room. The planks rest near end down; with no load they return.
  static void PlusE06(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.255f,.10f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.25f,-.20f);NextShell(c,.10f,true);
   NextPlinth(c,"Low wall",new Vector3(-.15f,-.26f,0),new Vector3(.03f,.08f,.59f));
   NextPlinth(c,"Low wall",new Vector3(.13f,-.26f,0),new Vector3(.03f,.08f,.59f));
   PlusSeesaw(c,"A",new Vector3(-.15f,-.20f,-.10f),.20f,27f);
   PlusSeesaw(c,"B",new Vector3(.13f,-.20f,.14f),.20f,27f);
   PlusStep(1,new Vector3(-.30f,-.29f,-.10f));PlusStep(2,new Vector3(-.02f,-.29f,-.10f));PlusStep(3,new Vector3(.25f,-.29f,.14f));
   PlusRoute("100",c.Spawn,new Vector3(-.32f,-.29f,-.10f),new Vector3(-.15f,-.19f,-.10f),new Vector3(.03f,-.29f,-.10f),new Vector3(-.05f,-.29f,.14f),new Vector3(.13f,-.19f,.14f),new Vector3(.31f,-.29f,.14f),c.Exit);
  }

  // E07 · 29 · Chồng hai tầng. Block B rides on block A. Pull A against the 18 cm slick shelf, climb it, push B along A
  // to the shelf edge (standing on A beside it), climb B, step across.
  static void PlusE07(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.075f,.17f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.28f,-.25f,-.22f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit shelf",new Vector3(.26f,-.21f,.17f),new Vector3(.28f,.18f,.26f));
   var low=NextCrate(c,"A",new Vector3(-.20f,-.255f,.14f),Vector3.right,.17f,new Vector3(.30f,.09f,.16f),floor,new Vector3(0,0,-.088f),new Vector3(0,0,-.06f),.08f,.012f,0,false);
   var high=NextCrate(c,"B",new Vector3(-.20f,-.163f,.14f),Vector3.right,.10f,new Vector3(.10f,.09f,.14f),DockedTop(low),new Vector3(-.058f,0,0),new Vector3(-.06f,0,0),.04f,.010f,0,false);
   high.Handle.localRotation=Quaternion.Euler(0,90,0);high.CompensateLoad=true;high.Rail.CatchTolerance=.004f;high.Rail.LatchAtEnd=false;
   // B floats 2 mm over A (y is held by its rail): lying on A, its corner caught A's end slab. No end latch: its brake locked
   // exactly at the catch threshold and sagged a fraction of a millimetre back under load, so the pull never counted as done.
   // At the end it docks (a flush static top on A carries the body) and nothing pushes B itself.
   MountOnCarrier(c,high,low.GetComponent<VenomMovableProp>());
   NextOutline(c,"A socket",new Vector3(-.03f,-.2995f,.14f),new Vector2(.305f,.165f));
   PlusStep(1,new Vector3(-.20f,-.29f,.03f));PlusStep(2,new Vector3(-.12f,-.21f,.14f));PlusStep(3,new Vector3(-.03f,-.12f,.14f));PlusStep(4,new Vector3(.24f,-.12f,.17f));
   PlusRoute("100",c.Spawn,new Vector3(-.20f,-.29f,.01f),new Vector3(-.03f,-.29f,.01f));
   PlusRoute("100",new Vector3(-.14f,-.29f,.02f),new Vector3(-.13f,-.21f,.14f),new Vector3(.07f,-.12f,.14f),new Vector3(.24f,-.12f,.17f),c.Exit);
  }

  // ---- BOSS · 30 · Tháp khối --------------------------------------------------------------------------------------
  // A three-tier stack is both the tool and the goal. C1 (9 cm, heavy: only the whole body hauls it) runs to the 27 cm
  // exit tower; C2 rides C1 across, C3 rides C2 front ↔ back. At the start C1 stands against the lever shelf, whose
  // ivory side is climbable from C1 only. C1's bolt is caught only with pad A loaded AND lever B pulled (a split).
  // From the shelf, push C2 to C1's tower end; standing on C2, push C3 back (it only unlocks once C2 is across), or
  // the tower's overhang (18–27 cm band, front half) stops C1 short. Merged, the body hauls C1 over and climbs.
  static void PlusB1(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,.02f,.15f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(38,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(.0f,-.30f,-.20f),.025f,.14f);
   NextPlinth(c,"Exit tower",new Vector3(.31f,-.165f,.15f),new Vector3(.18f,.27f,.30f));
   NextPlinth(c,"Tower overhang",new Vector3(.19f,-.072f,.06f),new Vector3(.06f,.084f,.12f)); // 6 mm over C2's top, C3's band
   var shelfFaces=NextPlinth(c,"Lever shelf",new Vector3(-.315f,-.21f,.21f),new Vector3(.17f,.18f,.18f));var shelf=Top(shelfFaces);
   shelfFaces.First(f=>f.Normal.x>.9f).Slippery=false; // ivory toward C1: the stack is the stair to the lever
   var c1=NextCrate(c,"C1",new Vector3(-.08f,-.255f,.14f),Vector3.right,.15f,new Vector3(.30f,.09f,.24f),floor,new Vector3(0,0,-.128f),new Vector3(0,0,-.06f),.22f,.45f,0,false);
   c1.CompensateLoad=true;c1.StallSeconds=5;
   // C2's handle is on its top-left edge, reached from 12 cm back on the shelf so the pusher never leaves the shelf;
   // C3 rides C2's right half, leaving C2's left strip to stand on while pushing C3. Riders float 2 mm over their carrier.
   var c2=NextCrate(c,"C2",new Vector3(-.12f,-.163f,.14f),Vector3.right,.08f,new Vector3(.22f,.09f,.24f),shelf,new Vector3(-.10f,.049f,0),new Vector3(-.12f,0,0),.06f,.012f,0,false);
   c2.Handle.localRotation=Quaternion.Euler(0,90,0);c2.CompensateLoad=true;MountOnCarrier(c,c2,c1.GetComponent<VenomMovableProp>());
   var c3=NextCrate(c,"C3",new Vector3(-.06f,-.071f,.07f),Vector3.forward,.14f,new Vector3(.10f,.09f,.10f),DockedTop(c2),new Vector3(-.058f,0,0),new Vector3(-.06f,0,0),.03f,.010f,0,false);
   c3.Handle.localRotation=Quaternion.Euler(0,90,0);c3.CompensateLoad=true;MountOnCarrier(c,c3,c2.GetComponent<VenomMovableProp>());
   var pad=ExpansionPad(c,"A",new Vector3(.30f,-.298f,-.20f),.009f,.10f);
   var bolt=PlusGate(c,"C1 bolt",new Vector3(-.12f,-.285f,.285f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var lever=ViewTask(c,"B",new Vector3(-.35f,-.097f,.21f),Vector3.right,.05f,shelf);lever.OneWay=true;
   var hold=new GameObject("A and B catch C1's bolt",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{pad};hold.Rails=new[]{lever.Rail};hold.Output=bolt;hold.Any=false;hold.Retain=true;
   c1.RequiredRail=bolt;c1.RequiredEnd=true;c3.RequiredRail=c2.Rail;c3.RequiredEnd=true;
   NextTrace("A",new Vector3(.30f,-.2992f,-.15f),new Vector3(.30f,-.2992f,-.07f),new Vector3(-.12f,-.2992f,-.07f),new Vector3(-.12f,-.2992f,.27f));
   NextTrace("B",new Vector3(-.31f,-.1192f,.21f),new Vector3(-.26f,-.1192f,.21f),new Vector3(-.26f,-.1192f,.29f));
   PlusStep(1,new Vector3(0,-.27f,-.17f));PlusStep(2,new Vector3(.30f,-.29f,-.20f));PlusStep(3,new Vector3(-.33f,-.12f,.21f));PlusStep(4,new Vector3(-.27f,-.12f,.16f));PlusStep(5,new Vector3(-.03f,-.12f,.09f));
   PlusStep(6,new Vector3(-.08f,-.29f,-.06f));PlusStep(7,new Vector3(.10f,-.29f,-.06f));PlusStep(8,new Vector3(.31f,-.03f,.15f));
   PlusGhost("50%",new Vector3(.30f,-.28f,-.20f));PlusGhost("50%",new Vector3(-.33f,-.11f,.21f));
   PlusLabel("C1",new Vector3(-.19f,-.21f,.03f));PlusLabel("C2",new Vector3(-.14f,-.12f,.03f));PlusLabel("C3",new Vector3(-.10f,-.03f,.03f));PlusLabel("mái chìa",new Vector3(.19f,-.03f,.02f));
   PlusRoute("100",c.Spawn,new Vector3(0,-.27f,-.17f));PlusRoute("50a",new Vector3(.14f,-.29f,-.18f),new Vector3(.30f,-.29f,-.20f));
   PlusRoute("50b",new Vector3(-.14f,-.29f,-.18f),new Vector3(-.19f,-.29f,-.05f),new Vector3(-.19f,-.21f,.10f),new Vector3(-.26f,-.12f,.18f),new Vector3(-.33f,-.12f,.21f));
   PlusRoute("100",new Vector3(-.08f,-.29f,-.06f),new Vector3(.10f,-.29f,-.06f));
   PlusRoute("100",new Vector3(.12f,-.21f,.05f),new Vector3(.13f,-.12f,.21f),new Vector3(.17f,-.03f,.21f),new Vector3(.31f,-.03f,.15f),c.Exit);
  }

  // ---- chapter 4 -------------------------------------------------------------------------------------------------
  // E08 · 31 · Bánh răng đầu tiên. Stand on pad A: the two gears turn and the middle deck rises between the ledges.
  static void PlusE08(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.155f,.13f);c.Outward=Vector3.right;c.Spawn=new Vector3(.12f,-.25f,-.22f);NextShell(c,.10f,true);
   NextPlinth(c,"Start ledge",new Vector3(-.28f,-.24f,.13f),new Vector3(.24f,.12f,.28f));
   NextStairs(c,"Start stairs",new Vector3(-.28f,0,-.01f),Vector3.back,-.18f,.12f,3,.06f);
   NextPlinth(c,"Exit ledge",new Vector3(.28f,-.24f,.13f),new Vector3(.24f,.12f,.28f));
   var deck=PlusRisingDeck(c,"A rising deck",new Vector3(0,-.285f,.13f),new Vector3(.31f,.03f,.16f),.09f);
   float y=PlusGearTable(c,"Gear table",new Vector3(.0f,0,-.10f),new Vector2(.20f,.10f));
   var pad=ExpansionPad(c,"A",new Vector3(-.12f,-.298f,-.22f),.009f,.10f);
   var g0=PlusGear(c.Root,"A motor gear",new Vector3(-.04f,y,-.10f));var g1=PlusGear(c.Root,"A output gear",new Vector3(.04f,y,-.10f));
   MechanismVisual(c.Root,"A rack drive",new Vector3(.04f,-.266f,-.005f),new Vector3(.012f,.006f,.19f),metal);
   PlusTrain(c,"A lifts the deck",pad,deck,false,null,g0,g1);
   NextTrace("A",new Vector3(-.12f,-.2992f,-.17f),new Vector3(-.12f,-.2992f,-.10f),new Vector3(-.10f,-.2992f,-.10f));
   PlusStep(1,new Vector3(-.12f,-.29f,-.22f));PlusStep(2,new Vector3(-.28f,-.18f,.06f));PlusStep(3,new Vector3(0,-.18f,.13f));PlusStep(4,new Vector3(.30f,-.18f,.13f));
   PlusGhost("bệ nâng",new Vector3(0,-.18f,.13f));
   PlusRoute("100",c.Spawn,new Vector3(-.12f,-.29f,-.22f));PlusRoute("100",new Vector3(-.18f,-.29f,-.24f),new Vector3(-.28f,-.29f,-.20f),new Vector3(-.28f,-.18f,.04f),new Vector3(-.20f,-.18f,.13f),new Vector3(.30f,-.18f,.13f),c.Exit);
  }

  // E09 · 33 · Đủ nặng mới mở. The heavy pad needs half a body (a quarter is too light). The half holds the door; the
  // two quarters walk onto the two light pads behind it, which catch the door pin; the half leaves, all merge.
  static void PlusE09(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.225f,.22f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   var door=PlusGate(c,"A door",new Vector3(.05f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
   NextQuantum(c,new Vector3(-.18f,-.30f,-.02f),.025f,.14f);
   var heavy=ExpansionPad(c,"A",new Vector3(-.32f,-.298f,-.20f),.030f,.10f);TapLabel(c.Root,"50%",new Vector3(-.32f,-.2985f,-.265f));
   var l1=ExpansionPad(c,"B1",new Vector3(.22f,-.298f,.12f),.009f,.09f);var l2=ExpansionPad(c,"B2",new Vector3(.32f,-.298f,-.10f),.009f,.09f);
   TapLabel(c.Root,"25%",new Vector3(.22f,-.2985f,.06f));TapLabel(c.Root,"25%",new Vector3(.32f,-.2985f,-.16f));
   var pin=PlusGate(c,"B door pin",new Vector3(.09f,-.285f,.26f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var both=new GameObject("B1 and B2 catch the door pin",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();both.transform.SetParent(c.Root,false);both.Inputs=new[]{l1,l2};both.Output=pin;both.Retain=true;
   var safe=new GameObject("Door safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(.05f,-.25f,-.20f);safe.Size=new Vector3(.06f,.10f,.12f);
   var hold=new GameObject("A holds, the pin keeps the door",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{heavy};hold.Rails=new[]{pin};hold.Output=door;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   NextTrace("A",new Vector3(-.27f,-.2992f,-.20f),new Vector3(-.05f,-.2992f,-.27f),new Vector3(.03f,-.2992f,-.27f));
   NextTrace("B",new Vector3(.22f,-.2992f,.17f),new Vector3(.22f,-.2992f,.26f),new Vector3(.11f,-.2992f,.26f));NextTrace("B",new Vector3(.32f,-.2992f,-.05f),new Vector3(.32f,-.2992f,.20f),new Vector3(.22f,-.2992f,.20f));
   PlusStep(1,new Vector3(-.18f,-.27f,-.10f));PlusStep(2,new Vector3(-.03f,-.27f,.01f));PlusStep(3,new Vector3(-.32f,-.29f,-.20f));PlusStep(4,new Vector3(.22f,-.29f,.12f));PlusStep(5,new Vector3(.12f,-.29f,-.20f));PlusStep(6,new Vector3(.34f,-.29f,.22f));
   PlusGhost("50%",new Vector3(-.32f,-.28f,-.20f));PlusGhost("25%",new Vector3(.22f,-.28f,.12f));PlusGhost("25%",new Vector3(.32f,-.28f,-.10f));
   PlusRoute("100",c.Spawn,new Vector3(-.18f,-.27f,-.06f));PlusRoute("50a",new Vector3(-.33f,-.29f,.00f),new Vector3(-.32f,-.29f,-.20f));
   PlusRoute("25",new Vector3(-.03f,-.29f,.00f),new Vector3(-.01f,-.29f,-.20f),new Vector3(.14f,-.29f,-.20f),new Vector3(.22f,-.29f,.12f));
   PlusRoute("25",new Vector3(.14f,-.29f,-.22f),new Vector3(.32f,-.29f,-.10f));PlusRoute("100",new Vector3(.20f,-.29f,.05f),new Vector3(.34f,-.29f,.22f),c.Exit);
  }

  // E10 · 36 · Đu rồi luồn. Swing from the ivory start bank to the slick island; its tube drops into the walled pen.
  static void PlusE10(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.255f,-.21f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.11f,.12f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var start=NextPlinth(c,"Start bank",new Vector3(-.28f,-.23f,.10f),new Vector3(.24f,.14f,.40f),false);
   NextStairs(c,"Recovery stairs",new Vector3(-.30f,0,-.10f),Vector3.back,-.16f,.12f,3,.06f);
   var island=NextPlinth(c,"Island",new Vector3(.2575f,-.25f,.10f),new Vector3(.285f,.10f,.40f));
   NextPlinth(c,"Exit pen wall",new Vector3(.2575f,-.21f,-.125f),new Vector3(.285f,.18f,.02f));
   NextPlinth(c,"Exit pen wall",new Vector3(.125f,-.21f,-.21f),new Vector3(.02f,.18f,.17f));
   NextSwing(c,"A",new Vector3(0,.14f,.12f),.28f,40f,new Vector3(-.23f,-.14f,.12f),Top(start),new[]{Top(island)},new[]{new Vector3(.25f,-.18f,.12f)},floor);
   ChapterTube(c,"Pen tube",new Vector3(.25f,-.155f,-.02f),new Vector3(.25f,-.12f,-.06f),new Vector3(.25f,-.085f,-.10f),new Vector3(.25f,-.085f,-.15f),new Vector3(.26f,-.16f,-.20f),new Vector3(.27f,-.24f,-.23f),new Vector3(.28f,-.255f,-.25f));
   PlusStep(1,new Vector3(-.23f,-.075f,.12f));PlusStep(2,new Vector3(.25f,-.20f,.12f));PlusStep(3,new Vector3(.25f,-.155f,-.02f));PlusStep(4,new Vector3(.33f,-.29f,-.21f));
   PlusRoute("100",c.Spawn,new Vector3(-.23f,-.14f,.12f),new Vector3(0,-.14f,.12f),new Vector3(.25f,-.18f,.12f),new Vector3(.25f,-.18f,-.03f));
   PlusRoute("tube",new Vector3(.25f,-.155f,-.03f),new Vector3(.26f,-.25f,-.19f),new Vector3(.27f,-.255f,-.24f));PlusRoute("100",new Vector3(.30f,-.29f,-.23f),c.Exit);
  }

  // E11 · 37 · Giữ thang cho bạn. One half holds lever B (the lift has power only while it is held); the other rides up
  // and pulls C, which keeps the power on for good; the holder lets go, calls the lift, rides up, they merge.
  static void PlusE11(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.032f,.22f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(40,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(0,-.30f,-.20f));
   var high=Top(NextPlinth(c,"High platform",new Vector3(.24f,-.18f,.14f),new Vector3(.32f,.24f,.32f)));
   var b=ViewTask(c,"B",new Vector3(-.28f,-.277f,-.02f),Vector3.right,.07f,floor);b.HoldAtEnd=true;
   var rail=ExpansionRail(c,"Lift tray",new Vector3(-.022f,-.288f,.16f),Vector3.up,.22f,0,new Vector3(.20f,.02f,.20f),.03f,.004f,true,false);
   rail.GetComponent<VenomMovableProp>().Manipulable=false;TrimSideSlabs(rail);
   foreach(var face in rail.GetComponentsInChildren<VenomSurfacePatch>()){face.MotionFrame=rail.Body;if(face.Normal.y<.9f)face.Slippery=true;}
   var lift=rail.gameObject.AddComponent<COghePassengerLift>();lift.Rail=rail;lift.DeckSize=new Vector2(.20f,.20f);lift.DeckHeight=.01f;
   lift.Deck=rail.GetComponentsInChildren<VenomSurfacePatch>().First(p=>p.Normal.y>.9f);lift.ReturnWhenDisabled=true;
   lift.BoardPoint=new GameObject("Rider stance").transform;lift.BoardPoint.SetParent(rail.transform,false);lift.BoardPoint.localPosition=new Vector3(0,.028f,0);
   lift.Panel=MechanismVisual(rail.transform,"Lift panel",new Vector3(.06f,.012f,-.06f),new Vector3(.03f,.008f,.03f),metal,PrimitiveType.Cylinder);
   lift.CallPanels=new[]{MechanismVisual(c.Root,"Lower call panel",new Vector3(-.15f,-.296f,.215f),new Vector3(.03f,.008f,.03f),metal,PrimitiveType.Cylinder),
    MechanismVisual(c.Root,"Upper call panel",new Vector3(.13f,-.058f,.27f),new Vector3(.03f,.008f,.03f),metal,PrimitiveType.Cylinder)};
   foreach(float x in new[]{-.13f,.086f})MechanismVisual(c.Root,"Lift upright",new Vector3(x,-.02f,.275f),new Vector3(.012f,.56f,.012f),metal);
   var pin=PlusGate(c,"Lift enable pin",new Vector3(-.16f,-.27f,.28f),Vector3.up,.03f,new Vector3(.012f,.012f,.012f));lift.RequiredRail=pin;
   var latch=ViewTask(c,"C",new Vector3(.22f,-.037f,.14f),Vector3.right,.07f,high);latch.OneWay=true;
   var power=new GameObject("B holds or C latches the lift power",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();power.transform.SetParent(c.Root,false);
   power.Holds=new[]{b};power.Rails=new[]{latch.Rail};power.Any=true;power.Retain=false;power.Output=pin;
   NextTrace("B",new Vector3(-.21f,-.2992f,-.02f),new Vector3(-.18f,-.2992f,-.02f),new Vector3(-.18f,-.2992f,.28f),new Vector3(-.17f,-.2992f,.28f));
   PlusStep(1,new Vector3(0,-.27f,-.17f));PlusStep(2,new Vector3(-.28f,-.28f,-.07f));PlusStep(3,new Vector3(-.02f,-.26f,.16f));PlusStep(4,new Vector3(.25f,-.04f,.14f));PlusStep(5,new Vector3(-.15f,-.29f,.215f));PlusStep(6,new Vector3(.30f,-.05f,.22f));
   PlusGhost("50%",new Vector3(-.28f,-.28f,-.07f));PlusGhost("50%",new Vector3(-.02f,-.06f,.16f));
   PlusRoute("100",c.Spawn,new Vector3(0,-.27f,-.17f));PlusRoute("50a",new Vector3(-.14f,-.29f,-.18f),new Vector3(-.28f,-.29f,-.07f));
   PlusRoute("50b",new Vector3(.14f,-.29f,-.18f),new Vector3(.12f,-.29f,.02f),new Vector3(-.02f,-.26f,.16f));PlusRoute("50b",new Vector3(-.02f,-.06f,.16f),new Vector3(.18f,-.06f,.14f));
   PlusRoute("50a",new Vector3(-.28f,-.29f,-.02f),new Vector3(-.15f,-.29f,.10f),new Vector3(-.02f,-.26f,.16f));PlusRoute("100",new Vector3(.12f,-.06f,.16f),new Vector3(.30f,-.06f,.22f),c.Exit);
  }

  // ---- chapter 5 -------------------------------------------------------------------------------------------------
  // E12 · 41 · Khớp một bánh. A train with a gap: pull A (its carriage brings gear G into the gap), then stand on P and
  // the output gear draws the step out of the 6 cm exit platform. On P with the gap open only the motor gear turns.
  static void PlusE12(ExpansionContext c)
  {
   c.Exit=new Vector3(.20f,-.172f,.30f);c.Spawn=new Vector3(-.28f,-.25f,-.22f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   var drawer=NextDrawerPlatform(c,.20f,.12f,.17f,.26f);
   float y=PlusGearTable(c,"Gear table",new Vector3(-.14f,0,.08f),new Vector2(.26f,.10f));
   var pad=ExpansionPad(c,"P",new Vector3(-.32f,-.298f,-.16f),.009f,.10f);
   var g0=PlusGear(c.Root,"Motor gear",new Vector3(-.22f,y,.08f));var g2=PlusGear(c.Root,"Output gear",new Vector3(-.06f,y,.08f));
   PlusGearCarriage(c,"A",new Vector3(-.14f,-.277f,-.20f),Vector3.forward,.19f,floor,y,.09f,out var gear);
   MechanismVisual(c.Root,"Drawer drive shaft",new Vector3(.025f,-.266f,.08f),new Vector3(.13f,.006f,.012f),metal);
   PlusTrain(c,"G completes the train",pad,drawer,false,null,g0,gear,g2);
   NextTrace("A",new Vector3(-.32f,-.2992f,-.11f),new Vector3(-.32f,-.2992f,.08f),new Vector3(-.27f,-.2992f,.08f));
   PlusStep(1,new Vector3(-.14f,-.29f,-.24f));PlusStep(2,new Vector3(-.32f,-.29f,-.16f));PlusStep(3,new Vector3(.20f,-.27f,.07f));PlusStep(4,new Vector3(.20f,-.24f,.21f));
   PlusGhost("G",new Vector3(-.14f,y,.08f));
   PlusRoute("100",c.Spawn,new Vector3(-.14f,-.29f,-.26f),new Vector3(-.14f,-.29f,-.06f));PlusRoute("100",new Vector3(-.20f,-.29f,-.08f),new Vector3(-.32f,-.29f,-.16f));
   PlusRoute("100",new Vector3(-.26f,-.29f,-.20f),new Vector3(.20f,-.29f,-.04f),new Vector3(.20f,-.27f,.07f),new Vector3(.20f,-.24f,.21f),c.Exit);
  }

  // E13 · 42 · Hai ống, hai nửa. Each half takes its own tube up to a high alcove and stands on its pad; both loaded
  // slide the drawer step out of the exit platform, where it catches. Down the same tubes, merge, out.
  static void PlusE13(ExpansionContext c)
  {
   c.Exit=new Vector3(0,-.172f,.30f);c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(42,20,0);NextShell(c,.30f,true);
   NextQuantum(c,new Vector3(0,-.30f,-.20f));
   var bridge=NextDrawerPlatform(c,0,.12f,.17f,.26f);
   NextPlinth(c,"Left alcove",new Vector3(-.31f,-.20f,.20f),new Vector3(.18f,.20f,.20f));
   NextPlinth(c,"Right alcove",new Vector3(.31f,-.20f,.20f),new Vector3(.18f,.20f,.20f));
   var a1=ExpansionPad(c,"A1",new Vector3(-.345f,-.098f,.245f),.009f,.09f);var a2=ExpansionPad(c,"A2",new Vector3(.345f,-.098f,.245f),.009f,.09f);
   // Each mouth faces the middle of the room and the tube rises behind it: a low run past the mouth caught parts
   // walking back beside it.
   ChapterTube(c,"Left tube",new Vector3(-.16f,-.255f,-.12f),new Vector3(-.20f,-.25f,-.12f),new Vector3(-.23f,-.19f,-.10f),new Vector3(-.25f,-.12f,-.05f),new Vector3(-.25f,-.07f,.03f),new Vector3(-.25f,-.055f,.14f));
   ChapterTube(c,"Right tube",new Vector3(.16f,-.255f,-.12f),new Vector3(.20f,-.25f,-.12f),new Vector3(.23f,-.19f,-.10f),new Vector3(.25f,-.12f,-.05f),new Vector3(.25f,-.07f,.03f),new Vector3(.25f,-.055f,.14f));
   // Where each tube starts to climb it is 1–8 cm off the floor: a body walking past wedged under it (wander test,
   // 05/10/2026). Slick columns follow the tube's underside 6 mm below it, so no gap is left to wedge in and the bore
   // and mouths stay clear.
   var low=COgheTubeNetwork.SampleCurve(new[]{new Vector3(-.16f,-.255f,-.12f),new Vector3(-.20f,-.25f,-.12f),new Vector3(-.23f,-.19f,-.10f),new Vector3(-.25f,-.12f,-.05f),new Vector3(-.25f,-.07f,.03f),new Vector3(-.25f,-.055f,.14f)},12);
   foreach(float side in new[]{-1f,1f})for(float x=.19f;x<=.2501f;x+=.01f)
   {
    Vector3 under=low[0];foreach(var p in low)if(p.y<-.12f&&Mathf.Abs(-p.x-x)<Mathf.Abs(-under.x-x))under=p;
    float top=under.y-.038f-.006f,gap=under.y-.038f+.30f;if(gap<.012f||gap>.085f)continue;
    int first=c.Surfaces.Count;ViewBlock(c,(side<0?"Left":"Right")+" tube underfill",new Vector3(side*x,(top-.30f)*.5f,under.z),new Vector3(.0105f,top+.30f,.07f));
    for(int i=first;i<c.Surfaces.Count;i++)c.Surfaces[i].Slippery=true;
   }
   var latch=new GameObject("A1 A2 slide the step",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();latch.transform.SetParent(c.Root,false);latch.Inputs=new[]{a1,a2};latch.Output=bridge;
   PlusStep(1,new Vector3(0,-.27f,-.17f));PlusStep(2,new Vector3(-.16f,-.255f,-.12f));PlusStep(3,new Vector3(-.345f,-.10f,.245f));PlusStep(4,new Vector3(0,-.27f,.12f));PlusStep(5,new Vector3(0,-.29f,.02f));
   PlusGhost("50%",new Vector3(-.345f,-.08f,.245f));PlusGhost("50%",new Vector3(.345f,-.08f,.245f));
   PlusRoute("50a",new Vector3(-.14f,-.29f,-.18f),new Vector3(-.16f,-.255f,-.12f));PlusRoute("tube",new Vector3(-.16f,-.255f,-.12f),new Vector3(-.25f,-.18f,.0f),new Vector3(-.25f,-.055f,.14f),new Vector3(-.345f,-.10f,.245f));
   PlusRoute("50b",new Vector3(.14f,-.29f,-.18f),new Vector3(.16f,-.255f,-.12f));PlusRoute("tube",new Vector3(.16f,-.255f,-.12f),new Vector3(.25f,-.18f,.0f),new Vector3(.25f,-.055f,.14f),new Vector3(.345f,-.10f,.245f));
   PlusRoute("100",new Vector3(0,-.29f,.02f),new Vector3(0,-.27f,.12f),new Vector3(0,-.24f,.20f),c.Exit);
  }

  // The gear column shared by E14, E18 and B2: a vertical shaft at (-.05, z .06) beside the mid deck's front-right
  // corner. The floor layer runs front → back into it; the mid layer leaves it westward over the mid deck.
  static readonly Vector3 PlusColumn=new Vector3(-.05f,0,.06f);
  const float PlusFloorGear=-.262f,PlusMidGear=-.112f,PlusTopGear=-.022f;
  // Mid deck M (18 cm, slick sides) left of the column, with stairs along its front face rising toward the left wall
  // (6, 12, 18 cm). Treads and the right-facing risers are ivory; everything else is slick, so the stairs are climbed
  // only from their low end, where a slick gate stands until its rack lifts it.
  static COgheRailSlider PlusMidDeck(ExpansionContext c,out VenomSurfacePatch mid,bool gated=true)
  {
   mid=Top(NextPlinth(c,"Mid deck",new Vector3(-.245f,-.21f,.10f),new Vector3(.31f,.18f,.40f)));
   for(int i=0;i<3;i++)
   {
    float h=.06f*(i+1),x=-.25f-.06f*i;
    var faces=NextPlinth(c,"Mid deck stair",new Vector3(x,-.30f+h*.5f,-.16f),new Vector3(.06f,h,.12f));
    faces.First(f=>f.Normal.x>.9f).Slippery=false;
   }
   // Ivory faces on the deck front above stairs 1 and 2: a body that drops off the top stair onto a lower tread climbs
   // straight back up. Both sit behind the stairs, so the gate still guards the way up.
   Panel(c.Root,"Mid deck stair face",new Vector3(-.25f,-.18f,-.1004f),Vector3.back,new Vector2(.06f,.12f),stone,false,Vector2.zero,0,c.Surfaces);
   Panel(c.Root,"Mid deck stair face",new Vector3(-.31f,-.15f,-.1004f),Vector3.back,new Vector2(.06f,.06f),stone,false,Vector2.zero,0,c.Surfaces);
   if(!gated)return null;
   var gate=PlusGate(c,"Stair gate",new Vector3(-.21f,-.25f,-.16f),Vector3.up,.17f,new Vector3(.012f,.10f,.11f));gate.LatchAtEnd=true; // open, 11 cm over the first tread: at 12 cm travel a body on the tread wedged under it
   foreach(var f in gate.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;
   return gate;
  }

  // E14 · 44 · Hai tầng răng. Two layers on one shaft. Floor layer: carriage A closes its gap; on pad P it lifts the
  // gate of the stairs to the mid deck. Mid layer (on the deck): carriage B closes its gap; back on P the shaft carries
  // the turn up and the bridge rises between the mid deck and the exit ledge.
  static void PlusE14(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.075f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(.20f,-.25f,-.24f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var gate=PlusMidDeck(c,out var mid);
   NextPlinth(c,"Exit ledge",new Vector3(.27f,-.21f,.20f),new Vector3(.26f,.18f,.20f));
   var bridge=PlusRisingDeck(c,"Upper bridge",new Vector3(.025f,-.285f,.20f),new Vector3(.21f,.03f,.18f),.15f);
   float y0=PlusGearTable(c,"Floor gear table",new Vector3(-.04f,0,-.04f),new Vector2(.10f,.26f));
   var pad=ExpansionPad(c,"P",new Vector3(-.12f,-.298f,-.25f),.009f,.09f);
   var m=PlusGear(c.Root,"Motor gear",new Vector3(-.05f,y0,-.10f));var s0=PlusGear(c.Root,"Column gear, floor",new Vector3(-.05f,y0,.06f));
   PlusGearCarriage(c,"A",new Vector3(.13f,-.277f,-.02f),Vector3.left,.08f,floor,y0,.10f,out var ga);
   PlusShaft(c,"Gear column",PlusColumn,-.27f,PlusMidGear+.01f);
   var s1=PlusGear(c.Root,"Column gear, mid",new Vector3(-.05f,PlusMidGear,.06f));
   PlusGearCarriage(c,"B",new Vector3(-.31f,-.107f,.06f),Vector3.right,.08f,mid,PlusMidGear,.10f,out var gb);
   var u=PlusGear(c.Root,"Mid output gear",new Vector3(-.13f,PlusMidGear,.14f));
   PlusTrain(c,"A lifts the stair gate",pad,gate,false,null,m,ga,s0);
   PlusTrain(c,"B raises the bridge",pad,bridge,false,new[]{2},m,ga,s0,s1,gb,u);
   NextTrace("A",new Vector3(-.12f,-.2992f,-.205f),new Vector3(-.12f,-.2992f,-.10f),new Vector3(-.10f,-.2992f,-.10f));
   PlusStep(1,new Vector3(.13f,-.29f,-.07f));PlusStep(2,new Vector3(-.12f,-.29f,-.25f));PlusStep(3,new Vector3(-.37f,-.12f,-.16f));PlusStep(4,new Vector3(-.31f,-.12f,.01f));PlusStep(5,new Vector3(-.12f,-.28f,-.25f));PlusStep(6,new Vector3(.025f,-.12f,.20f));PlusStep(7,new Vector3(.30f,-.12f,.20f));
   PlusLabel("tầng 1",new Vector3(-.05f,-.26f,-.19f));PlusLabel("tầng 2",new Vector3(-.13f,-.10f,.22f));PlusLabel("cổng",new Vector3(-.21f,-.19f,-.24f));
   PlusRoute("100",c.Spawn,new Vector3(.13f,-.29f,-.07f),new Vector3(.05f,-.29f,-.07f));PlusRoute("100",new Vector3(.04f,-.29f,-.14f),new Vector3(-.12f,-.29f,-.25f));
   PlusRoute("100",new Vector3(-.16f,-.29f,-.18f),new Vector3(-.25f,-.24f,-.16f),new Vector3(-.37f,-.12f,-.16f),new Vector3(-.31f,-.12f,.01f));
   PlusRoute("100",new Vector3(-.20f,-.12f,.20f),new Vector3(.025f,-.12f,.20f),new Vector3(.30f,-.12f,.20f),c.Exit);
  }

  // E15 · 46 · Người chạy máy. A gear lift that runs only while pad P is loaded. One half stands on P (the lift rises
  // with the other aboard); up top the rider pulls C, which raises the last stair step; the driver leaves P, climbs.
  static void PlusE15(ExpansionContext c)=>PlusE15(c,.03f);
  // deck: the lift deck's thickness (its edge is the step onto it); its top always rises to the high deck (-.12).
  static void PlusE15(ExpansionContext c,float deck)
  {
   c.Exit=new Vector3(.40f,-.075f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(40,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(0,-.30f,-.20f));
   var high=Top(NextPlinth(c,"High deck",new Vector3(.25f,-.21f,.16f),new Vector3(.30f,.18f,.28f)));
   var lift=PlusRisingDeck(c,"Gear lift",new Vector3(-.01f,-.30f+deck*.5f,.16f),new Vector3(.20f,deck,.20f),.18f-deck);lift.LatchAtEnd=false;
   foreach(var f in lift.GetComponentsInChildren<VenomSurfacePatch>())f.MotionFrame=lift.Body;
   float y=PlusGearTable(c,"Gear table",new Vector3(-.25f,0,.10f),new Vector2(.12f,.26f));
   var pad=ExpansionPad(c,"P",new Vector3(-.30f,-.298f,-.20f),.009f,.10f);
   var g0=PlusGear(c.Root,"Motor gear",new Vector3(-.25f,y,.02f));var g1=PlusGear(c.Root,"Lift gear",new Vector3(-.25f,y,.10f));var g2=PlusGear(c.Root,"Lift gear",new Vector3(-.25f,y,.18f));
   var train=PlusTrain(c,"P runs the lift",pad,lift,false,null,g0,g1,g2);train.LatchOutput=false;train.ReturnWhenDisconnected=true;train.MotorTorque=.12f;train.MotorSpeed=3f; // drive ≈1 N at stall: a half body rides
   NextStairs(c,"Lower stair",new Vector3(.25f,0,-.04f),Vector3.back,-.24f,.12f,1,.06f);
   var top=PlusRisingDeck(c,"C top step",new Vector3(.25f,-.24f,-.01f),new Vector3(.12f,.12f,.056f),.06f,true); // 2 mm clear of the stair and the decktop.LatchAtEnd=true;
   var lever=ViewTask(c,"C",new Vector3(.20f,-.097f,.22f),Vector3.right,.07f,high);lever.OneWay=true;ViewLink(c,lever.Rail,top,false,null);
   NextTrace("A",new Vector3(-.30f,-.2992f,-.15f),new Vector3(-.30f,-.2992f,.02f),new Vector3(-.29f,-.2992f,.02f));
   PlusStep(1,new Vector3(0,-.27f,-.17f));PlusStep(2,new Vector3(-.01f,-.26f,.16f));PlusStep(3,new Vector3(-.30f,-.29f,-.20f));PlusStep(4,new Vector3(.20f,-.12f,.22f));PlusStep(5,new Vector3(.25f,-.12f,-.01f));PlusStep(6,new Vector3(.32f,-.12f,.20f));
   PlusGhost("50%",new Vector3(-.30f,-.28f,-.20f));PlusGhost("50%",new Vector3(-.01f,-.10f,.16f));
   PlusRoute("100",c.Spawn,new Vector3(0,-.27f,-.17f));PlusRoute("50a",new Vector3(-.14f,-.29f,-.18f),new Vector3(-.30f,-.29f,-.20f));
   PlusRoute("50b",new Vector3(.14f,-.29f,-.18f),new Vector3(.12f,-.29f,.02f),new Vector3(-.01f,-.26f,.16f));PlusRoute("50b",new Vector3(-.01f,-.12f,.16f),new Vector3(.20f,-.12f,.22f));
   PlusRoute("50a",new Vector3(-.28f,-.29f,-.24f),new Vector3(.25f,-.29f,-.24f),new Vector3(.25f,-.18f,-.07f),new Vector3(.25f,-.12f,.06f));PlusRoute("100",new Vector3(.30f,-.12f,.16f),c.Exit);
  }

  // E16 · 47 · Bàn xoay. The gear output is a turntable: its deck points front–back; on pad P it turns a quarter turn and
  // catches, so its deck joins the two ledges.
  static void PlusE16(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.155f,.10f);c.Outward=Vector3.right;c.Spawn=new Vector3(.12f,-.25f,-.24f);NextShell(c,.10f,true);
   NextPlinth(c,"Start ledge",new Vector3(-.30f,-.24f,.10f),new Vector3(.20f,.12f,.30f));
   NextStairs(c,"Start stairs",new Vector3(-.30f,0,-.05f),Vector3.back,-.18f,.12f,3,.06f);
   NextPlinth(c,"Exit ledge",new Vector3(.30f,-.24f,.10f),new Vector3(.20f,.12f,.30f));
   MechanismVisual(c.Root,"Turntable pedestal",new Vector3(0,-.255f,.10f),new Vector3(.10f,.045f,.10f),metal,PrimitiveType.Cylinder);
   var deck=Prop(c.Root,"Turntable deck",new Vector3(0,-.195f,.10f),new Vector3(.39f,.03f,.12f),false,plastic,c.Surfaces);c.Props.Add(deck);
   deck.transform.localRotation=Quaternion.Euler(0,90,0);deck.Body.isKinematic=true;
   foreach(var f in deck.GetComponentsInChildren<VenomSurfacePatch>()){f.MotionFrame=deck.Body;if(f.Normal.y<.9f)f.Slippery=true;}
   MechanismVisual(deck.transform,"Turntable disc",new Vector3(0,-.017f,0),new Vector3(.20f,.004f,.20f),metal,PrimitiveType.Cylinder);
   float y=PlusGearTable(c,"Gear table",new Vector3(.0f,0,-.18f),new Vector2(.20f,.10f));
   var pad=ExpansionPad(c,"P",new Vector3(-.16f,-.298f,-.24f),.009f,.10f);
   var g0=PlusGear(c.Root,"Motor gear",new Vector3(-.04f,y,-.18f));var g1=PlusGear(c.Root,"Turntable gear",new Vector3(.04f,y,-.18f));
   MechanismVisual(c.Root,"Turntable drive shaft",new Vector3(.04f,-.266f,-.06f),new Vector3(.012f,.006f,.24f),metal);
   var train=PlusTrain(c,"P turns the table",pad,null,false,null,g0,g1);
   var safe=new GameObject("Turntable clearance",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(0,-.15f,.10f);safe.Size=new Vector3(.34f,.08f,.34f);
   var table=new GameObject("Turntable",typeof(COgheTurntable)).GetComponent<COgheTurntable>();table.transform.SetParent(c.Root,false);table.Deck=deck.Body;table.Train=train;table.Clearance=safe;
   NextTrace("A",new Vector3(-.16f,-.2992f,-.19f),new Vector3(-.16f,-.2992f,-.18f),new Vector3(-.10f,-.2992f,-.18f));
   PlusStep(1,new Vector3(-.16f,-.29f,-.24f));PlusStep(2,new Vector3(-.30f,-.18f,.03f));PlusStep(3,new Vector3(0,-.18f,.10f));PlusStep(4,new Vector3(.30f,-.18f,.10f));
   PlusGhost("90°",new Vector3(0,-.18f,.10f));
   PlusRoute("100",c.Spawn,new Vector3(-.16f,-.29f,-.24f));PlusRoute("100",new Vector3(-.24f,-.29f,-.26f),new Vector3(-.30f,-.29f,-.22f),new Vector3(-.30f,-.18f,.02f),new Vector3(-.22f,-.18f,.10f),new Vector3(.30f,-.18f,.10f),c.Exit);
  }

  // E17 · 48 · Hai máy nối nhau. A machine that moves a machine: P1 drives train 1, whose rack pushes gear G's carriage
  // into train 2; P2 then drives train 2, which draws the step out of the exit platform. One body, two pads.
  static void PlusE17(ExpansionContext c)
  {
   c.Exit=new Vector3(.24f,-.172f,.30f);c.Spawn=new Vector3(-.28f,-.25f,-.22f);NextShell(c,.10f,true);
   var drawer=NextDrawerPlatform(c,.24f,.12f,.17f,.26f);
   float y=PlusGearTable(c,"Gear table",new Vector3(-.17f,0,.02f),new Vector2(.34f,.22f));
   var p1=ExpansionPad(c,"P1",new Vector3(-.32f,-.298f,-.20f),.009f,.09f);var p2=ExpansionPad(c,"P2",new Vector3(.02f,-.298f,-.20f),.009f,.09f);
   var a0=PlusGear(c.Root,"Train 1 motor",new Vector3(-.30f,y,.02f));var a1=PlusGear(c.Root,"Train 1 rack gear",new Vector3(-.22f,y,.02f));
   var push=ExpansionRail(c,"Train 1 rack",new Vector3(-.17f,-.264f,.08f),Vector3.right,.06f,0,new Vector3(.06f,.008f,.016f),.02f,.004f,false,false);push.GetComponent<VenomMovableProp>().Manipulable=false;push.LatchAtEnd=true;
   PlusTrain(c,"P1 pushes G",p1,push,false,null,a0,a1);
   var carriage=ExpansionRail(c,"G carriage",new Vector3(-.10f,-.263f,.02f),Vector3.right,.06f,0,new Vector3(.04f,.006f,.04f),.02f,.004f,false,false);carriage.GetComponent<VenomMovableProp>().Manipulable=false;carriage.LatchAtEnd=true;
   var g=PlusGear(carriage.transform,"G carried gear",new Vector3(0,y+.263f,0));ViewLink(c,push,carriage,false,null);
   var b0=PlusGear(c.Root,"Train 2 motor",new Vector3(-.04f,y,-.06f));var b1=PlusGear(c.Root,"Train 2 output",new Vector3(-.04f,y,.10f));
   MechanismVisual(c.Root,"Drawer drive shaft",new Vector3(.04f,-.266f,.10f),new Vector3(.16f,.006f,.012f),metal);
   PlusTrain(c,"P2 draws the step",p2,drawer,false,null,b0,g,b1);
   NextTrace("A",new Vector3(-.32f,-.2992f,-.155f),new Vector3(-.32f,-.2992f,.02f),new Vector3(-.35f,-.2992f,.02f));NextTrace("B",new Vector3(.02f,-.2992f,-.155f),new Vector3(.02f,-.2992f,-.06f),new Vector3(-.01f,-.2992f,-.06f));
   PlusStep(1,new Vector3(-.32f,-.29f,-.20f));PlusStep(2,new Vector3(.02f,-.29f,-.20f));PlusStep(3,new Vector3(.24f,-.27f,.07f));PlusStep(4,new Vector3(.24f,-.24f,.21f));
   PlusGhost("G",new Vector3(-.04f,y,.02f));PlusLabel("máy 1",new Vector3(-.26f,-.25f,-.05f));PlusLabel("máy 2",new Vector3(-.04f,-.25f,-.13f));
   PlusRoute("100",c.Spawn,new Vector3(-.32f,-.29f,-.20f));PlusRoute("100",new Vector3(-.26f,-.29f,-.24f),new Vector3(.02f,-.29f,-.20f));
   PlusRoute("100",new Vector3(.08f,-.29f,-.20f),new Vector3(.24f,-.29f,-.04f),new Vector3(.24f,-.27f,.07f),new Vector3(.24f,-.24f,.21f),c.Exit);
  }

  // The three-layer tower shared by E18 and B2: the mid deck with its gated stairs, and top deck U (27 cm) behind the
  // column. A 9 cm ivory block slides along the mid deck's back strip to U's face: the only step up to U.
  static void PlusTower(ExpansionContext c,out COgheRailSlider gate,out VenomSurfacePatch mid,out VenomSurfacePatch top,out COgheRailSlider step)
  {
   gate=PlusMidDeck(c,out mid);
   top=Top(NextPlinth(c,"Top deck",new Vector3(.155f,-.165f,.20f),new Vector3(.49f,.27f,.20f)));
   PlusShaft(c,"Gear column",PlusColumn,-.27f,PlusTopGear+.01f);
   // 2 mm clear of the mid deck, top flush with the top deck: flush on the deck its corner caught the deck's end slab.
   step=ExpansionRail(c,"Step to the top deck",new Vector3(-.29f,-.074f,.245f),Vector3.right,.15f,0,new Vector3(.10f,.088f,.11f),.05f,.01f,false,false);
   step.GetComponent<VenomMovableProp>().Manipulable=false;step.LatchAtEnd=true;
  }

  // E18 · 49 · Ba lớp răng. One half stays on motor pad P; the other closes each layer's gap in turn: A on the floor lifts
  // the stair gate, B on the mid deck slides the step to the top deck, C on the top deck opens the exit shutter.
  static void PlusE18(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,.02f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(.22f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(36,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(.22f,-.30f,-.20f),.025f,.13f);
   PlusTower(c,out var gate,out var mid,out var top,out var step);
   var shutter=PlusGate(c,"Exit shutter",new Vector3(.392f,.02f,.20f),Vector3.up,.10f,new Vector3(.012f,.10f,.10f));
   float y0=PlusGearTable(c,"Floor gear table",new Vector3(-.04f,0,-.04f),new Vector2(.10f,.26f));
   var pad=ExpansionPad(c,"P",new Vector3(.355f,-.298f,.035f),.009f,.08f); // behind Q on the right: the driver never crosses the fitter
   var m=PlusGear(c.Root,"Motor gear",new Vector3(-.05f,y0,-.10f));var s0=PlusGear(c.Root,"Column gear, floor",new Vector3(-.05f,y0,.06f));
   PlusGearCarriage(c,"A",new Vector3(.13f,-.277f,-.02f),Vector3.left,.08f,floor,y0,.10f,out var ga);
   var s1=PlusGear(c.Root,"Column gear, mid",new Vector3(-.05f,PlusMidGear,.06f));
   PlusGearCarriage(c,"B",new Vector3(-.31f,-.107f,.06f),Vector3.right,.08f,mid,PlusMidGear,.10f,out var gb);
   var u1=PlusGear(c.Root,"Mid output gear",new Vector3(-.13f,PlusMidGear,.14f));
   var s2=PlusGear(c.Root,"Column gear, top",new Vector3(-.05f,PlusTopGear,.06f));
   var cc=PlusGearCarriage(c,"C",new Vector3(.11f,-.017f,.14f),Vector3.left,.06f,top,PlusTopGear,.10f,out var gc);cc.StandOffset=new Vector3(0,0,.085f); // stand behind the (slick) carriage, not on it
   var u2=PlusGear(c.Root,"Shutter gear",new Vector3(-.05f,PlusTopGear,.22f));
   PlusTrain(c,"Floor layer",pad,gate,false,null,m,ga,s0);
   PlusTrain(c,"Mid layer",pad,step,false,new[]{2},m,ga,s0,s1,gb,u1);
   PlusTrain(c,"Top layer",pad,shutter,true,new[]{2,3},m,ga,s0,s1,s2,gc,u2);
   NextTrace("A",new Vector3(.355f,-.2992f,-.005f),new Vector3(.355f,-.2992f,-.025f),new Vector3(.02f,-.2992f,-.025f),new Vector3(.02f,-.2992f,-.10f),new Vector3(-.01f,-.2992f,-.10f));
   PlusStep(1,new Vector3(.22f,-.27f,-.17f));PlusStep(2,new Vector3(.355f,-.29f,.035f));PlusStep(3,new Vector3(.13f,-.29f,-.07f));PlusStep(4,new Vector3(-.31f,-.12f,.01f));PlusStep(5,new Vector3(.11f,-.03f,.19f));PlusStep(6,new Vector3(.30f,-.03f,.20f));
   PlusLabel("tầng 1",new Vector3(-.05f,-.26f,-.19f));PlusLabel("tầng 2",new Vector3(-.13f,-.10f,.02f));PlusLabel("tầng 3",new Vector3(.02f,-.01f,.22f));
   PlusGhost("50%",new Vector3(.355f,-.28f,.035f));
   PlusRoute("100",c.Spawn,new Vector3(.22f,-.27f,-.17f));PlusRoute("50a",new Vector3(.36f,-.29f,-.18f),new Vector3(.355f,-.29f,.035f));
   PlusRoute("50b",new Vector3(.09f,-.29f,-.18f),new Vector3(.13f,-.29f,-.07f));
   PlusRoute("50b",new Vector3(.05f,-.29f,-.14f),new Vector3(-.18f,-.29f,-.16f),new Vector3(-.25f,-.24f,-.16f),new Vector3(-.37f,-.12f,-.16f),new Vector3(-.31f,-.12f,.01f));
   PlusRoute("50b",new Vector3(-.24f,-.12f,.12f),new Vector3(-.14f,-.03f,.245f),new Vector3(.11f,-.03f,.19f));PlusRoute("100",new Vector3(.24f,-.03f,.20f),c.Exit);
  }

  // ---- BOSS · 50 · Tháp bánh răng ---------------------------------------------------------------------------------
  // Every gear idea at once, on the three-layer tower. P1 (floor) drives the floor layer: with A closed it runs the
  // held lift to the mid deck (only while P1 is loaded). On the mid deck B closes the mid layer and L lifts the stair
  // gate for the others; P2 (on the mid deck) turns the mid layer, whose rack pushes carriage C into the top layer and
  // slides the step to the top deck. The shutter's train runs through all three layers and needs P1 AND P2 loaded.
  // Four quarters: two drivers, a rider/fitter and a fourth who climbs to P2. Then everyone climbs and merges on top.
  static void PlusB2(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,.02f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(.22f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(34,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(.22f,-.30f,-.20f),.025f,.13f);
   PlusTower(c,out var gate,out var mid,out var top,out var step);
   var lift=PlusRisingDeck(c,"Held lift",new Vector3(-.15f,-.285f,-.155f),new Vector3(.10f,.03f,.09f),.15f);lift.LatchAtEnd=false;
   var shutter=PlusGate(c,"Exit shutter",new Vector3(.392f,.02f,.20f),Vector3.up,.10f,new Vector3(.012f,.10f,.10f));
   float y0=PlusGearTable(c,"Floor gear table",new Vector3(-.04f,0,-.04f),new Vector2(.10f,.26f));
   var p1=ExpansionPad(c,"P1",new Vector3(.355f,-.298f,.035f),.009f,.08f); // behind Q on the right
   var p2=ExpansionPad(c,"P2",new Vector3(-.25f,-.118f,.14f),.009f,.08f);
   var m1=PlusGear(c.Root,"Motor 1",new Vector3(-.05f,y0,-.10f));var s0=PlusGear(c.Root,"Column gear, floor",new Vector3(-.05f,y0,.06f));
   PlusGearCarriage(c,"A",new Vector3(.13f,-.277f,-.02f),Vector3.left,.08f,floor,y0,.10f,out var ga);
   var liftTrain=PlusTrain(c,"P1 runs the lift",p1,lift,false,null,m1,ga,s0);liftTrain.LatchOutput=false;liftTrain.ReturnWhenDisconnected=true;liftTrain.MotorTorque=.08f;liftTrain.MotorSpeed=2.5f;
   var s1=PlusGear(c.Root,"Column gear, mid",new Vector3(-.05f,PlusMidGear,.06f));
   var cb=PlusGearCarriage(c,"B",new Vector3(-.31f,-.107f,.06f),Vector3.right,.08f,mid,PlusMidGear,.10f,out var gb);
   ViewLink(c,cb.Rail,gate,false,null); // closing the mid layer also lifts the stair gate for the half below
   var m2=PlusGear(c.Root,"Motor 2",new Vector3(-.13f,PlusMidGear,-.02f));var p2g=PlusGear(c.Root,"Mid rack gear",new Vector3(-.13f,PlusMidGear,.14f));
   // The rack runs toward the front, clear of the step's lane behind it and of carriage B beside it.
   var push=ExpansionRail(c,"Mid layer rack",new Vector3(-.185f,-.114f,.12f),Vector3.back,.06f,0,new Vector3(.016f,.008f,.06f),.02f,.004f,false,false);push.GetComponent<VenomMovableProp>().Manipulable=false;push.LatchAtEnd=true;
   PlusTrain(c,"P2 turns the mid layer",p2,push,false,null,m2,gb,p2g);
   var carriage=ExpansionRail(c,"C carriage",new Vector3(.05f,-.023f,.14f),Vector3.left,.06f,0,new Vector3(.04f,.006f,.04f),.02f,.004f,false,false);carriage.GetComponent<VenomMovableProp>().Manipulable=false;carriage.LatchAtEnd=true;
   var gc=PlusGear(carriage.transform,"C carried gear",new Vector3(-.04f,PlusTopGear+.023f,0));ViewLink(c,push,carriage,false,null);ViewLink(c,push,step,false,null);
   var s2=PlusGear(c.Root,"Column gear, top",new Vector3(-.05f,PlusTopGear,.06f));var u=PlusGear(c.Root,"Shutter gear",new Vector3(-.05f,PlusTopGear,.22f));
   var final=PlusTrain(c,"P1 and P2 open the shutter",p1,shutter,true,new[]{2,3},m1,ga,s0,s1,s2,gc,u);final.ExtraClutches=new[]{p2};
   NextTrace("A",new Vector3(.355f,-.2992f,-.005f),new Vector3(.355f,-.2992f,-.025f),new Vector3(.02f,-.2992f,-.025f),new Vector3(.02f,-.2992f,-.10f),new Vector3(-.01f,-.2992f,-.10f));
   NextTrace("B",new Vector3(-.25f,-.1192f,.10f),new Vector3(-.25f,-.1192f,-.02f),new Vector3(-.18f,-.1192f,-.02f));
   PlusStep(1,new Vector3(.22f,-.27f,-.17f));PlusStep(2,new Vector3(.13f,-.29f,-.07f));PlusStep(3,new Vector3(.355f,-.29f,.035f));PlusStep(4,new Vector3(-.15f,-.26f,-.155f));
   PlusStep(5,new Vector3(-.31f,-.12f,.01f));PlusStep(6,new Vector3(-.25f,-.12f,.14f));PlusStep(7,new Vector3(-.14f,-.03f,.245f));PlusStep(8,new Vector3(.30f,-.03f,.20f));
   PlusLabel("tầng 1",new Vector3(-.05f,-.26f,-.19f));PlusLabel("tầng 2",new Vector3(-.13f,-.10f,.02f));PlusLabel("tầng 3",new Vector3(.02f,-.01f,.22f));
   PlusGhost("25%",new Vector3(.355f,-.28f,.035f));PlusGhost("50%",new Vector3(-.25f,-.10f,.14f));PlusGhost("25%",new Vector3(-.31f,-.10f,-.01f));
   PlusRoute("100",c.Spawn,new Vector3(.22f,-.27f,-.17f));
   PlusRoute("50b",new Vector3(.09f,-.29f,-.24f),new Vector3(-.17f,-.29f,-.25f));
   PlusRoute("25",new Vector3(.36f,-.29f,-.18f),new Vector3(.355f,-.29f,.035f));
   PlusRoute("50a",new Vector3(.09f,-.29f,-.18f),new Vector3(.13f,-.29f,-.07f),new Vector3(-.15f,-.26f,-.155f),new Vector3(-.15f,-.12f,-.155f),new Vector3(-.20f,-.12f,-.06f),new Vector3(-.31f,-.12f,-.01f));
   PlusRoute("50b",new Vector3(-.17f,-.29f,-.22f),new Vector3(-.25f,-.24f,-.16f),new Vector3(-.37f,-.12f,-.16f),new Vector3(-.25f,-.12f,.14f));
   PlusRoute("100",new Vector3(-.24f,-.12f,.22f),new Vector3(-.14f,-.03f,.245f),new Vector3(.30f,-.03f,.20f),c.Exit);
  }

  // ---- chapter 2, rebuilt (Mrk, 05/10/2026) -----------------------------------------------------------------------
  // N13 · 13 · Khối chặn lò xo. Level 12's crossing, but the low block B is on a spring: pulled into its bay it springs
  // back across the lane when let go. So Q first: one half holds B aside while the other pushes the tall block A to the
  // island; let go, B springs back beside A as the low step. Merge, climb B, A, the island.
  static void PlusN13(ExpansionContext c)
  {
   c.Exit=new Vector3(.27f,-.142f,.30f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit island",new Vector3(.27f,-.255f,.175f),new Vector3(.22f,.09f,.23f));
   // Q in the front-right corner, its trays 4 cm clear of the right pane (a tray against the glass stalled the split).
   NextQuantum(c,new Vector3(.18f,-.30f,-.18f),.025f,.13f);
   NextCrate(c,"A",new Vector3(-.20f,-.27f,.14f),Vector3.right,.296f,new Vector3(.12f,.06f,.14f),floor,new Vector3(0,0,-.078f),new Vector3(0,0,-.052f),.05f,.012f,0,true);
   // B rests across the lane; pulled toward the camera into its bay it is held there, and its spring returns it.
   var b=NextCrate(c,"B",new Vector3(-.028f,-.285f,.14f),Vector3.back,.28f,new Vector3(.12f,.03f,.14f),floor,new Vector3(0,0,-.078f),new Vector3(0,0,-.052f),.04f,.010f,0,true);
   b.HoldAtEnd=true;b.ReturnForce=.03f;b.Rail.LatchAtEnd=false;
   NextOutline(c,"A parking outline",new Vector3(.096f,-.2995f,.14f),new Vector2(.125f,.145f));
   NextOutline(c,"B spring bay outline",new Vector3(-.028f,-.2995f,-.14f),new Vector2(.125f,.145f));
   PlusStep(1,new Vector3(.22f,-.20f,-.20f));PlusStep(2,new Vector3(-.03f,-.29f,-.27f));PlusStep(3,new Vector3(-.20f,-.29f,.01f));PlusStep(4,new Vector3(-.03f,-.27f,.14f));PlusStep(5,new Vector3(.27f,-.21f,.20f));
  }

  // N18 · 18 · Nửa thân không đủ sức (Mrk's points 9 and 10). As E03, one half holds pad A to keep the spring door up
  // while the other goes through. The exit sits on a slick shelf; the only step is the 100% crate C, which a half
  // strains at and lets go of. So the worker latches the door with B, the holder follows, they merge, and the whole
  // body pushes C against the shelf, climbs it and leaves.
  static void PlusN18(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.163f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   var door=PlusGate(c,"A door",new Vector3(.05f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
   // Q stands 15 cm from the partition: a narrower gap between its casing and the wall wedged a wandering body.
   NextQuantum(c,new Vector3(-.21f,-.30f,-.02f),.025f,.13f);
   var a=ExpansionPad(c,"A",new Vector3(-.32f,-.298f,-.20f),.009f,.10f);
   // B in the open middle of the right room, clear of the partition and of the crate's path.
   var b=ViewTask(c,"B",new Vector3(.14f,-.277f,-.02f),Vector3.right,.05f,floor);b.OneWay=true;
   var safe=new GameObject("Door safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(.05f,-.25f,-.20f);safe.Size=new Vector3(.06f,.10f,.12f);
   var hold=new GameObject("A holds, B latches the door",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a};hold.Rails=new[]{b.Rail};hold.Output=door;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   // A 9 cm slick shelf under the exit: no body climbs it from the floor.
   NextPlinth(c,"Exit shelf",new Vector3(.31f,-.255f,.20f),new Vector3(.18f,.09f,.20f));
   // C needs all of COghe: it holds back .45 N; a whole body pushes .67 N at most, a half .34 N.
   var crate=NextCrate(c,"C",new Vector3(.31f,-.27f,-.12f),Vector3.forward,.156f,new Vector3(.14f,.06f,.12f),floor,new Vector3(0,0,-.068f),new Vector3(0,0,-.052f),.08f,.45f);
   crate.CompensateLoad=true;crate.LoadShare=1;
   TapLabel(c.Root,"100%",new Vector3(.31f,-.2985f,-.215f));
   NextOutline(c,"C shelf step outline",new Vector3(.31f,-.2995f,.036f),new Vector2(.145f,.125f));
   NextTrace("A",new Vector3(-.27f,-.2992f,-.20f),new Vector3(-.05f,-.2992f,-.27f),new Vector3(.03f,-.2992f,-.27f));
   NextTrace("B",new Vector3(.19f,-.2992f,.012f),new Vector3(.07f,-.2992f,.012f),new Vector3(.07f,-.2992f,-.14f));
   PlusStep(1,new Vector3(-.18f,-.27f,-.07f));PlusStep(2,new Vector3(-.32f,-.29f,-.20f));PlusStep(3,new Vector3(.31f,-.28f,-.25f));PlusStep(4,new Vector3(.16f,-.28f,-.07f));PlusStep(5,new Vector3(.31f,-.24f,.04f));
  }

  // N15 · 15 · Chuẩn bị trước khi đi. A low slick partition with a tube over it. The exit sits on a 6 cm slick shelf in
  // the far room; its step is pushed there through a 3.4 cm slot under the partition by handle A, which is only on this
  // side. Push first, then take the tube; a body that went first comes back through the same tube.
  static void PlusN15(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.194f,.14f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.25f,-.13f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   // The slot is 3.4 cm high and 1 cm wider than the step on each side (a flush fit jammed it).
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.23f,-.12f),new Vector3(.03f,.14f,.36f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.23f,.26f),new Vector3(.03f,.14f,.08f));
   NextPlinth(c,"Partition lintel",new Vector3(.05f,-.213f,.14f),new Vector3(.03f,.106f,.16f));
   NextPlinth(c,"Exit shelf",new Vector3(.33f,-.27f,.14f),new Vector3(.14f,.06f,.32f));
   // The step: 3 cm, it docks against the shelf as the stair between floor and shelf.
   Vector3 start=new Vector3(-.25f,-.285f,.14f),size=new Vector3(.12f,.03f,.14f);const float travel=.446f;
   var step=ExpansionRail(c,"A step",start,Vector3.right,travel,0,size,.04f,.010f,false,false);step.LatchAtEnd=true;step.GetComponent<VenomMovableProp>().Manipulable=false;
   int first=c.Surfaces.Count;ViewBlock(c,"A docked step",start+Vector3.right*travel,size);
   var docked=c.Surfaces.GetRange(first,c.Surfaces.Count-first).ToArray();foreach(var d in docked)d.gameObject.SetActive(false);
   var deck=step.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=step;deck.MovingSurfaces=step.GetComponentsInChildren<VenomSurfacePatch>(true);deck.DockedSurfaces=docked;
   var a=ViewTask(c,"A",new Vector3(-.32f,-.277f,0),Vector3.right,.14f,floor);
   ViewLink(c,a.Rail,step,false,null);
   // The far mouth turns to face the step, so a body leaves the tube already lined up with it (clear of the shelf corner).
   ChapterTube(c,"Transfer tube",new Vector3(-.20f,-.255f,-.20f),new Vector3(-.12f,-.252f,-.20f),new Vector3(-.05f,-.21f,-.20f),new Vector3(-.01f,-.13f,-.20f),new Vector3(.05f,-.10f,-.20f),
    new Vector3(.10f,-.13f,-.20f),new Vector3(.14f,-.21f,-.19f),new Vector3(.16f,-.252f,-.15f),new Vector3(.17f,-.255f,-.08f));
   NextOutline(c,"A step outline",new Vector3(.196f,-.2995f,.14f),new Vector2(.125f,.145f));
   PlusStep(1,new Vector3(-.25f,-.29f,-.05f));PlusStep(2,new Vector3(-.20f,-.27f,-.20f));PlusStep(3,new Vector3(.196f,-.27f,.14f));PlusStep(4,new Vector3(.33f,-.24f,.14f));
  }

  // N19 · 19 · Người giữ có việc thứ hai. N18 mirrored (Q's room on the right, the exit room on the left) so the two do
  // not look alike. One half holds pad A (door up) while the other goes through and latches the door with B: that
  // frees the holder for its second job. The step D is blocked by bolt C, whose spring handle is only in Q's room: the
  // freed holder holds C (the bolt pulls back) while the other half pushes D to the shelf. Let go, merge, climb, out.
  // ---- chapter 3, rebuilt (PLANS/COGHE_LEVEL_HOOK_PLAN.md 5.3; Mrk, 06/10/2026: "xây dựng Chương 3 theo kế hoạch") -----
  // N22 · 22 · Chất hàng trước (E04 with an order). Once cart A docks at the shelf, a pin on the shelf edge locks crate B on
  // the cart: slide B across the cart first, then pull the cart. Pulled in the wrong order, the pin shows the lock and B
  // refuses; pull the cart back to free it.
  static void PlusN22(ExpansionContext c)
  {
   PlusE04(c);
   var tasks=c.Root.GetComponentsInChildren<COgheTapRail>(true);
   var cart=tasks.First(t=>t.Label=="A");var crate=tasks.First(t=>t.Label=="B");
   crate.RequiredRail=cart.Rail;crate.RequiredEnd=false;   // B slides only while the cart is away from the shelf
   var pin=new Vector3(.125f,-.168f,.055f);
   crate.InterlockPin=MechanismVisual(c.Root,"B locking pin",pin,new Vector3(.014f,.040f,.014f),metal);
   TapLink(c.Root,"Lock linkage",new Vector3(.125f,-.296f,.02f),pin);
  }

  // N29 · 29 · Xếp tầng trên trước (E07 with an order). Block B rides block A; once A docks at the shelf a pin on the
  // shelf edge locks B. Climb A where it stands, push B to A's far end first, then pull A over and climb the stack.
  static void PlusN29(ExpansionContext c)
  {
   PlusE07(c);
   var tasks=c.Root.GetComponentsInChildren<COgheTapRail>(true);
   var low=tasks.First(t=>t.Label=="A");var high=tasks.First(t=>t.Label=="B");
   high.RequiredRail=low.Rail;high.RequiredEnd=false;
   // B is pushed while A still stands at its start: the pusher stands on A's own (moving) top, not on its docked copy.
   high.WorkingSurface=low.Rail.GetComponentsInChildren<VenomSurfacePatch>(true).First(f=>f.Normal.y>.9f);
   var pin=new Vector3(.125f,-.105f,.09f);
   high.InterlockPin=MechanismVisual(c.Root,"B locking pin",pin,new Vector3(.014f,.040f,.014f),metal);
   TapLink(c.Root,"Lock linkage",new Vector3(.125f,-.296f,.05f),pin);
  }

  // N23 · 23 · Đưa bến lại gần (level 19, deeper; plan 5.3): lever B moved from beside the start to the far front-right,
  // under the landing's side. The way up the stairs passes nowhere near it: a player swings first, falls short (the floor
  // and the stairs bring the body back), then finds B, brings the landing into the arc, and swings again.
  static void PlusN23(ExpansionContext c)
  {
   c.Exit=new Vector3(.215f,-.132f,.30f);c.Outward=Vector3.forward;c.Spawn=new Vector3(-.10f,-.25f,-.20f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var left=NextPlinth(c,"Start bank",new Vector3(-.28f,-.23f,.10f),new Vector3(.24f,.14f,.40f),false); // ivory: climbable back from the floor
   NextStairs(c,"Start stairs",new Vector3(-.30f,0,-.10f),Vector3.back,-.16f,.12f,3,.06f); // the fixed way up from the floor
   // A solid sliding block down to 8 mm above the floor (Mrk's playtest sweep, 09/10/2026): as a 3 cm slab riding 7 cm up,
   // a body walking past wedged in the gap under it. Its top (the landing) stays at −.20; its tall sides stay slick.
   var tray=ExpansionRail(c,"B landing tray",new Vector3(.295f,-.246f,.13f),Vector3.left,.08f,0,new Vector3(.20f,.092f,.32f),.05f,.01f,false,false);
   tray.GetComponent<VenomMovableProp>().Manipulable=false;tray.LatchAtEnd=true;TrimSideSlabs(tray);
   foreach(var face in tray.GetComponentsInChildren<VenomSurfacePatch>())if(face.Normal.y<.9f)face.Slippery=true;
   int first=c.Surfaces.Count;var dockedTop=Panel(c.Root,"B landing tray docked",new Vector3(.215f,-.20f,.13f),Vector3.up,new Vector2(.20f,.32f),stone,false,Vector2.zero,0,c.Surfaces);
   dockedTop.gameObject.SetActive(false);
   var deck=tray.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=tray;deck.MovingSurfaces=tray.GetComponentsInChildren<VenomSurfacePatch>(true);deck.DockedSurfaces=new[]{dockedTop};
   var b=ViewTask(c,"B",new Vector3(.32f,-.277f,-.21f),Vector3.left,.08f,floor);   // far front-right, under the landing's side: not on the way up
   ViewLink(c,b.Rail,tray,false,null);
   var landing=tray.GetComponentsInChildren<VenomSurfacePatch>(true).First(f=>f.Normal.y>.9f);
   NextSwing(c,"A",new Vector3(0,.14f,.12f),.28f,40f,new Vector3(-.23f,-.14f,.12f),Top(left),new[]{landing,dockedTop},new[]{new Vector3(.25f,-.18f,.13f),new Vector3(.25f,-.18f,.13f)},floor);
   NextTrace("B",new Vector3(.26f,-.2992f,-.17f),new Vector3(.26f,-.2992f,-.03f));
  }
  // N25 · 25 · Chưa đủ nặng (level 21's counterweight, then not enough of it). The crate is light and the plank's axle has a
  // return spring: on the tray the crate alone lifts the plank only part way. COghe climbs onto the loaded tray; with its
  // weight added the plank comes level and the pawl catches. Then out of the pit and across.
  static void PlusN25(ExpansionContext c)
  {
   PlusN25Room(c);
   c.Props.Last(p=>p.name=="A crate").Body.mass=PinCrateMass;
   var hinge=c.Props.Last(p=>p.name=="Seesaw plank").GetComponent<HingeJoint>();
   hinge.useSpring=true;hinge.spring=new JointSpring{spring=PlankSpring,damper=PlankSpring*.05f,targetPosition=0};
  }
  const float PinCrateMass=.02f,PlankSpring=.10f;
  // Level 21's room with a long tray (24 cm pit): the crate takes its near end, COghe fits on the far end.
  static void PlusN25Room(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.132f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.15f,-.18f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var deck=NextDeckWithPit(c,"Load deck",new Rect(-.40f,-.30f,.25f,.60f),-.20f,new Rect(-.34f,.02f,.12f,.24f));
   NextPlinth(c,"Exit platform",new Vector3(.275f,-.25f,.125f),new Vector3(.25f,.10f,.35f));
   NextStairs(c,"Gap recovery stairs",new Vector3(-.15f,0,-.26f),Vector3.right,-.20f,.08f,2,.06f);
   var tray=ExpansionRail(c,"Load tray",new Vector3(-.28f,-.215f,.14f),Vector3.down,.066f,0,new Vector3(.115f,.026f,.235f),.01f,.004f,true,false);
   tray.GetComponent<VenomMovableProp>().Manipulable=false;TrimSideSlabs(tray);foreach(var f in tray.GetComponentsInChildren<VenomSurfacePatch>())f.MotionFrame=tray.Body;
   NextLooseCrate(c,"A crate",new Vector3(-.28f,-.185f,-.10f),new Vector3(.09f,.03f,.09f),.04f,Vector3.back);
   // Plank: level pose authored, then tilted 22 degrees (far end down) by rotating about the axle.
   const float tilt=22f;var pivot=new Vector3(0,-.21f,.10f);
   var plank=Prop(c.Root,"Seesaw plank",pivot,new Vector3(.28f,.02f,.12f),false,plastic,c.Surfaces);c.Props.Add(plank);plank.Body.mass=.05f;plank.Body.centerOfMass=new Vector3(.04f,0,0);
   foreach(var f in plank.GetComponentsInChildren<VenomSurfacePatch>())if(f.Normal.y<.9f)f.Slippery=true;
   int first=c.Surfaces.Count;ViewBlock(c,"Seesaw plank level",pivot,new Vector3(.28f,.02f,.12f));var docked=c.Surfaces.GetRange(first,c.Surfaces.Count-first).ToArray();foreach(var d in docked){d.gameObject.SetActive(false);if(d.Normal.y<.9f)d.Slippery=true;}
   plank.transform.localRotation=Quaternion.Euler(0,0,-tilt);
   var hinge=plank.gameObject.AddComponent<HingeJoint>();hinge.connectedBody=c.Root.GetComponent<Rigidbody>();hinge.autoConfigureConnectedAnchor=false;hinge.anchor=Vector3.zero;hinge.connectedAnchor=pivot;hinge.axis=Vector3.forward;
   hinge.useLimits=true;hinge.limits=new JointLimits{min=-tilt-3,max=tilt+3};hinge.enableCollision=true;
   var anchor=new GameObject("Seesaw rope anchor").transform;anchor.SetParent(plank.transform,false);anchor.localPosition=new Vector3(.13f,.01f,0);
   ViewBlock(c,"Seesaw bearer",new Vector3(-.13f,-.26f,.10f),new Vector3(.02f,.08f,.10f));
   ViewBlock(c,"Seesaw rest",new Vector3(.13f,-.29f,.10f),new Vector3(.02f,.02f,.10f));
   MechanismVisual(c.Root,"Seesaw axle stand",new Vector3(0,-.255f,.10f),new Vector3(.03f,.09f,.03f),metal);
   var seesaw=new GameObject("A counterweight rope",typeof(COgheSeesawBridge)).GetComponent<COgheSeesawBridge>();seesaw.transform.SetParent(c.Root,false);
   seesaw.Plank=plank.Body;seesaw.Hinge=hinge;seesaw.Anchor=anchor;seesaw.Tray=tray;seesaw.LevelLocalRotation=Quaternion.identity;seesaw.RequiredLoad=c.Props.Last(p=>p.name=="A crate").Body;
   seesaw.MovingSurfaces=plank.GetComponentsInChildren<VenomSurfacePatch>(true);seesaw.DockedSurfaces=docked;
   seesaw.Guides=new[]{NextMarker(c,"Rope pulley over tray",new Vector3(-.28f,.12f,.14f)),NextMarker(c,"Rope pulley over plank",new Vector3(.13f,.12f,.10f))};
   foreach(var g in seesaw.Guides){var wheel=MechanismVisual(c.Root,"A pulley wheel",g.localPosition,new Vector3(.05f,.014f,.05f),metal,PrimitiveType.Cylinder);wheel.localRotation=Quaternion.Euler(90,0,0);}
   seesaw.Rope=seesaw.gameObject.AddComponent<LineRenderer>();seesaw.Rope.useWorldSpace=true;seesaw.Rope.startWidth=seesaw.Rope.endWidth=.0028f;seesaw.Rope.sharedMaterial=metal;
   seesaw.Pawl=MechanismVisual(c.Root,"Seesaw pawl",new Vector3(-.13f,-.215f,.155f),new Vector3(.01f,.012f,.01f),metal);
   NextOutline(c,"Load tray outline",new Vector3(-.28f,-.1995f,.14f),new Vector2(.125f,.245f));
   PlankBackGuard(c);
  }

  // N26 · 26 · Nhẹ quá không nghiêng. Two pads A, far apart, open the door into the seesaw room (a split: one half on each;
  // once both have pressed, it stays open). Past the axle half a body only makes the heavy plank creak; merged, the whole
  // body tips it down into the exit room.
  static void PlusN26(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.255f,.10f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.21f,-.25f,-.17f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Partition wall",new Vector3(.02f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.02f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   var door=PlusGate(c,"A door",new Vector3(.02f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
   NextQuantum(c,new Vector3(-.21f,-.30f,-.02f),.025f,.13f);   // as in N18: its trays 6 cm from the wall, 9 cm from the partition
   var front=ExpansionPad(c,"A",new Vector3(-.34f,-.298f,-.21f),.009f,.10f);
   var back=ExpansionPad(c,"A",new Vector3(-.34f,-.298f,.21f),.009f,.10f);
   // Once both pads have pressed, the door stays open (Retain): no safety zone is needed, it never closes on a body.
   var both=new GameObject("Both A pads open the door",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();both.transform.SetParent(c.Root,false);
   both.Inputs=new[]{front,back};both.Rails=new COgheRailSlider[0];both.Output=door;both.Any=false;both.Retain=true;
   // The low wall and a short heavy seesaw into the exit room. Its weight sits 3 cm on the foot side: it holds half a body
   // anywhere past the axle (.048 kg × 14 cm < .26 kg × 3 cm) but tips under a whole one past 8 cm.
   NextPlinth(c,"Low wall",new Vector3(.21f,-.26f,0),new Vector3(.03f,.08f,.59f));
   PlusSeesaw(c,"B",new Vector3(.21f,-.20f,.14f),.17f,27f);
   var plank=c.Props.Last(p=>p.name=="B seesaw plank");plank.Body.mass=.26f;
   NextTrace("A",new Vector3(-.34f,-.2992f,-.16f),new Vector3(-.34f,-.2992f,-.27f),new Vector3(0,-.2992f,-.27f));
   NextTrace("A",new Vector3(-.34f,-.2992f,.16f),new Vector3(-.34f,-.2992f,.27f),new Vector3(-.02f,-.2992f,.27f),new Vector3(-.02f,-.2992f,-.15f));
  }

  // ---- chapter 4 (plan 5.4: split to the right size) ------------------------------------------------------------
  // A half body measures .033–.042 kg on a pad, a quarter .021–.024 (only tissue near the pad counts).
  const float HalfLoad=.028f;
  // Load gauge in front of a pad (plan 3.6): one tile per quarter body the pad needs, lit by the load on it. A quarter on a
  // two-tile pad lights one tile of two. Tiles take the pad's circuit colour (NextSpatialArt).
  static void PadGauge(COgheTissueSensor pad,int quarters)
  {
   float z=-(pad.Size.y*.5f+.014f),pitch=.022f;var tiles=new Renderer[quarters];
   for(int i=0;i<quarters;i++)
   {
    float x=(i-(quarters-1)*.5f)*pitch;
    MechanismVisual(pad.transform,"Gauge slot",new Vector3(x,-.0012f,z),new Vector3(.017f,.0014f,.017f),plastic);
    tiles[i]=MechanismVisual(pad.transform,"Gauge light",new Vector3(x,-.0004f,z),new Vector3(.014f,.0012f,.014f),plastic).GetComponent<Renderer>();
   }
   pad.GaugeTiles=tiles;
  }
  static COgheLoadLatch PlusLatch(ExpansionContext c,string name,COgheTissueSensor[] pads,COgheRailSlider output,bool any,bool retain)
  {
   var latch=new GameObject(name,typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();latch.transform.SetParent(c.Root,false);
   latch.Inputs=pads;latch.Rails=new COgheRailSlider[0];latch.Output=output;latch.Any=any;latch.Retain=retain;return latch;
  }
  // E09's two rooms: partition at x .05, door A at the front. The shell cuts the exit hole, so the exit is set here.
  static COgheRailSlider PlusTwoRooms(ExpansionContext c,Vector3? exit=null,Vector3? outward=null)
  {
   c.Exit=exit??new Vector3(.40f,-.225f,.22f);c.Outward=outward??Vector3.right;c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   NextQuantum(c,new Vector3(-.18f,-.30f,-.02f),.025f,.14f);
   return PlusGate(c,"A door",new Vector3(.05f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
  }

  // N31 · 31 · Cân ở cửa (plan 5.4, teaches the quarter split and the gauge). Three pads must be loaded at once to open the
  // door for good: the one at the door needs half a body (two tiles), the two at the back a quarter each. A quarter on
  // the door pad lights one tile of two. Split, half at the door; split the other half, a quarter on each back pad.
  static void PlusN31(ExpansionContext c)
  {
   var door=PlusTwoRooms(c);c.Spawn=new Vector3(-.20f,-.25f,-.215f);
   var heavy=ExpansionPad(c,"A",new Vector3(-.06f,-.298f,-.21f),HalfLoad,.10f);PadGauge(heavy,2);
   var left=ExpansionPad(c,"A",new Vector3(-.34f,-.298f,.21f),.009f,.10f);PadGauge(left,1);
   var right=ExpansionPad(c,"A",new Vector3(-.06f,-.298f,.21f),.009f,.10f);PadGauge(right,1);
   PlusLatch(c,"Three pads open the door",new[]{heavy,left,right},door,false,true); // stays open: it never closes on a body
   NextTrace("A",new Vector3(-.01f,-.2992f,-.21f),new Vector3(.03f,-.2992f,-.21f));
   NextTrace("A",new Vector3(-.34f,-.2992f,.26f),new Vector3(-.34f,-.2992f,.28f),new Vector3(.02f,-.2992f,.28f),new Vector3(.02f,-.2992f,-.14f));
   NextTrace("A",new Vector3(-.01f,-.2992f,.21f),new Vector3(.02f,-.2992f,.21f));
  }

  // N32 · 32 · Nặng đi trước (plan 5.4, level 12's crossing). Low block B parks in the crossing and needs the whole body
  // (100 %); tall block A is bolted until pad A is loaded, so it needs a split: one part on the pad, one pushing. Push B
  // aside while whole, then split; merged again, push B back as the low step and climb B, A, the island. Split first and
  // the half strains at B and lets go; merging back fixes it.
  static void PlusN32(ExpansionContext c)
  {
   c.Exit=new Vector3(.27f,-.142f,.30f);c.Spawn=new Vector3(-.18f,-.25f,-.22f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit island",new Vector3(.27f,-.255f,.175f),new Vector3(.22f,.09f,.23f));
   NextQuantum(c,new Vector3(.25f,-.30f,-.17f),.025f,.12f);
   var a=NextCrate(c,"A",new Vector3(-.20f,-.27f,.14f),Vector3.right,.296f,new Vector3(.12f,.06f,.14f),floor,new Vector3(0,0,-.078f),new Vector3(0,0,-.052f),.05f,.012f,0,true);
   // B holds back .45 N: a whole body pushes .67 N at most, a half .34 N.
   var b=NextCrate(c,"B",new Vector3(-.028f,-.285f,-.14f),Vector3.forward,.28f,new Vector3(.12f,.03f,.14f),floor,new Vector3(0,0,-.078f),new Vector3(0,0,-.052f),.08f,.45f,.28f,true);
   b.CompensateLoad=true;b.LoadShare=1;
   TapLabel(c.Root,"100%",new Vector3(-.028f,-.2985f,-.235f));
   var pad=ExpansionPad(c,"A",new Vector3(-.33f,-.298f,-.03f),.009f,.09f);PadGauge(pad,1);
   var bolt=PlusGate(c,"A lock bolt",new Vector3(-.285f,-.285f,.14f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   PlusLatch(c,"Pad A draws the bolt",new[]{pad},bolt,false,false);
   a.RequiredRail=bolt;a.RequiredEnd=true;
   NextTrace("A",new Vector3(-.33f,-.2992f,.015f),new Vector3(-.33f,-.2992f,.14f),new Vector3(-.30f,-.2992f,.14f));
   NextOutline(c,"A parking outline",new Vector3(.096f,-.2995f,.14f),new Vector2(.125f,.145f));
   NextOutline(c,"B waiting bay outline",new Vector3(-.028f,-.2995f,-.14f),new Vector2(.125f,.145f));
  }

  // N34 · 34 · Bập bênh nâng bạn (plan 5.4). A balance lift: the tray by the slick high bank and the counter tray on the floor
  // hang from one beam. The heavier side goes down: half a body on the counter tray lifts a quarter to the bank, where it
  // pulls B (the exit step slides out). Half with half, or quarter with quarter, balances and nothing moves. The quarter
  // rides back down once the counterweight steps off; all merge and climb to the exit.
  static void PlusN34(ExpansionContext c)
  {
   c.Exit=new Vector3(.23f,-.172f,.30f);c.Spawn=new Vector3(-.30f,-.25f,-.20f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);
   var bank=Top(NextPlinth(c,"High bank",new Vector3(.27f,-.24f,-.16f),new Vector3(.26f,.12f,.28f)));
   var bridge=NextDrawerPlatform(c,.23f,.13f,.17f,.26f);
   NextQuantum(c,new Vector3(-.22f,-.30f,.12f),.025f,.13f);
   // B only moves its own handle (a quarter pulls .17 N at most); once it is home a latch drives the step out.
   var b=ViewTask(c,"B",new Vector3(.25f,-.157f,-.10f),Vector3.right,.05f,bank);b.OneWay=true;
   var step=PlusLatch(c,"B slides the exit step out",new COgheTissueSensor[0],bridge,false,true);step.Rails=new[]{b.Rail};
   // Lift tray: from the floor up to 2 mm above the bank top, 2 cm off its face (a flush tray's side panel rubbed the bank).
   var rail=ExpansionRail(c,"A lift tray",new Vector3(.06f,-.292f,-.16f),Vector3.up,.108f,0,new Vector3(.12f,.012f,.12f),.03f,.004f,false,false);
   rail.GetComponent<VenomMovableProp>().Manipulable=false;TrimSideSlabs(rail);
   // A 1.2 cm deck with ivory sides: a quarter could not board over a 2 cm slick edge. Raised, it hangs 11 cm up: no ladder.
   foreach(var f in rail.GetComponentsInChildren<VenomSurfacePatch>())f.MotionFrame=rail.Body;
   var rider=new GameObject("A lift rider weight",typeof(COgheTissueSensor)).GetComponent<COgheTissueSensor>();rider.transform.SetParent(rail.transform,false);rider.transform.localPosition=new Vector3(0,.007f,0);rider.Size=new Vector2(.12f,.12f);rider.Column=.10f;
   // A wide counter tray: a half standing off-centre still weighs whole (on a 12 cm tray it read .036 against .024: "balanced").
   var counter=ExpansionPad(c,"A",new Vector3(-.12f,-.298f,-.16f),.009f,.14f);counter.Column=.10f;
   var lift=new GameObject("A balance lift",typeof(COgheBalanceLift)).GetComponent<COgheBalanceLift>();lift.transform.SetParent(c.Root,false);
   lift.Rising=rail;lift.RisingLoad=rider;lift.CounterLoad=counter;lift.Margin=.008f;
   // Beam on a post in front of the trays; one rope from the counter tray over the beam down to the lift tray.
   var blue=COgheDayLabBuilder.Circuit(c.Game,"A").body;
   MechanismVisual(c.Root,"Balance post",new Vector3(-.03f,-.18f,-.25f),new Vector3(.012f,.24f,.012f),plastic);
   lift.Beam=new GameObject("Balance beam").transform;lift.Beam.SetParent(c.Root,false);lift.Beam.localPosition=new Vector3(-.03f,-.06f,-.25f);
   MechanismVisual(lift.Beam,"Balance beam bar",Vector3.zero,new Vector3(.21f,.008f,.014f),blue);
   Transform Mark(Transform parent,string name,Vector3 at){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=at;return t;}
   lift.CounterEnd=Mark(lift.Beam,"Beam counter end",new Vector3(-.09f,0,0));lift.RisingEnd=Mark(lift.Beam,"Beam lift end",new Vector3(.09f,0,0));
   lift.CounterAnchor=Mark(c.Root,"Counter tray rope",new Vector3(-.12f,-.296f,-.225f));lift.RisingAnchor=Mark(rail.transform,"Lift tray rope",new Vector3(0,.007f,-.065f));
   lift.Rope=lift.gameObject.AddComponent<LineRenderer>();lift.Rope.useWorldSpace=true;lift.Rope.startWidth=lift.Rope.endWidth=.0028f;lift.Rope.sharedMaterial=metal;
   NextOutline(c,"Counter tray outline",new Vector3(-.12f,-.2995f,-.16f),new Vector2(.15f,.15f));
   NextTrace("B",new Vector3(.33f,-.1792f,-.10f),new Vector3(.385f,-.1792f,-.10f),new Vector3(.385f,-.1792f,-.04f));
  }

  // N33 · 33 · Hai phần tư thành một nửa (E09 reworked, plan 5.4). Half a body on A holds the door up. Beyond it, B1 and B2
  // together lift the cover off pad C for good; C needs half a body as well and slides the pin in the door frame (the door
  // then stays up). Three parts are needed at once (50 + 25 + 25), and C needs the two quarters merged back into a half.
  static void PlusN33(ExpansionContext c)
  {
   var door=PlusTwoRooms(c);c.Spawn=new Vector3(-.18f,-.25f,-.215f);
   var heavy=ExpansionPad(c,"A",new Vector3(-.32f,-.298f,-.20f),HalfLoad,.10f);PadGauge(heavy,2);
   var b1=ExpansionPad(c,"B1",new Vector3(.14f,-.298f,.22f),.009f,.09f);PadGauge(b1,1);
   var b2=ExpansionPad(c,"B2",new Vector3(.32f,-.298f,-.22f),.009f,.09f);PadGauge(b2,1);
   var pc=ExpansionPad(c,"C",new Vector3(.32f,-.298f,.02f),HalfLoad,.10f);PadGauge(pc,2);
   var cover=PlusGate(c,"B cover",new Vector3(.32f,-.27f,.02f),Vector3.up,.13f,new Vector3(.11f,.05f,.11f));
   PlusLatch(c,"B1 and B2 lift the cover",new[]{b1,b2},cover,false,true);
   // The pin sits in the door frame on the near side, at the top of the doorway; C slides it under the raised door.
   var pin=PlusGate(c,"C door pin",new Vector3(.022f,-.115f,-.275f),Vector3.forward,.03f,new Vector3(.02f,.016f,.03f));
   PlusLatch(c,"C pins the door",new[]{pc},pin,false,true);
   var safe=new GameObject("Door safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(.05f,-.25f,-.20f);safe.Size=new Vector3(.06f,.10f,.12f);
   var hold=PlusLatch(c,"A holds, the pin keeps the door",new[]{heavy},door,true,false);hold.Rails=new[]{pin};hold.Clearance=safe;
   NextTrace("A",new Vector3(-.27f,-.2992f,-.20f),new Vector3(-.05f,-.2992f,-.27f),new Vector3(.03f,-.2992f,-.27f));
   NextTrace("B",new Vector3(.185f,-.2992f,.22f),new Vector3(.32f,-.2992f,.22f),new Vector3(.32f,-.2992f,.085f));
   NextTrace("B",new Vector3(.32f,-.2992f,-.175f),new Vector3(.32f,-.2992f,-.045f));
   NextTrace("C",new Vector3(.265f,-.2992f,.02f),new Vector3(.09f,-.2992f,.02f),new Vector3(.09f,-.2992f,-.275f),new Vector3(.07f,-.2992f,-.275f));
  }

  // N35 · 35 · Ba phần tư (plan 5.4). Next 25's room with one pad: a quarter on A1 draws the bolt from heavy block B, and B
  // needs three quarters (75 %): half a body strains and lets go. Q only halves, so split, split one half again, a quarter
  // on A1, and the other quarter joins the half to push B into its socket. Then all merge and climb B.
  static void PlusN35(ExpansionContext c)
  {
   c.Exit=new Vector3(.31f,-.142f,.30f);c.Spawn=new Vector3(-.185f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(-.185f,-.30f,-.18f),.025f,.13f);
   NextPlinth(c,"Exit platform",new Vector3(.3125f,-.255f,.15f),new Vector3(.175f,.09f,.30f));
   NextPlinth(c,"Fixed low step",new Vector3(.17f,-.285f,.0125f),new Vector3(.10f,.03f,.085f));
   // .42 N of dry friction: half a body pushes .34 N at most, three quarters .50 N.
   var b=NextCrate(c,"B",new Vector3(-.02f,-.27f,.12f),Vector3.right,.19f,new Vector3(.10f,.06f,.12f),floor,new Vector3(-.058f,-.015f,0),new Vector3(-.052f,0,0),.08f,.42f,0,true);
   b.Handle.localRotation=Quaternion.Euler(0,90,0);b.StallSeconds=4;b.CompensateLoad=true;b.LoadShare=.75f;
   TapLabel(c.Root,"75%",new Vector3(-.02f,-.2985f,.035f));
   var a1=ExpansionPad(c,"A1",new Vector3(-.31f,-.298f,.16f),.009f,.09f);PadGauge(a1,1);
   var bolt=ViewGate(c,"A lock bolt",new Vector3(.05f,-.285f,.195f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   PlusLatch(c,"A1 draws the bolt",new[]{a1},bolt,false,false);
   b.RequiredRail=bolt;b.RequiredEnd=true;
   NextTrace("A",new Vector3(-.31f,-.2992f,.21f),new Vector3(-.31f,-.2992f,.25f),new Vector3(.05f,-.2992f,.25f),new Vector3(.05f,-.2992f,.205f));
   NextOutline(c,"B socket",new Vector3(.17f,-.2995f,.12f),new Vector2(.105f,.125f));
  }

  // N40 · 40 · BOSS Cân ba phần tư (plan 5.4). The scale in the start room accepts exactly three quarters: a whole body tips
  // its pan down (too heavy, a red tile lights), half a body leaves the counterweight down. Its axle pin is drawn only while
  // a part stands on pad B in the side room, behind door A (half a body on A holds it; from inside, any part on the pad by
  // the door). The scale slides out the step of the exit platform in the side room for good, and with it opens the door. Split; half holds A; split
  // the other half; a quarter goes to B; the half leaves A for the pan and the other quarter joins it. Holder pads stand off
  // every way a part walks: parts that touch fuse.
  static void PlusN40(ExpansionContext c)
  {
   var door=PlusTwoRooms(c,new Vector3(.23f,-.172f,.30f),Vector3.forward);c.Spawn=new Vector3(-.18f,-.25f,-.215f);var coral=COgheDayLabBuilder.Circuit(c.Game,"B").body;
   var bridge=NextDrawerPlatform(c,.23f,.13f,.17f,.26f);
   var heavy=ExpansionPad(c,"A",new Vector3(-.32f,-.298f,-.20f),HalfLoad,.10f);PadGauge(heavy,2);
   // The pan weighs the whole part (a 10 cm column): exactly 24 of 32 particles (.072 kg) sits in the window.
   var scale=ExpansionPad(c,"B",new Vector3(-.30f,-.298f,.20f),.060f,.12f);scale.Column=.10f;scale.MaxLoad=.084f;PadGauge(scale,3);
   scale.Overload=MechanismVisual(scale.transform,"Gauge overload",new Vector3(.044f,-.0004f,-.074f),new Vector3(.014f,.0012f,.014f),SpatialMaterial("Scale overload red",new Color(.86f,.24f,.20f))).GetComponent<Renderer>();
   MechanismVisual(scale.transform,"Gauge slot",new Vector3(.044f,-.0012f,-.074f),new Vector3(.017f,.0014f,.017f),plastic);
   TapLabel(c.Root,"75%",new Vector3(-.30f,-.2985f,.10f));
   // Beam behind the pan: pan end left, the three-quarter counterweight right.
   MechanismVisual(c.Root,"Scale post",new Vector3(-.30f,-.24f,.28f),new Vector3(.012f,.12f,.012f),plastic);
   var beam=new GameObject("Scale beam").transform;beam.SetParent(c.Root,false);beam.localPosition=new Vector3(-.30f,-.18f,.28f);scale.Beam=beam;
   MechanismVisual(beam,"Scale beam bar",Vector3.zero,new Vector3(.16f,.008f,.014f),coral);
   MechanismVisual(beam,"Scale pan hanger",new Vector3(-.075f,-.014f,0),new Vector3(.02f,.02f,.02f),plastic);
   MechanismVisual(beam,"Scale counterweight",new Vector3(.072f,-.016f,0),new Vector3(.03f,.026f,.024f),coral);
   var pin=MechanismVisual(c.Root,"B axle pin",new Vector3(-.30f,-.18f,.265f),new Vector3(.012f,.012f,.012f),coral,PrimitiveType.Cylinder);pin.localRotation=Quaternion.Euler(90,0,0);
   // Side room: pad B (any part) draws the axle pin; the pad inside the door lets any part out again.
   var axle=ExpansionPad(c,"B",new Vector3(.32f,-.298f,-.22f),.009f,.09f);PadGauge(axle,1);
   var inside=ExpansionPad(c,"A",new Vector3(.12f,-.298f,-.07f),.009f,.09f);PadGauge(inside,1);
   var weigh=PlusLatch(c,"Exactly three quarters, axle pin drawn",new[]{scale,axle},bridge,false,true);weigh.Pins=new Transform[]{null,pin};
   var safe=new GameObject("Door safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(.05f,-.25f,-.20f);safe.Size=new Vector3(.06f,.10f,.12f);
   var hold=PlusLatch(c,"A holds the door; the open exit keeps it",new[]{heavy,inside},door,true,false);hold.Rails=new[]{bridge};hold.Clearance=safe;
   NextTrace("A",new Vector3(-.27f,-.2992f,-.20f),new Vector3(-.05f,-.2992f,-.27f),new Vector3(.03f,-.2992f,-.27f));
   NextTrace("A",new Vector3(.075f,-.2992f,-.07f),new Vector3(.068f,-.2992f,-.07f));
   NextTrace("B",new Vector3(.32f,-.2992f,-.175f),new Vector3(.32f,-.2992f,.02f),new Vector3(.23f,-.2992f,.02f));
   NextTrace("B",new Vector3(-.30f,-.2992f,.26f),new Vector3(-.30f,-.2992f,.275f));
  }

  // ---- chapter 5 rebuilt (plan 5.5: gears) ---------------------------------------------------------------------
  // N44 · 44 · Bánh đệm đổi chiều. The motor gear and the step's output gear stand 16 cm apart. Straight gear A (already in
  // the gap) makes a three-wheel train: on P the step runs back into the platform. The idler pair B (two gears in a zigzag)
  // makes four wheels and the step runs out. Both in at once jam. So: see it run back, draw A out, push B in, P, climb.
  static void PlusN44(ExpansionContext c)
  {
   c.Exit=new Vector3(.20f,-.172f,.30f);c.Spawn=new Vector3(-.28f,-.25f,-.22f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   var drawer=NextDrawerPlatform(c,.20f,.12f,.17f,.26f);
   // The step starts 3 cm out (not yet a step): the wrong way it visibly runs back in.
   drawer.InitialTravel=.03f;drawer.transform.localPosition=drawer.Start+drawer.Axis.normalized*.03f;
   // The table carries the gear row only: B's carriage stops just behind it. 5 cm tall: a 3 cm slick table is a step, and
   // the way to B ran over it into the gears.
   float y=PlusGearTable(c,"Gear table",new Vector3(-.18f,0,0),new Vector2(.26f,.10f),-.25f);
   var pad=ExpansionPad(c,"P",new Vector3(-.32f,-.298f,-.16f),.009f,.10f);
   var m=PlusGear(c.Root,"Motor gear",new Vector3(-.26f,y,0));var o=PlusGear(c.Root,"Output gear",new Vector3(-.10f,y,0));
   var shaft=new GameObject("Motor gear shaft").transform;shaft.SetParent(c.Root,false);shaft.localPosition=m.localPosition;shaft.localRotation=m.localRotation;
   MechanismVisual(c.Root,"Drawer drive shaft",new Vector3(-.015f,-.266f,0),new Vector3(.17f,.006f,.012f),metal);
   // A: one gear straight into the gap, in from the start.
   var a=PlusGearCarriage(c,"A",new Vector3(-.18f,-.277f,-.19f),Vector3.forward,.10f,floor,y,.09f,out var ga);
   a.Rail.InitialTravel=a.Rail.Travel;a.Rail.transform.localPosition=a.Rail.Start+a.Rail.Axis.normalized*a.Rail.Travel;
   // B: the idler pair, in from the back; its gears land 6.93 cm behind the row, 8 cm from each other and from both ends.
   // Pushed from behind its block: in front of it is the gear table.
   var b=ViewTask(c,"B",new Vector3(-.18f,-.277f,.17f),Vector3.back,.07f,floor);b.StandOffset=new Vector3(0,0,.095f);
   const float zig=.0693f;float arm=.17f-.07f-zig;
   var gb1=PlusGear(b.Rail.transform,"B idler gear",new Vector3(-.04f,y+.277f,-arm));var gb2=PlusGear(b.Rail.transform,"B idler gear",new Vector3(.04f,y+.277f,-arm));
   MechanismVisual(b.Rail.transform,"B gear arm",new Vector3(0,y+.277f-.004f,-arm*.5f),new Vector3(.088f,.006f,arm+.008f),metal);
   foreach(var f in b.Rail.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;b.Rail.LatchAtEnd=true;
   var straight=PlusTrain(c,"A runs the step back",pad,drawer,false,null,m,ga,o);
   var idler=PlusTrain(c,"B runs the step out",pad,drawer,false,null,shaft,gb1,gb2,o);
   foreach(var t in new[]{straight,idler}){t.Reversible=true;t.LatchOutput=false;}
   NextTrace("P",new Vector3(-.32f,-.2992f,-.11f),new Vector3(-.32f,-.2992f,0),new Vector3(-.30f,-.2992f,0));
  }

  // N45 · 45 · Ai chạy máy (E15 reworked, plan 5.5). The motor pulls with twice the driver's weight (weighed whole, shown in
  // quarters on P's gauge) and a spring holds the lift back. Half drives half: the lift stalls half way. Half drives a
  // quarter: three quarters of the way. Three quarters drive a quarter: to the top. Then the rider raises the top step
  // with C and the driver climbs.
  static void PlusN45(ExpansionContext c)
  {
   PlusE15(c,.012f); // a 1.2 cm deck: a quarter could not board over a 3 cm edge (as in N34)
   var pad=c.Root.GetComponentsInChildren<COgheTissueSensor>().First(p=>p.name.StartsWith("P "));pad.Column=.10f;pad.GaugeFull=.096f;PadGauge(pad,4);
   var train=c.Root.GetComponentsInChildren<COgheGearTrain>().First();
   // At the top (16.8 cm) the spring holds back 1.05 N: 2 × .072 kg × g = 1.41 N lifts a quarter (.24 N) past it.
   train.ForcePerLoad=2*9.81f;train.RackSpring=6.28f;
   // Ivory deck sides, as N34's tray. Raised, the deck hangs clear of the floor.
   foreach(var f in train.Rack.GetComponentsInChildren<VenomSurfacePatch>())if(f.Normal.y<.9f)f.Slippery=false;
  }

  // ---- chapter 5, part 2 ------------------------------------------------------------------------------------------
  // A lamp beside a train: dark until every link meshes, then lit (plan 5.5: "đèn khớp").
  static void PlusMeshLamp(ExpansionContext c,COgheGearTrain train,Vector3 at)
  {
   MechanismVisual(c.Root,train.name+" lamp base",at+Vector3.down*.005f,new Vector3(.024f,.003f,.024f),metal,PrimitiveType.Cylinder);
   train.MeshLamp=MechanismVisual(c.Root,train.name+" mesh lamp",at,new Vector3(.016f,.008f,.016f),plastic,PrimitiveType.Sphere).GetComponent<Renderer>();
   train.MeshLampOn=SpatialMaterial("Mesh lamp lit",new Color(.40f,.86f,.58f));train.MeshLampOff=SpatialMaterial("Mesh lamp dark",new Color(.34f,.35f,.37f));
   train.MeshLamp.sharedMaterial=train.MeshLampOff;
  }
  // The empty slot a gear goes into: a thin ring on the table (plan 5.5: "vòng khe trống").
  static void PlusSlotRing(ExpansionContext c,string name,Vector3 at,float radius)
  {
   var t=new GameObject(name).transform;t.SetParent(c.Root,false);t.localPosition=at;t.localRotation=Quaternion.Euler(-90,0,0);
   Ring(t,Vector2.zero,radius,.0025f,metal);
  }
  static Transform PlusSmallGear(Transform parent,string name,Vector3 local){var g=MeshingStationWheel(parent,name,local,.03f,14,0);g.localRotation=Quaternion.Euler(90,0,0);return g;}
  // A pad that weighs the whole part on it and shows it in quarters (N45's motor pad).
  static COgheTissueSensor PlusWeighPad(ExpansionContext c,string name,Vector3 at)
  {
   var pad=ExpansionPad(c,name,at,.009f,.10f);pad.Column=.10f;pad.GaugeFull=.096f;PadGauge(pad,4);return pad;
  }


  // Bakery levels (Mrk, 10/10/2026: "no glass, a flat surface; where COghe climbs, the face of a block; win at a cherry"):
  // the floor, a slick rim COghe cannot climb (6 cm, over its 5 cm), a taller back wall where a platform meets the back edge,
  // no roof. COgheBakeryDress gives it the bakery look after the build.
  // The rim is the level's outer boundary (ExteriorGlass: the plaque and the "Slippery" tap mark use it).
  static void BakeryShell(ExpansionContext c)
  {
   Panel(c.Root,"Laboratory floor",new Vector3(0,-.30f,0),Vector3.up,new Vector2(.8f,.6f),stone,false,Vector2.zero,0,c.Surfaces);
   const float rim=.06f;
   var walls=new[]{(new Vector3(0,0,-.3f),Vector3.forward,.8f),(new Vector3(0,0,.3f),Vector3.back,.8f),
                   (new Vector3(-.4f,0,0),Vector3.right,.6f),(new Vector3(.4f,0,0),Vector3.left,.6f)};
   foreach(var (pos,normal,length) in walls)
   {
    var p=Panel(c.Root,"Bakery rim",pos+Vector3.up*(-.30f+rim*.5f),normal,new Vector2(length,rim),stone,false,Vector2.zero,0,c.Surfaces);
    p.Slippery=true;p.Selectable=true;p.ExteriorGlass=true;
   }
  }
  // Where a platform meets the back edge, the rim rises 6 cm over the platform so COghe cannot step off it.
  static void BakeryBackrest(ExpansionContext c,float x0,float x1,float top)
  {
   var p=Panel(c.Root,"Bakery backrest",new Vector3((x0+x1)*.5f,(top-.30f)*.5f,.30f),Vector3.back,new Vector2(x1-x0,top+.30f),stone,false,Vector2.zero,0,c.Surfaces);
   p.Slippery=true;p.Selectable=true;p.ExteriorGlass=true;
  }
  // The goal: a cherry standing at c.Exit (its pivot on the surface). A placeholder of spheres until the dress puts the mesh in.
  static void BakeryCherry(ExpansionContext c)
  {
   var goal=new GameObject("Cherry goal",typeof(COgheCherryGoal)).GetComponent<COgheCherryGoal>();goal.transform.SetParent(c.Root,false);
   var cherry=new GameObject("Cherry").transform;cherry.SetParent(goal.transform,false);cherry.localPosition=c.Exit;goal.Cherry=cherry;
   var visual=new GameObject("Cherry visual").transform;visual.SetParent(cherry,false);goal.Visual=visual;   // only this bobs
   var red=SpatialMaterial("Cherry red",new Color(.86f,.10f,.16f));
   foreach(var (o,d) in new[]{(new Vector3(-.014f,.017f,0),.034f),(new Vector3(.016f,.016f,.006f),.032f)})
   {var b=GameObject.CreatePrimitive(PrimitiveType.Sphere);Object.DestroyImmediate(b.GetComponent<Collider>());b.name="Cherry placeholder";b.transform.SetParent(visual,false);b.transform.localPosition=o;b.transform.localScale=Vector3.one*d;b.GetComponent<MeshRenderer>().sharedMaterial=red;}
  }
  // N41 · 41 · Bánh răng đầu tiên (E08 and E12 merged, plan 5.5: teaches gears). On P with the gap open only the motor gear
  // turns; the gap is a ring on the table and the lamp beside the train stays dark. Pull A: its carriage brings G into the
  // ring and the lamp lights; on P the output gear draws the step out of the exit platform.
  static void PlusN41(ExpansionContext c)
  {
   // Bakery trial (Mrk, 10/10/2026, for review): no glass box and a cherry instead of the hole. The cherry stands on the
   // exit platform 11 cm behind its front edge, so only a body on the platform reaches it; the step is still the way up.
   c.Exit=new Vector3(.20f,-.24f,.23f);c.Outward=Vector3.down;c.Spawn=new Vector3(-.28f,-.25f,-.22f);BakeryShell(c);var floor=c.Surfaces[0];
   BakeryBackrest(c,.07f,.33f,-.18f);BakeryCherry(c);
   c.Definition.CameraEuler=new Vector3(54,-16,0);   // steeper: the plate lip in front must not hide COghe or P (Codex review)
   var drawer=NextDrawerPlatform(c,.20f,.12f,.17f,.26f,true);
   // E12's table, 4 cm longer on the right for the lamp.
   float y=PlusGearTable(c,"Gear table",new Vector3(-.12f,0,.08f),new Vector2(.30f,.10f));
   var pad=ExpansionPad(c,"P",new Vector3(-.32f,-.298f,-.16f),.009f,.10f);
   var g0=PlusGear(c.Root,"Motor gear",new Vector3(-.22f,y,.08f));var g2=PlusGear(c.Root,"Output gear",new Vector3(-.06f,y,.08f));
   PlusGearCarriage(c,"A",new Vector3(-.14f,-.277f,-.20f),Vector3.forward,.19f,floor,y,.09f,out var gear);
   MechanismVisual(c.Root,"Drawer drive shaft",new Vector3(.085f,-.266f,.08f),new Vector3(.11f,.006f,.012f),metal);
   var train=PlusTrain(c,"G completes the train",pad,drawer,false,null,g0,gear,g2);
   PlusSlotRing(c,"G slot ring",new Vector3(-.14f,y-.0075f,.08f),.047f);
   PlusMeshLamp(c,train,new Vector3(.005f,y,.05f));
   NextTrace("A",new Vector3(-.32f,-.2992f,-.11f),new Vector3(-.32f,-.2992f,.08f),new Vector3(-.27f,-.2992f,.08f));
   PlusStep(1,new Vector3(-.32f,-.29f,-.16f));PlusStep(2,new Vector3(-.14f,-.29f,-.24f));PlusStep(3,new Vector3(-.32f,-.29f,-.16f));PlusStep(4,new Vector3(.20f,-.27f,.07f));PlusStep(5,new Vector3(.20f,-.24f,.21f));
   PlusGhost("G",new Vector3(-.14f,y,.08f));
  }

  // N42 · 42 · Hai xe chéo nhau (plan 5.5: level 12's crossing with gear carts). The train runs motor M → A's slot → B's slot
  // → output O in an L. Cart A comes in from the front: its small gear (3 cm) passes through B's slot on the way to its own,
  // on a low arm that runs under B's gear. Cart B comes in from the left. B starts in its slot, in A's way: A refuses. So
  // B out, A in, B back in; on P the step runs out.
  static void PlusN42(ExpansionContext c)
  {
   c.Exit=new Vector3(.20f,-.172f,.30f);c.Spawn=new Vector3(.05f,-.25f,-.20f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   var drawer=NextDrawerPlatform(c,.20f,.12f,.17f,.26f);
   // A 5 cm table (a 3 cm slick table is a step); gears ride 1.4 cm over it so A's arm passes under B's gear.
   float y=PlusGearTable(c,"Gear table",new Vector3(-.12f,0,.05f),new Vector2(.19f,.24f),-.25f)+.006f;
   const float xc=-.16f;
   var m=PlusGear(c.Root,"Motor gear",new Vector3(xc,y,.12f));var o=PlusGear(c.Root,"Output gear",new Vector3(-.08f,y,-.02f));
   var pad=ExpansionPad(c,"P",new Vector3(-.33f,-.298f,-.20f),.009f,.10f);
   // Cart A: the 3 cm gear 15 cm behind its body; in, it meshes the motor behind it and B's gear in front. Pushed from
   // its right side (behind it is the front glass).
   var a=ViewTask(c,"A",new Vector3(xc,-.277f,-.25f),Vector3.forward,.15f,floor);a.StandOffset=new Vector3(.065f,0,0);
   var ga=PlusSmallGear(a.Rail.transform,"A carried gear",new Vector3(0,y+.277f,.15f));
   MechanismVisual(a.Rail.transform,"A low gear arm",new Vector3(0,y+.277f-.010f,.087f),new Vector3(.010f,.004f,.126f),metal);
   MechanismVisual(a.Rail.transform,"A gear post",new Vector3(0,y+.277f-.0065f,.15f),new Vector3(.008f,.0035f,.008f),metal,PrimitiveType.Cylinder);
   foreach(var f in a.Rail.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;
   // Cart B: in from the left; it starts in its slot.
   var b=PlusGearCarriage(c,"B",new Vector3(-.345f,-.277f,-.02f),Vector3.right,.09f,floor,y,.095f,out var gb);
   b.Rail.InitialTravel=b.Rail.Travel;b.Rail.transform.localPosition=b.Rail.Start+b.Rail.Axis.normalized*b.Rail.Travel;
   a.RequiredRail=b.Rail;a.RequiredEnd=false; // B's gear sits in A's lane
   var train=PlusTrain(c,"A and B close the train",pad,drawer,false,null,m,ga,gb,o);train.PitchRadii[1]=.03f;train.ToothCounts[1]=14;
   PlusSlotRing(c,"A slot ring",new Vector3(xc,-.2495f,.05f),.036f);
   PlusMeshLamp(c,train,new Vector3(-.06f,y,.10f));
   MechanismVisual(c.Root,"Drawer drive shaft",new Vector3(.05f,-.296f,-.02f),new Vector3(.18f,.006f,.012f),metal);
   MechanismVisual(c.Root,"Drawer drive shaft",new Vector3(.14f,-.296f,.05f),new Vector3(.012f,.006f,.15f),metal);
   NextTrace("P",new Vector3(-.33f,-.2992f,-.15f),new Vector3(-.33f,-.2992f,.12f),new Vector3(-.215f,-.2992f,.12f));
   PlusStep(1,new Vector3(xc,-.29f,-.25f));PlusStep(2,new Vector3(-.30f,-.29f,-.07f));PlusStep(3,new Vector3(xc,-.29f,-.25f));PlusStep(4,new Vector3(-.30f,-.29f,-.07f));
   PlusStep(5,new Vector3(-.33f,-.29f,-.20f));PlusStep(6,new Vector3(.20f,-.27f,.07f));PlusStep(7,new Vector3(.20f,-.24f,.21f));
   PlusLabel("K1",new Vector3(xc,-.26f,-.20f));PlusLabel("K2",new Vector3(-.30f,-.26f,-.02f));
  }

  // N47 · 47 · Mượn bánh (plan 5.5). One gear G, two machines. G's cart starts in machine 1 (front row); a gate across its
  // lane keeps it there. Machine 1 (pad P1) lifts the gate, which latches up. Then G is free to go back to machine 2 (back
  // row, pad P2), which draws the step out of the exit platform. Machine 2 first: nothing turns (its slot is an empty ring).
  static void PlusN47(ExpansionContext c)
  {
   c.Exit=new Vector3(.20f,-.172f,.30f);c.Spawn=new Vector3(.10f,-.25f,-.20f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   var drawer=NextDrawerPlatform(c,.20f,.12f,.17f,.26f);
   float y=PlusGearTable(c,"Gear table",new Vector3(-.10f,0,.045f),new Vector2(.27f,.225f),-.25f);
   var p1=ExpansionPad(c,"P1",new Vector3(-.33f,-.298f,-.10f),.009f,.10f);var p2=ExpansionPad(c,"P2",new Vector3(-.33f,-.298f,.12f),.009f,.10f);
   var m1=PlusGear(c.Root,"Machine 1 motor",new Vector3(-.18f,y,-.02f));var o1=PlusGear(c.Root,"Machine 1 output",new Vector3(-.02f,y,-.02f));
   var m2=PlusGear(c.Root,"Machine 2 motor",new Vector3(-.18f,y,.10f));var o2=PlusGear(c.Root,"Machine 2 output",new Vector3(-.02f,y,.10f));
   // G's cart: on the floor in front of the table, the gear 20 cm behind it on an arm. Start: machine 1; end: machine 2.
   var g=ViewTask(c,"G",new Vector3(-.10f,-.277f,-.22f),Vector3.forward,.12f,floor);
   var gear=PlusGear(g.Rail.transform,"G carried gear",new Vector3(0,y+.277f,.20f));
   MechanismVisual(g.Rail.transform,"G gear arm",new Vector3(0,y+.277f-.004f,.1135f),new Vector3(.010f,.006f,.181f),metal);
   foreach(var f in g.Rail.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;
   // The gate: a 5 cm bar across G's lane just behind the cart (G's arm passes 1 mm over it, its gear beyond it). Machine 1
   // lifts it 8 cm and it latches up.
   var gate=PlusGate(c,"A gate",new Vector3(-.10f,-.275f,-.18f),Vector3.up,.08f,new Vector3(.08f,.05f,.012f));gate.LatchAtEnd=true;
   foreach(float x in new[]{-.15f,-.05f})MechanismVisual(c.Root,"A gate post",new Vector3(x,-.25f,-.18f),new Vector3(.010f,.10f,.010f),metal);
   g.RequiredRail=gate;g.RequiredEnd=true;
   var t1=PlusTrain(c,"Machine 1 lifts the gate",p1,gate,false,null,m1,gear,o1);
   var t2=PlusTrain(c,"Machine 2 draws the step",p2,drawer,false,null,m2,gear,o2);
   PlusSlotRing(c,"Machine 2 slot ring",new Vector3(-.10f,-.2495f,.10f),.047f);
   PlusMeshLamp(c,t1,new Vector3(.02f,y,-.06f));PlusMeshLamp(c,t2,new Vector3(.02f,y,.145f));
   MechanismVisual(c.Root,"Gate drive shaft",new Vector3(.045f,-.296f,-.10f),new Vector3(.012f,.006f,.16f),metal);
   MechanismVisual(c.Root,"Gate drive shaft",new Vector3(-.0075f,-.296f,-.18f),new Vector3(.105f,.006f,.012f),metal);
   MechanismVisual(c.Root,"Drawer drive shaft",new Vector3(.0875f,-.266f,.10f),new Vector3(.105f,.006f,.012f),metal);
   NextTrace("A",new Vector3(-.28f,-.2992f,-.10f),new Vector3(-.255f,-.2992f,-.10f),new Vector3(-.255f,-.2992f,-.02f),new Vector3(-.235f,-.2992f,-.02f));
   NextTrace("B",new Vector3(-.28f,-.2992f,.12f),new Vector3(-.255f,-.2992f,.12f),new Vector3(-.255f,-.2992f,.10f),new Vector3(-.235f,-.2992f,.10f));
   PlusLabel("máy 1",new Vector3(-.10f,-.23f,-.07f));PlusLabel("máy 2",new Vector3(-.10f,-.23f,.16f));
   PlusStep(1,new Vector3(-.33f,-.29f,-.10f));PlusStep(2,new Vector3(-.10f,-.29f,-.27f));PlusStep(3,new Vector3(-.33f,-.29f,.12f));PlusStep(4,new Vector3(.20f,-.27f,.07f));PlusStep(5,new Vector3(.20f,-.24f,.21f));
  }

  // N49 · 49 · Hai động cơ một cửa (plan 5.5, prepares the boss). Two motors in one train both drive the exit door; each runs
  // only while its pad is loaded and pulls with twice the weight on it, and the pulls add. A spring with a preload holds the
  // door down: three quarters on the motors open it, half stalls it half way, a quarter does not move it. Pad C (any part)
  // draws the lock bolt out of the output gear. Three pads at once from a body Q only halves: 50 + 25 on the motors, 25 on C.
  static void PlusN49(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.252f,.15f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.20f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(42,20,0);NextShell(c,.10f,true);
   NextQuantum(c,new Vector3(-.20f,-.30f,-.20f),.025f,.13f);
   float y=PlusGearTable(c,"Gear table",new Vector3(.02f,0,.15f),new Vector2(.32f,.08f),-.25f);
   var m1=PlusGear(c.Root,"Motor 1",new Vector3(-.08f,y,.15f));var m2=PlusGear(c.Root,"Motor 2",new Vector3(0,y,.15f));var o=PlusGear(c.Root,"Door gear",new Vector3(.08f,y,.15f));
   var p1=PlusWeighPad(c,"P1",new Vector3(-.10f,-.298f,.04f));var p2=PlusWeighPad(c,"P2",new Vector3(.10f,-.298f,.04f));
   var pc=ExpansionPad(c,"C",new Vector3(.30f,-.298f,-.20f),.009f,.10f);PadGauge(pc,1);
   // A heavy door (.25 kg): on a 22 g shutter the motor's speed control switched on and off every step and delivered half
   // its pull (the door stuck at 1 cm under three quarters).
   var door=PlusGate(c,"Exit door",new Vector3(.392f,-.249f,.15f),Vector3.up,.10f,new Vector3(.012f,.10f,.11f));door.Body.mass=.25f;
   var bolt=PlusGate(c,"C gear lock",new Vector3(.135f,y,.15f),Vector3.right,.03f,new Vector3(.03f,.01f,.012f));
   PlusLatch(c,"C draws the lock",new[]{pc},bolt,false,false);
   var train=PlusTrain(c,"Two motors open the door",p1,door,true,null,m1,m2,o);train.ExtraClutches=new[]{p2};train.PowerRail=bolt;
   // .072 kg × 2g = 1.41 N against .65 N + 6 N/m × .10 m = 1.25 N at the top; half (.94 N) stalls at 4.8 cm; a quarter
   // (.47 N) does not lift it.
   train.ForcePerLoad=2*9.81f;train.RackSpring=6f;train.RackPreload=.65f;train.ReturnWhenDisconnected=true;
   TapLabel(c.Root,"75%",new Vector3(.30f,-.2985f,.15f));
   MechanismVisual(c.Root,"Door drive shaft",new Vector3(.285f,-.266f,.15f),new Vector3(.21f,.006f,.012f),metal);
   NextTrace("A",new Vector3(-.10f,-.2992f,.09f),new Vector3(-.10f,-.2992f,.11f));
   NextTrace("A",new Vector3(.10f,-.2992f,.09f),new Vector3(.10f,-.2992f,.10f),new Vector3(0,-.2992f,.10f),new Vector3(0,-.2992f,.11f));
   NextTrace("C",new Vector3(.30f,-.2992f,-.15f),new Vector3(.22f,-.2992f,-.15f),new Vector3(.22f,-.2992f,.11f));
   PlusStep(1,new Vector3(-.20f,-.27f,-.17f));PlusStep(2,new Vector3(-.10f,-.29f,.04f));PlusStep(3,new Vector3(.30f,-.29f,-.20f));PlusStep(4,new Vector3(.10f,-.29f,.04f));PlusStep(5,new Vector3(.34f,-.29f,.15f));
   PlusGhost("50%",new Vector3(-.10f,-.28f,.04f));PlusGhost("25%",new Vector3(.10f,-.28f,.04f));PlusGhost("25%",new Vector3(.30f,-.28f,-.20f));
  }

  // A tube wall tinted with a route's circuit colour (plan 5.5: "ống tô màu theo tuyến").
  static Material PlusTubeTint(string name,Color tint)
  {
   string path=SpatialFolder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(glass);AssetDatabase.CreateAsset(m,path);}
   m.CopyPropertiesFromMaterial(glass);var a=glass.HasProperty("_BaseColor")?glass.GetColor("_BaseColor").a:.3f;
   var c=new Color(tint.r,tint.g,tint.b,Mathf.Max(a,.32f));if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);m.color=c;EditorUtility.SetDirty(m);return m;
  }

  // N43 · 43 · Ống theo hộp số (plan 5.5; 15's Y network, the branch chosen by a gearbox). A switch at junction Y lets the
  // tube take either the left branch (to the balcony) or the right one (to the exit landing), never both. One pad P runs two
  // motors; gear G, on a cart between the two rows, closes one of them: G in the left row turns the switch to the balcony,
  // in the right row to the landing. The exit shutter on the landing opens from handle C on the balcony. G and the switch
  // start on the right: the first trip ends at a shut exit. So: G left, P, tube to the balcony, C, back down the tube,
  // G right, P, tube to the landing, out. The route is set before going in.
  static void PlusN43(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.052f,.19f);c.Outward=Vector3.right;c.Spawn=new Vector3(.20f,-.25f,-.27f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var balcony=Top(NextPlinth(c,"Left balcony",new Vector3(-.28f,-.21f,.085f),new Vector3(.22f,.18f,.33f)));
   NextPlinth(c,"Right landing",new Vector3(.27f,-.21f,.12f),new Vector3(.24f,.18f,.34f));
   var nodes=new[]{new COgheTubeNetwork.Node("Vào",new Vector3(-.08f,-.255f,-.16f),COgheTubeNetwork.TerminalKind.Entry), // left of the gearbox, 14 cm from the glass: a body comes out of it too
    new COgheTubeNetwork.Node("Y",new Vector3(0,-.10f,.02f)),
    new COgheTubeNetwork.Node("Ban công",new Vector3(-.215f,-.075f,.10f),COgheTubeNetwork.TerminalKind.Entry),
    new COgheTubeNetwork.Node("Bến phải",new Vector3(.168f,-.078f,.12f),COgheTubeNetwork.TerminalKind.Entry)};
   var edges=new[]{Edge("Vào–Y",0,1,nodes,new Vector3(-.075f,-.225f,-.09f),new Vector3(-.04f,-.16f,-.03f)),
    Edge("Y–Ban công",1,2,nodes,new Vector3(-.08f,-.085f,.06f),new Vector3(-.15f,-.075f,.10f)),
    Edge("Y–Bến phải",1,3,nodes,new Vector3(.07f,-.09f,.07f),new Vector3(.12f,-.08f,.12f))};
   var tube=TubeNetwork(c.Root,"Y transfer tube",nodes,edges,.038f,glass);tube.CaptureSurfaceCommandsWhileInside=true;
   // Where the rising foot is 1–9 cm off the floor a body walking past wedged under it (as E13): slick underfill follows the
   // tube's underside 6 mm below it.
   foreach(var p in COgheTubeNetwork.SampleCurve(edges[0].ControlPoints,14))
   {
    float top=p.y-.038f-.006f,gap=top+.30f;if(gap<.012f||gap>.09f)continue;
    foreach(var f in NextPlinth(c,"Tube underfill",new Vector3(p.x,(top-.30f)*.5f,p.z),new Vector3(.05f,gap,.016f)))f.Slippery=true;
   }
   edges[1].Geometry.GetComponent<MeshRenderer>().sharedMaterial=PlusTubeTint("Tube tint A blue",new Color(.224f,.498f,.678f));
   edges[2].Geometry.GetComponent<MeshRenderer>().sharedMaterial=PlusTubeTint("Tube tint B coral",new Color(.784f,.424f,.345f));
   // The switch in front of Y: at its start the left branch is open, at its end the right one. It starts right.
   var sw=ViewGate(c,"Y switch",new Vector3(-.015f,-.088f,-.06f),Vector3.right,.03f,new Vector3(.02f,.02f,.02f)); // clear of Y's bowl
   sw.InitialTravel=sw.Travel;sw.transform.localPosition=sw.Start+sw.Axis.normalized*sw.Travel;
   edges[1].AccessGate=sw;edges[1].GateAtEnd=false;edges[2].AccessGate=sw;edges[2].GateAtEnd=true;
   // The exit shutter on the landing, raised for good by handle C on the balcony.
   var shutter=PlusGate(c,"C exit shutter",new Vector3(.392f,-.052f,.19f),Vector3.up,.10f,new Vector3(.012f,.10f,.10f));
   var handle=ViewTask(c,"C",new Vector3(-.34f,-.097f,.02f),Vector3.right,.08f,balcony);handle.OneWay=true;
   ViewLink(c,handle.Rail,shutter,true,ViewExitSurface(c));
   // The gearbox, front right (right of the tube, as P and the start: crossing the tube's rising foot, a body climbed it): two
   // rows front to back (motor, slot, output), 16 cm apart; G's cart runs between their slots. G (3.5 cm) between 4 cm
   // gears on 4.4 cm slick pedestals: the lane between the pedestals is 10.6 cm wide, and the body works the cart from 7.5 cm
   // beside it (at 6 cm and an 8 cm lane it was pinched between the cart and a pedestal).
   const float s0=-.18f,xl=.12f,xr=.28f,gy=-.252f,dz=.075f;
   Transform Wheel(Transform parent,string name,Vector3 local){var w=MeshingStationWheel(parent,name,local,.035f,16,0);w.localRotation=Quaternion.Euler(90,0,0);return w;}
   foreach(float x in new[]{xl,xr})foreach(float z in new[]{s0-dz,s0+dz})
    foreach(var f in NextPlinth(c,"Gear pedestal",new Vector3(x,-.28f,z),new Vector3(.044f,.04f,.044f)))f.Slippery=true;
   var ml=PlusGear(c.Root,"Left motor",new Vector3(xl,gy,s0-dz));var ol=PlusGear(c.Root,"Left output",new Vector3(xl,gy,s0+dz));
   var mr=PlusGear(c.Root,"Right motor",new Vector3(xr,gy,s0-dz));var ro=PlusGear(c.Root,"Right output",new Vector3(xr,gy,s0+dz));
   var g=ViewTask(c,"G",new Vector3(xl,-.277f,s0),Vector3.right,xr-xl,floor);g.StandOffset=new Vector3(-.075f,0,0);g.TwoSided=true;
   g.Rail.InitialTravel=g.Rail.Travel;g.Rail.transform.localPosition=g.Rail.Start+g.Rail.Axis.normalized*g.Rail.Travel;
   var gear=Wheel(g.Rail.transform,"G carried gear",new Vector3(0,gy+.277f,0));
   // The knob stands on a post above the gear: at the cart's front it hid behind the front pedestals.
   MechanismVisual(g.Rail.transform,"G gear post",new Vector3(0,.030f,0),new Vector3(.008f,.017f,.008f),metal,PrimitiveType.Cylinder);
   g.Handle.localPosition=new Vector3(0,.047f,0);
   foreach(var f in g.Rail.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;
   var pad=ExpansionPad(c,"P",new Vector3(.04f,-.298f,-.04f),.009f,.08f); // under the tube's top end, 16 cm clear
   var left=PlusTrain(c,"G left: the switch to the balcony",pad,sw,false,null,ml,gear,ol);
   var right=PlusTrain(c,"G right: the switch to the landing",pad,sw,false,null,mr,gear,ro);
   foreach(var t in new[]{left,right}){t.PitchRadii=new[]{.04f,.035f,.04f};t.ToothCounts=new[]{18,16,18};t.Reversible=true;t.LatchOutput=false;}
   left.ForwardSign=1;right.ForwardSign=-1;
   // Each row's slot ring takes its branch's colour: G in the blue ring opens the blue branch, in the coral ring the coral one.
   foreach(var (x,colour) in new[]{(xl,"Circuit A blue"),(xr,"Circuit B coral")})
   {
    var t=new GameObject("Slot ring "+colour).transform;t.SetParent(c.Root,false);t.localPosition=new Vector3(x,-.2995f,s0);t.localRotation=Quaternion.Euler(-90,0,0);
    Ring(t,Vector2.zero,.045f,.004f,AssetDatabase.LoadAssetAtPath<Material>(SpatialFolder+"/"+colour+".mat")??metal);
   }
   NextTrace("A",new Vector3(.055f,-.2992f,-.08f),new Vector3(.055f,-.2992f,-.295f),new Vector3(xr,-.2992f,-.295f));
   PlusLabel("trái",new Vector3(xl,-.24f,s0));PlusLabel("phải",new Vector3(xr,-.24f,s0));
   PlusStep(1,new Vector3(xr+.06f,-.29f,s0));PlusStep(2,new Vector3(.04f,-.29f,-.04f));PlusStep(3,new Vector3(-.08f,-.255f,-.16f));PlusStep(4,new Vector3(-.34f,-.10f,.07f));
   PlusStep(5,new Vector3(-.215f,-.075f,.10f));PlusStep(6,new Vector3(xl-.06f,-.29f,s0));PlusStep(7,new Vector3(.04f,-.29f,-.04f));PlusStep(8,new Vector3(.30f,-.10f,.19f));
  }

  // N46 · 46 · Bàn xoay chở hàng (plan 5.5, a breather). A floor-level turntable (3 cm deck, pointing front to back) carries
  // crate A on its own rail along the deck. The exit ledge (9 cm, slick) is as high as deck and crate together. Push the crate
  // to the deck's far end, step off, stand on P: the table turns a quarter and brings the crate against the ledge as the step.
  // Turned first, the crate lands at the near end; it still slides along the deck to the ledge.
  static void PlusN46(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.165f,.10f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.08f,-.25f,-.22f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit ledge",new Vector3(.30f,-.255f,.10f),new Vector3(.20f,.09f,.30f));
   // The gear train and the table first: on Retry the table returns before the crate is set back on it. The gear table sits
   // in the front-right corner: in front of the turntable a 7 cm gap to it wedged the body stepping off the deck.
   float y=PlusGearTable(c,"Gear table",new Vector3(.18f,0,-.22f),new Vector2(.20f,.08f),-.25f);
   var pad=ExpansionPad(c,"P",new Vector3(-.25f,-.298f,-.24f),.009f,.10f);
   var g0=PlusGear(c.Root,"Motor gear",new Vector3(.14f,y,-.22f));var g1=PlusGear(c.Root,"Turntable gear",new Vector3(.22f,y,-.22f));
   MechanismVisual(c.Root,"Turntable drive shaft",new Vector3(.11f,-.296f,-.15f),new Vector3(.22f,.006f,.012f),metal);
   var train=PlusTrain(c,"P turns the table",pad,null,false,null,g0,g1);
   MechanismVisual(c.Root,"Turntable pedestal",new Vector3(0,-.2995f,.10f),new Vector3(.12f,.0005f,.12f),metal,PrimitiveType.Cylinder);
   // 38 cm: its corners sweep 19.9 cm round the axle, clear of the ledge 20 cm away.
   var deck=Prop(c.Root,"Turntable deck",new Vector3(0,-.284f,.10f),new Vector3(.38f,.03f,.12f),false,plastic,c.Surfaces);c.Props.Add(deck);
   deck.transform.localRotation=Quaternion.Euler(0,90,0);deck.Body.isKinematic=true;
   foreach(var f in deck.GetComponentsInChildren<VenomSurfacePatch>()){f.MotionFrame=deck.Body;if(f.Normal.y<.9f)f.Slippery=true;}
   var top=deck.GetComponentsInChildren<VenomSurfacePatch>().First(f=>f.Normal.y>.9f);
   var safe=new GameObject("Turntable clearance",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(0,-.25f,.10f);safe.Size=new Vector3(.40f,.08f,.40f);
   var table=new GameObject("Turntable",typeof(COgheTurntable)).GetComponent<COgheTurntable>();table.transform.SetParent(c.Root,false);table.Deck=deck.Body;table.Train=train;table.Clearance=safe;
   // Crate A on the deck (deck local +x points to the front): from 6 cm in front of the middle to the back end, pushed from
   // the deck behind it. Its rail rides the deck (as 29's frame), so it turns with it.
   var crate=NextCrate(c,"A",new Vector3(0,-.237f,.04f),Vector3.forward,.20f,new Vector3(.10f,.06f,.10f),top,new Vector3(0,0,-.058f),new Vector3(0,0,-.05f));
   MountOnCarrier(c,crate,deck);
   crate.StandOffset=new Vector3(.05f,0,0); // in the deck's frame (its +x points to the front): behind the crate
   NextTrace("A",new Vector3(-.20f,-.2992f,-.24f),new Vector3(.08f,-.2992f,-.24f));
   PlusStep(1,new Vector3(0,-.27f,-.05f));PlusStep(2,new Vector3(-.25f,-.29f,-.24f));PlusStep(3,new Vector3(-.25f,-.29f,-.24f));PlusStep(4,new Vector3(.14f,-.21f,.10f));PlusStep(5,new Vector3(.30f,-.21f,.10f));
   PlusGhost("90°",new Vector3(0,-.25f,.10f));
  }

  // N48 · 48 · Hai tầng trục (plan 5.5: E14's column, without walking back). The floor layer (motor M on pad P, the gap A
  // fills, column gear S0) turns a screw that lifts gear C up the column's side into the upper layer's gap; then the column
  // carries the turn up (S0 → S1 on one shaft) through C to the bridge gear, and the bridge rises between the mid deck and
  // the exit ledge. One body: A, P, watch the upper layer close itself, climb the stairs, cross.
  static void PlusN48(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.075f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(.20f,-.25f,-.24f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   PlusMidDeck(c,out var mid,false);
   NextPlinth(c,"Exit ledge",new Vector3(.27f,-.21f,.20f),new Vector3(.26f,.18f,.20f));
   var bridge=PlusRisingDeck(c,"Upper bridge",new Vector3(.025f,-.285f,.20f),new Vector3(.21f,.03f,.18f),.15f);
   float y0=PlusGearTable(c,"Floor gear table",new Vector3(-.04f,0,-.04f),new Vector2(.10f,.26f),-.25f);
   PlusGearTable(c,"Screw gear table",new Vector3(.04f,0,.06f),new Vector2(.08f,.08f),-.25f);
   var pad=ExpansionPad(c,"P",new Vector3(-.12f,-.298f,-.25f),.009f,.09f);
   var m=PlusGear(c.Root,"Motor gear",new Vector3(-.05f,y0,-.10f));var s0=PlusGear(c.Root,"Column gear, floor",new Vector3(-.05f,y0,.06f));
   var kf=PlusGear(c.Root,"Screw gear",new Vector3(.03f,y0,.06f));
   PlusGearCarriage(c,"A",new Vector3(.13f,-.277f,-.02f),Vector3.left,.08f,floor,y0,.10f,out var ga);
   PlusShaft(c,"Gear column",PlusColumn,-.25f,PlusMidGear+.01f);
   var s1=PlusGear(c.Root,"Column gear, upper",new Vector3(-.05f,PlusMidGear,.06f));
   // The screw: a rod up from the screw gear; C rides it, 6 cm under the upper layer until the floor layer turns it up.
   PlusShaft(c,"C screw",new Vector3(.03f,0,.06f),-.25f,PlusMidGear-.004f);
   var lift=ExpansionRail(c,"C screw lift",new Vector3(.03f,PlusMidGear-.06f-.012f,.06f),Vector3.up,.06f,0,new Vector3(.024f,.012f,.024f),.05f,.004f,false,false);
   lift.GetComponent<VenomMovableProp>().Manipulable=false;lift.LatchAtEnd=true;
   foreach(var f in lift.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;
   var gc=PlusGear(lift.transform,"C lifted gear",new Vector3(0,.012f,0));
   // The bridge gear beside C on the upper layer, clear of the bridge and the exit ledge.
   var u=PlusGear(c.Root,"Bridge gear",new Vector3(.0993f,PlusMidGear,.02f));
   PlusShaft(c,"Bridge drive shaft",new Vector3(.0993f,0,.02f),-.30f,PlusMidGear-.004f);
   Transform Copy(Transform w,string name){var t=new GameObject(name).transform;t.SetParent(w.parent,false);t.localPosition=w.localPosition;t.localRotation=w.localRotation;return t;}
   var screw=PlusTrain(c,"The floor layer lifts C",pad,lift,false,null,m,ga,s0,kf);
   var upper=PlusTrain(c,"The column turns the bridge",pad,bridge,false,new[]{2},Copy(m,"Motor shaft"),Copy(ga,"A gear shaft"),Copy(s0,"Column shaft"),s1,gc,u);
   upper.SharedWheels=new[]{m,ga,s0};
   PlusSlotRing(c,"A slot ring",new Vector3(-.05f,-.2495f,-.02f),.047f);
   PlusMeshLamp(c,upper,new Vector3(-.10f,PlusMidGear-.006f,.02f));
   NextTrace("A",new Vector3(-.12f,-.2992f,-.205f),new Vector3(-.12f,-.2992f,-.10f),new Vector3(-.09f,-.2992f,-.10f));
   PlusStep(1,new Vector3(-.12f,-.29f,-.25f));PlusStep(2,new Vector3(.13f,-.29f,-.07f));PlusStep(3,new Vector3(-.12f,-.29f,-.25f));PlusStep(4,new Vector3(-.37f,-.12f,-.16f));PlusStep(5,new Vector3(.025f,-.12f,.20f));PlusStep(6,new Vector3(.30f,-.12f,.20f));
   PlusLabel("tầng 1",new Vector3(-.05f,-.23f,-.19f));PlusLabel("tầng 2",new Vector3(.03f,-.09f,.06f));
  }

  // ---- BOSS · 50 · Hộp số (plan 5.5) -----------------------------------------------------------------------------
  // The gearbox: motor M (weigh pad P: it pulls with twice the weight on P), a gap that straight gear A (in from the start,
  // from the right) or the idler pair B (from the left) fills, then hub X0. Two outputs share it. The stair gate up to the
  // mid deck opens with the idler in (reversed); it starts 3 cm up, so the first, wrong direction visibly drops it. The exit
  // door on the mid deck opens with A in (the first direction), but it is heavy (three quarters on P; half stalls it) and
  // bolted: pad C beside it draws the bolt only while loaded. So: idler in for the stairs, out again for the door; split so a
  // quarter holds C up on the deck while three quarters drive. Four trains (two outputs × A or B) share the wheels: each
  // has its own hidden motor shaft and the visible motor follows the one that turns.
  static void PlusN50(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.075f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.20f,-.25f,.058f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(-.20f,-.30f,.12f),.025f,.13f);
   // Mid deck (18 cm) at the back right; three stairs in front of it rise toward the right wall (PlusMidDeck mirrored).
   var mid=Top(NextPlinth(c,"Mid deck",new Vector3(.26f,-.21f,.16f),new Vector3(.28f,.18f,.28f)));
   for(int i=0;i<3;i++)
   {
    float h=.06f*(i+1),x=.25f+.06f*i;
    NextPlinth(c,"Mid deck stair",new Vector3(x,-.30f+h*.5f,-.04f),new Vector3(.06f,h,.12f)).First(f=>f.Normal.x<-.9f).Slippery=false;
   }
   Panel(c.Root,"Mid deck stair face",new Vector3(.25f,-.18f,.0196f),Vector3.back,new Vector2(.06f,.12f),stone,false,Vector2.zero,0,c.Surfaces);
   Panel(c.Root,"Mid deck stair face",new Vector3(.31f,-.15f,.0196f),Vector3.back,new Vector2(.06f,.06f),stone,false,Vector2.zero,0,c.Surfaces);
   var gate=PlusGate(c,"Stair gate",new Vector3(.21f,-.25f,-.04f),Vector3.up,.17f,new Vector3(.012f,.10f,.11f));gate.LatchAtEnd=true;
   gate.InitialTravel=.03f;gate.transform.localPosition=gate.Start+gate.Axis.normalized*.03f;
   // The exit door over the exit hole, and its bolt beside it on the deck; pad C draws the bolt while loaded.
   var door=PlusGate(c,"Exit door",new Vector3(.392f,-.0685f,.20f),Vector3.up,.10f,new Vector3(.012f,.10f,.11f));door.Body.mass=.25f; // heavy: see N49
   var bolt=PlusGate(c,"C door bolt",new Vector3(.388f,-.111f,.268f),Vector3.forward,.02f,new Vector3(.02f,.016f,.02f));
   var pc=ExpansionPad(c,"C",new Vector3(.22f,-.118f,.24f),.009f,.09f);PadGauge(pc,1);
   PlusLatch(c,"C draws the door bolt",new[]{pc},bolt,false,false);
   TapLabel(c.Root,"75%",new Vector3(.34f,-.1195f,.10f));
   // The gearbox on the floor, a row front to back: motor, the gap, hub X0. A in from the right, B in from the left.
   float y=PlusGearTable(c,"Gear table",new Vector3(-.05f,0,-.17f),new Vector2(.10f,.26f),-.25f);
   var m=PlusGear(c.Root,"Motor gear",new Vector3(-.05f,y,-.25f));var x0=PlusGear(c.Root,"Hub gear",new Vector3(-.05f,y,-.09f));
   Transform Shaft(string name){var t=new GameObject(name).transform;t.SetParent(c.Root,false);t.localPosition=m.localPosition;t.localRotation=m.localRotation;return t;}
   // Stand points on open floor: A is worked from behind it (right), B from its front side (the table runs to the front
   // glass, so the strip in front of A, and the gap between B and pad P, were too tight to reach).
   var a=PlusGearCarriage(c,"A",new Vector3(.14f,-.277f,-.17f),Vector3.left,.10f,floor,y,.09f,out var ga);a.StandOffset=new Vector3(.065f,0,0);
   a.Rail.InitialTravel=a.Rail.Travel;a.Rail.transform.localPosition=a.Rail.Start+a.Rail.Axis.normalized*a.Rail.Travel;
   // B: the idler pair, 6.93 cm left of the row, 8 cm from each other and from M and X0 (N44's pair, turned a quarter).
   var b=ViewTask(c,"B",new Vector3(-.22f,-.277f,-.17f),Vector3.right,.07f,floor);b.StandOffset=new Vector3(0,0,-.055f);
   const float zig=.0693f;float arm=.17f-.07f-zig;
   var gb1=PlusGear(b.Rail.transform,"B idler gear",new Vector3(arm,y+.277f,-.04f));var gb2=PlusGear(b.Rail.transform,"B idler gear",new Vector3(arm,y+.277f,.04f));
   MechanismVisual(b.Rail.transform,"B gear arm",new Vector3(arm*.5f,y+.277f-.004f,0),new Vector3(arm+.008f,.006f,.088f),metal);
   foreach(var f in b.Rail.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;b.Rail.LatchAtEnd=true;
   var pad=PlusWeighPad(c,"P",new Vector3(-.32f,-.298f,-.04f));
   var stairsA=PlusTrain(c,"A drops the stair gate",pad,gate,false,null,Shaft("Motor shaft 1"),ga,x0);
   var stairsB=PlusTrain(c,"B lifts the stair gate",pad,gate,false,null,Shaft("Motor shaft 2"),gb1,gb2,x0);
   var doorA=PlusTrain(c,"A opens the exit door",pad,door,true,null,Shaft("Motor shaft 3"),ga,x0);
   var doorB=PlusTrain(c,"B shuts the exit door",pad,door,true,null,Shaft("Motor shaft 4"),gb1,gb2,x0);
   foreach(var t in new[]{stairsA,stairsB,doorA,doorB}){t.Reversible=true;t.ForcePerLoad=2*9.81f;t.SharedWheels=new[]{m};}
   // The door: as N49's (.65 N + 6 N/m: three quarters open it, half stalls it), and only while the bolt is drawn.
   foreach(var t in new[]{doorA,doorB}){t.ForwardSign=-1;t.RackSpring=6f;t.RackPreload=.65f;t.ReturnWhenDisconnected=true;t.PowerRail=bolt;}
   MechanismVisual(c.Root,"Stair gate drive shaft",new Vector3(.08f,-.296f,-.06f),new Vector3(.26f,.006f,.012f),metal);
   NextTrace("A",new Vector3(-.32f,-.2992f,-.09f),new Vector3(-.32f,-.2992f,-.25f),new Vector3(-.10f,-.2992f,-.25f));
   NextTrace("C",new Vector3(.265f,-.1192f,.24f),new Vector3(.37f,-.1192f,.24f),new Vector3(.37f,-.1192f,.258f));
   PlusStep(1,new Vector3(-.32f,-.29f,-.04f));PlusStep(2,new Vector3(.10f,-.29f,-.22f));PlusStep(3,new Vector3(-.25f,-.29f,-.22f));PlusStep(4,new Vector3(-.32f,-.29f,-.04f));
   PlusStep(5,new Vector3(.37f,-.12f,-.04f));PlusStep(6,new Vector3(.22f,-.12f,.24f));PlusStep(7,new Vector3(-.20f,-.27f,.03f));PlusStep(8,new Vector3(.36f,-.08f,.20f));
   PlusGhost("75%",new Vector3(-.32f,-.28f,-.04f));PlusGhost("25%",new Vector3(.22f,-.10f,.24f));
  }

  static void PlusN19(ExpansionContext c)
  {
   c.Exit=new Vector3(-.40f,-.163f,.20f);c.Outward=Vector3.left;c.Spawn=new Vector3(.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Partition wall",new Vector3(-.05f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(-.05f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   var door=PlusGate(c,"A door",new Vector3(-.05f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
   NextQuantum(c,new Vector3(.21f,-.30f,-.02f),.025f,.13f);
   var a=ExpansionPad(c,"A",new Vector3(.32f,-.298f,-.20f),.009f,.10f);
   var b=ViewTask(c,"B",new Vector3(-.14f,-.277f,-.10f),Vector3.left,.05f,floor);b.OneWay=true;
   var safe=new GameObject("Door safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(-.05f,-.25f,-.20f);safe.Size=new Vector3(.06f,.10f,.12f);
   var hold=new GameObject("A holds, B latches the door",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a};hold.Rails=new[]{b.Rail};hold.Output=door;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   NextPlinth(c,"Exit shelf",new Vector3(-.31f,-.255f,.20f),new Vector3(.18f,.09f,.20f));
   NextCrate(c,"D",new Vector3(-.31f,-.27f,-.12f),Vector3.forward,.156f,new Vector3(.14f,.06f,.12f),floor,new Vector3(0,0,-.068f),new Vector3(0,0,-.052f),.06f,.012f);
   // Bolt C lies across D's lane, in the 3 cm between D's start and its place at the shelf; spring handle C in Q's
   // room draws it back only while it is held, and it springs back in front of the docked D.
   var bolt=ExpansionRail(c,"C bolt",new Vector3(-.26f,-.2875f,-.042f),Vector3.right,.10f,0,new Vector3(.12f,.025f,.03f),.02f,.004f,false,false);
   bolt.GetComponent<VenomMovableProp>().Manipulable=false;bolt.CatchTolerance=.003f;
   // At the front of Q's room, between pad A and the door: the freed holder walks straight to it.
   var spring=ViewTask(c,"C",new Vector3(.06f,-.277f,-.13f),Vector3.right,.06f,floor);spring.HoldAtEnd=true;spring.ReturnForce=.03f;spring.Rail.LatchAtEnd=false;
   ViewLink(c,spring.Rail,bolt,false,null);
   NextOutline(c,"D shelf step outline",new Vector3(-.31f,-.2995f,.036f),new Vector2(.145f,.125f));
   NextTrace("A",new Vector3(.27f,-.2992f,-.20f),new Vector3(.05f,-.2992f,-.27f),new Vector3(-.03f,-.2992f,-.27f));
   NextTrace("B",new Vector3(-.19f,-.2992f,-.068f),new Vector3(-.07f,-.2992f,-.068f),new Vector3(-.07f,-.2992f,-.14f));
   PlusStep(1,new Vector3(.21f,-.27f,-.07f));PlusStep(2,new Vector3(.32f,-.29f,-.20f));PlusStep(3,new Vector3(-.16f,-.28f,-.15f));PlusStep(4,new Vector3(.09f,-.28f,-.18f));PlusStep(5,new Vector3(-.31f,-.28f,-.25f));PlusStep(6,new Vector3(-.31f,-.24f,.04f));
  }
 }
}
