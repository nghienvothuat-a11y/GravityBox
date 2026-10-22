using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed partial class COgheSpatialCampaignTests
    {
        [Test] public void CleverGlassRoadLessonStartsWithTheGearBeforeThePad()
        {
            var steps = COgheSolutionSteps.For("coghe.spatial.plus.g11");
            Assert.IsNotNull(steps);
            CollectionAssert.AreEqual(new[] { "A carriage", "Fixed chamber/P measured tissue load", "Fixed chamber/B rope swing" },
                steps.Select(step => step.Path).ToArray(), "Walking across P while approaching A is incidental, not the first operation");
        }

        [UnityTest] public IEnumerator CleverBookCoversCurrentCatalogAndResolvesEveryStep()
        {
            var catalog = Resources.Load<COgheProductCatalog>("COgheUI/Catalog");
            var errors = new List<string>();
            foreach (var definition in catalog.Levels)
            {
                var steps = COgheSolutionSteps.For(definition.Id);
                if (steps == null) { errors.Add($"{definition.Order}: {definition.Id} has no Clever lesson"); continue; }
                yield return LoadScene(definition.SceneSequence[definition.Order - 1]);
                foreach (var step in steps)
                    if (COgheSolutionSteps.Resolve(game.Owner.Apparatus, step) == null)
                        errors.Add($"{definition.Order}: missing {step.Kind} {step.Path}");
            }
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [UnityTest] public IEnumerator CleverRetryRestartsItsIdleClock()
        {
            COgheIntelligence.ForceForTests = true; COgheIntelligence.LevelForTests = 0;
            try
            {
                yield return LoadPlus("G2"); yield return Wait(16);
                yield return Tap(game.Motion.Centre(game.Motion.Selected));
                Assert.Greater(game.LastInputTime, 15);
                game.ResetLevel(); COgheIntelligence.LevelForTests = 2;
                Assert.AreEqual(0, game.LastInputTime, .01f, "Retry must not retain the previous attempt's clock");
                yield return Wait(COgheIntelligence.IdleDelay + COgheIntelligence.LookTime + .2f);
                Assert.AreEqual(0, game.Intelligence.GuidedStep, "Clever starts after the usual pause, not the old attempt's duration");
            }
            finally { COgheIntelligence.ForceForTests = false; COgheIntelligence.LevelForTests = null; }
        }

        [UnityTest] public IEnumerator CleverLevelTwoGuidesLevel25At60Fps() { yield return CleverLevel25(60); }
        [UnityTest] public IEnumerator CleverLevelTwoGuidesLevel25At30Fps() { yield return CleverLevel25(30); }

        private IEnumerator CleverLevel25(int fps)
        {
            COgheIntelligence.ForceForTests = true;
            COgheShop.ResetForTests(new COgheShop.State { Intelligence = 2, LastAbility = 2 });
            try
            {
                yield return LoadPlus("G8");
                Assert.AreEqual(25, game.Definition.Order);
                var intel = game.Intelligence;
                Assert.AreEqual(2, intel.Remembered, "Use the saved ability, not a test level override");
                Assert.AreEqual(3, intel.StepCount, "Level 25 has A, B and C in order");
                var a = game.GetComponentsInChildren<COgheTapRail>().Single(r => r.Label == "A");
                var b = game.GetComponentsInChildren<COgheTapRail>().Single(r => r.Label == "B");
                bool Beside(COgheTapRail rail) => game.Motion.Get(game.Motion.Selected) == null &&
                    Vector3.Distance(game.Motion.Centre(game.Motion.Selected), rail.StandPoint) < .07f;
                yield return CleverFramesUntil(35, () => intel.GuidedStep == 0 && Beside(a), fps, "Walk beside A without player input");
                Assert.AreEqual(0, a.CompletedJourneys, "Clever must wait for the player to operate A");
                yield return Tap(a.HandPoint);
                yield return CleverFramesUntil(30, () => intel.Progress == 1, fps, "Player operates A");
                yield return CleverFramesUntil(45, () => intel.GuidedStep == 1 && Beside(b), fps, "Climb to B after A, without another walking tap");
                Assert.AreEqual(0, b.CompletedJourneys, "Clever approaches B but never operates it");
                Assert.IsFalse(game.Owner.Lost); Assert.AreEqual(1, game.Matter.TotalFragmentCount);
                yield return Tap(b.HandPoint);
                yield return CleverFramesUntil(30, () => intel.Progress == 2, fps, "Player operates B");
                yield return CleverFramesUntil(15, () => intel.Puzzled > 0, fps, "Level 2 remembers only A and B");
                Assert.AreEqual(1, intel.GuidedStep, "It must not guide to C at Clever level 2");

                game.ResetLevel();
                Assert.AreEqual(0, intel.Progress); Assert.AreEqual(-1, intel.GuidedStep); Assert.AreEqual(0, intel.Puzzled);
                yield return CleverFramesUntil(35, () => intel.GuidedStep == 0 && Beside(a), fps, "Retry starts again at A");
                Assert.AreEqual(0, a.CompletedJourneys);
            }
            finally { COgheIntelligence.ForceForTests = false; COgheIntelligence.LevelForTests = null; COgheShop.ResetForTests(); }
        }

        private IEnumerator CleverFramesUntil(float seconds, Func<bool> done, int fps, string reason)
        {
            for (int frame = 0; frame < seconds * fps && !done() && !game.Owner.Lost; frame++)
            {
                for (int tick = 0; tick < 120 / fps; tick++) Tick();
                yield return null; // Let personality and reading animation run at the same cadence as the player build.
            }
            Assert.IsTrue(done(), $"{reason}: progress={game.Intelligence.Progress}, guided={game.Intelligence.GuidedStep}, " +
                $"centre={game.Motion.Centre(game.Motion.Selected):F3}, act={game.Personality?.Act}, failure={game.Failure}");
        }
    }

    public partial class COgheProductUITests
    {
        [UnityTest] public IEnumerator CleverLevel25WorksWithSavedCardAndProductPresentation()
        {
            bool personality = COghePersonality.Enabled;
            COghePersonality.Enabled = true;
            try
            {
                // No ForceForTests or level override: the same activation and saved card path as the phone build.
                COgheShop.Current.Intelligence = 2; COgheShop.Current.LastAbility = 2;
                ui.LoadForTest(25);
                float deadline = Time.realtimeSinceStartup + 15;
                do
                {
                    yield return null;
                    game = UnityEngine.Object.FindFirstObjectByType<VenomCampaign>();
                } while ((game == null || game.Definition.Order != 25 || game.ProductUI == null) && Time.realtimeSinceStartup < deadline);
                Assert.IsNotNull(game); Assert.AreEqual(25, game.Definition.Order);
                ui = game.ProductUI;
                Assert.IsNotNull(game.Intelligence); Assert.AreEqual(2, game.Intelligence.Remembered);
                var a = game.GetComponentsInChildren<COgheTapRail>().Single(r => r.Label == "A");
                var b = game.GetComponentsInChildren<COgheTapRail>().Single(r => r.Label == "B");
                bool Beside(COgheTapRail rail) => game.Motion.Get(game.Motion.Selected) == null &&
                    Vector3.Distance(game.Motion.Centre(game.Motion.Selected), rail.StandPoint) < .07f;
                yield return CleverProductUntil(40, () => game.Intelligence.GuidedStep == 0 && Beside(a), "Saved Clever 2 approaches A");
                Assert.AreEqual(0, a.CompletedJourneys);

                Vector2 point = game.Owner.View.WorldToScreenPoint(a.HandPoint);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point, buttons = 1 }); yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); yield return null;
                yield return CleverProductUntil(65, () => game.Intelligence.Progress == 1 && game.Intelligence.GuidedStep == 1 && Beside(b), "One player tap on A, then Clever climbs to B");
                Assert.AreEqual(0, b.CompletedJourneys, "The player still owns the second operation");
                Assert.AreEqual(COgheProductPage.Game, ui.Page); Assert.IsFalse(game.Owner.Lost);

                ui.ShowPopup(COgheProductPopup.Restart); ui.Confirm();
                Assert.AreEqual(0, game.Intelligence.Progress); Assert.AreEqual(-1, game.Intelligence.GuidedStep);
                yield return CleverProductUntil(40, () => game.Intelligence.GuidedStep == 0 && Beside(a), "Product Retry guides from A again");
            }
            finally { COghePersonality.Enabled = personality; }
        }

        private IEnumerator CleverProductUntil(float seconds, Func<bool> done, string reason)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!done() && !game.Owner.Lost && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(done(), $"{reason}: progress={game.Intelligence.Progress}, guided={game.Intelligence.GuidedStep}, " +
                $"act={game.Personality?.Act}, page={ui.Page}, failure={game.Failure}");
        }
    }
}
