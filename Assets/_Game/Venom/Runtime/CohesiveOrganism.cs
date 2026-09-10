using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    // The fixed set of material particles conserves mass through every cut/fusion.
    // Connectivity changes; no replacement ball, animation-driven collider or teleport.
    public sealed class CohesiveOrganism : MonoBehaviour
    {
        public const int ParticleCount = 32;
        public VenomProfile Profile { get; private set; }
        public Rigidbody[] Bodies { get; private set; }
        public bool[] Escaped { get; private set; }
        public int[] Groups { get; private set; }
        public int FragmentCount { get; private set; }
        public int CutCount { get; private set; }
        public int MergeCount { get; private set; }
        public int EscapedCount { get; private set; }
        public float SimulationTime { get; private set; }
        public float FusionGlow { get; private set; }
        public float TotalMass => ParticleCount * Profile.ParticleMass;
        public IReadOnlyList<Bond> Bonds => bonds;
        public event Action Split;
        public event Action Fused;
        public sealed class Bond { public int A, B; public float Rest, Strength; }
        private readonly List<Bond> bonds = new List<Bond>(200);
        private readonly bool[,] connected = new bool[ParticleCount, ParticleCount];
        private readonly float[] healAt = new float[ParticleCount];
        private readonly int[] parents = new int[ParticleCount];
        private readonly int[] roots = new int[ParticleCount];
        private readonly Vector3[] start = new Vector3[ParticleCount];
        private VenomLevelController level;

        public void Initialize(VenomProfile profile, Transform spawn, VenomLevelController owner)
        {
            Profile = profile; level = owner;
            Bodies = new Rigidbody[ParticleCount]; Escaped = new bool[ParticleCount]; Groups = new int[ParticleCount];
            int index = 0;
            for (int y = 0; y < 4; y++) for (int z = 0; z < 4; z++) for (int x = 0; x < 4; x++)
            {
                Vector3 lattice = new Vector3(x - 1.5f, y - 1.5f, z - 1.5f);
                if (lattice.sqrMagnitude > 2.75f) continue;
                Vector3 offset = lattice * profile.Spacing;
                start[index] = spawn.TransformPoint(offset);
                var node = new GameObject("Matter " + index, typeof(Rigidbody), typeof(SphereCollider), typeof(VenomContact));
                node.transform.SetParent(transform, false); node.transform.position = start[index];
                var body = node.GetComponent<Rigidbody>(); Bodies[index] = body;
                body.mass = profile.ParticleMass; body.useGravity = false; body.linearDamping = .08f; body.angularDamping = 2;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.maxDepenetrationVelocity = .6f; body.solverIterations = 20; body.solverVelocityIterations = 8;
                body.sleepThreshold = 0;
                var shape = node.GetComponent<SphereCollider>(); shape.radius = profile.ParticleRadius;
                shape.sharedMaterial = profile.Contact; shape.contactOffset = .0003f;
                node.GetComponent<VenomContact>().Initialize(this, index);
                index++;
            }
            ResetMatter();
        }

        public void ResetMatter()
        {
            SimulationTime = 0; CutCount = MergeCount = EscapedCount = 0; FusionGlow = 0;
            bonds.Clear(); Array.Clear(connected, 0, connected.Length);
            for (int i = 0; i < ParticleCount; i++)
            {
                Bodies[i].position = start[i]; Bodies[i].rotation = Quaternion.identity;
                Bodies[i].linearVelocity = Bodies[i].angularVelocity = Vector3.zero;
                Escaped[i] = false; healAt[i] = 0;
            }
            for (int i = 0; i < ParticleCount; i++) for (int j = i + 1; j < ParticleCount; j++)
                if (Vector3.Distance(start[i], start[j]) < Profile.BondReach) Link(i, j, 1);
            RefreshGroups();
        }

        public void Step(float dt)
        {
            SimulationTime += dt; FusionGlow = Mathf.MoveTowards(FusionGlow, 0, dt);
            for (int i = 0; i < ParticleCount; i++) Bodies[i].AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            for (int k = bonds.Count - 1; k >= 0; k--)
            {
                Bond bond = bonds[k];
                Vector3 delta = Bodies[bond.B].position - Bodies[bond.A].position;
                float distance = delta.magnitude;
                if (distance < .00001f) continue;
                Vector3 axis = delta / distance;
                bond.Strength = Mathf.MoveTowards(bond.Strength, 1, dt / Profile.FusionSeconds);
                // Plastic rest lengths let the aggregate flatten/flow without restoring a rigid lattice.
                bond.Rest = Mathf.Lerp(bond.Rest, Mathf.Clamp(distance, Profile.Spacing, Profile.BondReach), Profile.Plasticity * dt);
                Vector3 relative = Bodies[bond.B].linearVelocity - Bodies[bond.A].linearVelocity;
                Vector3 force = axis * ((distance - bond.Rest) * Profile.Stiffness) + relative * Profile.Viscosity;
                force = Vector3.ClampMagnitude(force, .16f) * bond.Strength;
                Bodies[bond.A].AddForce(force); Bodies[bond.B].AddForce(-force);
            }
            bool fused = false;
            for (int i = 0; i < ParticleCount; i++) for (int j = i + 1; j < ParticleCount; j++)
            {
                if (connected[i,j] || Escaped[i] != Escaped[j] || SimulationTime < healAt[i] || SimulationTime < healAt[j]) continue;
                if (Vector3.SqrMagnitude(Bodies[i].position - Bodies[j].position) > Profile.Spacing * Profile.Spacing * 1.32f) continue;
                if (level != null && level.SegmentBlocked(Bodies[i].position, Bodies[j].position)) continue;
                fused |= Groups[i] != Groups[j];
                Link(i, j, .03f);
            }
            RefreshGroups();
            if (fused) { MergeCount++; FusionGlow = 1; Fused?.Invoke(); }
        }

        private void Link(int a, int b, float strength)
        {
            connected[a,b] = connected[b,a] = true;
            bonds.Add(new Bond { A = a, B = b, Rest = Vector3.Distance(Bodies[a].position, Bodies[b].position), Strength = strength });
        }

        public int Cut(Transform blade, Vector3 halfSize)
        {
            int removed = 0, before = FragmentCount;
            for (int k = bonds.Count - 1; k >= 0; k--)
            {
                Bond bond = bonds[k];
                if (Escaped[bond.A] || Escaped[bond.B]) continue;
                Vector3 a = blade.InverseTransformPoint(Bodies[bond.A].position), b = blade.InverseTransformPoint(Bodies[bond.B].position);
                if (a.x * b.x >= 0) continue;
                float t = -a.x / (b.x - a.x); Vector3 crossing = Vector3.Lerp(a, b, t);
                if (Mathf.Abs(crossing.y) > halfSize.y + Profile.ParticleRadius || Mathf.Abs(crossing.z) > halfSize.z + Profile.ParticleRadius) continue;
                connected[bond.A,bond.B] = connected[bond.B,bond.A] = false;
                healAt[bond.A] = healAt[bond.B] = SimulationTime + Profile.CutHealingDelay;
                bonds.RemoveAt(k); removed++;
            }
            RefreshGroups();
            if (FragmentCount > before) { CutCount++; Split?.Invoke(); }
            return removed;
        }

        public void RecordEscape(int index)
        {
            if (Escaped[index]) return;
            Escaped[index] = true; EscapedCount++;
            RefreshGroups();
        }

        public void Contact(int particle, Collider collider, Vector3 normal)
        {
            if (Escaped[particle]) return;
            var pad = collider.GetComponentInParent<VenomPressurePlate>();
            if (pad != null && Vector3.Dot(normal, pad.transform.up) > .45f) pad.Touch(particle, SimulationTime);
        }

        private int Root(int x) { while (parents[x] != x) { parents[x] = parents[parents[x]]; x = parents[x]; } return x; }
        public void RefreshGroups()
        {
            for (int i = 0; i < ParticleCount; i++) parents[i] = i;
            foreach (Bond bond in bonds) parents[Root(bond.A)] = Root(bond.B);
            int count = 0;
            for (int i = 0; i < ParticleCount; i++)
            {
                int root = Root(i), id = Array.IndexOf(roots, root, 0, count);
                if (id < 0) { id = count; roots[count++] = root; }
                Groups[i] = id;
            }
            // Escaped material is rendered, but no longer counts as an in-box fragment.
            FragmentCount = 0;
            for (int g = 0; g < count; g++)
                for (int i = 0; i < ParticleCount; i++) if (Groups[i] == g && !Escaped[i]) { FragmentCount++; break; }
        }
    }
}
