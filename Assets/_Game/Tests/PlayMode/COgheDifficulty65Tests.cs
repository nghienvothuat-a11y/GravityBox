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
        [UnityTest] public IEnumerator Boss32NeedsRelayHandoffAndRecoversWhenHolderLeaves()
        {
            yield return LoadPlus("G5");
            var s = new COgheSpatialNextScenario(game,Tap,Until);
            Vector3 W(float x,float y,float z) => game.Root.TransformPoint(new Vector3(x,y,z));
            var tasks=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();
            var a=tasks.Single(t=>t.Label=="A");var c=tasks.Single(t=>t.Label=="C");
            Assert.IsFalse(a.InterlockOpen);Assert.IsFalse(c.InterlockOpen);
            yield return Tap(a.HandPoint);yield return Wait(3);
            yield return Tap(c.HandPoint);yield return Wait(3);
            Assert.AreEqual(0,a.CompletedJourneys);Assert.AreEqual(0,c.CompletedJourneys);
            Assert.IsFalse(game.FinalExitAvailable);
            var q=game.Owner.Apparatus.GetComponentInChildren<COgheQuantumSplitter>();
            yield return s.Split(q,game.Motion.Selected);int holder=q.LastLeft,climber=q.LastRight;
            yield return s.Walk(holder,W(.20f,-.30f,-.16f),"P supports the roof");
            yield return Until(15,()=>s.Slider("Roof plate").AtEnd,"Roof docks");
            yield return s.Walk(climber,W(-.33f,-.30f,.24f),"Approach the bare back-left corner");
            game.CameraRig.Orbit(270,720);
            yield return s.Walk(climber,W(-.40f,.05f,.24f),"Climb the bare glass");
            game.CameraRig.Overview();
            yield return s.Walk(climber,W(.29f,-.01f,.02f),"Cross while P is held");
            game.SelectFragment(climber);yield return s.Operate("B");
            Assert.IsFalse(c.InterlockOpen,"B alone does not replace the second holder");
            yield return s.Walk(climber,W(.33f,-.02f,-.13f),"Stand on D");
            yield return Until(15,()=>c.InterlockOpen,"B plus D release C");
            yield return s.Walk(holder,W(.12f,-.30f,0),"Release P");
            yield return s.Walk(climber,W(.33f,-.02f,.02f),"Prematurely leave D");
            yield return Until(15,()=>!c.InterlockOpen,"C relocks without the holder");
            Assert.IsFalse(game.Owner.Lost);Assert.IsFalse(game.FinalExitAvailable);
            yield return s.Walk(climber,W(.33f,-.02f,-.13f),"Return to D to recover");
            yield return Until(15,()=>c.InterlockOpen,"Relay restores after a mistaken release");
            game.ResetLevel();yield return Wait(1);
            Assert.IsFalse(a.InterlockOpen);Assert.IsFalse(c.InterlockOpen);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            yield return new COgheSpatialScenario(game,Tap,Until).Solve();
            Assert.AreEqual(32,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Lost);
        }
    }
}
