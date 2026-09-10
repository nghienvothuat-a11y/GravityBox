using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Presentation after every particle has physically cleared the outlet.
    /// Hides scenery without disabling colliders or changing puzzle state.</summary>
    public sealed class VenomCelebration
    {
        public const float Duration = 4.8f;
        private static int previousVariant = -1;
        private readonly VenomLevelController level;
        private readonly Dictionary<Renderer, bool> scenery = new Dictionary<Renderer, bool>();
        private Vector3 cameraStart, gatherPoint;
        private Quaternion cameraRotation;
        private float sizeStart, startedAt;
        public bool Active { get; private set; }
        public int Variant { get; private set; } = -1;
        public string VariantName => Variant == 0 ? "Vẫy chào" : Variant == 1 ? "Nhảy vỗ xúc tu" : "Xoay bung pháo hoa";
        public float Elapsed => Active ? Mathf.Max(0, level.Organism.SimulationTime-startedAt) : 0;
        public bool ReadyForNext => Active && Elapsed >= Duration;
        public Vector3 Up => cameraRotation * Vector3.up;
        public Vector3 Right => cameraRotation * Vector3.right;
        public Vector3 Forward => cameraRotation * Vector3.forward;

        public VenomCelebration(VenomLevelController owner) { level = owner; }

        public void Begin()
        {
            if (Active || !level.Completed || level.Organism.EscapedCount != CohesiveOrganism.ParticleCount) return;
            Active = true; startedAt = level.Organism.SimulationTime;
            // Draw from the other two poses after the first victory. The player
            // still gets a random celebration without seeing an immediate repeat.
            int roll = Random.Range(0, previousVariant < 0 ? 3 : 2);
            Variant = previousVariant < 0 ? roll : roll >= previousVariant ? roll+1 : roll;
            previousVariant = Variant;
            cameraStart = level.View.transform.position; cameraRotation = level.View.transform.rotation;
            sizeStart = level.View.orthographicSize;
            gatherPoint = level.Outlet.TransformPoint(new Vector3(0, 0, .14f));
            // Include detached mechanisms and transparent panes, not just the box
            // root. forceRenderingOff survives their own LateUpdate visibility code.
            foreach (var root in level.gameObject.scene.GetRootGameObjects())
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer.transform.IsChildOf(level.Organism.transform)) continue;
                    scenery[renderer] = renderer.forceRenderingOff;
                    renderer.forceRenderingOff = true;
                }
            level.GetComponent<VenomInput>()?.CancelGesture();
        }

        public void Step()
        {
            if (!Active) return;
            // Extend the outlet's existing collection assist to the victory shot.
            // Tissue remains dynamic; no teleport, fake escape or collider removal.
            foreach (var body in level.Organism.Bodies)
            {
                Vector3 desired = Vector3.ClampMagnitude((gatherPoint-body.position)*5, .20f);
                body.AddForce(Vector3.up*9.81f + Vector3.ClampMagnitude((desired-body.linearVelocity)*24, 6), ForceMode.Acceleration);
            }
        }

        public void Frame(int width, int height)
        {
            if (!Active || width <= 0 || height <= 0) return;
            var camera = level.View;
            Vector3 centre = Vector3.zero;
            foreach (var body in level.Organism.Bodies) centre += body.transform.position / CohesiveOrganism.ParticleCount;
            // Reserve space for the raised crest and waving tendrils, including
            // both escaped pieces while their real tissue gathers together.
            float horizontal = .095f, vertical = .105f;
            foreach (var body in level.Organism.Bodies)
            {
                Vector3 offset = body.transform.position-centre;
                horizontal = Mathf.Max(horizontal, Mathf.Abs(Vector3.Dot(offset,Right))+.065f);
                vertical = Mathf.Max(vertical, Mathf.Abs(Vector3.Dot(offset,Up))+.085f);
            }
            float size = Mathf.Max(vertical/.64f, horizontal*height/(width*.82f));
            float blend = Mathf.SmoothStep(0, 1, Mathf.Clamp01(Elapsed/1.05f));
            camera.transform.rotation = cameraRotation;
            camera.transform.position = Vector3.Lerp(cameraStart, centre+Up*.02f-Forward*.42f, blend);
            camera.orthographicSize = Mathf.Lerp(sizeStart, size, blend);
        }

        public void Reset()
        {
            if (Active && level.View != null)
            {
                level.View.transform.SetPositionAndRotation(cameraStart, cameraRotation);
                level.View.orthographicSize = sizeStart;
            }
            foreach (var pair in scenery) if (pair.Key != null) pair.Key.forceRenderingOff = pair.Value;
            scenery.Clear(); Active = false; Variant = -1;
        }

#if UNITY_EDITOR
        public void SetVariantForTests(int variant) { Variant = Mathf.Clamp(variant, 0, 2); }
#endif
    }
}
