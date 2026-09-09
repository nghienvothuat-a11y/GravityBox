using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class PendulumGateBuilder
    {
        public static LevelRuntime Build(Material glass, Material frame, Material rim, PhysicsMaterial contact)
        {
            var a = new MechanicalAuthoring("Pendulum gate", new Vector3(.30f, .14f, .30f),
                new Vector2(.205f, -.23f), new Vector3(0, -.119f, .23f), glass, frame, rim, contact);
            // An 80 x 84 mm aperture is the only link between the waiting room and the exit court.
            a.Block("West full partition", new Vector3(-.17f, 0, 0), new Vector3(.26f, .274f, .012f), glass);
            a.Block("East full partition", new Vector3(.17f, 0, 0), new Vector3(.26f, .274f, .012f), glass);
            a.Block("Passage lintel", new Vector3(0, .042f, 0), new Vector3(.08f, .190f, .012f), glass);
            a.Block("West aperture edge", new Vector3(-.041f, -.095f, .0065f), new Vector3(.002f, .084f, .001f), frame, null, false);
            a.Block("East aperture edge", new Vector3(.041f, -.095f, .0065f), new Vector3(.002f, .084f, .001f), frame, null, false);
            // Cheeks keep a parked ball aligned while the player pumps the pendulum using sideways tilt.
            // They end at the partition; the bob swings on the opposite side with 11 mm wall clearance.
            a.Block("Waiting pocket left guide", new Vector3(-.044f, -.103f, .137f), new Vector3(.008f, .068f, .262f), glass);
            a.Block("Waiting pocket right guide", new Vector3(.044f, -.103f, .137f), new Vector3(.008f, .068f, .262f), glass);
            a.Block("Waiting pocket back stop", new Vector3(0, -.103f, .268f), new Vector3(.096f, .068f, .008f), glass);
            a.Block("Waiting floor stripe", new Vector3(0, -.1368f, .215f), new Vector3(.067f, .0003f, .075f), rim, null, false);
            var pendulum = a.Hinge("Gravity pendulum", new Vector3(0, .10f, -.025f), Vector3.forward, .28f, -68, 68, .00010f);
            a.Block("Pendulum arm", new Vector3(0, -.094f, 0), new Vector3(.009f, .188f, .012f), frame, pendulum.transform);
            // Eight millimetres under the closed bob prevents its lower corner scraping the floor
            // during the first few degrees of swing, while remaining much smaller than the ball.
            GameObject bob = a.Block("Pendulum blocking bob", new Vector3(0, -.190f, 0), new Vector3(.10f, .078f, .014f), frame, pendulum.transform);
            a.Block("Pendulum face stripe", new Vector3(0, -.190f, .0075f), new Vector3(.064f, .004f, .001f), rim, pendulum.transform, false);
            pendulum.Body.centerOfMass = new Vector3(0, -.175f, 0);
            a.Block("Pendulum fixed bearing", new Vector3(0, .10f, -.004f), new Vector3(.025f, .025f, .024f), frame);
            var puzzle = a.Level.gameObject.AddComponent<PendulumGatePuzzle>();
            puzzle.Pendulum = pendulum; puzzle.Bob = bob.GetComponent<Collider>();
            a.Label("SWING THEN CROSS", new Vector3(-.18f, -.1365f, .14f), .0055f);
            return a.Level;
        }
    }
}
