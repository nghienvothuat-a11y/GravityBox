using UnityEngine;

namespace GravityBox.Venom
{
    public sealed partial class COgheProductUI
    {
        private readonly COgheLevelAttempt attempt = new COgheLevelAttempt();
        private bool background;

        // Called only for the playable page, after an intro / Boss tour has ended.
        private void TrackAttempt()
        {
            if (Page != COgheProductPage.Game) return;
            if (!attempt.Active && !Game.Owner.Completed && !Game.Owner.Lost)
                attempt.Begin(Game.Definition.Id, Game.Definition.Order, Game.Definition.ProgressKey,
                    Game.Progress.Completed.Contains(Game.Definition.Id), VenomCampaignSave.PersistenceEnabled);
            attempt.Tick(Time.unscaledDeltaTime, !background && Application.isFocused &&
                !Game.Owner.Paused && Popup == COgheProductPopup.None && !COgheAds.Showing);
            if (Game.Owner.Completed) attempt.Finish(true, "exit");
            else if (Game.Owner.Lost) attempt.Finish(false,
                Game.Failure == VenomCampaign.MergeFailure ? "unmerged_exit" : "game_failure");
        }

        private void OnApplicationQuit() { attempt.Abandon("quit"); }
    }
}
