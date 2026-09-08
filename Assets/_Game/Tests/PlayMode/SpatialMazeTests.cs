#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using NUnit.Framework;
using UnityEngine;
using SessionState = GravityBox.Foundation.SessionState;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        [Test]
        public void SpatialMaze_UsesSmallIndependentTransparentPlanksWithOpenAirBetweenThem()
        {
            Load(11);
            SpatialMaze maze = levels.Current.GetComponent<SpatialMaze>();
            Assert.That(maze, Is.Not.Null);
            Assert.That(maze.Planks, Has.Length.EqualTo(32));
            Assert.That(maze.SpawnPlank, Is.Not.Null);
            Assert.That(maze.CatchPlank, Is.Not.Null);
            Assert.That(maze.SpawnPlank, Is.Not.SameAs(maze.CatchPlank));
            Assert.That(levels.Current.GetComponent<LayeredMaze>(), Is.Null);
            Assert.That(levels.Current.GetComponent<MazeLayerView>(), Is.Null);
            Assert.That(levels.Current.GetComponentsInChildren<MeshCollider>(), Has.Length.EqualTo(1),
                "Only the curved outer shell uses a mesh collider; the interior is a set of solid, finite planks.");
            Assert.That(levels.Current.GetComponentsInChildren<Collider>(), Has.Length.EqualTo(maze.Planks.Length + 1),
                "There must be no hidden corridor, cell, deck or transfer-hole colliders.");
            float solidVolume = 0;
            var normals = new List<Vector3>();
            var seen = new HashSet<BoxCollider>();
            foreach (BoxCollider plank in maze.Planks)
            {
                Assert.That(plank, Is.Not.Null);
                Assert.That(seen.Add(plank), Is.True);
                Assert.That(plank.enabled, Is.True);
                Assert.That(plank.isTrigger, Is.False);
                Assert.That(plank.attachedRigidbody, Is.SameAs(levels.Current.GetComponent<Rigidbody>()));
                Vector3 dimensions = Vector3.Scale(plank.size, plank.transform.lossyScale);
                float[] ordered = { Mathf.Abs(dimensions.x), Mathf.Abs(dimensions.y), Mathf.Abs(dimensions.z) };
                Array.Sort(ordered);
                Assert.That(ordered[0], Is.InRange(0.002f, 0.008f));
                Assert.That(ordered[1], Is.InRange(0.012f, 0.070f));
                Assert.That(ordered[2], Is.InRange(0.045f, 0.170f), "A plank must stay small relative to the 72 cm sphere.");
                solidVolume += ordered[0] * ordered[1] * ordered[2];
                MeshRenderer renderer = plank.GetComponent<MeshRenderer>();
                Assert.That(renderer, Is.Not.Null);
                Assert.That(renderer.sharedMaterial.GetFloat("_Surface"), Is.EqualTo(1));
                Assert.That(renderer.sharedMaterial.color.a, Is.InRange(0.005f, 0.35f));
                Vector3 normal = plank.transform.up;
                bool distinct = true;
                foreach (Vector3 previous in normals) if (Mathf.Abs(Vector3.Dot(normal, previous)) > 0.985f) distinct = false;
                if (distinct) normals.Add(normal);
            }
            Assert.That(normals.Count, Is.GreaterThanOrEqualTo(8), "The free planks should occupy varied planes rather than repeated horizontal decks.");
            float sphereVolume = 4f / 3f * Mathf.PI * Mathf.Pow(maze.InnerRadius, 3);
            Assert.That(solidVolume / sphereVolume, Is.LessThan(0.01f));
            int samples = 0, clear = 0;
            for (int x = -2; x <= 2; x++)
            for (int y = -2; y <= 2; y++)
            for (int z = -2; z <= 2; z++)
            {
                Vector3 local = new Vector3(x, y, z) * 0.09f;
                if (local.magnitude > maze.InnerRadius * 0.8f) continue;
                samples++;
                if (SpatialAirAt(local)) clear++;
            }
            Assert.That(samples, Is.GreaterThan(80));
            Assert.That(clear / (float)samples, Is.GreaterThan(0.65f), "Most of the central sphere must be usable open air, not a captive tunnel network.");
            Vector3 start = levels.Current.transform.InverseTransformPoint(maze.SpawnPlank.bounds.center);
            Vector3 catchPoint = levels.Current.transform.InverseTransformPoint(maze.CatchPlank.bounds.center);
            Assert.That(start.y - catchPoint.y, Is.GreaterThan(Radius * 6));
            Assert.That(SpatialAirAt(Vector3.Lerp(start, catchPoint, 0.5f)), Is.True,
                "There must be a real sphere-sized air gap between the first two supports.");
        }

        [Test]
        public void SpatialMaze_BallLeavesTheStartPlankFallsUnderEarthGravityAndLandsOnTheCatchPlank()
        {
            Load(11);
            SpatialMaze maze = levels.Current.GetComponent<SpatialMaze>();
            Steps(120);
            Assert.That(TouchesSpatialPlank(maze.SpawnPlank), Is.True);
            int consecutiveAir = 0, longestAir = 0, measuredAirSteps = 0;
            bool wasAir = false;
            Vector3 previousVelocity = levels.Ball.Body.linearVelocity;
            float highestFlightY = float.NegativeInfinity, lowestFlightY = float.PositiveInfinity;
            Action observe = () =>
            {
                bool inAir = !levels.Ball.HasContact;
                if (inAir)
                {
                    consecutiveAir++;
                    longestAir = Mathf.Max(longestAir, consecutiveAir);
                    Vector3 local = levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position);
                    highestFlightY = Mathf.Max(highestFlightY, local.y);
                    lowestFlightY = Mathf.Min(lowestFlightY, local.y);
                    if (wasAir)
                    {
                        Vector3 acceleration = (levels.Ball.Body.linearVelocity - previousVelocity) / Dt;
                        Assert.That(Vector3.Distance(acceleration, Vector3.down * 9.81f), Is.LessThan(0.05f),
                            "Between supports the free body must receive Earth gravity, not a trajectory correction.");
                        measuredAirSteps++;
                    }
                }
                else consecutiveAir = 0;
                wasAir = inAir;
                previousVelocity = levels.Ball.Body.linearVelocity;
            };
            DriveSpatialGravity(new Vector3(0, -9.81f, 1), () => TouchesSpatialPlank(maze.CatchPlank), observe, "first free fall", 1200, 2.5f);
            Assert.That(longestAir, Is.GreaterThanOrEqualTo(8));
            Assert.That(measuredAirSteps, Is.GreaterThanOrEqualTo(6));
            Assert.That(highestFlightY - lowestFlightY, Is.GreaterThan(0.07f));
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Assert.That(levels.Current.Exit.HasExited, Is.False);
            TestContext.WriteLine($"SPATIAL FREE FALL: {longestAir * Dt:F3}s uninterrupted air, measured drop {highestFlightY - lowestFlightY:F4}m before landing on the catch plank.");
        }

        [Test]
        public void SpatialMaze_FromSpawnUsesPlankContactsAndFreeFallsThenEscapesByRotationOnly()
        {
            Load(11);
            SpatialMaze maze = levels.Current.GetComponent<SpatialMaze>();
            int completions = 0, signals = 0, consecutiveAir = 0, longestAir = 0;
            levels.GameplayEvent += e => { if (e == "level_complete") completions++; };
            levels.Current.Signals.Changed += (_, __) => signals++;
            Assert.That(Vector3.Distance(levels.Ball.Body.position, levels.Current.BallSpawn.position), Is.LessThan(0.00001f));
            Vector3 previousPosition = levels.Ball.Body.position;
            var touched = new HashSet<BoxCollider>();
            Action observe = () =>
            {
                Vector3 position = levels.Ball.Body.position;
                Assert.That(Vector3.Distance(previousPosition, position), Is.LessThan(levels.Definition.Environment.MaxLinearSpeed * Dt + Radius * 0.1f),
                    "A gap between planks must be traversed by continuous physical movement.");
                previousPosition = position;
                Assert.That(forces.TargetCount, Is.EqualTo(1));
                Assert.That(forces.Environment.Acceleration, Is.EqualTo(Vector3.down * 9.81f));
                Assert.That(levels.Ball.Body.isKinematic, Is.False);
                if (!levels.Ball.HasContact) { consecutiveAir++; longestAir = Mathf.Max(longestAir, consecutiveAir); }
                else consecutiveAir = 0;
                foreach (BoxCollider plank in maze.Planks) if (TouchesSpatialPlank(plank)) touched.Add(plank);
            };
            // This is one admissible route through the open sculpture. It starts
            // at the authored spawn and never writes ball pose, velocity, forces,
            // gravity or collider state; other free-fall routes remain valid.
            for (int tick = 0; tick < 120; tick++) { Steps(1); observe(); }
            DriveSpatialGravity(new Vector3(0, -9.81f, 1), () => TouchesSpatialPlank(maze.CatchPlank), observe, "catch plank", 1800, 2.5f);
            DriveSpatialGravity(new Vector3(0, -9.81f, 2),
                () => levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position).y < -0.325f,
                observe, "below the exit baffles", 2400);
            DriveSpatialExit(observe);
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            Assert.That(levels.Ball.IsCaptured, Is.False);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(signals, Is.Zero);
            Assert.That(touched, Does.Contain(maze.SpawnPlank));
            Assert.That(touched, Does.Contain(maze.CatchPlank));
            Assert.That(longestAir, Is.GreaterThanOrEqualTo(8));
            TestContext.WriteLine($"SPATIAL PLANK ROUTE: touched {touched.Count} distinct planks, longest flight {longestAir * Dt:F3}s, valid whole-ball exit.");
        }

        [Test]
        public void SpatialMaze_ResetAndUnloadPreserveOnePhysicalBallAndRemoveTheWholeVolume()
        {
            Load(11);
            SpatialMaze maze = levels.Current.GetComponent<SpatialMaze>();
            var positions = new Vector3[maze.Planks.Length];
            var rotations = new Quaternion[maze.Planks.Length];
            for (int p = 0; p < maze.Planks.Length; p++)
            {
                positions[p] = maze.Planks[p].transform.localPosition;
                rotations[p] = maze.Planks[p].transform.localRotation;
            }
            Vector3 initialSpawn = levels.Current.BallSpawn.position;
            Quaternion initialOrientation = levels.Current.GetComponent<Rigidbody>().rotation;
            Collider[] colliders = levels.Current.GetComponentsInChildren<Collider>();
            for (int i = 0; i < 10; i++)
            {
                levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(i * 23, i * 41, 60));
                Steps(80);
                levels.ResetLevel();
                // Interpolated render transforms may still show the preceding
                // rotation in a manual-physics fixture; assert the physical pose.
                Assert.That(Vector3.Distance(levels.Ball.Body.position, initialSpawn), Is.LessThan(0.00001f));
                Assert.That(Quaternion.Angle(levels.Current.GetComponent<Rigidbody>().rotation, initialOrientation), Is.LessThan(0.001f));
                Assert.That(levels.Ball.Body.linearVelocity.sqrMagnitude + levels.Ball.Body.angularVelocity.sqrMagnitude, Is.Zero);
                Assert.That(levels.Current.Exit.HasExited, Is.False);
                Assert.That(forces.TargetCount, Is.EqualTo(1));
                for (int p = 0; p < maze.Planks.Length; p++)
                {
                    Assert.That(maze.Planks[p].transform.localPosition, Is.EqualTo(positions[p]));
                    Assert.That(Quaternion.Angle(maze.Planks[p].transform.localRotation, rotations[p]), Is.LessThan(0.001f));
                }
                foreach (Collider collider in colliders) Assert.That(collider.enabled, Is.True);
                Steps(1);
                Assert.That(Quaternion.Angle(levels.Current.GetComponent<Rigidbody>().rotation, initialOrientation), Is.LessThan(0.001f),
                    "A pending rotation command must not be reapplied after reset.");
                Assert.That(Vector3.Distance(levels.Ball.Body.position, initialSpawn), Is.LessThan(0.002f),
                    "The first reset step permits gravity displacement, not a stale-contact kick.");
            }
            GameObject oldRoot = levels.Current.gameObject, oldBall = levels.Ball.gameObject;
            Load(0);
            Assert.That(oldRoot.activeSelf || oldBall.activeSelf, Is.False);
            Assert.That(forces.TargetCount, Is.EqualTo(1));
        }

        private bool SpatialAirAt(Vector3 localPoint)
        {
            Vector3 world = levels.Current.transform.TransformPoint(localPoint);
            foreach (Collider collider in UnityEngine.Physics.OverlapSphere(world, Radius, ~0, QueryTriggerInteraction.Ignore))
                if (collider.transform.IsChildOf(levels.Current.transform)) return false;
            return true;
        }

        private bool TouchesSpatialPlank(BoxCollider plank)
            => levels.Ball.HasContact && Vector3.Distance(plank.ClosestPoint(levels.Ball.Body.position), levels.Ball.Body.position) < Radius + 0.0015f;

        private void SetSpatialGravity(Vector3 desiredLocalGravity)
        {
            Transform root = levels.Current.transform;
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            Vector3 currentLocalGravity = root.InverseTransformDirection(Vector3.down);
            Quaternion localCorrection = Quaternion.FromToRotation(desiredLocalGravity.normalized, currentLocalGravity);
            levels.Current.Rotation.SetTargetOrientation(box.rotation * localCorrection);
        }

        private void DriveSpatialGravity(Vector3 gravityDirection, Func<bool> completed, Action observe, string phase, int maxTicks, float rampSeconds = 0)
        {
            bool previousContact = levels.Ball.HasContact;
            for (int tick = 0; tick < maxTicks; tick++)
            {
                // Gradual hand rotation keeps the high starting plank from
                // sweeping sideways out from under the ball before it rolls.
                float fraction = rampSeconds > 0 ? Mathf.SmoothStep(0, 1, tick * Dt / rampSeconds) : 1;
                SetSpatialGravity(Vector3.Lerp(Vector3.down * 9.81f, gravityDirection, fraction));
                Steps(1); observe();
                if (levels.Ball.HasContact != previousContact)
                {
                    Transform root = levels.Current.transform;
                    TestContext.WriteLine($"SPATIAL {phase} contact={levels.Ball.HasContact} at {(tick + 1) * Dt:F3}s, " +
                        $"ball {root.InverseTransformPoint(levels.Ball.Body.position):F5}, velocity {root.InverseTransformDirection(levels.Ball.Body.linearVelocity):F5}.");
                    previousContact = levels.Ball.HasContact;
                }
                if (completed())
                {
                    TestContext.WriteLine($"SPATIAL {phase}: {(tick + 1) * Dt:F3}s, ball local {levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position):F5}.");
                    return;
                }
                Assert.That(levels.Current.Exit.HasExited, Is.False, "The fixture must observe its intended physical contacts before exiting.");
                Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False);
            }
            SpatialRouteFailed(phase, maxTicks);
        }

        private void DriveSpatialExit(Action observe)
        {
            Vector3 exit = levels.Current.transform.InverseTransformPoint(levels.Current.Exit.transform.position);
            Vector3 normal = levels.Current.transform.InverseTransformDirection(levels.Current.Exit.transform.forward);
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            for (int tick = 0; tick < 2400; tick++)
            {
                Transform root = levels.Current.transform;
                Vector3 local = root.InverseTransformPoint(levels.Ball.Body.position);
                Vector3 relative = root.InverseTransformDirection(levels.Ball.Body.linearVelocity - box.GetPointVelocity(levels.Ball.Body.position));
                Vector3 error = Vector3.ProjectOnPlane(exit - local, normal);
                Vector3 velocity = Vector3.ProjectOnPlane(relative, normal);
                SetSpatialGravity(normal * 9.81f + Vector3.ClampMagnitude(error * 8f - velocity * 5f, 1f));
                Steps(1); observe();
                if (levels.Current.Exit.HasExited)
                {
                    TestContext.WriteLine($"SPATIAL final exit: {(tick + 1) * Dt:F3}s, ball local {root.InverseTransformPoint(levels.Ball.Body.position):F5}.");
                    return;
                }
                Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False);
            }
            SpatialRouteFailed("final exit", 2400);
        }

        private void SpatialRouteFailed(string phase, int ticks)
        {
            Transform root = levels.Current.transform;
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            Vector3 local = root.InverseTransformPoint(levels.Ball.Body.position);
            Vector3 relative = root.InverseTransformDirection(levels.Ball.Body.linearVelocity - box.GetPointVelocity(levels.Ball.Body.position));
            Assert.Fail($"Spatial route timed out during {phase} after {ticks * Dt:F3}s: ball local {local:F5}, relative velocity {relative:F5}, " +
                $"local gravity {root.InverseTransformDirection(Vector3.down * 9.81f):F4}, box Euler {box.rotation.eulerAngles:F2}, contacts {levels.Ball.ContactCount}.");
        }

        private void AssertSpatialShellCoverage(SpatialMaze maze)
        {
            Assert.That(maze.ShellCollider, Is.Not.Null);
            Vector3 outletNormal = levels.Current.transform.InverseTransformDirection(levels.Current.Exit.transform.forward);
            Quaternion sampleOrientation = Quaternion.FromToRotation(Vector3.down, outletNormal);
            int samples = 0;
            // Only the shell collider is queried: interior planks cannot conceal
            // a leak. The sampling frame follows the real exit orientation.
            for (int latitude = 0; latitude <= 16; latitude++)
            for (int longitude = 0; longitude < 24; longitude++)
            {
                float theta = (3f + latitude * 10.5f) * Mathf.Deg2Rad;
                float phi = (longitude * 15f + 3.7f) * Mathf.Deg2Rad;
                Vector3 radial = sampleOrientation * new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));
                Vector3 origin = levels.Current.transform.TransformPoint(radial * (maze.InnerRadius - Radius - 0.008f));
                Vector3 direction = levels.Current.transform.TransformDirection(radial);
                bool blocked = false;
                foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(origin, Radius, direction, Radius * 2 + maze.ShellThickness + 0.016f, ~0, QueryTriggerInteraction.Ignore))
                    if (hit.collider == maze.ShellCollider)
                    {
                        blocked = true;
                        float radius = levels.Current.transform.InverseTransformPoint(hit.point).magnitude;
                        Assert.That(radius, Is.EqualTo(maze.InnerRadius).Within(0.0015f));
                    }
                Assert.That(blocked, Is.True, "The complete ball must meet the spherical shell at " + radial.ToString("F3"));
                samples++;
            }
            Assert.That(samples, Is.GreaterThan(350));
        }

        private void AssertSpatialShellSection(SpatialMaze maze)
        {
            Assert.That(levels.Current.InteriorDepth, Is.EqualTo(2 * (maze.InnerRadius + maze.ShellThickness)).Within(0.0001f));
            Transform root = levels.Current.transform;
            Vector3 outletNormal = root.InverseTransformDirection(levels.Current.Exit.transform.forward);
            Quaternion sampleOrientation = Quaternion.FromToRotation(Vector3.down, outletNormal);
            Vector3 rayDirection = root.TransformDirection(outletNormal);
            int covered = 0, empty = 0;
            float outer = maze.InnerRadius + maze.ShellThickness;
            for (float x = -outer - 0.04f; x <= outer + 0.04f; x += 0.03f)
            for (float z = -outer - 0.04f; z <= outer + 0.04f; z += 0.03f)
            {
                float radial = Mathf.Sqrt(x * x + z * z);
                if (Mathf.Abs(radial - outer) < 0.008f) continue;
                Vector3 origin = root.TransformPoint(sampleOrientation * new Vector3(x, outer + 0.04f, z));
                bool hit = maze.ShellCollider.Raycast(new Ray(origin, rayDirection), out RaycastHit surface, outer * 2 + 0.08f);
                Assert.That(hit, Is.EqualTo(radial < outer), "The shell silhouette must follow a sphere, including its empty corners.");
                if (hit)
                {
                    covered++;
                    Assert.That(root.InverseTransformPoint(surface.point).magnitude, Is.EqualTo(outer).Within(0.0015f));
                }
                else empty++;
            }
            Assert.That(covered, Is.GreaterThan(100));
            Assert.That(empty, Is.GreaterThan(100));
            bool blocked = maze.ShellCollider.Raycast(new Ray(root.position, rayDirection), out _, outer + 0.03f);
            Assert.That(blocked, Is.False, "The real exit must cut through both shell surfaces.");
        }
    }
}
#endif
