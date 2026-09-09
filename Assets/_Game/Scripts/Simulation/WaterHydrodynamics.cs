using UnityEngine;

namespace GravityBox.Simulation
{
    /// <summary>SI-unit reduced sphere models; sources and validity ranges: Docs/LEVEL13_WATER.md.</summary>
    public static class WaterHydrodynamics
    {
        public static Vector3 FreeSphereDrag(Vector3 velocity, float radius, float density, float viscosity)
        {
            float speed = velocity.magnitude;
            if (speed < 1e-9f) return Vector3.zero;
            float re = density * speed * 2 * radius / viscosity;
            float force = re < 1000
                ? 6 * Mathf.PI * viscosity * radius * speed * (1 + .15f * Mathf.Pow(re, .687f))
                : .5f * density * .44f * Mathf.PI * radius * radius * speed * speed;
            return -velocity * (force / speed);
        }

        // Effective translational resistance (hydrodynamic force + torque/r) for rolling
        // without slip. Apply at the centre, not again as a rolling torque: same power
        // along the no-slip degree of freedom. PhysX still supplies the static contact.
        public static float RollingSphereResistance(float speed, float radius, float density,
            float viscosity, float effectiveGap)
        {
            if (speed <= 0) return 0;
            float re = density * speed * 2 * radius / viscosity;
            float gapRatio = Mathf.Clamp(effectiveGap / (2 * radius), 1e-6f, .01f);
            float gapTerm = -44.2f * Mathf.Log10(gapRatio) + 34;
            // Nanayakkara et al. (2024), eqs 2.2–2.4. The wake polynomial is ONLY
            // evaluated in 5 <= Re <= 300. Bridge to the reported Cd ~ 1 plateau
            // at Re >= 1000, rather than extrapolating a polynomial to negative drag.
            float logRe = Mathf.Log10(Mathf.Clamp(re, 5, 300));
            float wake = 1.70f - .136f * logRe - .0716f * logRe * logRe;
            if (re < 5) wake *= Mathf.SmoothStep(0, 1, re / 5);
            if (re > 300) wake = Mathf.Lerp(wake, 1, Mathf.Clamp01(Mathf.Log(re / 300) / Mathf.Log(1000f / 300)));
            // Evaluate the 1/Re term algebraically: finite and linear as speed -> 0.
            float gapForce = .25f * Mathf.PI * viscosity * radius * gapTerm * speed;
            return gapForce + .5f * density * Mathf.PI * radius * radius * wake * speed * speed;
        }
    }
}
