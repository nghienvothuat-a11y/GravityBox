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
                    if(game.Definition.Order>=23&&patch.name.Contains("strip"))
                    {
                        // Grip overlays are part of the outer wall's cutaway, while the handles remain visible.
                        renderer.sharedMaterial=blue;outer.Add(patch);paneVisuals.Add(patch.transform);continue;
                    }
                    outer.Add(patch);renderer.enabled=false;
                    // Render the real perforated pane; trim stays outside its aperture and follows the cutaway.
                    var shell=Child(root,"V2 pane trim");shell.position=patch.transform.position;shell.rotation=patch.transform.rotation;
                    BuildViewPane(shell,patch,blue);
                    CombineByMaterial(shell);paneVisuals.Add(shell);continue;
                }
                if(patch.GetComponentInParent<VenomMovableProp>()!=null)continue;
                if(patch.name.StartsWith("Inspection baffle"))
                {
                    renderer.enabled=false;
                    if(Vector3.Dot(patch.Normal,Vector3.up)>.9f)
                    {
                        var skin=Child(root,"V2 satin inspection baffle");skin.position=patch.transform.position;skin.rotation=patch.transform.rotation;
                        Box(skin,"Rounded satin baffle",new Vector3(0,0,-.035f),new Vector3(patch.Size.x,patch.Size.y,.07f),.003f,lavender);
                        CombineByMaterial(skin);
                    }
                    continue;
                }
                renderer.sharedMaterial=patch.Slippery?lavender:Vector3.Dot(patch.Normal,Vector3.up)>.9f?cream:blue;
                renderer.shadowCastingMode=ShadowCastingMode.On;
                if(Vector3.Dot(patch.Normal,Vector3.up)>.9f&&!patch.Slippery)ViewFloorUV(patch,game.Definition.Order>10?.56f:.40f,game.Definition.Order>10?.40f:.30f);
                if(patch.name=="Docked bridge top")renderer.enabled=false;
                if(game.Definition.Order>10&&!patch.Slippery&&patch.Size.y>.15f&&Mathf.Abs(patch.Normal.y)<.05f&&
                    (patch.Normal.x>.1f||patch.Normal.z>.1f))
                {
                    var trim=Child(root,"V2 interior porcelain edge");trim.position=patch.transform.position;trim.rotation=patch.transform.rotation;
                    Box(trim,"Rounded partition top",new Vector3(0,patch.Size.y*.5f,-.006f),new Vector3(patch.Size.x,.006f,.012f),.002f,blue);
                    CombineByMaterial(trim);
                }
            }
            var presentation=game.GetComponent<COgheDayLabPresentation>();presentation.FadingFloors=System.Array.Empty<Renderer>();presentation.FocusOccluders=System.Array.Empty<Renderer>();
            // The state-based outer cutaway owns only renderers; collision and adhesion stay intact.
            var view=game.gameObject.AddComponent<COgheViewPresentation>();view.Panes=outer.ToArray();view.PaneVisuals=paneVisuals.ToArray();
            ConfigureViewFade(view);
            var details=Child(game.GetComponent<VenomLevelController>().Rotation.transform,"V2 porcelain platform");
            float trayX=game.Definition.Order>10?.56f:.40f, trayZ=game.Definition.Order>10?.40f:.30f;
            bool bridge=game.GetComponentInChildren<COgheDockedBridgeDeck>(true)!=null;
            bool floorExit=false;foreach(var patch in game.Surfaces)floorExit|=patch.Hole&&Vector3.Dot(patch.Normal,Vector3.up)>.9f;
            if(!bridge&&!floorExit)Box(details,"Porcelain tray",new Vector3(0,-.330f,0),new Vector3(trayX*2+.056f,.056f,trayZ*2+.056f),.009f,ivory);
            else
            {
                float baseY=bridge?-.418f:-.330f;
                foreach(float side in new[]{-1f,1f})
                {
                    Box(details,"Open-bottom tray edge",new Vector3(side*(trayX+.018f),baseY,0),new Vector3(.026f,.056f,trayZ*2+.056f),.006f,ivory);
                    Box(details,"Open-bottom tray edge",new Vector3(0,baseY,side*(trayZ+.018f)),new Vector3(trayX*2+.056f,.056f,.026f),.006f,ivory);
                }
            }
            foreach(var prop in game.Props)
            {
                bool blade=false;
                foreach(var knife in game.GetComponentsInChildren<COgheGuillotine>())if(knife.Rail.Body==prop.Body){blade=true;break;}
                if(blade)continue;
                Bounds bounds=default;bool found=false;
                foreach(var collider in prop.CollisionShapes)
                {
                    if(!(collider is BoxCollider box)||collider.GetComponent<VenomSurfacePatch>()==null||collider.name=="Bridge underside pocket cover")continue;
                    for(int i=0;i<8;i++)
                    {
                        Vector3 p=prop.transform.InverseTransformPoint(box.transform.TransformPoint(box.center+Vector3.Scale(box.size*.5f,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));
                        if(!found){bounds=new Bounds(p,Vector3.zero);found=true;}else bounds.Encapsulate(p);
                    }
                }
                if(!found)continue;
                foreach(var patch in prop.GetComponentsInChildren<VenomSurfacePatch>())
                {
                    var faceRenderer=patch.GetComponent<Renderer>();
                    bool separatePlate=patch.name=="Bridge underside pocket cover";
                    faceRenderer.enabled=separatePlate;
                    if(separatePlate)
                    {
                        faceRenderer.sharedMaterial=amber;
                        foreach(float side in new[]{-1f,1f})
                            ViewRod(prop.transform,"Pocket cover suspension",new Vector3(side*.048f,-.01f,0),new Vector3(side*.048f,-.09f,0),.004f,alloy);
                    }
                }
                Box(Child(prop.transform,"V2 shell"),"Soft amber casing",bounds.center,bounds.size,.005f,amber);
                var task=prop.GetComponent<COgheTapRail>();
                if(task!=null)
                {
                    foreach(var label in prop.GetComponentsInChildren<TextMesh>())UnityEngine.Object.DestroyImmediate(label.gameObject);
                    void HandleArt(Transform handle)
                    {
                        if(handle==null)return;var original=handle.GetComponent<Renderer>();if(original!=null)original.enabled=false;
                        var cap=Child(prop.transform,"V2 rounded handle");cap.position=handle.position;
                        if(task.HoldAtEnd)cap.rotation=Quaternion.LookRotation(-task.WorkingSurface.Normal,Vector3.up);
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
            if(game.Definition.Order>10)BuildPipeMazeArt(game,details,true);
            var batchedWheels = new HashSet<Transform>();
            foreach(var drive in game.GetComponentsInChildren<COgheViewTransmission>(true))
            {
                void Mount(Transform wheel)
                {
                    if(!batchedWheels.Add(wheel))return;
                    var prop=wheel.GetComponentInParent<VenomMovableProp>();
                    var mount=prop==null?details:prop.transform.Find("V2 shell");
                    if(mount==null)mount=wheel.parent;
                    Vector3 centre=mount.InverseTransformPoint(wheel.position);
                    Vector3 rear=mount.InverseTransformDirection(root.forward)*.018f;
                    Vector3 axis=mount.InverseTransformDirection(root.forward);
                    Disk(mount,"Satin gear axle",centre+rear*.5f,axis,.0045f,.025f,alloy);
                    Disk(mount,"Porcelain gear bearing",centre+rear,axis,.010f,.013f,ivory);
                    if(prop!=null||wheel.name.EndsWith(" source"))
                    {
                        Vector3 foot=centre+rear;foot.y=prop==null?-.294f:.014f;
                        ViewRod(mount,"Gear support stem",foot,centre+rear,.006f,ivory);
                    }
                    CombineByMaterial(wheel);
                }
                foreach(var wheel in drive.FirstWheels)Mount(wheel);
                if(drive.SecondWheels!=null)foreach(var wheel in drive.SecondWheels)Mount(wheel);
            }
            FinishViewStudio(game,details);
            CombineByMaterial(details);
            // Save generated beveled meshes; runtime only moves shared geometry.
            foreach(var prop in game.Props){var shell=prop.transform.Find("V2 shell");if(shell!=null)CombineByMaterial(shell);}
            foreach(var link in game.GetComponentsInChildren<COgheViewMechanism>(true))
            {
                var lamp=Child(link.Output.transform,"Dock status");
                Disk(lamp,"Physical state lamp",new Vector3(0,.085f,0),Vector3.back,.007f,.003f,graphite);
                link.Lamp=lamp.GetComponentInChildren<Renderer>();link.Waiting=graphite;link.Ready=mint;
            }
            foreach(var drive in game.GetComponentsInChildren<COgheViewTransmission>(true))
            {
                Renderer Status(COgheRailSlider rail,string name)
                {
                    if(rail==null)return null;
                    var lamp=Child(rail.transform,name);
                    Disk(lamp,"Measured catch lamp",new Vector3(.07f,.047f,0),Vector3.back,.006f,.003f,graphite);
                    return lamp.GetComponentInChildren<Renderer>();
                }
                drive.FirstLamp=Status(drive.First,"Catch I status");
                drive.SecondLamp=Status(drive.Second,"Catch II status");
                drive.Waiting=graphite;drive.Ready=mint;
            }
            if(game.Definition.Order==1)ApplyGlassPreview(game);else ApplySpecimenPlate(game);
            AssetDatabase.SaveAssets();
        }
    }
}
