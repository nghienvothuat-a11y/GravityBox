using System.Collections;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed partial class COgheViewExpansionTests
    {
        private COgheSimultaneousScenario Coop => new COgheSimultaneousScenario(game, Tap, Until);
        private COgheCooperativeDrive Drive => Mechanism<COgheCooperativeDrive>();
        private IEnumerator SoloSequence(bool reverse)
        {
            yield return Load(23); var coop = Coop;
            // Ordinary waypoints bypass the knife, keeping the original whole creature.
            yield return Route.Walk(new Vector3(-.50f, -.30f, -.29f), "Whole body skirts knife");
            yield return Route.Walk(new Vector3(-.50f, -.30f, .12f), "Reach clear rear floor without cutting");
            Assert.AreEqual(1, game.Matter.TotalFragmentCount);
            yield return coop.Hold(reverse ? "B" : "A", 0); yield return Advance(2);
            Assert.Less(Drive.Output.Position, .003f); Assert.IsFalse(Drive.Caught);
            yield return coop.Hold(reverse ? "A" : "B", 0); yield return Advance(3);
            Assert.AreEqual(1, game.Matter.TotalFragmentCount);
            Assert.IsFalse(Drive.Caught); Assert.Less(Drive.Output.Position, .003f);
            Assert.AreEqual(0, Drive.OverlapSeconds); Assert.IsFalse(game.FinalExitAvailable); IntactInside();
        }
        [UnityTest] public IEnumerator View23WholeBodyAThenBCannotComplete() { yield return SoloSequence(false); }
        [UnityTest] public IEnumerator View23WholeBodyBThenACannotComplete() { yield return SoloSequence(true); }
        [UnityTest] public IEnumerator View23ChangingOneTaskPreservesOtherHeldInput()
        {
            yield return Load(23); var coop = Coop; yield return coop.Split();
            yield return coop.Hold("A", coop.Holder); yield return coop.Start("B", coop.Worker);
            Assert.IsTrue(Route.Task("A").Holding);
            game.SelectFragment(coop.Holder); yield return Route.StartWalk(new Vector3(-.44f, -.30f, .10f));
            Assert.IsFalse(Route.Task("A").Busy);
            yield return Until(30, () => Route.Task("B").Holding, "B continues its own command after A leaves");
            Assert.IsFalse(Drive.Caught); Assert.IsFalse(game.FinalExitAvailable); IntactInside();
        }
        [UnityTest] public IEnumerator View23ReverseInputOrderStillCatchesAndReleasesBoth()
        {
            yield return Load(23); var coop = Coop; yield return coop.Split();
            yield return coop.Hold("B", coop.Worker); yield return coop.Start("A", coop.Holder);
            yield return Until(40, () => Drive.Caught, "Both physically supported inputs catch output in reverse order");
            Assert.Greater(Drive.OverlapSeconds, .5f);
            yield return Advance(2); Assert.IsTrue(Drive.Output.AtEnd);
            Assert.IsFalse(Route.Task("A").Busy || Route.Task("B").Busy);
            yield return Route.Merge(new Vector3(0, -.30f, -.05f)); IntactInside();
        }
        private IEnumerator ReturnAndRecover(int n)
        {
            yield return Load(n); var coop = Coop; yield return coop.Split();
            yield return coop.Hold("A", coop.Holder); yield return coop.Start("B", coop.Worker);
            yield return Until(35, () => Drive.Output.Fraction > .20f, "Continuous overlapping inputs begin output travel");
            Assert.IsFalse(Drive.Caught);
            game.SelectFragment(coop.Holder); yield return Route.StartWalk(new Vector3(-.45f, -.30f, .10f));
            yield return Until(15, () => Drive.Output.Position < .004f, "Lost input returns uncaught output");
            Assert.IsFalse(game.FinalExitAvailable); Assert.IsFalse(Drive.Caught);
            Assert.IsTrue(Route.Task("B").Busy, "Partner keeps its independent held task");
            yield return coop.Start("A", coop.Holder);
            yield return Until(35, () => Drive.Caught, "Reholding recovers without Retry"); IntactInside();
        }
        [UnityTest] public IEnumerator View23ReleaseReturnsDoorAndReholdingRecovers() { yield return ReturnAndRecover(23); }
        [UnityTest] public IEnumerator View26ReleaseDumpsPressureAndReholdingRecovers() { yield return ReturnAndRecover(26); }
        [UnityTest] public IEnumerator View24BrakeLossPausesWorkerAndReholdingResumesSameTask()
        {
            yield return Load(24); var coop = Coop; yield return coop.Split();
            yield return coop.Hold("A", coop.Holder); game.SelectFragment(coop.Worker); yield return coop.Start("B", coop.Worker);
            yield return Until(30, () => Drive.Output.Fraction > .20f, "Worker drives bridge with brake released");
            var b = Route.Task("B");
            game.SelectFragment(coop.Holder); yield return Route.StartWalk(new Vector3(-.46f, -.30f, .10f));
            yield return Advance(.25f); float stopped = Drive.Output.Position;
            yield return Advance(2); Assert.That(Drive.Output.Position, Is.EqualTo(stopped).Within(.004f));
            Assert.IsTrue(b.Busy); Assert.IsFalse(b.InterlockOpen); Assert.IsFalse(Drive.Caught);
            yield return coop.Start("A", coop.Holder);
            yield return Until(40, () => Drive.Caught, "Worker resumes existing command after reholding brake");
            Assert.Greater(Drive.OverlapSeconds, 1); IntactInside();
        }
        [UnityTest] public IEnumerator View24WorkerAloneCannotMoveBrakedBridgeAndCanCancelWaiting()
        {
            yield return Load(24); var coop = Coop; yield return coop.Split();
            game.SelectFragment(coop.Holder); yield return Route.StartWalk(new Vector3(-.49f, -.30f, .12f));
            yield return coop.Start("B", coop.Worker);
            yield return Until(30, () => Route.Task("B").Phase == COgheTapRail.TaskPhase.Operating, "Worker reaches braked bridge");
            yield return Advance(3); Assert.Less(Drive.Output.Position, .004f); Assert.AreEqual(0, Drive.OverlapSeconds);
            game.SelectFragment(coop.Worker); yield return Route.StartWalk(new Vector3(-.23f, -.30f, -.33f));
            Assert.IsFalse(Route.Task("B").Busy); IntactInside();
        }
        [UnityTest] public IEnumerator View25BrakeHolderAloneCannotDriveFarBridge()
        {
            yield return Load(25); var coop = Coop; yield return coop.Split(); yield return coop.Hold("A", coop.Holder);
            yield return Advance(3); Assert.Less(Drive.Output.Position, .003f); Assert.IsFalse(Drive.Caught);
            Assert.IsFalse(game.FinalExitAvailable); IntactInside();
        }
        [UnityTest] public IEnumerator View27CornerPullUsesRealWallSupportAndKeepsCatchAfterRelease()
        {
            yield return Load(27); var coop = Coop; yield return coop.Split();
            yield return coop.Hold("A", coop.Holder); yield return coop.Start("B", coop.Worker);
            yield return Until(40, () => Drive.Caught, "Two orthogonal wall inputs catch common output");
            Assert.Less(Vector3.Dot(Route.Task("A").WorkingSurface.Normal, Route.Task("B").WorkingSurface.Normal), .1f);
            yield return Advance(3); Assert.IsTrue(Drive.Output.AtEnd); IntactInside();
        }
        [UnityTest] public IEnumerator View28FirstRouteMustCatchBeforeSelectorCanChange()
        {
            yield return Load(28); Assert.IsFalse(Route.Task("T").InterlockOpen);
            Assert.IsFalse(Route.Task("T").Request(0));
            var coop = Coop; yield return coop.Split(); yield return coop.Hold("A", coop.Holder); yield return coop.Start("B", coop.Worker);
            var first = game.Owner.Apparatus.GetComponentsInChildren<COgheCooperativeDrive>().Single(d => !d.FinalGate);
            yield return Until(40, () => first.Caught, "First route actually catches");
            Assert.IsTrue(Route.Task("T").InterlockOpen); Assert.IsFalse(game.FinalExitAvailable);
            game.SelectFragment(coop.Worker); yield return Route.Operate("T"); yield return Advance(2);
            Assert.IsTrue(first.Caught && first.Output.AtEnd); IntactInside();
        }
        [UnityTest] public IEnumerator View30FinalValvesLockedUntilActualReturnBridge()
        {
            yield return Load(30); Assert.IsFalse(Route.Task("C").InterlockOpen || Route.Task("D").InterlockOpen);
            Assert.IsFalse(Route.Task("C").Request(0)); Assert.IsFalse(Route.Task("D").Request(0));
            Assert.IsFalse(game.FinalExitAvailable); IntactInside();
        }
        [UnityTest] public IEnumerator View26CutawayWallStillAcceptsAndSupportsHeldInput()
        {
            yield return Load(26); var coop = Coop; yield return coop.Split();
            var task = Route.Task("B"); var strip = task.WorkingSurface; var shape = strip.Shape; var bounds = shape.bounds;
            var view = game.GetComponent<COgheViewPresentation>(); view.Refresh(1);
            Assert.Greater(Vector3.Dot(game.Owner.View.transform.forward, strip.Normal), .01f, "Initial view looks through this near wall");
            Assert.IsFalse(strip.GetComponent<Renderer>().enabled, "Near grip overlay must not hide the creature or handle");
            game.SelectFragment(coop.Worker); yield return Tap(task.HandPoint);
            Assert.IsTrue(task.Busy, "Visible handle accepts its first tap through the cutaway wall");
            yield return Until(40, () => task.Holding, "Invisible wall surface still supplies real planted feet");
            Assert.IsTrue(shape.enabled); Assert.AreEqual(bounds, shape.bounds);
            game.CameraRig.Orbit(-540, 720); game.CameraRig.Frame(720, 1280, 0, true); view.Refresh(1);
            Assert.IsTrue(strip.GetComponent<Renderer>().enabled, "The same wall returns when viewed from inside");
            yield return Advance(.5f); Assert.IsTrue(task.Holding); Assert.IsFalse(Drive.Caught); IntactInside();
        }
        [UnityTest] public IEnumerator View23FragmentAtOpenWallExitLosesBeforeAnyWin()
        {
            yield return Load(23); var coop = Coop; yield return coop.Split();
            yield return coop.Hold("A", coop.Holder); yield return coop.Start("B", coop.Worker);
            yield return Until(40, () => game.FinalExitAvailable, "Both inputs catch the wall shutter");
            game.SelectFragment(coop.Worker);
            for (int view = 0; view < 4 && game.Motion.Get(coop.Worker)?.Exit != true; view++)
            { yield return Tap(game.Owner.Outlet.position); if (game.Motion.Get(coop.Worker)?.Exit != true) game.CameraRig.Orbit(-270, 720); }
            Assert.IsTrue(game.Motion.Get(coop.Worker)?.Exit);
            yield return Until(35, () => game.Owner.Lost, "One separate body really reaches the wall aperture");
            Assert.AreEqual(VenomCampaign.MergeFailure, game.Failure); Assert.IsFalse(game.Owner.Completed);
            game.ResetLevel(); yield return Advance(.5f); AssertReset();
        }
        [UnityTest] public IEnumerator View24UnequalRealCutStillCompletesBothRoles()
        {
            yield return Load(24); var coop = Coop; yield return coop.Split();
            var counts = game.Matter.Groups.GroupBy(g => g).Select(g => g.Count()).ToArray();
            Assert.AreEqual(32, counts.Sum()); Assert.AreEqual(2, counts.Length);
            Assert.IsTrue(counts.Any(n => n != 16), "Authored blade preserves its real unequal intersection");
            yield return coop.Hold("A", coop.Holder); yield return coop.Start("B", coop.Worker);
            yield return Until(40, () => Drive.Caught, "Unequal fragments supply real brake and bridge force");
            yield return Route.Merge(new Vector3(-.35f, -.30f, .02f)); IntactInside();
        }
        [UnityTest] public IEnumerator View23ApproachingPartnerCanMergeWithHeldBody()
        {
            yield return Load(23); var coop = Coop; yield return coop.Split(); yield return coop.Hold("A", coop.Holder);
            // Isolate the physical merge rule with a public surface command. A screen tap directly
            // on the now-visible partner correctly selects it instead of issuing this destination.
            game.SelectFragment(coop.Worker);
            game.MoveTo(Route.Task("A").WorkingSurface.Closest(game.Motion.Centre(coop.Holder)), Route.Task("A").WorkingSurface);
            yield return Until(40, () => game.Matter.TotalFragmentCount == 1, "Held task does not protect topology against a nearby partner");
            yield return Advance(.2f); Assert.IsFalse(Route.Task("A").Busy); Assert.IsFalse(Drive.Caught); IntactInside();
        }
        [UnityTest] public IEnumerator View23ReturnSafetyFixtureCannotCreateCatchOrOpenAperture()
        {
            yield return Load(23);
            // Isolated safety-volume fixture, not solution evidence: place one real particle in the swept box.
            var output = Drive.Output; output.Body.position = output.Frame.TransformPoint(output.Start + output.Axis * output.Travel * .5f);
            game.Matter.Bodies[0].position = output.Frame.TransformPoint(output.Start + output.Axis * output.Travel * .25f);
            Physics.SyncTransforms(); Drive.StepMechanism(game, Dt);
            Assert.IsTrue(Drive.Obstructed); Assert.IsTrue(output.Locked);
            Assert.IsFalse(Drive.Caught); Assert.IsFalse(game.FinalExitAvailable); Assert.IsTrue(Drive.Aperture.NavigationHoleBlocked);
            game.Matter.Bodies[0].position = new Vector3(-.50f, -.26f, -.30f); Physics.SyncTransforms();
            Drive.StepMechanism(game, Dt); Assert.IsFalse(Drive.Obstructed); Assert.IsFalse(output.Locked);
        }
    }
}
