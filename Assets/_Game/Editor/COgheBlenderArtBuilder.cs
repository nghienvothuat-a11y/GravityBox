using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static class COgheBlenderArtBuilder
    {
        const string Folder="Assets/_Game/Venom/Art/NewGraphic";
        [Serializable] public class Pack {public Level[] levels;public MeshSet[] meshes;}
        [Serializable] public class Level {public int number;public Part[] parts;}
        [Serializable] public class Part {public string id,parent,anchor,kind,key;public bool pane;}
        [Serializable] public class MeshSet {public string key;public Chunk[] chunks;}
        [Serializable] public class Chunk {public string material;public Vector3[] vertices,normals;public Vector2[] uv;public int[] triangles;}
        static Dictionary<string,Material> materials;
        static Dictionary<string,Mesh[]> meshes;
        static Pack pack;
        static Material studioMaterial;
        static string Scene(int n)=>$"Assets/_Game/Venom/ViewCampaign/COgheView{n:00}.unity";
        static Material Material(string name,Color color,float metal,float smooth)
        {
            string path=Folder+"/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetFloat("_Cull",name=="Floor"||name=="Blue"||name=="Lavender"||name=="Mint"||name=="Grout"?0:2);m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);return m;
        }
        static void LoadKit()
        {
            Directory.CreateDirectory(Folder+"/Models");Directory.CreateDirectory(Folder+"/Materials");AssetDatabase.Refresh();
            materials=new Dictionary<string,Material>{
                ["Grout"]=Material("Grout",new Color(.64f,.64f,.58f),0,.15f),
                ["Ivory"]=Material("Ivory",new Color(.94f,.90f,.82f),.04f,.51f),
                ["Blue"]=Material("Blue",new Color(.68f,.77f,.82f),.08f,.46f),
                ["Amber"]=Material("Amber",new Color(1f,.64f,.16f),.05f,.63f),
                ["Lavender"]=Material("Lavender",new Color(.60f,.52f,.80f),.02f,.40f),
                ["Metal"]=Material("Metal",new Color(.56f,.67f,.73f),.40f,.63f),
                ["Graphite"]=Material("Graphite",new Color(.065f,.09f,.11f),.08f,.3f),
                ["Mint"]=Material("Mint",new Color(.29f,.73f,.56f),.02f,.42f),
                ["Creature"]=Material("Creature",new Color(.018f,.025f,.030f),.16f,.77f),
                ["Floor"]=Material("Floor",Color.white,.02f,.32f)};
            const int resolution=256;
            var texture=new Texture2D(resolution,resolution,TextureFormat.RGB24,true);
            for(int y=0;y<resolution;y++)for(int x=0;x<resolution;x++)
            {
                float u=(x+.5f)/resolution,v=(y+.5f)/resolution;
                float edge=Mathf.Min(Mathf.Min(u,1-u),Mathf.Min(v,1-v));
                float seam=Mathf.Min(Mathf.Abs(Mathf.Repeat(u*4+.5f,1)-.5f),Mathf.Abs(Mathf.Repeat(v*3+.5f,1)-.5f));
                float shade=1-.08f*Mathf.Exp(-edge*70)-.18f*(1-Mathf.SmoothStep(0,1,seam/.023f));
                texture.SetPixel(x,y,new Color(.94f,.90f,.81f)*shade);
            }
            texture.Apply(true);texture.wrapMode=TextureWrapMode.Clamp;
            var texturePath=Folder+"/Materials/Porcelain.asset";
            var saved=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if(saved==null){AssetDatabase.CreateAsset(texture,texturePath);saved=texture;}else{EditorUtility.CopySerialized(texture,saved);Object.DestroyImmediate(texture);}
            // Panel texture gives manufactured joints and local contact shading without post effects.
            var wall=new Texture2D(128,128,TextureFormat.RGB24,true);
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                float u=(x+.5f)/128,v=(y+.5f)/128;
                float seam=Mathf.Abs(Mathf.Repeat(u*3+.5f,1)-.5f);
                float shade=1-.12f*Mathf.Exp(-v*12)-.05f*Mathf.Exp(-Mathf.Min(u,1-u)*35)-.08f*(1-Mathf.SmoothStep(0,1,seam/.016f));
                wall.SetPixel(x,y,Color.white*shade);
            }
            wall.Apply(true);wall.wrapMode=TextureWrapMode.Clamp;
            var wallPath=Folder+"/Materials/Panel.asset";var savedWall=AssetDatabase.LoadAssetAtPath<Texture2D>(wallPath);
            if(savedWall==null){AssetDatabase.CreateAsset(wall,wallPath);savedWall=wall;}else{EditorUtility.CopySerialized(wall,savedWall);Object.DestroyImmediate(wall);}
            materials["Blue"].SetTexture("_BaseMap",savedWall);EditorUtility.SetDirty(savedWall);
            materials["Floor"].SetTexture("_BaseMap",saved);EditorUtility.SetDirty(saved);
            var backgroundPath=Folder+"/Materials/LaboratoryBackdrop.png";
            var importer=AssetImporter.GetAtPath(backgroundPath) as TextureImporter;
            if(importer!=null){importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.mipmapEnabled=false;importer.maxTextureSize=1024;importer.SaveAndReimport();}
            var studioPath=Folder+"/Materials/Laboratory studio.mat";
            studioMaterial=AssetDatabase.LoadAssetAtPath<Material>(studioPath);
            if(studioMaterial==null){studioMaterial=new Material(Shader.Find("COghe/NewGraphic Laboratory Backdrop"));AssetDatabase.CreateAsset(studioMaterial,studioPath);}
            studioMaterial.SetColor("_BaseColor",new Color(.945f,.933f,.897f));
            studioMaterial.SetTexture("_LabTex",AssetDatabase.LoadAssetAtPath<Texture2D>(backgroundPath));EditorUtility.SetDirty(studioMaterial);
            pack=JsonUtility.FromJson<Pack>(File.ReadAllText(COgheBlenderExport.Source+"/meshpack.json"));meshes=new Dictionary<string,Mesh[]>();
            foreach(var set in pack.meshes)
            {
                var list=new List<Mesh>();
                foreach(var chunk in set.chunks)
                {
                    string path=Folder+"/Models/"+set.key+"-"+chunk.material+".asset";
                    var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
                    mesh.name="Blender "+set.key+" "+chunk.material;mesh.indexFormat=chunk.vertices.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16;
                    mesh.vertices=chunk.vertices;mesh.normals=chunk.normals;mesh.uv=chunk.uv;mesh.triangles=chunk.triangles;mesh.RecalculateBounds();
                    EditorUtility.SetDirty(mesh);list.Add(mesh);
                }
                meshes[set.key]=list.ToArray();
            }
            // Only this builder owns these generated mesh assets. Remove superseded
            // revisions after loading the current pack, never shared gameplay assets.
            foreach(var path in Directory.GetFiles(Folder+"/Models","*.asset"))
            {
                string file=Path.GetFileNameWithoutExtension(path);
                if(file.Length>17&&file[16]=='-'&&!meshes.ContainsKey(file.Substring(0,16)))AssetDatabase.DeleteAsset(path);
            }
        }
        [MenuItem("Gravity Box/COghe/NewGraphic/Apply Blender kit to V2 01–10")]
        public static void Apply()
        {
            LoadKit();
            string before=COgheViewArtVerification.CapturePhysics();
            foreach(var level in pack.levels)
            {
                var scene=EditorSceneManager.OpenScene(Scene(level.number));
                var game=Object.FindFirstObjectByType<VenomCampaign>();var owner=game.GetComponent<VenomLevelController>();
                var previous=game.GetComponent<COgheGraphicProfile>();
                if(previous!=null){foreach(var go in previous.NewRoots)if(go!=null)Object.DestroyImmediate(go);Object.DestroyImmediate(previous);}
                var profile=game.gameObject.AddComponent<COgheGraphicProfile>();
                var replaced=new HashSet<Renderer>();var roots=new List<GameObject>();var panes=new List<Transform>();
                var view=game.GetComponent<COgheViewPresentation>();
                foreach(var t in view.PaneVisuals)foreach(var r in t.GetComponentsInChildren<Renderer>())if(!(r is LineRenderer)&&r.GetComponent<TextMesh>()==null)replaced.Add(r);
                foreach(var t in game.GetComponentsInChildren<Transform>(true))
                    if(t.name=="V2 shell"||t.name=="V2 rounded handle"||t.name=="V2 satin inspection baffle")
                        foreach(var r in t.GetComponentsInChildren<MeshRenderer>())if(r.GetComponent<TextMesh>()==null)replaced.Add(r);
                var platform=owner.Rotation.transform.Find("V2 porcelain platform");
                // This batch also contains guide rails: retain it initially except the chassis,
                // which is replaced below by matching its material batch.
                if(platform!=null)foreach(var r in platform.GetComponentsInChildren<MeshRenderer>())
                    if(r.sharedMaterial.name.Contains("porcelain")||r.sharedMaterial.name.Contains("warm floor"))replaced.Add(r);
                foreach(var p in game.Surfaces)
                    if(!p.ExteriorGlass&&p.GetComponentInParent<VenomMovableProp>()==null&&p.GetComponent<MeshRenderer>()!=null)
                        replaced.Add(p.GetComponent<MeshRenderer>());
                var byParent=new Dictionary<string,Transform>();
                foreach(var part in level.parts)
                {
                    var indices=part.anchor.Split('/');
                    var parent=scene.GetRootGameObjects()[int.Parse(indices[0])].transform;
                    for(int depth=1;depth<indices.Length;depth++)parent=parent.GetChild(int.Parse(indices[depth]));if(parent==null||COgheBlenderExport.PathOf(parent)!=part.parent)throw new Exception("Art anchor changed: "+part.parent);
                    var group=new GameObject("NewGraphic · "+part.id);group.transform.SetParent(parent,false);roots.Add(group);byParent[part.anchor]=group.transform;
                    var set=Array.Find(pack.meshes,s=>s.key==part.key);
                    for(int i=0;i<set.chunks.Length;i++)
                    {
                        var go=new GameObject(set.chunks[i].material,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(group.transform,false);
                        go.GetComponent<MeshFilter>().sharedMesh=meshes[part.key][i];var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=materials[set.chunks[i].material];
                        bool thinSurface=part.kind=="surface"||part.kind=="exit";
                        r.shadowCastingMode=thinSurface?ShadowCastingMode.Off:ShadowCastingMode.On;r.receiveShadows=part.kind!="exit";
                    }
                }
                foreach(var p in view.Panes)panes.Add(byParent[COgheBlenderExport.KeyOf(p.transform)]);
                profile.Replaced=replaced.ToArray();profile.NewRoots=roots.ToArray();profile.NewPanes=panes.ToArray();profile.CreatureMaterial=materials["Creature"];profile.StudioMaterial=studioMaterial;
                // Bake fade variants without mutating the original view's serialized sources.
                var source=new List<Material>();var fade=new List<Material>();
                foreach(var mat in materials.Values)
                {
                    string path=Folder+"/Materials/"+mat.name+" Fade.mat";var f=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(f==null){f=new Material(mat);AssetDatabase.CreateAsset(f,path);}f.CopyPropertiesFromMaterial(mat);
                    f.SetFloat("_Surface",1);f.SetFloat("_Blend",0);f.SetFloat("_AlphaClip",0);f.SetFloat("_BlendModePreserveSpecular",0);
                    f.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);f.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);f.SetFloat("_SrcBlendAlpha",1);f.SetFloat("_DstBlendAlpha",(float)BlendMode.OneMinusSrcAlpha);f.SetFloat("_ZWrite",0);
                    f.SetOverrideTag("RenderType","Transparent");f.renderQueue=3000;f.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    f.SetShaderPassEnabled("DepthOnly",false);f.SetShaderPassEnabled("DepthNormals",false);f.SetShaderPassEnabled("ShadowCaster",false);EditorUtility.SetDirty(f);source.Add(mat);fade.Add(f);
                }
                profile.FadeSources=source.ToArray();profile.FadeVariants=fade.ToArray();
                EditorUtility.SetDirty(profile);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();var after=COgheViewArtVerification.CapturePhysics();
            File.WriteAllText("Artifacts/COgheNewGraphic/physics-after.txt",after);
            if(before!=after)throw new Exception("Art changed serialized physics data");
            Debug.Log("COGHE NEWGRAPHIC: ten scenes, Blender assets, serialized physics unchanged.");
        }
        public static void BuildMac()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=VenomCampaignBuilder.ViewCampaignScenePaths(),target=BuildTarget.StandaloneOSX,locationPathName="Builds/NewGraphic/macOS/COghe.app",options=BuildOptions.None,extraScriptingDefines=new[]{"COGHE_MOBILE_BENCHMARK"}});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("NewGraphic Mac build failed");
        }
        public static void ApplyAndBuildAll(){Apply();BuildMac();COgheAndroidBuilder.BuildNewGraphic();}
        public static void ApplyAndBuildAndroid(){Apply();COgheAndroidBuilder.BuildNewGraphic();}
        public static void ApplyAndBuildMac(){Apply();BuildMac();}
    }
}
