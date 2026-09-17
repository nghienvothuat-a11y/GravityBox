#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    public sealed partial class COgheMacPlaytest
    {
        private Vector3 World(float x,float y,float z)=>game.Root.TransformPoint(new Vector3(x,y,z));
        private List<int> Parts()
        {
            var result=new List<int>();var seen=new HashSet<int>();
            for(int i=0;i<32;i++)if(!game.Matter.Escaped[i]&&seen.Add(game.Matter.Groups[i]))result.Add(i);return result;
        }
        private int Count(int anchor)
        {int count=0;for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[anchor])count++;return count;}
        private void Stage(string name,int zone)
        {
            phase=name;game.CameraRig.SelectZone(zone);
            Debug.Log("COGHE_DEMO_STAGE "+name+" time="+game.Matter.SimulationTime);
        }
        private IEnumerator Wait(float seconds,Func<bool> condition,Action refresh=null)
        {
            float end=game.Matter.SimulationTime+seconds,next=game.Matter.SimulationTime;
            while(!condition()&&!game.Owner.Lost&&game.Matter.SimulationTime<end)
            {if(game.Matter.SimulationTime>=next){refresh?.Invoke();next+=1;}yield return null;}
            if(!condition())
            {
                string details=phase+" timeout; ";foreach(int p in Parts())details+=$"{p}:{Count(p)} at {game.Root.InverseTransformPoint(game.Motion.Centre(p))} ";
                Debug.LogError("COGHE_DEMO_FAILED "+details);Finish();throw new InvalidOperationException(details);
            }
        }
        private IEnumerator Walk(int anchor,Vector3 point,float tolerance=.035f,float seconds=30)
        {
            game.SelectFragment(anchor);game.Motion.Move(anchor,point,true);
            yield return Wait(seconds,()=>Vector3.Distance(game.Motion.Centre(anchor),point)<tolerance);
        }
        private IEnumerator Pad(int anchor,COgheTissueSensor pad)
        {yield return Walk(anchor,pad.transform.position+pad.transform.up*.018f);yield return Wait(10,()=>pad.Active);}
        private IEnumerator Pull(int anchor,COgheRailSlider rail,Func<bool> finished)
        {
            game.SelectFragment(anchor);game.SelectProp(rail.GetComponent<VenomMovableProp>());
            yield return Wait(30,()=>game.Attached);
            Vector3 point=rail.Frame.TransformPoint(rail.Start+rail.Axis*(rail.Travel+.12f));game.SetPropTarget(point);
            yield return Wait(35,finished,()=>game.SetPropTarget(point));game.ReleaseProp();
        }
        private IEnumerator Pipe(int anchor,COgheTubeNetwork pipe)
        {
            Vector3 entry=game.Root.TransformPoint(pipe.Nodes[0].LocalPosition);
            yield return Walk(anchor,entry+game.Root.TransformDirection(pipe.Nodes[0].LocalOutward)*.025f,.025f,35);
            if(!pipe.TryChoose(anchor,0))throw new InvalidOperationException("Reached pipe mouth rejected entry");
            yield return Wait(20,()=>pipe.LastReachedNode==1&&!pipe.IsParticleInside(anchor));
            Vector3 landing=pipe.Nodes[1].LocalPosition+pipe.Nodes[1].LocalOutward*.064f;landing.y=-.274f;
            yield return Walk(anchor,game.Root.TransformPoint(landing),.032f);
        }
        private IEnumerator Merge(Vector3 point)
        {foreach(int anchor in Parts())game.Motion.Move(anchor,point,true);yield return Wait(45,()=>game.Matter.TotalFragmentCount==1);}
        private IEnumerator DemonstrateBoss()
        {
            var winch=game.Owner.Apparatus.GetComponentInChildren<COgheCooperativeWinch>();
            var knives=game.Owner.Apparatus.GetComponentsInChildren<COgheGuillotine>();Array.Sort(knives,(a,b)=>a.Sensor.position.x.CompareTo(b.Sensor.position.x));
            var pipes=game.Owner.Apparatus.GetComponentsInChildren<COgheTubeNetwork>();Array.Sort(pipes,(a,b)=>a.Nodes[0].LocalPosition.x.CompareTo(b.Nodes[0].LocalPosition.x));
            Stage("1 · Chia phần giữ A",0);bool usable=false;
            foreach(float offset in new[]{.009f,-.009f,.016f,-.016f,0f})
            {
                if(game.Matter.TotalFragmentCount>1)yield return Merge(World(-.49f,-.274f,.04f));
                int anchor=Parts()[0];yield return Walk(anchor,World(-.50f,-.274f,.04f));
                yield return Wait(8,()=>knives[0].Phase==VenomCampaign.BladePhase.Ready);
                int before=game.Matter.CutCount;game.Motion.Move(anchor,knives[0].Sensor.position+game.Root.right*offset,true);
                yield return Wait(20,()=>game.Matter.CutCount>before);yield return new WaitForSeconds(.5f);
                var split=Parts();if(split.Count!=2)continue;
                split.Sort((a,b)=>Count(a).CompareTo(Count(b)));
                if(Count(split[0])>=7&&Count(split[1])>=19){usable=true;break;}
            }
            if(!usable)throw new InvalidOperationException("First cut needs a different physical position");
            var parts=Parts();parts.Sort((a,b)=>Count(a).CompareTo(Count(b)));int aHolder=parts[0],worker=parts[1];
            Stage("2 · Phần nhỏ giữ A",0);yield return Pad(aHolder,winch.Input);
            Stage("3 · Phần lớn qua ống",0);yield return Pipe(worker,pipes[0]);
            Stage("4 · Nâng bánh răng G",1);yield return Walk(worker,World(0,winch.GearCarriage.Start.y,.015f));
            yield return Pull(worker,winch.GearCarriage,()=>winch.GearCarriage.AtEnd);
            var ids=new HashSet<int>();for(int i=0;i<32;i++)if(game.Matter.Groups[i]==game.Matter.Groups[worker])ids.Add(i);
            Stage("5 · Chia B và C",1);int cuts=game.Matter.CutCount;game.SelectFragment(worker);game.Motion.Move(worker,knives[1].Sensor.position,true);
            yield return Wait(25,()=>game.Matter.CutCount>cuts&&game.Matter.TotalFragmentCount>=3);yield return new WaitForSeconds(.5f);
            var candidates=new List<int>();foreach(int p in Parts())if(ids.Contains(p))candidates.Add(p);
            candidates.Sort((a,b)=>game.Root.InverseTransformPoint(game.Motion.Centre(a)).x.CompareTo(game.Root.InverseTransformPoint(game.Motion.Centre(b)).x));
            if(candidates.Count!=2)throw new InvalidOperationException("Second cut must leave B and C separate");
            int bHolder=candidates[0],cOperator=candidates[1];
            Stage("6 · Phần phải sang khoang 3 trước",1);yield return Walk(cOperator,World(.13f,-.274f,-.24f),.025f);yield return Pipe(cOperator,pipes[1]);
            Stage("7 · Phần trái vòng sau bánh răng tới B",1);yield return Walk(bHolder,World(-.13f,-.274f,.22f),.025f);yield return Pad(bHolder,winch.Output);
            Stage("8 · Giữ A, B và kéo C",2);yield return Pull(cOperator,winch.Handle,()=>winch.Complete);
            Stage("9 · Ba phần tụ lại",-1);yield return Merge(World(.42f,-.274f,.12f));
            Stage("10 · Hợp thể kéo nắp H",2);int merged=Parts()[0];yield return Pull(merged,winch.FinalCap,()=>winch.FinalCap.AtEnd);
            Stage("11 · Thoát ra ngoài",2);game.SelectFragment(merged);game.Motion.Move(merged,game.Owner.Outlet.position-game.Owner.Outlet.forward*.024f,false,true);
            yield return Wait(40,()=>game.Owner.Completed);
            Debug.Log("COGHE_DEMO_WIN escaped="+game.Matter.EscapedCount);yield return new WaitForSeconds(5);
        }
    }
}
#endif
