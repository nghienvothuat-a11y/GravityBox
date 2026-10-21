using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // Ability cards (Mrk 16/10/2026): one card per "Hiểu ra", each ability 0–10, Agility +10% walking speed a level.
 public sealed partial class COgheSpatialCampaignTests
 {
  [Test] public void OneAbilityCardPerUnderstoodChapterUpToTen()
  {
   try
   {
    COgheShop.ResetForTests(new COgheShop.State());
    Assert.AreEqual(0,COgheAbilities.Pending(),"No card before the first Hiểu ra");
    COgheShop.Understand(COgheBonus.Key(1));
    Assert.AreEqual(1,COgheAbilities.Pending(),"Chapter 1's card waits once its Hiểu ra is done");
    Assert.IsTrue(COgheShop.PickAbility(1,COgheAbility.Agility));
    Assert.IsFalse(COgheShop.PickAbility(1,COgheAbility.Emotion),"Exactly one card per Hiểu ra");
    Assert.AreEqual(1,COgheAbilities.Level(COgheAbility.Agility));Assert.AreEqual(0,COgheAbilities.Level(COgheAbility.Emotion));
    Assert.AreEqual(0,COgheAbilities.Pending());
    Assert.AreEqual(1.1f,COgheAbilities.SpeedScale,.0001f);
    // a full ability takes no more cards; the others still do
    COgheShop.Current.Intelligence=COgheAbilities.Max;
    COgheShop.Understand(COgheBonus.Key(2));
    Assert.IsFalse(COgheShop.PickAbility(2,COgheAbility.Intelligence),"Intelligence is full");
    Assert.IsTrue(COgheShop.PickAbility(2,COgheAbility.Emotion));
    Assert.AreEqual(2,COgheAbilities.MoodsUnlocked);
    // all full: nothing waits
    COgheShop.Current.Emotion=COgheShop.Current.Agility=COgheAbilities.Max;
    COgheShop.Understand(COgheBonus.Key(3));
    Assert.AreEqual(0,COgheAbilities.Pending(),"Nothing waits once every ability is full");
    Assert.AreEqual(2f,COgheAbilities.SpeedScale,.0001f);
   }
   finally{COgheShop.ResetForTests();}
  }

  // Agility: in the same 1.5 s toward the far side of level 1's floor, COghe covers clearly more ground at Lv 10 (200%).
  [UnityTest] public IEnumerator AgilityMakesCOgheWalkFaster()
  {
   float Walk(float scale)
   {
    game.ResetLevel();game.Motion.SpeedScale=scale;
    for(int i=0;i<90;i++)Tick();
    Vector3 start=game.Motion.Centre(0),local=game.Root.InverseTransformPoint(start);
    var goal=game.Root.TransformPoint(new Vector3(local.x>0?-.32f:.32f,-.30f,local.z));
    game.Motion.Move(0,goal);
    for(int i=0;i<1.5f/Dt;i++)Tick();
    return Vector3.Distance(game.Motion.Centre(0),start);
   }
   yield return Load(1);
   float slow=Walk(1),fast=Walk(2);
   Debug.Log($"AGILITY 1.5 s walk: Lv0 {slow:F3} m, Lv10 {fast:F3} m");
   Assert.Greater(slow,.08f,"The base walk moves");
   Assert.Greater(fast,slow*1.5f,"Twice the walking speed covers clearly more ground");
   game.Motion.SpeedScale=1;
  }

  // Hungry or exhausted, COghe stops on the way to catch its breath, stands still meanwhile, and still gets where it was
  // sent; fed, it never stops.
  [UnityTest] public IEnumerator AHungryCOgheStopsForBreathOnTheWayAndStillArrives()
  {
   int Walk(COgheHunger hunger,out float seconds,out float still)
   {
    game.ResetLevel();COgheEnergy.ApplyRests(game.Motion,hunger);
    for(int i=0;i<90;i++)Tick();
    Vector3 start=game.Motion.Centre(0),local=game.Root.InverseTransformPoint(start);
    game.Motion.Move(0,game.Root.TransformPoint(new Vector3(local.x>0?-.32f:.32f,-.30f,local.z)));
    seconds=0;still=0;
    for(int i=0;i<20f/Dt&&game.Motion.Get(0)!=null;i++)
    {
     Tick();seconds+=Dt;
     if(game.Motion.RestingFor(0)>.3f)still=Mathf.Max(still,game.Matter.Bodies[0].linearVelocity.magnitude);
    }
    Assert.IsNull(game.Motion.Get(0),$"{hunger}: arrives");
    return game.Motion.Rests;
   }
   yield return Load(1);
   try
   {
    int fed=Walk(COgheHunger.Fed,out float fedTime,out _);
    int hungry=Walk(COgheHunger.Hungry,out float hungryTime,out float hungryStill);
    int exhausted=Walk(COgheHunger.Exhausted,out float exhaustedTime,out float exhaustedStill);
    Debug.Log($"RESTS fed {fed} in {fedTime:F2}s, hungry {hungry} in {hungryTime:F2}s, exhausted {exhausted} in {exhaustedTime:F2}s; still {hungryStill:F3}/{exhaustedStill:F3} m/s");
    Assert.AreEqual(0,fed,"Fed: no stops");
    Assert.GreaterOrEqual(hungry,1,"Hungry: a stop on a long walk");
    Assert.Greater(exhausted,hungry,"Exhausted: more stops");
    Assert.Greater(exhaustedTime,hungryTime,"and slower");Assert.Greater(hungryTime,fedTime);
    Assert.Less(Mathf.Max(hungryStill,exhaustedStill),.03f,"Standing still at a stop");
   }
   finally{COgheEnergy.ApplyRests(game.Motion,COgheHunger.Fed);}
  }

  // Every level of the catalog solved with COghe at Agility 10 (Explicit, ~2.5 min a speed): COGHE_AGILITY_SCALE (default 2;
  // a comma list runs each speed in turn; a speed ending in h or e adds a hungry or exhausted COghe's stops for breath,
  // e.g. 1e, and a factor after the mark scales the walk between stops, moving where they fall: 1e.85),
  // COGHE_AGILITY_LEVELS (positions, default all). Report: Artifacts/Agility/solve.txt, and solve-x<speed>.txt for each.
  [UnityTest,Explicit,Timeout(10800000)] public IEnumerator SolveEveryLevelAtTopAgility()
  {
   string scales=Environment.GetEnvironmentVariable("COGHE_AGILITY_SCALE");if(string.IsNullOrEmpty(scales))scales="2";
   string only=Environment.GetEnvironmentVariable("COGHE_AGILITY_LEVELS");
   Directory.CreateDirectory("Artifacts/Agility");string log="Artifacts/Agility/solve.txt";File.WriteAllText(log,"");
   yield return Load(1);var order=game.Definition.SceneSequence;
   var levels=new List<int>();if(string.IsNullOrEmpty(only))for(int i=1;i<=order.Length;i++)levels.Add(i);else foreach(var p in only.Split(','))levels.Add(int.Parse(p));
   var failures=new List<string>();
   var invariant=System.Globalization.CultureInfo.InvariantCulture;
   foreach(string text in scales.Split(','))
   {
    string token=text.Trim();int mark=token.IndexOfAny(new[]{'h','e'});
    var hunger=mark<0?COgheHunger.Fed:token[mark]=='e'?COgheHunger.Exhausted:COgheHunger.Hungry;
    float scale=float.Parse(mark<0?token:token.Substring(0,mark),invariant);
    float walkFactor=mark>=0&&mark<token.Length-1?float.Parse(token.Substring(mark+1),invariant):1;
    string speedLog=$"Artifacts/Agility/solve-x{token}.txt";File.WriteAllText(speedLog,$"speed x{token}\n");
    File.AppendAllText(log,$"speed x{token}\n");
    foreach(int n in levels)
    {
     yield return LoadScene(order[n-1]);game.Motion.SpeedScale=scale;COgheEnergy.ApplyRests(game.Motion,hunger);game.Motion.RestWalk*=walkFactor;
     string error="";float start=game.Matter.SimulationTime;
     yield return Flat(new COgheSpatialScenario(game,Tap,Until).Solve(),e=>error=e);
     bool solved=game.Owner.Completed&&string.IsNullOrEmpty(error);
     string line=$"{n:00} {Path.GetFileNameWithoutExtension(order[n-1])}: {(solved?"solved":"FAILED")} in {game.Matter.SimulationTime-start:F1}s rests {game.Motion.Rests} {error}";
     File.AppendAllText(log,line+"\n");File.AppendAllText(speedLog,line+"\n");if(!solved)failures.Add($"x{token} "+line);
    }
   }
   game.Motion.SpeedScale=1;COgheEnergy.ApplyRests(game.Motion,COgheHunger.Fed);
   Assert.IsEmpty(failures,string.Join("\n",failures));
  }

  // The order in which each catalog level's devices are worked in its reference solution, for the Intelligence ability
  // (Explicit, ~25 min): Assets/_Game/Venom/Resources/COgheUI/SolutionSteps.json. A rail counts when a journey completes, a
  // weight pad when it switches on, any other device when it takes a tap it does not refuse; repeats in a row count once.
  [UnityTest,Explicit,Timeout(10800000)] public IEnumerator RecordSolutionSteps()
  {
   const string output="Assets/_Game/Venom/Resources/COgheUI/SolutionSteps.json";
   yield return Load(1);var order=game.Definition.SceneSequence;var report=new System.Text.StringBuilder();
   string only=Environment.GetEnvironmentVariable("COGHE_RECORD_LEVELS");
   var book=string.IsNullOrEmpty(only)?new COgheSolutionSteps.Book():JsonUtility.FromJson<COgheSolutionSteps.Book>(File.ReadAllText(output));
   var selected=string.IsNullOrEmpty(only)?null:new HashSet<int>(only.Split(',').Select(int.Parse));
   var failures=new List<string>();
   for(int n=1;n<=order.Length;n++)
   {
    if(selected!=null&&!selected.Contains(n))continue;
    yield return LoadScene(order[n-1]);
    var root=game.Owner.Apparatus;var level=new COgheSolutionSteps.Level{Id=game.Definition.Id};
    var rails=root.GetComponentsInChildren<COgheTapRail>(true);var journeys=rails.Select(r=>r.CompletedJourneys).ToArray();
    var held=rails.Select(r=>r.Holding).ToArray();
    var pads=root.GetComponentsInChildren<COgheTissueSensor>(true);var active=pads.Select(p=>p.Active).ToArray();
    void Add(Component c,string kind)
    {
     string path=COgheSolutionSteps.PathOf(root,c.transform);
     if(level.Steps.Count>0&&level.Steps[level.Steps.Count-1].Path==path)return;
     level.Steps.Add(new COgheSolutionSteps.Step{Path=path,Kind=kind});
    }
    Action<COgheMechanism> touched=m=>
    {
     if(m is COgheTapRail||m is COgheTissueSensor)return;
     if(m.Refusals>0&&game.Matter.SimulationTime-m.RefusedAt<.05f)return;
     Add(m,COgheSolutionSteps.DeviceKind);
    };
    game.MechanismTouched+=touched;
    trace=()=>
    {
     for(int i=0;i<rails.Length;i++)
     {
      if(rails[i].CompletedJourneys>journeys[i]||(rails[i].Holding&&!held[i]))Add(rails[i],COgheSolutionSteps.RailKind);
      journeys[i]=rails[i].CompletedJourneys;held[i]=rails[i].Holding;
     }
     for(int i=0;i<pads.Length;i++)
     {
      // A ball/prop settling on a pad is the consequence of a player action, not another instruction for COghe.
      bool tissue=game.Matter.Bodies.Any(body=>pads[i].Holds(body.position));
      if(pads[i].Active&&!active[i]&&(pads[i].Weights.Length==0||tissue))Add(pads[i],COgheSolutionSteps.PadKind);
      active[i]=pads[i].Active;
     }
    };
    string error="";
    yield return Flat(new COgheSpatialScenario(game,Tap,Until).Solve(),e=>error=e);
    trace=null;game.MechanismTouched-=touched;
    if(!game.Owner.Completed||!string.IsNullOrEmpty(error)){failures.Add(level.Id+": "+error);continue;}
    book.Levels.RemoveAll(l=>l.Id==level.Id);book.Levels.Add(level);
    report.AppendLine($"{n:00} {level.Id}: {level.Steps.Count} steps {(game.Owner.Completed?"":"UNSOLVED "+error)} | "+string.Join(" > ",level.Steps.Select(st=>st.Kind+":"+st.Path.Split('/').Last())));
   }
   Assert.IsEmpty(failures,"Do not overwrite solution steps after an incomplete solve: "+string.Join("\n",failures));
   File.WriteAllText(output,JsonUtility.ToJson(book,true));
   Directory.CreateDirectory("Artifacts/Steps");File.WriteAllText("Artifacts/Steps/report.txt",report.ToString());
#if UNITY_EDITOR
   UnityEditor.AssetDatabase.ImportAsset(output);
#endif
   COgheSolutionSteps.ResetForTests();
  }

  // Intelligence: after 5 s without input COghe walks to the next device it remembers, waits for the player to work it,
  // and past what it remembers it wonders instead of walking (G2: rail A, then rail B on the high shelf).
  [UnityTest,Timeout(900000)] public IEnumerator IntelligenceLeadsToRememberedStepsThenWonders()
  {
   COgheIntelligence.ForceForTests=true;COgheIntelligence.LevelForTests=1;
   try
   {
    yield return LoadPlus("G2");
    var intel=game.Intelligence;Assert.IsNotNull(intel,"Intelligence runs in the level");
    Assert.GreaterOrEqual(intel.StepCount,2,"G2 has its steps recorded (rail A, rail B)");
    var a=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>().First(r=>r.Label=="A");
    // a pause: it looks around, then walks to A's handle
    for(int i=0;i<4/Dt;i++)Tick();
    Assert.AreEqual(-1,intel.GuidedStep,"Nothing before the 5 s pause");
    for(int i=0;i<25/Dt&&!(intel.GuidedStep==0&&game.Motion.Get(game.Motion.Selected)==null);i++){Tick();if(i%240==0)yield return null;}
    Assert.AreEqual(0,intel.GuidedStep,"It walked to the first step");
    Assert.Less(Vector3.Distance(game.Motion.Centre(game.Motion.Selected),a.StandPoint),.07f,"beside A's handle");
    Assert.AreEqual(0,a.CompletedJourneys,"It never works the device itself");
    // the player works A; it remembers only one step, so after the next pause it wonders instead of walking on
    yield return Tap(a.HandPoint);
    for(int i=0;i<30/Dt&&intel.Progress<1;i++){Tick();if(i%240==0)yield return null;}
    Assert.AreEqual(1,intel.Progress,"A worked: step 1 done");
    for(int i=0;i<8/Dt&&intel.Puzzled==0;i++){Tick();if(i%240==0)yield return null;}
    Assert.AreEqual(1,intel.Puzzled,"Past what it remembers, it wonders");
    Assert.AreEqual(0,intel.GuidedStep,"and does not walk to B");
    // smarter: after the player's next touch and a pause it goes for B
    COgheIntelligence.LevelForTests=2;
    yield return Tap(game.Root.TransformPoint(new Vector3(0,-.30f,-.05f)));
    for(int i=0;i<40/Dt&&intel.GuidedStep<1;i++){Tick();if(i%240==0)yield return null;}
    Assert.AreEqual(1,intel.GuidedStep,"With two steps remembered it heads for B");
   }
   finally{COgheIntelligence.ForceForTests=false;COgheIntelligence.LevelForTests=null;}
  }
 }
}
