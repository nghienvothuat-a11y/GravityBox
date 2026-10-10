using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The cherry is the goal instead of an exit hole (Mrk, 10/10/2026: no glass box, a flat surface; win where the cherry is).
    /// The level is won when the whole COghe, merged into one body, stands on the cherry's surface beside it. There is no pull
    /// toward it: the old exit assist lifted a body at the foot of a 6 cm slick riser into the hole (levels 13, 40, 49, 50, 56
    /// could be skipped). The goal point is the fixed <see cref="Cherry"/> pivot; only <see cref="Visual"/> bobs and is eaten,
    /// so presentation never moves the win threshold.
    /// </summary>
    public sealed class COgheCherryGoal : MonoBehaviour
    {
        [Tooltip("Fixed goal point: the pivot stands on the surface the cherry sits on. Never animated.")]
        public Transform Cherry;
        [Tooltip("What the player sees, a child of Cherry: bobs while waiting, shrinks into COghe when eaten.")]
        public Transform Visual;
        [Tooltip("How close (horizontally, metres) the body's centre must come to the goal point.")]
        public float Reach=.05f;
        [Tooltip("How far above the goal point's surface the body's centre may be.")]
        public float Height=.06f;
        [Tooltip("Particles that must rest on the goal's surface (within SurfaceBand of its height).")]
        public int Footing=4;
        public float SurfaceBand=.012f;
        public bool Eaten { get; private set; }
        private Vector3 visualRest;private float eatenAt=-1;private VenomCampaign game;

        public void Bind(VenomCampaign campaign){game=campaign;if(Visual!=null)visualRest=Visual.localPosition;ResetGoal();}
        public void ResetGoal()
        {
            Eaten=false;eatenAt=-1;
            if(Visual!=null){Visual.localPosition=visualRest;Visual.localScale=Vector3.one;Visual.localRotation=Quaternion.identity;Visual.gameObject.SetActive(true);}
        }
        /// <summary>The merged body's centre is over the goal and it stands on the goal's surface (not in the air, not below).</summary>
        public bool Reached(VenomCampaign campaign)
        {
            if(Cherry==null||campaign.Matter.TotalFragmentCount!=1)return false;
            Vector3 g=campaign.Root.InverseTransformPoint(Cherry.position);
            Vector3 c=campaign.Root.InverseTransformPoint(campaign.Motion.Centre(campaign.Motion.Selected));
            float dy=c.y-g.y;
            if(new Vector2(c.x-g.x,c.z-g.z).magnitude>=Reach||dy<0||dy>=Height)return false;
            int resting=0;
            for(int i=0;i<32&&resting<Footing;i++)
            {
                if(!campaign.Motion.Support(i,out _,out var point,out var normal))continue;
                Vector3 p=campaign.Root.InverseTransformPoint(point),n=campaign.Root.InverseTransformDirection(normal);
                if(n.y>.9f&&Mathf.Abs(p.y-g.y)<SurfaceBand&&new Vector2(p.x-g.x,p.z-g.z).magnitude<Reach+.04f)resting++;
            }
            return resting>=Footing;
        }
        public void Eat(float now){if(Eaten)return;Eaten=true;eatenAt=now;}
        private void LateUpdate()
        {
            if(Visual==null||game==null)return;
            float t=game.Matter!=null?game.Matter.SimulationTime:Time.time;
            if(!Eaten)
            {
                Visual.localPosition=visualRest+Vector3.up*(.004f*(.5f+.5f*Mathf.Sin(t*2.4f)));
                Visual.localRotation=Quaternion.Euler(0,Mathf.Sin(t*.9f)*12,0);return;
            }
            // eaten: a quick squash, then it shrinks into COghe
            float k=Mathf.Clamp01((t-eatenAt)/.45f);
            Visual.localScale=Vector3.one*Mathf.Max(0,(1-k)*(1+.25f*Mathf.Sin(k*Mathf.PI)));
            if(k>=1)Visual.gameObject.SetActive(false);
        }
    }
}
