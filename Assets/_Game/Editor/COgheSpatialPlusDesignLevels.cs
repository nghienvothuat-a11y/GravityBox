using System.Linq;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
 // Spatial Plus design greyboxes E05–E18, B1, B2 (see COgheSpatialPlusDesignBuilder). Proposals for plates only.
 public static partial class VenomCampaignBuilder
 {
  // ---- shared pieces -------------------------------------------------------------------------------------------
  // A flat gear on a vertical axle (lying on a table or deck), from the older gear levels' involute mesh.
  static Transform PlusGear(Transform parent,string name,Vector3 centre){var g=MeshingStationWheel(parent,name,centre,.04f,18,0);g.localRotation=Quaternion.Euler(90,0,0);return g;}
  // A vertical shaft joining gears on two layers (a compound gear): drawn only, the train owns the motion.
  static void PlusShaft(ExpansionContext c,string name,Vector3 bottom,float top)=>MechanismVisual(c.Root,name,new Vector3(bottom.x,(bottom.y+top)*.5f,bottom.z),new Vector3(.010f,(top-bottom.y)*.5f,.010f),metal,PrimitiveType.Cylinder);
  // A 3 cm ivory gear table; gears sit on its top.
  static float PlusGearTable(ExpansionContext c,string name,Vector3 centre,Vector2 size,float top=-.27f){NextPlinth(c,name,new Vector3(centre.x,(top-.30f)*.5f,centre.z),new Vector3(size.x,top+.30f,size.y),false);return top+.008f;}
  static COgheGearTrain PlusTrain(ExpansionContext c,string name,COgheTissueSensor clutch,COgheRailSlider rack,params Transform[] wheels)
  {
   var train=new GameObject(name,typeof(COgheGearTrain)).GetComponent<COgheGearTrain>();train.transform.SetParent(c.Root,false);
   train.Wheels=wheels;train.PitchRadii=wheels.Select(_=>.04f).ToArray();train.ToothCounts=wheels.Select(_=>18).ToArray();
   train.InputClutch=clutch;train.Rack=rack;train.MeshTolerance=.004f;train.LatchOutput=true;return train;
  }
  // A deck that a gear rack raises from the floor (or a lower deck) by `travel`; its catch keeps it up.
  static COgheRailSlider PlusRisingDeck(ExpansionContext c,string name,Vector3 start,Vector3 size,float travel)
  {
   var deck=ExpansionRail(c,name,start,Vector3.up,travel,0,size,.05f,.01f,false,false);deck.GetComponent<VenomMovableProp>().Manipulable=false;deck.LatchAtEnd=true;TrimSideSlabs(deck);
   foreach(var f in deck.GetComponentsInChildren<VenomSurfacePatch>()){f.MotionFrame=deck.Body;if(f.Normal.y<.9f)f.Slippery=true;}
   return deck;
  }
  // A carriage that slides one flat gear along a floor/deck rail into the gap of a train (handle = label).
  static COgheTapRail PlusGearCarriage(ExpansionContext c,string label,Vector3 start,Vector3 axis,float travel,VenomSurfacePatch floor,out Transform gear)
  {
   var task=ViewTask(c,label,start,axis,travel,floor);gear=PlusGear(task.Rail.transform,label+" carried gear",new Vector3(0,.022f,.07f));
   MechanismVisual(task.Rail.transform,label+" gear arm",new Vector3(0,.012f,.035f),new Vector3(.012f,.008f,.07f),metal);task.Rail.LatchAtEnd=true;return task;
  }
  // A free seesaw plank on the top of a low wall: counterweighted to rest near end (−x) down on the floor; a body that
  // walks past the axle tips it and walks down into the next room. Proposed new mechanism (hinge + mass only).
  static void PlusSeesaw(ExpansionContext c,string label,Vector3 pivot,float half,float rest)
  {
   var plank=Prop(c.Root,label+" seesaw plank",pivot,new Vector3(half*2,.02f,.12f),false,plastic,c.Surfaces);c.Props.Add(plank);
   plank.Body.mass=.05f;plank.Body.centerOfMass=new Vector3(-.03f,0,0);
   foreach(var f in plank.GetComponentsInChildren<VenomSurfacePatch>()){f.MotionFrame=plank.Body;if(f.Normal.y<.9f)f.Slippery=true;}
   plank.transform.localRotation=Quaternion.Euler(0,0,rest);
   var hinge=plank.gameObject.AddComponent<HingeJoint>();hinge.connectedBody=c.Root.GetComponent<Rigidbody>();hinge.autoConfigureConnectedAnchor=false;hinge.anchor=Vector3.zero;hinge.connectedAnchor=pivot;hinge.axis=Vector3.forward;
   hinge.useLimits=true;hinge.limits=new JointLimits{min=-2*rest-2,max=2};hinge.enableCollision=true;
   var axle=MechanismVisual(c.Root,label+" seesaw axle",pivot,new Vector3(.012f,.075f,.012f),metal,PrimitiveType.Cylinder);axle.localRotation=Quaternion.Euler(90,0,0);
   foreach(float z in new[]{-.07f,.07f})MechanismVisual(c.Root,label+" seesaw bracket",pivot+new Vector3(0,-.006f,z),new Vector3(.02f,.012f,.01f),metal);
  }

  // ---- easy levels ---------------------------------------------------------------------------------------------
  // E05 · position 23 · Hai nhịp dây. Right after the rope lesson (22 = old 18): two ropes in an L. Rope A (across)
  // from the ivory start bank to a slick island; rope B (front → back) from the island to the low exit bank. A drop
  // lands on the rescue floor, with stairs back to the start bank.
  static void PlusE05(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.19f,.235f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.11f,-.18f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var start=NextPlinth(c,"Start bank",new Vector3(-.28f,-.23f,-.14f),new Vector3(.24f,.14f,.32f),false);
   NextStairs(c,"Recovery stairs",new Vector3(-.28f,0,.02f),Vector3.forward,-.16f,.12f,3,.06f);
   var island=NextPlinth(c,"Middle island",new Vector3(.2425f,-.25f,-.16f),new Vector3(.315f,.10f,.28f));
   var exitBank=NextPlinth(c,"Exit bank",new Vector3(.2425f,-.27f,.2375f),new Vector3(.315f,.06f,.125f));
   NextSwing(c,"A",new Vector3(-.03f,.14f,-.14f),.28f,40f,new Vector3(-.26f,-.14f,-.14f),Top(start),new[]{Top(island)},new[]{new Vector3(.22f,-.18f,-.14f)},floor);
   NextSwing(c,"B",new Vector3(.26f,.10f,.04f),.28f,40f,new Vector3(.26f,-.18f,-.19f),Top(island),new[]{Top(exitBank)},new[]{new Vector3(.26f,-.22f,.24f)},floor,Vector3.forward);
   PlusStep(1,new Vector3(-.21f,-.075f,-.14f));PlusStep(2,new Vector3(.22f,-.20f,-.14f));PlusStep(3,new Vector3(.26f,-.115f,-.14f));PlusStep(4,new Vector3(.26f,-.24f,.24f));
   PlusLabel("sàn cứu hộ",new Vector3(.0f,-.30f,.12f));
   PlusRoute("100",c.Spawn,new Vector3(-.23f,-.14f,-.14f),new Vector3(.0f,-.14f,-.14f),new Vector3(.22f,-.18f,-.14f),new Vector3(.26f,-.18f,-.18f));
   PlusRoute("100",new Vector3(.26f,-.18f,-.14f),new Vector3(.26f,-.14f,.04f),new Vector3(.26f,-.22f,.24f),c.Exit);
  }

  // E06 · position 26 · Bập bênh. New mechanism for variety, after the counterweight (25 = old 21): two free seesaw
  // planks over two low slick walls. Walk up the plank; past the axle it tips and lets you down into the next room.
  static void PlusE06(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.255f,.10f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.25f,-.20f);NextShell(c,.10f,true);
   NextPlinth(c,"Low wall",new Vector3(-.15f,-.26f,0),new Vector3(.03f,.08f,.59f));
   NextPlinth(c,"Low wall",new Vector3(.13f,-.26f,0),new Vector3(.03f,.08f,.59f));
   PlusSeesaw(c,"A",new Vector3(-.15f,-.21f,-.10f),.20f,24f);
   PlusSeesaw(c,"B",new Vector3(.13f,-.21f,.14f),.20f,24f);
   PlusStep(1,new Vector3(-.30f,-.29f,-.10f));PlusStep(2,new Vector3(-.02f,-.29f,-.10f));PlusStep(3,new Vector3(.25f,-.29f,.14f));
   PlusLabel("trục",new Vector3(-.15f,-.19f,-.10f));PlusLabel("trục",new Vector3(.13f,-.19f,.14f));
   PlusRoute("100",c.Spawn,new Vector3(-.32f,-.29f,-.10f),new Vector3(-.15f,-.20f,-.10f),new Vector3(.03f,-.29f,-.10f),new Vector3(-.05f,-.29f,.14f),new Vector3(.13f,-.20f,.14f),new Vector3(.31f,-.29f,.14f),c.Exit);
  }

  // E07 · position 29 · Chồng hai tầng. Before the block boss: block B rides on block A. Pull A against the 18 cm slick
  // shelf, climb it, push B along A to the shelf edge, climb B, step across.
  static void PlusE07(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.075f,.17f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.28f,-.25f,-.20f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit shelf",new Vector3(.26f,-.21f,.17f),new Vector3(.28f,.18f,.26f));
   var low=NextCrate(c,"A",new Vector3(-.24f,-.255f,.14f),Vector3.right,.24f,new Vector3(.24f,.09f,.16f),floor,new Vector3(0,0,-.088f),new Vector3(0,0,-.06f),.08f,.012f,0,false);
   var high=NextCrate(c,"B",new Vector3(-.31f,-.165f,.14f),Vector3.right,.14f,new Vector3(.10f,.09f,.14f),floor,new Vector3(-.058f,0,0),new Vector3(-.06f,0,0),.04f,.010f,0,false);
   high.Handle.localRotation=Quaternion.Euler(0,90,0);
   MountOnCarrier(c,high,low.GetComponent<VenomMovableProp>());
   NextOutline(c,"A socket",new Vector3(.0f,-.2995f,.14f),new Vector2(.245f,.165f));
   PlusStep(1,new Vector3(-.24f,-.29f,.02f));PlusStep(2,new Vector3(-.12f,-.21f,.14f));PlusStep(3,new Vector3(.02f,-.12f,.14f));PlusStep(4,new Vector3(.24f,-.12f,.17f));
   PlusRoute("100",c.Spawn,new Vector3(-.24f,-.29f,.01f),new Vector3(-.01f,-.29f,.01f));
   PlusRoute("100",new Vector3(-.10f,-.29f,.02f),new Vector3(-.08f,-.21f,.14f),new Vector3(.02f,-.12f,.14f),new Vector3(.24f,-.12f,.17f),c.Exit);
  }

  // ---- BOSS · position 30 · Tháp khối --------------------------------------------------------------------------
  // A three-tier carrier stack is both the tool and the goal. C1 (9 cm, heavy: only the whole body moves it) runs to
  // the 27 cm exit tower; C2 rides C1 across, C3 rides C2 front ↔ back. At the start C2 sits beside the lever shelf,
  // so the tower is the stair to lever B. C1's bolt is drawn only while pad A is loaded AND B is pulled (then it
  // catches): a split. C2 must cross to the tower side; C3 must go back, or the tower's overhang stops C1 short.
  // Merged, the body hauls C1 over and climbs C1 → C2 → C3 → tower.
  static void PlusB1(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,.02f,.15f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(38,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(.0f,-.30f,-.20f),.025f,.14f);
   NextPlinth(c,"Exit tower",new Vector3(.31f,-.165f,.15f),new Vector3(.18f,.27f,.30f));
   NextPlinth(c,"Tower overhang",new Vector3(.19f,-.075f,.06f),new Vector3(.06f,.09f,.12f)); // the 18–27 cm band in front
   var shelf=Top(NextPlinth(c,"Lever shelf",new Vector3(-.315f,-.21f,.21f),new Vector3(.17f,.18f,.18f)));
   var c1=NextCrate(c,"C1",new Vector3(-.08f,-.255f,.14f),Vector3.right,.15f,new Vector3(.30f,.09f,.24f),floor,new Vector3(0,0,-.128f),new Vector3(0,0,-.06f),.22f,.45f,0,false);
   c1.CompensateLoad=true;c1.StallSeconds=5;
   var c2=NextCrate(c,"C2",new Vector3(-.14f,-.165f,.14f),Vector3.right,.12f,new Vector3(.18f,.09f,.24f),floor,new Vector3(-.098f,0,0),new Vector3(-.06f,0,0),.06f,.012f,0,false);
   c2.Handle.localRotation=Quaternion.Euler(0,90,0);MountOnCarrier(c,c2,c1.GetComponent<VenomMovableProp>());
   var c3=NextCrate(c,"C3",new Vector3(-.10f,-.075f,.07f),Vector3.forward,.14f,new Vector3(.10f,.09f,.10f),floor,new Vector3(-.058f,0,0),new Vector3(-.07f,0,0),.03f,.010f,0,false);
   c3.Handle.localRotation=Quaternion.Euler(0,90,0);MountOnCarrier(c,c3,c2.GetComponent<VenomMovableProp>());
   var pad=ExpansionPad(c,"A",new Vector3(.30f,-.298f,-.20f),.009f,.10f);
   var bolt=ViewGate(c,"C1 bolt",new Vector3(-.12f,-.285f,.285f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var lever=ViewTask(c,"B",new Vector3(-.37f,-.097f,.21f),Vector3.right,.06f,shelf);lever.OneWay=true;
   var hold=new GameObject("A and B catch C1's bolt",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{pad};hold.Rails=new[]{lever.Rail};hold.Output=bolt;hold.Any=false;hold.Retain=true;
   c1.RequiredRail=bolt;c1.RequiredEnd=true;c3.RequiredRail=c2.Rail;c3.RequiredEnd=true;
   NextTrace("A",new Vector3(.30f,-.2992f,-.15f),new Vector3(.30f,-.2992f,-.07f),new Vector3(-.12f,-.2992f,-.07f),new Vector3(-.12f,-.2992f,.27f));
   NextTrace("B",new Vector3(-.31f,-.1192f,.21f),new Vector3(-.26f,-.1192f,.21f),new Vector3(-.26f,-.1192f,.29f));
   PlusStep(1,new Vector3(0,-.27f,-.17f));PlusStep(2,new Vector3(.30f,-.29f,-.20f));PlusStep(3,new Vector3(-.33f,-.12f,.21f));PlusStep(4,new Vector3(-.20f,-.12f,.21f));PlusStep(5,new Vector3(-.03f,-.03f,.12f));
   PlusStep(6,new Vector3(-.08f,-.29f,-.06f));PlusStep(7,new Vector3(.10f,-.29f,-.06f));PlusStep(8,new Vector3(.31f,-.03f,.15f));
   PlusGhost("50%",new Vector3(.30f,-.28f,-.20f));PlusGhost("50%",new Vector3(-.33f,-.11f,.21f));
   PlusLabel("C1",new Vector3(-.19f,-.21f,.03f));PlusLabel("C2",new Vector3(-.14f,-.12f,.03f));PlusLabel("C3",new Vector3(-.10f,-.03f,.03f));PlusLabel("mái chìa",new Vector3(.19f,-.03f,.02f));
   PlusRoute("100",c.Spawn,new Vector3(0,-.27f,-.17f));PlusRoute("50a",new Vector3(.14f,-.29f,-.18f),new Vector3(.30f,-.29f,-.20f));
   PlusRoute("50b",new Vector3(-.14f,-.29f,-.18f),new Vector3(-.19f,-.29f,-.05f),new Vector3(-.19f,-.21f,.10f),new Vector3(-.15f,-.12f,.18f),new Vector3(-.33f,-.12f,.21f));
   PlusRoute("100",new Vector3(-.08f,-.29f,-.06f),new Vector3(.10f,-.29f,-.06f));
   PlusRoute("100",new Vector3(.12f,-.21f,.05f),new Vector3(.13f,-.12f,.21f),new Vector3(.17f,-.03f,.21f),new Vector3(.31f,-.03f,.15f),c.Exit);
  }

  // E08 · position 31 · Bánh răng đầu tiên. The rest level after boss 30 introduces gears: stand on pad A and the
  // two flat gears turn; the second drives the rack under the middle deck, which rises between the two ledges and
  // catches. Up the stairs, across, out. One idea: the machine does the lifting while you stand on its pad.
  static void PlusE08(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.155f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.10f,-.25f,-.22f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Start ledge",new Vector3(-.28f,-.24f,.13f),new Vector3(.24f,.12f,.28f));
   NextStairs(c,"Start stairs",new Vector3(-.28f,0,-.01f),Vector3.back,-.18f,.12f,3,.06f);
   NextPlinth(c,"Exit ledge",new Vector3(.28f,-.24f,.13f),new Vector3(.24f,.12f,.28f));
   var deck=PlusRisingDeck(c,"A rising deck",new Vector3(0,-.285f,.13f),new Vector3(.31f,.03f,.16f),.09f);
   float y=PlusGearTable(c,"Gear table",new Vector3(.0f,0,-.10f),new Vector2(.20f,.10f));
   var pad=ExpansionPad(c,"A",new Vector3(-.16f,-.298f,-.20f),.009f,.10f);
   var g0=PlusGear(c.Root,"A motor gear",new Vector3(-.04f,y,-.10f));var g1=PlusGear(c.Root,"A output gear",new Vector3(.04f,y,-.10f));
   PlusShaft(c,"A output shaft",new Vector3(.04f,y,-.10f),y+.02f);MechanismVisual(c.Root,"A rack drive",new Vector3(.04f,-.262f,.0f),new Vector3(.012f,.008f,.20f),metal);
   PlusTrain(c,"A lifts the deck",pad,deck,g0,g1);
   NextTrace("A",new Vector3(-.16f,-.2992f,-.15f),new Vector3(-.16f,-.2992f,-.10f),new Vector3(-.10f,-.2992f,-.10f));
   PlusStep(1,new Vector3(-.16f,-.29f,-.20f));PlusStep(2,new Vector3(-.28f,-.18f,.06f));PlusStep(3,new Vector3(0,-.18f,.13f));PlusStep(4,new Vector3(.30f,-.18f,.12f));
   PlusGhost("bệ nâng",new Vector3(0,-.18f,.13f));
   PlusRoute("100",c.Spawn,new Vector3(-.16f,-.29f,-.20f));PlusRoute("100",new Vector3(-.22f,-.29f,-.22f),new Vector3(-.28f,-.29f,-.18f),new Vector3(-.28f,-.18f,.04f),new Vector3(-.20f,-.18f,.13f),new Vector3(.30f,-.18f,.12f),c.Exit);
  }

  // E09 · position 33 · Đủ nặng mới mở. Before four parts (34 = old 24) and 50/25/25 (35 = old 25): the heavy pad needs half a body; a
  // quarter is too light. The half holds the door open; the two quarters walk through onto the two light pads, which
  // catch the door open for good; the half leaves, they merge.
  static void PlusE09(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.225f,.22f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   var door=ViewGate(c,"A door",new Vector3(.05f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
   NextQuantum(c,new Vector3(-.18f,-.30f,-.02f),.025f,.14f);
   var heavy=ExpansionPad(c,"A",new Vector3(-.32f,-.298f,-.20f),.036f,.10f);TapLabel(c.Root,"50%",new Vector3(-.32f,-.2985f,-.265f));
   var l1=ExpansionPad(c,"B1",new Vector3(.22f,-.298f,.12f),.009f,.09f);var l2=ExpansionPad(c,"B2",new Vector3(.32f,-.298f,-.10f),.009f,.09f);
   TapLabel(c.Root,"25%",new Vector3(.22f,-.2985f,.06f));TapLabel(c.Root,"25%",new Vector3(.32f,-.2985f,-.16f));
   var pin=ViewGate(c,"B door pin",new Vector3(.09f,-.285f,.26f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var both=new GameObject("B1 and B2 catch the door pin",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();both.transform.SetParent(c.Root,false);both.Inputs=new[]{l1,l2};both.Output=pin;both.Retain=true;
   var hold=new GameObject("A holds, the pin keeps the door",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{heavy};hold.Rails=new[]{pin};hold.Output=door;hold.Any=true;hold.Retain=false;
   NextTrace("A",new Vector3(-.27f,-.2992f,-.20f),new Vector3(-.05f,-.2992f,-.27f),new Vector3(.03f,-.2992f,-.27f));
   NextTrace("B",new Vector3(.22f,-.2992f,.17f),new Vector3(.22f,-.2992f,.26f),new Vector3(.11f,-.2992f,.26f));NextTrace("B",new Vector3(.32f,-.2992f,-.05f),new Vector3(.32f,-.2992f,.20f),new Vector3(.22f,-.2992f,.20f));
   PlusStep(1,new Vector3(-.18f,-.27f,-.10f));PlusStep(2,new Vector3(-.03f,-.27f,.01f));PlusStep(3,new Vector3(-.32f,-.29f,-.20f));PlusStep(4,new Vector3(.22f,-.29f,.12f));PlusStep(5,new Vector3(.12f,-.29f,-.20f));PlusStep(6,new Vector3(.34f,-.29f,.22f));
   PlusGhost("50%",new Vector3(-.32f,-.28f,-.20f));PlusGhost("25%",new Vector3(.22f,-.28f,.12f));PlusGhost("25%",new Vector3(.32f,-.28f,-.10f));
   PlusRoute("100",c.Spawn,new Vector3(-.18f,-.27f,-.06f));PlusRoute("50a",new Vector3(-.33f,-.29f,.00f),new Vector3(-.32f,-.29f,-.20f));
   PlusRoute("25",new Vector3(-.03f,-.29f,.00f),new Vector3(-.01f,-.29f,-.20f),new Vector3(.14f,-.29f,-.20f),new Vector3(.22f,-.29f,.12f));
   PlusRoute("25",new Vector3(.14f,-.29f,-.22f),new Vector3(.32f,-.29f,-.10f));
   PlusRoute("100",new Vector3(.20f,-.29f,.05f),new Vector3(.34f,-.29f,.22f),c.Exit);
  }

  // E10 · position 36 · Đu rồi luồn. Before 38 (old 26, rope + tube with two halves): the same pair with one body.
  // Swing from the ivory start bank to the island; the island's tube drops into the walled exit pen.
  static void PlusE10(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.255f,-.21f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.11f,.12f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var start=NextPlinth(c,"Start bank",new Vector3(-.28f,-.23f,.10f),new Vector3(.24f,.14f,.40f),false);
   NextStairs(c,"Recovery stairs",new Vector3(-.30f,0,-.10f),Vector3.back,-.16f,.12f,3,.06f);
   var island=NextPlinth(c,"Island",new Vector3(.2575f,-.25f,.10f),new Vector3(.285f,.10f,.40f));
   NextPlinth(c,"Exit pen wall",new Vector3(.2575f,-.21f,-.125f),new Vector3(.285f,.18f,.02f));
   NextPlinth(c,"Exit pen wall",new Vector3(.125f,-.21f,-.21f),new Vector3(.02f,.18f,.17f));
   NextSwing(c,"A",new Vector3(0,.14f,.12f),.28f,40f,new Vector3(-.23f,-.14f,.12f),Top(start),new[]{Top(island)},new[]{new Vector3(.25f,-.18f,.12f)},floor);
   ChapterTube(c,"Pen tube",new Vector3(.25f,-.155f,-.03f),new Vector3(.25f,-.16f,-.08f),new Vector3(.25f,-.21f,-.14f),new Vector3(.26f,-.25f,-.19f),new Vector3(.27f,-.255f,-.24f));
   PlusStep(1,new Vector3(-.23f,-.075f,.12f));PlusStep(2,new Vector3(.25f,-.20f,.12f));PlusStep(3,new Vector3(.25f,-.155f,-.03f));PlusStep(4,new Vector3(.33f,-.29f,-.21f));
   PlusLabel("chuồng thoát",new Vector3(.27f,-.12f,-.21f));
   PlusRoute("100",c.Spawn,new Vector3(-.23f,-.14f,.12f),new Vector3(0,-.14f,.12f),new Vector3(.25f,-.18f,.12f),new Vector3(.25f,-.18f,-.03f));
   PlusRoute("tube",new Vector3(.25f,-.155f,-.03f),new Vector3(.26f,-.25f,-.19f),new Vector3(.27f,-.255f,-.24f));PlusRoute("100",new Vector3(.30f,-.29f,-.23f),c.Exit);
  }

  // E11 · position 37 · Giữ thang cho bạn. Before 39 (old 27, four roles on a held lift; old 26 sits between): the held lift with two
  // halves. One holds lever B (the lift has power only while it is held); the other rides up and pulls C, which keeps
  // the power on for good; the holder lets go, calls the lift down, rides up, they merge.
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
   var pin=ViewGate(c,"Lift enable pin",new Vector3(-.16f,-.27f,.28f),Vector3.up,.03f,new Vector3(.012f,.012f,.012f));lift.RequiredRail=pin;
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

  // E12 · position 41 · Khớp một bánh. The rest level after boss 40, the second gear lesson: a train with a gap.
  // Pull A: its carriage slides gear G into the gap. Then stand on pad P: the train turns and the step rises beside
  // the slick shelf. Standing on P with the gap open turns only the first gear.
  static void PlusE12(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.155f,.16f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.28f,-.25f,-.20f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit shelf",new Vector3(.27f,-.24f,.16f),new Vector3(.26f,.12f,.28f));
   var step=PlusRisingDeck(c,"Rising step",new Vector3(.075f,-.285f,.16f),new Vector3(.13f,.03f,.16f),.09f);
   float y=PlusGearTable(c,"Gear table",new Vector3(-.10f,0,.16f),new Vector2(.26f,.10f));
   var pad=ExpansionPad(c,"P",new Vector3(-.30f,-.298f,.10f),.009f,.10f);
   var g0=PlusGear(c.Root,"Motor gear",new Vector3(-.18f,y,.16f));var g2=PlusGear(c.Root,"Output gear",new Vector3(-.02f,y,.16f));
   var carriage=PlusGearCarriage(c,"A",new Vector3(-.10f,-.277f,-.10f),Vector3.forward,.19f,floor,out var gear);gear.localPosition=new Vector3(0,y+.277f,.07f);
   PlusTrain(c,"G completes the train",pad,step,g0,gear,g2);
   NextTrace("A",new Vector3(-.30f,-.2992f,.15f),new Vector3(-.30f,-.2992f,.16f),new Vector3(-.23f,-.2992f,.16f));
   PlusStep(1,new Vector3(-.10f,-.29f,-.16f));PlusStep(2,new Vector3(-.30f,-.29f,.10f));PlusStep(3,new Vector3(.075f,-.18f,.16f));PlusStep(4,new Vector3(.30f,-.18f,.16f));
   PlusGhost("G",new Vector3(-.10f,y,.16f));
   PlusRoute("100",c.Spawn,new Vector3(-.10f,-.29f,-.20f),new Vector3(-.10f,-.29f,.00f));PlusRoute("100",new Vector3(-.16f,-.29f,.02f),new Vector3(-.30f,-.29f,.10f));
   PlusRoute("100",new Vector3(-.28f,-.29f,.00f),new Vector3(.04f,-.29f,.00f),new Vector3(.075f,-.18f,.16f),new Vector3(.30f,-.18f,.16f),c.Exit);
  }

  // E13 · position 42 · Hai ống, hai nửa. Before the 3D tube network (43 = old 28): each half takes its own tube up to
  // a high alcove and stands on its pad; both loaded slide the drawer step out of the exit platform, where it catches.
  // Down the same tubes, merge in front of the platform.
  static void PlusE13(ExpansionContext c)
  {
   c.Exit=new Vector3(0,-.172f,.30f);c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(42,20,0);NextShell(c,.30f,true);
   NextQuantum(c,new Vector3(0,-.30f,-.20f));
   var bridge=NextDrawerPlatform(c,0,.12f,.17f,.26f);
   var left=Top(NextPlinth(c,"Left alcove",new Vector3(-.31f,-.20f,.20f),new Vector3(.18f,.20f,.20f)));
   var right=Top(NextPlinth(c,"Right alcove",new Vector3(.31f,-.20f,.20f),new Vector3(.18f,.20f,.20f)));
   var a1=ExpansionPad(c,"A1",new Vector3(-.31f,-.098f,.22f),.009f,.09f);var a2=ExpansionPad(c,"A2",new Vector3(.31f,-.098f,.22f),.009f,.09f);
   ChapterTube(c,"Left tube",new Vector3(-.25f,-.255f,-.10f),new Vector3(-.28f,-.25f,-.04f),new Vector3(-.31f,-.18f,.02f),new Vector3(-.31f,-.10f,.06f),new Vector3(-.31f,-.055f,.13f));
   ChapterTube(c,"Right tube",new Vector3(.25f,-.255f,-.10f),new Vector3(.28f,-.25f,-.04f),new Vector3(.31f,-.18f,.02f),new Vector3(.31f,-.10f,.06f),new Vector3(.31f,-.055f,.13f));
   var latch=new GameObject("A1 A2 slide the step",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();latch.transform.SetParent(c.Root,false);latch.Inputs=new[]{a1,a2};latch.Output=bridge;
   PlusStep(1,new Vector3(0,-.27f,-.17f));PlusStep(2,new Vector3(-.25f,-.255f,-.10f));PlusStep(3,new Vector3(-.31f,-.10f,.22f));PlusStep(4,new Vector3(0,-.27f,.12f));PlusStep(5,new Vector3(0,-.29f,.02f));
   PlusGhost("50%",new Vector3(-.31f,-.08f,.22f));PlusGhost("50%",new Vector3(.31f,-.08f,.22f));
   PlusRoute("50a",new Vector3(-.14f,-.29f,-.18f),new Vector3(-.25f,-.255f,-.10f));PlusRoute("tube",new Vector3(-.25f,-.255f,-.10f),new Vector3(-.31f,-.18f,.02f),new Vector3(-.31f,-.055f,.13f),new Vector3(-.31f,-.10f,.22f));
   PlusRoute("50b",new Vector3(.14f,-.29f,-.18f),new Vector3(.25f,-.255f,-.10f));PlusRoute("tube",new Vector3(.25f,-.255f,-.10f),new Vector3(.31f,-.18f,.02f),new Vector3(.31f,-.055f,.13f),new Vector3(.31f,-.10f,.22f));
   PlusRoute("100",new Vector3(0,-.29f,.02f),new Vector3(0,-.27f,.12f),new Vector3(0,-.24f,.20f),c.Exit);
  }

  // E14 · position 44 · Hai tầng răng. Gears on two layers joined by one shaft. Lower layer (floor table): carriage A
  // closes the gap, pad P turns it and the first step rises to the mid deck. Upper layer (on the mid deck): carriage
  // B closes the upper gap; back on P, the shaft carries the turn up and the bridge deck rises to the exit ledge.
  static void PlusE14(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.075f,.16f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.10f,-.25f,-.24f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var mid=Top(NextPlinth(c,"Mid deck",new Vector3(-.28f,-.21f,.16f),new Vector3(.24f,.18f,.28f)));
   NextPlinth(c,"Exit ledge",new Vector3(.29f,-.21f,.16f),new Vector3(.22f,.18f,.28f));
   var step=PlusRisingDeck(c,"Lower step",new Vector3(-.28f,-.285f,-.06f),new Vector3(.12f,.03f,.12f),.15f);
   var bridge=PlusRisingDeck(c,"Upper bridge",new Vector3(.01f,-.285f,.16f),new Vector3(.33f,.03f,.14f),.15f);
   float y0=PlusGearTable(c,"Lower gear table",new Vector3(.06f,0,-.10f),new Vector2(.30f,.10f));
   var pad=ExpansionPad(c,"P",new Vector3(.30f,-.298f,-.20f),.009f,.10f);
   var m=PlusGear(c.Root,"Motor gear",new Vector3(.18f,y0,-.10f));var s0=PlusGear(c.Root,"Shaft gear, lower",new Vector3(.02f,y0,-.10f));
   var a=PlusGearCarriage(c,"A",new Vector3(.10f,-.277f,-.27f),Vector3.forward,.10f,floor,out var ga);ga.localPosition=new Vector3(0,y0+.277f,.07f);
   float y1=-.12f+.008f;
   var s1=PlusGear(c.Root,"Shaft gear, upper",new Vector3(-.20f,y1,.08f));
   PlusShaft(c,"Layer shaft",new Vector3(.02f,y0,-.10f),y0+.03f);MechanismVisual(c.Root,"Layer shaft link",new Vector3(-.09f,-.15f,-.01f),new Vector3(.25f,.008f,.008f),metal);
   var b=PlusGearCarriage(c,"B",new Vector3(-.36f,-.097f,.08f),Vector3.right,.08f,mid,out var gb);gb.localPosition=new Vector3(.08f,y1+.097f,0);
   var u=PlusGear(c.Root,"Upper output gear",new Vector3(-.20f,y1,.24f));
   PlusTrain(c,"A closes the lower train",pad,step,m,ga,s0);
   PlusTrain(c,"B closes the upper train",pad,bridge,m,ga,s0,s1,gb,u);
   PlusStep(1,new Vector3(.10f,-.29f,-.24f));PlusStep(2,new Vector3(.30f,-.29f,-.20f));PlusStep(3,new Vector3(-.28f,-.12f,-.06f));PlusStep(4,new Vector3(-.36f,-.12f,.08f));PlusStep(5,new Vector3(.30f,-.29f,-.20f));PlusStep(6,new Vector3(.01f,-.12f,.16f));PlusStep(7,new Vector3(.30f,-.12f,.16f));
   PlusLabel("tầng 1",new Vector3(.06f,-.27f,-.16f));PlusLabel("tầng 2",new Vector3(-.20f,-.10f,.02f));
   PlusRoute("100",c.Spawn,new Vector3(.10f,-.29f,-.27f));PlusRoute("100",new Vector3(.12f,-.29f,-.24f),new Vector3(.30f,-.29f,-.20f));
   PlusRoute("100",new Vector3(.24f,-.29f,-.22f),new Vector3(-.28f,-.29f,-.18f),new Vector3(-.28f,-.12f,-.06f),new Vector3(-.36f,-.12f,.08f));
   PlusRoute("100",new Vector3(-.28f,-.12f,.02f),new Vector3(.01f,-.12f,.16f),new Vector3(.30f,-.12f,.16f),c.Exit);
  }

  // E15 · position 46 · Người chạy máy. After the bridge frame (45 = old 29): two halves and a gear lift that runs only
  // while its pad is loaded. One half stands on P (the lift rises with the other aboard); up top the rider pulls C,
  // which pins the lift up and slides the stair out of the high deck; the driver walks up, they merge.
  static void PlusE15(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.04f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(40,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(0,-.30f,-.20f));
   var high=Top(NextPlinth(c,"High deck",new Vector3(.25f,-.18f,.16f),new Vector3(.30f,.24f,.28f)));
   var lift=PlusRisingDeck(c,"Gear lift",new Vector3(-.03f,-.285f,.16f),new Vector3(.20f,.03f,.20f),.225f);lift.LatchAtEnd=false;
   float y=PlusGearTable(c,"Gear table",new Vector3(-.25f,0,.10f),new Vector2(.12f,.26f));
   var pad=ExpansionPad(c,"P",new Vector3(-.30f,-.298f,-.20f),.009f,.10f);
   var g0=PlusGear(c.Root,"Motor gear",new Vector3(-.25f,y,.02f));var g1=PlusGear(c.Root,"Lift gear",new Vector3(-.25f,y,.10f));var g2=PlusGear(c.Root,"Lift gear",new Vector3(-.25f,y,.18f));
   var train=PlusTrain(c,"P runs the lift",pad,lift,g0,g1,g2);train.LatchOutput=false;train.ReturnWhenDisconnected=true;
   var stair=ExpansionRail(c,"C stair",new Vector3(.25f,-.195f,.00f),Vector3.back,.14f,0,new Vector3(.12f,.03f,.14f),.03f,.006f,false,false);stair.GetComponent<VenomMovableProp>().Manipulable=false;stair.LatchAtEnd=true;
   var c3=ViewTask(c,"C",new Vector3(.20f,-.037f,.22f),Vector3.right,.07f,high);c3.OneWay=true;ViewLink(c,c3.Rail,stair,false,null);
   NextStairs(c,"Lower stair",new Vector3(.25f,0,-.07f),Vector3.back,-.18f,.12f,2,.06f);
   NextTrace("A",new Vector3(-.30f,-.2992f,-.15f),new Vector3(-.30f,-.2992f,.02f),new Vector3(-.29f,-.2992f,.02f));
   PlusStep(1,new Vector3(0,-.27f,-.17f));PlusStep(2,new Vector3(-.03f,-.26f,.16f));PlusStep(3,new Vector3(-.30f,-.29f,-.20f));PlusStep(4,new Vector3(.20f,-.06f,.22f));PlusStep(5,new Vector3(.25f,-.15f,-.10f));PlusStep(6,new Vector3(.32f,-.06f,.20f));
   PlusGhost("50%",new Vector3(-.30f,-.28f,-.20f));PlusGhost("50%",new Vector3(-.03f,-.04f,.16f));
   PlusRoute("100",c.Spawn,new Vector3(0,-.27f,-.17f));PlusRoute("50a",new Vector3(-.14f,-.29f,-.18f),new Vector3(-.30f,-.29f,-.20f));
   PlusRoute("50b",new Vector3(.14f,-.29f,-.18f),new Vector3(.12f,-.29f,.02f),new Vector3(-.03f,-.26f,.16f));PlusRoute("50b",new Vector3(-.03f,-.06f,.16f),new Vector3(.20f,-.06f,.22f));
   PlusRoute("50a",new Vector3(-.28f,-.29f,-.24f),new Vector3(.25f,-.29f,-.24f),new Vector3(.25f,-.18f,-.10f),new Vector3(.25f,-.06f,.08f));PlusRoute("100",new Vector3(.30f,-.06f,.16f),c.Exit);
  }

  // E16 · position 47 · Bàn xoay. Variety: the gear output is a turntable. The turntable deck points front–back;
  // standing on P turns it a quarter turn (it stops and catches at 90°), so its deck joins the two ledges.
  // Proposed new output (a stepped turntable driven by the train): needs a prototype.
  static void PlusE16(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.155f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.10f,-.25f,-.24f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Start ledge",new Vector3(-.30f,-.24f,.12f),new Vector3(.20f,.12f,.30f));
   NextStairs(c,"Start stairs",new Vector3(-.30f,0,-.03f),Vector3.back,-.18f,.12f,3,.06f);
   NextPlinth(c,"Exit ledge",new Vector3(.30f,-.24f,.12f),new Vector3(.20f,.12f,.30f));
   var pedestal=MechanismVisual(c.Root,"Turntable pedestal",new Vector3(0,-.25f,.12f),new Vector3(.10f,.05f,.10f),metal,PrimitiveType.Cylinder);
   var table=MechanismVisual(c.Root,"Turntable disc",new Vector3(0,-.195f,.12f),new Vector3(.20f,.006f,.20f),plastic,PrimitiveType.Cylinder);
   var deck=Prop(c.Root,"Turntable deck",new Vector3(0,-.195f,.12f),new Vector3(.40f,.03f,.12f),false,plastic,c.Surfaces);c.Props.Add(deck);deck.transform.localRotation=Quaternion.Euler(0,90,0);
   deck.Body.isKinematic=true;foreach(var f in deck.GetComponentsInChildren<VenomSurfacePatch>())if(f.Normal.y<.9f)f.Slippery=true;
   float y=PlusGearTable(c,"Gear table",new Vector3(.0f,0,-.16f),new Vector2(.20f,.10f));
   var pad=ExpansionPad(c,"P",new Vector3(-.18f,-.298f,-.20f),.009f,.10f);
   PlusGear(c.Root,"Motor gear",new Vector3(-.04f,y,-.16f));PlusGear(c.Root,"Turntable gear",new Vector3(.04f,y,-.16f));
   MechanismVisual(c.Root,"Turntable drive shaft",new Vector3(.04f,-.262f,-.03f),new Vector3(.012f,.008f,.26f),metal);
   NextTrace("A",new Vector3(-.18f,-.2992f,-.15f),new Vector3(-.18f,-.2992f,-.16f),new Vector3(-.10f,-.2992f,-.16f));
   PlusStep(1,new Vector3(-.18f,-.29f,-.20f));PlusStep(2,new Vector3(-.30f,-.18f,.05f));PlusStep(3,new Vector3(0,-.18f,.12f));PlusStep(4,new Vector3(.30f,-.18f,.12f));
   PlusGhost("90°",new Vector3(0,-.18f,.12f));
   PlusRoute("100",c.Spawn,new Vector3(-.18f,-.29f,-.20f));PlusRoute("100",new Vector3(-.26f,-.29f,-.24f),new Vector3(-.30f,-.29f,-.20f),new Vector3(-.30f,-.18f,.04f),new Vector3(-.22f,-.18f,.12f),new Vector3(.30f,-.18f,.12f),c.Exit);
  }

  // E17 · position 48 · Hai máy nối nhau. A machine that moves a machine: pad P1 drives train 1, whose rack pushes
  // the carriage of gear G into train 2; pad P2 then drives train 2 and the exit step rises. One body, two pads.
  static void PlusE17(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.155f,.18f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.28f,-.25f,-.22f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit shelf",new Vector3(.29f,-.24f,.18f),new Vector3(.22f,.12f,.24f));
   var step=PlusRisingDeck(c,"Rising step",new Vector3(.115f,-.285f,.18f),new Vector3(.13f,.03f,.16f),.09f);
   float y=PlusGearTable(c,"Gear table",new Vector3(-.10f,0,.08f),new Vector2(.40f,.12f));
   var p1=ExpansionPad(c,"P1",new Vector3(-.32f,-.298f,-.14f),.009f,.10f);var p2=ExpansionPad(c,"P2",new Vector3(.06f,-.298f,-.18f),.009f,.10f);
   var a0=PlusGear(c.Root,"Train 1 motor",new Vector3(-.28f,y,.08f));var a1=PlusGear(c.Root,"Train 1 rack gear",new Vector3(-.20f,y,.08f));
   var push=ExpansionRail(c,"Train 1 rack",new Vector3(-.13f,-.262f,.03f),Vector3.right,.06f,0,new Vector3(.06f,.012f,.02f),.02f,.004f,false,false);push.GetComponent<VenomMovableProp>().Manipulable=false;push.LatchAtEnd=true;
   PlusTrain(c,"P1 pushes G",p1,push,a0,a1);
   var carriage=ExpansionRail(c,"G carriage",new Vector3(-.06f,-.262f,.08f),Vector3.right,.06f,0,new Vector3(.05f,.012f,.05f),.02f,.004f,false,false);carriage.GetComponent<VenomMovableProp>().Manipulable=false;carriage.LatchAtEnd=true;
   var g=PlusGear(carriage.transform,"G carried gear",new Vector3(0,y+.262f,0));ViewLink(c,push,carriage,false,null);
   var b0=PlusGear(c.Root,"Train 2 motor",new Vector3(.08f,y,.00f));var b1=PlusGear(c.Root,"Train 2 output",new Vector3(.08f,y,.16f));
   PlusTrain(c,"P2 raises the step",p2,step,b0,g,b1);
   PlusStep(1,new Vector3(-.32f,-.29f,-.14f));PlusStep(2,new Vector3(.06f,-.29f,-.18f));PlusStep(3,new Vector3(.115f,-.18f,.18f));PlusStep(4,new Vector3(.31f,-.18f,.18f));
   PlusGhost("G",new Vector3(.0f,y,.08f));PlusLabel("máy 1",new Vector3(-.24f,-.25f,.02f));PlusLabel("máy 2",new Vector3(.08f,-.25f,-.06f));
   PlusRoute("100",c.Spawn,new Vector3(-.32f,-.29f,-.14f));PlusRoute("100",new Vector3(-.26f,-.29f,-.18f),new Vector3(.06f,-.29f,-.18f));
   PlusRoute("100",new Vector3(.12f,-.29f,-.16f),new Vector3(.20f,-.29f,.02f),new Vector3(.115f,-.18f,.18f),new Vector3(.31f,-.18f,.18f),c.Exit);
  }

  // E18 · position 49 · Ba lớp răng. The last lesson before the gear boss: a three-layer gear tower (floor table, 18 cm
  // deck, 36 cm deck) on one vertical shaft line. One half stays on motor pad P; the other closes each layer's gap in
  // turn (A on the floor, B on the mid deck, C on the top deck); each closed layer raises the next step; the last opens
  // the exit shutter. The driver then climbs the same steps and they merge on top.
  static void PlusE18(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,.095f,.18f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(36,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(.18f,-.30f,-.20f),.025f,.13f);
   var mid=Top(NextPlinth(c,"Mid deck",new Vector3(-.26f,-.21f,.14f),new Vector3(.28f,.18f,.32f)));
   var top=Top(NextPlinth(c,"Top deck",new Vector3(.27f,-.12f,.18f),new Vector3(.26f,.36f,.24f)));
   var step1=PlusRisingDeck(c,"Step to the mid deck",new Vector3(-.26f,-.285f,-.08f),new Vector3(.14f,.03f,.12f),.15f);
   var step2=PlusRisingDeck(c,"Step to the top deck",new Vector3(.06f,-.195f,.18f),new Vector3(.16f,.03f,.14f),.18f);
   var shutter=ViewGate(c,"Exit shutter",new Vector3(.392f,.095f,.18f),Vector3.up,.10f,new Vector3(.012f,.10f,.10f));
   float y0=PlusGearTable(c,"Floor gear table",new Vector3(-.02f,0,-.10f),new Vector2(.20f,.10f));
   var pad=ExpansionPad(c,"P",new Vector3(-.24f,-.298f,-.22f),.009f,.10f);
   var m=PlusGear(c.Root,"Motor gear",new Vector3(-.06f,y0,-.10f));var s0=PlusGear(c.Root,"Tower shaft, floor",new Vector3(.06f,y0,-.10f));
   var a=PlusGearCarriage(c,"A",new Vector3(.0f,-.277f,-.26f),Vector3.forward,.09f,floor,out var ga);ga.localPosition=new Vector3(0,y0+.277f,.07f);
   float y1=-.12f+.008f,y2=.06f+.008f;
   PlusShaft(c,"Tower shaft",new Vector3(.06f,y0,-.10f),y2);
   var s1=PlusGear(c.Root,"Tower shaft, mid",new Vector3(-.16f,y1,.06f));
   var b=PlusGearCarriage(c,"B",new Vector3(-.36f,-.097f,.06f),Vector3.right,.07f,mid,out var gb);gb.localPosition=new Vector3(.07f,y1+.097f,0);
   var u1=PlusGear(c.Root,"Mid output",new Vector3(-.16f,y1,.22f));
   var s2=PlusGear(c.Root,"Tower shaft, top",new Vector3(.20f,y2,.10f));
   var cc=PlusGearCarriage(c,"C",new Vector3(.20f,.083f,.03f),Vector3.right,.06f,top,out var gc);gc.localPosition=new Vector3(.06f,y2-.083f,.07f);
   var u2=PlusGear(c.Root,"Shutter gear",new Vector3(.36f,y2,.10f));
   PlusTrain(c,"Layer 1",pad,step1,m,ga,s0);PlusTrain(c,"Layer 2",pad,step2,m,ga,s0,s1,gb,u1);PlusTrain(c,"Layer 3",pad,shutter,m,ga,s0,s1,gb,s2,gc,u2);
   PlusStep(1,new Vector3(.18f,-.27f,-.17f));PlusStep(2,new Vector3(-.24f,-.29f,-.22f));PlusStep(3,new Vector3(.0f,-.29f,-.24f));PlusStep(4,new Vector3(-.36f,-.12f,.06f));PlusStep(5,new Vector3(.20f,.06f,.03f));PlusStep(6,new Vector3(.30f,.06f,.18f));
   PlusLabel("tầng 1",new Vector3(-.02f,-.26f,-.16f));PlusLabel("tầng 2",new Vector3(-.16f,-.10f,.14f));PlusLabel("tầng 3",new Vector3(.24f,.08f,.10f));
   PlusGhost("50%",new Vector3(-.24f,-.28f,-.22f));
   PlusRoute("100",c.Spawn,new Vector3(.18f,-.27f,-.17f));PlusRoute("50a",new Vector3(.04f,-.29f,-.18f),new Vector3(-.24f,-.29f,-.22f));
   PlusRoute("50b",new Vector3(.32f,-.29f,-.18f),new Vector3(.08f,-.29f,-.26f),new Vector3(.0f,-.29f,-.24f));
   PlusRoute("50b",new Vector3(-.12f,-.29f,-.20f),new Vector3(-.26f,-.12f,-.08f),new Vector3(-.36f,-.12f,.06f));
   PlusRoute("50b",new Vector3(-.20f,-.12f,.18f),new Vector3(.06f,.06f,.18f),new Vector3(.20f,.06f,.03f));PlusRoute("100",new Vector3(.24f,.06f,.18f),c.Exit);
  }

  // ---- BOSS · position 50 · Tháp bánh răng ----------------------------------------------------------------------
  // The final boss stacks every gear idea: a gear tower with three layers on one shaft line, two motor pads, a lift
  // that runs only while its pad is held, a carriage moved by another train, and an exit shutter heavy enough that it
  // turns only with BOTH motors loaded. Four quarters: two drivers, one rider, one fitter; each layer opens the way to
  // the next. Layer 1 (floor): A closes the gap; P1 runs the lift to the mid deck (held). Layer 2 (mid deck): the
  // fitter closes B and pulls latch L, which pins the lift up; P2 (on the mid deck) turns layer 2, whose rack shoves
  // carriage C into layer 3 and raises the bridge to the top deck. Layer 3 (top deck) with P1 AND P2 loaded opens the
  // shutter. Then the drivers climb, all four merge on the top deck, exit.
  static void PlusB2(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,.095f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(.20f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(34,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(.20f,-.30f,-.20f),.025f,.13f);
   var mid=Top(NextPlinth(c,"Mid deck",new Vector3(-.25f,-.21f,.12f),new Vector3(.30f,.18f,.36f)));
   var top=Top(NextPlinth(c,"Top deck",new Vector3(.28f,-.12f,.20f),new Vector3(.24f,.36f,.20f)));
   var lift=PlusRisingDeck(c,"Held lift",new Vector3(-.03f,-.285f,.06f),new Vector3(.14f,.03f,.14f),.165f);lift.LatchAtEnd=false;
   var bridge=PlusRisingDeck(c,"Bridge to the top deck",new Vector3(.05f,-.195f,.22f),new Vector3(.22f,.03f,.12f),.18f);
   var shutter=ViewGate(c,"Exit shutter",new Vector3(.392f,.095f,.20f),Vector3.up,.10f,new Vector3(.012f,.10f,.10f));
   float y0=PlusGearTable(c,"Floor gear table",new Vector3(-.12f,0,-.12f),new Vector2(.26f,.10f));
   var p1=ExpansionPad(c,"P1",new Vector3(-.33f,-.298f,-.22f),.009f,.10f);
   var p2=ExpansionPad(c,"P2",new Vector3(-.32f,-.118f,.24f),.009f,.09f);
   var m1=PlusGear(c.Root,"Motor 1",new Vector3(-.20f,y0,-.12f));var s0=PlusGear(c.Root,"Tower shaft, floor",new Vector3(-.04f,y0,-.12f));
   var a=PlusGearCarriage(c,"A",new Vector3(-.12f,-.277f,-.28f),Vector3.forward,.09f,floor,out var ga);ga.localPosition=new Vector3(0,y0+.277f,.07f);
   var liftTrain=PlusTrain(c,"P1 runs the lift",p1,lift,m1,ga,s0);liftTrain.LatchOutput=false;liftTrain.ReturnWhenDisconnected=true;
   float y1=-.12f+.008f,y2=.06f+.008f;
   PlusShaft(c,"Tower shaft",new Vector3(-.04f,y0,-.12f),y2);
   var m2=PlusGear(c.Root,"Motor 2",new Vector3(-.32f,y1,.14f));var s1=PlusGear(c.Root,"Tower shaft, mid",new Vector3(-.16f,y1,.14f));
   var b=PlusGearCarriage(c,"B",new Vector3(-.36f,-.097f,.02f),Vector3.right,.12f,mid,out var gb);gb.localPosition=new Vector3(.12f,y1+.097f,.07f);
   var push=ExpansionRail(c,"Layer 2 rack",new Vector3(-.12f,-.110f,.24f),Vector3.right,.06f,0,new Vector3(.06f,.012f,.02f),.02f,.004f,false,false);push.GetComponent<VenomMovableProp>().Manipulable=false;push.LatchAtEnd=true;
   PlusTrain(c,"P2 turns layer 2",p2,push,m2,s1,gb);
   var carriage=ExpansionRail(c,"C carriage",new Vector3(.14f,y2-.008f,.12f),Vector3.right,.06f,0,new Vector3(.05f,.012f,.05f),.02f,.004f,false,false);carriage.GetComponent<VenomMovableProp>().Manipulable=false;carriage.LatchAtEnd=true;
   var gc=PlusGear(carriage.transform,"C carried gear",new Vector3(0,.008f,0));ViewLink(c,push,carriage,false,null);ViewLink(c,push,bridge,false,null);
   var s2=PlusGear(c.Root,"Tower shaft, top",new Vector3(.06f,y2,.12f));var u=PlusGear(c.Root,"Shutter gear",new Vector3(.36f,y2,.12f));
   var final=PlusTrain(c,"P1 and P2 open the shutter",p1,shutter,m1,ga,s0,s1,s2,gc,u);final.PowerRail=null;
   var lever=ViewTask(c,"L",new Vector3(-.14f,-.097f,.02f),Vector3.right,.06f,mid);lever.OneWay=true;
   NextTrace("A",new Vector3(-.33f,-.2992f,-.17f),new Vector3(-.33f,-.2992f,-.12f),new Vector3(-.26f,-.2992f,-.12f));
   NextTrace("B",new Vector3(-.32f,-.1192f,.19f),new Vector3(-.32f,-.1192f,.14f),new Vector3(-.36f,-.1192f,.14f));
   PlusStep(1,new Vector3(.20f,-.27f,-.17f));PlusStep(2,new Vector3(-.12f,-.29f,-.26f));PlusStep(3,new Vector3(-.33f,-.29f,-.22f));PlusStep(4,new Vector3(-.03f,-.25f,.06f));
   PlusStep(5,new Vector3(-.36f,-.12f,.02f));PlusStep(6,new Vector3(-.14f,-.12f,.02f));PlusStep(7,new Vector3(-.32f,-.12f,.24f));PlusStep(8,new Vector3(.05f,-.02f,.22f));PlusStep(9,new Vector3(.30f,.06f,.20f));
   PlusLabel("tầng 1",new Vector3(-.12f,-.26f,-.18f));PlusLabel("tầng 2",new Vector3(-.16f,-.10f,.20f));PlusLabel("tầng 3",new Vector3(.20f,.08f,.08f));
   PlusGhost("25%",new Vector3(-.33f,-.28f,-.22f));PlusGhost("25%",new Vector3(-.32f,-.10f,.24f));PlusGhost("25%",new Vector3(-.14f,-.10f,.02f));PlusGhost("25%",new Vector3(.05f,.0f,.22f));
   PlusRoute("100",c.Spawn,new Vector3(.20f,-.27f,-.17f));PlusRoute("25",new Vector3(.05f,-.29f,-.20f),new Vector3(-.33f,-.29f,-.22f));
   PlusRoute("25",new Vector3(.34f,-.29f,-.20f),new Vector3(.02f,-.29f,-.26f),new Vector3(-.12f,-.29f,-.26f));
   PlusRoute("25",new Vector3(.34f,-.29f,-.14f),new Vector3(.10f,-.29f,.02f),new Vector3(-.03f,-.26f,.06f),new Vector3(-.03f,-.10f,.06f),new Vector3(-.14f,-.12f,.02f));
   PlusRoute("25",new Vector3(-.14f,-.12f,.06f),new Vector3(-.32f,-.12f,.24f));
   PlusRoute("100",new Vector3(-.10f,-.12f,.22f),new Vector3(.05f,-.02f,.22f),new Vector3(.30f,.06f,.20f),c.Exit);
  }
 }
}
