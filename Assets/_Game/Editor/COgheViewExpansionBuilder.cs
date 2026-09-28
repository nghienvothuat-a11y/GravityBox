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
        private const float ChapterX = .56f, ChapterZ = .40f;
        private static readonly string[] ViewExpansionNames = {
            "Bến bí mật", "Dưới chiếc nắp", "Đi vòng trở lại", "Răng truyền lực", "Chuyền bộ truyền",
            "Hai nhịp cầu", "Ống vòng lưng", "Rẽ nhánh", "Mở rồi đóng", "Cỗ máy vòng",
            "Lối tắt quen", "Tách rồi gặp", "Cùng kéo", "Nhả phanh", "Giữ rồi sang",
            "Nâng đường", "Qua góc tường", "Chọn đường cấp", "Đổi vai", "Cùng trở về" };
        private static readonly string[] ViewExpansionLessons = {
            "Quan sát chỗ cầu vừa rời đi.", "Một đường đi nằm dưới nắp.", "Quan sát đường vòng sau vách.",
            "Chạm A, nhìn bánh răng và cửa cùng chuyển động.", "Một bộ truyền dùng cho hai cửa. Chốt giữ kết quả đã khớp.",
            "Nhường đường cho nhịp cầu còn lại.", "Chạm miệng ống. Ống dẫn tới một bệ khác trong hộp.",
            "Hai nhánh cùng dùng một bộ chọn.", "Có lúc cần đưa cơ quan trở về.", "",
            "Một chuyển động, hai kết quả.", "Chạm dao để tách. Chọn từng phần rồi đưa lại gần để nhập.",
            "Hai tay cùng kéo một cửa. Chốt gài thì có thể buông.", "Một phần nhả phanh, phần kia đưa cầu về bến.",
            "Sang bờ kia rồi làm đường gọi bạn về.", "Hai van cùng nâng đường. Chốt đỡ đường cho cả hai.",
            "Hai mặt tường, một cửa chung.", "Tạo đường về trước, rồi chuyển tuyến tới bệ cửa.",
            "Làm cầu rồi đổi vai để cùng kéo cửa.", "" };

        [MenuItem("Gravity Box/COghe/V2/Generate levels 11–30")]
        public static void GenerateViewExpansion()
        {
            PrepareCampaign30Assets();
            Directory.CreateDirectory(ViewFolder + "/Definitions"); Directory.CreateDirectory(ViewFolder + "/Meshes");
            AssetDatabase.Refresh();
            string previous = authoredMeshFolder;
            try {
                authoredMeshFolder = ViewFolder + "/Meshes";
                string requested = null; var args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-coghe-view-levels") requested = "," + args[i + 1] + ",";
                for (int n = 11; n <= 30; n++) if (requested == null || requested.Contains("," + n + ",")) BuildViewExpansionLevel(n);
            }
            finally { authoredMeshFolder = previous; }
            // Only the catalog data changes in the first chapter; its ten scene assets remain untouched.
            string[] sequence = Array.ConvertAll(ViewCampaignScenePaths(), Path.GetFileNameWithoutExtension);
            for (int n = 1; n <= 10; n++)
            {
                var definition = AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>($"{ViewFolder}/Definitions/View{n:00}.asset");
                definition.SceneSequence = sequence; EditorUtility.SetDirty(definition);
            }
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in ViewCampaignScenePaths()) scenes.Add(new EditorBuildSettingsScene(path, true));
            foreach (var scene in EditorBuildSettings.scenes) if (!scenes.Exists(s => s.path == scene.path)) scenes.Add(scene);
            EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets();
            Debug.Log("COGHE V2 EXPANSION GENERATED: 11–30; first chapter preserved; 30 stable IDs.");
        }
        public static void GenerateViewExpansionAndBuildMac() { GenerateViewExpansion(); BuildMac(); }

        private static void BuildViewExpansionLevel(int n)
        {
            meshSerial = n * 1000;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var owner = new GameObject("COghe V2 · " + ViewExpansionNames[n - 11]).AddComponent<VenomLevelController>();
            Undo.RegisterCreatedObjectUndo(owner.gameObject, "Author V2 expansion");
            owner.MatterProfile = AssetDatabase.LoadAssetAtPath<VenomProfile>(Folder + "/Matter.asset");
            owner.ControlMode = VenomControlMode.TouchSurface;
            owner.RotationProfile = AssetDatabase.LoadAssetAtPath<RotationSettings>("Assets/_Game/PhysicsLab/Profiles/Hand rotation.asset");
            owner.IndicatorMaterial = mint; owner.ApertureRadius = .046f;
            var game = owner.gameObject.AddComponent<VenomCampaign>();
            string path = $"{ViewFolder}/Definitions/View{n:00}.asset";
            var def = AssetDatabase.LoadAssetAtPath<VenomCampaignDefinition>(path);
            if (def == null) { def = ScriptableObject.CreateInstance<VenomCampaignDefinition>(); AssetDatabase.CreateAsset(def, path); }
            def.Id = $"coghe.view.v2.{n:00}"; def.Order = n; def.Title = $"{n:00} · {ViewExpansionNames[n - 11]}";
            def.Lesson = ViewExpansionLessons[n - 11]; def.ViewOnly = true; def.CanRotate = false;
            def.Passive = false; def.Boss = n % 10 == 0; def.ProgressKey = "";
            def.CameraEuler = new Vector3(37, -16, 0); def.CameraZones = Array.Empty<VenomCameraZone>(); def.InitialCameraZone = -1;
            def.SceneSequence = Array.ConvertAll(ViewCampaignScenePaths(), Path.GetFileNameWithoutExtension); game.Definition = def;
            owner.Apparatus = new GameObject("Apparatus").transform; owner.Apparatus.SetParent(owner.transform, false);
            var pivot = new GameObject("Fixed chamber", typeof(Rigidbody), typeof(BoxRotationController)); pivot.transform.SetParent(owner.Apparatus, false);
            pivot.GetComponent<Rigidbody>().isKinematic = true; pivot.GetComponent<Rigidbody>().useGravity = false;
            owner.Rotation = pivot.GetComponent<BoxRotationController>();
            var c = new ExpansionContext { Number = n, Owner = owner, Game = game, Definition = def, Root = pivot.transform,
                Spawn = new Vector3(-.40f, -.250f, -.28f), Exit = new Vector3(.39f, -.30f, .29f), Outward = Vector3.down };
            BuildViewExpansionGeometry(c);
            var floor = c.Surfaces[0];
            owner.Spawn = new GameObject("Spawn").transform; owner.Spawn.SetParent(c.Root, false); owner.Spawn.localPosition = c.Spawn;
            owner.Outlet = new GameObject("Final exit").transform; owner.Outlet.SetParent(c.Root, false); owner.Outlet.localPosition = c.Exit;
            owner.Outlet.localRotation = Quaternion.LookRotation(c.Outward, Mathf.Abs(c.Outward.y) > .9f ? Vector3.forward : Vector3.up);
            Ring(owner.Outlet, Vector2.zero, owner.ApertureRadius, .0032f, mint);
            game.Surfaces = c.Surfaces.ToArray(); game.Props = c.Props.ToArray();
            owner.CrawlFaces = new Collider[6]; for (int i = 0; i < 6; i++) owner.CrawlFaces[i] = floor.Shape;
            foreach (var prop in c.Props) prop.transform.SetParent(owner.Apparatus, true);
            var camera = new GameObject("Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.transform.SetParent(owner.transform, false); camera.tag = "MainCamera"; camera.orthographic = true;
            camera.nearClipPlane = .005f; camera.farClipPlane = 15; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.rotation = Quaternion.Euler(def.CameraEuler); camera.transform.position = -camera.transform.forward * 3; owner.View = camera;
            Lighting();
            string physicalBefore = ChapterPhysicsFingerprint(owner);
            COgheDayLabBuilder.ApplyViewLevel(game);
            if (physicalBefore != ChapterPhysicsFingerprint(owner)) throw new InvalidOperationException("V2 art changed physical/input data in level " + n);
            EditorUtility.SetDirty(def); EditorSceneManager.SaveScene(scene, ViewCampaignScenePaths()[n - 1]);
        }

        private static string ChapterPhysicsFingerprint(VenomLevelController owner)
        {
            var text = new System.Text.StringBuilder();
            foreach (var component in owner.GetComponentsInChildren<Component>(true))
                if (component is Collider || component is Rigidbody || component is Joint || component is VenomSurfacePatch || component is COgheTapRail || component is COgheTubeNetwork || component is COgheCooperativeDrive)
                {
                    text.Append(component.GetType().FullName).Append(EditorJsonUtility.ToJson(component));
                    text.Append(component.transform.localToWorldMatrix.ToString("R"));
                }
            return text.ToString();
        }

        private static VenomSurfacePatch ChapterFloor(ExpansionContext c, string name, Vector3 centre, Vector2 size, bool hole = false)
            => Panel(c.Root, name, centre, Vector3.up, size, stone, hole,
                new Vector2(centre.x - c.Exit.x, c.Exit.z - centre.z), hole ? c.Owner.ApertureRadius : 0, c.Surfaces);
        private static void ChapterShell(ExpansionContext c, bool floor = true)
        {
            if (floor) ChapterFloor(c, "Laboratory floor", new Vector3(0, -.30f, 0), new Vector2(ChapterX * 2, ChapterZ * 2), c.Outward == Vector3.down);
            var centres = new[] { new Vector3(0, .10f, 0), new Vector3(0, -.10f, -ChapterZ), new Vector3(0, -.10f, ChapterZ), new Vector3(-ChapterX, -.10f, 0), new Vector3(ChapterX, -.10f, 0) };
            var normals = new[] { Vector3.down, Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
            for (int i = 0; i < centres.Length; i++)
            {
                bool hole = Vector3.Dot(normals[i], -c.Outward) > .99f && Mathf.Abs(Vector3.Dot(c.Exit - centres[i], normals[i])) < .001f;
                Vector3 local = Quaternion.Inverse(Quaternion.LookRotation(normals[i], i == 0 ? Vector3.forward : Vector3.up)) * (c.Exit - centres[i]);
                var patch = Panel(c.Root, "Outer pane " + i, centres[i], normals[i], i == 0 ? new Vector2(ChapterX * 2, ChapterZ * 2) : new Vector2(i < 3 ? ChapterX * 2 : ChapterZ * 2, .40f), glass, hole, new Vector2(local.x, local.y), hole ? c.Owner.ApertureRadius : 0, c.Surfaces);
                patch.ExteriorGlass = true; patch.Selectable = i != 0;
            }
        }
        private static COgheTapRail ChapterTask(ExpansionContext c, string label, float x, float z, float travel = .10f, VenomSurfacePatch floor = null)
            => ViewTask(c, label, new Vector3(x, -.277f, z), Vector3.right, travel, floor ?? c.Surfaces[0]);
        private static COgheRailSlider ChapterFinal(ExpansionContext c)
            => ViewGate(c, "Final floor shutter", c.Exit + Vector3.up * .012f, c.Exit.z < -.25f ? Vector3.forward : Vector3.back, .14f, new Vector3(.13f, .018f, .13f));
        private static COgheViewTransmission ChapterDrive(ExpansionContext c, COgheRailSlider power, COgheRailSlider selector,
            COgheRailSlider first, COgheRailSlider second = null, bool final = true)
        {
            var drive = new GameObject("Two-station measured transmission", typeof(COgheViewTransmission)).GetComponent<COgheViewTransmission>();
            drive.transform.SetParent(c.Root, false); drive.Power = power; drive.Selector = selector; drive.First = first; drive.Second = second;
            drive.FirstIsFinal = final && second == null; drive.SecondIsFinal = final && second != null;
            if (final) { if (second == null) drive.FirstAperture = ViewExitSurface(c); else drive.SecondAperture = ViewExitSurface(c); }
            var moving = selector ?? power;
            Vector3 middle = moving.Start + new Vector3(0, .080f, .025f);
            if (selector == null) middle += power.Axis * power.Travel;
            var shared = ExpansionWheel(moving.transform, "Shared travelling gear", new Vector3(0, .080f, .025f), .025f, 18, 0);
            Transform[] Wheels(Vector3 p, string station) => new[] {
                ExpansionWheel(c.Root, station + " source", p + Vector3.up * .05f, .025f, 18, 10),
                shared,
                ExpansionWheel(c.Root, station + " output", p - Vector3.up * .05f, .025f, 18, 10) };
            drive.FirstWheels = Wheels(middle, "I");
            if (second != null) drive.SecondWheels = Wheels(middle + selector.Axis * selector.Travel, "II");
            TapLink(c.Root, "Visible linkage housing", middle - Vector3.up * .05f, first.Start);
            if (second != null) TapLink(c.Root, "Visible linkage housing", middle + selector.Axis * selector.Travel - Vector3.up * .05f, second.Start);
            drive.FirstPin = MechanismVisual(first.transform, "Real terminal catch I", new Vector3(.07f, .03f, 0), new Vector3(.026f, .012f, .018f), metal);
            if (second != null) drive.SecondPin = MechanismVisual(second.transform, "Real terminal catch II", new Vector3(.07f, .03f, 0), new Vector3(.026f, .012f, .018f), metal);
            return drive;
        }
        private static COgheRailSlider ChapterPartition(ExpansionContext c, string name, float z, float openingX = .28f, float width = .20f)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 normal = Vector3.forward * side;
                float low = openingX - width * .5f, high = openingX + width * .5f;
                Panel(c.Root, name + " left wall", new Vector3((-ChapterX + low) * .5f, -.10f, z + side * .006f), normal, new Vector2(low + ChapterX, .40f), stone, false, Vector2.zero, 0, c.Surfaces);
                Panel(c.Root, name + " right wall", new Vector3((ChapterX + high) * .5f, -.10f, z + side * .006f), normal, new Vector2(ChapterX - high, .40f), stone, false, Vector2.zero, 0, c.Surfaces);
                Panel(c.Root, name + " header", new Vector3(openingX, 0, z + side * .006f), normal, new Vector2(width, .20f), stone, false, Vector2.zero, 0, c.Surfaces);
            }
            return ViewGate(c, name, new Vector3(openingX, -.20f, z - .020f), Vector3.up, .19f, new Vector3(width + .004f, .20f, .018f));
        }
        private static void ChapterKnife(ExpansionContext c)
        {
            float x = c.Number == 24 || c.Number == 25 || c.Number == 26 || c.Number >= 29 ? -.42f : -.32f;
            if (x < -.4f) c.Spawn = new Vector3(-.48f, -.25f, -.28f);
            var knife = ExpansionKnife(c, "Knife", x, -.19f); knife.ReturnOnSeparation = true; knife.HoldUntilTissueClears = true;
            knife.Rail.Start = new Vector3(x, -.24f, -.19f);
            knife.Rail.Body.position = c.Root.TransformPoint(knife.Rail.Start + knife.Rail.Axis * knife.Rail.InitialTravel);
            knife.TouchHalfSize = new Vector3(.06f, .08f, .07f);
        }
        private static COgheTissueSensor ChapterPad(ExpansionContext c, Vector3? location = null)
        {
            var point = location ?? new Vector3(-.43f, -.297f, .20f);
            var pad = ExpansionPad(c, "A", point, .012f, .125f);
            var tap = pad.gameObject.AddComponent<COgheTapPad>(); tap.Sensor = pad;
            tap.Surface = c.Surfaces.Find(s => s.name == "A sensing surface");
            tap.ReleasePoint = new GameObject("A safe release").transform; tap.ReleasePoint.SetParent(c.Root, false);
            tap.ReleasePoint.localPosition = new Vector3(-.43f, -.28f, .03f);
            TapLabel(c.Root, "A", point + Vector3.up*.01f);
            return pad;
        }
        private static void BuildViewExpansionGeometry(ExpansionContext c)
        {
            int n = c.Number;
            if (n >= 23) { BuildSimultaneousLevel(c); return; }
            if (n == 15) { ChapterTransmission(c); return; }
            if (n == 18) { ChapterBranches(c); return; }
            if (n >= 22) { ChapterCooperation(c); return; }
            ChapterDiscovery(c);
        }
        private static void ChapterTransmission(ExpansionContext c)
        {
            ChapterShell(c);
            var first = ChapterPartition(c, "I access door", .04f);
            var second = ChapterFinal(c);
            var power = ChapterTask(c, "P", -.43f, -.19f);
            var selector = ChapterTask(c, "G", -.13f, -.19f, .20f);
            ChapterDrive(c, power.Rail, selector.Rail, first, second);
        }
        // Layout families below use the same physical kit; each configures distinct spatial dependencies.
    }
}
