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
            var blue=Lit("V2 pale blue casing",new Color(.68f,.80f,.86f),.04f,.38f);
            var cream=Lit("V2 warm floor",new Color(.94f,.91f,.82f),.04f,.35f);
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
                    var surface=Child(shell,"Blue casing");surface.gameObject.AddComponent<MeshFilter>().sharedMesh=patch.GetComponent<MeshFilter>().sharedMesh;
                    var casing=surface.gameObject.AddComponent<MeshRenderer>();casing.sharedMaterial=blue;casing.shadowCastingMode=ShadowCastingMode.Off;
                    foreach(float side in new[]{-1f,1f})
                    {
                        Box(shell,"Porcelain edge",new Vector3(side*(patch.Size.x*.5f+.004f),0,-.004f),new Vector3(.009f,patch.Size.y+.016f,.014f),.003f,ivory);
                        Box(shell,"Porcelain edge",new Vector3(0,side*(patch.Size.y*.5f+.004f),-.004f),new Vector3(patch.Size.x+.016f,.009f,.014f),.003f,ivory);
                    }
                    CombineByMaterial(shell);paneVisuals.Add(shell);continue;
                }
                if(patch.GetComponentInParent<VenomMovableProp>()!=null)continue;
                renderer.sharedMaterial=patch.Slippery?lavender:Vector3.Dot(patch.Normal,Vector3.up)>.9f?cream:blue;
                renderer.shadowCastingMode=ShadowCastingMode.On;
                if(patch.name=="Docked bridge top")renderer.enabled=false;
            }
            var presentation=game.GetComponent<COgheDayLabPresentation>();presentation.FadingFloors=System.Array.Empty<Renderer>();presentation.FocusOccluders=System.Array.Empty<Renderer>();
            // The state-based outer cutaway owns only renderers; collision and adhesion stay intact.
            var view=game.gameObject.AddComponent<COgheViewPresentation>();view.Panes=outer.ToArray();view.PaneVisuals=paneVisuals.ToArray();
            var details=Child(game.GetComponent<VenomLevelController>().Rotation.transform,"V2 porcelain platform");
            bool bridge=game.GetComponentInChildren<COgheDockedBridgeDeck>(true)!=null;
            bool floorExit=false;foreach(var patch in game.Surfaces)floorExit|=patch.Hole&&Vector3.Dot(patch.Normal,Vector3.up)>.9f;
            if(!bridge&&!floorExit)Box(details,"Porcelain tray",new Vector3(0,-.320f,0),new Vector3(.83f,.036f,.63f),.012f,ivory);
            else
            {
                float baseY=bridge?-.408f:-.320f;
                foreach(float side in new[]{-1f,1f})
                {
                    Box(details,"Open-bottom tray edge",new Vector3(side*.408f,baseY,0),new Vector3(.018f,.024f,.63f),.006f,ivory);
                    Box(details,"Open-bottom tray edge",new Vector3(0,baseY,side*.308f),new Vector3(.83f,.024f,.018f),.006f,ivory);
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
                        Box(cap,"Porcelain grip socket",new Vector3(0,-.009f,0),new Vector3(.073f,.007f,.044f),.003f,ivory);
                        Box(cap,"Amber thumb grip",Vector3.zero,new Vector3(.065f,.021f,.035f),.008f,amber);
                        Label(cap,task.Label,new Vector3(0,.013f,0),Quaternion.Euler(90,0,0),.022f,ink);
                        CombineByMaterial(cap);
                    }
                    HandleArt(task.Handle);HandleArt(task.AlternateHandle);
                    foreach(float travel in new[]{0f,task.Rail.Travel})
                        Box(details,"Visible stop",task.Rail.Start+task.Rail.Axis*travel+new Vector3(0,-.012f,0),new Vector3(.080f,.010f,.060f),.004f,ivory);
                }
            }
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
