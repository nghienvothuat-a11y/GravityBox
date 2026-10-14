using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
 public static class COgheCircuitSurfaceAudit
 {
  public const string Report="Artifacts/COghePipeCircuit20261014";
  [Serializable] public class Issue {public int level;public string scene,renderer,nearest;public Vector3 point;public float distance;public int samples,unsupported;}
  [Serializable] public class Results {public int scenes,renderers,samples,unsupported;public List<Issue> issues=new List<Issue>();public List<int> pipeLevels=new List<int>();}
  public static bool IsCircuit(MeshRenderer r)=>r.enabled&&r.sharedMaterial!=null&&(r.sharedMaterial.name.StartsWith("Spatial printed ")||r.name=="Batched Spatial satin guides"&&r.transform.parent.name=="Spatial circuit artwork");
  [MenuItem("Gravity Box/COghe/Spatial/Audit circuit surface support")]
  public static void Run()
  {
   Directory.CreateDirectory(Report);var result=new Results();
   foreach(var path in VenomCampaignBuilder.SpatialScenePaths())
   {
    var scene=EditorSceneManager.OpenScene(path);var game=Object.FindFirstObjectByType<VenomCampaign>();
    var root=game.GetComponent<VenomLevelController>().Rotation.transform;result.scenes++;
    if(game.GetComponentsInChildren<COgheTubeNetwork>(true).Length>0||game.Tube!=null)result.pipeLevels.Add(game.Definition.Order);
    var support=new COghePrintedCircuit(root);
    foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true).Where(IsCircuit))
    {
     var mf=r.GetComponent<MeshFilter>();if(mf==null||mf.sharedMesh==null)continue;
     result.renderers++;var mesh=mf.sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;
     var issue=new Issue{level=game.Definition.Order,scene=path,renderer=r.name};
     void Sample(Vector3 local)
     {
      Vector3 world=r.transform.TransformPoint(local);var anchor=support.Anchor(world);float best=Vector3.Distance(world,anchor.point);
      issue.samples++;result.samples++;
      if(best>.004f){issue.unsupported++;result.unsupported++;}
      if(best>issue.distance){issue.distance=best;issue.point=root.InverseTransformPoint(world);issue.nearest=anchor.face.Name;}
     }
     foreach(var v in vertices)Sample(v);
     for(int i=0;i<triangles.Length;i+=3)Sample((vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3);
     if(issue.unsupported>0)result.issues.Add(issue);
    }
   }
   File.WriteAllText(Report+"/surface-audit.json",JsonUtility.ToJson(result,true));
   Debug.Log($"CIRCUIT AUDIT {result.scenes} scenes, {result.renderers} renderers, {result.unsupported}/{result.samples} unsupported samples, {result.issues.Count} issues.");
   if(result.unsupported>0)throw new InvalidOperationException("Printed circuits have unsupported geometry; see surface-audit.json");
  }
  [MenuItem("Gravity Box/COghe/Spatial/Repair printed circuits on physical surfaces")]
  public static void Repair()
  {
   Directory.CreateDirectory(Report);var paths=VenomCampaignBuilder.SpatialScenePaths();
   string before=COgheViewArtVerification.CapturePhysics(paths);File.WriteAllText(Report+"/physics-before.txt",before);
   foreach(var path in paths)
   {
    var scene=EditorSceneManager.OpenScene(path);var game=Object.FindFirstObjectByType<VenomCampaign>();var root=game.GetComponent<VenomLevelController>().Rotation.transform;
    Material Load(string n)=>AssetDatabase.LoadAssetAtPath<Material>(VenomCampaignBuilder.SpatialFolder+"/"+n+".mat");
    COgheDayLabBuilder.DecorateSpatialMechanisms(game,Load("Circuit A blue"),Load("Circuit B coral"),Load("Ivory bodies"));
    var support=new COghePrintedCircuit(root);
    // Preserve each authored route's endpoints and colour; replace only its visual strip, never level objects.
    foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("Printed conductor")&&IsCircuit(r)).ToArray())
    {
     var t=r.transform;var parent=t.parent;
     var a=parent.InverseTransformPoint(t.TransformPoint(Vector3.back*.5f));var b=parent.InverseTransformPoint(t.TransformPoint(Vector3.forward*.5f));
     support.Draw(parent,new[]{a,b},r.sharedMaterial,r.name);Object.DestroyImmediate(r.gameObject);
    }
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();string after=COgheViewArtVerification.CapturePhysics(paths);File.WriteAllText(Report+"/physics-after.txt",after);
   if(before!=after)throw new InvalidOperationException("Circuit repair changed physical/gameplay data");
   Run();
  }
 }
}
