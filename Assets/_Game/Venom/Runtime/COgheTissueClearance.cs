using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>An authored safety envelope measures live tissue before changing a junction or deck.</summary>
    public sealed class COgheTissueClearance : COgheMechanism
    {
        public Vector3 LocalCentre, Size;
        public COgheTubeNetwork Network;
        public bool Blocked { get; private set; }
        public override string Activity => Blocked ? "Có mô trong vùng chuyển — đưa về bệ an toàn" : null;
        public override void ResetMechanism(VenomCampaign game) { Measure(game); }
        public override void StepMechanism(VenomCampaign game, float dt) { Measure(game); }
        private void Measure(VenomCampaign game)
        {
            Blocked = false;
            if (game?.Matter == null) return;
            if (Network != null)
                for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                    if (Network.IsParticleInside(i)) { Blocked = true; return; }
            var bounds = new Bounds(LocalCentre, Size + Vector3.one * game.Matter.Profile.ParticleRadius * 2);
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
                if (!game.Matter.Escaped[i] && bounds.Contains(transform.InverseTransformPoint(game.Matter.Bodies[i].position)))
                { Blocked = true; return; }
        }
    }
}
