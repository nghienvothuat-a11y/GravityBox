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
        public void SpatialMaze_IsAConnectedBranchingNetworkWithAThreeDimensionalMainPath()
        {
            Load(11);
            SpatialMaze maze = levels.Current.GetComponent<SpatialMaze>();
            Assert.That(maze, Is.Not.Null);
            Assert.That(maze.NodesLocal.Length, Is.GreaterThanOrEqualTo(28));
            Assert.That(maze.Edges.Length, Is.EqualTo(maze.NodesLocal.Length - 1));
            Assert.That(maze.MainPath.Length, Is.GreaterThanOrEqualTo(26));
            Assert.That(maze.MainPath[0], Is.EqualTo(maze.SpawnNode));
            Assert.That(maze.MainPath[maze.MainPath.Length - 1], Is.EqualTo(maze.ExitNode));
            Assert.That(levels.Current.GetComponent<LayeredMaze>(), Is.Null);
            Assert.That(levels.Current.GetComponent<MazeLayerView>(), Is.Null);
            var links = SpatialLinks(maze);
            var reached = new HashSet<int> { maze.SpawnNode };
            var pending = new Queue<int>(); pending.Enqueue(maze.SpawnNode);
            int[] previous = new int[maze.NodesLocal.Length];
            for (int i = 0; i < previous.Length; i++) previous[i] = -1;
            while (pending.Count > 0)
            {
                int node = pending.Dequeue();
                foreach (int neighbor in links[node]) if (reached.Add(neighbor))
                {
                    previous[neighbor] = node;
                    pending.Enqueue(neighbor);
                }
            }
            Assert.That(reached.Count, Is.EqualTo(maze.NodesLocal.Length));
            var shortest = new List<int>();
            for (int node = maze.ExitNode; node >= 0; node = previous[node]) shortest.Add(node);
            shortest.Reverse();
            CollectionAssert.AreEqual(maze.MainPath, shortest, "The authored main path must be the actual spawn-to-exit graph path, not padded visits.");
            int branches = 0, deadEnds = 0, turns = 0, previousAxis = -1;
            foreach (List<int> neighbors in links)
            {
                if (neighbors.Count >= 3) branches++;
                if (neighbors.Count == 1) deadEnds++;
            }
            Assert.That(branches, Is.GreaterThanOrEqualTo(2));
            Assert.That(deadEnds, Is.GreaterThanOrEqualTo(4));
            var directions = new HashSet<int>();
            for (int i = 1; i < maze.MainPath.Length; i++)
            {
                Vector3 delta = maze.NodesLocal[maze.MainPath[i]] - maze.NodesLocal[maze.MainPath[i - 1]];
                int axis = SpatialAxis(delta);
                directions.Add(axis * 2 + (delta[axis] > 0 ? 1 : 0));
                if (previousAxis >= 0 && previousAxis != axis) turns++;
                previousAxis = axis;
            }
            Assert.That(turns, Is.GreaterThanOrEqualTo(20));
            Assert.That(directions.Count, Is.EqualTo(6), "A genuine volumetric route uses both signs of all three axes.");
            Assert.That(maze.NodesLocal[maze.ExitNode].magnitude, Is.GreaterThan(maze.InnerRadius + maze.ShellThickness + Radius));
            Vector3 spawn = levels.Current.transform.InverseTransformPoint(levels.Current.BallSpawn.position);
            Assert.That(Vector3.Distance(spawn, maze.NodesLocal[maze.SpawnNode]), Is.LessThan(maze.ClearWidth * 0.5f));
        }

        [Test]
        public void SpatialMaze_ConnectedPassagesStayClearAndTheirSightSeamsCannotReleaseTheBall()
        {
            Load(11);
            SpatialMaze maze = levels.Current.GetComponent<SpatialMaze>();
            Assert.That(maze.ClearWidth, Is.InRange(0.05f, 0.056f));
            Assert.That(maze.SightGap, Is.GreaterThan(0.002f).And.LessThan(Radius * 2));
            Assert.That(maze.PlankThickness, Is.InRange(0.002f, 0.006f));
            Assert.That(maze.PlankWidth, Is.InRange(0.006f, 0.015f));
            Assert.That(maze.JunctionColliders.Length, Is.EqualTo(maze.NodesLocal.Length - 1));
            var links = SpatialLinks(maze);
            var sections = new HashSet<Collider> { maze.ShellCollider };
            foreach (SpatialMazeEdge edge in maze.Edges) sections.Add(edge.Collider);
            foreach (MeshCollider junction in maze.JunctionColliders) sections.Add(junction);
            foreach (Collider collider in levels.Current.GetComponentsInChildren<Collider>())
            {
                Assert.That(sections.Contains(collider), Is.True, "Every physical boundary must belong to a visible maze section or the real shell.");
                Assert.That(collider.enabled, Is.True);
                Assert.That(collider.isTrigger, Is.False);
                Assert.That(collider.attachedRigidbody, Is.SameAs(levels.Current.GetComponent<Rigidbody>()));
            }
            foreach (SpatialMazePlank plank in maze.Planks)
            {
                Assert.That(plank.SectionCollider, Is.Not.Null);
                Assert.That(sections.Contains(plank.SectionCollider), Is.True);
                float[] dimensions = { Mathf.Abs(plank.Size.x), Mathf.Abs(plank.Size.y), Mathf.Abs(plank.Size.z) };
                Array.Sort(dimensions);
                Assert.That(dimensions[0], Is.InRange(0.002f, 0.006f));
                Assert.That(dimensions[2], Is.LessThan(0.30f), "Interior geometry is made of local strips, not large panes dividing the sphere.");
                MeshRenderer renderer = plank.SectionCollider.GetComponent<MeshRenderer>();
                Assert.That(renderer, Is.Not.Null);
                Assert.That(renderer.sharedMaterial.GetFloat("_Surface"), Is.EqualTo(1));
                Assert.That(renderer.sharedMaterial.color.a, Is.InRange(0.005f, 0.35f));
            }
            int sideChecks = 0, sightChecks = 0, cappedDirections = 0, openDirections = 0;
            float half = maze.ClearWidth * 0.5f;
            foreach (SpatialMazeEdge edge in maze.Edges)
            {
                Vector3 a = maze.NodesLocal[edge.A], b = maze.NodesLocal[edge.B];
                Vector3 along = (b - a).normalized;
                Assert.That(SpatialSweepHits(a, b, Radius + 0.0005f), Is.False,
                    $"The whole ball needs clearance from junction {edge.A} to {edge.B}, including the bend entries.");
                Vector3 u = SpatialAxisVector((edge.Axis + 1) % 3), v = SpatialAxisVector((edge.Axis + 2) % 3);
                foreach (float fraction in new[] { 0.3f, 0.5f, 0.7f })
                {
                    Vector3 centre = Vector3.Lerp(a, b, fraction);
                    foreach (Vector3 outward in new[] { u, -u, v, -v })
                    {
                        Vector3 across = Mathf.Abs(Vector3.Dot(outward, u)) > 0.9f ? v : u;
                        foreach (float sign in new[] { -1f, 1f })
                        {
                            float seamOffset = (maze.PlankWidth + maze.SightGap) * 0.5f;
                            Vector3 sightLine = centre + across * seamOffset * sign;
                            Assert.That(SpatialSectionSweep(edge.Collider, sightLine, sightLine + outward * (half + 0.012f), 0.0005f), Is.False,
                                "The narrow sight seam must be a real gap, not invisible collision or a painted line.");
                            sightChecks++;
                            Vector3 outside = sightLine + outward * (half + Radius + 0.012f);
                            Assert.That(SpatialSectionSweep(edge.Collider, outside, sightLine, Radius), Is.True,
                                "The 30 mm sphere cannot pass through a narrow sight seam to bypass the maze.");
                            sideChecks++;
                        }
                    }
                }
            }
            for (int node = 0; node < maze.JunctionColliders.Length; node++)
            for (int axis = 0; axis < 3; axis++)
            foreach (float sign in new[] { -1f, 1f })
            {
                Vector3 direction = SpatialAxisVector(axis) * sign;
                bool open = false;
                foreach (int neighbor in links[node])
                    if (Vector3.Dot((maze.NodesLocal[neighbor] - maze.NodesLocal[node]).normalized, direction) > 0.99f) open = true;
                Vector3 u = SpatialAxisVector((axis + 1) % 3), v = SpatialAxisVector((axis + 2) % 3);
                foreach (Vector2 offset in new[] { Vector2.zero, new Vector2(0.005f, 0.005f), new Vector2(-0.005f, -0.005f) })
                {
                    Vector3 start = maze.NodesLocal[node] + u * offset.x + v * offset.y;
                    bool blocked = SpatialSectionSweep(maze.JunctionColliders[node], start, start + direction * (half + Radius + 0.005f), Radius);
                    Assert.That(blocked, Is.EqualTo(!open), $"Junction {node} has incorrect physical access along {direction}.");
                    if (open) openDirections++; else cappedDirections++;
                }
            }
            Assert.That(sideChecks, Is.GreaterThan(300));
            Assert.That(sightChecks, Is.EqualTo(sideChecks));
            Assert.That(cappedDirections, Is.GreaterThan(100));
            Assert.That(openDirections, Is.GreaterThan(100));
            AssertSpatialShellCoverage(maze);
        }

        [Test]
        public void SpatialMaze_FromSpawnTraversesTheEntireConnectedMazeAndEscapesByRotationOnly()
        {
            Load(11);
            SpatialMaze maze = levels.Current.GetComponent<SpatialMaze>();
            int completions = 0, signals = 0, consecutiveAir = 0, longestAir = 0, contactSteps = 0;
            levels.GameplayEvent += e => { if (e == "level_complete") completions++; };
            levels.Current.Signals.Changed += (_, __) => signals++;
            Vector3 previousPosition = levels.Ball.Body.position;
            float rotationTravel = 0;
            Quaternion previousRotation = levels.Current.Rotation.Orientation;
            Action observe = () =>
            {
                Vector3 position = levels.Ball.Body.position;
                Assert.That(Vector3.Distance(previousPosition, position), Is.LessThan(levels.Definition.Environment.MaxLinearSpeed * Dt + Radius * 0.1f),
                    "Every corridor transition must be continuous physical movement.");
                previousPosition = position;
                Quaternion rotation = levels.Current.Rotation.Orientation;
                rotationTravel += Quaternion.Angle(previousRotation, rotation);
                previousRotation = rotation;
                Assert.That(forces.TargetCount, Is.EqualTo(1));
                Assert.That(forces.Environment.Acceleration, Is.EqualTo(Vector3.down * 9.81f));
                Assert.That(levels.Ball.Body.isKinematic, Is.False);
                if (!levels.Ball.HasContact) { consecutiveAir++; longestAir = Mathf.Max(longestAir, consecutiveAir); }
                else { consecutiveAir = 0; contactSteps++; }
                if (!levels.Current.Exit.HasExited)
                {
                    Assert.That(levels.Current.IsOutside(position), Is.False);
                    Vector3 local = SpatialPhysicalPoint(position);
                    Assert.That(SpatialInsideNetwork(maze, local), Is.True,
                        $"The ball must remain inside the connected network instead of taking an outside shortcut. Local {local:F6}, " +
                        $"world velocity {levels.Ball.Body.linearVelocity:F5}, rotation {levels.Current.Rotation.Orientation.eulerAngles:F3}, contacts {levels.Ball.ContactCount}.");
                }
            };
            Assert.That(Vector3.Distance(levels.Ball.Body.position, levels.Current.BallSpawn.position), Is.LessThan(0.00001f));
            // Only rotation intent is sent after spawn. No pose, velocity, force,
            // gravity, collider or material is changed along the route.
            bool visitedDeadEnd = false;
            List<int>[] routeLinks = SpatialLinks(maze);
            var mainNodes = new HashSet<int>(maze.MainPath);
            for (int i = 1; i < maze.MainPath.Length; i++)
            {
                int from = maze.MainPath[i - 1], to = maze.MainPath[i];
                Vector3 direction = (maze.NodesLocal[to] - maze.NodesLocal[from]).normalized;
                bool final = to == maze.ExitNode;
                if (!final && !visitedDeadEnd)
                {
                    // Exercise a wrong turn before the final uninterrupted drop.
                    // Both the visit and return use the actual connected edge.
                    foreach (int branch in routeLinks[from])
                    {
                        if (mainNodes.Contains(branch)) continue;
                        TestContext.WriteLine($"SPATIAL deliberate dead end: {from} -> {branch}.");
                        DriveSpatialLeg(maze, maze.NodesLocal[from], maze.NodesLocal[branch],
                            (maze.NodesLocal[branch] - maze.NodesLocal[from]).normalized, false, observe);
                        Assert.That(levels.Current.Exit.HasExited, Is.False, "The blind branch must not lead to the outside.");
                        TestContext.WriteLine($"SPATIAL returning from dead end: {branch} -> {from}.");
                        DriveSpatialLeg(maze, maze.NodesLocal[branch], maze.NodesLocal[from],
                            (maze.NodesLocal[from] - maze.NodesLocal[branch]).normalized, false, observe);
                        visitedDeadEnd = true;
                        break;
                    }
                }
                TestContext.WriteLine($"SPATIAL LEG {i}/{maze.MainPath.Length - 1}: {from} -> {to}.");
                DriveSpatialLeg(maze, maze.NodesLocal[from], maze.NodesLocal[to], direction, final, observe);
                if (!final) Assert.That(completions, Is.Zero, "Every main-path junction must be visited before the real exit.");
            }
            Assert.That(visitedDeadEnd, Is.True);
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            Assert.That(levels.Ball.IsCaptured, Is.False);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(signals, Is.Zero);
            Assert.That(contactSteps, Is.GreaterThan(120));
            Assert.That(longestAir, Is.GreaterThanOrEqualTo(3));
            Assert.That(rotationTravel, Is.GreaterThan(1000));
            TestContext.WriteLine($"SPATIAL NETWORK: {maze.MainPath.Length - 1} connected legs, rotation {rotationTravel:F1} degrees, longest free fall {longestAir * Dt:F3}s.");
        }

        [Test]
        public void SpatialMaze_ResetAndUnloadPreserveOnePhysicalBallAndRemoveTheWholeVolume()
        {
            Load(11);
            SpatialMaze maze = levels.Current.GetComponent<SpatialMaze>();
            Vector3[] nodes = (Vector3[])maze.NodesLocal.Clone();
            Vector3 initialSpawn = levels.Current.BallSpawn.position;
            Quaternion initialOrientation = levels.Current.GetComponent<Rigidbody>().rotation;
            Collider[] colliders = levels.Current.GetComponentsInChildren<Collider>();
            for (int i = 0; i < 10; i++)
            {
                levels.Current.Rotation.SetTargetOrientation(Quaternion.Euler(i * 23, i * 41, 60));
                Steps(80);
                levels.ResetLevel();
                // Interpolated render transforms may lag a manual physics reset;
                // verify the physical pose and the following physics step.
                Assert.That(Vector3.Distance(levels.Ball.Body.position, initialSpawn), Is.LessThan(0.00001f));
                Assert.That(Quaternion.Angle(levels.Current.GetComponent<Rigidbody>().rotation, initialOrientation), Is.LessThan(0.001f));
                Assert.That(levels.Ball.Body.linearVelocity.sqrMagnitude + levels.Ball.Body.angularVelocity.sqrMagnitude, Is.Zero);
                Assert.That(levels.Current.Exit.HasExited, Is.False);
                Assert.That(forces.TargetCount, Is.EqualTo(1));
                CollectionAssert.AreEqual(nodes, maze.NodesLocal);
                foreach (Collider collider in colliders) Assert.That(collider.enabled, Is.True);
                Steps(1);
                Assert.That(Quaternion.Angle(levels.Current.GetComponent<Rigidbody>().rotation, initialOrientation), Is.LessThan(0.001f));
                Assert.That(Vector3.Distance(levels.Ball.Body.position, initialSpawn), Is.LessThan(0.002f));
            }
            GameObject oldRoot = levels.Current.gameObject, oldBall = levels.Ball.gameObject;
            Load(0);
            Assert.That(oldRoot.activeSelf || oldBall.activeSelf, Is.False);
            Assert.That(forces.TargetCount, Is.EqualTo(1));
        }

        private static List<int>[] SpatialLinks(SpatialMaze maze)
        {
            var links = new List<int>[maze.NodesLocal.Length];
            for (int i = 0; i < links.Length; i++) links[i] = new List<int>();
            var edges = new HashSet<int>();
            foreach (SpatialMazeEdge edge in maze.Edges)
            {
                Assert.That(edge.A, Is.InRange(0, links.Length - 1));
                Assert.That(edge.B, Is.InRange(0, links.Length - 1));
                Assert.That(edge.A, Is.Not.EqualTo(edge.B));
                Assert.That(edges.Add(Mathf.Min(edge.A, edge.B) * links.Length + Mathf.Max(edge.A, edge.B)), Is.True);
                Vector3 delta = maze.NodesLocal[edge.B] - maze.NodesLocal[edge.A];
                Assert.That(edge.Axis, Is.EqualTo(SpatialAxis(delta)));
                Assert.That(Vector3.ProjectOnPlane(delta, SpatialAxisVector(edge.Axis)).magnitude, Is.LessThan(0.0001f));
                Assert.That(edge.Collider, Is.Not.Null);
                links[edge.A].Add(edge.B); links[edge.B].Add(edge.A);
            }
            return links;
        }

        private bool SpatialSweepHits(Vector3 start, Vector3 end, float radius)
        {
            Vector3 a = levels.Current.transform.TransformPoint(start), b = levels.Current.transform.TransformPoint(end);
            foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(a, radius, (b - a).normalized, Vector3.Distance(a, b), ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider.transform.IsChildOf(levels.Current.transform)) return true;
            return false;
        }

        private bool SpatialSectionSweep(Collider section, Vector3 start, Vector3 end, float radius)
        {
            Vector3 a = levels.Current.transform.TransformPoint(start), b = levels.Current.transform.TransformPoint(end);
            foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(a, radius, (b - a).normalized, Vector3.Distance(a, b), ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider == section) return true;
            return false;
        }

        private static int SpatialAxis(Vector3 delta)
            => Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > Mathf.Abs(delta.z) ? 0 : Mathf.Abs(delta.y) > Mathf.Abs(delta.z) ? 1 : 2;

        private static Vector3 SpatialAxisVector(int axis) => axis == 0 ? Vector3.right : axis == 1 ? Vector3.up : Vector3.forward;

        private bool SpatialInsideNetwork(SpatialMaze maze, Vector3 point)
        {
            // Track the physical section envelope during contact/turning. The
            // erosion of joined passages is not the union of eroded rectangles;
            // full-radius sweeps separately verify their actual clearance.
            float limit = maze.ClearWidth * 0.5f + maze.PlankThickness + 0.001f;
            foreach (Vector3 node in maze.NodesLocal)
            {
                Vector3 d = point - node;
                if (Mathf.Abs(d.x) <= limit && Mathf.Abs(d.y) <= limit && Mathf.Abs(d.z) <= limit) return true;
            }
            foreach (SpatialMazeEdge edge in maze.Edges)
            {
                Vector3 a = maze.NodesLocal[edge.A], b = maze.NodesLocal[edge.B];
                float length = Vector3.Distance(a, b);
                Vector3 along = (b - a) / length;
                float projection = Vector3.Dot(point - a, along);
                if (projection < 0 || projection > length) continue;
                Vector3 lateral = Vector3.ProjectOnPlane(point - a, along);
                int u = (edge.Axis + 1) % 3, v = (edge.Axis + 2) % 3;
                if (Mathf.Abs(lateral[u]) <= limit && Mathf.Abs(lateral[v]) <= limit) return true;
            }
            return false;
        }

        private Vector3 SpatialPhysicalPoint(Vector3 world)
        {
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            return Quaternion.Inverse(box.rotation) * (world - box.position);
        }

        private void DriveSpatialLeg(SpatialMaze maze, Vector3 origin, Vector3 destination, Vector3 along, bool final, Action observe)
        {
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            float best = float.PositiveInfinity;
            bool recovering = false;
            Vector3 recoveryTarget = Vector3.zero, recoveryDirection = Vector3.zero;
            int recoveryStart = 0;
            const int maxTicks = 3600;
            for (int tick = 0; tick < maxTicks; tick++)
            {
                Transform root = levels.Current.transform;
                Vector3 local = SpatialPhysicalPoint(levels.Ball.Body.position);
                Vector3 relative = Quaternion.Inverse(box.rotation) * (levels.Ball.Body.linearVelocity - box.GetPointVelocity(levels.Ball.Body.position));
                Vector3 lateralError = Vector3.ProjectOnPlane(destination - local, along);
                best = Mathf.Min(best, Vector3.Distance(local, destination));
                if (!recovering && lateralError.magnitude > maze.ClearWidth)
                {
                    recoveryTarget = Vector3.SqrMagnitude(local - origin) < Vector3.SqrMagnitude(local - destination) ? origin : destination;
                    Vector3 returnVector = recoveryTarget - local;
                    int axis = SpatialAxis(returnVector);
                    recoveryDirection = SpatialAxisVector(axis) * Mathf.Sign(returnVector[axis]);
                    recovering = true; recoveryStart = tick;
                    TestContext.WriteLine($"SPATIAL backtracking from {local:F5} toward junction {recoveryTarget:F4}, gravity axis {recoveryDirection:F1}.");
                }
                if (recovering)
                {
                    Vector3 delta = local - recoveryTarget;
                    if (Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z)) <= 0.020f &&
                        Vector3.Dot(delta, recoveryDirection) >= -0.005f)
                    {
                        recovering = false;
                        TestContext.WriteLine($"SPATIAL rejoined junction {recoveryTarget:F4} after {(tick - recoveryStart) * Dt:F3}s, ball {local:F5}.");
                    }
                }
                Vector3 gravityAxis = recovering ? recoveryDirection : along;
                Vector3 goal = recovering ? recoveryTarget : destination;
                Vector3 steeringError = Vector3.ProjectOnPlane(goal - local, gravityAxis);
                Vector3 lateralVelocity = Vector3.ProjectOnPlane(relative, gravityAxis);
                Vector3 desiredLocalGravity = gravityAxis * 9.81f + Vector3.ClampMagnitude(steeringError * 24f - lateralVelocity * 8f, 1.5f);
                Vector3 currentLocalGravity = Quaternion.Inverse(box.rotation) * Vector3.down;
                Quaternion localCorrection = Quaternion.FromToRotation(desiredLocalGravity.normalized, currentLocalGravity);
                levels.Current.Rotation.SetTargetOrientation(box.rotation * localCorrection);
                Steps(1); observe();
                Vector3 after = SpatialPhysicalPoint(levels.Ball.Body.position);
                Vector3 afterVelocity = Quaternion.Inverse(box.rotation) * (levels.Ball.Body.linearVelocity - box.GetPointVelocity(levels.Ball.Body.position));
                Vector3 lateral = Vector3.ProjectOnPlane(after - destination, along);
                // Count entry into the real junction while moving. Open turn
                // faces and sight seams make its valid centre space wider than
                // an eroded straight corridor; actual colliders keep the sphere
                // inside, and the separate full-radius sweeps prove clearance.
                float usable = maze.ClearWidth * 0.5f + levels.Ball.Profile.ContactOffset * 2;
                float longitudinal = Vector3.Dot(after - destination, along);
                bool arrived = longitudinal >= -0.005f && longitudinal <= usable &&
                    Mathf.Max(Mathf.Abs(lateral.x), Mathf.Abs(lateral.y), Mathf.Abs(lateral.z)) <= usable;
                if (final ? levels.Current.Exit.HasExited : arrived)
                {
                    TestContext.WriteLine($"SPATIAL node {destination:F4}: {(tick + 1) * Dt:F3}s, ball {after:F5}, speed {afterVelocity.magnitude:F4}m/s.");
                    return;
                }
                if (!final) Assert.That(levels.Current.Exit.HasExited, Is.False, "The ball cannot bypass the remaining connected maze.");
            }
            Transform finalRoot = levels.Current.transform;
            Vector3 finalLocal = SpatialPhysicalPoint(levels.Ball.Body.position);
            Vector3 finalVelocity = Quaternion.Inverse(box.rotation) * (levels.Ball.Body.linearVelocity - box.GetPointVelocity(levels.Ball.Body.position));
            Assert.Fail($"Spatial leg timed out after {maxTicks * Dt:F1}s at {destination:F4}, along {along:F3}: local {finalLocal:F5}, relative velocity {finalVelocity:F5}, " +
                $"local gravity {Quaternion.Inverse(box.rotation) * (Vector3.down * 9.81f):F4}, orientation {box.rotation.eulerAngles:F2}, contacts {levels.Ball.ContactCount}, " +
                $"nearest node distance {best:F5}m, render local {finalRoot.InverseTransformPoint(levels.Ball.Body.position):F5}.");
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
