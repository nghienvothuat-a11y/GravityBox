using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    internal static class CooperativeBoxBuilder
    {
        public static void Build(LevelRuntime level, PhysicsMaterial contact)
        {
            Transform root = level.transform;
            Material cyan = Material("Relay cyan", new Color(.18f,.75f,.82f), .55f);
            Material amber = Material("Relay amber", new Color(.94f,.59f,.19f), .55f);
            Material steel = Material("Relay housing", new Color(.20f,.27f,.30f), .7f);
            Material floor = Material("Relay floor", new Color(.115f,.16f,.19f), .25f);
            string path = PhysicsLabBuilder.Folder + "/Materials/Relay inspection glass.mat";
            Material glass = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (glass == null) { glass = new Material(Shader.Find("GravityBox/Inspection Glass")); AssetDatabase.CreateAsset(glass,path); }
            root.Find("Floor with circular cut").GetComponent<Renderer>().sharedMaterial = floor;
            root.Find("Clear top cover").GetComponent<Renderer>().sharedMaterial = glass;
            root.Find("Clear side walls").GetComponent<Renderer>().sharedMaterial = glass;
            var spawn = new GameObject("BallSpawn B"); spawn.transform.SetParent(root,false);
            spawn.transform.localPosition = new Vector3(.07f,-.024f,.25f);
            level.AdditionalBallSpawns = new[] { spawn.transform };

            // Two entrance chambers can only meet in the exit court below both gates.
            Wall("Central divider", -.005f,.005f,-.215f,.32f);
            Partition("Left", -.32f,-.005f,-.20f);
            Partition("Right", .005f,.32f,.22f);
            Cup("Hold A", -.20f,.105f,cyan);
            Cup("Release B", .10f,-.180f,amber);

            var relay = level.gameObject.AddComponent<CooperativeRelay>();
            relay.LeftGate = Slider("Amber return gate",new Vector3(-.20f,0,-.025f),Vector3.right,.095f,
                new Vector3(.068f,.082f,.012f),.05f,amber,.072f);
            relay.RightGate = Slider("Cyan crossing gate",new Vector3(.22f,0,-.025f),Vector3.left,.095f,
                new Vector3(.068f,.082f,.012f),.05f,cyan,.072f);
            relay.HoldSwitch = Plunger("A hold plunger",-.20f,.105f,cyan);
            relay.ReleaseSwitch = Plunger("B retaining plunger",.10f,-.180f,amber);
            // Colour-coded link traces communicate the crossed control relationship.
            Trace("A to crossing gate",new[]{new Vector2(-.20f,.095f),new Vector2(-.075f,.095f),new Vector2(-.075f,.04f),new Vector2(.22f,.04f)},cyan);
            Trace("B to return gate",new[]{new Vector2(.10f,-.19f),new Vector2(.04f,-.19f),new Vector2(.04f,-.09f),new Vector2(-.20f,-.09f)},amber);
            Mark("Hold pocket",new Vector3(-.20f,-.0415f,.137f),new Vector3(.074f,.0004f,.049f),cyan);
            Mark("Latch pocket",new Vector3(.10f,-.0415f,-.148f),new Vector3(.074f,.0004f,.049f),amber);
            Label("A · HOLD",new Vector3(-.20f,-.0408f,.22f),cyan);
            Label("B · LATCH",new Vector3(.12f,-.0408f,-.102f),amber);
            Label("BOTH OUT",new Vector3(0,-.0408f,-.236f),cyan);

            void Partition(string name,float left,float right,float x)
            { Wall(name+" gate outer housing",left,x-.034f,-.031f,-.019f); Wall(name+" gate inner housing",x+.034f,right,-.031f,-.019f); }
            void Cup(string name,float x,float z,Material color)
            {
                Wall(name+" left cheek",x-.055f,x-.045f,z-.026f,z+.07f);
                Wall(name+" right cheek",x+.045f,x+.055f,z-.026f,z+.07f);
                Wall(name+" spring housing",x-.045f,x+.045f,z-.026f,z-.015f);
                Mark(name+" housing cap",new Vector3(x,.042f,z-.021f),new Vector3(.09f,.001f,.012f),color);
            }
            PressurePlunger Plunger(string name,float x,float z,Material material)
            {
                GravitySliderGuide guide = Slider(name,new Vector3(x,0,z),Vector3.back,.008f,new Vector3(.086f,.08f,.012f),.02f,material,.003f);
                string facePath = PhysicsLabBuilder.Folder + "/Materials/" + name + " glass face.mat";
                Material face = AssetDatabase.LoadAssetAtPath<Material>(facePath);
                if (face == null) { face = new Material(Shader.Find("GravityBox/Inspection Glass")); AssetDatabase.CreateAsset(face, facePath); }
                face.SetColor("_Tint", material.GetColor("_BaseColor")); face.SetFloat("_Opacity", .10f); face.SetFloat("_EdgeOpacity", .24f);
                EditorUtility.SetDirty(face);
                guide.GetComponentInChildren<Renderer>().sharedMaterial = face;
                Mark(name + " upper contact rim", new Vector3(0,.039f,.006f), new Vector3(.086f,.002f,.002f), material, guide.transform);
                var pressure = guide.gameObject.AddComponent<PressurePlunger>(); pressure.Guide = guide; return pressure;
            }
            GravitySliderGuide Slider(string name,Vector3 position,Vector3 axis,float stroke,Vector3 size,float mass,Material material,float clear)
            {
                var go = new GameObject(name,typeof(Rigidbody),typeof(BoxCollider),typeof(PhysicalProp));
                go.transform.SetParent(root,false); go.transform.localPosition=position;
                Rigidbody body=go.GetComponent<Rigidbody>(); body.mass=mass; body.useGravity=false; body.solverIterations=24; body.solverVelocityIterations=12;
                body.sleepThreshold=.00001f; body.maxDepenetrationVelocity=1;
                var collider=go.GetComponent<BoxCollider>();collider.size=size;collider.sharedMaterial=contact;collider.contactOffset=.0005f;
                Mark(name+" metal",Vector3.zero,size,material,go.transform);
                var joint=go.AddComponent<ConfigurableJoint>(); joint.autoConfigureConnectedAnchor=false;
                joint.connectedBody=root.GetComponent<Rigidbody>();joint.connectedAnchor=position+axis*stroke*.5f;
                joint.axis=axis;joint.secondaryAxis=Vector3.up;
                joint.xMotion=ConfigurableJointMotion.Limited;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
                joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;
                joint.linearLimit=new SoftJointLimit{limit=stroke*.5f,contactDistance=.0002f};
                // The guide is internal to a housing. Balls still collide with the shutter at every point of its stroke.
                joint.enableCollision=false;joint.projectionMode=JointProjectionMode.None;
                var guide=go.AddComponent<GravitySliderGuide>();guide.Configure(joint,position,axis,stroke,clear);return guide;
            }
            void Wall(string name,float x0,float x1,float z0,float z1)
            {
                Vector3 center=new Vector3((x0+x1)*.5f,0,(z0+z1)*.5f),size=new Vector3(x1-x0,.084f,z1-z0);
                GameObject wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name=name;wall.transform.SetParent(root,false);
                wall.transform.localPosition=center;wall.transform.localScale=size;wall.GetComponent<Renderer>().sharedMaterial=glass;
                wall.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
                wall.GetComponent<Collider>().sharedMaterial=contact;wall.GetComponent<Collider>().contactOffset=.0005f;
                Mark(name+" edge",center+Vector3.up*.041f,new Vector3(size.x,.002f,size.z),steel);
            }
            GameObject Mark(string name,Vector3 p,Vector3 size,Material material,Transform parent=null)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent!=null?parent:root,false);
                go.transform.localPosition=p;go.transform.localScale=size;Object.DestroyImmediate(go.GetComponent<Collider>());
                go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;return go;
            }
            void Trace(string name,Vector2[] points,Material material)
            {
                for(int i=1;i<points.Length;i++)
                {
                    Vector2 a=points[i-1],b=points[i];
                    Mark(name+" "+i,new Vector3((a.x+b.x)*.5f,-.0414f,(a.y+b.y)*.5f),
                        new Vector3(Mathf.Max(.002f,Mathf.Abs(a.x-b.x)),.0003f,Mathf.Max(.002f,Mathf.Abs(a.y-b.y))),material);
                }
            }
            void Label(string text,Vector3 p,Material material)
            {
                var go=new GameObject(text,typeof(TextMesh));go.transform.SetParent(root,false);go.transform.localPosition=p;
                go.transform.localRotation=Quaternion.Euler(90,0,0);
                var label=go.GetComponent<TextMesh>();label.text=text;label.fontSize=48;label.characterSize=.009f;
                label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=material.GetColor("_BaseColor");
            }
        }
        private static Material Material(string name,Color color,float metallic)
        {
            string path=PhysicsLabBuilder.Folder+"/Materials/"+name+".mat";
            Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.42f);EditorUtility.SetDirty(m);return m;
        }
    }
}
