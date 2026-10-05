using System;
using UnityEngine;

namespace GravityBox.Venom
{
    public sealed partial class VenomCampaign
    {
        public const int LevelCount=60;
        public COgheMechanism[] Mechanisms { get; private set; }=Array.Empty<COgheMechanism>();
        private COgheMechanism[] transportMechanisms=Array.Empty<COgheMechanism>(),fusionBarriers=Array.Empty<COgheMechanism>(),exitControllers=Array.Empty<COgheMechanism>();
        public int LevelPage {get;set;}
        private Vector3[] graphSurfacePositions;
        private Quaternion[] graphSurfaceRotations;
        private float mechanismGraphAt;

        private void InitializeMechanisms()
        {
            Mechanisms=Owner.Apparatus.GetComponentsInChildren<COgheMechanism>(true);
            // These drives consume current supported-hand state after tasks have observed cuts,
            // merged commands and contact loss. Preserve the relative order of all older mechanisms.
            var ordered = new COgheMechanism[Mechanisms.Length]; int next = 0;
            foreach(var mechanism in Mechanisms)if(!(mechanism is COgheCooperativeDrive))ordered[next++]=mechanism;
            foreach(var mechanism in Mechanisms)if(mechanism is COgheCooperativeDrive)ordered[next++]=mechanism;
            Mechanisms=ordered;
            tapRails=Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(true);
            // Mechanisms are drawn between physics ticks like the creature is, so a 120 Hz rail does not judder against it
            // at an uneven frame rate. FixedUpdate still reads the simulated pose (COgheInterpolationProbeTests).
            // Scripted physics (tests, proofs) steps many ticks per frame and never interpolates; there transforms must
            // keep following the simulation, so only real-time play turns interpolation on.
            var shell=Owner.Rotation!=null?Owner.Rotation.GetComponent<Rigidbody>():null;
            if(Physics.simulationMode==SimulationMode.FixedUpdate)
                foreach(var body in Owner.Apparatus.GetComponentsInChildren<Rigidbody>(true))
                    if(!body.isKinematic&&body!=shell)body.interpolation=RigidbodyInterpolation.Interpolate;
            foreach(var mechanism in Mechanisms)mechanism.InitializeMechanism(this);
            transportMechanisms=Array.FindAll(Mechanisms,m=>m.TransportsTissue);
            fusionBarriers=Array.FindAll(Mechanisms,m=>m.SeparatesTissue);
            exitControllers=Array.FindAll(Mechanisms,m=>m.ControlsExit);
            LevelPage=(Definition.Order-1)/10;
        }
        private void ResetMechanisms()
        {
            foreach(var mechanism in Mechanisms)mechanism.ResetMechanism(this);
            graphSurfacePositions=new Vector3[Surfaces.Length];graphSurfaceRotations=new Quaternion[Surfaces.Length];
            CaptureMechanismGraphPose();mechanismGraphAt=0;
        }
        private void StepMechanisms(float dt)
        {
            foreach(var mechanism in Mechanisms)if(mechanism.isActiveAndEnabled)mechanism.StepMechanism(this,dt);
            if(Mechanisms.Length==0||Matter.SimulationTime<mechanismGraphAt)return;
            // A handle in mid-pull moves only its carriage and what it drives. The task rebuilds the graph once it settles
            // and an opening aperture rebuilds at once; a full rebuild every 0.35 s of the pull only stalls frames.
            foreach(var tap in tapRails)if(tap.Phase==COgheTapRail.TaskPhase.Operating&&!tap.Holding&&tap.Rail.Moving)return;
            mechanismGraphAt=Matter.SimulationTime+.35f;
            for(int i=0;i<Surfaces.Length;i++)
                if(Vector3.Distance(graphSurfacePositions[i],Root.InverseTransformPoint(Surfaces[i].transform.position))>.010f||
                   Quaternion.Angle(graphSurfaceRotations[i],Quaternion.Inverse(Root.rotation)*Surfaces[i].transform.rotation)>5)
                {Motion.BuildGraph();CaptureMechanismGraphPose();break;}
        }
        private void CaptureMechanismGraphPose()
        {
            for(int i=0;i<Surfaces.Length;i++)
            {graphSurfacePositions[i]=Root.InverseTransformPoint(Surfaces[i].transform.position);graphSurfaceRotations[i]=Quaternion.Inverse(Root.rotation)*Surfaces[i].transform.rotation;}
        }
        public bool FinalExitAvailable
        {
            get
            {
                if(!GateOpen)return false;
                foreach(var m in exitControllers)if(m.isActiveAndEnabled&&m.ControlsExit&&!m.ExitUnlocked)return false;
                return true;
            }
        }
        public bool MechanismSuppressesMotion(int particle)
        {
            if(Home)return false;
            foreach(var m in transportMechanisms)if(m.isActiveAndEnabled&&m.SuppressesMotion(particle))return true;
            return false;
        }
        public bool IsFlowing(int particle)
        {
            if(InTube&&Matter.Groups[particle]==Matter.Groups[tubeAnchor])return true;
            if(Home)return false;
            foreach(var m in transportMechanisms)if(m.isActiveAndEnabled&&m.IsFlowing(particle))return true;
            return false;
        }
        private bool MechanismBlocksFusion(int a,int b)
        {
            foreach(var m in fusionBarriers)if(m.isActiveAndEnabled&&m.BlocksFusion(a,b))return true;
            return false;
        }
        private bool TouchMechanism(Ray ray,float obstruction)
        {
            if(Home)return false;
            if(heldProp!=null)
            {
                // Holding a crate, a tap on a lift button lets the crate go and presses the button (Mrk, 05/10/2026).
                foreach(var m in Mechanisms)if(m.isActiveAndEnabled&&m.ClaimsTapWhileHolding(ray,obstruction)){ReleaseProp();return m.TryTouch(this,ray,obstruction);}
                return false;
            }
            foreach(var m in Mechanisms)if(m.isActiveAndEnabled&&m.TryTouch(this,ray,obstruction))return true;
            return false;
        }
        private string MechanismActivity()
        {
            foreach(var m in Mechanisms)if(m.isActiveAndEnabled&&!string.IsNullOrEmpty(m.Activity))return m.Activity;
            return null;
        }
        public void SelectFragment(int anchor)
        {
            if(anchor<0||anchor>=Matter.Bodies.Length||Matter.Escaped[anchor])return;
            if(Matter.Groups[anchor]!=Matter.Groups[Motion.Selected]&&(heldProp!=null||approachProp!=null))ReleaseProp();
            Motion.Selected=anchor;
        }

