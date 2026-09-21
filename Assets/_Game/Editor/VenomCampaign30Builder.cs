using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Simulation;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        public const string Campaign30Folder = "Assets/_Game/Venom/Campaign30";
        public const string Campaign30ScenePrefix = "COgheOrigin";
        public const int LegacyLevelCount = 20;

        [MenuItem("Gravity Box/COghe/Rebuild Campaign 30 · Readable Boss")]
        public static void RebuildReadableBoss()
        {
            var buildScenes=EditorBuildSettings.scenes;
            GenerateExpansionRange(20,20);
            CopyLegacyCampaignScene(20,30);
            EditorBuildSettings.scenes=buildScenes;
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Gravity Box/COghe/Rebuild Campaign 24 · Gear Selection")]
        public static void RebuildGearSelection()
        {
            var buildScenes=EditorBuildSettings.scenes;
            GenerateExpansionRange(17,17);
            CopyLegacyCampaignScene(17,24);
            EditorBuildSettings.scenes=buildScenes;
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Gravity Box/COghe/Rebuild Campaign 22 · Accessible Slide")]
        public static void RebuildAccessibleSlide()
        {
            var buildScenes=EditorBuildSettings.scenes;
            GenerateExpansionRange(12,12);
            CopyLegacyCampaignScene(12,22);
            EditorBuildSettings.scenes=buildScenes;
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Gravity Box/COghe/Rebuild Campaign 21 · Meshing Stations")]
        public static void RebuildMeshingStations()
        {
            PrepareCampaign30Assets();
            BuildCampaign30Content(27,21);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Gravity Box/COghe/Rebuild Campaign 05 · Learning Slide")]
        public static void RebuildLearningSlide()
        {
            PrepareCampaign30Assets();
            BuildCampaign30Content(21, 5);
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE LEARNING SLIDE REBUILT: slot 05, content 21.");
        }

        [MenuItem("Gravity Box/COghe/Rebuild Campaign 11 · Catch and Flow")]
        public static void RebuildCatchAndFlow()
        {
            PrepareCampaign30Assets();
            BuildCampaign30Content(22, 11);
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE CATCH AND FLOW REBUILT: slot 11, content 22.");
        }

        [MenuItem("Gravity Box/COghe/Rebuild Campaign 14 16 18 · Mechanism Review")]
        public static void RebuildReviewedMechanisms()
        {
            PrepareCampaign30Assets();
            BuildCampaign30Content(24, 14);
            BuildCampaign30Content(25, 16);
            BuildCampaign30Content(26, 18);
            AssetDatabase.SaveAssets();
        }

        private static readonly int[] Campaign30Content =
        {
            1,2,3,4,21,5,6,7,23,30,
            22,8,9,24,11,25,13,26,16,10,
            27,12,28,17,14,29,18,15,19,20
        };

        public static string[] Campaign30ScenePaths()
        {
            var paths = new string[Campaign30Content.Length];
            for (int i = 0; i < paths.Length; i++)
                paths[i] = $"{Campaign30Folder}/{Campaign30ScenePrefix}{i + 1:00}.unity";
            return paths;
        }

        [MenuItem("Gravity Box/COghe/Generate Integrated Campaign 01–30")]
        public static void GenerateCampaign30()
        {
            PrepareCampaign30Assets();
            for (int slot = 1; slot <= Campaign30Content.Length; slot++)
            {
                int content = Campaign30Content[slot - 1];
                if (content <= LegacyLevelCount) CopyLegacyCampaignScene(content, slot);
                else BuildCampaign30Content(content, slot);
            }
            var paths = Campaign30ScenePaths();
            var scenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < paths.Length; i++) scenes.Add(new EditorBuildSettingsScene(paths[i], true));
            // Keep canonical source scenes loadable by the regression suite and editor tools.
            // Player builders still receive CampaignScenePaths(), which contains only 30 integrated slots.
            foreach (string path in LegacyCampaignScenePaths()) scenes.Add(new EditorBuildSettingsScene(path, true));
            foreach (string path in VenomPrototypeBuilder.JourneyScenes)
                if (File.Exists(path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("COGHE CAMPAIGN 30 GENERATED: stable content IDs, display order 01–30.");
        }

        private static void PrepareCampaign30Assets()
        {
            Directory.CreateDirectory(Campaign30Folder);
            Directory.CreateDirectory(Campaign30Folder + "/Definitions");
            Directory.CreateDirectory(Folder + "/Meshes");
            AssetDatabase.Refresh();
            glass = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Glass.mat");
            stone = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Floor.mat");
            mint = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Mint.mat");
            slip = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Slippery.mat");
            metal = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Blade.mat");
            plastic = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Warm resin.mat");
            skin = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Living obsidian.mat");
            contact = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder + "/Contact.physicMaterial");
            slick = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder + "/Slippery.physicMaterial");
            stepContact = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder + "/Weighted step.physicMaterial");
            if (glass == null || stone == null || contact == null || slick == null)
                throw new InvalidOperationException("Generate the Origin campaign materials before Campaign 30.");
        }

        private static void CopyLegacyCampaignScene(int content, int slot)
        {
            string source = $"{Folder}/VenomOrigin{content:00}.unity";
            string destination = $"{Campaign30Folder}/{Campaign30ScenePrefix}{slot:00}.unity";
            if (!File.Exists(source)) throw new FileNotFoundException("Legacy COghe scene is missing", source);
            AssetDatabase.DeleteAsset(destination);
            if (!AssetDatabase.CopyAsset(source, destination)) throw new IOException($"Could not copy {source} to {destination}");
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.OpenScene(destination, OpenSceneMode.Single);
            var game = Object.FindFirstObjectByType<VenomCampaign>();
            if (game == null) throw new InvalidOperationException($"Copied scene {content:00} has no VenomCampaign");
            var clone = CloneDefinition(game.Definition, slot, content);
            game.Definition = clone;
            game.gameObject.name = $"COghe Campaign {slot:00} · content {content:00}";
            foreach (var label in game.GetComponentsInChildren<TextMesh>(true))
                if (label.text == content.ToString("00")) label.text = slot.ToString("00");
            EditorUtility.SetDirty(game);
            EditorSceneManager.SaveScene(scene, destination);
        }

        private static VenomCampaignDefinition CloneDefinition(VenomCampaignDefinition source, int slot, int content)
        {
            string path = $"{Campaign30Folder}/Definitions/Slot{slot:00}.asset";
            var result = AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
            if (result == null)
            {
                result = ScriptableObject.CreateInstance<VenomCampaignDefinition>();
                AssetDatabase.CreateAsset(result, path);
            }
            EditorUtility.CopySerialized(source, result);
            result.name = $"Slot{slot:00}";
            result.Id = $"venom.origin.{content:00}";
            result.Order = slot;
            result.Boss = slot % 10 == 0;
            string suffix = source.Title ?? string.Empty;
            int marker = suffix.IndexOf('·');
            if (marker >= 0) suffix = suffix.Substring(marker + 1).Trim();
            result.Title = string.IsNullOrEmpty(suffix) ? slot.ToString("00") : $"{slot:00} · {suffix}";
            EditorUtility.SetDirty(result);
            return result;
        }

        private static VenomCampaignDefinition NewDefinition(int content, int slot)
        {
            string path = $"{Campaign30Folder}/Definitions/Slot{slot:00}.asset";
            var definition = AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<VenomCampaignDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }
            definition.Id = $"venom.origin.{content:00}";
            definition.name = $"Slot{slot:00}";
            definition.Order = slot;
            definition.Title = slot.ToString("00");
            definition.Lesson = string.Empty;
            definition.CanRotate = false;
            definition.Boss = slot % 10 == 0;
            definition.Passive = false;
            definition.CameraEuler = new Vector3(34, 18, 0);
            definition.ViewRadius = .48f;
            definition.CameraZones = Array.Empty<VenomCameraZone>();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void BuildCampaign30Content(int content, int slot)
        {
            meshSerial = content * 1000;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var owner = new GameObject($"COghe Campaign {slot:00} · content {content:00}").AddComponent<VenomLevelController>();
            owner.MatterProfile = AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder + "/Matter.asset");
            owner.ControlMode = VenomControlMode.TouchSurface;
            owner.RotationProfile = AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");
            owner.IndicatorMaterial = mint;
            var game = owner.gameObject.AddComponent<VenomCampaign>();
            var definition = NewDefinition(content, slot); game.Definition = definition;
            owner.Apparatus = new GameObject("Apparatus").transform; owner.Apparatus.SetParent(owner.transform, false);
            var pivot = new GameObject("Box pivot", typeof(Rigidbody), typeof(BoxRotationController)); pivot.transform.SetParent(owner.Apparatus, false);
            var rootBody = pivot.GetComponent<Rigidbody>(); rootBody.isKinematic = true; rootBody.useGravity = false;
            owner.Rotation = pivot.GetComponent<BoxRotationController>(); owner.ApertureRadius = .041f;
            var c = new ExpansionContext { Number = content, Owner = owner, Game = game, Definition = definition, Root = pivot.transform };
            BuildCampaign30Level(content, c);
            if (c.Surfaces.Count == 0) throw new InvalidOperationException($"No authored geometry for content {content}");
            owner.Spawn = new GameObject("Spawn").transform; owner.Spawn.SetParent(c.Root, false); owner.Spawn.localPosition = c.Spawn;
            owner.Outlet = new GameObject("Final exit").transform; owner.Outlet.SetParent(c.Root, false); owner.Outlet.localPosition = c.Exit;
            owner.Outlet.localRotation = Quaternion.LookRotation(c.Outward, Mathf.Abs(c.Outward.y) > .9f ? Vector3.forward : Vector3.up);
            Ring(owner.Outlet, Vector2.zero, owner.ApertureRadius, .0013f, mint);
            game.Surfaces = c.Surfaces.ToArray(); game.Props = c.Props.ToArray();
            owner.CrawlFaces = new Collider[6]; for (int i = 0; i < owner.CrawlFaces.Length; i++) owner.CrawlFaces[i] = game.Surfaces[0].Shape;
            foreach (var prop in c.Props) prop.transform.SetParent(owner.Apparatus, true);
            var camera = new GameObject("Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.transform.SetParent(owner.transform, false); camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 1;
            camera.nearClipPlane = .005f; camera.farClipPlane = 15; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.93f, .93f, .89f); camera.transform.rotation = Quaternion.Euler(definition.CameraEuler); camera.transform.position = -camera.transform.forward * 3;
            owner.View = camera;
            Lighting(); COgheDayLabBuilder.ApplyExpansionLevel(game);
            // Authored opening pose: the receiving tray starts uphill, as in
            // the approved 21 September sketch. CaptureInitialState also makes
            // Retry restore this pose, without changing camera or gravity.
            // Apply after building local geometry/art; this level has no props
            // detached from the pivot that need a separate initial transform.
            if (content == 21) c.Root.rotation = Quaternion.AngleAxis(45, camera.transform.forward);
            EditorUtility.SetDirty(definition);
            EditorSceneManager.SaveScene(scene, $"{Campaign30Folder}/{Campaign30ScenePrefix}{slot:00}.unity");
            Debug.Log($"COGHE CAMPAIGN SLOT {slot:00}: content {content:00} · {definition.Title}");
        }

        private static void BuildCampaign30Level(int content, ExpansionContext c)
        {
            switch (content)
            {
                case 21: BuildCampaign21(c); break;
                case 22: BuildCampaign22(c); break;
                case 23: BuildCampaign23(c); break;
                case 24: BuildCampaign24(c); break;
                case 25: BuildCampaign25(c); break;
                case 26: BuildCampaign26(c); break;
                case 27: BuildCampaign27(c); break;
                case 28: BuildCampaign28(c); break;
                case 29: BuildCampaign29(c); break;
                case 30: BuildCampaign30Boss(c); break;
                default: throw new ArgumentOutOfRangeException(nameof(content));
            }
        }

        private static void ConfigureNew(ExpansionContext c, string title, string lesson, bool rotate, Vector3 camera, float radius)
        {
            c.Definition.Title = $"{c.Definition.Order:00} · {title}";
            c.Definition.Lesson = lesson;
            c.Definition.CanRotate = rotate;
            c.Definition.CameraEuler = camera;
            c.Definition.ViewRadius = radius;
        }

        private static void BuildCampaign21(ExpansionContext c)
        {
            ConfigureNew(c, "Nghiêng là tới", "Chạm máng tím, rồi nghiêng nhẹ về phía khay bám.", true, new Vector3(22, -16, 0), .52f);
            // The lowest particle is 1.5 lattice spacings below the spawn.
            // Include its collider radius: spawning inside the 8 mm deck can
            // resolve a few nodes underneath it and pin a tail to the platform.
            c.Spawn = new Vector3(-.35f, .162f, 0); c.Exit = new Vector3(.45f, -.125f, 0); c.Outward = Vector3.right;
            // A wide inspection chamber keeps the complete slide silhouette,
            // both cradles and the outlet visible from the initial camera.
            foreach(var pane in Cube(c.Root,Vector3.zero,.33f,c.Exit,c.Outward,c.Owner.ApertureRadius,c.Surfaces))
            {
                Vector3 n=pane.Normal;
                if(Mathf.Abs(n.x)>.9f)pane.transform.localPosition=-n*.45f;
                else if(Mathf.Abs(n.z)>.9f)pane.transform.localPosition=-n*.24f;
                pane.Size=Mathf.Abs(n.y)>.9f?new Vector2(.90f,.48f):Mathf.Abs(n.x)>.9f?new Vector2(.48f,.66f):new Vector2(.90f,.66f);
                var mesh=Save(PanelMesh(pane.Size,pane.Hole,pane.HoleCentre,pane.HoleRadius));
                ((MeshCollider)pane.Shape).sharedMesh=mesh;
                pane.GetComponent<MeshFilter>().sharedMesh=Save(PanelMesh(pane.Size,pane.Hole,pane.HoleCentre,pane.HoleRadius,false));
            }
            Panel(c.Root, "Grippy start cradle", new Vector3(-.36f, .115f, 0), Vector3.up, new Vector2(.18f, .24f), stone, false, Vector2.zero, 0, c.Surfaces);
            Vector3[] curve = EarlySmoothPath(new[]
            {
                new Vector3(-.27f, .115f, 0),
                new Vector3(-.215f, .045f, 0),
                new Vector3(-.13f, -.080f, 0),
                new Vector3(.025f, -.155f, 0),
                new Vector3(.25f, -.18f, 0)
            }, 5);
            EarlyCurvedTrough(c.Root, curve, .22f, .032f, slip, c.Surfaces,.024f);
            Panel(c.Root, "Grippy receiving cradle", new Vector3(.35f, -.18f, 0), Vector3.up, new Vector2(.20f, .26f), stone, false, Vector2.zero, 0, c.Surfaces);
            foreach (float side in new[] { -1f, 1f })
            {
                Panel(c.Root, "Receiving cradle cheek", new Vector3(.35f, -.162f, side * .13f), -Vector3.forward * side,
                    new Vector2(.20f, .036f), stone, false, Vector2.zero, 0, c.Surfaces);
                Panel(c.Root, "Start cradle cheek", new Vector3(-.36f, .128f, side * .12f), -Vector3.forward * side,
                    new Vector2(.18f, .026f), stone, false, Vector2.zero, 0, c.Surfaces);
            }
            // The real glass wall closes the tray's far end around its aperture.
            // Falls from extreme rotations have a visible route back to the deck.
            Panel(c.Root,"Start cradle recovery climb",new Vector3(-.435f,-.1075f,0),Vector3.right,
                new Vector2(.20f,.445f),stone,false,Vector2.zero,0,c.Surfaces);
        }

        private static void BuildCampaign22(ExpansionContext c)
        {
            ConfigureNew(c, "Đáp rồi chui", "Chạm mép tím để rơi vào vành bám, rồi chảy sang hộp bên kia.", false, new Vector3(26, 22, 0), .55f);
            c.Definition.CameraZones = new[]
            {
                new VenomCameraZone("Hộp 1", new Vector3(-.36f, 0, 0), Vector3.one * .48f),
                new VenomCameraZone("Hộp 2", new Vector3(.36f, 0, 0), Vector3.one * .48f)
            };
            c.Spawn = new Vector3(-.40f, .17f, -.08f);
            c.Exit = new Vector3(.58f, -.025f, -.08f); c.Outward = Vector3.right;
            Vector3 mouth = new Vector3(-.14f, -.045f, -.08f);
            var left = Cube(c.Root, new Vector3(-.36f, 0, 0), .22f, mouth, Vector3.right, .042f, c.Surfaces);
            var right = Cube(c.Root, new Vector3(.36f, 0, 0), .22f, c.Exit, c.Outward, c.Owner.ApertureRadius, c.Surfaces);
            // Each bore coincides with its chamber wall; no interior fake wall or
            // displaced roof/floor seam. The only connection is the physical tube.
            Object.DestroyImmediate(right[2].gameObject); c.Surfaces.Remove(right[2]);
            Panel(c.Root, "Receiving transfer mouth", new Vector3(.14f, 0, 0), Vector3.right,
                Vector2.one * .44f, glass, true, new Vector2(-mouth.z, mouth.y), .042f, c.Surfaces);
            var entrance = left[3]; entrance.name = "Broad grippy transfer mouth";
            entrance.RingGrip = true; entrance.GripRadius = .105f;
            entrance.Slippery = true; entrance.Shape.sharedMaterial = slick;

            Panel(c.Root, "Climb deck", new Vector3(-.3925f, .14f, -.08f), Vector3.up,
                new Vector2(.375f, .20f), stone, false, Vector2.zero, 0, c.Surfaces);
            Panel(c.Root, "Recovery climb", new Vector3(-.49f, -.04f, -.08f), Vector3.right,
                new Vector2(.20f, .36f), stone, false, Vector2.zero, 0, c.Surfaces);
            foreach (float side in new[] { -1f, 1f })
                Panel(c.Root, "Recovery climb side", new Vector3(-.535f, -.04f, -.08f + side * .10f),
                    Vector3.forward * side, new Vector2(.09f, .36f), stone, false, Vector2.zero, 0, c.Surfaces);
            EarlySlope(c.Root, "Short slippery departure", new Vector3(-.205f, .14f, -.08f),
                new Vector3(-.163f, .105f, -.08f), .20f, slip, true, c.Surfaces);
            // A 45 mm open fall to the upper grip edge. The two real cheeks
            // constrain lateral misses, while leaving the fall visible from front.
            foreach (float side in new[] { -1f, 1f })
                Panel(c.Root, "Broad fall guide", new Vector3(-.205f, .025f, -.08f + side * .105f),
                    -Vector3.forward * side, new Vector2(.13f, .20f), glass, false, Vector2.zero, 0, c.Surfaces);
            Panel(c.Root, "Receiving body deck", new Vector3(.365f, -.089f, -.08f), Vector3.up,
                new Vector2(.43f, .22f), stone, false, Vector2.zero, 0, c.Surfaces);
            Panel(c.Root, "Receiving recovery climb", new Vector3(.555f, -.1545f, -.08f), Vector3.left,
                new Vector2(.22f, .131f), stone, false, Vector2.zero, 0, c.Surfaces);

            var tubeGo = new GameObject("Short transfer tube", typeof(VenomTransferTube));
            tubeGo.transform.SetParent(c.Root, false); tubeGo.transform.localPosition = mouth;
            tubeGo.transform.localRotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            c.Game.Tube = tubeGo.GetComponent<VenomTransferTube>(); c.Game.Tube.Entrance = entrance;
            c.Game.Tube.Length = .28f; c.Game.Tube.Radius = .042f;
            c.Game.Tube.FlowSpeed = .13f; c.Game.Tube.AutoEnterOnContact = true;
            var departureHint = new GameObject("Purple edge instruction").transform;
            departureHint.SetParent(c.Root, false); departureHint.localPosition = new Vector3(-.173f, .1163f, -.08f);
            c.Game.Tube.DepartureHint = departureHint;
            TubeWall(tubeGo.transform, c.Game.Tube.Length, c.Game.Tube.Radius);
        }

        private static void BuildCampaign23(ExpansionContext c)
        {
            ConfigureNew(c, "Khớp rồi!", "Đẩy bánh G vào giữa hai bánh cố định để nâng cửa.", false, new Vector3(34, 18, 0), .48f);
            c.Spawn = new Vector3(-.24f, -.265f, -.16f); c.Exit = new Vector3(.34f, -.18f, .15f); c.Outward = Vector3.right;
            ExpansionShell(c, .34f);
            var carriage = ExpansionRail(c, "G sliding gear", new Vector3(0, -.18f, .10f), Vector3.up, .16f, 0, new Vector3(.066f, .040f, .048f), .07f, .035f, false, true); carriage.LatchAtEnd = true;
            var gate = ExpansionRail(c, "Exit rack shutter", new Vector3(.332f, -.22f, .15f), Vector3.up, .18f, 0, new Vector3(.014f, .14f, .14f), .04f, .014f, false, false); gate.LatchAtEnd = true;
            var train = new GameObject("Three wheel transmission", typeof(COgheGearTrain)).GetComponent<COgheGearTrain>(); train.transform.SetParent(c.Root, false);
            train.Wheels = new[] { ExpansionWheel(c.Root, "Motor source", new Vector3(-.12f, -.02f, .165f), .06f, 20, 0), ExpansionWheel(carriage.transform, "G intermediate", new Vector3(0, 0, .065f), .06f, 20, 9), ExpansionWheel(c.Root, "Output wheel", new Vector3(.12f, -.02f, .165f), .06f, 20, 0) };
            train.PitchRadii = new[] { .06f, .06f, .06f }; train.ToothCounts = new[] { 20, 20, 20 }; train.Rack = gate; train.MeshTolerance = .004f;
            AddRackTeeth(gate); Panel(c.Root, "Gear guard and climb backing", new Vector3(0, -.02f, .145f), Vector3.back, new Vector2(.40f, .52f), glass, false, Vector2.zero, 0, c.Surfaces);
        }

        private static void BuildCampaign24(ExpansionContext c)
        {
            ConfigureNew(c, "Kéo ra mới qua", "Chạm tay nắm rồi kéo nắp về hốc trống.", false, new Vector3(27, 58, 0), .44f);
            c.Spawn = new Vector3(-.16f, -.265f, -.15f); c.Exit = new Vector3(.30f, -.18f, -.10f); c.Outward = Vector3.right;
            ExpansionShell(c, .30f);
            var cover = ExpansionRail(c, "A sliding exit cover", new Vector3(.281f, -.18f, -.10f), Vector3.forward, .16f, 0, new Vector3(.018f, .13f, .13f), .10f, .035f, false, true);
            cover.LatchAtEnd = true;
            var prop = cover.GetComponent<VenomMovableProp>();
            prop.ManipulationGrip.localPosition = new Vector3(-.020f, -.025f, 0);
            prop.ManipulationGrip.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            prop.ManipulationGrip.localScale = new Vector3(.052f, .014f, .018f);
            var controller = cover.gameObject.AddComponent<COgheExitRailLock>(); controller.Rail = cover;
            // A recessed receiver against the glass; its open face leaves the
            // real handle accessible even when the cover reaches the catch.
            MechanismVisual(c.Root, "Cover receiving pocket back", new Vector3(.296f, -.18f, .06f), new Vector3(.003f, .15f, .15f), plastic);
            foreach(float y in new[]{-.256f,-.104f})
                MechanismVisual(c.Root, "Cover receiving pocket lip", new Vector3(.282f,y,.06f),new Vector3(.034f,.010f,.16f),metal);
            MechanismVisual(c.Root, "Cover receiving pocket end", new Vector3(.282f,-.18f,.142f),new Vector3(.034f,.16f,.010f),metal);
        }

        private static void BuildCampaign25(ExpansionContext c)
        {
            ConfigureNew(c, "Hai nhịp một cửa", "Kéo A sang trái, buông tay rồi kéo B để nâng cửa.", false, new Vector3(29, 32, 0), .48f);
            c.Spawn = new Vector3(-.23f, -.265f, -.19f); c.Exit = new Vector3(.34f, -.18f, .14f); c.Outward = Vector3.right;
            ExpansionShell(c, .34f);
            // The cover is the front door of a closed shallow alcove, not a
            // floating roof with an accessible handle underneath it.
            var access = ExpansionRail(c, "A access cover", new Vector3(.05f, -.225f, -.069f), Vector3.left, .19f, 0, new Vector3(.18f, .15f, .016f), .10f, .035f, false, true); access.LatchAtEnd = true;
            access.GetComponent<VenomMovableProp>().ManipulationGrip.localPosition += Vector3.down*.025f;
            var handle = ExpansionRail(c, "B winch handle", new Vector3(.025f, -.268f, .017f), Vector3.right, .065f, 0, new Vector3(.042f, .038f, .042f), .06f, .030f, false, true); handle.LatchAtEnd = true;
            var gate = ExpansionRail(c, "Final cable shutter", new Vector3(.329f, -.18f, .14f), Vector3.up, .16f, 0, new Vector3(.016f, .14f, .14f), .04f, .014f, false, false); gate.LatchAtEnd = true;
            Panel(c.Root,"Handle alcove back",new Vector3(.05f,-.225f,.11f),Vector3.back,new Vector2(.18f,.15f),stone,false,Vector2.zero,0,c.Surfaces);
            foreach(float side in new[]{-1f,1f})
                Panel(c.Root,"Handle alcove cheek",new Vector3(.05f+side*.09f,-.225f,.025f),Vector3.left*side,new Vector2(.17f,.15f),stone,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Handle alcove roof",new Vector3(.05f,-.15f,.025f),Vector3.down,new Vector2(.18f,.17f),stone,false,Vector2.zero,0,c.Surfaces).InterceptExterior=true;
            var sequence = new GameObject("A then B cable sequence", typeof(COgheSequentialWinch)).GetComponent<COgheSequentialWinch>(); sequence.transform.SetParent(c.Root, false);
            sequence.Access = access; sequence.Handle = handle; sequence.Door = gate;
            AddWinchVisuals(c, sequence, new Vector3(.20f, -.075f, .16f));
            var display=sequence.gameObject.AddComponent<COgheWinchPresentation>();
            display.Sequence=sequence;display.Material=metal;
        }

        private static void BuildCampaign26(ExpansionContext c)
        {
            ConfigureNew(c, "Bắc một nhịp", "Chạm tay nắm để ghép cầu. Buông tay rồi chạm mặt cầu để bò qua.", false, new Vector3(32, 24, 0), .49f);
            c.Owner.ApertureRadius = .050f;
            c.Spawn = new Vector3(-.25f, -.105f, -.06f); c.Exit = new Vector3(.34f, -.08f, .08f); c.Outward = Vector3.right;
            ExpansionShell(c, .34f);
            var floor=c.Surfaces[0];floor.HasSlipRegion=true;floor.SlipRegion=new Rect(-.13f,-.30f,.275f,.60f);
            // Solid banks and a full-height partition leave one real crossing.
            RaisedBank(c,"Left raised bank",new Vector3(-.235f,-.23f,.02f),new Vector3(.21f,.14f,.32f));
            RaisedBank(c,"Right raised bank",new Vector3(.2425f,-.23f,.08f),new Vector3(.195f,.14f,.22f));
            Panel(c.Root,"Bridge destination divider",new Vector3(.145f,0,0),Vector3.left,new Vector2(.60f,.60f),glass,true,new Vector2(.08f,-.10f),.075f,c.Surfaces);
            var bridge = ExpansionRail(c, "Single bridge module", new Vector3(0, -.173f, -.06f), Vector3.forward, .14f, 0, new Vector3(.258f, .026f, .14f), .12f, .040f, false, true); bridge.LatchAtEnd = true;
            VenomSurfacePatch movingTop=null;
            foreach(var face in bridge.GetComponentsInChildren<VenomSurfacePatch>())
                if(Vector3.Dot(face.Normal,c.Root.up)>.9f)movingTop=face;
            var dockedTop=Panel(c.Root,"Docked seamless bridge deck",new Vector3(0,-.16f,.08f),Vector3.up,new Vector2(.258f,.14f),stone,false,Vector2.zero,0,c.Surfaces);
            dockedTop.gameObject.SetActive(false);
            var deck=bridge.gameObject.AddComponent<COgheDockedBridgeDeck>();
            deck.Rail=bridge;deck.MovingSurfaces=new[]{movingTop};deck.DockedTop=dockedTop;
            var prop=bridge.GetComponent<VenomMovableProp>();
            var grip=prop.ManipulationGrip;grip.name="Bridge handle";
            grip.localPosition=new Vector3(-.18f,.035f,0);
            grip.localRotation=Quaternion.LookRotation(Vector3.left,Vector3.up);
            grip.localScale=new Vector3(.054f,.022f,.024f);
            grip.gameObject.AddComponent<BoxCollider>();prop.ManipulationHandleOnly=true;
            var stem=MechanismVisual(bridge.transform,"Bridge handle stem",new Vector3(-.15f,.022f,0),new Vector3(.080f,.012f,.012f),metal);
            stem.SetParent(grip,true);
            // The whole visible handle is a touch target; the expanded trigger
            // is for picking only and adds no contact surface or puzzle force.
            var handlePick=stem.gameObject.AddComponent<BoxCollider>();handlePick.isTrigger=true;handlePick.size=new Vector3(1,2.5f,2.5f);
            var aim=new GameObject("Bridge command plane").transform;aim.SetParent(c.Root,false);aim.localPosition=new Vector3(0,-.16f,0);prop.ManipulationPlane=aim;
            // This plate remains present after docking. Its aperture physically
            // aligns with the partition bore; no solved flag removes collision.
            AddMovingAperturePlate(c,bridge,new Vector3(.130f,.083f,0),.34f,.44f,.13f,.18f);
        }

        private static void RaisedBank(ExpansionContext c,string name,Vector3 centre,Vector3 size)
        {
            foreach(var normal in new[]{Vector3.up,Vector3.left,Vector3.right,Vector3.forward,Vector3.back})
            {
                float depth=Mathf.Abs(normal.x)*size.x+Mathf.Abs(normal.y)*size.y+Mathf.Abs(normal.z)*size.z;
                Vector2 face=Mathf.Abs(normal.y)>.9f?new Vector2(size.x,size.z):Mathf.Abs(normal.x)>.9f?new Vector2(size.z,size.y):new Vector2(size.x,size.y);
                Panel(c.Root,name,centre+normal*depth*.5f,normal,face,stone,false,Vector2.zero,0,c.Surfaces);
            }
        }

        private static void BuildCampaign27(ExpansionContext c)
        {
            ConfigureNew(c, "Một bánh hai việc", "Dùng bánh G ở trạm A, rồi chuyển nó sang trạm B.", false, new Vector3(36, 12, 0), .55f);
            c.Spawn = new Vector3(-.30f, -.265f, -.18f); c.Exit = new Vector3(.38f, -.18f, .16f); c.Outward = Vector3.right;
            ExpansionShell(c, .38f);
            var carriage = ExpansionRail(c, "G reusable carriage", new Vector3(-.09f, -.22f, .04f), Vector3.right, .18f, .01f, new Vector3(.050f, .050f, .055f), .08f, .040f, false, true);
            const float pitch=.045f,gearY=-.145f,gearZ=.085f;
            var wheel = MeshingStationWheel(carriage.transform, "G reusable gear", new Vector3(0, .075f, .045f), pitch, 18, 0);
            // Clear the full gear-tip envelope, not just the smaller carriage.
            var blocker = ExpansionRail(c, "A rail blocker", new Vector3(0, -.22f, .04f), Vector3.up, .225f, 0, new Vector3(.014f, .14f, .14f), .05f, .014f, false, false); blocker.LatchAtEnd = true;
            var exitGate = ExpansionRail(c, "B exit shutter", new Vector3(.372f, -.22f, .16f), Vector3.up, .18f, 0, new Vector3(.014f, .14f, .14f), .05f, .014f, false, false); exitGate.LatchAtEnd = true;
            // G runs between two equal-pitch wheels at each stop. All five
            // wheels share the same axle plane; no visual offset from contact.
            var aWheels = new[] { MeshingStationWheel(c.Root, "A motor", new Vector3(-.09f, gearY+pitch*2, gearZ), pitch, 18, 10), MeshingStationWheel(c.Root, "A load", new Vector3(-.09f, gearY-pitch*2, gearZ), pitch, 18, 10) };
            var bWheels = new[] { MeshingStationWheel(c.Root, "B motor", new Vector3(.09f, gearY+pitch*2, gearZ), pitch, 18, 10), MeshingStationWheel(c.Root, "B load", new Vector3(.09f, gearY-pitch*2, gearZ), pitch, 18, 10) };
            var mechanism = new GameObject("One gear two stations", typeof(COgheDualDockTransmission)).GetComponent<COgheDualDockTransmission>(); mechanism.transform.SetParent(c.Root, false);
            mechanism.Carriage = carriage; mechanism.AccessGate = blocker; mechanism.ExitGate = exitGate; mechanism.CarriageWheel = wheel; mechanism.StationAWheels = aWheels; mechanism.StationBWheels = bWheels;
            mechanism.PitchRadius=pitch;mechanism.ToothCount=18;
            Panel(c.Root, "Dual station safety backing", new Vector3(0, -.01f, .14f), Vector3.back, new Vector2(.52f, .54f), glass, false, Vector2.zero, 0, c.Surfaces);
        }

        // One baked, collider-free mesh per wheel: involute flanks with a small
        // visual backlash replace the overlapping rectangular tooth blocks.
        private static Transform MeshingStationWheel(Transform parent,string name,Vector3 centre,float radius,int teeth,float phase)
        {
            var wheel=new GameObject(name).transform;wheel.SetParent(parent,false);
            wheel.localPosition=centre;wheel.localRotation=Quaternion.Euler(0,0,phase);
            float module=2*radius/teeth,rootRadius=radius-module*1.25f,tip=radius+module;
            float pressure=20*Mathf.Deg2Rad,baseRadius=radius*Mathf.Cos(pressure);
            float invPressure=Mathf.Tan(pressure)-pressure,halfTooth=Mathf.PI/(2*teeth)-.00015f/radius;
            float Flank(float r)
            {
                float angle=Mathf.Acos(Mathf.Clamp01(baseRadius/r));
                return halfTooth+invPressure-(Mathf.Tan(angle)-angle);
            }
            var outline=new List<Vector2>();
            void Point(float r,float a)=>outline.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r);
            for(int tooth=0;tooth<teeth;tooth++)
            {
                float a=tooth*Mathf.PI*2/teeth;
                Point(rootRadius,a-Mathf.PI/teeth);Point(rootRadius,a-Flank(baseRadius));
                for(int s=0;s<=4;s++){float r=Mathf.Lerp(baseRadius,tip,s/4f);Point(r,a-Flank(r));}
                for(int s=4;s>=0;s--){float r=Mathf.Lerp(baseRadius,tip,s/4f);Point(r,a+Flank(r));}
                Point(rootRadius,a+Flank(baseRadius));
            }
            var vertices=new List<Vector3>();var triangles=new List<int>();
            void Triangle(Vector3 a,Vector3 b,Vector3 c)
            {int i=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);}
            for(int i=0;i<outline.Count;i++)
            {
                Vector3 a=outline[i],b=outline[(i+1)%outline.Count];
                Vector3 af=a+Vector3.back*.006f,bf=b+Vector3.back*.006f,ab=a+Vector3.forward*.006f,bb=b+Vector3.forward*.006f;
                Triangle(Vector3.back*.006f,bf,af);Triangle(Vector3.forward*.006f,ab,bb);
                Triangle(af,bf,bb);Triangle(af,bb,ab);
            }
            var mesh=new Mesh{name="Meshing 18 tooth gear"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var gear=new GameObject("Continuous involute gear",typeof(MeshFilter),typeof(MeshRenderer));gear.transform.SetParent(wheel,false);
            gear.GetComponent<MeshFilter>().sharedMesh=Save(mesh);
            gear.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Amber resin.mat")??plastic;
            var bearing=MechanismVisual(wheel,"Silver bearing",Vector3.back*.009f,new Vector3(.017f,.009f,.017f),metal,PrimitiveType.Cylinder);bearing.localRotation=Quaternion.Euler(90,0,0);
            return wheel;
        }

        private static void BuildCampaign28(ExpansionContext c)
        {
            ConfigureNew(c, "Nhường đường", "Dời thùng vào hốc, rồi kéo bánh G trên đường ray vừa trống.", false, new Vector3(38, 15, 0), .53f);
            c.Spawn = new Vector3(-.29f, -.265f, -.18f); c.Exit = new Vector3(.36f, -.18f, .15f); c.Outward = Vector3.right;
            ExpansionShell(c, .36f);
            // The crate is constrained to a perpendicular parking rail.  It is
            // therefore a real collider in G's swept path and cannot simply be
            // shoved along the same rail as a shortcut.
            var crate = ExpansionRail(c, "Amber blocking crate", new Vector3(-.02f, -.150f, .075f), Vector3.back, .18f, 0,
                new Vector3(.12f, .28f, .12f), .16f, .045f, false, true); crate.LatchAtEnd = true;
            Panel(c.Root, "Wide parking bay", new Vector3(-.02f, -.292f, -.105f), Vector3.up, new Vector2(.20f, .18f), stone, false, Vector2.zero, 0, c.Surfaces);
            foreach (float x in new[] { -.13f, .09f }) Panel(c.Root, "Parking bay curb", new Vector3(x, -.25f, -.105f), -Vector3.right * Mathf.Sign(x + .02f), new Vector2(.18f, .08f), glass, false, Vector2.zero, 0, c.Surfaces);
            var carriage = ExpansionRail(c, "G rail carriage", new Vector3(-.20f, -.235f, .075f), Vector3.right, .32f, 0, new Vector3(.064f, .045f, .055f), .07f, .040f, false, true); carriage.LatchAtEnd = true;
            var gate = ExpansionRail(c, "Exit rack shutter", new Vector3(.352f, -.22f, .15f), Vector3.up, .18f, 0, new Vector3(.014f, .14f, .14f), .04f, .014f, false, false); gate.LatchAtEnd = true;
            var train = new GameObject("Cleared rail transmission", typeof(COgheGearTrain)).GetComponent<COgheGearTrain>(); train.transform.SetParent(c.Root, false);
            train.Wheels = new[] { ExpansionWheel(c.Root, "Motor source", new Vector3(.04f, -.235f, .140f), .04f, 18, 0), ExpansionWheel(carriage.transform, "G intermediate", new Vector3(0, 0, .065f), .04f, 18, 9), ExpansionWheel(c.Root, "Output wheel", new Vector3(.20f, -.235f, .140f), .04f, 18, 0) };
            train.PitchRadii = new[] { .04f, .04f, .04f }; train.ToothCounts = new[] { 18, 18, 18 }; train.Rack = gate; train.MeshTolerance = .004f; AddRackTeeth(gate);
            Panel(c.Root, "Gear climb backing", new Vector3(0, -.14f, .140f), Vector3.back, new Vector2(.52f, .30f), glass, false, Vector2.zero, 0, c.Surfaces);
        }

        private static void BuildCampaign29(ExpansionContext c)
        {
            ConfigureNew(c, "Bạn giữ, tớ kéo", "Chọn từng phần: một phần giữ A, phần kia kéo B. Hợp thể trước khi ra.", false, new Vector3(40, 12, 0), .59f);
            c.Spawn = new Vector3(-.31f, -.265f, -.18f); c.Exit = new Vector3(.42f, -.18f, .17f); c.Outward = Vector3.right;
            ExpansionShell(c, .42f);
            var knife = ExpansionKnife(c, "Wide gravity cutter", -.25f, -.12f); knife.TouchHalfSize = new Vector3(.038f, .10f, .09f);
            var pad = ExpansionPad(c, "A clutch", new Vector3(-.29f, -.297f, .18f), .012f);
            var handle = ExpansionRail(c, "B pulling handle", new Vector3(.20f, -.255f, -.13f), Vector3.right, .065f, 0, new Vector3(.050f, .052f, .045f), .08f, .045f, false, true);
            var gate = ExpansionRail(c, "Cooperative exit shutter", new Vector3(.412f, -.22f, .17f), Vector3.up, .18f, 0, new Vector3(.014f, .14f, .14f), .05f, .014f, false, false); gate.LatchAtEnd = true;
            var winch = new GameObject("A clutch and B winch", typeof(COgheCooperativeWinch)).GetComponent<COgheCooperativeWinch>(); winch.transform.SetParent(c.Root, false);
            winch.Input = pad; winch.Handle = handle; winch.Doors = new[] { gate }; winch.HandleForce = .050f; winch.DoorSpeed = .060f; ExpansionLinkage(c, winch);
            Panel(c.Root, "Wide reunion bay", new Vector3(.29f, -.292f, .02f), Vector3.up, new Vector2(.20f, .22f), stone, false, Vector2.zero, 0, c.Surfaces);
        }

        private static void BuildCampaign30Boss(ExpansionContext c)
        {
            ConfigureNew(c, "BOSS · Nhà máy tí hon", string.Empty, false, new Vector3(37, 15, 0), .55f);
            c.Definition.Boss = true; c.Spawn = new Vector3(-.30f, -.265f, -.18f); c.Exit = new Vector3(.38f, -.18f, .16f); c.Outward = Vector3.right;
            ExpansionShell(c, .38f);
            var carriage = ExpansionRail(c, "G factory carriage", new Vector3(-.13f, -.18f, .10f), Vector3.up, .15f, 0, new Vector3(.066f, .040f, .048f), .07f, .040f, false, true); carriage.LatchAtEnd = true;
            var cover = ExpansionRail(c, "B alcove cover", new Vector3(.13f, -.170f, .02f), Vector3.left, .12f, 0, new Vector3(.18f, .025f, .14f), .10f, .014f, false, false); cover.LatchAtEnd = true;
            var train = new GameObject("Factory cover transmission", typeof(COgheGearTrain)).GetComponent<COgheGearTrain>(); train.transform.SetParent(c.Root, false);
            train.Wheels = new[] { ExpansionWheel(c.Root, "Factory motor", new Vector3(-.25f, -.03f, .165f), .06f, 20, 0), ExpansionWheel(carriage.transform, "G intermediate", new Vector3(0, 0, .065f), .06f, 20, 9), ExpansionWheel(c.Root, "Cover output", new Vector3(-.01f, -.03f, .165f), .06f, 20, 0) };
            train.PitchRadii = new[] { .06f, .06f, .06f }; train.ToothCounts = new[] { 20, 20, 20 }; train.Rack = cover; train.MeshTolerance = .004f;
            var handle = ExpansionRail(c, "B final winch handle", new Vector3(.10f, -.268f, .04f), Vector3.right, .075f, 0, new Vector3(.048f, .038f, .045f), .06f, .035f, false, true); handle.LatchAtEnd = true;
            var gate = ExpansionRail(c, "Boss final shutter", new Vector3(.372f, -.22f, .16f), Vector3.up, .18f, 0, new Vector3(.014f, .14f, .14f), .05f, .014f, false, false); gate.LatchAtEnd = true;
            BuildHandleAlcove(c, new Vector3(.135f, -.245f, .04f), .22f, .19f);
            var sequence = new GameObject("Factory second stage winch", typeof(COgheSequentialWinch)).GetComponent<COgheSequentialWinch>(); sequence.transform.SetParent(c.Root, false);
            sequence.Access = cover; sequence.Handle = handle; sequence.Door = gate; AddWinchVisuals(c, sequence, new Vector3(.25f, .13f, .18f));
            AddRackTeeth(cover); AddRackTeeth(gate);
            Panel(c.Root, "Factory gear guard", new Vector3(-.13f, -.03f, .145f), Vector3.back, new Vector2(.40f, .50f), glass, false, Vector2.zero, 0, c.Surfaces);
        }

        private static void AddRackTeeth(COgheRailSlider rail)
        {
            MechanismVisual(rail.transform, "Rack carrier", new Vector3(-.045f, .04f, .005f), new Vector3(.009f, .20f, .014f), metal);
            for (int i = 0; i < 12; i++) MechanismVisual(rail.transform, "Rack tooth", new Vector3(-.052f, -.035f + i * .014f, .005f), new Vector3(.014f, .007f, .014f), metal);
        }

        private static void BuildHandleAlcove(ExpansionContext c, Vector3 centre, float width, float depth)
        {
            Panel(c.Root, "Handle alcove back", centre + Vector3.forward * depth * .5f, Vector3.back, new Vector2(width, .12f), glass, false, Vector2.zero, 0, c.Surfaces);
            foreach (float side in new[] { -1f, 1f }) Panel(c.Root, "Handle alcove cheek", centre + Vector3.right * side * width * .5f, Vector3.left * side, new Vector2(depth, .12f), glass, false, Vector2.zero, 0, c.Surfaces);
        }

        private static void AddWinchVisuals(ExpansionContext c, COgheSequentialWinch sequence, Vector3 point)
        {
            sequence.Drum = MechanismVisual(c.Root, "Visible cable drum", point, new Vector3(.052f, .025f, .052f), metal, PrimitiveType.Cylinder);
            sequence.LockPin = MechanismVisual(sequence.Door.transform, "Visible terminal catch", new Vector3(0, .06f, .015f), new Vector3(.030f, .010f, .010f), metal);
            Vector3 a = point, b = sequence.Door.Start + Vector3.up * .20f; Vector3 delta = b - a;
            var cable = MechanismVisual(c.Root, "Winch cable", (a + b) * .5f, new Vector3(.003f, delta.magnitude, .003f), metal); cable.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }

        private static void AddMovingAperturePlate(ExpansionContext c, COgheRailSlider bridge, Vector3 localCentre, float height, float width, float opening, float openingHeight = 0)
        {
            if(openingHeight<=0)openingHeight=opening;
            float sideBorder=(width-opening)*.5f, verticalBorder=(height-openingHeight)*.5f;
            DynamicFace(bridge.transform,"Moving aperture top",localCentre+Vector3.up*(openingHeight+verticalBorder)*.5f,Vector3.left,new Vector2(width,verticalBorder),plastic,c.Surfaces);
            DynamicFace(bridge.transform,"Moving aperture bottom",localCentre-Vector3.up*(openingHeight+verticalBorder)*.5f,Vector3.left,new Vector2(width,verticalBorder),plastic,c.Surfaces);
            DynamicFace(bridge.transform,"Moving aperture front",localCentre-Vector3.forward*(opening+sideBorder)*.5f,Vector3.left,new Vector2(sideBorder,openingHeight),plastic,c.Surfaces);
            DynamicFace(bridge.transform,"Moving aperture rear",localCentre+Vector3.forward*(opening+sideBorder)*.5f,Vector3.left,new Vector2(sideBorder,openingHeight),plastic,c.Surfaces);
        }
    }
}
