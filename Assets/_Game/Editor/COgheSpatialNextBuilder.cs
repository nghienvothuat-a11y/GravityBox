using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace GravityBox.Editor
{
 // Spatial 11–30 (NewGraphic handoff). Same fixed Glass C box, touch control and art pipeline as Spatial 01–10.
 // Level layouts follow Docs/LevelDesign/COghe/SpatialNext20; no runtime logic reads the level number.
 public static partial class VenomCampaignBuilder
 {
  static readonly string[] SpatialNextNames={"Kê một bậc","Khối lớn đi trước","Thùng đi thang","Luồn một vòng","Gặp nhau ở ngã ba","Một thành hai","Bạn giữ, mình luồn","Bám dây sang bờ","Đưa bến lại gần","Hai nửa một máy",
   "Kéo đối trọng","Ba mảnh thành đường","Đổi tuyến trên vách","Hai rồi bốn","Giữ lại phần lớn","Đu và luồn","Bốn trạm tiếp sức","Đường ống ba chiều","Xưởng lắp cầu","Hộp cộng hưởng"};
  static readonly string[] SpatialNextLessons={"Đẩy thùng A sát bệ để làm bậc.","Khối cao vào trước, khối thấp vào sau.","Đưa thùng lên khay, rồi cùng lên thang.","Kéo A mở ống, rồi chạm miệng ống.","Ở ngã ba, chạm nhánh muốn đi.",
   "Vào máy Q để thành hai phần bằng nhau.","Một phần giữ A, phần kia luồn ống.","Chạm vòng A để bám, rồi chạm bến muốn tới.","Đưa bến vào tầm đu trước.","",
   "Thùng trên khay kéo cầu hạ.","Nhịp xa vào trước.","Đổi tuyến rồi quay lại ngã ba.","Vào Q lần nữa để chia tiếp.","Phần lớn đẩy, phần nhỏ giữ.","Một nửa mở bến, nửa kia đu.","Bốn phần, bốn trạm.","Ngoài đổi tuyến, trong giữ chốt.","Ráp đủ khung rồi mới nâng.",""};
  static int[] SpatialNextSelection()
  {
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-coghe-spatial-levels");
   return i>=0&&i+1<args.Length?args[i+1].Split(',').Select(int.Parse).ToArray():Enumerable.Range(11,20).ToArray();
  }
  [MenuItem("Gravity Box/COghe/Spatial 11–30/Generate levels")]
  public static void GenerateSpatialNext()
  {
   PrepareCampaign30Assets();Directory.CreateDirectory(SpatialFolder+"/Definitions");Directory.CreateDirectory(SpatialFolder+"/Meshes");AssetDatabase.Refresh();
   string old=authoredMeshFolder;try{authoredMeshFolder=SpatialFolder+"/Meshes";foreach(int n in SpatialNextSelection())BuildSpatialNext(n);}finally{authoredMeshFolder=old;}
   // Extend the pilot's scene sequence data only; its scenes and physics are untouched.
   var names=Array.ConvertAll(SpatialScenePaths(),Path.GetFileNameWithoutExtension);
   for(int n=1;n<=10;n++)
   {
    var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>($"{SpatialFolder}/Definitions/Spatial{n:00}.asset");
    if(def!=null&&!def.SceneSequence.SequenceEqual(names)){def.SceneSequence=names;EditorUtility.SetDirty(def);}
   }
   var scenes=EditorBuildSettings.scenes.ToList();foreach(var p in SpatialScenePaths())if(File.Exists(p)&&!scenes.Any(s=>s.path==p))scenes.Add(new EditorBuildSettingsScene(p,true));
   EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
   Debug.Log("SPATIAL NEXT GENERATED "+string.Join(",",SpatialNextSelection()));
  }
  public static void GenerateSpatialNextAndBuildMac(){GenerateSpatialNext();BuildSpatialMac();}

  static void BuildSpatialNext(int n)
  {
   meshSerial=n*1000;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var owner=new GameObject("COghe Spatial · "+SpatialNextNames[n-11]).AddComponent<VenomLevelController>();
   owner.MatterProfile=AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder+"/Matter.asset");owner.ControlMode=VenomControlMode.TouchSurface;
   owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");owner.IndicatorMaterial=mint;owner.ApertureRadius=.046f;
   var game=owner.gameObject.AddComponent<VenomCampaign>();string path=$"{SpatialFolder}/Definitions/Spatial{n:00}.asset";var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
   if(def==null){def=ScriptableObject.CreateInstance<VenomCampaignDefinition>();AssetDatabase.CreateAsset(def,path);}
   def.Id=$"coghe.spatial.next.{n:00}";def.Order=n;def.Title=$"{n:00} · {SpatialNextNames[n-11]}";def.Lesson=SpatialNextLessons[n-11];def.ViewOnly=true;def.CanRotate=false;def.Passive=false;def.Boss=n==20||n==30;
   def.ProgressKey="coghe.spatial.pilot";def.CameraEuler=new Vector3(36,20,0);def.CameraZones=Array.Empty<VenomCameraZone>();def.InitialCameraZone=-1;
   def.SceneSequence=Array.ConvertAll(SpatialScenePaths(),Path.GetFileNameWithoutExtension);game.Definition=def;
   owner.Apparatus=new GameObject("Apparatus").transform;owner.Apparatus.SetParent(owner.transform,false);
   var pivot=new GameObject("Fixed chamber",typeof(Rigidbody),typeof(BoxRotationController));pivot.transform.SetParent(owner.Apparatus,false);pivot.GetComponent<Rigidbody>().isKinematic=true;pivot.GetComponent<Rigidbody>().useGravity=false;owner.Rotation=pivot.GetComponent<BoxRotationController>();
   // Number is offset so shared helpers keyed to older catalogs never branch for these scenes.
   var c=new ExpansionContext{Number=100+n,Owner=owner,Game=game,Definition=def,Root=pivot.transform,Spawn=new Vector3(-.26f,-.25f,-.19f),Exit=new Vector3(.23f,-.225f,.30f),Outward=Vector3.forward};
   switch(n)
   {
    case 11:Next11(c);break;
    case 12:Next12(c);break;
    case 13:Next13(c);break;
    case 14:Next14(c);break;
    case 15:Next15(c);break;
    case 16:Next16(c);break;
    case 17:Next17(c);break;
    case 18:Next18(c);break;
    case 19:Next19(c);break;
    case 20:Next20(c);break;
    case 21:Next21(c);break;
    case 22:Next22(c);break;
    case 23:Next23(c);break;
    case 24:Next24(c);break;
    case 25:Next25(c);break;
    case 26:Next26(c);break;
    case 27:Next27(c);break;
    case 28:Next28(c);break;
    case 29:Next29(c);break;
    case 30:Next30(c);break;
    default:throw new NotImplementedException("Spatial "+n);
   }
   foreach(var surface in c.Surfaces)if(surface.ExteriorGlass)surface.Selectable=true;
   var floor=c.Surfaces[0];
   owner.Spawn=new GameObject("Spawn").transform;owner.Spawn.SetParent(c.Root,false);owner.Spawn.localPosition=c.Spawn;
   owner.Outlet=new GameObject("Final exit").transform;owner.Outlet.SetParent(c.Root,false);owner.Outlet.localPosition=c.Exit;owner.Outlet.localRotation=Quaternion.LookRotation(c.Outward,Mathf.Abs(c.Outward.y)>.9f?Vector3.forward:Vector3.up);Ring(owner.Outlet,Vector2.zero,owner.ApertureRadius,.0032f,mint);
   game.Surfaces=c.Surfaces.ToArray();game.Props=c.Props.ToArray();owner.CrawlFaces=Enumerable.Repeat(floor.Shape,6).ToArray();foreach(var prop in c.Props)prop.transform.SetParent(owner.Apparatus,true);
   var camera=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.transform.SetParent(owner.transform,false);camera.tag="MainCamera";camera.orthographic=true;camera.nearClipPlane=.005f;camera.farClipPlane=15;camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.rotation=Quaternion.Euler(def.CameraEuler);camera.transform.position=-camera.transform.forward*3;owner.View=camera;
   Lighting();COgheDayLabBuilder.ApplyExpansionLevel(game,$"Meshes/Spatial/Level{n:00}");COgheDayLabBuilder.ApplyGlassPreview(game,true);
   // Slick outer panes keep the pilot's quiet satin shell at every box height (the art pass keys it to pane size).
   var shellSatin=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Expansion satin shell.mat");
   foreach(var p in c.Surfaces)if(p.ExteriorGlass&&p.Slippery)p.GetComponent<Renderer>().sharedMaterial=shellSatin;
   var ivory=SpatialMaterial("Ivory bodies",new Color(.88f,.87f,.76f));var blue=SpatialMaterial("Circuit A blue",new Color(.224f,.498f,.678f));var coral=SpatialMaterial("Circuit B coral",new Color(.784f,.424f,.345f));var slipMat=SpatialMaterial("Lavender slippery",new Color(.53f,.48f,.72f));
   foreach(var p in c.Surfaces)if(!p.ExteriorGlass&&p!=floor&&p.GetComponent<Renderer>()!=null)p.GetComponent<Renderer>().sharedMaterial=p.Slippery?slipMat:ivory;
   foreach(var task in owner.Apparatus.GetComponentsInChildren<COgheTapRail>()){var material=task.Label.StartsWith("B")?coral:blue;foreach(var r in task.Handle.GetComponentsInChildren<Renderer>())r.sharedMaterial=material;}
   foreach(var rail in owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())if(rail.name.Contains("shutter"))foreach(var r in rail.GetComponentsInChildren<Renderer>())r.sharedMaterial=rail.name.StartsWith("B")?coral:blue;
   COgheDayLabBuilder.DecorateSpatialMechanisms(game,blue,coral,ivory);
   NextSpatialArt(c,blue,coral,ivory);
   EditorUtility.SetDirty(def);EditorSceneManager.SaveScene(scene,SpatialScenePaths()[n-1]);
  }

  // Presentation for Spatial 11–30 mechanisms the pilot art pass does not know: pad caps, printed control traces,
  // Q casing and quiet scan sheet. Nothing here owns a collider or changes a route.
  static readonly List<(string label,Vector3[] points)> nextTraces=new List<(string,Vector3[])>();
  static void NextTrace(string label,params Vector3[] points)=>nextTraces.Add((label,points));
  static void NextSpatialArt(ExpansionContext c,Material blue,Material coral,Material ivory)
  {
   var printedA=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/../Art/DayLab/Spatial printed A.mat")??AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Spatial printed A.mat");
   var printedB=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Spatial printed B.mat");
   var pearl=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Spatial pearl casing.mat")??ivory;
   foreach(var task in c.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
    if(task.Label!="A"&&task.Label!="B"&&task.Handle!=null)foreach(var r in task.Handle.GetComponentsInChildren<Renderer>())r.sharedMaterial=pearl;
   foreach(var sensor in c.Root.GetComponentsInChildren<COgheTissueSensor>())
   {
    bool b=sensor.name.StartsWith("B");if(sensor.Cap!=null)sensor.Cap.GetComponent<Renderer>().sharedMaterial=b?coral:blue;
   }
   foreach(var q in c.Root.GetComponentsInChildren<COgheQuantumSplitter>())
    foreach(var r in q.GetComponentsInChildren<Renderer>(true))if(r.name.Contains("casing")||r.name.Contains("housing"))r.sharedMaterial=pearl;
   var art=new GameObject("Spatial next printed traces").transform;art.SetParent(c.Root,false);
   foreach(var (label,points) in nextTraces)
    for(int i=1;i<points.Length;i++)
    {
     Vector3 a=points[i-1],b=points[i],d=b-a;if(d.magnitude<.001f)continue;
     var strip=MechanismVisual(art,"Printed conductor "+label,(a+b)*.5f,new Vector3(.003f,.0008f,d.magnitude+.003f),label.StartsWith("B")?printedB:printedA);
     strip.localRotation=Quaternion.LookRotation(d,Mathf.Abs(d.normalized.y)>.9f?Vector3.forward:Vector3.up);
     strip.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    }
   nextTraces.Clear();
  }

  // Quantum machine Q: a low chamber open at the front (local -z). The septum is stored in a rear housing and slides
  // forward through a slot in the back wall between the two lobes; side lanes lead to two trays. Casing is climbable
  // ivory (it leads nowhere); only the septum and scan sheet belong to the machine's action.
  static COgheQuantumSplitter NextQuantum(ExpansionContext c,Vector3 origin,float trayZ=.025f,float trayX=.155f)
  {
   var q=new GameObject("Q quantum machine",typeof(COgheQuantumSplitter)).GetComponent<COgheQuantumSplitter>();q.transform.SetParent(c.Root,false);q.transform.localPosition=origin;q.Chamber=q.transform;
   Vector3 L(float x,float y,float z)=>origin+new Vector3(x,y,z);
   foreach(float side in new[]{-1f,1f})
   {
    ViewBlock(c,"Q casing side",L(side*.085f,.045f,-.025f),new Vector3(.02f,.09f,.05f));
    ViewBlock(c,"Q casing back",L(side*.0495f,.045f,.06f),new Vector3(.091f,.09f,.02f));
   }
   // Roof in three pieces: 6 mm slots over the two tray lanes let the lane gates rise.
   ViewBlock(c,"Q casing roof",L(0,.095f,.01f),new Vector3(.144f,.01f,.12f));
   foreach(float side in new[]{-1f,1f})ViewBlock(c,"Q casing roof",L(side*.0865f,.095f,.01f),new Vector3(.017f,.01f,.12f));
   foreach(float side in new[]{-1f,1f})
   {
    // 3 mm clear of the casing at both ends so the gate never rubs a coplanar face.
    var gate=ExpansionRail(c,side<0?"Q left lane gate":"Q right lane gate",L(side*.075f,.046f,.025f),Vector3.up,.09f,0,new Vector3(.004f,.088f,.044f),.01f,.002f,false,false);gate.CatchTolerance=.003f;
    foreach(var face in gate.GetComponentsInChildren<VenomSurfacePatch>(true)){c.Surfaces.Remove(face);Object.DestroyImmediate(face.gameObject);}
    var gateBox=gate.gameObject.AddComponent<BoxCollider>();gateBox.size=new Vector3(.004f,.088f,.044f);gateBox.sharedMaterial=contact;gateBox.contactOffset=.0003f;
    MechanismVisual(gate.transform,"Q lane gate glass",Vector3.zero,new Vector3(.004f,.088f,.044f),glass);
    if(side<0)q.LeftGate=gate;else q.RightGate=gate;
   }
   var septum=ExpansionRail(c,"Q septum",L(0,.041f,.10f),Vector3.back,.10f,0,new Vector3(.004f,.078f,.10f),.02f,.002f,false,false);septum.CatchTolerance=.003f;
   foreach(var face in septum.GetComponentsInChildren<VenomSurfacePatch>(true)){c.Surfaces.Remove(face);Object.DestroyImmediate(face.gameObject);}
   var plate=septum.gameObject.AddComponent<BoxCollider>();plate.size=new Vector3(.004f,.078f,.10f);plate.sharedMaterial=contact;plate.contactOffset=.0003f;
   MechanismVisual(septum.transform,"Q septum glass",Vector3.zero,new Vector3(.004f,.078f,.10f),glass);
   q.Septum=septum;
   // A real rear housing encloses the stored septum (hollow: panels only), so nothing walks into the plate.
   foreach(float side in new[]{-1f,1f})Panel(c.Root,"Q rear housing",L(side*.025f,.05f,.12f),Vector3.right*side,new Vector2(.10f,.10f),stone,false,Vector2.zero,0,c.Surfaces);
   Panel(c.Root,"Q rear housing",L(0,.05f,.17f),Vector3.forward,new Vector2(.05f,.10f),stone,false,Vector2.zero,0,c.Surfaces);
   Panel(c.Root,"Q rear housing",L(0,.10f,.12f),Vector3.up,new Vector2(.05f,.10f),stone,false,Vector2.zero,0,c.Surfaces);
   MechanismVisual(q.transform,"Q housing cap",new Vector3(0,.107f,.01f),new Vector3(.19f,.014f,.12f),plastic);
   TapLabel(q.transform,"Q",new Vector3(0,.07f,-.0515f));var mark=q.transform.Find("Q");mark.localRotation=Quaternion.identity;mark.GetComponent<TextMesh>().characterSize=.012f;
   foreach(float side in new[]{-1f,1f})
   {
    var tray=new GameObject(side<0?"Q left tray":"Q right tray").transform;tray.SetParent(c.Root,false);tray.localPosition=L(side*trayX,0,trayZ);
    if(side<0)q.LeftTray=tray;else q.RightTray=tray;
    NextOutline(c,"Q tray rim",L(side*trayX,.0006f,trayZ),new Vector2(.10f,.10f));
    TapLabel(c.Root,"50%",L(side*trayX,.001f,trayZ-.062f));
   }
   q.LeftLane=new[]{new Vector3(-.045f,.025f,trayZ),new Vector3(-Mathf.Min(.10f,trayX-.03f),.025f,trayZ)};q.RightLane=new[]{new Vector3(.045f,.025f,trayZ),new Vector3(Mathf.Min(.10f,trayX-.03f),.025f,trayZ)};
   q.ScanSheet=MechanismVisual(q.transform,"Q scan sheet",new Vector3(0,.045f,0),new Vector3(.002f,.085f,.095f),mint);q.ScanSheet.gameObject.SetActive(false);
   q.GatherLocal=new Vector3(0,.03f,0);q.ReceiveHalfSize=new Vector3(.075f,.05f,.05f);q.TouchCentre=new Vector3(0,.075f,.01f);q.TouchHalfSize=new Vector3(.10f,.04f,.065f);
   return q;
  }

  // Rope swing: one anchor on a beam, a ring on a finite rope constraint (planar), parked on a hook by the winch.
  static COgheSwingTransfer NextSwing(ExpansionContext c,string label,Vector3 pivot,float length,float startAngle,Vector3 stand,VenomSurfacePatch startBank,VenomSurfacePatch[] docks,Vector3[] targets,VenomSurfacePatch rescue,Vector3? acrossDirection=null)
  {
   // across: horizontal direction of travel from the start hook toward the docks (default +x).
   Vector3 across=acrossDirection??Vector3.right,normal=Vector3.Cross(across,Vector3.up);
   var swing=new GameObject(label+" rope swing",typeof(COgheSwingTransfer)).GetComponent<COgheSwingTransfer>();swing.transform.SetParent(c.Root,false);swing.Label=label;
   swing.Pivot=NextMarker(c,label+" rope anchor",pivot);swing.RopeLength=length;
   float a=startAngle*Mathf.Deg2Rad;Vector3 hook=pivot+(-across*Mathf.Sin(a)+Vector3.down*Mathf.Cos(a))*length;swing.PlaneNormal=normal;
   swing.StartHook=NextMarker(c,label+" start hook",hook);swing.StandPoint=NextMarker(c,label+" grip stance",stand);
   var ringGo=new GameObject(label+" grip ring",typeof(Rigidbody));ringGo.transform.SetParent(c.Owner.Apparatus,false);ringGo.transform.position=c.Root.TransformPoint(hook);
   var ring=ringGo.GetComponent<Rigidbody>();ring.mass=.03f;ring.useGravity=false;ring.isKinematic=true;ring.interpolation=RigidbodyInterpolation.Interpolate;ring.inertiaTensor=Vector3.one*1e-5f;ring.inertiaTensorRotation=Quaternion.identity;
   ring.linearDamping=0;ring.angularDamping=0;ring.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
   var joint=ringGo.AddComponent<ConfigurableJoint>();joint.connectedBody=c.Root.GetComponent<Rigidbody>();joint.autoConfigureConnectedAnchor=false;joint.anchor=Vector3.zero;joint.connectedAnchor=pivot;
   joint.axis=normal;joint.secondaryAxis=Vector3.up;joint.xMotion=ConfigurableJointMotion.Locked;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Limited;
   joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;joint.linearLimit=new SoftJointLimit{limit=length,contactDistance=.002f};
   swing.Ring=ring;
   var visual=new GameObject(label+" ring visual").transform;visual.SetParent(ringGo.transform,false);swing.RingVisual=visual;
   var disc=MechanismVisual(visual,label+" ring grip",Vector3.down*.012f,new Vector3(.036f,.004f,.036f),plastic,PrimitiveType.Cylinder);disc.localRotation=Quaternion.Euler(90,0,0);
   MechanismVisual(visual,label+" ring shackle",Vector3.up*.008f,new Vector3(.008f,.018f,.008f),metal);
   swing.StartBank=startBank;swing.Docks=docks;swing.RescueFloor=rescue;
   swing.DockTargets=new Transform[targets.Length];for(int i=0;i<targets.Length;i++)swing.DockTargets[i]=NextMarker(c,label+" landing "+i,targets[i]);
   swing.Rope=swing.gameObject.AddComponent<LineRenderer>();swing.Rope.useWorldSpace=true;swing.Rope.startWidth=swing.Rope.endWidth=.004f;swing.Rope.sharedMaterial=metal;swing.Rope.positionCount=2;
   swing.Drum=MechanismVisual(c.Root,label+" winch drum",pivot+Vector3.up*.012f,new Vector3(.05f,.012f,.05f),metal,PrimitiveType.Cylinder);swing.Drum.localRotation=Quaternion.FromToRotation(Vector3.up,normal);
   MechanismVisual(c.Root,label+" rope beam",new Vector3(pivot.x,pivot.y+.03f,pivot.z),new Vector3(.36f,.016f,.02f),metal).localRotation=Quaternion.FromToRotation(Vector3.right,across);
   MechanismVisual(c.Root,label+" start hook post",hook-across*.018f+Vector3.down*.004f,new Vector3(.006f,.03f,.006f),metal);
   return swing;
  }

  // Floor plus five clear panes; the final exit is cut in the pane its outward direction faces.
  static void NextShell(ExpansionContext c,float top,bool slippery)
  {
   Panel(c.Root,"Laboratory floor",new Vector3(0,-.30f,0),Vector3.up,new Vector2(.8f,.6f),stone,false,Vector2.zero,0,c.Surfaces);
   float mid=(top-.30f)*.5f,height=top+.30f;
   var pos=new[]{new Vector3(0,top,0),new Vector3(0,mid,-.3f),new Vector3(0,mid,.3f),new Vector3(-.4f,mid,0),new Vector3(.4f,mid,0)};
   var normal=new[]{Vector3.down,Vector3.forward,Vector3.back,Vector3.right,Vector3.left};
   for(int i=0;i<5;i++)
   {
    bool hole=Vector3.Dot(normal[i],c.Outward)<-.9f;var rot=Quaternion.LookRotation(normal[i],i==0?Vector3.forward:Vector3.up);var local=Quaternion.Inverse(rot)*(c.Exit-pos[i]);
    var p=Panel(c.Root,"Outer pane "+i,pos[i],normal[i],i==0?new Vector2(.8f,.6f):new Vector2(i<3?.8f:.6f,height),glass,hole,new Vector2(local.x,local.y),hole?c.Owner.ApertureRadius:0,c.Surfaces);
    p.ExteriorGlass=true;p.Selectable=true;p.Slippery=slippery;
   }
  }
  // A fixed block whose risers (and underside) carry no grip; only its top is a route.
  static List<VenomSurfacePatch> NextPlinth(ExpansionContext c,string name,Vector3 centre,Vector3 size,bool slipperySides=true)
  {
   int first=c.Surfaces.Count;ViewBlock(c,name,centre,size);var faces=c.Surfaces.GetRange(first,c.Surfaces.Count-first);
   foreach(var face in faces)if(face.Normal.y<.9f)face.Slippery=slipperySides;
   return faces;
  }
  static VenomSurfacePatch Top(List<VenomSurfacePatch> faces)=>faces.First(f=>f.Normal.y>.9f);
  // A block on a real rail, driven by its own handle. When it rests latched in its socket a static replica
  // carries the creature, so walking over it cannot shove the parked block. Pulling the handle undocks it.
  static COgheTapRail NextCrate(ExpansionContext c,string label,Vector3 start,Vector3 axis,float travel,Vector3 size,VenomSurfacePatch floor,Vector3 handle,Vector3 stance,float mass=.06f,float resistance=.012f,float initial=0,bool slickRisers=false)
  {
   var rail=ExpansionRail(c,label+" block",start,axis,travel,initial,size,mass,resistance,false,true);rail.LatchAtStart=rail.LatchAtEnd=true;
   var prop=rail.GetComponent<VenomMovableProp>();prop.Manipulable=false;prop.ManipulationGrip.localPosition=handle;
   var task=rail.gameObject.AddComponent<COgheTapRail>();task.Rail=rail;task.Handle=prop.ManipulationGrip;task.WorkingSurface=floor;task.Label=label;
   task.StandOffset=stance;task.PickHandleOnly=true;task.TrackStandPoint=true;task.TouchSize=new Vector3(.085f,.07f,.07f);task.StallSeconds=5;
   int first=c.Surfaces.Count;ViewBlock(c,label+" docked block",start+axis*travel,size);
   var docked=c.Surfaces.GetRange(first,c.Surfaces.Count-first).ToArray();foreach(var s in docked)s.gameObject.SetActive(false);
   var deck=rail.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=rail;deck.MovingSurfaces=rail.GetComponentsInChildren<VenomSurfacePatch>(true);deck.DockedSurfaces=docked;
   if(slickRisers)foreach(var face in deck.MovingSurfaces.Concat(docked))if(face.Normal.y<.9f)face.Slippery=true;
   return task;
  }
  // A loose crate (no rail) pushed with the shared prop manipulation; it cannot tip, it slides and rides decks.
  static VenomMovableProp NextLooseCrate(ExpansionContext c,string name,Vector3 centre,Vector3 size,float mass,Vector3? handleSide=null)
  {
   var prop=Prop(c.Root,name,centre,size,true,plastic,c.Surfaces);c.Props.Add(prop);
   prop.Body.mass=mass;prop.Body.constraints=RigidbodyConstraints.FreezeRotation;prop.Body.maxDepenetrationVelocity=.2f;prop.Body.centerOfMass=Vector3.zero;
   if(handleSide.HasValue)
   {
    // A short push bar: the hand holds its tip, so the body can shove the crate over a lip without standing on it.
    Vector3 side=handleSide.Value.normalized;float half=Mathf.Abs(Vector3.Dot(size*.5f,new Vector3(Mathf.Abs(side.x),Mathf.Abs(side.y),Mathf.Abs(side.z))));
    var bar=MechanismVisual(prop.transform,name+" push bar",side*(half+.02f),new Vector3(Mathf.Abs(side.x)>.5f?.04f:.012f,.008f,Mathf.Abs(side.z)>.5f?.04f:.012f),metal);
    var grip=MechanismVisual(prop.transform,name+" push grip",side*(half+.04f),new Vector3(.03f,.012f,.012f),metal);grip.localRotation=Quaternion.LookRotation(side);
    prop.ManipulationGrip=grip;
   }
   TrimSideSlabs(prop);return prop;
  }
  // Each prop face is its own 8 mm box. Side boxes reaching the top plane leave an internal seam that a sliding
  // crate catches on (PhysX ghost edge). Trim side slabs clear of the top/bottom slabs; the outer shape is unchanged.
  static void TrimSideSlabs(Component prop)
  {
   foreach(var face in prop.GetComponentsInChildren<VenomSurfacePatch>(true))
   {
    if(Mathf.Abs(face.Normal.y)>.5f||!(face.Shape is BoxCollider box))continue;
    var size=box.size;size.y=Mathf.Max(.002f,size.y-.018f);box.size=size;
   }
  }
  // A thin printed outline on a horizontal surface (four 3 mm strips); presentation only.
  static void NextOutline(ExpansionContext c,string name,Vector3 centre,Vector2 size)
  {
   foreach(float s in new[]{-1f,1f})
   {
    MechanismVisual(c.Root,name,centre+new Vector3(s*size.x*.5f,0,0),new Vector3(.003f,.0008f,size.y),metal);
    MechanismVisual(c.Root,name,centre+new Vector3(0,0,s*size.y*.5f),new Vector3(size.x,.0008f,.003f),metal);
   }
  }
  // A fixed climbable ramp from a to b (rising along z); its top edge meets the next top flush.
  static VenomSurfacePatch NextRamp(ExpansionContext c,string name,Vector3 a,Vector3 b,float width)
  {
   Vector3 d=b-a;Vector3 side=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(d,Vector3.up)).normalized;Vector3 normal=Vector3.Cross(d.normalized,side);if(normal.y<0)normal=-normal;
   return Panel(c.Root,name,(a+b)*.5f,normal,new Vector2(width,d.magnitude),stone,false,Vector2.zero,0,c.Surfaces);
  }
  static Transform NextMarker(ExpansionContext c,string name,Vector3 point)
  {var t=new GameObject(name).transform;t.SetParent(c.Root,false);t.localPosition=point;return t;}

  // 12 · Two blocks on crossing rails. The low block parks in the crossing; it must wait aside while the tall block
  // passes to the island, then return to its foot. Slick risers of 3 cm are the only steps the body can mount.
  static void Next12(ExpansionContext c)
  {
   c.Exit=new Vector3(.27f,-.142f,.30f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit island",new Vector3(.27f,-.255f,.175f),new Vector3(.22f,.09f,.23f));
   NextCrate(c,"A",new Vector3(-.20f,-.27f,.14f),Vector3.right,.296f,new Vector3(.12f,.06f,.14f),floor,new Vector3(0,0,-.078f),new Vector3(0,0,-.052f),.05f,.012f,0,true);
   NextCrate(c,"B",new Vector3(-.028f,-.285f,-.14f),Vector3.forward,.28f,new Vector3(.12f,.03f,.14f),floor,new Vector3(0,0,-.078f),new Vector3(0,0,-.052f),.04f,.010f,.28f,true);
   NextOutline(c,"A parking outline",new Vector3(.096f,-.2995f,.14f),new Vector2(.125f,.145f));
   NextOutline(c,"B waiting bay outline",new Vector3(-.028f,-.2995f,-.14f),new Vector2(.125f,.145f));
  }
  // 13 · A loose crate rides the lift with the body, then becomes the last step beside the raised exit bench.
  static void Next13(ExpansionContext c)
  {
   c.Exit=new Vector3(.335f,.058f,.30f);c.Spawn=new Vector3(-.30f,-.22f,-.16f);NextShell(c,.30f,true);
   // Landings sit 2 mm above (lower) and below (upper) the tray so a sliding crate drops onto the next deck instead of catching its edge.
   var lower=Top(NextPlinth(c,"Lower landing",new Vector3(-.255f,-.284f,0),new Vector3(.29f,.032f,.59f)));
   var upper=Top(NextPlinth(c,"Upper landing",new Vector3(.203f,-.186f,.16f),new Vector3(.13f,.228f,.27f)));
   NextPlinth(c,"Exit bench",new Vector3(.3315f,-.155f,.16f),new Vector3(.127f,.29f,.27f));
   var rail=ExpansionRail(c,"Passenger tray",new Vector3(.014f,-.283f,.16f),Vector3.up,.20f,0,new Vector3(.24f,.026f,.24f),.05f,.004f,true,false);rail.GetComponent<VenomMovableProp>().Manipulable=false;
   var lift=rail.gameObject.AddComponent<COghePassengerLift>();lift.Rail=rail;foreach(var face in rail.GetComponentsInChildren<VenomSurfacePatch>())face.MotionFrame=rail.Body;TrimSideSlabs(rail);
   lift.DeckSize=new Vector2(.24f,.24f);lift.DeckHeight=.013f;lift.Deck=rail.GetComponentsInChildren<VenomSurfacePatch>().First(p=>p.Normal.y>.9f);lift.CarriesProps=true;lift.MaximumForce=4;
   lift.BoardPoint=new GameObject("Rider stance").transform;lift.BoardPoint.SetParent(rail.transform,false);lift.BoardPoint.localPosition=new Vector3(-.064f,.031f,0);
   lift.Panel=MechanismVisual(rail.transform,"A lift panel",new Vector3(.09f,.018f,-.09f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder);
   lift.CallPanels=new[]{MechanismVisual(c.Root,"A lower call panel",new Vector3(-.14f,-.262f,-.04f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder),
    MechanismVisual(c.Root,"A upper call panel",new Vector3(.20f,-.062f,.05f),new Vector3(.04f,.010f,.04f),metal,PrimitiveType.Cylinder)};
   foreach(float x in new[]{-.12f,.14f})MechanismVisual(c.Root,"Lift upright",new Vector3(x,-.18f,.285f),new Vector3(.012f,.24f,.012f),metal);
   var crate=NextLooseCrate(c,"A crate",new Vector3(-.25f,-.2525f,.16f),new Vector3(.10f,.03f,.10f),.04f);
   var socket=new GameObject("A crate socket",typeof(COghePropSocket)).GetComponent<COghePropSocket>();socket.transform.SetParent(c.Root,false);socket.transform.localPosition=new Vector3(.214f,-.057f,.16f);
   socket.Prop=crate;socket.Socket=socket.transform;socket.Pawl=MechanismVisual(c.Root,"A socket pawl",new Vector3(.266f,-.069f,.16f),new Vector3(.006f,.006f,.05f),metal);
   NextOutline(c,"Upper crate socket outline",new Vector3(.214f,-.0715f,.16f),new Vector2(.106f,.106f));
  }
  // 14 · A slick partition splits the room; pulling A lifts the cap of a clear U transfer tube over it.
  static void Next14(ExpansionContext c)
  {
   c.Exit=new Vector3(.27f,-.225f,.30f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Low partition",new Vector3(0,-.23f,0),new Vector3(.03f,.14f,.59f));
   var tube=ChapterTube(c,"U transfer tube",new Vector3(-.25f,-.255f,.12f),new Vector3(-.17f,-.252f,.12f),new Vector3(-.10f,-.21f,.12f),new Vector3(-.06f,-.13f,.12f),new Vector3(0,-.10f,.12f),
    new Vector3(.06f,-.13f,.12f),new Vector3(.10f,-.21f,.12f),new Vector3(.17f,-.252f,.12f),new Vector3(.25f,-.255f,.12f));
   var cap=ViewGate(c,"A tube cap",new Vector3(-.272f,-.253f,.12f),Vector3.up,.12f,new Vector3(.012f,.092f,.092f));
   tube.EntryBlocker=cap.GetComponent<VenomMovableProp>().CollisionShapes[0];
   var a=ViewTask(c,"A",new Vector3(-.30f,-.277f,-.12f),Vector3.right,.10f,floor);
   ViewLink(c,a.Rail,cap,false,null);
  }
  // 15 · A Y network: the left branch reaches the balcony handle that latches the right branch door open.
  static void Next15(ExpansionContext c)
  {
   // The exit is cut in the right pane so the raised branch door never hides it from the camera.
   c.Exit=new Vector3(.40f,-.052f,.19f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.08f,-.25f,-.20f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var balcony=Top(NextPlinth(c,"Left balcony",new Vector3(-.28f,-.21f,.085f),new Vector3(.22f,.18f,.33f)));
   NextPlinth(c,"Right landing",new Vector3(.27f,-.21f,.12f),new Vector3(.24f,.18f,.34f));
   var nodes=new[]{new COgheTubeNetwork.Node("Vào",new Vector3(0,-.255f,-.20f),COgheTubeNetwork.TerminalKind.Entry),
    new COgheTubeNetwork.Node("Y",new Vector3(0,-.10f,.02f)),
    new COgheTubeNetwork.Node("Ban công",new Vector3(-.215f,-.075f,.10f),COgheTubeNetwork.TerminalKind.Entry),
    new COgheTubeNetwork.Node("Bến phải",new Vector3(.168f,-.078f,.12f),COgheTubeNetwork.TerminalKind.Entry)};
   var edges=new[]{Edge("Vào–Y",0,1,nodes,new Vector3(0,-.22f,-.12f),new Vector3(0,-.15f,-.05f)),
    Edge("Y–Ban công",1,2,nodes,new Vector3(-.08f,-.085f,.06f),new Vector3(-.15f,-.075f,.10f)),
    Edge("Y–Bến phải",1,3,nodes,new Vector3(.07f,-.09f,.07f),new Vector3(.12f,-.08f,.12f))};
   var tube=TubeNetwork(c.Root,"Y transfer tube",nodes,edges,.038f,glass);tube.CaptureSurfaceCommandsWhileInside=true;
   var door=ViewGate(c,"A branch door",new Vector3(.25f,-.075f,.12f),Vector3.up,.12f,new Vector3(.012f,.09f,.09f));
   edges[2].AccessGate=door;
   var a=ViewTask(c,"A",new Vector3(-.34f,-.097f,.02f),Vector3.right,.08f,balcony);
   ViewLink(c,a.Rail,door,false,null);
  }
  // 16 · Q splits the whole body 50/50; each half loads one pad; together they slide the step bridge out of the
  // platform until its pawl catches, then both halves may leave and merge.
  static void Next16(ExpansionContext c)
  {
   c.Exit=new Vector3(0,-.172f,.30f);c.Spawn=new Vector3(-.10f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   var q=NextQuantum(c,new Vector3(-.10f,-.30f,-.18f));
   // Platform 6 cm high (slick) with a drawer slot at floor level for the step bridge.
   Panel(c.Root,"Exit platform",new Vector3(0,-.24f,.205f),Vector3.up,new Vector2(.26f,.17f),stone,false,Vector2.zero,0,c.Surfaces);
   foreach(float x in new[]{-1f,1f})Panel(c.Root,"Exit platform",new Vector3(x*.13f,-.27f,.205f),Vector3.right*x,new Vector2(.17f,.06f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   Panel(c.Root,"Exit platform",new Vector3(0,-.27f,.29f),Vector3.forward,new Vector2(.26f,.06f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   Panel(c.Root,"Exit platform lintel",new Vector3(0,-.253f,.12f),Vector3.back,new Vector2(.26f,.026f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   foreach(float x in new[]{-1f,1f})Panel(c.Root,"Exit platform cheek",new Vector3(x*.1175f,-.283f,.12f),Vector3.back,new Vector2(.025f,.034f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   var bridge=ExpansionRail(c,"Step bridge",new Vector3(0,-.2845f,.19f),Vector3.back,.10f,0,new Vector3(.20f,.029f,.10f),.03f,.004f,false,false);
   bridge.GetComponent<VenomMovableProp>().Manipulable=false;TrimSideSlabs(bridge);
   int first=c.Surfaces.Count;ViewBlock(c,"Step bridge docked",new Vector3(0,-.2845f,.09f),new Vector3(.20f,.029f,.10f));
   var docked=c.Surfaces.GetRange(first,c.Surfaces.Count-first).ToArray();foreach(var d in docked)d.gameObject.SetActive(false);
   var deck=bridge.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=bridge;deck.MovingSurfaces=bridge.GetComponentsInChildren<VenomSurfacePatch>(true);deck.DockedSurfaces=docked;
   var a=ExpansionPad(c,"A",new Vector3(-.27f,-.298f,.02f),.009f,.10f);var b=ExpansionPad(c,"B",new Vector3(.27f,-.298f,.02f),.009f,.10f);
   var latch=new GameObject("A B bridge locks",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();latch.transform.SetParent(c.Root,false);
   latch.Inputs=new[]{a,b};latch.Output=bridge;
   latch.Pins=new[]{MechanismVisual(c.Root,"A bridge lock",new Vector3(-.115f,-.255f,.118f),new Vector3(.012f,.02f,.012f),metal),MechanismVisual(c.Root,"B bridge lock",new Vector3(.115f,-.255f,.118f),new Vector3(.012f,.02f,.012f),metal)};
   latch.Pawl=MechanismVisual(c.Root,"Bridge pawl",new Vector3(0,-.238f,.118f),new Vector3(.03f,.006f,.008f),metal);
   NextTrace("A",new Vector3(-.27f,-.2992f,.07f),new Vector3(-.27f,-.2992f,.10f),new Vector3(-.115f,-.2992f,.10f),new Vector3(-.115f,-.2992f,.118f));
   NextTrace("B",new Vector3(.27f,-.2992f,.07f),new Vector3(.27f,-.2992f,.10f),new Vector3(.115f,-.2992f,.10f),new Vector3(.115f,-.2992f,.118f));
  }
  // 17 · Q splits in the left room. One half holds pad A (tube cap held open); the other flows through the tube to
  // the right room and pulls B, which latches the return door and the cap open. The holder walks back; they merge.
  static void Next17(ExpansionContext c)
  {
   // The exit sits at the back of the right room, away from the door and B, so no route brushes past it.
   c.Exit=new Vector3(.40f,-.225f,.20f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.16f,.0775f),new Vector3(.03f,.28f,.435f));
   NextPlinth(c,"Partition wall",new Vector3(.05f,-.16f,-.2775f),new Vector3(.03f,.28f,.035f));
   var door=ViewGate(c,"B return door",new Vector3(.05f,-.16f,-.20f),Vector3.up,.20f,new Vector3(.012f,.28f,.114f));
   NextQuantum(c,new Vector3(-.18f,-.30f,-.08f));
   var a=ExpansionPad(c,"A",new Vector3(-.30f,-.298f,.17f),.009f,.10f);
   // The mouth faces the camera, so the lifted cap sits above it without hiding it.
   var tube=ChapterTube(c,"Transfer tube",new Vector3(-.10f,-.255f,.12f),new Vector3(-.10f,-.25f,.18f),new Vector3(-.07f,-.19f,.225f),new Vector3(-.02f,-.06f,.23f),new Vector3(.05f,.04f,.23f),
    new Vector3(.12f,-.06f,.23f),new Vector3(.17f,-.19f,.225f),new Vector3(.20f,-.25f,.18f),new Vector3(.20f,-.255f,.12f));
   var cap=ViewGate(c,"A tube cap",new Vector3(-.10f,-.253f,.098f),Vector3.up,.12f,new Vector3(.092f,.092f,.012f));
   tube.EntryBlocker=cap.GetComponent<VenomMovableProp>().CollisionShapes[0];
   var b=ViewTask(c,"B",new Vector3(.25f,-.277f,-.02f),Vector3.right,.08f,floor);
   ViewLink(c,b.Rail,door,false,null);
   var safe=new GameObject("Tube cap safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(-.10f,-.255f,.10f);
   safe.Network=tube;safe.Size=new Vector3(.05f,.10f,.10f);
   var hold=new GameObject("A holds, B latches the tube cap",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a};hold.Rails=new[]{b.Rail};hold.Output=cap;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   NextTrace("A",new Vector3(-.25f,-.2992f,.17f),new Vector3(-.16f,-.2992f,.17f),new Vector3(-.16f,-.2992f,.09f),new Vector3(-.146f,-.2992f,.09f));
   NextTrace("B",new Vector3(.25f,-.2992f,.02f),new Vector3(.25f,-.2992f,.05f),new Vector3(.08f,-.2992f,.05f),new Vector3(.08f,-.2992f,-.14f));
  }
  // 18 · Two high banks with a gap. Grip ring A, choose the far bank; the winch hook releases and gravity swings the
  // body across. It lets go only inside the far bank's contact envelope. Below is a rescue floor with a climb back.
  static void Next18(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.132f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.11f,.12f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var left=NextPlinth(c,"Start bank",new Vector3(-.28f,-.23f,.10f),new Vector3(.24f,.14f,.40f));
   NextRamp(c,"Recovery ramp",new Vector3(-.30f,-.30f,-.29f),new Vector3(-.30f,-.16f,-.10f),.12f); // the climb back up from the rescue floor
   // Placed on the measured arc of the loaded rope: the edge sits just short of the apex and the top 2 cm under the
   // body's lowest point there, so the body clears the lip and settles on the tray (not a symmetric ideal pendulum).
   var right=NextPlinth(c,"Receiving bank",new Vector3(.2575f,-.25f,.12f),new Vector3(.285f,.10f,.36f));
   NextSwing(c,"A",new Vector3(0,.14f,.12f),.28f,40f,new Vector3(-.23f,-.14f,.12f),Top(left),new[]{Top(right)},new[]{new Vector3(.25f,-.18f,.12f)},floor);
  }
  // 19 · Same rope as 18, but the landing tray rides a rail and starts beyond the arc. B on the floor pulls it into the
  // arc (its catch holds it); a swing to the far tray misses and the winch returns the body to the start hook.
  static void Next19(ExpansionContext c)
  {
   c.Exit=new Vector3(.215f,-.132f,.30f);c.Outward=Vector3.forward;c.Spawn=new Vector3(-.10f,-.25f,-.20f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var left=NextPlinth(c,"Start bank",new Vector3(-.28f,-.23f,.10f),new Vector3(.24f,.14f,.40f));
   NextRamp(c,"Start ramp",new Vector3(-.30f,-.30f,-.29f),new Vector3(-.30f,-.16f,-.10f),.12f); // the fixed way up from the floor
   var tray=ExpansionRail(c,"B landing tray",new Vector3(.295f,-.215f,.13f),Vector3.left,.08f,0,new Vector3(.20f,.03f,.32f),.05f,.01f,false,false);
   tray.GetComponent<VenomMovableProp>().Manipulable=false;tray.LatchAtEnd=true;TrimSideSlabs(tray);
   foreach(var face in tray.GetComponentsInChildren<VenomSurfacePatch>())if(face.Normal.y<.9f)face.Slippery=true;
   int first=c.Surfaces.Count;var dockedTop=Panel(c.Root,"B landing tray docked",new Vector3(.215f,-.20f,.13f),Vector3.up,new Vector2(.20f,.32f),stone,false,Vector2.zero,0,c.Surfaces);
   dockedTop.gameObject.SetActive(false);
   var deck=tray.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=tray;deck.MovingSurfaces=tray.GetComponentsInChildren<VenomSurfacePatch>(true);deck.DockedSurfaces=new[]{dockedTop};
   foreach(float z in new[]{-.03f,.29f})MechanismVisual(c.Root,"B tray rail",new Vector3(.255f,-.232f,z),new Vector3(.30f,.006f,.008f),metal);
   foreach(float x in new[]{.12f,.39f})MechanismVisual(c.Root,"B tray rail post",new Vector3(x,-.266f,-.03f),new Vector3(.008f,.068f,.008f),metal);
   var b=ViewTask(c,"B",new Vector3(-.06f,-.277f,-.14f),Vector3.right,.08f,floor);
   ViewLink(c,b.Rail,tray,false,null);
   var landing=tray.GetComponentsInChildren<VenomSurfacePatch>(true).First(f=>f.Normal.y>.9f);
   NextSwing(c,"A",new Vector3(0,.14f,.12f),.28f,40f,new Vector3(-.23f,-.14f,.12f),Top(left),new[]{landing,dockedTop},new[]{new Vector3(.25f,-.18f,.13f),new Vector3(.25f,-.18f,.13f)},floor);
   NextTrace("B",new Vector3(.02f,-.2992f,-.11f),new Vector3(.02f,-.2992f,-.06f),new Vector3(.20f,-.2992f,-.06f),new Vector3(.20f,-.2992f,-.03f));
  }
  // 20 · BOSS. Q splits; one half holds pad A (tube cap open) while the other flows up to the balcony and pulls B:
  // B latches the cap and lifts the gate at the head of the return ramp. Merged, the whole body pushes crate C into its
  // socket, climbs the winch bench and, with its full mass, winds winch D to raise the span to the exit platform.
  // Layout grid: back row = bench | span pit | exit platform | gap | balcony; front-left = Q + crate lane;
  // front-right = pad A, tube mouth, return ramp; a 13 cm corridor runs between them.
  static void Next20(ExpansionContext c)
  {
   c.Exit=new Vector3(-.02f,-.172f,.30f);c.Outward=Vector3.forward;c.Spawn=new Vector3(-.10f,-.25f,-.255f);c.Definition.CameraEuler=new Vector3(42,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(-.10f,-.30f,-.16f),.025f,.13f);
   var a=ExpansionPad(c,"A",new Vector3(.13f,-.298f,-.25f),.009f,.09f);
   var balcony=Top(NextPlinth(c,"B balcony",new Vector3(.27f,-.20f,.21f),new Vector3(.26f,.20f,.18f)));
   // The pipe climbs the balcony's front face and ends in a short level stub on its left side, mouth facing back.
   var tube=ChapterTube(c,"Balcony tube",new Vector3(.20f,-.255f,.02f),new Vector3(.20f,-.24f,.065f),new Vector3(.20f,-.16f,.085f),new Vector3(.20f,-.09f,.10f),new Vector3(.20f,-.057f,.135f),new Vector3(.20f,-.057f,.18f));
   var cap=ViewGate(c,"A tube cap",new Vector3(.20f,-.253f,-.002f),Vector3.up,.12f,new Vector3(.092f,.092f,.012f));
   tube.EntryBlocker=cap.GetComponent<VenomMovableProp>().CollisionShapes[0];
   var b=ViewTask(c,"B",new Vector3(.29f,-.077f,.27f),Vector3.right,.07f,balcony);b.StandOffset=new Vector3(0,0,-.05f);
   NextRamp(c,"Return ramp",new Vector3(.36f,-.30f,-.10f),new Vector3(.36f,-.10f,.12f),.08f);
   var gate=ViewGate(c,"B return gate",new Vector3(.358f,-.069f,.11f),Vector3.up,.08f,new Vector3(.074f,.06f,.012f));
   ViewLink(c,b.Rail,gate,false,null);
   var safe=new GameObject("Tube cap safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(.20f,-.255f,-.01f);safe.Network=tube;safe.Size=new Vector3(.10f,.10f,.05f);
   var hold=new GameObject("A holds, B latches the tube cap",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a};hold.Rails=new[]{b.Rail};hold.Output=cap;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   var bench=Top(NextPlinth(c,"Winch bench",new Vector3(-.30f,-.27f,.21f),new Vector3(.20f,.06f,.18f)));
   NextPlinth(c,"Exit platform",new Vector3(-.02f,-.27f,.21f),new Vector3(.16f,.06f,.18f));
   NextCrate(c,"C",new Vector3(-.34f,-.285f,-.02f),Vector3.forward,.096f,new Vector3(.09f,.03f,.08f),floor,new Vector3(0,0,-.048f),new Vector3(0,0,-.052f),.04f,.01f,0,true);
   // Handle stroke = span travel + cable stretch at the load (0.5 N / 45 N/m): the handle ends as the span docks.
   var winch=ViewTask(c,"D",new Vector3(-.355f,-.217f,.255f),Vector3.right,.046f,bench);winch.CompensateLoad=true;winch.StallSeconds=6;
   var span=ExpansionRail(c,"Winch lifting span",new Vector3(-.15f,-.2855f,.21f),Vector3.up,.0325f,0,new Vector3(.094f,.026f,.17f),.05f,.01f,true,false);
   span.GetComponent<VenomMovableProp>().Manipulable=false;span.CatchTolerance=.004f;span.LatchAtEnd=true;TrimSideSlabs(span);
   foreach(var face in span.GetComponentsInChildren<VenomSurfacePatch>())if(face.Normal.y<.9f)face.Slippery=true;
   var cable=new GameObject("D winch cable").AddComponent<COghePulleyDrive>();cable.transform.SetParent(c.Root,false);cable.Stiffness=45;cable.Input=winch.Rail;cable.Command=winch;cable.Output=span;
   cable.Cable=cable.gameObject.AddComponent<LineRenderer>();cable.Cable.sharedMaterial=metal;cable.Cable.startWidth=cable.Cable.endWidth=.0028f;cable.Cable.useWorldSpace=true;
   var guides=new List<Transform>();foreach(var g in new[]{new Vector3(-.355f,.12f,.255f),new Vector3(-.15f,.12f,.21f)}){guides.Add(NextMarker(c,"D rope guide",g));var wheel=MechanismVisual(c.Root,"A pulley wheel",g,new Vector3(.05f,.014f,.05f),metal,PrimitiveType.Cylinder);wheel.localRotation=Quaternion.Euler(90,0,0);}
   cable.Guides=guides.ToArray();
   NextTrace("A",new Vector3(.13f,-.2992f,-.205f),new Vector3(.13f,-.2992f,-.03f),new Vector3(.154f,-.2992f,-.03f),new Vector3(.154f,-.2992f,-.015f));
   NextTrace("B",new Vector3(.33f,-.0992f,.29f),new Vector3(.385f,-.0992f,.29f),new Vector3(.385f,-.0992f,.13f));
  }
  // A raised deck (slick sides) with an optional rectangular pit cut in its top; the pit walls are slick.
  static VenomSurfacePatch NextDeckWithPit(ExpansionContext c,string name,Rect deck,float top,Rect pit)
  {
   VenomSurfacePatch first=null;
   void Top(float x0,float x1,float z0,float z1){if(x1-x0<.001f||z1-z0<.001f)return;var p=Panel(c.Root,name,new Vector3((x0+x1)*.5f,top,(z0+z1)*.5f),Vector3.up,new Vector2(x1-x0,z1-z0),stone,false,Vector2.zero,0,c.Surfaces);if(first==null)first=p;}
   Top(deck.xMin,deck.xMax,deck.yMin,pit.yMin);Top(deck.xMin,deck.xMax,pit.yMax,deck.yMax);Top(deck.xMin,pit.xMin,pit.yMin,pit.yMax);Top(pit.xMax,deck.xMax,pit.yMin,pit.yMax);
   float h=top+.30f,mid=(top-.30f)*.5f;
   void Side(Vector3 centre,Vector3 normal,Vector2 size){Panel(c.Root,name+" side",centre,normal,size,stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;}
   Side(new Vector3(deck.xMax,mid,deck.center.y),Vector3.right,new Vector2(deck.height,h));Side(new Vector3(deck.xMin,mid,deck.center.y),Vector3.left,new Vector2(deck.height,h));
   Side(new Vector3(deck.center.x,mid,deck.yMax),Vector3.forward,new Vector2(deck.width,h));Side(new Vector3(deck.center.x,mid,deck.yMin),Vector3.back,new Vector2(deck.width,h));
   // Pit walls stop 4 mm under the deck top so a crate sliding over the lip meets no coplanar seam.
   float ph=h-.004f,pm=mid-.002f;
   if(pit.width>0){Side(new Vector3(pit.xMin,pm,pit.center.y),Vector3.right,new Vector2(pit.height,ph));Side(new Vector3(pit.xMax,pm,pit.center.y),Vector3.left,new Vector2(pit.height,ph));
    Side(new Vector3(pit.center.x,pm,pit.yMin),Vector3.forward,new Vector2(pit.width,ph));Side(new Vector3(pit.center.x,pm,pit.yMax),Vector3.back,new Vector2(pit.width,ph));}
   return first;
  }
  // 21 · A plank on a real axle, held tilted by its own weight. A crate pushed onto the sinking load tray tensions a
  // rope (over two pulleys) that lifts the plank's far end until it rests level on its bearer; a pawl catches it.
  static void Next21(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.132f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.15f,-.18f);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var deck=NextDeckWithPit(c,"Load deck",new Rect(-.40f,-.30f,.25f,.60f),-.20f,new Rect(-.34f,.05f,.12f,.12f));
   NextPlinth(c,"Exit platform",new Vector3(.275f,-.25f,.125f),new Vector3(.25f,.10f,.35f));
   NextRamp(c,"Gap recovery ramp",new Vector3(0,-.30f,-.28f),new Vector3(-.13f,-.20f,-.28f),.06f);
   var tray=ExpansionRail(c,"Load tray",new Vector3(-.28f,-.215f,.11f),Vector3.down,.066f,0,new Vector3(.115f,.026f,.115f),.01f,.004f,true,false);
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
   seesaw.Plank=plank.Body;seesaw.Hinge=hinge;seesaw.Anchor=anchor;seesaw.Tray=tray;seesaw.LevelLocalRotation=Quaternion.identity;
   seesaw.MovingSurfaces=plank.GetComponentsInChildren<VenomSurfacePatch>(true);seesaw.DockedSurfaces=docked;
   seesaw.Guides=new[]{NextMarker(c,"Rope pulley over tray",new Vector3(-.28f,.12f,.11f)),NextMarker(c,"Rope pulley over plank",new Vector3(.13f,.12f,.10f))};
   foreach(var g in seesaw.Guides){var wheel=MechanismVisual(c.Root,"A pulley wheel",g.localPosition,new Vector3(.05f,.014f,.05f),metal,PrimitiveType.Cylinder);wheel.localRotation=Quaternion.Euler(90,0,0);}
   seesaw.Rope=seesaw.gameObject.AddComponent<LineRenderer>();seesaw.Rope.useWorldSpace=true;seesaw.Rope.startWidth=seesaw.Rope.endWidth=.0028f;seesaw.Rope.sharedMaterial=metal;
   seesaw.Pawl=MechanismVisual(c.Root,"Seesaw pawl",new Vector3(-.13f,-.215f,.155f),new Vector3(.01f,.012f,.01f),metal);
   NextOutline(c,"Load tray outline",new Vector3(-.28f,-.1995f,.11f),new Vector2(.125f,.125f));
  }
  // 22 · Three rail pieces build low-high-level steps over a floor gap to the exit platform. The tall block B starts
  // in its socket and blocks the path to span C's handle, so B waits aside, C goes in, then B, then the low block A.
  static void Next22(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.172f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.28f,-.25f,-.20f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Exit platform",new Vector3(.28f,-.27f,.10f),new Vector3(.24f,.06f,.40f));
   // A waits at the back and is pulled forward into its socket (its handle faces the player).
   NextCrate(c,"A",new Vector3(-.12f,-.285f,.25f),Vector3.back,.17f,new Vector3(.10f,.03f,.10f),floor,new Vector3(0,0,-.058f),new Vector3(0,0,-.052f),.04f,.01f,0,true);
   var b=NextCrate(c,"B",new Vector3(-.30f,-.27f,.08f),Vector3.right,.284f,new Vector3(.10f,.06f,.10f),floor,new Vector3(-.058f,-.015f,0),new Vector3(-.052f,0,0),.05f,.012f,.284f,true);
   b.Handle.localRotation=Quaternion.Euler(0,90,0);
   var span=NextCrate(c,"C",new Vector3(.095f,-.255f,-.22f),Vector3.forward,.30f,new Vector3(.12f,.03f,.12f),floor,new Vector3(-.068f,-.015f,0),new Vector3(-.07f,0,0),.04f,.01f,0,true);
   span.Handle.localRotation=Quaternion.Euler(0,90,0);span.Rail.GetComponent<VenomMovableProp>().Body.mass=.04f;
   foreach(var bearer in new[]{.045f,.148f})ViewBlock(c,"Span bearer",new Vector3(bearer,-.286f,.08f),new Vector3(.006f,.028f,.10f));
   NextOutline(c,"B waiting bay",new Vector3(-.30f,-.2995f,.08f),new Vector2(.105f,.105f));
   NextOutline(c,"A socket",new Vector3(-.12f,-.2995f,.08f),new Vector2(.105f,.105f));
   NextOutline(c,"B socket",new Vector3(-.016f,-.2995f,.08f),new Vector2(.105f,.105f));
  }
  // 23 · The lower route leads through junction J to the maintenance ledge, where A turns J's valve onto the upper
  // route. The upper route crosses over the lower one (14 cm apart, no joint) to the high tray and exit.
  static void Next23(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,.048f,.21f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.30f,-.25f,-.22f);c.Definition.CameraEuler=new Vector3(40,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var ledge=Top(NextPlinth(c,"Maintenance ledge",new Vector3(.27f,-.23f,-.11f),new Vector3(.26f,.14f,.26f)));
   NextPlinth(c,"High tray",new Vector3(.27f,-.16f,.20f),new Vector3(.26f,.28f,.20f));
   var nodes=new[]{new COgheTubeNetwork.Node("Vào",new Vector3(-.28f,-.255f,-.12f),COgheTubeNetwork.TerminalKind.Entry),
    new COgheTubeNetwork.Node("J",new Vector3(-.02f,-.10f,-.02f)),
    new COgheTubeNetwork.Node("Bệ bảo trì",new Vector3(.165f,-.115f,-.10f),COgheTubeNetwork.TerminalKind.Entry),
    new COgheTubeNetwork.Node("Khay cao",new Vector3(.165f,.025f,.20f),COgheTubeNetwork.TerminalKind.Entry)};
   var edges=new[]{Edge("Vào–J",0,1,nodes,new Vector3(-.20f,-.25f,-.12f),new Vector3(-.11f,-.17f,-.07f)),
    Edge("J–Bệ",1,2,nodes,new Vector3(.05f,-.13f,-.05f),new Vector3(.12f,-.115f,-.10f)),
    Edge("J–Khay cao",1,3,nodes,new Vector3(.02f,-.03f,-.10f),new Vector3(.09f,.02f,.04f),new Vector3(.13f,.025f,.20f))};
   var tube=TubeNetwork(c.Root,"Wall route network",nodes,edges,.038f,glass);tube.CaptureSurfaceCommandsWhileInside=true;
   var a=ViewTask(c,"A",new Vector3(.23f,-.137f,-.19f),Vector3.right,.08f,ledge);
   edges[2].AccessGate=a.Rail;
   var safe=new GameObject("Valve safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.Network=tube;safe.Size=Vector3.zero;a.Clearance=safe;
   MechanismVisual(c.Root,"J valve selector",new Vector3(-.02f,-.10f,-.075f),new Vector3(.03f,.03f,.012f),metal);
   NextTrace("A",new Vector3(.23f,-.1592f,-.23f),new Vector3(.155f,-.1592f,-.23f));
  }
  // The exit platform with a drawer step (shared by 16 and 24): 6 cm slick platform, slot at floor level,
  // the step slides out toward the player when its latch drives it; a static replica carries the body when caught.
  static COgheRailSlider NextDrawerPlatform(ExpansionContext c,float x,float zFront,float depth,float width)
  {
   float zc=zFront+depth*.5f,zBack=zFront+depth;
   Panel(c.Root,"Exit platform",new Vector3(x,-.24f,zc),Vector3.up,new Vector2(width,depth),stone,false,Vector2.zero,0,c.Surfaces);
   foreach(float s in new[]{-1f,1f})Panel(c.Root,"Exit platform",new Vector3(x+s*width*.5f,-.27f,zc),Vector3.right*s,new Vector2(depth,.06f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   Panel(c.Root,"Exit platform",new Vector3(x,-.27f,zBack),Vector3.forward,new Vector2(width,.06f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   Panel(c.Root,"Exit platform lintel",new Vector3(x,-.253f,zFront),Vector3.back,new Vector2(width,.026f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   foreach(float s in new[]{-1f,1f})Panel(c.Root,"Exit platform cheek",new Vector3(x+s*(width*.5f-.0125f),-.283f,zFront),Vector3.back,new Vector2(.025f,.034f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   var bridge=ExpansionRail(c,"Step bridge",new Vector3(x,-.2845f,zFront+.07f),Vector3.back,.10f,0,new Vector3(width-.06f,.029f,.10f),.03f,.004f,false,false);
   bridge.GetComponent<VenomMovableProp>().Manipulable=false;TrimSideSlabs(bridge);
   int first=c.Surfaces.Count;ViewBlock(c,"Step bridge docked",new Vector3(x,-.2845f,zFront-.03f),new Vector3(width-.06f,.029f,.10f));
   var docked=c.Surfaces.GetRange(first,c.Surfaces.Count-first).ToArray();foreach(var d in docked)d.gameObject.SetActive(false);
   var deck=bridge.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=bridge;deck.MovingSurfaces=bridge.GetComponentsInChildren<VenomSurfacePatch>(true);deck.DockedSurfaces=docked;
   return bridge;
  }
  // 24 · Q splits 100 → 50/50; each half goes back through Q → four 25 % parts. A1, A2, B1, B2 in the four corners
  // must all carry load to slide the step out of the exit platform; then the parts leave and merge.
  static void Next24(ExpansionContext c)
  {
   c.Exit=new Vector3(0,-.172f,.30f);c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(46,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(0,-.30f,-.20f));
   var bridge=NextDrawerPlatform(c,0,.12f,.17f,.26f);
   var pads=new[]{ExpansionPad(c,"A1",new Vector3(-.33f,-.298f,-.20f),.009f,.09f),ExpansionPad(c,"A2",new Vector3(-.33f,-.298f,.14f),.009f,.09f),
    ExpansionPad(c,"B1",new Vector3(.33f,-.298f,-.20f),.009f,.09f),ExpansionPad(c,"B2",new Vector3(.33f,-.298f,.14f),.009f,.09f)};
   var latch=new GameObject("A1 A2 B1 B2 bridge locks",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();latch.transform.SetParent(c.Root,false);
   latch.Inputs=pads;latch.Output=bridge;
   latch.Pins=new[]{MechanismVisual(c.Root,"A lock 1",new Vector3(-.115f,-.255f,.118f),new Vector3(.01f,.02f,.01f),metal),MechanismVisual(c.Root,"A lock 2",new Vector3(-.095f,-.255f,.118f),new Vector3(.01f,.02f,.01f),metal),
    MechanismVisual(c.Root,"B lock 1",new Vector3(.095f,-.255f,.118f),new Vector3(.01f,.02f,.01f),metal),MechanismVisual(c.Root,"B lock 2",new Vector3(.115f,-.255f,.118f),new Vector3(.01f,.02f,.01f),metal)};
   latch.Pawl=MechanismVisual(c.Root,"Bridge pawl",new Vector3(0,-.238f,.118f),new Vector3(.03f,.006f,.008f),metal);
   NextTrace("A",new Vector3(-.33f,-.2992f,-.15f),new Vector3(-.33f,-.2992f,.09f));NextTrace("A",new Vector3(-.28f,-.2992f,.14f),new Vector3(-.105f,-.2992f,.14f),new Vector3(-.105f,-.2992f,.12f));
   NextTrace("B",new Vector3(.33f,-.2992f,-.15f),new Vector3(.33f,-.2992f,.09f));NextTrace("B",new Vector3(.28f,-.2992f,.14f),new Vector3(.105f,-.2992f,.14f),new Vector3(.105f,-.2992f,.12f));
  }
  // 25 · 50 % + 25 % + 25 %. The two small parts load A1 and A2, which draw the lock bolt from heavy block B's rail.
  // B's dry friction is tuned so 25 % stalls and 50 % pushes it; once B sits in its socket its own catch holds it.
  static void Next25(ExpansionContext c)
  {
   c.Exit=new Vector3(.31f,-.142f,.30f);c.Spawn=new Vector3(-.185f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(-.185f,-.30f,-.18f),.025f,.13f);
   NextPlinth(c,"Exit platform",new Vector3(.3125f,-.255f,.15f),new Vector3(.175f,.09f,.30f));
   NextPlinth(c,"Fixed low step",new Vector3(.17f,-.285f,.0125f),new Vector3(.10f,.03f,.085f));
   var b=NextCrate(c,"B",new Vector3(-.02f,-.27f,.12f),Vector3.right,.19f,new Vector3(.10f,.06f,.12f),floor,new Vector3(-.058f,-.015f,0),new Vector3(-.052f,0,0),.08f,.25f,0,true);
   b.Handle.localRotation=Quaternion.Euler(0,90,0);b.StallSeconds=4;b.CompensateLoad=true;
   var a1=ExpansionPad(c,"A1",new Vector3(-.31f,-.298f,.16f),.009f,.09f);var a2=ExpansionPad(c,"A2",new Vector3(.06f,-.298f,-.22f),.009f,.09f);
   var bolt=ViewGate(c,"A lock bolt",new Vector3(.05f,-.285f,.195f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var hold=new GameObject("A1 A2 draw the bolt",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a1,a2};hold.Output=bolt;hold.Retain=false;
   b.RequiredRail=bolt;b.RequiredEnd=true;
   NextTrace("A",new Vector3(-.31f,-.2992f,.21f),new Vector3(-.31f,-.2992f,.25f),new Vector3(.05f,-.2992f,.25f),new Vector3(.05f,-.2992f,.205f));
   NextTrace("A",new Vector3(.06f,-.2992f,-.175f),new Vector3(.06f,-.2992f,-.10f),new Vector3(.08f,-.2992f,-.10f),new Vector3(.08f,-.2992f,.19f));
   NextOutline(c,"B socket",new Vector3(.17f,-.2995f,.12f),new Vector2(.105f,.125f));
  }
  // 26 · Q on the start bank. One half holds pad A (tube cap open) while the other flows through the low back tube
  // onto the middle platform and pulls B: the landing tray slides out into the swing arc and the cap is latched.
  // The holder then grips rope A and swings onto the tray; the halves merge on the platform.
  static void Next26(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.160f,.12f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.20f,-.11f,-.262f);c.Definition.CameraEuler=new Vector3(40,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   var bankFront=Top(NextPlinth(c,"Start bank",new Vector3(-.20f,-.23f,-.16f),new Vector3(.40f,.14f,.28f)));
   var bankBack=Top(NextPlinth(c,"Start bank",new Vector3(-.28f,-.23f,.14f),new Vector3(.24f,.14f,.32f)));
   // A shoulder behind Q's right tray: the right half walks behind the rear housing to the back bank.
   // It stops short of the swing plane (z .14) so the rope arc never scrapes it.
   NextPlinth(c,"Start bank shoulder",new Vector3(-.125f,-.23f,.03f),new Vector3(.07f,.14f,.10f));
   NextRamp(c,"Recovery ramp",new Vector3(-.045f,-.30f,.10f),new Vector3(-.045f,-.16f,-.02f),.06f);
   NextQuantum(c,new Vector3(-.20f,-.16f,-.18f),.025f,.14f);
   var a=ExpansionPad(c,"A",new Vector3(-.34f,-.158f,.24f),.009f,.09f);
   // Middle platform (slick sides, walkable top at -.188). The landing tray is stored inside it and slides out
   // through a slot under the top of its left face, so the top stays clear for the half that arrives by tube.
   // The top slab reaches 8 mm down; the stored tray (top -.198) clears it by 2 mm.
   const float top=-.188f;float mid=(top-.30f)*.5f;
   var middle=Panel(c.Root,"Middle platform",new Vector3(.3075f,top,.05f),Vector3.up,new Vector2(.185f,.50f),stone,false,Vector2.zero,0,c.Surfaces);
   Panel(c.Root,"Middle platform",new Vector3(.3075f,mid,-.20f),Vector3.back,new Vector2(.185f,top+.30f),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   void Left(float y0,float y1,float z0,float z1)=>Panel(c.Root,"Middle platform",new Vector3(.215f,(y0+y1)*.5f,(z0+z1)*.5f),Vector3.left,new Vector2(z1-z0,y1-y0),stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   Left(-.30f,-.2255f,-.20f,.30f);Left(-.2255f,top,-.20f,.058f);Left(-.2255f,top,.222f,.30f);Left(-.1965f,top,.058f,.222f);
   var tube=ChapterTube(c,"Low back tube",new Vector3(-.25f,-.115f,.17f),new Vector3(-.25f,-.12f,.24f),new Vector3(-.18f,-.14f,.27f),new Vector3(-.10f,-.24f,.27f),new Vector3(0,-.255f,.27f),new Vector3(.10f,-.235f,.27f),new Vector3(.18f,-.18f,.27f),new Vector3(.245f,-.149f,.27f));
   var cap=ViewGate(c,"A tube cap",new Vector3(-.25f,-.113f,.148f),Vector3.up,.12f,new Vector3(.092f,.092f,.012f));
   tube.EntryBlocker=cap.GetComponent<VenomMovableProp>().CollisionShapes[0];
   // B's handle faces the back, toward the tube's landing.
   var b=ViewTask(c,"B",new Vector3(.27f,top+.023f,-.10f),Vector3.right,.07f,middle);
   b.Handle.localPosition=new Vector3(0,0,.035f);b.Handle.localRotation=Quaternion.identity;b.StandOffset=new Vector3(0,0,.05f);
   var flap=ExpansionRail(c,"B landing tray",new Vector3(.315f,-.211f,.14f),Vector3.left,.15f,0,new Vector3(.10f,.026f,.16f),.03f,.006f,false,false);
   flap.GetComponent<VenomMovableProp>().Manipulable=false;flap.LatchAtEnd=true;TrimSideSlabs(flap);
   foreach(var f in flap.GetComponentsInChildren<VenomSurfacePatch>())if(f.Normal.y<.9f)f.Slippery=true;
   int first=c.Surfaces.Count;var dockedTop=Panel(c.Root,"B landing tray docked",new Vector3(.165f,-.198f,.14f),Vector3.up,new Vector2(.10f,.16f),stone,false,Vector2.zero,0,c.Surfaces);dockedTop.gameObject.SetActive(false);
   var deck=flap.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=flap;deck.MovingSurfaces=flap.GetComponentsInChildren<VenomSurfacePatch>(true);deck.DockedSurfaces=new[]{dockedTop};
   ViewLink(c,b.Rail,flap,false,null);
   var safe=new GameObject("Tube cap safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.transform.localPosition=new Vector3(-.25f,-.115f,.14f);safe.Network=tube;safe.Size=new Vector3(.10f,.10f,.05f);
   var hold=new GameObject("A holds, B latches the tube cap",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();hold.transform.SetParent(c.Root,false);
   hold.Inputs=new[]{a};hold.Rails=new[]{b.Rail};hold.Output=cap;hold.Any=true;hold.Retain=false;hold.Clearance=safe;
   var landing=flap.GetComponentsInChildren<VenomSurfacePatch>(true).First(f=>f.Normal.y>.9f);
   NextSwing(c,"A",new Vector3(0,.14f,.14f),.28f,40f,new Vector3(-.23f,-.14f,.14f),bankBack,new[]{landing,dockedTop,middle},new[]{new Vector3(.26f,top+.02f,.14f),new Vector3(.26f,top+.02f,.14f),new Vector3(.26f,top+.02f,.14f)},floor);
   NextTrace("A",new Vector3(-.34f,-.1592f,.195f),new Vector3(-.34f,-.1592f,.15f),new Vector3(-.296f,-.1592f,.15f));
   NextTrace("B",new Vector3(.31f,-.1872f,-.135f),new Vector3(.385f,-.1872f,-.135f),new Vector3(.385f,-.1872f,.06f));
  }
  // 27 · Four 25 % parts. A1 and A2 on two floor corners draw the bolt from lever B; a third part holds B (a dead-man
  // lever) which powers the lift tray that carries the fourth up to the high platform. There the fourth pulls the
  // neutral latch C: it keeps the lift powered for everyone, so the three holders may leave, ride up and merge.
  static void Next27(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.032f,.22f);c.Outward=Vector3.right;c.Spawn=new Vector3(0,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(40,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(0,-.30f,-.20f));
   var high=Top(NextPlinth(c,"High platform",new Vector3(.24f,-.18f,.14f),new Vector3(.32f,.24f,.32f)));
   var a1=ExpansionPad(c,"A1",new Vector3(-.33f,-.298f,-.22f),.009f,.09f);var a2=ExpansionPad(c,"A2",new Vector3(.33f,-.298f,-.22f),.009f,.09f);
   var bolt=ViewGate(c,"A lock bolt",new Vector3(-.25f,-.285f,.185f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var pads=new GameObject("A1 A2 draw the bolt",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();pads.transform.SetParent(c.Root,false);
   pads.Inputs=new[]{a1,a2};pads.Output=bolt;pads.Retain=false;
   var b=ViewTask(c,"B",new Vector3(-.25f,-.277f,.14f),Vector3.right,.07f,floor);b.HoldAtEnd=true;b.RequiredRail=bolt;
   // Lift tray: 2 cm deck on the floor landing; its upper stop sits 2 mm above the high platform top, 2 mm from its face.
   var rail=ExpansionRail(c,"Lift tray",new Vector3(-.012f,-.288f,.16f),Vector3.up,.22f,0,new Vector3(.18f,.02f,.18f),.03f,.004f,true,false);
   rail.GetComponent<VenomMovableProp>().Manipulable=false;TrimSideSlabs(rail);
   foreach(var face in rail.GetComponentsInChildren<VenomSurfacePatch>()){face.MotionFrame=rail.Body;if(face.Normal.y<.9f)face.Slippery=true;}
   var lift=rail.gameObject.AddComponent<COghePassengerLift>();lift.Rail=rail;lift.DeckSize=new Vector2(.18f,.18f);lift.DeckHeight=.01f;
   lift.Deck=rail.GetComponentsInChildren<VenomSurfacePatch>().First(p=>p.Normal.y>.9f);lift.ReturnWhenDisabled=true;
   lift.BoardPoint=new GameObject("Rider stance").transform;lift.BoardPoint.SetParent(rail.transform,false);lift.BoardPoint.localPosition=new Vector3(0,.028f,0);
   lift.Panel=MechanismVisual(rail.transform,"Lift panel",new Vector3(.06f,.012f,-.06f),new Vector3(.03f,.008f,.03f),metal,PrimitiveType.Cylinder);
   lift.CallPanels=new[]{MechanismVisual(c.Root,"Lower call panel",new Vector3(-.125f,-.296f,.215f),new Vector3(.03f,.008f,.03f),metal,PrimitiveType.Cylinder),
    MechanismVisual(c.Root,"Upper call panel",new Vector3(.13f,-.058f,.27f),new Vector3(.03f,.008f,.03f),metal,PrimitiveType.Cylinder)};
   foreach(float x in new[]{-.11f,.086f})MechanismVisual(c.Root,"Lift upright",new Vector3(x,-.02f,.262f),new Vector3(.012f,.56f,.012f),metal);
   var pin=ViewGate(c,"Lift enable pin",new Vector3(-.16f,-.27f,.28f),Vector3.up,.03f,new Vector3(.012f,.012f,.012f));
   lift.RequiredRail=pin;
   var latch=ViewTask(c,"C",new Vector3(.22f,-.037f,.14f),Vector3.right,.07f,high);
   var power=new GameObject("B holds or C latches the lift power",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();power.transform.SetParent(c.Root,false);
   power.Holds=new[]{b};power.Rails=new[]{latch.Rail};power.Any=true;power.Retain=false;power.Output=pin;
   NextTrace("A",new Vector3(-.33f,-.2992f,-.175f),new Vector3(-.33f,-.2992f,.185f),new Vector3(-.26f,-.2992f,.185f));
   NextTrace("A",new Vector3(.33f,-.2992f,-.175f),new Vector3(.33f,-.2992f,-.05f),new Vector3(.06f,-.2992f,-.05f),new Vector3(.06f,-.2992f,.0f),new Vector3(-.20f,-.2992f,.0f),new Vector3(-.20f,-.2992f,.185f),new Vector3(-.24f,-.2992f,.185f));
   NextTrace("B",new Vector3(-.18f,-.2992f,.19f),new Vector3(-.18f,-.2992f,.28f),new Vector3(-.17f,-.2992f,.28f));
  }
  // 28 · Tall box, tube network with windows at three heights (J1 low, J2 middle, J3 high); J1 has a return branch to the floor.
  // Two 50 % halves: the outer half climbs the ivory faces to station A (a held valve lever for J2's maintenance
  // branch), later to station B (J3's branch to the upper balcony). The inner half flows to the maintenance balcony
  // and latches route A with C, then on to the upper balcony where D lifts the gate at the head of the outer bridge.
  static void Next28(ExpansionContext c)
  {
   // The exit is in the back pane, well away from D, so bracing on D never pushes tissue out early.
   c.Exit=new Vector3(.33f,.108f,.30f);c.Outward=Vector3.forward;c.Spawn=new Vector3(-.20f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(40,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(-.20f,-.30f,-.20f),.025f,.13f);
   // Outer route: ivory (climbable) front faces, lavender (slick) everywhere else.
   var aFaces=NextPlinth(c,"Station A balcony",new Vector3(-.29f,-.21f,.14f),new Vector3(.22f,.18f,.32f));var aTop=Top(aFaces);
   aFaces.First(f=>f.Normal.z<-.9f).Slippery=false;
   var bFaces=NextPlinth(c,"Station B balcony",new Vector3(-.29f,-.05f,.22f),new Vector3(.22f,.14f,.16f));var bTop=Top(bFaces);
   bFaces.First(f=>f.Normal.z<-.9f).Slippery=false;
   var maintenance=Top(NextPlinth(c,"Maintenance balcony",new Vector3(.30f,-.19f,-.05f),new Vector3(.20f,.22f,.18f)));
   var upper=Top(NextPlinth(c,"Upper balcony",new Vector3(.275f,-.11f,.18f),new Vector3(.25f,.38f,.24f)));
   // Outer bridge rising along x (NextRamp assumes z): local x of a near-level panel runs along the slope.
   {Vector3 from=new Vector3(-.18f,.02f,.22f),to=new Vector3(.15f,.08f,.22f),d=to-from;Vector3 n=new Vector3(-d.y,d.x,0).normalized;
    Panel(c.Root,"Outer bridge",(from+to)*.5f,n,new Vector2(d.magnitude,.08f),stone,false,Vector2.zero,0,c.Surfaces);}
   // Junction windows are 15–17 cm apart (a straight run survives both 6.8 cm bowls) and every bend point sits outside the 6.8 cm junction bowl.
   var nodes=new[]{new COgheTubeNetwork.Node("Vào",new Vector3(.06f,-.255f,-.21f),COgheTubeNetwork.TerminalKind.Entry),
    new COgheTubeNetwork.Node("J1",new Vector3(.06f,-.17f,-.04f)),
    new COgheTubeNetwork.Node("Quay lại",new Vector3(-.08f,-.255f,.02f),COgheTubeNetwork.TerminalKind.Entry),
    new COgheTubeNetwork.Node("J2",new Vector3(.08f,-.02f,-.02f)),
    new COgheTubeNetwork.Node("Bệ bảo trì",new Vector3(.235f,-.045f,.01f),COgheTubeNetwork.TerminalKind.Entry),
    new COgheTubeNetwork.Node("J3",new Vector3(.05f,.14f,.02f)),
    new COgheTubeNetwork.Node("Ban công trên",new Vector3(.20f,.115f,.12f),COgheTubeNetwork.TerminalKind.Entry)};
   var edges=new[]{Edge("Vào–J1",0,1,nodes,new Vector3(.06f,-.235f,-.15f),new Vector3(.06f,-.215f,-.12f)),
    Edge("J1–Quay lại",1,2,nodes,new Vector3(-.01f,-.20f,-.01f),new Vector3(-.05f,-.245f,.015f)),
    Edge("J1–J2",1,3,nodes),
    Edge("J2–Bệ",3,4,nodes,new Vector3(.16f,-.04f,-.01f)),
    Edge("J2–J3",3,5,nodes),
    Edge("J3–Ban công",5,6,nodes,new Vector3(.13f,.12f,.07f))};
   var tube=TubeNetwork(c.Root,"Three-level route network",nodes,edges,.038f,glass);tube.CaptureSurfaceCommandsWhileInside=true;
   var safe=new GameObject("Valve safety",typeof(COgheTissueClearance)).GetComponent<COgheTissueClearance>();safe.transform.SetParent(c.Root,false);safe.Network=tube;safe.Size=Vector3.zero;
   // A: a held lever on the station A balcony; C (neutral) on the maintenance balcony latches the same valve.
   var a=ViewTask(c,"A",new Vector3(-.30f,-.097f,.10f),Vector3.right,.07f,aTop);a.HoldAtEnd=true;
   // C's handle faces +x so the half standing beside it never hides it from the default camera.
   var latchC=ViewTask(c,"C",new Vector3(.25f,-.057f,-.11f),Vector3.forward,.06f,maintenance);
   latchC.Handle.localPosition=new Vector3(.042f,0,0);latchC.Handle.localRotation=Quaternion.Euler(0,90,0);latchC.StandOffset=new Vector3(.05f,0,0);
   var valve=ViewGate(c,"A route valve",new Vector3(.15f,-.10f,-.02f),Vector3.up,.02f,new Vector3(.018f,.018f,.018f));
   var route=new GameObject("A holds or C latches the maintenance route",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();route.transform.SetParent(c.Root,false);
   route.Holds=new[]{a};route.Rails=new[]{latchC.Rail};route.Any=true;route.Retain=false;route.Output=valve;route.Clearance=safe;
   edges[3].AccessGate=valve;
   // B: J3's valve onto the upper balcony. Its handle faces the bank's open side (stand to +x).
   var b=ViewTask(c,"B",new Vector3(-.34f,.043f,.18f),Vector3.forward,.07f,bTop);b.Clearance=safe;
   b.Handle.localPosition=new Vector3(.042f,0,0);b.Handle.localRotation=Quaternion.Euler(0,90,0);b.StandOffset=new Vector3(.05f,0,0);
   edges[5].AccessGate=b.Rail;
   // D (neutral) on the upper balcony lifts the gate at the head of the outer bridge.
   var latchD=ViewTask(c,"D",new Vector3(.30f,.103f,.20f),Vector3.right,.05f,upper);
   var gate=ViewGate(c,"D climb gate",new Vector3(.138f,.112f,.22f),Vector3.up,.08f,new Vector3(.012f,.06f,.09f));
   foreach(var f in gate.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;
   ViewLink(c,latchD.Rail,gate,false,null);
   MechanismVisual(c.Root,"J2 valve selector",new Vector3(.08f,-.02f,-.09f),new Vector3(.03f,.03f,.012f),metal);
   MechanismVisual(c.Root,"J3 valve selector",new Vector3(.05f,.14f,-.05f),new Vector3(.03f,.03f,.012f),metal);
   NextTrace("A",new Vector3(-.23f,-.1192f,.10f),new Vector3(-.19f,-.1192f,.10f));
   NextTrace("B",new Vector3(-.26f,.0208f,.25f),new Vector3(-.19f,.0208f,.25f));
  }
  // 29 · 50 % + 25 % + 25 %. A1 and A2 draw the bolt from both bridge pieces. The far piece C starts in the near
  // socket, so it must go first (pushed from the start landing); then the near piece D slides in from its front bay.
  // Their catches hold them, so the holders may leave. Merged, the whole body winds winch B: the rope lifts the hinged
  // span's far end level with the exit platform, where a pawl catches it (half a body cannot lift it).
  static void Next29(ExpansionContext c)
  {
   c.Exit=new Vector3(.40f,-.152f,.05f);c.Outward=Vector3.right;c.Spawn=new Vector3(.10f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(42,20,0);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(.10f,-.30f,-.20f),.025f,.13f);
   // Road at 12 cm along x (z -.02….16, deep enough for a whole body): start landing | near socket | far socket | winch platform | span | exit platform.
   var landing=Top(NextPlinth(c,"Start landing",new Vector3(-.331f,-.24f,.07f),new Vector3(.098f,.12f,.18f)));
   // 11 cm wide so a whole body climbs it without spilling onto the bay beside it.
   NextRamp(c,"Start ramp",new Vector3(-.34f,-.30f,-.20f),new Vector3(-.34f,-.18f,-.02f),.11f);
   // The winch platform runs 10 cm deeper than the road so a whole body can brace behind the span, off its deck.
   var winchDeck=Top(NextPlinth(c,"Winch platform",new Vector3(.013f,-.24f,.10f),new Vector3(.174f,.12f,.24f)));
   NextPlinth(c,"Exit platform",new Vector3(.33f,-.24f,.05f),new Vector3(.14f,.12f,.14f));
   NextPlinth(c,"D waiting shelf",new Vector3(-.23f,-.256f,-.12f),new Vector3(.10f,.088f,.18f),false); // ivory: brushing it never peels a climber
   var a1=ExpansionPad(c,"A1",new Vector3(.34f,-.298f,-.20f),.009f,.09f);var a2=ExpansionPad(c,"A2",new Vector3(.34f,-.298f,-.065f),.009f,.09f);
   var bolt=ViewGate(c,"A piece bolt",new Vector3(-.23f,-.285f,.19f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var pads=new GameObject("A1 A2 draw the piece bolt",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();pads.transform.SetParent(c.Root,false);
   pads.Inputs=new[]{a1,a2};pads.Output=bolt;pads.Retain=false;
   // Far piece C: pushed from the landing (handle on its top-left edge, so the hand is in plain sight).
   var far=NextCrate(c,"C",new Vector3(-.23f,-.195f,.07f),Vector3.right,.104f,new Vector3(.10f,.03f,.18f),landing,new Vector3(-.04f,.02f,0),new Vector3(-.07f,0,0),.03f,.01f,0,true);
   far.Handle.localRotation=Quaternion.Euler(0,90,0);far.RequiredRail=bolt;
   // Near piece D: drawn from its front bay across the path C has just left, by a part on the landing beside it.
   var near=NextCrate(c,"D",new Vector3(-.23f,-.195f,-.12f),Vector3.forward,.19f,new Vector3(.10f,.03f,.18f),landing,new Vector3(-.04f,.02f,0),new Vector3(-.07f,0,0),.03f,.01f,0,true);
   near.Handle.localRotation=Quaternion.Euler(0,90,0);near.RequiredRail=bolt;
   // B winds leftward: the bracing body tracks the handle away from the span, never loading its deck.
   var winch=ViewTask(c,"B",new Vector3(.04f,-.157f,.17f),Vector3.left,.08f,winchDeck);winch.CompensateLoad=true;winch.StallSeconds=6;
   // Span: hinged at the winch platform's edge, authored level then tilted 22 degrees far end down onto its rest.
   // The hinge sits 7 mm clear of the platform face so the tilted span's lower corner never enters it.
   const float tilt=22f,half=.073f;var hingePoint=new Vector3(.107f,-.19f,.05f);var centre=hingePoint+new Vector3(half,0,0);
   var span=Prop(c.Root,"Lift span",centre,new Vector3(half*2,.02f,.12f),false,plastic,c.Surfaces);c.Props.Add(span);span.Body.mass=.045f;span.Body.centerOfMass=Vector3.zero; // measured: rope needs ≈0.46 N; a whole body gives 0.67 N, half 0.34 N
   foreach(var f in span.GetComponentsInChildren<VenomSurfacePatch>())if(f.Normal.y<.9f)f.Slippery=true;
   int first=c.Surfaces.Count;ViewBlock(c,"Lift span level",centre,new Vector3(half*2,.02f,.12f));var docked=c.Surfaces.GetRange(first,c.Surfaces.Count-first).ToArray();foreach(var d in docked){d.gameObject.SetActive(false);if(d.Normal.y<.9f)d.Slippery=true;}
   span.transform.localPosition=hingePoint+Quaternion.Euler(0,0,-tilt)*new Vector3(half,0,0);span.transform.localRotation=Quaternion.Euler(0,0,-tilt);
   var hinge=span.gameObject.AddComponent<HingeJoint>();hinge.connectedBody=c.Root.GetComponent<Rigidbody>();hinge.autoConfigureConnectedAnchor=false;hinge.anchor=new Vector3(-half,0,0);hinge.connectedAnchor=hingePoint;hinge.axis=Vector3.forward;
   // The axle stop is exactly level: the rope pins the span there instead of lifting it past the exit edge.
   hinge.useLimits=true;hinge.limits=new JointLimits{min=-3,max=tilt};hinge.enableCollision=true;
   ViewBlock(c,"Lift span rest",new Vector3(.232f,-.278f,.05f),new Vector3(.02f,.044f,.10f));
   var anchor=new GameObject("Lift span rope anchor").transform;anchor.SetParent(span.transform,false);anchor.localPosition=new Vector3(half-.005f,.01f,0);
   var bridge=new GameObject("B winch rope",typeof(COgheSeesawBridge)).GetComponent<COgheSeesawBridge>();bridge.transform.SetParent(c.Root,false);
   bridge.Plank=span.Body;bridge.Hinge=hinge;bridge.Anchor=anchor;bridge.Tray=winch.Rail;bridge.LevelLocalRotation=Quaternion.identity;
   bridge.MovingSurfaces=span.GetComponentsInChildren<VenomSurfacePatch>(true);bridge.DockedSurfaces=docked;
   bridge.Guides=new[]{NextMarker(c,"Rope pulley over the winch",new Vector3(.02f,.06f,.17f)),NextMarker(c,"Rope pulley over the span",new Vector3(.17f,.06f,.05f))};
   foreach(var g in bridge.Guides){var wheel=MechanismVisual(c.Root,"B pulley wheel",g.localPosition,new Vector3(.05f,.014f,.05f),metal,PrimitiveType.Cylinder);wheel.localRotation=Quaternion.Euler(90,0,0);}
   bridge.Rope=bridge.gameObject.AddComponent<LineRenderer>();bridge.Rope.useWorldSpace=true;bridge.Rope.startWidth=bridge.Rope.endWidth=.0028f;bridge.Rope.sharedMaterial=metal;
   bridge.Pawl=MechanismVisual(c.Root,"Lift span pawl",new Vector3(.262f,-.172f,.12f),new Vector3(.01f,.012f,.01f),metal);
   NextOutline(c,"C far socket",new Vector3(-.126f,-.2995f,.07f),new Vector2(.104f,.18f));
   NextTrace("A",new Vector3(.34f,-.2992f,-.155f),new Vector3(.34f,-.2992f,-.07f),new Vector3(-.16f,-.2992f,-.07f),new Vector3(-.16f,-.2992f,.17f),new Vector3(-.22f,-.2992f,.17f)); // under the road to the bolt
   NextTrace("A",new Vector3(.295f,-.2992f,-.065f),new Vector3(.24f,-.2992f,-.065f),new Vector3(.24f,-.2992f,-.07f));
  }
  // 30 · BOSS. Three bays in a U open to the camera. Left: Q, pad A and the tube up to latch A on the high ledge.
  // Right: pad B and rope B from the front bank to the back dock with latch B, whose gate opens the ramp home.
  // Each pad holds its bolt drawn; each latch only catches a drawn bolt, which frees the holder. Bolt A frees the tall
  // far block C, bolt B the low near block D; C starts in D's socket, so it goes in first. Merged, the whole body winds
  // E: two spans rise out of the deck's pit and latch, the only way across to the exit behind them.
  static void Next30(ExpansionContext c)
  {
   c.Exit=new Vector3(-.02f,-.182f,.30f);c.Outward=Vector3.forward;c.Spawn=new Vector3(-.22f,-.25f,-.262f);c.Definition.CameraEuler=new Vector3(42,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(-.22f,-.30f,-.16f),.025f,.13f); // 9 cm corridor in front of Q to the right bay
   // Left bay: the high ledge with latch A is reached only through the tube (which also brings the worker home).
   var ledgeA=Top(NextPlinth(c,"Latch A ledge",new Vector3(-.285f,-.16f,.21f),new Vector3(.23f,.28f,.18f)));
   var padA=ExpansionPad(c,"A",new Vector3(-.35f,-.298f,-.02f),.009f,.09f);
   // The foot mouth faces the centre (+x), 9 cm behind pad A, so a part entering never brushes the pad's holder;
   // the tube climbs in front of the ledge and ends at its far-left, well inside its top.
   ChapterTube(c,"Left tube",new Vector3(-.27f,-.255f,.07f),new Vector3(-.31f,-.25f,.07f),new Vector3(-.362f,-.18f,.075f),new Vector3(-.362f,-.06f,.08f),new Vector3(-.362f,0f,.10f),new Vector3(-.362f,.015f,.16f));
   // Latch A at the ledge's right end: taps on it pass well clear of the tube mouth's pick radius.
   var latchA=ViewTask(c,"A",new Vector3(-.22f,.003f,.27f),Vector3.left,.04f,ledgeA);
   var boltA=ViewGate(c,"A bolt",new Vector3(-.06f,-.285f,.02f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var holdA=new GameObject("Pad A holds, latch A catches bolt A",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();holdA.transform.SetParent(c.Root,false);
   holdA.Inputs=new[]{padA};holdA.Rails=new[]{latchA.Rail};holdA.Any=true;holdA.Retain=false;holdA.Output=boltA;latchA.RequiredRail=boltA;
   // Right bay: rope B runs front to back (+z), 30 cm clear of the ramps; the dock ledge's ramp home is gated by latch B.
   // A 10 cm bank still reaches the parked ring (grip reach is 11 cm). Its ivory left face is the climb (no ramp
   // with an open underside to wander into); every other face is slick.
   var bankFaces=NextPlinth(c,"Rope B start bank",new Vector3(.28f,-.25f,-.24f),new Vector3(.24f,.10f,.12f));var bank=Top(bankFaces);
   bankFaces.First(f=>f.Normal.x<-.9f).Slippery=false;
   // Measured arc of a 25 % part on this rope: its lowest particle passes z .115 at y ≈ -.17. The dock edge sits
   // there with its top 1.8 cm lower, inside the contact envelope, so the part clears the lip and lands.
   var dock=Top(NextPlinth(c,"Latch B dock",new Vector3(.28f,-.244f,.1975f),new Vector3(.24f,.112f,.165f)));
   NextRamp(c,"Return ramp",new Vector3(.20f,-.30f,-.01f),new Vector3(.20f,-.188f,.115f),.08f);
   // Pad B sits off the camera ray through the parked ring (a tap over a part selects that part).
   var padB=ExpansionPad(c,"B",new Vector3(.285f,-.298f,.045f),.009f,.09f);
   NextSwing(c,"B",new Vector3(.30f,.14f,-.02f),.28f,40f,new Vector3(.30f,-.18f,-.25f),bank,new[]{dock},new[]{new Vector3(.30f,-.168f,.17f)},floor,Vector3.forward);
   var latchB=ViewTask(c,"B",new Vector3(.30f,-.165f,.235f),Vector3.right,.05f,dock);
   var boltB=ViewGate(c,"B bolt",new Vector3(-.06f,-.285f,-.10f),Vector3.up,.03f,new Vector3(.02f,.02f,.02f));
   var holdB=new GameObject("Pad B holds, latch B catches bolt B",typeof(COgheLoadLatch)).GetComponent<COgheLoadLatch>();holdB.transform.SetParent(c.Root,false);
   holdB.Inputs=new[]{padB};holdB.Rails=new[]{latchB.Rail};holdB.Any=true;holdB.Retain=false;holdB.Output=boltB;latchB.RequiredRail=boltB;
   var gate=ViewGate(c,"B return gate",new Vector3(.20f,-.158f,.121f),Vector3.up,.07f,new Vector3(.08f,.06f,.012f));
   foreach(var f in gate.GetComponentsInChildren<VenomSurfacePatch>(true))f.Slippery=true;
   ViewLink(c,latchB.Rail,gate,false,null);
   // Centre deck (9 cm, slick sides): front strip with winch E | pit with two lifting spans (open ends) | exit strip.
   const float top=-.21f,x0=-.16f,x1=.12f;float xm=(x0+x1)*.5f,w=x1-x0;
   var deckFront=Panel(c.Root,"Centre deck",new Vector3(xm,top,.10f),Vector3.up,new Vector2(w,.12f),stone,false,Vector2.zero,0,c.Surfaces);
   Panel(c.Root,"Centre deck",new Vector3(xm,top,.26f),Vector3.up,new Vector2(w,.08f),stone,false,Vector2.zero,0,c.Surfaces);
   void Slick(Vector3 centre,Vector3 normal,Vector2 size)=>Panel(c.Root,"Centre deck side",centre,normal,size,stone,false,Vector2.zero,0,c.Surfaces).Slippery=true;
   Slick(new Vector3(xm,-.255f,.04f),Vector3.back,new Vector2(w,.09f));
   foreach(var (z,d) in new[]{(.10f,.12f),(.26f,.08f)}){Slick(new Vector3(x0,-.255f,z),Vector3.left,new Vector2(d,.09f));Slick(new Vector3(x1,-.255f,z),Vector3.right,new Vector2(d,.09f));}
   Slick(new Vector3(xm,-.257f,.16f),Vector3.forward,new Vector2(w,.086f));Slick(new Vector3(xm,-.257f,.22f),Vector3.back,new Vector2(w,.086f));
   // Steps: floor → D (3 cm) → C (6 cm) → deck (9 cm). C starts in D's socket; both handles face +x (stand on the floor).
   var far=NextCrate(c,"C",new Vector3(.02f,-.27f,-.078f),Vector3.forward,.08f,new Vector3(.116f,.06f,.076f),floor,new Vector3(.066f,0,0),new Vector3(.052f,0,0),.04f,.01f,0,true);
   far.Handle.localRotation=Quaternion.Euler(0,90,0);far.RequiredRail=boltA;
   var near=NextCrate(c,"D",new Vector3(.02f,-.285f,-.162f),Vector3.forward,.084f,new Vector3(.116f,.03f,.076f),floor,new Vector3(.066f,0,0),new Vector3(.052f,0,0),.04f,.01f,0,true);
   near.Handle.localRotation=Quaternion.Euler(0,90,0);near.RequiredRail=boltB;
   // Winch E: whole-body load (two spans, 0.49 N through the cables); half a body cannot wind it.
   // Handle stroke = span travel + cable stretch at its load (0.245 N / 45 N/m): the handle ends as the spans dock.
   var winch=ViewTask(c,"E",new Vector3(.06f,-.187f,.14f),Vector3.left,.066f,deckFront);winch.CompensateLoad=true;winch.StallSeconds=6;
   foreach(float sx in new[]{-.09f,.05f})
   {
    var span=ExpansionRail(c,"E lifting span",new Vector3(sx,-.283f,.19f),Vector3.up,.06f,0,new Vector3(.136f,.026f,.056f),.025f,.01f,true,false);
    span.GetComponent<VenomMovableProp>().Manipulable=false;span.CatchTolerance=.004f;span.LatchAtEnd=true;TrimSideSlabs(span);
    foreach(var face in span.GetComponentsInChildren<VenomSurfacePatch>())if(face.Normal.y<.9f)face.Slippery=true;
    var cable=new GameObject("E winch cable").AddComponent<COghePulleyDrive>();cable.transform.SetParent(c.Root,false);cable.Stiffness=45;cable.Input=winch.Rail;cable.Command=winch;cable.Output=span;
    cable.Cable=cable.gameObject.AddComponent<LineRenderer>();cable.Cable.sharedMaterial=metal;cable.Cable.startWidth=cable.Cable.endWidth=.0028f;cable.Cable.useWorldSpace=true;
    var guides=new List<Transform>();foreach(var g in new[]{new Vector3(.06f,.12f,.14f),new Vector3(sx,.12f,.19f)}){guides.Add(NextMarker(c,"E rope guide",g));var wheel=MechanismVisual(c.Root,"E pulley wheel",g,new Vector3(.05f,.014f,.05f),metal,PrimitiveType.Cylinder);wheel.localRotation=Quaternion.Euler(90,0,0);}
    cable.Guides=guides.ToArray();
   }
   NextOutline(c,"C socket",new Vector3(.02f,-.2995f,.002f),new Vector2(.12f,.08f));NextOutline(c,"D socket",new Vector3(.02f,-.2995f,-.078f),new Vector2(.12f,.08f));
   NextTrace("A",new Vector3(-.305f,-.2992f,-.02f),new Vector3(-.17f,-.2992f,-.02f),new Vector3(-.17f,-.2992f,.02f),new Vector3(-.07f,-.2992f,.02f));
   NextTrace("A",new Vector3(-.235f,-.0192f,.25f),new Vector3(-.215f,-.0192f,.25f),new Vector3(-.215f,-.0192f,.16f));
   NextTrace("B",new Vector3(.285f,-.2992f,.0f),new Vector3(.285f,-.2992f,-.02f),new Vector3(.25f,-.2992f,-.10f),new Vector3(-.05f,-.2992f,-.10f));
   NextTrace("B",new Vector3(.37f,-.1872f,.235f),new Vector3(.37f,-.1872f,.13f),new Vector3(.25f,-.1872f,.13f));
  }
  // 11 · A crate pushed along its rail into the socket beside a slick plinth becomes the step.
  static void Next11(ExpansionContext c)
  {
   c.Exit=new Vector3(.25f,-.068f,.30f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Receiving plinth",new Vector3(.23f,-.22f,.155f),new Vector3(.30f,.16f,.27f));
   NextCrate(c,"A",new Vector3(-.21f,-.22f,.10f),Vector3.right,.206f,new Vector3(.16f,.16f,.14f),floor,new Vector3(0,-.035f,-.078f),new Vector3(0,0,-.052f));
  }
 }
}
