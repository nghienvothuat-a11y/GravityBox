using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
 // Renders the Spatial Plus levels for their design plates: the in-game view at spawn and a plan view, with the
 // builder's notes (steps, routes) projected into both. Explicit: run it by name; it is not part of the suites.
 [Explicit("Design plates for Spatial Plus; run by name")]
 public sealed class COgheSpatialPlusDesignRender
 {
  const string Scenes="Assets/_Game/Venom/SpatialCampaign",Out="Artifacts/SpatialPlusDesign";
  [UnityTest] public IEnumerator RenderPlusDesigns()
  {
   var mode=Physics.simulationMode;var persistence=VenomCampaignSave.PersistenceEnabled;
   Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;
   try
   {
    foreach(var path in Directory.GetFiles(Scenes,"COgheSpatialPlus*.unity").OrderBy(p=>p))
    {
     string key=Path.GetFileNameWithoutExtension(path).Substring("COgheSpatialPlus".Length);
     yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
     yield return null;
     var game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
     // Let the body settle at spawn so its skin reads as the creature, then render.
     for(int i=0;i<90;i++){game.Owner.Step(1f/120);Physics.Simulate(1f/120);}
     yield return null;yield return null;
     game.CameraRig.Frame(720,1280,0,true);
     var notes=File.Exists($"{Out}/{key}.notes")?File.ReadAllLines($"{Out}/{key}.notes"):new string[0];
     var lines=new List<string>();
     Render(game.Owner.View,720,1280,$"{Out}/{key}-game.png",notes,game.Root,"game",lines);
     var plan=new GameObject("Design plan camera",typeof(Camera)).GetComponent<Camera>();
     plan.orthographic=true;plan.clearFlags=CameraClearFlags.SolidColor;plan.backgroundColor=new Color(.95f,.95f,.93f);plan.nearClipPlane=.01f;plan.farClipPlane=10;
     plan.transform.rotation=game.Root.rotation*Quaternion.Euler(90,0,0);plan.transform.position=game.Root.TransformPoint(new Vector3(0,1.5f,0));
     plan.orthographicSize=.33f; // the 0.8 × 0.6 box with a margin, at 4:3
     Render(plan,960,720,$"{Out}/{key}-plan.png",notes,game.Root,"plan",lines);
     Object.Destroy(plan.gameObject);
     File.WriteAllLines($"{Out}/{key}.proj",lines);
    }
   }
   finally{Physics.simulationMode=mode;VenomCampaignSave.PersistenceEnabled=persistence;}
  }
  static void Render(Camera camera,int w,int h,string file,string[] notes,Transform root,string view,List<string> lines)
  {
   var target=RenderTexture.GetTemporary(w,h,24);var previous=RenderTexture.active;var texture=new Texture2D(w,h,TextureFormat.RGB24,false);
   try
   {
    camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,w,h),0,0);texture.Apply();
    File.WriteAllBytes(file,texture.EncodeToPNG());
    foreach(var note in notes)
    {
     var f=note.Split('|');
     var local=new Vector3(float.Parse(f[2],CultureInfo.InvariantCulture),float.Parse(f[3],CultureInfo.InvariantCulture),float.Parse(f[4],CultureInfo.InvariantCulture));
     var s=camera.WorldToScreenPoint(root.TransformPoint(local));
     lines.Add(string.Join("|",view,f[0],f[1],s.x.ToString("F1",CultureInfo.InvariantCulture),(h-s.y).ToString("F1",CultureInfo.InvariantCulture)));
    }
   }
   finally{camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.DestroyImmediate(texture);}
  }
 }
}
