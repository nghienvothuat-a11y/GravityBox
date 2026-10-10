using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace GravityBox.Tests
{
 // Level 07 "Chìa khoá dưới hố" (play position 8) probes (Mrk 09/10/2026, playtest): a first tap straight on the exit sent
 // COghe into the pit where it stayed; another time it fell out of the box and the level was lost. Explicit: they write
 // a log and frames to Artifacts/L07 for the investigation.
 public sealed partial class COgheSpatialCampaignTests
 {
  private string probeLog;
  private void Note(string line){File.AppendAllText(probeLog,line+"\n");}
  private string Where()
  {
   var c=Local(game.Motion.Centre(0));float minY=9,maxR=0;
   for(int i=0;i<32;i++){var p=Local(game.Matter.Bodies[i].position);minY=Mathf.Min(minY,p.y);maxR=Mathf.Max(maxR,game.Matter.Bodies[i].position.magnitude);}
   return $"centre {c:F3} minY {minY:F3} maxR {maxR:F2} act={game.Activity} lost={game.Owner.Lost} fail={game.Failure}";
  }
  private void Shot(string name)
  {
   Directory.CreateDirectory("Artifacts/L07");game.CameraRig.Frame(720,1280,0,true);var camera=game.Owner.View;
   var rt=RenderTexture.GetTemporary(720,1280,24);var tex=new Texture2D(720,1280,TextureFormat.RGB24,false);var prev=RenderTexture.active;
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,720,1280),0,0);tex.Apply();File.WriteAllBytes($"Artifacts/L07/{name}.png",tex.EncodeToPNG());}
   finally{camera.targetTexture=null;RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);}
  }
  // A close-up of COghe from the game's view direction and from above (for stuck reports).
  private void ShotClose(string name)
  {
   Directory.CreateDirectory("Artifacts/L07");game.CameraRig.Frame(720,1280,0,true);var view=game.Owner.View;var c0=game.Motion.Centre(0);
   var cam=new GameObject("Probe close camera").AddComponent<Camera>();cam.CopyFrom(view);cam.enabled=false;cam.orthographic=true;cam.aspect=1;cam.nearClipPlane=.01f;cam.farClipPlane=20;
   var rt=RenderTexture.GetTemporary(600,600,24);var tex=new Texture2D(600,600,TextureFormat.RGB24,false);var prev=RenderTexture.active;
   try
   {
    foreach(var (tag,rot) in new[]{("view",view.transform.rotation),("top",Quaternion.LookRotation(-game.Root.up,game.Root.forward)),("side",Quaternion.LookRotation(game.Root.right,game.Root.up))})
    {
     cam.transform.rotation=rot;cam.orthographicSize=.10f;cam.transform.position=c0-cam.transform.forward*2f;
     cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,600,600),0,0);tex.Apply();File.WriteAllBytes($"Artifacts/L07/{name}-{tag}.png",tex.EncodeToPNG());
    }
   }
   finally{cam.targetTexture=null;RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);Object.DestroyImmediate(cam.gameObject);}
  }
  // Can a player still get COghe home (to the start) with taps? The same detours the wander test uses.
  private IEnumerator TryHome(Vector3 home)
  {
   var detours=new List<Vector3>{Vector3.zero,new Vector3(-.30f,0,0),new Vector3(.30f,0,0),new Vector3(0,0,-.22f),new Vector3(0,0,.22f),new Vector3(-.045f,-.10f,.06f)};
   for(int attempt=0;attempt<=detours.Count&&!game.Owner.Lost&&Vector3.Distance(game.Motion.Centre(0),home)>.07f;attempt++)
   {
    if(attempt>0){var d=game.Root.TransformPoint(detours[attempt-1]+Vector3.down*.30f);yield return Tap(d);yield return WaitFor(20,()=>game.Motion.Get(0)==null);Note($"  detour {detours[attempt-1]:F2}: {Where()}");}
    yield return Tap(home+Vector3.down*.025f);yield return WaitFor(40,()=>Vector3.Distance(game.Motion.Centre(0),home)<.07f||game.Motion.Get(0)==null);
   }
  }

  [UnityTest,Explicit,Timeout(1800000)] public IEnumerator Level07ExitTapAtStart()
  {
   Directory.CreateDirectory("Artifacts/L07");probeLog="Artifacts/L07/exit-tap.txt";File.WriteAllText(probeLog,"");
   yield return Load(7);yield return Wait(1);var home=game.Motion.Centre(0);Shot("exit-0-start");
   Note("start: "+Where());
   foreach(var f in game.Surfaces)if(f!=null&&f.isActiveAndEnabled)Note($"  face {f.name} n={Local(f.transform.position+f.Normal)-Local(f.transform.position):F2} at {Local(f.transform.position):F3} size {f.Size:F3} slick={f.Slippery} glass={f.ExteriorGlass} hole={f.Hole}");
   yield return Tap(game.Owner.Outlet.position);
   for(int s=1;s<=20;s++){yield return Wait(1);Note($"t={s}s {Where()}");if(s==3||s==8||s==20)Shot($"exit-{s}s");}
   Note("try home:");yield return TryHome(home);Note("after: "+Where());Shot("exit-home");
   Assert.IsFalse(game.Owner.Lost,"Lost after the exit tap");
   Assert.Less(Vector3.Distance(game.Motion.Centre(0),home),.07f,"Trapped after the exit tap at "+Where());
  }

  // Random taps anywhere on the box, as a new player does: walls, the pit, the exit, the bridge, handles. After each run a
  // player must still be able to bring COghe home, and it must never leave the box.
  [UnityTest,Explicit,Timeout(3600000)] public IEnumerator Level07RandomTaps()
  {
   Directory.CreateDirectory("Artifacts/L07");probeLog="Artifacts/L07/random-taps.txt";File.WriteAllText(probeLog,"");
   int runs=int.TryParse(Environment.GetEnvironmentVariable("COGHE_PROBE_RUNS"),out int r)?r:12,trapped=0,lost=0;
   for(int run=0;run<runs;run++)
   {
    yield return Load(7);yield return Wait(1);var home=game.Motion.Centre(0);var rng=new System.Random(1000+run);
    game.CameraRig.Frame(720,1280,0,true);var cam=game.Owner.View;
    // the box's screen rectangle
    float x0=1e9f,x1=-1e9f,y0=1e9f,y1=-1e9f;
    foreach(var cx in new[]{-.4f,.4f})foreach(var cy in new[]{-.42f,.04f})foreach(var cz in new[]{-.3f,.3f})
    {var sp=cam.WorldToScreenPoint(game.Root.TransformPoint(new Vector3(cx,cy,cz)));x0=Mathf.Min(x0,sp.x);x1=Mathf.Max(x1,sp.x);y0=Mathf.Min(y0,sp.y);y1=Mathf.Max(y1,sp.y);}
    Note($"run {run}");
    for(int k=0;k<25&&!game.Owner.Lost;k++)
    {
     var screen=new Vector3((float)(x0+rng.NextDouble()*(x1-x0)),(float)(y0+rng.NextDouble()*(y1-y0)),0);
     // every fourth tap goes to the exit or into the pit, the two places the playtest found
     if(k%4==1)screen=cam.WorldToScreenPoint(game.Owner.Outlet.position);
     if(k%4==3)screen=cam.WorldToScreenPoint(game.Root.TransformPoint(new Vector3((float)(rng.NextDouble()*.16-.08),-.40f,(float)(rng.NextDouble()*.5-.25))));
     game.CameraRig.Frame(720,1280,0,true);game.TouchPoint(screen);yield return null;
     if(game.Attached)game.ReleaseProp();
     yield return Wait((float)(.5+rng.NextDouble()*5));
     Note($"  tap {k} screen {screen.x:F0},{screen.y:F0}: {Where()}");
    }
    if(game.Owner.Lost){lost++;Note("  LOST: "+game.Failure);Shot($"random-{run}-lost");continue;}
    yield return TryHome(home);
    bool ok=Vector3.Distance(game.Motion.Centre(0),home)<.07f&&!game.Owner.Lost;
    if(!ok){trapped++;Shot($"random-{run}-trapped");}
    Note($"  home? {ok}: {Where()}");
   }
   Note($"runs {runs} lost {lost} trapped {trapped}");
   Assert.AreEqual(0,lost,"Runs that lost COghe out of the box; see "+probeLog);
   Assert.AreEqual(0,trapped,"Runs that left COghe trapped; see "+probeLog);
  }

  // For Mrk: the playtest case after the fix, recorded (Artifacts/Clips/l07-fix): a first tap on the exit takes COghe into
  // the pit, where it stops; a tap on the start bank brings it back up; then the level is solved. COGHE_CLIP_SIZE sets the size.
  [UnityTest,Explicit,Timeout(1800000)] public IEnumerator RecordLevel07Fix()
  {
   string size=Environment.GetEnvironmentVariable("COGHE_CLIP_SIZE");
   if(!string.IsNullOrEmpty(size)){var wh=size.Split('x');clipWidth=int.Parse(wh[0]);clipHeight=int.Parse(wh[1]);}
   bool eyesWere=COgheEyes.Enabled;ClipEyesFromEnvironment();
   try
   {
    // COGHE_L07_BEFORE=1: the old scene (before the fix) only gets the first tap; it falls out of the box.
    bool beforeFix=Environment.GetEnvironmentVariable("COGHE_L07_BEFORE")=="1";
    ClipFolder(beforeFix?"l07-before":"l07-fix");clipTaps=new System.Text.StringBuilder();clipTrack=new System.Text.StringBuilder();var marks=new System.Text.StringBuilder();
    yield return LoadScene("COgheSpatial07");yield return Hold(1);var home=game.Motion.Centre(0);
    marks.AppendLine($"{clipFrame} exit tap");
    yield return RecordTap(game.Owner.Outlet.position);
    if(beforeFix)
    {
     for(int i=0;i<6*120&&!game.Owner.Lost;i++){Tick();if(i%4==3)yield return Shot();}
     marks.AppendLine($"{clipFrame} lost: {game.Failure}");yield return Hold(1.5f);
     File.WriteAllText($"{clipDirectory}/taps.txt",clipTaps.ToString());File.WriteAllText($"{clipDirectory}/marks.txt",marks.ToString());
     Assert.IsTrue(game.Owner.Lost,"The old level loses COghe out of the box");yield break;
    }
    yield return RecordUntil(15,()=>game.Motion.Get(0)==null&&Local(game.Motion.Centre(0)).y<-.33f,"COghe stops down in the pit");yield return Hold(1.2f);
    marks.AppendLine($"{clipFrame} start tap");
    yield return RecordTap(home+Vector3.down*.025f);
    yield return RecordUntil(20,()=>Vector3.Distance(game.Motion.Centre(0),home)<.07f,"back up on the start bank");yield return Hold(.6f);
    marks.AppendLine($"{clipFrame} solve");
    yield return new GravityBox.Venom.ChapterProof.COgheSpatialScenario(game,RecordTap,RecordUntil,RecordOrbit).Solve();
    yield return Hold(clipEyes?5.2f:1.5f);
    File.WriteAllText($"{clipDirectory}/taps.txt",clipTaps.ToString());File.WriteAllText($"{clipDirectory}/marks.txt",marks.ToString());
    File.WriteAllText($"{clipDirectory}/track.txt",clipTrack.ToString());clipTrack=null;
    Assert.IsTrue(game.Owner.Completed,"Solved after the detour into the pit");
   }
   finally{COgheEyes.Enabled=eyesWere;COgheEyes.Trace=clipEyes=false;}
  }

  // Plan A (Mrk, 09/10/2026): glass is never climbable. A tap on the glass is marked slippery where it landed, and COghe
  // still goes to the foot of it; a tap on a cream climb board is a plain walk.
  [UnityTest] public IEnumerator SlickGlassTapIsMarked()
  {
   yield return Load(5);yield return Wait(1);
   var board=System.Array.Find(game.Surfaces,f=>f!=null&&f.name=="Climb board A");
   Assert.IsNotNull(board);Assert.IsFalse(board.Slippery,"The cream board grips");
   yield return Tap(board.transform.position);
   Assert.IsFalse(game.Feedback.SlickVisible,"A tap on a cream board is a plain walk");
   var pane=System.Array.Find(game.Surfaces,f=>f!=null&&f.name=="Outer pane 2");   // the back glass, facing the view
   Assert.IsTrue(System.Array.TrueForAll(game.Surfaces,f=>f==null||!f.ExteriorGlass||f.Slippery),"Every pane is slick glass");
   var point=pane.Closest(game.Root.TransformPoint(new Vector3(-.10f,.20f,.30f)));   // above the climb boards
   Assert.IsFalse(pane.Grip(point));
   yield return Tap(point);
   Assert.IsTrue(game.Feedback.SlickVisible,"The slick tap is marked");
   Assert.IsNotNull(game.Motion.Get(0),"COghe still goes toward it");
   yield return null;
   var label=System.Array.Find(game.Feedback.GetComponentsInChildren<TextMesh>(true),t=>t.name=="Slippery label");
   Assert.IsNotNull(label);Assert.IsTrue(label.gameObject.activeInHierarchy,"\"Slippery\" shows beside the mark");
   float tall=label.GetComponent<MeshRenderer>().bounds.size.y;Debug.Log($"Slippery label height {tall*100:F2} cm");
   Assert.That(tall,Is.InRange(.02f,.05f),"Readable on a phone, not a sign");
  }
  // Level 2: the route COghe takes from the start to the top of the first step (Artifacts/L07/route02.txt, route02-*.png).
  [UnityTest,Explicit,Timeout(600000)] public IEnumerator Level02Route()
  {
   Directory.CreateDirectory("Artifacts/L07");probeLog="Artifacts/L07/route02.txt";File.WriteAllText(probeLog,"");
   yield return Load(2);yield return Wait(1);Note($"start {Where()}");
   yield return Tap(game.Root.TransformPoint(new Vector3(-.12f,-.22f,.10f)));
   for(int s=1;s<=24;s++){yield return Wait(.5f);Note($"t={s*.5f:F1} {Where()}");if(s%6==0)Shot($"route02-{s:00}");}
  }
  // Start frames of chapter 1 for a look (Artifacts/L07/start-NN.png).
  [UnityTest,Explicit,Timeout(600000)] public IEnumerator CaptureChapterOneStarts()
  {
   for(int n=1;n<=10;n++){yield return Load(n);yield return Wait(.5f);Shot($"start-{game.Definition.Order:00}");}
  }

  // Level 28 (content 21): can COghe's own weight on the load tray level the plank (with no crate)?
  [UnityTest,Explicit,Timeout(600000)] public IEnumerator Level21TrayWeight()
  {
   Directory.CreateDirectory("Artifacts/L07");probeLog="Artifacts/L07/tray21.txt";File.WriteAllText(probeLog,"");
   yield return Load(21);yield return Wait(1);
   var seesaw=game.Owner.Apparatus.GetComponentInChildren<COgheSeesawBridge>();var tray=seesaw.Tray;
   float bodyMass=0;for(int i=0;i<32;i++)bodyMass+=game.Matter.Bodies[i].mass;
   foreach(var pr in game.Props)Note($"prop {pr.name} mass={pr.Body.mass:F3}");
   Note($"COghe mass={bodyMass:F3} tray mass={tray.Body.mass:F3} stiffness={seesaw.Stiffness} maxTension={seesaw.MaximumTension} slack={seesaw.Slack} spring={seesaw.Hinge.useSpring}/{seesaw.Hinge.spring.spring}");
   Note($"start: tray {tray.Position:F3} caught={seesaw.Caught} angle={seesaw.AngleToLevel:F1} {Where()}");
   // the crate out of the way (down on the floor at the front): COghe alone
   var crate=seesaw.RequiredLoad!=null?seesaw.RequiredLoad:System.Array.Find(game.Props,pr=>pr.name=="A crate").Body;
   crate.position=game.Root.TransformPoint(new Vector3(.05f,-.27f,-.22f));crate.linearVelocity=Vector3.zero;yield return Wait(1);
   yield return Tap(tray.Body.position+game.Root.up*.015f);
   for(int s=1;s<=12;s++){yield return Wait(1);Note($"t={s}s tray {tray.Position:F3} locked={tray.Locked} latched={tray.Latched} load={(seesaw.RequiredLoad!=null?seesaw.RequiredLoad.name:"none")} caught={seesaw.Caught} angle={seesaw.AngleToLevel:F1} tension={seesaw.Tension:F3} {Where()}");}
   ShotClose("tray21-on");
  }

  // The same random taps on every play position (COGHE_PROBE_LEVELS, default 1–60; COGHE_PROBE_RUNS per level, default 3).
  // Two failures: COghe leaves the box, or it ends somewhere no tap moves it (8 taps on walkable faces, none moves it 3 cm).
  [UnityTest,Explicit,Timeout(14400000)] public IEnumerator RandomTapsSweep()
  {
   Directory.CreateDirectory("Artifacts/L07");probeLog="Artifacts/L07/sweep.txt";File.WriteAllText(probeLog,"");
   string only=Environment.GetEnvironmentVariable("COGHE_PROBE_LEVELS");
   int runs=int.TryParse(Environment.GetEnvironmentVariable("COGHE_PROBE_RUNS"),out int r)?r:3;
   yield return Load(1);var order=game.Definition.SceneSequence;
   var levels=new List<int>();if(string.IsNullOrEmpty(only))for(int i=1;i<=order.Length;i++)levels.Add(i);else foreach(var p in only.Split(','))levels.Add(int.Parse(p));
   var failures=new List<string>();
   foreach(int n in levels)
   {
    int outCount=0,stuck=0,otherLost=0;
    for(int run=0;run<runs;run++)
    {
     yield return LoadScene(order[n-1]);yield return Wait(1);var rng=new System.Random(n*100+run);
     bool rec=Environment.GetEnvironmentVariable("COGHE_PROBE_RECORD")=="1";if(rec){ClipFolder($"probe-{n:00}-{run}");clipTrack=null;}
     game.CameraRig.Frame(720,1280,0,true);var cam=game.Owner.View;
     float x0=1e9f,x1=-1e9f,y0=1e9f,y1=-1e9f;
     foreach(var f in game.Surfaces)if(f!=null&&f.isActiveAndEnabled&&f.ExteriorGlass)
      foreach(float a in new[]{-.5f,.5f})foreach(float b in new[]{-.5f,.5f})
      {var sp=cam.WorldToScreenPoint(f.transform.TransformPoint(new Vector3(f.Size.x*a,f.Size.y*b,0)));x0=Mathf.Min(x0,sp.x);x1=Mathf.Max(x1,sp.x);y0=Mathf.Min(y0,sp.y);y1=Mathf.Max(y1,sp.y);}
     for(int k=0;k<20&&!game.Owner.Lost&&!game.Owner.Completed;k++)
     {
      var screen=k%4==1?cam.WorldToScreenPoint(game.Owner.Outlet.position):new Vector3((float)(x0+rng.NextDouble()*(x1-x0)),(float)(y0+rng.NextDouble()*(y1-y0)),0);
      game.CameraRig.Frame(720,1280,0,true);game.TouchPoint(screen);yield return null;
      if(game.Attached)game.ReleaseProp();
      float w=(float)(.5+rng.NextDouble()*3.5);
      if(rec)for(int i=0;i<w/Dt;i++){Tick();if(i%4==3)yield return Shot();}else yield return Wait(w);
     }
     if(game.Owner.Completed){Note($"{n:00} run {run}: solved by chance");continue;}
     if(game.Owner.Lost)
     {
      bool outside=game.Failure!=null&&game.Failure.Contains("ra ngoài vỏ hộp");
      if(outside){outCount++;Shot($"sweep-{n:00}-{run}-out");}else otherLost++;
      Note($"{n:00} run {run}: LOST {(outside?"OUT OF THE BOX":"")} {game.Failure} {Where()}");continue;
     }
     // Can a tap still move it? Walkable faces (fixed, grippy, facing up), nearest first.
     var targets=new List<Vector3>();
     // the middle and corners (3.5 cm in) of each face, away from an exit hole cut in it (crate levels: one big floor)
     foreach(var f in game.Surfaces)if(f!=null&&f.isActiveAndEnabled&&!f.Slippery&&f.MotionFrame==null&&f.Normal.y>.9f)
     {
      float hx=Mathf.Max(0,f.Size.x*.5f-.035f),hy=Mathf.Max(0,f.Size.y*.5f-.035f);
      foreach(var o in new[]{Vector2.zero,new Vector2(-hx,-hy),new Vector2(hx,-hy),new Vector2(-hx,hy),new Vector2(hx,hy)})
       if(!f.Hole||Vector2.Distance(o,f.HoleCentre)>f.HoleRadius+.06f)targets.Add(f.transform.TransformPoint(new Vector3(o.x,o.y,0)));
     }
     targets.Sort((a,b)=>Vector3.Distance(a,game.Motion.Centre(0)).CompareTo(Vector3.Distance(b,game.Motion.Centre(0))));
     bool moved=false;var before=new Vector3[32];var tried=new System.Text.StringBuilder();
     for(int t=0,used=0;t<targets.Count&&used<10&&!moved;t++)
     {
      for(int i=0;i<32;i++)before[i]=game.Matter.Bodies[i].position;
      if(Vector3.Distance(targets[t],game.Motion.Centre(0))<.06f)continue;
      used++;yield return Tap(targets[t]);var o=game.Motion.Get(0);int pathLength=o!=null?o.Path.Count:-1;
      yield return Wait(8);
      for(int i=0;i<32&&!moved;i++)moved=Vector3.Distance(before[i],game.Matter.Bodies[i].position)>.03f;
      tried.Append($" [{Local(targets[t]):F2} path={pathLength} act={game.Activity}]");
     }
     // Inside the tubes a player taps a branch, not a floor: try the middle of every tube.
     foreach(var net in game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>())
      foreach(var e in net.Edges)
      {
       if(moved||e.ControlPoints==null||e.ControlPoints.Length==0)continue;
       for(int i=0;i<32;i++)before[i]=game.Matter.Bodies[i].position;
       yield return Tap(game.Root.TransformPoint(e.ControlPoints[e.ControlPoints.Length/2]));yield return Wait(8);
       for(int i=0;i<32&&!moved;i++)moved=Vector3.Distance(before[i],game.Matter.Bodies[i].position)>.03f;
       tried.Append($" [tube {e.Name} open={e.Open} act={game.Activity}]");
      }
     // Crate levels: crates can wall COghe into a cell; a player taps a crate (the middle of its top takes the nearest face
     // that works) and COghe pushes it away. Stuck only if no crate tap moves it either.
     foreach(var crate in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
     {
      if(moved||!crate.CrateFaces)continue;
      for(int i=0;i<32;i++)before[i]=game.Matter.Bodies[i].position;
      yield return Tap(crate.Rail.Body.position+game.Root.up*crate.CrateSize.y*.5f);yield return Wait(12);
      for(int i=0;i<32&&!moved;i++)moved=Vector3.Distance(before[i],game.Matter.Bodies[i].position)>.03f;
      tried.Append($" [crate {crate.name} {crate.Phase}/{crate.LastFailure}]");
     }
     if(!moved)
     {
      stuck++;Shot($"sweep-{n:00}-{run}-stuck");ShotClose($"sweep-{n:00}-{run}-close");
      string tasks="";foreach(var tr in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())tasks+=$" {tr.Label}:{tr.Phase}/{tr.LastFailure}";
      Note($"   stuck detail: parts={game.Matter.TotalFragmentCount} attached={game.Attached} tasks={tasks} tried={tried}");
      // what it is wedged against: faces within 8 cm of the body, and where each body point touches
      var c0=game.Motion.Centre(0);var near=new System.Text.StringBuilder();
      foreach(var f in game.Surfaces)if(f!=null&&f.isActiveAndEnabled){float d=Vector3.Distance(f.Closest(c0),c0);if(d<.08f)near.Append($" [{f.name} d={d:F3} slick={f.Slippery} moving={f.MotionFrame!=null} n={game.Root.InverseTransformDirection(f.Normal):F1}]");}
      foreach(var col in Physics.OverlapSphere(c0,.08f))if(col.attachedRigidbody==null||System.Array.IndexOf(game.Matter.Bodies,col.attachedRigidbody)<0)
       near.Append($" (collider {col.name} at {Local(col.bounds.center):F3} size {col.bounds.size:F3}{(col.attachedRigidbody!=null?" body "+col.attachedRigidbody.name:"")})");
      Note($"   near:{near}");
      foreach(var net in game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>())Note($"   tube: {net.DebugState(game.Motion.Selected)}");
      // COGHE_PROBE_STUCKCLIP=1: close-up frames while a tap on the floor 12 cm away tries to move it (after detection only)
      if(Environment.GetEnvironmentVariable("COGHE_PROBE_STUCKCLIP")=="1")
      {
       var away=game.Root.TransformPoint(Local(game.Motion.Centre(0))+new Vector3(.08f,0,.09f));away=game.Root.TransformPoint(new Vector3(Local(away).x,-.30f,Local(away).z));
       yield return Tap(away);var o=game.Motion.Get(0);Note($"   clip tap {Local(away):F3} path={(o!=null?string.Join(" ",o.Path.ConvertAll(q=>q.ToString("F2"))):"none")}");
       for(int f=0;f<90;f++){for(int k=0;k<4;k++)Tick();if(f%3==0)ShotClose($"stuckclip-{n:00}-{run}-{f:000}");}
       Note($"   after clip: {Where()}");
       var oo=game.Motion.Get(0);int grips=0;var gripNames=new System.Text.StringBuilder();
       for(int i=0;i<32;i++)if(game.Motion.HasGrip(i))grips++;
       Note($"   order: {(oo==null?"none":$"cursor {oo.Cursor}/{oo.Path.Count} awaiting={oo.AwaitingContact} holding={oo.Holding} exit={oo.Exit}")} grips={grips}");
      }
     }
     Note($"{n:00} run {run}: {(moved?"moves":"STUCK")} {Where()}");
    }
    string line=$"{n:00} {game.Definition.Title}: out {outCount}, stuck {stuck}, other lost {otherLost} / {runs}";Note(line);Debug.Log("SWEEP "+line);
    if(outCount>0||stuck>0)failures.Add(line);
   }
   Note("FAILURES:\n"+string.Join("\n",failures));
   Assert.IsEmpty(failures,"Levels where random taps lose COghe out of the box or leave it stuck; see "+probeLog);
  }
 }
}
