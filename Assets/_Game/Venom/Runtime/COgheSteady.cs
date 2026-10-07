using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// A one-euro filter for things that ride COghe's skin (its eyes, its hat). The skin is rebuilt every frame, so a point
    /// read off it shakes. Still, this smooths hard; moving fast, it follows closely (a squash or a hop does not leave the
    /// thing behind). Filter offsets from the body's middle rather than world positions, so walking costs no lag.
    /// </summary>
    public struct COgheSteady
    {
        public Vector3 X, Speed; public bool Set;
        public Vector3 Step(Vector3 x, float dt)
        {
            if (!Set) { X = x; Speed = Vector3.zero; Set = true; return X; }
            if (dt <= 0) return X;
            Speed = Vector3.Lerp(Speed, (x - X) / dt, Alpha(dt, 1f));
            X = Vector3.Lerp(X, x, Alpha(dt, 1.2f + 30 * Speed.magnitude));
            return X;
        }
        private static float Alpha(float dt, float cutoff) => 1 / (1 + 1 / (2 * Mathf.PI * cutoff * dt));
    }
}
