using UnityEngine;

namespace GravityBox.Simulation
{
    public static class PhysicsTiming
    {
        public const float Step = 1f / 120f;
        public static void Apply()
        {
            Time.fixedDeltaTime = Step;
            Time.maximumDeltaTime = 0.1f;
            Physics.gravity = Vector3.zero; // EnvironmentForceSystem applies world gravity once.
            Physics.defaultContactOffset = 0.0005f;
            // Exceed the gravity velocity gained in a solver step so resting contacts
            // do not repeatedly restitute it into a visible micro-bounce.
            Physics.bounceThreshold = 0.2f;
            Physics.defaultSolverIterations = 16;
            Physics.defaultSolverVelocityIterations = 8;
        }
    }
}
