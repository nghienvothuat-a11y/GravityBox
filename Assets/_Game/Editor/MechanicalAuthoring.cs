using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    // Shared authoring only: all generated geometry is saved into level prefabs.
    internal sealed class MechanicalAuthoring
    {
        public LevelRuntime Level { get; }
        public Transform Root => Level.transform;
        public Material Glass { get; }
        public Material Frame { get; }
        public Material Rim { get; }
        public PhysicsMaterial Contact { get; }
        public MechanicalAuthoring(LevelRuntime level, Material glass, Material frame, Material rim, PhysicsMaterial contact)
        { Level=level; Glass=glass; Frame=frame; Rim=rim; Contact=contact; }
        public MechanicalAuthoring(string name, Vector3 halfSize, Vector2 exit, Vector3 spawn,
            Material glass, Material frame, Material rim, PhysicsMaterial contact)
        {
            Glass = glass; Frame = frame; Rim = rim; Contact = contact;
            var root = new GameObject(name, typeof(Rigidbody), typeof(BoxRotationController), typeof(LevelRuntime));
            Rigidbody rb = root.GetComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
            Level = root.GetComponent<LevelRuntime>(); Level.Rotation = root.GetComponent<BoxRotationController>();
            Level.InteriorDepth = halfSize.y * 2; Level.BoundsHalfExtent = halfSize.magnitude + .08f;
            Level.Footprint = new[] { new Vector2(-halfSize.x,-halfSize.z), new Vector2(halfSize.x,-halfSize.z),
                new Vector2(halfSize.x,halfSize.z), new Vector2(-halfSize.x,halfSize.z) };
            Material floor = AssetDatabase.LoadAssetAtPath<Material>(PhysicsLabBuilder.Folder + "/Materials/Satin aluminium.mat");
            MeshObject("Floor with circular cut", PhysicsLabGeometry.Panel(name+" floor", Level.Footprint,
                -halfSize.y, .003f, exit, .023f), floor != null ? floor : frame);
            MeshObject("Clear top cover", PhysicsLabGeometry.Panel(name+" cover", Level.Footprint,
                halfSize.y, .003f, Vector2.zero, 0), glass);
            MeshObject("Clear side walls", PhysicsLabGeometry.Border(name+" walls", Level.Footprint,
                .006f, -halfSize.y, halfSize.y), glass);
            MeshObject("Lower machined edge", PhysicsLabGeometry.Border(name+" lower edge", Level.Footprint,
                .007f,-halfSize.y-.003f,-halfSize.y+.001f), frame, false);
            MeshObject("Upper machined edge", PhysicsLabGeometry.Border(name+" upper edge", Level.Footprint,
                .007f,halfSize.y-.001f,halfSize.y+.003f), frame, false);
            var spawnObject = new GameObject("BallSpawn"); spawnObject.transform.SetParent(Root,false);
            spawnObject.transform.localPosition = spawn; Level.BallSpawn = spawnObject.transform;
            var outlet = new GameObject("Flush round exit",typeof(ExitSocket)); outlet.transform.SetParent(Root,false);
            outlet.transform.localPosition = new Vector3(exit.x,-halfSize.y,exit.y);
            outlet.transform.localRotation = Quaternion.LookRotation(Vector3.down,Vector3.forward);
            Level.Exit = outlet.GetComponent<ExitSocket>(); Level.Exit.ApertureRadius = .023f; Level.Exit.WallHalfDepth = .003f;
            MeshObject("Subtle light inlay", PhysicsLabGeometry.Inlay(.023f,.003f),rim,false,outlet.transform);
        }
        public GameObject Block(string name, Vector3 p, Vector3 size, Material material, Transform parent = null, bool collision = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
            go.transform.SetParent(parent != null ? parent : Root,false);
            go.transform.localPosition = p; go.transform.localScale = size;
            var renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = material == Glass ? ShadowCastingMode.Off : ShadowCastingMode.On;
            Collider collider = go.GetComponent<Collider>();
            if (!collision) Object.DestroyImmediate(collider);
            else { collider.sharedMaterial = Contact; collider.contactOffset = .0005f; }
            return go;
        }
        public GameObject MeshObject(string name, Mesh mesh, Material material, bool collision = true, Transform parent = null)
        {
            Mesh saved=mesh;
            if (!AssetDatabase.Contains(mesh))
            {
                string safe = Level.name.Replace('/','-') + " " + name.Replace('/','-') + ".asset";
                string path = GeometryAssetScope.MeshPath(safe.Substring(0, safe.Length - ".asset".Length));
                saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (saved == null) { saved = mesh; AssetDatabase.CreateAsset(saved,path); }
                else { EditorUtility.CopySerialized(mesh,saved); Object.DestroyImmediate(mesh); }
            }
            EditorUtility.SetDirty(saved);
            var go = new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(parent != null ? parent : Root,false);
            go.GetComponent<MeshFilter>().sharedMesh = saved;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.GetComponent<MeshRenderer>().shadowCastingMode = material == Glass ? ShadowCastingMode.Off : ShadowCastingMode.On;
            if(collision) { var collider=go.AddComponent<MeshCollider>(); collider.sharedMesh=saved; collider.sharedMaterial=Contact; collider.contactOffset=.0005f; }
            return go;
        }
        public PhysicalHinge Hinge(string name, Vector3 anchor, Vector3 axis, float mass,
            float minDegrees, float maxDegrees, float damping = .00002f)
        {
            var go = new GameObject(name,typeof(Rigidbody),typeof(PhysicalProp),typeof(HingeJoint));
            go.transform.SetParent(Root,false); go.transform.localPosition=anchor;
            Rigidbody body=go.GetComponent<Rigidbody>(); body.mass=mass; body.useGravity=false;
            body.solverIterations=32; body.solverVelocityIterations=12; body.maxDepenetrationVelocity=1;
            HingeJoint joint=go.GetComponent<HingeJoint>(); joint.autoConfigureConnectedAnchor=false;
            joint.connectedBody=Root.GetComponent<Rigidbody>(); joint.anchor=Vector3.zero; joint.connectedAnchor=anchor;
            joint.axis=axis.normalized; joint.useMotor=false; joint.useSpring=false; joint.enableCollision=true;
            joint.limits=new JointLimits{min=minDegrees,max=maxDegrees,bounciness=0,contactDistance=.05f}; joint.useLimits=true;
            var guide=go.AddComponent<PhysicalHinge>(); guide.Configure(joint,damping); return guide;
        }
        public Transform AddBall(Vector3 spawn, string name = "PartnerSpawn")
        {
            var go=new GameObject(name);go.transform.SetParent(Root,false);go.transform.localPosition=spawn;
            var list=new System.Collections.Generic.List<Transform>(Level.AdditionalBallSpawns){go.transform};
            Level.AdditionalBallSpawns=list.ToArray();return go.transform;
        }
        public void Label(string text, Vector3 p, float size = .009f)
        {
            var go=new GameObject(text,typeof(TextMesh));go.transform.SetParent(Root,false);go.transform.localPosition=p;
            go.transform.localRotation=Quaternion.Euler(90,0,0);var label=go.GetComponent<TextMesh>();
            label.text=text;label.fontSize=48;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;
            label.alignment=TextAlignment.Center;label.color=new Color(.6f,.8f,.85f);
        }
    }
}
