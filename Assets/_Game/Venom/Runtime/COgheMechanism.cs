using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Campaign-owned simulation; presentation never advances puzzle state.</summary>
    public abstract class COgheMechanism : MonoBehaviour
    {
        public virtual void InitializeMechanism(VenomCampaign game) { }
        public virtual bool TryTouch(VenomCampaign game, Ray ray, float nearestSolidDistance) => false;
        public virtual bool SuppressesMotion(int particle) => false;
        public virtual bool IsFlowing(int particle) => false;
        public virtual bool HasSkinConstraint => false;
        // Register expensive tissue queries only on mechanisms implementing
        // them. A rail, lamp or pressure sensor does not transport/cut skin.
        public virtual bool TransportsTissue => false;
        public virtual bool SeparatesTissue => false;
        public virtual void PrepareSkinFrame() { }
        public virtual bool MayConstrainSkin(Bounds worldBounds) => true;
        public virtual bool ConstrainSkin(ref Vector3 world, out Vector3 normal) { normal = Vector3.up; return false; }
        public virtual bool BlocksFusion(int a, int b) => false;
        public virtual bool AllowsExitAssist(int particle, Vector3 capturePoint) => true;
        public virtual bool ControlsExit => false;
        public virtual bool ExitUnlocked => true;
        public virtual string Activity => null;
        /// <summary>Why a tap on this mechanism was refused. Presentation (marker, sound, product toast) reads it.</summary>
        public enum Refusal { None, Locked, Busy, NoPower, Blocked, Unreachable, NeedsHold, TooHeavy, Closed }
        public Refusal LastRefusal { get; private set; }
        public int Refusals { get; private set; }
        public Vector3 RefusalPoint { get; private set; }
        public float RefusedAt { get; private set; } = -10;
        protected void Refuse(VenomCampaign game, Refusal kind, Vector3 point)
        {
            LastRefusal = kind; Refusals++; RefusalPoint = point;
            RefusedAt = game != null && game.Matter != null ? game.Matter.SimulationTime : 0;
        }
        /// <summary>A tap this mechanism would take even while COghe holds a loose prop (the prop is let go first).</summary>
        public virtual bool ClaimsTapWhileHolding(Ray ray, float nearestSolidDistance) => false;
        public abstract void ResetMechanism(VenomCampaign game);
        public abstract void StepMechanism(VenomCampaign game, float dt);
    }
}
