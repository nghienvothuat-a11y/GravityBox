using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.App;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace GravityBox.Editor
{
    [InitializeOnLoad]
    public static class CooperativePreviewCapture
    {
        [Serializable] private sealed class Frame
        {
            public string File; public float Seconds; public Vector3[] BallPositions;
            public int Escaped; public bool Holding, Retained, LeftClear, RightClear;
            public float HoldTravel, ReleaseTravel; public Quaternion BoxRotation;
        }
        [Serializable] private sealed class Evidence
        {
            public string CapturedUtc;
            public string Purpose = "Continuous Editor physics/render fixture from both authored spawns. Only box tilt is commanded. No ball pose/velocity writes after initialization; not a human playthrough.";
            public List<Frame> Frames = new List<Frame>();
        }
        private const string PendingKey = "GravityBox.CooperativeCapturePending";
        static CooperativePreviewCapture()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingKey, false)) return;
                SessionState.SetBool(PendingKey, false);
                int result = 0;
                try { Run(); } catch (Exception e) { Debug.LogException(e); result = 1; }
                EditorApplication.Exit(result);
            };
        }
        // Batch utility enters Play Mode so real collision callbacks operate the switches.
        public static void Capture()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run this fixture with Unity -batchmode and graphics.");
            Scene scene = EditorSceneManager.OpenScene(PrototypeBuilder.ScenePath, OpenSceneMode.Single);
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.GetComponent<GameBootstrap>() != null) root.SetActive(false);
            SessionState.SetBool(PendingKey, true);
            EditorApplication.EnterPlaymode();
        }
        private static void Run()
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new InvalidOperationException("Omit -nographics for capture.");
            if(!Application.isPlaying)throw new InvalidOperationException("Physics capture requires Play Mode callbacks.");

            SimulationMode oldMode=UnityEngine.Physics.simulationMode;Vector3 oldGravity=UnityEngine.Physics.gravity;
            float oldFixed=Time.fixedDeltaTime;
            LevelRuntime level=null;var balls=new List<BallController>();GameObject service=null;RenderTexture texture=null;Camera camera=null;
            try
            {
                Scene scene=SceneManager.GetActiveScene();
                foreach(GameObject root in scene.GetRootGameObjects())
                {var boot=root.GetComponent<GameBootstrap>();if(boot!=null){boot.enabled=false;camera=boot.GameplayCamera;}}
                var catalog=AssetDatabase.LoadAssetAtPath<LevelCatalog>(PrototypeBuilder.CatalogPath);
                var definition=catalog.Levels[15];
                level=UnityEngine.Object.Instantiate(definition.Prefab);
                service=new GameObject("Cooperative capture forces");var forces=service.AddComponent<EnvironmentForceSystem>();forces.enabled=false;
                for(int i=0;i<level.BallCount;i++)
                {
                    var ball=UnityEngine.Object.Instantiate(catalog.BallPrefab);
                    ball.Body.position=level.GetBallSpawn(i).position;ball.transform.position=ball.Body.position;
                    ball.Configure(catalog.BallProfile,Vector3.zero,false);balls.Add(ball);
                    var tint=new MaterialPropertyBlock();tint.SetColor("_BaseColor",i==0?new Color(.42f,.8f,.88f):new Color(.95f,.67f,.3f));
                    foreach(Renderer renderer in ball.GetComponentsInChildren<Renderer>())renderer.SetPropertyBlock(tint);
                    if(i==0)forces.Configure(ball,definition.Environment);else forces.Register(ball);
                    ball.Body.interpolation=RigidbodyInterpolation.None;
                }
                level.Initialize(balls,catalog.Rotation,definition.RotationMode,forces);level.Rotation.enabled=false;
                Rigidbody box=level.GetComponent<Rigidbody>();box.interpolation=RigidbodyInterpolation.None;
                foreach(var prop in level.Props)prop.Body.interpolation=RigidbodyInterpolation.None;
                camera.GetComponent<CameraRig>().enabled=false;camera.enabled=false;
                UnityEngine.Physics.simulationMode=SimulationMode.Script;UnityEngine.Physics.gravity=Vector3.zero;Time.fixedDeltaTime=1f/120;
                UnityEngine.Physics.SyncTransforms();level.Exit.BeginTracking();
                texture=new RenderTexture(796,1494,24){antiAliasing=4};texture.Create();camera.targetTexture=texture;camera.aspect=796f/1494;
                camera.backgroundColor=definition.Environment.Background;MazePreviewCapture.Reframe(camera,level.BoundsHalfExtent*1.05f);
                // A slightly steeper inspection angle keeps the two contact pockets readable.
                float distance=camera.transform.position.magnitude;camera.transform.position=new Vector3(0,14,-12).normalized*distance;camera.transform.LookAt(Vector3.zero);
                var relay=level.GetComponent<CooperativeRelay>();var evidence=new Evidence{CapturedUtc=DateTime.UtcNow.ToString("O")};
                string folder=Path.Combine(Path.GetDirectoryName(Application.dataPath),"Artifacts");float time=0;
                var a=balls[0];var b=balls[1];Tick(120);Save("Coop16Start.png");
                Tilt(new Vector2(0,-.9f));for(int t=0;t<600&&!relay.Holding;t++)Tick(1);
                Require(relay.Holding,"A did not hold.");Save("Coop16Hold.png");
                for(int t=0;t<1800&&Local(b).z>-.06f;t++)
                {Vector3 p=Local(b),v=Velocity(b);Tilt(new Vector2(Mathf.Clamp((.22f-p.x)*8-v.x*5,-.65f,.65f),-.9f));Tick(1);}
                Require(Local(b).z<-.06f,"B did not cross.");Save("Coop16Cross.png");
                Move(b,new Vector2(.10f,-.09f));Tilt(new Vector2(0,-.9f));for(int t=0;t<600&&!relay.Released;t++)Tick(1);
                Require(relay.Released,"B did not release.");Tick(90);Save("Coop16Retained.png");
                Move(a,new Vector2(Local(a).x,.24f));
                foreach(Vector2 goal in new[]{new Vector2(-.282f,.24f),new Vector2(-.282f,.035f),new Vector2(-.20f,.035f),new Vector2(-.20f,-.09f),new Vector2(-.20f,-.278f),new Vector2(0,-.278f)})Move(a,goal);
                Require(level.Exit.EscapedCount==1,"First departure must leave one ball.");Save("Coop16OneOut.png");
                foreach(Vector2 goal in new[]{new Vector2(.22f,.06f),new Vector2(.22f,-.09f),new Vector2(.26f,-.09f),new Vector2(.26f,-.278f),new Vector2(0,-.278f)})Move(b,goal);
                Require(level.Exit.HasExited,"Both must leave.");Save("Coop16BothOut.png");
                File.WriteAllText(Path.Combine(folder,"Coop16Evidence.json"),JsonUtility.ToJson(evidence,true));
                Debug.Log("COOPERATIVE CAPTURE: six physics frames, both balls exited.");
                void Tick(int n){for(int i=0;i<n;i++){level.Rotation.Step(Time.fixedDeltaTime);forces.Step();UnityEngine.Physics.Simulate(Time.fixedDeltaTime);level.Exit.EvaluateTraversal();time+=Time.fixedDeltaTime;}}
                Vector3 Local(BallController ball)=>Quaternion.Inverse(box.rotation)*(ball.Body.position-box.position);
                Vector3 Velocity(BallController ball)=>Quaternion.Inverse(box.rotation)*(ball.Body.linearVelocity-box.GetPointVelocity(ball.Body.position));
                void Tilt(Vector2 acc)=>level.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(acc.x,-9.81f,acc.y),Vector3.down));
                void Move(BallController ball,Vector2 goal)
                {
                    if(level.Exit.HasBallExited(ball))return;
                    for(int t=0;t<2400;t++)
                    {
                        Vector3 p=Local(ball),v=Velocity(ball);Vector2 error=goal-new Vector2(p.x,p.z);
                        Tilt(Vector2.ClampMagnitude(error*8-new Vector2(v.x,v.z)*5,1));Tick(1);
                        if(level.Exit.HasBallExited(ball)||(goal!=new Vector2(0,-.278f)&&error.magnitude<.008f&&v.magnitude<.06f))return;
                    }
                    throw new InvalidOperationException("Fixture blocked at "+goal+", ball at "+Local(ball));
                }
                void Save(string file)
                {
                    evidence.Frames.Add(new Frame{File=file,Seconds=time,BallPositions=new[]{a.Body.position,b.Body.position},Escaped=level.Exit.EscapedCount,
                        Holding=relay.Holding,Retained=relay.Released,LeftClear=relay.LeftClear,RightClear=relay.RightClear,
                        HoldTravel=relay.HoldSwitch.Guide.Displacement,ReleaseTravel=relay.ReleaseSwitch.Guide.Displacement,BoxRotation=box.rotation});
                    MazePreviewCapture.Render(camera,texture,Path.Combine(folder,file));
                }
                void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
            }
            finally
            {
                if(camera!=null)camera.targetTexture=null;
                if(texture!=null){texture.Release();UnityEngine.Object.DestroyImmediate(texture);}
                if(level!=null)
                {
                    foreach(var prop in level.Props)if(prop!=null)UnityEngine.Object.DestroyImmediate(prop.gameObject);
                    UnityEngine.Object.DestroyImmediate(level.gameObject);
                }
                foreach(var ball in balls)if(ball!=null)UnityEngine.Object.DestroyImmediate(ball.gameObject);
                if(service!=null)UnityEngine.Object.DestroyImmediate(service);
                UnityEngine.Physics.simulationMode=oldMode;UnityEngine.Physics.gravity=oldGravity;Time.fixedDeltaTime=oldFixed;

            }
        }
    }
}
