using UnityEngine;

namespace GravityBox.Presentation
{
    // Presentation follows the two attachment points; it has no physics access.
    // The builder calls Refresh immediately so edit-mode previews have a valid
    // local-space coil even before the first rendered play-mode frame.
    [RequireComponent(typeof(LineRenderer))]
    public sealed class RackSpringVisual : MonoBehaviour
    {
        [SerializeField] private Transform fixedFrame;
        [SerializeField] private Transform movingRack;
        [SerializeField] private LineRenderer coil;
        [SerializeField] private Vector3 fixedPoint;
        [SerializeField] private Vector3 movingPoint;

        public void Configure(Transform frame,Transform rack,LineRenderer line,Vector3 start,Vector3 end)
        {
            fixedFrame=frame;movingRack=rack;coil=line;fixedPoint=start;movingPoint=end;
            coil.useWorldSpace=false;
            Refresh();
        }

        private void OnEnable()=>Refresh();
        private void LateUpdate()=>Refresh();

        public void Refresh()
        {
            if(coil==null || fixedFrame==null || movingRack==null) return;
            Vector3 start=fixedFrame.TransformPoint(fixedPoint);
            Vector3 end=movingRack.TransformPoint(movingPoint);
            Vector3 axis=(end-start).normalized,up=fixedFrame.up;
            Vector3 side=Vector3.Cross(axis,up).normalized;
            const int count=32;
            coil.positionCount=count;
            for(int i=0;i<count;i++)
            {
                float t=i/(float)(count-1),phase=t*10*Mathf.PI;
                float radius=(i==0 || i==count-1)?0:.003f;
                Vector3 point=Vector3.Lerp(start,end,t)
                    +radius*(up*Mathf.Sin(phase)+side*Mathf.Cos(phase));
                coil.SetPosition(i,coil.transform.InverseTransformPoint(point));
            }
        }
    }
}
