using GravityBox.Venom;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        public static void ApplyVesselDetails(VenomCampaign game,int shape)
        {
            var root=game.GetComponent<VenomLevelController>().Rotation.transform;
            var art=Child(root,"Vessel identity · presentation only");
            // Fine external mould seams and closed facial motifs carry identity.
            // Only the flush mint outline denotes an actual opening.
            if(shape==0)
            {
                Ring(art,"Porcelain foot band",new Vector3(0,-.285f,0),Vector3.up,.096f,.007f,ivory);
                Ring(art,"Neck mould seam",new Vector3(0,.29f,0),Vector3.up,.073f,.002f,alloy);
                Ring(art,"Shoulder mould seam",new Vector3(0,.13f,0),Vector3.up,.197f,.0015f,alloy);
            }
            if(shape==1||shape==3)
            {
                float y=shape==1?.05f:-.005f,z=shape==1?-.105f:-.195f;
                for(int side=-1;side<=1;side+=2)
                {
                    var points=new Vector3[65];
                    for(int i=0;i<points.Length;i++)
                    {float a=i*Mathf.PI*2/64;points[i]=new Vector3(side*.105f+Mathf.Cos(a)*(shape==1?.063f:.068f),y+Mathf.Sin(a)*(shape==1?.029f:.055f),z+Mathf.Abs(Mathf.Cos(a))*.010f);}
                    VesselStroke(art,"Closed eye socket moulding",points,shape==1?.006f:.011f,ivory);
                }
                VesselStroke(art,"Nose ridge",new[]{new Vector3(-.023f,y-.032f,z-.002f),new Vector3(0,y-.090f,z-.020f),new Vector3(.023f,y-.032f,z-.002f)},.006f,alloy);
                if(shape==1)VesselStroke(art,"Closed mouth engraving",new[]{new Vector3(-.055f,-.175f,-.078f),new Vector3(0,-.187f,-.081f),new Vector3(.055f,-.175f,-.078f)},.003f,alloy);
                else
                {
                    for(int i=0;i<7;i++)
                        Box(art,"Porcelain tooth moulding",new Vector3((i-3)*.021f,-.219f,-.133f),new Vector3(.016f,.027f,.007f),.004f,ivory);
                    VesselStroke(art,"Jaw contour",new[]{new Vector3(-.12f,-.17f,-.134f),new Vector3(-.10f,-.25f,-.115f),new Vector3(0,-.268f,-.12f),new Vector3(.10f,-.25f,-.115f),new Vector3(.12f,-.17f,-.134f)},.006f,alloy);
                }
            }
            if(shape==2)
            {
                var handle=new Vector3[49];
                for(int i=0;i<handle.Length;i++){float a=Mathf.Lerp(65,295,i/48f)*Mathf.Deg2Rad;handle[i]=new Vector3(-.17f+Mathf.Cos(a)*.16f,-.035f+Mathf.Sin(a)*.175f,0);}
                VesselStroke(art,"External porcelain handle",handle,.018f,ivory);
                Ring(art,"Sealed lid seam",new Vector3(.026f,.075f,0),Vector3.up,.192f,.002f,alloy);
                Ring(art,"Teapot foot",new Vector3(0,-.245f,0),Vector3.up,.12f,.006f,ivory);
            }
            if(shape==4)
            {
                var seam=new Vector3[73];
                for(int i=0;i<seam.Length;i++)
                {
                    float t=i/72f,a=Mathf.Lerp(-150,230,t)*Mathf.Deg2Rad,r=Mathf.Lerp(.12f,.35f,t);
                    seam[i]=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,-Mathf.Lerp(.070f,.048f,t)-.007f);
                }
                VesselStroke(art,"Spiral mould seam",seam,.002f,alloy);
            }
            var owner=game.GetComponent<VenomLevelController>();
            Ring(art,"Physical opening lip",owner.Outlet.localPosition-owner.Outlet.forward*.001f,owner.Outlet.forward,owner.ApertureRadius+.0055f,.0015f,alloy);
            CombineByMaterial(art);
            var presentation=game.GetComponent<COgheDayLabPresentation>();
            var occluders=new System.Collections.Generic.List<Renderer>(presentation.FocusOccluders);
            occluders.AddRange(art.GetComponentsInChildren<Renderer>());
            presentation.FocusOccluders=occluders.ToArray();
        }
        private static void VesselStroke(Transform parent,string name,Vector3[] points,float width,Material material)
        {
            var go=Child(parent,name);var line=go.gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace=false;line.sharedMaterial=material;line.positionCount=points.Length;line.SetPositions(points);
            line.startWidth=line.endWidth=width;line.numCornerVertices=4;line.numCapVertices=4;
            line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
        }
    }
}
