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
 // Spatial Plus: the 18 easy levels and 2 bosses added to the Spatial campaign on 30/09/2026 (design:
 // Docs/LevelDesign/COghe/SpatialPlus20). Scenes COgheSpatialPlusKEY sit beside COgheSpatial01–30; their position in
 // the 50-level campaign comes from SpatialOrder. Built with the Spatial 11–30 helpers and Glass C art.
 public static partial class VenomCampaignBuilder
 {
  public const string PlusNotes="Artifacts/SpatialPlusDesign";
  sealed class PlusLevel{public string Key,Title,Lesson;public bool Boss;public Action<ExpansionContext> Build;public Vector3 Camera=new Vector3(40,20,0);}
  static PlusLevel NewPlus(string key,string title,string lesson,Action<ExpansionContext> build,bool boss=false)=>new PlusLevel{Key=key,Title=title,Lesson=lesson,Build=build,Boss=boss};
  static PlusLevel[] PlusLevels()=>new[]{
   NewPlus("E01","Thang chở hàng","Lên khay cạnh thùng, chạm nút để lên.",PlusE01),
   NewPlus("E02","Hai ống, một đích","Xem ống nào dẫn tới đâu rồi hãy chui.",PlusE02),
   NewPlus("E03","Giữ cửa cho bạn","Một phần giữ nút A, phần kia chốt cửa bằng B.",PlusE03),
   NewPlus("E04","Cõng thùng","Kéo xe A, rồi đẩy thùng B sát bệ.",PlusE04),
   NewPlus("E05","Hai nhịp dây","Bám vòng A sang đảo, rồi bám tiếp vòng B.",PlusE05),
   NewPlus("E06","Bập bênh","Đi qua trục, ván sẽ nghiêng xuống phòng bên.",PlusE06),
   NewPlus("E07","Chồng hai tầng","Khối A vào trước, rồi đẩy khối B tới mép.",PlusE07),
   NewPlus("B1","Tháp khối","",PlusB1,true),
   NewPlus("E08","Bánh răng đầu tiên","Đứng lên nút A để máy chạy.",PlusE08),
   NewPlus("E09","Đủ nặng mới mở","Nút 50% cần nửa thân; 25% quá nhẹ.",PlusE09),
   NewPlus("E10","Đu rồi luồn","Đu sang đảo, rồi chui ống trên đảo.",PlusE10),
   NewPlus("E11","Giữ thang cho bạn","Một phần giữ cần B, phần kia chốt C.",PlusE11),
   NewPlus("E12","Khớp một bánh","Đưa bánh G vào khe trước, rồi bật máy.",PlusE12),
   NewPlus("E13","Hai ống, hai nửa","Mỗi phần một ống, đứng hai nút cùng lúc.",PlusE13),
   NewPlus("E14","Hai tầng răng","Khớp tầng dưới trước, rồi tới tầng trên.",PlusE14),
   NewPlus("E15","Người chạy máy","Một phần đứng máy, phần kia đi thang.",PlusE15),
   NewPlus("E16","Bàn xoay","Đứng nút P để bàn xoay nối hai bờ.",PlusE16),
   NewPlus("E17","Hai máy nối nhau","Máy 1 đưa bánh G vào máy 2.",PlusE17),
   NewPlus("E18","Ba lớp răng","Một phần giữ máy, phần kia khớp từng tầng.",PlusE18),
   NewPlus("B2","Tháp bánh răng","",PlusB2,true),
   // Chapter 2 rebuilt for the hook plan (Mrk, 05/10/2026; PLANS/COGHE_LEVEL_HOOK_PLAN.md §5.2). Appended so the
   // indexes (mesh serials) of the levels above never move.
   NewPlus("N13","Khối chặn lò xo","Khối chặn có lò xo: buông ra là nó bật về chỗ cũ.",PlusN13),
   NewPlus("N18","Nửa thân không đủ sức","Khối 100% cần cả thân COghe; một nửa chỉ gồng được.",PlusN18),
   NewPlus("N15","Chuẩn bị trước khi đi","Tay nắm chỉ ở phòng này: việc gì ở đây thì làm trước khi đi.",PlusN15),
  };

  // Design notes (root-local): numbered steps, labels and routes, projected onto renders for the design plates.
  static readonly List<string> plusNotes=new List<string>();
  static int plusPath;
  static void PlusNote(string kind,string text,Vector3 p)=>plusNotes.Add(string.Join("|",kind,text,p.x.ToString("F4",CultureInfo.InvariantCulture),p.y.ToString("F4",CultureInfo.InvariantCulture),p.z.ToString("F4",CultureInfo.InvariantCulture)));
  static void PlusStep(int n,Vector3 p)=>PlusNote("step",n.ToString(CultureInfo.InvariantCulture),p);
  static void PlusLabel(string text,Vector3 p)=>PlusNote("label",text,p);
  static void PlusGhost(string text,Vector3 p)=>PlusNote("ghost",text,p);
  static void PlusRoute(string who,params Vector3[] points){plusPath++;foreach(var p in points)PlusNote("path",plusPath+":"+who,p);}

  static string[] PlusSelection()
  {
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-coghe-plus-levels");
   return i>=0&&i+1<args.Length?args[i+1].Split(','):null;
  }
  [MenuItem("Gravity Box/COghe/Spatial Plus/Generate levels")]
  public static void GenerateSpatialPlus()
  {
   PrepareCampaign30Assets();Directory.CreateDirectory(SpatialFolder+"/Definitions");Directory.CreateDirectory(SpatialFolder+"/Meshes");Directory.CreateDirectory(PlusNotes);AssetDatabase.Refresh();
   var only=PlusSelection();var levels=PlusLevels();
   string old=authoredMeshFolder;
   try{authoredMeshFolder=SpatialFolder+"/Meshes";for(int i=0;i<levels.Length;i++)if(only==null||only.Contains(levels[i].Key))BuildSpatialPlus(levels[i],i+1);}
   finally{authoredMeshFolder=old;}
   ApplySpatialOrder();
   Debug.Log("SPATIAL PLUS GENERATED "+string.Join(",",levels.Where(d=>only==null||only.Contains(d.Key)).Select(d=>d.Key)));
  }

  /// <summary>Chapter 2 rebuild (Mrk, 05/10/2026): builds the levels named by -coghe-plus-levels (only those), applies the
  /// play order, then renumbers the plaque of every level that moved. Other scenes are opened for the plaque only.</summary>
  public static void GenerateChapterTwoLevels(){GenerateSpatialPlus();RenumberSpatialPlaques();}
  /// <summary>The plaque on the glass shows the play position; a level that moved in the order gets its new number.</summary>
  public static void RenumberSpatialPlaques()
  {
   foreach(var path in SpatialScenePaths().Where(File.Exists))
   {
    var scene=EditorSceneManager.OpenScene(path);var game=Object.FindFirstObjectByType<VenomCampaign>();string number=game.Definition.Order.ToString("00");bool changed=false;
    foreach(var t in game.GetComponentsInChildren<TextMesh>(true))
     if(t.transform.parent!=null&&t.transform.parent.name=="COghe specimen number"&&t.text!=number){t.text=number;t.name="Label · "+number;changed=true;}
    if(changed){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Debug.Log("PLAQUE renumbered "+number+" "+path);}
   }
  }

  static void BuildSpatialPlus(PlusLevel d,int index)
  {
   plusNotes.Clear();plusPath=0;int position=SpatialPosition(d.Key);
   meshSerial=(40+index)*1000;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var owner=new GameObject("COghe Spatial · "+d.Title).AddComponent<VenomLevelController>();
   owner.MatterProfile=AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder+"/Matter.asset");owner.ControlMode=VenomControlMode.TouchSurface;
   owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");owner.IndicatorMaterial=mint;owner.ApertureRadius=.046f;
   var game=owner.gameObject.AddComponent<VenomCampaign>();string path=$"{SpatialFolder}/Definitions/SpatialPlus{d.Key}.asset";var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
   if(def==null){def=ScriptableObject.CreateInstance<VenomCampaignDefinition>();AssetDatabase.CreateAsset(def,path);}
   def.Id="coghe.spatial.plus."+d.Key.ToLowerInvariant();def.Order=position;def.Title=$"{position:00} · {d.Title}";def.Lesson=d.Lesson;def.ViewOnly=true;def.CanRotate=false;def.Passive=false;def.Boss=d.Boss;
   def.ProgressKey="coghe.spatial.pilot";def.CameraEuler=d.Camera;def.CameraZones=Array.Empty<VenomCameraZone>();def.InitialCameraZone=-1;
   def.SceneSequence=SpatialSceneNames();game.Definition=def;
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
   Lighting();COgheDayLabBuilder.ApplyExpansionLevel(game,$"Meshes/Spatial/Plus{d.Key}");COgheDayLabBuilder.ApplyGlassPreview(game,true);
   var shellSatin=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Expansion satin shell.mat");
   foreach(var p in c.Surfaces)if(p.ExteriorGlass&&p.Slippery)p.GetComponent<Renderer>().sharedMaterial=shellSatin;
   var ivory=SpatialMaterial("Ivory bodies",new Color(.88f,.87f,.76f));var blue=SpatialMaterial("Circuit A blue",new Color(.224f,.498f,.678f));var coral=SpatialMaterial("Circuit B coral",new Color(.784f,.424f,.345f));var slipMat=SpatialMaterial("Lavender slippery",new Color(.53f,.48f,.72f));
   foreach(var p in c.Surfaces)if(!p.ExteriorGlass&&p!=floor&&p.GetComponent<Renderer>()!=null)p.GetComponent<Renderer>().sharedMaterial=p.Slippery?slipMat:ivory;
   foreach(var task in owner.Apparatus.GetComponentsInChildren<COgheTapRail>()){var material=task.Label.StartsWith("B")?coral:blue;foreach(var r in task.Handle.GetComponentsInChildren<Renderer>())r.sharedMaterial=material;}
   COgheDayLabBuilder.DecorateSpatialMechanisms(game,blue,coral,ivory);
   NextSpatialArt(c,blue,coral,ivory);
   EditorUtility.SetDirty(def);EditorSceneManager.SaveScene(scene,SpatialContentPath(d.Key));
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
  static VenomSurfacePatch DockedTop(COgheTapRail task)=>task.Rail.GetComponent<COgheDockedBridgeDeck>().DockedSurfaces.First(s=>s.Normal.y>.9f);
 }
}
