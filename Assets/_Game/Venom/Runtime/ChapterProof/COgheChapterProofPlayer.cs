#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace GravityBox.Venom.ChapterProof
{
    /// <summary>Opt-in player evidence using the exact physical solution tests.
    /// The only additions are assertions, framebuffer captures and presentation
    /// time after an earned victory. This is an automated player-command replay,
    /// not evidence of a human touch-only playthrough or mobile performance.</summary>
    public sealed class COgheChapterProofPlayer : MonoBehaviour
    {
        [Serializable] private sealed class FrameEvidence
        {
            public int level, escaped, fragments, width, height;
            public bool completed, lost, exitAvailable;
            public float simulationTime;
            public string stage, label, png, sha256, utc;
        }
        [Serializable] private sealed class LevelEvidence
        {
            public int level;
            public bool passed;
            public string error, sourceManifest;
            public List<FrameEvidence> frames = new List<FrameEvidence>();
        }
        [Serializable] private sealed class RunEvidence
        {
            public string startedUtc, endedUtc, unityVersion, applicationVersion, platform, device, gpu, sourceManifest;
            public string execution = "Generated solution commands; 1/120-second Physics.Simulate; actual player framebuffer with HUD.";
            public int passed, failed;
            public List<string> runtimeErrors = new List<string>();
            public List<LevelEvidence> levels = new List<LevelEvidence>();
        }

        private static string requestedDirectory;
        private string directory;
        private RunEvidence run;
        private LevelEvidence current;
        private bool hasOpen, hasWin;
        private const float Dt = 1f / 120f;

        private void OnEnable() => Application.logMessageReceived += ObserveLog;
        private void OnDestroy() => Application.logMessageReceived -= ObserveLog;
        private void ObserveLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            string error = type + ": " + message + "\n" + trace;
            run?.runtimeErrors.Add(error);
            if (current != null && string.IsNullOrEmpty(current.error)) current.error = error;
        }

        private static string Argument(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == flag) return args[i + 1];
            return null;
        }
        private static bool HasFlag(string flag) => Array.IndexOf(Environment.GetCommandLineArgs(), flag) >= 0;
        private static void IsolatePersistence()
        {
            VenomCampaignSave.PersistenceEnabled = false;
            VenomGuidanceMemory.PersistenceEnabled = false;
            VenomJourneyProgress.PersistenceEnabled = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PrepareRequestedRun()
        {
            requestedDirectory = Argument("-coghe-chapter-proof-directory");
            if (!string.IsNullOrWhiteSpace(requestedDirectory)) IsolatePersistence();
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartRequestedRun()
        {
            if (string.IsNullOrWhiteSpace(requestedDirectory)) return;
            var holder = new GameObject("COghe explicit chapter player proof");
            DontDestroyOnLoad(holder);
            holder.AddComponent<COgheChapterProofPlayer>();
        }

        private IEnumerator Start()
        {
            Application.runInBackground = true;
            IsolatePersistence();
            // A unique run directory preserves previous evidence and prevents
            // an interrupted run from being mistaken for a new complete one.
            directory = Path.Combine(Path.GetFullPath(requestedDirectory), DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ"));
            Directory.CreateDirectory(directory);
            run = new RunEvidence
            {
                startedUtc = DateTime.UtcNow.ToString("O"), unityVersion = Application.unityVersion,
                applicationVersion = Application.version, platform = Application.platform.ToString(),
                device = SystemInfo.deviceModel, gpu = SystemInfo.graphicsDeviceName,
                sourceManifest = COgheChapterProofScenarios.SourceManifest
            };
            Debug.Log("COGHE_CHAPTER_PROOF_STARTED " + directory);
            Physics.simulationMode = SimulationMode.Script;
            Time.timeScale = 1;
            int firstLevel = 31, lastLevel = 40;
            string requestedLevel = Argument("-coghe-chapter-proof-level");
            if (requestedLevel != null)
            {
                if (!int.TryParse(requestedLevel, out firstLevel) || firstLevel < 30 || firstLevel > 40)
                    throw new ArgumentException("Proof level must be between 30 and 40.");
                lastLevel = firstLevel;
            }
            for (int level = firstLevel; level <= lastLevel; level++)
            {
                current = new LevelEvidence { level = level, sourceManifest = run.sourceManifest };
                run.levels.Add(current); hasOpen = hasWin = false;
                Debug.Log("COGHE_CHAPTER_PROOF_LEVEL_START " + level);
                yield return ExecuteScenario(COgheChapterProofScenarios.Run(level));
                current.passed = string.IsNullOrEmpty(current.error) && hasOpen && hasWin;
                if (!current.passed && string.IsNullOrEmpty(current.error)) current.error = "Scenario ended without both real open and won captures.";
                if (current.passed) run.passed++; else run.failed++;
                File.WriteAllText(Path.Combine(directory, $"level-{level:00}.json"), JsonUtility.ToJson(current, true));
                SaveRun();
                Debug.Log($"COGHE_CHAPTER_PROOF_LEVEL_DONE {level} passed={current.passed} directory={directory}");
                // Tests may restore simulation mode in teardown. The next
                // scenario still owns every physics step and the isolated save.
                Physics.simulationMode = SimulationMode.Script; IsolatePersistence();
            }
            run.endedUtc = DateTime.UtcNow.ToString("O"); SaveRun();
            Debug.Log($"COGHE_CHAPTER_PROOF_DONE passed={run.passed} failed={run.failed} directory={directory}");
            if (HasFlag("-coghe-chapter-proof-quit")) Application.Quit(run.failed == 0 ? 0 : 1);
            // Keep persistence isolated for the entire explicit process. With
            // no quit flag the final scenario remains visible for inspection.
        }

        private IEnumerator ExecuteScenario(IEnumerator scenario)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(scenario);
            while (stack.Count > 0)
            {
                bool advanced = false; object yielded = null; Exception failure = null;
                try
                {
                    IsolatePersistence();
                    advanced = stack.Peek().MoveNext();
                    if (advanced) yielded = stack.Peek().Current;
                    IsolatePersistence();
                }
                catch (Exception error) { failure = error; }
                if (failure != null)
                {
                    current.error = failure.ToString(); Debug.LogError("COGHE_CHAPTER_PROOF_FAILED " + current.level + " " + failure);
                    while (stack.Count > 0)
                    {
                        try { (stack.Pop() as IDisposable)?.Dispose(); }
                        catch (Exception cleanup) { Debug.LogException(cleanup); }
                    }
                    IsolatePersistence();
                    yield break;
                }
                if (!advanced)
                {
                    (stack.Pop() as IDisposable)?.Dispose(); continue;
                }
                if (yielded is COgheChapterProofCapture request)
                { stack.Push(Capture(request)); continue; }
                if (yielded is IEnumerator nested)
                { stack.Push(nested); continue; }
                yield return yielded;
            }
        }

        private IEnumerator Capture(COgheChapterProofCapture request)
        {
            var game = request.Game;
            Assert.NotNull(game, "Capture requires the live scenario.");
            Assert.AreEqual(current.level, game.Definition.Order, "Capture must match its scenario.");
            string stage;
            if (request.Label == "before-exit")
            {
                Assert.IsFalse(game.Owner.Completed); Assert.IsFalse(game.Owner.Lost);
                Assert.IsTrue(game.FinalExitAvailable); Assert.AreEqual(1, game.Matter.TotalFragmentCount);
                Assert.AreEqual(0, game.Matter.EscapedCount, "Whole-level capture precedes the exit command.");
                stage = "open";
                game.CameraRig.SelectZone(-1);
                game.CameraRig.Frame(Screen.width, Screen.height, 0, true);
            }
            else if (game.Owner.Completed)
            {
                Assert.IsFalse(game.Owner.Lost); Assert.AreEqual(32, game.Matter.EscapedCount);
                Assert.AreEqual(1, game.Matter.TotalFragmentCount);
                stage = "won";
                // Let the existing celebration gather physical tissue and
                // frame it. All movement still comes from normal Step/Simulate.
                for (int i = 0; i < 168; i++)
                {
                    game.Owner.Step(Dt); game.Owner.Rotation.Step(Dt); Physics.Simulate(Dt);
                    if (i % 4 == 0) yield return null;
                }
            }
            else if (request.Label.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0) stage = "failed";
            else yield break; // The injected before-exit request owns overview capture.

            // LateUpdate uses the production camera, creature mesh and visible
            // mechanisms. End-of-frame includes the real OnGUI victory HUD.
            yield return null;
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            Assert.NotNull(texture, "Player framebuffer capture failed.");
            byte[] png;
            int width = texture.width, height = texture.height;
            try { png = texture.EncodeToPNG(); }
            finally { Destroy(texture); }
            string filename = $"level-{current.level:00}-{stage}.png";
            File.WriteAllBytes(Path.Combine(directory, filename), png);
            string hash;
            using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(png)).Replace("-", "").ToLowerInvariant();
            var evidence = new FrameEvidence
            {
                level = current.level, stage = stage, label = request.Label, png = filename, sha256 = hash,
                width = width, height = height, completed = game.Owner.Completed, lost = game.Owner.Lost,
                escaped = game.Matter.EscapedCount, fragments = game.Matter.TotalFragmentCount,
                exitAvailable = game.FinalExitAvailable, simulationTime = game.Matter.SimulationTime,
                utc = DateTime.UtcNow.ToString("O")
            };
            current.frames.Add(evidence);
            if (stage == "open") hasOpen = true;
            if (stage == "won") hasWin = true;
            File.AppendAllText(Path.Combine(directory, "captures.jsonl"), JsonUtility.ToJson(evidence) + "\n");
            SaveRun();
            Debug.Log($"COGHE_CHAPTER_PROOF_CAPTURE {current.level} {stage} completed={evidence.completed} escaped={evidence.escaped} {Path.Combine(directory, filename)}");
        }

        private void SaveRun() => File.WriteAllText(Path.Combine(directory, "run.json"), JsonUtility.ToJson(run, true));
    }
}
#endif
