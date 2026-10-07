using System.Collections;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // COghe's eyes (Mrk 07/10/2026: "tao chốt phương án có mắt" and "lúc chiến thắng phải cho nó có mắt thể hiện sự vui vẻ").
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator COgheHasEyesAndTheyAreHappyInTheVictoryDance()
  {
   Assert.IsTrue(COgheEyes.Enabled,"Eyes are on by default");
   yield return Load(1);
   for(int i=0;i<60;i++){for(int k=0;k<4;k++)Tick();yield return null;}
   var view=game.Matter.transform.Find("COghe eyes");
   Assert.IsNotNull(view,"The eyes ride on COghe's skin");Assert.IsTrue(view.gameObject.activeSelf);
   var mesh=view.GetComponent<MeshFilter>().sharedMesh;
   Assert.Greater(mesh.vertexCount,0,"and are drawn while it rests");
   Assert.AreEqual(COgheMood.Normal,game.Personality.EyeMood(game.Matter.Groups[0],out _));
   yield return new COgheSpatialScenario(game,Tap,Until).Solve();
   yield return Until(10,()=>game.Owner.Celebration.Active,"The victory dance");
   for(int i=0;i<45;i++){for(int k=0;k<4;k++)Tick();yield return null;}
   Assert.AreEqual(COgheMood.Happy,game.Personality.EyeMood(game.Matter.Groups[0],out _),"Happy eyes for the win");
   Assert.Greater(mesh.vertexCount,0,"drawn through the dance, though COghe has left the box");
   COgheEyes.Enabled=false;
   try{for(int i=0;i<2;i++){Tick();yield return null;}Assert.IsFalse(view.gameObject.activeSelf,"Switched off, they are gone");}
   finally{COgheEyes.Enabled=true;}
  }
 }
}
