using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Simulation;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        public const string TapCampaignFolder = "Assets/_Game/Venom/TapCampaign";
        private static readonly string[] TapNames = { "Khớp nối", "Đưa trở về", "Mở từng bước", "Tách và gặp lại", "Cùng vận hành", "Đón bạn trở về", "Bốn điểm dừng", "Chia việc đổi khớp", "Rút rồi nối", "Cỗ máy chung" };
        private static readonly string[] TapLessons = {
            "Chạm bánh răng. COghe sẽ đưa nó vào ổ, rồi tự dừng.",
            "A nhấc chốt B. B bật nguồn. Đưa A về để nối bộ truyền.",
            "Quan sát chốt và đường truyền. Tìm cách nối hai bánh răng.",
            "Chạm vùng dao để tách. Chọn từng phần, đưa lại gần để nhập.",
            "Một phần đứng giữ A; phần kia đưa bánh răng B vào ổ.",
            "Giữ bàn đạp, nối bộ truyền rồi đưa cả hai phần về.",
            "Mỗi chạm đi một chốt. Đọc vị trí nhả khóa và vị trí ăn khớp.",
            "Phối hợp hai phần để đổi kết nối và bật nguồn.",
            "Có lúc phải rút một bánh răng trước khi nối cả bộ truyền.",
            ""
        };
        public static string[] TapCampaignScenePaths()
        {
            var paths = new string[TapNames.Length];
            for (int i = 0; i < paths.Length; i++) paths[i] = $"{TapCampaignFolder}/COgheTap{i + 1:00}.unity";
            return paths;
        }

        [MenuItem("Gravity Box/COghe/Generate Tap Campaign · Ten Levels")]
        public static void GenerateTapCampaign()
        {
            // This generation owns only its new scenes/definitions/meshes; Origin stays loadable for regression.
            PrepareCampaign30Assets();
            Directory.CreateDirectory(TapCampaignFolder + "/Definitions");
            Directory.CreateDirectory(TapCampaignFolder + "/Meshes");
            AssetDatabase.Refresh();
            string previousMeshFolder = authoredMeshFolder;
            try
            {
                authoredMeshFolder = TapCampaignFolder + "/Meshes";
                for (int number = 1; number <= TapNames.Length; number++) BuildTapLevel(number);
            }
            finally { authoredMeshFolder = previousMeshFolder; }
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var path in TapCampaignScenePaths())
                if (!scenes.Exists(s => s.path == path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE TAP CAMPAIGN GENERATED: ten independent scenes, original campaign preserved.");
        }

        private static void BuildTapLevel(int number)
        {
            meshSerial = number * 1000;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var owner = new GameObject($"COghe · {TapNames[number - 1]}").AddComponent<VenomLevelController>();
            Undo.RegisterCreatedObjectUndo(owner.gameObject, "Create tap campaign level");
            owner.MatterProfile = AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder + "/Matter.asset");
            owner.ControlMode = VenomControlMode.TouchSurface;
            owner.RotationProfile = AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");
            owner.IndicatorMaterial = mint;
            var game = owner.gameObject.AddComponent<VenomCampaign>();
            owner.gameObject.AddComponent<COgheTapLesson>().Lesson=(COgheTapLesson.LessonKind)(number-1);
            string dataPath = $"{TapCampaignFolder}/Definitions/Tap{number:00}.asset";
            var definition = AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(dataPath);
            if (definition == null) { definition = ScriptableObject.CreateInstance<VenomCampaignDefinition>(); AssetDatabase.CreateAsset(definition, dataPath); }
            definition.Id = $"coghe.tap.v1.{number:00}"; definition.Order = number;
            definition.Title = $"{number:00} · {TapNames[number - 1]}"; definition.Lesson = TapLessons[number - 1];
            definition.Boss = number == 10; definition.Passive = false; definition.CanRotate = false;
            definition.CameraEuler = new Vector3(55, 25, 0); definition.ViewRadius = .52f;
            definition.SceneSequence = Array.ConvertAll(TapCampaignScenePaths(), Path.GetFileNameWithoutExtension);
            definition.CameraZones = Array.Empty<VenomCameraZone>(); definition.InitialCameraZone = -1;
            game.Definition = definition;
            owner.Apparatus = new GameObject("Apparatus").transform; owner.Apparatus.SetParent(owner.transform, false);
            var pivot = new GameObject("Box pivot", typeof(Rigidbody), typeof(BoxRotationController)); pivot.transform.SetParent(owner.Apparatus, false);
            pivot.GetComponent<Rigidbody>().isKinematic = true; pivot.GetComponent<Rigidbody>().useGravity = false;
            owner.Rotation = pivot.GetComponent<BoxRotationController>(); owner.ApertureRadius = .041f;
            var c = new ExpansionContext { Number = number, Owner = owner, Game = game, Definition = definition, Root = pivot.transform,
                Spawn = new Vector3(-.23f, -.275f, -.19f), Exit = new Vector3(.4f, -.22f, .16f), Outward = Vector3.right };
            if(number>=4){c.Exit=new Vector3(.25f,-.30f,.18f);c.Outward=Vector3.down;}
            if(number>=4)c.Spawn=new Vector3(.20f,-.275f,-.20f);
            TapShell(c);
            var floor = c.Surfaces[0];
            if(number==2)c.Spawn=new Vector3(.20f,-.275f,-.19f);
            if (number == 4)
            {
                TapKnife(c);
                TapLabel(c.Root, "Gặp lại", new Vector3(.19f, -.292f, -.14f));
            }
            else BuildTapMechanisms(c, number, floor);
            owner.Spawn = new GameObject("Spawn").transform; owner.Spawn.SetParent(c.Root, false); owner.Spawn.localPosition = c.Spawn;
            owner.Outlet = new GameObject("Final exit").transform; owner.Outlet.SetParent(c.Root, false); owner.Outlet.localPosition = c.Exit;
            owner.Outlet.localRotation = Quaternion.LookRotation(c.Outward, Mathf.Abs(c.Outward.y) > .9f ? Vector3.forward : Vector3.up);
            Ring(owner.Outlet, Vector2.zero, owner.ApertureRadius, .0013f, mint);
            game.Surfaces = c.Surfaces.ToArray(); game.Props = c.Props.ToArray();
            owner.CrawlFaces = new Collider[6]; for (int i = 0; i < 6; i++) owner.CrawlFaces[i] = floor.Shape;
            foreach (var prop in c.Props) prop.transform.SetParent(owner.Apparatus, true);
            var camera = new GameObject("Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.transform.SetParent(owner.transform, false); camera.tag = "MainCamera"; camera.orthographic = true;
            camera.nearClipPlane = .005f; camera.farClipPlane = 15; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.93f, .93f, .89f); camera.transform.rotation = Quaternion.Euler(definition.CameraEuler);
            camera.transform.position = -camera.transform.forward * 3; owner.View = camera;
            Lighting(); COgheDayLabBuilder.ApplyExpansionLevel(game, $"Meshes/TapCampaign/Level{number:00}");
            EditorUtility.SetDirty(definition);
            EditorSceneManager.SaveScene(scene, TapCampaignScenePaths()[number - 1]);
        }

        private static void TapShell(ExpansionContext c)
        {
            bool floorExit=c.Outward==Vector3.down;
            Panel(c.Root, "Laboratory floor", new Vector3(0, -.30f, 0), Vector3.up, new Vector2(.8f, .60f), stone, floorExit,
                new Vector2(-c.Exit.x,c.Exit.z),floorExit?c.Owner.ApertureRadius:0,c.Surfaces);
            Panel(c.Root, "Laboratory ceiling", new Vector3(0, .02f, 0), Vector3.down, new Vector2(.8f, .60f), glass, false, Vector2.zero, 0, c.Surfaces).Selectable = false;
            Panel(c.Root, "Laboratory front", new Vector3(0, -.14f, -.30f), Vector3.forward, new Vector2(.8f, .32f), glass, false, Vector2.zero, 0, c.Surfaces);
            Panel(c.Root, "Laboratory rear", new Vector3(0, -.14f, .30f), Vector3.back, new Vector2(.8f, .32f), glass, false, Vector2.zero, 0, c.Surfaces);
            Panel(c.Root, "Laboratory left", new Vector3(-.4f, -.14f, 0), Vector3.right, new Vector2(.60f, .32f), glass, false, Vector2.zero, 0, c.Surfaces);
            Panel(c.Root, "Laboratory outlet wall", new Vector3(.4f, -.14f, 0), Vector3.left, new Vector2(.60f, .32f), glass, !floorExit,
                new Vector2(c.Exit.z, c.Exit.y + .14f), c.Owner.ApertureRadius, c.Surfaces);
        }
        private static COgheTapRail TapRail(ExpansionContext c, string label, Vector3 start, Vector3 axis, float travel, VenomSurfacePatch floor)
        {
            var rail = ExpansionRail(c, label + " carriage", start, axis, travel, 0, new Vector3(.068f, .026f, .054f), .035f, .010f, false, true);
            rail.LatchAtStart = rail.LatchAtEnd = true;
            var prop = rail.GetComponent<VenomMovableProp>(); prop.Manipulable = false;
            var task = rail.gameObject.AddComponent<COgheTapRail>(); task.Rail = rail; task.Handle = prop.ManipulationGrip; task.WorkingSurface = floor;
            task.Label = label; task.StandOffset = new Vector3(0, 0, -.053f);
            TapLabel(rail.transform, label, new Vector3(0, .038f, -.030f));
            foreach (float end in new[] { 0f, travel })
                MechanismVisual(c.Root, label + " visible socket", start + axis * end + Vector3.down * .011f, new Vector3(.081f, .005f, .065f), metal);
            return task;
        }
        private static Transform TapWheel(Transform parent, string name, Vector3 centre, float radius)
        {
            var wheel = ExpansionWheel(parent, name, centre, radius, 20, 0);
            wheel.localRotation = Quaternion.Euler(90, 0, 0);
            return wheel;
        }
        private static void TapLink(Transform parent, string name, Vector3 from, Vector3 to)
        {
            var link = MechanismVisual(parent, name, (from + to) * .5f, new Vector3(.008f, .008f, Vector3.Distance(from, to)), metal);
            link.localRotation = Quaternion.LookRotation(to - from);
        }
        private static void TapLabel(Transform parent, string text, Vector3 point)
        {
            var label = new GameObject(text).AddComponent<TextMesh>(); label.transform.SetParent(parent, false); label.transform.localPosition = point;
            label.transform.localRotation = Quaternion.Euler(90, 0, 0); label.text = text; label.fontSize = 64; label.characterSize = text.Length == 1 ? .011f : .0045f;
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.color = new Color(.10f, .17f, .21f);
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/World labels.mat");
            if (material != null) label.GetComponent<Renderer>().sharedMaterial = material;
        }
        private static void TapKnife(ExpansionContext c)
        {
            // Straddle adjacent floor routes, so each new half starts on its own side of a route cell.
            const float knifeX=-.177f;
            var knife = ExpansionKnife(c, "Knife", knifeX, -.12f);
            knife.ReturnOnSeparation = true;
            // Let the cutting volume cross the tissue at floor height, without lowering the blunt collision proxy into it.
            knife.Rail.Start=new Vector3(knifeX,-.24f,-.12f);
            knife.Rail.Body.position=c.Root.TransformPoint(knife.Rail.Start+knife.Rail.Axis*knife.Rail.InitialTravel);
            knife.TouchHalfSize = new Vector3(c.Number == 8 || c.Number == 10 ? .025f : .060f, .080f, .070f);
            TapLabel(c.Root, "DAO", new Vector3(knifeX, -.287f, -.22f));
        }
        private static void TapLock(ExpansionContext c, COgheTapRail task, Vector3 input)
        {
            Vector3 point = task.Rail.Start + Vector3.right * .035f + Vector3.up * .01f;
            task.InterlockPin = MechanismVisual(c.Root, task.Label + " locking pin", point, new Vector3(.015f, .050f, .014f), metal);
            TapLink(c.Root, "Lock linkage", input, point);
        }
        private static void BuildTapMechanisms(ExpansionContext c, int number, VenomSurfacePatch floor)
        {
            bool floorHatch=c.Outward==Vector3.down;
            bool selector = number == 7 || number == 8 || number == 10;
            bool load = number == 5 || number == 6 || number == 8 || number == 10;
            bool chain = number == 3 || number == 6 || number == 9 || number == 10;
            bool reverse = number == 2 || number == 9;
            var gateStart=floorHatch?c.Exit+Vector3.up*.008f:new Vector3(.392f,-.22f,.16f);
            var gate = ExpansionRail(c, "Exit rack shutter", gateStart, Vector3.up, .14f, 0,
                floorHatch?new Vector3(.115f,.014f,.115f):new Vector3(.014f,.115f,.115f), .025f, .01f, false, false);
            var train = new GameObject("Visible gear transmission", typeof(COgheGearTrain)).GetComponent<COgheGearTrain>(); train.transform.SetParent(c.Root, false);
            train.Rack = gate; train.ReturnWhenDisconnected = true; train.LatchOutput = load;
            train.MotorSpeed = 2; train.MotorTorque = .06f;
            const float radius = .055f, gearZ = .13f;
            var source = TapWheel(c.Root, "Visible motor", new Vector3(-.15f, -.263f, gearZ), radius);
            MechanismVisual(c.Root, "Motor casing", new Vector3(-.15f, -.286f, gearZ), new Vector3(.07f, .025f, .07f), metal);
            var a = TapRail(c, number == 5 || number == 6 ? "B" : "A", new Vector3(-.04f, -.282f, selector ? -.11f : reverse ? gearZ : -.02f),
                reverse ? Vector3.back : Vector3.forward, selector ? .24f : .15f, floor);
            if (selector) AddTapStops(c, a);
            var movingA = TapWheel(a.transform, "Movable gear A", new Vector3(0, .019f, 0), radius);
            var wheels = new List<Transform> { source, movingA };
            COgheTapRail b = null;
            if (chain)
            {
                b = TapRail(c, number == 6 || number == 10 ? "C" : "B", new Vector3(.07f, -.282f, -.02f), Vector3.forward, .15f, floor);
                b.RequiredRail = a.Rail; if (number != 10) TapLock(c, b, a.Rail.Start + (reverse ? Vector3.back : Vector3.forward) * .15f);
                wheels.Add(TapWheel(b.transform, "Movable gear B", new Vector3(0, .019f, 0), radius));
            }
            if (number == 2)
            {
                b = TapRail(c, "B", new Vector3(-.22f, -.282f, -.19f), Vector3.right, .09f, floor);
                b.RequiredRail = a.Rail; TapLock(c, b, a.Rail.Start + Vector3.back * .15f);
                train.PowerRail = b.Rail;
                TapLink(c.Root, "Motor power linkage", new Vector3(-.13f, -.281f, -.19f), new Vector3(-.15f, -.281f, gearZ));
                TapLabel(c.Root, "NGUỒN", new Vector3(-.27f, -.291f, -.10f));
            }
            COgheTapRail power = null;
            if (selector)
            {
                power = TapRail(c, "B", new Vector3(.19f, -.282f, -.06f), Vector3.right, .08f, floor);
                power.RequiredRail = a.Rail; power.RequiredPosition = .08f;
                TapLock(c, power, a.Rail.Start + Vector3.forward * .08f);
                MechanismVisual(a.transform, "Selector cam", new Vector3(.036f, .008f, 0), new Vector3(.024f, .018f, .025f), metal);
                TapLabel(c.Root, "KHÓA NGUỒN", new Vector3(.25f, -.29f, -.145f));
                TapLink(c.Root, "Motor power linkage", new Vector3(.27f, -.29f, -.06f), new Vector3(-.15f, -.29f, gearZ));
                train.PowerRail = power.Rail;
                if (number == 10) { b.RequiredRail = power.Rail; TapLock(c, b, power.Rail.Start + Vector3.right * .08f); }
            }
            float outputX = chain ? .18f : .07f;
            var output = TapWheel(c.Root, "Output gear", new Vector3(outputX, -.263f, gearZ), radius); wheels.Add(output);
            train.Wheels = wheels.ToArray(); train.PitchRadii = new float[wheels.Count]; train.ToothCounts = new int[wheels.Count];
            for (int i = 0; i < wheels.Count; i++) { train.PitchRadii[i] = radius; train.ToothCounts[i] = 20; }
            // The output shaft leads to a visible bevel pair beside the vertical rack.
            float rackX=floorHatch?-.065f:-.012f;
            float driveX=gateStart.x+rackX-.056f;
            TapLink(c.Root, "Output drive shaft", new Vector3(outputX, -.263f, gearZ), new Vector3(driveX, -.263f, gearZ));
            var pinion=ExpansionWheel(c.Root, "Rack pinion", new Vector3(driveX, -.20f, gateStart.z), .05f, 18, 0);
            TapLink(c.Root, "Bevel drive", new Vector3(driveX, -.263f, gearZ), new Vector3(driveX, -.20f, gateStart.z));
            MechanismVisual(gate.transform, "Rack spine", new Vector3(rackX, .05f, 0), new Vector3(.009f, .22f, .015f), metal);
            for (int i = 0; i < 14; i++) MechanismVisual(gate.transform, "Rack tooth", new Vector3(rackX-.006f, -.045f + i * .014f, 0), new Vector3(.015f, .007f, .015f), metal);
            TapLink(c.Root, "Return spring housing", new Vector3(gateStart.x-.012f, -.27f, gateStart.z+.07f), new Vector3(gateStart.x-.012f, -.05f, gateStart.z+.07f));
            var presentation=train.gameObject.AddComponent<COgheTapGatePresentation>();presentation.Rack=gate;presentation.Pinion=pinion;
            presentation.SpringCoils=new Transform[10];
            for (int i = 0; i < 10; i++) presentation.SpringCoils[i]=MechanismVisual(c.Root, "Return spring coil", new Vector3(gateStart.x-.012f, -.26f + i * .018f, gateStart.z+.07f), new Vector3(.025f, .004f, .025f), metal, PrimitiveType.Cylinder);
            if (load)
            {
                TapKnife(c);
                string padName = selector ? "P" : "A";
                var pad = ExpansionPad(c, padName, new Vector3(-.28f, -.298f, .08f), .005f, .10f);
                var control = pad.gameObject.AddComponent<COgheTapPad>(); control.Sensor = pad;
                control.Surface = c.Surfaces.Find(s => s.name == padName + " sensing surface");
                control.ReleasePoint = new GameObject("A safe release").transform; control.ReleasePoint.SetParent(c.Root, false);
                control.ReleasePoint.localPosition = new Vector3(-.28f, -.278f, -.07f);
                var loaded = selector ? power : a;
                loaded.RequiredLoad = pad; train.InputClutch = pad;
                if (!selector) TapLock(c, loaded, pad.transform.localPosition);
                else TapLink(c.Root, "Pad clutch linkage", pad.transform.localPosition, new Vector3(-.15f, -.286f, gearZ));
                MechanismVisual(gate.transform, "Visible open catch", new Vector3(0, -.055f, -.065f), new Vector3(.028f, .02f, .025f), metal);
                if (number != 10) TapLabel(c.Root, "Gặp lại", new Vector3(.21f, -.292f, selector ? -.23f : -.15f));
            }
        }


        private static void AddTapStops(ExpansionContext c, COgheTapRail task)
        {
            task.Stops = new[] { 0f, .08f, .16f, .24f };
            for (int i = 0; i < task.Stops.Length; i++)
            {
                Vector3 p = task.Rail.Start + task.Rail.Axis * task.Stops[i];
                MechanismVisual(c.Root, "Detent " + (i + 1), p + new Vector3(.042f, -.008f, 0), new Vector3(.018f, .008f, .016f), metal);
                TapLabel(c.Root, (i + 1).ToString(), p + new Vector3(.055f, .085f, 0));
            }
            var view = task.gameObject.AddComponent<COgheTapStopPresentation>(); view.Task = task;
            view.Offset = new Vector3(.082f, .078f, 0);
            view.NextMarker = MechanismVisual(c.Root, "Next detent marker", task.Rail.Start + view.Offset, new Vector3(.012f, .006f, .012f), mint, PrimitiveType.Sphere);
        }

        [MenuItem("Gravity Box/COghe/Use Tap Campaign for Player Build")]
        public static void UseTapCampaignForPlayerBuild()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var path in TapCampaignScenePaths())
            {
                if (!File.Exists(path)) throw new FileNotFoundException("Generate tap campaign first", path);
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            // Keep old entries available for returning to the original campaign, without putting them in this player.
            foreach (var scene in EditorBuildSettings.scenes)
                if (!scenes.Exists(s => s.path == scene.path)) scenes.Add(new EditorBuildSettingsScene(scene.path, false));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("COGHE TAP PLAYER SCENES: 01 through 10 only, in order.");
        }

        [MenuItem("Gravity Box/COghe/Build Tap Campaign · macOS")]
        public static void BuildTapCampaignMac()
        {
            foreach (var path in TapCampaignScenePaths()) if (!File.Exists(path)) throw new FileNotFoundException("Generate tap campaign first", path);
            Directory.CreateDirectory("Builds/COgheTapChapter/macOS");
            string previous = PlayerSettings.productName;
            try
            {
                // Preserve the existing macOS PlayerPrefs namespace; the bundle and in-game name are COghe.
                PlayerSettings.productName = "Venom";
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = TapCampaignScenePaths(), target = BuildTarget.StandaloneOSX,
                    locationPathName = "Builds/COgheTapChapter/macOS/COghe.app", options = BuildOptions.Development });
                if (report.summary.result != BuildResult.Succeeded) throw new Exception("Tap campaign build failed: " + report.summary.result);
                Debug.Log("COGHE TAP BUILD SUCCESS");
            }
            finally { PlayerSettings.productName = previous; }
        }
    }
}
