using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    /// <summary>Authored Unity mesh kit. Only presentation is replaced; the level's
    /// original surface patches, physics meshes, transforms and masses stay intact.</summary>
    public static class COgheDayLabBuilder
    {
        public const string Folder = "Assets/_Game/Venom/Art/DayLab";
        private const string ArtRoot = "COghe · Day Lab art";
        private static Material ivory, graphite, alloy, amber, floor, mint, ink;
        private static int serial;

        [MenuItem("Gravity Box/COghe/Rebuild Day Lab · Level 07")]
        public static void RebuildLevel07()
        {
            var scene = EditorSceneManager.OpenScene(VenomCampaignBuilder.Folder+"/VenomOrigin07.unity");
            Apply(Object.FindFirstObjectByType<VenomCampaign>());
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE DAY LAB: Level 07 art rebuilt; physics geometry unchanged.");
        }

        public static void Apply(VenomCampaign game)
        {
            if(game == null || game.Definition.Order != 7) throw new InvalidOperationException("Day Lab prototype requires level 07.");
            Directory.CreateDirectory(Folder+"/Meshes"); AssetDatabase.Refresh(); serial=0;
            var owner=game.GetComponent<VenomLevelController>();
            var root=owner.Rotation.transform;
            Remove(root,ArtRoot); Remove(owner.transform,"Day Lab studio");
            foreach(var prop in game.Props) Remove(prop.transform,"Amber resin shell");
            ivory=Lit("Warm porcelain",new Color(.91f,.88f,.81f),.08f,.42f);
            graphite=Lit("Graphite fittings",new Color(.065f,.10f,.12f),.55f,.56f);
            alloy=Lit("Brushed aluminium",new Color(.65f,.74f,.77f),.60f,.61f);
            amber=Lit("Amber resin",new Color(.84f,.61f,.32f),.03f,.52f);
            floor=Lit("Pearl tray",new Color(.77f,.83f,.84f),.08f,.38f);
            mint=Lit("Quiet mint light",new Color(.32f,.70f,.59f),.10f,.6f);
            mint.EnableKeyword("_EMISSION");mint.SetColor("_EmissionColor",new Color(.10f,.27f,.19f));
            ink=Lit("Label ink",new Color(.10f,.17f,.21f),0,.3f);
            Material skin=Lit("COghe obsidian",new Color(.022f,.030f,.035f),.32f,.80f);
            var profile=SaveAsset("Day Lab matter.asset",()=>ScriptableObject.CreateInstance<VenomProfile>());
            EditorUtility.CopySerialized(AssetDatabase.LoadAssetAtPath<VenomProfile>(VenomCampaignBuilder.Folder+"/Matter.asset"),profile);
            profile.Skin=skin;EditorUtility.SetDirty(profile);owner.MatterProfile=profile;owner.IndicatorMaterial=mint;

            Material clear=Glass("Clear optical glass",.025f,0,false);
            Material coat=Glass("Ice satin coating",.24f,1,true);
            Material top=Glass("Ceiling satin coating",.085f,.65f,false);
            foreach(var p in game.Surfaces)
            {
                var renderer=p.GetComponent<MeshRenderer>();
                if(p.GetComponentInParent<VenomMovableProp>()!=null){renderer.enabled=false;continue;}
                renderer.enabled=true;
                renderer.sharedMaterial=p.transform.localPosition.y<-.29f?floor:p.Slippery?(p.RingGrip?coat:top):clear;
                renderer.shadowCastingMode=ShadowCastingMode.Off;
                renderer.receiveShadows=!p.Slippery;
                foreach(var line in p.GetComponentsInChildren<LineRenderer>()) line.enabled=false;
            }
            foreach(var line in owner.Outlet.GetComponentsInChildren<LineRenderer>()) line.sharedMaterial=mint;

            var art=Child(root,ArtRoot);
            var nearCorner=Child(art,"Near corner · hide for inspection");
            // All external trim lives outside the original 0.6 m play volume.
            Box(art,"Lower instrument chassis",new Vector3(0,-.326f,0),new Vector3(.652f,.048f,.652f),.010f,ivory);
            Box(art,"Dark lower gasket",new Vector3(0,-.351f,0),new Vector3(.609f,.005f,.609f),.002f,graphite);
            foreach(float a in new[]{-1f,1f})
            {
                Box(art,"Top rim X",new Vector3(0,.307f,a*.308f),new Vector3(.62f,.010f,.014f),.004f,alloy);
                Box(art,"Top rim Z",new Vector3(a*.308f,.307f,0),new Vector3(.014f,.010f,.62f),.004f,alloy);
                Box(art,"Tray bright edge X",new Vector3(0,-.300f,a*.307f),new Vector3(.62f,.004f,.007f),.002f,alloy);
                Box(art,"Tray bright edge Z",new Vector3(a*.307f,-.300f,0),new Vector3(.007f,.004f,.62f),.002f,alloy);
                foreach(float b in new[]{-1f,1f})
                {
                    var corner=a<0&&b<0?nearCorner:art;
                    Box(corner,"Vertical glass joint",new Vector3(a*.309f,0,b*.309f),new Vector3(.009f,.606f,.009f),.003f,alloy);
                    Box(art,"Foot",new Vector3(a*.275f,-.36f,b*.275f),new Vector3(.067f,.020f,.067f),.008f,graphite);
                    foreach(float y in new[]{-.294f,.29f})
                    {
                        Box(corner,"Corner porcelain mount",new Vector3(a*.313f,y,b*.313f),new Vector3(.045f,.059f,.045f),.008f,ivory);
                        // Recessed fastener with a dark socket and a steel pin.
                        var pos=new Vector3(a*.313f,y,b*.337f);
                        Disk(corner,"Fastener socket",pos,Vector3.forward*b,.009f,.0015f,graphite);
                        Disk(corner,"Fastener pin",pos+Vector3.forward*b*.001f,Vector3.forward*b,.004f,.001f,alloy);
                    }
                }
            }
            Box(art,"Identification plaque",new Vector3(-.18f,.242f,-.318f),new Vector3(.11f,.063f,.013f),.006f,ivory);
            Label(art,"07",new Vector3(-.18f,.242f,-.326f),Quaternion.identity,.019f,ink);
            Box(art,"COghe plaque",new Vector3(0,-.328f,-.327f),new Vector3(.158f,.030f,.006f),.003f,alloy);
            Label(art,"COghe",new Vector3(0,-.327f,-.331f),Quaternion.identity,.010f,ink);

            var wall=Array.Find(game.Surfaces,p=>p.RingGrip);
            Ring(art,"Grip island boundary",wall.transform.TransformPoint(new Vector3(wall.HoleCentre.x,wall.HoleCentre.y,.0008f)),wall.Normal,.15f,.00055f,alloy);
            Ring(art,"Flush exit lip",owner.Outlet.position-owner.Outlet.forward*.0008f,owner.Outlet.forward,owner.ApertureRadius+.0018f,.0017f,alloy);
            // Small satin ticks communicate a coating, without covering the body.
            for(int i=0;i<5;i++)
                Box(art,"Slip material marks",new Vector3(.299f,-.262f+i*.006f,.235f),new Vector3(.001f,.001f,.024f-i*.003f),.0004f,alloy);

            foreach(var prop in game.Props)
            {
                var shell=Child(prop.transform,"Amber resin shell");
                Box(shell,"Rounded resin body",Vector3.zero,Vector3.one*.18f,.006f,amber);
                // Marks sit on the body, entirely inside its collision footprint.
                var face=Child(shell,"Resin batch mark");face.localPosition=new Vector3(0,0,-.09005f);
                Label(face,"07 / R",Vector3.zero,Quaternion.identity,.0055f,ink);
                for(int i=0;i<3;i++)Box(shell,"Mould seam detail",new Vector3(-.038f+i*.012f,-.063f,-.09005f),new Vector3(.006f,.0015f,.0001f),.00004f,ivory);
            }

            var studio=Child(owner.transform,"Day Lab studio");
            Material desk=Lit("Warm lab bench",new Color(.90f,.89f,.855f),0,.2f);
            Box(studio,"Seamless laboratory bench",new Vector3(0,-.379f,0),new Vector3(12,.018f,12),.006f,desk);
            SoftShadow(studio,"Soft chassis contact",new Vector3(.025f,-.369f,.025f),.88f,.23f,.64f);
            var stepShadow=SoftShadow(owner.Apparatus,"Day Lab step contact",new Vector3(game.Props[0].transform.position.x,-.2997f,game.Props[0].transform.position.z),.255f,.13f,.56f);
            Lighting(owner,studio);
            var presentation=owner.GetComponent<COgheDayLabPresentation>();
            if(presentation==null)presentation=owner.gameObject.AddComponent<COgheDayLabPresentation>();
            presentation.Step=game.Props[0].transform;presentation.StepShadow=stepShadow;
            // Static mesh batching reduces the cost of the decorative fasteners
            // and frame. The movable crate is combined separately and stays dynamic.
            CombineByMaterial(art,nearCorner);
            CombineByMaterial(nearCorner);
            CombineByMaterial(studio);
            foreach(var prop in game.Props)CombineByMaterial(prop.transform.Find("Amber resin shell"));
            presentation.FocusOccluders=nearCorner.GetComponentsInChildren<Renderer>();EditorUtility.SetDirty(presentation);
            // Remove stale outputs when this kit uses fewer batches on a rebuild.
            foreach(string guid in AssetDatabase.FindAssets("t:Mesh",new[]{Folder+"/Meshes"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid),name=Path.GetFileNameWithoutExtension(path);
                if(name.StartsWith("LabMesh")&&int.TryParse(name.Substring(7),out int index)&&index>=serial)AssetDatabase.DeleteAsset(path);
            }
            EditorUtility.SetDirty(owner);
        }

        private static void Lighting(VenomLevelController owner,Transform studio)
        {
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))Object.DestroyImmediate(l.gameObject);
            var key=new GameObject("Day Lab · large window",typeof(Light)).GetComponent<Light>();key.transform.SetParent(studio,false);
            key.type=LightType.Directional;key.transform.rotation=Quaternion.Euler(48,-35,0);key.color=new Color(1,.97f,.91f);key.intensity=1.10f;
            key.shadows=LightShadows.Soft;key.shadowStrength=.20f;key.shadowBias=.015f;key.shadowNormalBias=.10f;key.shadowNearPlane=.05f;RenderSettings.sun=key;
            var fill=new GameObject("Day Lab · cool bounce",typeof(Light)).GetComponent<Light>();fill.transform.SetParent(studio,false);
            fill.type=LightType.Directional;fill.transform.rotation=Quaternion.Euler(30,125,0);fill.color=new Color(.81f,.90f,1);fill.intensity=.45f;fill.shadows=LightShadows.None;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.70f,.77f,.82f);
            RenderSettings.ambientEquatorColor=new Color(.49f,.54f,.54f);RenderSettings.ambientGroundColor=new Color(.32f,.30f,.25f);RenderSettings.ambientIntensity=1;
            RenderSettings.skybox=null;RenderSettings.fog=false;
            RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.reflectionIntensity=.8f;
            RenderSettings.customReflectionTexture=StudioReflection();
            owner.View.backgroundColor=new Color(.86f,.85f,.80f);
        }

        private static Cubemap StudioReflection()
        {
            const int size=128;
            var cube=SaveAsset("Day studio reflection.asset",()=>new Cubemap(size,TextureFormat.RGBAHalf,true));
            var pixels=new Color[size*size];
            for(int face=0;face<6;face++)
            {
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    float u=(x+.5f)/size*2-1,v=(y+.5f)/size*2-1;Vector3 d;
                    switch((CubemapFace)face){case CubemapFace.PositiveX:d=new Vector3(1,-v,-u);break;case CubemapFace.NegativeX:d=new Vector3(-1,-v,u);break;case CubemapFace.PositiveY:d=new Vector3(u,1,v);break;case CubemapFace.NegativeY:d=new Vector3(u,-1,-v);break;case CubemapFace.PositiveZ:d=new Vector3(u,-v,1);break;default:d=new Vector3(-u,-v,-1);break;}
                    d.Normalize();Color c=Color.Lerp(new Color(.11f,.13f,.14f),new Color(.54f,.59f,.63f),Mathf.InverseLerp(-.5f,.9f,d.y));
                    float a=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,new Vector3(-.5f,.8f,-.5f).normalized)),26);
                    float b=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,new Vector3(.9f,.3f,.4f).normalized)),50);
                    pixels[y*size+x]=c+new Color(1,.89f,.71f)*a*1.8f+new Color(.72f,.86f,1)*b*.7f;
                }
                cube.SetPixels(pixels,(CubemapFace)face);
            }
            cube.Apply(true);EditorUtility.SetDirty(cube);return cube;
        }
        private static Material Glass(string name,float alpha,float frost,bool grip)
        {
            var m=SaveAsset(name+".mat",()=>new Material(Shader.Find("COghe/Lab Glass")));m.shader=Shader.Find("COghe/Lab Glass");
            m.SetColor("_BaseColor",new Color(.64f,.78f,.82f,alpha));m.SetFloat("_Frost",frost);m.SetFloat("_HasGrip",grip?1:0);m.SetVector("_GripCentre",new Vector4(0,.05f,.15f,0));EditorUtility.SetDirty(m);return m;
        }
        private static Material Lit(string name,Color color,float metallic,float smoothness)
        {
            var m=SaveAsset(name+".mat",()=>new Material(Shader.Find("Universal Render Pipeline/Lit")));
            m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smoothness);m.SetFloat("_Cull",0);EditorUtility.SetDirty(m);return m;
        }
        private static T SaveAsset<T>(string name,Func<T> create) where T:Object
        {var p=Folder+"/"+name;var a=AssetDatabase.LoadAssetAtPath<T>(p);if(a==null){a=create();AssetDatabase.CreateAsset(a,p);}return a;}
        private static Transform Child(Transform parent,string name)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
        private static void Remove(Transform parent,string name)
        {var old=parent.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);}
        private static Transform SoftShadow(Transform root,string name,Vector3 position,float width,float alpha,float core)
        {
            Remove(root,name);
            var material=SaveAsset(name+".mat",()=>new Material(Shader.Find("COghe/Soft Contact Shadow")));
            material.SetColor("_BaseColor",new Color(.21f,.26f,.27f,alpha));material.SetFloat("_Core",core);EditorUtility.SetDirty(material);
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name=name;quad.transform.SetParent(root,false);
            quad.transform.localPosition=position;quad.transform.localRotation=Quaternion.Euler(90,0,0);quad.transform.localScale=Vector3.one*width;
            Object.DestroyImmediate(quad.GetComponent<Collider>());var renderer=quad.GetComponent<Renderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            return quad.transform;
        }
        private static Mesh SavedMesh(Mesh source)
        {
            string path=$"Meshes/LabMesh{serial++:000}.asset";var mesh=SaveAsset(path,()=>new Mesh());EditorUtility.CopySerialized(source,mesh);Object.DestroyImmediate(source);return mesh;
        }
        private static void Box(Transform root,string name,Vector3 p,Vector3 size,float radius,Material material)
        {
            var go=Child(root,name);go.localPosition=p;
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var indices=new List<int>();
            var half=size*.5f;float r=Mathf.Min(radius,Mathf.Min(half.x,Mathf.Min(half.y,half.z))*.99f);var inner=half-Vector3.one*r;
            foreach(var n in new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back})
            {
                var u=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.up;var v=Vector3.Cross(n,u);int start=vertices.Count;
                float h=Vector3.Dot(half,new Vector3(Mathf.Abs(n.x),Mathf.Abs(n.y),Mathf.Abs(n.z)));
                float hu=Mathf.Abs(Vector3.Dot(half,u)),hv=Mathf.Abs(Vector3.Dot(half,v));
                float[] Values(float extent)=>new[]{-extent,-extent+r*.293f,-extent+r,0,extent-r,extent-r*.293f,extent};
                foreach(float y in Values(hv))foreach(float x in Values(hu))
                {
                    var raw=n*h+u*x+v*y;var close=new Vector3(Mathf.Clamp(raw.x,-inner.x,inner.x),Mathf.Clamp(raw.y,-inner.y,inner.y),Mathf.Clamp(raw.z,-inner.z,inner.z));
                    var normal=(raw-close).normalized;vertices.Add(close+normal*r);normals.Add(normal);
                }
                for(int y=0;y<6;y++)for(int x=0;x<6;x++){int a=start+y*7+x;indices.AddRange(new[]{a,a+1,a+7,a+1,a+8,a+7});}
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
            go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;go.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        private static void Disk(Transform parent,string name,Vector3 point,Vector3 normal,float radius,float depth,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=point;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,normal);go.transform.localScale=new Vector3(radius*2,depth*.5f,radius*2);Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;
        }
        private static void Ring(Transform parent,string name,Vector3 point,Vector3 normal,float radius,float width,Material mat)
        {
            var go=Child(parent,name);go.position=point;go.rotation=Quaternion.LookRotation(normal);var line=go.gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=mat;line.positionCount=97;line.startWidth=line.endWidth=width;line.shadowCastingMode=ShadowCastingMode.Off;
            for(int i=0;i<=96;i++){float a=i*Mathf.PI*2/96;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));}
        }
        private static void Label(Transform parent,string text,Vector3 p,Quaternion rotation,float height,Material material)
        {
            var t=Child(parent,"Label · "+text);t.localPosition=p;t.localRotation=rotation;
            var label=t.gameObject.AddComponent<TextMesh>();label.text=text;label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=96;label.characterSize=height*.35f;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=material.GetColor("_BaseColor");
            t.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            t.GetComponent<Renderer>().sharedMaterial=label.font.material;
        }
        private static void CombineByMaterial(Transform root,Transform exclude=null)
        {
            var groups=new Dictionary<Material,List<MeshFilter>>();
            foreach(var f in root.GetComponentsInChildren<MeshFilter>())
            {if(exclude!=null&&f.transform.IsChildOf(exclude))continue;var renderer=f.GetComponent<MeshRenderer>();if(renderer==null||!renderer.enabled)continue;var m=renderer.sharedMaterial;if(!groups.ContainsKey(m))groups[m]=new List<MeshFilter>();groups[m].Add(f);}
            foreach(var pair in groups)
            {
                var instances=new List<CombineInstance>();foreach(var f in pair.Value)instances.Add(new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix});
                var mesh=new Mesh{name="Batched "+pair.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(instances.ToArray(),true,true);
                var t=Child(root,mesh.name);t.gameObject.AddComponent<MeshFilter>().sharedMesh=SavedMesh(mesh);var renderer=t.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=pair.Key;
                if(pair.Key.shader.name=="COghe/Soft Contact Shadow")renderer.shadowCastingMode=ShadowCastingMode.Off;
                foreach(var f in pair.Value){var source=f.sharedMesh;Object.DestroyImmediate(f.gameObject);if(!AssetDatabase.Contains(source))Object.DestroyImmediate(source);}
            }
        }
    }
}
