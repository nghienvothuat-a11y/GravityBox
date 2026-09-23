using UnityEngine;

namespace GravityBox.Venom
{
    public sealed partial class VenomCampaign
    {
        private COgheTapRail[] tapRails = System.Array.Empty<COgheTapRail>();
        public int PlayableLevelCount => Definition.SceneSequence != null && Definition.SceneSequence.Length > 0 ? Definition.SceneSequence.Length : LevelCount;
        public bool PrepareTapCommand(int anchor)
        {
            foreach (var rail in tapRails)
                if (rail.Owns(anchor))
                {
                    if (rail.Phase == COgheTapRail.TaskPhase.Operating) return false;
                    rail.CancelTask();
                }
            return true;
        }
        public bool TryManipulationContact(int anchor, out Vector3 point, out bool pulling)
        {
            foreach (var rail in tapRails)
                if (rail.Phase == COgheTapRail.TaskPhase.Operating && rail.Owns(anchor))
                { point = rail.HandPoint; pulling = rail.Pulling; return true; }
            point = PropContact; pulling = IsPulling;
            return heldProp != null && Matter.Groups[anchor] == Matter.Groups[Motion.Selected];
        }
    }
}
