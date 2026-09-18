using UnityEngine;

namespace GravityBox.Venom
{
    // The studio is a stationary, collider-free table. An orthographic zoom-out
    // can see beyond its edges, or start rays below it at a low camera pitch.
    // Extend only this existing mesh and move the camera along its view axis;
    // neither operation changes the chamber's screen projection or physics.
    internal sealed class COgheStudioBackdrop
    {
        private readonly Transform bench;
        private readonly Bounds meshBounds;
        private readonly Matrix4x4 originalToLocal;
        private readonly Vector3 originalPosition,originalScale;
        private readonly Quaternion originalRotation;
        private readonly float tabletop;
        private float scaleX=1,scaleZ=1;

        public COgheStudioBackdrop(Transform owner)
        {
            var studio=owner.Find("Day Lab studio");if(studio==null)return;
            // Both art builders batch the broad tabletop separately from the
            // contact shadow. Discover once, never scan renderers per frame.
            foreach(var filter in studio.GetComponentsInChildren<MeshFilter>())
            {
                var renderer=filter.GetComponent<Renderer>();
                if(renderer==null||renderer.bounds.size.x<8||renderer.bounds.size.z<8||filter.GetComponent<Collider>()!=null)continue;
                bench=filter.transform;meshBounds=filter.sharedMesh.bounds;tabletop=renderer.bounds.max.y;
                originalPosition=bench.localPosition;originalScale=bench.localScale;originalRotation=bench.localRotation;
                originalToLocal=bench.worldToLocalMatrix;break;
            }
        }

        public void Fit(Camera camera,Vector3 framingCentre)
        {
            Vector3 forward=camera.transform.forward,right=camera.transform.right,up=camera.transform.up;
            float distance=2,down=-forward.y;
            float halfHeight=camera.orthographicSize,halfWidth=halfHeight*camera.aspect;
            if(bench!=null&&down>.01f)
            {
                float lowestOffset=Mathf.Abs(up.y)*halfHeight+Mathf.Abs(right.y)*halfWidth;
                distance=Mathf.Max(distance,(tabletop+.05f-framingCentre.y+lowestOffset)/down+camera.nearClipPlane);
            }
            camera.transform.position=framingCentre-forward*distance;
            if(bench==null||down<=.01f)return;

            float x=scaleX,z=scaleZ,far=camera.farClipPlane;
            for(int i=0;i<4;i++)
            {
                Vector3 origin=camera.transform.position+right*((i&1)==0?-halfWidth:halfWidth)+up*((i&2)==0?-halfHeight:halfHeight);
                float depth=(tabletop-origin.y)/forward.y;
                Vector3 point=originalToLocal.MultiplyPoint3x4(origin+forward*depth);
                x=Mathf.Max(x,(Mathf.Abs(point.x-meshBounds.center.x)+.15f)/meshBounds.extents.x);
                z=Mathf.Max(z,(Mathf.Abs(point.z-meshBounds.center.z)+.15f)/meshBounds.extents.z);
                far=Mathf.Max(far,depth+.25f);
            }
            camera.farClipPlane=far;
            if(x<=scaleX&&z<=scaleZ)return;
            scaleX=x;scaleZ=z;
            var scale=Vector3.Scale(originalScale,new Vector3(x,1,z));
            // Scale around the authored tabletop centre, keeping its height and
            // shadow coordinates fixed. No mesh allocation or extra draw call.
            bench.localPosition=originalPosition+originalRotation*Vector3.Scale(originalScale-scale,meshBounds.center);
            bench.localScale=scale;
        }
    }
}
