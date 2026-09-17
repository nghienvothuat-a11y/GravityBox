using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static void BuildAssemblyBridge(ExpansionContext c)
        {
            c.Definition.Title="16 · Ghép cầu";
            c.Definition.Lesson="Bám tay nắm, chỉ hướng đẩy/kéo. Ghép đường rồi trèo qua vùng trơn.";
            c.Definition.CanRotate=false;c.Definition.Passive=false;
            c.Definition.CameraEuler=new Vector3(34,-22,0);c.Definition.ViewRadius=.46f;
            c.Spawn=new Vector3(-.255f,-.27f,-.255f);
            c.Exit=new Vector3(.30f,-.076f,0);c.Outward=Vector3.right;
            ExpansionShell(c,.30f);
            foreach(var face in c.Surfaces)
            {
                if(face.name=="Laboratory floor")
                {
                    // A dry workshop remains in front of the bridge. The central
                    // band and the glass walls cannot supply a climbing shortcut.
                    face.HasSlipRegion=true;face.SlipRegion=new Rect(-.21f,-.072f,.42f,.144f);
                }
                else {face.Slippery=true;face.Shape.sharedMaterial=slick;}
            }
            var outlet=c.Surfaces.Find(p=>p.Hole);
            outlet.RingGrip=true;outlet.GripRadius=.068f;
            void Bank(string name,Vector3 position,Vector3 size,bool climb)
            {
                foreach(var normal in new[]{Vector3.up,Vector3.left,Vector3.right,Vector3.forward,Vector3.back})
                {
                    float d=Mathf.Abs(normal.x)*size.x+Mathf.Abs(normal.y)*size.y+Mathf.Abs(normal.z)*size.z;
                    Vector2 area=Mathf.Abs(normal.y)>.9f?new Vector2(size.x,size.z):Mathf.Abs(normal.x)>.9f?new Vector2(size.z,size.y):new Vector2(size.x,size.y);
                    var p=Panel(c.Root,name+" "+normal,position+normal*d*.5f,normal,area,plastic,false,Vector2.zero,0,c.Surfaces);
                    p.Slippery=!climb&&normal!=Vector3.up;
                    if(p.Slippery)p.Shape.sharedMaterial=slick;
                }
            }
            Bank("Climbable start bank",new Vector3(-.255f,-.20f,0),new Vector3(.09f,.20f,.36f),true);
            Bank("Exit bank",new Vector3(.255f,-.20f,0),new Vector3(.09f,.20f,.12f),false);
            var commandPlane=new GameObject("Bridge assembly command plane").transform;
            commandPlane.SetParent(c.Root,false);commandPlane.localPosition=new Vector3(0,-.10f,0);
            var monitor=new GameObject("Three bridge sockets",typeof(COgheAssemblyBridge)).GetComponent<COgheAssemblyBridge>();
            monitor.transform.SetParent(c.Root,false);monitor.Rails=new COgheRailSlider[3];
            for(int i=0;i<3;i++)
            {
                string key=((char)('A'+i)).ToString();float x=(i-1)*.14f;
                var rail=ExpansionRail(c,key+" bridge block",new Vector3(x,-.20f,-.16f),Vector3.forward,.16f,0,
                    new Vector3(.138f,.20f,.09f),.09f,.026f,false,true);
                rail.LatchAtEnd=true;monitor.Rails[i]=rail;
                Object.DestroyImmediate(rail.transform.Find(key+" bridge block stationary handle").gameObject);
                var prop=rail.GetComponent<VenomMovableProp>();
                foreach(var face in prop.GetComponentsInChildren<VenomSurfacePatch>())
                {face.Slippery=Vector3.Dot(face.Normal,Vector3.up)<.9f;if(face.Slippery)face.Shape.sharedMaterial=slick;}
                var grip=new GameObject(key+" low push-pull grip").transform;grip.SetParent(rail.transform,false);
                grip.localPosition=new Vector3(0,-.064f,-.075f);grip.localRotation=Quaternion.LookRotation(Vector3.back);
                prop.ManipulationGrip=grip;prop.ManipulationHandleOnly=true;prop.ManipulationPlane=commandPlane;
                var handle=MechanismVisual(grip,key+" handle",new Vector3(0,0,-.011f),new Vector3(.067f,.018f,.040f),metal);
                var contactBox=handle.gameObject.AddComponent<BoxCollider>();contactBox.sharedMaterial=contact;contactBox.contactOffset=.0003f;
                // Real dry top, slippery sides: only the left bank provides a
                // climb to bridge height. Ground-level handles remain reachable.
            }
        }
    }
}
