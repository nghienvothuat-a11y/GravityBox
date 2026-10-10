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
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-bakery-variant")variant=args[i+1];
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

        /// <summary>A flat slab shaped |x/a|^4 + |z/b|^4 = 1 (the plate's inner lip), top at y, a rounded skirt of depth d; UVs in metres.</summary>
        static Mesh Superellipse(float a,float b,float y,float d,int n)
        {
            var verts=new List<Vector3>{new Vector3(0,y,0)};var norms=new List<Vector3>{Vector3.up};var uvs=new List<Vector2>{Vector2.zero};var tris=new List<int>();
            for(int i=0;i<n;i++)
            {
                float t=i*Mathf.PI*2/n,c=Mathf.Cos(t),sn=Mathf.Sin(t);
                var e=new Vector3(Mathf.Sign(c)*Mathf.Sqrt(Mathf.Abs(c))*a,y,Mathf.Sign(sn)*Mathf.Sqrt(Mathf.Abs(sn))*b);
                var outward=new Vector3(e.x,0,e.z).normalized;
                verts.Add(e);norms.Add(Vector3.up);uvs.Add(new Vector2(e.x,e.z));                                  // top rim
                verts.Add(e+outward*.002f+Vector3.down*d*.4f);norms.Add((Vector3.up+outward).normalized);uvs.Add(new Vector2(e.x,e.z));
                verts.Add(e+outward*.002f+Vector3.down*d);norms.Add(outward);uvs.Add(new Vector2(e.x,e.z));
            }
            for(int i=0;i<n;i++)
            {
                int j=(i+1)%n,a0=1+i*3,b0=1+j*3;
                tris.AddRange(new[]{0,b0,a0});
                for(int k=0;k<2;k++)tris.AddRange(new[]{a0+k,b0+k,a0+k+1,b0+k,b0+k+1,a0+k+1});
            }
            var m=new Mesh{name="Superellipse floor"};m.SetVertices(verts);m.SetNormals(norms);m.SetUVs(0,uvs);m.SetTriangles(tris,0);m.RecalculateBounds();m.RecalculateTangents();return m;
        }
        /// <summary>One tile per texture repeat: a soft-dotted vanilla square with a narrow, soft seam on its border.</summary>
        static Texture2D SaveTileTexture(string name,Color tile,Color seam,int seed)
        {
            string path=$"{dir}/Textures/{name}.png";var rng=new System.Random(seed);const int size=256;
            var t=new Texture2D(size,size,TextureFormat.RGBA32,false);var px=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float g=(float)rng.NextDouble()*.018f;var c=new Color(tile.r-g,tile.g-g,tile.b-g,1);
                float edge=Mathf.Min(Mathf.Min(x,size-1-x),Mathf.Min(y,size-1-y));   // pixels to the border
                c=Color.Lerp(seam,c,Mathf.Clamp01((edge-.5f)/2.2f));
                px[y*size+x]=c;
            }
            for(int k=0;k<40;k++)
            {
                float ox=(float)rng.NextDouble()*size,oy=(float)rng.NextDouble()*size,r=2+(float)rng.NextDouble()*3;
                for(int y=(int)(oy-r-1);y<=oy+r+1;y++)for(int x=(int)(ox-r-1);x<=ox+r+1;x++)
                {if(x<4||y<4||x>size-5||y>size-5)continue;float d=Mathf.Sqrt((x-ox)*(x-ox)+(y-oy)*(y-oy));float w=Mathf.Clamp01(r-d)*.25f;px[y*size+x]=Color.Lerp(px[y*size+x],tile*.93f,w);}
            }
            t.SetPixels(px);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.wrapMode=TextureWrapMode.Repeat;imp.maxTextureSize=256;imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material glazeMat,spongeMat,creamMat;static Dictionary<string,Material> kitRef;static Transform dressRoot;
        static void Combined(Transform parent,string name,List<CombineInstance> parts,Material mat)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);
            var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.CombineMeshes(parts.ToArray(),true,true);
            go.GetComponent<MeshFilter>().sharedMesh=SaveMesh(m,name.Replace(' ','-'));go.GetComponent<MeshRenderer>().sharedMaterial=mat;
        }
        /// <summary>A row of drips along an edge (CreamDrip, 10 cm segments fitted to the length), hanging from y, facing out.</summary>
        static void Drips(Transform parent,Vector3 from,Vector3 to,float y,Vector3 outward,Material mat,string fbx="GlazeDrip")
        {
            // GlazeDrip (Codex): 10 cm, pivot at the centre of its top edge (Y=0), drips to -.031, X along the edge. Whole
            // segments at true scale, centred on the edge (no long non-uniform stretch); the joins tuck into the coating.
            float length=Vector3.Distance(from,to);int n=Mathf.FloorToInt(length/.1f);if(n<1)return;
            var along=(to-from).normalized;var rot=Quaternion.LookRotation(Vector3.Cross(along,Vector3.up),Vector3.up);
            if(Vector3.Dot(rot*Vector3.forward,outward)<0)rot=Quaternion.LookRotation(-Vector3.Cross(along,Vector3.up),Vector3.up);
            var mid=(from+to)*.5f;
            for(int i=0;i<n;i++)
            {
                var centre=mid+along*((i-(n-1)*.5f)*.1f);
                var t=Place(fbx,parent,Vector3.zero,Vector3.one,kitRef,Quaternion.LookRotation(along,Vector3.up)*Quaternion.Euler(0,-90,0));
                t.localRotation=Quaternion.LookRotation(Vector3.Cross(Vector3.up,along)*(Vector3.Dot(Vector3.Cross(Vector3.up,along),outward)>=0?1:-1),Vector3.up);
                t.localPosition=new Vector3(centre.x,y,centre.z)+outward*.0036f;
                foreach(var r in t.GetComponentsInChildren<Renderer>())r.sharedMaterial=mat;
            }
        }
        /// <summary>A gameplay block as a piece of cake over its real faces. Slick sides: the whole body in glaze (it covers the
        /// slick faces completely), a thicker glaze lip with drips on top. Grippy top: a soft vanilla cream cap. All grippy: sponge
        /// body under the cream.</summary>
        static void CakeBlock(Transform parent,string name,Bounds b,bool grippyTop,bool slickSides)
        {
            if(variant=="tripo"){TripoBlock(parent,grippyTop?"glaze_block":"glaze_bar",b.center,b.size+new Vector3(.004f,.002f,.004f));return;}
            // reduced Tripo where the proportions suit (a glazed cake with a cream crown); long or fully slick pieces stay code
            if(variant=="tripomobile"&&grippyTop&&slickSides){MobileBlock(parent,"GlazeBlock",.0511f,new Vector3(b.center.x,b.max.y,b.center.z),new Vector2(b.size.x+.004f,b.size.z+.004f),b.size.y);return;}
            float rad=Mathf.Min(.016f,b.size.y*.4f,Mathf.Min(b.size.x,b.size.z)*.3f);const float cap=.011f;
            // with a cream cap the body stops under it (no coplanar faces), the cap's thickness below the contact height
            var bodyCentre=grippyTop?b.center-Vector3.up*(cap*.5f-.0015f):b.center;var bodySize=grippyTop?b.size-Vector3.up*(cap-.003f):b.size;
            Rounded(parent,name+(slickSides?" glaze":" sponge"),bodyCentre,bodySize,rad,slickSides?glazeMat:spongeMat,.12f,7);
            if(grippyTop)
                Rounded(parent,name+" cream",new Vector3(b.center.x,b.max.y-cap*.5f,b.center.z),new Vector3(b.size.x+.004f,cap,b.size.z+.004f),.0052f,creamMat,.08f,6);
            else if(slickSides)
                Rounded(parent,name+" glaze lip",new Vector3(b.center.x,b.max.y-.004f,b.center.z),new Vector3(b.size.x+.006f,.012f,b.size.z+.006f),.0058f,glazeMat,.08f,6);
            if(!slickSides||b.size.y<.02f)return;
            // glaze drips under the lip, on the sides the camera sees (front and right), as trim on the glaze
            float y=b.max.y-.004f;
            Drips(parent,new Vector3(b.min.x+.012f,0,b.min.z),new Vector3(b.max.x-.012f,0,b.min.z),y,Vector3.back,glazeMat);
            Drips(parent,new Vector3(b.max.x,0,b.min.z+.012f),new Vector3(b.max.x,0,b.max.z-.012f),y,Vector3.right,glazeMat);
        }
        /// <summary>A perimeter cake bar outside the play floor: sponge body, its inner face (the slick rim) fully glazed, a
        /// glaze top with drips toward the play area.</summary>
        static void GlazedBar(Transform parent,Vector3 centre,Vector3 size,Vector3 inward,float top,float bottom)
        {
            if(variant=="tripo"){TripoBlock(parent,"glaze_bar",new Vector3(centre.x,(top+bottom)*.5f,centre.z),new Vector3(size.x,top-bottom,size.z));return;}
            var c=new Vector3(centre.x,(top+bottom)*.5f,centre.z);var s=new Vector3(size.x,top-bottom,size.z);
            Rounded(parent,"Bakery bar sponge",c,s,.016f,spongeMat,.12f,7);
            // piped cream along the bar's outer top edge (outside every route), stopped short of the rounded corners
            var outward=-inward;var edgeLen=inward.x!=0?s.z:s.x;var along=inward.x!=0?Vector3.forward:Vector3.right;
            var edge=new Vector3(c.x,top+.008f,c.z)+outward*(Vector3.Dot(new Vector3(Mathf.Abs(inward.x),0,Mathf.Abs(inward.z)),s)*.5f-.008f);
            int pieces=Mathf.FloorToInt((edgeLen-.05f)/.1f);
            for(int i=0;i<pieces;i++)
            {var t=Place("PipedCream",parent,Vector3.zero,Vector3.one,kitRef,Quaternion.LookRotation(Vector3.Cross(along,Vector3.up),Vector3.up));t.localPosition=edge+along*((i-(pieces-1)*.5f)*.1f);}
            // the inner face: a glaze slab from the floor tiles up over the top
            var face=Vector3.Scale(new Vector3(Mathf.Abs(inward.x),0,Mathf.Abs(inward.z)),s);
            var glazeSize=new Vector3(inward.x!=0?.012f:s.x+.004f,top+.30f+.006f,inward.z!=0?.012f:s.z+.004f);
            var glazeCentre=new Vector3(c.x,(top+.006f-.30f)*.5f-.0f,c.z)+inward*(Vector3.Dot(face,Vector3.one)*.5f-.005f);
            glazeCentre.y=(top+.006f+(-.30f))*.5f;
            Rounded(parent,"Bakery bar glaze",glazeCentre,glazeSize,.0055f,glazeMat,.08f,6);
            Rounded(parent,"Bakery bar glaze top",new Vector3(c.x,top+.002f,c.z),new Vector3(s.x+.004f,.012f,s.z+.004f),.0058f,glazeMat,.08f,6);
        }

        // Tripo comparison (Mrk, 10/10/2026: "tạo model bằng Tripo3D rồi xếp vào màn"): -coghe-bakery-variant tripo puts the raw
        // Tripo models (Art/BakeryTripo, converted in Blender, textures as URP maps) over the same faces, stretched to each size.
        static string variant="code";
        static readonly Dictionary<string,Material> tripoMats=new Dictionary<string,Material>();
        static Material TripoMaterial(string model)
        {
            if(tripoMats.TryGetValue(model,out var m))return m;string d=$"Assets/_Game/Venom/Art/BakeryTripo/{model}/Tripo_{model}";
            var normal=AssetImporter.GetAtPath(d+"_normal.png") as TextureImporter;if(normal!=null&&normal.textureType!=TextureImporterType.NormalMap){normal.textureType=TextureImporterType.NormalMap;normal.SaveAndReimport();}
            var mask=AssetImporter.GetAtPath(d+"_mask.png") as TextureImporter;if(mask!=null&&mask.sRGBTexture){mask.sRGBTexture=false;mask.SaveAndReimport();}
            m=SaveMaterial("Tripo "+model,Color.white,.5f,AssetDatabase.LoadAssetAtPath<Texture2D>(d+"_basecolor.png"),AssetDatabase.LoadAssetAtPath<Texture2D>(d+"_normal.png"),1,AssetDatabase.LoadAssetAtPath<Texture2D>(d+"_mask.png"));
            tripoMats[model]=m;return m;
        }
        static Transform TripoBlock(Transform parent,string model,Vector3 centre,Vector3 size)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Game/Venom/Art/BakeryTripo/{model}/Tripo_{model}.fbx");
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            go.name="Tripo "+model;go.transform.SetParent(parent,false);go.transform.localRotation=Quaternion.identity;
            var mf=go.GetComponentInChildren<MeshFilter>();var mb=mf.sharedMesh.bounds;var child=mf.transform;
            // the child may carry the FBX's own transform: measure in the root's frame
            var lossy=Vector3.Scale(child.localScale,mb.size);
            go.transform.localScale=new Vector3(size.x/Mathf.Max(lossy.x,1e-5f),size.y/Mathf.Max(lossy.y,1e-5f),size.z/Mathf.Max(lossy.z,1e-5f));
            go.transform.localPosition=new Vector3(centre.x,centre.y-size.y*.5f,centre.z);
            foreach(var r in go.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=TripoMaterial(model);r.sharedMaterials=mats;r.shadowCastingMode=ShadowCastingMode.On;}
            foreach(var col in go.GetComponentsInChildren<Collider>())Object.DestroyImmediate(col);
            return go.transform;
        }

        // Codex's reduced Tripo pieces (Art/Bakery/TripoMobile): placed by their CONTACT height, not their bounds (the cream
        // block's toppings rise 1.7 cm over its cream; measured in Blender: cream .0638 of .0812, glaze crown .0511 of .0511).
        static readonly Dictionary<string,Material> mobileMats=new Dictionary<string,Material>();
        static Transform MobileBlock(Transform parent,string model,float contactInModel,Vector3 centreTop,Vector2 footprint,float height)
        {
            const string d="Assets/_Game/Venom/Art/Bakery/TripoMobile/";
            if(!mobileMats.TryGetValue(model,out var mat))
            {
                var n=AssetImporter.GetAtPath(d+model+"_normal.png") as TextureImporter;if(n!=null&&n.textureType!=TextureImporterType.NormalMap){n.textureType=TextureImporterType.NormalMap;n.SaveAndReimport();}
                var k=AssetImporter.GetAtPath(d+model+"_Mask.png") as TextureImporter;if(k!=null&&k.sRGBTexture){k.sRGBTexture=false;k.SaveAndReimport();}
                mat=SaveMaterial("TripoMobile "+model,Color.white,.5f,AssetDatabase.LoadAssetAtPath<Texture2D>(d+model+"_basecolor.png"),AssetDatabase.LoadAssetAtPath<Texture2D>(d+model+"_normal.png"),1,AssetDatabase.LoadAssetAtPath<Texture2D>(d+model+"_Mask.png"));
                mobileMats[model]=mat;
            }
            // keep the FBX's own root transform (its axis conversion); fit a container by the model's measured bounds
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(d+model+".fbx");
            var box=new GameObject("TripoMobile "+model).transform;box.SetParent(parent,false);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            go.transform.SetParent(box,false);
            bool first=true;Bounds mb=default;
            foreach(var r in go.GetComponentsInChildren<Renderer>())
            {
                var lb=r.GetComponent<MeshFilter>().sharedMesh.bounds;
                for(int i=0;i<8;i++)
                {
                    var corner=lb.center+Vector3.Scale(lb.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    var v=box.InverseTransformPoint(r.transform.TransformPoint(corner));if(first){mb=new Bounds(v,Vector3.zero);first=false;}else mb.Encapsulate(v);
                }
            }
            float contactFraction=contactInModel/Mathf.Max(contactHeightOf[model],1e-5f);   // contact height as a share of the full height
            var scale=new Vector3(footprint.x/mb.size.x,height/(mb.size.y*contactFraction),footprint.y/mb.size.z);
            box.localScale=scale;
            box.localPosition=new Vector3(centreTop.x,centreTop.y-height,centreTop.z)-Vector3.Scale(new Vector3(mb.center.x,mb.min.y,mb.center.z),scale);
            foreach(var r in go.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=mat;r.sharedMaterials=mats;r.shadowCastingMode=ShadowCastingMode.On;}
            foreach(var col in go.GetComponentsInChildren<Collider>())Object.DestroyImmediate(col);
            Debug.Log($"BAKERY mobile {model}: model bounds {mb.size:F4}, scale {scale:F3}");
            return box;
        }
        // full heights measured in Blender alongside the contact heights
        static readonly Dictionary<string,float> contactHeightOf=new Dictionary<string,float>{{"CreamBlock",.0812f},{"GlazeBlock",.0511f}};
        // ---- the dress -------------------------------------------------------------------------------------------------
        public static void Dress(string key)
        {
            dir=$"{Out}/{key}";meshSerial=0;
            var scene=EditorSceneManager.OpenScene(VenomCampaignBuilder.SpatialContentPath(key));
            var game=Object.FindFirstObjectByType<VenomCampaign>();var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
            // Not idempotent by design (Codex review): rounded bodies and the candy live under their real owners, and the
            // generated assets are rewritten. Checked before anything changes: dress a freshly generated scene only.
            if(root.Find("Bakery dress")!=null)throw new InvalidOperationException($"{key} is already dressed: regenerate it first (GenerateChapterTwoLevels -coghe-plus-levels {key})");
            if(AssetDatabase.IsValidFolder(dir))AssetDatabase.DeleteAsset(dir);
            foreach(var sub in new[]{"","/Materials","/Meshes","/Textures"})Directory.CreateDirectory(dir+sub);
            AssetDatabase.Refresh();

            var dress=new GameObject("Bakery dress").transform;dress.SetParent(root,false);

            var kit=KitMaterials();
            // the porcelain read near white and warm in the first frames (Codex review): darker and less glossy here
            kit["Porcelain"].SetColor("_BaseColor",new Color(.88f,.84f,.79f));kit["Porcelain"].SetFloat("_Smoothness",.55f);
            var jelly=kit.TryGetValue("Grape jelly",out var grape)?grape:SaveMaterial("Grape jelly",new Color(.58f,.43f,.78f),.76f);
            var tiles=SaveMaterial("Vanilla tiles",new Color(1f,.93f,.80f),.42f,SaveDots("vanilla-dots",Color.white,new Color(.95f,.91f,.86f),5,.16f,.9f,7));
            var sponge=kit.TryGetValue("Sponge",out var spongeKit)?spongeKit:kit["Wafer"];var cream=kit["Cream"];var biscuit=kit["Biscuit"];
            var vanilla=kit.TryGetValue("Vanilla cream",out var vk)?vk:SaveMaterial("Vanilla cream",new Color(1f,.96f,.88f),.5f);
            glazeMat=kit.TryGetValue("Glaze lilac",out var gk)?gk:jelly;spongeMat=sponge;creamMat=vanilla;kitRef=kit;dressRoot=dress;
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
                CakeBlock(parent,"Bakery "+kv.Key.Item2,b,grippyTop,slickSides);
            }

            // 2. The floor is a cake: 6 x 4 sponge tiles 4 cm thick, grown DOWN from the contact height (cream tops at -.30),
            //    6 mm seams showing the crumb. The rim and backrest faces (slick) are wrapped by glazed cake bars whose inner faces
            //    are fully glazed; sponge shows only on their outer faces. The plate sits lower and wider under it all.
            foreach(var p in root.GetComponentsInChildren<VenomSurfacePatch>(true))
            {var r=p.GetComponent<Renderer>();if(r!=null&&(p.name=="Laboratory floor"||p.name.StartsWith("Bakery rim")||p.name.StartsWith("Bakery backrest")))r.enabled=false;}
            {
                const int nx=6,nz=4;float tx=.80f/nx,tz=.60f/nz,gap=.006f;
                var bodies=new List<CombineInstance>();var caps=new List<CombineInstance>();
                // one continuous sponge slab under the contact height; the portions show only as shallow seams in the cream
                bodies.Add(new CombineInstance{mesh=RoundedBox(new Vector3(.80f,.034f,.60f),.010f,.12f,6),transform=Matrix4x4.Translate(new Vector3(0,-.34f+.017f,0))});
                var cap=RoundedBox(new Vector3(tx-.004f,.009f,tz-.004f),.0042f,.12f,4);
                for(int ix=0;ix<nx;ix++)for(int iz=0;iz<nz;iz++)
                    caps.Add(new CombineInstance{mesh=cap,transform=Matrix4x4.Translate(new Vector3(-.40f+tx*(ix+.5f),-.30f-.0045f,-.30f+tz*(iz+.5f)))});
                if(variant=="tripo")
                {for(int ix=0;ix<nx;ix++)for(int iz=0;iz<nz;iz++)TripoBlock(dress,"cream_block",new Vector3(-.40f+tx*(ix+.5f),-.32f,-.30f+tz*(iz+.5f)),new Vector3(tx,.04f,tz));}
                else if(variant=="tripomobile")
                {for(int ix=0;ix<nx;ix++)for(int iz=0;iz<nz;iz++)MobileBlock(dress,"CreamBlock",.0638f,new Vector3(-.40f+tx*(ix+.5f),-.30f,-.30f+tz*(iz+.5f)),new Vector2(tx-.002f,tz-.002f),.04f);}
                else{Combined(dress,"Bakery floor sponge",bodies,sponge);Combined(dress,"Bakery floor cream",caps,vanilla);}
                // the bars: front, back, left, right; the backrest raises the back bar where the exit platform meets it
                const float w=.06f,top=-.24f,bottom=-.345f;
                GlazedBar(dress,new Vector3(0,0,-.30f-w*.5f),new Vector3(.80f+2*w,0,w),Vector3.forward,top,bottom);
                GlazedBar(dress,new Vector3(0,0,.30f+w*.5f),new Vector3(.80f+2*w,0,w),Vector3.back,top,bottom);
                GlazedBar(dress,new Vector3(-.40f-w*.5f,0,0),new Vector3(w,0,.60f),Vector3.right,top,bottom);
                GlazedBar(dress,new Vector3(.40f+w*.5f,0,0),new Vector3(w,0,.60f),Vector3.left,top,bottom);
                foreach(var p in root.GetComponentsInChildren<VenomSurfacePatch>(true))
                    if(p.name.StartsWith("Bakery backrest"))
                    {var c=root.InverseTransformPoint(p.transform.position);GlazedBar(dress,new Vector3(c.x,0,.30f+w*.5f),new Vector3(p.Size.x,0,w),Vector3.back,-.30f+p.Size.y,-.25f);}
            }
            // plate lathe: inner flat to .411 (x) / .327 (z): wide enough for the bars (outer .46 / .36), 3 mm under the tiles
            Place("Plate",dress,new Vector3(0,-.345f-.008f*.8f,0),new Vector3(1.14f,.8f,1.13f),kit);

            // Toppings in sparse clusters on the plate's rim crest (outside the play floor, away from the cherry and handles),
            // set on the plate's actual surface: a ray down onto a temporary collider of the plate mesh (Codex review).
            {
                var plate=dress.Find("Plate");var probes=new List<MeshCollider>();
                foreach(var mf in plate.GetComponentsInChildren<MeshFilter>()){var mc=mf.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=mf.sharedMesh;probes.Add(mc);}
                Physics.SyncTransforms();
                var rng=new System.Random(41);var sx=plate.localScale.x;var sz=plate.localScale.z;
                // the crest of the lathe profile: radius .468 (x) and .468*.78/.98 (z), superellipse of order 4, then the dress scale
                Vector3 Crest(float t){float c=Mathf.Cos(t),sn=Mathf.Sin(t);return new Vector3(Mathf.Sign(c)*Mathf.Sqrt(Mathf.Abs(c))*.468f*sx,0,Mathf.Sign(sn)*Mathf.Sqrt(Mathf.Abs(sn))*.468f*.78f/.98f*sz);}
                bool OnPlate(Vector3 local,out Vector3 point,out Vector3 normal)
                {
                    point=normal=default;var from=root.TransformPoint(local+Vector3.up*.3f);
                    foreach(var h in Physics.RaycastAll(from,-root.up,.6f).OrderBy(h=>h.distance))
                        if(probes.Contains(h.collider as MeshCollider)){point=h.point;normal=h.normal;return true;}
                    return false;
                }
                void Put(string fbx,Vector3 local,float scale,Material only=null)
                {
                    if(!OnPlate(local,out var p,out var n))return;
                    var t=Place(fbx,dress,Vector3.zero,Vector3.one*scale,kit);
                    t.position=p;t.rotation=Quaternion.FromToRotation(Vector3.up,n)*Quaternion.Euler(0,rng.Next(360),0);
                    if(only!=null)foreach(var r in t.GetComponentsInChildren<Renderer>())r.sharedMaterial=only;
                }
                var sprinkleMats=new[]{kit["Strawberry"],kit["Cream"],kit["Wafer"]};
                float[] clusters={.35f,1.2f,2.0f,2.75f,3.6f,4.4f,5.3f,5.95f};
                for(int k=0;k<clusters.Length;k++)
                {
                    for(int i=0;i<3;i++)Put("Sprinkle",Crest(clusters[k]+(float)(rng.NextDouble()-.5)*.08f),1.5f,sprinkleMats[rng.Next(sprinkleMats.Length)]);
                    if(k%2==0)Put("ChocolateChip",Crest(clusters[k]+.05f),1.5f);
                    if(k%3==1)Put("MintLeaf",Crest(clusters[k]-.05f),1.5f);
                }
                foreach(var mc in probes)Object.DestroyImmediate(mc);
            }
            // 3. Props on their real owners: pad P, handle A, the gear lamp, the cherry; gears in biscuit.
            foreach(var sensor in game.GetComponentsInChildren<COgheTissueSensor>(true))
            {
                foreach(var r in sensor.GetComponentsInChildren<Renderer>(true))if(r.bounds.size.y<.02f)r.enabled=false;
                var c=root.InverseTransformPoint(sensor.transform.position);
                Place("PressurePad",dress,new Vector3(c.x,-.30f,c.z),Vector3.one,kit);
            }
            foreach(var task in game.GetComponentsInChildren<COgheTapRail>(true))
            {
                if(task.Handle==null)continue;
                foreach(var r in task.Handle.GetComponentsInChildren<Renderer>(true))r.enabled=false;
                var heading=Quaternion.Euler(0,game.Definition.CameraEuler.y,0);
                var candy=Place("CandyHandle",task.Handle,Vector3.zero,Vector3.one,kit);
                var ls=task.Handle.lossyScale;candy.localScale=new Vector3(1.6f/ls.x,1.6f/ls.y,1.6f/ls.z);   // the handle is a scaled primitive
                candy.rotation=root.rotation*heading;candy.position=task.Handle.position+root.up*.004f;
                var cb=candy.GetComponentInChildren<Renderer>().bounds;Debug.Log($"BAKERY candy {task.Label} at {root.InverseTransformPoint(cb.center):F3} size {cb.size:F3}");
                foreach(var r in candy.GetComponentsInChildren<Renderer>())
                {var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i]==kit["Strawberry"])mats[i]=candyBlue;r.sharedMaterials=mats;}   // A's circuit colour is blue
            }
            foreach(var train in game.GetComponentsInChildren<COgheGearTrain>(true))
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
            foreach(var r in game.GetComponentsInChildren<Renderer>(true))
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
            // the key (large window) alone casts soft shadows; the bounce fills from the other side, weaker, without shadow
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l.type!=LightType.Directional)continue;
                if(l.name.Contains("bounce")){l.color=new Color(.86f,.90f,1f);l.intensity=.3f;l.shadows=LightShadows.None;l.transform.rotation=Quaternion.Euler(35,140,0);}
                else{l.color=new Color(1f,.94f,.86f);l.intensity=.95f;l.shadows=LightShadows.Soft;l.shadowStrength=.7f;l.transform.rotation=Quaternion.Euler(58,-60,0);}
            }
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
