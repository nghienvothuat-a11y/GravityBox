using System;
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
        [UnityTest,Timeout(900000)] public IEnumerator GlassRuleCampaignContract()
        {
            yield return Load(1);
            var scenes=(string[])game.Definition.SceneSequence.Clone();
            foreach(var name in scenes)
            {
                yield return LoadScene(name);
                var glass=game.Surfaces.Where(p=>p!=null&&p.ExteriorGlass).ToArray();
                Assert.IsNotEmpty(glass,name);
                foreach(var pane in glass)
                {
                    Assert.IsFalse(pane.Slippery,$"{name}/{pane.name}: invisible full-pane no-grip flag");
                    Assert.IsTrue(pane.Selectable,$"{name}/{pane.name}: glass must remain tappable");
                }
                if(game.Definition.Order<=6)
                {
                    Assert.IsTrue(glass.Any(p=>p.Hole),name+": opening exit must be in glass");
                    Assert.Greater(game.Root.InverseTransformPoint(game.Owner.Outlet.position).y,-.13f,name+": exit requires a climb");
                    Assert.IsTrue(glass.All(p=>!p.HasSlipRegion&&(p.SlipRegions==null||p.SlipRegions.Length==0)),name+": no perimeter coating in opening levels");
                    Assert.IsFalse(game.Surfaces.Any(p=>p.name.StartsWith("Climb board")||p.name=="Service panel"),name+": no auxiliary grip board");
                }
            }
        }

        [UnityTest] public IEnumerator GlassRuleBareWallClimbAndReturn()
        {
            yield return Load(1);
            var target=game.Root.TransformPoint(new Vector3(-.25f,-.045f,.30f));
            yield return Tap(target);
            yield return Until(25,()=>Vector3.Distance(game.Motion.Centre(0),target+Vector3.back*.022f)<.045f,"Climb bare glass from the floor");
            int feet=Enumerable.Range(0,32).Count(i=>game.Motion.HasGrip(i)&&game.Motion.Support(i,out var shape,out _,out _)&&shape.GetComponent<VenomSurfacePatch>()?.ExteriorGlass==true);
            Assert.GreaterOrEqual(feet,2,"Climb is supported by the glass, not airborne reaching");
            Capture(1,"glass-climb");
            var home=game.Owner.Spawn.position+Vector3.down*.05f;
            yield return Tap(home);
            yield return Until(25,()=>Vector3.Distance(game.Motion.Centre(0),home+Vector3.up*.025f)<.06f,"Crawl back down to the floor");
            yield return new COgheSpatialScenario(game,Tap,Until).Solve();
            Assert.AreEqual(32,game.Matter.EscapedCount);
        }

        [UnityTest] public IEnumerator GlassRulePulleyCannotBypassOnGlass()
        {
            yield return Load(6);
            var pulley=game.Owner.Apparatus.GetComponentInChildren<COghePulleyDrive>();
            // Real taps on high glass, the far plinth and outlet must not bypass the unraised bridge.
            foreach(var local in new[]{new Vector3(.26f,-.04f,.30f),new Vector3(.4f,.10f,.10f),new Vector3(.24f,-.10f,.16f)})
            {
                yield return Tap(game.Root.TransformPoint(local));yield return Wait(10);
                Assert.IsFalse(game.Owner.Completed,"Unraised pulley bypassed by a glass command");
            }
            yield return Tap(game.Owner.Outlet.position);yield return Wait(15);
            Assert.IsFalse(game.Owner.Completed);Assert.IsFalse(pulley.Output.AtEnd);
            game.ResetLevel();yield return Wait(1);
            yield return new COgheSpatialScenario(game,Tap,Until).Solve();
            Assert.AreEqual(32,game.Matter.EscapedCount);
        }

        [UnityTest,Timeout(900000)] public IEnumerator GlassRuleCampaignDirectExitSweep()
        {
            yield return Load(1);var scenes=(string[])game.Definition.SceneSequence.Clone();
            var bypassed=new System.Collections.Generic.List<string>();
            // 1–3 intentionally have no locked mechanism. Later layouts must still require their authored apparatus.
            foreach(var name in scenes.Skip(3))
            {
                yield return LoadScene(name);
                yield return Tap(game.Owner.Outlet.position);yield return Wait(25);
                if(game.Owner.Completed)bypassed.Add(game.Definition.Order+": "+name);
            }
            Assert.IsEmpty(bypassed,"Mechanism bypass from a direct outlet tap: "+string.Join(", ",bypassed));
        }
    }
}
