using System.Collections.Generic;
using GravityBox.Venom;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static bool BuildExpansion17To20(int number, ExpansionContext c)
        {
            if (number < 17 || number > 20) return false;
            c.Definition.Title=number+" · "+(number==17?"Nối bánh răng":number==18?"Ghép đường":number==19?"Cùng nhau":"TAM HỢP");
            c.Definition.Lesson=number==17?"Chạm tay nắm, rồi chỉ hướng để bạn nhỏ đẩy hoặc kéo giá bánh răng.":
                number==18?"Xoay hộp để trọng lực ghép ba thanh cầu. Giữ tư thế khi đi qua.":
                number==19?"Chọn từng phần để phối hợp. Khi gặp nhau, các phần tự hợp thể.":"";
            c.Definition.CanRotate = number == 18;
            c.Definition.Boss = number == 20;
            c.Definition.CameraEuler = number==20 ? new Vector3(40,12,0) : number==19 ? new Vector3(40,18,0) : new Vector3(32,18,0);
            c.Definition.ViewRadius = number == 20 ? .68f : number == 19 ? .57f : .50f;
            c.Definition.CameraZones=number==19?new[]{
                new VenomCameraZone("Khoang 1",new Vector3(-.24f,-.06f,0),new Vector3(.516f,.516f,.636f)),
                new VenomCameraZone("Khoang 2",new Vector3(.24f,-.06f,0),new Vector3(.516f,.516f,.636f))}:
                number==20?new[]{
                new VenomCameraZone("Khoang 1",new Vector3(-.4f,-.06f,0),new Vector3(.436f,.516f,.636f)),
                new VenomCameraZone("Khoang 2",new Vector3(0,-.06f,0),new Vector3(.436f,.516f,.636f)),
                new VenomCameraZone("Khoang 3",new Vector3(.4f,-.06f,0),new Vector3(.436f,.516f,.636f))}:System.Array.Empty<VenomCameraZone>();
            c.Outward = Vector3.right;
            c.Exit = new Vector3(number == 20 ? .60f : number == 19 ? .48f : .30f, -.19f, .16f);
            c.Spawn = new Vector3(number == 20 ? -.48f : number == 19 ? -.35f : -.22f, -.267f, -.17f);
            if (number == 18) { c.Exit = new Vector3(.30f,-.105f,0); c.Spawn = new Vector3(-.255f,-.084f,0); }
            ExpansionShell(c, number == 20 ? .60f : number == 19 ? .48f : .30f);
            if (number == 17) BuildGearLesson(c);
            if (number == 18) BuildGravityBridge(c);
            if (number == 19) BuildTogether(c);
            if (number == 20) BuildThreeRoles(c);
            return true;
        }

        private static void ExpansionShell(ExpansionContext c, float halfX)
        {
            Panel(c.Root,"Laboratory floor",new Vector3(0,-.30f,0),Vector3.up,new Vector2(halfX*2,.60f),stone,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Laboratory ceiling",new Vector3(0,.30f,0),Vector3.down,new Vector2(halfX*2,.60f),glass,false,Vector2.zero,0,c.Surfaces).InterceptExterior=true;
            Panel(c.Root,"Laboratory front",new Vector3(0,0,-.30f),Vector3.forward,new Vector2(halfX*2,.60f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Laboratory rear",new Vector3(0,0,.30f),Vector3.back,new Vector2(halfX*2,.60f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Laboratory left",new Vector3(-halfX,0,0),Vector3.right,new Vector2(.60f,.60f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Laboratory outlet wall",new Vector3(halfX,0,0),Vector3.left,new Vector2(.60f,.60f),glass,true,
                new Vector2(c.Exit.z,c.Exit.y),c.Owner.ApertureRadius,c.Surfaces);
        }

        private static Transform MechanismVisual(Transform parent,string name,Vector3 position,Vector3 size,Material material,PrimitiveType type=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go.transform;
        }

        private static COgheRailSlider ExpansionRail(ExpansionContext c,string name,Vector3 start,Vector3 axis,float travel,float initial,Vector3 size,float mass,float resistance,bool gravity,bool handle)
        {
            var prop=Prop(c.Root,name,start+axis*initial,size,handle,plastic,c.Surfaces);c.Props.Add(prop);
            prop.Body.mass=mass;prop.Body.maxDepenetrationVelocity=.20f;
            var rail=prop.gameObject.AddComponent<COgheRailSlider>();rail.Body=prop.Body;rail.Frame=c.Root;rail.Start=start;rail.Axis=axis;rail.Travel=travel;rail.InitialTravel=initial;rail.Resistance=resistance;rail.Gravity=gravity;
            var joint=prop.gameObject.AddComponent<ConfigurableJoint>();rail.Joint=joint;joint.connectedBody=c.Root.GetComponent<Rigidbody>();joint.autoConfigureConnectedAnchor=false;
            joint.anchor=Vector3.zero;joint.connectedAnchor=start+axis*travel*.5f;joint.axis=axis;joint.secondaryAxis=Mathf.Abs(axis.y)>.9f?Vector3.forward:Vector3.up;
            joint.xMotion=ConfigurableJointMotion.Limited;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;
            joint.linearLimit=new SoftJointLimit{limit=travel*.5f,contactDistance=.0005f};joint.enableCollision=true;
            Vector3 cross=Vector3.Cross(axis,Vector3.forward).normalized;if(cross.sqrMagnitude<.1f)cross=Vector3.right;
            for(int s=-1;s<=1;s+=2)
            {
                var guide=MechanismVisual(c.Root,name+" fixed guide",start+axis*travel*.5f+cross*(size.magnitude*.25f+.012f),new Vector3(.006f,travel+.04f,.006f),metal);
                guide.localPosition=start+axis*travel*.5f+cross*s*(size.magnitude*.25f+.012f);guide.localRotation=Quaternion.FromToRotation(Vector3.up,axis);
            }
            foreach(float end in new[]{0f,travel})MechanismVisual(c.Root,name+" rail stop",start+axis*end,new Vector3(.022f,.008f,.015f),metal);
            if(handle)MechanismVisual(prop.transform,name+" stationary handle",new Vector3(0,0,-size.z*.5f-.008f),new Vector3(.046f,.012f,.012f),metal);
            return rail;
        }

        private static Transform ExpansionWheel(Transform parent,string name,Vector3 centre,float radius,int teeth,float phase)
        {
            var wheel=new GameObject(name).transform;wheel.SetParent(parent,false);wheel.localPosition=centre;wheel.localRotation=Quaternion.Euler(0,0,phase);
            var disc=MechanismVisual(wheel,"Brass gear web",Vector3.zero,new Vector3(radius*1.76f,.009f,radius*1.76f),plastic,PrimitiveType.Cylinder);disc.localRotation=Quaternion.Euler(90,0,0);
            for(int i=0;i<teeth;i++)
            {
                float a=i*Mathf.PI*2/teeth;var tooth=MechanismVisual(wheel,"Tooth",new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius,
                    new Vector3(radius*.20f,radius*.18f,.012f),plastic);tooth.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg);
            }
            var bearing=MechanismVisual(wheel,"Silver bearing",Vector3.back*.009f,new Vector3(.017f,.009f,.017f),metal,PrimitiveType.Cylinder);bearing.localRotation=Quaternion.Euler(90,0,0);
            return wheel;
        }

        private static void BuildGearLesson(ExpansionContext c)
        {
            var a=ExpansionRail(c,"A sliding bearing",new Vector3(-.06f,-.18f,.11f),Vector3.up,.16f,0,new Vector3(.064f,.035f,.038f),.06f,.035f,false,true);
            var b=ExpansionRail(c,"B sliding bearing",new Vector3(.06f,-.02f,.11f),Vector3.up,.15f,.15f,new Vector3(.064f,.035f,.038f),.06f,.035f,false,true);
            a.LatchAtEnd=true;b.LatchAtStart=true;
            MechanismVisual(a.transform,"A terminal bearing catch",new Vector3(-.035f,0,.004f),new Vector3(.012f,.018f,.014f),metal);
            MechanismVisual(b.transform,"B terminal bearing catch",new Vector3(.035f,0,.004f),new Vector3(.012f,.018f,.014f),metal);
            var rack=ExpansionRail(c,"Exit rack shutter",new Vector3(.292f,-.19f,.16f),Vector3.up,.17f,0,new Vector3(.012f,.12f,.125f),.035f,.012f,false,false);
            var train=new GameObject("Four wheel transmission",typeof(COgheGearTrain)).GetComponent<COgheGearTrain>();train.transform.SetParent(c.Root,false);
            train.Wheels=new[]{ExpansionWheel(c.Root,"Motor source",new Vector3(-.18f,-.02f,.165f),.06f,20,0),
                ExpansionWheel(a.transform,"Gear A",new Vector3(0,0,.055f),.06f,20,9),
                ExpansionWheel(b.transform,"Gear B",new Vector3(0,0,.055f),.06f,20,0),
                ExpansionWheel(c.Root,"Output",new Vector3(.18f,-.02f,.165f),.06f,20,9)};
            train.PitchRadii=new[]{.06f,.06f,.06f,.06f};train.ToothCounts=new[]{20,20,20,20};train.Rack=rack;
            MechanismVisual(rack.transform,"Rack teeth carrier",new Vector3(-.05f,.08f,.005f),new Vector3(.009f,.24f,.014f),metal);
            for(int i=0;i<16;i++)MechanismVisual(rack.transform,"Rack tooth",new Vector3(-.056f,-.02f+i*.014f,.005f),new Vector3(.015f,.007f,.014f),metal);
            // A climbable clear guard reaches the floor. Bearings/handles sit in front; the tooth train sits behind.
            Panel(c.Root,"Gear safety glass and climb backing",new Vector3(0,-.02f,.145f),Vector3.back,new Vector2(.52f,.56f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"A fixed grip cheek",new Vector3(-.151f,-.02f,.068f),Vector3.right,new Vector2(.22f,.56f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"B fixed grip cheek",new Vector3(.151f,-.02f,.068f),Vector3.left,new Vector2(.22f,.56f),glass,false,Vector2.zero,0,c.Surfaces);
        }

        private static void BuildGravityBridge(ExpansionContext c)
        {
            foreach(var surface in c.Surfaces){surface.Slippery=true;surface.Shape.sharedMaterial=slick;surface.GetComponent<Renderer>().sharedMaterial=slip;}
            Panel(c.Root,"Start grip island",new Vector3(-.255f,-.12f,0),Vector3.up,new Vector2(.09f,.10f),stone,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Exit grip island",new Vector3(.255f,-.12f,0),Vector3.up,new Vector2(.09f,.10f),stone,false,Vector2.zero,0,c.Surfaces);
            // Two-millimetre assembly seams give the perpendicular sliders
            // clearance beyond the solver's contact skin; 18 mm tissue keeps
            // simultaneous physical contact with both banks of each seam.
            Vector3 bar=new Vector3(.138f,.024f,.10f);
            ExpansionRail(c,"A crosswise bridge",new Vector3(-.14f,-.132f,0),Vector3.right,.12f,.12f,bar,.10f,.012f,true,false);
            ExpansionRail(c,"B depth bridge",new Vector3(0,-.132f,0),Vector3.forward,.16f,.16f,bar,.10f,.012f,true,false);
            ExpansionRail(c,"C lifting bridge",new Vector3(.14f,-.132f,0),Vector3.up,.17f,.17f,bar,.10f,.012f,true,false);
            // At the lower stops all interfaces share real edges; no component ever latches the bridges.
        }

        private static COgheTissueSensor ExpansionPad(ExpansionContext c,string name,Vector3 point,float threshold)
        {
            var panel=Panel(c.Root,name+" sensing surface",point,Vector3.up,new Vector2(.084f,.084f),stone,false,Vector2.zero,0,c.Surfaces);
            var root=new GameObject(name+" measured tissue load",typeof(COgheTissueSensor)).GetComponent<COgheTissueSensor>();root.transform.SetParent(c.Root,false);root.transform.localPosition=point;root.Threshold=threshold;root.Size=new Vector2(.084f,.084f);
            root.Cap=MechanismVisual(root.transform,"Amber cap",Vector3.zero,new Vector3(.075f,.004f,.075f),plastic,PrimitiveType.Cylinder);
            root.Indicator=MechanismVisual(root.transform,"Load indicator",new Vector3(.042f,.005f,0),new Vector3(.008f,.006f,.014f),metal).GetComponent<Renderer>();
            return root;
        }

        private static COgheGuillotine ExpansionKnife(ExpansionContext c,string name,float x,float z)
        {
            var rail=ExpansionRail(c,name,new Vector3(x,-.225f,z),Vector3.up,.15f,.15f,new Vector3(.006f,.12f,.12f),.045f,0,true,false);
            foreach(var face in rail.GetComponentsInChildren<Renderer>())face.sharedMaterial=metal;
            // Like the Origin knife, the honed cutting volume protrudes below the
            // blunt collision proxy. A full-height box would push tissue away
            // before any actual bond could intersect the cutting edge.
            foreach(var face in rail.GetComponentsInChildren<VenomSurfacePatch>())
            {c.Surfaces.Remove(face);Object.DestroyImmediate(face.Shape);Object.DestroyImmediate(face);}
            var bladeCollider=rail.gameObject.AddComponent<BoxCollider>();bladeCollider.size=new Vector3(.006f,.084f,.12f);bladeCollider.center=Vector3.up*.018f;bladeCollider.sharedMaterial=contact;bladeCollider.contactOffset=.0003f;
            var knife=new GameObject(name+" controller",typeof(COgheGuillotine)).GetComponent<COgheGuillotine>();knife.transform.SetParent(c.Root,false);knife.Rail=rail;
            knife.Sensor=new GameObject("Actual tissue trigger").transform;knife.Sensor.SetParent(c.Root,false);knife.Sensor.localPosition=new Vector3(x,-.274f,z);
            knife.Lamps=new Renderer[4];for(int i=0;i<4;i++)knife.Lamps[i]=MechanismVisual(c.Root,"One second warning",new Vector3(x-.025f+i*.017f,.09f,z),new Vector3(.010f,.010f,.010f),metal).GetComponent<Renderer>();
            return knife;
        }

        private static COgheRailSlider ExpansionDivider(ExpansionContext c,float x,bool knifePassage)
        {
            // Two real faces seal both directions; only the authored ports and door bores are absent.
            foreach(float side in new[]{-1f,1f})
            {
                Vector3 normal=Vector3.right*side;float px=x+side*.006f;
                void Wall(string name,float z,float width,float y,float height,bool hole=false)
                {
                    var rotation=Quaternion.LookRotation(normal,Vector3.up);Vector3 hc=Quaternion.Inverse(rotation)*(new Vector3(px,.075f,-.19f)-new Vector3(px,y,z));
                    Panel(c.Root,name,new Vector3(px,y,z),normal,new Vector2(width,height),glass,hole,new Vector2(hc.x,hc.y),.025f,c.Surfaces);
                }
                if(knifePassage)
                {Wall("Knife passage header",-.19f,.22f,.08f,.44f);Wall("Front divider edge",-.286f,.028f,-.23f,.14f);Wall("Knife passage jamb",-.094f,.028f,-.23f,.14f);}
                else Wall("High transfer aperture",-.19f,.22f,0,.60f,true);
                Wall("Solid middle divider",0,.16f,0,.60f);
                Wall("Reunion door header",.17f,.18f,.075f,.45f);
                Wall("Rear divider edge",.28f,.04f,0,.60f);
            }
            // The shutter runs along the left side of the divider, into a clear
            // overhead pocket. A coplanar plate would collide with its own header.
            return ExpansionRail(c,"Reunion gate",new Vector3(x-.025f,-.225f,.17f),Vector3.up,.17f,0,new Vector3(.020f,.15f,.18f),.035f,.015f,true,false);
        }

        private static void ExpansionLinkage(ExpansionContext c,COgheCooperativeWinch winch)
        {
            var pinRoot=new GameObject("Mechanical interlock pin").transform;pinRoot.SetParent(winch.FinalCap!=null?winch.FinalCap.transform:winch.Handle.transform,false);
            winch.LockPin=MechanismVisual(pinRoot,"Retracting steel pin",new Vector3(0,.04f,0),new Vector3(.033f,.009f,.009f),metal);
            winch.Drum=MechanismVisual(c.Root,"Winch drum",new Vector3(.15f,.17f,.20f),new Vector3(.052f,.025f,.052f),metal,PrimitiveType.Cylinder);
            foreach(var gate in winch.Doors)
            {
                Vector3 a=new Vector3(gate.Start.x,.20f,gate.Start.z),b=new Vector3(.15f,.20f,.20f),d=b-a;
                var cable=MechanismVisual(c.Root,"Overhead winch cable",(a+b)*.5f,new Vector3(.003f,d.magnitude,.003f),metal);cable.localRotation=Quaternion.FromToRotation(Vector3.up,d.normalized);
                MechanismVisual(gate.transform,"Gate lift cable",Vector3.up*.16f,new Vector3(.003f,.32f,.003f),metal);
            }
        }

        private static void BuildTogether(ExpansionContext c)
        {
            var gate=ExpansionDivider(c,0,true);
            var knife=ExpansionKnife(c,"Divider gravity knife",-.03f,-.19f);
            knife.TouchHalfSize=new Vector3(.030f,.10f,.08f);
            var pad=ExpansionPad(c,"A",new Vector3(-.32f,-.297f,.16f),.012f);
            var handle=ExpansionRail(c,"B pulling handle",new Vector3(.28f,-.255f,-.16f),Vector3.right,.055f,0,new Vector3(.045f,.052f,.045f),.10f,.04f,false,true);
            var exit=ExpansionRail(c,"Outlet shutter",new Vector3(.470f,-.19f,.16f),Vector3.up,.16f,0,new Vector3(.012f,.12f,.12f),.035f,.015f,true,false);
            var winch=new GameObject("A lock B cable winch",typeof(COgheCooperativeWinch)).GetComponent<COgheCooperativeWinch>();winch.transform.SetParent(c.Root,false);
            winch.Input=pad;winch.Handle=handle;winch.HandleForce=.065f;winch.DoorSpeed=.10f;winch.Doors=new[]{gate,exit};ExpansionLinkage(c,winch);
        }

        private static void BuildThreeRoles(ExpansionContext c)
        {
            var left=ExpansionDivider(c,-.20f,false);var right=ExpansionDivider(c,.20f,false);
            ExpansionKnife(c,"I gravity knife",-.41f,-.16f);ExpansionKnife(c,"II gravity knife",0,-.17f);
            var a=ExpansionPad(c,"A input clutch",new Vector3(-.46f,-.297f,.16f),.018f);
            var b=ExpansionPad(c,"B output clutch",new Vector3(.08f,-.297f,.17f),.015f);
            var g=ExpansionRail(c,"G heavy gear carriage",new Vector3(0,-.17f,.08f),Vector3.up,.12f,0,new Vector3(.050f,.038f,.04f),.032f,.055f,true,true);g.LatchAtEnd=true;
            var gears=new GameObject("Boss three wheel transmission",typeof(COgheGearTrain)).GetComponent<COgheGearTrain>();gears.transform.SetParent(c.Root,false);gears.InputClutch=a;
            gears.Wheels=new[]{ExpansionWheel(c.Root,"Input source",new Vector3(-.09f,-.05f,.145f),.045f,18,0),ExpansionWheel(g.transform,"G intermediate",new Vector3(0,0,.065f),.045f,18,10),ExpansionWheel(c.Root,"Output clutch",new Vector3(.09f,-.05f,.145f),.045f,18,0)};gears.PitchRadii=new[]{.045f,.045f,.045f};gears.ToothCounts=new[]{18,18,18};
            Panel(c.Root,"G climb backing and gear guard",new Vector3(0,-.13f,.125f),Vector3.back,new Vector2(.12f,.34f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"Fixed wheel safety guard",new Vector3(0,-.05f,.125f),Vector3.back,new Vector2(.30f,.13f),glass,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"G fixed foot brace",new Vector3(0,-.13f,-.009f),Vector3.forward,new Vector2(.12f,.34f),glass,false,Vector2.zero,0,c.Surfaces);
            var handle=ExpansionRail(c,"C winch handle",new Vector3(.36f,-.254f,-.16f),Vector3.right,.055f,0,new Vector3(.04f,.055f,.045f),.10f,.025f,false,true);
            // H has a real static-friction threshold. A sufficiently large uneven fragment can move it; merged-body escape is separate.
            var cap=ExpansionRail(c,"H heavy final cover",new Vector3(.589f,-.19f,.16f),Vector3.back,.145f,0,new Vector3(.012f,.13f,.13f),.20f,.57f,false,true);cap.LatchAtEnd=true;
            var winch=new GameObject("Three room cooperative winch",typeof(COgheCooperativeWinch)).GetComponent<COgheCooperativeWinch>();winch.transform.SetParent(c.Root,false);
            winch.Input=a;winch.Output=b;winch.Transmission=gears;winch.GearCarriage=g;winch.Handle=handle;winch.FinalCap=cap;winch.Doors=new[]{left,right};ExpansionLinkage(c,winch);
            ExpansionBossPipes(c);
        }

        private static void ExpansionBossPipes(ExpansionContext c)
        {
            foreach(float x in new[]{-.20f,.20f})
            {
                Vector3 inlet=new Vector3(x-.065f,.075f,-.19f),outlet=new Vector3(x+.065f,.075f,-.19f);
                TubeNetwork(c.Root,"Bidirectional high transfer "+x,new[]{
                    new COgheTubeNetwork.Node("Left mouth",inlet,COgheTubeNetwork.TerminalKind.Entry,Vector3.left),
                    new COgheTubeNetwork.Node("Right mouth",outlet,COgheTubeNetwork.TerminalKind.Entry,Vector3.right)},
                    new[]{new COgheTubeNetwork.Edge("Open transfer bore",0,1,inlet,outlet)},.025f,glass);
                foreach(float side in new[]{-1f,1f})
                {
                    float centre=x+side*.057f;
                    Panel(c.Root,"High mouth approach shelf",new Vector3(x+side*.057f,.040f,-.19f),Vector3.up,new Vector2(.10f,.12f),stone,false,Vector2.zero,0,c.Surfaces);
                    // A closed grippable pedestal gives the tail a continuous
                    // outer climb face. A floating shelf over the divider made
                    // an inaccessible underside pocket when an entering body stretched.
                    foreach(float edge in new[]{-1f,1f})
                    {
                        Panel(c.Root,"Mouth pedestal side",new Vector3(centre+edge*.05f,-.13f,-.19f),Vector3.right*edge,new Vector2(.12f,.34f),glass,false,Vector2.zero,0,c.Surfaces);
                        Panel(c.Root,"Mouth pedestal face",new Vector3(centre,-.13f,-.19f+edge*.06f),Vector3.forward*edge,new Vector2(.10f,.34f),glass,false,Vector2.zero,0,c.Surfaces);
                    }
                }
            }
        }
    }
}
