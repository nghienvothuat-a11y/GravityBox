using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Simulation
{
    public interface IPhysicsAffectable
    {
        Rigidbody Body { get; }
        BallPhysicsProfile Profile { get; }
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
        private IPhysicsAffectable target;
        public EnvironmentProfile Environment { get; private set; }

        public void Configure(IPhysicsAffectable body, EnvironmentProfile profile)
        {
            target = body;
            Environment = profile;
            providers.Clear();
            providers.Add(new GravityForceProvider());
            Rigidbody rb = body.Body;
            rb.useGravity = false;
            rb.linearDamping = profile.LinearDamping;
            rb.angularDamping = profile.AngularDamping;
            rb.maxLinearVelocity = profile.MaxLinearSpeed;
            rb.maxAngularVelocity = profile.MaxAngularSpeed;
        }

        public void AddProvider(IForceProvider provider)
        {
            if (provider != null && !providers.Contains(provider)) providers.Add(provider);
        }

        public void Clear()
        {
            target = null;
            Environment = null;
            providers.Clear();
        }

        private void FixedUpdate() => Step();

        public void Step()
        {
            if (target == null || target.Body == null || target.Body.isKinematic || Environment == null) return;
            Rigidbody rb = target.Body;
            Vector3 acceleration = Vector3.zero;
            for (int i = 0; i < providers.Count; i++) acceleration += providers[i].GetAcceleration(target, Environment);
            // ForceMode.Acceleration already integrates fixedDeltaTime and ignores mass.
            if (acceleration.sqrMagnitude > 0) rb.AddForce(acceleration, ForceMode.Acceleration);
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, Environment.MaxLinearSpeed);
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, Environment.MaxAngularSpeed);
        }
    }
}
