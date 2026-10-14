using System.Collections;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed partial class COgheSpatialCampaignTests
    {
        [UnityTest] public IEnumerator Boss10RejectsShortcutsAndRecoversAfterReversingBridge()
        {
            yield return Load(10);
            var tasks=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();
            var a=tasks.Single(t=>t.Label=="A");var b=tasks.Single(t=>t.Label=="B");var d=tasks.Single(t=>t.Label=="D");
            var span=game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>().Single(r=>r.name=="B rising bridge");
            var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();
            Assert.IsFalse(a.InterlockOpen);Assert.IsFalse(b.InterlockOpen);
            yield return Tap(a.HandPoint);yield return Wait(4);Assert.AreEqual(0,a.CompletedJourneys);
            yield return Tap(b.HandPoint);yield return Wait(4);Assert.AreEqual(0,b.CompletedJourneys);
            game.CameraRig.Orbit(432,720);
            yield return Tap(d.HandPoint);yield return Wait(15);Assert.AreEqual(0,d.CompletedJourneys,"Glass cannot bypass the missing bridge");
            game.CameraRig.Overview();
            yield return Tap(game.Owner.Outlet.position);yield return Wait(8);Assert.IsFalse(game.Owner.Completed);
            // Retry then exercise the real reverse operation; no state/pose assignment grants a solution.
            game.ResetLevel();yield return Wait(1);
            yield return Tap(lift.Panel.position);yield return Until(45,()=>lift.Trips==1&&!lift.Moving,"Lift up");
            yield return RailTrip("C");
            yield return Tap(lift.Panel.position);yield return Until(45,()=>lift.Trips==2&&!lift.Moving,"Lift down");
            yield return Until(10,()=>b.InterlockOpen,"Bridge handle uncovered");yield return RailTrip("B");
            yield return Until(20,()=>span.AtEnd,"Bridge docks");yield return RailTrip("B");
            yield return Until(20,()=>span.Position<.01f,"Bridge lowers safely");
            Assert.IsFalse(game.Owner.Lost);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            game.ResetLevel();yield return Wait(1);
            Assert.IsFalse(a.InterlockOpen);Assert.IsFalse(b.InterlockOpen);Assert.Less(span.Position,.009f);
            yield return new COgheSpatialScenario(game,Tap,Until).Solve();
            Assert.AreEqual(32,game.Matter.EscapedCount);
        }
    }
}
