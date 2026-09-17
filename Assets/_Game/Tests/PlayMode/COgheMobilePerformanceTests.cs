using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class COgheMobilePerformanceTests
    {
        private SimulationMode previous;
        private VenomCampaign game;
        private const float Dt=1f/120;
        [UnitySetUp] public IEnumerator Before()
        {previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Physics.simulationMode=previous;VenomCampaignSave.PersistenceEnabled=true;Time.timeScale=1;yield return null;}
        private IEnumerator Load(int number)
        {
            void Loaded(Scene scene,LoadSceneMode mode)
            {game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;}
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"VenomOrigin{number:00}");}finally{SceneManager.sceneLoaded-=Loaded;}
            Steps(30);yield return null;
        }
        private void Steps(int n)
        {for(int i=0;i<n;i++){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);Physics.Simulate(Dt);}}

        [UnityTest] public IEnumerator CachedQueriesPreserveAperturesAndMovingSurfaceGeometryAcrossExpansion()
        {
            var prepare=typeof(VenomCampaign).GetMethod("PrepareSkinConstraints",BindingFlags.Instance|BindingFlags.NonPublic);
            var constrain=typeof(VenomCampaign).GetMethod("ConstrainSkinCached",BindingFlags.Instance|BindingFlags.NonPublic);
            var fragment=typeof(VenomCampaign).GetMethod("PrepareSkinFragment",BindingFlags.Instance|BindingFlags.NonPublic);
            var snapshotType=typeof(VenomCampaign).Assembly.GetType("GravityBox.Venom.VenomNavigationSnapshot");
            var navigation=Activator.CreateInstance(snapshotType);
            var capture=snapshotType.GetMethod("Capture");var clear=snapshotType.GetMethod("Clear");var occupied=snapshotType.GetMethod("Occupied");
            var boundaryType=typeof(VenomCampaign).Assembly.GetType("GravityBox.Venom.VenomBoundaryQueries");
            var boundaryQueries=Activator.CreateInstance(boundaryType);
            var captureBoundaries=typeof(VenomLevelController).GetMethod("CaptureAnimationBoundaries",BindingFlags.Instance|BindingFlags.NonPublic);
            var raycast=boundaryType.GetMethod("Raycast");var blocked=boundaryType.GetMethod("SegmentBlocked");
            var random=new System.Random(1439);
            for(int level=11;level<=20;level++)
            {
                yield return Load(level);
                var skin=game.Matter.transform;
                for(int pose=0;pose<2;pose++)
                {
                    if(pose==1&&game.Definition.CanRotate){game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(73,-39,17));Steps(240);}
                    Physics.SyncTransforms();prepare.Invoke(game,new object[]{skin});capture.Invoke(navigation,new object[]{game});
                    captureBoundaries.Invoke(game.Owner,new[]{boundaryQueries});
                    foreach(var patch in game.Surfaces)
                    {
                        for(int i=0;i<6;i++)
                        {
                            Vector3 p=new Vector3(((float)random.NextDouble()-.5f)*patch.Size.x,((float)random.NextDouble()-.5f)*patch.Size.y,-.025f*(float)random.NextDouble());
                            if(i==0)p=new Vector3(patch.HoleCentre.x,patch.HoleCentre.y,-.01f);
                            Vector3 world=patch.transform.TransformPoint(p),point=skin.InverseTransformPoint(world),normal=Vector3.up;
                            fragment.Invoke(game,new object[]{new Bounds(point,Vector3.one*.00001f)});
                            object[] args={point,normal};constrain.Invoke(game,args);game.ConstrainSkin(ref point,ref normal,skin);
                            Assert.Less(Vector3.Distance(point,(Vector3)args[0]),.00001f,$"Skin {level}/{patch.name}/{pose}/{p} legacy={point:F7} cached={(Vector3)args[0]:F7}");
                            Assert.Less(Vector3.Distance(normal,(Vector3)args[1]),.0001f,$"Normal {level}/{patch.name}");
                            Assert.AreEqual(game.Occupied(world),(bool)occupied.Invoke(navigation,new object[]{world}));
                            Vector3 end=world+new Vector3((float)random.NextDouble()-.5f,(float)random.NextDouble()-.5f,(float)random.NextDouble()-.5f).normalized*.09f;
                            object[] rayArgs={world,(end-world).normalized,.09f,default(RaycastHit)};
                            bool expectedRay=game.Owner.RaycastBoundary(world,(end-world).normalized,.09f,out var expectedHit);
                            Assert.AreEqual(expectedRay,(bool)raycast.Invoke(boundaryQueries,rayArgs),$"Animation ray {level}/{patch.name}");
                            if(expectedRay)
                            {
                                var hit=(RaycastHit)rayArgs[3];Assert.AreEqual(expectedHit.collider,hit.collider);
                                Assert.Less(Mathf.Abs(expectedHit.distance-hit.distance),.00001f);
                            }
                            Assert.AreEqual(game.Owner.SegmentBlocked(world,end),(bool)blocked.Invoke(boundaryQueries,new object[]{world,end}));
                            bool reference=game.Clear(world,end,.006f),cached=(bool)clear.Invoke(navigation,new object[]{world,end,.006f});
                            if(reference!=cached)
                            {
                                var delta=end-world;var ray=new Ray(world,delta.normalized);
                                foreach(var s in game.Surfaces)
                                {
                                    var a=s.transform.InverseTransformPoint(world);var b=s.transform.InverseTransformPoint(end);float t=a.z/(a.z-b.z);
                                    bool plane=s.SphereRadius<=0&&Mathf.Abs(a.z-b.z)>.00001f&&t>=0&&t<=1&&s.ContainsForNavigation(Vector3.Lerp(a,b,t),.006f);
                                    bool hit=s.Shape!=null&&s.Shape.Raycast(ray,out _,delta.magnitude-.002f);
                                    if(plane||hit)Debug.Log($"BLOCKER {s.name} plane={plane} hit={hit} bounds={s.Shape?.bounds} a={a:F8} b={b:F8} world={world:F8} end={end:F8}");
                                }
                            }
                            Assert.AreEqual(reference,cached,$"Route {level}/{patch.name}");
                        }
                    }
                }
            }
        }

        [UnityTest] public IEnumerator Level14_RotationCannotPullHingeOrGateOffTheirPhysicalRails()
        {
            yield return Load(14);var assembly=Object.FindFirstObjectByType<COgheGravityBridgeAssembly>();
            var hinge=assembly.Bridge.GetComponent<HingeJoint>();var slider=assembly.ExitGate.GetComponent<ConfigurableJoint>();
            float hingeError=0,railError=0,angularError=0,lateralError=0;
            Quaternion rest=Quaternion.Inverse(game.Root.rotation)*assembly.ExitGate.rotation;
            foreach(var orientation in new[]{new Vector3(90,0,0),new Vector3(-65,0,0),new Vector3(170,80,25),new Vector3(-90,-130,-65),Vector3.zero})
            {
                game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(orientation));
                for(int i=0;i<600;i++)
                {
                    Steps(1);
                    Vector3 a=assembly.Bridge.position+assembly.Bridge.rotation*hinge.anchor,b=game.Root.TransformPoint(hinge.connectedAnchor);
                    hingeError=Mathf.Max(hingeError,Vector3.Distance(a,b));
                    Vector3 offset=game.Root.InverseTransformPoint(assembly.ExitGate.position)-assembly.GateClosedLocal;
                    railError=Mathf.Max(railError,new Vector2(offset.x,offset.y).magnitude,Mathf.Max(0,offset.z),Mathf.Max(0,-assembly.GateTravel-offset.z));
                    lateralError=Mathf.Max(lateralError,new Vector2(offset.x,offset.y).magnitude);
                    angularError=Mathf.Max(angularError,Quaternion.Angle(rest,Quaternion.Inverse(game.Root.rotation)*assembly.ExitGate.rotation));
                    if(i%120==0)yield return null;
                }
                COgheExpansionIntegrationTests.Capture(game,"14-rotation-"+orientation.x);
            }
            Debug.Log($"L14 JOINT STRESS hinge={hingeError:F6}m rail={railError:F6}m lateral={lateralError:F6}m angle={angularError:F3}deg");
            Assert.Less(hingeError,.003f,"Hinge anchors separated while rotating.");
            Assert.Less(railError,.003f,"Gate left its physical travel envelope.");
            Assert.Less(angularError,2f,"Gate twists away from the glass.");
        }

        [UnityTest] public IEnumerator RepeatedCommandsReuseGraphButMovingDoorAndApertureInvalidateIt()
        {
            yield return Load(20);game.Motion.BuildGraph(true);int count=game.Motion.GraphBuildCount;
            for(int i=0;i<5;i++){game.Motion.BuildGraph();game.Motion.Move(0,game.Root.TransformPoint(new Vector3(-.5f,-.274f,.02f+i*.02f)));}
            Assert.AreEqual(count,game.Motion.GraphBuildCount,"Repeated commands on unchanged geometry must not rebuild the graph");
            var hole=Array.Find(game.Surfaces,p=>p.Hole);Assert.NotNull(hole);
            hole.NavigationHoleBlocked=!hole.NavigationHoleBlocked;game.Motion.BuildGraph();
            Assert.AreEqual(++count,game.Motion.GraphBuildCount,"Opening/closing an aperture invalidates cached routing");
            hole.NavigationHoleBlocked=!hole.NavigationHoleBlocked;game.Motion.BuildGraph();count++;
            var prop=game.Props[0];Vector3 old=prop.Body.position;prop.Body.position+=Vector3.up*.02f;prop.transform.position=prop.Body.position;Physics.SyncTransforms();game.Motion.BuildGraph();
            Assert.AreEqual(++count,game.Motion.GraphBuildCount,"A moving mechanism must invalidate the graph immediately");
            prop.Body.position=old;prop.transform.position=old;Physics.SyncTransforms();game.Motion.BuildGraph();
        }
        [UnityTest] public IEnumerator HeapPathsMatchOriginalDijkstraIncludingTieBreaksAndCornerWaypoints()
        {
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var nodeField=typeof(VenomCampaignMotion).GetField("nodes",flags);
            var linkField=typeof(VenomCampaignMotion).GetField("links",flags);
            var nearest=typeof(VenomCampaignMotion).GetMethod("Nearest",flags);
            var corner=typeof(VenomCampaignMotion).GetMethod("TryInsideCorner",flags);
            var random=new System.Random(2117);
            foreach(int level in new[]{8,12,13,14,16,19,20})
            {
                yield return Load(level);game.Motion.BuildGraph(true);
                var nodes=(List<Vector3>)nodeField.GetValue(game.Motion);var links=(List<List<int>>)linkField.GetValue(game.Motion);
                for(int trial=0;trial<12;trial++)
                {
                    Vector3 start=game.Root.TransformPoint(nodes[random.Next(nodes.Count)]),goal=game.Root.TransformPoint(nodes[random.Next(nodes.Count)]);
                    int a=(int)nearest.Invoke(game.Motion,new object[]{start}),b=(int)nearest.Invoke(game.Motion,new object[]{goal});
                    var costs=new float[nodes.Count];var parents=new int[nodes.Count];var done=new bool[nodes.Count];
                    for(int i=0;i<nodes.Count;i++){costs[i]=float.PositiveInfinity;parents[i]=-1;}costs[a]=0;
                    for(int k=0;k<nodes.Count;k++)
                    {
                        int current=-1;float best=float.PositiveInfinity;
                        for(int i=0;i<nodes.Count;i++)if(!done[i]&&costs[i]<best){best=costs[i];current=i;}
                        if(current<0||current==b)break;done[current]=true;
                        foreach(int next in links[current]){float d=costs[current]+Vector3.Distance(nodes[current],nodes[next]);if(d<costs[next]){costs[next]=d;parents[next]=current;}}
                    }
                    var expected=new List<Vector3>();bool reachable=!float.IsPositiveInfinity(costs[b]);
                    if(reachable)
                    {
                        for(int at=b;at>=0;at=parents[at])
                        {
                            expected.Add(nodes[at]);if(at==a)break;
                            object[] args={parents[at],at,Vector3.zero};if((bool)corner.Invoke(game.Motion,args))expected.Add((Vector3)args[2]);
                        }
                        expected.Reverse();
                    }
                    expected.Add(game.Root.InverseTransformPoint(goal));var actual=new List<Vector3>();
                    Assert.AreEqual(reachable,game.Motion.FindPath(start,goal,actual),$"Reachability {level}/{trial}");
                    CollectionAssert.AreEqual(expected,actual,$"Route and equal-cost tie break {level}/{trial}");
                }
            }
        }

        [UnityTest] public IEnumerator GraphRevisionMatchesFreshGraphAcrossDoorMotionAndApertureChanges()
        {
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var nodeField=typeof(VenomCampaignMotion).GetField("nodes",flags);
            var linkField=typeof(VenomCampaignMotion).GetField("links",flags);
            foreach(int level in new[]{13,14,19,20})
            {
                yield return Load(level);game.Motion.BuildGraph(true);
                var prop=game.Props[0];Vector3 initial=prop.Body.position;
                for(int move=0;move<4;move++)
                {
                    prop.Body.position=initial+game.Root.up*(.012f*(move+1));prop.transform.position=prop.Body.position;
                    if(move==2){var hole=Array.Find(game.Surfaces,p=>p.Hole);hole.NavigationHoleBlocked=!hole.NavigationHoleBlocked;}
                    Physics.SyncTransforms();game.Motion.BuildGraph();
                    var nodes=((List<Vector3>)nodeField.GetValue(game.Motion)).ToArray();
                    var links=new List<int[]>();foreach(var row in (List<List<int>>)linkField.GetValue(game.Motion))links.Add(row.ToArray());
                    game.Motion.BuildGraph(true);
                    CollectionAssert.AreEqual(nodes,(List<Vector3>)nodeField.GetValue(game.Motion),$"Nodes {level}/{move}");
                    var actual=(List<List<int>>)linkField.GetValue(game.Motion);Assert.AreEqual(links.Count,actual.Count);
                    for(int i=0;i<links.Count;i++)CollectionAssert.AreEqual(links[i],actual[i],$"Edge validity {level}/{move}/{i}");
                }
            }
        }
    }
}
