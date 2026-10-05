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
                    if (rail.Phase == COgheTapRail.TaskPhase.Operating && !rail.CanInterrupt) return false;
                    rail.CancelTask();
                }
            return true;
        }
        /// <summary>A lift button COghe is pressing right now (presentation draws one tendril to it).</summary>
        public bool TryPressContact(int anchor, out Vector3 point, out float depth)
        {
            foreach (var m in Mechanisms)
                if (m is COghePassengerLift lift && lift.isActiveAndEnabled && lift.Boarding && lift.Button == COghePassengerLift.ButtonState.Pressing &&
                    lift.Actor >= 0 && Matter.Groups[lift.Actor] == Matter.Groups[anchor])
                { point = lift.PressPoint; depth = lift.PanelDepth; return true; }
            point = Vector3.zero; depth = 0; return false;
        }
        /// <summary>0..1: how hard the body holding a handle strains against a load that is not moving.</summary>
        public float ManipulationStrain(int anchor)
        {
            foreach (var rail in tapRails)
                if (rail.Phase == COgheTapRail.TaskPhase.Operating && rail.Owns(anchor)) return rail.Strain;
            return 0;
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
