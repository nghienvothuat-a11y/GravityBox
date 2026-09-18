using UnityEngine;

namespace GravityBox.Venom
{
    public sealed partial class VenomCampaign
    {
        private VenomSurfacePatch propStance;
        private Vector3 propStanceOffset,propHandOffset,propHandOutward;
        private float propStanceHeight;
        private const float StanceHeight=.022f, StanceClearance=.028f, HandReach=.12f;

        // Choose planted feet independently of the hand. The old fixed offset
        // in front of every handle pulled the whole body away from the backing,
        // lost adhesion and repeated a fall/grasp cycle on vertical sliders.
        // This is a cold query on selection, never a scene scan per physics tick.
        private void FindPropStance(VenomMovableProp prop,Vector3 centre,Vector3 hand,Vector3 outward)
        {
            propStance=null;
            var rail=prop.GetComponent<COgheRailSlider>();
            if(rail==null)return;
            float best=float.PositiveInfinity;
            FindPropStanceAtHand(prop,rail,centre,hand,outward,ref best);
            // Existing supported approaches already choose their final hand
            // contact on arrival. Preserve that behavior, including diagonal
            // pulls, instead of pinning the contact selected from far away.
            if(best<0){propStance=null;return;}
            if(prop.ManipulationGrip==null)
                foreach(var face in prop.GetComponentsInChildren<VenomSurfacePatch>())
                {
                    if(Mathf.Abs(Vector3.Dot(face.Normal,rail.Frame.up))>.5f)continue;
                    Vector3 reach=centre;reach.y=prop.Body.worldCenterOfMass.y;
                    FindPropStanceAtHand(prop,rail,centre,face.Closest(reach),face.Normal,ref best);
                }
        }

        private void FindPropStanceAtHand(VenomMovableProp prop,COgheRailSlider rail,
            Vector3 centre,Vector3 hand,Vector3 outward,ref float best)
        {
            Vector3 preferred=hand+outward*.045f;
            foreach(var surface in Surfaces)
            {
                if(!surface.isActiveAndEnabled||surface.Shape==null||!surface.Shape.enabled||surface.SphereRadius>0)continue;
                var body=surface.Shape.attachedRigidbody;
                if(body!=null&&body!=Owner.Rotation.GetComponent<Rigidbody>())continue;
                // The feet must be able to accompany the hand along the rail.
                if(Mathf.Abs(Vector3.Dot(surface.Normal,rail.WorldAxis))>.25f)continue;
                // Keep an already supported working position (e.g. a floor
                // winch) instead of moving it behind the handle or its cover.
                float height=surface.DistanceInside(preferred);
                Vector3 preferredFoot=surface.Closest(preferred);
                if(height>=.012f&&height<=.055f&&surface.Contains(surface.transform.InverseTransformPoint(preferredFoot),-StanceClearance)&&
                    surface.Grip(preferredFoot)&&StanceIsClear(preferred)&&Clear(preferred,hand,0,prop.Body))
                {
                    float score=-1+(preferred-centre).sqrMagnitude*.1f;
                    if(score<best){best=score;StorePropStance(prop,surface,preferred,hand,outward,height);}
                }
                Vector3 basePoint=surface.Closest(hand),side=Vector3.Cross(surface.Normal,rail.WorldAxis).normalized;
                for(int sample=-3;sample<=3;sample++)
                {
                    Vector3 foot=basePoint+side*(sample*.035f);
                    Vector3 local=surface.transform.InverseTransformPoint(foot);
                    if(!surface.Contains(local,-StanceClearance)||!surface.Grip(foot))continue;
                    Vector3 candidate=foot+surface.Normal*StanceHeight;
                    // A hand can wrap the edge of the object it is grasping;
                    // other scenery still blocks reach. The torso never overlaps
                    // that object: StanceIsClear checks its full collider set.
                    // Leave room for the locomotor's 23 mm arrival tolerance.
                    if(Vector3.Distance(candidate,hand)>HandReach-.025f||!StanceIsClear(candidate)||!Clear(candidate,hand,0,prop.Body))continue;
                    // Prefer the working side of the authored handle. Choosing
                    // only the shortest walk can put the body behind a floor
                    // handle inside its cover instead of in the clear aisle.
                    float score=(candidate-preferred).sqrMagnitude*4+(candidate-centre).sqrMagnitude*.1f;
                    if(score>=best)continue;
                    best=score;StorePropStance(prop,surface,candidate,hand,outward,StanceHeight);
                }
            }
        }

        private void StorePropStance(VenomMovableProp prop,VenomSurfacePatch surface,
            Vector3 candidate,Vector3 hand,Vector3 outward,float height)
        {
            propStance=surface;propStanceHeight=height;
            Quaternion toLocal=Quaternion.Inverse(prop.Body.rotation);
            propStanceOffset=toLocal*(candidate-prop.Body.position);
            propHandOffset=toLocal*(hand-prop.Body.position);propHandOutward=toLocal*outward;
        }

        private bool StanceIsClear(Vector3 point)
        {
            foreach(var prop in Props)foreach(var shape in prop.CollisionShapes)
            {
                if(!shape.enabled||shape.isTrigger)continue;
                if((shape.ClosestPoint(point)-point).sqrMagnitude<StanceClearance*StanceClearance)return false;
            }
            return true;
        }

        private Vector3 PropBodyTarget(VenomMovableProp prop,Vector3 hand,Vector3 outward)
        {
            if(propStance==null)return hand+outward*.045f;
            return propStance.Closest(prop.Body.position+prop.Body.rotation*propStanceOffset)+propStance.Normal*propStanceHeight;
        }

        private bool CanGraspProp(VenomMovableProp prop,Vector3 centre,Vector3 hand)
        {
            if(propStance==null)return Vector3.Distance(centre,hand)<.065f;
            if(Vector3.Distance(centre,hand)>HandReach||
                Vector3.Distance(centre,PropBodyTarget(prop,hand,Vector3.zero))>.04f)return false;
            int feet=0;
            for(int i=0;i<32;i++)
                if(Matter.Groups[i]==Matter.Groups[Motion.Selected]&&Motion.HasGrip(i)&&
                    Motion.Support(i,out var shape,out _,out _)&&shape==propStance.Shape)feet++;
            return feet>=2;
        }
    }
}