        public bool TryRailManipulationIntent(int anchor,out Vector3 target,out Vector3 velocity)
        {
            foreach(var tap in tapRails)if(tap.TryIntent(anchor,out target,out velocity))return true;
            target=velocity=Vector3.zero;
            if(heldProp==null||Matter.Groups[anchor]!=Matter.Groups[Motion.Selected])return false;
            var rail=heldProp.GetComponent<COgheRailSlider>();if(rail==null)return false;
            velocity=Vector3.ClampMagnitude(Vector3.Project(propTarget-heldProp.Body.position,rail.WorldAxis)*2,.085f);
            target=PropBodyTarget(heldProp,heldProp.Body.position+heldProp.Body.rotation*gripLocal,
                heldProp.Body.rotation*gripNormalLocal)+velocity*.5f;
            return true;
        }

        public void NotifyMechanismCut(Transform blade,int[] groupsBeforeCut)
        {
            if(blade==null||groupsBeforeCut==null||groupsBeforeCut.Length!=Matter.Bodies.Length)return;
            var affected=new bool[32];bool any=false;
            for(int i=0;i<32;i++)for(int j=i+1;j<32;j++)
                if(groupsBeforeCut[i]==groupsBeforeCut[j]&&Matter.Groups[i]!=Matter.Groups[j])
                {
                    for(int k=0;k<32;k++)if(groupsBeforeCut[k]==groupsBeforeCut[i])affected[k]=true;
                    any=true;
                }
            if(!any)return;
            Vector3 normal=blade.right;float positive=0,negative=0;var sides=new int[32];
            for(int i=0;i<32;i++)if(affected[i]&&!Matter.Escaped[i])
            {
                Motion.Cancel(i);sides[i]=Vector3.Dot(Motion.Centre(i)-blade.position,normal)>=0?1:-1;
                if(sides[i]>0)positive+=Matter.Bodies[i].mass;else negative+=Matter.Bodies[i].mass;
            }
            if(positive<=0||negative<=0)return;
            float total=positive+negative,relativeSpeed=Mathf.Min(.8f,.60f*total/Mathf.Max(positive,negative));
            for(int i=0;i<32;i++)if(affected[i]&&!Matter.Escaped[i])
                Matter.Bodies[i].AddForce(normal*(relativeSpeed*(sides[i]>0?negative:-positive)/total),ForceMode.VelocityChange);
        }
    }
}
