using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    /// <summary>
    /// Bakery look for the review level (Mrk, 10/10/2026: "phương án kết hợp", one level as close to the concept as possible).
    /// Runs after generation on the saved scene, presentation only: colliders, surfaces and mechanisms are untouched.
    /// Code builds what must fit each level (rounded blocks over the real faces, floor tiles, the backrest, lighting); Codex's
    /// authored kit (Assets/_Game/Venom/Art/Bakery, Tools/Bakery/README.md) gives the props: cherry, plate, pad P, candy
    /// handle, gummy lamp, cream drip, and the biscuit/wafer materials. Generated meshes, textures and materials are saved
    /// under Assets/_Game/Venom/Art/BakeryKit/&lt;key&gt;. Usage: -executeMethod GravityBox.Editor.COgheBakeryDress.DressFromCommandLine
    /// -coghe-bakery-levels N41
    /// </summary>
    public static class COgheBakeryDress
    {
        const string Kit="Assets/_Game/Venom/Art/Bakery",Out="Assets/_Game/Venom/Art/BakeryKit";
        public static void DressFromCommandLine()
        {
            var args=Environment.GetCommandLineArgs();string keys="N41";
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-bakery-levels")keys=args[i+1];
            foreach(var key in keys.Split(','))Dress(key.Trim());
        }

        // ---- materials -------------------------------------------------------------------------------------------------
        [Serializable] class KitColor{public float r,g,b,a=1;}
        [Serializable] class KitMaterial{public string name;public KitColor color;public float smoothness;public string baseColorTexture,normalTexture,maskTexture;public float normalScale=1;}
        [Serializable] class KitManifest{public KitMaterial[] materials;}
        static Shader Lit=>Shader.Find("Universal Render Pipeline/Lit");
        static string dir;
        static Material SaveMaterial(string name,Color colour,float smooth,Texture2D baseMap=null,Texture2D normal=null,float normalScale=1,Texture2D mask=null)
        {
            string path=$"{dir}/Materials/BK {name}.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Lit);AssetDatabase.CreateAsset(m,path);}
            m.shader=Lit;m.SetColor("_BaseColor",colour);m.SetFloat("_Metallic",0);m.SetFloat("_Smoothness",mask!=null?1:smooth);
            m.SetTexture("_BaseMap",baseMap);
            if(normal!=null){m.SetTexture("_BumpMap",normal);m.SetFloat("_BumpScale",normalScale);m.EnableKeyword("_NORMALMAP");}else{m.SetTexture("_BumpMap",null);m.DisableKeyword("_NORMALMAP");}
            if(mask!=null){m.SetTexture("_MetallicGlossMap",mask);m.SetFloat("_SmoothnessTextureChannel",0);m.EnableKeyword("_METALLICSPECGLOSSMAP");}else{m.SetTexture("_MetallicGlossMap",null);m.DisableKeyword("_METALLICSPECGLOSSMAP");}
            EditorUtility.SetDirty(m);return m;
        }
        static Texture2D KitTexture(string file,bool normal,bool linear)
        {
            if(string.IsNullOrEmpty(file))return null;string path=$"{Kit}/{file}";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer!=null)
            {
                bool changed=false;
                if(normal&&importer.textureType!=TextureImporterType.NormalMap){importer.textureType=TextureImporterType.NormalMap;changed=true;}
                if(linear&&importer.sRGBTexture){importer.sRGBTexture=false;changed=true;}
                if(importer.wrapMode!=TextureWrapMode.Repeat){importer.wrapMode=TextureWrapMode.Repeat;changed=true;}
                if(importer.maxTextureSize!=512){importer.maxTextureSize=512;changed=true;}
                if(changed)importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Dictionary<string,Material> KitMaterials()
        {
            var manifest=JsonUtility.FromJson<KitManifest>(File.ReadAllText(Kit+"/materials.json"));var result=new Dictionary<string,Material>();
            foreach(var k in manifest.materials)
                result[k.name]=SaveMaterial(k.name,new Color(k.color.r,k.color.g,k.color.b,k.color.a),k.smoothness,
                    KitTexture(k.baseColorTexture,false,false),KitTexture(k.normalTexture,true,true),k.normalScale,KitTexture(k.maskTexture,false,true));
            return result;
        }
        /// <summary>A tileable PNG drawn in code (soft dots), saved as an asset.</summary>
        static Texture2D SaveDots(string name,Color ground,Color dot,int cells,float radius,float jitter,int seed)
        {
            string path=$"{dir}/Textures/{name}.png";var rng=new System.Random(seed);const int size=256;
            var t=new Texture2D(size,size,TextureFormat.RGBA32,false);var px=new Color[size*size];
            for(int i=0;i<px.Length;i++){float g=(float)rng.NextDouble()*.02f;px[i]=new Color(ground.r-g,ground.g-g,ground.b-g,1);}
            float cell=size/(float)cells;
            for(int cy=0;cy<cells;cy++)for(int cx=0;cx<cells;cx++)
            {
                float ox=(cx+.5f+(float)(rng.NextDouble()-.5)*jitter)*cell,oy=(cy+.5f+(float)(rng.NextDouble()-.5)*jitter)*cell,r=radius*cell;
                for(int y=(int)(oy-r-2);y<=oy+r+2;y++)for(int x=(int)(ox-r-2);x<=ox+r+2;x++)
                {
                    float d=Mathf.Sqrt((x-ox)*(x-ox)+(y-oy)*(y-oy));float a=Mathf.Clamp01(r+.8f-d);if(a<=0)continue;
                    int ix=((x%size)+size)%size,iy=((y%size)+size)%size;px[iy*size+ix]=Color.Lerp(px[iy*size+ix],dot,a);
                }
            }
            t.SetPixels(px);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.wrapMode=TextureWrapMode.Repeat;imp.maxTextureSize=256;imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ---- meshes ----------------------------------------------------------------------------------------------------
        /// <summary>A box with rounded edges: a subdivided cube pulled onto a rounded hull; planar UVs per face in metres/period.</summary>
        static Mesh RoundedBox(Vector3 size,float radius,float period,int n)
        {
            radius=Mathf.Min(radius,Mathf.Min(size.x,Mathf.Min(size.y,size.z))*.5f-.0005f);
            var half=size*.5f;var inner=half-Vector3.one*radius;
            var verts=new List<Vector3>();var norms=new List<Vector3>();var uvs=new List<Vector2>();var tris=new List<int>();
            var faces=new[]{(Vector3.right,Vector3.forward,Vector3.up),(Vector3.left,Vector3.back,Vector3.up),(Vector3.up,Vector3.right,Vector3.forward),
                            (Vector3.down,Vector3.right,Vector3.back),(Vector3.forward,Vector3.left,Vector3.up),(Vector3.back,Vector3.right,Vector3.up)};
            foreach(var (normal,u,v) in faces)
            {
                int start=verts.Count;
                for(int j=0;j<=n;j++)for(int i=0;i<=n;i++)
                {
                    // denser near the edges, where the rounding is
                    float a=Ease(i/(float)n)*2-1,b=Ease(j/(float)n)*2-1;
                    var p=Vector3.Scale(normal,half)+Vector3.Scale(u,half)*a+Vector3.Scale(v,half)*b;
                    var c=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));
                    var d=p-c;var nn=d.sqrMagnitude>1e-12f?d.normalized:normal;
                    verts.Add(c+nn*radius);norms.Add(nn);uvs.Add(new Vector2(Vector3.Dot(p,u),Vector3.Dot(p,v))/period);
                }
                for(int j=0;j<n;j++)for(int i=0;i<n;i++){int k=start+j*(n+1)+i;tris.AddRange(new[]{k,k+n+1,k+1,k+1,k+n+1,k+n+2});}
            }
            var m=new Mesh{name="Rounded box"};m.SetVertices(verts);m.SetNormals(norms);m.SetUVs(0,uvs);m.SetTriangles(tris,0);m.RecalculateBounds();m.RecalculateTangents();return m;
        }
        static float Ease(float t){float s=t*2-1;return .5f+.5f*Mathf.Sign(s)*(1-Mathf.Pow(1-Mathf.Abs(s),1.6f));}
        static int meshSerial;
        static Mesh SaveMesh(Mesh m,string name){string path=$"{dir}/Meshes/{name}-{meshSerial++:000}.asset";AssetDatabase.CreateAsset(m,path);return m;}
        static Transform Rounded(Transform parent,string name,Vector3 centre,Vector3 size,float radius,Material mat,float period=.08f,int n=6)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.transform.localPosition=centre;
            go.GetComponent<MeshFilter>().sharedMesh=SaveMesh(RoundedBox(size,radius,period,n),name.Replace(' ','-'));
            var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.On;return go.transform;
        }
        static Transform Place(string fbx,Transform parent,Vector3 local,Vector3 scale,Dictionary<string,Material> kit,Quaternion? rotation=null)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>($"{Kit}/{fbx}.fbx");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=scale;go.transform.localRotation=rotation??Quaternion.identity;
            foreach(var r in go.GetComponentsInChildren<Renderer>())
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)if(mats[i]!=null&&kit.TryGetValue(mats[i].name,out var m))mats[i]=m;
                r.sharedMaterials=mats;r.shadowCastingMode=ShadowCastingMode.On;
            }
            foreach(var col in go.GetComponentsInChildren<Collider>())Object.DestroyImmediate(col);
            return go.transform;
        }
        /// <summary>Box UVs in metres/period on a copy of a face mesh, saved, so a textured material shows on it.</summary>
        static void BoxUV(Renderer r,Transform root,float period)
        {
            var mf=r.GetComponent<MeshFilter>();if(mf==null||mf.sharedMesh==null)return;
            var src=mf.sharedMesh;var mesh=Object.Instantiate(src);var v=src.vertices;var n=src.normals;var uv=new Vector2[v.Length];
            for(int i=0;i<v.Length;i++)
            {
                var p=root.InverseTransformPoint(r.transform.TransformPoint(v[i]));var d=n.Length==v.Length?root.InverseTransformDirection(r.transform.TransformDirection(n[i])):Vector3.up;
                var a=new Vector3(Mathf.Abs(d.x),Mathf.Abs(d.y),Mathf.Abs(d.z));
                uv[i]=(a.y>=a.x&&a.y>=a.z?new Vector2(p.x,p.z):a.x>=a.z?new Vector2(p.z,p.y):new Vector2(p.x,p.y))/period;
            }
            mesh.uv=uv;mf.sharedMesh=SaveMesh(mesh,"uv-"+r.name.Replace(' ','-'));
        }


        /// <summary>Adds (once) a copy of the shipped renderer with an SSAO feature to the pipeline asset; returns its index.</summary>
        static int BakeryRenderer()
        {
            var asset=(UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
            var so=new SerializedObject(asset);var list=so.FindProperty("m_RendererDataList");
            string path=Out+"/Bakery renderer.asset";var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if(data==null)
            {
                var shipped=(UniversalRendererData)list.GetArrayElementAtIndex(0).objectReferenceValue;
                data=Object.Instantiate(shipped);data.name="Bakery renderer";AssetDatabase.CreateAsset(data,path);
                var ssao=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();ssao.name="Bakery SSAO";
                AssetDatabase.AddObjectToAsset(ssao,data);data.rendererFeatures.Add(ssao);
                // the serialized feature map must list the sub-asset, as the inspector does when a feature is added
                var map=new SerializedObject(data);var features=map.FindProperty("m_RendererFeatureMap");
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ssao,out string _,out long id);
                features.arraySize=data.rendererFeatures.Count;features.GetArrayElementAtIndex(features.arraySize-1).longValue=id;map.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
            }
            // settings every run, so a change here reaches the existing renderer asset
            foreach(var feature in data.rendererFeatures.OfType<ScreenSpaceAmbientOcclusion>())
            {
                var settings=typeof(ScreenSpaceAmbientOcclusion).GetField("m_Settings",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                var value=settings?.GetValue(feature);if(value==null)continue;
                void Set(string f,object v){var fi=value.GetType().GetField(f,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance);fi?.SetValue(value,v);}
                Set("Intensity",.9f);Set("Radius",.035f);Set("DirectLightingStrength",.3f);settings.SetValue(feature,value);EditorUtility.SetDirty(feature);
            }
            for(int i=0;i<list.arraySize;i++)if(list.GetArrayElementAtIndex(i).objectReferenceValue==data)return i;
            list.arraySize++;list.GetArrayElementAtIndex(list.arraySize-1).objectReferenceValue=data;so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);AssetDatabase.SaveAssets();return list.arraySize-1;
        }
        // ---- the dress -------------------------------------------------------------------------------------------------
        public static void Dress(string key)
        {
            dir=$"{Out}/{key}";meshSerial=0;
            if(AssetDatabase.IsValidFolder(dir))AssetDatabase.DeleteAsset(dir);
            foreach(var sub in new[]{"","/Materials","/Meshes","/Textures"})Directory.CreateDirectory(dir+sub);
            AssetDatabase.Refresh();
            var scene=EditorSceneManager.OpenScene(VenomCampaignBuilder.SpatialContentPath(key));
            var game=Object.FindFirstObjectByType<VenomCampaign>();var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
            var old=root.Find("Bakery dress");if(old!=null)Object.DestroyImmediate(old.gameObject);
            var dress=new GameObject("Bakery dress").transform;dress.SetParent(root,false);

            var kit=KitMaterials();
            // the porcelain read near white and warm in the first frames (Codex review): darker and less glossy here
            kit["Porcelain"].SetColor("_BaseColor",new Color(.88f,.84f,.79f));kit["Porcelain"].SetFloat("_Smoothness",.55f);
            var jelly=SaveMaterial("Grape jelly",new Color(.62f,.47f,.90f),.9f);
            var tiles=SaveMaterial("Vanilla tiles",new Color(1f,.93f,.80f),.42f,SaveDots("vanilla-dots",Color.white,new Color(.95f,.91f,.86f),5,.16f,.9f,7));
            var sponge=kit.TryGetValue("Wafer",out var wafer)?wafer:tiles;var cream=kit["Cream"];var biscuit=kit["Biscuit"];
            var candyBlue=SaveMaterial("Candy blue",new Color(.36f,.62f,.95f),.85f);var mint=SaveMaterial("Mint sugar",new Color(.62f,.88f,.78f),.6f);
            var white=SaveMaterial("Sugar white",new Color(.98f,.97f,.95f),.55f);

            // 1. Blocks: each fixed or moving block (faces sharing a name under one parent) gets a rounded body over its real
            //    faces, coloured by what the faces do: slick -> grape jelly, grippy top -> a cake layer with a cream cap.
            var groups=new Dictionary<(Transform,string),List<VenomSurfacePatch>>();
            foreach(var p in root.GetComponentsInChildren<VenomSurfacePatch>(true))
            {
                var r=p.GetComponent<Renderer>();if(r==null||!p.gameObject.activeInHierarchy||p.ExteriorGlass||p.SphereRadius>0||p.Curved!=null)continue;
                if(p.name=="Laboratory floor"||p.name.StartsWith("Bakery"))continue;
                var k=(p.transform.parent,p.name);if(!groups.TryGetValue(k,out var list))groups[k]=list=new List<VenomSurfacePatch>();list.Add(p);
            }
            foreach(var kv in groups)
            {
                var parent=kv.Key.Item1;var faces=kv.Value;if(faces.Count<4)continue;
                bool first=true;Bounds b=default;
                foreach(var f in faces)foreach(float x in new[]{-.5f,.5f})foreach(float y in new[]{-.5f,.5f})
                {var v=parent.InverseTransformPoint(f.transform.TransformPoint(new Vector3(f.Size.x*x,f.Size.y*y,0)));if(first){b=new Bounds(v,Vector3.zero);first=false;}else b.Encapsulate(v);}
                if(Mathf.Min(b.size.x,Mathf.Min(b.size.y,b.size.z))<.004f)continue;
                var top=faces.FirstOrDefault(f=>parent.InverseTransformDirection(f.Normal).y>.9f);
                bool slickSides=faces.Where(f=>Mathf.Abs(parent.InverseTransformDirection(f.Normal).y)<.5f).All(f=>f.Slippery);
                bool grippyTop=top!=null&&!top.Slippery;
                foreach(var f in faces){var r=f.GetComponent<Renderer>();if(r!=null)r.enabled=false;}
                string name="Bakery "+kv.Key.Item2;float rad=Mathf.Min(.012f,b.size.y*.35f);
                if(!grippyTop){Rounded(parent,name+" jelly",b.center,b.size,rad,jelly);continue;}
                // Slick sides stay jelly up to a cream cap: a sponge-looking side would read as climbable (purple = slippery).
                if(slickSides)Rounded(parent,name+" jelly",b.center,b.size,rad,jelly);
                else
                {
                    float layer=Mathf.Min(.022f,b.size.y*.45f);
                    Rounded(parent,name+" sponge",b.center,b.size,rad,sponge);
                }
                Rounded(parent,name+" cream",new Vector3(b.center.x,b.max.y,b.center.z),new Vector3(b.size.x+.006f,.012f,b.size.z+.006f),.0058f,cream,.08f,6);
            }

            // 2. Floor tiles over the real floor, Codex's plate under them with its lip on the rim, the rim and backrest faces
            //    hidden (the plate lip and a jelly backrest show them).
            foreach(var p in root.GetComponentsInChildren<VenomSurfacePatch>(true))
            {
                var r=p.GetComponent<Renderer>();if(r==null)continue;
                if(p.name=="Laboratory floor"||p.name.StartsWith("Bakery rim"))r.enabled=false;
                if(p.name.StartsWith("Bakery backrest"))
                {
                    r.enabled=false;var t=p.transform;var c=root.InverseTransformPoint(t.position);
                    Rounded(dress,"Bakery backrest jelly",new Vector3(c.x,c.y,.316f),new Vector3(p.Size.x,p.Size.y,.032f),.012f,jelly);
                }
            }
            // six by four large tiles, 1.5 mm seams (Codex review: small tiles with dark seams read as an ice tray)
            const int nx=6,nz=4;float tx=.80f/nx,tz=.60f/nz;
            var floorTiles=new List<CombineInstance>();var tileMesh=RoundedBox(new Vector3(tx-.0015f,.014f,tz-.0015f),.004f,.10f,3);
            for(int ix=0;ix<nx;ix++)for(int iz=0;iz<nz;iz++)
                if(!((ix==0||ix==nx-1)&&(iz==0||iz==nz-1)))   // the plate's rounded corners
                floorTiles.Add(new CombineInstance{mesh=tileMesh,transform=Matrix4x4.Translate(new Vector3(-.40f+tx*(ix+.5f),-.307f,-.30f+tz*(iz+.5f)))});
            var floor=new GameObject("Bakery floor tiles",typeof(MeshFilter),typeof(MeshRenderer));floor.transform.SetParent(dress,false);
            var combined=new Mesh{name="Floor tiles",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};combined.CombineMeshes(floorTiles.ToArray(),true,true);
            floor.GetComponent<MeshFilter>().sharedMesh=SaveMesh(combined,"floor-tiles");floor.GetComponent<MeshRenderer>().sharedMaterial=tiles;
            // plate lathe: inner flat to .411 (x) /.327 (z), lip crest .468/.372: scaled so the lip rises at the rim (.40/.30)
            Place("Plate",dress,new Vector3(0,-.30f-.008f*.9f-.003f,0),new Vector3(.40f/.44f,.9f,.30f/.35f),kit);

            // 3. Props on their real owners: pad P, handle A, the gear lamp, the cherry; gears in biscuit.
            foreach(var sensor in root.GetComponentsInChildren<COgheTissueSensor>(true))
            {
                foreach(var r in sensor.GetComponentsInChildren<Renderer>(true))if(r.bounds.size.y<.02f)r.enabled=false;
                var c=root.InverseTransformPoint(sensor.transform.position);
                Place("PressurePad",dress,new Vector3(c.x,-.30f,c.z),Vector3.one,kit);
            }
            foreach(var task in root.GetComponentsInChildren<COgheTapRail>(true))
            {
                if(task.Handle==null)continue;
                foreach(var r in task.Handle.GetComponentsInChildren<Renderer>(true))r.enabled=false;
                var heading=Quaternion.Euler(0,game.Definition.CameraEuler.y,0);
                var candy=Place("CandyHandle",task.Handle,Vector3.zero,Vector3.one*1.6f,kit);
                candy.rotation=root.rotation*heading;candy.position=task.Handle.position+root.up*.004f;
                var cb=candy.GetComponentInChildren<Renderer>().bounds;Debug.Log($"BAKERY candy {task.Label} at {root.InverseTransformPoint(cb.center):F3} size {cb.size:F3}");
                foreach(var r in candy.GetComponentsInChildren<Renderer>())
                {var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i]==kit["Strawberry"])mats[i]=candyBlue;r.sharedMaterials=mats;}   // A's circuit colour is blue
            }
            foreach(var train in root.GetComponentsInChildren<COgheGearTrain>(true))
            {
                if(train.MeshLamp!=null)
                {
                    var lamp=AssetDatabase.LoadAllAssetsAtPath($"{Kit}/GummyLamp.fbx").OfType<Mesh>().FirstOrDefault();
                    var mf=train.MeshLamp.GetComponent<MeshFilter>();if(lamp!=null&&mf!=null){mf.sharedMesh=lamp;train.MeshLamp.transform.localScale=Vector3.one;}
                    train.MeshLampOff=SaveMaterial("Gummy off",new Color(.80f,.55f,.62f),.75f);
                    var on=SaveMaterial("Gummy on",new Color(.45f,.95f,.55f),.85f);on.EnableKeyword("_EMISSION");on.SetColor("_EmissionColor",new Color(.25f,.7f,.3f));train.MeshLampOn=on;
                    train.MeshLamp.sharedMaterial=train.MeshLampOff;EditorUtility.SetDirty(train);
                }
            }
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if(r.sharedMaterial==null)continue;string m=r.sharedMaterial.name;
                if(m.StartsWith("Amber resin")||r.name.Contains("involute gear")){BoxUV(r,root,.06f);r.sharedMaterial=biscuit;}
                else if(m.StartsWith("Spatial satin guides"))r.sharedMaterial=mint;
                else if(m.StartsWith("Spatial pearl casing"))r.sharedMaterial=white;
            }
            var goal=root.GetComponentInChildren<COgheCherryGoal>(true);
            if(goal!=null&&goal.Visual!=null)
            {
                foreach(Transform child in goal.Visual.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
                Place("Cherry",goal.Visual,Vector3.zero,Vector3.one*2f,kit);
            }
            // the old exit ring lines on the platform go: the cherry is the mark
            foreach(var line in owner.Outlet.GetComponentsInChildren<Renderer>(true))Object.DestroyImmediate(line.gameObject);
            // glass frame and contact shadow of the old chamber
            foreach(var t in game.GetComponentsInChildren<Transform>(true))if(t.name=="Glass preview frame")t.gameObject.SetActive(false);

            // 4. Light: a warm key with soft shadows, a three-colour ambient, a peach backdrop, gentle grading.
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l.type==LightType.Directional){l.color=new Color(1f,.94f,.86f);l.intensity=.9f;l.shadows=LightShadows.Soft;l.shadowStrength=.8f;l.transform.rotation=Quaternion.Euler(58,-60,0);}
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.72f,.66f,.68f);
            RenderSettings.ambientEquatorColor=new Color(.62f,.52f,.50f);RenderSettings.ambientGroundColor=new Color(.42f,.36f,.38f);
            var look=game.gameObject.GetComponent<COgheBakeryPresentation>()??game.gameObject.AddComponent<COgheBakeryPresentation>();
            look.Backdrop=new Color(.97f,.78f,.69f);
            var profilePath=$"{dir}/Bakery volume.asset";var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,profilePath);
            var ca=profile.Add<ColorAdjustments>(true);ca.contrast.Override(10);ca.saturation.Override(12);
            var tm=profile.Add<Tonemapping>(true);tm.mode.Override(TonemappingMode.Neutral);
            var vg=profile.Add<Vignette>(true);vg.intensity.Override(.16f);vg.smoothness.Override(.5f);
            foreach(var comp in profile.components)AssetDatabase.AddObjectToAsset(comp,profile);
            var volume=new GameObject("Bakery volume",typeof(Volume)).GetComponent<Volume>();volume.transform.SetParent(dress,false);volume.isGlobal=true;volume.sharedProfile=profile;
            var cam=owner.View;var camData=cam.GetComponent<UniversalAdditionalCameraData>()??cam.gameObject.AddComponent<UniversalAdditionalCameraData>();camData.renderPostProcessing=true;
            // Soft contact darkening (SSAO) on a renderer of its own, listed after the shipped one: only this camera uses it.
            try{int index=BakeryRenderer();if(index>0)camData.SetRenderer(index);Debug.Log("BAKERY renderer "+index);}
            catch(Exception e){Debug.LogWarning("BAKERY renderer not added: "+e.Message);}

            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log($"BAKERY DRESSED {key}: {groups.Count} face groups, {meshSerial} meshes");
        }
    }
}
