#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using UnityEngine;
namespace GravityBox.Venom.ChapterProof
{
 public sealed class COgheSpatialScenario
 {
  readonly VenomCampaign game;readonly Func<Vector3,IEnumerator> tap;readonly Func<float,Func<bool>,string,IEnumerator> until;readonly Func<float,IEnumerator> orbit;
  public COgheSpatialScenario(VenomCampaign g,Func<Vector3,IEnumerator> t,Func<float,Func<bool>,string,IEnumerator> u,Func<float,IEnumerator> o=null){game=g;tap=t;until=u;orbit=o;}
  IEnumerator Go(Vector3 point,string reason){yield return tap(point);yield return until(35,()=>Vector3.Distance(game.Motion.Centre(0),point+Vector3.up*.02f)<.055f,reason);}
  IEnumerator Ride(string reason){var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();int trips=lift.Trips;yield return tap(lift.Panel.position);yield return until(45,()=>lift.Trips>trips&&!lift.Moving,reason);}
  // A player waits for a cover or pin to finish moving out of the way before trying the handle it locked.
  IEnumerator Operate(string label){var t=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),x=>x.Label==label);yield return until(10,()=>t.InterlockOpen,"Unlocked "+label);int before=t.CompletedJourneys;yield return tap(t.HandPoint+Vector3.up*.004f);yield return until(35,()=>t.CompletedJourneys>before,"Operate "+label);}
  public IEnumerator Solve()
  {
   string key=COgheSpatialNextScenario.ContentKey(game);
   if(!char.IsDigit(key[0])){yield return new COgheSpatialPlusScenario(game,tap,until,orbit).Solve();yield break;}
   int n=int.Parse(key);
   if(n>=11){yield return new COgheSpatialNextScenario(game,tap,until,orbit).Solve();yield break;}
   if(n==2){yield return Go(new Vector3(-.12f,-.22f,.10f),"First block, climbed from its ivory side");yield return Go(new Vector3(.15f,-.14f,.18f),"Second block");}
   if(n==3){yield return Go(new Vector3(-.23f,-.30f,.23f),"Around observation wall");if(orbit!=null)yield return orbit(-72);else game.CameraRig.Orbit(216,720);yield return Go(new Vector3(.20f,-.30f,.25f),"Behind observation wall");}
   if(n==4)yield return Operate("A");
   if(n==5){yield return Operate("C");yield return Operate("A");}
   if(n==6)
   {
    yield return Operate("C");yield return Operate("A");
    var pulley=game.Owner.Apparatus.GetComponentInChildren<COghePulleyDrive>();yield return until(15,()=>pulley.Output.AtEnd,"Pulley raises physical bridge");
    yield return Go(new Vector3(-.23f,-.10f,.16f),"Climb departure plinth");yield return Go(new Vector3(0,-.10f,.16f),"Onto raised bridge");yield return Go(new Vector3(.22f,-.10f,.16f),"Cross to receiving plinth");
   }
   if(n==7){yield return Operate("D");yield return Operate("B");yield return Go(new Vector3(-.04f,-.30f,.13f),"Onto bridge");yield return Go(new Vector3(.22f,-.30f,.13f),"Across bridge");}
   if(n==8){yield return Ride("Ride up to the ledge");yield return Operate("A");yield return Ride("Press again: ride down");}
   if(n==9)
   {
    yield return Operate("C");yield return Operate("B");
    var bridge=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>(),r=>r.name=="Sliding bridge");yield return until(10,()=>bridge.AtEnd,"The bridge leaves the shaft");
    yield return Ride("Ride up");yield return Go(new Vector3(-.003f,.04f,.15f),"Onto the bridge");yield return Go(new Vector3(-.25f,.04f,.20f),"Onto the exit tower");
   }
   if(n==10)
   {
    yield return Ride("Up to the balcony");yield return Operate("C");
    // D is hidden behind the balcony screen from the first view: look from behind to reach it.
    if(orbit!=null)yield return orbit(-144);else game.CameraRig.Orbit(432,720);
    yield return Operate("D");game.CameraRig.Overview();
    yield return Ride("Back down");yield return Operate("A");
   }
   yield return until(10,()=>game.FinalExitAvailable,"Exit unlocked physically");yield return tap(game.Owner.Outlet.position);yield return until(40,()=>game.Owner.Completed,"All tissue through final exit");
  }
 }
}
#endif
