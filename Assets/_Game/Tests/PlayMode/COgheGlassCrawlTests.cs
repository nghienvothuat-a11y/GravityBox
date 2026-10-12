using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using GravityBox.Venom;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // The glass-crawl levels G1–G5 (Mrk 12/10/2026): COghe climbs the glass, but the slick tape holds and the exit is only
 // reached through the mechanisms. From the start, with no mechanism touched: taps high on every tappable pane, on every
 // shelf and on the exit; COghe must not win (the highest point it reached is logged: bare lanes may take it high). The
 // tape's own hold is checked from the floor by GlassCrawlTapeHolds.
 public sealed partial class COgheSpatialCampaignTests
 {
  private IEnumerator GlassCrawlNoShortcut(string key)
  {
   yield return LoadPlus(key);yield return Wait(.5f);
   Directory.CreateDirectory("Artifacts/GlassCrawl");string log=$"Artifacts/GlassCrawl/shortcut-{key}.txt";File.WriteAllText(log,"");
   var targets=new System.Collections.Generic.List<Vector3>();
   // high on the back and right glass (the panes the camera sees from inside), and every shelf's top
   foreach(float x in new[]{-.30f,-.10f,.10f,.30f})foreach(float y in new[]{-.15f,.0f,.07f})targets.Add(new Vector3(x,y,.30f));
   foreach(float z in new[]{-.20f,.0f,.20f})foreach(float y in new[]{-.15f,.0f,.07f})targets.Add(new Vector3(.40f,y,z));
   foreach(var f in game.Surfaces)if(f!=null&&f.isActiveAndEnabled&&f.MotionFrame==null&&!f.ExteriorGlass&&f.Normal.y>.9f&&f.name!="Laboratory floor")
    targets.Add(game.Root.InverseTransformPoint(f.transform.position));
   float top=-1;
   foreach(var local in targets)
   {
    yield return Tap(game.Root.TransformPoint(local));
    for(int i=0;i<10/Dt&&!game.Owner.Completed&&!game.Owner.Lost;i++){Tick();if(i%60==0)yield return null;}
    var c=game.Root.InverseTransformPoint(game.Motion.Centre(0));top=Mathf.Max(top,c.y);
    File.AppendAllText(log,$"tap {local:F2} -> body {c:F3}\n");
    Assert.IsFalse(game.Owner.Completed,$"{key}: won without the mechanisms after a tap at {local}");
    if(game.Owner.Lost){File.AppendAllText(log,"lost: "+game.Failure+"\n");game.ResetLevel();yield return Wait(.5f);}
   }
   yield return Tap(game.Owner.Outlet.position);
   for(int i=0;i<15/Dt&&!game.Owner.Completed;i++){Tick();if(i%60==0)yield return null;}
   Assert.IsFalse(game.Owner.Completed,$"{key}: the exit let COghe out without the mechanisms");
   File.AppendAllText(log,$"highest body centre {top:F3}\n");
  }
  // From the floor, the tape alone keeps COghe off the glass: a tap high on taped glass leaves it below the tape's top.
  private IEnumerator GlassCrawlTapeHolds(string key,Vector3 target)
  {
   Directory.CreateDirectory("Artifacts/GlassCrawl");
   yield return LoadPlus(key);yield return Wait(.5f);
   yield return Tap(game.Root.TransformPoint(target));
   float highest=-1;
   string where="";
   for(int i=0;i<12/Dt;i++)
   {
    Tick();
    foreach(var b in game.Matter.Bodies)
    {
     var p=game.Root.InverseTransformPoint(b.position);
     if(p.y>-.19f&&where=="")where=$" first above the tape at {p:F3}, body {game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3}, t {i*Dt:F1}s";
     highest=Mathf.Max(highest,p.y);
    }
    if(i%60==0)yield return null;
   }
   File.AppendAllText("Artifacts/GlassCrawl/tape.txt",$"{key} target {target:F2}: highest particle {highest:F3}{where}\n");
   Assert.Less(highest,-.30f+.11f,$"{key}: COghe got above the tape toward {target}");
  }
  // The reference solution with a frame every two physics steps (a 60 fps phone; 120 Hz physics) or every four (30 fps, as
  // the review clips): whatever runs per frame (Update, FixedUpdate) runs as often as in play, not once every 2 s as in the
  // solve tests.
  private int ticksPerFrame=2;
  private IEnumerator FrameUntil(float seconds,Func<bool> done,string reason)
  {
   var trace=new System.Text.StringBuilder();
   for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++)
   {
    Tick();if(i%ticksPerFrame==ticksPerFrame-1)yield return null;
    if(i%60==0)trace.Append($" | {i*Dt:F1}s {game.Root.InverseTransformPoint(game.Motion.Centre(0)):F2} {game.Activity}");
   }
   Assert.IsTrue(done(),reason+$"; level={game.Definition.Order} body {game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} activity {game.Activity}{trace}");
  }
  private IEnumerator GlassCrawlSolveAtFrameRate(string key,int fps=60)
  {
   Time.captureFramerate=fps;ticksPerFrame=Mathf.RoundToInt(1/(Dt*fps));
   try
   {
    yield return LoadPlus(key);yield return Wait(.5f);
    yield return new GravityBox.Venom.ChapterProof.COgheSpatialScenario(game,Tap,FrameUntil).Solve();
    Assert.AreEqual(32,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Lost);
   }
   finally{Time.captureFramerate=0;ticksPerFrame=2;}
  }
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG1SolveAtFrameRate(){yield return GlassCrawlSolveAtFrameRate("G1");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG2SolveAtFrameRate(){yield return GlassCrawlSolveAtFrameRate("G2");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG3SolveAtFrameRate(){yield return GlassCrawlSolveAtFrameRate("G3");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG4SolveAtFrameRate(){yield return GlassCrawlSolveAtFrameRate("G4");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG5SolveAtFrameRate(){yield return GlassCrawlSolveAtFrameRate("G5");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG1SolveAt30Fps(){yield return GlassCrawlSolveAtFrameRate("G1",30);}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG2SolveAt30Fps(){yield return GlassCrawlSolveAtFrameRate("G2",30);}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG3SolveAt30Fps(){yield return GlassCrawlSolveAtFrameRate("G3",30);}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG4SolveAt30Fps(){yield return GlassCrawlSolveAtFrameRate("G4",30);}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG5SolveAt30Fps(){yield return GlassCrawlSolveAtFrameRate("G5",30);}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG1NoShortcut(){yield return GlassCrawlNoShortcut("G1");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG2NoShortcut(){yield return GlassCrawlNoShortcut("G2");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG3NoShortcut(){yield return GlassCrawlNoShortcut("G3");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG4NoShortcut(){yield return GlassCrawlNoShortcut("G4");}
  [UnityTest,Timeout(900000)] public IEnumerator GlassCrawlG5NoShortcut(){yield return GlassCrawlNoShortcut("G5");}
  [UnityTest,Timeout(600000)] public IEnumerator GlassCrawlTapeHoldsRightWall(){yield return GlassCrawlTapeHolds("G2",new Vector3(.40f,.0f,.10f));}
  [UnityTest,Timeout(600000)] public IEnumerator GlassCrawlTapeHoldsBackWall(){Directory.CreateDirectory("Artifacts/GlassCrawl");yield return GlassCrawlTapeHolds("G2",new Vector3(-.25f,.0f,.30f));}
 }
}
