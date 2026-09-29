#if UNITY_EDITOR
using System.Collections;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 static class ScenarioStepper
 {
  // Runs a nested coroutine (flattening inner enumerators) until the stop condition holds.
  public static IEnumerator MoveNextUntil(this IEnumerator e,System.Func<bool> stop)
  {
   var stack=new System.Collections.Generic.Stack<IEnumerator>();stack.Push(e);
   while(stack.Count>0&&!stop()){var top=stack.Peek();if(!top.MoveNext()){stack.Pop();continue;}if(top.Current is IEnumerator inner)stack.Push(inner);else yield return top.Current;}
  }
 }
 // Engineering trace only (not solution evidence).
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator Spatial14StepTrace()
  {
   yield return Load(14);var s=new COgheSpatialNextScenario(game,Tap,Until);var tube=s.Find<COgheTubeNetwork>();string log="";
   string State()=>$" centre={game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected)):F3} inside={tube.IsParticleInside(game.Motion.Selected)} t={game.Matter.SimulationTime:F2} sel={game.Motion.Selected}";
   yield return s.Operate("A");log+="\n after operate"+State();
   yield return s.Until(8,()=>tube.IsEntryOpen(0),"open");log+="\n after open"+State();
   yield return s.EnterTube(tube,0);log+="\n after enter"+State();
   yield return s.LeaveTube(tube);log+="\n after leave"+State();
   Debug.Log("SPATIAL14 STEPS"+log);
  }
  [UnityTest] public IEnumerator Spatial13PushTrace()
  {
   yield return Load(13);var s=new COgheSpatialNextScenario(game,Tap,Until);var lift=s.Find<COghePassengerLift>();var crate=System.Array.Find(game.Props,p=>p.name=="A crate");
   yield return s.Push(crate,game.Root.TransformPoint(new Vector3(.07f,-.2545f,.16f)));
   yield return Tap(lift.Panel.position);yield return Until(45,()=>lift.Trips==1&&lift.Rail.AtEnd,"ride");
   Capture(13,"top");
   string probe="";
   for(int i=0;i<60;i++){crate.Body.AddForce(game.Root.right*.2f);Tick();if(i%10==0)probe+=$" {game.Root.InverseTransformPoint(crate.Body.position).x:F4}/{crate.Body.linearVelocity.x:F3}";}
   var contacts=new System.Collections.Generic.List<string>();foreach(var col in crate.CollisionShapes)foreach(var hit in Physics.OverlapBox(col.bounds.center,col.bounds.extents+Vector3.one*.001f))if(!hit.transform.IsChildOf(crate.transform))contacts.Add(col.name+"->"+hit.name+"@"+hit.attachedRigidbody?.name);
   Debug.Log("SPATIAL13 FORCE PROBE"+probe+" overlaps="+string.Join(",",contacts));
   yield return Tap(crate.Body.position+Vector3.up*.012f);
   string log="";
   for(int k=0;k<30;k++){if(k==2||k==10||k==18)yield return Tap(game.Root.TransformPoint(new Vector3(.214f,-.0565f,.16f)));for(int i=0;i<60;i++)Tick();yield return null;
    var f=typeof(VenomCampaign).GetField("heldProp",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);var t=typeof(VenomCampaign).GetField("propTarget",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
    var held=f.GetValue(game) as VenomMovableProp;
    log+=$"\n t={k*.5f:F1} crate={game.Root.InverseTransformPoint(crate.Body.position):F3} v={crate.Body.linearVelocity:F3} sleep={crate.Body.IsSleeping()} kin={crate.Body.isKinematic} body={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} held={(held!=null?held.name:"none")} target={game.Root.InverseTransformPoint((Vector3)t.GetValue(game)):F3} contact={game.Root.InverseTransformPoint(game.PropContact):F3} act={game.Activity}";}
   Capture(13,"pushed");
   Debug.Log("SPATIAL13 PUSH"+log);
  }
  [UnityTest] public IEnumerator Spatial15ExitTrace()
  {
   yield return Load(15);var s=new COgheSpatialNextScenario(game,Tap,Until);var tube=s.Find<COgheTubeNetwork>();
   yield return s.EnterTube(tube,0);yield return s.Choose(tube,1,1);yield return s.LeaveTube(tube);
   yield return s.Operate("A");yield return s.Until(8,()=>tube.Edges[2].Open,"open");
   yield return s.EnterTube(tube,2);yield return s.Choose(tube,1,2);yield return s.LeaveTube(tube);
   Capture(15,"landed");string log="";
   string Supports(){var names=new System.Collections.Generic.Dictionary<string,int>();for(int i=0;i<32;i++){string n=game.Motion.Support(i,out var col,out _,out _)?col.name:"none";var hits=Physics.OverlapSphere(game.Matter.Bodies[i].position,.0105f);foreach(var h in hits)if(h.GetComponent<VenomContact>()==null)n+="|"+h.name;names[n]=names.TryGetValue(n,out int k)?k+1:1;}string o="";foreach(var kv in names)o+=kv.Key+"x"+kv.Value+" ";return o;}
   log+="\n landed centre="+game.Root.InverseTransformPoint(game.Motion.Centre(0)).ToString("F3")+" "+Supports();
   yield return Tap(game.Owner.Outlet.position);
   {var o=game.Motion.Get(0);string path="";if(o!=null)foreach(var q in o.Path)path+=q.ToString("F3");log+="\n path="+path+" target="+(o!=null?o.Target.ToString("F3"):"none")+" outlet="+game.Root.InverseTransformPoint(game.Owner.Outlet.position).ToString("F3");
    var door=System.Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name.Contains("door"));log+=" door="+game.Root.InverseTransformPoint(door.Body.position).ToString("F3");}
   for(int k=0;k<16;k++){for(int i=0;i<60;i++)Tick();yield return null;log+=$"\n t={k*.5f:F1} centre={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} act={game.Activity} order={(game.Motion.Get(0)!=null)} "+Supports();}
   Capture(15,"exiting");Debug.Log("SPATIAL15 EXIT"+log);
  }
  [UnityTest] public IEnumerator Spatial16QuantumTrace()
  {
   yield return Load(16);var q=Object.FindFirstObjectByType<COgheQuantumSplitter>();string log="";
   yield return Tap(q.transform.position+q.transform.rotation*new Vector3(0,.1f,-.02f));
   for(int k=0;k<60;k++)
   {
    for(int i=0;i<30;i++)Tick();yield return null;
    float minX=9,maxX=-9,minY=9,maxY=-9,minZ=9,maxZ=-9;int inside=0;
    for(int i=0;i<32;i++){var p=q.transform.InverseTransformPoint(game.Matter.Bodies[i].position);minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);minZ=Mathf.Min(minZ,p.z);maxZ=Mathf.Max(maxZ,p.z);if(q.Inside(i))inside++;}
    var xs=new float[32];for(int i=0;i<32;i++)xs[i]=q.transform.InverseTransformPoint(game.Matter.Bodies[i].position).x;System.Array.Sort(xs);
    log+=$"\n median-gap=[{xs[15]:F3},{xs[16]:F3}] t={k*.25f:F2} phase={q.Phase} act={game.Activity} inside={inside} x[{minX:F3},{maxX:F3}] y[{minY:F3},{maxY:F3}] z[{minZ:F3},{maxZ:F3}] septum={q.Septum.Position:F3} frags={game.Matter.TotalFragmentCount} order={(game.Motion.Get(0)!=null)}";
    if(k==12)Capture(16,"q-3s");if(k==24)Capture(16,"q-6s");
   }
   Debug.Log("SPATIAL16 Q"+log);
  }
  [UnityTest] public IEnumerator Spatial16StepTrace()
  {
   yield return Load(16);var s=new COgheSpatialNextScenario(game,Tap,Until);var q=s.Find<COgheQuantumSplitter>();var latch=s.Find<COgheLoadLatch>();string log="";
   string Groups(){var seen=new System.Collections.Generic.HashSet<int>();string o="";for(int i=0;i<32;i++)if(seen.Add(game.Matter.Groups[i])){int n=0;for(int j=0;j<32;j++)if(game.Matter.Groups[j]==game.Matter.Groups[i])n++;o+=$" g{i}:{n}@{game.Root.InverseTransformPoint(game.Motion.Centre(i)):F3}";}return o+$" sel={game.Motion.Selected} q={q.Phase}/{q.Splits}";}
   yield return s.Split(q,0);log+="\n split"+Groups();Capture(16,"step-split");
   int left=q.LastLeft,right=q.LastRight;
   yield return s.Walk(left,game.Root.TransformPoint(new Vector3(-.27f,-.298f,.02f)),"A");log+="\n padA"+Groups();
   yield return s.Walk(right,game.Root.TransformPoint(new Vector3(.27f,-.298f,.02f)),"B");log+="\n padB"+Groups()+" caught="+latch.Caught;Capture(16,"step-pads");
   yield return s.Until(20,()=>latch.Caught,"caught");log+="\n caught"+Groups();
   yield return s.Merge(game.Root.TransformPoint(new Vector3(.10f,-.30f,.04f)));log+="\n merged"+Groups();Capture(16,"step-merged");
   Debug.Log("SPATIAL16 STEPS"+log);
  }
  [UnityTest] public IEnumerator Spatial18SwingTrace()
  {
   yield return Load(18);var s=new COgheSpatialNextScenario(game,Tap,Until);var swing=s.Find<COgheSwingTransfer>();string log="";
   yield return s.Grip(swing);Capture(18,"gripped");
   log+=$"\n gripped ring={game.Root.InverseTransformPoint(swing.Ring.position):F3} centre={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3}";
   yield return Tap(swing.Docks[0].Closest(swing.DockTargets[0].position));
   for(int k=0;k<40;k++)
   {
    for(int i=0;i<12;i++)Tick();yield return null;
    int sup=0;for(int i=0;i<32;i++)if(game.Motion.Support(i,out var col,out _,out _)&&col==swing.Docks[0].Shape)sup++;
    float minY=9;for(int i=0;i<32;i++)minY=Mathf.Min(minY,game.Root.InverseTransformPoint(game.Matter.Bodies[i].position).y);
    log+=$"\n t={k*.1f:F1} phase={swing.Phase} ring={game.Root.InverseTransformPoint(swing.Ring.position):F3} v={swing.Ring.linearVelocity.magnitude:F2} centre={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} minY={minY:F3} dockSupport={sup} landings={swing.Landings}";
    if(k==6)Capture(18,"mid");
   }
   Debug.Log("SPATIAL18 SWING"+log);
  }
  [UnityTest] public IEnumerator Spatial19ClimbTrace()
  {
   yield return Load(19);string log="";
   yield return Tap(game.Root.TransformPoint(new Vector3(-.30f,-.23f,-.195f)));
   {var o=game.Motion.Get(0);string path="";if(o!=null)foreach(var q in o.Path)path+=game.Root.InverseTransformPoint(game.Root.TransformPoint(q)).ToString("F3");log+="\n order target="+(o!=null?o.Target.ToString("F3"):"none")+" path="+path;}
   for(int k=0;k<30;k++){for(int i=0;i<60;i++)Tick();yield return null;var o=game.Motion.Get(0);log+=$"\n t={k*.5f:F1} centre={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} act={game.Activity} order={(o!=null?o.Cursor+"/"+o.Path.Count+(o.AwaitingContact?" await":""):"none")}";}
   Debug.Log("SPATIAL19 CLIMB"+log);
  }
  [UnityTest] public IEnumerator Spatial20MergeTrace()
  {
   yield return Load(20);var s=new COgheSpatialNextScenario(game,Tap,Until);var q=s.Find<COgheQuantumSplitter>();var tube=s.Find<COgheTubeNetwork>();string log="";
   var gate=System.Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="B return gate");
   yield return s.Split(q,game.Motion.Selected);int left=q.LastLeft,right=q.LastRight;
   yield return s.Walk(right,game.Root.TransformPoint(new Vector3(.13f,-.298f,-.25f)),"pad");
   yield return s.Until(10,()=>tube.IsEntryOpen(0),"cap");
   game.SelectFragment(left);yield return s.EnterTube(tube,0);yield return s.LeaveTube(tube);
   yield return s.Operate("B");yield return s.Until(10,()=>gate.AtEnd,"gate");
   yield return s.Walk(left,game.Root.TransformPoint(new Vector3(.36f,-.29f,-.14f)),"ramp",.07f);
   log+="\n before merge L="+game.Root.InverseTransformPoint(game.Motion.Centre(left)).ToString("F3")+" R="+game.Root.InverseTransformPoint(game.Motion.Centre(right)).ToString("F3");
   yield return s.Merge(game.Root.TransformPoint(new Vector3(.08f,-.30f,-.04f)));
   log+="\n merged at "+game.Root.InverseTransformPoint(game.Motion.Centre(0)).ToString("F3");Capture(20,"merged");
   var c=s.Task("C");yield return Tap(c.HandPoint+Vector3.up*.004f);
   {var o=game.Motion.Get(0);string path="";if(o!=null)foreach(var p in o.Path)path+=game.Root.InverseTransformPoint(game.Root.TransformPoint(p)).ToString("F2");log+="\n C busy="+c.Busy+" last="+c.LastFailure+" path="+path;}
   for(int k=0;k<24;k++){for(int i=0;i<60;i++)Tick();yield return null;log+=$"\n t={k*.5f:F1} centre={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} C={c.Phase} act={game.Activity}";}
   Capture(20,"towardC");Debug.Log("SPATIAL20 MERGE"+log);
  }
  [UnityTest] public IEnumerator Spatial20BTrace()
  {
   yield return Load(20);var s=new COgheSpatialNextScenario(game,Tap,Until);var q=s.Find<COgheQuantumSplitter>();var tube=s.Find<COgheTubeNetwork>();string log="";
   yield return s.Split(q,game.Motion.Selected);int left=q.LastLeft,right=q.LastRight;
   yield return s.Walk(right,game.Root.TransformPoint(new Vector3(.13f,-.298f,-.25f)),"pad");
   yield return s.Until(10,()=>tube.IsEntryOpen(0),"cap");
   game.SelectFragment(left);yield return s.EnterTube(tube,0);yield return s.LeaveTube(tube);
   var b=s.Task("B");log+="\n after tube centre="+game.Root.InverseTransformPoint(game.Motion.Centre(left)).ToString("F3")+" hand="+game.Root.InverseTransformPoint(b.HandPoint).ToString("F3")+" stand="+game.Root.InverseTransformPoint(b.StandPoint).ToString("F3");
   Capture(20,"b-before");
   var ray=game.Owner.View.ScreenPointToRay(game.Owner.View.WorldToScreenPoint(b.HandPoint+Vector3.up*.004f));
   foreach(var h in Physics.RaycastAll(ray,10))log+="\n  hit "+h.collider.name+" d="+h.distance.ToString("F3");
   yield return Tap(b.HandPoint+Vector3.up*.004f);
   var o=game.Motion.Get(left);log+="\n after tap busy="+b.Busy+" last="+b.LastFailure+" order="+(o!=null?game.Root.InverseTransformPoint(game.Root.TransformPoint(o.Target)).ToString("F3"):"none")+" tube="+tube.DebugState(left).Substring(0,60)+" sel="+game.Motion.Selected+" act="+game.Activity;
   Debug.Log("SPATIAL20 B"+log);
  }
  [UnityTest] public IEnumerator Spatial20WinchTrace()
  {
   yield return Load(20);var s=new COgheSpatialNextScenario(game,Tap,Until);string log="";
   var d=s.Task("D");var cable=s.Find<COghePulleyDrive>();var span=cable.Output;
   yield return s.Solve().MoveNextUntil(()=>d.Busy);
   for(int k=0;k<40;k++){for(int i=0;i<30;i++)Tick();yield return null;log+=$"\n t={k*.25f:F2} phase={d.Phase} applied={d.AppliedEffort:F3} railEffort={d.Rail.Effort:F3} in={d.Rail.Position:F4} out={span.Position:F4} tension={cable.Tension:F3} latched={d.Rail.Latched} locked={d.Rail.Locked} centre={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} last={d.LastFailure}";}
   Debug.Log("SPATIAL20 WINCH"+log);
  }
  [UnityTest] public IEnumerator Spatial21CrateProbe()
  {
   yield return Load(21);var crate=System.Array.Find(game.Props,p=>p.name=="A crate");string log="";
   for(int i=0;i<600;i++){crate.Body.AddForce(game.Root.forward*.15f);Tick();if(i%60==0){log+=$" z={game.Root.InverseTransformPoint(crate.Body.position).z:F3}/{crate.Body.linearVelocity.z:F3}";yield return null;}}
   foreach(var col in crate.CollisionShapes)foreach(var hit in Physics.OverlapBox(col.bounds.center,col.bounds.extents+Vector3.one*.001f))if(!hit.transform.IsChildOf(crate.transform))log+="\n "+col.name+" -> "+hit.name+" @"+(hit.attachedRigidbody!=null?hit.attachedRigidbody.name:"static")+" bounds="+game.Root.InverseTransformPoint(hit.bounds.center).ToString("F3")+"/"+hit.bounds.size.ToString("F3");
   Debug.Log("SPATIAL21 CRATE"+log);
  }
  [UnityTest] public IEnumerator Spatial21PushTrace()
  {
   yield return Load(21);var crate=System.Array.Find(game.Props,p=>p.name=="A crate");string log="";
   var f=typeof(VenomCampaign).GetField("propTarget",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
   yield return Tap(crate.Body.position+Vector3.up*.012f);
   for(int k=0;k<24;k++){if(k%4==1)yield return Tap(game.Root.TransformPoint(new Vector3(-.28f,-.187f,.108f)));for(int i=0;i<60;i++)Tick();yield return null;
    log+=$"\n t={k*.5f:F1} crate={game.Root.InverseTransformPoint(crate.Body.position).z:F3} v={crate.Body.linearVelocity.z:F3} held={(game.HeldProp!=null)} target={game.Root.InverseTransformPoint((Vector3)f.GetValue(game)):F3} contact={game.Root.InverseTransformPoint(game.PropContact):F3} body={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F3} act={game.Activity}";}
   Debug.Log("SPATIAL21 PUSH"+log);
  }
  [UnityTest] public IEnumerator Spatial24SplitTrace()
  {
   yield return Load(24);var s=new COgheSpatialNextScenario(game,Tap,Until);var q=s.Find<COgheQuantumSplitter>();string log="";
   yield return s.Split(q,game.Motion.Selected);int l=q.LastLeft,r=q.LastRight;
   yield return s.Walk(r,game.Root.TransformPoint(new Vector3(.20f,-.30f,-.02f)),"r off");
   game.SelectFragment(l);yield return Tap(q.transform.position+q.transform.rotation*new Vector3(0,.1f,-.02f));
   for(int k=0;k<60;k++){for(int i=0;i<30;i++)Tick();yield return null;
    float minX=9,maxX=-9;int n=0;for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[l]){var p=q.transform.InverseTransformPoint(game.Matter.Bodies[i].position);minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);n++;}
    log+=$"\n t={k*.25f:F2} {q.Phase} act={game.Activity} req={q.Requested} armedIn={q.ArmedInside} n={n} x[{minX:F3},{maxX:F3}] septum={q.Septum.Position:F3} gates={q.LeftGate.Position:F3}/{q.RightGate.Position:F3} splits={q.Splits}";}
   Debug.Log("SPATIAL24 SPLIT"+log);
  }
  [UnityTest] public IEnumerator Spatial14TubeTrace()
  {
   yield return Load(14);var s=new COgheSpatialNextScenario(game,Tap,Until);var tube=s.Find<COgheTubeNetwork>();
   yield return s.Operate("A");
   yield return Tap(game.Root.TransformPoint(tube.Nodes[0].LocalPosition));
   string log="";
   for(int k=0;k<40;k++){for(int i=0;i<60;i++)Tick();yield return null;log+=$"\n t={k*.5f:F1} inside={tube.IsParticleInside(game.Motion.Selected)} approach={tube.IsApproachingEntry(game.Motion.Selected,0)} {tube.DebugState(game.Motion.Selected).Substring(0,System.Math.Min(160,tube.DebugState(game.Motion.Selected).Length))} order={(game.Motion.Get(0)!=null?game.Root.InverseTransformPoint(game.Root.TransformPoint(game.Motion.Get(0).Target)).ToString("F3"):"none")}";
    if(k==6)Capture(14,"trace-3s");if(k==14)Capture(14,"trace-7s");}
   Debug.Log("SPATIAL14 TRACE"+log);
  }
 }
}
#endif
