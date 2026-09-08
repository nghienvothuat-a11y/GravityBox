using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.App;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GravityBox.Editor
{
    public static class PrototypeBuilder
    {
        public const string GamePath = "Assets/_Game";
        public const string CatalogPath = GamePath + "/ScriptableObjects/LevelCatalog.asset";
        public const string ScenePath = GamePath + "/Scenes/Gameplay.unity";
        private static Material frame, shell, geometry, accent, hazard, steel, dark, blue, gold, trail, exitRim;
        private static PhysicsMaterial earthContact, spaceContact;
        private static PhysicsMaterial currentContact;
        private static readonly string[] Names = {
            "First principles", "Around the corner", "Small steps", "A little caution", "Carry the motion",
            "Let it fall", "No turning back", "Spring theory", "Chain reaction", "Gravity graduate",
            "Weightless", "Equal and opposite", "A gentle push", "Moving the walls", "Permission to pass", "Orbital mechanics"
        };
        private static readonly string[] Hints = {
            "Tilt the shelf. Let gravity do the rest.", "A change of angle opens a new path.", "Right, then left. Take it one shelf at a time.",
            "Stay above the coral hazard. Land near the exit.", "Build a little momentum before the climb.", "Turn the hole up. Let the lid fall, then tilt it aside.",
            "The striped gate lets you through in one direction.", "The amber spring gives you a lift. Catch the upper exit.", "Open the door, then find your way through the gate.",
            "Two shelves. One switch. Put it all together.", "No gravity here. Watch the ball keep moving.", "A slanted wall can turn motion into a new direction.",
            "Meet the spring. Follow the new trajectory.", "Move the corridor into the ball's path.", "Find the switch before crossing the chamber.",
            "Plan the contacts. Every wall can change the journey."
        };

        [MenuItem("Gravity Box/Generate Three Physics Boxes")]
        public static void Generate() => PhysicsLabBuilder.Generate();

        // Preserved as historical authoring code; active builds use the three-box catalog.
        private static void GenerateLegacy()
        {
            string[] folders = { "Scenes", "Materials", "Audio", "Prefabs/Ball", "Prefabs/Levels", "Prefabs/Mechanisms", "ScriptableObjects/Environments", "ScriptableObjects/Physics", "ScriptableObjects/Levels", "Settings" };
            foreach (string folder in folders) Directory.CreateDirectory(GamePath + "/" + folder);
            AssetDatabase.Refresh();
            ConfigureProject();
            CreateMaterials();
            EnvironmentProfile earth = Asset<EnvironmentProfile>(GamePath + "/ScriptableObjects/Environments/Earth.asset");
            earth.Id = "earth"; earth.DisplayName = "GRAVITY"; earth.GravityScale = 1;
            EnvironmentProfile space = Asset<EnvironmentProfile>(GamePath + "/ScriptableObjects/Environments/ZeroG.asset");
            space.Id = "zero-g"; space.DisplayName = "ZERO-G"; space.GravityScale = 0;
            space.LinearDamping = 0; space.AngularDamping = 0;
            space.Accent = new Color(0.42f, 0.8f, 1); space.Background = new Color(0.035f, 0.035f, 0.09f);
            var physics = Asset<BallPhysicsProfile>(GamePath + "/ScriptableObjects/Physics/Ball.asset");
            physics.ContactMaterial = earthContact;
            var rotation = Asset<RotationSettings>(GamePath + "/ScriptableObjects/Physics/Rotation.asset");
            var catalog = Asset<LevelCatalog>(CatalogPath);
            catalog.BallProfile = physics;
            catalog.Rotation = rotation;
            catalog.CompletionDelay = 1.8f;
            catalog.BallPrefab = BuildBall();
            AddBallReadability();
            catalog.Levels = new LevelDefinition[16];
            for (int i = 0; i < catalog.Levels.Length; i++)
            {
                currentContact = i < 10 ? earthContact : spaceContact;
                var level = Asset<LevelDefinition>($"{GamePath}/ScriptableObjects/Levels/L{i + 1:00}.asset");
                level.Id = $"gb-{i + 1:00}";
                level.DisplayIndex = i + 1; level.DisplayName = Names[i]; level.TeachingHint = Hints[i];
                level.Environment = i < 10 ? earth : space;
                level.Tutorial = i == 0 || i == 1 || i == 10;
                level.Prefab = BuildLevel(i, out Vector3 velocity, out string solution);
                level.InitialLocalVelocity = velocity;
                level.DesignerSolution = solution;
                EditorUtility.SetDirty(level);
                catalog.Levels[i] = level;
            }
            foreach (var asset in new UnityEngine.Object[] { earth, space, physics, rotation, catalog }) EditorUtility.SetDirty(asset);
            BuildMechanismLibrary();
            BuildScene(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("GRAVITY BOX: generated 16 level prefabs, profiles, mechanism library and Gameplay scene.");
        }

        internal static void ConfigureProject()
        {
            PlayerSettings.companyName = "Gravity Box Lab";
            PlayerSettings.productName = "Gravity Box";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.gravityboxlab.prototype");
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, "com.gravityboxlab.prototype");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            var serialized = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            SerializedProperty input = serialized.FindProperty("activeInputHandler");
            if (input != null) { input.intValue = 1; serialized.ApplyModifiedPropertiesWithoutUndo(); }
            PhysicsTiming.Apply();
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 0;
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(GamePath + "/Settings/MobileURP.asset");
            if (pipeline == null)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, GamePath + "/Settings/MobileRenderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, GamePath + "/Settings/MobileURP.asset");
            }
            pipeline.msaaSampleCount = 4;
            pipeline.renderScale = 1;
            pipeline.supportsHDR = false;
            pipeline.shadowDistance = 3;
            var pipelineSettings = new SerializedObject(pipeline);
            pipelineSettings.FindProperty("m_SoftShadowsSupported").boolValue = true;
            pipelineSettings.ApplyModifiedPropertiesWithoutUndo();
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline);
            EditorSettings.serializationMode = SerializationMode.ForceText;
        }

        private static T Asset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void CreateMaterials()
        {
            frame = Mat("Titanium frame", new Color(0.22f, 0.39f, 0.44f), 0.6f, 0.65f);
            shell = Mat("Clear acrylic", new Color(0.36f, 0.68f, 0.74f, 0.045f), 0, 0.9f, true);
            geometry = Mat("Ceramic platforms", new Color(0.39f, 0.55f, 0.6f), 0.25f, 0.45f);
            accent = Mat("Exit mint", new Color(0.65f, 0.96f, 0.48f), 0.1f, 0.5f, false, true);
            hazard = Mat("Hazard coral", new Color(1f, 0.27f, 0.23f), 0.1f, 0.5f, false, true);
            steel = Mat("Ball polished steel", new Color(0.9f, 0.95f, 0.96f), 0.45f, 0.88f);
            dark = Mat("Graphite", new Color(0.07f, 0.12f, 0.15f), 0.2f, 0.4f);
            blue = Mat("Cyan marker", new Color(0.3f, 0.77f, 0.96f), 0.1f, 0.45f, false, true);
            gold = Mat("Mechanism amber", new Color(1f, 0.66f, 0.26f), 0.35f, 0.55f, false, true);
            CreateExitRimMaterial();
            trail = Mat("Momentum trail", new Color(0.39f, 0.8f, 1f, 0.5f), 0, 0, true, true);
            earthContact = Contact("Earth contact", 0.20f, 0.10f);
            spaceContact = Contact("ZeroG contact", 0.01f, 0.95f);
        }

        private static PhysicsMaterial Contact(string name, float friction, float bounce)
        {
            string path = GamePath + "/Materials/" + name + ".physicMaterial";
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (mat == null) { mat = new PhysicsMaterial(name); AssetDatabase.CreateAsset(mat, path); }
            mat.staticFriction = mat.dynamicFriction = friction;
            mat.bounciness = bounce;
            mat.frictionCombine = PhysicsMaterialCombine.Minimum;
            mat.bounceCombine = PhysicsMaterialCombine.Maximum;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material Mat(string name, Color color, float metallic, float smoothness, bool transparent = false, bool emission = false)
        {
            string path = GamePath + "/Materials/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            if (transparent)
            {
                mat.SetFloat("_Surface", 1);
                mat.SetFloat("_Blend", 0);
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)RenderQueue.Transparent;
                mat.SetShaderPassEnabled("ShadowCaster", false);
            }
            if (emission) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", color * 0.55f); }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameObject Node(string name, Transform parent, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go;
        }

        private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 size, Material material, bool collider = true, float angle = 0)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.transform.localRotation = Quaternion.Euler(0, 0, angle);
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            else go.GetComponent<Collider>().sharedMaterial = currentContact;
            return go;
        }

        private static GameObject Sphere(string name, Transform parent, Vector3 position, float diameter, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = Vector3.one * diameter;
            go.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static BallController BuildBall()
        {
            GameObject go = new GameObject("Ball", typeof(Rigidbody), typeof(SphereCollider), typeof(BallController));
            Sphere("Polished shell", go.transform, Vector3.zero, 0.54f, steel);
            Sphere("Position marker", go.transform, new Vector3(0, 0.18f, -0.19f), 0.11f, accent);
            var path = go.AddComponent<TrailRenderer>();
            path.sharedMaterial = trail; path.time = 1.1f; path.startWidth = 0.07f; path.endWidth = 0;
            path.minVertexDistance = 0.07f; path.emitting = false;
            path.shadowCastingMode = ShadowCastingMode.Off;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, GamePath + "/Prefabs/Ball/Ball.prefab");
            UnityEngine.Object.DestroyImmediate(go);
            return prefab.GetComponent<BallController>();
        }

        public static void AddBallReadability()
        {
            string materialPath = GamePath + "/Materials/Ball occlusion.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("GravityBox/Ball Occlusion"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.SetColor("_BaseColor", new Color(0.8f, 1, 0.9f, 0.6f));
            string prefabPath = GamePath + "/Prefabs/Ball/Ball.prefab";
            GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Transform existing = contents.transform.Find("Occluded position cue");
                if (existing == null)
                {
                    GameObject cue = Sphere("Occluded position cue", contents.transform, Vector3.zero, 0.546f, material);
                    cue.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                    cue.GetComponent<Renderer>().receiveShadows = false;
                }
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }

        private static void BuildShell(Transform root, ExitSocket outlet)
        {
            Transform group = Node("GlassShell", root, Vector3.zero).transform;
            Vector3[] sizes = { new Vector3(0.18f, 6.1f, 6.1f), new Vector3(6.1f, 0.18f, 6.1f), new Vector3(6.1f, 6.1f, 0.18f) };
            for (int axis = 0; axis < 3; axis++)
            {
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector3 p = Vector3.zero; p[axis] = sign * 3f;
                    Vector3 normal = Vector3.zero; normal[axis] = sign;
                    if (Vector3.Dot(outlet.transform.forward, normal) > 0.99f)
                    {
                        Transform face = Node("Acrylic wall with circular cut", group, p).transform;
                        face.rotation = outlet.transform.rotation;
                        Vector3 center = face.InverseTransformPoint(outlet.transform.position);
                        Mesh mesh = CircularExitGeometry.Wall(root.name + " cut wall", center, outlet.ApertureRadius, outlet.WallHalfDepth);
                        GameObject wall = CircularExitGeometry.Visual("Flush cut surface", face, mesh, shell);
                        var collider = wall.AddComponent<MeshCollider>();
                        collider.sharedMesh = mesh;
                        collider.convex = false; // Kinematic root, convex ball; preserve the actual hole.
                        collider.sharedMaterial = currentContact;
                    }
                    else
                    {
                        GameObject wall = Cube("Acrylic wall", group, p, sizes[axis], shell);
                        wall.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                    }
                }
                for (int a = -1; a <= 1; a += 2)
                    for (int b = -1; b <= 1; b += 2)
                    {
                        Vector3 p = Vector3.zero; p[(axis + 1) % 3] = a * 3; p[(axis + 2) % 3] = b * 3;
                        Vector3 s = Vector3.one * 0.075f; s[axis] = 6.1f;
                        Cube("Edge rail", group, p, s, frame, false);
                    }
            }
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        Sphere("Corner fastener", group, new Vector3(x, y, z) * 3, 0.19f, steel);
            for (int i = 0; i < 13; i++) Cube("Calibration tick", group, new Vector3(-2.7f + i * 0.45f, -3.1f, -3.11f), new Vector3(0.018f, i % 3 == 0 ? 0.17f : 0.08f, 0.015f), frame, false);
        }

        private static void Shelf(Transform root, float x, float y, float width, float angle = 0)
        {
            GameObject platform = Node("Platform", root, new Vector3(x, y, 0));
            platform.transform.localRotation = Quaternion.Euler(0, 0, angle);
            Cube("Ceramic slab", platform.transform, Vector3.zero, new Vector3(width, 0.25f, 5.72f), geometry);
            Cube("Front edge inlay", platform.transform, new Vector3(0, 0.12f, -2.88f), new Vector3(width, 0.045f, 0.035f), blue, false);
        }

        private static void CreateExitRimMaterial()
        {
            string path = GamePath + "/Materials/Exit subtle rim.mat";
            exitRim = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (exitRim == null)
            {
                exitRim = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(exitRim, path);
            }
            // Soft constant tint, no bloom, point light, emission multiplier or collision.
            exitRim.SetColor("_BaseColor", new Color(0.33f, 0.52f, 0.39f, 1));
            EditorUtility.SetDirty(exitRim);
        }

        private static ExitSocket Exit(Transform parent, Vector2 position, string required = "")
        {
            bool bottom = position.y < -1.5f;
            Vector3 center = bottom ? new Vector3(Mathf.Clamp(position.x, -2.05f, 2.05f), -3, -1.15f)
                : new Vector3(3, Mathf.Clamp(position.y, -2.05f, 2.05f), -1.15f);
            var go = Node("Exit aperture", parent, center);
            go.transform.localRotation = Quaternion.LookRotation(bottom ? Vector3.down : Vector3.right, Vector3.back);
            var socket = go.AddComponent<ExitSocket>(); socket.RequiredChannel = required;
            float radius = socket.ApertureRadius;
            CircularExitGeometry.Visual("Faint circular inlay", go.transform,
                CircularExitGeometry.Rim(radius, socket.WallHalfDepth), exitRim);
            if (!string.IsNullOrEmpty(required))
            {
                var shutter = Node("Exit safety shutter", go.transform, Vector3.zero);
                var door = shutter.AddComponent<SignalDoor>(); door.Channel = required;
                var blocker = shutter.AddComponent<BoxCollider>(); blocker.size = new Vector3(radius * 2, radius * 2, 0.12f);
                blocker.sharedMaterial = currentContact;
                door.Blocker = blocker;
                door.Visual = CircularExitGeometry.Visual("Recessed amber shutter", shutter.transform,
                    CircularExitGeometry.Shutter(radius, 0.06f), gold).transform;
                door.OpenOffset = new Vector3(radius * 2 + 0.15f, 0, 0);
            }
            return socket;
        }

        public static void UpgradePhysicalExits()
        {
            frame = AssetDatabase.LoadAssetAtPath<Material>(GamePath + "/Materials/Titanium frame.mat");
            shell = AssetDatabase.LoadAssetAtPath<Material>(GamePath + "/Materials/Clear acrylic.mat");
            accent = AssetDatabase.LoadAssetAtPath<Material>(GamePath + "/Materials/Exit mint.mat");
            dark = AssetDatabase.LoadAssetAtPath<Material>(GamePath + "/Materials/Graphite.mat");
            gold = AssetDatabase.LoadAssetAtPath<Material>(GamePath + "/Materials/Mechanism amber.mat");
            steel = AssetDatabase.LoadAssetAtPath<Material>(GamePath + "/Materials/Ball polished steel.mat");
            CreateExitRimMaterial();
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            foreach (LevelDefinition definition in catalog.Levels)
            {
                currentContact = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(GamePath + "/Materials/" + (definition.Environment.IsZeroGravity ? "ZeroG contact" : "Earth contact") + ".physicMaterial");
                string path = AssetDatabase.GetAssetPath(definition.Prefab);
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var level = contents.GetComponent<LevelRuntime>();
                    Vector3 previous = level.Exit.transform.localPosition;
                    string channel = level.Exit.RequiredChannel;
                    bool hasLid = level.Exit.GetComponentInChildren<PhysicalProp>() != null;
                    Transform parent = level.Exit.transform.parent;
                    UnityEngine.Object.DestroyImmediate(level.Exit.gameObject);
                    UnityEngine.Object.DestroyImmediate(contents.transform.Find("GlassShell").gameObject);
                    level.Exit = Exit(parent, previous, channel);
                    if (hasLid) LooseLid(level.Exit);
                    BuildShell(contents.transform, level.Exit);
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
            var library = new GameObject("Exit library");
            ExitSocket standalone = Exit(library.transform, Vector2.zero);
            PrefabUtility.SaveAsPrefabAsset(standalone.gameObject, GamePath + "/Prefabs/Mechanisms/ExitSocket.prefab");
            UnityEngine.Object.DestroyImmediate(library);
            catalog.CompletionDelay = 1.8f;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            ContentValidator.Validate();
        }

        private static PhysicalProp LooseLid(ExitSocket outlet)
        {
            var go = Node("Loose gravity lid", outlet.transform, Vector3.zero);
            var body = go.AddComponent<Rigidbody>();
            body.mass = 2;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            var prop = go.AddComponent<PhysicalProp>();
            PhysicsMaterial contact = Contact("Loose lid contact", 0.28f, 0.02f);
            // A loose disc rests on the INNER skin; there is no plug that can wedge in the hole.
            GameObject cover = CircularExitGeometry.Visual("Loose cover", go.transform,
                CircularExitGeometry.Disc("Loose lid cover", outlet.ApertureRadius + 0.05f, 0.045f), gold);
            cover.transform.localPosition = new Vector3(0, 0, -outlet.WallHalfDepth - 0.045f - 0.012f);
            var collider = cover.AddComponent<MeshCollider>();
            collider.sharedMesh = cover.GetComponent<MeshFilter>().sharedMesh;
            collider.convex = true;
            collider.sharedMaterial = contact;
            cover.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
            return prop;
        }

        public static void UpgradeLevel6PhysicalLid()
        {
            gold = AssetDatabase.LoadAssetAtPath<Material>(GamePath + "/Materials/Mechanism amber.mat");
            dark = AssetDatabase.LoadAssetAtPath<Material>(GamePath + "/Materials/Graphite.mat");
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            LevelDefinition definition = catalog.Levels[5];
            string path = AssetDatabase.GetAssetPath(definition.Prefab);
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var level = contents.GetComponent<LevelRuntime>();
                foreach (PressurePlate plate in contents.GetComponentsInChildren<PressurePlate>()) UnityEngine.Object.DestroyImmediate(plate.gameObject);
                foreach (SignalDoor door in contents.GetComponentsInChildren<SignalDoor>()) UnityEngine.Object.DestroyImmediate(door.gameObject);
                foreach (PhysicalProp old in contents.GetComponentsInChildren<PhysicalProp>()) UnityEngine.Object.DestroyImmediate(old.gameObject);
                level.Exit.RequiredChannel = "";
                PhysicalProp lid = LooseLid(level.Exit);
                PrefabUtility.SaveAsPrefabAsset(lid.gameObject, GamePath + "/Prefabs/Mechanisms/LooseLid.prefab");
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            definition.DisplayName = Names[5];
            definition.TeachingHint = Hints[5];
            definition.DesignerSolution = "Invert the box so the opening faces upward. Gravity withdraws the loose inner lid. Tilt again to guide the ball through the clear hole; displaced lid remains physical.";
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            ContentValidator.Validate();
        }

        private static PressurePlate Plate(Transform parent, Vector2 p, string channel = "gate-a")
        {
            var go = Node("Pressure plate", parent, new Vector3(p.x, p.y, 0));
            var trigger = go.AddComponent<BoxCollider>(); trigger.isTrigger = true;
            trigger.size = new Vector3(1.1f, 0.5f, 5.65f);
            var plate = go.AddComponent<PressurePlate>(); plate.Channel = channel;
            plate.Visual = Cube("Amber switch", go.transform, new Vector3(0, -0.12f, 0), new Vector3(0.95f, 0.10f, 5.6f), gold, false).transform;
            Cube("Switch foot", go.transform, new Vector3(0, -0.25f, 0), new Vector3(1.15f, 0.12f, 5.65f), dark, false);
            return plate;
        }

        private static SignalDoor Door(Transform parent, float x, float y, float height, string channel = "gate-a")
        {
            var go = Node("Signal door", parent, new Vector3(x, y, 0));
            var blocker = go.AddComponent<BoxCollider>(); blocker.size = new Vector3(0.22f, height, 5.7f); blocker.sharedMaterial = currentContact;
            var door = go.AddComponent<SignalDoor>(); door.Channel = channel; door.Blocker = blocker;
            door.Visual = Node("Retracting panels", go.transform, Vector3.zero).transform;
            door.OpenOffset = new Vector3(0, height + 0.25f, 0);
            for (int i = 0; i < 7; i++) Cube("Amber gate bar", door.Visual, new Vector3(0, 0, -2.5f + i * 0.83f), new Vector3(0.20f, height, 0.10f), gold, false);
            return door;
        }

        private static OneWayGate Gate(Transform parent, float x, float y, float height)
        {
            var go = Node("One-way gate", parent, new Vector3(x, y, 0));
            var blocker = go.AddComponent<BoxCollider>(); blocker.size = new Vector3(0.18f, height, 5.7f); blocker.sharedMaterial = currentContact;
            var gate = go.AddComponent<OneWayGate>(); gate.Blocker = blocker;
            for (int i = 0; i < 8; i++) Cube("Gate slat", go.transform, new Vector3(0, -height / 2 + height * (i + 0.5f) / 8, 0), new Vector3(0.16f, 0.065f, 5.68f), blue, false);
            Cube("Pass arrow upper", go.transform, new Vector3(0.22f, 0.15f, -2.9f), new Vector3(0.5f, 0.055f, 0.055f), accent, false, -35);
            Cube("Pass arrow lower", go.transform, new Vector3(0.22f, -0.15f, -2.9f), new Vector3(0.5f, 0.055f, 0.055f), accent, false, 35);
            return gate;
        }

        private static ImpulsePad Pad(Transform parent, Vector2 p, Vector3 direction, float speed, bool spring = true)
        {
            var go = Node(spring ? "Spring bumper" : "Impulse pad", parent, new Vector3(p.x, p.y, 0));
            var trigger = go.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.size = new Vector3(0.95f, 0.55f, 5.65f);
            var pad = go.AddComponent<ImpulsePad>(); pad.LocalDirection = direction; pad.Speed = speed; pad.Spring = spring;
            Cube("Bumper face", go.transform, new Vector3(0, -0.2f, 0), new Vector3(0.92f, 0.16f, 5.6f), gold, false);
            for (int i = 0; i < 4; i++) Cube("Spring rib", go.transform, new Vector3(0, -0.28f - i * 0.07f, 0), new Vector3(0.55f, 0.035f, 5.45f), frame, false);
            return pad;
        }

        private static void Hazard(Transform parent, Vector2 p, Vector2 size)
        {
            var go = Node("Kill volume", parent, new Vector3(p.x, p.y, 0));
            var trigger = go.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.size = new Vector3(size.x, size.y, 5.65f);
            go.AddComponent<KillVolume>();
            Cube("Coral hazard", go.transform, Vector3.zero, new Vector3(size.x, size.y * 0.35f, 5.65f), hazard, false);
            for (int i = 0; i < 5; i++) Cube("Warning stripe", go.transform, new Vector3(-size.x * 0.4f + i * size.x * 0.2f, 0, -2.87f), new Vector3(0.10f, size.y, 0.03f), dark, false, -20);
        }

        private static LevelRuntime BuildLevel(int index, out Vector3 velocity, out string solution)
        {
            var go = new GameObject($"L{index + 1:00} {Names[index]}", typeof(Rigidbody), typeof(BoxRotationController), typeof(LevelRuntime));
            var root = go.GetComponent<LevelRuntime>(); root.Rotation = go.GetComponent<BoxRotationController>();
            Rigidbody rb = go.GetComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
            Transform geo = Node("Geometry", go.transform, Vector3.zero).transform;
            Transform mech = Node("Mechanisms", go.transform, Vector3.zero).transform;
            Vector2 spawn = new Vector2(-2, 1.8f);
            Vector2 exit = new Vector2(2.2f, -2.3f);
            velocity = Vector3.zero;
            string required = "";
            solution = "Authored route, awaiting recorded playtest: " + Hints[index];
            switch (index)
            {
                case 0: Shelf(geo, -0.8f, -0.9f, 4.1f, 3); spawn = new Vector2(-1.9f, -0.2f); break;
                case 1:
                    Shelf(geo, -1.25f, 0.65f, 3.2f);
                    Cube("Corner divider", geo, new Vector3(0.7f, 0.0f, 0), new Vector3(0.25f, 2.7f, 5.7f), geometry);
                    break;
                case 2:
                    Shelf(geo, -1.0f, 1.0f, 3.6f); Shelf(geo, 1.0f, -0.75f, 3.6f); break;
                case 3:
                    Shelf(geo, -0.65f, -0.8f, 4.35f, -8); Hazard(mech, new Vector2(-0.2f, -2.6f), new Vector2(1.75f, 0.48f)); break;
                case 4:
                    Shelf(geo, -1.9f, 1.05f, 1.75f); Shelf(geo, 0.25f, -0.5f, 3.9f, 20); break;
                case 5:
                    Shelf(geo, -1.65f, 0.2f, 2.4f); break;
                case 6:
                    Shelf(geo, -1.3f, -0.5f, 3.0f); Gate(mech, 0.55f, 0, 5.8f); break;
                case 7:
                    spawn = new Vector2(-1.9f, 1.5f); Pad(mech, new Vector2(-1.9f, -2.25f), new Vector3(0.8f, 1f, 0), 8);
                    Shelf(geo, 2.1f, 1.1f, 1.5f); exit = new Vector2(2.15f, 1.8f); break;
                case 8:
                    Plate(mech, new Vector2(-2.1f, -2.45f)); Door(mech, 0.2f, 0, 5.8f); required = "gate-a";
                    Shelf(geo, 1.65f, 0.55f, 2.2f, 16); Gate(mech, 1.25f, -1.45f, 2.4f); break;
                case 9:
                    Shelf(geo, -1.0f, 1.0f, 3.6f); Shelf(geo, 0.9f, -0.7f, 3.7f);
                    Plate(mech, new Vector2(-2.1f, -2.45f)); Door(mech, 1.15f, -1.8f, 2.1f); required = "gate-a";
                    Hazard(mech, new Vector2(2.4f, 2.2f), new Vector2(0.6f, 0.6f)); break;
                case 10:
                    spawn = new Vector2(-2.1f, 0); velocity = new Vector3(1.9f, 0.1f, 0); exit = new Vector2(2.15f, 0.22f); break;
                case 11:
                    spawn = new Vector2(-2.1f, 1.4f); velocity = new Vector3(2.4f, 0, 0);
                    Shelf(geo, 0.65f, 1.0f, 3.0f, -45); exit = new Vector2(0.08f, -2.3f); break;
                case 12:
                    spawn = new Vector2(-2.1f, 0); velocity = new Vector3(2.1f, 0, 0);
                    Pad(mech, new Vector2(1.85f, 0), new Vector3(-0.8f, -1f, 0), 4.5f); exit = new Vector2(-0.8f, -2.3f); break;
                case 13:
                    spawn = new Vector2(-2.1f, 0); velocity = new Vector3(2.1f, 0.4f, 0); exit = new Vector2(2.15f, 0);
                    Cube("Upper corridor", geo, new Vector3(-0.55f, 1.55f, 0), new Vector3(0.25f, 2.65f, 5.7f), geometry);
                    Cube("Lower corridor", geo, new Vector3(0.8f, -1.55f, 0), new Vector3(0.25f, 2.65f, 5.7f), geometry); break;
                case 14:
                    spawn = new Vector2(-1.3f, -1.25f); velocity = new Vector3(-1.4f, -0.7f, 0);
                    Plate(mech, new Vector2(-2.0f, -2.25f)); Door(mech, 0.4f, 0, 5.8f); required = "gate-a";
                    exit = new Vector2(2.15f, 1.9f); break;
                case 15:
                    spawn = new Vector2(-2.1f, 1.9f); velocity = new Vector3(2.1f, -0.35f, 0);
                    Shelf(geo, -0.15f, 1.0f, 2.0f, -35); Shelf(geo, 1.0f, -0.7f, 2.0f, 32);
                    Pad(mech, new Vector2(-1.6f, -2.1f), new Vector3(1, 1.1f, 0), 4.2f, false);
                    exit = new Vector2(2.15f, 1.95f); break;
            }
            root.BallSpawn = Node("BallSpawn", go.transform, new Vector3(spawn.x, spawn.y, -1.15f)).transform;
            root.Exit = Exit(mech, exit, required);
            if (index == 5) LooseLid(root.Exit);
            BuildShell(go.transform, root.Exit);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{GamePath}/Prefabs/Levels/L{index + 1:00}.prefab");
            UnityEngine.Object.DestroyImmediate(go);
            return prefab.GetComponent<LevelRuntime>();
        }

        private static void BuildMechanismLibrary()
        {
            currentContact = earthContact;
            var group = new GameObject("Mechanism library");
            var components = new Component[] { Plate(group.transform, Vector2.zero), Door(group.transform, 0, 0, 3), Gate(group.transform, 0, 0, 3),
                Pad(group.transform, Vector2.zero, Vector3.up, 6), Exit(group.transform, Vector2.zero) };
            foreach (Component component in components) PrefabUtility.SaveAsPrefabAsset(component.gameObject, GamePath + "/Prefabs/Mechanisms/" + component.GetType().Name + ".prefab");
            PhysicalProp lid = LooseLid(group.GetComponentInChildren<ExitSocket>());
            PrefabUtility.SaveAsPrefabAsset(lid.gameObject, GamePath + "/Prefabs/Mechanisms/LooseLid.prefab");
            UnityEngine.Object.DestroyImmediate(group);
        }

        private static void BuildScene(LevelCatalog catalog)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("CameraRig", typeof(Camera), typeof(AudioListener), typeof(CameraRig));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.037f, 0.061f, 0.08f);
            camera.nearClipPlane = 0.1f; camera.farClipPlane = 100;
            cameraObject.transform.position = new Vector3(13.5f, 9.3f, -23.4f); cameraObject.transform.LookAt(Vector3.zero);
            camera.fieldOfView = 40;
            var light = new GameObject("Softbox key", typeof(Light)).GetComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.5f; light.color = new Color(0.85f, 0.93f, 1);
            light.transform.rotation = Quaternion.Euler(35, -30, 0); light.shadows = LightShadows.Soft;
            var rim = new GameObject("Warm fill", typeof(Light)).GetComponent<Light>();
            rim.type = LightType.Directional; rim.intensity = 0.65f; rim.color = new Color(0.86f, 1, 0.88f);
            rim.transform.rotation = Quaternion.Euler(20, 140, 0); rim.shadows = LightShadows.None;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.58f, 0.65f);
            RenderSettings.skybox = null;
            var bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            bootstrap.Catalog = catalog; bootstrap.GameplayCamera = camera;
            var feedback = new GameObject("Audio feedback").AddComponent<GameFeedback>();
            feedback.ImpactClip = Tone("Impact", 180, 0.12f, 1);
            feedback.SuccessClip = Tone("Success", 660, 0.65f, 2);
            feedback.FailClip = Tone("Fail", 130, 0.32f, 1);
            feedback.MechanismClip = Tone("Mechanism", 420, 0.15f, 1);
            feedback.RollClip = Tone("Roll", 72, 1f, 3);
            bootstrap.Feedback = feedback;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static AudioClip Tone(string name, float frequency, float duration, int mode)
        {
            string path = GamePath + "/Audio/" + name + ".wav";
            const int sampleRate = 22050;
            int count = Mathf.RoundToInt(sampleRate * duration);
            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
                writer.Write(sampleRate); writer.Write(sampleRate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = mode == 3 ? 0.2f : Mathf.Exp(-t * (mode == 2 ? 4 : 22)) * Mathf.Min(t * 120, 1);
                    float value = Mathf.Sin(2 * Mathf.PI * frequency * t);
                    if (mode == 2) value = (value + Mathf.Sin(2 * Mathf.PI * frequency * 1.25f * t) + Mathf.Sin(2 * Mathf.PI * frequency * 1.5f * t)) / 3;
                    if (mode == 3) value *= Mathf.Sin(2 * Mathf.PI * 31 * t);
                    writer.Write((short)(value * envelope * 14000));
                }
            }
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        [MenuItem("Gravity Box/Open Gameplay")]
        public static void OpenGameplay() => EditorSceneManager.OpenScene(ScenePath);

        [MenuItem("Gravity Box/Build/macOS Development")]
        public static void BuildMac() => Build(BuildTarget.StandaloneOSX, "Builds/macOS/Gravity Box.app");
        [MenuItem("Gravity Box/Build/Android Development APK")]
        public static void BuildAndroid() => Build(BuildTarget.Android, "Builds/Android/GravityBox.apk");
        [MenuItem("Gravity Box/Build/iOS Xcode Project")]
        public static void BuildIOS() => Build(BuildTarget.iOS, "Builds/iOS");

        [MenuItem("Gravity Box/Build/iOS Simulator")]
        public static void BuildIOSSimulator()
        {
            iOSSdkVersion original = PlayerSettings.iOS.sdkVersion;
            AppleMobileArchitectureSimulator originalArchitecture = PlayerSettings.iOS.simulatorSdkArchitecture;
            var pipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            int originalSamples = pipeline != null ? pipeline.msaaSampleCount : 1;
            try
            {
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
                PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;
                // Simulator Metal back buffers can be single-sampled even when URP asks
                // for 4x. Keep simulator captures valid without changing device quality.
                if (pipeline != null)
                {
                    pipeline.msaaSampleCount = 1;
                    EditorUtility.SetDirty(pipeline);
                    AssetDatabase.SaveAssets();
                }
                Build(BuildTarget.iOS, "Builds/iOS-Simulator");
            }
            finally
            {
                PlayerSettings.iOS.sdkVersion = original;
                PlayerSettings.iOS.simulatorSdkArchitecture = originalArchitecture;
                if (pipeline != null)
                {
                    pipeline.msaaSampleCount = originalSamples;
                    EditorUtility.SetDirty(pipeline);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private static void Build(BuildTarget target, string path)
        {
            ContentValidator.Validate();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, target = target, locationPathName = path,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + report.summary.result);
            Debug.Log($"GRAVITY BOX BUILD SUCCESS: {path} ({report.summary.totalSize} bytes)");
        }
    }
}
