using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class BossCooperationBuilder
    {
        internal static LevelRuntime Build(CampaignLevelSpec spec,CampaignBuildContext c,bool universe)
        {
            var l=BossGeometry.Sphere(universe?"Universe in the palm":"Mechanical heart",universe?.82f:.73f,Vector3.zero,c);
            var a=BossGeometry.Author(l,c);
            var heart=AddBalance(a,new Vector3(0,universe?.43f:.34f,0));
            MemoryRatchetAssembly memory=null;
            if(universe)
            {
                // The load act drains into a separate cam court. The roof's broad
                // inlet is internal geometry and has no exit force/waypoint logic.
                var rect=BossGeometry.Rectangle(.36f,.30f);
                BossGeometry.Mesh(a,"Cosmos cam court floor",PhysicsLabGeometry.Panel("Cosmos cam floor",rect,-.04f,.003f,new Vector2(-.25f,.23f),.04f),c.Glass);
                BossGeometry.Mesh(a,"Cosmos cam court roof",PhysicsLabGeometry.Panel("Cosmos cam roof",rect,.08f,.003f,new Vector2(0,-.19f),.04f),c.Glass);
                BossGeometry.Mesh(a,"Cosmos cam court sides",PhysicsLabGeometry.Border("Cosmos cam sides",rect,.006f,-.04f,.08f),c.Glass);
                memory=HardCampaignModules.Cam(a,new Vector3(0,.02f,0),Quaternion.identity,3);
                Tube(a,"Load to memory transfer",new Vector3(0,.175f,-.19f),.20f,.082f);
                // A straight down-transfer begins only on the cleared side of the cam.
                Tube(a,"Memory to cage transfer",new Vector3(-.25f,-.08f,.23f),.08f,.082f);
            }
            Vector3 cageOrigin=universe?new Vector3(-.25f,-.23f,.23f):new Vector3(0,-.12f,-.19f);
            if(!universe)Tube(a,"Heart visible transfer sleeve",new Vector3(0,.06f,-.19f),.20f,.082f);
            var cage=TransferCage(a,cageOrigin,universe?Quaternion.Euler(0,180,0):Quaternion.identity);heart.Cage=cage;
            var marker=new GameObject("Cage inlet observation");marker.transform.SetParent(cage.transform,false);marker.transform.localPosition=new Vector3(0,.05f,0);heart.CageInlet=marker.transform;
            var mouth=new GameObject("Cage mouth observation");mouth.transform.SetParent(cage.transform,false);mouth.transform.localPosition=new Vector3(0,-.025f,.134f);heart.CageMouth=mouth.transform;
            Renderer finalRing;
            if(universe)
            {
                // An actual broad rest court separates the cage from the ramp.
                a.Block("Cosmos preparation court",new Vector3(-.27f,-.31f,-.015f),new Vector3(.23f,.008f,.23f),c.Frame);
                HardCampaignModules.Pocket(a,new Vector3(-.29f,-.304f,.02f),Quaternion.identity,.11f,.11f);
                HardCampaignModules.Ramp(a,new Vector3(-.25f,-.30f,-.02f),new Vector3(-.35f,-.28f,-.12f),.11f,c.Frame);
                HardCampaignModules.Flight(a,new Vector3(-.05f,-.40f,-.12f),Quaternion.identity,.025f);
                // The final receive tray is enclosed by glass to make each ball
                // remain available while the partner is prepared and caught.
                var basin=new[]{new Vector2(-.43f,-.30f),new Vector2(.35f,-.30f),new Vector2(.35f,.15f),new Vector2(-.43f,.15f)};
                BossGeometry.Mesh(a,"Cosmos contained recovery basin",PhysicsLabGeometry.Panel("Cosmos recovery floor",basin,-.605f,.004f,new Vector2(.21f,-.095f),.04f),c.Glass);
                a.Block("Cosmos basin left",new Vector3(-.43f,-.4025f,-.075f),new Vector3(.006f,.405f,.45f),c.Glass);
                a.Block("Cosmos basin right",new Vector3(.35f,-.4025f,-.075f),new Vector3(.006f,.405f,.45f),c.Glass);
                a.Block("Cosmos basin front",new Vector3(-.04f,-.4025f,-.30f),new Vector3(.78f,.405f,.006f),c.Glass);
                a.Block("Cosmos basin back lower sill",new Vector3(-.04f,-.459f,.15f),new Vector3(.78f,.292f,.006f),c.Glass);
                a.Block("Cosmos basin back right jamb",new Vector3(.11f,-.2575f,.15f),new Vector3(.48f,.115f,.006f),c.Glass);
                a.Block("Cosmos basin back left jamb",new Vector3(-.4125f,-.2575f,.15f),new Vector3(.035f,.115f,.006f),c.Glass);
                Tube(a,"Cosmos sealed catch outlet",new Vector3(.21f,-.526f,-.095f),.15f,.082f);
                Funnel(a,-.60f,-.817f,.38f,.024f);
                finalRing=BossGeometry.Arc(a,"Universe receiving orbit",new Vector3(.17f,-.439f,-.10f),.11f,.09f,c.Rim);
            }
            else
            {
                // Passive wide catch only: no timing input, no launch mechanic in chapter four.
                var catchOutline=new[]{new Vector2(-.20f,-.12f),new Vector2(.20f,-.12f),new Vector2(.20f,.24f),new Vector2(-.20f,.24f)};
                BossGeometry.Mesh(a,"Heart passive catch court",PhysicsLabGeometry.Panel("Heart catch",catchOutline,-.335f,.004f,Vector2.zero,.04f),c.Glass);
                BossGeometry.Mesh(a,"Heart catch retaining rim",PhysicsLabGeometry.Border("Heart catch rim",catchOutline,.006f,-.335f,-.245f),c.Glass);
                Funnel(a,-.34f,-.727f,.30f,.024f);
                finalRing=BossGeometry.Arc(a,"Heart passive receiving orbit",new Vector3(0,-.329f,.025f),.15f,.13f,c.Rim);
            }
            var loadRing=BossGeometry.Arc(a,"Load heart outline",new Vector3(0,universe?.277f:.187f,0),.33f,.22f,c.Rim);
            var cageRing=BossGeometry.Arc(a,"Independent cage orbit",cageOrigin+Vector3.up*.078f,.17f,.17f,c.Rim);
            var observed=new System.Collections.Generic.List<BossPresentation.Milestone>{new BossPresentation.Milestone{Name="Partner secures the loaded balance",Seat=heart.BridgeCatch,Inlays=new[]{loadRing}},BossGeometry.Region("Both can leave the cage",cageOrigin+new Vector3(0,-.08f,universe?-.20f:.20f),new Vector3(.30f,.16f,.18f),cageRing)};
            if(memory!=null) observed.Add(new BossPresentation.Milestone{Name="Cam opened by actual rack work",Memory=memory,Inlays=new[]{finalRing}});
            else observed.Add(BossGeometry.Region("Passive catch",new Vector3(0,-.30f,.02f),new Vector3(.39f,.10f,.34f),finalRing));
            var presentation=BossGeometry.Present(a,observed.ToArray());presentation.Finale=new[]{loadRing,cageRing,finalRing};return l;
        }

        internal static MechanicalHeart AddBalance(MechanicalAuthoring a,Vector3 origin)
        {
            var f=new HardCampaignModules.Frame(a,origin,Quaternion.identity);
            var outline=BossGeometry.Rectangle(.40f,.28f);
            var floor=PhysicsLabGeometry.Panel("Wide balance module floor",outline,-.16f,.003f,new Vector2(0,-.19f),.04f);
            var roof=PhysicsLabGeometry.Panel("Wide balance module roof",outline,.16f,.003f,Vector2.zero,0);
            var sides=PhysicsLabGeometry.Border("Wide balance module walls",outline,.006f,-.16f,.16f);
            foreach(var item in new[]{(name:"Load module floor",mesh:floor),(name:"Load module roof",mesh:roof),(name:"Load module sides",mesh:sides)})
                BossGeometry.Mesh(a,item.name,item.mesh,a.Glass).transform.localPosition=origin;
            var beam=f.Hinge("Heart unequal arm balance",new Vector3(0,.002f,0),Vector3.forward,.060f,-1,27,.000025f);
            var carrier=new GameObject("Enlarged balanced carrier").transform;carrier.SetParent(beam.transform,false);carrier.localRotation=Quaternion.Euler(0,0,-25);
            a.Block("A 65mm recessed holding floor",new Vector3(-.25f,-.008f,-.065f),new Vector3(.065f,.006f,.09f),a.Frame,carrier);
            a.Block("A inner approach",new Vector3(-.10f,0,-.065f),new Vector3(.235f,.006f,.09f),a.Frame,carrier);
            a.Block("A outer pocket lip",new Vector3(-.289f,0,-.065f),new Vector3(.012f,.006f,.09f),a.Frame,carrier);
            foreach(int s in new[]{-1,1})a.Block("A rail "+s,new Vector3(-.14f,.024f,-.065f+s*.047f),new Vector3(.31f,.044f,.004f),a.Glass,carrier);
            a.Block("A covered refuge",new Vector3(-.14f,.049f,-.065f),new Vector3(.31f,.004f,.098f),a.Glass,carrier);
            a.Block("A inner cap",new Vector3(.018f,.025f,-.065f),new Vector3(.006f,.048f,.09f),a.Glass,carrier);
            a.Block("Balance cross member",new Vector3(0,-.006f,0),new Vector3(.028f,.01f,.22f),a.Frame,carrier);
            var cradle=new GameObject("B broad inclined cradle").transform;cradle.SetParent(carrier,false);cradle.localPosition=new Vector3(.11f,.012f,.065f);cradle.localRotation=Quaternion.Euler(0,0,25);
            a.Block("B 90mm floor",Vector3.zero,new Vector3(.094f,.006f,.09f),a.Frame,cradle);
            a.Block("B inner cap",new Vector3(-.049f,.025f,0),new Vector3(.006f,.05f,.09f),a.Glass,cradle);
            foreach(int s in new[]{-1,1})a.Block("B wide rail "+s,new Vector3(0,.025f,s*.047f),new Vector3(.094f,.046f,.004f),a.Glass,cradle);
            a.Block("B roof",new Vector3(0,.049f,0),new Vector3(.094f,.004f,.098f),a.Glass,cradle);
            a.Block("Visible balance trim",new Vector3(.11f,-.016f,0),new Vector3(.04f,.018f,.04f),a.Frame,carrier);beam.Body.centerOfMass=Vector3.zero;
            // A .111 kg ball at .25 m supplies 0.272 Nm at the level pose;
            // B at .11 m opposes with 0.120 Nm. Native contact still resolves both.
            ExitWall("A",-.302f,-.065f,.075f);ExitWall("B",.17f,.065f,.086f);
            foreach(int s in new[]{-1,1})f.Block("Load inner enclosure "+s,new Vector3(-.066f,0,s*.133f),new Vector3(.48f,.314f,.006f),a.Glass);
            f.Block("A escape landing",new Vector3(-.351f,-.002f,0),new Vector3(.094f,.008f,.26f),a.Frame);
            f.Block("B escape landing",new Vector3(.285f,-.002f,0),new Vector3(.224f,.008f,.26f),a.Frame);
            var receiver=f.Block("Physical retaining seat",new Vector3(-.285f,-.011f,-.065f),new Vector3(.01f,.020f,.09f),a.Frame);
            var latch=beam.gameObject.AddComponent<ContactSeatLatch>();latch.Armed=false;latch.Configure(beam,receiver.GetComponent<Collider>(),25,.8f);
            var heart=a.Level.gameObject.AddComponent<MechanicalHeart>();heart.BridgeCatch=latch;
            var press=new GameObject("B ball-contact securing plunger",typeof(Rigidbody),typeof(PhysicalProp),typeof(ConfigurableJoint));press.transform.SetParent(a.Root,false);
            Vector3 at=f.P(new Vector3(.275f,.027f,.107f));press.transform.localPosition=at;
            var body=press.GetComponent<Rigidbody>();body.mass=.020f;body.useGravity=false;body.solverIterations=32;body.solverVelocityIterations=12;
            a.Block("B securing contact face",Vector3.zero,new Vector3(.092f,.044f,.008f),a.Rim,press.transform);
            var joint=press.GetComponent<ConfigurableJoint>();joint.autoConfigureConnectedAnchor=false;joint.connectedBody=a.Root.GetComponent<Rigidbody>();joint.connectedAnchor=at+Vector3.forward*.004f;
            joint.axis=Vector3.forward;joint.secondaryAxis=Vector3.up;joint.xMotion=ConfigurableJointMotion.Limited;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;joint.linearLimit=new SoftJointLimit{limit=.004f,contactDistance=.0002f};
            var guide=press.AddComponent<GravitySliderGuide>();guide.Configure(joint,at,Vector3.forward,.008f,.003f);
            heart.ReleasePlunger=press.AddComponent<PressurePlunger>();heart.ReleasePlunger.Guide=guide;
            var bore=new GameObject("Wide internal transfer bore");bore.transform.SetParent(a.Root,false);bore.transform.localPosition=f.P(new Vector3(0,-.16f,-.19f));heart.TransferBore=bore.transform;
            a.Level.BallSpawn.localPosition=f.P(new Vector3(0,.002f,0)+Quaternion.Euler(0,0,-25)*new Vector3(-.04f,.02f,-.065f));
            a.AddBall(f.P(new Vector3(0,.002f,0)+Quaternion.Euler(0,0,-25)*(new Vector3(.11f,.012f,.065f)+Quaternion.Euler(0,0,25)*new Vector3(0,.02f,0))),"Partner B — lifted lane");
            a.Label("A → B → A",f.P(new Vector3(0,-.154f,-.24f)),.007f);
            return heart;
            void ExitWall(string name,float x,float z,float top)
            {
                f.Block(name+" low sill",new Vector3(x,-.0785f,0),new Vector3(.006f,.161f,.266f),a.Glass);
                f.Block(name+" upper jamb",new Vector3(x,(top+.157f)*.5f,0),new Vector3(.006f,.157f-top,.266f),a.Glass);
                float left=-.133f,right=.133f,min=z-.048f,max=z+.048f;
                f.Block(name+" near jamb",new Vector3(x,(top+.002f)*.5f,(left+min)*.5f),new Vector3(.006f,top-.002f,min-left),a.Glass);
                f.Block(name+" far jamb",new Vector3(x,(top+.002f)*.5f,(max+right)*.5f),new Vector3(.006f,top-.002f,right-max),a.Glass);
            }
        }
        internal static PhysicalHinge TransferCage(MechanicalAuthoring a,Vector3 origin,Quaternion rotation)
        {
            var f=new HardCampaignModules.Frame(a,origin,rotation);
            var cage=f.Hinge("Heart roof-fed hanging cage",Vector3.zero,Vector3.right,.20f,-24,24,.00012f);
            f.Block("Heart transfer cage floor",new Vector3(0,-.05f,0),new Vector3(.23f,.006f,.26f),a.Glass,cage.transform);
            foreach(int s in new[]{-1,1})
            {
                f.Block("Heart cage side "+s,new Vector3(s*.112f,0,0),new Vector3(.006f,.10f,.26f),a.Glass,cage.transform);
                f.Block("Heart cage inlet side roof "+s,new Vector3(s*.0875f,.05f,0),new Vector3(.055f,.006f,.26f),a.Glass,cage.transform);
                f.Block("Heart cage inlet end roof "+s,new Vector3(0,.05f,s*.0975f),new Vector3(.12f,.006f,.065f),a.Glass,cage.transform);
                f.Block("Heart cage outlet cheek "+s,new Vector3(s*.08f,0,.127f),new Vector3(.07f,.10f,.006f),a.Glass,cage.transform);
            }
            f.Block("Heart cage rear",new Vector3(0,0,-.127f),new Vector3(.23f,.10f,.006f),a.Glass,cage.transform);
            f.Block("Heart cage keel",new Vector3(0,-.065f,0),new Vector3(.12f,.02f,.075f),a.Frame,cage.transform);cage.Body.centerOfMass=new Vector3(0,-.043f,0);
            return cage;
        }
        internal static void Tube(MechanicalAuthoring a,string name,Vector3 center,float length,float width)
        {
            foreach(int s in new[]{-1,1})
            {
                a.Block(name+" X "+s,center+Vector3.right*(s*(width*.5f+.003f)),new Vector3(.006f,length,width+.012f),a.Glass);
                a.Block(name+" Z "+s,center+Vector3.forward*(s*(width*.5f+.003f)),new Vector3(width,.006f+length,.006f),a.Glass);
            }
        }
        private static void Funnel(MechanicalAuthoring a,float top,float bottom,float topRadius,float bottomRadius)
        {
            var v=new System.Collections.Generic.List<Vector3>();var t=new System.Collections.Generic.List<int>();
            for(int i=0;i<64;i++)
            {
                float p=i*Mathf.PI*2/64,q=(i+1)*Mathf.PI*2/64;
                Vector3 V(float r,float y,float angle)=>new Vector3(Mathf.Cos(angle)*r,y,Mathf.Sin(angle)*r);
                Quad(V(topRadius,top,p),V(topRadius,top,q),V(bottomRadius,bottom,q),V(bottomRadius,bottom,p));
                Quad(V(topRadius+.004f,top,q),V(topRadius+.004f,top,p),V(bottomRadius+.004f,bottom,p),V(bottomRadius+.004f,bottom,q));
            }
            var mesh=new Mesh{name="Boss physical receiver funnel"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();BossGeometry.Mesh(a,"Physical final receiver funnel",mesh,a.Glass);
            void Quad(Vector3 x,Vector3 y,Vector3 z,Vector3 w){int n=v.Count;v.AddRange(new[]{x,y,z,w});t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
        }
    }
}
