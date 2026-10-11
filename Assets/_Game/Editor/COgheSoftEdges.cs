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
    /// <summary>
    /// Soft edges (Mrk, 10/10/2026: "vẫn dùng phong cách đồ hoạ cũ. Là cái hộp"): slick tape along the foot of the glass
    /// wherever a walkable surface meets it (the glass is never climbable; the tape shows why), and block-shaped mechanisms
    /// drawn with rounded edges. A presentation pass over saved Spatial scenes, after the art builders. It adds meshes and
    /// hides face renderers only: no collider, navigation patch, input target or runtime behaviour changes, so the bevels
    /// are visual (STYLE_RULES §3). Run it on a freshly generated (or checked-out) scene; it refuses a scene it already did.
    /// The slick look is option E, the pastel hologram (Mrk, 11/10/2026): the shared "Lavender slippery" material and the
    /// tape use the COghe/Slick shader. The Q machine's pearl casing stays (equipment, not a slick block).
    /// All levels: -executeMethod GravityBox.Editor.COgheSoftEdges.ApplyAllFromCommandLine (checks physics and definitions
    /// are unchanged). Some: ApplyFromCommandLine -coghe-soft-levels 01,02,PlusK01 (-coghe-soft-dump lists the faces).
    /// The Spatial, Next and Plus generators call <see cref="Apply"/> before they save a scene.
    /// </summary>
    public static class COgheSoftEdges
    {
        const string Out="Assets/_Game/Venom/Art/SoftEdges";
        const string Marker="COghe soft edges";
        // The tape: 2.2 cm tall, 0.6 mm thick, on the inner face of the glass, starting just above the surface it guards.
        const float TapeHeight=.022f,TapeThick=.0006f,TapeLift=.0005f,TapeRepeat=.03f;
        // Clear of the exit hole by this much (its mint ring and aluminium lip stay readable).
        const float HoleMargin=.008f;
        static string dir;static int meshSerial;

        public static void ApplyFromCommandLine()
        {
            var args=Environment.GetCommandLineArgs();string keys="01";bool dump=args.Contains("-coghe-soft-dump");
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-soft-levels")keys=args[i+1];
            if(!dump)SlickMaterials();
            foreach(var key in keys.Split(','))
            {
                var scene=EditorSceneManager.OpenScene(VenomCampaignBuilder.SpatialContentPath(key.Trim()));
                var game=Object.FindFirstObjectByType<VenomCampaign>();
                if(dump){Dump(key,game);continue;}
                Apply(key.Trim(),game);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>Every Spatial scene, after the slick materials are switched; refuses to finish if the physics or the
        /// level definitions differ afterwards. Writes Artifacts/COgheSoftEdges/audit.csv.</summary>
        public static void ApplyAllFromCommandLine()
        {
            const string report="Artifacts/COgheSoftEdges";Directory.CreateDirectory(report);
            var paths=VenomCampaignBuilder.SpatialScenePaths();
            string before=COgheViewArtVerification.CapturePhysics(paths);File.WriteAllText(report+"/physics-before.txt",before);
            var definitions=Directory.GetFiles(VenomCampaignBuilder.SpatialFolder+"/Definitions","*.asset").OrderBy(p=>p).ToArray();
            var definitionText=definitions.Select(File.ReadAllText).ToArray();
            SlickMaterials();
            var rows=new List<string>{"position,scene,tapeRuns,roundedBlocks,skippedOpen,skippedNotBox"};
            foreach(var path in paths)
            {
                var scene=EditorSceneManager.OpenScene(path);var game=Object.FindFirstObjectByType<VenomCampaign>();
                string key=Path.GetFileNameWithoutExtension(path).Replace("COgheSpatial","");
                var (tapes,blocks,open,notBox)=Apply(key,game);
                rows.Add($"{game.Definition.Order},{Path.GetFileNameWithoutExtension(path)},{tapes},{blocks},{open},{notBox}");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            string after=COgheViewArtVerification.CapturePhysics(paths);File.WriteAllText(report+"/physics-after.txt",after);
            File.WriteAllLines(report+"/audit.csv",rows);
            if(before!=after||!definitionText.SequenceEqual(definitions.Select(File.ReadAllText)))
                throw new InvalidOperationException("Soft edges changed physical/input data or definitions.");
            Debug.Log($"SOFT EDGES VERIFIED: {paths.Length} scenes; physics and definitions identical.");
        }

        /// <summary>Option E, the pastel hologram: soft pink, mint and sky swirls drifting over lilac, glossy, with a slow shine.
        /// The tape marks its two edges.</summary>
        public static void Hologram(Material m,bool tape)
        {
            m.shader=Shader.Find("COghe/Slick");m.shaderKeywords=Array.Empty<string>();
            m.SetColor("_BaseColor",new Color(.60f,.52f,.91f));m.SetColor("_EdgeColor",new Color(.44f,.37f,.80f));m.SetColor("_SheenColor",Color.white);
            m.SetFloat("_Mode",2);m.SetFloat("_Film",1);m.SetFloat("_Pattern",0);m.SetFloat("_Iridescence",0);m.SetFloat("_Sparkle",0);m.SetFloat("_FlowSpeed",1);
            m.SetFloat("_Gloss",300);m.SetFloat("_Spec",1.5f);m.SetFloat("_Reflect",.75f);m.SetFloat("_Fresnel",.45f);
            m.SetFloat("_SheenStrength",.3f);m.SetFloat("_SheenSpeed",.25f);m.SetFloat("_SheenWidth",.05f);m.SetFloat("_EdgeBand",tape?.12f:0);
            EditorUtility.SetDirty(m);
        }
        /// <summary>The shared slick material of the Spatial levels takes the hologram look (the generators keep it: see
        /// VenomCampaignBuilder.SpatialMaterial), and so does the tape.</summary>
        public static void SlickMaterials()
        {
            var slick=AssetDatabase.LoadAssetAtPath<Material>(VenomCampaignBuilder.SpatialFolder+"/Lavender slippery.mat");
            if(slick!=null)Hologram(slick,false);
            TapeMaterial();AssetDatabase.SaveAssets();
        }

        public static (int tapes,int blocks,int open,int notBox) Apply(string key,VenomCampaign game)
        {
            if(game.GetComponentsInChildren<Transform>(true).Any(t=>t.name.StartsWith(Marker)))
                throw new InvalidOperationException($"{key} already has soft edges: regenerate it (or check its scene out) first");
            dir=$"{Out}/{key}";meshSerial=0;
            if(AssetDatabase.IsValidFolder(dir))AssetDatabase.DeleteAsset(dir);
            Directory.CreateDirectory(dir);AssetDatabase.Refresh();
            var root=game.GetComponent<VenomLevelController>().Rotation.transform;
            int tapes=Tape(game,root,TapeMaterial());
            var (blocks,open,notBox)=RoundBlocks(game);
            Debug.Log($"SOFT EDGES {key}: {tapes} tape runs, {blocks} rounded blocks ({open} open, {notBox} not boxes left as they were)");
            return (tapes,blocks,open,notBox);
        }

        // ---- the tape ---------------------------------------------------------------------------------------------------
        /// <summary>Along every side pane of the glass, above each fixed surface COghe stands on whose edge meets the pane:
        /// the floor all round, a platform against the glass. Cut round the exit hole and wherever something stands against
        /// the glass in the band (a climb board, a block).</summary>
        static int Tape(VenomCampaign game,Transform root,Material mat)
        {
            var patches=game.GetComponentsInChildren<VenomSurfacePatch>(true).Where(p=>p.gameObject.activeInHierarchy).ToArray();
            var panes=patches.Where(p=>p.ExteriorGlass&&Mathf.Abs(root.InverseTransformDirection(p.Normal).y)<.5f).ToArray();
            var supports=patches.Where(p=>!p.ExteriorGlass&&p.MotionFrame==null&&p.SphereRadius<=0&&p.Curved==null&&root.InverseTransformDirection(p.Normal).y>.9f&&p.Size.x>.03f&&p.Size.y>.03f).ToArray();
            int runs=0;
            foreach(var pane in panes)
            {
                var t=pane.transform;var bands=new List<(float y,float x0,float x1)>();
                foreach(var f in supports)
                {
                    var q=Corners(f).Select(c=>t.InverseTransformPoint(c)).ToArray();
                    for(int i=0;i<4;i++)
                    {
                        Vector3 a=q[i],b=q[(i+1)%4];
                        if(Mathf.Abs(a.z)>.006f||Mathf.Abs(b.z)>.006f||Mathf.Abs(a.y-b.y)>.003f)continue;   // not an edge lying on this pane
                        float y=(a.y+b.y)*.5f,x0=Mathf.Max(Mathf.Min(a.x,b.x),-pane.Size.x*.5f),x1=Mathf.Min(Mathf.Max(a.x,b.x),pane.Size.x*.5f);
                        if(x1-x0>.01f&&y+TapeHeight<pane.Size.y*.5f)bands.Add((y,x0,x1));
                    }
                }
                if(bands.Count==0)continue;
                // what stands against the glass inside a band: any other fixed face within 3 cm of the pane
                var blockers=patches.Where(p=>p!=pane&&!p.ExteriorGlass&&p.MotionFrame==null).Select(p=>Corners(p).Select(c=>t.InverseTransformPoint(c)).ToArray()).ToArray();
                var parts=new List<CombineInstance>();
                foreach(var (y,x0,x1) in bands)
                {
                    float lo=y+TapeLift,hi=y+TapeHeight;var keep=new List<(float,float)>{(x0,x1)};
                    if(pane.Hole&&pane.HoleCentre.y-pane.HoleRadius-HoleMargin<hi&&pane.HoleCentre.y+pane.HoleRadius+HoleMargin>lo)
                        keep=Cut(keep,pane.HoleCentre.x-pane.HoleRadius-HoleMargin,pane.HoleCentre.x+pane.HoleRadius+HoleMargin);
                    foreach(var q in blockers)
                    {
                        float zMin=q.Min(v=>v.z),yMin=q.Min(v=>v.y),yMax=q.Max(v=>v.y);
                        if(zMin>.03f||q.Max(v=>v.z)<-.001f||yMax<=lo+.001f||yMin>=hi-.001f)continue;
                        keep=Cut(keep,q.Min(v=>v.x)-.002f,q.Max(v=>v.x)+.002f);
                    }
                    foreach(var (a,b) in keep)if(b-a>.012f){parts.Add(new CombineInstance{mesh=TapeStrip(b-a,hi-lo,a),transform=Matrix4x4.Translate(new Vector3(0,lo,0))});runs++;}
                }
                if(parts.Count==0)continue;
                var go=new GameObject(Marker+" · tape",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(t,false);
                var mesh=new Mesh{name="Slick tape"};mesh.CombineMeshes(parts.ToArray(),true,true);mesh.RecalculateBounds();
                go.GetComponent<MeshFilter>().sharedMesh=Save(mesh,"tape");
                var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;
                foreach(var p in parts)Object.DestroyImmediate(p.mesh);
            }
            return runs;
        }
        static List<(float,float)> Cut(List<(float,float)> spans,float a,float b)
        {
            var result=new List<(float,float)>();
            foreach(var (x0,x1) in spans)
            {
                if(b<=x0||a>=x1){result.Add((x0,x1));continue;}
                if(a>x0)result.Add((x0,a));if(b<x1)result.Add((b,x1));
            }
            return result;
        }
        /// <summary>A thin slab in the pane's frame: x from <paramref name="x0"/> along <paramref name="length"/>, y 0..height,
        /// z just inside the glass (the pane's normal points into the box). U runs along the tape, V across it.</summary>
        static Mesh TapeStrip(float length,float height,float x0)
        {
            float x1=x0+length,z0=TapeThick*.7f,z1=z0+TapeThick;
            var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector2 ua,Vector2 ub,Vector2 uc,Vector2 ud)
            {int k=v.Count;v.AddRange(new[]{a,b,c,d});uv.AddRange(new[]{ua,ub,uc,ud});tri.AddRange(new[]{k,k+2,k+1,k,k+3,k+2});}   // clockwise seen from outside each face
            float u0=x0/TapeRepeat,u1=x1/TapeRepeat;
            // inside face (toward the box), outside face (seen through the glass), the two long edges
            Quad(new Vector3(x0,0,z1),new Vector3(x0,height,z1),new Vector3(x1,height,z1),new Vector3(x1,0,z1),new Vector2(u0,0),new Vector2(u0,1),new Vector2(u1,1),new Vector2(u1,0));
            Quad(new Vector3(x1,0,z0),new Vector3(x1,height,z0),new Vector3(x0,height,z0),new Vector3(x0,0,z0),new Vector2(u1,0),new Vector2(u1,1),new Vector2(u0,1),new Vector2(u0,0));
            Quad(new Vector3(x0,height,z0),new Vector3(x1,height,z0),new Vector3(x1,height,z1),new Vector3(x0,height,z1),new Vector2(u0,1),new Vector2(u1,1),new Vector2(u1,1),new Vector2(u0,1));
            Quad(new Vector3(x0,0,z1),new Vector3(x1,0,z1),new Vector3(x1,0,z0),new Vector3(x0,0,z0),new Vector2(u0,0),new Vector2(u1,0),new Vector2(u1,0),new Vector2(u0,0));
            var m=new Mesh();m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tri,0);m.RecalculateNormals();m.RecalculateTangents();return m;
        }
        /// <summary>Shared by every level: the slick hologram with its edges marked (STYLE_RULES: slippery areas need a
        /// visible boundary).</summary>
        static Material TapeMaterial()
        {
            string matPath=$"{Out}/Slick tape.mat";Directory.CreateDirectory(Out);
            var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(mat==null){mat=new Material(Shader.Find("COghe/Slick")){name="Slick tape"};AssetDatabase.CreateAsset(mat,matPath);}
            if(mat.GetFloat("_Mode")!=2||mat.shader.name!="COghe/Slick")Hologram(mat,true);
            return mat;
        }

        // ---- rounded blocks ---------------------------------------------------------------------------------------------
        /// <summary>Each block (faces sharing a name under one parent, at least four, every face filling its whole side of the
        /// block's box) is drawn as one rounded box, each side in its own face's material (an ivory top keeps its lavender
        /// slick sides). The faces' renderers are hidden; the faces, colliders and navigation stay as they were.</summary>
        static (int rounded,int open,int notBox) RoundBlocks(VenomCampaign game)
        {
            var groups=new Dictionary<(Transform,string),List<VenomSurfacePatch>>();
            foreach(var p in game.GetComponentsInChildren<VenomSurfacePatch>(true))
            {
                var r=p.GetComponent<Renderer>();
                if(r==null||!r.enabled||!p.gameObject.activeInHierarchy||p.ExteriorGlass||p.SphereRadius>0||p.Curved!=null||p.Hole||p.name=="Laboratory floor")continue;
                var k=(p.transform.parent,p.name);if(!groups.TryGetValue(k,out var list))groups[k]=list=new List<VenomSurfacePatch>();list.Add(p);
            }
            var all=game.GetComponentsInChildren<VenomSurfacePatch>(true).Where(p=>p.gameObject.activeInHierarchy).ToArray();
            int count=0,openSides=0,notBoxes=0;
            foreach(var kv in groups)
            {
                var parent=kv.Key.Item1;var faces=kv.Value;if(faces.Count<4)continue;
                var pts=faces.SelectMany(f=>Corners(f).Select(c=>parent.InverseTransformPoint(c))).ToArray();
                var b=new Bounds(pts[0],Vector3.zero);foreach(var v in pts)b.Encapsulate(v);
                float min=Mathf.Min(b.size.x,Mathf.Min(b.size.y,b.size.z));if(min<.006f)continue;
                // every face must be a whole side of the box: else the box would cover an opening the faces leave
                var sides=new Material[6];bool whole=true;
                foreach(var f in faces)
                {
                    var n=parent.InverseTransformDirection(f.Normal);int axis=Mathf.Abs(n.x)>.9f?0:Mathf.Abs(n.y)>.9f?1:Mathf.Abs(n.z)>.9f?2:-1;
                    if(axis<0){whole=false;break;}
                    var fp=Corners(f).Select(c=>parent.InverseTransformPoint(c)).ToArray();
                    float plane=fp[0][axis],side=n[axis]>0?b.max[axis]:b.min[axis];
                    if(Mathf.Abs(plane-side)>.002f){whole=false;break;}
                    for(int a=0;a<3;a++)if(a!=axis&&(Mathf.Abs(fp.Min(v=>v[a])-b.min[a])>.002f||Mathf.Abs(fp.Max(v=>v[a])-b.max[a])>.002f))whole=false;
                    if(!whole)break;
                    sides[axis*2+(n[axis]>0?0:1)]=f.GetComponent<Renderer>().sharedMaterial;
                }
                if(!whole){Debug.Log($"SOFT EDGES skip {kv.Key.Item2} under {parent.name}: its faces do not close a box");notBoxes++;continue;}
                // a side with no face must be closed by the glass, the floor or another body's face against it; else it is an
                // opening (level 49's drawer slot under the exit platform) and the box would cover it
                bool open=false;
                for(int i=0;i<6&&!open;i++)if(sides[i]==null&&!Closed(i,b,parent,faces,all))open=true;
                if(open){Debug.Log($"SOFT EDGES skip {kv.Key.Item2} under {parent.name}: one side is open");openSides++;continue;}
                var fallback=sides[2]??faces[0].GetComponent<Renderer>().sharedMaterial;
                for(int i=0;i<6;i++)if(sides[i]==null)sides[i]=fallback;
                float radius=Mathf.Min(.012f,min*.2f);
                var go=new GameObject($"{Marker} · {kv.Key.Item2}",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.transform.localPosition=b.center;
                go.GetComponent<MeshFilter>().sharedMesh=Save(RoundedBox(b.size,radius,4,.10f),kv.Key.Item2.Replace(' ','-'));
                var mr=go.GetComponent<MeshRenderer>();mr.sharedMaterials=sides;var first=faces[0].GetComponent<Renderer>();
                mr.shadowCastingMode=first.shadowCastingMode;mr.receiveShadows=first.receiveShadows;
                foreach(var f in faces)f.GetComponent<Renderer>().enabled=false;
                count++;
            }
            return (count,openSides,notBoxes);
        }
        /// <summary>Side <paramref name="side"/> (+x, −x, +y, −y, +z, −z) of the box is covered: a face of something else
        /// lies in its plane, facing back at it, under its centre.</summary>
        static bool Closed(int side,Bounds b,Transform parent,List<VenomSurfacePatch> own,VenomSurfacePatch[] all)
        {
            int axis=side/2;float sign=side%2==0?1:-1;var centre=b.center;centre[axis]=sign>0?b.max[axis]:b.min[axis];
            var dirLocal=Vector3.zero;dirLocal[axis]=sign;
            Vector3 point=parent.TransformPoint(centre),outward=parent.TransformDirection(dirLocal).normalized;
            foreach(var g in all)
            {
                if(own.Contains(g)||Vector3.Dot(g.Normal,outward)>-.9f)continue;
                var q=g.transform.InverseTransformPoint(point);
                if(Mathf.Abs(q.z)<.006f&&Mathf.Abs(q.x)<=g.Size.x*.5f+.002f&&Mathf.Abs(q.y)<=g.Size.y*.5f+.002f)return true;
            }
            return false;
        }
        /// <summary>A box with rounded edges and corners, one submesh per side (+x, −x, +y, −y, +z, −z). Each side is a grid
        /// whose lines fall at even angles round the edge (<paramref name="steps"/> per 45°), flat across the middle.
        /// UVs are planar, in metres over <paramref name="period"/>.</summary>
        static Mesh RoundedBox(Vector3 size,float radius,int steps,float period)
        {
            var half=size*.5f;radius=Mathf.Min(radius,Mathf.Min(half.x,Mathf.Min(half.y,half.z))-.0005f);var inner=half-Vector3.one*radius;
            float[] Lines(float h,float inr)
            {
                var l=new List<float>();for(int k=steps;k>=1;k--)l.Add(-inr-radius*Mathf.Tan(Mathf.PI*.25f*k/steps));
                l.Add(-inr);if(inr>1e-5f)l.Add(inr);for(int k=1;k<=steps;k++)l.Add(inr+radius*Mathf.Tan(Mathf.PI*.25f*k/steps));
                return l.ToArray();
            }
            var verts=new List<Vector3>();var norms=new List<Vector3>();var uvs=new List<Vector2>();var subs=new List<int>[6];
            var sides=new[]{(Vector3.right,Vector3.forward,Vector3.up),(Vector3.left,Vector3.back,Vector3.up),(Vector3.up,Vector3.right,Vector3.forward),
                            (Vector3.down,Vector3.right,Vector3.back),(Vector3.forward,Vector3.left,Vector3.up),(Vector3.back,Vector3.right,Vector3.up)};
            for(int s=0;s<6;s++)
            {
                var (normal,u,v)=sides[s];subs[s]=new List<int>();
                float hu=Vector3.Dot(Abs(u),half),hv=Vector3.Dot(Abs(v),half),iu=Vector3.Dot(Abs(u),inner),iv=Vector3.Dot(Abs(v),inner);
                var lu=Lines(hu,iu);var lv=Lines(hv,iv);int start=verts.Count;
                foreach(float b in lv)foreach(float a in lu)
                {
                    // a point on the cube's side (its edge band reaches past the box, tan 45° = 1: exactly to the edge)
                    var p=Vector3.Scale(normal,half)+u*Mathf.Clamp(a,-hu,hu)+v*Mathf.Clamp(b,-hv,hv);
                    var c=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));
                    var d=p-c;var nn=d.sqrMagnitude>1e-12f?d.normalized:normal;
                    verts.Add(c+nn*radius);norms.Add(nn);uvs.Add(new Vector2(Vector3.Dot(p,u),Vector3.Dot(p,v))/period);
                }
                int nu=lu.Length;
                for(int j=0;j<lv.Length-1;j++)for(int i=0;i<nu-1;i++){int k=start+j*nu+i;subs[s].AddRange(new[]{k,k+nu,k+1,k+1,k+nu,k+nu+1});}
            }
            var m=new Mesh{name="Rounded block"};m.SetVertices(verts);m.SetNormals(norms);m.SetUVs(0,uvs);m.subMeshCount=6;
            for(int s=0;s<6;s++)m.SetTriangles(subs[s],s);
            m.RecalculateBounds();m.RecalculateTangents();return m;
        }
        static Vector3 Abs(Vector3 v)=>new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));

        // ---- helpers ----------------------------------------------------------------------------------------------------
        static Vector3[] Corners(VenomSurfacePatch f)
        {
            var t=f.transform;var h=f.Size*.5f;
            return new[]{t.TransformPoint(new Vector3(-h.x,-h.y,0)),t.TransformPoint(new Vector3(h.x,-h.y,0)),t.TransformPoint(new Vector3(h.x,h.y,0)),t.TransformPoint(new Vector3(-h.x,h.y,0))};
        }
        static Mesh Save(Mesh m,string name){AssetDatabase.CreateAsset(m,$"{dir}/{name}-{meshSerial++:000}.asset");return m;}

        /// <summary>Every Spatial scene: the materials on slick faces and on other renderers that look slick, for the
        /// switch to the slick shader. Writes Artifacts/soft-inventory.txt.</summary>
        public static void InventoryFromCommandLine()
        {
            var faceMats=new Dictionary<string,int>();var artMats=new Dictionary<string,int>();var sb=new System.Text.StringBuilder();
            foreach(var path in VenomCampaignBuilder.SpatialScenePaths())
            {
                EditorSceneManager.OpenScene(path);var game=Object.FindFirstObjectByType<VenomCampaign>();if(game==null){sb.AppendLine("no game: "+path);continue;}
                int slick=0;
                foreach(var p in game.GetComponentsInChildren<VenomSurfacePatch>(true))
                {
                    if(!p.Slippery||p.ExteriorGlass)continue;slick++;
                    var r=p.GetComponent<Renderer>();string m=r==null?"(no renderer)":!r.enabled?"(renderer off) "+r.sharedMaterial?.name:r.sharedMaterial?.name;
                    faceMats[m]=faceMats.TryGetValue(m,out int k)?k+1:1;
                    if(m=="Spatial pearl casing"){string fk="  pearl slick: "+p.name;faceMats[fk]=faceMats.TryGetValue(fk,out int j)?j+1:1;}
                }
                foreach(var r in game.GetComponentsInChildren<Renderer>(true))
                {
                    if(r.GetComponent<VenomSurfacePatch>()!=null||!r.enabled)continue;
                    foreach(var m in r.sharedMaterials){if(m==null)continue;string n=m.name;
                        if(n.IndexOf("lavender",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("slip",StringComparison.OrdinalIgnoreCase)>=0)
                        {string key=$"{n} [{m.shader.name}] on '{r.name}'";artMats[key]=artMats.TryGetValue(key,out int k)?k+1:1;}}
                }
                sb.AppendLine($"{Path.GetFileNameWithoutExtension(path)}: {slick} slick faces");
            }
            sb.AppendLine("\nSLICK FACE MATERIALS");foreach(var kv in faceMats.OrderByDescending(k=>k.Value))sb.AppendLine($"  {kv.Value,5}  {kv.Key}");
            sb.AppendLine("\nOTHER SLICK-LOOKING RENDERERS");foreach(var kv in artMats.OrderByDescending(k=>k.Value))sb.AppendLine($"  {kv.Value,5}  {kv.Key}");
            File.WriteAllText("Artifacts/soft-inventory.txt",sb.ToString());
        }

        static void Dump(string key,VenomCampaign game)
        {
            var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;var sb=new System.Text.StringBuilder();
            sb.AppendLine($"SOFT DUMP {key} root={root.name}");
            foreach(var p in game.GetComponentsInChildren<VenomSurfacePatch>(true))
            {
                var r=p.GetComponent<Renderer>();var n=root.InverseTransformDirection(p.Normal);
                sb.AppendLine($"  face '{p.name}' parent='{p.transform.parent.name}' size={p.Size:F3} n={n:F1} glass={p.ExteriorGlass} slick={p.Slippery} hole={p.Hole} moving={p.MotionFrame!=null} " +
                              $"renderer={(r==null?"none":(r.enabled?"on":"off"))} mat={(r!=null&&r.sharedMaterial!=null?r.sharedMaterial.name:"-")} active={p.gameObject.activeInHierarchy}");
            }
            foreach(var r in game.GetComponentsInChildren<Renderer>(true))
                if(r.enabled&&r.GetComponent<VenomSurfacePatch>()==null&&r.bounds.size.magnitude>.05f)
                    sb.AppendLine($"  art '{r.name}' parent='{r.transform.parent?.name}' mat={r.sharedMaterial?.name} size={r.bounds.size:F3}");
            File.WriteAllText($"Artifacts/soft-dump-{key}.txt",sb.ToString());
        }
    }
}
