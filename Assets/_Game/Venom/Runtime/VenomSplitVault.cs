using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Experiment 05: a physical knife and a ceiling vault. The followers
    /// are released by complete leader entry, never by an elapsed-time shortcut.</summary>
    public sealed class VenomSplitVault : MonoBehaviour
    {
        public Transform Knife;
        public Vector3 KnifeHalfSize=new Vector3(.0015f,.047f,.021f);
        public Collider[] Obstacles;
        public Bounds Interior=new Bounds(new Vector3(0,.20f,0),new Vector3(.2f,.1f,.15f));
        public Vector3 Entry=new Vector3(0,.228f,.079f);
        public float DoorWidth=.034f;
        public float NavigationY=.227f;
        public bool FollowersReleased {get;private set;}
        public int Captain {get;private set;}=-1;
        public int EntryCount {get;private set;}
        private VenomLevelController level;
        public void Initialize(VenomLevelController owner){level=owner;ResetState();}
        public void ResetState(){FollowersReleased=false;Captain=-1;EntryCount=0;}
        public void Cut()=>level.Organism.Cut(Knife,KnifeHalfSize);
        public bool IsInside(Vector3 world)
        {
            Vector3 local=level.Rotation.transform.InverseTransformPoint(world);
            return Interior.Contains(local) && local.z<Interior.max.z-level.MatterProfile.ParticleRadius;
        }
        public void ObserveLeader()
        {
            if(FollowersReleased || level.Organism.CutCount==0)return;
            var leader=level.Locomotion.Selected;
            if(Captain<0)
            {
                if(leader==null || leader.Count>=32)return;
                for(int i=0;i<32;i++)
                    if(!level.Organism.Escaped[i] && level.Organism.Groups[i]==leader.Group && IsInside(level.Organism.Bodies[i].position)){Captain=leader.Anchor;break;}
                if(Captain<0)return;
            }
            int inside=0,total=0;
            for(int i=0;i<32;i++)if(level.Organism.Groups[i]==level.Organism.Groups[Captain])
            {
                total++;
                if(level.Organism.Escaped[i] || IsInside(level.Organism.Bodies[i].position))inside++;
            }
            EntryCount=inside;
            if(inside==total && total>0)FollowersReleased=true;
            else if(inside==0)Captain=-1;
        }
        public bool AutoFollow(VenomLocomotion.Fragment fragment)
        {
            return FollowersReleased && fragment.Group!=level.Organism.Groups[Captain];
        }
        public bool NavigationFree(Vector3 world,float clearance,bool allowOutlet)
        {
            var root=level.Rotation.transform;Vector3 local=root.InverseTransformPoint(world);
            if(Mathf.Abs(local.x)>.25f-clearance || Mathf.Abs(local.z)>.25f-clearance)return false;
            if(!allowOutlet && new Vector2(local.x,local.z).magnitude<level.ApertureRadius+clearance)return false;
            local.y=NavigationY;world=root.TransformPoint(local);
            foreach(var shape in Obstacles)
                if(shape!=null && shape.enabled && shape.gameObject.activeInHierarchy && (shape.ClosestPoint(world)-world).sqrMagnitude<clearance*clearance)return false;
            return true;
        }
    }
}
