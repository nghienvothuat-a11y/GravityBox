using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Simulation
{
    public interface IPhysicsAffectable
    {
        Rigidbody Body { get; }
    }

    public interface IForceProvider
    {
        Vector3 GetAcceleration(IPhysicsAffectable target, EnvironmentProfile environment);
    }

    public sealed class GravityForceProvider : IForceProvider
    {
        public Vector3 GetAcceleration(IPhysicsAffectable target, EnvironmentProfile environment)
            => environment.Acceleration;
    }

    [DefaultExecutionOrder(-30)]
    public sealed class EnvironmentForceSystem : MonoBehaviour
    {
        private readonly List<IForceProvider> providers = new List<IForceProvider>(4);
        private readonly List<IPhysicsAffectable> targets = new List<IPhysicsAffectable>(8);
        public int TargetCount => targets.Count;
        public EnvironmentProfile Environment { get; private set; }

        public void Configure(IPhysicsAffectable body, EnvironmentProfile profile)
        {
            targets.Clear();
            Environment = profile;
            providers.Clear();
            providers.Add(new GravityForceProvider());
            Register(body);
        }

        public void Register(IPhysicsAffectable body)
        {
            if (body == null || body.Body == null || Environment == null || targets.Contains(body)) return;
            targets.Add(body);
            EnvironmentProfile profile = Environment;
            Rigidbody rb = body.Body;
            rb.useGravity = false;
            rb.linearDamping = profile.LinearDamping;
            rb.angularDamping = profile.AngularDamping;
            rb.maxLinearVelocity = profile.MaxLinearSpeed;
            rb.maxAngularVelocity = profile.MaxAngularSpeed;
        }

        public void Unregister(IPhysicsAffectable body) => targets.Remove(body);

        public void AddProvider(IForceProvider provider)
        {
            if (provider != null && !providers.Contains(provider)) providers.Add(provider);
        }

        public void Clear()
        {
            targets.Clear();
            Environment = null;
            providers.Clear();
        }

        private void FixedUpdate() => Step();

        public void Step()
        {
            if (Environment == null) return;
            for (int t = 0; t < targets.Count; t++)
            {
                IPhysicsAffectable target = targets[t];
                if (target == null || target.Body == null || target.Body.isKinematic || !target.Body.gameObject.activeInHierarchy) continue;
                Rigidbody rb = target.Body;
                Vector3 acceleration = Vector3.zero;
                for (int i = 0; i < providers.Count; i++) acceleration += providers[i].GetAcceleration(target, Environment);
                // Every free body receives the same world acceleration, independent of mass and box pose.
                if (acceleration.sqrMagnitude > 0) rb.AddForce(acceleration, ForceMode.Acceleration);
                rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, Environment.MaxLinearSpeed);
                rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, Environment.MaxAngularSpeed);
            }
        }
    }
}
