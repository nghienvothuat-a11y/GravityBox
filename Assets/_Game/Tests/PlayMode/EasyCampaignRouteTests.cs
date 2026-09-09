#if UNITY_EDITOR
using GravityBox.Gameplay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Tests
{
    public sealed partial class PhysicsLifecycleTests
    {
        private void LoadEasyCampaign(int campaignIndex)
        {
            LevelCatalog catalog=AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/_Game/Campaign/CampaignCatalog.asset");
            Assert.That(catalog,Is.Not.Null,"Generate campaign prefabs before easy route fixtures.");
            levels.Initialize(catalog,forces,storage:new VolatileCampaignStorage());
            Load(campaignIndex-1);
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void EasyCampaign_DryIntroRoutesEscapeFromTheirAuthoredSpawnByTiltOnly(int campaignIndex)
        {
            LoadEasyCampaign(campaignIndex);
            Assert.That(levels.Current.GetComponent<GravityBox.Simulation.WaterVolume>(),Is.Null);
            DriveEasyCampaignRoute(EasyDryRoute(campaignIndex),1f,1500);
            Assert.That(levels.Current.Exit.HasExited,Is.True,"C"+campaignIndex+" route did not physically clear the exit.");
        }

        [TestCase(61)]
        [TestCase(62)]
        [TestCase(63)]
        [TestCase(64)]
        [TestCase(65)]
        [TestCase(69)]
        [TestCase(71)]
        public void EasyCampaign_WaterComparisonAndBaffleRoutesEscapeFromAuthoredSpawnByTiltOnly(int campaignIndex)
        {
            LoadEasyCampaign(campaignIndex);
            bool dry=campaignIndex==61;
            Assert.That(levels.Current.GetComponent<GravityBox.Simulation.WaterVolume>()==null,Is.EqualTo(dry));
            DriveEasyCampaignRoute(EasyWaterRoute(campaignIndex),dry?1f:1.8f,dry?1500:2600);
            Assert.That(levels.Current.Exit.HasExited,Is.True,"C"+campaignIndex+" route did not physically clear the exit.");
        }

        private void DriveEasyCampaignRoute(Vector2[] route,float maximumAcceleration,int ticksPerWaypoint)
        {
            Rigidbody box=levels.Current.GetComponent<Rigidbody>();
            levels.Current.Exit.BeginTracking();
            for(int waypoint=0;waypoint<route.Length;waypoint++)
            {
                bool final=waypoint==route.Length-1;
                float closest=float.PositiveInfinity;
                for(int tick=0;tick<ticksPerWaypoint;tick++)
                {
                    Transform root=levels.Current.transform;
                    Vector3 p=root.InverseTransformPoint(levels.Ball.Body.position);
                    Vector3 v3=root.InverseTransformDirection(levels.Ball.Body.linearVelocity-box.GetPointVelocity(levels.Ball.Body.position));
                    Vector2 error=route[waypoint]-new Vector2(p.x,p.z);
                    Vector2 velocity=new Vector2(v3.x,v3.z);
                    closest=Mathf.Min(closest,error.magnitude);
                    Vector2 acceleration=Vector2.ClampMagnitude(error*10f-velocity*5f,maximumAcceleration);
                    levels.Current.Rotation.SetTargetOrientation(
                        Quaternion.FromToRotation(new Vector3(acceleration.x,-9.81f,acceleration.y),Vector3.down));
                    Steps(1);
                    if(final ? levels.Current.Exit.HasExited : error.magnitude<Radius*.9f && velocity.magnitude<.17f)
                    {
                        TestContext.WriteLine($"C{levels.Index+1:00} easy route point {waypoint}: {(tick+1)*Dt:F3}s, closest {closest:F4}m.");
                        break;
                    }
                    Assert.That(levels.Current.IsOutside(levels.Ball.Body.position),Is.False,"Ball leaked before reaching the physical exit.");
                    if(tick==ticksPerWaypoint-1)
                        Assert.Fail($"C{levels.Index+1:00} did not reach {route[waypoint]:F4}; local {p:F4}, closest {closest:F4}m.");
                }
            }
        }

        private static Vector2[] EasyDryRoute(int index)
        {
            switch(index)
            {
                case 1:return new[]{new Vector2(.095f,-.085f)};
                case 3:return new[]{new Vector2(-.105f,-.085f),new Vector2(.080f,-.105f),new Vector2(.125f,-.105f)};
                case 4:return new[]{new Vector2(-.080f,-.050f),new Vector2(.082f,-.068f)};
                case 5:return new[]{new Vector2(-.155f,-.115f),new Vector2(.120f,-.115f),new Vector2(.170f,-.145f)};
                case 6:
                {
                    var route=new Vector2[7];
                    for(int i=0;i<route.Length;i++)
                    {float angle=Mathf.PI-i*Mathf.PI/(route.Length-1);route[i]=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.183f;}
                    return route;
                }
                case 7:return new[]{new Vector2(-.190f,-.120f),new Vector2(-.185f,-.205f),new Vector2(.185f,-.205f),new Vector2(.165f,.150f)};
                case 8:return new[]{new Vector2(-.090f,0),new Vector2(.090f,0),new Vector2(.235f,0)};
                case 9:return new[]{new Vector2(-.100f,.020f),new Vector2(-.075f,.105f),new Vector2(0,.190f)};
                default:throw new System.ArgumentOutOfRangeException(nameof(index));
            }
        }

        private static Vector2[] EasyWaterRoute(int index)
        {
            switch(index)
            {
                case 61:case 62:case 71:
                    return new[]{new Vector2(-.105f,-.105f),new Vector2(.105f,-.105f)};
                case 63:
                    return new[]{new Vector2(-.105f,.020f),new Vector2(-.105f,-.105f),new Vector2(.120f,-.120f)};
                case 64:
                {
                    var route=new Vector2[6];
                    for(int i=0;i<route.Length;i++)
                    {float angle=Mathf.PI-i*Mathf.PI/(route.Length-1);route[i]=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*.135f;}
                    return route;
                }
                case 65:return new[]{new Vector2(-.145f,-.120f),new Vector2(.010f,-.140f),new Vector2(.155f,-.105f)};
                case 69:return new[]{new Vector2(-.080f,-.075f),new Vector2(.105f,-.095f)};
                default:throw new System.ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
#endif
