using System;
using System.Collections;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace GravityBox.Tests
{
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator RoundedDockSkinsHandOffAndRetryAcrossCatalog()
  {
   var catalog=Resources.Load<COgheProductCatalog>("COgheUI/Catalog");int checkedDecks=0;
   foreach(var definition in catalog.Levels)
   {
    yield return LoadScene(definition.SceneSequence[definition.Order-1]);
    foreach(var deck in game.GetComponentsInChildren<COgheDockedBridgeDeck>(true))
    {
     if(deck.MovingArt==null||deck.MovingArt.Length==0)continue;
     deck.ResetMechanism(game);
     Assert.IsTrue(deck.MovingArt.All(r=>r!=null&&r.enabled),definition.Id+" moving shell");
     if(deck.DockedSurfaces==null||deck.DockedSurfaces.Length<4)continue;
     Assert.IsNotEmpty(deck.DockedArt,definition.Id+" missing rounded dock shell");
     Assert.IsTrue(deck.DockedArt.All(r=>!r.enabled));
     var rail=deck.Rail;
     rail.Body.position=rail.Frame.TransformPoint(rail.Start+rail.Axis.normalized*rail.Travel);
     rail.Body.linearVelocity=Vector3.zero;deck.StepMechanism(game,Dt);
     Assert.IsTrue(deck.IsDocked);
     Assert.IsTrue(deck.MovingArt.All(r=>!r.enabled));Assert.IsTrue(deck.DockedArt.All(r=>r.enabled));
     Assert.IsTrue(deck.DockedSurfaces.All(f=>!f.GetComponent<Renderer>().enabled),"No sharp face overlays");
     foreach(var r in deck.DockedArt)
     {Assert.IsNull(r.GetComponent<Collider>());Assert.Greater(r.GetComponent<MeshFilter>().sharedMesh.vertexCount,24);}
     deck.ResetMechanism(game);
     Assert.IsTrue(deck.MovingArt.All(r=>r.enabled));Assert.IsTrue(deck.DockedArt.All(r=>!r.enabled));checkedDecks++;
    }
   }
   Assert.Greater(checkedDecks,20);
  }
  [UnityTest] public IEnumerator CargoLiftHasNoRedLowerCallAndCanBeRetrieved()
  {
   yield return Load(13);var lift=game.GetComponentInChildren<COghePassengerLift>();
   Assert.IsNull(lift.CallPanels[0]);Assert.IsTrue(lift.EmptyTrayCallFromLowerLanding);
   Assert.IsFalse(game.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="A lower call panel"));
   yield return Tap(lift.Panel.position);yield return Until(45,()=>lift.Trips==1&&!lift.Moving,"Ride empty cargo lift up");
   var s=new COgheSpatialNextScenario(game,Tap,Until);
   yield return s.Go(game.Root.TransformPoint(new Vector3(-.25f,-.268f,-.16f)),"Back to the lower landing",.06f);
   yield return Tap(lift.Panel.position);yield return Until(45,()=>lift.Trips==2&&!lift.Moving,"Call the empty tray down without a second floor button");
   Assert.Less(lift.Rail.Position,.01f);
  }
  [UnityTest] public IEnumerator LatchedDeckKeepsGripThroughLoadSettlingButReleasesOnReverse()
  {
   yield return LoadPlus("B1");
   var task=game.GetComponentsInChildren<COgheTapRail>().Single(t=>t.Label=="C2");
   var rail=task.Rail;var deck=rail.GetComponent<COgheDockedBridgeDeck>();
   rail.Body.position=rail.Frame.TransformPoint(rail.Start+rail.Axis.normalized*rail.Travel);
   rail.Body.linearVelocity=Vector3.zero;rail.StepMechanism(game,Dt);deck.StepMechanism(game,Dt);
   Assert.IsTrue(rail.Latched);Assert.IsTrue(deck.IsDocked);
   rail.Body.position=rail.Frame.TransformPoint(rail.Start+rail.Axis.normalized*(rail.Travel-rail.CatchTolerance*1.2f));
   deck.StepMechanism(game,Dt);
   Assert.IsFalse(rail.AtEnd);Assert.IsTrue(deck.IsDocked,"A loaded pawl must not withdraw the surface under the rider");
   rail.ApplyEffort(-rail.WorldAxis*(rail.Resistance+.02f));rail.StepMechanism(game,Dt);deck.StepMechanism(game,Dt);
   Assert.IsFalse(deck.IsDocked,"Deliberate reverse pulling releases the contact replica");
  }
  [UnityTest] public IEnumerator Stair31HasFourClearElevationsAndStillSolves()
  {
   yield return Load(22);var tasks=game.GetComponentsInChildren<COgheTapRail>();
   float Top(string label)=>tasks.Single(t=>t.Label==label).Rail.GetComponentsInChildren<VenomSurfacePatch>(true).Where(f=>f.Normal.y>.9f).Max(f=>f.transform.position.y);
   Assert.AreEqual(.03f,Top("B")-Top("A"),.002f);Assert.AreEqual(.031f,Top("C")-Top("B"),.002f);
   var exit=game.Surfaces.Where(f=>f.name=="Exit platform"&&f.Normal.y>.9f).Single();
   Assert.AreEqual(.029f,exit.transform.position.y-Top("C"),.002f);
   yield return new COgheSpatialScenario(game,Tap,Until).Solve();Assert.AreEqual(32,game.Matter.EscapedCount);
  }
  [UnityTest] public IEnumerator Refined19Wander(){yield return Wander(Load(13),"13");}
  [UnityTest] public IEnumerator Refined31Wander(){yield return Wander(Load(22),"22");}
  [UnityTest] public IEnumerator Refined21NeedsTwoSimultaneousRoles()
  {
   COgheIntelligence.ForceForTests=true;
   var book=new COgheSolutionSteps.Book();var level=new COgheSolutionSteps.Level{Id="coghe.spatial.plus.b1"};
   level.Steps.Add(new COgheSolutionSteps.Step{Path="B carriage",Kind=COgheSolutionSteps.RailKind});book.Levels.Add(level);COgheSolutionSteps.ResetForTests(book);
   try
   {
   yield return LoadPlus("B1");var s=new COgheSpatialNextScenario(game,Tap,Until);
   var latch=game.GetComponentInChildren<COgheLoadLatch>();
   Assert.AreEqual(1,latch.Holds.Length);Assert.IsEmpty(latch.Rails);
   var q=game.GetComponentInChildren<COgheQuantumSplitter>();
   yield return s.Split(q,game.Motion.Selected);int climber=q.LastLeft,other=q.LastRight;
   yield return s.Walk(other,game.Root.TransformPoint(new Vector3(.30f,-.298f,-.04f)),"Park the other half off A");
   yield return s.Walk(climber,game.Root.TransformPoint(new Vector3(.02f,-.21f,.06f)),"One half climbs C1");
   yield return s.Walk(climber,game.Root.TransformPoint(new Vector3(-.28f,-.12f,.16f)),"One half onto B shelf");
   game.SelectFragment(climber);
   yield return s.Hold("B");Assert.IsFalse(latch.Caught,"B alone cannot unlock C1");
   yield return Wait(.1f);Assert.AreEqual(1,game.Intelligence.Progress,"Clever recognizes a held lever without a completed journey");
   yield return s.Go(game.Root.TransformPoint(new Vector3(.30f,-.298f,-.20f)),"Leave B and load A alone");yield return Wait(2);
   Assert.IsFalse(latch.Caught,"B is no longer held; sequential single-body actions must not bypass Q");
   Assert.IsFalse(s.Task("B").Holding);
   }
   finally{COgheIntelligence.ForceForTests=false;COgheSolutionSteps.ResetForTests();}
  }
  [UnityTest] public IEnumerator Refined21And31RequireTheirMechanisms()
  {
   foreach(string key in new[]{"B1","22"})
   {
    yield return LoadScene(key=="B1"?"COgheSpatialPlusB1":"COgheSpatial22");
    foreach(var point in new[]{new Vector3(-.39f,.24f,0),new Vector3(.39f,.24f,0),new Vector3(0,.24f,.29f),new Vector3(0,.24f,-.29f)})
    {
     game.ResetLevel();yield return Wait(.6f);yield return Tap(game.Root.TransformPoint(point));yield return Wait(12);
     yield return Tap(game.Owner.Outlet.position);yield return Wait(15);
     Assert.IsFalse(game.Owner.Completed,key+": glass/exit shortcut without the mechanisms");
    }
   }
  }
  [UnityTest] public IEnumerator Refined21NeedsC3AndRecoversByMovingIt()
  {
   yield return LoadPlus("B1");var s=new COgheSpatialNextScenario(game,Tap,Until);
   Vector3 W(float x,float y,float z)=>game.Root.TransformPoint(new Vector3(x,y,z));
   var q=game.GetComponentInChildren<COgheQuantumSplitter>();var latch=game.GetComponentInChildren<COgheLoadLatch>();
   yield return s.Split(q,game.Motion.Selected);int climber=q.LastLeft,holder=q.LastRight;
   yield return s.Walk(holder,W(.30f,-.298f,-.20f),"Half loads A");
   yield return s.Walk(climber,W(.02f,-.21f,.06f),"Climb C1");yield return s.Walk(climber,W(-.28f,-.12f,.16f),"Reach B");
   game.SelectFragment(climber);yield return s.Hold("B");yield return Until(10,()=>latch.Caught,"Catch C1");
   yield return s.Operate("C2");yield return Until(30,()=>s.Task("C2").Rail.AtEnd,"C2 across");
   yield return s.Merge(W(-.10f,-.30f,-.08f));
   yield return s.Operate("C1");yield return Until(60,()=>s.Task("C1").Rail.AtEnd,"Haul C1 before moving C3");
   yield return s.Go(W(-.04f,-.21f,.08f),"Up C1");yield return s.Go(W(.06f,-.12f,.08f),"Up C2");
   yield return s.Go(W(.17f,-.03f,.07f),"Up C3 in its wrong position");
   yield return Tap(game.Owner.Outlet.position);yield return Wait(12);
   Assert.IsFalse(game.Owner.Completed,"C3 must align the rear landing; it is not a redundant prop");
   Assert.AreEqual(0,game.Matter.EscapedCount);
   yield return s.Go(W(.06f,-.12f,.08f),"Back down to C2");
   yield return s.Operate("C3");yield return Until(30,()=>s.Task("C3").Rail.AtEnd,"Align C3 after moving C1");
   yield return s.Go(W(.17f,-.03f,.21f),"Up aligned C3");yield return s.Go(W(.31f,-.03f,.27f),"Onto rear tower");
   yield return Tap(game.Owner.Outlet.position);yield return Until(40,()=>game.Owner.Completed,"Exit after correcting the stack");
   Assert.AreEqual(32,game.Matter.EscapedCount);
  }
  IEnumerator RefinedFrameSolve(string key,int fps)
  {
   Time.captureFramerate=fps;ticksPerFrame=Mathf.RoundToInt(120f/fps);
   try{yield return LoadScene(char.IsDigit(key[0])?"COgheSpatial"+key:"COgheSpatialPlus"+key);yield return new COgheSpatialScenario(game,Tap,FrameUntil).Solve();Assert.AreEqual(32,game.Matter.EscapedCount);}
   finally{Time.captureFramerate=0;ticksPerFrame=2;}
  }
  [UnityTest,Timeout(900000)] public IEnumerator Refined21At30Fps(){yield return RefinedFrameSolve("B1",30);}
  [UnityTest,Timeout(900000)] public IEnumerator Refined21At60Fps(){yield return RefinedFrameSolve("B1",60);}
  [UnityTest,Timeout(900000)] public IEnumerator Refined31At30Fps(){yield return RefinedFrameSolve("22",30);}
  [UnityTest,Timeout(900000)] public IEnumerator Refined31At60Fps(){yield return RefinedFrameSolve("22",60);}
  [UnityTest,Timeout(900000)] public IEnumerator Refined17At30Fps(){yield return RefinedFrameSolve("N13",30);}
  [UnityTest,Timeout(900000)] public IEnumerator Refined17At60Fps(){yield return RefinedFrameSolve("N13",60);}
  [UnityTest] public IEnumerator Refined17StillNeedsTheTallCrate()
  {
   yield return LoadPlus("N13");var s=new COgheSpatialNextScenario(game,Tap,Until);
   yield return s.Go(game.Root.TransformPoint(new Vector3(-.028f,-.27f,.14f)),"Mount B without moving A");
   yield return Tap(game.Root.TransformPoint(new Vector3(.27f,-.21f,.20f)));yield return Wait(15);
   yield return Tap(game.Owner.Outlet.position);yield return Wait(15);
   Assert.IsFalse(game.Owner.Completed);Assert.AreEqual(0,game.Matter.EscapedCount);
  }
 }
 public partial class COgheProductUITests
 {
  [UnityTest] public IEnumerator FlowerCrownKeepsItsOwnColourPalette()
  {
   ui.Play();yield return new WaitForSeconds(.5f);
   var wear=COgheAccessories.Attach(game);wear.Dress("HAT_FLOWER",Array.Empty<string>());
   yield return new WaitForSeconds(.8f);
   var renderer=wear.Holder.GetComponentInChildren<MeshRenderer>();
   Assert.NotNull(renderer);Assert.GreaterOrEqual(renderer.sharedMaterials.Length,6);
   foreach(var material in renderer.sharedMaterials)
   {
    var color=material.GetColor("_BaseColor");
    Assert.Greater(color.maxColorComponent,.6f,"Petals must retain their saturated colour");
   }
   game.CameraRig.PinchAt(2.7f,game.Owner.View.WorldToScreenPoint(game.Motion.Centre(0)));
   yield return new WaitForSeconds(.5f);
   void Photo(string suffix)
   {
    var cam=game.Owner.View;var rt=RenderTexture.GetTemporary(720,960,24);var tex=new Texture2D(720,960,TextureFormat.RGB24,false);
    var previous=RenderTexture.active;var old=cam.targetTexture;
    try
    {
     cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,720,960),0,0);tex.Apply();
     System.IO.File.WriteAllBytes("Artifacts/COghePlaytestRefinement/flower-"+suffix+".png",tex.EncodeToPNG());
     var centre=cam.WorldToViewportPoint(renderer.bounds.center);int brightPetals=0;
     for(int y=Mathf.Max(0,(int)(centre.y*960)-70);y<Mathf.Min(960,(int)(centre.y*960)+70);y++)
      for(int x=Mathf.Max(0,(int)(centre.x*720)-70);x<Mathf.Min(720,(int)(centre.x*720)+70);x++)
      {var c=tex.GetPixel(x,y);if(c.r>.45f&&c.r>c.g*1.25f&&c.r>c.b*1.1f)brightPetals++;}
     Assert.Greater(brightPetals,10,"Actual lit crown must show colourful petals, not only correctly stored material values");
    }
    finally{cam.targetTexture=old;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);}
   }
   Photo("editor");
  }
  [UnityTest] public IEnumerator RefinedCatalogMovesF1WithoutLosingEarnedMilestones()
  {
   Assert.AreEqual(67,ui.Catalog.Levels.Length);Assert.AreEqual("coghe.spatial.plus.f1",ui.Catalog.Levels[14].Id);
   Assert.IsFalse(ui.Catalog.Levels.Any(l=>l.Id=="coghe.spatial.plus.n29"));
   foreach(int n in new[]{10,21,32,43,54,65})Assert.IsTrue(ui.Catalog.Levels[n-1].Boss);
   COgheShop.ResetForTests(new COgheShop.State{Migrated=true,PacingRevision=2});
   game.Progress.Completed.Add("coghe.spatial.plus.n29");game.Progress.Completed.Add("coghe.spatial.plus.f1");
   COgheShop.Migrate(game);CollectionAssert.Contains(COgheShop.Current.ReachedBeforePacing,15);CollectionAssert.Contains(COgheShop.Current.ReachedBeforePacing,66);
   Assert.AreEqual(3,COgheShop.Current.PacingRevision);yield return null;
  }
  [UnityTest] public IEnumerator CleverReadsButPlayerCommandRemainsImmediate()
  {
   bool enabled=COghePersonality.Enabled;COghePersonality.Enabled=true;
   try
   {
    COgheShop.Current.Intelligence=1;COgheShop.Current.LastAbility=2;ui.Play();
    float deadline=Time.realtimeSinceStartup+6;
    while(game.Personality.Act!=COgheAct.Read&&Time.realtimeSinceStartup<deadline)yield return null;
    Assert.AreEqual(COgheAct.Read,game.Personality.Act);yield return new WaitForSeconds(.8f);
    var book=Object.FindFirstObjectByType<COgheReadingBook>();Assert.NotNull(book);Assert.IsTrue(book.Visible);
    Assert.AreEqual(COgheMood.Focus,game.Personality.EyeMood(0,out var look));Assert.IsTrue(look.HasValue);
    Assert.AreEqual(0,book.GetComponentsInChildren<Collider>().Length);Assert.IsFalse(game.Personality.LocksInput);
    game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Root.TransformPoint(new Vector3(.20f,-.30f,.10f))));
    yield return new WaitForSeconds(.45f);Assert.IsFalse(book.Visible);Assert.IsTrue(game.Owner.CanControl);
   }
   finally{COghePersonality.Enabled=enabled;}
  }
  [UnityTest] public IEnumerator SpeedyWaterFollowsWalkAndClearsInMenu()
  {
   COgheShop.Current.Agility=1;COgheShop.Current.LastAbility=1;ui.Play();yield return new WaitForSeconds(.5f);
   var fx=ui.GetComponentInChildren<COgheAbilityFx>();Assert.AreEqual(0,fx.WaterSegments);
   game.Motion.Move(0,game.Root.TransformPoint(new Vector3(.25f,-.30f,.12f)));
   float deadline=Time.realtimeSinceStartup+3;while(fx.WaterSegments<3&&Time.realtimeSinceStartup<deadline)yield return null;
   Assert.GreaterOrEqual(fx.WaterSegments,3);Assert.LessOrEqual(fx.WaterSegments,80);
   var water=Object.FindFirstObjectByType<COgheWaterTrail>();Assert.NotNull(water);Assert.IsNull(water.GetComponent<Collider>());
   Assert.AreEqual(640,water.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3);
   ui.ShowMenu();yield return null;Assert.AreEqual(0,fx.WaterSegments);Assert.IsFalse(water.GetComponent<Renderer>().enabled);
  }
 }
}
