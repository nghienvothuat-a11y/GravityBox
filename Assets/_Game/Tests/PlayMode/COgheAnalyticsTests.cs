using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed class COgheAttemptTests
    {
        private readonly List<(string name, IReadOnlyDictionary<string, object> data)> events = new();
        [SetUp] public void Before() { events.Clear(); COgheAnalytics.Sink = (n, p) => events.Add((n, p)); }
        [TearDown] public void After() { COgheAnalytics.Sink = null; }
        [Test] public void OneStartAndOneOutcomeWithOnlyActivePlayTime()
        {
            var a = new COgheLevelAttempt(); a.Begin("spatial_01", 1, "spatial", false, false);
            a.Begin("spatial_01", 1, "spatial", false, false);
            a.Tick(4, true); a.Tick(50, false); a.Tick(2, true);
            a.Finish(true, "exit"); a.Finish(true, "exit"); a.Finish(false, "bad"); a.Abandon("menu");
            Assert.AreEqual(1, events.Count(e => e.name == "level_start"));
            Assert.AreEqual(1, events.Count(e => e.name == "level_end"));
            var end = events.Single(e => e.name == "level_end").data;
            Assert.AreEqual(6d, end["duration_seconds"]); Assert.AreEqual(1, end["success"]);
            Assert.AreEqual("spatial_01", end["level_id"]); Assert.AreEqual("win", end["outcome"]);
        }
        [Test] public void RetryAndAbandonCannotMasqueradeAsLoss()
        {
            var a = new COgheLevelAttempt(); a.Begin("id", 8, "spatial", false, false); string first = a.Id;
            a.Abandon("restart"); a.Retry(); a.Begin("id", 8, "spatial", false, false);
            Assert.AreNotEqual(first, a.Id); Assert.AreEqual(2, a.Number);
            Assert.IsFalse(events.Any(e => e.name == "level_fail" || e.name == "level_end"));
            a.Finish(false, "unmerged_exit"); a.Finish(false, "unmerged_exit");
            Assert.AreEqual(1, events.Count(e => e.name == "level_fail"));
            Assert.AreEqual(0, events.Single(e => e.name == "level_end").data["success"]);
        }
        [Test] public void ProviderFailureCannotBreakGameFlow()
        {
            COgheAnalytics.Sink = (n, p) => throw new InvalidOperationException();
            LogAssert.Expect(LogType.Warning, "COghe analytics unavailable: InvalidOperationException");
            Assert.DoesNotThrow(() => COgheAnalytics.Log("level_start", "level", 1));
        }
    }

    public partial class COgheProductUITests
    {
        [UnityTest] public IEnumerator RetryStartsNewAttemptAndFailurePopupDoesNotDuplicateLoss()
        {
            COgheAnalytics.Recent.Clear(); ui.Play(); yield return null; yield return null;
            Assert.AreEqual(1, COgheAnalytics.Recent.Count(e => e.StartsWith("level_start ")));
            ui.ShowPopup(COgheProductPopup.Restart); ui.Confirm(); yield return null; yield return null;
            Assert.AreEqual(2, COgheAnalytics.Recent.Count(e => e.StartsWith("level_start ")));
            Assert.AreEqual(0, COgheAnalytics.Recent.Count(e => e.StartsWith("level_fail ")));
            game.Fail(VenomCampaign.MergeFailure); yield return null; yield return null;
            ui.Resume(); yield return null; yield return null;
            Assert.AreEqual(1, COgheAnalytics.Recent.Count(e => e.StartsWith("level_fail ")));
            Assert.AreEqual(1, COgheAnalytics.Recent.Count(e => e.StartsWith("level_end ")));
            ui.Confirm(); yield return null; yield return null;
            Assert.AreEqual(3, COgheAnalytics.Recent.Count(e => e.StartsWith("level_start ")));
        }
    }
}
