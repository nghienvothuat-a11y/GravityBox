using System.Collections;
using System.Text;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    // Mystery gift box on levels 18–63 (Mrk 21/10/2026): each box sits on the way out, so a real solve with the product UI
    // meets it. The authored solution runs with physics ticked by the test (as COgheSpatialCampaignTests); when the box
    // offer opens the player opens it, closes the prize and taps their last target again, then the level must still end.
    public partial class COgheProductUITests
    {
        private const float BoxDt = 1f / 120;
        private Vector3? boxLastTap; private int boxOffers;

        private void BoxTick() { game.Owner.Step(BoxDt); game.Owner.Rotation.Step(BoxDt); if (!game.Owner.Paused) Physics.Simulate(BoxDt); }
        private IEnumerator BoxTap(Vector3 point)
        { boxLastTap = point; game.CameraRig.Frame(720, 1280, 0, true); game.TouchPoint(game.Owner.View.WorldToScreenPoint(point)); yield return null; }
        private bool BoxBusy => ui.BoxPhase == COgheProductUI.MysteryBoxPhase.Walking || ui.Popup == COgheProductPopup.MysteryBox || ui.Popup == COgheProductPopup.MysteryReveal;
        private IEnumerator BoxUntil(float seconds, System.Func<bool> done, string reason)
        {
            for (int i = 0; i < seconds / BoxDt && !done() && !game.Owner.Lost; i++)
            {
                if (BoxBusy) { yield return ServiceBox(); i = 0; continue; }
                BoxTick(); if (i % 4 == 3) yield return null;   // the product UI sees the body every 4 ticks
            }
            // Thrown, not asserted: the level is written down as failed and the next level still runs.
            if (!done()) throw new System.InvalidOperationException($"{reason}; centre {game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected)):F3} box {ui.BoxPhase}");
        }
        /// <summary>Runs nested coroutines here, so an exception anywhere in a solve is caught (as COgheLevelMetrics.Flat).</summary>
        private static IEnumerator BoxFlat(IEnumerator root, System.Action<string> fail)
        {
            var stack = new System.Collections.Generic.Stack<IEnumerator>(); stack.Push(root);
            while (stack.Count > 0)
            {
                var top = stack.Peek(); bool more;
                try { more = top.MoveNext(); } catch (System.Exception e) { fail(e.Message.Split('\n')[0]); yield break; }
                if (!more) { stack.Pop(); continue; }
                if (top.Current is IEnumerator inner) stack.Push(inner); else yield return top.Current;
            }
        }
        private IEnumerator ServiceBox()
        {
            float end = Time.realtimeSinceStartup + 20;
            while (ui.BoxPhase == COgheProductUI.MysteryBoxPhase.Walking && Time.realtimeSinceStartup < end) { for (int k = 0; k < 4; k++) BoxTick(); yield return null; }
            if (ui.Popup == COgheProductPopup.MysteryBox)
            {
                boxOffers++; yield return new WaitForSecondsRealtime(1.2f); BoxSnapshot($"L{game.Definition.Order:00}-offer");
                ui.OpenMysteryBox();
            }
            if (ui.Popup == COgheProductPopup.MysteryReveal)
            {
                yield return new WaitForSecondsRealtime(1.1f); BoxSnapshot($"L{game.Definition.Order:00}-prize");   // the prize risen
                yield return new WaitForSecondsRealtime(1.2f); ui.CloseMysteryBox();                                   // Great!
            }
            if (boxLastTap.HasValue) yield return BoxTap(boxLastTap.Value);   // the player taps where they were going again
        }

        /// <summary>Every collider (triggers too) on a 60 cm vertical line through the level's spot: why no box landed.</summary>
        private string BoxProbe()
        {
            var from = game.Root.TransformPoint(COgheProductUI.BoxSpots[game.Definition.Id].spot) + game.Root.up * .3f; var text = new StringBuilder();
            foreach (var hit in Physics.RaycastAll(from, -game.Root.up, .6f, ~0, QueryTriggerInteraction.Collide))
                text.Append($" {hit.collider.name}{(hit.collider.isTrigger ? "(trigger)" : "")}@{game.Root.InverseTransformPoint(hit.point).y:F3}");
            return text.Length == 0 ? " nothing" : text.ToString();
        }

        [UnityTest, Timeout(1800000)] public IEnumerator EveryLaterMysteryBoxIsMetOnTheWayOutAndTheLevelStillEnds()
        {
            var ads = new CountingAds(); COgheAds.Provider = ads; COgheStyle.ResetForTests(new COgheStyle());
            var simulation = Physics.simulationMode; Physics.simulationMode = SimulationMode.Script;
            var report = new StringBuilder(); int failed = 0;
            try
            {
                foreach (int order in new[] { 18, 23, 28, 33, 38, 44, 48, 53, 58, 63 })
                {
                    yield return LoadDisplay(order);
                    // The test is the only clock (as COgheSpatialCampaignTests.LoadScene): the level's own FixedUpdate
                    // would step the campaign a second time without physics.
                    game.Owner.enabled = false; game.Owner.Rotation.enabled = false;
                    boxOffers = 0; boxLastTap = null;
                    for (int i = 0; i < 240 && (ui.MysteryBox == null || !ui.MysteryBox.Landed); i++) { BoxTick(); yield return null; }
                    string result;
                    if (ui.MysteryBox == null) result = "NO BOX; under the spot:" + BoxProbe();
                    else
                    {
                        Vector3 rest = game.Root.InverseTransformPoint(ui.MysteryBox.Rest);
                        string error = null;
                        yield return BoxFlat(new COgheSpatialScenario(game, BoxTap, BoxUntil).Solve(), e => error = e);
                        if (error == null && !game.Owner.Completed) yield return BoxFlat(BoxUntil(20, () => game.Owner.Completed, "Exit after the box"), e => error = e);
                        bool ok = error == null && game.Owner.Completed && boxOffers == 1 && ui.BoxPhase == COgheProductUI.MysteryBoxPhase.Done && COgheShop.Paid("box:" + game.Definition.Id);
                        result = (ok ? "ok" : "FAIL") + $" rest={rest:F3} offers={boxOffers} completed={game.Owner.Completed} phase={ui.BoxPhase}" + (error != null ? " error=" + error : "");
                        if (!ok) failed++;
                    }
                    report.Append($"display {order} {game.Definition.Id}: {result}\n");
                    System.IO.File.WriteAllText("Artifacts/MysteryBox/solve-report.txt", report.ToString());
                }
            }
            finally { Physics.simulationMode = simulation; EndShop(); }
            Debug.Log("MYSTERY BOX SOLVES\n" + report);
            Assert.AreEqual(0, failed, report.ToString());
        }
    }
}
