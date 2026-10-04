using System;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>One real puzzle attempt. UI redraws, pause and intro are not attempts.</summary>
    public sealed class COgheLevelAttempt
    {
        public bool Active { get; private set; }
        public float Seconds { get; private set; }
        public int Number { get; private set; }
        public string Id { get; private set; }
        private string levelId, campaign;
        private int level;
        private bool replay;

        public void Begin(string stableId, int order, string catalog, bool completedBefore, bool persist)
        {
            if (Active) return;
            levelId = stableId; level = order; campaign = catalog; replay = completedBefore;
            string key = "coghe.attempt.v1." + catalog + "." + stableId;
            Number = persist ? PlayerPrefs.GetInt(key, 0) + 1 : Number + 1;
            if (persist) { PlayerPrefs.SetInt(key, Number); PlayerPrefs.Save(); }
            Id = Guid.NewGuid().ToString("N"); Seconds = 0; Active = true;
            Emit("level_start");
        }

        public void Tick(float delta, bool playing)
        {
            if (Active && playing && delta > 0 && !float.IsNaN(delta) && !float.IsInfinity(delta)) Seconds += delta;
        }

        public void Finish(bool won, string reason)
        {
            if (!Active) return;
            Active = false;
            Emit("level_end", won ? "win" : "lost", reason, won ? 1 : 0);
            Emit(won ? "level_complete" : "level_fail", won ? "win" : "lost", reason, won ? 1 : 0);
        }

        public void Abandon(string reason)
        {
            if (!Active) return;
            Active = false; Emit("level_abandon", "abandon", reason);
        }

        public void Retry() => Emit("level_retry", "retry", "retry");

        private void Emit(string name, string outcome = "", string reason = "", int success = 0)
        {
            COgheAnalytics.Log(name, "level", level, "level_id", levelId, "level_name", levelId,
                "campaign", campaign, "attempt", Number, "attempt_id", Id,
                "duration_seconds", Math.Round(Seconds, 2), "replay", replay ? 1 : 0,
                "outcome", outcome, "reason", reason, "success", success);
        }
    }
}
