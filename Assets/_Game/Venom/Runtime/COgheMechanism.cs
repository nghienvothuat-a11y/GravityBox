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
        public virtual bool ConstrainSkin(ref Vector3 world, out Vector3 normal) { normal = Vector3.up; return false; }
        public virtual bool BlocksFusion(int a, int b) => false;
        public virtual bool AllowsExitAssist(int particle, Vector3 capturePoint) => true;
        public virtual bool ControlsExit => false;
        public virtual bool ExitUnlocked => true;
        public virtual string Activity => null;
        public abstract void ResetMechanism(VenomCampaign game);
        public abstract void StepMechanism(VenomCampaign game, float dt);
    }
}
