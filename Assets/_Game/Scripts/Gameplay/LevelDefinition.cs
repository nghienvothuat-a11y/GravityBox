using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public enum ContainerShape { Circle, Square, Triangle, LShape, UShape, Annulus, Dumbbell, Star, GravityLock, MechanicalMaze, LayeredMaze, SphereMaze, WaterBox, MercuryBox, LionHead, CooperativeBox,
        GravityBridge, BalanceMachine, NestedCage, PendulumGate, FlightCatch, MechanicalMemory, MechanicalHeart }

    [CreateAssetMenu(menuName = "Gravity Box/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public string Id;
        public int DisplayIndex;
        public string DisplayName;
        public ContainerShape Shape;
        [TextArea] public string TeachingHint;
        public EnvironmentProfile Environment;
        public LevelRuntime Prefab;
        public RotationMode RotationMode = RotationMode.Assisted;
        public Vector3 InitialLocalVelocity;
        [Min(0)] public float ParSeconds = 30;
        public bool Tutorial;
        [TextArea] public string DesignerSolution;
    }
}
