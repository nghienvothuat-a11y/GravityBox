using System.Linq;
using GravityBox.Venom;
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
  static COgheRailSlider PlusMidDeck(ExpansionContext c,out VenomSurfacePatch mid)
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
  static void PlusE15(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.075f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(40,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(0,-.30f,-.20f));
   var high=Top(NextPlinth(c,"High deck",new Vector3(.25f,-.21f,.16f),new Vector3(.30f,.18f,.28f)));
   var lift=PlusRisingDeck(c,"Gear lift",new Vector3(-.01f,-.285f,.16f),new Vector3(.20f,.03f,.20f),.15f);lift.LatchAtEnd=false;
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
