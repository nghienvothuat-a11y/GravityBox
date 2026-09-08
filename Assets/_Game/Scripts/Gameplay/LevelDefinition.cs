using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    [CreateAssetMenu(menuName = "Gravity Box/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public string Id;
        public int DisplayIndex;
        public string DisplayName;
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
