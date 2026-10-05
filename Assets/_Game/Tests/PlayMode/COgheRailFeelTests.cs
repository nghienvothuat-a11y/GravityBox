using System.Collections;
using System.Collections.Generic;
using System.Text;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    /// <summary>
    /// Handles pull smoothly (Mrk, 05/10/2026: "khi kéo các cơ quan ... vẫn bị rung và giật"). Every tap rail that is free
    /// when a Spatial level opens is operated by the whole body and its rail is traced each physics tick. A hunting servo
    /// shows as hand effort that keeps reversing and as a rail that steps back against its own target; a smooth pull
    /// has neither. Rails behind an interlock are covered by the solve tests, not here.
    /// </summary>
    public sealed class COgheRailFeelTests
    {
        private const float Dt=1f/120;
        private VenomCampaign game;
        private SimulationMode simulation;
        private bool persistence;

        public struct Pull
        {
            public string Name;
            public int Ticks, Reversals, Backsteps;
            public float MaxEffort, Travelled;
            public override string ToString()=>$"{Name}: ticks={Ticks} reversals={Reversals} backsteps={Backsteps} maxEffort={MaxEffort:F3}N travelled={Travelled*1000:F1}mm";
        }

        [UnitySetUp] public IEnumerator Before()
        {simulation=Physics.simulationMode;persistence=VenomCampaignSave.PersistenceEnabled;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Physics.simulationMode=simulation;VenomCampaignSave.PersistenceEnabled=persistence;yield return null;}

        private IEnumerator Load(string scene)
        {
            void Loaded(Scene s,LoadSceneMode m){game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;}
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync(scene);}finally{SceneManager.sceneLoaded-=Loaded;}
            for(int i=0;i<60;i++)Tick();
        }
        private void Tick(){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);if(!game.Owner.Paused)Physics.Simulate(Dt);}

        private IEnumerator Measure(COgheTapRail tap,List<Pull> results)
        {
            if(!tap.isActiveAndEnabled||!tap.InterlockOpen||tap.OneWay&&tap.Rail.AtEnd||!tap.Request(game.Motion.Selected))yield break;
            for(int i=0;i<25/Dt&&tap.Phase==COgheTapRail.TaskPhase.Approaching;i++){Tick();if(i%240==0)yield return null;}
            if(tap.Phase!=COgheTapRail.TaskPhase.Operating){tap.CancelTask();yield break;}
            float start=tap.Rail.Position,target=tap.RequestedPosition,sign=Mathf.Sign(target-start);
            var pull=new Pull{Name=$"{game.Definition.Order:00} {tap.Label} · {tap.Rail.name}"};
            float before=start,twoAgo=start,lastEffort=0;
            for(int i=0;i<10/Dt&&tap.Phase==COgheTapRail.TaskPhase.Operating;i++)
            {
                Tick();if(i%240==0)yield return null;
                float position=tap.Rail.Position,effort=tap.Rail.Effort;
                // Judge the travel between the catches, where a smooth pull is a steady glide.
                if(sign*(position-start)>.006f&&sign*(target-position)>.006f)
                {
                    pull.Ticks++;
                    if(sign*(position-twoAgo)<-.0002f)pull.Backsteps++;
                    if(Mathf.Abs(effort)>.05f&&Mathf.Abs(lastEffort)>.05f&&Mathf.Sign(effort)!=Mathf.Sign(lastEffort))pull.Reversals++;
                    pull.MaxEffort=Mathf.Max(pull.MaxEffort,Mathf.Abs(effort));
                }
                twoAgo=before;before=position;lastEffort=effort;
            }
            pull.Travelled=Mathf.Abs(tap.Rail.Position-start);
            if(tap.Busy)tap.CancelTask();
            if(pull.Ticks>0)results.Add(pull);
        }

        private IEnumerator Chapter(int first)
        {
            yield return Load("COgheSpatial01");
            var scenes=game.Definition.SceneSequence;
            var results=new List<Pull>();
            for(int n=first;n<first+10&&n<=scenes.Length;n++)
            {
                yield return Load(scenes[n-1]);
                foreach(var tap in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())yield return Measure(tap,results);
            }
            var report=new StringBuilder($"Rail feel, levels {first}-{first+9}:\n");
            foreach(var pull in results)report.AppendLine(pull.ToString());
            Debug.Log(report);
            foreach(var pull in results)
            {
                Assert.AreEqual(0,pull.Backsteps,"The rail stepped back against its own pull. "+report);
                Assert.LessOrEqual(pull.Reversals,2,"The hand effort kept reversing (a hunting servo). "+report);
            }
        }
        [UnityTest,Timeout(900000)] public IEnumerator HandlesPullSmoothly01to10(){yield return Chapter(1);}
        [UnityTest,Timeout(900000)] public IEnumerator HandlesPullSmoothly11to20(){yield return Chapter(11);}
        [UnityTest,Timeout(900000)] public IEnumerator HandlesPullSmoothly21to30(){yield return Chapter(21);}
        [UnityTest,Timeout(900000)] public IEnumerator HandlesPullSmoothly31to40(){yield return Chapter(31);}
        [UnityTest,Timeout(900000)] public IEnumerator HandlesPullSmoothly41to50(){yield return Chapter(41);}
    }
}
