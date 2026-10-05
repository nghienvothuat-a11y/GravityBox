namespace GravityBox.Venom
{
    public sealed partial class COgheProductUI
    {
        private int seenRefusals = -1;

        /// <summary>A refused tap gets its reason on screen (Mrk, 05/10/2026: the game never said why nothing happened).</summary>
        private void WatchRefusals()
        {
            int total = 0; COgheMechanism last = null;
            foreach (var m in Game.Mechanisms)
            {
                if (m == null) continue; total += m.Refusals;
                if (m.Refusals > 0 && (last == null || m.RefusedAt > last.RefusedAt)) last = m;
            }
            if (seenRefusals < 0 || total < seenRefusals) { seenRefusals = total; return; }
            if (total == seenRefusals || last == null) return;
            seenRefusals = total;
            string text = RefusalText(last.LastRefusal);
            if (text != null) Notify(text);
        }

        public static string RefusalText(COgheMechanism.Refusal kind)
        {
            switch (kind)
            {
                case COgheMechanism.Refusal.Locked: return "Locked. Find what opens it first.";
                case COgheMechanism.Refusal.Busy: return "Still moving. Wait for it to stop.";
                case COgheMechanism.Refusal.NoPower: return "No power yet. Something else has to switch it on.";
                case COgheMechanism.Refusal.Blocked: return "Something is in the way.";
                case COgheMechanism.Refusal.Unreachable: return "COghe can't reach that from here.";
                case COgheMechanism.Refusal.NeedsHold: return "Someone has to stand on the pad first.";
                case COgheMechanism.Refusal.TooHeavy: return "Too heavy for this part. Merge and try again.";
                case COgheMechanism.Refusal.Closed: return "That way is closed.";
                default: return null;
            }
        }
    }
}
