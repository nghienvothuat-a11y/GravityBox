using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    public sealed partial class VenomCampaign
    {
        private VenomSurfaceSnapshot[] skinSurfaces=Array.Empty<VenomSurfaceSnapshot>();
        private Matrix4x4 skinToWorld,worldToSkin;
        private Quaternion worldNormalToSkin;
        private readonly List<COgheMechanism> skinMechanisms=new List<COgheMechanism>(4);
        private readonly List<COgheMechanism> fragmentSkinMechanisms=new List<COgheMechanism>(4);
        private int[] skinCandidates=Array.Empty<int>();
        private int skinCandidateCount;
        internal void PrepareSkinConstraints(Transform skin)
        {
            skinToWorld=skin.localToWorldMatrix;worldToSkin=skin.worldToLocalMatrix;worldNormalToSkin=Quaternion.Inverse(skin.rotation);
            if(skinSurfaces.Length!=Surfaces.Length){skinSurfaces=new VenomSurfaceSnapshot[Surfaces.Length];skinCandidates=new int[Surfaces.Length];}
            skinCandidateCount=Surfaces.Length;
            for(int i=0;i<Surfaces.Length;i++){skinSurfaces[i].Capture(Surfaces[i]);skinCandidates[i]=i;}
            skinMechanisms.Clear();fragmentSkinMechanisms.Clear();
            foreach(var mechanism in Mechanisms)if(mechanism.HasSkinConstraint&&mechanism.isActiveAndEnabled)
            {mechanism.PrepareSkinFrame();skinMechanisms.Add(mechanism);fragmentSkinMechanisms.Add(mechanism);}
        }
        internal void PrepareSkinFragment(Bounds localBounds)
        {
            Vector3 centre=skinToWorld.MultiplyPoint3x4(localBounds.center),e=localBounds.extents;
            Vector3 x=skinToWorld.MultiplyVector(Vector3.right*e.x),y=skinToWorld.MultiplyVector(Vector3.up*e.y),z=skinToWorld.MultiplyVector(Vector3.forward*e.z);
            Vector3 extent=new Vector3(Mathf.Abs(x.x)+Mathf.Abs(y.x)+Mathf.Abs(z.x),Mathf.Abs(x.y)+Mathf.Abs(y.y)+Mathf.Abs(z.y),Mathf.Abs(x.z)+Mathf.Abs(y.z)+Mathf.Abs(z.z))+Vector3.one*.00002f;
            var worldBounds=new Bounds(centre,extent*2);fragmentSkinMechanisms.Clear();
            foreach(var mechanism in skinMechanisms)if(mechanism.MayConstrainSkin(worldBounds))fragmentSkinMechanisms.Add(mechanism);
            if(InTube&&Tube!=null)extent+=Vector3.one*.031f;
            Vector3 min=centre-extent,max=centre+extent;skinCandidateCount=0;
            for(int i=0;i<skinSurfaces.Length;i++)
            {
                ref var s=ref skinSurfaces[i];
                if(max.x<s.SkinMin.x||min.x>s.SkinMax.x||max.y<s.SkinMin.y||min.y>s.SkinMax.y||max.z<s.SkinMin.z||min.z>s.SkinMax.z)continue;
                skinCandidates[skinCandidateCount++]=i;
                // A clip can reach a second pane. Grow the reachable envelope
                // by its maximum displacement before considering the next pane.
                // This preserves ordered corner/opposing-face constraints.
                min+=s.ClipMin;max+=s.ClipMax;
            }
        }
        internal void ConstrainSkinCached(ref Vector3 local,ref Vector3 normal)
        {
            Vector3 world=skinToWorld.MultiplyPoint3x4(local);
            foreach(var mechanism in fragmentSkinMechanisms)
                if(mechanism.ConstrainSkin(ref world,out Vector3 worldNormal))
                {local=worldToSkin.MultiplyPoint3x4(world);normal=worldNormalToSkin*worldNormal;return;}
            if(InTube&&Tube!=null)
            {
                Vector3 p=Tube.transform.InverseTransformPoint(world);float radial=new Vector2(p.x,p.y).magnitude;
                if(p.z>0&&p.z<Tube.Length&&radial>Tube.Radius-.0005f&&radial<Tube.Radius+.03f)
                {
                    Vector3 outward=new Vector3(p.x,p.y,0).normalized;float scale=(Tube.Radius-.0005f)/radial;p.x*=scale;p.y*=scale;
                    world=Tube.transform.TransformPoint(p);normal=worldNormalToSkin*Tube.transform.TransformDirection(outward);
                }
            }
            for(int i=0;i<skinCandidateCount;i++)
            {
                ref var s=ref skinSurfaces[skinCandidates[i]];
                // Bounds.Contains itself crosses the managed/native boundary.
                // Reject distant panes in managed code; only nearby candidates
                // use the original exact transform and clipping calculation.
                if(world.x<s.SkinMin.x||world.x>s.SkinMax.x||world.y<s.SkinMin.y||world.y>s.SkinMax.y||world.z<s.SkinMin.z||world.z>s.SkinMax.z)continue;
                var patch=s.Patch;Vector3 p=patch.transform.InverseTransformPoint(world);
                if(patch.SphereRadius>0)
                {
                    float magnitude=p.magnitude;
                    if(magnitude>patch.SphereRadius&&magnitude<patch.SphereRadius+.026f&&patch.Contains(p))
                    {
                        world=patch.transform.TransformPoint(p.normalized*(patch.SphereRadius-.0006f));
                        normal=worldNormalToSkin*(s.ToWorld.MultiplyPoint3x4(Vector3.zero)-world).normalized;
                    }
                }
                else if(p.z<0&&p.z>=-.026f&&patch.Contains(p))
                {p.z=.0006f;world=patch.transform.TransformPoint(p);normal=worldNormalToSkin*(s.Rotation*Vector3.forward);}
            }
            local=worldToSkin.MultiplyPoint3x4(world);
        }
    }
}
