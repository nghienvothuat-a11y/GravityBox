using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class MechanicalMemoryBuilder
    {
        public static LevelRuntime Build(Material glass,Material frame,Material rim,PhysicsMaterial contact)
        {
            var a=new MechanicalAuthoring("Mechanical memory",new Vector3(.36f,.060f,.34f),
                new Vector2(.22f,.25f),new Vector3(-.275f,-.038f,-.245f),glass,frame,rim,contact);
            Vector3 pivot=new Vector3(0,0,.045f);
            var cam=a.Hinge("Memory cam and twelve tooth wheel",pivot,Vector3.down,.065f,0,31.5f,.00008f);
            // The rotor is two convex half-discs separated by a real 64 mm slot.
            // It blocks north/south initially, and aligns with both courts at 90°.
            foreach(int sign in new[]{-1,1})
            {
                GameObject half=a.MeshObject("Cam half "+sign,HalfDisc(.094f,.032f,.052f,sign),frame,true,cam.transform);
                half.GetComponent<MeshCollider>().convex=true;
            }
            for(int i=0;i<12;i++)
            {
                float angle=i*30*Mathf.Deg2Rad;
                Vector3 p=new Vector3(Mathf.Sin(angle)*.099f,.045f,Mathf.Cos(angle)*.099f);
                GameObject tooth=a.Block("Physical ratchet tooth "+i,p,new Vector3(.016f,.016f,.015f),frame,cam.transform);
                tooth.transform.localRotation=Quaternion.Euler(0,i*30,0);
            }
            // A high shaft joins both halves without filling the rolling slot.
            a.Block("Cam axle above ball passage",new Vector3(0,.046f,0),new Vector3(.066f,.012f,.025f),frame,cam.transform);
            // The fixed sleeve has only north/south mouths. Without this sleeve
            // the ball could enter the oblique slot after only its first click.
            a.Block("Memory west partition",new Vector3(-.234f,0,.045f),new Vector3(.252f,.114f,.010f),glass);
            a.Block("Memory east partition",new Vector3(.234f,0,.045f),new Vector3(.252f,.114f,.010f),glass);
            for(int side=0;side<2;side++)
            for(int segment=0;segment<24;segment++)
            {
                float angle=(-72+side*180+(segment+.5f)*6)*Mathf.Deg2Rad;
                Vector3 radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                var sleeve=a.Block("Fixed cam sleeve "+side+" "+segment,pivot+radial*.116f,
                    new Vector3(.0135f,.114f,.014f),glass);
                sleeve.transform.localRotation=Quaternion.LookRotation(radial,Vector3.up);
            }

            Vector3 rackAt=new Vector3(-.145f,0,-.100f);
            GameObject rackGo=new GameObject("Ball driven spring rack",typeof(Rigidbody),typeof(PhysicalProp),typeof(ConfigurableJoint));
            rackGo.transform.SetParent(a.Root,false);rackGo.transform.localPosition=rackAt;
            Rigidbody rackBody=rackGo.GetComponent<Rigidbody>();rackBody.mass=.018f;rackBody.useGravity=false;
            rackBody.solverIterations=32;rackBody.solverVelocityIterations=12;
            a.Block("Rack ball contact face",Vector3.zero,new Vector3(.010f,.108f,.062f),glass,rackGo.transform);
            a.Block("Rack face upper rim",new Vector3(0,.053f,0),new Vector3(.011f,.002f,.062f),frame,rackGo.transform,false);
            a.Block("Rack face lower rim",new Vector3(0,-.053f,0),new Vector3(.011f,.002f,.062f),frame,rackGo.transform,false);
            a.Block("Rack linkage above ball",new Vector3(.065f,.046f,0),new Vector3(.140f,.012f,.013f),frame,rackGo.transform);
            var slider=rackGo.GetComponent<ConfigurableJoint>();slider.autoConfigureConnectedAnchor=false;
            slider.connectedBody=a.Root.GetComponent<Rigidbody>();slider.connectedAnchor=rackAt+Vector3.right*.031f;
            slider.axis=Vector3.right;slider.secondaryAxis=Vector3.up;
            slider.xMotion=ConfigurableJointMotion.Limited;slider.yMotion=slider.zMotion=ConfigurableJointMotion.Locked;
            slider.angularXMotion=slider.angularYMotion=slider.angularZMotion=ConfigurableJointMotion.Locked;
            slider.linearLimit=new SoftJointLimit{limit=.031f,contactDistance=.0002f};slider.enableCollision=true;
            slider.xDrive=slider.yDrive=slider.zDrive=new JointDrive();
            slider.angularXDrive=slider.angularYZDrive=slider.slerpDrive=new JointDrive();
            var guide=rackGo.AddComponent<GravitySliderGuide>();guide.Configure(slider,rackAt,Vector3.right,.062f,.057f);

            // This finger is a separate small Rigidbody: it drives the next tooth
            // on the rightward stroke and folds clockwise over it on the return.
            var pawlGo=new GameObject("Hinged rack drive finger",typeof(Rigidbody),typeof(PhysicalProp),typeof(HingeJoint));
            pawlGo.transform.SetParent(a.Root,false);pawlGo.transform.localPosition=new Vector3(-.015f,.045f,-.091f);
            Rigidbody pawlBody=pawlGo.GetComponent<Rigidbody>();pawlBody.mass=.004f;pawlBody.useGravity=false;
            pawlBody.solverIterations=32;pawlBody.solverVelocityIterations=12;
            HingeJoint finger=pawlGo.GetComponent<HingeJoint>();finger.autoConfigureConnectedAnchor=false;
            finger.connectedBody=rackBody;finger.connectedAnchor=pawlGo.transform.localPosition-rackAt;
            finger.axis=Vector3.up;finger.limits=new JointLimits{min=0,max=85,contactDistance=.05f};finger.useLimits=true;
            finger.enableCollision=false;
            a.Block("Ratchet driving finger",new Vector3(0,0,.016f),new Vector3(.008f,.013f,.033f),rim,pawlGo.transform);
            var pawl=pawlGo.AddComponent<PhysicalHinge>();pawl.Configure(finger,.000005f);
            var memory=a.Root.gameObject.AddComponent<MemoryRatchetAssembly>();memory.Cam=cam;memory.Rack=guide;memory.DrivePawl=pawl;
            var springObject=new GameObject("Visible rack return spring",typeof(LineRenderer));springObject.transform.SetParent(a.Root,false);
            var spring=springObject.GetComponent<LineRenderer>();spring.sharedMaterial=rim;spring.useWorldSpace=false;
            spring.positionCount=32;spring.startWidth=spring.endWidth=.0014f;spring.numCornerVertices=2;
            spring.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            springObject.AddComponent<RackSpringVisual>().Configure(a.Root,rackGo.transform,spring,
                new Vector3(-.235f,.048f,-.134f),new Vector3(0,.048f,-.034f));

            // Visible stationary retaining tooth and index scale communicate the
            // ideal unilateral joint catch. They are above the moving wheel.
            a.Block("Retaining pawl housing",new Vector3(.108f,.055f,.045f),new Vector3(.020f,.004f,.026f),rim,null,false);
            a.Block("One tooth escapement keeper",new Vector3(.095f,.055f,.073f),new Vector3(.024f,.004f,.009f),rim,null,false);
            for(int i=0;i<4;i++)
                a.Block("Memory index "+i,new Vector3(.145f+i*.034f,-.0558f,-.115f),new Vector3(.018f,.0004f,.008f),rim,null,false);
            a.Label("PUSH · RETURN · REPEAT",new Vector3(-.105f,-.055f,-.185f),.006f);
            a.Label("90°",new Vector3(.180f,-.055f,.093f),.008f);
            a.Label("CAM",new Vector3(0,.0545f,.045f),.006f);
            return a.Level;
        }

        private static Mesh HalfDisc(float radius,float gap,float halfDepth,int sign)
        {
            const int segments=20;
            float start=Mathf.Asin(gap/radius),end=Mathf.PI-start;
            var contour=new List<Vector2>();
            for(int i=0;i<=segments;i++)
            {float angle=Mathf.Lerp(start,end,i/(float)segments);contour.Add(new Vector2(Mathf.Cos(angle)*radius,sign*Mathf.Sin(angle)*radius));}
            if(sign<0) contour.Reverse();
            var vertices=new List<Vector3>();var triangles=new List<int>();
            Vector2 c=Vector2.zero;foreach(Vector2 p in contour)c+=p;c/=contour.Count;
            for(int i=0;i<contour.Count;i++)
            {
                Vector2 p=contour[i],q=contour[(i+1)%contour.Count];
                Tri(P(c,halfDepth),P(q,halfDepth),P(p,halfDepth));
                Tri(P(c,-halfDepth),P(p,-halfDepth),P(q,-halfDepth));
                Tri(P(p,-halfDepth),P(p,halfDepth),P(q,halfDepth));
                Tri(P(p,-halfDepth),P(q,halfDepth),P(q,-halfDepth));
            }
            var mesh=new Mesh{name="Convex slotted cam half"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            void Tri(Vector3 p,Vector3 q,Vector3 r){int n=vertices.Count;vertices.AddRange(new[]{p,q,r});triangles.AddRange(new[]{n,n+1,n+2});}
            Vector3 P(Vector2 p,float y)=>new Vector3(p.x,y,p.y);
        }
    }
}
