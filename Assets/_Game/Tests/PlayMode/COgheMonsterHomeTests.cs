using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public partial class COgheProductUITests
    {
        [UnityTest] public IEnumerator HomeMonsterReturnsToLiquidAndCanBeInterrupted()
        {
            game.Progress.HomeUnlocked=true;ui.OpenHome();ui.ToggleHomeZoom();
            yield return new WaitForSecondsRealtime(.6f);
            game.Personality.Force(COgheAct.Monster);
            yield return new WaitForSecondsRealtime(5.8f);
            var life=game.Matter.GetComponent<VenomLifeAnimation>();
            Assert.Greater(life.Monster.Fangs,.9f);Assert.Greater(life.Monster.Tongue,.8f);
            var tongue=game.Matter.transform.Find("Monster tongue").GetComponent<MeshFilter>();
            var tongueVertices=tongue.sharedMesh.vertices;
            float width=Vector3.Distance(tongue.transform.TransformPoint(tongueVertices[0]),tongue.transform.TransformPoint(tongueVertices[8]));
            float thickness=Vector3.Distance(tongue.transform.TransformPoint(tongueVertices[4]),tongue.transform.TransformPoint(tongueVertices[12]));
            Assert.That(width/life.Monster.Size,Is.InRange(.27f,.28f),"Tongue cross-section is twice the former 0.1377 head units");
            Assert.That(thickness/life.Monster.Size,Is.InRange(.05f,.06f),"Wider, not twice as thick");
            var head=game.Owner.View.WorldToViewportPoint(life.Monster.Head);
            Assert.That(head.x,Is.InRange(.1f,.9f));Assert.That(head.y,Is.InRange(.2f,.85f));
            Assert.AreEqual(0,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            game.Personality.Force(COgheAct.None);yield return null;yield return null;
            Assert.AreEqual(0,life.Monster.Body);
            Assert.IsFalse(game.Matter.transform.Find("Monster eyes").gameObject.activeSelf);
            ui.ShowMenu();yield return null;ui.Play();yield return null;yield return null;
            Assert.IsFalse(game.Home);Assert.AreEqual(COgheProductPage.Game,ui.Page);
            Assert.AreEqual(0,game.Progress.Completed.Count,"No puzzle/save progress comes from the act");
        }
    }
}
