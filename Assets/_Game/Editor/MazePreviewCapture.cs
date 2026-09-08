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
    /// <summary>
    /// Repeatable rendering fixtures for the layered maze. Run in a separate Unity
    /// Editor process with graphics enabled; this never enters Play Mode or saves a scene.
    /// </summary>
    public static class MazePreviewCapture
    {
        private const int Width = 796, Height = 1494;

        [Serializable]
        private sealed class Snapshot
        {
            public string File, Mode;
            public int Layer;
            public Vector3 BallLocalPosition;
            public bool Overview;
        }

        [Serializable]
        private sealed class Manifest
        {
            public string Purpose = "Rendering fixture only. The ball is placed at authored deck entries; no physics simulation or play-route verification is performed.";
            public string CapturedUtc, Scene, Level;
            public int Width, Height;
            public Vector3 CameraPosition, CameraEuler;
            public Snapshot[] Images;
        }

        [MenuItem("Gravity Box/Capture Layered Maze Render Fixtures")]
        public static void Capture()
        {
            int exitCode = 0;
            try { CaptureFixtures(); }
            catch (Exception exception)
            {
                exitCode = 1;
                Debug.LogException(exception);
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        private static void CaptureFixtures()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Render fixtures run in Edit Mode, independently of the playable application.");
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("Rendering requires a graphics device. Run batch mode without -nographics.");
            // A menu invocation must not discard unsaved work when opening the source scene.
            if (!Application.isBatchMode)
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    if (SceneManager.GetSceneAt(i).isDirty)
                        throw new InvalidOperationException("Save the open Editor scenes before running the rendering fixture.");

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            LevelRuntime level = null;
            BallController ball = null;
            MazeLayerView layerView = null;
            Camera camera = null;
            RenderTexture target = null;
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(PrototypeBuilder.ScenePath, OpenSceneMode.Single);
                var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(PrototypeBuilder.CatalogPath);
                if (catalog == null || catalog.Levels.Length <= 10 || catalog.Levels[10].Shape != ContainerShape.LayeredMaze)
                    throw new InvalidOperationException("Generate the catalog containing level 11, the layered maze, before capturing it.");
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    var bootstrap = root.GetComponent<GameBootstrap>();
                    if (bootstrap != null) { bootstrap.enabled = false; camera = bootstrap.GameplayCamera; }
                }
                if (camera == null) throw new InvalidOperationException("The Gameplay scene does not contain its configured camera.");
                camera.enabled = false;
                LevelDefinition definition = catalog.Levels[10];
                level = ((GameObject)PrefabUtility.InstantiatePrefab(definition.Prefab.gameObject, scene)).GetComponent<LevelRuntime>();
                ball = ((GameObject)PrefabUtility.InstantiatePrefab(catalog.BallPrefab.gameObject, scene)).GetComponent<BallController>();
                level.name = "Layer 11 rendering fixture";
                ball.name = "Steel ball rendering fixture (not simulated)";
                ball.Configure(catalog.BallProfile, Vector3.zero, false);
                ball.Body.isKinematic = true;
                var maze = level.GetComponent<LayeredMaze>();
                if (maze == null || maze.Decks.Length != 3)
                    throw new InvalidOperationException("The rendering fixture expects the three authored maze decks.");

                target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
                {
                    name = "Layered maze QA target",
                    antiAliasing = 4,
                    hideFlags = HideFlags.HideAndDontSave
                };
                target.Create();
                camera.targetTexture = target;
                camera.aspect = (float)Width / Height;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = definition.Environment.Background;
                Reframe(camera, level.BoundsHalfExtent * 1.05f);
                PlaceBall(0);
                layerView = level.gameObject.AddComponent<MazeLayerView>();
                layerView.Initialize(maze, ball);
                string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Artifacts");
                Directory.CreateDirectory(output);
                string[] files = { "Layer11Top.png", "Layer11Middle.png", "Layer11Bottom.png", "Layer11Overview.png" };
                var snapshots = new Snapshot[4];
                for (int layer = 0; layer < 3; layer++)
                {
                    PlaceBall(layer);
                    layerView.SetOverview(false);
                    layerView.Refresh();
                    if (layerView.ActiveLayer != layer)
                        throw new InvalidOperationException("The actual layer view did not select the requested rendering fixture deck.");
                    Render(camera, target, Path.Combine(output, files[layer]));
                    snapshots[layer] = Record(files[layer], layer, false);
                }
                PlaceBall(0);
                layerView.Refresh();
                layerView.SetOverview(true);
                Render(camera, target, Path.Combine(output, files[3]));
                snapshots[3] = Record(files[3], 0, true);
                var manifest = new Manifest
                {
                    CapturedUtc = DateTime.UtcNow.ToString("O"), Scene = PrototypeBuilder.ScenePath,
                    Level = definition.Id, Width = Width, Height = Height,
                    CameraPosition = camera.transform.position, CameraEuler = camera.transform.eulerAngles,
                    Images = snapshots
                };
                File.WriteAllText(Path.Combine(output, "Layer11RenderFixtures.json"), JsonUtility.ToJson(manifest, true));
                Debug.Log("LAYER 11 RENDER FIXTURES: captured top, middle, bottom focus and overview at 796x1494. "
                    + "Each ball pose is an explicitly placed visual fixture, not evidence of a physics solve. No scene assets saved.");

                void PlaceBall(int layer)
                {
                    Vector3 local = maze.Decks[layer].RouteLocalPoints[0];
                    Vector3 world = level.transform.TransformPoint(local);
                    ball.transform.SetPositionAndRotation(world, Quaternion.identity);
                    ball.Body.position = world;
                    UnityEngine.Physics.SyncTransforms();
                }

                Snapshot Record(string file, int layer, bool overview) => new Snapshot
                {
                    File = file, Mode = overview ? "Overview" : "Focused deck", Layer = layer,
                    BallLocalPosition = level.transform.InverseTransformPoint(ball.Body.position), Overview = overview
                };
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (camera != null) camera.targetTexture = null;
                if (layerView != null) UnityEngine.Object.DestroyImmediate(layerView);
                if (ball != null) UnityEngine.Object.DestroyImmediate(ball.gameObject);
                if (level != null) UnityEngine.Object.DestroyImmediate(level.gameObject);
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                // Reopen the saved source state; the temporary fixture is never written to disk.
                if (Array.Exists(previousSetup, setup => setup.isLoaded && setup.isActive))
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static void Reframe(Camera camera, float radius)
        {
            camera.fieldOfView = 40;
            float vertical = camera.fieldOfView * .5f * Mathf.Deg2Rad;
            float horizontal = Mathf.Atan(Mathf.Tan(vertical) * camera.aspect);
            float angle = Mathf.Min(vertical * .6f, horizontal * .92f);
            float distance = radius / Mathf.Sin(angle);
            camera.nearClipPlane = Mathf.Max(.0001f, radius * .02f);
            camera.farClipPlane = Mathf.Max(1, distance + radius * 30);
            camera.transform.position = new Vector3(5, 9, -10).normalized * distance;
            camera.transform.LookAt(Vector3.zero);
        }

        private static void Render(Camera camera, RenderTexture target, string path)
        {
            var request = new RenderPipeline.StandardRequest { destination = target };
            if (!RenderPipeline.SupportsRenderRequest(camera, request))
                throw new InvalidOperationException("The current render pipeline does not support the off-screen rendering request.");
            // The first render warms material/light state; the second is the recorded fixture.
            RenderPipeline.SubmitRenderRequest(camera, request);
            RenderPipeline.SubmitRenderRequest(camera, request);
            RenderTexture previous = RenderTexture.active;
            var image = new Texture2D(Width, Height, TextureFormat.RGBA32, false, false);
            try
            {
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
                var colors = new HashSet<Color32>();
                Color32[] pixels = image.GetPixels32();
                for (int i = 0; i < pixels.Length && colors.Count < 32; i += 17) colors.Add(pixels[i]);
                if (colors.Count < 16)
                    throw new InvalidOperationException("Rendering fixture appears blank or unshaded: " + path);
                Debug.Log("Captured rendering fixture: " + path);
            }
            finally
            {
                RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(image);
            }
        }
    }
}
