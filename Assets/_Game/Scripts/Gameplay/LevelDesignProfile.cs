using System;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public enum CampaignRole { Intro, Practice, Combine, Rest, Calibration, Boss }

    [Serializable]
    public struct DifficultyAxes
    {
        [Range(0, 5)] public float Reasoning, Spatial, Precision, Coordination, Timing, RecoveryCost;
        public float Score => 20 * (.22f * Reasoning + .20f * Spatial + .18f * Precision +
                                   .18f * Coordination + .12f * Timing + .10f * RecoveryCost);
    }

    [CreateAssetMenu(menuName = "Gravity Box/Level Design Profile")]
    public sealed class LevelDesignProfile : ScriptableObject
    {
        public int SchemaVersion = 1;
        public int ContentVersion = 1;
        public int Chapter;
        public CampaignRole Role;
        public bool IsBoss;
        public int[] SourcePrototypes = Array.Empty<int>();
        public string[] IntroducedSkills = Array.Empty<string>();
        public string[] RequiredSkills = Array.Empty<string>();
        [Range(0, 100)] public float DifficultyBudget;
        public DifficultyAxes AuthorRating;
        // Unrated is intentionally distinct from a zero-difficulty, measured level.
        public bool HasAuthorRating;
        public bool PlaytestCalibrated;
        public string ImplementedByModel;
        [TextArea] public string DesignIntent;
        [TextArea] public string RecoveryPlan;
        [Min(0)] public float RecoveryBudgetSeconds = 10;
        [TextArea] public string ValidationNotes;
    }
}
