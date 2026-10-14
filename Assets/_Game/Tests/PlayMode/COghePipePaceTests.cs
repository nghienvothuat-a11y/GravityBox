using System.Collections;
using System.IO;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator Spatial14PipePaceKeepsTheWholeBody()
  {
   yield return Load(14);var s=new COgheSpatialNextScenario(game,Tap,Until);
   var tube=s.Find<COgheTubeNetwork>();
   yield return s.Operate("A");yield return s.Until(8,()=>tube.IsEntryOpen(0),"Open the pipe");
   yield return s.EnterTube(tube,0);float start=game.Matter.SimulationTime;
   int maxFragments=1;
   while(tube.IsParticleInside(game.Motion.Selected)&&game.Matter.SimulationTime-start<35)
   {
    Tick();maxFragments=Mathf.Max(maxFragments,game.Matter.TotalFragmentCount);
    if((int)((game.Matter.SimulationTime-start)/Dt)%240==0)yield return null;
   }
   float elapsed=game.Matter.SimulationTime-start;
   Assert.IsFalse(tube.IsParticleInside(game.Motion.Selected),"The tail must clear the physical mouth");
   Assert.AreEqual(1,maxFragments,"Faster flow must not tear the creature at bends or the outlet");
   Assert.IsFalse(game.Owner.Lost);
   // Baseline before the 1.5x drive: 10.658 simulated seconds on this same physical route.
   Assert.Less(elapsed,8.5f,"Pipe flow has regressed to the old pace");
   Directory.CreateDirectory("Artifacts/COghePipeCircuit20261014");
   File.WriteAllText("Artifacts/COghePipeCircuit20261014/pipe-pace.txt",elapsed.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
   Debug.Log($"PIPE PACE: entry to complete tail release {elapsed:F3}s; peak fragments {maxFragments}");
  }
 }
}
