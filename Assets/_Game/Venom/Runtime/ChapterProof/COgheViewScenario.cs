#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using UnityEngine;

namespace GravityBox.Venom.ChapterProof
{
    // Author replay only; gameplay and Boss HUD never read this solution.
    public sealed class COgheViewScenario
    {
        private readonly VenomCampaign game;
        private readonly Func<Vector3,IEnumerator> tap;
        private readonly Func<float,Func<bool>,string,IEnumerator> until;
        private readonly Func<float,IEnumerator> orbit;
        private readonly Func<IEnumerator> pinch;
        public COgheViewScenario(VenomCampaign game,Func<Vector3,IEnumerator> tap,Func<float,Func<bool>,string,IEnumerator> until,Func<float,IEnumerator> orbit=null,Func<IEnumerator> pinch=null)
        {this.game=game;this.tap=tap;this.until=until;this.orbit=orbit;this.pinch=pinch;}
        private IEnumerator Observe(float degrees)
        {if(orbit!=null)yield return orbit(degrees);else{game.CameraRig.Orbit(-degrees*3,720);yield return null;}}
        private COgheTapRail Task(string label)
        {foreach(var task in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())if(task.Label==label)return task;throw new InvalidOperationException("Missing task "+label);}
        private IEnumerator Operate(string label)
        {
            var task=Task(label);int before=task.CompletedJourneys;
            Vector3 point=task.TwoSided&&task.AlternateHandle!=null&&game.Motion.Centre(0).z>.08f?task.AlternateHandle.position:task.HandPoint;
            yield return tap(point+Vector3.up*.004f);
            if(!task.Busy)throw new InvalidOperationException("Handle did not accept tap: "+label+" "+task.LastFailure);
            yield return until(30,()=>task.CompletedJourneys>before,"Operate "+label+"; "+task.LastFailure);
        }
        public IEnumerator Solve()
        {
            int n=game.Definition.Order;
            if(n==2||n==4)
            {
                if(n==4){if(pinch!=null)yield return pinch();else game.CameraRig.Pinch(1.4f);game.CameraRig.Overview();}
                yield return tap(new Vector3(.09f,-.20f,.24f));
                yield return until(22,()=>game.Motion.Centre(0).y>-.19f,"Climb broad step");
            }
            if(n==3)yield return Observe(72);
            if(n==5||n==6)yield return Operate("A");
            if(n==7)
            {
                yield return Operate("A");
                yield return Observe(180);
                yield return tap(new Vector3(-.12f,-.30f,.17f));
                yield return until(24,()=>game.Motion.Centre(0).z>.10f,"Enter reversible room");
                yield return Operate("A");
            }
            if(n==8)yield return Operate("A");
            if(n==9||n==10){yield return Operate("A");yield return until(8,()=>Task("B").InterlockOpen,"Expose B");yield return Operate("B");}
            if(n==8||n==10)
            {
                yield return tap(new Vector3(-.04f,-.30f,.13f));
                yield return until(18,()=>game.Motion.Centre(0).z>.03f&&game.Motion.Centre(0).x>-.13f,"Step onto bridge");
                yield return tap(new Vector3(.22f,-.30f,.13f));
                yield return until(30,()=>game.Motion.Centre(0).x>.17f,"Cross connected bridge");
            }
            if(n==10)yield return Operate("C");
            yield return until(8,()=>game.FinalExitAvailable,"Door physically opens");
            yield return tap(game.Owner.Outlet.position);
            yield return until(40,()=>game.Owner.Completed,"All 32 particles through final exit");
        }
    }
}
#endif
