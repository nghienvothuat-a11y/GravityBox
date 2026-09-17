using System.Collections;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Rendering;

namespace GravityBox.Tests
{
    public sealed class COghePipeExpansionTests
    {
        private const float Dt=1f/120;
        private VenomCampaign game;
        private SimulationMode previousSimulation;

        [UnitySetUp]
        public IEnumerator Before()
        {
            VenomCampaignSave.PersistenceEnabled=false;previousSimulation=Physics.simulationMode;
            Physics.simulationMode=SimulationMode.Script;yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Time.timeScale=1;Physics.simulationMode=previousSimulation;
            VenomCampaignSave.PersistenceEnabled=true;yield return null;
        }

        private IEnumerator Load(int number)
        {
            void Loaded(Scene scene,LoadSceneMode mode)
            {
                game=Object.FindFirstObjectByType<VenomCampaign>();
                if(game==null)return;game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            }
            SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync($"VenomOrigin{number:00}");}
            finally{SceneManager.sceneLoaded-=Loaded;}
            Assert.NotNull(game);yield return null;Steps(30);
        }

        private void Steps(int count)
        {for(int i=0;i<count;i++){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);Physics.Simulate(Dt);}}

        private IEnumerator Until(float seconds,System.Func<bool> condition)
        {
            int count=Mathf.CeilToInt(seconds/Dt);
            for(int i=0;i<count&&!condition()&&!game.Owner.Lost;i++){Steps(1);if(i%240==0)yield return null;}
        }

        private void Capture(string name)
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            string directory="Artifacts/COgheExpansion/Pipes";Directory.CreateDirectory(directory);
            var camera=game.Owner.View;camera.aspect=720f/1280;camera.transform.rotation=Quaternion.Euler(game.Definition.CameraEuler);
            camera.transform.position=-camera.transform.forward*3;camera.orthographicSize=game.Definition.ViewRadius*1280/(720*.87f);
            game.Matter.GetComponent<VenomSurface>().Rebuild(false);
            var target=new RenderTexture(720,1280,24){antiAliasing=4};target.Create();var old=RenderTexture.active;
            var picture=new Texture2D(720,1280,TextureFormat.RGBA32,false);
            try
            {
                RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=target});
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,720,1280),0,0);picture.Apply();
                File.WriteAllBytes(Path.Combine(directory,name+".png"),picture.EncodeToPNG());
            }
            finally{RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }

        [UnityTest]
        public IEnumerator TappingAVisibleFarMouthQueuesARealSurfaceApproachThenEntersOnContact()
        {
            yield return Load(15);var tube=Object.FindFirstObjectByType<COgheTubeNetwork>();Assert.NotNull(tube);
            Vector3 mouth=game.Root.TransformPoint(tube.Nodes[0].LocalPosition);
            Vector3 far=game.Root.TransformPoint(new Vector3(-.045f,-.275f,-.20f));
            game.Motion.Move(0,far);
            yield return Until(8,()=>Vector3.Distance(game.Motion.Centre(0),mouth)>.13f);
            Assert.Greater(Vector3.Distance(game.Motion.Centre(0),mouth),.11f,"test body must begin beyond direct-entry range");

            Camera camera=game.Owner.View;
            game.TouchPoint(camera.WorldToScreenPoint(mouth));
            Assert.NotNull(game.Motion.Get(0),"a visible mouth tap queues locomotion to its physical approach surface");
            Assert.IsFalse(tube.IsParticleInside(0),"far tissue does not enter before reaching the mouth");
            yield return Until(10,()=>tube.IsParticleInside(0));
            Assert.IsTrue(tube.IsParticleInside(0),"the queued command starts flow only after real mouth contact; "+tube.DebugState(0));
            Assert.AreEqual(0,tube.LastChosenEdge);
        }

        [UnityTest]
        public IEnumerator MazeFlowsToAJunctionWaitsAndPhysicallyReturnsFromADeadEnd()
        {
            yield return Load(15);var tube=Object.FindFirstObjectByType<COgheTubeNetwork>();Assert.NotNull(tube);
            Assert.AreEqual(10,tube.Nodes.Length);Assert.AreEqual(10,tube.Edges.Length);
            CollectionAssert.AreEquivalent(new[]{0,1,2},Adjacent(tube,1),"A must connect only to In, B and C");
            CollectionAssert.AreEquivalent(new[]{3,4,7},Adjacent(tube,4),"D must connect only to B, C and E");
            for(int node=1;node<=5;node++)
            {
                int[] ports=Adjacent(tube,node);
                for(int a=0;a<ports.Length;a++)for(int b=a+1;b<ports.Length;b++)
                    Assert.GreaterOrEqual(Vector3.Angle(PortDirection(tube,ports[a],node),PortDirection(tube,ports[b],node)),85f,
                        $"junction {tube.Nodes[node].Name} has overlapping physical tube ports {ports[a]}/{ports[b]}");
            }
            for(int a=0;a<tube.Edges.Length;a++)for(int b=a+1;b<tube.Edges.Length;b++)
            {
                var first=tube.Edges[a];var second=tube.Edges[b];
                int shared=first.A==second.A||first.A==second.B?first.A:first.B==second.A||first.B==second.B?first.B:-1;
                Vector3[] pa=COgheTubeNetwork.SampleCurve(first.ControlPoints,24),pb=COgheTubeNetwork.SampleCurve(second.ControlPoints,24);
                float clearance=float.PositiveInfinity;
                foreach(Vector3 x in pa)foreach(Vector3 y in pb)
                {
                    if(shared>=0&&(Vector3.Distance(x,tube.Nodes[shared].LocalPosition)<tube.JunctionRadius*1.2f||
                        Vector3.Distance(y,tube.Nodes[shared].LocalPosition)<tube.JunctionRadius*1.2f))continue;
                    clearance=Mathf.Min(clearance,Vector3.Distance(x,y));
                }
                Assert.GreaterOrEqual(clearance,.085f,$"tube centre-lines {a}/{b} overlap beyond their real junction envelope");
            }

            Assert.IsTrue(tube.TryChoose(0,0),"The body begins physically beside the inlet");
            yield return Until(8,()=>tube.LastReachedNode==1);
            Assert.AreEqual(1,tube.LastReachedNode,"The entrance edge stops at A instead of solving ahead; "+tube.DebugState(0));Capture("15-at-A");
            Vector3 atA=game.Motion.Centre(0);Steps(120);
            Assert.Less(Vector3.Distance(atA,game.Motion.Centre(0)),.025f,"A waits for an explicit branch choice");

            Assert.IsTrue(tube.TryChoose(0,1));yield return Until(8,()=>tube.LastReachedNode==2);
            Assert.AreEqual(2,tube.LastReachedNode,"A–B stalled; "+tube.DebugState(0));
            Assert.IsTrue(tube.TryChoose(0,5));yield return Until(8,()=>tube.LastReachedNode==6);
            Assert.AreEqual(6,tube.LastReachedNode,"B–dead-end stalled; "+tube.DebugState(0));Capture("15-dead-end");
            Assert.IsFalse(game.Owner.Completed,"A capped dead end is reversible and cannot count as escape");
            Assert.IsFalse(tube.ExitReached);
            Assert.IsTrue(tube.TryChoose(0,5),"The same physical edge can be selected in reverse");
            yield return Until(8,()=>tube.LastReachedNode==2);
            Assert.AreEqual(2,tube.LastReachedNode,"dead-end return stalled; "+tube.DebugState(0));Capture("15-returned-B");
        }

        [UnityTest]
        public IEnumerator MazeBranchCanBeChosenByAnActualScreenTap()
        {
            yield return Load(15);var tube=Object.FindFirstObjectByType<COgheTubeNetwork>();Assert.NotNull(tube);
            Vector3[] outside={new Vector3(.24f,.299f,.24f),new Vector3(.24f,-.299f,-.24f)};
            void Tap(Vector3 local)=>game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Root.TransformPoint(local)));
            void AssertSurfaceTapsIgnored()
            {
                int edge=tube.LastChosenEdge;Vector3 marker=game.Feedback.CommandPoint;
                var face=game.Feedback.CommandSurface;
                foreach(var point in outside)
                {
                    Tap(point);
                    Assert.IsNull(game.Motion.Get(0),"Inside the maze, a shell tap must not create a crawl destination.");
                    Assert.AreEqual(edge,tube.LastChosenEdge,"A shell tap must not choose another pipe.");
                    Assert.AreEqual(marker,game.Feedback.CommandPoint,"Rejected shell taps must not move the destination marker.");
                    Assert.AreSame(face,game.Feedback.CommandSurface);
                }
            }
            Tap(outside[0]);Assert.NotNull(game.Motion.Get(0),"Before entry, the creature still accepts surface guidance.");
            game.ResetLevel();Steps(30);
            Assert.IsTrue(tube.TryChoose(0,0));
            AssertSurfaceTapsIgnored(); // Flowing between junctions.
            yield return Until(8,()=>tube.LastReachedNode==1);
            Assert.AreEqual(1,tube.LastReachedNode,"entry did not reach junction A; "+tube.DebugState(0));
            AssertSurfaceTapsIgnored(); // Waiting for a branch selection.

            // Tap the visible early section of A–C through the campaign's real
            // screen input path. The fixed frame and perspective camera must
            // resolve this branch without calling the tube command API.
            Vector3 branch=game.Root.TransformPoint(tube.Edges[2].ControlPoints[1]);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(branch));
            Assert.AreEqual(2,tube.LastChosenEdge,"screen tap did not select the visible A–C branch; "+tube.DebugState(0));
            yield return Until(8,()=>tube.LastReachedNode==3);
            Assert.AreEqual(3,tube.LastReachedNode,"screen-selected A–C branch did not physically reach C; "+tube.DebugState(0));
            game.ResetLevel();Steps(30);Tap(outside[0]);
            Assert.NotNull(game.Motion.Get(0),"Reset must restore normal surface input before the new entry.");
        }

        [UnityTest]
        public IEnumerator MazeOnlyUnlocksExitAfterTheChosenConnectedRouteReachesItsDistalTerminal()
        {
            yield return Load(15);var tube=Object.FindFirstObjectByType<COgheTubeNetwork>();
            Assert.IsFalse(game.FinalExitAvailable);Assert.IsFalse(tube.ExitReached);
            int[] route={0,2,6,6,4,3,5,5,3,7,8,8,9};
            int[] nodes={1,3,7,3,4,2,6,2,4,5,8,5,9};
            for(int i=0;i<route.Length;i++)
            {
                Assert.IsTrue(tube.TryChoose(0,route[i]),$"edge {route[i]} at node {tube.LastReachedNode}");
                int target=nodes[i];yield return Until(9,()=>tube.LastReachedNode==target||tube.ExitReached);
                Assert.IsTrue(tube.LastReachedNode==target||tube.ExitReached,$"force-driven travel stalled before node {target}; {tube.DebugState(0)}");
            }
            Assert.IsTrue(tube.ExitReached);Assert.IsTrue(game.FinalExitAvailable);
            yield return Until(8,()=>game.Owner.Completed);
            Assert.IsTrue(game.Owner.Completed,"Only the real distal detector completes the merged body");
            Assert.AreEqual(CohesiveOrganism.ParticleCount,game.Matter.EscapedCount);
            Capture("15-true-exit");
        }



        [UnityTest]
        public IEnumerator FusionIsOpenAtAMouthButBlockedAcrossRealTubeAndCapGeometry()
        {
            yield return Load(15);var tube=Object.FindFirstObjectByType<COgheTubeNetwork>();Assert.NotNull(tube);
            Transform root=game.Root;

            var inlet=tube.Edges[0];
            Vector3 mouth=inlet.ControlPoints[0];
            Vector3 into=(inlet.ControlPoints[1]-mouth).normalized;
            Assert.IsFalse(tube.SolidSeparates(root.TransformPoint(mouth-into*.025f),root.TransformPoint(mouth+into*.025f)),
                "nearby tissue may fuse through a real open mouth");

            Vector3 centre=Vector3.Lerp(inlet.ControlPoints[0],inlet.ControlPoints[1],.5f);
            Vector3 side=Vector3.Cross(into,Mathf.Abs(Vector3.Dot(into,Vector3.up))>.8f?Vector3.right:Vector3.up).normalized;
            Assert.IsTrue(tube.SolidSeparates(root.TransformPoint(centre),root.TransformPoint(centre+side*(tube.Radius+.02f))),
                "the swept tube wall is a real solid barrier to fusion");

            var deadEnd=tube.Edges[5];
            int last=deadEnd.ControlPoints.Length-1;
            Vector3 cap=deadEnd.ControlPoints[last];
            Vector3 towardCap=(cap-deadEnd.ControlPoints[last-1]).normalized;
            Assert.IsTrue(tube.SolidSeparates(root.TransformPoint(cap-towardCap*.025f),root.TransformPoint(cap+towardCap*.025f)),
                "the physical dead-end cap blocks fusion in either ray direction");
            Assert.AreEqual(CohesiveOrganism.ParticleCount,game.Matter.Bodies.Length);
        }



        private static int[] Adjacent(COgheTubeNetwork tube,int node)
        {
            var result=new List<int>();
            for(int i=0;i<tube.Edges.Length;i++)if(tube.Edges[i].A==node||tube.Edges[i].B==node)result.Add(i);
            return result.ToArray();
        }

        private static Vector3 PortDirection(COgheTubeNetwork tube,int edgeIndex,int node)
        {
            var edge=tube.Edges[edgeIndex];var points=edge.ControlPoints;
            return (edge.A==node?points[1]-points[0]:points[points.Length-2]-points[points.Length-1]).normalized;
        }
    }
}
