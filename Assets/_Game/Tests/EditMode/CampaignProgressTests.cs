using System.Collections.Generic;
using GravityBox.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed class CampaignProgressTests
    {
        private sealed class MemoryStorage : ICampaignProgressStorage
        {
            public readonly Dictionary<string,string> Values = new Dictionary<string,string>();
            public string Read(string key) => Values.TryGetValue(key, out string value) ? value : "";
            public void Write(string key, string value) => Values[key] = value;
        }
        [Test]
        public void SavedIdsSurviveReorderingAndRepeatedCompletions()
        {
            var storage = new MemoryStorage();
            var a = ScriptableObject.CreateInstance<LevelDefinition>(); a.Id = "campaign-001";
            var b = ScriptableObject.CreateInstance<LevelDefinition>(); b.Id = "campaign-100";
            var catalog = ScriptableObject.CreateInstance<LevelCatalog>(); catalog.Levels = new[] { b, a };
            try
            {
                var progress = new CampaignProgress("test", storage); progress.Complete(b); progress.Complete(b); progress.Visit(a.Id);
                var restored = new CampaignProgress("test", storage);
                Assert.That(restored.CompletedCount, Is.EqualTo(1));
                Assert.That(restored.IsComplete(b.Id), Is.True);
                Assert.That(restored.IsComplete(a.Id), Is.False);
                Assert.That(restored.ResumeIndex(catalog), Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(a); Object.DestroyImmediate(b); Object.DestroyImmediate(catalog); }
        }
        [Test]
        public void FutureSaveVersionIsNotOverwrittenByAnOlderBuild()
        {
            var storage = new MemoryStorage();
            const string original = "{\"SchemaVersion\":99,\"LastLevelId\":\"future\"}";
            storage.Write("gravitybox.campaign.test", original);
            var progress = new CampaignProgress("test", storage); progress.Visit("campaign-001");
            Assert.That(storage.Read("gravitybox.campaign.test"), Is.EqualTo(original));
        }
        [Test]
        public void InvalidSaveAndRemovedLastLevelSafelyStartAtTheBeginning()
        {
            var storage = new MemoryStorage(); storage.Write("gravitybox.campaign.test", "broken-json");
            var progress = new CampaignProgress("test", storage);
            Assert.That(progress.CompletedCount, Is.Zero);
            Assert.That(progress.ResumeIndex(null), Is.Zero);
        }
    }
}
