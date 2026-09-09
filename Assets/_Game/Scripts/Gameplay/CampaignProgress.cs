using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public interface ICampaignProgressStorage
    {
        string Read(string key);
        void Write(string key, string value);
    }

    public sealed class PlayerPrefsCampaignStorage : ICampaignProgressStorage
    {
        public string Read(string key) => PlayerPrefs.GetString(key, "");
        public void Write(string key, string value) { PlayerPrefs.SetString(key, value); PlayerPrefs.Save(); }
    }

    public sealed class VolatileCampaignStorage : ICampaignProgressStorage
    {
        private readonly Dictionary<string, string> values = new Dictionary<string, string>();
        public string Read(string key) => values.TryGetValue(key, out string value) ? value : "";
        public void Write(string key, string value) => values[key] = value;
    }

    // Persistent identity is the level ID, never its current position in the curriculum.
    public sealed class CampaignProgress
    {
        [Serializable] public sealed class Completion
        {
            public string LevelId;
            public int ContentVersion;
        }
        [Serializable] private sealed class SaveData
        {
            public int SchemaVersion = 1;
            public string LastLevelId;
            public List<Completion> Completed = new List<Completion>();
        }
        private readonly ICampaignProgressStorage storage;
        private readonly string key;
        private SaveData data = new SaveData();
        private bool canWrite = true;
        public string LastLevelId => data.LastLevelId;
        public int CompletedCount => data.Completed.Count;

        public CampaignProgress(string catalogId, ICampaignProgressStorage storage = null)
        {
            this.storage = storage ?? new PlayerPrefsCampaignStorage();
            key = "gravitybox.campaign." + catalogId;
            string json = this.storage.Read(key);
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                SaveData restored = JsonUtility.FromJson<SaveData>(json);
                if (restored == null) return;
                if (restored.SchemaVersion > 1) { canWrite = false; return; }
                if (restored.SchemaVersion != 1) return;
                data = restored;
                if (data.Completed == null) data.Completed = new List<Completion>();
                var seen = new HashSet<string>();
                data.Completed.RemoveAll(c => c == null || string.IsNullOrEmpty(c.LevelId) || !seen.Add(c.LevelId));
            }
            catch (ArgumentException) { data = new SaveData(); }
        }

        public bool IsComplete(string id) => data.Completed.Exists(c => c.LevelId == id);
        public int ResumeIndex(LevelCatalog catalog)
        {
            if (catalog == null || catalog.Levels == null) return 0;
            for (int i = 0; i < catalog.Levels.Length; i++)
                if (catalog.Levels[i] != null && catalog.Levels[i].Id == data.LastLevelId) return i;
            return 0;
        }
        public void Visit(string id) { data.LastLevelId = id; Save(); }
        public void Complete(LevelDefinition definition)
        {
            Completion record = data.Completed.Find(c => c.LevelId == definition.Id);
            if (record == null) { record = new Completion { LevelId = definition.Id }; data.Completed.Add(record); }
            record.ContentVersion = definition.Design != null ? definition.Design.ContentVersion : 1;
            data.LastLevelId = definition.Id;
            Save();
        }
        private void Save() { if (canWrite) storage.Write(key, JsonUtility.ToJson(data)); }
    }
}
