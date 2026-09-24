#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using UnityEngine;

namespace GravityBox.Venom.ChapterProof
{
    /// <summary>Author solution through player commands. Shared by tests and opt-in framebuffer replay.</summary>
    public sealed class COgheOnboardingScenario
    {
        private readonly VenomCampaign game;
        private readonly Func<Vector3,IEnumerator> tap;
        private readonly Func<float,Func<bool>,string,IEnumerator> until;
        private readonly Func<float,IEnumerator> delay;
        private readonly Func<IEnumerator> release;
        public COgheOnboardingScenario(VenomCampaign game,Func<Vector3,IEnumerator> tap,
            Func<float,Func<bool>,string,IEnumerator> until,Func<float,IEnumerator> delay,Func<IEnumerator> release)
        {this.game=game;this.tap=tap;this.until=until;this.delay=delay;this.release=release;}
        public IEnumerator Solve()
        {
            var lesson=game.Onboarding;
            switch(lesson.Lesson)
            {
                case COgheOnboarding.LessonKind.Climb:
                    yield return tap(game.Root.TransformPoint(lesson.LocalDestination));
                    yield return until(35,()=>game.Root.InverseTransformPoint(game.Motion.Centre(0)).y>lesson.LocalDestination.y-.03f,"climb the visible pane");break;
                case COgheOnboarding.LessonKind.Tap:
                    yield return Operate(lesson.TapA);
                    if(lesson.TapB!=null){yield return Operate(lesson.TapB);yield return Operate(lesson.TapA);}
                    yield return until(10,()=>game.FinalExitAvailable,"tap gears open the measured shutter");break;
                case COgheOnboarding.LessonKind.Crate:
                    yield return tap(lesson.Primary.Body.position);
                    yield return until(20,()=>game.Attached,"grasp the crate");
                    for(int stroke=0;stroke<6&&!game.Owner.Completed;stroke++)
                    {
                        if(!game.Attached&&game.Motion.Get(0)==null)
                        {yield return tap(lesson.Primary.Body.position);yield return until(15,()=>game.Attached||game.Owner.Completed,"regrasp crate if needed");}
                        if(game.Attached)yield return tap(game.Root.TransformPoint(lesson.LocalDestination));
                        yield return delay(2.4f);
                    }
                    yield return until(35,()=>game.Owner.Completed,"push the real crate, climb and exit");yield break;
                case COgheOnboarding.LessonKind.Rail:
                    yield return Pull(lesson.PrimaryRail,()=>lesson.PrimaryRail.AtEnd);
                    yield return until(12,()=>game.FinalExitAvailable,"open the real exit");break;
                case COgheOnboarding.LessonKind.Bridge:
                    yield return Pull(lesson.PrimaryRail,()=>lesson.PrimaryRail.AtEnd);
                    yield return tap(lesson.Bridge.DockedTop.transform.position);
                    yield return until(28,()=>Vector3.Distance(game.Motion.Centre(0),lesson.Bridge.DockedTop.transform.position+Vector3.up*.020f)<.045f,"walk across the docked deck");break;
                case COgheOnboarding.LessonKind.Sequence:
                    yield return Pull(lesson.PrimaryRail,()=>lesson.Sequence.AccessOpen);
                    yield return Pull(lesson.SecondaryRail,()=>lesson.Sequence.Complete);break;
                case COgheOnboarding.LessonKind.Boss:
                    var sequence=game.GetComponentInChildren<COgheSequentialWinch>();
                    var gear=Array.Find(game.Props,p=>p.name=="G factory carriage").GetComponent<COgheRailSlider>();
                    yield return Pull(gear,()=>sequence.AccessOpen);
                    yield return Pull(sequence.Handle,()=>sequence.Complete);break;
            }
            yield return tap(game.Owner.Outlet.position);
            yield return until(40,()=>game.Owner.Completed,"all 32 particles leave through the aperture");
        }
        private IEnumerator Operate(COgheTapRail task)
        {
            int before=task.CompletedJourneys;
            yield return tap(task.HandPoint);
            yield return until(30,()=>task.CompletedJourneys==before+1&&!task.Busy,"one tap operates "+task.Label);
        }
        private IEnumerator Pull(COgheRailSlider rail,Func<bool> done)
        {
            var prop=rail.GetComponent<VenomMovableProp>();
            var stem=prop.ManipulationGrip!=null?prop.ManipulationGrip.Find("Bridge handle stem"):null;
            yield return tap(stem!=null?stem.position:prop.ManipulationGrip!=null?prop.ManipulationGrip.position:prop.Body.position);
            yield return until(30,()=>game.Attached,"grasp "+rail.name);
            var target=rail.Frame.TransformPoint(rail.Start+rail.Axis*(rail.Travel+.10f));
            for(int stroke=0;stroke<15&&!done()&&!game.Owner.Lost;stroke++)
            {
                if(!game.Attached)
                {
                    yield return tap(prop.ManipulationGrip!=null?prop.ManipulationGrip.position:prop.Body.position);
                    yield return until(20,()=>game.Attached,"recover grip "+rail.name);
                }
                yield return tap(target);yield return delay(1.5f);
            }
            yield return until(5,done,"complete real rail journey "+rail.name);
            if(game.Attached)yield return release();
        }
    }
}
#endif
