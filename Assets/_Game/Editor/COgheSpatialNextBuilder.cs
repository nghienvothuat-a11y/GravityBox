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
   ViewBlock(c,"Q casing roof",L(0,.095f,.01f),new Vector3(.19f,.01f,.12f));
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
  static COgheSwingTransfer NextSwing(ExpansionContext c,string label,Vector3 pivot,float length,float startAngle,Vector3 stand,VenomSurfacePatch startBank,VenomSurfacePatch[] docks,Vector3[] targets,VenomSurfacePatch rescue)
  {
   var swing=new GameObject(label+" rope swing",typeof(COgheSwingTransfer)).GetComponent<COgheSwingTransfer>();swing.transform.SetParent(c.Root,false);swing.Label=label;
   swing.Pivot=NextMarker(c,label+" rope anchor",pivot);swing.RopeLength=length;
   float a=startAngle*Mathf.Deg2Rad;Vector3 hook=pivot+new Vector3(-Mathf.Sin(a),-Mathf.Cos(a),0)*length;
   swing.StartHook=NextMarker(c,label+" start hook",hook);swing.StandPoint=NextMarker(c,label+" grip stance",stand);
   var ringGo=new GameObject(label+" grip ring",typeof(Rigidbody));ringGo.transform.SetParent(c.Owner.Apparatus,false);ringGo.transform.position=c.Root.TransformPoint(hook);
   var ring=ringGo.GetComponent<Rigidbody>();ring.mass=.03f;ring.useGravity=false;ring.isKinematic=true;ring.interpolation=RigidbodyInterpolation.Interpolate;ring.inertiaTensor=Vector3.one*1e-5f;ring.inertiaTensorRotation=Quaternion.identity;
   ring.linearDamping=0;ring.angularDamping=0;ring.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
   var joint=ringGo.AddComponent<ConfigurableJoint>();joint.connectedBody=c.Root.GetComponent<Rigidbody>();joint.autoConfigureConnectedAnchor=false;joint.anchor=Vector3.zero;joint.connectedAnchor=pivot;
   joint.axis=Vector3.forward;joint.secondaryAxis=Vector3.up;joint.xMotion=ConfigurableJointMotion.Locked;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Limited;
   joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;joint.linearLimit=new SoftJointLimit{limit=length,contactDistance=.002f};
   swing.Ring=ring;
   var visual=new GameObject(label+" ring visual").transform;visual.SetParent(ringGo.transform,false);swing.RingVisual=visual;
   var disc=MechanismVisual(visual,label+" ring grip",Vector3.down*.012f,new Vector3(.036f,.004f,.036f),plastic,PrimitiveType.Cylinder);disc.localRotation=Quaternion.Euler(90,0,0);
   MechanismVisual(visual,label+" ring shackle",Vector3.up*.008f,new Vector3(.008f,.018f,.008f),metal);
   swing.StartBank=startBank;swing.Docks=docks;swing.RescueFloor=rescue;
   swing.DockTargets=new Transform[targets.Length];for(int i=0;i<targets.Length;i++)swing.DockTargets[i]=NextMarker(c,label+" landing "+i,targets[i]);
   swing.Rope=swing.gameObject.AddComponent<LineRenderer>();swing.Rope.useWorldSpace=true;swing.Rope.startWidth=swing.Rope.endWidth=.004f;swing.Rope.sharedMaterial=metal;swing.Rope.positionCount=2;
   swing.Drum=MechanismVisual(c.Root,label+" winch drum",pivot+Vector3.up*.012f,new Vector3(.05f,.012f,.05f),metal,PrimitiveType.Cylinder);swing.Drum.localRotation=Quaternion.Euler(90,0,0);
   MechanismVisual(c.Root,label+" rope beam",new Vector3(pivot.x,pivot.y+.03f,pivot.z),new Vector3(.36f,.016f,.02f),metal);
   MechanismVisual(c.Root,label+" start hook post",hook+new Vector3(-.018f,-.004f,0),new Vector3(.006f,.03f,.006f),metal);
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
  static VenomMovableProp NextLooseCrate(ExpansionContext c,string name,Vector3 centre,Vector3 size,float mass)
  {
   var prop=Prop(c.Root,name,centre,size,true,plastic,c.Surfaces);c.Props.Add(prop);
   prop.Body.mass=mass;prop.Body.constraints=RigidbodyConstraints.FreezeRotation;prop.Body.maxDepenetrationVelocity=.2f;prop.Body.centerOfMass=Vector3.zero;
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
   c.Exit=new Vector3(.40f,-.225f,-.18f);c.Outward=Vector3.right;c.Spawn=new Vector3(-.18f,-.25f,-.215f);c.Definition.CameraEuler=new Vector3(44,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
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
   c.Exit=new Vector3(-.02f,-.172f,.30f);c.Outward=Vector3.forward;c.Spawn=new Vector3(-.10f,-.25f,-.265f);c.Definition.CameraEuler=new Vector3(42,20,0);NextShell(c,.30f,true);var floor=c.Surfaces[0];
   NextQuantum(c,new Vector3(-.10f,-.30f,-.18f),.025f,.13f);
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
  // 11 · A crate pushed along its rail into the socket beside a slick plinth becomes the step.
  static void Next11(ExpansionContext c)
  {
   c.Exit=new Vector3(.25f,-.068f,.30f);NextShell(c,.10f,true);var floor=c.Surfaces[0];
   NextPlinth(c,"Receiving plinth",new Vector3(.23f,-.22f,.155f),new Vector3(.30f,.16f,.27f));
   NextCrate(c,"A",new Vector3(-.21f,-.22f,.10f),Vector3.right,.206f,new Vector3(.16f,.16f,.14f),floor,new Vector3(0,-.035f,-.078f),new Vector3(0,0,-.052f));
  }
 }
}
