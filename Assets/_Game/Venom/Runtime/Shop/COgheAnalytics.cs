using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Game events for tracking (level_start/complete/fail, item_buy, drops_earn, ad_*, iap_*… — the list in
    /// PLANS/COGHE_MONETIZATION_PLAN.md §8). The tracking SDK plugs in through <see cref="Sink"/>; until then events go to
    /// the log in development builds and to <see cref="Recent"/> (tests).
    /// </summary>
    public static class COgheAnalytics
    {
        /// <summary>The tracking SDK: event name and parameters (name, value pairs).</summary>
        public static Action<string, IReadOnlyDictionary<string, object>> Sink;
        public static readonly List<string> Recent = new List<string>(64);
        private static readonly StringBuilder line = new StringBuilder(128);

        /// <summary>An event with parameters given as name, value, name, value…</summary>
        public static void Log(string name, params object[] pairs)
        {
            Dictionary<string, object> values = null;
            if (Sink != null || Debug.isDebugBuild)
            {
                values = new Dictionary<string, object>(pairs.Length / 2);
                for (int i = 0; i + 1 < pairs.Length; i += 2) values[pairs[i].ToString()] = pairs[i + 1];
            }
            // Telemetry is observational: a provider fault must never interrupt a win, save or reward.
            try { Sink?.Invoke(name, values); }
            catch (Exception e) { Debug.LogWarning("COghe analytics unavailable: " + e.GetType().Name); }
            line.Clear().Append(name);
            for (int i = 0; i + 1 < pairs.Length; i += 2) line.Append(' ').Append(pairs[i]).Append('=').Append(pairs[i + 1]);
            if (Recent.Count == Recent.Capacity) Recent.RemoveAt(0);
            Recent.Add(line.ToString());
            if (Debug.isDebugBuild && !Application.isBatchMode) Debug.Log("[COghe event] " + line);
        }
    }
}
