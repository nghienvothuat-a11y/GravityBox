using System;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
 public static partial class COgheDayLabBuilder
 {
  private const string SpatialCircuitRoot="Spatial circuit artwork";

  [MenuItem("Gravity Box/COghe/Spatial pilot/Refine circuits and verify physics")]
  public static void RebuildSpatialCircuits()
  {
   const string report="Artifacts/COgheSpatialCircuits";
   Directory.CreateDirectory(report);
   var paths=VenomCampaignBuilder.SpatialScenePaths().Where(File.Exists).ToArray();
   string before=COgheViewArtVerification.CapturePhysics(paths);
   File.WriteAllText(report+"/physics-before.txt",before);
   var definitions=Directory.GetFiles(VenomCampaignBuilder.SpatialFolder+"/Definitions","*.asset").OrderBy(p=>p).ToArray();
   var definitionText=definitions.Select(File.ReadAllText).ToArray();
   foreach(var path in paths)
   {
    var scene=EditorSceneManager.OpenScene(path);
    var game=Object.FindFirstObjectByType<VenomCampaign>();
    Material Load(string n)=>AssetDatabase.LoadAssetAtPath<Material>(VenomCampaignBuilder.SpatialFolder+"/"+n+".mat");
    DecorateSpatialMechanisms(game,Load("Circuit A blue"),Load("Circuit B coral"),Load("Ivory bodies"));
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();
   string after=COgheViewArtVerification.CapturePhysics(paths);
   File.WriteAllText(report+"/physics-after.txt",after);
   if(before!=after||!definitionText.SequenceEqual(definitions.Select(File.ReadAllText)))
    throw new InvalidOperationException("Spatial circuit art changed physical/input data or definitions.");
   Debug.Log($"SPATIAL CIRCUIT ART VERIFIED: {paths.Length} scenes; identical physics, input, mechanisms and definitions.");
  }

  public static void RebuildSpatialCircuitsAndBuildMac()
  {
   RebuildSpatialCircuits();
   VenomCampaignBuilder.BuildSpatialMac();
  }

  public static void DecorateSpatialMechanisms(VenomCampaign game,Material blue,Material coral,Material ivoryBody)
  {
   var owner=game.Owner!=null?game.Owner:game.GetComponent<VenomLevelController>();
   var root=owner.Rotation.transform;
   meshDirectory=$"Meshes/SpatialCircuit/{SpatialArtKey(game)}";serial=0;
   Directory.CreateDirectory(Folder+"/"+meshDirectory);AssetDatabase.Refresh();
   // Load approved shared assets without reinitializing unrelated materials.
   ink=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Blue grey lettering.mat");
   if(ink==null)ink=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Label ink.mat");
   worldLabels=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/World labels.mat");
   mint=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Quiet mint light.mat");
   var railMat=Lit("Spatial satin guides",new Color(.60f,.69f,.70f),.24f,.40f);
   var shell=Lit("Spatial pearl casing",new Color(.86f,.85f,.77f),.08f,.40f);
   var traceA=Lit("Spatial printed A",new Color(.30f,.51f,.58f),.08f,.36f);
   var traceB=Lit("Spatial printed B",new Color(.67f,.43f,.36f),.08f,.36f);
   var cord=Lit("Spatial woven cable",new Color(.34f,.41f,.42f),.12f,.30f);
   if(ink==null)ink=Lit("Spatial label ink",new Color(.10f,.17f,.21f),0,.3f);
   Remove(root,SpatialCircuitRoot);var art=Child(root,SpatialCircuitRoot);
   var tasks=owner.Apparatus.GetComponentsInChildren<COgheTapRail>();

   // Retire decorative bars only. None of these objects is a collider or input target.
   foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
   {
    if(r.name=="Visible linkage housing"||r.name.Contains("visible socket"))r.enabled=false;
    if(r.name.Contains("fixed guide")||r.name.Contains("rail stop"))
    {
     if(r.GetComponent<Collider>()!=null)throw new InvalidOperationException("Unexpected collider on guide decoration");
     r.enabled=false;
     if(r.name.Contains("rail stop"))continue;
     var mount=Child(art,"Satin "+r.name);mount.position=r.transform.position;mount.rotation=r.transform.rotation;
     var size=new Vector3(.003f,r.transform.lossyScale.y,.003f);
     var task=Array.Find(tasks,t=>r.name==t.Rail.name+" fixed guide");
     if(task!=null)
     {
      // Floor controls have coplanar printed guides, not the old pair of bars
      // floating above/below the carriage. Wall controls use the rear-glass plane.
      bool wall=Mathf.Abs(task.WorkingSurface.Normal.y)<.5f;
      float sign=r.transform.localPosition.y>task.Rail.Start.y?1:-1;
      Vector3 p=task.Rail.Start+task.Rail.Axis*task.Rail.Travel*.5f;
      if(wall){p.z=.293f;p.y+=sign*.022f;}
      else {p.y=root.InverseTransformPoint(task.WorkingSurface.transform.position).y+.0015f;p.z+=sign*.030f;}
      mount.localPosition=p;
     }
     Box(mount,"Recessed guide",Vector3.zero,size,.001f,railMat);
    }
    if(r.name=="Lift upright"||r.name=="Pulley bearing support")r.sharedMaterial=railMat;
   }

   foreach(var rail in owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())
   {
    Remove(rail.transform,SpatialCircuitRoot);
    // Keep lavender contact areas and text distinct; neutralize the machine casing.
    foreach(var r in rail.GetComponentsInChildren<MeshRenderer>())
    {
     if(r.GetComponent<TextMesh>()!=null)
     {
      // Retire the old oversized/free-floating letters; the authored input handle
      // and its collider are unchanged. One badge is attached to each body below.
      r.enabled=false;continue;
     }
     var patch=r.GetComponent<VenomSurfacePatch>();
     if(patch!=null&&patch.Slippery)continue;
     r.sharedMaterial=shell;
    }
    if(!rail.name.Contains("shutter"))continue;
    bool b=rail.name.StartsWith("B");var trim=Child(rail.transform,SpatialCircuitRoot);
    // Narrow identity band, rather than a large saturated door panel. Moving artwork
    // is parented to the actual shutter; never added to the stationary trace batch.
    Box(trim,"Circuit identity band",new Vector3(-.051f,0,-.010f),new Vector3(.010f,.09f,.001f),.0004f,b?coral:blue);
    SpatialBadge(trim,b?"B":"A",new Vector3(0,0,-.012f),Vector3.back,b?coral:blue,shell);
    CombineByMaterial(trim);
   }

   foreach(var task in owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
   {
    // A and B are the two control circuits. Any other handle (a latch C, a winch E…) is neutral: ink badge, pearl
    // handle, satin contacts — never a third circuit colour.
    bool neutral=task.Label!="A"&&task.Label!="B";
    var color=neutral?ink:task.Label=="B"?coral:blue;
    Remove(task.Rail.transform,"Circuit badge");
    var trim=Child(task.Rail.transform,SpatialCircuitRoot);
    bool wall=Mathf.Abs(task.WorkingSurface.Normal.y)<.5f;
    SpatialBadge(trim,task.Label,wall?new Vector3(0,0,-.029f):new Vector3(0,.014f,0),wall?Vector3.back:Vector3.up,color,shell);
    foreach(var r in task.Handle.GetComponentsInChildren<MeshRenderer>())if(r.GetComponent<TextMesh>()==null)r.sharedMaterial=neutral?shell:color;
    // Small fixed contact pads at each end. The travel itself stays quiet satin metal.
    foreach(float end in new[]{0f,task.Rail.Travel})
    {
     Vector3 p=task.Rail.Start+task.Rail.Axis*end;
     p+=wall?new Vector3(0,0,.024f):new Vector3(0,-.020f,0);
     Box(art,"Terminal base",p,wall?new Vector3(.028f,.033f,.002f):new Vector3(.030f,.002f,.040f),.0008f,shell);
     Disk(art,"Terminal contact",p+(wall?Vector3.back:Vector3.up)*.0013f,wall?Vector3.back:Vector3.up,.0035f,.0005f,neutral?railMat:task.Label=="B"?traceB:traceA);
    }
    CombineByMaterial(trim);
   }

   foreach(var link in owner.Apparatus.GetComponentsInChildren<COgheViewMechanism>())
   {
    bool b=link.Output.name.StartsWith("B");
    var task=link.Input.GetComponent<COgheTapRail>();
    Vector3 from=link.Input.Start+link.Input.Axis*link.Input.Travel;
    Vector3 to=link.Output.Start+new Vector3(-.055f,-.045f,0);
    // Printed on fixed receiving surfaces: floor -> rear glass, wall -> rear glass,
    // or the top of the raised plinth -> rear glass. No floating diagonal shaft.
    if(task!=null&&task.WorkingSurface.Normal.y>.5f)
    {
     float y=root.InverseTransformPoint(task.WorkingSurface.transform.position).y+.0012f;
     from.y=y;
     SpatialTrace(art,new[]{from,new Vector3(from.x+.025f,y,from.z),new Vector3(from.x+.025f,y,.298f),new Vector3(from.x+.025f,to.y,.298f),new Vector3(to.x,to.y,.298f)},b?traceB:traceA);
    }
    else
    {
     from.z=.298f;
     SpatialTrace(art,new[]{from,new Vector3(from.x,to.y,.298f),new Vector3(to.x,to.y,.298f)},b?traceB:traceA);
    }
    SpatialPort(art,new Vector3(to.x,to.y,.2965f),Vector3.back,b?coral:blue,shell);
   }

   foreach(var pulley in owner.Apparatus.GetComponentsInChildren<COghePulleyDrive>())
   {
    var wheels=root.GetComponentsInChildren<Transform>().Where(t=>t.name=="A pulley wheel").ToArray();pulley.Wheels=wheels;
    Remove(root,"A geared cable winch");
    var t=Child(root,"A geared cable winch");t.localPosition=pulley.Input.Start+new Vector3(-.01f,.038f,.048f);
    var drum=Child(t,"Winding drum");drum.localRotation=Quaternion.Euler(90,0,0);
    Disk(drum,"Spool",Vector3.zero,Vector3.up,.027f,.036f,railMat);
    Disk(drum,"Blue winding flange",Vector3.up*.019f,Vector3.up,.028f,.002f,blue);
    CombineByMaterial(drum);pulley.Drum=drum;pulley.DrumAnchor=t;
    var bearing=Child(t,"Bearing casing");Box(bearing,"Ivory bearing",new Vector3(0,-.028f,0),new Vector3(.074f,.018f,.064f),.004f,shell);CombineByMaterial(bearing);
    pulley.Cable.sharedMaterial=cord;pulley.Cable.startWidth=pulley.Cable.endWidth=.0022f;pulley.Cable.shadowCastingMode=ShadowCastingMode.Off;
    foreach(var w in wheels)
    {
     w.GetComponent<Renderer>().sharedMaterial=railMat;Remove(w,"Ivory bearing hub");Remove(w,SpatialCircuitRoot);
     var hub=Child(w,SpatialCircuitRoot);
     // Wheel transform already carries the authored cylinder scale.
     Disk(hub,"A axle cap",new Vector3(0,1.015f,0),Vector3.up,.23f,.035f,blue);CombineByMaterial(hub);
    }
   }
   foreach(var lift in owner.Apparatus.GetComponentsInChildren<COghePassengerLift>())
   {
    // Pilot: the boss lift is B's. Spatial 11–30: a lift powered through an enabling rail belongs to B's circuit.
    bool pilot=(game.Definition.Id??"").Contains(".pilot."),b=game.Definition.Boss&&pilot||!pilot&&lift.RequiredRail!=null;
    lift.Panel.GetComponent<Renderer>().sharedMaterial=b?coral:blue;
    var trim=lift.Rail.transform.Find(SpatialCircuitRoot)??Child(lift.Rail.transform,SpatialCircuitRoot);
    // The panel itself keeps its depression motion; no overlay obstructs the tap area.
    SpatialBadge(trim,b?"B":"A",new Vector3(-.05f,.020f,-.06f),Vector3.up,b?coral:blue,shell);
    CombineByMaterial(trim);
    if(lift.RequiredRail!=null&&pilot)
    {
     var start=lift.RequiredRail.Start+lift.RequiredRail.Axis*lift.RequiredRail.Travel;start.y=-.0988f;
     SpatialTrace(art,new[]{start,new Vector3(start.x,-.0988f,.278f),new Vector3(.385f,-.0988f,.278f)},traceB);
    }
   }
   CombineByMaterial(art);
   foreach(var r in art.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
   owner.IndicatorMaterial=mint;COgheDayLabPresentation.ConfigureExitOutline(owner);
  }

  private static void SpatialBadge(Transform parent,string text,Vector3 point,Vector3 normal,Material color,Material backing)
  {
   var badge=Child(parent,"Identity "+text);badge.localPosition=point;badge.localRotation=Quaternion.FromToRotation(Vector3.up,normal);
   Disk(badge,"Porcelain rim",Vector3.zero,Vector3.up,.018f,.0014f,color);
   Disk(badge,"Circuit enamel",Vector3.up*.001f,Vector3.up,.015f,.001f,backing);
   Label(badge,text,Vector3.up*.002f,Quaternion.Euler(90,0,0),.010f,ink);
  }
  private static void SpatialPort(Transform parent,Vector3 p,Vector3 normal,Material color,Material backing)
  {
   Disk(parent,"Receiver surround",p,normal,.008f,.001f,backing);
   Disk(parent,"Receiver contact",p+normal*.0006f,normal,.004f,.0005f,color);
  }
  private static void SpatialTrace(Transform parent,Vector3[] points,Material mat)
  {
   // Two millimetre printed tracks. Chamfer planar elbows, keep surface-fold corners
   // sharp so the conductor stays on the glass/floor instead of cutting across space.
   var path=new System.Collections.Generic.List<Vector3>{points[0]};
   for(int i=1;i<points.Length-1;i++)
   {
    var a=(points[i-1]-points[i]).normalized;var b=(points[i+1]-points[i]).normalized;
    bool samePlane=(Mathf.Abs(points[i-1].y-points[i].y)<.0001f&&Mathf.Abs(points[i+1].y-points[i].y)<.0001f)||
      (Mathf.Abs(points[i-1].z-points[i].z)<.0001f&&Mathf.Abs(points[i+1].z-points[i].z)<.0001f);
    float bevel=Mathf.Min(.010f,Mathf.Min(Vector3.Distance(points[i-1],points[i]),Vector3.Distance(points[i+1],points[i]))*.25f);
    if(samePlane){path.Add(points[i]+a*bevel);path.Add(points[i]+b*bevel);}else path.Add(points[i]);
   }
   path.Add(points[points.Length-1]);
   for(int i=1;i<path.Count;i++)
   {
    var d=path[i]-path[i-1];if(d.sqrMagnitude<.0000001f)continue;
    var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Printed conductor";go.transform.SetParent(parent,false);
    go.transform.localPosition=(path[i]+path[i-1])*.5f;
    Vector3 normal=Mathf.Abs(d.y)<.0001f&&Mathf.Abs(path[i].z-.298f)>.0001f?Vector3.up:Vector3.back;
    go.transform.localRotation=Quaternion.LookRotation(d,normal);go.transform.localScale=new Vector3(.003f,.0005f,d.magnitude+.0002f);
    Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;
   }
  }
 }
}
