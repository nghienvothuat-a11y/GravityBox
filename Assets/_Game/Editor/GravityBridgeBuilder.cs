using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class GravityBridgeBuilder
    {
        public static LevelRuntime Build(Material glass, Material frame, Material rim, PhysicsMaterial contact)
        {
            var a = new MechanicalAuthoring("Gravity bridge", new Vector3(.30f,.14f,.18f),
                new Vector2(.245f,0),new Vector3(-.215f,-.003f,-.095f),glass,frame,rim,contact);
            // The approach is a solid raised abutment, so the closed bridge cannot
            // be bypassed underneath. The only crossing in the partition is 110 mm.
            a.Block("West abutment",new Vector3(-.1515f,-.0705f,0),new Vector3(.297f,.103f,.354f),glass);
            a.Block("North dividing wall",new Vector3(0,0,.1175f),new Vector3(.010f,.244f,.125f),glass);
            a.Block("South dividing wall",new Vector3(0,0,-.1175f),new Vector3(.010f,.244f,.125f),glass);
            a.Block("Solid sill below bridge",new Vector3(0,-.076f,0),new Vector3(.010f,.092f,.110f),frame);
            a.Block("East landing",new Vector3(.1725f,-.023f,0),new Vector3(.055f,.008f,.110f),frame);
            a.Block("East landing north rail",new Vector3(.1725f,0,.054f),new Vector3(.055f,.043f,.006f),glass);
            a.Block("East landing south rail",new Vector3(.1725f,0,-.054f),new Vector3(.055f,.043f,.006f),glass);

            // The three-sided parking recess retains the ball against +X gravity
            // while the player lets the bridge fall toward its eastern receiver.
            a.Block("Parking outer cheek",new Vector3(-.258f,.019f,.078f),new Vector3(.008f,.076f,.114f),glass);
            a.Block("Parking holding cheek",new Vector3(-.172f,.019f,.078f),new Vector3(.008f,.076f,.114f),glass);
            a.Block("Parking back",new Vector3(-.215f,.019f,.135f),new Vector3(.094f,.076f,.008f),glass);
            a.Block("Parking mark",new Vector3(-.215f,-.0187f,.085f),new Vector3(.064f,.0004f,.062f),rim,null,false);

            var bridge = a.Hinge("Folding bridge",new Vector3(0,-.022f,0),Vector3.forward,.080f,-91,0,.000035f);
            a.Block("Bridge plank",new Vector3(0,.070f,0),new Vector3(.006f,.140f,.100f),frame,bridge.transform);
            a.Block("Bridge north rail",new Vector3(-.020f,.075f,.049f),new Vector3(.040f,.130f,.006f),glass,bridge.transform);
            a.Block("Bridge south rail",new Vector3(-.020f,.075f,-.049f),new Vector3(.040f,.130f,.006f),glass,bridge.transform);
            a.Block("Bridge floor centre line",new Vector3(-.0033f,.070f,0),new Vector3(.0004f,.136f,.003f),rim,bridge.transform,false);
            var receiver = a.Block("Bridge receiving pawl",new Vector3(.133f,-.033f,0),new Vector3(.015f,.016f,.110f),frame);
            bridge.gameObject.AddComponent<ContactSeatLatch>().Configure(bridge,receiver.GetComponent<Collider>(),-90,.8f);
            a.Block("West hinge bearing",new Vector3(0,-.022f,.065f),new Vector3(.022f,.022f,.020f),frame);
            a.Block("East hinge bearing",new Vector3(0,-.022f,-.065f),new Vector3(.022f,.022f,.020f),frame);
            a.Label("PARK",new Vector3(-.215f,-.018f,.108f),.007f);
            a.Label("FOLD + CROSS",new Vector3(-.100f,-.018f,-.115f),.006f);
            a.Label("SEAT",new Vector3(.165f,-.017f,0),.006f);
            return a.Level;
        }
    }
}
