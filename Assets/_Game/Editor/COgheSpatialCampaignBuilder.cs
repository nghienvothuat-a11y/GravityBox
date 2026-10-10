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
 public static partial class VenomCampaignBuilder
 {
  public const string SpatialFolder="Assets/_Game/Venom/SpatialCampaign";
  // Chapter 1 as rebuilt for the hook plan (05/10/2026). Lessons say how a new mechanism works, never the answer.
  static readonly string[] SpatialNames={"Chạm để đi","Đi vòng mặt tím","Nhìn quanh vách","Kéo là mở","Mở nắp trước","Leo lên, mở hộp","Chìa khoá dưới hố","Lên, kéo, xuống","Cầu chắn giếng thang","Cỗ máy thân quen"};
  static readonly string[] SpatialLessons={"Chạm vòng xanh để chỉ đường.","COghe leo được mặt ngà, trượt khỏi mặt tím.","Kéo ngang để nhìn quanh vách.","Chạm tay nắm để COghe kéo nó.","Hộp màu khoá tay nắm bên trong: kéo cần cùng màu để mở hộp.","Tay nắm có thể ở trên cao.","Chốt chặn có cần riêng.","Nút thang bật lên lại khi tới nơi.","Một vật có thể làm hai việc.",""};
  // The Spatial campaign in play order: 60 levels in five chapters of 12, each closed by its boss (50 levels proposed
  // 29/09/2026 and approved 30/09/2026; the 10 crate levels interleaved 07/10/2026). A key names the content: "01"…"30" are the scenes COgheSpatialNN (pilot 01–10, Spatial 11–30),
  // "E01"…"E18", "B1", "B2" the Spatial Plus scenes COgheSpatialPlusKEY. Position = index + 1 = Definition.Order;
  // IDs, scene files and art folders follow the content, so reordering never touches saves or assets.
  public static readonly string[] SpatialOrder={
   // Five chapters of 12 (Mrk, 06/10/2026: "thêm các level của 51-60 xen kẽ trong các chương. Xếp levels theo độ khó tăng
   // dần"; applied 07/10/2026). Each chapter keeps its designed curve (teach, practise, combine, prepare, boss) and takes two
   // crate levels (K01–K10, ordered easy to hard) where their decision count fits; the boss still closes the chapter.
   // Chapter 1. K01 follows 04 (pull a handle) and 05.
   "01","02","03","04","05","K01","06","07","08","K02","09","10",
   // Chapter 2 rebuilt (Mrk, 05/10/2026): Q is taught first (16); N13, N15, N18 and N19 are new; "11" (Kê một bậc), E01
   // (Thang chở hàng), E02 (Hai ống, một đích) and E03 (Giữ cửa cho bạn) are retired.
   "16","12","N13","14","N15","K03","15","K04","17","N18","N19","20",
   // Chapter 3 rebuilt (Mrk, 06/10/2026: "xây dựng Chương 3 theo kế hoạch"; PLANS/COGHE_LEVEL_HOOK_PLAN.md 5.3): rope taught
   // first, the counterweight next; N22 and N29 add an order; N29 prepares the boss.
   "18","N22","N23","21","N25","N26","K05","22","13","K06","N29","B1",
   // Chapter 4 rebuilt (Mrk, 06/10/2026: "Sửa tiếp Chương 4 và 5"; plan 5.4): split to the right size. N31 teaches the
   // quarter and the load gauge, N32 replaces 23 (a copy of 16), N33 replaces E09, N34 replaces 24 (a copy of 17), N35
   // replaces 25; Xưởng lắp cầu (29) prepares the boss; Bốn trạm (27) is retired; boss N40 (Cân ba phần tư) replaces 30.
   // E10 (a breather) follows the peak of N35 and K07.
   "N31","N32","N33","N34","N35","K07","E10","E11","26","K08","29","N40",
   // Chapter 5 rebuilt (plan 5.5: gears), all new: N41 merges E08 and E12 (the gear lesson); N42 two gear carts cross; N43 a
   // gearbox sets the tube's branch (replaces 28); N44 an idler reverses the train; N45 the motor is as strong as its driver
   // is heavy; N46 the turntable carries a crate; N47 one gear, two machines; N48 the floor layer lifts the upper layer's
   // gear; N49 two motors add up; boss N50 (Hộp số: idler in for the stairs, out again for the heavy door). Retired: E08,
   // E12, E13, E14, E15, E16, E17, E18, B2, 28. K09 follows the peak of N45, before the breather N46.
   "N41","N42","N43","N44","N45","K09","N46","N47","N48","N49","K10","N50"};
  public static string SpatialContentPath(string key)=>char.IsDigit(key[0])?$"{SpatialFolder}/COgheSpatial{key}.unity":$"{SpatialFolder}/COgheSpatialPlus{key}.unity";
  public static string SpatialContentPath(int n)=>SpatialContentPath(n.ToString("00"));
  public static int SpatialPosition(string key){int i=Array.IndexOf(SpatialOrder,key);if(i<0)throw new ArgumentException("Not in the Spatial order: "+key);return i+1;}
  public static int SpatialPosition(int n)=>SpatialPosition(n.ToString("00"));
  public static string[] SpatialScenePaths()=>SpatialOrder.Select(SpatialContentPath).ToArray();
  public static string[] SpatialSceneNames()=>SpatialScenePaths().Select(Path.GetFileNameWithoutExtension).ToArray();
  // Chapter 3 (06/10/2026): the line under the title names the situation, not the answer (audit L11–30, point 1). The
  // Spatial 11–30 hints live in SpatialNextLessons; this writes them into the existing definitions without a rebuild.
  public static void ApplySpatialNextHints()
  {
   foreach(var path in Directory.GetFiles(SpatialFolder+"/Definitions","Spatial*.asset"))
   {
    var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);if(def==null||def.Id==null||!def.Id.StartsWith("coghe.spatial.next."))continue;
    int n=int.Parse(def.Id.Substring(def.Id.LastIndexOf('.')+1));if(n<11||n>30)continue;
    string lesson=SpatialNextLessons[n-11];if(def.Lesson!=lesson){def.Lesson=lesson;EditorUtility.SetDirty(def);Debug.Log($"HINT {n}: {lesson}");}
   }
   AssetDatabase.SaveAssets();
  }
  // Every existing Spatial definition gets the play order (scene sequence), its position and its numbered title.
  public static void ApplySpatialOrder()
  {
   var names=SpatialSceneNames();
   foreach(var path in Directory.GetFiles(SpatialFolder+"/Definitions","*.asset"))
   {
    var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);if(def==null||string.IsNullOrEmpty(def.Id))continue;
    string tail=def.Id.Substring(def.Id.LastIndexOf('.')+1),key=def.Id.Contains(".plus.")?tail.ToUpperInvariant():tail;
    if(Array.IndexOf(SpatialOrder,key)<0)continue;
    int position=SpatialPosition(key);string name=def.Title.Contains(" · ")?def.Title.Substring(def.Title.IndexOf(" · ")+3):def.Title;
    if(def.Order!=position||!def.SceneSequence.SequenceEqual(names)||def.Title!=$"{position:00} · {name}")
    {def.Order=position;def.Title=$"{position:00} · {name}";def.SceneSequence=names;EditorUtility.SetDirty(def);}
   }
   var scenes=EditorBuildSettings.scenes.ToList();foreach(var p in SpatialScenePaths())if(File.Exists(p)&&!scenes.Any(s=>s.path==p))scenes.Add(new EditorBuildSettingsScene(p,true));
   EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
  }
  [MenuItem("Gravity Box/COghe/Spatial pilot/Generate 10 levels")]
  public static void GenerateSpatialCampaign()
  {
   PrepareCampaign30Assets();Directory.CreateDirectory(SpatialFolder+"/Definitions");Directory.CreateDirectory(SpatialFolder+"/Meshes");AssetDatabase.Refresh();
   // -coghe-spatial-levels 7,9 rebuilds only those levels (batch); the menu rebuilds all ten.
   var only=new System.Collections.Generic.HashSet<int>();var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-coghe-spatial-levels");
   if(at>=0&&at+1<args.Length)foreach(var part in args[at+1].Split(','))if(int.TryParse(part,out int v))only.Add(v);
   string old=authoredMeshFolder;try{authoredMeshFolder=SpatialFolder+"/Meshes";for(int n=1;n<=10;n++)if(only.Count==0||only.Contains(n))BuildSpatial(n);}finally{authoredMeshFolder=old;}
   var scenes=EditorBuildSettings.scenes.ToList();foreach(var p in SpatialScenePaths())if(File.Exists(p)&&!scenes.Any(s=>s.path==p))scenes.Add(new EditorBuildSettingsScene(p,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
   Debug.Log("SPATIAL GENERATED 10 isolated levels");
  }
  [MenuItem("Gravity Box/COghe/Spatial pilot/Build Mac test")]
  public static void BuildSpatialMac()
  {
   var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SpatialScenePaths().Where(File.Exists).ToArray(),target=BuildTarget.StandaloneOSX,locationPathName="Builds/SpatialLab/macOS/COghe.app",options=BuildOptions.None,extraScriptingDefines=new[]{"COGHE_MOBILE_BENCHMARK"}});
   if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Spatial Mac failed");
  }
  internal static Material SpatialMaterial(string name,Color color)
  {
   string path=SpatialFolder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(stone);AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find("Universal Render Pipeline/Lit");m.SetFloat("_Surface",0);m.SetFloat("_SrcBlend",1);m.SetFloat("_DstBlend",0);m.SetFloat("_ZWrite",1);m.SetFloat("_Cull",0);m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");m.DisableKeyword("_ALPHAPREMULTIPLY_ON");m.renderQueue=2000;m.color=color;
   if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.35f);EditorUtility.SetDirty(m);return m;
  }
  static void BuildSpatial(int n)
  {
   meshSerial=n*1000;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var owner=new GameObject("COghe Spatial · "+SpatialNames[n-1]).AddComponent<VenomLevelController>();
   owner.MatterProfile=AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder+"/Matter.asset");owner.ControlMode=VenomControlMode.TouchSurface;
   owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");owner.IndicatorMaterial=mint;owner.ApertureRadius=.046f;
   var game=owner.gameObject.AddComponent<VenomCampaign>();string path=$"{SpatialFolder}/Definitions/Spatial{n:00}.asset";var def=AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
   if(def==null){def=ScriptableObject.CreateInstance<VenomCampaignDefinition>();AssetDatabase.CreateAsset(def,path);}
   def.Id=$"coghe.spatial.pilot.{n:00}";def.Order=SpatialPosition(n);def.Title=$"{SpatialPosition(n):00} · {SpatialNames[n-1]}";def.Lesson=SpatialLessons[n-1];def.ViewOnly=true;def.CanRotate=false;def.Passive=false;def.Boss=n==10;def.ProgressKey="coghe.spatial.pilot";def.CameraEuler=new Vector3(36,20,0);def.CameraZones=Array.Empty<VenomCameraZone>();def.InitialCameraZone=-1;def.SceneSequence=Array.ConvertAll(SpatialScenePaths(),Path.GetFileNameWithoutExtension);game.Definition=def;
   owner.Apparatus=new GameObject("Apparatus").transform;owner.Apparatus.SetParent(owner.transform,false);
   var pivot=new GameObject("Fixed chamber",typeof(Rigidbody),typeof(BoxRotationController));pivot.transform.SetParent(owner.Apparatus,false);pivot.GetComponent<Rigidbody>().isKinematic=true;pivot.GetComponent<Rigidbody>().useGravity=false;owner.Rotation=pivot.GetComponent<BoxRotationController>();
   var c=new ExpansionContext{Number=n,Owner=owner,Game=game,Definition=def,Root=pivot.transform,Spawn=new Vector3(-.26f,-.25f,-.19f),Exit=new Vector3(.23f,-.225f,.30f),Outward=Vector3.forward};
   // Glass is never climbable (Mrk, 09/10/2026, plan A): what COghe climbs is cream. An exit on the glass is reached from
   // the surface under it: 1, 3 and 4 exit at floor level; 2's hole starts at the top of the second step; 5 is reached up
   // cream climb boards.
   if(n==1||n==3||n==4)c.Exit.y=-.252f;if(n==2)c.Exit.y=-.094f;if(n==5)c.Exit.y=.05f;if(n==6)c.Exit.y=-.035f;
   // 8 and 10 exit at floor level through slick glass: the hole starts at the floor, there is nothing to climb.
   if(n==8)c.Exit=new Vector3(-.23f,-.252f,.30f);if(n==9)c.Exit=new Vector3(-.25f,.09f,.30f);if(n==10)c.Exit=new Vector3(.25f,-.252f,.30f);
   if(n==7){c.Exit=new Vector3(.32f,-.30f,.16f);c.Outward=Vector3.down;ViewShell(c,true);}
   // Bakery trial (Mrk, 10/10/2026: "tạo level 1 theo phương án 1"): no glass box; the cherry stands on the floor at the
   // back right, where the exit was (COgheBakeryDress pa1 gives the look after the build).
   // A flat level reads better from higher up (level 49's 54°); the yaw stays level 1's.
   else if(n==1){c.Exit=new Vector3(.23f,-.30f,.20f);c.Outward=Vector3.down;BakeryShell(c);BakeryCherry(c);def.CameraEuler=new Vector3(54,20,0);}
   else SpatialShell(c,true);
   foreach(var surface in c.Surfaces)if(surface.ExteriorGlass)surface.Selectable=true;
   var floor=c.Surfaces[0];
   if(n!=1&&n!=4)ChapterOne(c,n,floor);
   if(n==4)
   {
    var work=n==4?floor:c.Surfaces.Find(p=>p.name=="Outer pane 2");
    var start=n==4?new Vector3(-.19f,-.277f,.09f):new Vector3(-.23f,-.04f,.268f);
    var task=ViewTask(c,"A",start,Vector3.right,.16f,work);
    if(n==5){task.StandOffset=new Vector3(0,-.068f,0);task.TouchSize=new Vector3(.10f,.08f,.07f);}
    // The exit starts at the floor: the shutter sits 2 cm up, clear of the floor, and still covers the hole (as 8).
    var gate=ViewGate(c,"A blue shutter",c.Exit+Vector3.back*.020f+Vector3.up*.02f,Vector3.left,.14f,new Vector3(.13f,.13f,.018f));ViewLink(c,task.Rail,gate,true,ViewExitSurface(c));
   }
   owner.Spawn=new GameObject("Spawn").transform;owner.Spawn.SetParent(c.Root,false);owner.Spawn.localPosition=c.Spawn;
   owner.Outlet=new GameObject("Final exit").transform;owner.Outlet.SetParent(c.Root,false);owner.Outlet.localPosition=c.Exit;owner.Outlet.localRotation=Quaternion.LookRotation(c.Outward,Mathf.Abs(c.Outward.y)>.9f?Vector3.forward:Vector3.up);Ring(owner.Outlet,Vector2.zero,owner.ApertureRadius,.0032f,mint);
   game.Surfaces=c.Surfaces.ToArray();game.Props=c.Props.ToArray();owner.CrawlFaces=Enumerable.Repeat(floor.Shape,6).ToArray();foreach(var prop in c.Props)prop.transform.SetParent(owner.Apparatus,true);
   var camera=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.transform.SetParent(owner.transform,false);camera.tag="MainCamera";camera.orthographic=true;camera.nearClipPlane=.005f;camera.farClipPlane=15;camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.rotation=Quaternion.Euler(def.CameraEuler);camera.transform.position=-camera.transform.forward*3;owner.View=camera;
   Lighting();COgheDayLabBuilder.ApplyExpansionLevel(game,$"Meshes/Spatial/Level{n:00}");COgheDayLabBuilder.ApplyGlassPreview(game,true);
   var ivory=SpatialMaterial("Ivory bodies",new Color(.88f,.87f,.76f));var blue=SpatialMaterial("Circuit A blue",new Color(.224f,.498f,.678f));var coral=SpatialMaterial("Circuit B coral",new Color(.784f,.424f,.345f));var slip=SpatialMaterial("Lavender slippery",new Color(.53f,.48f,.72f));
   // Chapter 1 colours its third and fourth controls (Mrk, 05/10/2026: colours, not letters, show what works what).
   SpatialMaterial("Circuit C amber",ChapterAmber);SpatialMaterial("Circuit D green",ChapterGreen);
   foreach(var p in c.Surfaces)if(!p.ExteriorGlass&&p!=floor&&p.name!="Departure bank"&&p.name!="Receiving bank"&&p.name!="Recovery basin")p.GetComponent<Renderer>().sharedMaterial=p.Slippery?slip:ivory;
   foreach(var task in owner.Apparatus.GetComponentsInChildren<COgheTapRail>()){var material=task.Label=="B"?coral:blue;foreach(var r in task.Handle.GetComponentsInChildren<Renderer>())r.sharedMaterial=material;}
   foreach(var rail in owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())if(rail.name.Contains("shutter"))foreach(var r in rail.GetComponentsInChildren<Renderer>())r.sharedMaterial=rail.name.StartsWith("B")?coral:blue;
   foreach(var lift in owner.Apparatus.GetComponentsInChildren<COghePassengerLift>())lift.Panel.GetComponent<Renderer>().sharedMaterial=n==10?coral:blue;
   COgheDayLabBuilder.DecorateSpatialMechanisms(game,blue,coral,ivory);
   ChapterOneFinish(game);
   EditorUtility.SetDirty(def);EditorSceneManager.SaveScene(scene,SpatialContentPath(n));
  }
  static void SpatialShell(ExpansionContext c,bool slippery)
  {
   Panel(c.Root,"Laboratory floor",new Vector3(0,-.30f,0),Vector3.up,new Vector2(.8f,.6f),stone,false,Vector2.zero,0,c.Surfaces);
   float top=c.Number>=5?.30f:.10f,mid=(top-.30f)*.5f,height=top+.30f;
   var pos=new[]{new Vector3(0,top,0),new Vector3(0,mid,-.3f),new Vector3(0,mid,.3f),new Vector3(-.4f,mid,0),new Vector3(.4f,mid,0)};
   var normal=new[]{Vector3.down,Vector3.forward,Vector3.back,Vector3.right,Vector3.left};
   for(int i=0;i<5;i++){var rot=Quaternion.LookRotation(normal[i],i==0?Vector3.forward:Vector3.up);var local=Quaternion.Inverse(rot)*(c.Exit-pos[i]);var p=Panel(c.Root,"Outer pane "+i,pos[i],normal[i],i==0?new Vector2(.8f,.6f):new Vector2(i<3?.8f:.6f,height),glass,i==2,new Vector2(local.x,local.y),i==2?c.Owner.ApertureRadius:0,c.Surfaces);p.ExteriorGlass=true;p.Selectable=true;p.Slippery=slippery;}
  }
  static COghePulleyDrive SpatialPulley(ExpansionContext c,VenomSurfacePatch floor)
  {
   ViewBlock(c,"Departure plinth",new Vector3(-.25f,-.20f,.16f),new Vector3(.30f,.20f,.24f));
   int first=c.Surfaces.Count;ViewBlock(c,"Receiving plinth",new Vector3(.25f,-.20f,c.Number==10?.07f:.16f),new Vector3(.30f,.20f,c.Number==10?.42f:.24f));for(int i=first;i<c.Surfaces.Count;i++)if(c.Surfaces[i].Normal.y<.9f)c.Surfaces[i].Slippery=true;
   var task=ViewTask(c,"A",new Vector3(-.31f,-.277f,-.12f),Vector3.right,.16f,floor);task.CompensateLoad=true;task.StallSeconds=6;
   var deck=ExpansionRail(c,"Pulley lifting deck",new Vector3(0,-.278f,.16f),Vector3.up,.16f,0,new Vector3(.198f,.036f,.24f),.022f,.001f,true,false);deck.CatchTolerance=.008f;deck.LatchAtEnd=true;deck.GetComponent<VenomMovableProp>().Manipulable=false;
   var cable=new GameObject("A tension cable").AddComponent<COghePulleyDrive>();cable.transform.SetParent(c.Root,false);cable.Stiffness=45;cable.Input=task.Rail;cable.Command=task;cable.Output=deck;
   cable.Cable=cable.gameObject.AddComponent<LineRenderer>();cable.Cable.sharedMaterial=metal;cable.Cable.startWidth=cable.Cable.endWidth=.0028f;cable.Cable.useWorldSpace=true;
   var guides=new List<Transform>();foreach(var p in new[]{new Vector3(-.24f,.22f,-.12f),new Vector3(0,.22f,.16f)}){var t=new GameObject("Pulley rope guide").transform;t.SetParent(c.Root,false);t.localPosition=p;guides.Add(t);var wheel=MechanismVisual(c.Root,"A pulley wheel",p,new Vector3(.065f,.018f,.065f),metal,PrimitiveType.Cylinder);wheel.localRotation=Quaternion.Euler(90,0,0);MechanismVisual(c.Root,"Pulley bearing support",new Vector3(p.x,.26f,p.z),new Vector3(.016f,.08f,.018f),metal);}
   cable.Guides=guides.ToArray();return cable;
  }
  static void SpatialLift(ExpansionContext c,COgheRailSlider required)
  {
   bool boss=required!=null;float y=boss?-.078f:-.278f,travel=boss?.18f:.30f;
   var rail=ExpansionRail(c,"Passenger elevator",new Vector3(.23f,y,.15f),Vector3.up,travel,0,new Vector3(.27f,.036f,.27f),.04f,.004f,true,false);rail.GetComponent<VenomMovableProp>().Manipulable=false;
   var lift=rail.gameObject.AddComponent<COghePassengerLift>();if(boss){var landing=c.Surfaces.Find(p=>p.name=="Receiving plinth"&&p.Normal.y>.9f);var enable=ViewTask(c,"B",new Vector3(.17f,-.077f,-.04f),Vector3.right,.10f,landing);enable.StandOffset=new Vector3(0,0,-.03f);required=enable.Rail;}
   lift.Rail=rail;foreach(var face in rail.GetComponentsInChildren<VenomSurfacePatch>())face.MotionFrame=rail.Body;lift.RequiredRail=required;lift.DeckSize=new Vector2(.27f,.27f);lift.DeckHeight=.018f;lift.Deck=rail.GetComponentsInChildren<VenomSurfacePatch>().First(p=>p.Normal.y>.9f);
   lift.Panel=MechanismVisual(rail.transform,boss?"B lift panel":"A lift panel",new Vector3(0,.023f,-.04f),new Vector3(.05f,.012f,.05f),metal,PrimitiveType.Cylinder);
   foreach(float x in new[]{.075f,.385f})MechanismVisual(c.Root,"Lift upright",new Vector3(x,y+travel*.5f,.245f),new Vector3(.014f,travel+.10f,.018f),metal);
   if(boss){var gate=ViewGate(c,"B coral shutter",c.Exit+Vector3.back*.02f,Vector3.left,.14f,new Vector3(.13f,.14f,.018f));ViewLink(c,rail,gate,true,ViewExitSurface(c));}
  }
 }
}
