using System.IO;
using GravityBox.App;
using GravityBox.Foundation;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GravityBox.Editor
{
    public static class PhysicsLabBuilder
    {
        public const string Folder = "Assets/_Game/PhysicsLab";
        private const float Radius = 0.015f, Depth = 0.09f, Thickness = 0.006f, Aperture = 0.023f;
        private static Material floor, glass, frame, steel, obstacle, rim;
        private static PhysicsMaterial contact;

        public static void Generate()
        {
            foreach (string folder in new[] { "Meshes", "Materials", "Profiles", "Levels", "Prefabs" })
                Directory.CreateDirectory(Folder + "/" + folder);
            AssetDatabase.Refresh();
            PrototypeBuilder.ConfigureProject();
            CreateMaterials();
            var environment = Asset<EnvironmentProfile>("Profiles/Earth.asset");
            environment.Id = "steel-lab-earth"; environment.DisplayName = "EARTH GRAVITY";
            environment.GravityScale = 1; environment.WorldGravityDirection = Vector3.down;
            environment.LinearDamping = environment.AngularDamping = 0;
            environment.MaxLinearSpeed = 6; environment.MaxAngularSpeed = 400;
            environment.Background = new Color(0.035f, 0.045f, 0.055f);
            environment.Accent = new Color(0.68f, 0.86f, 0.70f);
            var physics = Asset<BallPhysicsProfile>("Profiles/Solid steel.asset");
            physics.Radius = Radius;
            physics.Mass = 7850 * 4f / 3f * Mathf.PI * Radius * Radius * Radius;
            physics.ContactMaterial = contact; physics.RollingResistanceCoefficient = 0.008f;
            physics.ContactOffset = 0.0005f; physics.MinimumAngularSpeedLimit = 400;
            physics.MaxDepenetrationSpeed = 1; physics.SolverIterations = 16; physics.SolverVelocityIterations = 8;
            physics.CollisionDetection = CollisionDetectionMode.ContinuousDynamic;
            var rotation = Asset<RotationSettings>("Profiles/Hand rotation.asset");
            rotation.DegreesPerScreen = 200; rotation.MaxDegreesPerSecond = 85;
            rotation.MaxDegreesPerSecondSquared = 360; rotation.SmoothingSeconds = 0.11f;
            rotation.MaxQueuedAngle = 14; rotation.AssistedSnapAngle = 0;
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(PrototypeBuilder.CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, PrototypeBuilder.CatalogPath);
            }
            catalog.BallProfile = physics; catalog.Rotation = rotation; catalog.BallPrefab = BuildBall(physics);
            PhysicsLabLayout[] layouts = PhysicsLabLayouts.All();
            catalog.Levels = new LevelDefinition[layouts.Length];
            for (int i = 0; i < layouts.Length; i++)
            {
                PhysicsLabLayout layout = layouts[i];
                var level = Asset<LevelDefinition>($"Levels/{layout.Shape}.asset");
                level.Id = "steel-lab-" + layout.Shape.ToString().ToLowerInvariant();
                level.Shape = layout.Shape; level.DisplayIndex = i + 1; level.DisplayName = layout.Title;
                level.TeachingHint = layout.Hint; level.Environment = environment;
                level.RotationMode = RotationMode.Free; level.InitialLocalVelocity = Vector3.zero;
                level.Tutorial = false; level.ParSeconds = 0;
                level.DesignerSolution = "Explore rolling, acceleration and impacts. Tilt the ball through the flush circular opening to observe a complete physical escape. Reset or choose another shape manually.";
                if (layout.Shape == ContainerShape.GravityLock)
                    level.DesignerSolution = "From spawn, roll down the left chamber and around the south end of the holding recess. Roll right, then north beneath its back wall. Keep the ball against this wall while northward gravity slides the amber block into its rail housing. Tilt mainly right, retaining a little northward slope, to cross the open passage. Guide the ball to the circular exit in the right chamber. All motion comes from gravity, contacts and a passive slider joint; the recess sends no signal.";
                level.Prefab = BuildContainer(layout);
                catalog.Levels[i] = level;
                EditorUtility.SetDirty(level);
            }
            foreach (Object asset in new Object[] { environment, physics, rotation, catalog }) EditorUtility.SetDirty(asset);
            BuildScene(catalog);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            ContentValidator.Validate();
            Debug.Log($"STEEL BALL LAB: generated {layouts.Length} physical containers, 30 mm solid steel ball, Earth gravity, 120 Hz simulation.");
        }

        private static T Asset<T>(string relative) where T : ScriptableObject
        {
            string path = Folder + "/" + relative;
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }

        private static void CreateMaterials()
        {
            floor = Material("Satin aluminium", new Color(0.29f, 0.36f, 0.40f), 0.55f, 0.38f);
            glass = Material("Clear cover", new Color(0.62f, 0.81f, 0.88f, 0.045f), 0, 0.7f, true);
            frame = Material("Machined edges", new Color(0.45f, 0.54f, 0.58f), 0.8f, 0.65f);
            steel = Material("Polished solid steel", new Color(0.82f, 0.85f, 0.88f), 1, 0.82f);
            obstacle = Material("Fixed brass cube", new Color(0.54f, 0.34f, 0.15f), 0.72f, 0.48f);
            rim = Material("Faint exit inlay", new Color(0.36f, 0.55f, 0.40f), 0.05f, 0.4f);
            rim.EnableKeyword("_EMISSION"); rim.SetColor("_EmissionColor", new Color(0.12f, 0.22f, 0.14f));
            string path = Folder + "/Materials/Steel on metal.physicMaterial";
            contact = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (contact == null) { contact = new PhysicsMaterial("Steel on metal"); AssetDatabase.CreateAsset(contact, path); }
            contact.staticFriction = 0.32f; contact.dynamicFriction = 0.24f;
            contact.bounciness = 0.38f;
            contact.frictionCombine = PhysicsMaterialCombine.Average;
            contact.bounceCombine = PhysicsMaterialCombine.Average;
            EditorUtility.SetDirty(contact);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/Materials/Steel brushing.asset");
            if (texture == null)
            {
                texture = new Texture2D(256, 128, TextureFormat.RGBA32, true, true) { name = "Steel brushing", wrapMode = TextureWrapMode.Repeat };
                AssetDatabase.CreateAsset(texture, Folder + "/Materials/Steel brushing.asset");
            }
            var pixels = new Color[256 * 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 256; x++)
            {
                float brush = 0.86f + 0.035f * Mathf.Sin(y * 2.31f + Mathf.Sin(x * 0.024f));
                // A faint machining band belongs to the sphere and reveals angular motion.
                float band = Mathf.Exp(-Mathf.Pow((x - 68f) / 2.2f, 2)) * 0.19f;
                pixels[y * 256 + x] = new Color(brush - band, brush - band, brush - band, 1);
            }
            texture.SetPixels(pixels); texture.Apply(); EditorUtility.SetDirty(texture);
            steel.SetTexture("_BaseMap", texture); EditorUtility.SetDirty(steel); EditorUtility.SetDirty(rim);
        }

        private static Material Material(string name, Color color, float metallic, float smoothness, bool transparent = false)
        {
            string path = Folder + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
            material.SetColor("_BaseColor", color); material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", smoothness);
            if (transparent)
            {
                material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0); material.SetFloat("_Cull", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent; material.SetShaderPassEnabled("ShadowCaster", false);
            }
            EditorUtility.SetDirty(material); return material;
        }

        private static BallController BuildBall(BallPhysicsProfile profile)
        {
            var root = new GameObject("Solid steel ball", typeof(Rigidbody), typeof(SphereCollider), typeof(BallController));
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Brushed steel surface"; visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * Radius * 2;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = steel;
            root.GetComponent<SphereCollider>().radius = Radius;
            root.GetComponent<SphereCollider>().sharedMaterial = contact;
            root.GetComponent<Rigidbody>().mass = profile.Mass;
            root.GetComponent<Rigidbody>().useGravity = false;
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Prefabs/Steel ball.prefab");
            Object.DestroyImmediate(root); return saved.GetComponent<BallController>();
        }

        private static LevelRuntime BuildContainer(PhysicsLabLayout layout)
        {
            ContainerShape shape = layout.Shape;
            Vector2[] outline = layout.Outline;
            Vector2 exit = layout.Exit, spawn = layout.Spawn;
            var root = new GameObject(shape + " box", typeof(Rigidbody), typeof(BoxRotationController), typeof(LevelRuntime));
            root.GetComponent<Rigidbody>().isKinematic = true; root.GetComponent<Rigidbody>().useGravity = false;
            var level = root.GetComponent<LevelRuntime>(); level.Rotation = root.GetComponent<BoxRotationController>();
            level.BoundsHalfExtent = layout.BoundsHalfExtent;
            level.Footprint = outline;
            level.FootprintVoids = new Vector2Contour[layout.Voids.Length];
            for (int i = 0; i < layout.Voids.Length; i++) level.FootprintVoids[i] = new Vector2Contour { Points = layout.Voids[i] };
            MeshObject("Floor with circular cut", root.transform, PhysicsLabGeometry.Panel(shape + " floor", outline, -Depth / 2, Thickness / 2, exit, Aperture, layout.Voids), floor, true);
            MeshObject("Clear side walls", root.transform, PhysicsLabGeometry.Border(shape + " walls", outline, Thickness, -Depth / 2, Depth / 2), glass, true);
            MeshObject("Clear top cover", root.transform, PhysicsLabGeometry.Panel(shape + " cover", outline, Depth / 2, Thickness / 2, Vector2.zero, 0, layout.Voids), glass, true);
            MeshObject("Lower machined edge", root.transform, PhysicsLabGeometry.Border(shape + " lower edge", outline, Thickness + 0.001f, -Depth / 2 - Thickness / 2, -Depth / 2 + 0.001f), frame, false);
            MeshObject("Upper machined edge", root.transform, PhysicsLabGeometry.Border(shape + " upper edge", outline, Thickness + 0.001f, Depth / 2 - 0.001f, Depth / 2 + Thickness / 2), frame, false);
            for (int i = 0; i < layout.Voids.Length; i++)
            {
                Vector2[] boundary = layout.Voids[i];
                MeshObject("Inner clear wall " + i, root.transform, PhysicsLabGeometry.Border(shape + " inner wall " + i, boundary, Thickness, -Depth / 2, Depth / 2), glass, true);
                MeshObject("Inner lower edge " + i, root.transform, PhysicsLabGeometry.Border(shape + " inner lower edge " + i, boundary, Thickness + .001f, -Depth / 2 - Thickness / 2, -Depth / 2 + .001f), frame, false);
                MeshObject("Inner upper edge " + i, root.transform, PhysicsLabGeometry.Border(shape + " inner upper edge " + i, boundary, Thickness + .001f, Depth / 2 - .001f, Depth / 2 + Thickness / 2), frame, false);
            }
            if (shape == ContainerShape.Square)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); cube.name = "Fixed cube";
                cube.transform.SetParent(root.transform, false); cube.transform.localScale = Vector3.one * 0.064f;
                cube.transform.localPosition = new Vector3(0, -Depth / 2 + Thickness / 2 + 0.032f, 0);
                cube.GetComponent<Renderer>().sharedMaterial = obstacle;
                cube.GetComponent<Collider>().sharedMaterial = contact; cube.GetComponent<Collider>().contactOffset = 0.0005f;
            }
            if (shape == ContainerShape.GravityLock) BuildGravityPuzzle(root.transform);
            var spawnObject = new GameObject("BallSpawn"); spawnObject.transform.SetParent(root.transform, false);
            spawnObject.transform.localPosition = new Vector3(spawn.x, -Depth / 2 + Thickness / 2 + Radius + 0.003f, spawn.y);
            level.BallSpawn = spawnObject.transform;
            var outlet = new GameObject("Flush round exit", typeof(ExitSocket)); outlet.transform.SetParent(root.transform, false);
            outlet.transform.localPosition = new Vector3(exit.x, -Depth / 2, exit.y);
            outlet.transform.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            level.Exit = outlet.GetComponent<ExitSocket>(); level.Exit.ApertureRadius = Aperture; level.Exit.WallHalfDepth = Thickness / 2;
            MeshObject("Subtle light inlay", outlet.transform, PhysicsLabGeometry.Inlay(Aperture, Thickness / 2), rim, false);
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, Folder + "/Prefabs/" + shape + " box.prefab");
            Object.DestroyImmediate(root); return saved.GetComponent<LevelRuntime>();
        }

        private static void BuildGravityPuzzle(Transform parent)
        {
            // The partition reaches both skins: flipping the box cannot bypass the gate.
            const float height = Depth - Thickness;
            PuzzleWall("Lower partition", -.036f, .036f, -.19f, -.04f);
            PuzzleWall("Left rail housing", -.036f, -.024f, .04f, .19f);
            PuzzleWall("Right rail housing", .024f, .036f, .04f, .19f);
            PuzzleWall("Rail end stop", -.024f, .024f, .158f, .19f);
            PuzzleWall("Holding recess back wall", -.136f, -.036f, .030f, .042f);
            PuzzleWall("Holding recess side wall", -.148f, -.136f, -.065f, .042f);

            Material amber = Material("Amber gravity slider", new Color(.79f, .43f, .12f), .62f, .42f);
            string path = Folder + "/Materials/Slider on metal.physicMaterial";
            var sliderContact = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (sliderContact == null)
            {
                sliderContact = new PhysicsMaterial("Slider on metal");
                AssetDatabase.CreateAsset(sliderContact, path);
            }
            sliderContact.staticFriction = .20f; sliderContact.dynamicFriction = .14f;
            sliderContact.bounciness = .06f;
            sliderContact.frictionCombine = PhysicsMaterialCombine.Average;
            sliderContact.bounceCombine = PhysicsMaterialCombine.Minimum;
            EditorUtility.SetDirty(sliderContact);
            GravitySliderAuthoring.Build(parent, amber, sliderContact);

            // Cover-mounted runners engage the carriage throughout its stroke,
            // including the closed position, and leave the rolling floor flush.
            foreach (float side in new[] { -1f, 1f })
            {
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube); rail.name = "Captured slider rail";
                rail.transform.SetParent(parent, false);
                rail.transform.localPosition = new Vector3(side * .014f, .0416f, .060f);
                rail.transform.localScale = new Vector3(.006f, .001f, .196f);
                rail.GetComponent<Renderer>().sharedMaterial = frame;
                rail.GetComponent<Collider>().sharedMaterial = sliderContact;
                rail.GetComponent<Collider>().contactOffset = .0005f;
            }

            // Quiet floor markings describe the real holding space and travel direction.
            Material marking = Material("Recess floor marking", new Color(.30f, .54f, .60f), .25f, .35f);
            Mark("Recess left inlay", new Vector3(-.114f, -.0418f, -.024f), new Vector3(.0015f, .0002f, .057f), marking);
            Mark("Recess back inlay", new Vector3(-.085f, -.0418f, .004f), new Vector3(.058f, .0002f, .0015f), marking);
            for (int i = 0; i < 3; i++)
                Mark("Rail travel notch " + i, new Vector3(.032f, .0422f, .085f + i * .025f), new Vector3(.008f, .0004f, .002f), amber);

            void PuzzleWall(string name, float x0, float x1, float z0, float z1)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = name;
                wall.transform.SetParent(parent, false);
                wall.transform.localPosition = new Vector3((x0 + x1) / 2, 0, (z0 + z1) / 2);
                wall.transform.localScale = new Vector3(x1 - x0, height, z1 - z0);
                wall.GetComponent<Renderer>().sharedMaterial = glass;
                wall.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                wall.GetComponent<Collider>().sharedMaterial = contact;
                wall.GetComponent<Collider>().contactOffset = .0005f;
                Mark(name + " upper edge", new Vector3((x0 + x1) / 2, .041f, (z0 + z1) / 2),
                    new Vector3(x1 - x0, .002f, z1 - z0), frame);
                Mark(name + " lower edge", new Vector3((x0 + x1) / 2, -.041f, (z0 + z1) / 2),
                    new Vector3(x1 - x0, .002f, z1 - z0), frame);
            }

            void Mark(string name, Vector3 position, Vector3 size, Material material)
            {
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cube); visual.name = name;
                visual.transform.SetParent(parent, false); visual.transform.localPosition = position;
                visual.transform.localScale = size; Object.DestroyImmediate(visual.GetComponent<Collider>());
                visual.GetComponent<Renderer>().sharedMaterial = material;
                visual.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material material, bool collision)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (material == glass || material == rim) go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            if (collision)
            {
                var collider = go.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
                collider.sharedMaterial = contact; collider.contactOffset = 0.0005f;
            }
            return go;
        }

        private static void BuildScene(LevelCatalog catalog)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("CameraRig", typeof(Camera), typeof(AudioListener), typeof(CameraRig));
            cameraObject.tag = "MainCamera"; Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = catalog.Levels[0].Environment.Background;
            camera.nearClipPlane = 0.005f; camera.farClipPlane = 8; camera.fieldOfView = 40;
            cameraObject.transform.position = new Vector3(0.48f, 0.76f, -0.82f); cameraObject.transform.LookAt(Vector3.zero);
            cameraObject.GetComponent<CameraRig>().ViewDirection = new Vector3(5, 9, -10);
            Light("Large cool softbox", Quaternion.Euler(42, -35, 0), new Color(0.89f, 0.94f, 1), 2.1f, true);
            Light("Warm edge light", Quaternion.Euler(24, 135, 0), new Color(1, 0.87f, 0.70f), 1.1f, false);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.50f, 0.57f, 0.64f);
            RenderSettings.ambientEquatorColor = new Color(0.23f, 0.27f, 0.30f); RenderSettings.ambientGroundColor = new Color(0.07f, 0.09f, 0.10f);
            RenderSettings.skybox = null; RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = StudioReflection(); RenderSettings.reflectionIntensity = 1;
            var bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>(); bootstrap.Catalog = catalog; bootstrap.GameplayCamera = camera;
            bootstrap.Feedback = new GameObject("Contact audio").AddComponent<GameFeedback>();
            EditorSceneManager.SaveScene(scene, PrototypeBuilder.ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(PrototypeBuilder.ScenePath, true) };
        }

        private static void Light(string name, Quaternion rotation, Color color, float intensity, bool shadow)
        {
            var light = new GameObject(name, typeof(Light)).GetComponent<Light>(); light.type = LightType.Directional;
            light.transform.rotation = rotation; light.color = color; light.intensity = intensity;
            light.shadows = shadow ? LightShadows.Soft : LightShadows.None; light.shadowBias = 0.02f; light.shadowNormalBias = 0.02f;
            light.GetUniversalAdditionalLightData().usePipelineSettings = false;
        }

        private static Cubemap StudioReflection()
        {
            const int size = 128;
            string path = Folder + "/Materials/Studio reflection.asset";
            var cube = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            if (cube == null) { cube = new Cubemap(size, TextureFormat.RGBAHalf, true) { name = "Studio reflection" }; AssetDatabase.CreateAsset(cube, path); }
            for (int face = 0; face < 6; face++)
            {
                var pixels = new Color[size * size];
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2 - 1, v = (y + 0.5f) / size * 2 - 1;
                    Vector3 d = face == 0 ? new Vector3(1, -v, -u) : face == 1 ? new Vector3(-1, -v, u) : face == 2 ? new Vector3(u, 1, v) : face == 3 ? new Vector3(u, -1, -v) : face == 4 ? new Vector3(u, -v, 1) : new Vector3(-u, -v, -1);
                    d.Normalize();
                    float key = Mathf.Pow(Mathf.Max(0, Vector3.Dot(d, new Vector3(-0.5f, 0.8f, -0.3f).normalized)), 32) * 3.8f;
                    float strip = Mathf.Exp(-Mathf.Pow((d.x - 0.55f) / 0.055f, 2)) * Mathf.Clamp01((d.y + 0.3f) * 2) * 1.8f;
                    Color ambient = Color.Lerp(new Color(0.025f, 0.035f, 0.045f), new Color(0.38f, 0.45f, 0.55f), Mathf.Clamp01(d.y * 0.5f + 0.5f));
                    pixels[y * size + x] = ambient + new Color(0.95f, 0.97f, 1) * key + new Color(1, 0.89f, 0.74f) * strip;
                }
                cube.SetPixels(pixels, (CubemapFace)face);
            }
            cube.Apply(); EditorUtility.SetDirty(cube); return cube;
        }
    }
}
