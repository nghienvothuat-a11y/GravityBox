using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GravityBox.Simulation;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
 // Spatial Plus (proposal of 29/09/2026): greyboxes of 18 easy levels to interleave with the 30 built ones and of two
 // bosses, laid out with the same helpers, sizes and Glass C art as Spatial 11–30. They exist to render design plates:
 // they are NOT playable levels, are never added to the build settings or the catalog, and a mechanism shown here is
 // unproven until the level is built and solved with real taps. Build, render, then CleanPlusDesigns().
 public static partial class VenomCampaignBuilder
 {
  public const string PlusFolder="Assets/_Game/Venom/SpatialPlusDesign";
  public const string PlusNotes="Artifacts/SpatialPlusDesign";
  sealed class PlusDesign{public string Key,Title;public int Position;public bool Boss;public Action<ExpansionContext> Build;}
  static PlusDesign NewPlus(string key,int position,string title,Action<ExpansionContext> build,bool boss=false)=>new PlusDesign{Key=key,Position=position,Title=title,Build=build,Boss=boss};
  static PlusDesign[] PlusDesigns()=>new[]{
   NewPlus("E01",13,"Thang chở hàng",PlusE01),
   NewPlus("E02",15,"Hai ống, một đích",PlusE02),
   NewPlus("E03",18,"Giữ cửa cho bạn",PlusE03),
   NewPlus("E04",21,"Cõng thùng",PlusE04),
   NewPlus("E05",23,"Hai nhịp dây",PlusE05),
   NewPlus("E06",26,"Bập bênh",PlusE06),
   NewPlus("E07",29,"Chồng hai tầng",PlusE07),
   NewPlus("B1",30,"Tháp khối",PlusB1,true),
   NewPlus("E08",31,"Bánh răng đầu tiên",PlusE08),
   NewPlus("E09",33,"Đủ nặng mới mở",PlusE09),
   NewPlus("E10",36,"Đu rồi luồn",PlusE10),
   NewPlus("E11",37,"Giữ thang cho bạn",PlusE11),
   NewPlus("E12",41,"Khớp một bánh",PlusE12),
   NewPlus("E13",42,"Hai ống, hai nửa",PlusE13),
   NewPlus("E14",44,"Hai tầng răng",PlusE14),
   NewPlus("E15",46,"Người chạy máy",PlusE15),
   NewPlus("E16",47,"Bàn xoay",PlusE16),
   NewPlus("E17",48,"Hai máy nối nhau",PlusE17),
   NewPlus("E18",49,"Ba lớp răng",PlusE18),
   NewPlus("B2",50,"Tháp bánh răng",PlusB2,true),
  };

  // Design annotations (root-local): numbered steps, labels and the route, projected onto the renders later.
  static readonly List<string> plusNotes=new List<string>();
  static int plusPath;
  static void PlusNote(string kind,string text,Vector3 p)=>plusNotes.Add(string.Join("|",kind,text,p.x.ToString("F4",CultureInfo.InvariantCulture),p.y.ToString("F4",CultureInfo.InvariantCulture),p.z.ToString("F4",CultureInfo.InvariantCulture)));
  static void PlusStep(int n,Vector3 p)=>PlusNote("step",n.ToString(CultureInfo.InvariantCulture),p);
  static void PlusLabel(string text,Vector3 p)=>PlusNote("label",text,p);
  static void PlusGhost(string text,Vector3 p)=>PlusNote("ghost",text,p);
  // One polyline per call; "kind" names who walks it (whole body, a half, a quarter) so the plate can colour it.
  static void PlusRoute(string who,params Vector3[] points){plusPath++;foreach(var p in points)PlusNote("path",plusPath+":"+who,p);}

  static string[] PlusSelection()
  {
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-coghe-plus-designs");
   return i>=0&&i+1<args.Length?args[i+1].Split(','):null;
  }
  [MenuItem("Gravity Box/COghe/Spatial Plus/Build design greyboxes")]
  public static void GeneratePlusDesigns()
  {
   PrepareCampaign30Assets();Directory.CreateDirectory(PlusFolder+"/Definitions");Directory.CreateDirectory(PlusFolder+"/Meshes");Directory.CreateDirectory(PlusNotes);AssetDatabase.Refresh();
   var only=PlusSelection();var designs=PlusDesigns();
   string old=authoredMeshFolder;
   try{authoredMeshFolder=PlusFolder+"/Meshes";for(int i=0;i<designs.Length;i++)if(only==null||only.Contains(designs[i].Key))BuildPlus(designs[i],i+1);}
   finally{authoredMeshFolder=old;}
   AssetDatabase.SaveAssets();
   Debug.Log("SPATIAL PLUS DESIGNS BUILT "+string.Join(",",designs.Where(d=>only==null||only.Contains(d.Key)).Select(d=>d.Key)));
  }
  // Removes every asset the greyboxes created (scenes, definitions, meshes, art meshes); the builder code stays.
  [MenuItem("Gravity Box/COghe/Spatial Plus/Remove design greyboxes")]
  public static void CleanPlusDesigns()
  {
   var designs=PlusDesigns();
   // The art passes write under the Day Lab art folder, keyed by the private numbers 201…
   string art=COgheDayLabBuilder.Folder+"/Meshes";
   AssetDatabase.DeleteAsset(PlusFolder);AssetDatabase.DeleteAsset(art+"/SpatialPlusDesign");
   for(int i=1;i<=designs.Length;i++)foreach(var pass in new[]{"SpatialGlass","SpatialCircuit","SpecimenPlates/Spatial"})AssetDatabase.DeleteAsset($"{art}/{pass}/Level{200+i}");
   AssetDatabase.Refresh();Debug.Log("SPATIAL PLUS DESIGNS REMOVED");
  }

  static void BuildPlus(PlusDesign d,int index)
  {
   plusNotes.Clear();plusPath=0;
   meshSerial=(200+index)*1000;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var owner=new GameObject("COghe Spatial Plus · "+d.Title).AddComponent<VenomLevelController>();
   owner.MatterProfile=AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder+"/Matter.asset");owner.ControlMode=VenomControlMode.TouchSurface;
   owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");owner.IndicatorMaterial=mint;owner.ApertureRadius=.046f;
   var game=owner.gameObject.AddComponent<VenomCampaign>();string path=$"{PlusFolder}/Definitions/Plus{d.Key}.asset";var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
   if(def==null){def=ScriptableObject.CreateInstance<VenomCampaignDefinition>();AssetDatabase.CreateAsset(def,path);}
   def.Id="coghe.spatial.plus."+d.Key.ToLowerInvariant();def.Order=d.Position;def.Title=$"{d.Position:00} · {d.Title}";def.Lesson="";def.ViewOnly=true;def.CanRotate=false;def.Passive=false;def.Boss=d.Boss;
   def.ProgressKey="coghe.spatial.plus.design";def.CameraEuler=new Vector3(40,20,0);def.CameraZones=Array.Empty<VenomCameraZone>();def.InitialCameraZone=-1;
   def.SceneSequence=new[]{"Plus"+d.Key};game.Definition=def;
   owner.Apparatus=new GameObject("Apparatus").transform;owner.Apparatus.SetParent(owner.transform,false);
   var pivot=new GameObject("Fixed chamber",typeof(Rigidbody),typeof(BoxRotationController));pivot.transform.SetParent(owner.Apparatus,false);pivot.GetComponent<Rigidbody>().isKinematic=true;pivot.GetComponent<Rigidbody>().useGravity=false;owner.Rotation=pivot.GetComponent<BoxRotationController>();
   var c=new ExpansionContext{Number=300+index,Owner=owner,Game=game,Definition=def,Root=pivot.transform,Spawn=new Vector3(-.26f,-.25f,-.19f),Exit=new Vector3(.23f,-.225f,.30f),Outward=Vector3.forward};
   d.Build(c);
   PlusLabel("BẮT ĐẦU",c.Spawn);PlusLabel("THOÁT",c.Exit);
   foreach(var surface in c.Surfaces)if(surface.ExteriorGlass)surface.Selectable=true;
   var floor=c.Surfaces[0];
   owner.Spawn=new GameObject("Spawn").transform;owner.Spawn.SetParent(c.Root,false);owner.Spawn.localPosition=c.Spawn;
   owner.Outlet=new GameObject("Final exit").transform;owner.Outlet.SetParent(c.Root,false);owner.Outlet.localPosition=c.Exit;owner.Outlet.localRotation=Quaternion.LookRotation(c.Outward,Mathf.Abs(c.Outward.y)>.9f?Vector3.forward:Vector3.up);Ring(owner.Outlet,Vector2.zero,owner.ApertureRadius,.0032f,mint);
   game.Surfaces=c.Surfaces.ToArray();game.Props=c.Props.ToArray();owner.CrawlFaces=Enumerable.Repeat(floor.Shape,6).ToArray();foreach(var prop in c.Props)prop.transform.SetParent(owner.Apparatus,true);
   var camera=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.transform.SetParent(owner.transform,false);camera.tag="MainCamera";camera.orthographic=true;camera.nearClipPlane=.005f;camera.farClipPlane=15;camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.rotation=Quaternion.Euler(def.CameraEuler);camera.transform.position=-camera.transform.forward*3;owner.View=camera;
   // The plaque shows the proposed position; the other art passes key their mesh folders by Order, so they get a
   // private number (never one of the built levels' folders).
   Lighting();COgheDayLabBuilder.ApplyExpansionLevel(game,$"Meshes/SpatialPlusDesign/{d.Key}");
   def.Order=200+index;COgheDayLabBuilder.ApplyGlassPreview(game,true);
   var shellSatin=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Expansion satin shell.mat");
   foreach(var p in c.Surfaces)if(p.ExteriorGlass&&p.Slippery)p.GetComponent<Renderer>().sharedMaterial=shellSatin;
   var ivory=SpatialMaterial("Ivory bodies",new Color(.88f,.87f,.76f));var blue=SpatialMaterial("Circuit A blue",new Color(.224f,.498f,.678f));var coral=SpatialMaterial("Circuit B coral",new Color(.784f,.424f,.345f));var slipMat=SpatialMaterial("Lavender slippery",new Color(.53f,.48f,.72f));
   foreach(var p in c.Surfaces)if(!p.ExteriorGlass&&p!=floor&&p.GetComponent<Renderer>()!=null)p.GetComponent<Renderer>().sharedMaterial=p.Slippery?slipMat:ivory;
   foreach(var task in owner.Apparatus.GetComponentsInChildren<COgheTapRail>()){var material=task.Label.StartsWith("B")?coral:blue;foreach(var r in task.Handle.GetComponentsInChildren<Renderer>())r.sharedMaterial=material;}
   COgheDayLabBuilder.DecorateSpatialMechanisms(game,blue,coral,ivory);
   NextSpatialArt(c,blue,coral,ivory);
   // The glass plaque is a TextMesh printed with Order: show the proposed position on it.
   foreach(var text in c.Root.GetComponentsInChildren<TextMesh>(true))if(text.text==(200+index).ToString("00"))text.text=d.Position.ToString("00");
   def.Order=d.Position;EditorUtility.SetDirty(def);EditorSceneManager.SaveScene(scene,$"{PlusFolder}/Plus{d.Key}.unity");
   File.WriteAllLines($"{PlusNotes}/{d.Key}.notes",plusNotes);
  }

  // A rail block riding a moving carrier (itself a rail slider). Called while the carrier is at its authored pose:
  // the rider's rail, guides and seated deck become the carrier's, so the rider travels with it (as 29's frame).
  static void MountOnCarrier(ExpansionContext c,COgheTapRail task,VenomMovableProp carrier)
  {
   var rail=task.Rail;
   rail.Start=carrier.transform.InverseTransformPoint(c.Root.TransformPoint(rail.Start));rail.Axis=carrier.transform.InverseTransformDirection(c.Root.TransformDirection(rail.Axis));rail.Frame=carrier.transform;
   rail.Joint.connectedBody=carrier.Body;rail.Joint.connectedAnchor=rail.Start+rail.Axis*rail.Travel*.5f;
   foreach(var child in c.Root.Cast<Transform>().ToArray())
    if(child.name==task.Label+" block fixed guide"||child.name==task.Label+" block rail stop"||child.name==task.Label+" docked block")child.SetParent(carrier.transform,true);
   foreach(var d in rail.GetComponent<COgheDockedBridgeDeck>().DockedSurfaces)
   {
    d.MotionFrame=carrier.Body;Object.DestroyImmediate(d.Shape);
    var box=d.gameObject.AddComponent<BoxCollider>();box.center=Vector3.back*.004f;box.size=new Vector3(d.Size.x,d.Size.y,.008f);box.sharedMaterial=stepContact;box.contactOffset=.0003f;d.Shape=box;
   }
  }

  // E01 · position 13 · Thang chở hàng. The crate already rides the lift tray: board beside it, send the tray up,
  // push the crate off into the socket beside the exit bench, climb. Level 13's idea without loading the crate
  // (mirrored so the two rooms read differently); 13 itself moves to position 28.
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
   lift.Panel=MechanismVisual(rail.transform,"A lift panel",new Vector3(-.09f,.018f,-.09f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder);
   lift.CallPanels=new[]{MechanismVisual(c.Root,"A lower call panel",new Vector3(.14f,-.262f,-.04f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder),
    MechanismVisual(c.Root,"A upper call panel",new Vector3(-.20f,-.062f,.05f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder)};
   foreach(float x in new[]{.12f,-.14f})MechanismVisual(c.Root,"Lift upright",new Vector3(x,-.18f,.285f),new Vector3(.012f,.24f,.012f),metal);
   var crate=NextLooseCrate(c,"A crate",new Vector3(-.07f,-.2545f,.16f),new Vector3(.10f,.03f,.10f),.04f);
   var socket=new GameObject("A crate socket",typeof(COghePropSocket)).GetComponent<COghePropSocket>();socket.transform.SetParent(c.Root,false);socket.transform.localPosition=new Vector3(-.214f,-.057f,.16f);
   socket.Prop=crate;socket.Socket=socket.transform;socket.Pawl=MechanismVisual(c.Root,"A socket pawl",new Vector3(-.266f,-.069f,.16f),new Vector3(.006f,.006f,.05f),metal);
   NextOutline(c,"Upper crate socket outline",new Vector3(-.214f,-.0715f,.16f),new Vector2(.106f,.106f));
   PlusStep(1,new Vector3(.05f,-.26f,.16f));PlusStep(2,new Vector3(.09f,-.25f,.07f));PlusStep(3,new Vector3(-.20f,-.06f,.16f));PlusStep(4,new Vector3(-.33f,-.01f,.16f));
   PlusLabel("A",new Vector3(.09f,-.25f,.07f));
   PlusRoute("100",c.Spawn,new Vector3(.18f,-.26f,.04f),new Vector3(.05f,-.26f,.16f));
   PlusRoute("100",new Vector3(.05f,-.06f,.16f),new Vector3(-.14f,-.06f,.16f),new Vector3(-.21f,-.04f,.16f),new Vector3(-.33f,-.01f,.20f),c.Exit);
  }

  // E02 · position 15 · Hai ống, một đích. After the tube lesson (14) and before the Y junction (15): two separate
  // tubes. Tube 1 climbs to a slick balcony where A lifts the cap of tube 2; back down the same tube, then tube 2 over
  // the partition into the exit room. Reading where each tube ends is the whole idea.
  static void PlusE02(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.225f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.12f,-.25f,-.22f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Low partition",new Vector3(.08f,-.23f,0),new Vector3(.03f,.14f,.59f));
   var balcony=Top(NextPlinth(c,"Left balcony",new Vector3(-.28f,-.21f,.15f),new Vector3(.22f,.18f,.29f)));
   ChapterTube(c,"Balcony tube",new Vector3(-.06f,-.255f,-.10f),new Vector3(-.06f,-.25f,-.03f),new Vector3(-.075f,-.20f,.04f),new Vector3(-.11f,-.12f,.09f),new Vector3(-.16f,-.08f,.12f),new Vector3(-.21f,-.075f,.14f));
   var tube=ChapterTube(c,"Exit tube",new Vector3(-.08f,-.255f,.22f),new Vector3(-.03f,-.252f,.22f),new Vector3(.01f,-.21f,.22f),new Vector3(.045f,-.13f,.22f),new Vector3(.095f,-.10f,.22f),
    new Vector3(.145f,-.13f,.22f),new Vector3(.18f,-.21f,.22f),new Vector3(.22f,-.252f,.22f),new Vector3(.27f,-.255f,.22f));
   var cap=ViewGate(c,"A tube cap",new Vector3(-.102f,-.253f,.22f),Vector3.up,.12f,new Vector3(.012f,.092f,.092f));
   tube.EntryBlocker=cap.GetComponent<VenomMovableProp>().CollisionShapes[0];
   var a=ViewTask(c,"A",new Vector3(-.34f,-.097f,.08f),Vector3.right,.08f,balcony);
   ViewLink(c,a.Rail,cap,false,null);
   NextTrace("A",new Vector3(-.30f,-.1192f,.04f),new Vector3(-.30f,-.1192f,.02f),new Vector3(-.39f,-.1192f,.02f));
   PlusStep(1,new Vector3(-.06f,-.255f,-.10f));PlusStep(2,new Vector3(-.30f,-.12f,.08f));PlusStep(3,new Vector3(-.21f,-.075f,.14f));PlusStep(4,new Vector3(-.08f,-.255f,.22f));PlusStep(5,new Vector3(.33f,-.30f,.12f));
   PlusLabel("A",new Vector3(-.30f,-.10f,.08f));PlusLabel("ống 1",new Vector3(-.06f,-.27f,-.13f));PlusLabel("ống 2",new Vector3(-.10f,-.27f,.26f));
   PlusRoute("100",c.Spawn,new Vector3(-.06f,-.255f,-.10f));PlusRoute("tube",new Vector3(-.06f,-.255f,-.10f),new Vector3(-.075f,-.20f,.04f),new Vector3(-.16f,-.08f,.12f),new Vector3(-.21f,-.075f,.14f));
   PlusRoute("100",new Vector3(-.21f,-.12f,.14f),new Vector3(-.30f,-.12f,.08f));
   PlusRoute("100",new Vector3(-.12f,-.30f,.05f),new Vector3(-.08f,-.255f,.22f));PlusRoute("tube",new Vector3(-.08f,-.255f,.22f),new Vector3(.095f,-.10f,.22f),new Vector3(.27f,-.255f,.22f));PlusRoute("100",new Vector3(.27f,-.30f,.22f),new Vector3(.36f,-.30f,.12f),c.Exit);
  }

  // E03 · position 18 · Giữ cửa cho bạn. After Q (16), before 17 and boss 20: the hold-then-latch pattern alone,
  // without a tube. One half holds pad A (the door rises); the other walks through and pulls B, which latches the door
  // open for good; the holder then leaves the pad, follows and they merge.
  static void PlusE03(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.225f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,.0775f),new Vector3(.03f,.20f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.20f,-.2775f),new Vector3(.03f,.20f,.035f));
   var door=ViewGate(c,"A door",new Vector3(.05f,-.20f,-.20f),Vector3.up,.20f,new Vector3(.012f,.20f,.114f));
   NextQuantum(c,new Vector3(-.18f,-.30f,-.02f),.025f,.14f);
   var a=ExpansionPad(c,"A",new Vector3(-.32f,-.298f,-.20f),.009f,.10f);
   var b=ViewTask(c,"B",new Vector3(.25f,-.277f,-.04f),Vector3.right,.08f,floor);b.OneWay=true;
   var safe=new GameObject("Door safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(.05f,-.25f,-.20f);safe.Size=new Vector3(.06f,.10f,.12f);
   var hold=new GameObject("A holds, B latches the door",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a};hold.Rails=new[]{b.Rail};hold.Output=door;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   NextTrace("A",new Vector3(-.27f,-.2992f,-.20f),new Vector3(-.05f,-.2992f,-.27f),new Vector3(.03f,-.2992f,-.27f));
   NextTrace("B",new Vector3(.25f,-.2992f,.0f),new Vector3(.25f,-.2992f,.03f),new Vector3(.07f,-.2992f,.03f),new Vector3(.07f,-.2992f,-.14f));
   PlusStep(1,new Vector3(-.18f,-.27f,-.04f));PlusStep(2,new Vector3(-.32f,-.29f,-.20f));PlusStep(3,new Vector3(.25f,-.28f,-.04f));PlusStep(4,new Vector3(.12f,-.29f,-.20f));PlusStep(5,new Vector3(.34f,-.29f,.20f));
   PlusGhost("50%",new Vector3(-.32f,-.28f,-.20f));PlusGhost("50%",new Vector3(.25f,-.28f,-.10f));
   PlusRoute("100",c.Spawn,new Vector3(-.18f,-.27f,-.06f));
   PlusRoute("50a",new Vector3(-.33f,-.29f,.00f),new Vector3(-.32f,-.29f,-.20f));
   PlusRoute("50b",new Vector3(-.03f,-.29f,.00f),new Vector3(-.01f,-.29f,-.20f),new Vector3(.12f,-.29f,-.20f),new Vector3(.25f,-.29f,-.10f));
   PlusRoute("50a",new Vector3(-.28f,-.29f,-.24f),new Vector3(.12f,-.29f,-.24f),new Vector3(.22f,-.29f,-.14f));
   PlusRoute("100",new Vector3(.25f,-.29f,-.08f),new Vector3(.34f,-.29f,.20f),c.Exit);
  }

  // E04 · position 21 · Cõng thùng. The rest level after boss 20, and the first rail-on-rail piece: cart A (3 cm)
  // runs along the floor to the slick exit shelf; crate B (9 cm, ivory sides) runs across the cart. Pull the cart to
  // the shelf, push the crate back beside it, climb. Either order works.
  static void PlusE04(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.152f,.17f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.25f,-.25f,-.20f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit shelf",new Vector3(.26f,-.24f,.16f),new Vector3(.28f,.12f,.28f));
   var cart=NextCrate(c,"A",new Vector3(-.22f,-.285f,.15f),Vector3.right,.27f,new Vector3(.14f,.03f,.30f),floor,new Vector3(-.03f,0,-.158f),new Vector3(0,0,-.052f),.06f,.012f,0,true);
   var crate=NextCrate(c,"B",new Vector3(-.22f,-.225f,.06f),Vector3.forward,.12f,new Vector3(.12f,.09f,.12f),floor,new Vector3(0,0,-.068f),new Vector3(0,0,-.08f),.04f,.010f,0,false);
   MountOnCarrier(c,crate,cart.GetComponent<VenomMovableProp>());
   NextOutline(c,"Cart stop outline",new Vector3(.05f,-.2995f,.15f),new Vector2(.145f,.305f));
   PlusStep(1,new Vector3(-.25f,-.29f,-.02f));PlusStep(2,new Vector3(.05f,-.26f,-.03f));PlusStep(3,new Vector3(.05f,-.18f,.18f));PlusStep(4,new Vector3(.30f,-.18f,.17f));
   PlusGhost("thùng",new Vector3(.05f,-.225f,.18f));
   PlusRoute("100",c.Spawn,new Vector3(-.25f,-.29f,-.04f),new Vector3(.04f,-.29f,-.04f));
   PlusRoute("100",new Vector3(.05f,-.29f,-.07f),new Vector3(.05f,-.26f,.04f),new Vector3(.05f,-.18f,.18f),new Vector3(.30f,-.18f,.17f),c.Exit);
  }
 }
}
