using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    /// <summary>Probe (Explicit): in this Unity version, does an interpolated Rigidbody's transform equal its physics pose
    /// while FixedUpdate runs? Campaign simulation reads moving faces through transforms, so rails may only be
    /// interpolated if it does.</summary>
    public sealed class COgheInterpolationProbeTests
    {
        private sealed class Probe : MonoBehaviour
        {
            public Rigidbody Body; public float MaxGap; public int Steps;
            private void FixedUpdate()
            {
                if (Steps > 0) MaxGap = Mathf.Max(MaxGap, Vector3.Distance(Body.transform.position, Body.position));
                Body.linearVelocity = new Vector3(.3f, 0, 0); Steps++;
            }
        }

        [UnityTest, Explicit]
        public IEnumerator InterpolatedTransformMatchesBodyInFixedUpdate()
        {
            var simulation = Physics.simulationMode; Physics.simulationMode = SimulationMode.FixedUpdate;
            float fixedStep = Time.fixedDeltaTime; Time.fixedDeltaTime = 1f / 120;
            var go = new GameObject("Interpolated probe body");
            var body = go.AddComponent<Rigidbody>(); body.useGravity = false; body.interpolation = RigidbodyInterpolation.Interpolate;
            var probe = go.AddComponent<Probe>(); probe.Body = body;
            try
            {
                for (int i = 0; i < 90; i++) { Time.captureDeltaTime = i % 3 == 0 ? 1f / 30 : 1f / 70; yield return null; }
                Debug.Log($"INTERPOLATION PROBE: steps={probe.Steps} max transform-vs-body gap in FixedUpdate = {probe.MaxGap * 1000:F3} mm");
                Assert.Greater(probe.Steps, 20);
                Assert.Less(probe.MaxGap, 1e-5f, "FixedUpdate sees the interpolated transform, not the physics pose");
            }
            finally
            {
                Time.captureDeltaTime = 0; Time.fixedDeltaTime = fixedStep; Physics.simulationMode = simulation; Object.Destroy(go);
            }
        }
    }
}
