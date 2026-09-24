using System;
using System.Collections;
using System.Collections.Generic;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class COgheCampaign60Tests
    {
        const float Dt=1f/120;
        VenomCampaign game;
        SimulationMode previousMode;bool previousPersistence;float previousTime;
        [UnitySetUp] public IEnumerator Before()
        {previousMode=Physics.simulationMode;previousPersistence=VenomCampaignSave.PersistenceEnabled;previousTime=Time.timeScale;Physics.simulationMode=SimulationMode.Script;VenomCampaignSave.PersistenceEnabled=false;Time.timeScale=1;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Physics.simulationMode=previousMode;VenomCampaignSave.PersistenceEnabled=previousPersistence;Time.timeScale=previousTime;yield return null;}
        IEnumerator Load(int slot)
        {
            void Loaded(Scene scene,LoadSceneMode mode){game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;}
            SceneManager.sceneLoaded+=Loaded;try{yield return SceneManager.LoadSceneAsync($"COgheOrigin{slot:00}");}finally{SceneManager.sceneLoaded-=Loaded;}
            Steps(120);game.CameraRig.Frame(720,1280,0,true);
        }
        void Steps(int count)
        {for(int i=0;i<count;i++){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);if(!game.Owner.Paused)Physics.Simulate(Dt);}}
        string State=>$"slot={game.Definition.Order} centre={game.Root.InverseTransformPoint(game.Motion.Centre(0)):F4} escaped={game.Matter.EscapedCount} fragments={game.Matter.TotalFragmentCount} loss={game.Failure}";
        void Capture(string suffix)=>COgheExpansionIntegrationTests.Capture(game,$"vessel-{game.Definition.Order}-{suffix}");
        IEnumerator Solution(int slot)
        {
            yield return Load(slot);Capture("start");
            Steps(600);Assert.IsFalse(game.Owner.Completed,"Rotation must be required: "+State);Assert.AreEqual(0,game.Matter.EscapedCount);
            if(slot!=60)
            {
                // Put the mouth below the belly, then align its short neck.
                var outward=game.Root.InverseTransformDirection(game.Owner.Outlet.forward);
                game.Owner.Rotation.SetTargetOrientation(Quaternion.FromToRotation(slot==59?Vector3.down:Vector3.up,Vector3.down));
                for(int i=0;i<2400&&!game.Owner.Completed;i++)
                {if(i==720)game.Owner.Rotation.SetTargetOrientation(Quaternion.FromToRotation(outward,Vector3.down));Steps(1);if(i%240==0)yield return null;}
            }
            else
            {
                // Author replay: progressively turn the next bend downhill using only rotation commands.
                var mesh=game.Surfaces[0].Curved.Interior;var vertices=mesh.vertices;int rings=(vertices.Length-1)/64;
                var centres=new Vector3[rings];for(int r=0;r<rings;r++)for(int a=0;a<64;a++)centres[r]+=vertices[r*64+a]/64;
                int progress=0;
                for(int tick=0;tick<9000&&!game.Owner.Completed&&!game.Owner.Lost;tick++)
                {
                    if(tick%24==0)
                    {
                        Vector3 p=game.Root.InverseTransformPoint(game.Motion.Centre(0));int nearest=progress;float best=float.PositiveInfinity;
                        for(int i=0;i<rings;i++){float d=(p-centres[i]).sqrMagnitude;if(d<best){best=d;nearest=i;}}
                        progress=Math.Max(progress,nearest);int target=Math.Min(rings-1,progress+4);
                        Vector3 direction=progress>=rings-3?game.Root.InverseTransformDirection(game.Owner.Outlet.forward):(centres[target]-p).normalized;
                        game.Owner.Rotation.SetTargetOrientation(Quaternion.FromToRotation(direction,Vector3.down));
                    }
                    Steps(1);if(tick%240==0)yield return null;
                }
            }
            Capture(game.Owner.Completed?"won":"failed");Assert.IsTrue(game.Owner.Completed,State);Assert.IsFalse(game.Owner.Lost,State);
            Assert.AreEqual(32,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
        }
        [UnityTest] public IEnumerator Vase56RotatesThroughNarrowNeck(){yield return Solution(56);}
        [UnityTest] public IEnumerator Mask57RotatesThroughOffsetCrown(){yield return Solution(57);}
        [UnityTest] public IEnumerator Teapot58RotatesThroughBentSpout(){yield return Solution(58);}
        [UnityTest] public IEnumerator Skull59RotatesThroughFinalOpening(){yield return Solution(59);}
        [UnityTest] public IEnumerator Shell60RotatesThroughAllBends(){yield return Solution(60);}

        [UnityTest] public IEnumerator HollowVesselsStaySlipperyAndContainedDuringWrongRotationAndRetry()
        {
            for(int slot=56;slot<=60;slot++)
            {
                yield return Load(slot);var initial=game.Owner.Rotation.Orientation;
                Assert.IsTrue(game.Definition.Passive);Assert.IsTrue(game.Definition.CanRotate);Assert.AreEqual(slot==60,game.Definition.Boss);
                Assert.AreEqual($"venom.origin.{slot}",game.Definition.Id);Assert.AreEqual(60,game.PlayableLevelCount);
                if(slot==60)Assert.IsEmpty(game.Definition.Lesson);
                foreach(var surface in game.Surfaces){Assert.IsTrue(surface.Slippery);Assert.IsFalse(surface.RingGrip);Assert.AreEqual(0,surface.Shape.sharedMaterial.dynamicFriction);}
                var shell=game.Surfaces[0];var vertices=shell.Curved.Interior.vertices;
                for(int turn=0;turn<4;turn++)
                {
                    game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(20,turn*80,turn%2==0?20:-20)*initial);
                    Steps(240);
                    for(int i=0;i<32;i++)
                    {
                        Assert.IsFalse(game.Motion.HasGrip(i));
                        var p=game.Root.InverseTransformPoint(game.Matter.Bodies[i].position);
                        shell.Curved.Nearest(p,out var q,out var inward);
                        Assert.GreaterOrEqual(Vector3.Dot(p-q,inward),-.009f,"Tissue escaped through solid wall: "+State+$" particle={i} local={p:F4} nearest={q:F4} inward={inward:F4}");
                    }
                }
                Assert.AreEqual(0,game.Matter.EscapedCount,State);Assert.IsFalse(game.Owner.Lost,State);
                game.Owner.TogglePause();var pose=game.Root.rotation;var centre=game.Motion.Centre(0);Steps(240);
                Assert.Less(Quaternion.Angle(pose,game.Root.rotation),.001f);Assert.AreEqual(centre,game.Motion.Centre(0));game.Owner.TogglePause();
                game.ResetLevel();Steps(120);Assert.Less(Quaternion.Angle(initial,game.Owner.Rotation.Orientation),.001f);
                Assert.AreEqual(32,game.Matter.Bodies.Length);Assert.AreEqual(0,game.Matter.EscapedCount);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            }
        }
        [UnityTest] public IEnumerator CurvedShellPickingAndPortraitFramingUseRealGeometry()
        {
            for(int slot=56;slot<=60;slot++)foreach(int height in new[]{1280,1612})
            {
                yield return Load(slot);var camera=game.Owner.View;var target=new RenderTexture(720,height,24);camera.targetTexture=target;
                try
                {
                    game.CameraRig.Frame(720,height,0,true);var bounds=game.Surfaces[0].Curved.LocalBounds;var rect=game.CameraRig.UsableRect(720,height,new Rect(0,0,720,height));
                    for(int i=0;i<8;i++)Assert.IsTrue(rect.Contains(camera.WorldToScreenPoint(game.Root.TransformPoint(VenomCampaignCamera.Corner(bounds,i)))),State);
                    var surface=game.Surfaces[0];bool picked=false;
                    var meshPoints=surface.Curved.Interior.vertices;
                    for(int i=0;i<meshPoints.Length&&!picked;i+=47)
                    {
                        Vector3 aim=surface.transform.TransformPoint(meshPoints[i]);
                        game.TouchPoint(camera.WorldToScreenPoint(aim));picked=game.Feedback.CommandSurface==surface;
                    }
                    Assert.IsTrue(picked,"Visible curved shell must accept taps: "+State);
                    Steps(120);Assert.AreEqual(0,game.Matter.EscapedCount);for(int i=0;i<32;i++)Assert.IsFalse(game.Motion.HasGrip(i));
                    COgheExpansionIntegrationTests.Capture(game,$"vessel-{slot}-portrait-{height}",720,height);
                    game.CameraRig.ToggleFollow();game.CameraRig.Frame(720,height,0,true);yield return null;
                    var presentation=game.GetComponent<COgheDayLabPresentation>();
                    Assert.IsNotEmpty(presentation.FocusOccluders,"Vessel mouldings must participate in close-view visibility.");
                    foreach(var renderer in presentation.FocusOccluders)Assert.IsFalse(renderer.enabled,"Follow view must reveal the body behind decoration.");
                    Assert.IsTrue(surface.GetComponent<Renderer>().enabled,"The physical satin shell remains visible.");
                    COgheExpansionIntegrationTests.Capture(game,$"vessel-{slot}-follow-{height}",720,height);
                    game.CameraRig.ToggleFollow();yield return null;
                    foreach(var renderer in presentation.FocusOccluders)Assert.IsTrue(renderer.enabled,"Overview restores the vessel identity.");
                }
                finally{camera.targetTexture=null;Object.DestroyImmediate(target);}
            }
        }
        [UnityTest] public IEnumerator CurvedProximityMatchesBruteForceAcrossBendsAndMouth()
        {
            for(int slot=56;slot<=60;slot++)
            {
                yield return Load(slot);var shell=game.Surfaces[0].Curved;var vertices=shell.Interior.vertices;var triangles=shell.Interior.triangles;var random=new System.Random(56);
                for(int trial=0;trial<80;trial++)
                {
                    var p=new Vector3((float)random.NextDouble()*.8f-.4f,(float)random.NextDouble()*.8f-.4f,(float)random.NextDouble()*.6f-.3f);
                    float best=float.PositiveInfinity;
                    for(int i=0;i<triangles.Length;i+=3){var q=COgheCurvedSurface.ClosestTriangle(p,vertices[triangles[i]],vertices[triangles[i+1]],vertices[triangles[i+2]]);best=Mathf.Min(best,(p-q).sqrMagnitude);}
                    shell.Nearest(p,out var actual,out var normal);Assert.AreEqual(best,(p-actual).sqrMagnitude,.000001f);Assert.AreEqual(1,normal.magnitude,.0001f);
                }
                // The final aperture remains empty for skin as well as collision.
                Vector3 hole=game.Root.InverseTransformPoint(game.Owner.Outlet.position);Vector3 normalOut=game.Root.InverseTransformDirection(game.Owner.Outlet.forward);
                var probe=hole+normalOut*.005f;Assert.IsFalse(shell.Constrain(ref probe,out _),"Do not paint skin across the real opening.");
                Assert.IsFalse(game.Surfaces[0].Shape.Raycast(new Ray(game.Owner.Outlet.position-game.Owner.Outlet.forward*.025f,game.Owner.Outlet.forward),out _, .06f),"Actual collider must leave the mouth open.");
                var edges=new Dictionary<ulong,int>();
                for(int i=0;i<triangles.Length;i+=3)for(int edge=0;edge<3;edge++)
                {
                    uint a=(uint)triangles[i+edge],b=(uint)triangles[i+(edge+1)%3];ulong key=((ulong)Math.Min(a,b)<<32)|Math.Max(a,b);
                    edges.TryGetValue(key,out int count);edges[key]=count+1;
                }
                int boundary=0;foreach(var count in edges.Values){Assert.LessOrEqual(count,2,"No nonmanifold internal seams");if(count==1)boundary++;}
                Assert.AreEqual(64,boundary,"The only open boundary is the 64-sided final mouth.");
            }
        }
        [UnityTest] public IEnumerator SurfaceTapsNeverAddDriveForceOrMoveCollisionParticles()
        {
            // Measure the exact same state before/after command processing. A comparison
            // of separate concave PhysX scene reloads also measures solver divergence.
            var forces=new Vector3[32];var torques=new Vector3[32];var positions=new Vector3[32];var velocities=new Vector3[32];
            for(int slot=56;slot<=60;slot++)
            {
                yield return Load(slot);var initial=game.Root.rotation;var surface=game.Surfaces[0];
                for(int pose=0;pose<3;pose++)
                {
                    game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(pose*10,pose*25,-pose*10)*initial);
                    Steps(120);
                    for(int i=0;i<32;i++)
                    {
                        var body=game.Matter.Bodies[i];forces[i]=body.GetAccumulatedForce(Dt);torques[i]=body.GetAccumulatedTorque(Dt);
                        positions[i]=body.position;velocities[i]=body.linearVelocity;
                        Assert.IsFalse(game.ExitAssisting(i),"This check must stay outside the authorized exit-assist region.");
                    }
                    var target=surface.Closest(game.Motion.Centre(0)+game.Root.right*.12f);
                    game.MoveTo(target,surface);Assert.NotNull(game.Motion.Get(0));game.Motion.Step(Dt);
                    game.Matter.GetComponent<VenomSurface>().Rebuild(false);
                    for(int i=0;i<32;i++)
                    {
                        var body=game.Matter.Bodies[i];Assert.IsFalse(game.Motion.HasGrip(i));
                        Assert.Less((forces[i]-body.GetAccumulatedForce(Dt)).sqrMagnitude,1e-12f,"A slippery command must add no force.");
                        Assert.Less((torques[i]-body.GetAccumulatedTorque(Dt)).sqrMagnitude,1e-12f,"A slippery command must add no torque.");
                        Assert.AreEqual(positions[i],body.position,"Animation must not move physics tissue.");
                        Assert.AreEqual(velocities[i],body.linearVelocity,"Animation must not change velocity.");
                    }
                    yield return null;
                }
            }
        }
        [UnityTest] public IEnumerator Campaign60NavigationPreservesOldContentAndStopsAtLastSlot()
        {
            yield return Load(55);
            foreach(int slot in new[]{56,57,58,59,60,7,55})
            {
                game.Load(slot);yield return null;game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
                Assert.AreEqual(slot,game.Definition.Order);
            }
            game.Load(61);yield return null;Assert.AreEqual(55,game.Definition.Order);
        }
    }
}
