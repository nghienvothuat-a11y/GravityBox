using System.Collections.Generic;
using GravityBox.Venom;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        public static void ApplyViewLevel(VenomCampaign game)
        {
            ApplyExpansionLevel(game,$"Meshes/ViewV2/Level{game.Definition.Order:00}");
            ivory=Lit("V2 porcelain",new Color(.87f,.83f,.75f),.02f,.38f);
            amber=Lit("V2 honey resin",new Color(.91f,.55f,.18f),.08f,.49f);
            alloy=Lit("V2 satin aluminium",new Color(.70f,.74f,.76f),.72f,.68f);
            var blue=ViewWallMaterial();
            var cream=ViewFloorMaterial();
            var lavender=Lit("V2 recovery satin",new Color(.48f,.44f,.62f),.05f,.35f);
            var root=game.GetComponent<VenomLevelController>().Rotation.transform;
            Remove(root,ArtRoot);
            var outer=new List<VenomSurfacePatch>();var paneVisuals=new List<Transform>();
            foreach(var patch in game.Surfaces)
            {
                var renderer=patch.GetComponent<Renderer>();if(renderer==null)continue;
                if(patch.ExteriorGlass)
                {
                    outer.Add(patch);renderer.enabled=false;
                    // Render the real perforated pane; trim stays outside its aperture and follows the cutaway.
                    var shell=Child(root,"V2 pane trim");shell.position=patch.transform.position;shell.rotation=patch.transform.rotation;
                    BuildViewPane(shell,patch,blue);
                    CombineByMaterial(shell);paneVisuals.Add(shell);continue;
                }
                if(patch.GetComponentInParent<VenomMovableProp>()!=null)continue;
                renderer.sharedMaterial=patch.Slippery?lavender:Vector3.Dot(patch.Normal,Vector3.up)>.9f?cream:blue;
                renderer.shadowCastingMode=ShadowCastingMode.On;
                if(Vector3.Dot(patch.Normal,Vector3.up)>.9f&&!patch.Slippery)ViewFloorUV(patch);
                if(patch.name=="Docked bridge top")renderer.enabled=false;
            }
            var presentation=game.GetComponent<COgheDayLabPresentation>();presentation.FadingFloors=System.Array.Empty<Renderer>();presentation.FocusOccluders=System.Array.Empty<Renderer>();
            // The state-based outer cutaway owns only renderers; collision and adhesion stay intact.
            var view=game.gameObject.AddComponent<COgheViewPresentation>();view.Panes=outer.ToArray();view.PaneVisuals=paneVisuals.ToArray();
            var details=Child(game.GetComponent<VenomLevelController>().Rotation.transform,"V2 porcelain platform");
            bool bridge=game.GetComponentInChildren<COgheDockedBridgeDeck>(true)!=null;
            bool floorExit=false;foreach(var patch in game.Surfaces)floorExit|=patch.Hole&&Vector3.Dot(patch.Normal,Vector3.up)>.9f;
            if(!bridge&&!floorExit)Box(details,"Porcelain tray",new Vector3(0,-.330f,0),new Vector3(.856f,.056f,.656f),.009f,ivory);
            else
            {
                float baseY=bridge?-.418f:-.330f;
                foreach(float side in new[]{-1f,1f})
                {
                    Box(details,"Open-bottom tray edge",new Vector3(side*.418f,baseY,0),new Vector3(.026f,.056f,.656f),.006f,ivory);
                    Box(details,"Open-bottom tray edge",new Vector3(0,baseY,side*.318f),new Vector3(.856f,.056f,.026f),.006f,ivory);
                }
            }
            foreach(var prop in game.Props)
            {
                Bounds bounds=default;bool found=false;
                foreach(var collider in prop.CollisionShapes)
                {
                    if(!(collider is BoxCollider box))continue;
                    for(int i=0;i<8;i++)
                    {
                        Vector3 p=prop.transform.InverseTransformPoint(box.transform.TransformPoint(box.center+Vector3.Scale(box.size*.5f,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));
                        if(!found){bounds=new Bounds(p,Vector3.zero);found=true;}else bounds.Encapsulate(p);
                    }
                }
                if(!found)continue;
                foreach(var patch in prop.GetComponentsInChildren<VenomSurfacePatch>())patch.GetComponent<Renderer>().enabled=false;
                Box(Child(prop.transform,"V2 shell"),"Soft amber casing",bounds.center,bounds.size,.005f,amber);
                var task=prop.GetComponent<COgheTapRail>();
                if(task!=null)
                {
                    foreach(var label in prop.GetComponentsInChildren<TextMesh>())UnityEngine.Object.DestroyImmediate(label.gameObject);
                    void HandleArt(Transform handle)
                    {
                        if(handle==null)return;var original=handle.GetComponent<Renderer>();if(original!=null)original.enabled=false;
                        var cap=Child(prop.transform,"V2 rounded handle");cap.position=handle.position;
                        Box(cap,"Porcelain grip socket",new Vector3(0,-.007f,.005f),new Vector3(.073f,.008f,.045f),.003f,ivory);
                        Box(cap,"Amber rounded carriage",new Vector3(0,.010f,.003f),new Vector3(.065f,.037f,.034f),.008f,amber);
                        Disk(cap,"Grip metal bezel",new Vector3(0,.012f,-.016f),Vector3.back,.023f,.004f,alloy);
                        Disk(cap,"Circular amber thumb grip",new Vector3(0,.012f,-.021f),Vector3.back,.020f,.009f,amber);
                        Label(cap,task.Label,new Vector3(0,.012f,-.026f),Quaternion.identity,.012f,ivory);
                        CombineByMaterial(cap);
                    }
                    HandleArt(task.Handle);HandleArt(task.AlternateHandle);
                    BuildViewTrack(details,task);
                }
            }
            FinishViewStudio(game,details);
            CombineByMaterial(details);
            // Save generated beveled meshes; runtime only moves shared geometry.
            foreach(var prop in game.Props)CombineByMaterial(prop.transform.Find("V2 shell"));
            foreach(var link in game.GetComponentsInChildren<COgheViewMechanism>(true))
            {
                var lamp=Child(link.Output.transform,"Dock status");
                Disk(lamp,"Physical state lamp",new Vector3(0,.085f,0),Vector3.back,.007f,.003f,graphite);
                link.Lamp=lamp.GetComponentInChildren<Renderer>();link.Waiting=graphite;link.Ready=mint;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
