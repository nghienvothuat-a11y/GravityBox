using System;
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
    public static class WaterPreviewCapture
    {
        [Serializable] private sealed class Evidence
        {
            public string CapturedUtc;
            public string Purpose = "Editor rendering/physics fixture, not a human playthrough. Starts at authored spawn; only box rotation is commanded. Optical wake is presentation.";
            public int Width = 796, Height = 1494;
            public string[] Files = { "Water13Still.png", "Water13Moving.png", "Water13Turned.png" };
            public Vector3[] BallWorldPositions = new Vector3[3];
            public int[] LiveWakeParticles = new int[3];
        }

        [MenuItem("Gravity Box/Capture Water Experiment")]
        public static void Capture()
        {
            int result = 0;
            try { Run(); } catch (Exception e) { Debug.LogException(e); result = 1; }
            if (Application.isBatchMode) EditorApplication.Exit(result);
        }

        private static void Run()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) throw new InvalidOperationException("Water capture needs graphics; omit -nographics.");
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Capture in Edit Mode.");
            if (!Application.isBatchMode)
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scenes before capture.");
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            SimulationMode mode = UnityEngine.Physics.simulationMode;
            float oldFixed = Time.fixedDeltaTime;
            LevelRuntime level = null; BallController ball = null; GameObject service = null;
            RenderTexture target = null; Camera camera = null;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(PrototypeBuilder.ScenePath, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    var bootstrap = root.GetComponent<GameBootstrap>();
                    if (bootstrap != null) { bootstrap.enabled = false; camera = bootstrap.GameplayCamera; }
                }
                var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(PrototypeBuilder.CatalogPath);
                if (catalog.Levels.Length < 13) throw new InvalidOperationException("Generate water level first.");
                LevelDefinition definition = catalog.Levels[12];
                level = ((GameObject)PrefabUtility.InstantiatePrefab(definition.Prefab.gameObject, scene)).GetComponent<LevelRuntime>();
                ball = ((GameObject)PrefabUtility.InstantiatePrefab(catalog.BallPrefab.gameObject, scene)).GetComponent<BallController>();
                ball.Configure(catalog.BallProfile, Vector3.zero, false);
                ball.Body.position = level.BallSpawn.position; ball.transform.position = level.BallSpawn.position;
                service = new GameObject("Water fixture force system");
                var forces = service.AddComponent<EnvironmentForceSystem>(); forces.enabled = false;
                forces.Configure(ball, definition.Environment);
                level.Initialize(ball, catalog.Rotation, definition.RotationMode, forces);
                level.Rotation.enabled = false;
                WaterVolume water = level.GetComponent<WaterVolume>();
                WaterVisuals visuals = level.GetComponent<WaterVisuals>(); visuals.Initialize(water); visuals.enabled = false;
                // CameraRig and Rigidbody interpolation should not overwrite the fixture's render pose.
                camera.GetComponent<CameraRig>().enabled = false; camera.enabled = false;
                ball.Body.interpolation = RigidbodyInterpolation.None;
                level.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
                UnityEngine.Physics.simulationMode = SimulationMode.Script; Time.fixedDeltaTime = 1f / 120;
                UnityEngine.Physics.SyncTransforms();
                target = new RenderTexture(796, 1494, 24) { antiAliasing = 4 }; target.Create();
                camera.targetTexture = target; camera.aspect = 796f / 1494;
                camera.backgroundColor = definition.Environment.Background;
                MazePreviewCapture.Reframe(camera, level.BoundsHalfExtent * 1.05f);
                string directory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Artifacts");
                Directory.CreateDirectory(directory);
                var evidence = new Evidence { CapturedUtc = DateTime.UtcNow.ToString("O") };
                Tick(120); Save(0);
                level.Rotation.SetTargetOrientation(Quaternion.Euler(0, 0, -25)); Tick(50); Save(1);
                level.Rotation.SetTargetOrientation(Quaternion.Euler(48, 28, 14)); Tick(80); Save(2);
                File.WriteAllText(Path.Combine(directory, "Water13RenderFixtures.json"), JsonUtility.ToJson(evidence, true));
                Debug.Log("WATER 13: three rendered views after continuous physics; no ball pose/velocity edits after spawn.");

                void Tick(int count)
                {
                    for (int i = 0; i < count; i++)
                    {
                        level.Rotation.Step(Time.fixedDeltaTime); forces.Step(); UnityEngine.Physics.Simulate(Time.fixedDeltaTime);
                        visuals.Advance(Time.fixedDeltaTime);
                    }
                }
                void Save(int i)
                {
                    visuals.Refresh(camera); evidence.BallWorldPositions[i] = ball.Body.position;
                    evidence.LiveWakeParticles[i] = visuals.LiveWakeCount;
                    MazePreviewCapture.Render(camera, target, Path.Combine(directory, evidence.Files[i]));
                    foreach (string shaderName in new[] { "GravityBox/Underwater Caustics", "GravityBox/Retained Water", "GravityBox/Water Tracers" })
                        if (ShaderUtil.ShaderHasError(Shader.Find(shaderName))) throw new InvalidOperationException("Water shader failed: " + shaderName);
                }
            }
            finally
            {
                UnityEngine.Physics.simulationMode = mode; Time.fixedDeltaTime = oldFixed;
                if (camera != null) camera.targetTexture = null;
                if (service != null) UnityEngine.Object.DestroyImmediate(service);
                if (ball != null) UnityEngine.Object.DestroyImmediate(ball.gameObject);
                if (level != null) UnityEngine.Object.DestroyImmediate(level.gameObject);
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (Array.Exists(setup, item => item.isLoaded && item.isActive)) EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }
    }
}
