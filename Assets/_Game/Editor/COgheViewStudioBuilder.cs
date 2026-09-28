using GravityBox.Venom;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        // V2-only art: no collision, navigation, rail or tissue settings are edited here.
        private static Material ViewFloorMaterial()
        {
            const int size=512;
            var texture=SaveAsset("V2 porcelain tiles.asset",()=>new Texture2D(size,size,TextureFormat.RGB24,true));
            texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Trilinear;texture.anisoLevel=2;
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(x+.5f)/size,v=(y+.5f)/size;
                float edge=Mathf.Min(Mathf.Min(u,1-u)*.8f,Mathf.Min(v,1-v)*.6f);
                float seam=Mathf.Min(Mathf.Abs(Mathf.Repeat(u*8+.5f,1)-.5f),Mathf.Abs(Mathf.Repeat(v*6+.5f,1)-.5f));
                float grout=1-Mathf.SmoothStep(0,1,seam/.018f);
                float shade=1-.14f*Mathf.Exp(-edge*95)-.14f*grout;
                float grain=(Mathf.PerlinNoise(x*.51f,y*.51f)-.5f)*.008f;
                pixels[y*size+x]=new Color(.90f,.86f,.78f)*(shade+grain);
            }
            texture.SetPixels(pixels);texture.Apply(true);EditorUtility.SetDirty(texture);
            var material=Lit("V2 warm floor",Color.white,.02f,.32f);
            material.SetTexture("_BaseMap",texture);EditorUtility.SetDirty(material);return material;
        }

        private static Material ViewWallMaterial()
        {
            const int size=128;
            var texture=SaveAsset("V2 wall shading.asset",()=>new Texture2D(size,size,TextureFormat.RGB24,true));
            texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Trilinear;
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(x+.5f)/size,v=(y+.5f)/size;
                float shade=1-.15f*Mathf.Exp(-v*11)-.07f*Mathf.Exp(-Mathf.Min(u,1-u)*35);
                pixels[y*size+x]=new Color(.64f,.76f,.85f)*shade;
            }
            texture.SetPixels(pixels);texture.Apply(true);EditorUtility.SetDirty(texture);
            var material=Lit("V2 pale blue casing",Color.white,.04f,.38f);
            material.SetTexture("_BaseMap",texture);EditorUtility.SetDirty(material);return material;
        }

        private static void ViewFloorUV(VenomSurfacePatch patch, float halfX = .40f, float halfZ = .30f)
        {
            var filter=patch.GetComponent<MeshFilter>();
            // Copy only the render mesh. The MeshCollider keeps the original geometry.
            var mesh=Object.Instantiate(filter.sharedMesh);mesh.name="V2 tiled surface";
            var vertices=mesh.vertices;var uv=new Vector2[vertices.Length];
            for(int i=0;i<vertices.Length;i++)
            {
                Vector3 p=patch.transform.TransformPoint(vertices[i]);
                uv[i]=new Vector2((p.x+halfX)/(halfX*2),(p.z+halfZ)/(halfZ*2));
            }
            mesh.uv=uv;filter.sharedMesh=SavedMesh(mesh);
        }

        private static void BuildViewPane(Transform shell,VenomSurfacePatch patch,Material blue)
        {
            bool roof=Mathf.Abs(Vector3.Dot(patch.Normal,Vector3.up))>.9f;
            if(!patch.Hole)
            {
                // Thickness grows out of the room. The playable inner plane stays at z=0.
                Box(shell,"Rounded blue wall",new Vector3(0,0,-.017f),new Vector3(patch.Size.x+.008f,patch.Size.y+.008f,.034f),.004f,blue);
            }
            else
            {
                var surface=Child(shell,"Blue wall with real aperture");
                var mesh=Object.Instantiate(patch.GetComponent<MeshFilter>().sharedMesh);var vertices=mesh.vertices;
                for(int i=0;i<vertices.Length;i++)if(vertices[i].z<-.001f)vertices[i].z=-.034f;
                mesh.vertices=vertices;mesh.RecalculateBounds();
                surface.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                surface.gameObject.AddComponent<MeshRenderer>().sharedMaterial=blue;
                Box(shell,"Soft top wall cap",new Vector3(0,patch.Size.y*.5f,-.017f),new Vector3(patch.Size.x+.008f,.008f,.034f),.003f,blue);
                foreach(float side in new[]{-1f,1f})Box(shell,"Soft wall end",new Vector3(side*patch.Size.x*.5f,0,-.017f),new Vector3(.008f,patch.Size.y,.034f),.003f,blue);
            }
            if(!roof)
            {
                Box(shell,"Aluminium glass seat",new Vector3(0,patch.Size.y*.5f+.007f,-.014f),new Vector3(patch.Size.x+.012f,.003f,.006f),.001f,alloy);
                foreach(float side in new[]{-1f,1f})
                    Box(shell,"Fine glass edge",new Vector3(side*(patch.Size.x*.5f+.007f),0,-.015f),new Vector3(.003f,patch.Size.y+.016f,.004f),.001f,alloy);
            }
            foreach(var filter in shell.GetComponentsInChildren<MeshFilter>())
            {
                if(filter.GetComponent<Renderer>().sharedMaterial!=blue)continue;
                var mesh=filter.sharedMesh;var vertices=mesh.vertices;var uv=new Vector2[vertices.Length];
                for(int i=0;i<vertices.Length;i++)
                {
                    Vector3 p=shell.InverseTransformPoint(filter.transform.TransformPoint(vertices[i]));
                    uv[i]=new Vector2(p.x/patch.Size.x+.5f,p.y/patch.Size.y+.5f);
                }
                mesh.uv=uv;
            }
        }

        private static void ViewRod(Transform parent,string name,Vector3 a,Vector3 b,float radius,Material material)
            =>Disk(parent,name,(a+b)*.5f,(b-a).normalized,radius,Vector3.Distance(a,b),material);

        private static void BuildViewTrack(Transform details,COgheTapRail task)
        {
            var rail=task.Rail;Vector3 support=task.HoldAtEnd?task.WorkingSurface.Normal:Vector3.up;
            Vector3 side=Vector3.Cross(rail.Axis,support).normalized;
            // A bridge's rail stays by its handle on the bank, clear of the walking deck.
            Vector3 handleOffset=rail.Frame.InverseTransformVector(task.Handle.position-rail.transform.position);
            Vector3 start=rail.Start+handleOffset-support*.012f;
            foreach(float sign in new[]{-1f,1f})
                ViewRod(details,"Paired satin guide",start+side*(sign*.023f)-rail.Axis*.015f,start+rail.Axis*(rail.Travel+.015f)+side*(sign*.023f),.0035f,alloy);
            foreach(float travel in new[]{0f,rail.Travel})
            {
                var mount=Child(details,"Porcelain end bearing");mount.localPosition=start+rail.Axis*travel;
                mount.localRotation=Quaternion.LookRotation(rail.Axis,support);
                Box(mount,"Rounded bearing block",Vector3.down*.003f,new Vector3(.072f,.029f,.022f),.005f,ivory);
                foreach(float sign in new[]{-1f,1f})Disk(mount,"Satin bearing collar",new Vector3(sign*.023f,0,-.0115f),Vector3.forward,.005f,.002f,alloy);
            }
        }

        private static void FinishViewStudio(VenomCampaign game,Transform details)
        {
            var owner=game.GetComponent<VenomLevelController>();
            var root=owner.Rotation.transform;
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())
            {
                string name=renderer.name;
                if(name.EndsWith("visible socket")||name.EndsWith("fixed guide")||name.EndsWith("rail stop"))renderer.enabled=false;
                if(name=="Visible linkage housing")
                {
                    var t=renderer.transform;renderer.enabled=false;
                    Vector3 a=root.InverseTransformPoint(t.TransformPoint(Vector3.back*.5f));
                    Vector3 b=root.InverseTransformPoint(t.TransformPoint(Vector3.forward*.5f));
                    ViewRod(details,"Satin transmission shaft",a,b,.0035f,alloy);
                    Disk(details,"Transmission pivot",a,Vector3.up,.008f,.005f,alloy);
                    Disk(details,"Transmission pivot",b,Vector3.up,.008f,.005f,alloy);
                }
            }
            // Output tracks stay aligned with their physical shutter, outside real openings.
            foreach(var rail in owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())
            {
                if(rail.GetComponent<COgheTapRail>()!=null)continue;
                foreach(Transform child in root)
                    if(child.name==rail.name+" fixed guide")
                    {
                        Vector3 half=rail.Axis*(rail.Travel+.04f)*.5f;
                        ViewRod(details,"Shutter guide",child.localPosition-half,child.localPosition+half,.003f,alloy);
                    }
            }
            var bench=SaveAsset("V2 neutral studio.mat",()=>new Material(Shader.Find("Universal Render Pipeline/Unlit")));
            bench.shader=Shader.Find("Universal Render Pipeline/Unlit");bench.SetColor("_BaseColor",new Color(.945f,.933f,.897f));EditorUtility.SetDirty(bench);
            var studio=owner.transform.Find("Day Lab studio");
            foreach(var renderer in studio.GetComponentsInChildren<MeshRenderer>())
                if(renderer.sharedMaterial.name=="Warm porcelain")renderer.sharedMaterial=bench;
            foreach(var light in studio.GetComponentsInChildren<Light>())
            {
                if(light.shadows!=LightShadows.None)
                {
                    light.color=new Color(1,.985f,.965f);light.intensity=1.1f;light.shadowStrength=.27f;
                    light.shadowBias=.008f;light.shadowNormalBias=.015f;
                }
                else light.intensity=.45f;
            }
            RenderSettings.ambientSkyColor=new Color(.70f,.75f,.80f);
            RenderSettings.ambientEquatorColor=new Color(.49f,.53f,.54f);
            RenderSettings.ambientGroundColor=new Color(.28f,.26f,.23f);
            owner.View.backgroundColor=new Color(.89f,.875f,.83f);
            // Match the concept's lower, left-front viewpoint; the observation lesson
            // keeps its authored hidden-target angle.
            if(game.Definition.Order!=3)game.Definition.CameraEuler=new Vector3(37,-16,0);
            EditorUtility.SetDirty(game.Definition);
        }
    }
}
