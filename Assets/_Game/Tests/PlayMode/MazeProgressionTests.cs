#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using NUnit.Framework;
using UnityEngine;
using SessionState = GravityBox.Foundation.SessionState;

namespace GravityBox.Tests
{
    // Shares the real catalog/manual-physics fixture with the earlier regression tests.
    public sealed partial class PhysicsLifecycleTests
    {
        private GravitySliderGuide[] MazeSliders()
        {
            var guides = new List<GravitySliderGuide>();
            foreach (PhysicalProp prop in levels.Current.Props)
            {
                GravitySliderGuide guide = prop.GetComponent<GravitySliderGuide>();
                Assert.That(guide, Is.Not.Null);
                guides.Add(guide);
            }
            guides.Sort((a, b) => a.Body.position.x.CompareTo(b.Body.position.x));
            return guides.ToArray();
        }

        [Test]
        public void MechanicalMaze_TwoPassiveSlidersRespondToOppositeGravityDirections()
        {
            Load(9);
            GravitySliderGuide[] gates = MazeSliders();
            Assert.That(gates, Has.Length.EqualTo(2));
            Assert.That(forces.TargetCount, Is.EqualTo(3));
            Assert.That(Vector3.Dot(gates[0].SlideAxisInBox, gates[1].SlideAxisInBox), Is.LessThan(-0.99f));
            int signals = 0;
            levels.Current.Signals.Changed += (_, __) => signals++;
            levels.Ball.Capture(levels.Ball.Body.position);
            foreach (GravitySliderGuide gate in gates)
            {
                Assert.That(gate.Joint.xDrive.positionSpring + gate.Joint.xDrive.positionDamper + gate.Joint.xDrive.maximumForce, Is.Zero);
                Assert.That(gate.Joint.projectionMode, Is.EqualTo(JointProjectionMode.None));
                Assert.That(gate.GetComponent<Collider>().enabled, Is.True);
                Assert.That(gate.IsPassageClear, Is.False);
            }
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(Vector3.down * 9.81f + gates[0].SlideAxisInBox * 3f, Vector3.down));
            Steps(360);
            Assert.That(gates[0].Displacement, Is.GreaterThan(gates[0].ClearanceDisplacement));
            Assert.That(gates[1].Displacement, Is.LessThan(0.004f));
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(Vector3.down * 9.81f + gates[1].SlideAxisInBox * 3f, Vector3.down));
            Steps(360);
            Assert.That(gates[1].Displacement, Is.GreaterThan(gates[1].ClearanceDisplacement));
            Assert.That(gates[0].Displacement, Is.LessThan(0.004f));
            Assert.That(signals, Is.Zero);
        }

        [Test]
        public void MechanicalMaze_ClosedSlidersBlockWholeSphereAcrossTheFullInteriorDepth()
        {
            Load(9);
            foreach (GravitySliderGuide gate in MazeSliders())
            {
                Collider gateCollider = gate.GetComponent<Collider>();
                Vector3 gateCenter = levels.Current.transform.InverseTransformPoint(gate.Body.position);
                foreach (float y in new[] { -0.027f, 0, 0.027f })
                foreach (float z in new[] { -0.023f, 0, 0.023f })
                {
                    Vector3 origin = levels.Current.transform.TransformPoint(gateCenter + new Vector3(-0.08f, y, z));
                    bool blocked = false;
                    foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(origin, Radius, levels.Current.transform.right, 0.16f, ~0, QueryTriggerInteraction.Ignore))
                        if (hit.collider == gateCollider) blocked = true;
                    Assert.That(blocked, Is.True, gate.name + " must physically block the full sphere at every legal depth.");
                }
            }
        }

        [Test]
        public void MechanicalMaze_ResetRestoresBothSlidersAndUnloadRemovesBothTargets()
        {
            Load(9);
            GravitySliderGuide[] gates = MazeSliders();
            Assert.That(gates, Has.Length.EqualTo(2));
            Vector3[] positions = { gates[0].Body.position, gates[1].Body.position };
            Quaternion[] rotations = { gates[0].Body.rotation, gates[1].Body.rotation };
            for (int iteration = 0; iteration < 100; iteration++)
            {
                levels.Current.GetComponent<Rigidbody>().rotation = Quaternion.Euler(iteration, iteration * 2, 30);
                for (int gate = 0; gate < gates.Length; gate++)
                {
                    gates[gate].Body.position += Vector3.one;
                    gates[gate].Body.linearVelocity = Vector3.one;
                    gates[gate].Body.angularVelocity = Vector3.one * 5;
                }
                levels.ResetLevel();
                for (int gate = 0; gate < gates.Length; gate++)
                {
                    Assert.That(Vector3.Distance(gates[gate].Body.position, positions[gate]), Is.LessThan(0.00001f));
                    Assert.That(Quaternion.Angle(gates[gate].Body.rotation, rotations[gate]), Is.LessThan(0.001f));
                    Assert.That(Mathf.Abs(gates[gate].Displacement), Is.LessThan(0.0001f));
                    Assert.That(gates[gate].Body.linearVelocity.sqrMagnitude + gates[gate].Body.angularVelocity.sqrMagnitude, Is.Zero);
                }
                Assert.That(forces.TargetCount, Is.EqualTo(3));
            }
            GameObject first = gates[0].gameObject, second = gates[1].gameObject;
            Load(0);
            Assert.That(forces.TargetCount, Is.EqualTo(1));
            Assert.That(first.activeSelf || second.activeSelf, Is.False);
        }

        [Test]
        public void MechanicalMaze_FromSpawnUsesBothOpposingSlidersAndEscapesByTiltingOnly()
        {
            Load(9);
            GravitySliderGuide[] gates = MazeSliders();
            int signals = 0;
            levels.Current.Signals.Changed += (_, __) => signals++;
            Vector2[] firstCorridor = {
                new Vector2(-0.200f, -0.240f), new Vector2(-0.200f, -0.156f),
                new Vector2(-0.315f, -0.156f), new Vector2(-0.315f, -0.070f),
                new Vector2(-0.200f, -0.070f), new Vector2(-0.200f, 0.030f)
            };
            foreach (Vector2 point in firstCorridor) TiltMazeTo(point);
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(0, -9.81f, 3), Vector3.down));
            Steps(300);
            Assert.That(gates[0].IsPassageClear, Is.True);
            Assert.That(gates[1].IsPassageClear, Is.False);
            Assert.That(levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position).x, Is.LessThan(-0.17f), "The first recess must hold the ball while gate A moves away.");
            TiltMazeTo(new Vector2(-0.045f, 0.042f));
            TiltMazeTo(new Vector2(0.045f, 0.042f));
            TiltMazeTo(new Vector2(0.045f, -0.080f));
            levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(0, -9.81f, -3), Vector3.down));
            Steps(300);
            Assert.That(gates[1].IsPassageClear, Is.True);
            Assert.That(gates[0].IsPassageClear, Is.False);
            Vector2[] lastCorridor = {
                new Vector2(0.200f, -0.092f), new Vector2(0.200f, 0.065f),
                new Vector2(0.315f, 0.065f), new Vector2(0.315f, 0.145f),
                new Vector2(0.200f, 0.145f), new Vector2(0.200f, 0.240f)
            };
            foreach (Vector2 point in lastCorridor) TiltMazeTo(point);
            Vector3 exit = levels.Current.transform.InverseTransformPoint(levels.Current.Exit.transform.position);
            TiltMazeTo(new Vector2(exit.x, exit.z), () => levels.Current.Exit.HasExited);
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Assert.That(signals, Is.Zero);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
        }

        [Test]
        public void LayeredMaze_HasThreeSolidDecksAndPhysicalStaggeredTransferHoles()
        {
            Load(10);
            LayeredMaze maze = levels.Current.GetComponent<LayeredMaze>();
            Assert.That(maze, Is.Not.Null);
            Assert.That(maze.Decks, Has.Length.EqualTo(3));
            Assert.That(maze.TransferPorts, Has.Length.EqualTo(2));
            Assert.That(maze.TransferRadius, Is.GreaterThan(Radius * 2));
            Assert.That(levels.Current.InteriorDepth, Is.EqualTo(0.27f).Within(0.0001f));
            Assert.That(Vector2.Distance(new Vector2(maze.TransferPorts[0].localPosition.x, maze.TransferPorts[0].localPosition.z),
                new Vector2(maze.TransferPorts[1].localPosition.x, maze.TransferPorts[1].localPosition.z)), Is.GreaterThan(0.4f));
            for (int deck = 0; deck < 3; deck++)
            {
                float floorHeight = maze.Decks[deck].FloorHeight;
                Assert.That(floorHeight, Is.EqualTo(0.045f - deck * 0.09f).Within(0.0001f));
                Collider floor = maze.Decks[deck].FloorCollider;
                Assert.That(floor, Is.Not.Null);
                Vector3 port = deck < 2 ? levels.Current.transform.InverseTransformPoint(maze.TransferPorts[deck].position)
                    : levels.Current.transform.InverseTransformPoint(levels.Current.Exit.transform.position);
                Vector3 origin = levels.Current.transform.TransformPoint(new Vector3(port.x, floorHeight + 0.003f + Radius + 0.002f, port.z));
                foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(origin, Radius, -levels.Current.transform.up, Radius * 2 + 0.014f, ~0, QueryTriggerInteraction.Ignore))
                    Assert.That(hit.collider, Is.Not.SameAs(floor), "The entire sphere must fit through the physical transfer hole.");
                Vector3 solid = maze.Decks[deck].RouteLocalPoints[0];
                solid.y = floorHeight + 0.003f + Radius + 0.002f;
                bool blocked = false;
                foreach (RaycastHit hit in UnityEngine.Physics.SphereCastAll(levels.Current.transform.TransformPoint(solid), Radius,
                    -levels.Current.transform.up, Radius * 2 + 0.014f, ~0, QueryTriggerInteraction.Ignore))
                    if (hit.collider == floor) blocked = true;
                Assert.That(blocked, Is.True, "A solid deck cannot be crossed away from its transfer hole.");

                // Isolate physical support and transfer with controlled initial states.
                // The separate route proof below starts at the real spawn without repositioning.
                levels.ResetLevel();
                levels.Ball.Body.position = levels.Current.transform.TransformPoint(solid);
                levels.Ball.Body.linearVelocity = Vector3.zero;
                UnityEngine.Physics.SyncTransforms(); levels.Current.Exit.BeginTracking();
                Steps(36);
                Assert.That(levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position).y, Is.GreaterThan(floorHeight + Radius * 0.9f));
                levels.ResetLevel();
                levels.Ball.Body.position = origin;
                levels.Ball.Body.linearVelocity = -levels.Current.transform.up * 0.2f;
                UnityEngine.Physics.SyncTransforms(); levels.Current.Exit.BeginTracking();
                Steps(24);
                Assert.That(levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position).y, Is.LessThan(floorHeight - Radius - 0.003f));
                if (deck < 2) Assert.That(levels.Current.Exit.HasExited, Is.False, "Dropping between decks is not yet an exit.");
            }
        }

        [Test]
        public void LayeredMaze_FromSpawnTraversesAllThreeMazesThenEscapesWithoutTeleporting()
        {
            Load(10);
            LayeredMaze maze = levels.Current.GetComponent<LayeredMaze>();
            Assert.That(maze, Is.Not.Null);
            var visited = new HashSet<int> { maze.GetLayerIndex(levels.Ball.Body.position) };
            Vector3 previous = levels.Ball.Body.position;
            Action observe = () =>
            {
                Vector3 current = levels.Ball.Body.position;
                Assert.That(Vector3.Distance(previous, current), Is.LessThan(levels.Definition.Environment.MaxLinearSpeed * Dt + Radius * 0.1f),
                    "A layer transition must be continuous physical motion, not a teleport.");
                previous = current;
                visited.Add(maze.GetLayerIndex(current));
            };
            Assert.That(maze.GetLayerIndex(levels.Ball.Body.position), Is.Zero);
            for (int deck = 0; deck < maze.Decks.Length; deck++)
            {
                Vector3[] points = maze.Decks[deck].RouteLocalPoints;
                Assert.That(points.Length, Is.GreaterThan(8), "Each deck should require a real maze route.");
                for (int waypoint = 0; waypoint < points.Length; waypoint++)
                {
                    Vector2 goal = new Vector2(points[waypoint].x, points[waypoint].z);
                    if (waypoint + 1 < points.Length) TiltMazeTo(goal, observe: observe);
                    else if (deck < 2)
                    {
                        float floor = maze.Decks[deck].FloorHeight;
                        TiltMazeTo(goal, () => levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position).y < floor - Radius - 0.003f, observe);
                        Assert.That(levels.Current.Exit.HasExited, Is.False);
                    }
                    else TiltMazeTo(goal, () => levels.Current.Exit.HasExited, observe);
                }
            }
            CollectionAssert.IsSubsetOf(new[] { 0, 1, 2 }, visited);
            Assert.That(levels.Current.Exit.HasExited, Is.True);
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
        }

        [Test]
        public void LayerView_FollowsActualBodyHeightAndNeverChangesPhysicsState()
        {
            Load(10);
            LayeredMaze maze = levels.Current.GetComponent<LayeredMaze>();
            var view = levels.Current.gameObject.AddComponent<GravityBox.Presentation.MazeLayerView>();
            view.Initialize(maze, levels.Ball);
            Collider[] colliders = levels.Current.GetComponentsInChildren<Collider>();
            for (int deck = 0; deck < maze.Decks.Length; deck++)
            {
                Vector3 position = levels.Current.transform.TransformPoint(maze.Decks[deck].RouteLocalPoints[0]);
                levels.Ball.Body.position = position;
                Vector3 velocity = new Vector3(0.02f, -0.01f, 0.03f);
                levels.Ball.Body.linearVelocity = velocity;
                view.Refresh();
                Assert.That(view.ActiveLayer, Is.EqualTo(deck));
                foreach (bool overview in new[] { true, false })
                {
                    view.SetOverview(overview);
                    Assert.That(view.Overview, Is.EqualTo(overview));
                    Assert.That(levels.Ball.Body.position, Is.EqualTo(position));
                    Assert.That(levels.Ball.Body.linearVelocity, Is.EqualTo(velocity));
                    Assert.That(levels.Ball.Body.isKinematic, Is.False);
                    foreach (Collider collider in colliders) Assert.That(collider.enabled, Is.True);
                }
            }
        }

        private void TiltMazeTo(Vector2 goal, Func<bool> completed = null, Action observe = null, int maxTicks = 1800)
        {
            Rigidbody box = levels.Current.GetComponent<Rigidbody>();
            float closest = float.PositiveInfinity;
            for (int tick = 0; tick < maxTicks; tick++)
            {
                Transform root = levels.Current.transform;
                Vector3 local = root.InverseTransformPoint(levels.Ball.Body.position);
                Vector3 relative = root.InverseTransformDirection(levels.Ball.Body.linearVelocity - box.GetPointVelocity(levels.Ball.Body.position));
                Vector2 error = goal - new Vector2(local.x, local.z);
                Vector2 velocity = new Vector2(relative.x, relative.z);
                closest = Mathf.Min(closest, error.magnitude);
                Vector2 acceleration = Vector2.ClampMagnitude(error * 8f - velocity * 5f, 1f);
                levels.Current.Rotation.SetTargetOrientation(Quaternion.FromToRotation(new Vector3(acceleration.x, -9.81f, acceleration.y), Vector3.down));
                Steps(1);
                observe?.Invoke();
                bool reached = completed != null ? completed() : error.magnitude < Radius * 1.1f && velocity.magnitude < 0.16f;
                if (reached)
                {
                    TestContext.WriteLine($"MAZE {levels.Definition.Id}, goal {goal:F4}: {(tick + 1) * Dt:F3}s, ball {local:F5}.");
                    return;
                }
                Assert.That(levels.Current.IsOutside(levels.Ball.Body.position), Is.False, "A maze route must stay inside until a valid exit.");
            }
            Vector3 final = levels.Current.transform.InverseTransformPoint(levels.Ball.Body.position);
            Assert.Fail($"Maze {levels.Definition.Id} cannot reach {goal:F4}; ball {final:F5}, closest {closest:F5}m.");
        }
    }
}
#endif
