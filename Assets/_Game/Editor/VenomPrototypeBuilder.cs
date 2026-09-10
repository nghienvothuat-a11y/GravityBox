using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Simulation;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    public static class VenomPrototypeBuilder
    {
        public const string Folder = "Assets/_Game/Venom";
        public const string ScenePath = Folder + "/Venom01.unity";
        private static PhysicsMaterial contact;
        private static Material floor, glass, frame, glow, bladeMaterial;
        [MenuItem("Gravity Box/Venom/Generate Experiment 01")]
        public static void Generate()
        {
            Directory.CreateDirectory(Folder + "/Materials"); Directory.CreateDirectory(Folder + "/Meshes"); AssetDatabase.Refresh();
            contact=Asset<PhysicsMaterial>(Folder+"/Materials/Soft contact.physicMaterial",()=>new PhysicsMaterial("Soft contact"));
            contact.dynamicFriction=.12f;contact.staticFriction=.18f;contact.bounciness=0;
            contact.frictionCombine=PhysicsMaterialCombine.Minimum;contact.bounceCombine=PhysicsMaterialCombine.Minimum;
            floor=Material("Porcelain chamber",new Color(.30f,.37f,.36f),.18f,.38f);
            Transparent(floor);floor.SetFloat("_Cull",(int)CullMode.Back);
            frame=Material("Graphite edges",new Color(.05f,.075f,.074f),.6f,.6f);
            glow=Material("Mint inlay",new Color(.24f,.82f,.64f),.25f,.65f); glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(.15f,.65f,.44f));
            bladeMaterial=Material("Surgical blade",new Color(.4f,.46f,.48f),.85f,.7f);
            glass=Material("Observation glass",new Color(.33f,.6f,.58f,.07f),.05f,.2f);
            Transparent(glass);glass.SetFloat("_EnvironmentReflections",0);glass.SetFloat("_SpecularHighlights",0);
            glass.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");glass.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            var profile=Asset<VenomProfile>(Folder+"/Living matter.asset",()=>ScriptableObject.CreateInstance<VenomProfile>());
            profile.Stiffness=2.6f;profile.Viscosity=.009f;
            profile.Contact=contact;profile.Skin=Material("Obsidian symbiote",new Color(.012f,.022f,.026f),.52f,.91f);
            profile.Skin.EnableKeyword("_EMISSION");profile.Skin.SetColor("_EmissionColor",Color.black);
            EditorUtility.SetDirty(profile);EditorUtility.SetDirty(contact);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var owner=new GameObject("Venom 01 — living matter laboratory").AddComponent<VenomLevelController>();
            owner.MatterProfile=profile;owner.RotationProfile=AssetDatabase.LoadAssetAtPath<RotationSettings>(PhysicsLabBuilder.Folder+"/Profiles/Hand rotation.asset");
            var apparatus=new GameObject("Apparatus ownership");apparatus.transform.SetParent(owner.transform,false);owner.Apparatus=apparatus.transform;
            var root=new GameObject("Hand-held chamber pivot",typeof(Rigidbody),typeof(BoxRotationController));root.transform.SetParent(apparatus.transform,false);
            var rigid=root.GetComponent<Rigidbody>();rigid.isKinematic=true;rigid.useGravity=false;owner.Rotation=root.GetComponent<BoxRotationController>();
            Vector2 hole=new Vector2(0,-.235f); const float floorY=-.07f, roofY=.075f;
            var floorGo=new GameObject("Floor — real circular bore",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));floorGo.transform.SetParent(root.transform,false);
            var floorMesh=FloorMesh(hole,.035f,floorY,.003f);floorGo.GetComponent<MeshFilter>().sharedMesh=floorMesh;
            floorGo.GetComponent<MeshRenderer>().sharedMaterial=floor;floorGo.GetComponent<MeshCollider>().sharedMesh=floorMesh;floorGo.GetComponent<MeshCollider>().sharedMaterial=contact;
            Block("Left glass",root.transform,new Vector3(-.254f,0,0),new Vector3(.008f,.15f,.648f),glass);
            Block("Right glass",root.transform,new Vector3(.254f,0,0),new Vector3(.008f,.15f,.648f),glass);
            Block("Far glass",root.transform,new Vector3(0,0,.324f),new Vector3(.5f,.15f,.008f),glass);
            Block("Near glass",root.transform,new Vector3(0,0,-.324f),new Vector3(.5f,.15f,.008f),glass);
            // A narrow mechanical slot admits the moving gate; it is smaller than a matter particle.
            Block("Upper observation cover",root.transform,new Vector3(0,roofY,.126f),new Vector3(.5f,.004f,.388f),glass);
            Block("Lower observation cover",root.transform,new Vector3(0,roofY,-.207f),new Vector3(.5f,.004f,.226f),glass);
            foreach(float x in new[]{-.25f,.25f})Block("Side frame",root.transform,new Vector3(x,floorY,0),new Vector3(.006f,.006f,.646f),frame,false);
            foreach(float z in new[]{-.32f,.32f})Block("End frame",root.transform,new Vector3(0,floorY,z),new Vector3(.506f,.006f,.006f),frame,false);
            // The divider prevents cut faces from rejoining until both pressure stations are solved.
            Block("Separation spine",root.transform,new Vector3(0,.003f,.012f),new Vector3(.007f,.139f,.19f),glass);
            Block("Spine light",root.transform,new Vector3(0,-.064f,.012f),new Vector3(.002f,.001f,.19f),glow,false);
            // Broad angled cheeks guide the two deformable fragments onto the independent stations.
            var left=Block("Left flow guide",root.transform,new Vector3(-.175f,-.01f,.08f),new Vector3(.014f,.115f,.19f),frame);left.transform.localRotation=Quaternion.Euler(0,-24,0);
            var right=Block("Right flow guide",root.transform,new Vector3(.175f,-.01f,.08f),new Vector3(.014f,.115f,.19f),frame);right.transform.localRotation=Quaternion.Euler(0,24,0);
            owner.Spawn=new GameObject("Organism spawn").transform;owner.Spawn.SetParent(root.transform,false);owner.Spawn.localPosition=new Vector3(0,-.02f,.252f);
            owner.Outlet=new GameObject("Round exit — whole material traversal").transform;owner.Outlet.SetParent(root.transform,false);
            owner.Outlet.localPosition=new Vector3(hole.x,floorY,hole.y);owner.Outlet.localRotation=Quaternion.LookRotation(Vector3.down,Vector3.forward);
            Ring(root.transform,new Vector3(hole.x,floorY+.0031f,hole.y),.035f,.0012f);
            owner.BladeRest=new Vector3(0,.055f,.145f);
            owner.Blade=Slider("Gravity guillotine",root.transform,owner.BladeRest,new Vector3(.006f,.12f,.11f),bladeMaterial,.04f,Vector3.up,.065f);
            // Starts lifted. Gravity brings the actual blade down and the solid edge cuts bonds on contact.
            Block("Blade glowing edge",owner.Blade.transform,new Vector3(0,-.057f,0),new Vector3(.007f,.002f,.11f),glow,false);
            owner.GateRest=new Vector3(0,.002f,-.083f);
            owner.Gate=Slider("Contact-retained outlet gate",root.transform,owner.GateRest,new Vector3(.499f,.138f,.012f),glass,.05f,Vector3.up,.15f);
            Block("Gate top frame",owner.Gate.transform,new Vector3(0,.062f,0),new Vector3(.499f,.012f,.013f),frame,false);
            Block("Gate band",owner.Gate.transform,new Vector3(0,.056f,-.007f),new Vector3(.46f,.003f,.002f),glow,false);
            owner.LeftPad=Pad(root.transform,new Vector3(-.083f,-.065f,-.033f),"A");
            owner.RightPad=Pad(root.transform,new Vector3(.083f,-.065f,-.033f),"B");
            // A narrowing reunion chamber lets shared tilt press the two fragments
            // together using wall contact, even when the blade cut unequal volumes.
            foreach(float side in new[]{-1f,1f})
            {
                var guide=Block("Reunion funnel",root.transform,new Vector3(side*.083f,-.004f,-.145f),new Vector3(.008f,.132f,.148f),glass);
                guide.transform.localRotation=Quaternion.Euler(0,side*45,0);
                var edge=Block("Reunion guide inlay",root.transform,new Vector3(side*.083f,-.063f,-.145f),new Vector3(.002f,.001f,.148f),glow,false);
                edge.transform.localRotation=guide.transform.localRotation;
            }
            var camera=new GameObject("Venom observation camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.tag="MainCamera";
            camera.orthographic=true;camera.orthographicSize=.78f;camera.nearClipPlane=.005f;camera.farClipPlane=10;
            camera.transform.position=new Vector3(.36f,.99f,-.80f);camera.transform.LookAt(new Vector3(0,0,.01f));
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.017f,.029f,.030f);owner.View=camera;
            var visibility=owner.gameObject.AddComponent<VenomVisibility>();visibility.Floor=floorGo.GetComponent<Renderer>();visibility.View=camera;
            Light("Soft white reflection",new Vector3(48,-25,0),new Color(.88f,.95f,1),2.2f);
            Light("Warm rim",new Vector3(25,145,0),new Color(1,.8f,.62f),1.6f);
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.52f,.64f,.67f);
            RenderSettings.ambientEquatorColor=new Color(.3f,.38f,.4f);RenderSettings.ambientGroundColor=new Color(.08f,.13f,.15f);
            RenderSettings.skybox=null;RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture=AssetDatabase.LoadAssetAtPath<Cubemap>(PhysicsLabBuilder.Folder+"/Materials/Studio reflection.asset");RenderSettings.reflectionIntensity=1.3f;
            EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();Debug.Log("VENOM GENERATED: one cohesive organism, gravity blade, two contact plates, physical gate and round exit.");
        }
        private static T Asset<T>(string path,Func<T> create) where T:Object
        { var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a==null){a=create();AssetDatabase.CreateAsset(a,path);}return a; }
        private static void Transparent(Material material)
        {
            material.SetFloat("_Surface",1);material.SetFloat("_Blend",0);material.SetFloat("_BlendModePreserveSpecular",0);
            material.SetFloat("_ZWrite",0);material.SetFloat("_SrcBlend",(int)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha",(int)BlendMode.One);material.SetFloat("_DstBlendAlpha",(int)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetOverrideTag("RenderType","Transparent");material.SetShaderPassEnabled("ShadowCaster",false);material.SetShaderPassEnabled("DepthOnly",false);material.renderQueue=3000;
        }
        private static Material Material(string name,Color color,float metal,float smooth)
        {
            var m=Asset<Material>(Folder+"/Materials/"+name+".mat",()=>new Material(Shader.Find("Universal Render Pipeline/Lit")));
            m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);return m;
        }
        private static GameObject Block(string name,Transform parent,Vector3 p,Vector3 size,Material m,bool collide=true)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.transform.localPosition=p;
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var source=primitive.GetComponent<MeshFilter>().sharedMesh;
            go.GetComponent<MeshFilter>().sharedMesh=source;go.GetComponent<MeshRenderer>().sharedMaterial=m;
            go.transform.localScale=size;Object.DestroyImmediate(primitive);
            if(collide){var c=go.AddComponent<BoxCollider>();c.sharedMaterial=contact;c.contactOffset=.0003f;}
            if(m==glass)go.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            return go;
        }
        private static Rigidbody Slider(string name,Transform parent,Vector3 p,Vector3 size,Material m,float mass,Vector3 axis,float travel)
        {
            var go=new GameObject(name,typeof(Rigidbody),typeof(ConfigurableJoint));go.transform.SetParent(parent,false);go.transform.localPosition=p;
            Block("Physical surface",go.transform,Vector3.zero,size,m);
            var rb=go.GetComponent<Rigidbody>();rb.mass=mass;rb.useGravity=false;rb.interpolation=RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            rb.solverIterations=32;rb.solverVelocityIterations=12;rb.maxDepenetrationVelocity=.5f;
            var joint=go.GetComponent<ConfigurableJoint>();joint.connectedBody=parent.GetComponent<Rigidbody>();joint.autoConfigureConnectedAnchor=false;
            joint.connectedAnchor=p;joint.axis=axis;joint.secondaryAxis=Vector3.forward;
            joint.xMotion=ConfigurableJointMotion.Limited;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;
            joint.linearLimit=new SoftJointLimit{limit=travel,bounciness=0,contactDistance=.0001f};joint.enableCollision=false;
            return rb;
        }
        private static VenomPressurePlate Pad(Transform parent,Vector3 position,string name)
        {
            var rb=Slider("Pressure station "+name,parent,position,new Vector3(.153f,.004f,.073f),glow,.008f,Vector3.up,.003f);
            var pad=rb.gameObject.AddComponent<VenomPressurePlate>();pad.Body=rb;pad.RestLocal=position;pad.Light=rb.GetComponentInChildren<Renderer>();return pad;
        }
        private static void Light(string name,Vector3 rotation,Color color,float intensity)
        { var l=new GameObject(name,typeof(Light)).GetComponent<Light>();l.type=LightType.Directional;l.transform.rotation=Quaternion.Euler(rotation);l.color=color;l.intensity=intensity; }
        private static void Ring(Transform root,Vector3 p,float radius,float width)
        {
            var go=new GameObject("Faint flush exit inlay",typeof(LineRenderer));go.transform.SetParent(root,false);go.transform.localPosition=p;
            var line=go.GetComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=96;line.widthMultiplier=width;line.sharedMaterial=glow;
            for(int i=0;i<96;i++)line.SetPosition(i,new Vector3(Mathf.Cos(i*Mathf.PI/48)*radius,0,Mathf.Sin(i*Mathf.PI/48)*radius));
        }
        private static Mesh FloorMesh(Vector2 hole,float radius,float y,float half)
        {
            var v=new List<Vector3>();var t=new List<int>();var angles=new List<float>();
            for(int i=0;i<96;i++)angles.Add(i*Mathf.PI/48);
            foreach(float x in new[]{-.25f,.25f})foreach(float z in new[]{-.32f,.32f})angles.Add(Mathf.Repeat(Mathf.Atan2(z-hole.y,x-hole.x),2*Mathf.PI));angles.Sort();
            Vector2 Outer(float a)
            {var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));float tx=Mathf.Abs(d.x)<.00001f?float.PositiveInfinity:((d.x>0?.25f:-.25f)-hole.x)/d.x;float tz=Mathf.Abs(d.y)<.00001f?float.PositiveInfinity:((d.y>0?.32f:-.32f)-hole.y)/d.y;return hole+d*Mathf.Min(tx,tz);}
            Vector3 P(Vector2 p,float height)=>new Vector3(p.x,height,p.y);
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=v.Count;v.AddRange(new[]{a,b,c,d});t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
            for(int i=0;i<angles.Count;i++)
            {
                float a=angles[i],b=angles[(i+1)%angles.Count];Vector2 oa=Outer(a),ob=Outer(b),ia=hole+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,ib=hole+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius;
                Quad(P(ob,y+half),P(oa,y+half),P(ia,y+half),P(ib,y+half));Quad(P(oa,y-half),P(ob,y-half),P(ib,y-half),P(ia,y-half));
                Quad(P(ia,y+half),P(ia,y-half),P(ib,y-half),P(ib,y+half));Quad(P(oa,y-half),P(oa,y+half),P(ob,y+half),P(ob,y-half));
            }
            var mesh=Asset<Mesh>(Folder+"/Meshes/Chamber floor.asset",()=>new Mesh());mesh.Clear();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);return mesh;
        }
        [MenuItem("Gravity Box/Venom/Build macOS")]
        public static void BuildMac()
        {
            const string path="Builds/Venom/macOS/Venom.app";Directory.CreateDirectory(Path.GetDirectoryName(path));
            string previous=PlayerSettings.productName;
            try
            {
                PlayerSettings.productName="Venom";
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},target=BuildTarget.StandaloneOSX,locationPathName=path,options=BuildOptions.Development});
                if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Venom build failed: "+report.summary.result);
                Debug.Log("VENOM BUILD SUCCESS: "+path+" ("+report.summary.totalSize+" bytes)");
            }
            finally{PlayerSettings.productName=previous;}
        }
    }
}
