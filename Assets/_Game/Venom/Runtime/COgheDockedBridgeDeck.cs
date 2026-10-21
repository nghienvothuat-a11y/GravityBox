using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Replaces a latched moving bridge top with an indistinguishable static
    /// collision surface. This removes the solver seam at the two dock lips
    /// while preserving the real movable bridge during manipulation.
    /// </summary>
    public sealed class COgheDockedBridgeDeck : COgheMechanism
    {
        public COgheRailSlider Rail;
        public VenomSurfacePatch[] MovingSurfaces;
        public VenomSurfacePatch DockedTop;
        public VenomSurfacePatch[] DockedSurfaces;
        // Rounded skins are siblings of the collision faces, so follow the same hand-off explicitly.
        // Keeping this presentation-only avoids changing the contact solver at the two dock lips.
        public Renderer[] MovingArt, DockedArt;
        public bool IsDocked => docked;
        private bool docked;

        public override void ResetMechanism(VenomCampaign game)
        {
            Apply(game, false, true);
        }

        public override void StepMechanism(VenomCampaign game, float dt)
        {
            // A carried, latched rail can settle just outside the entry tolerance under load. Keep its
            // contact replica until a real reverse pull releases the pawl; do not tear a rider's grip
            // down for sub-millimetre solver drift. The bounded exit band still rejects a displaced deck.
            bool atDock = Rail != null && (Rail.AtEnd || docked && Rail.Latched &&
                Rail.Position >= Rail.Travel - Rail.CatchTolerance * 2);
            bool shouldDock = atDock && Rail.Effort >= -Rail.Resistance - .004f;
            if (shouldDock != docked) Apply(game, shouldDock, false);
        }

        private void Apply(VenomCampaign game, bool value, bool force)
        {
            if (!force && value == docked) return;
            docked = value;
            if (MovingSurfaces != null)
                foreach (var surface in MovingSurfaces)
                    if (surface != null) surface.gameObject.SetActive(!value);
            if (DockedTop != null) DockedTop.gameObject.SetActive(value);
            if (DockedSurfaces != null)
                foreach (var surface in DockedSurfaces)
                    if (surface != null) surface.gameObject.SetActive(value);
            if (MovingArt != null) foreach (var r in MovingArt) if (r != null) r.enabled = !value;
            if (DockedArt != null) foreach (var r in DockedArt) if (r != null) r.enabled = value;
            Physics.SyncTransforms();
            if (game != null && game.Motion != null) game.Motion.BuildGraph(true);
        }
    }
}
