using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class NestedCageBuilder
    {
        public static LevelRuntime Build(Material glass, Material frame, Material rim, PhysicsMaterial contact)
        {
            var a = new MechanicalAuthoring("Nested cage", new Vector3(.32f, .18f, .30f),
                new Vector2(.235f, -.235f), new Vector3(0, -.004f, .08f), glass, frame, rim, contact);
            Vector3 pivot = new Vector3(0, .025f, .08f);
            PhysicalHinge cage = a.Hinge("Weighted inner cage", pivot, Vector3.right, .34f, -32, 32, .00018f);
            Transform inner = cage.transform;
            a.Block("Inner supporting floor", new Vector3(0, -.05f, 0), new Vector3(.266f, .006f, .266f), glass, inner);
            a.Block("Inner clear roof", new Vector3(0, .05f, 0), new Vector3(.266f, .006f, .266f), glass, inner);
            a.Block("Inner left wall", new Vector3(-.13f, 0, 0), new Vector3(.006f, .10f, .266f), glass, inner);
            a.Block("Inner right wall", new Vector3(.13f, 0, 0), new Vector3(.006f, .10f, .266f), glass, inner);
            a.Block("Inner back wall", new Vector3(0, 0, .13f), new Vector3(.266f, .10f, .006f), glass, inner);
            // The only opening is a 64 mm mouth. Both skins remain solid when the player inverts the box.
            a.Block("Mouth left cheek", new Vector3(-.0825f, 0, -.13f), new Vector3(.101f, .10f, .006f), glass, inner);
            a.Block("Mouth right cheek", new Vector3(.0825f, 0, -.13f), new Vector3(.101f, .10f, .006f), glass, inner);
            a.Block("Left moving floor edge", new Vector3(-.13f, -.047f, 0), new Vector3(.003f, .003f, .266f), frame, inner, false);
            a.Block("Right moving floor edge", new Vector3(.13f, -.047f, 0), new Vector3(.003f, .003f, .266f), frame, inner, false);
            a.Block("Rear moving floor edge", new Vector3(0, -.047f, .13f), new Vector3(.266f, .003f, .003f), frame, inner, false);
            a.Block("Left mouth indicator", new Vector3(-.033f, 0, -.134f), new Vector3(.002f, .10f, .002f), rim, inner, false);
            a.Block("Right mouth indicator", new Vector3(.033f, 0, -.134f), new Vector3(.002f, .10f, .002f), rim, inner, false);
            // A dense keel lowers the centre of mass; gravity seeks a level inner floor until a stop is reached.
            a.Block("Cage counterweight keel", new Vector3(0, -.061f, .025f), new Vector3(.15f, .016f, .08f), frame, inner);
            cage.Body.centerOfMass = new Vector3(0, -.045f, 0);
            a.Block("Left fixed axle", pivot + Vector3.left * .23f, new Vector3(.18f, .008f, .008f), frame);
            a.Block("Right fixed axle", pivot + Vector3.right * .23f, new Vector3(.18f, .008f, .008f), frame);
            // The catch tray is the enclosure floor: a missed alignment is recoverable by tilting again.
            a.Block("Catch floor landing stripe", new Vector3(0, -.1768f, -.125f), new Vector3(.11f, .0003f, .10f), rim, null, false);
            var mouth = new GameObject("Inner cage transfer mouth"); mouth.transform.SetParent(inner, false);
            mouth.transform.localPosition = new Vector3(0, -.025f, -.134f);
            var puzzle = a.Level.gameObject.AddComponent<NestedCagePuzzle>();
            puzzle.Cage = cage; puzzle.Mouth = mouth.transform;
            a.Label("TILT PAST THE STOP", new Vector3(-.13f, -.1765f, -.24f), .0065f);
            return a.Level;
        }
    }
}
