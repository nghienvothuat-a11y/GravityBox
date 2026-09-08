using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class LevelRuntime : MonoBehaviour
    {
        public Transform BallSpawn;
        public ExitSocket Exit;
        public BoxRotationController Rotation;
        public float BoundsHalfExtent = 3.7f;
        public readonly MechanismSignals Signals = new MechanismSignals();
        public readonly ResetRegistry Resets = new ResetRegistry();
        private EnvironmentForceSystem forces;
        public PhysicalProp[] Props { get; private set; } = System.Array.Empty<PhysicalProp>();
        public PressurePlate[] Plates { get; private set; }
        public ImpulsePad[] Pads { get; private set; }
        public KillVolume[] Hazards { get; private set; }

        public void Initialize(BallController ball, RotationSettings settings, RotationMode mode, EnvironmentForceSystem forceSystem)
        {
            forces = forceSystem;
            Props = GetComponentsInChildren<PhysicalProp>(true);
            foreach (PhysicalProp prop in Props)
            {
                prop.Initialize();
                forces.Register(prop);
            }
            Rotation.Configure(settings, mode);
            Resets.Register(Rotation);
            Plates = GetComponentsInChildren<PressurePlate>(true);
            Pads = GetComponentsInChildren<ImpulsePad>(true);
            Hazards = GetComponentsInChildren<KillVolume>(true);
            foreach (PressurePlate plate in Plates) plate.Bind(Signals, ball);
            foreach (SignalDoor door in GetComponentsInChildren<SignalDoor>(true)) door.Bind(Signals);
            foreach (OneWayGate gate in GetComponentsInChildren<OneWayGate>(true)) gate.Bind(ball);
            foreach (ImpulsePad pad in Pads) pad.Bind(ball);
            foreach (KillVolume hazard in Hazards) hazard.Bind(ball);
            Exit.Bind(ball, Signals);
            // Explicit hierarchy order; no global FindObjectsOfType or singleton registry.
            foreach (MonoBehaviour component in GetComponentsInChildren<MonoBehaviour>(true))
                if (component is IResettable resettable && component != Rotation) Resets.Register(resettable);
            foreach (PhysicalProp prop in Props) Resets.Register(prop);
            Resets.Register(ball);
        }

        public void ResetAll()
        {
            Signals.Clear();
            Resets.RestoreAll();
            UnityEngine.Physics.SyncTransforms();
            Exit.BeginTracking();
        }

        public void ReleaseProps()
        {
            foreach (PhysicalProp prop in Props)
            {
                if (prop == null) continue;
                if (forces != null) forces.Unregister(prop);
                prop.gameObject.SetActive(false);
                Destroy(prop.gameObject);
            }
            Props = System.Array.Empty<PhysicalProp>();
        }

        private void OnDestroy() => ReleaseProps();

        public bool IsOutside(Vector3 worldPosition)
        {
            Vector3 p = transform.InverseTransformPoint(worldPosition);
            return Mathf.Abs(p.x) > BoundsHalfExtent || Mathf.Abs(p.y) > BoundsHalfExtent || Mathf.Abs(p.z) > BoundsHalfExtent;
        }
    }
}
