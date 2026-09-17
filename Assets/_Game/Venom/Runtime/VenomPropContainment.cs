using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Last-resort contact recovery for the loose lid in its authored
    /// cube. PhysX/CCD handle normal contacts; this removes residual penetration
    /// after a rotating wall sweeps across a corner. The lid is too large to fit
    /// the final aperture, so it always belongs inside this chamber.</summary>
    public sealed class VenomPropContainment : MonoBehaviour
    {
        public float ChamberHalfSize=.3f;
        private Rigidbody body,chamber;
        private Vector3[] corners;
        private readonly Vector3[] support=new Vector3[6];
        public void Initialize(Transform root)
        {
            body=GetComponent<Rigidbody>();chamber=root.GetComponent<Rigidbody>();
            var boxes=GetComponentsInChildren<BoxCollider>();corners=new Vector3[boxes.Length*8];int at=0;
            foreach(var box in boxes)for(int i=0;i<8;i++)
            {
                Vector3 signs=new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1);
                corners[at++]=transform.InverseTransformPoint(box.transform.TransformPoint(box.center+Vector3.Scale(signs,box.size*.5f)));
            }
        }
        public void ResolveContacts()
        {
            if(chamber==null||body==null||corners==null)return;
            Quaternion inv=Quaternion.Inverse(chamber.rotation);
            Vector3 min=Vector3.one*float.PositiveInfinity,max=Vector3.one*float.NegativeInfinity;
            foreach(var corner in corners)
            {
                Vector3 world=body.position+body.rotation*corner;
                Vector3 p=inv*(world-chamber.position);
                for(int axis=0;axis<3;axis++)
                {
                    if(p[axis]<min[axis]){min[axis]=p[axis];support[axis*2]=world;}
                    if(p[axis]>max[axis]){max[axis]=p[axis];support[axis*2+1]=world;}
                }
            }
            Vector3 correction=Vector3.zero;
            for(int axis=0;axis<3;axis++)
            {
                float delta=min[axis]<-ChamberHalfSize?-ChamberHalfSize-min[axis]:max[axis]>ChamberHalfSize?ChamberHalfSize-max[axis]:0;
                if(delta==0)continue;
                correction[axis]=delta;
                Vector3 localNormal=Vector3.zero;localNormal[axis]=Mathf.Sign(delta);
                Vector3 normal=chamber.rotation*localNormal,point=support[axis*2+(delta<0?1:0)];
                // A moving wall transfers its contact velocity. Remove only the
                // relative velocity into that wall; retain tangential sliding.
                float closing=Vector3.Dot(body.GetPointVelocity(point)-chamber.GetPointVelocity(point),normal);
                if(closing<0)body.linearVelocity-=normal*closing;
            }
            if(correction.sqrMagnitude>0)body.position+=chamber.rotation*correction;
        }
    }
}
