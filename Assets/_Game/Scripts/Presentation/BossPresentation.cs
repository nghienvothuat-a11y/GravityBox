using System;
using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Presentation
{
    /// <summary>Read-only milestone light: never changes a body, joint, collider or exit.</summary>
    public sealed class BossPresentation : MonoBehaviour, IBallMechanism, IResettable
    {
        [Serializable] public sealed class Milestone
        {
            public string Name;
            public Transform Reference;
            public Bounds Region;
            public ContactSeatLatch Seat;
            public MemoryRatchetAssembly Memory;
            public PhysicalHinge Hinge;
            public float MinimumAngle;
            public Renderer[] Inlays = Array.Empty<Renderer>();
            [NonSerialized] public bool Observed;
        }
        public Milestone[] Milestones = Array.Empty<Milestone>();
        public ExitSocket Exit;
        public Renderer[] Finale = Array.Empty<Renderer>();
        public Color Quiet = new Color(.08f,.15f,.17f,1);
        public Color Awake = new Color(.55f,.68f,.49f,1);
        private IReadOnlyList<BallController> balls;
        private MaterialPropertyBlock block;
        private float completedAt = -1;
        public void Bind(IReadOnlyList<BallController> roster) { balls=roster; ResetState(); }
        public void CaptureInitialState() { }
        public void ResetState()
        {
            completedAt=-1;
            foreach(var milestone in Milestones) { milestone.Observed=false; Paint(milestone.Inlays,Quiet); }
            Paint(Finale,Quiet);
        }
        private void Update()
        {
            if(balls==null) return;
            foreach(var m in Milestones)
            {
                if(m.Observed) continue;
                bool observed=m.Seat!=null ? m.Seat.Latched : m.Memory!=null ? m.Memory.PassageAligned
                    : m.Hinge!=null ? m.Hinge.Angle>=m.MinimumAngle : Inside(m);
                if(observed) { m.Observed=true; Paint(m.Inlays,Awake); }
            }
            if(Exit!=null && Exit.HasExited)
            {
                if(completedAt<0) completedAt=Time.unscaledTime;
                float intensity=Mathf.Clamp01((Time.unscaledTime-completedAt)/.3f);
                if(Time.unscaledTime-completedAt>2) intensity=.4f;
                Paint(Finale,Color.Lerp(Quiet,Awake,intensity));
            }
        }
        private bool Inside(Milestone m)
        {
            Transform basis=m.Reference!=null?m.Reference:transform;
            foreach(var ball in balls)
                if(ball!=null && (Exit==null || !Exit.HasBallExited(ball)) && m.Region.Contains(basis.InverseTransformPoint(ball.Body.position))) return true;
            return false;
        }
        private void Paint(Renderer[] renderers,Color color)
        {
            if(block==null) block=new MaterialPropertyBlock();
            foreach(var r in renderers)
            {
                if(r==null) continue;
                r.GetPropertyBlock(block);block.SetColor("_BaseColor",color);block.SetColor("_Color",color);
                block.SetColor("_EmissionColor",color*.2f);r.SetPropertyBlock(block);
            }
        }
    }
}
