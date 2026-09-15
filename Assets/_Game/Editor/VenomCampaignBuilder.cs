using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static class VenomCampaignBuilder
    {
        public const string Folder="Assets/_Game/Venom/Campaign";
        private static Material glass,stone,mint,slip,metal,plastic,skin;
        private static PhysicsMaterial contact,slick,stepContact;
        private static readonly string[] Names={"Bò đi","Leo đi","Xoay đi","Trơn đấy","Trượt đi","Xoay tròn","Đẩy","Chui qua lỗ","Lật đi","BOSS · Chờ nhau"};
        private static readonly string[] Lessons={"Chạm để chỉ đường cho bạn nhỏ tới lỗ.","Bạn nhỏ có thể leo qua vách và bám lên kính.","Kéo xoay hộp để nhìn và chạm đúng mặt.","Có những bề mặt bạn nhỏ không bám được.","Đổi hướng chiếc hộp, đổi cách dùng mặt trơn.","Đưa lỗ tới bạn nhỏ đang trượt trong khối cầu.","Chạm hộp rồi chạm đích để đẩy / kéo. Buông sau 3 giây không điều khiển.","Căn từ nóc để rơi vào vành bám quanh ống.","Có vật đang đậy lỗ. Trọng lực cũng tác động lên đồ vật.",""};
        private static int meshSerial;
        [MenuItem("Gravity Box/Venom/Generate Origin Campaign 01–10")]
        public static void Generate()
        {
            Directory.CreateDirectory(Folder+"/Meshes");Directory.CreateDirectory(Folder+"/Definitions");AssetDatabase.Refresh();
            contact=Asset<PhysicsMaterial>("Contact.physicMaterial",()=>new PhysicsMaterial());contact.dynamicFriction=.10f;contact.staticFriction=.12f;contact.bounciness=.02f;contact.frictionCombine=PhysicsMaterialCombine.Minimum;EditorUtility.SetDirty(contact);
            slick=Asset<PhysicsMaterial>("Slippery.physicMaterial",()=>new PhysicsMaterial());slick.dynamicFriction=slick.staticFriction=0;slick.bounciness=.02f;slick.frictionCombine=PhysicsMaterialCombine.Minimum;EditorUtility.SetDirty(slick);
            stepContact=Asset<PhysicsMaterial>("Weighted step.physicMaterial",()=>new PhysicsMaterial());stepContact.dynamicFriction=.025f;stepContact.staticFriction=.03f;stepContact.bounciness=0;stepContact.frictionCombine=PhysicsMaterialCombine.Minimum;EditorUtility.SetDirty(stepContact);
            glass=Mat("Glass",new Color(.29f,.58f,.55f,.085f),true,.08f,.35f);
            stone=Mat("Floor",new Color(.24f,.37f,.34f,.36f),true,.12f,.35f);
            slip=Mat("Slippery",new Color(.31f,.50f,.72f,.33f),true,.3f,.94f);
            mint=Mat("Mint",new Color(.34f,.9f,.66f),false,.15f,.6f);mint.EnableKeyword("_EMISSION");mint.SetColor("_EmissionColor",new Color(.08f,.34f,.20f));
            metal=Mat("Blade",new Color(.40f,.47f,.48f),false,.86f,.8f);plastic=Mat("Warm resin",new Color(.65f,.43f,.22f),false,.05f,.45f);
            skin=Mat("Living obsidian",new Color(.024f,.032f,.04f),false,.5f,.87f);
            var profile=Asset<VenomProfile>("Matter.asset",()=>ScriptableObject.CreateInstance<VenomProfile>());
            EditorUtility.CopySerialized(AssetDatabase.LoadAssetAtPath<VenomProfile>("Assets/_Game/Venom/Living matter.asset"),profile);
            profile.Skin=skin;profile.Contact=contact;profile.AnimationSpeed=1.65f;profile.MeshCell=.007f;profile.DanceInterval=14;EditorUtility.SetDirty(profile);
            var scenePaths=new List<EditorBuildSettingsScene>();
            for(int number=1;number<=10;number++)
            {
                meshSerial=number*1000;
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var owner=new GameObject("Venom Origin "+number.ToString("00")).AddComponent<VenomLevelController>();
                owner.MatterProfile=profile;owner.ControlMode=VenomControlMode.TouchSurface;
                owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");owner.IndicatorMaterial=mint;
                var game=owner.gameObject.AddComponent<VenomCampaign>();
                var data=Asset<VenomCampaignDefinition>("Definitions/Level"+number.ToString("00")+".asset",()=>ScriptableObject.CreateInstance<VenomCampaignDefinition>());
                data.Id="venom.origin."+number.ToString("00");data.Order=number;data.Title=number.ToString("00")+" · "+Names[number-1];data.Lesson=Lessons[number-1];
                data.CanRotate=number!=7&&number!=8;data.Boss=number==10;data.Passive=number==6;data.ViewRadius=number==8?.59f:.46f;
                // Look into the inlet from the left. A steeper pitch would put
                // the selectable ceiling over the inlet's touch target.
                data.CameraEuler=number==8?new Vector3(30,42,0):new Vector3(number==3?14:25,-24,0);EditorUtility.SetDirty(data);game.Definition=data;
                owner.Apparatus=new GameObject("Apparatus").transform;owner.Apparatus.SetParent(owner.transform,false);
                var root=new GameObject("Box pivot",typeof(Rigidbody),typeof(BoxRotationController));root.transform.SetParent(owner.Apparatus,false);
                var rb=root.GetComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;owner.Rotation=root.GetComponent<BoxRotationController>();
                var surfaces=new List<VenomSurfacePatch>();var props=new List<VenomMovableProp>();
                Vector3 spawn=new Vector3(-.17f,-.255f,-.16f),exit=new Vector3(.17f,-.3f,.15f),outward=Vector3.down;
                if(number==2||number==4||number==7){exit=new Vector3(.3f,.05f,0);outward=Vector3.right;}
                if(number==3){exit=new Vector3(0,.12f,.3f);outward=Vector3.forward;}
                if(number==5||number==6||number==10){exit=new Vector3(0,.3f,0);outward=Vector3.up;}
                if(number==9)exit=new Vector3(0,-.3f,-.05f);
                if(number>=4&&number<=9)spawn=new Vector3(-.265f,0,-.10f);
                if(number==6)spawn=new Vector3(0,-.25f,0);
                owner.ApertureRadius=.041f;
                if(number==6)
                {
                    exit=new Vector3(0,Mathf.Sqrt(.3f*.3f-owner.ApertureRadius*owner.ApertureRadius),0);
                    var shell=Sphere(root.transform,.3f,owner.ApertureRadius);surfaces.Add(shell);
                }
                else if(number==8)
                {
                    spawn=new Vector3(-.50f,-.08f,-.10f);exit=new Vector3(.57f,0,0);outward=Vector3.right;
                    var left=Cube(root.transform,new Vector3(-.34f,0,0),.23f,new Vector3(-.11f,0,0),Vector3.right,.021f,surfaces);
                    var right=Cube(root.transform,new Vector3(.34f,0,0),.23f,exit,outward,owner.ApertureRadius,surfaces);
                    // A second actual opening joins the right room to the tube.
                    Object.DestroyImmediate(right[2].gameObject);surfaces.Remove(right[2]);
                    Panel(root.transform,"Receiving mouth",new Vector3(.11f,0,0),Vector3.right,new Vector2(.46f,.46f),glass,true,Vector2.zero,.021f,surfaces);
                    var entrance=left[3];entrance.Slippery=true;entrance.RingGrip=true;entrance.GripRadius=.082f;entrance.Shape.sharedMaterial=slick;
                    left[5].Selectable=true;left[5].InterceptExterior=true;
                    entrance.GetComponent<Renderer>().sharedMaterial=slip;
                    // Top edge of the slippery wall is the departure into the fall.
                    left[5].HasSlipRegion=true;left[5].SlipRegion=new Rect(.17f,-.23f,.07f,.46f);
                    var tubeGo=new GameObject("Transfer tube",typeof(VenomTransferTube));tubeGo.transform.SetParent(root.transform,false);tubeGo.transform.localPosition=new Vector3(-.11f,0,0);tubeGo.transform.localRotation=Quaternion.LookRotation(Vector3.right,Vector3.up);
                    game.Tube=tubeGo.GetComponent<VenomTransferTube>();game.Tube.Entrance=entrance;game.Tube.Length=.22f;game.Tube.Radius=.021f;
                    TubeWall(tubeGo.transform,.22f,.021f);
                    Ring(entrance.transform,Vector2.zero,.077f,.002f,mint);
                }
                else
                {
                    var faces=Cube(root.transform,Vector3.zero,.3f,exit,outward,owner.ApertureRadius,surfaces);
                    faces[1].Selectable=number==3;faces[1].InterceptExterior=number==3;
                    if(number==2)
                    {Panel(root.transform,"Low wall left",new Vector3(-.008f,-.24f,0),Vector3.left,new Vector2(.60f,.12f),glass,false,Vector2.zero,0,surfaces);
                     Panel(root.transform,"Low wall right",new Vector3(.008f,-.24f,0),Vector3.right,new Vector2(.60f,.12f),glass,false,Vector2.zero,0,surfaces);
                     Panel(root.transform,"Low wall top",new Vector3(0,-.18f,0),Vector3.up,new Vector2(.016f,.60f),stone,false,Vector2.zero,0,surfaces);}
                    if(number==4)
                    {faces[3].HasSlipRegion=true;faces[3].SlipRegion=new Rect(-.13f,-.08f,.26f,.11f);Overlay(faces[3],faces[3].SlipRegion,slip);}
                    if(number==5){faces[5].Slippery=true;faces[5].GetComponent<Renderer>().sharedMaterial=slip;faces[5].Shape.sharedMaterial=slick;}
                    if(number==7)
                    {
                        faces[5].Slippery=true;faces[5].GetComponent<Renderer>().sharedMaterial=slip;
                        faces[3].Slippery=true;faces[3].RingGrip=true;faces[3].GripRadius=.15f;faces[3].GetComponent<Renderer>().sharedMaterial=slip;
                        Ring(faces[3].transform,faces[3].HoleCentre,.15f,.001f,mint);
                        var prop=Prop(root.transform,"Plastic step",new Vector3(-.04f,-.21f,-.05f),Vector3.one*.18f,true,plastic,surfaces);prop.ProvidesStep=true;props.Add(prop);
                    }
                    if(number==9)
                    {
                        var cup=new GameObject("Loose transparent cover",typeof(Rigidbody),typeof(VenomMovableProp));cup.transform.SetParent(root.transform,false);cup.transform.localPosition=exit+Vector3.up*.05f;
                        var body=cup.GetComponent<Rigidbody>();body.mass=.045f;body.useGravity=true;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
                        var prop=cup.GetComponent<VenomMovableProp>();prop.Body=body;props.Add(prop);
                        DynamicFace(cup.transform,"Cover top",Vector3.up*.05f,Vector3.up,new Vector2(.15f,.15f),glass,surfaces);
                        foreach(var n in new[]{Vector3.left,Vector3.right,Vector3.forward,Vector3.back})DynamicFace(cup.transform,"Cover side",n*.075f,n,new Vector2(.15f,.10f),glass,surfaces);
                    }
                    if(number==10)Boss(game,root.transform,surfaces,props);
                }
                owner.Spawn=new GameObject("Spawn").transform;owner.Spawn.SetParent(root.transform,false);owner.Spawn.localPosition=spawn;
                owner.Outlet=new GameObject("Final exit").transform;owner.Outlet.SetParent(root.transform,false);owner.Outlet.localPosition=exit;owner.Outlet.localRotation=Quaternion.LookRotation(outward,Mathf.Abs(outward.y)>.9f?Vector3.forward:Vector3.up);
                Ring(owner.Outlet,Vector2.zero,owner.ApertureRadius,.0013f,mint);
                game.Surfaces=surfaces.ToArray();game.Props=props.ToArray();owner.CrawlFaces=new Collider[6];for(int i=0;i<6;i++)owner.CrawlFaces[i]=surfaces[0].Shape;
                // Every movable body stays world-space below the ownership object.
                foreach(var p in props)p.transform.SetParent(owner.Apparatus,true);
                if(game.Knife!=null)game.Knife.transform.SetParent(owner.Apparatus,true);
                if(game.ButtonCover!=null)game.ButtonCover.transform.SetParent(owner.Apparatus,true);
                if(game.ExitCover!=null)game.ExitCover.transform.SetParent(owner.Apparatus,true);
                var cam=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();cam.transform.SetParent(owner.transform,false);cam.tag="MainCamera";
                cam.orthographic=true;cam.orthographicSize=1;cam.nearClipPlane=.005f;cam.farClipPlane=10;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.016f,.031f,.030f);owner.View=cam;
                cam.transform.rotation=Quaternion.Euler(data.CameraEuler);cam.transform.position=-cam.transform.forward*2;
                Lighting();string path=Folder+"/VenomOrigin"+number.ToString("00")+".unity";EditorSceneManager.SaveScene(scene,path);scenePaths.Add(new EditorBuildSettingsScene(path,true));
            }
            EditorBuildSettings.scenes=scenePaths.ToArray();AssetDatabase.SaveAssets();Debug.Log("ORIGIN GENERATED: ten authored levels.");
        }
        private static T Asset<T>(string name,Func<T> make) where T:Object
        {string path=Folder+"/"+name;var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a==null){a=make();AssetDatabase.CreateAsset(a,path);}return a;}
        private static Material Mat(string name,Color c,bool transparent,float metallic,float smooth)
        {
            var m=Asset<Material>(name+".mat",()=>new Material(Shader.Find("Universal Render Pipeline/Lit")));m.SetColor("_BaseColor",c);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Cull",0);
            m.SetFloat("_BlendModePreserveSpecular",0);m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            if(transparent){m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",(int)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;}
            else{m.SetFloat("_Surface",0);m.SetFloat("_SrcBlend",1);m.SetFloat("_DstBlend",0);m.SetFloat("_ZWrite",1);m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=2000;}
            EditorUtility.SetDirty(m);return m;
        }
        private static Mesh Save(Mesh mesh){string path=Folder+"/Meshes/Geometry"+(meshSerial++)+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old!=null){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
        private static VenomSurfacePatch Panel(Transform root,string name,Vector3 position,Vector3 normal,Vector2 size,Material mat,bool hole,Vector2 centre,float radius,List<VenomSurfacePatch> list)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider),typeof(VenomSurfacePatch));go.transform.SetParent(root,false);go.transform.localPosition=position;go.transform.localRotation=Quaternion.LookRotation(normal,Mathf.Abs(normal.y)>.9f?Vector3.forward:Vector3.up);
            var mesh=Save(PanelMesh(size,hole,centre,radius));go.GetComponent<MeshFilter>().sharedMesh=mat.GetFloat("_Surface")>.5f?Save(PanelMesh(size,hole,centre,radius,false)):mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;
            var shape=go.GetComponent<MeshCollider>();shape.sharedMesh=mesh;shape.sharedMaterial=contact;shape.contactOffset=.0003f;
            var s=go.GetComponent<VenomSurfacePatch>();s.Size=size;s.Hole=hole;s.HoleCentre=centre;s.HoleRadius=radius;s.Shape=shape;list.Add(s);return s;
        }
        private static VenomSurfacePatch[] Cube(Transform root,Vector3 c,float half,Vector3 hole,Vector3 outward,float radius,List<VenomSurfacePatch> list)
        {
            var normals=new[]{Vector3.up,Vector3.forward,Vector3.right,Vector3.left,Vector3.back,Vector3.down};var result=new VenomSurfacePatch[6];
            for(int i=0;i<6;i++)
            {
                Vector3 n=normals[i],p=c-n*half;bool open=Vector3.Dot(n,-outward)>.9f;
                Quaternion q=Quaternion.LookRotation(n,Mathf.Abs(n.y)>.9f?Vector3.forward:Vector3.up);Vector3 hp=Quaternion.Inverse(q)*(hole-p);
                result[i]=Panel(root,"Glass face "+i,p,n,Vector2.one*(half*2),i==0?stone:glass,open,new Vector2(hp.x,hp.y),radius,list);
                result[i].Selectable=i!=1&&i!=5;
            }
            return result;
        }
        private static Mesh PanelMesh(Vector2 size,bool hole,Vector2 centre,float radius,bool solid=true)
        {
            if(!hole)centre=Vector2.zero;
            var v=new List<Vector3>();var t=new List<int>();
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int k=v.Count;v.Add(a);v.Add(b);v.Add(c);v.Add(d);t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
            if(!hole)
            {
                Vector3 a=new Vector3(-size.x*.5f,-size.y*.5f,0),b=new Vector3(size.x*.5f,-size.y*.5f,0),c=new Vector3(size.x*.5f,size.y*.5f,0),d=new Vector3(-size.x*.5f,size.y*.5f,0),z=Vector3.back*.008f;
                Quad(a,b,c,d);if(solid){Quad(a+z,d+z,c+z,b+z);Quad(a,a+z,b+z,b);Quad(b,b+z,c+z,c);Quad(c,c+z,d+z,d);Quad(d,d+z,a+z,a);}
                var box=new Mesh{name="Flat panel"};box.SetVertices(v);box.SetTriangles(t,0);box.RecalculateNormals();box.RecalculateBounds();return box;
            }
            var angles=new List<float>();for(int i=0;i<64;i++)angles.Add(i*Mathf.PI*2/64);
            foreach(float x in new[]{-size.x*.5f,size.x*.5f})foreach(float y in new[]{-size.y*.5f,size.y*.5f})angles.Add(Mathf.Repeat(Mathf.Atan2(y-centre.y,x-centre.x),Mathf.PI*2));angles.Sort();
            for(int i=0;i<angles.Count;i++)
            {
                Vector2 Point(float a,bool inside)
                {Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));if(inside)return centre+d*(hole?radius:0);
                 float dx=Mathf.Abs(d.x)<.00001f?float.PositiveInfinity:(d.x>0?size.x*.5f-centre.x:-size.x*.5f-centre.x)/d.x;
                 float dy=Mathf.Abs(d.y)<.00001f?float.PositiveInfinity:(d.y>0?size.y*.5f-centre.y:-size.y*.5f-centre.y)/d.y;
                 return centre+d*Mathf.Min(dx,dy);}
                Vector3 a=Point(angles[i],true),b=Point(angles[(i+1)%angles.Count],true),c=Point(angles[(i+1)%angles.Count],false),d=Point(angles[i],false),z=Vector3.back*.008f;
                Quad(a,d,c,b);if(solid){Quad(a+z,b+z,c+z,d+z);Quad(d,d+z,c+z,c);if(hole)Quad(a,b,b+z,a+z);}
            }
            var m=new Mesh{name="Surface with real aperture"};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        private static void Ring(Transform parent,Vector2 centre,float radius,float width,Material material)
        {
            var go=new GameObject("Fine rim",typeof(LineRenderer));go.transform.SetParent(parent,false);var l=go.GetComponent<LineRenderer>();l.sharedMaterial=material;l.useWorldSpace=false;l.startWidth=l.endWidth=width;l.positionCount=65;
            for(int i=0;i<=64;i++){float a=i*Mathf.PI*2/64;l.SetPosition(i,new Vector3(centre.x+Mathf.Cos(a)*radius,centre.y+Mathf.Sin(a)*radius,.001f));}
        }
        private static void Overlay(VenomSurfacePatch patch,Rect rect,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name="Visible slippery coating";go.transform.SetParent(patch.transform,false);go.transform.localPosition=new Vector3(rect.center.x,rect.center.y,.0008f);go.transform.localScale=new Vector3(rect.width,rect.height,1);go.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(go.GetComponent<Collider>());
        }
        private static VenomMovableProp Prop(Transform root,string name,Vector3 p,Vector3 size,bool movable,Material material,List<VenomSurfacePatch> list)
        {
            var go=new GameObject(name,typeof(Rigidbody),typeof(VenomMovableProp));go.transform.SetParent(root,false);go.transform.localPosition=p;
            var rb=go.GetComponent<Rigidbody>();rb.mass=.18f;rb.useGravity=false;rb.interpolation=RigidbodyInterpolation.None;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;rb.solverIterations=20;rb.solverVelocityIterations=8;
            // A weighted plastic base resists tipping; rotation remains physical.
            rb.centerOfMass=Vector3.down*size.y*.22f;
            var prop=go.GetComponent<VenomMovableProp>();prop.Body=rb;prop.Manipulable=movable;
            foreach(var n in new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back})
            {float d=Mathf.Abs(n.x)*size.x+Mathf.Abs(n.y)*size.y+Mathf.Abs(n.z)*size.z;Vector2 s=Mathf.Abs(n.y)>.9f?new Vector2(size.x,size.z):Mathf.Abs(n.x)>.9f?new Vector2(size.z,size.y):new Vector2(size.x,size.y);var face=DynamicFace(go.transform,name+" surface",n*d*.5f,n,s,material,list);face.Shape.sharedMaterial=stepContact;}
            return prop;
        }
        private static VenomSurfacePatch DynamicFace(Transform root,string name,Vector3 p,Vector3 n,Vector2 size,Material material,List<VenomSurfacePatch> list)
        {
            var patch=Panel(root,name,p,n,size,material,false,Vector2.zero,0,list);Object.DestroyImmediate(patch.Shape);
            var box=patch.gameObject.AddComponent<BoxCollider>();box.center=Vector3.back*.004f;box.size=new Vector3(size.x,size.y,.008f);box.sharedMaterial=contact;box.contactOffset=.0003f;patch.Shape=box;return patch;
        }
        private static void TubeWall(Transform root,float length,float radius)
        {
            const int segments=64;var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<=segments;i++){float a=i*Mathf.PI*2/segments;v.Add(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));v.Add(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,length));}
            for(int i=0;i<segments;i++){int k=i*2;t.AddRange(new[]{k,k+1,k+2,k+2,k+1,k+3});}
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh);
            var go=new GameObject("Glass pipe bore",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=glass;go.GetComponent<MeshCollider>().sharedMesh=mesh;
        }
        private static VenomSurfacePatch Sphere(Transform root,float r,float aperture)
        {
            const int rings=40,sectors=80;float start=Mathf.Asin(aperture/r);var v=new List<Vector3>();var t=new List<int>();
            for(int j=0;j<=rings;j++)for(int i=0;i<=sectors;i++)
            {float a=i*Mathf.PI*2/sectors,b=Mathf.Lerp(start,Mathf.PI,j/(float)rings);v.Add(new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a))*r);}
            for(int j=0;j<rings;j++)for(int i=0;i<sectors;i++){int k=j*(sectors+1)+i;t.AddRange(new[]{k,k+sectors+1,k+1,k+1,k+sectors+1,k+sectors+2});}
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh);
            var go=new GameObject("Slippery sphere — real opening",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider),typeof(VenomSurfacePatch));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=slip;
            var mc=go.GetComponent<MeshCollider>();mc.sharedMesh=mesh;mc.sharedMaterial=slick;
            var patch=go.GetComponent<VenomSurfacePatch>();patch.Shape=mc;patch.Size=Vector2.zero;patch.Slippery=true;patch.Selectable=false;return patch;
        }
        private static void Boss(VenomCampaign game,Transform root,List<VenomSurfacePatch> surfaces,List<VenomMovableProp> props)
        {
            foreach(float side in new[]{-1f,1f})
            {
                var p=Panel(root,side<0?"A":"B",new Vector3(side*.135f,-.296f,.13f),Vector3.up,new Vector2(.086f,.086f),mint,false,Vector2.zero,0,surfaces);
                if(side<0)game.PadA=p.transform;else game.PadB=p.transform;
            }
            Rigidbody Cover(string name,Vector3 p,Vector3 size)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=plastic;var rb=go.AddComponent<Rigidbody>();rb.isKinematic=true;return rb;
            }
            game.ButtonCover=Cover("B cover",new Vector3(.135f,-.272f,.13f),new Vector3(.108f,.05f,.108f));
            game.ExitCover=Cover("Latched exit shutter",new Vector3(0,.304f,0),new Vector3(.105f,.009f,.105f));
            game.Knife=Cover("Gravity blade",new Vector3(0,-.13f,-.13f),new Vector3(.006f,.12f,.13f));game.Knife.isKinematic=false;game.Knife.useGravity=false;game.Knife.mass=.045f;game.Knife.GetComponent<Renderer>().sharedMaterial=metal;
            var collider=game.Knife.GetComponent<BoxCollider>();collider.size=new Vector3(1,.70f,1);collider.center=new Vector3(0,.15f,0);
            game.Knife.transform.localScale=Vector3.one;collider.size=new Vector3(.006f,.084f,.13f);collider.center=new Vector3(0,.018f,0);
            // Mesh alone is scaled; joint has metres and an unscaled local frame.
            var mesh=game.Knife.GetComponent<MeshFilter>();var bladeMesh=Object.Instantiate(mesh.sharedMesh);var vv=bladeMesh.vertices;for(int i=0;i<vv.Length;i++)vv[i]=Vector3.Scale(vv[i],new Vector3(.006f,.12f,.13f));bladeMesh.vertices=vv;bladeMesh.RecalculateBounds();mesh.sharedMesh=Save(bladeMesh);
            var joint=game.Knife.gameObject.AddComponent<ConfigurableJoint>();joint.connectedBody=root.GetComponent<Rigidbody>();joint.autoConfigureConnectedAnchor=false;joint.connectedAnchor=new Vector3(0,-.18f,-.13f);joint.axis=Vector3.right;joint.secondaryAxis=Vector3.up;
            joint.xMotion=joint.zMotion=ConfigurableJointMotion.Locked;joint.yMotion=ConfigurableJointMotion.Limited;joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;joint.linearLimit=new SoftJointLimit{limit=.05f,contactDistance=.006f};joint.enableCollision=true;
        }
        private static void Lighting()
        {
            foreach(var angle in new[]{new Vector3(40,-35,0),new Vector3(25,150,0)}){var l=new GameObject("Softbox",typeof(Light)).GetComponent<Light>();l.type=LightType.Directional;l.intensity=1.6f;l.transform.rotation=Quaternion.Euler(angle);l.color=new Color(.83f,.94f,1);}
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.55f,.65f,.67f);RenderSettings.ambientEquatorColor=new Color(.3f,.4f,.38f);RenderSettings.ambientGroundColor=new Color(.1f,.16f,.15f);RenderSettings.skybox=null;
            RenderSettings.customReflectionTexture=AssetDatabase.LoadAssetAtPath<Cubemap>("Assets/_Game/PhysicsLab/Materials/Studio reflection.asset");RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;
        }
        public static void BuildMac()
        {
            var scenes=new string[10];for(int i=0;i<10;i++)scenes[i]=Folder+"/VenomOrigin"+(i+1).ToString("00")+".unity";
            Directory.CreateDirectory("Builds/Venom/macOS");string previous=PlayerSettings.productName;
            try{PlayerSettings.productName="Venom";var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,target=BuildTarget.StandaloneOSX,locationPathName="Builds/Venom/macOS/Venom.app",options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Origin build failed: "+report.summary.result);Debug.Log("ORIGIN BUILD SUCCESS");}
            finally{PlayerSettings.productName=previous;}
        }
    }
}
