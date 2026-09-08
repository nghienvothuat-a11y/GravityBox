using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    // Metre-scale fixed geometry. Both movable gates are passive jointed rigidbodies.
    public static class MechanicalMazeBuilder
    {
        public static readonly Vector2 Spawn = new Vector2(-.315f, -.240f);
        public static readonly Vector2 Exit = new Vector2(.315f, .240f);

        public static void Build(Transform root, Material glass, Material frame, Material floor,
            Material amber, Material marking, PhysicsMaterial contact, PhysicsMaterial sliderContact)
        {
            const float height = .084f;

            // Gate A divides the west and middle chambers. Its housing runs north.
            Wall("Gate A south partition", -.156f, -.084f, -.280f, -.010f);
            Wall("Gate A west housing", -.156f, -.144f, .070f, .280f);
            Wall("Gate A east housing", -.096f, -.084f, .070f, .280f);
            Wall("Gate A travel stop", -.144f, -.096f, .188f, .280f);
            Wall("Gate A holding recess back", -.256f, -.156f, .060f, .072f);
            Wall("Gate A holding recess side", -.268f, -.256f, -.035f, .072f);

            // Gate B divides the middle and east chambers. Its housing runs south.
            Wall("Gate B north partition", .084f, .156f, -.040f, .280f);
            Wall("Gate B west housing", .084f, .096f, -.280f, -.120f);
            Wall("Gate B east housing", .144f, .156f, -.280f, -.120f);
            Wall("Gate B travel stop", .096f, .144f, -.280f, -.238f);
            Wall("Gate B holding recess back", -.016f, .084f, -.122f, -.110f);
            Wall("Gate B holding recess side", -.028f, -.016f, -.122f, -.015f);

            // Alternating openings make the ball turn through 71–90 mm clear corridors.
            Wall("West maze lower crosswall", -.360f, -.245f, -.204f, -.192f);
            Wall("West maze upper crosswall", -.270f, -.156f, -.121f, -.109f);
            Wall("East maze lower crosswall", .245f, .360f, .016f, .028f);
            Wall("East maze middle crosswall", .156f, .270f, .099f, .111f);
            Wall("East maze upper crosswall", .245f, .360f, .182f, .194f);

            Gate("Maze gate A", new Vector3(-.120f, 0, .030f), Quaternion.identity);
            Gate("Maze gate B", new Vector3(.120f, 0, -.080f), Quaternion.Euler(0, 180, 0));

            // These inlays describe holding spaces; they have no triggers or colliders.
            Mark("Gate A recess side inlay", new Vector3(-.246f, -.0418f, .004f),
                new Vector3(.0015f, .0002f, .065f), marking);
            Mark("Gate A recess back inlay", new Vector3(-.205f, -.0418f, .036f),
                new Vector3(.084f, .0002f, .0015f), marking);
            Mark("Gate B recess side inlay", new Vector3(-.006f, -.0418f, -.054f),
                new Vector3(.0015f, .0002f, .065f), marking);
            Mark("Gate B recess back inlay", new Vector3(.035f, -.0418f, -.086f),
                new Vector3(.084f, .0002f, .0015f), marking);

            void Gate(string name, Vector3 position, Quaternion orientation)
            {
                var prop = GravitySliderAuthoring.Build(root, amber, sliderContact, position, orientation);
                prop.name = name;
                // Cover-side guides physically capture the gate without a raised floor obstacle.
                for (int side = -1; side <= 1; side += 2)
                {
                    GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rail.name = name + " cover rail " + (side < 0 ? "left" : "right");
                    rail.transform.SetParent(root, false);
                    rail.transform.localPosition = position + orientation * new Vector3(side * .014f, .0416f, .060f);
                    rail.transform.localRotation = orientation;
                    rail.transform.localScale = new Vector3(.006f, .001f, .196f);
                    rail.GetComponent<Renderer>().sharedMaterial = frame;
                    rail.GetComponent<Collider>().sharedMaterial = sliderContact;
                    rail.GetComponent<Collider>().contactOffset = .0005f;
                }
                for (int i = 0; i < 3; i++)
                    Mark(name + " travel notch " + i,
                        position + orientation * new Vector3(.032f, .0422f, .075f + i * .025f),
                        new Vector3(.008f, .0004f, .002f), amber);
            }

            void Wall(string name, float x0, float x1, float z0, float z1)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = name;
                wall.transform.SetParent(root, false);
                Vector3 center = new Vector3((x0 + x1) * .5f, 0, (z0 + z1) * .5f);
                wall.transform.localPosition = center;
                wall.transform.localScale = new Vector3(x1 - x0, height, z1 - z0);
                wall.GetComponent<Renderer>().sharedMaterial = glass;
                wall.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                wall.GetComponent<Collider>().sharedMaterial = contact;
                wall.GetComponent<Collider>().contactOffset = .0005f;
                Mark(name + " upper edge", center + Vector3.up * .041f,
                    new Vector3(x1 - x0, .002f, z1 - z0), frame);
                Mark(name + " lower edge", center - Vector3.up * .041f,
                    new Vector3(x1 - x0, .002f, z1 - z0), floor);
            }

            void Mark(string name, Vector3 position, Vector3 size, Material material)
            {
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = name;
                visual.transform.SetParent(root, false);
                visual.transform.localPosition = position;
                visual.transform.localScale = size;
                Object.DestroyImmediate(visual.GetComponent<Collider>());
                visual.GetComponent<Renderer>().sharedMaterial = material;
                visual.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
        }
    }
}
