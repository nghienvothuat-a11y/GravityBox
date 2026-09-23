using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>One tap requests measured occupancy; another tap asks its owner to step off.</summary>
    public sealed class COgheTapPad : COgheMechanism
    {
        public COgheTissueSensor Sensor;
        public VenomSurfacePatch Surface;
        public Transform ReleasePoint;
        public int Actor { get; private set; } = -1;
        private VenomCampaignMotion.Order order;
        public bool Holding => Actor >= 0;
        public override void ResetMechanism(VenomCampaign game) { Actor = -1; order = null; }
        public override bool TryTouch(VenomCampaign game, Ray ray, float nearestSolidDistance)
        {
            if (!Surface.Shape.Raycast(ray, out var hit, nearestSolidDistance + .002f)) return false;
            if (!game.PrepareTapCommand(game.Motion.Selected)) return true;
            if (Actor >= 0 && game.Matter.Groups[Actor] == game.Matter.Groups[game.Motion.Selected])
            { game.Motion.Move(Actor, ReleasePoint.position); Actor = -1; order = null; }
            else if (Actor < 0)
            {
                Actor = game.Motion.Selected;
                game.Motion.Move(Actor, Sensor.transform.position + Surface.Normal * .019f, true);
                order = game.Motion.Get(Actor);
            }
            game.Feedback.ShowCommand(hit.point, Surface.Normal, Surface.transform, Surface);
            return true;
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (Actor >= 0 && (!ReferenceEquals(game.Motion.Get(Actor), order) || game.Owner.Lost || game.Home))
            { Actor = -1; order = null; }
        }
    }
}
