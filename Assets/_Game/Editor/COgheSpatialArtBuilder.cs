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

  /// <summary>
  /// Mrk's chapter-1 rules on levels 11–50 (05/10/2026: "tiếp tục sửa những chương sau theo quy tắc này"): colours, not
  /// letters; no dark parts. An art pass on the existing scenes, never a regeneration (regenerating reorders objects and
  /// flips marginal physics); physics, input, mechanisms and definitions are verified unchanged.
  /// </summary>
  [MenuItem("Gravity Box/COghe/Spatial/Colour rule on levels 11–50 (physics unchanged)")]
  public static void ApplyColourRuleToLaterChapters()
  {
   const string report="Artifacts/COgheSpatialColourRule";
   Directory.CreateDirectory(report);
   var paths=VenomCampaignBuilder.SpatialScenePaths().Skip(10).Where(File.Exists).ToArray();
   string before=COgheViewArtVerification.CapturePhysics(paths);
   File.WriteAllText(report+"/physics-before.txt",before);
   var definitions=Directory.GetFiles(VenomCampaignBuilder.SpatialFolder+"/Definitions","*.asset").OrderBy(p=>p).ToArray();
   var definitionText=definitions.Select(File.ReadAllText).ToArray();
   var gunmetal=AssetDatabase.LoadAssetAtPath<Material>(VenomCampaignBuilder.Folder+"/Blade.mat");
   foreach(var path in paths)
   {
    var scene=EditorSceneManager.OpenScene(path);
    var game=Object.FindFirstObjectByType<VenomCampaign>();
    Material Load(string n)=>AssetDatabase.LoadAssetAtPath<Material>(VenomCampaignBuilder.SpatialFolder+"/"+n+".mat");
    DecorateSpatialMechanisms(game,Load("Circuit A blue"),Load("Circuit B coral"),Load("Ivory bodies"));
    // Spatial 11–30 / Plus art (VenomCampaignBuilder.NextSpatialArt): pads and printed traces take their circuit's colour.
    foreach(var sensor in game.GetComponentsInChildren<COgheTissueSensor>(true))
     if(sensor.Cap!=null)sensor.Cap.GetComponent<Renderer>().sharedMaterial=Circuit(game,sensor.name).body;
    foreach(var r in game.GetComponentsInChildren<MeshRenderer>(true))
     if(r.name.StartsWith("Printed conductor ")&&r.transform.parent!=null&&r.transform.parent.name=="Spatial next printed traces")
      r.sharedMaterial=Circuit(game,r.name.Substring("Printed conductor ".Length)).trace;
    Debug.Log($"COLOUR RULE {game.Definition.Order:00}: {RetireLetters(game)} letters removed, {NoDarkParts(game,gunmetal)} dark parts now satin or circuit colour");
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();
   string after=COgheViewArtVerification.CapturePhysics(paths);
   File.WriteAllText(report+"/physics-after.txt",after);
   if(before!=after||!definitionText.SequenceEqual(definitions.Select(File.ReadAllText)))
    throw new InvalidOperationException("Colour rule changed physical/input data or definitions.");
   Debug.Log($"COLOUR RULE VERIFIED: {paths.Length} scenes; identical physics, input, mechanisms and definitions.");
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
   if(ink==null)ink=Lit("Spatial label ink",new Color(.10f,.17f,.21f),0,.3f);
   Remove(root,SpatialCircuitRoot);var art=Child(root,SpatialCircuitRoot);
   var tasks=owner.Apparatus.GetComponentsInChildren<COgheTapRail>();
   bool pilot=(game.Definition.Id??"").Contains(".pilot.");
   // Mrk (05/10/2026): no letters anywhere; a control and what it works share one colour of their own (Circuit).
   (Material body,Material trace) Colour(string label)=>Circuit(game,label);
   string Driver(COgheRailSlider output)
   {
    foreach(var link in owner.Apparatus.GetComponentsInChildren<COgheViewMechanism>())
     if(link.Output==output&&link.Input!=null){var t=link.Input.GetComponent<COgheTapRail>();if(t!=null)return t.Label;}
    return output.name.StartsWith("B")?"B":"A";
   }
   RetireLetters(game);

   // Retire decorative bars only. None of these objects is a collider or input target.
   foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
   {
    if(r.name=="Visible linkage housing"||r.name.Contains("visible socket"))r.enabled=false;
    if(r.name.Contains("fixed guide")||r.name.Contains("rail stop"))
    {
     if(r.GetComponent<Collider>()!=null)throw new InvalidOperationException("Unexpected collider on guide decoration");
     r.enabled=false;
     if(r.name.Contains("rail stop")||ChapterOneHiddenLock(r.name))continue;
     var mount=Child(art,"Satin "+r.name);mount.position=r.transform.position;mount.rotation=r.transform.rotation;
     var size=new Vector3(.003f,r.transform.lossyScale.y,.003f);
     var task=Array.Find(tasks,t=>r.name==t.Rail.name+" fixed guide");
     // Guide lines only where they lie on a surface (handles, shutters); a lift's, bridge's or pin's stood in mid-air.
     if(task==null&&!r.name.Contains("shutter")){Object.DestroyImmediate(mount.gameObject);continue;}
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
    if(r.name=="Pulley bearing support")r.sharedMaterial=railMat;
   }
   // No posts beside lift shafts: dark columns read as clutter (Mrk, 05/10/2026).
   foreach(var post in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Lift upright").ToArray())Object.DestroyImmediate(post.gameObject);

   foreach(var rail in owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())
   {
    Remove(rail.transform,SpatialCircuitRoot);
    // Keep lavender contact areas distinct; neutralize the machine casing. A bolt wears the colour of what pulls it.
    var casing=rail.name.Contains(" bolt")?Colour(rail.name).body:shell;
    foreach(var r in rail.GetComponentsInChildren<MeshRenderer>())
    {
     if(r.GetComponent<TextMesh>()!=null){r.enabled=false;continue;}
     var patch=r.GetComponent<VenomSurfacePatch>();
     if(patch!=null&&patch.Slippery)continue;
     r.sharedMaterial=casing;
    }
    if(!rail.name.Contains("shutter"))continue;
    var trim=Child(rail.transform,SpatialCircuitRoot);
    // Narrow identity band, rather than a large saturated door panel. Moving artwork
    // is parented to the actual shutter; never added to the stationary trace batch.
    Box(trim,"Circuit identity band",new Vector3(-.051f,0,-.010f),new Vector3(.010f,.09f,.001f),.0004f,Colour(Driver(rail)).body);
    CombineByMaterial(trim);
   }

   foreach(var task in owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
   {
    var color=Colour(task.Label);
    Remove(task.Rail.transform,"Circuit badge");
    var trim=Child(task.Rail.transform,SpatialCircuitRoot);
    bool wall=Mathf.Abs(task.WorkingSurface.Normal.y)<.5f;
    foreach(var r in task.Handle.GetComponentsInChildren<MeshRenderer>())if(r.GetComponent<TextMesh>()==null)r.sharedMaterial=color.body;
    // Small fixed contact pads at each end. The travel itself stays quiet satin metal.
    foreach(float end in new[]{0f,task.Rail.Travel})
    {
     Vector3 p=task.Rail.Start+task.Rail.Axis*end;
     p+=wall?new Vector3(0,0,.024f):new Vector3(0,-.020f,0);
     Box(art,"Terminal base",p,wall?new Vector3(.028f,.033f,.002f):new Vector3(.030f,.002f,.040f),.0008f,shell);
     Disk(art,"Terminal contact",p+(wall?Vector3.back:Vector3.up)*.0013f,wall?Vector3.back:Vector3.up,.0035f,.0005f,color.trace);
    }
    CombineByMaterial(trim);
   }

   foreach(var link in owner.Apparatus.GetComponentsInChildren<COgheViewMechanism>())
   {
    var task=link.Input.GetComponent<COgheTapRail>();
    // Chapter 1 draws the trace to a box or its clip itself (VenomCampaignBuilder.ChapterOneFinish).
    if(VenomCampaignBuilder.ChapterOneOwnTrace(link.Output))continue;
    var colour=Colour(task!=null?task.Label:Driver(link.Output));
    Vector3 from=link.Input.Start+link.Input.Axis*link.Input.Travel;
    Vector3 to=link.Output.Start+new Vector3(-.055f,-.045f,0);
    // Printed on fixed receiving surfaces: floor -> rear glass, wall -> rear glass,
    // or the top of the raised plinth -> rear glass. No floating diagonal shaft.
    if(task!=null&&task.WorkingSurface.Normal.y>.5f)
    {
     float y=root.InverseTransformPoint(task.WorkingSurface.transform.position).y+.0012f;
     from.y=y;
     SpatialTrace(art,new[]{from,new Vector3(from.x+.025f,y,from.z),new Vector3(from.x+.025f,y,.298f),new Vector3(from.x+.025f,to.y,.298f),new Vector3(to.x,to.y,.298f)},colour.trace);
    }
    else
    {
     from.z=.298f;
     SpatialTrace(art,new[]{from,new Vector3(from.x,to.y,.298f),new Vector3(to.x,to.y,.298f)},colour.trace);
    }
    SpatialPort(art,new Vector3(to.x,to.y,.2965f),Vector3.back,colour.body,shell);
   }

   foreach(var pulley in owner.Apparatus.GetComponentsInChildren<COghePulleyDrive>())
   {
    var colour=Colour(pulley.Command!=null?pulley.Command.Label:"A");
    var wheels=root.GetComponentsInChildren<Transform>().Where(t=>t.name=="A pulley wheel").ToArray();pulley.Wheels=wheels;
    Remove(root,"A geared cable winch");
    // Chapter 1's winch sits 1.5 cm further back, clear of the box over its handle.
    var t=Child(root,"A geared cable winch");t.localPosition=pulley.Input.Start+new Vector3(-.01f,.038f,pilot?.063f:.048f);
    var drum=Child(t,"Winding drum");drum.localRotation=Quaternion.Euler(90,0,0);
    Disk(drum,"Spool",Vector3.zero,Vector3.up,.027f,.036f,railMat);
    Disk(drum,"Blue winding flange",Vector3.up*.019f,Vector3.up,.028f,.002f,colour.body);
    CombineByMaterial(drum);pulley.Drum=drum;pulley.DrumAnchor=t;
    var bearing=Child(t,"Bearing casing");Box(bearing,"Ivory bearing",new Vector3(0,-.028f,0),new Vector3(.074f,.018f,.064f),.004f,shell);CombineByMaterial(bearing);
    // The cable is the winch's connection, in its colour.
    pulley.Cable.sharedMaterial=colour.trace;pulley.Cable.startWidth=pulley.Cable.endWidth=.0022f;pulley.Cable.shadowCastingMode=ShadowCastingMode.Off;
    foreach(var w in wheels)
    {
     w.GetComponent<Renderer>().sharedMaterial=railMat;Remove(w,"Ivory bearing hub");Remove(w,SpatialCircuitRoot);
     var hub=Child(w,SpatialCircuitRoot);
     // Wheel transform already carries the authored cylinder scale.
     Disk(hub,"A axle cap",new Vector3(0,1.015f,0),Vector3.up,.23f,.035f,colour.body);CombineByMaterial(hub);
    }
   }
   // Every lift button is coral, as in chapter 1.
   foreach(var lift in owner.Apparatus.GetComponentsInChildren<COghePassengerLift>())
   {
    lift.Panel.GetComponent<Renderer>().sharedMaterial=coral;
    foreach(var call in lift.CallPanels)if(call!=null)foreach(var r in call.GetComponentsInChildren<Renderer>())r.sharedMaterial=coral;
   }
   CombineByMaterial(art);
   foreach(var r in art.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
   owner.IndicatorMaterial=mint;COgheDayLabPresentation.ConfigureExitOutline(owner);
   RefineSpatialReadability(game);
  }

  /// <summary>Colour of a control's circuit (Mrk, 05/10/2026): no letters, a control and what it works share a colour.
  /// The letter that names it in code picks the colour: A blue, B coral, C amber, D green, E and P (motor pads) pink;
  /// A1/A2, C1–C3… share their letter's colour. Chapter 1's level 7 lever D wears amber.</summary>
  internal static (Material body,Material trace) Circuit(VenomCampaign game,string label)
  {
   string key=VenomCampaignBuilder.ChapterOneColourKey(game,label);
   key=string.IsNullOrEmpty(key)?"A":key.Substring(0,1).ToUpperInvariant();if(key=="P")key="E";
   switch(key)
   {
    case "B":return (VenomCampaignBuilder.SpatialMaterial("Circuit B coral",new Color(.784f,.424f,.345f)),Lit("Spatial printed B",new Color(.67f,.43f,.36f),.08f,.36f));
    case "C":return (VenomCampaignBuilder.SpatialMaterial("Circuit C amber",VenomCampaignBuilder.ChapterAmber),Lit("Spatial printed C",new Color(.74f,.56f,.18f),.08f,.36f));
    case "D":return (VenomCampaignBuilder.SpatialMaterial("Circuit D green",VenomCampaignBuilder.ChapterGreen),Lit("Spatial printed D",new Color(.36f,.58f,.27f),.08f,.36f));
    case "E":return (VenomCampaignBuilder.SpatialMaterial("Circuit E pink",new Color(.78f,.40f,.64f)),Lit("Spatial printed E",new Color(.66f,.40f,.56f),.08f,.36f));
    default:return (VenomCampaignBuilder.SpatialMaterial("Circuit A blue",new Color(.224f,.498f,.678f)),Lit("Spatial printed A",new Color(.30f,.51f,.58f),.08f,.36f));
   }
  }
  /// <summary>Letters that only named a control (A, B2, P…) go; Q's name, weights (50%) and the level plaque stay.</summary>
  internal static int RetireLetters(VenomCampaign game)
  {
   int gone=0;
   foreach(var t in game.GetComponentsInChildren<TextMesh>(true).ToArray())
    if(t.text!="Q"&&System.Text.RegularExpressions.Regex.IsMatch(t.text??"","^[A-Z][0-9]?$")){Object.DestroyImmediate(t.gameObject);gone++;}
   return gone;
  }
  /// <summary>No dark parts (Mrk, 05/10/2026): gunmetal pieces become satin, or their circuit's colour when they belong
  /// to one (a lock, a bolt, a cable named for it); the dark woven cable likewise.</summary>
  internal static int NoDarkParts(VenomCampaign game,Material gunmetal)
  {
   var railMat=Lit("Spatial satin guides",new Color(.60f,.69f,.70f),.24f,.40f);var cord=Lit("Spatial woven cable",new Color(.34f,.41f,.42f),.12f,.30f);
   int changed=0;
   foreach(var r in game.GetComponentsInChildren<Renderer>(true))
   {
    if(r.sharedMaterial==null||r.sharedMaterial!=gunmetal&&r.sharedMaterial!=cord)continue;
    var m=System.Text.RegularExpressions.Regex.Match(r.name,"^([A-Z][0-9]?) ");
    bool line=r.name.Contains("cable")||r.name.EndsWith(" rope")||r.name.Contains("rope swing");
    bool owned=m.Success&&(line||r.name.Contains(" lock")||r.name.Contains(" bolt"));
    r.sharedMaterial=!owned?railMat:line?Circuit(game,m.Groups[1].Value).trace:Circuit(game,m.Groups[1].Value).body;
    changed++;
   }
   return changed;
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
