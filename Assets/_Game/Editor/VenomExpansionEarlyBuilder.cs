using System.Collections.Generic;
using GravityBox.Venom;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static bool BuildExpansion11To14(int number,ExpansionContext c)
        {
            switch(number)
            {
                case 11: BuildExpansion11(c);return true;
                case 12: BuildExpansion12(c);return true;
                case 13: BuildExpansion13(c);return true;
                case 14: BuildExpansion14(c);return true;
                default:return false;
            }
        }

        private static void BuildExpansion11(ExpansionContext c)
        {
            c.Definition.Title="11 · Kê cao lên!";
            c.Definition.Lesson="Kê thùng sát vách, leo qua dải trơn rồi tới lỗ.";
            c.Definition.CanRotate=false;c.Definition.CameraEuler=new Vector3(25,38,0);c.Definition.ViewRadius=.46f;
            c.Spawn=new Vector3(-.20f,-.266f,-.13f);c.Exit=new Vector3(.3f,.105f,.03f);c.Outward=Vector3.right;
            var faces=Cube(c.Root,Vector3.zero,.3f,c.Exit,c.Outward,c.Owner.ApertureRadius,c.Surfaces);
            for(int i=1;i<=4;i++)
            {
                faces[i].HasSlipRegion=true;faces[i].SlipRegion=new Rect(-.3f,-.3f,.6f,.17f);
                Overlay(faces[i],faces[i].SlipRegion,slip);
            }
            var crate=Prop(c.Root,"Amber climbing crate",new Vector3(-.09f,-.205f,.10f),new Vector3(.17f,.19f,.17f),true,plastic,c.Surfaces);
            crate.ProvidesStep=true;crate.Body.mass=.22f;crate.Body.centerOfMass=Vector3.down*.050f;c.Props.Add(crate);
            EarlyLabel(c.Root,"SLIP BAND",new Vector3(.292f,-.205f,-.18f),new Vector3(.008f,.16f,.085f),metal);
        }

        private static void BuildExpansion12(ExpansionContext c)
        {
            c.Definition.Title="12 · Trượt rồi bay!";
            c.Definition.Lesson="Leo lên bệ, trượt theo trọng lực rồi bám vùng đón.";
            c.Definition.CanRotate=false;c.Definition.CameraEuler=new Vector3(20,32,0);c.Definition.ViewRadius=.49f;
            c.Spawn=new Vector3(-.235f,-.265f,.13f);c.Exit=new Vector3(.3f,.015f,0);c.Outward=Vector3.right;
            var faces=Cube(c.Root,Vector3.zero,.3f,c.Exit,c.Outward,c.Owner.ApertureRadius,c.Surfaces);
            for(int i=1;i<6;i++)
            {
                faces[i].Slippery=true;faces[i].Shape.sharedMaterial=slick;faces[i].GetComponent<Renderer>().sharedMaterial=slip;
            }
            var catcher=faces[3];catcher.RingGrip=true;catcher.GripRadius=.135f;
            Ring(catcher.transform,catcher.HoleCentre,catcher.GripRadius,.0017f,mint);

            // A dedicated vertical gripping strip makes the climb a normal
            // surface-crawl transition.  It does not touch the slippery shell.
            Panel(c.Root,"Gripping climb",new Vector3(-.268f,-.070f,0),Vector3.right,new Vector2(.16f,.43f),stone,false,Vector2.zero,0,c.Surfaces);
            // End the grippy deck exactly where the slippery trough begins. An
            // overlap would let the deck keep supplying traction above slide 1.
            Panel(c.Root,"Launch platform",new Vector3(-.1715f,.145f,0),Vector3.up,new Vector2(.193f,.10f),stone,false,Vector2.zero,0,c.Surfaces);
            Vector3[] anchors={new Vector3(-.075f,.145f,0),new Vector3(-.035f,.095f,0),new Vector3(.005f,.015f,0),new Vector3(.055f,-.085f,0),new Vector3(.105f,-.130f,0),new Vector3(.175f,-.130f,0),new Vector3(.220f,-.115f,0)};
            Vector3[] points=EarlySmoothPath(anchors,4);
            var slide=EarlyCurvedTrough(c.Root,points,.17f,.052f,slip,c.Surfaces);
            for(int i=0;i<points.Length-1;i++)
            {
                EarlySlideWall(c.Root,$"Trough wall L {i+1}",points[i],points[i+1],.091f,.052f,.012f,metal,false);
                EarlySlideWall(c.Root,$"Trough wall R {i+1}",points[i],points[i+1],-.091f,.052f,.012f,metal,false);
            }
            var plane=new GameObject("Physical launch plane").transform;plane.SetParent(c.Root,false);plane.localPosition=points[points.Length-1];
            plane.localRotation=Quaternion.LookRotation((points[points.Length-1]-points[points.Length-2]).normalized,Vector3.up);
            var monitor=c.Root.gameObject.AddComponent<COgheSlideLaunchMonitor>();
            monitor.SlideSurfaces=slide.ToArray();monitor.CatchSurface=catcher;monitor.LaunchPlane=plane;
        }

        private static void BuildExpansion13(ExpansionContext c)
        {
            c.Definition.Title="13 · Mở đường!";
            c.Definition.Lesson="Kéo cần A, vào phòng nhấn B, rồi chui qua ống.";
            c.Definition.CanRotate=false;c.Definition.CameraEuler=new Vector3(27,31,0);c.Definition.ViewRadius=.50f;
            c.Spawn=new Vector3(-.205f,-.265f,-.22f);c.Exit=new Vector3(.3f,.10f,.10f);c.Outward=Vector3.right;
            Cube(c.Root,Vector3.zero,.3f,c.Exit,c.Outward,c.Owner.ApertureRadius,c.Surfaces);

            const float front=-.115f,back=.175f,left=-.045f,right=.235f,roof=-.055f;
            Vector3 inlet=new Vector3(.095f,-.165f,back-.008f);
            Panel(c.Root,"Inner room roof",new Vector3((left+right)*.5f,roof,(front+back)*.5f),Vector3.down,new Vector2(right-left,back-front),stone,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Inner room left wall",new Vector3(left,(roof-.3f)*.5f,(front+back)*.5f),Vector3.right,new Vector2(back-front,.3f-roof),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Inner room right wall",new Vector3(right,(roof-.3f)*.5f,(front+back)*.5f),Vector3.left,new Vector2(back-front,.3f-roof),glass,false,Vector2.zero,0,c.Surfaces);
            Vector3 backCentre=new Vector3((left+right)*.5f,(roof-.3f)*.5f,back);
            Quaternion backFrame=Quaternion.LookRotation(Vector3.back,Vector3.up);
            Vector3 inletOnBack=Quaternion.Inverse(backFrame)*(inlet-backCentre);
            Panel(c.Root,"Inner room tube wall",backCentre,Vector3.back,new Vector2(right-left,.3f-roof),glass,true,new Vector2(inletOnBack.x,inletOnBack.y),.043f,c.Surfaces);
            Panel(c.Root,"Door frame left",new Vector3(-.012f,-.18f,front),Vector3.back,new Vector2(.066f,.24f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Door frame right",new Vector3(.19f,-.18f,front),Vector3.back,new Vector2(.09f,.24f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Door frame header",new Vector3(.082f,-.075f,front),Vector3.back,new Vector2(.12f,.03f),glass,false,Vector2.zero,0,c.Surfaces);

            // Run the shutter on the outside track, clear of the stationary
            // frame edges throughout its joint-limited vertical stroke.
            var door=Prop(c.Root,"Door A",new Vector3(.082f,-.20f,front-.020f),new Vector3(.115f,.18f,.014f),false,metal,c.Surfaces);
            door.Body.mass=.14f;door.Manipulable=false;c.Props.Add(door);
            EarlySlider(door.Body,c.Root.GetComponent<Rigidbody>(),door.transform.localPosition,Vector3.up,.145f);

            var lever=Prop(c.Root,"Lever A",new Vector3(-.19f,-.215f,-.17f),new Vector3(.028f,.15f,.028f),true,plastic,c.Surfaces);
            lever.Body.mass=.055f;lever.Body.centerOfMass=Vector3.up*.012f;c.Props.Add(lever);
            var hinge=lever.gameObject.AddComponent<HingeJoint>();hinge.connectedBody=c.Root.GetComponent<Rigidbody>();hinge.autoConfigureConnectedAnchor=false;
            hinge.anchor=Vector3.down*.075f;hinge.connectedAnchor=lever.transform.localPosition+Vector3.down*.075f;hinge.axis=Vector3.forward;
            hinge.useLimits=true;hinge.limits=new JointLimits{min=-7,max=52,bounciness=0,contactDistance=2};
            hinge.useSpring=true;hinge.spring=new JointSpring{spring=.0007f,damper=.0003f,targetPosition=0};hinge.enableCollision=true;
            EarlyLabel(c.Root,"A",new Vector3(-.19f,-.105f,-.172f),new Vector3(.045f,.035f,.008f),mint);

            Vector3 buttonAt=new Vector3(.095f,-.287f,.015f);
            var buttonGo=new GameObject("Latched pressure button B",typeof(Rigidbody),typeof(VenomPressurePlate));buttonGo.transform.SetParent(c.Root,false);buttonGo.transform.localPosition=buttonAt;
            var buttonBody=buttonGo.GetComponent<Rigidbody>();buttonBody.mass=.035f;buttonBody.useGravity=false;buttonBody.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
            var buttonFace=DynamicFace(buttonGo.transform,"Button B cap",Vector3.up*.006f,Vector3.up,new Vector2(.076f,.076f),mint,c.Surfaces);
            buttonFace.Shape.sharedMaterial=stepContact;
            EarlySlider(buttonBody,c.Root.GetComponent<Rigidbody>(),buttonAt,Vector3.down,.008f);
            var button=buttonGo.GetComponent<VenomPressurePlate>();button.Body=buttonBody;button.RestLocal=buttonAt;button.Light=buttonFace.GetComponent<Renderer>();
            buttonGo.transform.SetParent(c.Owner.Apparatus,true);
            EarlyLabel(c.Root,"B",buttonAt+new Vector3(0,.004f,.055f),new Vector3(.04f,.008f,.028f),mint);

            // The compact shutter retracts sideways into the clear pocket
            // beside the inlet, leaving the approach below the bore empty.
            Vector3 lidRest=inlet-Vector3.forward*.012f;
            var lid=Prop(c.Root,"Tube inlet lid B",lidRest,new Vector3(.082f,.082f,.014f),false,plastic,c.Surfaces);
            lid.Body.mass=.10f;lid.Manipulable=false;c.Props.Add(lid);EarlySlider(lid.Body,c.Root.GetComponent<Rigidbody>(),lidRest,Vector3.right,.085f);
            foreach(var face in lid.GetComponentsInChildren<VenomSurfacePatch>()){face.Slippery=true;face.Shape.sharedMaterial=slick;}

            // The winding bore is supplied by the shared 15/16 tube builder.
            // The exact network call is kept in one helper so both mechanisms
            // use the same continuous collider and flow implementation.
            var lidColliders=lid.GetComponentsInChildren<Collider>();
            var blocker=System.Array.Find(lidColliders,shape=>Mathf.Abs(Vector3.Dot(shape.transform.forward,Vector3.forward))>.9f);
            EarlyLevel13Tube(c,inlet,blocker??lidColliders[0]);

            var sequence=c.Root.gameObject.AddComponent<COgheLatchedAccessSequence>();
            sequence.Lever=lever.Body;sequence.LeverJoint=hinge;sequence.Button=button;sequence.RoomDoor=door.Body;sequence.TubeLid=lid.Body;
            sequence.RoomDoorRest=door.transform.localPosition;sequence.TubeLidRest=lid.transform.localPosition;
            sequence.DoorAxis=Vector3.up;sequence.LidAxis=Vector3.right;sequence.DoorTravel=.145f;sequence.LidTravel=.085f;
        }

        private static void BuildExpansion14(ExpansionContext c)
        {
            c.Definition.Title="14 · Đổi chiều!";
            c.Definition.Lesson="Đổi chiều trọng lực: qua cầu, nghỉ an toàn, rồi mở cửa.";
            c.Definition.CanRotate=true;c.Definition.CameraEuler=new Vector3(24,34,0);c.Definition.ViewRadius=.49f;
            c.Spawn=new Vector3(-.18f,-.265f,-.22f);c.Exit=new Vector3(.3f,.07f,.12f);c.Outward=Vector3.right;
            var faces=Cube(c.Root,Vector3.zero,.3f,c.Exit,c.Outward,c.Owner.ApertureRadius,c.Surfaces);
            for(int i=1;i<6;i++){faces[i].Slippery=true;faces[i].Shape.sharedMaterial=slick;faces[i].GetComponent<Renderer>().sharedMaterial=slip;}
            faces[3].RingGrip=true;faces[3].GripRadius=.105f;Ring(faces[3].transform,faces[3].HoleCentre,.105f,.0015f,mint);

            // A closed pedestal gives the climb a real exposed convex crest.
            // Spawn is left of it, so the floor route goes around the left edge
            // before reaching the grippy front face.
            Panel(c.Root,"Safe climb",new Vector3(-.04f,-.134f,-.105f),Vector3.forward,new Vector2(.18f,.332f),stone,false,Vector2.zero,0,c.Surfaces);
            var pedestalBack=Panel(c.Root,"Departure pedestal back",new Vector3(-.04f,-.134f,-.245f),Vector3.back,new Vector2(.18f,.332f),slip,false,Vector2.zero,0,c.Surfaces);
            var pedestalLeft=Panel(c.Root,"Departure pedestal left",new Vector3(-.13f,-.134f,-.175f),Vector3.left,new Vector2(.14f,.332f),slip,false,Vector2.zero,0,c.Surfaces);
            var pedestalRight=Panel(c.Root,"Departure pedestal right",new Vector3(.05f,-.134f,-.175f),Vector3.right,new Vector2(.14f,.332f),slip,false,Vector2.zero,0,c.Surfaces);
            foreach(var side in new[]{pedestalBack,pedestalLeft,pedestalRight}){side.Slippery=true;side.Shape.sharedMaterial=slick;}
            Panel(c.Root,"Bridge departure perch",new Vector3(-.04f,.04f,-.175f),Vector3.up,new Vector2(.18f,.14f),stone,false,Vector2.zero,0,c.Surfaces);
            // Keep a real rail gap beside gate B; the perch must not overlap
            // the closed slider at x=.284..298.
            Panel(c.Root,"Safe resting perch",new Vector3(.034f,.04f,.145f),Vector3.up,new Vector2(.488f,.17f),stone,false,Vector2.zero,0,c.Surfaces);

            Vector3 pivot=new Vector3(-.04f,.035f,-.095f);
            var bridge=Prop(c.Root,"Gravity hinge bridge A",pivot+Vector3.down*.11f,new Vector3(.15f,.22f,.014f),false,plastic,c.Surfaces);
            bridge.Body.mass=.095f;bridge.Body.centerOfMass=Vector3.down*.030f;bridge.Body.solverIterations=28;bridge.Body.solverVelocityIterations=12;bridge.Manipulable=false;c.Props.Add(bridge);
            var hinge=bridge.gameObject.AddComponent<HingeJoint>();hinge.connectedBody=c.Root.GetComponent<Rigidbody>();hinge.autoConfigureConnectedAnchor=false;
            hinge.anchor=Vector3.up*.11f;hinge.connectedAnchor=pivot;hinge.axis=Vector3.right;hinge.useLimits=true;
            hinge.limits=new JointLimits{min=-94,max=4,bounciness=0,contactDistance=2};hinge.enableCollision=true;
            EarlyLabel(c.Root,"A",pivot+new Vector3(-.10f,.015f,0),new Vector3(.035f,.03f,.025f),mint);

            Vector3 gateClosed=new Vector3(.291f,.07f,.12f);
            var gate=Prop(c.Root,"Gravity sliding exit gate B",gateClosed,new Vector3(.014f,.13f,.13f),false,metal,c.Surfaces);
            gate.Body.mass=.12f;gate.Manipulable=false;c.Props.Add(gate);
            EarlyBidirectionalSlider(gate.Body,c.Root.GetComponent<Rigidbody>(),gateClosed,Vector3.forward,Vector3.back,.14f);
            EarlyRail(c.Root,"Gate rail upper",gateClosed+new Vector3(-.012f,.075f,-.07f),Vector3.forward*.27f,.012f,metal);
            EarlyRail(c.Root,"Gate rail lower",gateClosed+new Vector3(-.012f,-.075f,-.07f),Vector3.forward*.27f,.012f,metal);
            EarlyLabel(c.Root,"B",gateClosed+new Vector3(-.015f,.09f,0),new Vector3(.01f,.03f,.035f),mint);

            var mechanism=c.Root.gameObject.AddComponent<COgheGravityBridgeAssembly>();
            mechanism.Bridge=bridge.Body;mechanism.ExitGate=gate.Body;mechanism.GateClosedLocal=gateClosed;
            mechanism.GateAxis=Vector3.forward;mechanism.GateTravel=.14f;mechanism.GateClearance=.105f;
        }

        private static VenomSurfacePatch EarlySlope(Transform root,string name,Vector3 a,Vector3 b,float width,Material material,bool slippery,List<VenomSurfacePatch> surfaces)
        {
            Vector3 tangent=(b-a).normalized,normal=new Vector3(-tangent.y,tangent.x,0).normalized;
            var patch=Panel(root,name,(a+b)*.5f,normal,new Vector2(width,Vector3.Distance(a,b)+.0006f),material,false,Vector2.zero,0,surfaces);
            patch.Slippery=slippery;if(slippery)patch.Shape.sharedMaterial=slick;return patch;
        }

        private static Vector3[] EarlySmoothPath(Vector3[] anchors,int subdivisions)
        {
            var points=new List<Vector3>{anchors[0]};
            for(int segment=0;segment<anchors.Length-1;segment++)
            {
                Vector3 p0=anchors[Mathf.Max(0,segment-1)],p1=anchors[segment];
                Vector3 p2=anchors[segment+1],p3=anchors[Mathf.Min(anchors.Length-1,segment+2)];
                for(int step=1;step<=subdivisions;step++)
                {
                    float t=step/(float)subdivisions,t2=t*t,t3=t2*t;
                    points.Add(.5f*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t2+(-p0+3*p1-3*p2+p3)*t3));
                }
            }
            return points.ToArray();
        }

        private static List<VenomSurfacePatch> EarlyCurvedTrough(Transform root,Vector3[] points,float width,float wallHeight,Material material,List<VenomSurfacePatch> surfaces)
        {
            var patches=new List<VenomSurfacePatch>();
            for(int i=0;i<points.Length-1;i++)
            {
                var patch=EarlySlope(root,$"Slippery trough {i+1}",points[i],points[i+1],width,material,true,surfaces);
                // These fine patches supply local normals, routing and visuals.
                // One mesh below owns every physical contact without seams.
                patch.Shape.enabled=false;patches.Add(patch);
            }

            float half=width*.5f;
            var vertices=new List<Vector3>(points.Length*6);
            for(int i=0;i<points.Length;i++)
            {
                Vector3 before=points[Mathf.Max(0,i-1)],after=points[Mathf.Min(points.Length-1,i+1)];
                Vector3 tangent=(after-before).normalized;
                Vector3 normal=new Vector3(-tangent.y,tangent.x,0).normalized;
                vertices.Add(points[i]+Vector3.back*half);
                vertices.Add(points[i]+Vector3.forward*half);
                vertices.Add(points[i]+Vector3.back*half+normal*wallHeight);
                vertices.Add(points[i]+Vector3.forward*half+normal*wallHeight);
            }
            var triangles=new List<int>((points.Length-1)*18);
            for(int i=0;i<points.Length-1;i++)
            {
                int a=i*4,b=(i+1)*4;
                triangles.AddRange(new[]{a,a+1,b+1,a,b+1,b});
                triangles.AddRange(new[]{a,a+2,b+2,a,b+2,b});
                triangles.AddRange(new[]{a+1,b+3,a+3,a+1,b+1,b+3});
            }
            var mesh=new Mesh{name="Continuous curved trough"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var colliderObject=new GameObject("Continuous curved trough collider",typeof(MeshCollider));colliderObject.transform.SetParent(root,false);
            var shape=colliderObject.GetComponent<MeshCollider>();shape.sharedMesh=Save(mesh);shape.sharedMaterial=slick;shape.contactOffset=.0003f;
            return patches;
        }

        private static void EarlyRail(Transform root,string name,Vector3 centre,Vector3 direction,float thickness,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=centre;
            go.transform.localRotation=direction.sqrMagnitude>.0001f?Quaternion.FromToRotation(Vector3.up,direction.normalized):Quaternion.identity;
            go.transform.localScale=new Vector3(thickness,direction.magnitude+thickness,thickness);go.GetComponent<Renderer>().sharedMaterial=material;
            var shape=go.GetComponent<BoxCollider>();shape.sharedMaterial=contact;shape.contactOffset=.0003f;
        }

        private static void EarlySlideWall(Transform root,string name,Vector3 a,Vector3 b,float side,float height,float thickness,Material material,bool physical=true)
        {
            Vector3 tangent=(b-a).normalized,normal=new Vector3(-tangent.y,tangent.x,0).normalized;
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);
            go.transform.localPosition=(a+b)*.5f+Vector3.forward*side+normal*(height*.5f);
            go.transform.localRotation=Quaternion.LookRotation(tangent,normal);
            go.transform.localScale=new Vector3(thickness,height,Vector3.Distance(a,b)+.008f);
            go.GetComponent<Renderer>().sharedMaterial=material;var shape=go.GetComponent<BoxCollider>();shape.sharedMaterial=contact;shape.contactOffset=.0003f;shape.enabled=physical;
        }

        private static void EarlyLabel(Transform root,string name,Vector3 centre,Vector3 size,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=centre;go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        private static ConfigurableJoint EarlySlider(Rigidbody body,Rigidbody box,Vector3 rest,Vector3 direction,float travel)
        {
            Vector3 axis=direction.normalized;
            var joint=body.gameObject.AddComponent<ConfigurableJoint>();joint.connectedBody=box;joint.autoConfigureConnectedAnchor=false;
            joint.anchor=Vector3.zero;joint.connectedAnchor=rest+axis*(travel*.5f);joint.axis=body.transform.InverseTransformDirection(box.transform.TransformDirection(axis));
            joint.secondaryAxis=Vector3.forward;joint.xMotion=ConfigurableJointMotion.Limited;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;
            joint.linearLimit=new SoftJointLimit{limit=travel*.5f,contactDistance=.001f,bounciness=0};joint.enableCollision=true;return joint;
        }

        private static ConfigurableJoint EarlyBidirectionalSlider(Rigidbody body,Rigidbody box,Vector3 closed,Vector3 jointAxis,Vector3 openDirection,float travel)
        {
            var joint=body.gameObject.AddComponent<ConfigurableJoint>();joint.connectedBody=box;joint.autoConfigureConnectedAnchor=false;
            joint.anchor=Vector3.zero;joint.connectedAnchor=closed+openDirection.normalized*(travel*.5f);
            joint.axis=body.transform.InverseTransformDirection(box.transform.TransformDirection(jointAxis.normalized));joint.secondaryAxis=Vector3.up;
            joint.xMotion=ConfigurableJointMotion.Limited;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;
            joint.linearLimit=new SoftJointLimit{limit=travel*.5f,contactDistance=.001f,bounciness=0};joint.enableCollision=true;return joint;
        }

        private static void EarlyLevel13Tube(ExpansionContext c,Vector3 inlet,Collider gate)
        {
            var nodes=new[]
            {
                new COgheTubeNetwork.Node("Inner inlet",inlet,COgheTubeNetwork.TerminalKind.Entry,Vector3.forward),
                new COgheTubeNetwork.Node("Outer exit",c.Exit,COgheTubeNetwork.TerminalKind.Exit,c.Outward)
            };
            var edges=new[]
            {
                new COgheTubeNetwork.Edge("Winding transfer",0,1,inlet,inlet+Vector3.forward*.060f,
                    new Vector3(.155f,-.085f,.252f),new Vector3(.245f,.065f,.185f),
                    c.Exit-Vector3.right*.035f,c.Exit)
            };
            var tube=TubeNetwork(c.Root,"Winding transfer pipe",nodes,edges,.035f,glass);
            tube.EntryBlocker=gate;tube.EntryClearance=.006f;
        }
    }
}
