using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    [System.Serializable]
    public sealed class Vector2Contour
    {
        public Vector2[] Points = System.Array.Empty<Vector2>();
    }

    public sealed class LevelRuntime : MonoBehaviour
    {
        public Transform BallSpawn;
        public ExitSocket Exit;
        public BoxRotationController Rotation;
        public float BoundsHalfExtent = 3.7f;
        [Min(.04f), Tooltip("Enclosure depth in metres: floor/cover centre spacing for boxes, outer diameter for a sphere.")]
        public float InteriorDepth = .09f;
        [Tooltip("Authored XZ footprint, counter-clockwise. Collision remains defined by the baked shell meshes.")]
        public Vector2[] Footprint = System.Array.Empty<Vector2>();
        [Tooltip("Open regions inside the footprint, clockwise; separate from the small ball exit.")]
        public Vector2Contour[] FootprintVoids = System.Array.Empty<Vector2Contour>();
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
            WaterVolume water = GetComponent<WaterVolume>();
            if (water != null)
            {
                water.Bind(ball, forces.Environment);
                forces.AddProvider(water);
            }
            Plates = GetComponentsInChildren<PressurePlate>(true);
            Pads = GetComponentsInChildren<ImpulsePad>(true);
            Hazards = GetComponentsInChildren<KillVolume>(true);
            foreach (PressurePlate plate in Plates) plate.Bind(Signals, ball);
            foreach (SignalDoor door in GetComponentsInChildren<SignalDoor>(true)) door.Bind(Signals);
            foreach (OneWayGate gate in GetComponentsInChildren<OneWayGate>(true)) gate.Bind(ball);
            foreach (ImpulsePad pad in Pads) pad.Bind(ball);
            foreach (KillVolume hazard in Hazards) hazard.Bind(ball);
            Exit.Bind(ball, Signals);
            forces.AddProvider(Exit);
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
