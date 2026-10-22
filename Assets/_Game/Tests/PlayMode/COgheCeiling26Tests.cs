using System;
using System.Collections;
using System.IO;
using System.Text;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 public sealed partial class COgheSpatialCampaignTests
 {
  // Display 26 = G3. A direct exit command used a different path policy from a far-floor command and tried the
  // slick divider. All regression actions below are screen-projected taps; no teleport, forced rail or forced win.
  private IEnumerator Ceiling26Ride()
  {
   yield return LoadPlus("G3");
   yield return RailTrip("B");
   var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();
   yield return Tap(lift.Panel.position);
   yield return FrameUntil(45,()=>lift.Trips==1&&lift.Rail.AtEnd&&!lift.Moving,"Lift stopped at the upper landing");
   Capture(26,"upper-landing");
  }
  private IEnumerator Ceiling26Trace(string name,float seconds,Func<bool> done)
  {
   var log=new StringBuilder();int command=-1;
   for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++)
   {
    var order=game.Motion.Get(game.Motion.Selected);
    if(order!=null&&order.CommandId!=command){command=order.CommandId;log.AppendLine($"command {command} target={order.Target:F3} exit={order.Exit} path={string.Join(" > ",order.Path)}");}
    Tick();
    if(i%60==0)log.AppendLine($"{i*Dt:F1}s body={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} cursor={order?.Cursor} activity={game.Activity}");
    if(i%ticksPerFrame==ticksPerFrame-1)yield return null;
   }
   Directory.CreateDirectory("Artifacts/Ceiling26");File.WriteAllText($"Artifacts/Ceiling26/{name}.txt",log.ToString());
   Capture(26,name);
  }
  private IEnumerator Ceiling26Cross(Vector3? target,string name)
  {
   yield return Ceiling26Ride();
   yield return Tap(target.HasValue?game.Root.TransformPoint(target.Value):game.Owner.Outlet.position);
   bool crossedAtCeiling=false;
   yield return Ceiling26Trace(name,35,()=>
   {
    var p=game.Root.InverseTransformPoint(game.Motion.Centre(0));
    crossedAtCeiling|=p.x>-.02f&&p.x<.08f&&p.y>.045f;
    return game.Owner.Completed||p.x>.14f;
   });
   Assert.IsTrue(game.Owner.Completed||game.Root.InverseTransformPoint(game.Motion.Centre(0)).x>.14f,"Cross after the lift stops: "+name);
   Assert.IsTrue(crossedAtCeiling,"The crossing must actually use the underside of the roof plate");
   yield return Tap(game.Owner.Outlet.position);
   yield return FrameUntil(25,()=>game.Owner.Completed,"Exit after ceiling crossing");
   Assert.AreEqual(32,game.Matter.EscapedCount);
  }
  [UnityTest,Timeout(900000)] public IEnumerator Ceiling26ExitFromUpperLanding(){yield return Ceiling26Cross(null,"exit");}
  [UnityTest,Timeout(900000)] public IEnumerator Ceiling26FloorFromUpperLanding(){yield return Ceiling26Cross(new Vector3(.25f,-.30f,.05f),"floor");}
  // In the default view, this screen location sees through the roof and hits high on the back glass.
  [UnityTest,Timeout(900000)] public IEnumerator Ceiling26HighGlassFromUpperLanding(){yield return Ceiling26Cross(new Vector3(.25f,.10f,.12f),"high-glass");}
  [UnityTest,Timeout(900000)] public IEnumerator Ceiling26ExitFromUpperLandingAt30Fps()
  {
   ticksPerFrame=4;Time.captureFramerate=30;
   try{yield return Ceiling26Cross(null,"exit-30fps");}
   finally{ticksPerFrame=2;Time.captureFramerate=0;}
  }
  [UnityTest,Timeout(900000)] public IEnumerator Ceiling26RecallAfterLeavingUpperLanding()
  {
   yield return Ceiling26Ride();
   yield return Tap(game.Root.TransformPoint(new Vector3(-.24f,-.30f,-.17f)));
   yield return FrameUntil(25,()=>game.Root.InverseTransformPoint(game.Motion.Centre(0)).y<-.25f,"Return to lower floor");
   var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();
   yield return Tap(lift.Panel.position);
   yield return Ceiling26Trace("recall",25,()=>lift.Rail.Position<=lift.Rail.CatchTolerance&&!lift.Moving);
   Assert.IsTrue(lift.Rail.Position<=lift.Rail.CatchTolerance&&!lift.Moving,"An empty raised lift must be recoverable from its lower landing");
   yield return Tap(lift.Panel.position);
   yield return FrameUntil(45,()=>lift.Trips==3&&lift.Rail.AtEnd&&!lift.Moving,"Board and ride again after recalling the tray");
   yield return Tap(game.Owner.Outlet.position);
   yield return FrameUntil(30,()=>game.Owner.Completed,"Recovery still ends in a full solve");
   Assert.AreEqual(32,game.Matter.EscapedCount);
  }
  [UnityTest,Timeout(900000)] public IEnumerator Ceiling26DepartureDuringRiseCanRecover()
  {
   yield return LoadPlus("G3");yield return RailTrip("B");
   var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();
   yield return Tap(lift.Panel.position);
   yield return FrameUntil(45,()=>lift.Moving&&lift.Rail.Position>.06f,"Start the ride");
   yield return Tap(game.Root.TransformPoint(new Vector3(-.24f,-.30f,-.17f)));
   yield return FrameUntil(25,()=>!lift.Moving&&game.Root.InverseTransformPoint(game.Motion.Centre(0)).y<-.25f,"Leave the moving lift for the lower floor");
   Assert.Less(lift.Rail.Position,lift.Rail.Travel-lift.Rail.CatchTolerance,"Passengerless ascent must stop");
   yield return Tap(lift.Panel.position);
   yield return FrameUntil(25,()=>!lift.Moving&&lift.Rail.Position<=lift.Rail.CatchTolerance,"Call the interrupted lift down");
   int trips=lift.Trips;yield return Tap(lift.Panel.position);
   yield return FrameUntil(45,()=>lift.Trips>trips&&lift.Rail.AtEnd&&!lift.Moving,"Board before going up again");
   yield return Tap(game.Owner.Outlet.position);
   yield return FrameUntil(30,()=>game.Owner.Completed,"Solve after interrupting the lift");
   Assert.AreEqual(32,game.Matter.EscapedCount);
  }
  [UnityTest,Explicit,Timeout(900000)] public IEnumerator Ceiling26Film()
  {
   yield return Ceiling26Ride();
   string dir="Artifacts/Ceiling26/film";Directory.CreateDirectory(dir);int frames=0,ticks=0;
   trace=()=>{if(++ticks%8==0)FilmFrame($"{dir}/f{frames++:00000}.jpg");};
   try
   {
    yield return Tap(game.Owner.Outlet.position);
    yield return FrameUntil(30,()=>game.Owner.Completed,"Direct exit tap crosses the ceiling and wins");
    yield return Wait(2);
   }
   finally{trace=null;}
   Assert.AreEqual(32,game.Matter.EscapedCount);
  }
 }
}
