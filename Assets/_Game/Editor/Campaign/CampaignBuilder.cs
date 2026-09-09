using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GravityBox.App;
using GravityBox.Foundation;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GravityBox.Editor
{
    public static class CampaignBuilder
    {
        public const string Folder = "Assets/_Game/Campaign";
        public const string CatalogPath = Folder + "/CampaignCatalog.asset";
        public const string ScenePath = "Assets/_Game/Scenes/Campaign.unity";
        [Serializable] private sealed class Manifest { public CampaignLevelSpec[] levels; }

        [MenuItem("Gravity Box/Campaign/Generate 100 Levels")]
        public static void Generate()
        {
            foreach (string folder in new[] { "Levels", "Profiles", "Prefabs", "Meshes", "Materials" }) Directory.CreateDirectory(Folder + "/" + folder);
            AssetDatabase.Refresh();
            var lab = AssetDatabase.LoadAssetAtPath<LevelCatalog>(PrototypeBuilder.CatalogPath);
            if (lab == null || lab.Levels.Length != 23) throw new InvalidOperationException("Campaign requires the preserved 23-level Physics Lab.");
            var specs = JsonUtility.FromJson<Manifest>("{\"levels\":" + File.ReadAllText("Docs/CAMPAIGN_100_LEVELS.json") + "}").levels;
            if (specs == null || specs.Length != 100) throw new InvalidOperationException("Campaign manifest must contain exactly 100 authored levels.");
            var context = new CampaignBuildContext(lab);
            var catalog = Asset<LevelCatalog>(CatalogPath);
            catalog.Id = "gravity-box-campaign-v1"; catalog.DisplayName = "Hành trình 100 màn"; catalog.IsCampaign = true;
            catalog.BallPrefab = lab.BallPrefab; catalog.BallProfile = lab.BallProfile; catalog.Rotation = lab.Rotation;
            catalog.CompletionDelay = 1.15f; catalog.FailureDelay = lab.FailureDelay;
            catalog.Levels = new LevelDefinition[specs.Length];
            foreach (CampaignLevelSpec spec in specs)
            {
                if (spec.index < 1 || spec.index > 100 || catalog.Levels[spec.index - 1] != null || spec.boss != (spec.index % 10 == 0))
                    throw new InvalidOperationException("Invalid campaign slot " + spec.index);
                LevelRuntime authored = null;
                using var geometryScope = new GeometryAssetScope(Folder + "/Meshes", $"C{spec.index:000}-");
                try
                {
                    authored = spec.boss ? BossCampaignBuilder.Build(spec, context)
                        : EasyCampaignBuilder.Handles(spec.index) ? EasyCampaignBuilder.Build(spec, context)
                        : HardCampaignBuilder.Handles(spec.index) ? HardCampaignBuilder.Build(spec, context)
                        : throw new InvalidOperationException("No author assigned for campaign " + spec.index);
                    if (authored == null) throw new InvalidOperationException("Missing authored content for " + spec.index);
                    authored.name = $"C{spec.index:000} {spec.title}";
                    context.ApplyCampaignMaterials(authored);
                    PersistTransientAssets(authored, spec.index);
                    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(authored.gameObject, $"{Folder}/Prefabs/C{spec.index:000}.prefab");
                    var definition = Asset<LevelDefinition>($"{Folder}/Levels/C{spec.index:000}.asset");
                    definition.Id = $"campaign-{spec.index:000}"; definition.DisplayIndex = spec.index; definition.DisplayName = spec.title;
                    definition.Prefab = prefab.GetComponent<LevelRuntime>(); definition.Environment = lab.Levels[0].Environment;
                    WaterVolume fluid = authored.GetComponent<WaterVolume>();
                    definition.Shape = fluid != null ? (fluid.Profile.Density > 10000 ? ContainerShape.MercuryBox : ContainerShape.WaterBox)
                        : lab.Levels[Mathf.Clamp(spec.source_prototypes[0] - 1, 0, lab.Levels.Length - 1)].Shape;
                    definition.RotationMode = RotationMode.Free; definition.InitialLocalVelocity = Vector3.zero;
                    definition.Tutorial = spec.role == "intro"; definition.ParSeconds = 0;
                    definition.TeachingHint = TeachingHint(spec);
                    definition.DesignerSolution = spec.skill + "\nRecovery: " + spec.recovery;
                    var design = Asset<LevelDesignProfile>($"{Folder}/Profiles/C{spec.index:000}.asset");
                    design.Chapter = spec.chapter; design.IsBoss = spec.boss;
                    design.Role = (CampaignRole)Enum.Parse(typeof(CampaignRole), spec.role, true);
                    design.SourcePrototypes = spec.source_prototypes; design.DifficultyBudget = spec.target_d;
                    design.IntroducedSkills = CampaignCurriculum.Introduced(spec.index); design.RequiredSkills = CampaignCurriculum.Required(spec);
                    design.DesignIntent = spec.skill; design.RecoveryPlan = spec.recovery; design.RecoveryBudgetSeconds = spec.boss ? 20 : 10;
                    design.ImplementedByModel = !spec.boss && EasyCampaignBuilder.Handles(spec.index) ? "gpt-5.6-sol" : "gpt-6-astra";
                    design.ValidationNotes = "Difficulty budget is an authoring ceiling; human playtest calibration pending. See Docs/Verification/Campaign100 for actual validation evidence.";
                    definition.Design = design; catalog.Levels[spec.index - 1] = definition;
                    EditorUtility.SetDirty(design); EditorUtility.SetDirty(definition);
                    Debug.Log($"CAMPAIGN AUTHORED {spec.index:000}: {spec.title} [{design.ImplementedByModel}]");
                }
                finally { if (authored != null) UnityEngine.Object.DestroyImmediate(authored.gameObject); }
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            BuildScene(catalog, lab);
            CampaignValidator.Validate();
            Debug.Log("CAMPAIGN GENERATED: 100 levels, 10 bosses, preserved Physics Lab.");
        }

        public static void GenerateAndAudit() { Generate(); CampaignGeometryAudit.Run(); }

        private static string TeachingHint(CampaignLevelSpec spec)
        {
            switch (spec.index)
            {
                case 10: return "Đưa bi men theo má sư tử xuống lỗ ở miệng. Cả hai phía đều có đường.";
                case 20: return "Giữ bi trong hốc khi đổi hướng mở cửa. Để cầu gập xuống bệ rồi đưa bi qua.";
                case 30: return "Nhìn vùng đỡ kế tiếp. Xoay từng hướng để đưa bi qua chòm sao.";
                case 40: return "Đưa A ra đầu tay đòn để nâng B. Dùng B ấn chốt, rồi đưa cả hai bi ra.";
                case 50: return "Đẩy rồi nhả thanh đẩy để giữ từng nấc. Qua cầu, mở cam còn lại và về tâm sen.";
                case 60: return "Đợi con lắc rời lối, chuẩn bị ở sân nghỉ rồi nghiêng để đón bi.";
                case 70: return "Bi chìm trong nước. Nghiêng sớm, quan sát quán tính trước mỗi góc rẽ.";
                case 80: return "Bi nổi trong thủy ngân: dẫn bi theo mặt cao. Hình nhìn xuyên giúp quan sát.";
                case 90: return "Đổi mặt đỡ qua các nhánh. Chọn mốc tiếp theo trước khi xoay cả khối cầu.";
                case 100: return "Giúp hai bi mở đường cho nhau. Giữ nấc cam, qua lồng rồi đón từng bi ở sân cuối.";
            }
            string text = spec.skill.Split(new[] { " Bố trí:" }, StringSplitOptions.None)[0].Split(';')[0].Trim();
            text = Regex.Replace(text, @"^(Giới thiệu[^:]*|Luyện[^:]*|Tổng hợp[^:]*|Kết hợp[^:]*|Đối chứng):\s*", "");
            text = text.Replace("rack", "thanh đẩy").Replace("mechanic", "cơ chế");
            if (text.Length > 0 && !text.EndsWith(".")) text += ".";
            return text.Length == 0 ? "Xoay nhẹ. Đưa tất cả bi ra khỏi lỗ tròn." : char.ToUpperInvariant(text[0]) + text.Substring(1);
        }

        private static T Asset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }

        private static void PersistTransientAssets(LevelRuntime level, int index)
        {
            var meshes = new Dictionary<Mesh, Mesh>(); int meshIndex = 0, materialIndex = 0;
            Mesh SaveMesh(Mesh mesh)
            {
                if (mesh == null || AssetDatabase.Contains(mesh)) return mesh;
                if (meshes.TryGetValue(mesh, out Mesh saved)) return saved;
                string path = $"{Folder}/Meshes/C{index:000}-{meshIndex++:000}.asset";
                saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (saved == null) { saved = mesh; AssetDatabase.CreateAsset(saved, path); }
                else EditorUtility.CopySerialized(mesh, saved);
                meshes.Add(mesh, saved); EditorUtility.SetDirty(saved); return saved;
            }
            foreach (MeshFilter filter in level.GetComponentsInChildren<MeshFilter>(true)) filter.sharedMesh = SaveMesh(filter.sharedMesh);
            foreach (MeshCollider collider in level.GetComponentsInChildren<MeshCollider>(true)) collider.sharedMesh = SaveMesh(collider.sharedMesh);
            var materials = new Dictionary<Material, Material>();
            foreach (Renderer renderer in level.GetComponentsInChildren<Renderer>(true))
            {
                Material[] list = renderer.sharedMaterials;
                for (int i = 0; i < list.Length; i++)
                {
                    Material material = list[i];
                    if (material == null || AssetDatabase.Contains(material)) continue;
                    if (!materials.TryGetValue(material, out Material saved))
                    {
                        string path = $"{Folder}/Materials/C{index:000}-{materialIndex++:000}.mat";
                        saved = AssetDatabase.LoadAssetAtPath<Material>(path);
                        if (saved == null) { saved = material; AssetDatabase.CreateAsset(saved, path); }
                        else EditorUtility.CopySerialized(material, saved);
                        materials.Add(material, saved); EditorUtility.SetDirty(saved);
                    }
                    list[i] = saved;
                }
                renderer.sharedMaterials = list;
            }
        }

        private static void BuildScene(LevelCatalog campaign, LevelCatalog lab)
        {
            var scene = EditorSceneManager.OpenScene(PrototypeBuilder.ScenePath);
            GameBootstrap bootstrap = UnityEngine.Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null) throw new InvalidOperationException("Missing prototype composition root.");
            bootstrap.Catalog = campaign; bootstrap.PhysicsLab = lab; bootstrap.ResumeCampaign = true;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Gravity Box/Campaign/Build macOS")]
        public static void BuildMac()
        {
            BuildCampaign(BuildTarget.StandaloneOSX, "Builds/macOS/Gravity Box.app");
        }

        [MenuItem("Gravity Box/Campaign/Build Android Development APK")]
        public static void BuildAndroid()
        {
            PrototypeBuilder.ConfigureProject();
            EditorUserBuildSettings.buildAppBundle = false;
            AssetDatabase.SaveAssets();
            const string path = "Builds/Android/GravityBox.apk";
            BuildCampaign(BuildTarget.Android, path);
            File.Copy(path, "Builds/Android/GravityBox-Campaign100.apk", true);
        }

        private static void BuildCampaign(BuildTarget target, string path)
        {
            CampaignValidator.Validate();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, target = target, locationPathName = path, options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Campaign build failed: " + report.summary.result);
            Debug.Log($"GRAVITY BOX CAMPAIGN BUILD SUCCESS: {target} {path} ({report.summary.totalSize} bytes)");
        }
    }
}
