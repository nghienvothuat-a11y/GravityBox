using System;
using System.Collections.Generic;
using System.IO;
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
        public static string[] Campaign55ScenePaths()
        {
            var paths = new string[55];
            for (int i = 0; i < paths.Length; i++) paths[i] = $"{Campaign30Folder}/{Campaign30ScenePrefix}{i + 1:00}.unity";
            return paths;
        }

        [MenuItem("Gravity Box/COghe/Build Campaign 55 · macOS")]
        public static void BuildCampaign55Mac()
        {
            foreach(var path in Campaign55ScenePaths())if(!File.Exists(path))throw new FileNotFoundException("Generate campaign 55 first",path);
            const string output="Builds/COgheCampaign55/macOS/COghe.app";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            string previous=PlayerSettings.productName;
            try
            {
                PlayerSettings.productName="Venom";
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Campaign55ScenePaths(),target=BuildTarget.StandaloneOSX,
                    locationPathName=output,options=BuildOptions.Development});
                if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Campaign 55 build failed: "+report.summary.result);
                Debug.Log("COGHE CAMPAIGN 55 BUILD SUCCESS: "+output);
            }
            finally{PlayerSettings.productName=previous;}
        }

        [MenuItem("Gravity Box/COghe/Generate Campaign 41–55")]
        public static void GenerateCampaign55()
        {
            foreach (string path in Campaign40ScenePaths())
                if (!File.Exists(path)) throw new FileNotFoundException("Existing campaign must be preserved", path);
            foreach (string path in TapCampaignScenePaths())
                if (!File.Exists(path)) throw new FileNotFoundException("Tap source scene is required", path);
            PrepareCampaign30Assets();
            for (int lesson = 1; lesson <= 10; lesson++)
            {
                int slot = lesson + 40;
                var scene = EditorSceneManager.OpenScene(TapCampaignScenePaths()[lesson - 1], OpenSceneMode.Single);
                var game = Object.FindFirstObjectByType<VenomCampaign>();
                var definition = NewDefinition(slot, slot);
                EditorUtility.CopySerialized(game.Definition, definition);
                definition.name = $"Slot{slot:00}";
                // Stable content identity preserves completion earned in the earlier tap prototype.
                definition.Order = slot;
                definition.Title = $"{slot:00} · {TapNames[lesson - 1]}";
                definition.SceneSequence = Array.Empty<string>();
                game.Definition = definition;
                game.name = $"COghe Campaign {slot:00} · tap lesson {lesson:00}";
                foreach(var label in game.GetComponentsInChildren<TextMesh>(true))
                    if(label.text==lesson.ToString("00"))label.text=slot.ToString("00");
                EditorUtility.SetDirty(definition);
                EditorSceneManager.SaveScene(scene, Campaign55ScenePaths()[slot - 1], true);
            }
            string previousMeshFolder = authoredMeshFolder;
            try
            {
                authoredMeshFolder = Campaign30Folder + "/AssemblyMeshes";
                Directory.CreateDirectory(authoredMeshFolder);
                AssetDatabase.Refresh();
                for (int slot = 51; slot <= 55; slot++) BuildCampaign30Content(slot, slot);
            }
            finally { authoredMeshFolder = previousMeshFolder; }
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in Campaign55ScenePaths()) scenes.Add(new EditorBuildSettingsScene(path, true));
            foreach (var scene in EditorBuildSettings.scenes)
                if (!scenes.Exists(s => s.path == scene.path)) scenes.Add(scene);
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE CAMPAIGN 55 GENERATED: 01–40 preserved; tap content 41–50; assembly 51–55.");
        }

        private static void BuildAssemblyChapter(ExpansionContext c)
        {
            int index = c.Number - 51;
            string[] names = { "Nối hai nhịp", "Đường lệch", "Kéo về đúng chỗ", "Đẩy và kéo", "Xếp đường dài" };
            int count = index == 0 ? 2 : index == 4 ? 4 : 3;
            float[] offsets = index == 1 ? new[] { 0f, .035f, 0f } : index == 4 ? new[] { 0f, .025f, -.025f, 0f } : new float[count];
            ConfigureNew(c, names[index], index == 0 ? "Chạm tay nắm, rồi chỉ đích để đẩy hộp. Ghép mặt cầu tới lỗ sáng." :
                index == 1 ? "Các ổ không thẳng hàng. Quan sát mép cầu để nối đường." :
                index == 2 ? "Hộp đã đi quá đường nối. Bám tay nắm rồi chỉ đích để kéo về." :
                "Quan sát vị trí từng hộp. Đẩy hoặc kéo để nối một đường liên tục.", false, new Vector3(52, -12, 0), .46f);
            c.Spawn = new Vector3(-.255f, -.27f, -.255f);
            c.Exit = new Vector3(.30f, -.076f, 0); c.Outward = Vector3.right;
            ExpansionShell(c, .30f);
            foreach (var face in c.Surfaces)
            {
                if (face.name == "Laboratory floor")
                { face.HasSlipRegion = true; face.SlipRegion = new Rect(-.21f, -.085f, .42f, .17f); }
                else { face.Slippery = true; face.Shape.sharedMaterial = slick; }
            }
            var outlet = c.Surfaces.Find(p => p.Hole); outlet.RingGrip = true; outlet.GripRadius = .068f;
            AssemblyBank(c, "Climbable start bank", new Vector3(-.255f, -.20f, 0), new Vector3(.09f, .20f, .36f), true);
            AssemblyBank(c, "Exit bank", new Vector3(.255f, -.20f, 0), new Vector3(.09f, .20f, .12f), false);
            var plane = new GameObject("Bridge assembly command plane").transform;
            plane.SetParent(c.Root, false); plane.localPosition = new Vector3(0, -.10f, 0);
            var monitor = new GameObject("Bridge sockets", typeof(COgheAssemblyBridge)).GetComponent<COgheAssemblyBridge>();
            monitor.transform.SetParent(c.Root, false); monitor.Rails = new COgheRailSlider[count];
            float cell = .42f / count;
            for (int i = 0; i < count; i++)
            {
                string key = ((char)('A' + i)).ToString(); float x = -.21f + cell * (i + .5f);
                bool reverse = index == 2 || index >= 3 && i % 2 == 1;
                // All handles remain on the front dry aisle. Fixed end sockets show the intended path.
                float destination = offsets[i];
                float travel = reverse ? .14f : .16f;
                Vector3 axis = reverse ? Vector3.back : Vector3.forward;
                var rail = ExpansionRail(c, key + " bridge block", new Vector3(x, -.20f, destination) - axis * travel,
                    axis, travel, 0, new Vector3(cell - .002f, .20f, .09f), .09f, .026f, false, true);
                rail.LatchAtEnd = true; monitor.Rails[i] = rail;
                Object.DestroyImmediate(rail.transform.Find(key + " bridge block stationary handle").gameObject);
                var prop = rail.GetComponent<VenomMovableProp>();
                foreach (var face in prop.GetComponentsInChildren<VenomSurfacePatch>())
                { face.Slippery = Vector3.Dot(face.Normal, Vector3.up) < .9f; if (face.Slippery) face.Shape.sharedMaterial = slick; }
                var grip = new GameObject(key + " low push-pull grip").transform; grip.SetParent(rail.transform, false);
                // Pull handles extend over the slick strip to an accessible dry stance.
                grip.localPosition = new Vector3(0, -.064f, reverse ? -.22f : -.075f);
                grip.localRotation = Quaternion.LookRotation(Vector3.back);
                prop.ManipulationGrip = grip; prop.ManipulationHandleOnly = true; prop.ManipulationPlane = plane;
                if (reverse) MechanismVisual(rail.transform, key + " extended handle stem", new Vector3(0, -.064f, -.13f), new Vector3(.012f, .012f, .19f), metal);
                var handle = MechanismVisual(grip, key + " handle", new Vector3(0, 0, -.011f), new Vector3(.067f, .018f, .040f), metal);
                var shape = handle.gameObject.AddComponent<BoxCollider>(); shape.sharedMaterial = contact; shape.contactOffset = .0003f;
            }
        }

        private static void AssemblyBank(ExpansionContext c, string name, Vector3 position, Vector3 size, bool climb)
        {
            foreach (var normal in new[] { Vector3.up, Vector3.left, Vector3.right, Vector3.forward, Vector3.back })
            {
                float d = Mathf.Abs(normal.x) * size.x + Mathf.Abs(normal.y) * size.y + Mathf.Abs(normal.z) * size.z;
                Vector2 area = Mathf.Abs(normal.y) > .9f ? new Vector2(size.x, size.z) : Mathf.Abs(normal.x) > .9f ? new Vector2(size.z, size.y) : new Vector2(size.x, size.y);
                var face = Panel(c.Root, name + " " + normal, position + normal * d * .5f, normal, area, plastic, false, Vector2.zero, 0, c.Surfaces);
                face.Slippery = !climb && normal != Vector3.up; if (face.Slippery) face.Shape.sharedMaterial = slick;
            }
        }
    }
}
