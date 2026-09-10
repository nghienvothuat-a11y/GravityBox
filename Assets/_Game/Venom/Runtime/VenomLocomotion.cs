using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Stable particle anchors own selection. Connectivity labels are transient.
    /// Only supported particles exert bounded traction; no kinematic fragments.
    /// </summary>
    public sealed class VenomLocomotion
    {
        public sealed class Fragment
        {
            public int Anchor, Group, Count;
            public Vector3 Centre, Velocity, Intent, GripPoint;
            public float WaitRemaining;
            public bool Selected, Following, Blocked, Gripping;
            internal int PreviousCount, Waypoint;
            internal float RepathAt;
            internal readonly List<Vector3> Path = new List<Vector3>(32);
            public readonly VenomSqueeze Squeeze = new VenomSqueeze();
        }
        private readonly VenomLevelController level;
        private readonly CohesiveOrganism matter;
        private readonly VenomLocomotionProfile profile;
        private readonly Fragment[] states = new Fragment[CohesiveOrganism.ParticleCount];
        private readonly List<Fragment> fragments = new List<Fragment>(8);
        private readonly float[] detachedAt = new float[CohesiveOrganism.ParticleCount];
        private Vector3 input;
        public int SelectedParticle { get; private set; }
        public Fragment Selected { get; private set; }
        public IReadOnlyList<Fragment> Fragments => fragments;
        public VenomNavigator Navigator { get; }
        public Vector3 Input => input;
        public int PathSearches { get; private set; }

        public VenomLocomotion(VenomLevelController owner)
        {
            level = owner; matter = owner.Organism; profile = owner.LocomotionProfile;
            Navigator = new VenomNavigator(owner); Reset();
        }
        public void Reset()
        {
            input = Vector3.zero; SelectedParticle = 0; PathSearches = 0;
            Array.Clear(states,0,states.Length);
            for (int i = 0; i < detachedAt.Length; i++) detachedAt[i] = -1;
            Refresh();
        }
        public void SetInput(Vector3 direction)
        {
            input = level.CanControl ? Vector3.ClampMagnitude(level.WallCrawl?direction:Vector3.ProjectOnPlane(direction,level.Rotation.transform.up),1) : Vector3.zero;
        }
        public bool Select(int particle)
        {
            if (!level.CanControl || level.ControlMode != VenomControlMode.SelectFragment || particle < 0 || particle >= 32 || matter.Escaped[particle]) return false;
            SelectedParticle = particle; input = Vector3.zero; Refresh(); return true;
        }
        public void SelectNext()
        {
            if (fragments.Count < 2) return;
            int index = fragments.IndexOf(Selected); Select(fragments[(index+1)%fragments.Count].Anchor);
        }
        public bool SelectAt(Vector2 screen)
        {
            float best = Mathf.Max(28,Screen.width*.065f); int particle = -1;
            for (int i = 0; i < 32; i++)
            {
                if (matter.Escaped[i]) continue;
                Vector3 p = level.View.WorldToScreenPoint(matter.Bodies[i].position);
                float distance = Vector2.Distance(screen,p);
                if (p.z > 0 && distance < best) { best = distance; particle = i; }
            }
            return Select(particle);
        }
        private void Refresh()
        {
            fragments.Clear(); Selected = null;
            for (int i = 0; i < 32; i++)
            {
                if (matter.Escaped[i]) continue;
                int group = matter.Groups[i]; Fragment fragment = null;
                foreach (var candidate in fragments) if (candidate.Group == group) { fragment = candidate; break; }
                if (fragment == null)
                {
                    fragment = states[i] ?? (states[i] = new Fragment { Anchor = i });
                    fragment.Group = group; fragment.Count = 0; fragment.Centre = fragment.Velocity = fragment.Intent = Vector3.zero;
                    fragment.Selected = fragment.Following = fragment.Blocked = false;
                    fragments.Add(fragment);
                }
                fragment.Count++; fragment.Centre += matter.Bodies[i].position; fragment.Velocity += matter.Bodies[i].linearVelocity;
            }
            Fragment largest = null;
            foreach (var fragment in fragments)
            {
                fragment.Centre /= fragment.Count; fragment.Velocity /= fragment.Count;
                if (fragment.Count != fragment.PreviousCount)
                { fragment.Gripping = false; fragment.Path.Clear(); fragment.RepathAt = 0; fragment.PreviousCount = fragment.Count; }
                if (largest == null || fragment.Count > largest.Count || fragment.Count == largest.Count && fragment.Group == matter.Groups[SelectedParticle]) largest = fragment;
                if (!matter.Escaped[SelectedParticle] && fragment.Group == matter.Groups[SelectedParticle]) Selected = fragment;
            }
            bool chooseLargest=level.ControlMode==VenomControlMode.FollowLargest || level.SplitVault!=null && level.SplitVault.Captain<0;
            if(chooseLargest || Selected==null)Selected=largest;
            if(level.SplitVault!=null && level.SplitVault.Captain>=0)
                foreach(var fragment in fragments)if(fragment.Group==matter.Groups[level.SplitVault.Captain])Selected=fragment;
            if (Selected != null)
            {
                Selected.Selected = true;
                // Preserve the selected material point through a split, including
                // when the fragment's minimum particle ID changes after a fusion.
                if (matter.Escaped[SelectedParticle] || matter.Groups[SelectedParticle] != Selected.Group) SelectedParticle = Selected.Anchor;
            }
        }
        public void Step(float dt)
        {
            Refresh();
            level.SplitVault?.ObserveLeader();
            if (Selected == null) return;
            for (int i = 0; i < 32; i++)
            {
                if (matter.Escaped[i] || matter.Groups[i] == Selected.Group) detachedAt[i] = -1;
                else if (detachedAt[i] < 0) detachedAt[i] = matter.SimulationTime;
            }
            foreach (var fragment in fragments)
            {
                fragment.WaitRemaining = 0;
                Vector3 command = fragment.Selected ? input : Vector3.zero;
                if(level.WallCrawl)
                {
                    if(level.Guidance!=null)command=level.Guidance.Steer(fragment);
                    if(level.SplitVault!=null && level.SplitVault.AutoFollow(fragment))
                    {
                        fragment.Following=true;command=Follow(fragment,level.Outlet.position);
                    }
                    fragment.Intent=level.Climbing.Step(fragment,command,dt);continue;
                }
                float speed = profile.CrawlSpeed;
                if (!fragment.Selected && level.ControlMode == VenomControlMode.FollowLargest)
                {
                    float separated = float.PositiveInfinity;
                    for (int i = 0; i < 32; i++) if (!matter.Escaped[i] && matter.Groups[i] == fragment.Group) separated = Mathf.Min(separated,detachedAt[i]);
                    fragment.WaitRemaining = Mathf.Max(0,profile.FollowDelay-(matter.SimulationTime-separated));
                    if (fragment.WaitRemaining <= 0)
                    {
                        fragment.Following = true; speed = profile.FollowSpeed;
                        command = Follow(fragment);
                    }
                }
                fragment.Intent = command;
                fragment.Squeeze.Step(level,fragment.Group,fragment.Centre,command,dt);
                ApplyTraction(fragment,command,speed);
            }
        }
        private Vector3 Follow(Fragment fragment)=>Follow(fragment,Selected.Centre);
        private Vector3 Follow(Fragment fragment,Vector3 target)
        {
            if (matter.SimulationTime >= fragment.RepathAt)
            {
                PathSearches++;
                if (!Navigator.FindPath(fragment.Centre,target,profile.NavigationClearance,fragment.Path))
                    Navigator.FindPath(fragment.Centre,target,matter.Profile.ParticleRadius+.002f,fragment.Path);
                fragment.Waypoint = 0; fragment.RepathAt = matter.SimulationTime+profile.RepathSeconds;
            }
            while (fragment.Waypoint < fragment.Path.Count-1 && Vector3.ProjectOnPlane(fragment.Path[fragment.Waypoint]-fragment.Centre,level.NavigationUp).magnitude < .016f) fragment.Waypoint++;
            if (fragment.Path.Count == 0) { fragment.Blocked = true; return Vector3.zero; }
            Vector3 delta = Vector3.ProjectOnPlane(fragment.Path[fragment.Waypoint]-fragment.Centre,level.NavigationUp);
            return Vector3.ClampMagnitude(delta/.028f,1);
        }
        private void ApplyTraction(Fragment fragment, Vector3 command, float speed)
        {
            for (int i = 0; i < 32; i++)
                if (matter.Groups[i] == fragment.Group) matter.SetFlow(i,fragment.Squeeze.Amount);
            int supported = 0;
            for (int i = 0; i < 32; i++)
                if (!matter.Escaped[i] && matter.Groups[i] == fragment.Group && matter.TryGetSupport(i,out _,out _,out var n) && n.y > .65f) supported++;
            if (supported == 0) { fragment.Gripping = false; return; }
            Vector3 velocity = Vector3.ProjectOnPlane(fragment.Velocity,Vector3.up);
            Vector3 acceleration;
            if (command.sqrMagnitude > .001f)
            {
                fragment.Gripping = false;
                acceleration = (command*speed-velocity)*profile.VelocityResponse + command*profile.FrictionCompensation;
                acceleration = Vector3.ClampMagnitude(acceleration,profile.MaxAcceleration);
            }
            else
            {
                if (!fragment.Gripping || Vector3.ProjectOnPlane(fragment.GripPoint-fragment.Centre,Vector3.up).magnitude > .045f)
                { fragment.GripPoint = fragment.Centre; fragment.Gripping = true; }
                acceleration = Vector3.ProjectOnPlane(fragment.GripPoint-fragment.Centre,Vector3.up)*35-velocity*20;
                acceleration = Vector3.ClampMagnitude(acceleration,profile.GripAcceleration);
            }
            // A few contacts cannot suspend an entire body over the outlet. As
            // support disappears its available traction falls, leaving gravity.
            float weight = Mathf.Min(fragment.Count/(float)supported,2.5f);
            for (int i = 0; i < 32; i++)
            {
                if (matter.Escaped[i] || matter.Groups[i] != fragment.Group || !matter.TryGetSupport(i,out var surface,out var point,out var normal) || normal.y <= .65f) continue;
                Vector3 outlet = level.Outlet.InverseTransformPoint(matter.Bodies[i].position);
                if (level.GateLatched && outlet.z > -.06f && new Vector2(outlet.x,outlet.y).magnitude < .057f) continue;
                Vector3 localAcceleration = acceleration;
                if (command.sqrMagnitude > .001f && fragment.Squeeze.Amount > 0)
                {
                    Vector3 desired = fragment.Squeeze.Velocity(level,matter.Bodies[i].position,command,speed);
                    Vector3 individual = Vector3.ProjectOnPlane(matter.Bodies[i].linearVelocity,Vector3.up);
                    localAcceleration = Vector3.ClampMagnitude((desired-individual)*profile.VelocityResponse +
                        desired.normalized*profile.FrictionCompensation,profile.MaxAcceleration);
                }
                Vector3 force = Vector3.ProjectOnPlane(localAcceleration,normal)*matter.Bodies[i].mass*weight;
                matter.Bodies[i].AddForce(force);
                Rigidbody supportBody = surface.attachedRigidbody;
                if (supportBody != null && !supportBody.isKinematic) supportBody.AddForceAtPosition(-force,point);
            }
        }
        public Vector3 IntentForParticle(int particle)
        {
            foreach (var fragment in fragments) if (fragment.Group == matter.Groups[particle]) return fragment.Intent;
            return Vector3.zero;
        }
    }
}
