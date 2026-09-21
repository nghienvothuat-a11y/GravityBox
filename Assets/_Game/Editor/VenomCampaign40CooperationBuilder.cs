using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static void CooperationRoom(ExpansionContext c, string title, string lesson, float halfX, float dividerX)
        {
            ConfigureNew(c, title, lesson, false, new Vector3(40, 14, 0), halfX + .12f);
            c.Spawn = new Vector3(-halfX + .11f, -.265f, -.18f);
            c.Exit = new Vector3(halfX, -.19f, .16f); c.Outward = Vector3.right;
            c.Definition.CameraZones = new[] {
                new VenomCameraZone("Khoang 1", new Vector3((-halfX + dividerX) * .5f, -.02f, 0), new Vector3(halfX + dividerX + .03f, .64f, .64f)),
                new VenomCameraZone("Khoang 2", new Vector3((halfX + dividerX) * .5f, -.02f, 0), new Vector3(halfX - dividerX + .03f, .64f, .64f)) };
            ExpansionShell(c, halfX);
        }

        // A genuinely sealed divider: no knife slot, high transfer bore or
        // walk around the roof. Both face normals participate in navigation.
        private static COgheRailSlider CooperationDoor(ExpansionContext c, string name, float x, float z = .14f, float width = .22f, float height = .16f)
        {
            float low = z - width * .5f, high = z + width * .5f;
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 normal = Vector3.right * side;
                float px = x + side * .006f;
                Panel(c.Root, name + " solid front divider", new Vector3(px, 0, (-.30f + low) * .5f), normal,
                    new Vector2(low + .30f, .60f), glass, false, Vector2.zero, 0, c.Surfaces);
                Panel(c.Root, name + " solid rear divider", new Vector3(px, 0, (.30f + high) * .5f), normal,
                    new Vector2(.30f - high, .60f), glass, false, Vector2.zero, 0, c.Surfaces);
                Panel(c.Root, name + " sealed header", new Vector3(px, height * .5f, z), normal,
                    new Vector2(width, .60f - height), glass, false, Vector2.zero, 0, c.Surfaces);
            }
            var door = ExpansionRail(c, name, new Vector3(x - .026f, -.30f + height * .5f, z), Vector3.up,
                height + .018f, 0, new Vector3(.020f, height, width + .002f), .045f, .016f, false, false);
            return door;
        }

        private static COgheRailSlider CooperationExit(ExpansionContext c)
        {
            var gate = ExpansionRail(c, "E final shutter", c.Exit + Vector3.left * .012f, Vector3.up, .17f, 0,
                new Vector3(.016f, .14f, .14f), .045f, .016f, false, false);
            gate.LatchAtEnd = true;
            return gate;
        }

        private static COgheRailSlider CooperationHandle(ExpansionContext c, string name, Vector3 point)
        {
            var handle = ExpansionRail(c, name, point, Vector3.right, .065f, 0, new Vector3(.046f, .050f, .045f), .08f, .040f, false, true);
            // Keep a generous floor stance on the front of the handle throughout
            // its travel, with a fixed working face rather than floating feet.
            return handle;
        }

        private static COgheSpringAccessDoor CooperationSpring(ExpansionContext c, COgheTissueSensor input, COgheRailSlider door, COgheRailSlider latch)
        {
            var driver = new GameObject("A spring D and far-side L catch", typeof(COgheSpringAccessDoor)).GetComponent<COgheSpringAccessDoor>();
            driver.transform.SetParent(c.Root, false); driver.Input = input; driver.Door = door; driver.LatchHandle = latch;
            driver.DoorSize = new Vector3(.020f, .16f, .222f);
            var pin = new GameObject("D upper mechanical catch").transform; pin.SetParent(c.Root, false);
            pin.localPosition = new Vector3(.001f, -.10f, .25f);
            driver.CatchPin = MechanismVisual(pin, "L actuated catch pin", Vector3.zero, new Vector3(.04f, .012f, .016f), metal);
            var spring = new GameObject("D return spring").transform; spring.SetParent(c.Root, false); spring.localPosition = new Vector3(-.053f, .07f, .24f);
            driver.Spring = spring;
            for (int i = 0; i < 8; i++)
            {
                var coil = MechanismVisual(spring, "Visible return spring coil", new Vector3(i % 2 == 0 ? -.008f : .008f, i * .009f, 0), new Vector3(.019f, .003f, .004f), metal);
                coil.localRotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 25 : -25);
            }
            return driver;
        }

        private static void BuildCampaign35(ExpansionContext c)
        {
            CooperationRoom(c, "Giữ rồi trả tự do", "Một phần giữ A; phần bên kia kéo L để chốt cửa. Hợp thể rồi kéo nắp E.", .50f, 0);
            var door = CooperationDoor(c, "D spring access door", 0);
            ExpansionKnife(c, "I gravity knife", -.34f, -.13f).TouchHalfSize = new Vector3(.036f, .10f, .08f);
            var a = ExpansionPad(c, "A temporary door clutch", new Vector3(-.38f, -.297f, .17f), .012f, .112f);
            var latch = CooperationHandle(c, "L far-side door catch", new Vector3(.23f, -.255f, -.12f)); latch.LatchAtEnd = true;
            var cap = ExpansionRail(c, "E movable final cover", c.Exit + Vector3.left * .012f, Vector3.back, .15f, 0,
                new Vector3(.016f, .14f, .14f), .10f, .06f, false, true); cap.LatchAtEnd = true;
            var driver = CooperationSpring(c, a, door, latch); driver.FinalCover = cap;
        }

        private static void BuildCampaign36(ExpansionContext c)
        {
            CooperationRoom(c, "Đổi ca", "Giữ A cho bạn qua. Kéo L, giữ B rồi đổi phần để kéo C.", .50f, 0);
            var door = CooperationDoor(c, "D spring access door", 0);
            ExpansionKnife(c, "I gravity knife", -.34f, -.13f).TouchHalfSize = new Vector3(.036f, .10f, .08f);
            var a = ExpansionPad(c, "A temporary door clutch", new Vector3(-.38f, -.297f, .17f), .012f, .112f);
            var b = ExpansionPad(c, "B winch clutch", new Vector3(.21f, -.297f, .14f), .012f, .112f);
            // L is itself B's opaque, solid cover. Its 10 mm underside gap
            // clears the sensor's collision skin but is narrower than an
            // 18 mm tissue particle. Coplanar plate contact wedged this rail.
            var latch = ExpansionRail(c, "L physical B cover and D catch", new Vector3(.21f, -.252f, .14f), Vector3.right, .18f, 0,
                new Vector3(.14f, .070f, .14f), .075f, .040f, false, true); latch.LatchAtEnd = true;
            CooperationSpring(c, a, door, latch);
            var handle = CooperationHandle(c, "C return-shift winch", new Vector3(-.23f, -.255f, -.17f));
            var winch = new GameObject("B holds C opens E", typeof(COgheCooperativeWinch)).GetComponent<COgheCooperativeWinch>(); winch.transform.SetParent(c.Root, false);
            winch.Input = b; winch.Handle = handle; winch.Doors = new[] { CooperationExit(c) }; winch.HandleForce = .05f; winch.DoorSpeed = .08f;
            ExpansionLinkage(c, winch);
        }

        private static void BuildCampaign37(ExpansionContext c)
        {
            CooperationRoom(c, "Chung một bánh", "Giữ A. Dùng G ở I mở đường, rồi chuyển G tới II và kéo C.", .52f, 0);
            // A taller/wider aperture contains the whole gear-tip envelope.
            var door = CooperationDoor(c, "D station-I access door", 0, .085f, .32f, .26f); door.LatchAtEnd = true;
            ExpansionKnife(c, "I gravity knife", -.36f, -.15f).TouchHalfSize = new Vector3(.036f, .10f, .08f);
            var a = ExpansionPad(c, "A held motor clutch", new Vector3(-.40f, -.297f, .18f), .012f, .112f);
            var carriage = ExpansionRail(c, "G shared travelling gear", new Vector3(-.14f, -.235f, .065f), Vector3.right, .28f, .025f,
                new Vector3(.052f, .050f, .05f), .075f, .035f, false, true);
            carriage.LatchAtStart = carriage.LatchAtEnd = true;
            const float radius = .04f, y = -.15f, z = .11f;
            var wheel = MeshingStationWheel(carriage.transform, "G reused gear", new Vector3(0, .085f, .045f), radius, 18, 0);
            Transform[] Station(float x, string name) => new[] {
                MeshingStationWheel(c.Root, name + " source", new Vector3(x, y + radius * 2, z), radius, 18, 10),
                MeshingStationWheel(c.Root, name + " output", new Vector3(x, y - radius * 2, z), radius, 18, 10) };
            var mechanism = new GameObject("A and reused G two-stage transmission", typeof(COgheCooperativeDockTransmission)).GetComponent<COgheCooperativeDockTransmission>(); mechanism.transform.SetParent(c.Root, false);
            mechanism.Input = a; mechanism.Carriage = carriage; mechanism.AccessDoor = door; mechanism.CarriageWheel = wheel;
            mechanism.StationIWheels = Station(-.14f, "I"); mechanism.StationIIWheels = Station(.14f, "II");
            mechanism.Handle = CooperationHandle(c, "C powered final winch", new Vector3(.32f, -.255f, -.15f));
            mechanism.ExitDoor = CooperationExit(c);
            // Equipment labels map the actual two fixed stations to their
            // outputs. These are identifiers, never a numbered solution guide.
            FortyMark(c, "I / D", new Vector3(-.14f, -.287f, -.035f));
            FortyMark(c, "II / C", new Vector3(.14f, -.287f, -.035f));
            FortyCable(c, "A motor feed", new Vector3(-.40f, -.291f, .18f), new Vector3(-.40f, -.291f, .27f));
            FortyCable(c, "A fixed power bus", new Vector3(-.40f, -.291f, .27f), new Vector3(.14f, -.291f, .27f));
            foreach (float stationX in new[] { -.14f, .14f })
            {
                FortyCable(c, "Station fixed feed", new Vector3(stationX, -.291f, .27f), new Vector3(stationX, -.07f, .27f));
                FortyCable(c, "Station bearing feed", new Vector3(stationX, -.07f, .27f), new Vector3(stationX, -.07f, .11f));
            }
            // The conduits terminate at fixed actuator/rail mounts. None
            // pretend to be an inextensible rope attached to a moving handle.
            FortyCable(c, "I to D actuator", new Vector3(-.14f, -.23f, .11f), new Vector3(-.14f, -.291f, .11f));
            FortyCable(c, "D actuator supply", new Vector3(-.14f, -.291f, .11f), new Vector3(-.026f, -.291f, .11f));
            FortyCable(c, "II to C actuator", new Vector3(.14f, -.23f, .11f), new Vector3(.14f, -.291f, .11f));
            FortyCable(c, "C fixed supply", new Vector3(.14f, -.291f, .11f), new Vector3(.32f, -.291f, .11f));
            FortyCable(c, "C rail supply", new Vector3(.32f, -.291f, .11f), new Vector3(.32f, -.291f, -.15f));
            FortyCable(c, "C to E output", new Vector3(.32f, -.291f, -.15f), new Vector3(.47f, -.291f, -.15f));
            FortyCable(c, "E guide supply", new Vector3(.47f, -.291f, -.15f), new Vector3(.47f, -.291f, .16f));
            FortyCable(c, "E actuator feed", new Vector3(.47f, -.291f, .16f), new Vector3(.508f, -.291f, .16f));
            // A wide aisle behind G permits walking away after release. The
            // backing meets the floor, avoiding a narrow underside pocket.
            // The gate physically prevents carrying G to II early.
            foreach (float x in new[] { -.14f, .14f })
                Panel(c.Root, "Station clear gear guard", new Vector3(x, -.13f, .22f), Vector3.back, new Vector2(.16f, .34f), glass, false, Vector2.zero, 0, c.Surfaces);
        }

        private static void BuildCampaign38(ExpansionContext c)
        {
            CooperationRoom(c, "Tụ để đổi việc", "Chia việc mở D, tụ để đẩy Q, rồi chia lại vận hành B và C.", .66f, -.13f);
            var door = CooperationDoor(c, "D first-stage reunion door", -.13f); door.LatchAtEnd = true;
            ExpansionKnife(c, "I first gravity knife", -.48f, -.13f).TouchHalfSize = new Vector3(.036f, .10f, .08f);
            var a = ExpansionPad(c, "A first clutch", new Vector3(-.53f, -.297f, .18f), .012f, .112f);
            var k = CooperationHandle(c, "K first-stage winch", new Vector3(-.31f, -.255f, -.13f));
            var first = new GameObject("A K first-stage winch", typeof(COgheCooperativeWinch)).GetComponent<COgheCooperativeWinch>(); first.transform.SetParent(c.Root, false);
            first.Input = a; first.Handle = k; first.Doors = new[] { door }; first.HandleForce = .05f; first.DoorSpeed = .09f; ExpansionLinkage(c, first);

            // 0.39 N exceeds a balanced 16-particle part's 0.336 N maximum,
            // while 32 particles can supply 0.672 N. No group-count gate exists.
            var q = ExpansionRail(c, "Q heavy transmission carriage", new Vector3(.04f, -.247f, .05f), Vector3.right, .14f, 0,
                new Vector3(.08f, .07f, .07f), .22f, .39f, false, true); q.LatchAtEnd = true;
            var b = ExpansionPad(c, "B second clutch", new Vector3(.12f, -.297f, -.20f), .012f, .112f);
            ExpansionKnife(c, "II second gravity knife", .34f, -.15f).TouchHalfSize = new Vector3(.036f, .10f, .08f);
            var transmission = new GameObject("Q seated gear transmission", typeof(COgheGearTrain)).GetComponent<COgheGearTrain>(); transmission.transform.SetParent(c.Root, false);
            transmission.InputClutch = b;
            transmission.Wheels = new[] {
                MeshingStationWheel(c.Root, "Q fixed input", new Vector3(.10f, -.172f, .11f), .04f, 18, 10),
                MeshingStationWheel(q.transform, "Q heavy intermediate", new Vector3(0, .075f, .06f), .04f, 18, 0),
                MeshingStationWheel(c.Root, "Q fixed output", new Vector3(.26f, -.172f, .11f), .04f, 18, 10) };
            transmission.PitchRadii = new[] { .04f, .04f, .04f }; transmission.ToothCounts = new[] { 18, 18, 18 }; transmission.MeshTolerance = .004f;
            var handle = CooperationHandle(c, "C second-stage winch", new Vector3(.48f, -.255f, -.15f));
            var second = new GameObject("Q B C second-stage winch", typeof(COgheCooperativeWinch)).GetComponent<COgheCooperativeWinch>(); second.transform.SetParent(c.Root, false);
            second.Input = b; second.Transmission = transmission; second.Handle = handle; second.Doors = new[] { CooperationExit(c) }; second.HandleForce = .05f; second.DoorSpeed = .08f;
            ExpansionLinkage(c, second);
            Panel(c.Root, "Q gear safety backing", new Vector3(.18f, -.13f, .15f), Vector3.back, new Vector2(.34f, .34f), glass, false, Vector2.zero, 0, c.Surfaces);
        }
    }
}
