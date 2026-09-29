using System;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace GravityBox.Editor
{
 // Engineering probe only (not part of the catalog): how tall a slick riser the creature can surmount.
 public static partial class VenomCampaignBuilder
 {
  public const string SpatialProbeScene="Assets/_Game/Venom/SpatialCampaign/Probe/COgheSpatialProbe.unity";
  public static void GenerateSpatialProbe()
  {
   PrepareCampaign30Assets();Directory.CreateDirectory(Path.GetDirectoryName(SpatialProbeScene));AssetDatabase.Refresh();
   string old=authoredMeshFolder;authoredMeshFolder=SpatialFolder+"/Probe";meshSerial=99000;
   try
   {
    var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    var owner=new GameObject("COghe Spatial probe").AddComponent<VenomLevelController>();
    owner.MatterProfile=AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder+"/Matter.asset");owner.ControlMode=VenomControlMode.TouchSurface;
    owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");owner.IndicatorMaterial=mint;owner.ApertureRadius=.046f;
    var game=owner.gameObject.AddComponent<VenomCampaign>();var def=ScriptableObject.CreateInstance<VenomCampaignDefinition>();
    AssetDatabase.CreateAsset(def,SpatialFolder+"/Probe/ProbeDefinition.asset");def.Id="probe";def.Order=99;def.Title="probe";def.ViewOnly=true;def.CanRotate=false;def.CameraEuler=new Vector3(36,20,0);game.Definition=def;
    owner.Apparatus=new GameObject("Apparatus").transform;owner.Apparatus.SetParent(owner.transform,false);
    var pivot=new GameObject("Fixed chamber",typeof(Rigidbody),typeof(BoxRotationController));pivot.transform.SetParent(owner.Apparatus,false);pivot.GetComponent<Rigidbody>().isKinematic=true;owner.Rotation=pivot.GetComponent<BoxRotationController>();
    var c=new ExpansionContext{Number=199,Owner=owner,Game=game,Definition=def,Root=pivot.transform,Spawn=new Vector3(0,-.25f,-.20f),Exit=new Vector3(.30f,-.225f,.30f),Outward=Vector3.forward};
    NextShell(c,.10f,true);var floor=c.Surfaces[0];
    float[] heights={.02f,.035f,.05f,.065f,.08f};
    for(int i=0;i<heights.Length;i++)NextPlinth(c,"Probe riser "+(int)(heights[i]*1000),new Vector3(-.32f+i*.16f,-.30f+heights[i]*.5f,.08f),new Vector3(.13f,heights[i],.16f));
    owner.Spawn=new GameObject("Spawn").transform;owner.Spawn.SetParent(c.Root,false);owner.Spawn.localPosition=c.Spawn;
    owner.Outlet=new GameObject("Final exit").transform;owner.Outlet.SetParent(c.Root,false);owner.Outlet.localPosition=c.Exit;
    game.Surfaces=c.Surfaces.ToArray();game.Props=c.Props.ToArray();owner.CrawlFaces=Enumerable.Repeat(floor.Shape,6).ToArray();
    var camera=new GameObject("Camera",typeof(Camera)).GetComponent<Camera>();camera.transform.SetParent(owner.transform,false);camera.orthographic=true;camera.transform.rotation=Quaternion.Euler(def.CameraEuler);camera.transform.position=-camera.transform.forward*3;owner.View=camera;
    EditorSceneManager.SaveScene(scene,SpatialProbeScene);
   }
   finally{authoredMeshFolder=old;}
  }
 }
}
