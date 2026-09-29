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
  IEnumerator Operate(string label){var t=Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),x=>x.Label==label);int before=t.CompletedJourneys;yield return tap(t.HandPoint+Vector3.up*.004f);yield return until(35,()=>t.CompletedJourneys>before,"Operate "+label);}
  public IEnumerator Solve()
  {
   int n=game.Definition.Order;
   if(n>=11){yield return new COgheSpatialNextScenario(game,tap,until,orbit).Solve();yield break;}
   if(n==2){yield return Go(new Vector3(-.12f,-.22f,.10f),"First block");yield return Go(new Vector3(.15f,-.14f,.18f),"Second block");}
   if(n==3){yield return Go(new Vector3(-.23f,-.30f,.23f),"Around observation wall");if(orbit!=null)yield return orbit(-72);else game.CameraRig.Orbit(216,720);yield return Go(new Vector3(.20f,-.30f,.25f),"Behind observation wall");}
   if(n==4||n==5||n==6||n==9||n==10)yield return Operate("A");
   if(n==6||n==9||n==10)
   {
    var pulley=game.Owner.Apparatus.GetComponentInChildren<COghePulleyDrive>();yield return until(15,()=>pulley.Output.AtEnd,"Pulley raises physical bridge");
    yield return Go(new Vector3(-.23f,-.10f,.16f),"Climb departure plinth");yield return Go(new Vector3(0,-.10f,.16f),"Onto raised bridge");if(n!=10)yield return Go(new Vector3(.22f,-.10f,.16f),"Cross to receiving plinth");
   }
   if(n==7){yield return Operate("B");yield return Go(new Vector3(-.04f,-.30f,.13f),"Onto bridge");yield return Go(new Vector3(.22f,-.30f,.13f),"Across bridge");}
   if(n==9||n==10)yield return Operate("B");
   if(n==8||n==10){var lift=game.Owner.Apparatus.GetComponentInChildren<COghePassengerLift>();yield return tap(lift.Panel.position);yield return until(45,()=>lift.Trips>0&&lift.Rail.AtEnd,"Board and ride elevator");}
   yield return until(10,()=>game.FinalExitAvailable,"Exit unlocked physically");yield return tap(game.Owner.Outlet.position);yield return until(40,()=>game.Owner.Completed,"All tissue through final exit");
  }
 }
}
#endif
