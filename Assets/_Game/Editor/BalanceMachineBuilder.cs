using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class BalanceMachineBuilder
    {
        public static LevelRuntime Build(Material glass, Material frame, Material rim, PhysicsMaterial contact)
        {
            Quaternion initial = Quaternion.Euler(0,0,-25);
            Vector3 pivot = new Vector3(0,.002f,0);
            var a = new MechanicalAuthoring("Two balls one balance",new Vector3(.32f,.16f,.23f),
                new Vector2(0,-.184f),pivot+initial*new Vector3(-.038f,.020f,-.055f),glass,frame,rim,contact);
            var second = new GameObject("BallSpawn B");second.transform.SetParent(a.Root,false);
            Quaternion shortTrayRotation=Quaternion.Euler(0,0,25);
            Vector3 shortTrayOrigin=new Vector3(.095f,.012f,.055f);
            second.transform.localPosition = pivot+initial*(shortTrayOrigin+shortTrayRotation*new Vector3(0,.020f,0));
            a.Level.AdditionalBallSpawns = new[] {second.transform};
            // Two short enclosed lanes share one rigid, trimmed lever. They can
            // meet the fixed exit windows only after the lever has lifted B.
            var beam = a.Hinge("Unequal arm balance",pivot,Vector3.forward,.060f,-1,27,.000025f);
            // Bake the initial carrier slope into compound geometry. The joint's
            // own frame stays identity: Unity's native limit reference becomes
            // unstable if the Rigidbody itself starts rotated around this Z axis.
            var carrier = new GameObject("Balanced carrier frame").transform;
            carrier.SetParent(beam.transform,false);carrier.localRotation=initial;
            a.Block("A long arm floor",new Vector3(-.100f,0,-.055f),new Vector3(.280f,.006f,.070f),frame,carrier);
            a.Block("Balance cross member",new Vector3(0,-.004f,0),new Vector3(.024f,.008f,.180f),frame,carrier);
            Lane("A long arm",-.100f,.280f,-.055f);
            a.Block("A inner retaining cap",new Vector3(.042f,.023f,-.055f),new Vector3(.006f,.046f,.076f),glass,carrier);
            // B's cradle is initially level with the world. It cannot roll into
            // the fixed exit sill before the player has started tilting the box.
            // Raising the beam tilts this cradle upward toward its eventual exit.
            var shortTray=new GameObject("B inclined cradle").transform;
            shortTray.SetParent(carrier,false);shortTray.localPosition=shortTrayOrigin;shortTray.localRotation=shortTrayRotation;
            a.Block("B short arm floor",Vector3.zero,new Vector3(.076f,.006f,.070f),frame,shortTray);
            a.Block("B inner holding cap",new Vector3(-.040f,.023f,0),new Vector3(.006f,.046f,.076f),glass,shortTray);
            a.Block("B near rail",new Vector3(0,.024f,-.037f),new Vector3(.076f,.044f,.004f),glass,shortTray);
            a.Block("B far rail",new Vector3(0,.024f,.037f),new Vector3(.076f,.044f,.004f),glass,shortTray);
            a.Block("B clear roof",new Vector3(0,.047f,0),new Vector3(.076f,.004f,.078f),glass,shortTray);
            a.Block("Trim counterweight",new Vector3(.095f,-.012f,0),new Vector3(.032f,.017f,.025f),frame,carrier);
            // The manufactured beam is statically balanced about its pin before
            // either ball loads it; the visible trim weight represents adjustment.
            beam.Body.centerOfMass = Vector3.zero;

            ExitWall("A",-.249f,-.055f);
            ExitWall("B",.152f,.055f);
            a.Block("Balance front enclosure",new Vector3(-.0485f,0,-.112f),new Vector3(.401f,.314f,.006f),glass);
            a.Block("Balance back enclosure",new Vector3(-.0485f,0,.112f),new Vector3(.401f,.314f,.006f),glass);
            a.Block("A landing",new Vector3(-.2825f,-.002f,0),new Vector3(.065f,.008f,.221f),frame);
            a.Block("B landing",new Vector3(.2345f,-.002f,0),new Vector3(.159f,.008f,.221f),frame);
            // Both side courts spill into the same unobstructed southern exit
            // court. No plate needs to stay pressed after the pawl has engaged.
            a.Block("Balance axle front",new Vector3(0,.002f,-.106f),new Vector3(.030f,.030f,.012f),frame);
            a.Block("Balance axle back",new Vector3(0,.002f,.106f),new Vector3(.030f,.030f,.012f),frame);
            var receiver = a.Block("Balance retaining pawl",new Vector3(-.218f,-.011f,-.055f),new Vector3(.024f,.020f,.070f),frame);
            beam.gameObject.AddComponent<ContactSeatLatch>().Configure(beam,receiver.GetComponent<Collider>(),25,.8f);
            a.Block("Long arm mark",new Vector3(-.180f,.0034f,-.055f),new Vector3(.044f,.0004f,.040f),rim,carrier,false);
            a.Block("Short arm mark",new Vector3(0,.0034f,0),new Vector3(.044f,.0004f,.040f),rim,shortTray,false);
            a.Label("A: LONG ARM",new Vector3(-.125f,-.153f,-.19f),.006f);
            a.Label("B: LIFT",new Vector3(.19f,.004f,.075f),.006f);
            return a.Level;

            void Lane(string name,float x,float length,float z)
            {
                a.Block(name+" near rail",new Vector3(x,.024f,z-.037f),new Vector3(length,.044f,.004f),glass,carrier);
                a.Block(name+" far rail",new Vector3(x,.024f,z+.037f),new Vector3(length,.044f,.004f),glass,carrier);
                a.Block(name+" clear roof",new Vector3(x,.047f,z),new Vector3(length,.004f,.078f),glass,carrier);
            }
            void ExitWall(string name,float x,float z)
            {
                a.Block(name+" fixed low exit sill",new Vector3(x,-.0785f,0),new Vector3(.006f,.161f,.224f),glass);
                float top=name=="B"?.068f:.053f;
                a.Block(name+" fixed upper exit wall",new Vector3(x,(top+.157f)*.5f,0),new Vector3(.006f,.157f-top,.224f),glass);
                float left=-.112f,right=.112f,min=z-.040f,max=z+.040f;
                a.Block(name+" left window jamb",new Vector3(x,(top+.002f)*.5f,(left+min)*.5f),new Vector3(.006f,top-.002f,min-left),glass);
                a.Block(name+" right window jamb",new Vector3(x,(top+.002f)*.5f,(max+right)*.5f),new Vector3(.006f,top-.002f,right-max),glass);
            }
        }
    }
}
