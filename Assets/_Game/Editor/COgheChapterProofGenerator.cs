using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>Reuses the actual solution tests in an opt-in Development player.
    /// Generate before building, in a separate import/compilation pass. A build
    /// fails if test edits have made the generated evidence scenarios stale.</summary>
    public sealed class COgheChapterProofGenerator : IPreprocessBuildWithReport
    {
        private const string TestRoot = "Assets/_Game/Tests/PlayMode/";
        private const string OutputRoot = "Assets/_Game/Venom/Runtime/ChapterProof/Generated/";
        private const string Guard = "#if DEVELOPMENT_BUILD && !UNITY_EDITOR\n";
        private static readonly string[] Sources =
        {
            "COgheBoss30ScreenTests", "COgheCampaign40RouteTests", "COgheCampaign40CooperationTests", "COgheCampaign40BossTests"
        };
        public int callbackOrder => -1000;

        [MenuItem("Gravity Box/COghe/Generate Chapter 31–40 Player Proof")]
        public static void Generate()
        {
            var files = BuildFiles();
            Directory.CreateDirectory(OutputRoot);
            foreach (var pair in files)
                if (!File.Exists(pair.Key) || File.ReadAllText(pair.Key) != pair.Value)
                    File.WriteAllText(pair.Key, pair.Value, new UTF8Encoding(false));
            AssetDatabase.Refresh();
            Debug.Log("COGHE_CHAPTER_PROOF_GENERATED " + OutputRoot);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if ((report.summary.options & BuildOptions.Development) == 0) return;
            foreach (var pair in BuildFiles())
                if (!File.Exists(pair.Key) || File.ReadAllText(pair.Key) != pair.Value)
                    throw new BuildFailedException("Chapter proof is stale: " + pair.Key +
                        ". Run GravityBox.Editor.COgheChapterProofGenerator.Generate, allow script import, then build again.");
        }

        private static Dictionary<string, string> BuildFiles()
        {
            var files = new Dictionary<string, string>();
            var methods = new SortedDictionary<int, (string type, string name, bool asyncSetup, bool asyncTeardown)>();
            var hashes = new List<string>();
            foreach (string type in Sources)
            {
                string path = TestRoot + type + ".cs";
                string source = File.ReadAllText(path);
                string hash;
                using (var sha = SHA256.Create())
                    hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
                hashes.Add(type + ":" + hash);
                string replayType = type.Replace("Tests", "Replay");
                foreach (Match method in Regex.Matches(source,
                    @"\[UnityTest\]\s*public\s+IEnumerator\s+((?:Level|Campaign)(3[0-9]|40)\w+)\s*\("))
                {
                    int level = int.Parse(method.Groups[2].Value);
                    if (methods.ContainsKey(level)) throw new InvalidOperationException("Duplicate solution for level " + level);
                    methods.Add(level, (replayType, method.Groups[1].Value,
                        Regex.IsMatch(source, @"public\s+IEnumerator\s+Before\s*\("),
                        Regex.IsMatch(source, @"public\s+IEnumerator\s+After\s*\(")));
                }

                source = source.Replace("using NUnit.Framework;", "").Replace("using UnityEngine.TestTools;", "")
                    .Replace("namespace GravityBox.Tests", "namespace GravityBox.Venom.ChapterProof")
                    .Replace(type, replayType);
                source = Regex.Replace(source, @"\[(?:UnityTest|UnitySetUp|UnityTearDown|SetUp|TearDown)\]\s*", "");
                // Test teardown must not re-enable the real campaign save while
                // the explicit evidence session remains open in the player.
                source = Regex.Replace(source, @"VenomCampaignSave\.PersistenceEnabled\s*=\s*[^;]+;",
                    "VenomCampaignSave.PersistenceEnabled = false;");
                source = source.Replace("COgheExpansionIntegrationTests.Capture(",
                    "yield return COgheChapterProofCapture.Request(");
                int exits = 0;
                source = Regex.Replace(source, @"game\.Motion\.Move\([^;]*game\.Owner\.Outlet[^;]*;", match =>
                {
                    exits++;
                    return "yield return COgheChapterProofCapture.Request(game, \"before-exit\");\n            " + match.Value;
                });
                if (type == "COgheBoss30ScreenTests")
                {
                    source = source.Replace("\"30-screen-before-exit\"", "\"before-exit\"");
                    if (exits != 0 || !source.Contains("Tap(game.Owner.Outlet.position)"))
                        throw new InvalidOperationException("Boss 30 proof must use its screen exit command.");
                }
                else if (exits != 1) throw new InvalidOperationException(type + " must contain one shared real outlet command; found " + exits);
                if (source.Contains("NUnit.") || source.Contains("UnityEngine.TestTools") || source.Contains("COgheExpansionIntegrationTests"))
                    throw new InvalidOperationException("Unhandled test-only dependency in " + type);
                files.Add(OutputRoot + replayType + ".cs", Guard +
                    "// Generated from " + path + "\n// SHA256 " + hash + "\n// Do not edit; regenerate after test changes.\n" + source + "\n#endif\n");
            }
            if (!methods.Keys.SequenceEqual(Enumerable.Range(30, 11)))
                throw new InvalidOperationException("Expected exactly one full solution for each campaign level 30–40.");

            var registry = new StringBuilder(Guard +
                "// Generated. Explicit calls preserve scenarios under player stripping.\n" +
                "using System;\nusing System.Collections;\nnamespace GravityBox.Venom.ChapterProof\n{\n" +
                "    internal static class COgheChapterProofScenarios\n    {\n" +
                "        internal const string SourceManifest = \"" + string.Join(";", hashes) + "\";\n" +
                "        internal static IEnumerator Run(int level)\n        {\n            switch (level)\n            {\n");
            foreach (var pair in methods)
            {
                var item = pair.Value;
                registry.Append("                case ").Append(pair.Key).Append(":\n                {\n")
                    .Append("                    var scenario = new ").Append(item.type).Append("();\n")
                    .Append("                    ").Append(item.asyncSetup ? "yield return " : "").Append("scenario.Before();\n")
                    .Append("                    yield return scenario.").Append(item.name).Append("();\n")
                    .Append("                    ").Append(item.asyncTeardown ? "yield return " : "").Append("scenario.After();\n")
                    .Append("                    break;\n                }\n");
            }
            registry.Append("                default: throw new ArgumentOutOfRangeException(nameof(level));\n" +
                "            }\n        }\n    }\n}\n#endif\n");
            files.Add(OutputRoot + "COgheChapterProofScenarios.cs", registry.ToString());
            return files;
        }
    }
}
