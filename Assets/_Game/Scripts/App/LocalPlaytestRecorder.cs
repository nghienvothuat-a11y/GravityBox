using System;
using System.Globalization;
using System.IO;
using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.App
{
    /// <summary>Local, event-based telemetry only. No network or account identifiers.</summary>
    public sealed class LocalPlaytestRecorder : MonoBehaviour
    {
        private StreamWriter writer;
        private LevelManager levels;
        public string FilePath { get; private set; }

        public void Initialize(LevelManager manager)
        {
            levels = manager;
            try
            {
                string folder = Path.Combine(Application.persistentDataPath, "Playtests");
                Directory.CreateDirectory(folder);
                FilePath = Path.Combine(folder, "session-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".csv");
                writer = new StreamWriter(FilePath) { AutoFlush = true };
                writer.WriteLine("utc,event,level,environment,elapsed_seconds,resets,drags,drag_distance,content_version,chapter,boss,role,difficulty_budget,escaped,ball_count");
                levels.GameplayEvent += Record;
                Record("level_start");
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                Debug.LogWarning("Local playtest recording is unavailable: " + error.Message);
            }
        }

        private static string Cell(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
        private void Record(string name)
        {
            if (writer == null) return;
            try
            {
                LevelDesignProfile design = levels.Definition.Design;
                writer.WriteLine(string.Join(",", Cell(DateTime.UtcNow.ToString("O")), Cell(name), Cell(levels.Definition.Id),
                    Cell(levels.Definition.Environment.Id), levels.Elapsed.ToString("F3", CultureInfo.InvariantCulture),
                    levels.ResetCount, levels.DragCount, levels.DragDistance.ToString("F3", CultureInfo.InvariantCulture),
                    design != null ? design.ContentVersion : 1, design != null ? design.Chapter : 0,
                    design != null && design.IsBoss ? 1 : 0, Cell(design != null ? design.Role.ToString() : "Lab"),
                    design != null ? design.DifficultyBudget.ToString("F1", CultureInfo.InvariantCulture) : "",
                    levels.EscapedCount, levels.Balls.Count));
            }
            catch (IOException error)
            {
                Debug.LogWarning("Local recording stopped: " + error.Message);
                writer.Dispose(); writer = null;
            }
        }

        private void OnDestroy()
        {
            if (levels != null && levels.Current != null) Record("session_end_" + levels.Session.State.ToString().ToLowerInvariant());
            if (levels != null) levels.GameplayEvent -= Record;
            writer?.Dispose();
        }
    }
}
