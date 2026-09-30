using System;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Ordered references, never new IDs or a second copy of puzzle state.</summary>
    public sealed class COgheProductCatalog : ScriptableObject
    {
        public const string ProgressKey = "coghe.spatial.pilot";
        public VenomCampaignDefinition[] Levels = Array.Empty<VenomCampaignDefinition>();
        public int NextIncomplete(VenomCampaignSave save)
        {
            for (int i = 0; i < Levels.Length; i++)
                if (Levels[i] != null && !save.Completed.Contains(Levels[i].Id)) return i + 1;
            return 0;
        }
        public bool AllComplete(VenomCampaignSave save) => Levels.Length > 0 && NextIncomplete(save) == 0;
    }

    public static class COgheProductMode
    {
        public static bool? OverrideForTests;
        internal static bool SessionStarted;
        internal static bool ReplayIntroOnLoad;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { OverrideForTests = null; SessionStarted = false; ReplayIntroOnLoad = false; }
        public static bool Applies(VenomCampaign game)
        {
            if (game == null || game.Definition == null || game.Definition.ProgressKey != COgheProductCatalog.ProgressKey) return false;
            if (OverrideForTests.HasValue) return OverrideForTests.Value;
            if (!VenomCampaignSave.PersistenceEnabled || Application.isBatchMode) return false;
            foreach (string arg in Environment.GetCommandLineArgs())
                if (arg == "-coghe-developer-ui" || arg == "-coghe-view-proof" || arg == "-coghe-profile" || arg == "-coghe-benchmark") return false;
            return true;
        }
    }
}
