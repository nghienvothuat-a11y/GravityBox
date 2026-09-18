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
        private bool docked;

        public override void ResetMechanism(VenomCampaign game)
        {
            Apply(game, false, true);
        }

        public override void StepMechanism(VenomCampaign game, float dt)
        {
            bool shouldDock = Rail != null && Rail.AtEnd;
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
            Physics.SyncTransforms();
            if (game != null && game.Motion != null) game.Motion.BuildGraph(true);
        }
    }
}
