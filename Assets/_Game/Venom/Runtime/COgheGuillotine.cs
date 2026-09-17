using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Reusable gravity knife with a one-second warning, actual bond cuts and clear-to-rearm.</summary>
    public sealed class COgheGuillotine : COgheMechanism
    {
        public override bool SeparatesTissue=>true;
        public COgheRailSlider Rail;
        public Transform Sensor;
        public Vector3 SensorHalfSize = new Vector3(.027f,.04f,.065f);
        public Vector3 CutHalfSize = new Vector3(.005f,.065f,.065f);
        public Renderer[] Lamps;
        public Vector3 TouchHalfSize; // Optional input-only envelope; never a cutting or collision volume.
        public VenomCampaign.BladePhase Phase { get; private set; }
        private float clock;
        private VenomCampaign owner;
        private MaterialPropertyBlock block;
        private readonly int[] groupsBeforeCut = new int[CohesiveOrganism.ParticleCount];
        public float WarningProgress => Phase == VenomCampaign.BladePhase.Warning ? Mathf.Clamp01(clock) : 0;
        public override void InitializeMechanism(VenomCampaign game) { owner = game; }
        public override void ResetMechanism(VenomCampaign game) { owner = game; Phase = VenomCampaign.BladePhase.Ready; clock = 0; Rail.Locked = true; }
        public override bool BlocksFusion(int a, int b) => owner != null && owner.Matter.CrossesBlade(Rail.Body.transform, CutHalfSize, a, b);
        public override bool TryTouch(VenomCampaign game, Ray ray, float nearestSolidDistance)
        {
            if(TouchHalfSize.sqrMagnitude>0)
            {
                var frame=Rail.transform;
                var localRay=new Ray(frame.InverseTransformPoint(ray.origin),frame.InverseTransformDirection(ray.direction));
                if(new Bounds(Vector3.zero,TouchHalfSize*2).IntersectRay(localRay,out float distance))
                {
                    var point=frame.TransformPoint(localRay.GetPoint(distance));
                    if(Vector3.Distance(ray.origin,point)<=nearestSolidDistance+.002f)
                    {game.Motion.Move(game.Motion.Selected,Sensor.position,true);return true;}
                }
            }
            foreach (var shape in Rail.GetComponentsInChildren<Collider>())
                if (shape.Raycast(ray, out var hit, nearestSolidDistance + .002f))
                { game.Motion.Move(game.Motion.Selected, Sensor.position, true); return true; }
            return false;
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            bool occupied = false;
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
            {
                if (game.Matter.Escaped[i]) continue;
                Vector3 p = Sensor.InverseTransformPoint(game.Matter.Bodies[i].position);
                if (Mathf.Abs(p.x) < SensorHalfSize.x && Mathf.Abs(p.y) < SensorHalfSize.y && Mathf.Abs(p.z) < SensorHalfSize.z) occupied = true;
            }
            if (Phase == VenomCampaign.BladePhase.Ready && occupied) { Phase = VenomCampaign.BladePhase.Warning; clock = 0; }
            else if (Phase == VenomCampaign.BladePhase.Warning && (clock += dt) >= 1) { Phase = VenomCampaign.BladePhase.Falling; clock = 0; }
            Rail.Locked = Phase == VenomCampaign.BladePhase.Ready || Phase == VenomCampaign.BladePhase.Warning || Phase == VenomCampaign.BladePhase.AwaitClear;
            if (Phase == VenomCampaign.BladePhase.Falling)
            {
                int before = game.Matter.TotalFragmentCount;
                System.Array.Copy(game.Matter.Groups, groupsBeforeCut, groupsBeforeCut.Length);
                game.Matter.Cut(Rail.Body.transform, CutHalfSize);
                if (game.Matter.TotalFragmentCount > before) game.NotifyMechanismCut(Rail.Body.transform, groupsBeforeCut);
                if ((clock += dt) >= .5f) { Phase = VenomCampaign.BladePhase.Returning; clock = 0; }
            }
            if (Phase == VenomCampaign.BladePhase.Returning)
            {
                Rail.ApplyEffort(Rail.WorldAxis * Mathf.Clamp((Rail.Travel - Rail.Position) * 8 + .5f, 0, .8f));
                if (Rail.AtEnd) Phase = VenomCampaign.BladePhase.AwaitClear;
            }
            if (Phase == VenomCampaign.BladePhase.AwaitClear && !occupied) Phase = VenomCampaign.BladePhase.Ready;
            if (Lamps != null)
            {
                if (block == null) block = new MaterialPropertyBlock();
                for (int i = 0; i < Lamps.Length; i++)
                {
                    bool lit = Phase == VenomCampaign.BladePhase.Warning && WarningProgress >= i / 4f;
                    block.SetColor("_BaseColor", lit ? new Color(.84f,.61f,.32f) : new Color(.65f,.74f,.77f));
                    Lamps[i].SetPropertyBlock(block);
                }
            }
        }
    }
}
