using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Rendering;

namespace GravityBox.Tests
{
    public sealed class VenomOriginTests
    {
        private VenomCampaign game;
        private SimulationMode previousSimulation;
        private const float Dt=1f/120;
        [UnitySetUp] public IEnumerator Before()
        {VenomCampaignSave.PersistenceEnabled=false;previousSimulation=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {Time.timeScale=1;Physics.simulationMode=previousSimulation;VenomCampaignSave.PersistenceEnabled=true;yield return null;}
        private IEnumerator Load(int n)
        {
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("VenomOrigin"+n.ToString("00"));yield return null;
            game=Object.FindFirstObjectByType<VenomCampaign>();Assert.NotNull(game);game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            Steps(60);
            if(n==2||n==8)DumpGraph(n);
        }
        private void Capture(string name)
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            string dir="Artifacts/Venom01/OriginFrames";Directory.CreateDirectory(dir);
            var cam=game.Owner.View;cam.aspect=720f/1280;cam.transform.rotation=Quaternion.Euler(game.Definition.CameraEuler);
            Vector3 focus=game.Home?game.Motion.Centre(0):Vector3.zero;cam.transform.position=focus-cam.transform.forward*2;
            cam.orthographicSize=game.Home?.22f:game.Definition.ViewRadius*1280/(720*.87f);
            game.Matter.GetComponent<VenomSurface>().Rebuild(false);
            var target=new RenderTexture(720,1280,24){antiAliasing=4};target.Create();var old=RenderTexture.active;var picture=new Texture2D(720,1280,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};RenderPipeline.SubmitRenderRequest(cam,request);RenderPipeline.SubmitRenderRequest(cam,request);
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,720,1280),0,0);picture.Apply();File.WriteAllBytes(Path.Combine(dir,name+".png"),picture.EncodeToPNG());
            }
            finally{RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }
        private void DumpGraph(int n)
        {
            var type=game.Motion.GetType();var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var nodes=(System.Collections.Generic.List<Vector3>)type.GetField("nodes",flags).GetValue(game.Motion);
            var links=(System.Collections.Generic.List<System.Collections.Generic.List<int>>)type.GetField("links",flags).GetValue(game.Motion);
            var lines=new System.Collections.Generic.List<string>();
            for(int i=0;i<nodes.Count;i++)lines.Add($"{nodes[i].x},{nodes[i].y},{nodes[i].z},"+string.Join(" ",links[i]));
            Directory.CreateDirectory("Artifacts/Venom01");File.WriteAllLines($"Artifacts/Venom01/graph{n}.csv",lines);
        }
        private IEnumerator Until(float seconds,System.Func<bool> condition)
        {
            int ticks=Mathf.CeilToInt(seconds/Dt);
            for(int i=0;i<ticks&&!condition()&&!game.Owner.Lost;i++){Steps(1);if(i%240==0)yield return null;}
        }
        private void Steps(int n){for(int i=0;i<n;i++){game.Owner.Step(Dt);game.Owner.Rotation.Step(Dt);Physics.Simulate(Dt);}}
        private string State=>$"centre={game.Motion.Centre(0)}, groups={game.Matter.TotalFragmentCount}, escaped={game.Matter.EscapedCount}, {game.Activity}, {game.Failure}; cursor={game.Motion.Get(0)?.Cursor}, path={string.Join(";",game.Motion.Get(0)?.Path??new System.Collections.Generic.List<Vector3>())}";
        private void AimExit(){game.Motion.Move(0,game.Owner.Outlet.position-game.Owner.Outlet.forward*.020f,false,true);}
        private IEnumerator Rotate(Quaternion q){game.Owner.Rotation.SetTargetOrientation(q);yield return Until(6,()=>Quaternion.Angle(game.Root.rotation,q)<.2f);}
        private IEnumerator Exit(int n)
        {
            yield return Load(n);
            game.Motion.Move(0,game.Owner.Outlet.position-game.Owner.Outlet.forward*.020f,false,true);
            yield return Until(35,()=>game.Owner.Completed);
            var o=game.Motion.Get(0);
            Assert.IsTrue(game.Owner.Completed,$"L{n}: centre={game.Motion.Centre(0)} escaped={game.Matter.EscapedCount}, groups={game.Matter.TotalFragmentCount}, {game.Failure}; cursor={o?.Cursor}, path={string.Join(";",o?.Path??new System.Collections.Generic.List<Vector3>())}");
        }
        [UnityTest] public IEnumerator FirstLessonCrawlsToFarFloorExit(){yield return Exit(1);}
        [UnityTest] public IEnumerator FirstLessonAcceptsVisibleHoleTapAndExits()
        {
            yield return Load(1);yield return null;
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            var ray=game.Owner.View.ScreenPointToRay(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            var hits=Physics.RaycastAll(ray,5);System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            string evidence="";foreach(var hit in hits)evidence+=$" {hit.collider.name}: {hit.distance:F5} selectable={hit.collider.GetComponent<VenomSurfacePatch>()?.Selectable}";
            Assert.IsTrue(game.Motion.Get(0)?.Exit??false,"The visible floor hole must accept a screen tap; "+evidence);
            yield return Until(35,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator SecondLessonCrossesLowWallAndClimbs(){yield return Exit(2);}
        [UnityTest] public IEnumerator ThirdLessonCanReachRearExit(){yield return Exit(3);}
        [UnityTest] public IEnumerator AllTenScenesLoadAndKeepMaterialInside()
        {
            for(int n=1;n<=10;n++)
            {
                yield return Load(n);yield return Until(1,()=>false);
                Assert.AreEqual(32,game.Matter.Bodies.Length);Assert.AreEqual(1,game.Matter.TotalFragmentCount,"L"+n);
                Assert.IsFalse(game.Owner.Lost,"L"+n);Assert.IsFalse(game.Owner.Completed,"L"+n);
                Assert.AreEqual(n!=7&&n!=8,game.Definition.CanRotate);
                Assert.AreEqual(n==10,game.Definition.Boss);Capture($"overview-{n:00}");
            }
        }
        [UnityTest] public IEnumerator ThirdLessonFrontGlassInterceptsUntilRotated()
        {
            yield return Load(3);yield return null;game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            Assert.IsFalse(game.Motion.Get(0)?.Exit??false,"Hidden rear hole must not be picked through front pane");
            yield return Rotate(Quaternion.Euler(0,180,0));yield return null;game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            Assert.IsTrue(game.Motion.Get(0)?.Exit??false,"Exposed rear hole accepts the tap");
        }
        [UnityTest] public IEnumerator SeventhLessonBoxAcceptsScreenTap()
        {
            yield return Load(7);yield return null;game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Props[0].Body.position));
            yield return Until(15,()=>game.Attached);Assert.IsTrue(game.Attached,State);
        }
        [UnityTest] public IEnumerator FourthLessonRoutesAroundSlipperyPatch()
        {
            yield return Load(4);
            game.Motion.Move(0,new Vector3(.278f,.10f,.21f));yield return Until(20,()=>Vector3.Distance(game.Motion.Centre(0),new Vector3(.278f,.10f,.21f))<.04f);
            AimExit();yield return Until(20,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator FourthLessonPeelsAndFallsWhenClimbingIntoSlipperyBand()
        {
            yield return Load(4);
            Vector3 below=new Vector3(.278f,-.14f,0);
            game.Motion.Move(0,below);yield return Until(25,()=>Vector3.Distance(game.Motion.Centre(0),below)<.04f);
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),below),.04f,"Reach the safe wall below the band: "+State);
            game.Motion.Cancel(0);float restingHeight=game.Motion.Centre(0).y;yield return Until(1.5f,()=>false);
            Assert.Greater(game.Motion.Centre(0).y,restingHeight-.025f,"A healthy footprint must still hold on ordinary glass");
            AimExit();bool touched=false,fell=false;float highest=-1;string samples="";
            for(int t=0;t<2400&&!game.Owner.Completed&&!game.Owner.Lost;t++)
            {
                Steps(1);int grips=0,slick=0;
                for(int i=0;i<32;i++)
                {
                    if(game.Motion.HasGrip(i))grips++;
                    if(game.Motion.Support(i,out var collider,out var point,out _)&&collider.GetComponent<VenomSurfacePatch>() is VenomSurfacePatch patch&&!patch.Grip(point))slick++;
                }
                touched|=slick>2;
                float y=game.Motion.Centre(0).y;
                if(touched){highest=Mathf.Max(highest,y);fell|=highest-y>.09f;}
                if(t%120==0){samples+=$" [{t/120f:F1}s y={y:F3} grips={grips} slick={slick}]";yield return null;}
                if(fell)break;
            }
            Assert.IsTrue(touched,"Must actually enter the slippery band: "+State+samples);
            Assert.IsTrue(fell,"A few lower anchors must not suspend the body indefinitely: "+State+samples);
            Assert.IsFalse(game.Owner.Completed,"Climbing straight through the coating must not bypass the puzzle");
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Slipping must not tear off material");
            Capture("04-slipped");
            Vector3 detour=new Vector3(.278f,.10f,.21f);
            game.Motion.Move(0,detour);yield return Until(25,()=>Vector3.Distance(game.Motion.Centre(0),detour)<.04f);
            AimExit();yield return Until(20,()=>game.Owner.Completed);
            Assert.IsTrue(game.Owner.Completed,"After a slip, a new safe route must still complete the level: "+State);
        }
        [UnityTest] public IEnumerator FifthLessonFlipsSlipperyCeilingDown()
        {
            yield return Load(5);yield return Rotate(Quaternion.Euler(0,0,180));AimExit();
            yield return Until(12,()=>game.Owner.Completed);
            for(int t=0;t<1800&&!game.Owner.Completed;t++)
            {
                if(t%24==0)
                {
                    Vector3 v=Vector3.zero;foreach(var b in game.Matter.Bodies)v+=b.linearVelocity/32;
                    Vector3 error=Vector3.ProjectOnPlane(game.Owner.Outlet.position-game.Motion.Centre(0),Vector3.up);
                    Vector3 acceleration=Vector3.ClampMagnitude(error*3-Vector3.ProjectOnPlane(v,Vector3.up)*3,.65f);
                    Vector3 normal=(Vector3.up+acceleration/9.81f).normalized;
                    game.Owner.Rotation.SetTargetOrientation(Quaternion.FromToRotation(Vector3.up,normal)*Quaternion.Euler(0,0,180));
                }
                Steps(1);if(t%240==0)yield return null;
            }
            Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator SixthLessonRotatesHoleToPassiveBody()
        {
            yield return Load(6);yield return Rotate(Quaternion.Euler(0,0,180));
            yield return Until(15,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator SeventhLessonPushesPullsAndReleasesAfterThreeSeconds()
        {
            yield return Load(7);var prop=game.Props[0];game.SelectProp(prop);
            yield return Until(15,()=>game.Attached);Assert.IsTrue(game.Attached,"Attach: "+State);
            Vector3 origin=prop.Body.position;game.SetPropTarget(origin+Vector3.right*.16f);yield return Until(2,()=>false);
            Assert.Greater(prop.Body.position.x,origin.x+.01f,"Push: "+State);
            Vector3 pushed=prop.Body.position;game.SetPropTarget(pushed-Vector3.right*.12f);yield return Until(2,()=>false);
            Assert.Less(prop.Body.position.x,pushed.x-.01f,"Pull: "+State);
            yield return Until(1.2f,()=>!game.Attached);Assert.IsFalse(game.Attached,"Must release after three seconds");
        }
        [UnityTest] public IEnumerator SeventhLessonNeedsStepAndClimbsAfterPlacement()
        {
            yield return Load(7);AimExit();yield return Until(12,()=>game.Owner.Completed);Assert.IsFalse(game.Owner.Completed,"The plain wall must not bypass the box puzzle");
            game.ResetLevel();Steps(60);game.SelectProp(game.Props[0]);yield return Until(15,()=>game.Attached);Assert.IsTrue(game.Attached,State);
            for(int i=0;i<6&&!game.Owner.Completed;i++)
            {
                if(!game.Attached&&game.Motion.Get(0)==null){game.SelectProp(game.Props[0]);yield return Until(8,()=>game.Attached);}
                if(game.Attached)game.SetPropTarget(new Vector3(.3f,-.21f,0));
                yield return Until(2.4f,()=>game.Owner.Completed);
            }
            Capture("07-step-placed");yield return Until(25,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State+$" prop={game.Props[0].Body.position}, tilt={game.Props[0].Body.rotation.eulerAngles}");
        }
        [UnityTest] public IEnumerator SeventhLessonCanPullBoxAwayFromGlass()
        {
            yield return Load(7);var prop=game.Props[0];game.SelectProp(prop);yield return Until(15,()=>game.Attached);Assert.IsTrue(game.Attached,State);
            for(int i=0;i<4;i++){if(!game.Attached)game.SelectProp(prop);yield return Until(4,()=>game.Attached);game.SetPropTarget(new Vector3(.3f,-.21f,-.13f));yield return Until(2.4f,()=>false);}
            Assert.Greater(prop.Body.position.x,.17f,"Reach glass");Vector3 atWall=prop.Body.position;
            if(!game.Attached){game.SelectProp(prop);yield return Until(8,()=>game.Attached);}
            game.SetPropTarget(new Vector3(.02f,-.21f,-.13f));yield return Until(2.4f,()=>false);
            Assert.Less(prop.Body.position.x,atWall.x-.025f,"Pull away from glass: "+State);
        }
        [UnityTest] public IEnumerator EighthLessonCeilingAcceptsScreenTap()
        {
            yield return Load(8);yield return null;Vector3 target=new Vector3(-.49f,.23f,0);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(target));var order=game.Motion.Get(0);Assert.NotNull(order);
            Assert.Greater(game.Root.TransformPoint(order.Target).y,.18f,"A player must be able to tap the ceiling while box rotation is locked");
        }
        [UnityTest] public IEnumerator EighthLessonFallsCatchesAndFlowsThroughTube()
        {
            yield return Load(8);Vector3 safeTop=new Vector3(-.49f,.208f,0);game.Motion.Move(0,safeTop);
            yield return Until(20,()=>Vector3.Distance(game.Motion.Centre(0),safeTop)<.04f);
            Vector3 perch=new Vector3(-.19f,.208f,0);game.Motion.Move(0,perch);
            yield return Until(22,()=>Vector3.Distance(game.Motion.Centre(0),perch)<.04f);
            Assert.Greater(game.Motion.Centre(0).y,.15f,"Reach ceiling: "+State);
            game.Motion.Move(0,game.Tube.transform.position-Vector3.right*.020f);
            yield return Until(15,()=>game.EnterTube());Assert.IsTrue(game.InTube,"Catch ring: "+State);Steps(150);Capture("08-flow");
            yield return Until(15,()=>!game.InTube);Assert.IsFalse(game.InTube,"Flow: "+State);Assert.Greater(game.Motion.Centre(0).x,.11f);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.AreEqual(0,game.Matter.EscapedCount);
            AimExit();yield return Until(25,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator NinthLessonCoverFallsAwayUnderGravity()
        {
            yield return Load(9);var cover=game.Props[0];yield return Rotate(Quaternion.Euler(0,0,180));
            yield return Until(3,()=>false);Assert.IsTrue(game.Clear(game.Owner.Outlet.position-game.Owner.Outlet.forward*.06f,game.Owner.Outlet.position+game.Owner.Outlet.forward*.06f),"Fallen cover must leave the actual mouth clear");
            AimExit();yield return Until(30,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator BossRequiresCutHoldReuniteAndEscape()
        {
            yield return Load(10);game.RequestCut();yield return Until(20,()=>game.Matter.TotalFragmentCount>1);
            Assert.AreEqual(2,game.Matter.TotalFragmentCount,"Knife: "+State);
            int other=-1;for(int i=1;i<32;i++)if(game.Matter.Groups[i]!=game.Matter.Groups[0]){other=i;break;}
            game.Motion.Move(0,game.PadA.position+Vector3.up*.018f,true);
            yield return Until(15,()=>game.MassA>=.012f);Assert.GreaterOrEqual(game.MassA,.012f,"A: "+State);
            game.Motion.Move(other,game.PadB.position+Vector3.up*.018f,true);
            yield return Until(15,()=>game.GateOpen);Assert.IsTrue(game.GateOpen,"B: "+State+$" other={game.Motion.Centre(other)}, mass={game.MassB}, cover={game.ButtonCover.position}, clock={game.Matter.SimulationTime}");
            Capture("10-cooperation");Vector3 reunion=new Vector3(0,-.277f,.13f);game.Motion.Move(0,reunion);game.Motion.Move(other,reunion);
            yield return Until(15,()=>game.Matter.TotalFragmentCount==1);Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Reunite: "+State);
            AimExit();yield return Until(30,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);Assert.IsTrue(game.Progress.HomeUnlocked);
            game.EnterHome();Assert.IsTrue(game.Home);Assert.IsFalse(game.Owner.Rotation.InputEnabled);yield return null;Steps(180);Capture("home");
        }
        [UnityTest] public IEnumerator UnmergedDepartureFailsBeforeOtherPieceCanFollow()
        {
            yield return Load(1);
            // Isolated detector fixture: construct two actual groups, then sweep one
            // particle through the real exit. Full-level solutions never teleport.
            var blade=new GameObject("Test cutter").transform;blade.position=game.Motion.Centre(0);
            game.Matter.Cut(blade,Vector3.one);Assert.Greater(game.Matter.TotalFragmentCount,1);
            var b=game.Matter.Bodies[0];b.position=game.Owner.Outlet.TransformPoint(Vector3.back*.03f);game.EvaluateExit();
            b.position=game.Owner.Outlet.TransformPoint(Vector3.forward*.025f);game.EvaluateExit();
            Assert.IsTrue(game.Owner.Lost);Assert.AreEqual(VenomCampaign.MergeFailure,game.Failure);Assert.IsFalse(game.Progress.HomeUnlocked);
            game.ResetLevel();Assert.IsFalse(game.Owner.Lost);Object.Destroy(blade.gameObject);
        }
        [Test] public void BossWinUnlocksCollectionOnceAndOnlyBoss()
        {
            var save=new VenomCampaignSave();var d=ScriptableObject.CreateInstance<VenomCampaignDefinition>();d.Id="venom.origin.09";save.Win(d);Assert.IsFalse(save.HomeUnlocked);
            d.Id="venom.origin.10";d.Boss=true;save.Win(d);Assert.IsTrue(save.HomeUnlocked);save.RevealHome=false;save.Win(d);Assert.IsFalse(save.RevealHome);Assert.AreEqual(2,save.Completed.Count);Object.DestroyImmediate(d);
        }
    }
}
