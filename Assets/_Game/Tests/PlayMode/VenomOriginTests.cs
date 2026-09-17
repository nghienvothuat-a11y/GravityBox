using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GravityBox.Tests
{
    public sealed class VenomOriginTests
    {
        private VenomCampaign game;
        private Mouse inputMouse;
        private SimulationMode previousSimulation;
        private const float Dt=1f/120;
        [UnitySetUp] public IEnumerator Before()
        {VenomCampaignSave.PersistenceEnabled=false;previousSimulation=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;yield return null;}
        [UnityTearDown] public IEnumerator After()
        {if(inputMouse!=null){InputSystem.RemoveDevice(inputMouse);inputMouse=null;}Time.timeScale=1;Physics.simulationMode=previousSimulation;VenomCampaignSave.PersistenceEnabled=true;yield return null;}
        private IEnumerator Load(int n)
        {
            // Physics.SimulationMode.Script does not suppress FixedUpdate. Stop
            // force-producing components in sceneLoaded, before the first tick,
            // so paired trials start from the same state regardless of render time.
            void Loaded(Scene scene,LoadSceneMode mode)
            {
                game=Object.FindFirstObjectByType<VenomCampaign>();
                if(game==null)return;
                game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;
            }
            Time.timeScale=1;SceneManager.sceneLoaded+=Loaded;
            try{yield return SceneManager.LoadSceneAsync("VenomOrigin"+n.ToString("00"));}
            finally{SceneManager.sceneLoaded-=Loaded;}
            Assert.NotNull(game);Assert.AreEqual(0,game.Matter.SimulationTime,"No simulation ticks before the scripted trial");
            yield return null;
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
            game.Feedback?.Refresh();
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
        private void MouseStateAt(Vector2 point,bool down)
        {InputSystem.QueueStateEvent(inputMouse,new MouseState{position=point}.WithButton(MouseButton.Left,down));}
        private void ConsumeInputFrame()
        {
            InputSystem.Update();
            typeof(VenomCampaign).GetMethod("Update",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(game,null);
        }
        private void QueueFastMouseDrag(Vector2 start)
        {
            Vector2 end=start+Vector2.right*Mathf.Min(Screen.width,Screen.height)*.28f;
            MouseStateAt(start,false);MouseStateAt(start,true);MouseStateAt(end,true);MouseStateAt(end,false);MouseStateAt(start,false);
        }
        [UnityTest] public IEnumerator CampaignQueuedMouseDragRotatesBeforeCursorRestorationAndNeverBecomesATap()
        {
            yield return Load(18);inputMouse=InputSystem.AddDevice<Mouse>();
            Vector2 start=new Vector2(Screen.width*.35f,Screen.height*.5f);
            Quaternion before=game.Owner.Rotation.Orientation;
            QueueFastMouseDrag(start);ConsumeInputFrame();
            Assert.IsNull(game.Motion.Get(0),"A complete down/move/up/restore sequence before one Update must remain a drag.");
            Steps(120);
            Assert.Greater(Quaternion.Angle(before,game.Owner.Rotation.Orientation),5,"The actual rotation controller must receive the intermediate drag delta.");
            Assert.IsNull(game.Motion.Get(0));
        }
        [UnityTest] public IEnumerator CampaignQueuedMouseDragHonoursRotationLockAndPreservesRealTaps()
        {
            yield return Load(10);inputMouse=InputSystem.AddDevice<Mouse>();
            Assert.IsFalse(game.Definition.CanRotate);
            Quaternion before=game.Owner.Rotation.Orientation;
            QueueFastMouseDrag(new Vector2(Screen.width*.35f,Screen.height*.5f));ConsumeInputFrame();Steps(120);
            Assert.Less(Quaternion.Angle(before,game.Owner.Rotation.Orientation),.001f);
            Assert.IsNull(game.Motion.Get(0),"A drag in a locked lesson must not fall back to a move command.");
            yield return Load(1);
            Vector2 target=game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position);
            MouseStateAt(target,false);MouseStateAt(target,true);MouseStateAt(target,false);ConsumeInputFrame();
            Assert.IsTrue(game.Motion.Get(0)?.Exit??false,"A genuine down/up tap still chooses the visible outlet through the queued path.");
        }
        [UnityTest] public IEnumerator CampaignQueuedMouseInputIsDiscardedAcrossPauseResetAndDisable()
        {
            yield return Load(18);inputMouse=InputSystem.AddDevice<Mouse>();
            Vector2 start=new Vector2(Screen.width*.35f,Screen.height*.5f);
            for(int interruption=0;interruption<3;interruption++)
            {
                game.ResetLevel();Quaternion before=game.Owner.Rotation.Orientation;
                QueueFastMouseDrag(start);InputSystem.Update();
                if(interruption==0){game.Owner.TogglePause();ConsumeInputFrame();game.Owner.TogglePause();}
                else if(interruption==1)game.ResetLevel();
                else{game.enabled=false;game.enabled=true;}
                ConsumeInputFrame();Steps(60);
                Assert.Less(Quaternion.Angle(before,game.Owner.Rotation.Orientation),.001f,$"Stale gesture survived interruption {interruption}.");
                Assert.IsNull(game.Motion.Get(0));
            }
            // Resuming with the button still down must require a fresh press.
            MouseStateAt(start,true);ConsumeInputFrame();game.Owner.TogglePause();ConsumeInputFrame();game.Owner.TogglePause();
            Quaternion rest=game.Owner.Rotation.Orientation;
            MouseStateAt(start+Vector2.right*100,true);MouseStateAt(start+Vector2.right*100,false);ConsumeInputFrame();Steps(60);
            Assert.Less(Quaternion.Angle(rest,game.Owner.Rotation.Orientation),.001f);Assert.IsNull(game.Motion.Get(0));
        }
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
        [UnityTest] public IEnumerator EarlyLessonsAcceptCeilingTapsAfterBoxRotation()
        {
            foreach(int level in new[]{1,2,3})
            {
                yield return Load(level);
                var ceiling=System.Array.Find(game.Surfaces,p=>p.name=="Glass face 5");
                Assert.NotNull(ceiling);
                foreach(var orientation in new[]{Quaternion.Euler(180,0,0),Quaternion.Euler(180,45,0)})
                {
                    // Input-only fixture: orient the shell without advancing the
                    // creature through an accidental exit between two tap probes.
                    game.ResetLevel();
                    game.Root.GetComponent<Rigidbody>().rotation=orientation;
                    game.Root.rotation=orientation;
                    Physics.SyncTransforms();
                    Assert.IsTrue(game.Owner.CanControl);
                    Vector3 point=ceiling.transform.TransformPoint(new Vector3(-.10f,-.10f,0));
                    game.TouchPoint(game.Owner.View.WorldToScreenPoint(point));
                    Assert.AreSame(ceiling,game.Feedback.CommandSurface,$"Level {level}: the rotated ceiling must accept a screen tap.");
                    Assert.Less(Vector3.Distance(game.Feedback.CommandPoint,point),.002f);
                    var order=game.Motion.Get(game.Motion.Selected);
                    Assert.NotNull(order,$"Level {level}, orientation {orientation.eulerAngles}: accepted ceiling tap must issue a movement command.");
                    Assert.Less(Vector3.Distance(game.Root.TransformPoint(order.Target),point+ceiling.Normal*.019f),.002f);
                    game.Motion.StopAll();
                }
            }
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
        [UnityTest] public IEnumerator SecondLessonCrossesDividerAndReturns()
        {
            yield return Load(2);
            foreach(float x in new[]{.16f,-.16f,.16f,-.16f})
            {
                Vector3 target=new Vector3(x,-.278f,-.16f);
                game.Motion.Move(0,target);
                yield return Until(18,()=>game.Motion.Get(0)==null);
                Capture(x>0?"02-crossed":"02-returned");
                if(Vector3.Distance(game.Motion.Centre(0),target)>=.03f)
                {
                    string particles="";
                    for(int i=0;i<32;i++){game.Motion.Support(i,out var c,out _,out _);particles+=$"\n{i}: {game.Matter.Bodies[i].position:F5}, v={game.Matter.Bodies[i].linearVelocity:F5}, support={c?.name}, grip={game.Motion.HasGrip(i)}";}
                    Debug.Log("DIVIDER STALL "+State+particles);
                }
                Assert.Less(Vector3.Distance(game.Motion.Centre(0),target),.03f,"Must cross the divider in either direction: "+State);
                Assert.IsNull(game.Motion.Get(0),"Finish the command after the tail clears: "+State);
                foreach(var body in game.Matter.Bodies)
                    Assert.Greater(body.position.x*Mathf.Sign(x),.017f,"The whole body, including the tail, must clear the divider");
                Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Climbing must not tear off material");
            }
            AimExit();yield return Until(30,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator SecondLessonCanReverseWhileStraddlingDivider()
        {
            yield return Load(2);
            Vector3 start=new Vector3(-.16f,-.278f,-.16f),far=new Vector3(.16f,-.278f,-.16f);
            for(int pass=0;pass<3;pass++)
            {
                game.Motion.Move(0,far);
                yield return Until(18,()=>Mathf.Abs(game.Motion.Centre(0).x)<.02f&&game.Motion.Centre(0).y>-.19f);
                Assert.Greater(game.Motion.Centre(0).y,-.19f,"Reach the top before reversing: "+State);
                game.Motion.Move(0,start);
                yield return Until(18,()=>game.Motion.Get(0)==null);
                Assert.Less(Vector3.Distance(game.Motion.Centre(0),start),.03f,"Reverse at the rim: "+State);
                foreach(var body in game.Matter.Bodies)
                    Assert.Less(body.position.x,-.017f,"No tissue left behind after reversing");
            }
            AimExit();yield return Until(30,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator ThirdLessonCanReachRearExit(){yield return Exit(3);}
        [UnityTest] public IEnumerator AllTenScenesLoadAndKeepMaterialInside()
        {
            for(int n=1;n<=10;n++)
            {
                yield return Load(n);yield return Until(1,()=>false);
                Assert.AreEqual(32,game.Matter.Bodies.Length);Assert.AreEqual(1,game.Matter.TotalFragmentCount,"L"+n);
                Assert.IsFalse(game.Owner.Lost,"L"+n);Assert.IsFalse(game.Owner.Completed,"L"+n);
                Assert.AreEqual(n!=7&&n!=8&&n!=10,game.Definition.CanRotate);
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
            Capture("07-attached");
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
            yield return Load(5);yield return Rotate(Quaternion.Euler(0,0,180));Capture("05-inverted");AimExit();
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
        [UnityTest] public IEnumerator SixthLessonTapsAnimateSkiddingWithoutChangingPassivePhysics()
        {
            const int frames=150;
            var baseline=new Vector3[frames,32];float crawl=0,relativeSpeed=0;int skidding=0;
            for(int trial=0;trial<2;trial++)
            {
                yield return Load(6);Steps(240);yield return null;
                var surface=game.Matter.GetComponent<VenomSurface>();var life=game.Matter.GetComponent<VenomLifeAnimation>();
                var camera=game.Owner.View;var pivot=game.Owner.Rotation.GetComponent<Rigidbody>();
                Vector3 localTarget=Vector3.zero;
                if(trial==1)
                {
                    game.TouchPoint(camera.WorldToScreenPoint(game.Root.position+camera.transform.right*.13f));
                    Assert.NotNull(game.Motion.Get(0),"A tap on the visible curved shell must be acknowledged");
                    localTarget=game.Motion.Get(0).Target;
                    Assert.Less(Vector3.Dot(game.Root.TransformPoint(localTarget)-game.Root.position,camera.transform.forward),0,"Select the near shell, not the far side through the sphere");
                }
                for(int frame=0;frame<frames;frame++)
                {
                    if(frame==60)game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(0,0,65));
                    Steps(4);
                    var positions=new Vector3[32];var velocities=new Vector3[32];
                    for(int i=0;i<32;i++){positions[i]=game.Matter.Bodies[i].position;velocities[i]=game.Matter.Bodies[i].linearVelocity;}
                    surface.Rebuild(false);
                    for(int i=0;i<32;i++)
                    {
                        Assert.AreEqual(positions[i],game.Matter.Bodies[i].position,"Animation cannot move collision particles");
                        Assert.AreEqual(velocities[i],game.Matter.Bodies[i].linearVelocity,"Animation cannot add an impulse");
                        Assert.IsFalse(game.Motion.HasGrip(i),"Slick sphere never gains traction");
                        if(trial==0)baseline[frame,i]=positions[i];
                        else Assert.Less(Vector3.Distance(positions[i],baseline[frame,i]),.0005f,"Taps must preserve the no-command gravity/inertia trajectory");
                        if(frame>=60)relativeSpeed=Mathf.Max(relativeSpeed,(velocities[i]-pivot.GetPointVelocity(positions[i])).magnitude);
                    }
                    if(trial==1)
                    {
                        crawl=Mathf.Max(crawl,life.CrawlAmount);skidding=Mathf.Max(skidding,life.SlidingTendrilCount);
                        Assert.AreEqual(0,life.PlantedFeet.Count,"Attempted footsteps must slide, not lock to the sphere");
                    }
                    if(frame%30==0)yield return null;
                    if(trial==1&&frame==45)Capture("06-trying-to-crawl");
                }
                if(trial==1)
                {
                    Assert.AreEqual(localTarget,game.Motion.Get(0).Target,"The chosen point must rotate with the shell");
                    Assert.AreEqual("Cố bò / trượt",game.Activity);
                    Capture("06-sliding-while-rotated");
                }
            }
            Assert.Greater(crawl,.7f,"A stationary body must visibly try to crawl after a tap");
            Assert.Greater(skidding,0,"Show tendrils slipping across the curved contact surface");
            Assert.Greater(relativeSpeed,.05f,"The body must move relative to a rotating shell");
            game.ResetLevel();Steps(60);Assert.IsNull(game.Motion.Get(0));Assert.AreEqual(Vector3.zero,game.Motion.Intent(0));
        }
        [UnityTest] public IEnumerator SlickPlanarContactAcceptsCommandsAndAnimatesWithoutTraction()
        {
            const int frames=120;var baseline=new Vector3[frames,32];float crawl=0;
            for(int trial=0;trial<2;trial++)
            {
                yield return Load(1);
                var floor=System.Array.Find(game.Surfaces,p=>p.name=="Glass face 0");
                // A planar slick fixture in a normally active level verifies the
                // same traction rule outside passive sphere lesson 06.
                floor.Slippery=true;Steps(60);
                if(trial==1)
                {
                    game.TouchPoint(game.Owner.View.WorldToScreenPoint(new Vector3(.09f,-.3f,-.16f)));
                    Assert.AreSame(floor,game.Feedback.CommandSurface);
                    Assert.NotNull(game.Motion.Get(0));
                }
                for(int frame=0;frame<frames;frame++)
                {
                    Steps(4);game.Matter.GetComponent<VenomSurface>().Rebuild(false);
                    for(int i=0;i<32;i++)
                    {
                        Assert.IsFalse(game.Motion.HasGrip(i));
                        if(trial==0)baseline[frame,i]=game.Matter.Bodies[i].position;
                        else Assert.Less(Vector3.Distance(baseline[frame,i],game.Matter.Bodies[i].position),.0005f,
                            "Slick commands must not change the physical trajectory.");
                    }
                    if(trial==1) crawl=Mathf.Max(crawl,game.Matter.GetComponent<VenomLifeAnimation>().CrawlAmount);
                    if(frame%30==0)yield return null;
                }
            }
            Assert.Greater(crawl,.5f,"Show effort even when planar slick contact supplies no traction.");
        }
        [UnityTest] public IEnumerator SeventhLessonPushesPullsAndReleasesAfterThreeSeconds()
        {
            yield return Load(7);var prop=game.Props[0];game.SelectProp(prop);
            yield return Until(15,()=>game.Attached);Assert.IsTrue(game.Attached,"Attach: "+State);
            Vector3 origin=prop.Body.position;game.SetPropTarget(origin+Vector3.right*.16f);yield return Until(2,()=>false);
            Assert.Greater(prop.Body.position.x,origin.x+.01f,"Push: "+State);
            Capture("07-pushing");
            Vector3 pushed=prop.Body.position;game.SetPropTarget(pushed-Vector3.right*.12f);yield return Until(2,()=>false);
            Assert.Less(prop.Body.position.x,pushed.x-.01f,"Pull: "+State);
            Capture("07-pulling");
            yield return Until(1.2f,()=>!game.Attached);Assert.IsFalse(game.Attached,"Must release after three seconds");
        }
        [UnityTest] public IEnumerator SeventhLessonNeedsStepAndClimbsAfterPlacement()
        {yield return SolveSeventhLessonWithStep(0);}
        [UnityTest] public IEnumerator SeventhLessonCanClimbACratePlacedAtAnAngle()
        {yield return SolveSeventhLessonWithStep(12);yield return SolveSeventhLessonWithStep(-12);}
        private IEnumerator SolveSeventhLessonWithStep(float initialYaw)
        {
            yield return Load(7);AimExit();yield return Until(12,()=>game.Owner.Completed);Assert.IsFalse(game.Owner.Completed,"The plain wall must not bypass the box puzzle");
            game.ResetLevel();
            // Initial fixture only: all subsequent placement, climb and escape
            // must come from screen commands and ordinary physical contacts.
            game.Props[0].Body.rotation=Quaternion.Euler(0,initialYaw,0);
            Physics.SyncTransforms();Steps(60);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Props[0].Body.position));yield return Until(15,()=>game.Attached);Assert.IsTrue(game.Attached,State);
            bool climbed=false,exiting=false;
            bool ObserveClimbAndExit()
            {
                if(!climbed&&!game.Attached&&game.Motion.Centre(0).y>-.15f)
                {climbed=true;Capture("07-climbing-step");}
                if(!exiting&&game.Matter.EscapedCount>0)
                {exiting=true;Capture("07-through-hole");}
                return game.Owner.Completed;
            }
            for(int i=0;i<6&&!game.Owner.Completed;i++)
            {
                if(!game.Attached&&game.Motion.Get(0)==null){game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Props[0].Body.position));yield return Until(8,()=>game.Attached);}
                // Point to visible wall above the crate; the push target ignores
                // height. A low target would be hidden behind the crate itself.
                if(game.Attached)game.TouchPoint(game.Owner.View.WorldToScreenPoint(new Vector3(.3f,-.025f,0)));
                yield return Until(2.4f,ObserveClimbAndExit);
            }
            Capture("07-step-placed");yield return Until(25,ObserveClimbAndExit);Assert.IsTrue(game.Owner.Completed,State+$" prop={game.Props[0].Body.position}, tilt={game.Props[0].Body.rotation.eulerAngles}");
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
        [UnityTest] public IEnumerator EighthCloserCameraKeepsBothRoomsAndTubeInPortraitFrame()
        {
            yield return Load(8);
            Assert.AreEqual(.52f,game.Definition.ViewRadius,.0001f);
            Assert.IsTrue(game.Tube.AutoEnterOnContact);
            COgheExpansionIntegrationTests.Capture(game,"08-closer-overview");
            // Both glass boxes plus their frame rails, in actual portrait framing.
            foreach(float x in new[]{-.588f,.588f})foreach(float y in new[]{-.248f,.248f})foreach(float z in new[]{-.248f,.248f})
            {
                Vector3 screen=game.Owner.View.WorldToViewportPoint(game.Root.TransformPoint(new Vector3(x,y,z)));
                Assert.That(screen.x,Is.InRange(.025f,.975f),"Keep outer frame rails visible");
                Assert.That(screen.y,Is.InRange(.18f,.81f),"Keep the chambers clear of the HUD");
            }
        }
        [UnityTest] public IEnumerator EighthLessonCeilingAcceptsScreenTap()
        {
            yield return Load(8);yield return null;Vector3 target=new Vector3(-.49f,.23f,0);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(target));var order=game.Motion.Get(0);Assert.NotNull(order);
            Assert.Greater(game.Root.TransformPoint(order.Target).y,.18f,"A player must be able to tap the ceiling while box rotation is locked");
        }
        [UnityTest] public IEnumerator EighthLessonFallsCatchesAndFlowsThroughTube()
        {
            yield return Load(8);yield return null;
            Vector3 safeTop=new Vector3(-.49f,.208f,0);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(new Vector3(safeTop.x,.23f,0)));
            yield return Until(20,()=>Vector3.Distance(game.Motion.Centre(0),safeTop)<.04f);
            Vector3 perch=new Vector3(-.19f,.208f,0);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(new Vector3(perch.x,.23f,0)));
            yield return Until(22,()=>Vector3.Distance(game.Motion.Centre(0),perch)<.04f);
            Assert.Greater(game.Motion.Centre(0).y,.15f,"Reach ceiling: "+State);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Tube.transform.position));
            Assert.Less(Vector3.Distance(game.Root.TransformPoint(game.Motion.Get(0).Target),game.Tube.transform.position),.04f,"The visible inlet must not be intercepted by the ceiling");
            yield return Until(15,()=>game.InTube);Assert.IsTrue(game.InTube,"Catch ring after a screen tap: "+State);Steps(150);Capture("08-flow");
            yield return Until(15,()=>!game.InTube);Assert.IsFalse(game.InTube,"Flow: "+State);Assert.Greater(game.Motion.Centre(0).x,.11f);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.AreEqual(0,game.Matter.EscapedCount);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            Assert.IsTrue(game.Motion.Get(0)?.Exit??false,"The final exit must also accept a screen tap from the fixed camera");
            yield return Until(25,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator EighthLessonAutomaticallyEntersTubeAfterCatchingRing()
        {foreach(float offset in new[]{0f,-.035f,.035f})yield return CatchTubeFromRoof(offset);}
        [UnityTest] public IEnumerator EighthRoofHintGuidesClimbThenFallOntoCatchRing()
        {
            yield return Load(8);yield return null;
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Feedback.RoofHintPoint));
            Assert.NotNull(game.Motion.Get(0));
            Assert.Greater(game.Root.TransformPoint(game.Motion.Get(0).Target).y,.18f,"Roof cue must hit the ceiling, not the wall behind it");
            yield return Until(22,()=>Vector3.Distance(game.Motion.Centre(0),new Vector3(-.49f,.208f,0))<.04f);
            Assert.Greater(game.Motion.Centre(0).y,.18f,"The first cue reaches the safe ceiling: "+State);
            game.Feedback.Refresh();
            Assert.Greater(game.Feedback.RoofHintPoint.x,-.17f,"Only after climbing, indicate the slippery departure");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Feedback.RoofHintPoint));
            bool climbed=false,caught=false;
            for(int t=0;t<4800&&!game.Owner.Lost;t++)
            {
                Steps(1);climbed|=game.Motion.Centre(0).y>.15f;
                caught=game.Motion.TryCatchPoint(0,out _);
                if(caught)break;
                if(t%120==0)yield return null;
            }
            Assert.IsTrue(climbed,"The cue must lead onto the roof");
            Assert.IsTrue(caught,"Following the roof cue must produce a catchable fall: "+State);
            Capture("08-roof-cue-catch");
        }
        private IEnumerator CatchTubeFromRoof(float offset)
        {
            yield return Load(8);yield return null;
            var top=new Vector3(-.49f,.208f,offset);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(new Vector3(top.x,.23f,offset)));
            yield return Until(20,()=>Vector3.Distance(game.Motion.Centre(0),top)<.04f);
            Assert.Greater(game.Motion.Centre(0).y,.15f,"Reach the ceiling: "+State);
            Assert.IsFalse(game.Motion.TryCatchPoint(0,out _),"The ring cannot catch tissue still on the ceiling");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(new Vector3(-.12f,.23f,offset)));
            bool touched=false,caught=false;string samples="";
            for(int t=0;t<1200&&!game.Owner.Lost;t++)
            {
                Steps(1);int contacts=0,grips=0;Vector3 velocity=Vector3.zero;
                for(int i=0;i<32;i++)
                {
                    velocity+=game.Matter.Bodies[i].linearVelocity/32;
                    if(game.Motion.Support(i,out var shape,out var point,out _)&&shape==game.Tube.Entrance.Shape&&game.Tube.Entrance.Grip(point))
                    {contacts++;if(game.Motion.HasGrip(i))grips++;}
                }
                touched|=contacts>=2;
                caught=grips>=2&&Mathf.Abs(velocity.y)<.18f&&Vector3.Distance(game.Motion.Centre(0),game.Tube.transform.position)<.13f;
                if(t%24==0){samples+=$" [{t/120f:F1}s c={game.Motion.Centre(0):F3} vy={velocity.y:F2} ring={contacts} grip={grips}]";yield return null;}
                if(game.InTube){caught=true;break;}
                if(caught)break;
            }
            Assert.IsTrue(touched,"The fall must actually contact the ring: "+State+samples);
            Assert.IsTrue(caught,$"A real ring contact must arrest the fall without a timed second tap (offset {offset}): "+State+samples);
            // No additional tap: a real catch arms the existing physical flow.
            yield return Until(4,()=>game.InTube);Assert.IsTrue(game.InTube,"Automatically enter after catching, without a second command: "+State);
            yield return Until(20,()=>!game.InTube);Assert.Greater(game.Motion.Centre(0).x,.11f,"Pass through after the catch: "+State);
        }
        [UnityTest] public IEnumerator EighthLessonMissedRingKeepsFalling()
        {
            yield return Load(8);yield return null;var top=new Vector3(-.49f,.208f,.17f);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(new Vector3(top.x,.23f,top.z)));
            yield return Until(22,()=>Vector3.Distance(game.Motion.Centre(0),top)<.04f);
            Assert.Greater(game.Motion.Centre(0).y,.15f,"Reach the ceiling before a missed fall: "+State);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(new Vector3(-.12f,.23f,top.z)));
            bool caught=false;
            for(int t=0;t<1440&&game.Motion.Centre(0).y>-.17f;t++)
            {Steps(1);caught|=game.Motion.TryCatchPoint(0,out _);if(t%120==0)yield return null;}
            Assert.Less(game.Motion.Centre(0).y,-.17f,"A fall outside the ring must reach the floor");
            Assert.IsFalse(caught,"The ring must not attract tissue from outside its contact area");
            Assert.IsFalse(game.InTube);Assert.IsFalse(game.Owner.Lost);
        }
        [UnityTest] public IEnumerator NinthLessonCoverFallsAwayUnderGravity()
        {
            yield return Load(9);var cover=game.Props[0];yield return Rotate(Quaternion.Euler(0,0,180));
            yield return Until(3,()=>false);Assert.IsTrue(game.Clear(game.Owner.Outlet.position-game.Owner.Outlet.forward*.06f,game.Owner.Outlet.position+game.Owner.Outlet.forward*.06f),"Fallen cover must leave the actual mouth clear");
            Capture("09-cover-fallen");
            AimExit();yield return Until(30,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator TouchFeedbackUsesThePickedSurfaceAndFollowsRotation()
        {
            yield return Load(3);yield return null;
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            var feedback=game.Feedback;Assert.IsTrue(feedback.HasCommand);
            Assert.NotNull(feedback.CommandSurface,"Hidden exit tap must acknowledge the near glass, not the far hole");
            Vector3 local=game.Root.InverseTransformPoint(feedback.CommandPoint);
            Vector3 goal=game.Root.TransformPoint(game.Motion.Get(0).Target);
            Assert.Less(Vector3.Distance(goal,feedback.CommandPoint+feedback.CommandNormal*.019f),.0005f);
            Capture("03-touch-surface");
            yield return Rotate(Quaternion.Euler(0,60,0));
            Assert.Less(Vector3.Distance(feedback.CommandPoint,game.Root.TransformPoint(local)),.0005f);
            Assert.Greater(Vector3.Dot(feedback.CommandNormal,feedback.CommandSurface.Normal),.999f);
            game.ResetLevel();Assert.IsFalse(feedback.HasCommand);
        }
        [UnityTest] public IEnumerator NinthLidWithstandsClimbingAndRepeatedFastRotation()
        {
            yield return Load(9);var prop=game.Props[0];
            var boxes=prop.GetComponentsInChildren<BoxCollider>();
            float worst=0;
            void CheckBounds()
            {
                foreach(var box in boxes)for(int c=0;c<8;c++)
                {
                    var sign=new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1);
                    Vector3 local=prop.transform.InverseTransformPoint(box.transform.TransformPoint(box.center+Vector3.Scale(sign,box.size*.5f)));
                    Vector3 p=Quaternion.Inverse(game.Root.GetComponent<Rigidbody>().rotation)*(prop.Body.position+prop.Body.rotation*local-game.Root.position);
                    worst=Mathf.Max(worst,Mathf.Abs(p.x),Mathf.Abs(p.y),Mathf.Abs(p.z));
                    Assert.LessOrEqual(Mathf.Max(Mathf.Abs(p.x),Mathf.Abs(p.y),Mathf.Abs(p.z)),.308f,"Loose lid penetrated the chamber: "+p);
                }
            }
            var top=System.Array.Find(prop.GetComponentsInChildren<VenomSurfacePatch>(),p=>Vector3.Dot(p.Normal,Vector3.up)>.9f);
            game.MoveTo(top.transform.position,top);
            bool touched=false;float minY=float.PositiveInfinity,maxY=float.NegativeInfinity;
            for(int t=0;t<1200;t++)
            {
                Steps(1);CheckBounds();
                for(int i=0;i<32;i++)if(game.Motion.Support(i,out var shape,out _,out _)&&shape.attachedRigidbody==prop.Body)touched=true;
                if(t>900){minY=Mathf.Min(minY,prop.Body.position.y);maxY=Mathf.Max(maxY,prop.Body.position.y);}
                if(t%120==0)yield return null;
            }
            Assert.IsTrue(touched,"Exercise actual creature-to-lid contact");
            Assert.Less(maxY-minY,.008f,"Resting lid should not bounce visibly under the creature");
            Capture("09-reinforced-lid");game.Motion.StopAll();
            foreach(var angle in new[]{new Vector3(170,15,0),new Vector3(-130,160,90),new Vector3(35,-120,170),new Vector3(180,0,0),new Vector3(0,180,160),Vector3.zero})
            {
                game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(angle));
                for(int t=0;t<360;t++){Steps(1);CheckBounds();if(t%120==0)yield return null;}
            }
            Assert.IsFalse(game.Owner.Lost,State);Capture("09-after-fast-rotation");
            Debug.Log($"Lid stress: worst corner {worst:F5} m; settled vertical range {maxY-minY:F5} m");
        }
        [UnityTest] public IEnumerator BossRequiresCutHoldReuniteAndEscape()
        {
            yield return Load(10);game.RequestCut();yield return Until(20,()=>game.Matter.TotalFragmentCount>1);
            Steps(120); // Let the visible cut impulse separate the two bodies.
            Assert.AreEqual(2,game.Matter.TotalFragmentCount,"Knife: "+State);
            int other=-1;for(int i=1;i<32;i++)if(game.Matter.Groups[i]!=game.Matter.Groups[0]){other=i;break;}
            game.Motion.Move(0,game.PadA.position+Vector3.up*.018f,true);
            yield return Until(15,()=>game.MassA>=.012f);Assert.GreaterOrEqual(game.MassA,.012f,"A: "+State);
            game.Motion.Move(other,game.PadB.position+Vector3.up*.018f,true);
            yield return Until(15,()=>game.GateOpen);Assert.IsTrue(game.GateOpen,"B: "+State+$" other={game.Motion.Centre(other)}, mass={game.MassB}, cover={game.ButtonCover.position}, clock={game.Matter.SimulationTime}");
            yield return new WaitForSeconds(.3f);
            var feedback=game.GetComponent<COgheBossPresentation>();
            Assert.Less(feedback.PadCaps[0].localPosition.z,.002f);Assert.Less(feedback.PadCaps[1].localPosition.z,.002f);
            Capture("10-cooperation");Vector3 reunion=new Vector3(0,-.277f,.13f);game.Motion.Move(0,reunion);game.Motion.Move(other,reunion);
            yield return Until(15,()=>game.Matter.TotalFragmentCount==1);Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Reunite: "+State);
            yield return new WaitForSeconds(.3f);Assert.Greater(feedback.PadCaps[0].localPosition.z,.006f);Assert.Greater(feedback.PadCaps[1].localPosition.z,.006f);
            AimExit();yield return Until(30,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);Assert.IsTrue(game.Progress.HomeUnlocked);
            game.EnterHome();Assert.IsTrue(game.Home);Assert.IsFalse(game.Owner.Rotation.InputEnabled);yield return null;Steps(180);Capture("home");
        }
        private IEnumerator OpenBossWithTwoParts()
        {
            yield return Load(10);game.RequestCut();yield return Until(20,()=>game.Matter.TotalFragmentCount>1);
            Steps(120); // Commands begin after the sideways cut motion settles.
            Assert.AreEqual(2,game.Matter.TotalFragmentCount,State);
            int other=OtherPart();
            game.Motion.Move(0,game.PadA.position+Vector3.up*.018f,true);
            yield return Until(15,()=>game.MassA>=.012f);
            game.Motion.Move(other,game.PadB.position+Vector3.up*.018f,true);
            yield return Until(15,()=>game.GateOpen);Assert.IsTrue(game.GateOpen,State);
        }
        private int OtherPart()
        {for(int i=1;i<32;i++)if(game.Matter.Groups[i]!=game.Matter.Groups[0])return i;return -1;}
        [UnityTest] public IEnumerator BossSecondCutPreservesUnrelatedPadHolderOrderAndImpulse()
        {
            yield return Load(10);game.RequestCut();yield return Until(20,()=>game.Matter.TotalFragmentCount>1);
            Steps(120);Assert.AreEqual(2,game.Matter.TotalFragmentCount,State);
            int worker=OtherPart();
            game.Motion.Move(0,game.PadA.position+Vector3.up*.018f,true);
            yield return Until(15,()=>game.MassA>=.012f);
            Assert.GreaterOrEqual(game.MassA,.012f,"The unaffected fragment must physically hold A before the second cut.");
            yield return Until(5,()=>game.KnifePhase==VenomCampaign.BladePhase.Ready);
            Assert.AreEqual(VenomCampaign.BladePhase.Ready,game.KnifePhase);

            // Isolate the second blade callback from gravity/contact forces.
            // Only the worker is arranged across the real edge; the holder stays
            // at its measured pad contact. This fixture does not claim a route.
            int[] groups=(int[])game.Matter.Groups.Clone();int k=0;
            for(int i=0;i<32;i++)if(groups[i]==groups[worker])
            {
                game.Matter.Bodies[i].transform.position=game.Root.TransformPoint(new Vector3(
                    k%2==0?-.013f:.013f,-.278f+(k/8)*.018f,-.157f+(k/2%4)*.018f));
                k++;
            }
            Assert.GreaterOrEqual(k,4);
            Physics.SyncTransforms();game.Motion.Move(worker,game.Root.TransformPoint(new Vector3(0,-.275f,-.13f)),true);
            var hold=game.Motion.Get(0);Assert.NotNull(hold);Assert.IsTrue(hold.Holding);
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var bladeStep=typeof(VenomCampaign).GetMethod("StepKnife",flags);Assert.NotNull(bladeStep);
            for(int tick=0;tick<130&&game.KnifePhase!=VenomCampaign.BladePhase.Falling;tick++)bladeStep.Invoke(game,new object[]{Dt});
            Assert.AreEqual(VenomCampaign.BladePhase.Falling,game.KnifePhase);
            // Cut reads the blade Transform, whereas its fall threshold reads
            // Rigidbody.position. Sync an authored fixture pose into both; a
            // Rigidbody-only assignment is not a simulated transform update.
            game.Knife.transform.position=game.Root.TransformPoint(new Vector3(0,-.23f,-.13f));Physics.SyncTransforms();
            Assert.Less(Vector3.Distance(game.Knife.position,game.Knife.transform.position),.000001f);
            Assert.IsFalse((bool)typeof(VenomCampaign).GetField("cutDone",flags).GetValue(game),"The new stroke must be armed.");
            var connected=(bool[,])typeof(CohesiveOrganism).GetField("connected",flags).GetValue(game.Matter);
            int crossingBonds=0;
            for(int i=0;i<32;i++)if(groups[i]==groups[worker])
            {
                Vector3 local=game.Knife.transform.InverseTransformPoint(game.Matter.Bodies[i].position);
                Assert.Less(Mathf.Abs(local.y),.065f,"Worker tissue must lie inside the actual blade's cutting span.");
                Assert.Less(Mathf.Abs(local.z),.075f);
                for(int j=i+1;j<32;j++)if(groups[j]==groups[worker]&&connected[i,j]&&
                    game.Matter.CrossesBlade(game.Knife.transform,new Vector3(.006f,.065f,.075f),i,j))crossingBonds++;
            }
            Assert.Greater(crossingBonds,0,"The second fixture must cross existing intact worker bonds, not merely nearby particles.");
            var forces=new Vector3[32];var velocities=new Vector3[32];
            for(int i=0;i<32;i++){forces[i]=game.Matter.Bodies[i].GetAccumulatedForce(Dt);velocities[i]=game.Matter.Bodies[i].linearVelocity;}
            int cuts=game.Matter.CutCount;bladeStep.Invoke(game,new object[]{Dt});
            Assert.Greater(game.Matter.CutCount,cuts,"The second stroke must actually sever the worker's bonds.");
            Assert.Greater(game.Matter.TotalFragmentCount,2);
            Assert.AreSame(hold,game.Motion.Get(0),"Cutting the worker must preserve the unrelated pad-hold command.");
            Vector3 netImpulse=Vector3.zero;float separationImpulse=0;
            for(int i=0;i<32;i++)
            {
                Vector3 impulse=(game.Matter.Bodies[i].GetAccumulatedForce(Dt)-forces[i])*Dt;
                if(groups[i]==groups[0])
                {
                    Assert.Less(impulse.magnitude,.000001f,$"Unrelated holder particle {i} received a cut impulse.");
                    Assert.AreEqual(velocities[i],game.Matter.Bodies[i].linearVelocity);
                }
                else
                {
                    Assert.IsNull(game.Motion.Get(i),"Only the actually split group loses its old movement command.");
                    netImpulse+=impulse;separationImpulse+=impulse.magnitude;
                }
            }
            Assert.Greater(separationImpulse,.001f,"New pieces must retain the visible physical separation impulse.");
            Assert.Less(netImpulse.magnitude,.000001f,"The scoped separation must conserve momentum.");
            var afterFirstCallback=new Vector3[32];for(int i=0;i<32;i++)afterFirstCallback[i]=game.Matter.Bodies[i].GetAccumulatedForce(Dt);
            bladeStep.Invoke(game,new object[]{Dt});
            Assert.AreEqual(cuts+1,game.Matter.CutCount,"cutDone must prevent a second cut in the same stroke.");
            for(int i=0;i<32;i++)Assert.AreEqual(afterFirstCallback[i],game.Matter.Bodies[i].GetAccumulatedForce(Dt),"No repeated tissue impulse during this stroke.");
        }
        [UnityTest] public IEnumerator BossCutImpulseKeepsIdlePartsSeparatedUntilGuidedTogether()
        {
            yield return Load(10);game.RequestCut();yield return Until(20,()=>game.Matter.TotalFragmentCount>1);
            Assert.AreEqual(2,game.Matter.TotalFragmentCount);int other=OtherPart();
            Steps(120);
            Assert.AreEqual(2,game.Matter.TotalFragmentCount,"The physical kick must stop an immediate rejoin without player input");
            float nearest=float.PositiveInfinity;
            for(int a=0;a<32;a++)for(int b=a+1;b<32;b++)
                if(game.Matter.Groups[a]!=game.Matter.Groups[b])nearest=Mathf.Min(nearest,Vector3.Distance(game.Matter.Bodies[a].position,game.Matter.Bodies[b].position));
            Assert.Greater(nearest,game.Matter.Profile.Spacing*Mathf.Sqrt(1.32f)+.005f,"Actual tissue gap must exceed the fusion range");
            Assert.Greater(Vector3.Distance(game.Motion.Centre(0),game.Motion.Centre(other)),.07f);
            Steps(360);Assert.AreEqual(2,game.Matter.TotalFragmentCount,"Stay apart after the old timer would have expired");
            Assert.AreEqual(0,game.Matter.EscapedCount);Assert.IsFalse(game.Owner.Lost);
            foreach(var body in game.Matter.Bodies)
            {var p=game.Root.InverseTransformPoint(body.position);Assert.Less(Mathf.Abs(p.x),.30f);Assert.Less(p.y,-.25f,"No upward ejection");}
            yield return null;Capture("10-cut-separated");
            var target=new Vector3(0,-.278f,.13f);
            game.Motion.Move(0,target);game.Motion.Move(other,target);
            yield return Until(15,()=>game.Matter.TotalFragmentCount==1);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Return within contact range to fuse normally");
            Assert.AreEqual(.096f,game.Matter.TotalMass,.0001f);
        }
        [UnityTest] public IEnumerator BossMovingPartMergesWithHeldButtonAndKeepsNewestCommand()
        {
            yield return OpenBossWithTwoParts();
            int other=OtherPart();Assert.IsTrue(game.Motion.Busy(0));
            Vector3 target=new Vector3(-.22f,-.278f,.13f);
            game.Motion.Selected=other;game.Motion.Move(other,target);
            int command=game.Motion.Get(other).CommandId;
            yield return Until(15,()=>game.Matter.TotalFragmentCount==1);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"A held pad must not veto proximity fusion: "+State);
            Assert.AreEqual(command,game.Motion.Get(other)?.CommandId,"Newest command survives, not the old pad hold");
            Assert.IsFalse(game.Motion.Busy(other));Assert.IsTrue(game.GateOpen);
            AimExit();yield return Until(30,()=>game.Owner.Completed);Assert.IsTrue(game.Owner.Completed,State);
        }
        [UnityTest] public IEnumerator BossMovingPartMergesWithIdlePart()
        {
            yield return OpenBossWithTwoParts();
            int other=OtherPart();game.Motion.Cancel(0);Assert.IsNull(game.Motion.Get(0));
            game.Motion.Move(other,new Vector3(-.22f,-.278f,.13f));
            yield return Until(15,()=>game.Matter.TotalFragmentCount==1);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"A moving part can merge into an idle one before arriving: "+State);
            Assert.NotNull(game.Motion.Get(other));Assert.IsFalse(game.Owner.Lost);
        }
        [UnityTest] public IEnumerator BossPartsWithDifferentDestinationsMergeWhenTheyMeet()
        {
            yield return OpenBossWithTwoParts();
            int other=OtherPart();
            game.Motion.Move(0,new Vector3(.22f,-.278f,.13f));
            game.Motion.Move(other,new Vector3(-.22f,-.278f,.13f));
            int command=game.Motion.Get(other).CommandId;
            yield return Until(15,()=>game.Matter.TotalFragmentCount==1);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Destination disagreement must not prevent fusion: "+State);
            Assert.AreEqual(command,game.Motion.Get(0)?.CommandId);Assert.IsFalse(game.Owner.Lost);
        }
        [UnityTest] public IEnumerator ProximityFusionHasNoTimerAndRespectsSolidDivider()
        {
            yield return Load(2);
            // Isolated contact fixture: no solver step or puzzle completion is
            // inferred from these arranged particles. Full Boss tests use forces.
            var blade=new GameObject("Fusion fixture cutter").transform;blade.position=game.Motion.Centre(0);
            game.Matter.Cut(blade,Vector3.one);Assert.AreEqual(2,game.Matter.TotalFragmentCount);
            int left=game.Matter.Groups[0];var ids=(int[])game.Matter.Groups.Clone();
            void Arrange(float separation,float offset)
            {
                int a=0,b=0;
                for(int i=0;i<32;i++)
                {
                    bool first=ids[i]==left;int k=first?a++:b++;
                    game.Matter.Bodies[i].position=new Vector3(offset+(first?-1:1)*separation,-.27f+(k%4)*.0185f,-.1f+(k/4)*.0185f);
                }
            }
            Arrange(.04f,.15f);game.Matter.Step(Dt);
            Assert.AreEqual(2,game.Matter.TotalFragmentCount,"Distant parts must not merge");
            Arrange(.010f,0);game.Matter.Step(Dt);
            Assert.AreEqual(2,game.Matter.TotalFragmentCount,"Nearby tissue must not merge through a solid divider");
            Arrange(.010f,.15f);game.Matter.Step(Dt);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Unobstructed nearby tissue merges immediately, with no cut cooldown");
            Object.Destroy(blade.gameObject);
        }
        [UnityTest] public IEnumerator BossSeparatedPartsCanClimbAndHoldCeiling()
        {
            yield return OpenBossWithTwoParts();
            int other=OtherPart();
            foreach(int anchor in new[]{0,other})
            {
                Vector3 target=new Vector3(anchor==0?-.18f:.18f,.278f,.16f);
                game.Motion.Move(anchor,target,true);
                yield return Until(25,()=>Vector3.Distance(game.Motion.Centre(anchor),target)<.03f);
                Assert.Less(Vector3.Distance(game.Motion.Centre(anchor),target),.03f,"Split fragment cannot reach ceiling: "+State);
                Steps(360);
                Assert.Greater(game.Motion.Centre(anchor).y,.25f,"Split fragment must remain attached on normal glass");
            }
            Assert.AreEqual(2,game.Matter.TotalFragmentCount);Assert.IsFalse(game.Owner.Lost);
            yield return null;Capture("10-split-ceiling");
            Vector3 reunion=new Vector3(0,.278f,.16f);
            game.Motion.Move(0,reunion);game.Motion.Move(other,reunion);
            yield return Until(15,()=>game.Matter.TotalFragmentCount==1);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Fragments can reunite on the ceiling: "+State);
            AimExit();yield return Until(25,()=>game.Owner.Completed);
            Assert.IsTrue(game.Owner.Completed,"Reunited creature must exit from the ceiling: "+State);
        }
        [UnityTest] public IEnumerator BossUnmergedExitShowsFailureInsteadOfFallingRepeatedly()
        {
            yield return OpenBossWithTwoParts();
            AimExit();yield return Until(30,()=>game.Owner.Lost);
            Assert.IsTrue(game.Owner.Lost,"An unmerged exit attempt must reach the exit rule, not fall forever: "+State);
            Assert.AreEqual(VenomCampaign.MergeFailure,game.Failure);Assert.IsFalse(game.Owner.Completed);
            Assert.AreEqual(0,game.Matter.EscapedCount);Assert.IsFalse(game.Progress.HomeUnlocked);
            game.ResetLevel();Steps(60);Assert.IsFalse(game.Owner.Lost);Assert.AreEqual(1,game.Matter.TotalFragmentCount);
        }
        [UnityTest] public IEnumerator BossWarnsOneSecondThenDropsAndResets()
        {
            yield return Load(10);Assert.IsFalse(game.Definition.CanRotate);Assert.IsFalse(game.Owner.Rotation.InputEnabled);
            float readyHeight=game.Knife.position.y;
            game.RequestCut();yield return Until(15,()=>game.KnifePhase==VenomCampaign.BladePhase.Warning);
            Assert.AreEqual(VenomCampaign.BladePhase.Warning,game.KnifePhase,State);
            float entered=game.Matter.SimulationTime;
            Steps(108); // 0.9 s, regardless of render frame rate.
            Assert.AreEqual(VenomCampaign.BladePhase.Warning,game.KnifePhase);
            Assert.AreEqual(0,game.Matter.CutCount);Assert.AreEqual(readyHeight,game.Knife.position.y,.003f);
            yield return null;Capture("10-warning");
            yield return Until(.2f,()=>game.KnifePhase==VenomCampaign.BladePhase.Falling);
            Assert.GreaterOrEqual(game.Matter.SimulationTime-entered,.999f);
            Assert.Less(game.Matter.SimulationTime-entered,1.03f);
            yield return Until(1,()=>game.Matter.TotalFragmentCount>1);Assert.Greater(game.Matter.TotalFragmentCount,1,State);
            yield return null;Capture("10-cut");
            Assert.IsFalse(game.Owner.Rotation.InputEnabled);
            game.ResetLevel();Steps(60);Assert.AreEqual(VenomCampaign.BladePhase.Ready,game.KnifePhase);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.AreEqual(readyHeight,game.Knife.position.y,.003f);
            Assert.IsFalse(game.Owner.Rotation.InputEnabled);
        }
        [UnityTest] public IEnumerator BossCanMissCreatureThatLeavesDuringWarning()
        {
            yield return Load(10);game.RequestCut();yield return Until(15,()=>game.KnifePhase==VenomCampaign.BladePhase.Warning);
            Assert.AreEqual(VenomCampaign.BladePhase.Warning,game.KnifePhase);
            game.Motion.Move(0,new Vector3(-.18f,-.275f,-.13f));
            yield return Until(3,()=>game.KnifePhase==VenomCampaign.BladePhase.Returning);
            Assert.AreEqual(VenomCampaign.BladePhase.Returning,game.KnifePhase);
            Assert.AreEqual(0,game.Matter.CutCount,"A triggered machine must not cut tissue that has moved away");
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.Less(game.Motion.Centre(0).x,-.08f);
            yield return Until(3,()=>game.KnifePhase==VenomCampaign.BladePhase.Ready);
            Assert.AreEqual(VenomCampaign.BladePhase.Ready,game.KnifePhase);Assert.IsFalse(game.Owner.Lost);
        }
        [UnityTest] public IEnumerator BossCutsAtPlayersChosenOffset()
        {
            yield return Load(10);game.RequestCut();yield return Until(15,()=>game.KnifePhase==VenomCampaign.BladePhase.Warning);
            Assert.AreEqual(VenomCampaign.BladePhase.Warning,game.KnifePhase);
            game.Motion.Move(0,new Vector3(.018f,-.275f,-.13f),true);
            yield return Until(3,()=>game.Matter.TotalFragmentCount>1);
            Assert.AreEqual(2,game.Matter.TotalFragmentCount,State);
            int positive=0,negative=0;
            foreach(var part in game.Matter.Bodies){if(part.position.x>0)positive++;else negative++;}
            Assert.Greater(positive,negative+2,"The late offset must remain effective, rather than forcing a central 50/50 cut");
            Assert.IsFalse(game.Owner.Lost);yield return null;Capture("10-offset-cut");
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
