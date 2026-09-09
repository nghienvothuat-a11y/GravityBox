using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    // Metre-authored compound mechanisms. All work comes from gravity/ball contact;
    // only the existing contact latch and rack-return spring retain/store energy.
    // The caller owns the enclosing shell and the courts between modules.
    internal static class HardCampaignModules
    {
        internal sealed class Frame
        {
            internal readonly MechanicalAuthoring A;
            internal readonly Vector3 Origin;
            internal readonly Quaternion Rotation;
            internal Frame(MechanicalAuthoring a, Vector3 origin, Quaternion rotation)
            { A=a; Origin=origin; Rotation=rotation; }
            internal Vector3 P(Vector3 p)=>Origin+Rotation*p;
            internal GameObject Block(string name,Vector3 p,Vector3 size,Material material,Transform parent=null,bool collision=true)
            {
                var go=A.Block(name,parent==null?P(p):p,size,material,parent,collision);
                if(parent==null) go.transform.localRotation=Rotation;
                return go;
            }
            internal PhysicalHinge Hinge(string name,Vector3 anchor,Vector3 axis,float mass,float min,float max,float damping=.000025f)
            {
                var hinge=A.Hinge(name,P(anchor),axis,mass,min,max,damping);
                hinge.transform.localRotation=Rotation;
                return hinge;
            }
        }

        internal static void Pocket(MechanicalAuthoring a,Vector3 origin,Quaternion rotation,float width=.09f,float length=.10f,float height=.07f)
        {
            var f=new Frame(a,origin,rotation);
            f.Block("Pocket left cheek",new Vector3(-width*.5f-.003f,height*.5f,0),new Vector3(.006f,height,length),a.Glass);
            f.Block("Pocket right cheek",new Vector3(width*.5f+.003f,height*.5f,0),new Vector3(.006f,height,length),a.Glass);
            f.Block("Pocket closed back",new Vector3(0,height*.5f,length*.5f),new Vector3(width+.012f,height,.006f),a.Glass);
            f.Block("Pocket resting inlay",new Vector3(0,.001f,0),new Vector3(width*.75f,.0005f,length*.7f),a.Rim,null,false);
        }

        // A 76 mm ball aperture with a real 120 mm gravity slide and no motor.
        internal static GravitySliderGuide Slider(MechanicalAuthoring a,Vector3 origin,Quaternion rotation,float halfWidth=.30f,float halfDepth=.06f)
        {
            var f=new Frame(a,origin,rotation);
            var gate=GravitySliderAuthoring.Build(a.Root,a.Frame,a.Contact,origin,rotation);
            // The gate's metal extends to y +/-40 mm; lintels close the unused height.
            const float apertureHalfHeight=.043f;
            float h=halfDepth-apertureHalfHeight;
            if(h>.001f)
            {
                f.Block("Slider lower lintel",new Vector3(0,-halfDepth+h*.5f,0),new Vector3(.028f,h,.080f),a.Glass);
                f.Block("Slider upper lintel",new Vector3(0,halfDepth-h*.5f,0),new Vector3(.028f,h,.080f),a.Glass);
            }
            f.Block("Slider south partition",new Vector3(0,0,(-halfWidth-.04f)*.5f),new Vector3(.028f,halfDepth*2,halfWidth-.04f),a.Glass);
            // Split around the gate pocket. The pocket is inaccessible to the ball
            // because its X clearance beside the moving plate is only 3 mm.
            f.Block("Slider north partition",new Vector3(0,0,(halfWidth+.17f)*.5f),new Vector3(.028f,halfDepth*2,Mathf.Max(.01f,halfWidth-.17f)),a.Glass);
            f.Block("Slider west sleeve",new Vector3(-.019f,0,.108f),new Vector3(.006f,halfDepth*2,.136f),a.Glass);
            f.Block("Slider east sleeve",new Vector3(.019f,0,.108f),new Vector3(.006f,halfDepth*2,.136f),a.Glass);
            return gate.GetComponent<GravitySliderGuide>();
        }

        internal static PhysicalHinge Bridge(MechanicalAuthoring a,Vector3 origin,Quaternion rotation,bool parkingNorth=true,float recoveryFloor=-.147f)
        {
            var f=new Frame(a,origin,rotation);
            // An enclosed doorway is traversable only over the sill. The recovery
            // floor is a local dead-end beneath the east landing, never an exit bypass.
            f.Block("Bridge approach abutment",new Vector3(-.14f,-.056f,0),new Vector3(.274f,.106f,.30f),a.Glass);
            f.Block("Bridge south partition",new Vector3(0,.015f,-.1125f),new Vector3(.010f,.264f,.115f),a.Glass);
            f.Block("Bridge north partition",new Vector3(0,.015f,.1125f),new Vector3(.010f,.264f,.115f),a.Glass);
            f.Block("Bridge solid lower sill",new Vector3(0,-.069f,0),new Vector3(.010f,.106f,.110f),a.Frame);
            // Keep a 5 mm longitudinal gap beyond the plank's 140 mm tip.
            // Previously the landing began at 138 mm and its rails exactly
            // touched the moving rails at Z=52 mm: the fold jammed near 80°.
            f.Block("Bridge landing",new Vector3(.1925f,-.001f,0),new Vector3(.095f,.008f,.118f),a.Frame);
            foreach(int sign in new[]{-1,1})
                f.Block("Bridge landing rail "+sign,new Vector3(.1925f,.028f,sign*.059f),new Vector3(.095f,.050f,.006f),a.Glass);
            var bridge=f.Hinge("Campaign folding bridge",Vector3.zero,Vector3.forward,.080f,-91,0,.000035f);
            f.Block("Bridge moving plank",new Vector3(0,.070f,0),new Vector3(.006f,.140f,.100f),a.Frame,bridge.transform);
            foreach(int sign in new[]{-1,1})
                f.Block("Bridge moving rail "+sign,new Vector3(-.020f,.075f,sign*.049f),new Vector3(.040f,.130f,.006f),a.Glass,bridge.transform);
            var seat=f.Block("Bridge contact receiver",new Vector3(.133f,-.011f,0),new Vector3(.015f,.016f,.110f),a.Frame);
            bridge.gameObject.AddComponent<ContactSeatLatch>().Configure(bridge,seat.GetComponent<Collider>(),-90,.8f);
            Pocket(a,f.P(new Vector3(-.20f,-.003f,parkingNorth?.080f:-.080f)),rotation*(parkingNorth?Quaternion.identity:Quaternion.Euler(0,180,0)),.09f,.10f);
            // The recovery ramp reaches the right landing without needing the lid.
            // Its mouth faces back to the current stage; surrounding shell partitions
            // must prevent a route from this court directly into later modules.
            var recovery=Ramp(a,f.P(new Vector3(.30f,recoveryFloor,.10f)),f.P(new Vector3(.245f,.003f,0)),.075f,a.Frame);
            recovery.name="Bridge graded recovery return";
            return bridge;
        }

        // The rotating split cam, gear and rack retain exactly the native P22
        // tooth contact mechanism. One/two tooth variants rotate the slot geometry
        // relative to the gear, so passage opens at a reachable retained position.
        internal static MemoryRatchetAssembly Cam(MechanicalAuthoring a,Vector3 origin,Quaternion rotation,int teeth=1,int sweepTeeth=0,float halfWidth=.36f)
        {
            teeth=Mathf.Clamp(teeth,1,3);
            var f=new Frame(a,origin,rotation);
            var cam=f.Hinge("Campaign retained cam",Vector3.zero,Vector3.down,.065f,0,31.5f,.00008f);
            float slotStart=(3-teeth)*30f;
            foreach(int sign in new[]{-1,1})
            {
                var go=new GameObject("Split cam half "+sign,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));
                go.transform.SetParent(cam.transform,false);go.transform.localRotation=Quaternion.Euler(0,-slotStart,0);
                Mesh mesh=HalfDisc(.094f,.036f,.052f,sign,sweepTeeth*30f);
                go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=a.Frame;
                var collider=go.GetComponent<MeshCollider>();collider.sharedMesh=mesh;collider.convex=true;collider.sharedMaterial=a.Contact;collider.contactOffset=.0005f;
            }
            for(int i=0;i<12;i++)
            {
                float angle=i*30*Mathf.Deg2Rad;
                var tooth=f.Block("Cam contact tooth "+i,new Vector3(Mathf.Sin(angle)*.099f,.045f,Mathf.Cos(angle)*.099f),new Vector3(.016f,.016f,.015f),a.Frame,cam.transform);
                tooth.transform.localRotation=Quaternion.Euler(0,i*30,0);
            }
            f.Block("Cam overhead axle",new Vector3(0,.046f,0),new Vector3(.066f,.012f,.025f),a.Frame,cam.transform);
            foreach(int sign in new[]{-1,1})
                f.Block("Cam partition "+sign,new Vector3(sign*(halfWidth+.108f)*.5f,0,0),new Vector3(halfWidth-.108f,.120f,.010f),a.Glass);
            for(int side=0;side<2;side++) for(int segment=0;segment<24;segment++)
            {
                float angle=(-70+side*180+(segment+.5f)*(140f/24))*Mathf.Deg2Rad;
                Vector3 radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                var sleeve=f.Block("Cam fixed sleeve "+side+" "+segment,radial*.116f,new Vector3(.014f,.120f,.014f),a.Glass);
                sleeve.transform.localRotation=rotation*Quaternion.LookRotation(radial,Vector3.up);
            }
            Vector3 rackAt=new Vector3(-.145f,0,-.145f);
            var rackGo=new GameObject("Campaign ball driven rack",typeof(Rigidbody),typeof(PhysicalProp),typeof(ConfigurableJoint));
            rackGo.transform.SetParent(a.Root,false);rackGo.transform.localPosition=f.P(rackAt);rackGo.transform.localRotation=rotation;
            var body=rackGo.GetComponent<Rigidbody>();body.mass=.018f;body.useGravity=false;body.solverIterations=32;body.solverVelocityIterations=12;
            f.Block("Rack contact face",Vector3.zero,new Vector3(.010f,.108f,.068f),a.Glass,rackGo.transform);
            f.Block("Rack overhead linkage",new Vector3(.065f,.046f,0),new Vector3(.140f,.012f,.013f),a.Frame,rackGo.transform);
            var slider=rackGo.GetComponent<ConfigurableJoint>();slider.autoConfigureConnectedAnchor=false;
            slider.connectedBody=a.Root.GetComponent<Rigidbody>();slider.connectedAnchor=f.P(rackAt+Vector3.right*.031f);
            slider.axis=Vector3.right;slider.secondaryAxis=Vector3.up;
            slider.xMotion=ConfigurableJointMotion.Limited;slider.yMotion=slider.zMotion=ConfigurableJointMotion.Locked;
            slider.angularXMotion=slider.angularYMotion=slider.angularZMotion=ConfigurableJointMotion.Locked;
            slider.linearLimit=new SoftJointLimit{limit=.031f,contactDistance=.0002f};slider.enableCollision=true;
            var guide=rackGo.AddComponent<GravitySliderGuide>();guide.Configure(slider,f.P(rackAt),rotation*Vector3.right,.062f,.057f);
            var fingerGo=new GameObject("Campaign rack folding finger",typeof(Rigidbody),typeof(PhysicalProp),typeof(HingeJoint));
            Vector3 fingerAt=new Vector3(-.015f,.045f,-.136f);
            fingerGo.transform.SetParent(a.Root,false);fingerGo.transform.localPosition=f.P(fingerAt);fingerGo.transform.localRotation=rotation;
            var fingerBody=fingerGo.GetComponent<Rigidbody>();fingerBody.mass=.004f;fingerBody.useGravity=false;fingerBody.solverIterations=32;fingerBody.solverVelocityIterations=12;
            var finger=fingerGo.GetComponent<HingeJoint>();finger.autoConfigureConnectedAnchor=false;finger.connectedBody=body;
            finger.connectedAnchor=fingerAt-rackAt;finger.axis=Vector3.up;finger.useLimits=true;finger.limits=new JointLimits{min=0,max=85,contactDistance=.05f};
            f.Block("Rack tooth driving finger",new Vector3(0,0,.016f),new Vector3(.008f,.013f,.033f),a.Rim,fingerGo.transform);
            var pawl=fingerGo.AddComponent<PhysicalHinge>();pawl.Configure(finger,.000005f);
            var assemblyObject=new GameObject("Cam retained state assembly");assemblyObject.transform.SetParent(a.Root,false);
            var memory=assemblyObject.AddComponent<MemoryRatchetAssembly>();memory.Cam=cam;memory.Rack=guide;memory.DrivePawl=pawl;memory.MaximumTeeth=teeth;memory.PassageOpenDegrees=teeth*30-4;
            f.Block("Visible retaining pawl",new Vector3(.108f,.055f,0),new Vector3(.020f,.004f,.026f),a.Rim,null,false);
            // The same ideal retaining pawl as P22 stops releasing the escapement
            // after the useful opening; extra strokes cannot close a cleared route.
            return memory;
        }

        internal static PhysicalHinge Cage(MechanicalAuthoring a,Vector3 origin,Quaternion rotation,bool through=true)
        {
            var f=new Frame(a,origin,rotation);
            var cage=f.Hinge("Campaign independent cage",new Vector3(0,.05f,0),Vector3.forward,.34f,-32,32,.00018f);
            f.Block("Cage floor",new Vector3(0,-.05f,0),new Vector3(.266f,.006f,.266f),a.Glass,cage.transform);
            f.Block("Cage roof",new Vector3(0,.05f,0),new Vector3(.266f,.006f,.266f),a.Glass,cage.transform);
            foreach(int sign in new[]{-1,1})
            {
                f.Block("Cage closed side "+sign,new Vector3(0,0,sign*.13f),new Vector3(.266f,.10f,.006f),a.Glass,cage.transform);
                foreach(int flank in new[]{-1,1})
                    f.Block("Cage mouth cheek "+sign+" "+flank,new Vector3(sign*.13f,0,flank*.087f),new Vector3(.006f,.10f,.092f),a.Glass,cage.transform);
            }
            if(!through) f.Block("Cage closed entry",new Vector3(-.13f,0,0),new Vector3(.006f,.10f,.080f),a.Glass,cage.transform);
            f.Block("Cage counterweight",new Vector3(0,-.064f,0),new Vector3(.15f,.020f,.08f),a.Frame,cage.transform);
            cage.Body.centerOfMass=new Vector3(0,-.045f,0);
            foreach(int sign in new[]{-1,1})
                f.Block("Cage fixed axle "+sign,new Vector3(0,.05f,sign*.20f),new Vector3(.010f,.010f,.13f),a.Frame);
            // Catch pans stop 20 mm outside the entire sweep envelope; their
            // sloping aprons let a missed transfer roll back to the same mouth.
            foreach(int sign in new[]{-1,1})
            {
                f.Block("Cage catch court "+sign,new Vector3(sign*.225f,-.105f,0),new Vector3(.150f,.008f,.24f),a.Frame);
                Ramp(a,f.P(new Vector3(sign*.295f,-.100f,0)),f.P(new Vector3(sign*.18f,-.015f,0)),.10f,a.Frame);
            }
            return cage;
        }

        internal static PhysicalHinge Pendulum(MechanicalAuthoring a,Vector3 origin,Quaternion rotation,float halfWidth=.30f)
        {
            var f=new Frame(a,origin,rotation);
            foreach(int sign in new[]{-1,1})
                f.Block("Pendulum partition "+sign,new Vector3(sign*(halfWidth+.046f)*.5f,0,0),new Vector3(halfWidth-.046f,.274f,.012f),a.Glass);
            f.Block("Pendulum aperture lintel",new Vector3(0,.042f,0),new Vector3(.092f,.190f,.012f),a.Glass);
            var p=f.Hinge("Campaign stable pendulum",new Vector3(0,.10f,-.025f),Vector3.forward,.28f,-68,68,.00010f);
            f.Block("Pendulum rod",new Vector3(0,-.094f,0),new Vector3(.009f,.188f,.012f),a.Frame,p.transform);
            f.Block("Pendulum bob",new Vector3(0,-.190f,0),new Vector3(.112f,.078f,.014f),a.Frame,p.transform);
            p.Body.centerOfMass=new Vector3(0,-.175f,0);return p;
        }

        internal static void Flight(MechanicalAuthoring a,Vector3 origin,Quaternion rotation,float lateralOffset=.025f)
        {
            var f=new Frame(a,origin,rotation);
            Vector3[] path={new Vector3(-.30f,.12f,0),new Vector3(-.25f,.12f,0),new Vector3(-.16f,.06f,0),new Vector3(-.07f,.025f,0),new Vector3(0,.03f,0)};
            for(int i=1;i<path.Length;i++)
            {
                Ramp(a,f.P(path[i-1]),f.P(path[i]),.092f,a.Frame);
                foreach(int sign in new[]{-1,1})
                    Beam(a,"Launch containment rail",f.P(path[i-1]+new Vector3(0,.021f,sign*.05f)),f.P(path[i]+new Vector3(0,.021f,sign*.05f)),.008f,.046f,a.Glass);
            }
            f.Block("Launch rear wall",new Vector3(-.309f,.155f,0),new Vector3(.008f,.07f,.11f),a.Glass);
            // A genuine 90 mm gap. The receiver is broad and 70 mm below takeoff.
            Vector2[] receiverOutline={new Vector2(.09f,lateralOffset-.11f),new Vector2(.34f,lateralOffset-.11f),new Vector2(.34f,lateralOffset+.11f),new Vector2(.09f,lateralOffset+.11f)};
            var receive=BossGeometry.Mesh(a,"Flight receiving court with transfer bore",PhysicsLabGeometry.Panel("Campaign receiving transfer",receiverOutline,-.044f,.004f,new Vector2(.26f,lateralOffset),.04f),a.Frame);
            receive.transform.localPosition=origin;receive.transform.localRotation=rotation;
            foreach(int sign in new[]{-1,1})
                f.Block("Flight receiver side "+sign,new Vector3(.215f,-.065f,lateralOffset+sign*.113f),new Vector3(.25f,.246f,.006f),a.Glass);
            f.Block("Flight braking back",new Vector3(.339f,-.065f,lateralOffset),new Vector3(.008f,.246f,.22f),a.Glass);
            f.Block("Receiver recovery bypass barrier",new Vector3(.086f,-.115f,lateralOffset),new Vector3(.008f,.146f,.22f),a.Glass);
            // The miss floor is close to the outer floor, not a suspended shelf
            // that can trap the ball beneath it. Its eastern wing follows the
            // receiver's outside edge and never plugs the transfer bore.
            f.Block("Flight local recovery floor",new Vector3(-.1175f,-.190f,.055f),new Vector3(.405f,.008f,.410f),a.Frame);
            float southEdge=lateralOffset-.117f;
            f.Block("Flight missed-catch recovery wing",new Vector3(.2175f,-.190f,(-.15f+southEdge)*.5f),new Vector3(.265f,.008f,southEdge+.15f),a.Frame);
            Ramp(a,f.P(new Vector3(-.10f,-.186f,-.105f)),f.P(new Vector3(-.285f,.116f,-.105f)),.085f,a.Frame);
            f.Block("Recovery upper connector",new Vector3(-.285f,.116f,-.055f),new Vector3(.075f,.008f,.13f),a.Frame);
            f.Block("Launch parking connector",new Vector3(-.27f,.116f,.065f),new Vector3(.10f,.008f,.14f),a.Frame);
            Pocket(a,f.P(new Vector3(-.27f,.124f,.12f)),rotation,.09f,.075f,.065f);
        }

        internal static GameObject Ramp(MechanicalAuthoring a,Vector3 from,Vector3 to,float width,Material material)
            =>Beam(a,"Continuous graded support",from,to,width,.008f,material);
        internal static GameObject Beam(MechanicalAuthoring a,string name,Vector3 from,Vector3 to,float width,float thickness,Material material)
        {
            Vector3 d=to-from;var go=a.Block(name,(from+to)*.5f,new Vector3(d.magnitude+.008f,thickness,width),material);
            go.transform.localRotation=Quaternion.FromToRotation(Vector3.right,d.normalized);return go;
        }
        private static Mesh HalfDisc(float radius,float gap,float halfDepth,int sign,float sweepDegrees=0)
        {
            const int segments=20;float start=Mathf.Asin(gap/radius),end=Mathf.PI-start;
            var contour=new List<Vector2>();for(int i=0;i<=segments;i++){float angle=Mathf.Lerp(start,end,i/(float)segments);contour.Add(new Vector2(Mathf.Cos(angle)*radius,sign*Mathf.Sin(angle)*radius));}
            if(sign<0)contour.Reverse();
            if(sweepDegrees>0)
            {
                float angle=sweepDegrees*Mathf.Deg2Rad;
                Vector2 normal=new Vector2(Mathf.Sin(angle),Mathf.Cos(angle))*sign;
                var clipped=new List<Vector2>();
                for(int i=0;i<contour.Count;i++)
                {
                    Vector2 p=contour[i],q=contour[(i+1)%contour.Count];float dp=Vector2.Dot(p,normal)-gap,dq=Vector2.Dot(q,normal)-gap;
                    if(dp>=0)clipped.Add(p);
                    if((dp>=0)!=(dq>=0))clipped.Add(Vector2.Lerp(p,q,dp/(dp-dq)));
                }
                contour=clipped;
            }
            var vertices=new List<Vector3>();var triangles=new List<int>();Vector2 c=Vector2.zero;foreach(var p in contour)c+=p;c/=contour.Count;
            for(int i=0;i<contour.Count;i++){var p=contour[i];var q=contour[(i+1)%contour.Count];Tri(P(c,halfDepth),P(q,halfDepth),P(p,halfDepth));Tri(P(c,-halfDepth),P(p,-halfDepth),P(q,-halfDepth));Tri(P(p,-halfDepth),P(p,halfDepth),P(q,halfDepth));Tri(P(p,-halfDepth),P(q,halfDepth),P(q,-halfDepth));}
            var mesh=new Mesh{name="Campaign convex split cam"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            Vector3 P(Vector2 p,float y)=>new Vector3(p.x,y,p.y);
            void Tri(Vector3 p,Vector3 q,Vector3 r){int n=vertices.Count;vertices.AddRange(new[]{p,q,r});triangles.AddRange(new[]{n,n+1,n+2});}
        }
    }
}
