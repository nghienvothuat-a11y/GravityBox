using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Floor-plane A*: inflated, live chamber colliders define free space.
    /// Produces steering waypoints only. PhysX remains responsible for motion.
    /// </summary>
    public sealed class VenomNavigator
    {
        private const float Cell = .0125f;
        private const int Width = 39, Height = 51, Size = Width * Height;
        private readonly VenomLevelController level;
        private readonly bool[] walkable = new bool[Size], closed = new bool[Size];
        private readonly float[] cost = new float[Size], score = new float[Size];
        private readonly int[] parent = new int[Size];
        private readonly List<int> reverse = new List<int>(Size);
        private readonly List<Vector3> raw = new List<Vector3>(Size);
        public VenomNavigator(VenomLevelController owner) => level = owner;

        private Vector3 Point(int index) => level.Rotation.transform.TransformPoint(
            new Vector3((index % Width - 19) * Cell, level.FloorBoundary.Top + .025f, (index / Width - 25) * Cell));

        public bool Clear(Vector3 a, Vector3 b, float clearance, bool allowOutlet = false)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(a,b) / .008f));
            for (int i = 0; i <= steps; i++)
                if (!level.NavigationFree(Vector3.Lerp(a,b,i/(float)steps),clearance,allowOutlet)) return false;
            return true;
        }

        public bool FindPath(Vector3 from, Vector3 to, float clearance, List<Vector3> path)
        {
            path.Clear();
            var box = level.Rotation.transform;
            Vector3 a = box.InverseTransformPoint(from), b = box.InverseTransformPoint(to);
            a.y = b.y = level.FloorBoundary.Top + .025f;
            from = box.TransformPoint(a); to = box.TransformPoint(b);
            // Followers may enter the real opening only when their target is there.
            bool allowOutlet = Vector3.ProjectOnPlane(to-level.Outlet.position,box.up).magnitude < .065f;
            if (Clear(from,to,clearance,allowOutlet)) { path.Add(to); return true; }
            for (int i = 0; i < Size; i++)
            {
                walkable[i] = level.NavigationFree(Point(i),clearance,allowOutlet);
                closed[i] = false; cost[i] = score[i] = float.PositiveInfinity; parent[i] = -1;
            }
            int start = NearestReachable(from,clearance,allowOutlet), goal = NearestReachable(to,clearance,allowOutlet);
            if (start < 0 || goal < 0) return false;
            cost[start] = 0; score[start] = Vector3.Distance(Point(start),Point(goal));
            bool found = false;
            for (int iteration = 0; iteration < Size; iteration++)
            {
                int current = -1; float best = float.PositiveInfinity;
                for (int i = 0; i < Size; i++) if (!closed[i] && score[i] < best) { best = score[i]; current = i; }
                if (current < 0) break;
                if (current == goal) { found = true; break; }
                closed[current] = true;
                int x = current % Width, z = current / Width;
                for (int dz = -1; dz <= 1; dz++) for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dz == 0 || x+dx < 0 || x+dx >= Width || z+dz < 0 || z+dz >= Height) continue;
                    int next = current + dx + dz*Width;
                    if (!walkable[next] || closed[next]) continue;
                    // Diagonal moves cannot cut the corner of an inflated wall.
                    if (dx != 0 && dz != 0 && (!walkable[current+dx] || !walkable[current+dz*Width])) continue;
                    float candidate = cost[current] + Cell * (dx != 0 && dz != 0 ? 1.414214f : 1);
                    if (candidate >= cost[next]) continue;
                    if (!Clear(Point(current),Point(next),clearance,allowOutlet)) continue;
                    parent[next] = current; cost[next] = candidate;
                    score[next] = candidate + Vector3.Distance(Point(next),Point(goal));
                }
            }
            if (!found) return false;
            reverse.Clear(); raw.Clear();
            for (int index = goal; index >= 0; index = parent[index]) reverse.Add(index);
            for (int i = reverse.Count-1; i >= 0; i--) raw.Add(Point(reverse[i]));
            // A target touching a wall may be outside the inflated grid; the last
            // short approach uses particle clearance, never a through-wall seek.
            if (Clear(raw[raw.Count-1],to,level.MatterProfile.ParticleRadius+.001f,allowOutlet)) raw.Add(to);
            Vector3 previous = from;
            for (int i = 0; i < raw.Count;)
            {
                int farthest = i;
                for (int j = i+1; j < raw.Count; j++)
                    if (Clear(previous,raw[j],clearance,allowOutlet)) farthest = j;
                path.Add(raw[farthest]); previous = raw[farthest]; i = farthest+1;
            }
            return path.Count > 0;
        }

        private int NearestReachable(Vector3 p, float clearance, bool allowOutlet)
        {
            int result = -1; float best = .12f*.12f;
            for (int i = 0; i < Size; i++)
            {
                if (!walkable[i]) continue;
                float d = (Point(i)-p).sqrMagnitude;
                if (d >= best) continue;
                if (!Clear(p,Point(i),Mathf.Min(clearance,level.MatterProfile.ParticleRadius+.001f),allowOutlet)) continue;
                best = d; result = i;
            }
            return result;
        }
    }
}
