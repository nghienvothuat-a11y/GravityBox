using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class MechanicalHeartBuilder
    {
        public static LevelRuntime Build(Material glass,Material frame,Material rim,PhysicsMaterial contact)
        {
            LevelRuntime level=BalanceMachineBuilder.Build(glass,frame,rim,contact);
            level.name="Mechanical heart";
            var a=new MechanicalAuthoring(level,glass,frame,rim,contact);
            var heart=level.gameObject.AddComponent<MechanicalHeart>();
            heart.BridgeCatch=level.GetComponentInChildren<ContactSeatLatch>(); heart.BridgeCatch.Armed=false;
            // The boss must keep A loaded while B is steered in the opposite direction.
            // A real 8 mm recess provides this memory without freezing or attracting the ball.
            Transform carrier=heart.BridgeCatch.Hinge.transform.Find("Balanced carrier frame");
            Object.DestroyImmediate(carrier.Find("A long arm floor").gameObject);
            a.Block("A holding pocket outer lip",new Vector3(-.233f,0,-.055f),new Vector3(.014f,.006f,.070f),frame,carrier);
            a.Block("A holding pocket recessed floor",new Vector3(-.205f,-.008f,-.055f),new Vector3(.042f,.006f,.070f),frame,carrier);
            a.Block("A holding pocket inner approach",new Vector3(-.072f,0,-.055f),new Vector3(.224f,.006f,.070f),frame,carrier);
            carrier.Find("Long arm mark").localPosition=new Vector3(-.205f,-.0046f,-.055f);
            carrier.Find("Long arm mark").localScale=new Vector3(.032f,.0004f,.040f);
            Transform receiver=heart.BridgeCatch.Receiver.transform;
            receiver.localPosition=new Vector3(-.234f,-.011f,-.055f); receiver.localScale=new Vector3(.010f,.020f,.070f);
            // The previous final bore is now an internal transfer: no assist and
            // no completion tracking is attached to this aperture.
            GameObject transfer=level.Exit.gameObject; transfer.name="Inner module transfer bore";
            heart.TransferBore=transfer.transform;
            Object.DestroyImmediate(level.Exit); level.Exit=null;
            foreach(string part in new[]{"Floor with circular cut","Clear top cover","Clear side walls","Lower machined edge","Upper machined edge"})
                level.transform.Find(part).name="Upper module "+part;
            level.transform.Find("Upper module Floor with circular cut").GetComponent<Renderer>().sharedMaterial=glass;
            const float radius=.50f,thickness=.006f,aperture=.023f;
            var shell=level.gameObject.AddComponent<SphericalEnclosure>();shell.InnerRadius=radius;shell.ShellThickness=thickness;
            shell.ShellCollider=a.MeshObject("Continuous heart sphere",SphereMazeGeometry.Shell(radius,thickness,aperture,"Mechanical heart spherical shell"),glass).GetComponent<MeshCollider>();
            level.BoundsHalfExtent=.57f;level.InteriorDepth=(radius+thickness)*2;
            level.Footprint=new Vector2[96];
            for(int i=0;i<96;i++){float angle=i*Mathf.PI*2/96;level.Footprint[i]=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;}
            float innerCut=Mathf.Sqrt(radius*radius-aperture*aperture),outerCut=Mathf.Sqrt((radius+thickness)*(radius+thickness)-aperture*aperture);
            var exit=new GameObject("Flush round exit",typeof(ExitSocket));exit.transform.SetParent(a.Root,false);
            exit.transform.localPosition=Vector3.down*((innerCut+outerCut)*.5f);
            exit.transform.localRotation=Quaternion.LookRotation(Vector3.down,Vector3.forward);
            level.Exit=exit.GetComponent<ExitSocket>();level.Exit.ApertureRadius=aperture;level.Exit.WallHalfDepth=(outerCut-innerCut)*.5f;
            a.MeshObject("Subtle light inlay",PhysicsLabGeometry.Inlay(aperture,level.Exit.WallHalfDepth),rim,false,exit.transform);

            // B can reach this spring contact only beyond the raised short arm.
            var plunger=new GameObject("B mechanical catch release",typeof(Rigidbody),typeof(PhysicalProp),typeof(ConfigurableJoint));
            plunger.transform.SetParent(a.Root,false);Vector3 pressAt=new Vector3(.235f,.027f,.082f);plunger.transform.localPosition=pressAt;
            Rigidbody pressBody=plunger.GetComponent<Rigidbody>();pressBody.mass=.020f;pressBody.useGravity=false;pressBody.solverIterations=32;pressBody.solverVelocityIterations=12;
            a.Block("Release contact face",Vector3.zero,new Vector3(.080f,.044f,.008f),rim,plunger.transform);
            var joint=plunger.GetComponent<ConfigurableJoint>();joint.autoConfigureConnectedAnchor=false;joint.connectedBody=level.GetComponent<Rigidbody>();
            joint.connectedAnchor=pressAt+Vector3.forward*.004f;joint.axis=Vector3.forward;joint.secondaryAxis=Vector3.up;
            joint.xMotion=ConfigurableJointMotion.Limited;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;
            joint.linearLimit=new SoftJointLimit{limit=.004f,contactDistance=.0002f};joint.enableCollision=false;
            var guide=plunger.AddComponent<GravitySliderGuide>();guide.Configure(joint,pressAt,Vector3.forward,.008f,.003f);
            heart.ReleasePlunger=plunger.AddComponent<PressurePlunger>();heart.ReleasePlunger.Guide=guide;
            a.Block("Release pocket outer cheek",new Vector3(.282f,.027f,.054f),new Vector3(.008f,.045f,.090f),glass);
            a.Block("Release pocket inner cheek",new Vector3(.205f,.027f,.066f),new Vector3(.008f,.045f,.066f),glass);
            a.Label("B: SECURE A",new Vector3(.235f,.004f,.025f),.006f);

            Vector3 pivot=new Vector3(0,-.285f,-.130f);
            PhysicalHinge cage=a.Hinge("Heart suspended transfer cage",pivot,Vector3.right,.20f,-24,24,.00012f);heart.Cage=cage;
            a.Block("Heart cage floor",new Vector3(0,-.05f,0),new Vector3(.186f,.006f,.226f),glass,cage.transform);
            a.Block("Heart cage left",new Vector3(-.09f,0,0),new Vector3(.006f,.10f,.226f),glass,cage.transform);
            a.Block("Heart cage right",new Vector3(.09f,0,0),new Vector3(.006f,.10f,.226f),glass,cage.transform);
            a.Block("Heart cage back",new Vector3(0,0,-.11f),new Vector3(.186f,.10f,.006f),glass,cage.transform);
            a.Block("Heart mouth left",new Vector3(-.063f,0,.11f),new Vector3(.060f,.10f,.006f),glass,cage.transform);
            a.Block("Heart mouth right",new Vector3(.063f,0,.11f),new Vector3(.060f,.10f,.006f),glass,cage.transform);
            // Four roof pieces frame the inlet below the upper module's bore.
            a.Block("Heart inlet left roof",new Vector3(-.064f,.05f,0),new Vector3(.058f,.006f,.226f),glass,cage.transform);
            a.Block("Heart inlet right roof",new Vector3(.064f,.05f,0),new Vector3(.058f,.006f,.226f),glass,cage.transform);
            // The inlet remains wide enough for a 30 mm ball with the cage on either stop.
            a.Block("Heart inlet front roof",new Vector3(0,.05f,.0615f),new Vector3(.070f,.006f,.103f),glass,cage.transform);
            a.Block("Heart inlet back roof",new Vector3(0,.05f,-.109f),new Vector3(.070f,.006f,.008f),glass,cage.transform);
            a.Block("Heart counterweight keel",new Vector3(0,-.063f,0),new Vector3(.10f,.019f,.060f),frame,cage.transform);
            cage.Body.centerOfMass=new Vector3(0,-.043f,0);
            var inlet=new GameObject("Heart cage inlet");inlet.transform.SetParent(cage.transform,false);inlet.transform.localPosition=new Vector3(0,.05f,-.055f);heart.CageInlet=inlet.transform;
            var mouth=new GameObject("Heart cage mouth");mouth.transform.SetParent(cage.transform,false);mouth.transform.localPosition=new Vector3(0,-.025f,.114f);heart.CageMouth=mouth.transform;
            a.Block("Heart axle left",pivot+Vector3.left*.14f,new Vector3(.09f,.008f,.008f),frame);
            a.Block("Heart axle right",pivot+Vector3.right*.14f,new Vector3(.09f,.008f,.008f),frame);
            // Catching is a real unsupported drop. The sealed funnel prevents a
            // missed ball from simply rolling down the outside shell to the exit.
            var outline=new[]{new Vector2(-.14f,-.10f),new Vector2(.14f,-.10f),new Vector2(.14f,.22f),new Vector2(-.14f,.22f)};
            a.MeshObject("Heart receiving tray",PhysicsLabGeometry.Panel("Heart receiver deck",outline,-.402f,.003f,Vector2.zero,.053f),glass);
            a.Block("Receiver front frame",new Vector3(0,-.398f,.218f),new Vector3(.28f,.002f,.002f),frame,null,false);
            a.Block("Receiver rear frame",new Vector3(0,-.398f,-.098f),new Vector3(.28f,.002f,.002f),frame,null,false);
            a.Block("Receiver left frame",new Vector3(-.138f,-.398f,.060f),new Vector3(.002f,.002f,.32f),frame,null,false);
            a.Block("Receiver right frame",new Vector3(.138f,-.398f,.060f),new Vector3(.002f,.002f,.32f),frame,null,false);
            a.Block("Receiver left cheek",new Vector3(-.138f,-.380f,.06f),new Vector3(.004f,.050f,.32f),glass);
            a.Block("Receiver right cheek",new Vector3(.138f,-.380f,.06f),new Vector3(.004f,.050f,.32f),glass);
            a.Block("Receiver end cheek",new Vector3(0,-.380f,.218f),new Vector3(.28f,.050f,.004f),glass);
            a.Block("Receiver rear cheek",new Vector3(0,-.380f,-.098f),new Vector3(.28f,.050f,.004f),glass);
            a.MeshObject("Sealed exit funnel",Funnel(-.401f,-innerCut,.053f,aperture),glass);
            a.Label("CATCH BOTH",new Vector3(0,-.397f,.102f),.006f);
            return level;
        }
        private static Mesh Funnel(float top,float bottom,float topRadius,float bottomRadius)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();const int n=64;
            for(int i=0;i<n;i++)
            {
                float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;
                Vector3 T(float angle,float r,float y)=>new Vector3(Mathf.Cos(angle)*r,y,Mathf.Sin(angle)*r);
                Quad(T(a,topRadius,top),T(b,topRadius,top),T(b,bottomRadius,bottom),T(a,bottomRadius,bottom));
                Quad(T(a,bottomRadius+.004f,bottom),T(b,bottomRadius+.004f,bottom),T(b,topRadius+.004f,top),T(a,topRadius+.004f,top));
                Quad(T(a,topRadius+.004f,top),T(b,topRadius+.004f,top),T(b,topRadius,top),T(a,topRadius,top));
                Quad(T(a,bottomRadius,bottom),T(b,bottomRadius,bottom),T(b,bottomRadius+.004f,bottom),T(a,bottomRadius+.004f,bottom));
            }
            var mesh=new Mesh{name="Physical receiver funnel"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int k=vertices.Count;vertices.AddRange(new[]{a,b,c,d});triangles.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
        }
    }
}
