using System;
using System.Collections;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed partial class COgheViewCampaignTests
    {
        private static float Opacity(Renderer renderer)
        {
            if (!renderer.enabled) return 0;
            if (renderer.sharedMaterial.GetFloat("_Surface") == 0) return renderer.sharedMaterial.GetColor("_BaseColor").a;
            var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block, 0);
            return block.GetColor("_BaseColor").a;
        }
        private void ViewFacing(Vector3 normal, float dot)
        {
            var direction = normal * dot + Vector3.Cross(Vector3.up, normal) * Mathf.Sqrt(1 - dot * dot);
            game.Owner.View.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        [UnityTest] public IEnumerator CutawayReversalFadesPaneAndTrimWithoutChangingPhysicsOrAssets()
        {
            // Level 01 is now optical glass; level 02 retains the opaque cutaway contract.
            yield return Load(2);
            var view = game.GetComponent<COgheViewPresentation>(); view.enabled = false;
            int index = Array.FindIndex(view.Panes, p => p.Normal.z > .9f);
            var renderers = view.PaneVisuals[index].GetComponentsInChildren<Renderer>();
            Assert.GreaterOrEqual(renderers.Length, 2, "Wall and metal trim share the same fade");
            var normal = view.Panes[index].Normal; var camera = game.Owner.View.transform.rotation;
            var root = game.Root.rotation; var body = game.Matter.Bodies[0].position;
            var shape = view.Panes[index].Shape; var bounds = shape.bounds;
            var colors = Array.ConvertAll(view.FadeSources, m => m.GetColor("_BaseColor"));
            try
            {
                ViewFacing(normal, -.6f); view.Refresh(0);
                foreach (var r in renderers) Assert.AreEqual(1, Opacity(r));
                ViewFacing(normal, .6f); view.Refresh(.03f);
                float fadingOut = Opacity(renderers[0]); Assert.That(fadingOut, Is.InRange(.1f, .95f));
                foreach (var r in renderers)
                {
                    Assert.That(Opacity(r), Is.EqualTo(fadingOut).Within(.0001f));
                    Assert.IsTrue(r.sharedMaterial.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"));
                    Assert.AreEqual(0, r.sharedMaterial.GetFloat("_ZWrite"));
                }
                ViewFacing(normal, -.6f); view.Refresh(.02f);
                float reverse = Opacity(renderers[0]); Assert.Greater(reverse, fadingOut); Assert.Less(reverse, 1);
                ViewFacing(normal, .6f); view.Refresh(.02f); Assert.Less(Opacity(renderers[0]), reverse);
                view.Refresh(1); foreach (var r in renderers) Assert.IsFalse(r.enabled);
                ViewFacing(normal, -.6f); view.Refresh(.02f);
                foreach (var r in renderers) Assert.That(Opacity(r), Is.InRange(.01f, .9f));
                view.Refresh(1); foreach (var r in renderers) Assert.AreEqual(1, Opacity(r));
                Assert.AreEqual(root, game.Root.rotation); Assert.AreEqual(body, game.Matter.Bodies[0].position);
                Assert.IsTrue(shape.enabled); Assert.AreEqual(bounds, shape.bounds);
                for (int i = 0; i < colors.Length; i++) Assert.AreEqual(colors[i], view.FadeSources[i].GetColor("_BaseColor"));
                Assert.IsNull(game.Motion.Get(0), "Camera presentation must not issue a gameplay command");
            }
            finally { game.Owner.View.transform.rotation = camera; view.enabled = true; view.Refresh(1); }
        }
        [UnityTest] public IEnumerator CutawayKeepsAnIntermediateAngleAndSettlesWhilePaused()
        {
            // Level 01 is now optical glass; level 02 retains the opaque cutaway contract.
            yield return Load(2);
            var view = game.GetComponent<COgheViewPresentation>(); view.enabled = false;
            int index = Array.FindIndex(view.Panes, p => p.Normal.z > .9f);
            var r = view.PaneVisuals[index].GetComponentInChildren<Renderer>(); var normal = view.Panes[index].Normal;
            var camera = game.Owner.View.transform.rotation; float time = game.Matter.SimulationTime;
            try
            {
                ViewFacing(normal, -view.AngleBand * .5f); view.Refresh(0);
                Assert.That(Opacity(r), Is.InRange(.25f, .75f), "A shallow fixed angle stays partially visible");
                float opacity = Opacity(r); view.Refresh(.5f); Assert.AreEqual(opacity, Opacity(r));
                game.Owner.TogglePause(); ViewFacing(normal, -.6f); view.Refresh(.02f);
                Assert.Greater(Opacity(r), opacity); Assert.Less(Opacity(r), 1);
                view.Refresh(1); Assert.AreEqual(1, Opacity(r)); Assert.AreEqual(time, game.Matter.SimulationTime);
                game.Owner.TogglePause();
            }
            finally { if(game.Owner.Paused)game.Owner.TogglePause(); game.Owner.View.transform.rotation = camera; view.enabled = true; view.Refresh(1); }
        }
        [UnityTest] public IEnumerator GlassDepthVariantsRetainExitAndClearContactAfterEscape()
        {
            int selection=COgheGlassDepthStudy.Selection;
            try
            {
                for(int variant=0;variant<4;variant++)
                {
                    COgheGlassDepthStudy.Selection=variant;yield return Load(1);yield return null;
                    var study=game.GetComponent<COgheGlassDepthStudy>();Assert.AreEqual(variant,study.Active);
                    game.CameraRig.Frame(720,1280,0,true);Capture(1,"depth-"+variant);
                    if(variant>0)
                    {
                        bool contact=false;var block=new MaterialPropertyBlock();
                        foreach(var renderer in study.Surfaces){renderer.GetPropertyBlock(block);contact|=block.GetFloat("_ContactAlpha")>0;}
                        Assert.IsTrue(contact,"Resting tissue must cast local contact shading");
                    }
                    yield return new COgheViewScenario(game,Tap,Until).Solve();yield return null;
                    Assert.AreEqual(32,game.Matter.EscapedCount);Assert.IsTrue(game.Owner.Completed);Assert.IsFalse(game.Owner.Lost);
                    if(variant>0)
                    {
                        var block=new MaterialPropertyBlock();
                        foreach(var renderer in study.Surfaces){renderer.GetPropertyBlock(block);Assert.AreEqual(0,block.GetFloat("_ContactAlpha"),"No stranded contact stain after exit");}
                    }
                }
            }
            finally{COgheGlassDepthStudy.Selection=selection;}
        }
        [UnityTest] public IEnumerator GlassPilotKeepsTransparentPanesAndPhysicsThroughOrbit()
        {
            yield return Load(1);
            var initialRotation=game.Root.rotation;
            var glass=Array.FindAll(game.Surfaces,p=>p.ExteriorGlass);
            Assert.GreaterOrEqual(glass.Length,5);
            var frame=game.Root.Find("Glass preview frame");Assert.IsNotNull(frame);
            Assert.IsEmpty(frame.GetComponentsInChildren<Collider>(),"Decorative frame cannot obstruct tissue or picking");
            foreach(float drag in new[]{180f,-360f,180f})
            {
                game.CameraRig.Orbit(drag,720);game.CameraRig.Frame(720,1280,0,true);
                game.GetComponent<COgheViewPresentation>().Refresh(1);
                foreach(var pane in glass)
                {
                    var r=pane.GetComponent<Renderer>();Assert.IsTrue(r.enabled);
                    Assert.That(r.sharedMaterial.shader.name,Is.EqualTo("COghe/Lab Glass").Or.EqualTo("COghe/Depth Study Glass"));
                    Assert.Less(r.sharedMaterial.GetColor("_BaseColor").a,.1f);
                    Assert.IsTrue(pane.Shape.enabled);
                }
                Assert.AreEqual(initialRotation,game.Root.rotation);
                Assert.IsNull(game.Motion.Get(0));
            }
        }
        [UnityTest] public IEnumerator View04HasAnInspectionRouteWhoseDetailsEnlargeWithZoom()
        {
            yield return Load(4);
            Assert.IsFalse(Array.Exists(game.Surfaces, p => p.name.StartsWith("Broad step") || p.name.StartsWith("Broad inclined")));
            var walls = Array.FindAll(game.Surfaces, p => p.name.StartsWith("Inspection baffle"));
            Assert.AreEqual(12, walls.Length); foreach(var wall in walls)Assert.IsTrue(wall.Slippery);
            var first = new Vector3(-.12f, -.298f, 0); var second = new Vector3(.12f, -.298f, 0);
            float size = Vector2.Distance(game.Owner.View.WorldToScreenPoint(first), game.Owner.View.WorldToScreenPoint(second));
            game.CameraRig.PinchAt(1.6f, game.Owner.View.WorldToScreenPoint((first+second)*.5f));
            game.CameraRig.Frame(720,1280,0,true);
            Assert.Greater(Vector2.Distance(game.Owner.View.WorldToScreenPoint(first), game.Owner.View.WorldToScreenPoint(second)),size*1.5f);
            Assert.IsTrue(game.FinalExitAvailable, "Inspection changes the view, not an invisible gate");
            Assert.IsNull(game.Motion.Get(0)); Capture(4,"inspection-zoom");
            game.ResetLevel(); Assert.AreEqual(1,game.CameraRig.ZoomScale);
        }
        private static IEnumerator NoZoom() { yield break; }
        [UnityTest] public IEnumerator View04KnownRouteCanBeCompletedWithoutAHiddenZoomFlag()
        {
            yield return Load(4);
            yield return new COgheViewScenario(game,Tap,Until,null,NoZoom).Solve();
            Assert.IsTrue(game.Owner.Completed); Assert.IsFalse(game.Owner.Lost);
            Assert.AreEqual(32,game.Matter.EscapedCount); Assert.AreEqual(1,game.Matter.TotalFragmentCount);
        }
    }
}
