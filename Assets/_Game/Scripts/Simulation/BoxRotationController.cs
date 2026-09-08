using System;
using GravityBox.Foundation;
using UnityEngine;

namespace GravityBox.Simulation
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoxRotationController : MonoBehaviour, IResettable
    {
        private static readonly Quaternion[] Canonical = BuildCanonical();
        private Rigidbody body;
        private RotationSettings settings;
        private Quaternion desired;
        private Quaternion initial;
        private bool dragging;
        private bool snapPending;
        public bool InputEnabled { get; set; } = true;
        public RotationMode Mode { get; set; }
        public Quaternion Orientation => body.rotation;
        public event Action Snapped;

        public void Configure(RotationSettings config, RotationMode mode)
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            settings = config;
            Mode = mode;
            desired = body.rotation;
        }

        public void BeginDrag()
        {
            if (!InputEnabled) return;
            dragging = true;
            snapPending = false;
        }

        public void Drag(Vector2 normalizedDelta, Vector3 cameraUp, Vector3 cameraRight)
        {
            if (!InputEnabled || !dragging) return;
            Quaternion delta = Quaternion.AngleAxis(-normalizedDelta.x * settings.DegreesPerScreen, cameraUp)
                             * Quaternion.AngleAxis(normalizedDelta.y * settings.DegreesPerScreen, cameraRight);
            desired = delta * desired;
            // Bound backlog: a violent swipe cannot leave seconds of queued rotation.
            desired = Quaternion.RotateTowards(body.rotation, desired, settings.MaxQueuedAngle);
        }

        public void EndDrag()
        {
            dragging = false;
            if (!InputEnabled || Mode == RotationMode.Free) return;
            Quaternion nearest = NearestCanonical(desired);
            if (Mode == RotationMode.QuarterTurn || Quaternion.Angle(desired, nearest) <= settings.AssistedSnapAngle)
            {
                desired = nearest;
                snapPending = true;
            }
        }

        public void SetTargetOrientation(Quaternion orientation)
        {
            if (InputEnabled) desired = orientation.normalized;
        }

        private void FixedUpdate() => Step(Time.fixedDeltaTime);

        public void Step(float dt)
        {
            if (body == null || settings == null || !InputEnabled) return;
            Quaternion smoothed = Quaternion.Slerp(body.rotation, desired, 1f - Mathf.Exp(-dt / settings.SmoothingSeconds));
            body.MoveRotation(Quaternion.RotateTowards(body.rotation, smoothed, settings.MaxDegreesPerSecond * dt));
            if (snapPending && Quaternion.Angle(body.rotation, desired) < 0.2f)
            {
                snapPending = false;
                Snapped?.Invoke();
            }
        }

        public void CaptureInitialState() => initial = body.rotation;
        public void ResetState()
        {
            body.rotation = initial;
            desired = initial;
            dragging = false;
            snapPending = false;
            InputEnabled = true;
        }

        public static Quaternion NearestCanonical(Quaternion q)
        {
            Quaternion best = Quaternion.identity;
            float distance = float.MaxValue;
            for (int i = 0; i < Canonical.Length; i++)
            {
                float candidate = Quaternion.Angle(q, Canonical[i]);
                if (candidate < distance) { distance = candidate; best = Canonical[i]; }
            }
            return best;
        }

        private static Quaternion[] BuildCanonical()
        {
            var rotations = new System.Collections.Generic.List<Quaternion>(24);
            Vector3[] axes = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (Vector3 forward in axes)
                foreach (Vector3 up in axes)
                    if (Mathf.Abs(Vector3.Dot(forward, up)) < 0.1f) rotations.Add(Quaternion.LookRotation(forward, up));
            return rotations.ToArray();
        }
    }
}
