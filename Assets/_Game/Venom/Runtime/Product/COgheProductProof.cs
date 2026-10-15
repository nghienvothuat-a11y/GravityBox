#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in review reel of the main menu's monster as a player sees it (UI included): one PNG per frame at a
    /// fixed 30 fps and the sounds it made (-coghe-monster-reel &lt;dir&gt;). Development builds only; writes no saves.</summary>
    public sealed class COgheMonsterReel : MonoBehaviour
    {
        [Serializable] private sealed class DeviceRequest { public string[] args; }
        private static string[] requestedArgs;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            requestedArgs=Environment.GetCommandLineArgs();
            // iOS IL2CPP does not consistently forward native launch arguments to System.Environment.
            // An explicit, consumed request in the app sandbox enables the same test fixture.
            string request=Path.Combine(Application.persistentDataPath,"COgheMonsterRequest.json");
            if(File.Exists(request))
            {
                try { requestedArgs=JsonUtility.FromJson<DeviceRequest>(File.ReadAllText(request)).args??requestedArgs; }
                finally { File.Delete(request); }
            }
            if(Array.IndexOf(requestedArgs,"-coghe-monster-reel")<0)return;
            Debug.Log("COGHE MONSTER REQUEST "+Application.persistentDataPath);
            VenomCampaignSave.PersistenceEnabled=false;COgheProductMode.OverrideForTests=true;
            var go=new GameObject("Monster reel");DontDestroyOnLoad(go);go.AddComponent<COgheMonsterReel>();
        }
        [Serializable] private struct MonsterTiming
        { public float fps,p95Ms; public double skinMs,faceMs; public int frames,maxSkinVertices; public string gpu; public bool home; }
        private IEnumerator Start()
        {
            var args=requestedArgs;int at=Array.IndexOf(args,"-coghe-monster-reel");
            string output=at+1<args.Length&&!args[at+1].StartsWith("-")?Path.GetFullPath(args[at+1]):Path.Combine(Application.persistentDataPath,"MonsterReel");
            Directory.CreateDirectory(output);
            if (!Application.isMobilePlatform) Screen.SetResolution(472,1022,FullScreenMode.Windowed);
            Application.runInBackground=true;Screen.sleepTimeout=SleepTimeout.NeverSleep;
            var sounds=new System.Text.StringBuilder();int frame=0;
            System.Action<string,float> heard=(clip,volume)=>sounds.AppendLine($"{frame/30f:F3} {clip} {volume:F2}");
            yield return new WaitForSecondsRealtime(1.5f);
            var game=FindFirstObjectByType<VenomCampaign>();
            if(game==null||game.ProductUI==null){Debug.LogError("Monster reel: no menu");Application.Quit(1);yield break;}
            bool home = Array.IndexOf(args, "-coghe-monster-home") >= 0;
            bool realtime = Array.IndexOf(args, "-coghe-monster-realtime") >= 0;
            if (home)
            {
                // A review fixture only: opens the existing Home without changing saved progress/entitlements.
                game.Progress.HomeUnlocked = true; game.ProductUI.OpenHome(); game.ProductUI.ToggleHomeZoom();
                yield return new WaitForSecondsRealtime(2);
            }
            else game.ProductUI.ShowMenu();
            if (realtime)
            {
                int cyclesAt=Array.IndexOf(args,"-coghe-monster-cycles");
                int cycles=3;
                if(cyclesAt>=0&&cyclesAt+1<args.Length&&int.TryParse(args[cyclesAt+1],out int requested))cycles=Mathf.Clamp(requested,1,90);
                yield return Measure(game,output,home,cycles);
                Debug.Log("COGHE MONSTER REEL DONE");Application.Quit(0);yield break;
            }
            game.Personality.Force(COgheAct.Monster);
            COgheAudio.Heard+=heard;Time.captureFramerate=realtime?0:30;
            var samples = new System.Collections.Generic.List<float>(1200);
            COgheMobileMetrics.Enabled = realtime; double skinMs = 0, faceMs = 0; Array.Clear(COgheMobileMetrics.Milliseconds,0,4);
            int maxVertices=0;float until=Time.realtimeSinceStartup+COghePersonality.MonsterLength+.6f;
            int total=Mathf.CeilToInt((COghePersonality.MonsterLength+.6f)*30);
            for(frame=0;realtime?Time.realtimeSinceStartup<until:frame<total;frame++)
            {
                if (!realtime)
                {
                    bool stills=Array.IndexOf(args,"-coghe-monster-stills")>=0;
                    if(!stills||frame==45||frame==99||frame==174||frame==281)
                        ScreenCapture.CaptureScreenshot(Path.Combine(output,$"frame_{frame:00000}.png"));
                }
                else if (frame>2)
                {
                    samples.Add(Time.unscaledDeltaTime*1000); skinMs+=COgheMobileMetrics.Milliseconds[0];
                    var face=game.Matter.GetComponent<COgheMonsterFace>();if(face!=null)faceMs+=face.LastBuildMilliseconds;
                }
                Array.Clear(COgheMobileMetrics.Milliseconds,0,4);
                maxVertices=Mathf.Max(maxVertices,game.Matter.GetComponent<VenomSurface>().VertexCount);
                yield return null;
            }
            Time.captureFramerate=0;COgheAudio.Heard-=heard;
            File.WriteAllText(Path.Combine(output,"sounds.txt"),sounds.ToString());
            if (realtime && samples.Count>0)
            {
                float mean=samples.Average();samples.Sort();
                File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(new MonsterTiming {
                    fps=1000/mean,p95Ms=samples[(int)(samples.Count*.95f)],frames=samples.Count,maxSkinVertices=maxVertices,
                    gpu=SystemInfo.graphicsDeviceName,home=home,skinMs=skinMs/samples.Count,faceMs=faceMs/samples.Count },true));
            }
            Debug.Log("COGHE MONSTER REEL DONE");Application.Quit(0);
        }
        [Serializable] private struct DeviceTiming
        {
            public string device,os,gpu,unity; public int width,height,targetFps,cycles,frames,over33,over50,gcCollections,maxSkinVertices;
            public float seconds,fps,p50Ms,p95Ms,p99Ms,worstMs,batteryStart,batteryEnd;
            public string batteryStatus; public double skinMs,faceMs; public long memoryStart,memoryEnd;
        }
        private IEnumerator Measure(VenomCampaign game,string output,bool home,int cycles)
        {
            var skin=game.Matter.GetComponent<VenomSurface>();
            // Warm mesh buffers/shaders once before the measured cycles. No save changes.
            game.Personality.Force(COgheAct.Monster);
            yield return new WaitForSecondsRealtime(COghePersonality.MonsterLength+.8f);
            var face=game.Matter.GetComponent<COgheMonsterFace>();
            var samples=new System.Collections.Generic.List<float>(cycles*700);
            var rows=new System.Collections.Generic.List<Vector4>(cycles*700);
            var endFrame=new WaitForEndOfFrame();
            var result=new DeviceTiming { device=SystemInfo.deviceModel,os=SystemInfo.operatingSystem,gpu=SystemInfo.graphicsDeviceName,
                unity=Application.unityVersion,width=Screen.width,height=Screen.height,targetFps=Application.targetFrameRate,cycles=cycles,
                batteryStart=SystemInfo.batteryLevel,batteryStatus=SystemInfo.batteryStatus.ToString(),memoryStart=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() };
            int gc=GC.CollectionCount(0);double skinTotal=0,faceTotal=0;
            COgheMobileMetrics.Enabled=true;
            for(int cycle=0;cycle<cycles;cycle++)
            {
                game.Personality.Force(COgheAct.Monster);
                float start=Time.realtimeSinceStartup,until=start+COghePersonality.MonsterLength+.6f;
                while(Time.realtimeSinceStartup<until)
                {
                    Array.Clear(COgheMobileMetrics.Milliseconds,0,4);
                    yield return endFrame;
                    float ms=Time.unscaledDeltaTime*1000;
                    samples.Add(ms);skinTotal+=COgheMobileMetrics.Milliseconds[0];faceTotal+=face!=null?face.LastBuildMilliseconds:0;
                    rows.Add(new Vector4(cycle,Time.realtimeSinceStartup-start,(float)COgheMobileMetrics.Milliseconds[0],face!=null?(float)face.LastBuildMilliseconds:0));
                    result.maxSkinVertices=Mathf.Max(result.maxSkinVertices,skin.VertexCount);
                    if(ms>33.334f)result.over33++;if(ms>50)result.over50++;
                    yield return null;
                }
            }
            COgheMobileMetrics.Enabled=false;
            result.frames=samples.Count;result.gcCollections=GC.CollectionCount(0)-gc;
            result.memoryEnd=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();result.batteryEnd=SystemInfo.batteryLevel;
            var csv=new System.Text.StringBuilder("cycle,act_seconds,frame_ms,skin_ms,face_ms\n");
            for(int i=0;i<samples.Count;i++)
                csv.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"{0},{1:F4},{2:F4},{3:F4},{4:F4}\n",(int)rows[i].x,rows[i].y,samples[i],rows[i].z,rows[i].w);
            File.WriteAllText(Path.Combine(output,"frames.csv"),csv.ToString());
            result.seconds=samples.Sum()/1000;result.fps=samples.Count/result.seconds;
            samples.Sort();result.p50Ms=samples[(int)(samples.Count*.5f)];result.p95Ms=samples[(int)(samples.Count*.95f)];
            result.p99Ms=samples[(int)(samples.Count*.99f)];result.worstMs=samples[samples.Count-1];
            result.skinMs=skinTotal/samples.Count;result.faceMs=faceTotal/samples.Count;
            File.WriteAllText(Path.Combine(output,"timing.json"),JsonUtility.ToJson(result,true));
            Debug.Log("COGHE MONSTER TIMING "+JsonUtility.ToJson(result));
        }

    }

    /// <summary>Opt-in native-player visual verification; never runs for a normal player or writes campaign saves.</summary>
    public sealed class COgheProductProof : MonoBehaviour
    {
        private VenomCampaign game;
        private COgheProductUI ui;
        private string output;
        private float deadline;
        private bool failed, phone;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-product-proof")<0)return;
            VenomCampaignSave.PersistenceEnabled=false;COgheProductMode.OverrideForTests=true;
            var go=new GameObject("Product UI native proof");DontDestroyOnLoad(go);go.AddComponent<COgheProductProof>();
        }
        private IEnumerator Start()
        {
            deadline=Time.realtimeSinceStartup+380;Application.runInBackground=true;
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-coghe-product-proof-output");
            output=at>=0&&at+1<args.Length?Path.GetFullPath(args[at+1]):Path.Combine(Application.persistentDataPath,"ProductUIProof");Directory.CreateDirectory(output);
            // -coghe-product-proof-phone: keep the window at a phone's shape (19.5:9) for the layout review
            phone=Array.IndexOf(args,"-coghe-product-proof-phone")>=0;
            if(phone)Screen.SetResolution(472,1022,FullScreenMode.Windowed);else Screen.SetResolution(720,1280,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            if(ui==null){Fail("Product UI missing");yield break;}
            yield return new WaitForSecondsRealtime(3);yield return Capture("01-main-menu");
            ui.OpenHome();yield return Capture("02-home-locked");ui.Resume();
            ui.Play();if(COgheIntro.Playing){var intro=FindFirstObjectByType<COgheIntro>();while(!intro.Started)yield return null;intro.Speed=30;intro.Skip();while(COgheIntro.Playing)yield return null;}
            yield return Capture("03-game");ui.ShowPopup(COgheProductPopup.Pause);yield return Capture("04-pause");
            ui.ShowPopup(COgheProductPopup.Help);yield return Capture("05-help");
            ui.ShowPopup(COgheProductPopup.Restart);yield return Capture("06-restart");ui.Resume();
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            float end=Time.realtimeSinceStartup+45;
            while(!game.Owner.Completed&&Time.realtimeSinceStartup<end)yield return null;
            if(!game.Owner.Completed){Fail("Native first puzzle could not be solved");yield break;}
            game.AutoAdvance=false;yield return new WaitForSecondsRealtime(1f);yield return Capture("07-victory");   // the confetti pops as the shot settles
            yield return new WaitForSecondsRealtime(5);
            if(game.Definition.Order!=1||ui.Page!=COgheProductPage.Victory){Fail("Victory did not wait for Next Level");yield break;}
            ui.NextLevel();yield return new WaitForSecondsRealtime(2);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            if(game.Definition.Order!=2){Fail("Next Level did not load level 2");yield break;}
            yield return Capture("08-next-puzzle");
            // Home with the furniture earned up to level 26 (a fixture: no progress is written)
            COgheHomeRoom.UnlockedLevelOverride=32;
            ui.ShowMenu();game.Progress.HomeUnlocked=true;ui.OpenHome();yield return new WaitForSecondsRealtime(5);yield return Capture("09-home");
            ui.ShowPopup(COgheProductPopup.Collection);yield return Capture("10-collection");ui.Resume();
            var room=game.HomeRoom;
            if(room!=null)
            {
                room.ShowGhost(room.Find("TRAMPOLINE"),8);ui.Notify("Trampoline unlocks at level 38");yield return new WaitForSecondsRealtime(1.5f);yield return Capture("16-home-ghost");room.HideGhost();
                game.Personality.PlayNow(room.Find("SWING"));yield return new WaitForSecondsRealtime(2.5f);yield return Capture("17-home-swing");
                game.Personality.PlayNow(room.Find("DUMBBELL"));yield return new WaitForSecondsRealtime(2.2f);yield return Capture("18-home-dumbbell");
            }
            COgheHomeRoom.UnlockedLevelOverride=null;ui.ShowMenu();
            ui.ReplayIntro();yield return new WaitForSecondsRealtime(4);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            yield return Capture("11-intro-replay");var replay=FindFirstObjectByType<COgheIntro>();replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;
            ui.Play();if(COgheIntro.Playing){replay=FindFirstObjectByType<COgheIntro>();while(!replay.Started)yield return null;replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;}
            game.Owner.enabled=false;var half=new bool[32];for(int i=0;i<16;i++)half[i]=true;game.Matter.Partition(0,half);
            yield return Capture("12-fragments-ui-fixture");game.Owner.enabled=true;ui.ShowMenu();
            if(!phone)Screen.SetResolution(720,1612,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(1);
            yield return Capture("13-tall-menu");ui.Play();if(COgheIntro.Playing){replay=FindFirstObjectByType<COgheIntro>();while(!replay.Started)yield return null;replay.Speed=30;replay.Skip();while(COgheIntro.Playing)yield return null;}
            yield return Capture("14-tall-game");ui.ShowPopup(COgheProductPopup.Pause);yield return Capture("15-tall-pause");
            // test-build level picker, then a Boss level's opening tour and warning
            ui.ShowPopup(COgheProductPopup.Levels);yield return Capture("19-test-levels");
            ui.LoadForTest(10);yield return new WaitForSecondsRealtime(3f);
            game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
            if(COgheBossIntro.Current==null){Fail("Boss level opened without its tour");yield break;}
            yield return Capture("20-boss-tour");
            var boss=COgheBossIntro.Current;boss.Seek(boss.TourLength+COgheBossIntro.PullBack+1.1f);yield return Capture("21-boss-warning");
            float bossEnd=Time.realtimeSinceStartup+10;while(COgheBossIntro.Current!=null&&Time.realtimeSinceStartup<bossEnd)yield return null;
            if(COgheBossIntro.Current!=null||game.Owner.Paused){Fail("Boss tour did not hand over");yield break;}
            yield return Capture("22-boss-level");
            // first-level guides: the exit arrow (1), "Hold to rotate" (3), the handle to pull (4)
            int shot=23;
            foreach(int n in new[]{1,3,4})
            {
                ui.LoadForTest(n);yield return new WaitForSecondsRealtime(2.5f);
                game=FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
                if(COgheIntro.Playing){var intro=FindFirstObjectByType<COgheIntro>();while(!intro.Started)yield return null;intro.Speed=30;intro.Skip();while(COgheIntro.Playing)yield return null;}
                yield return new WaitForSecondsRealtime(1.6f);
                if(ui.Guide==null||!ui.Guide.ArrowShown||(n==3&&!ui.Guide.RotateHintShown)){Fail("Level "+n+" guide missing");yield break;}
                yield return Capture((shot++)+"-guide-level"+n);
            }
            // Home: turned round, zoomed in on COghe, feeding (steel balls), the ball game
            COgheHomeRoom.UnlockedLevelOverride=32;
            ui.ShowMenu();game.Progress.HomeUnlocked=true;ui.OpenHome();yield return new WaitForSecondsRealtime(5);
            if(game.HomeRoom==null){Fail("Furnished Home missing");yield break;}
            ui.OrbitHome(-Screen.width*.5f);yield return new WaitForSecondsRealtime(1.5f);yield return Capture("26-home-turned");
            ui.OrbitHome(Screen.width*.5f);ui.ToggleHomeZoom();yield return new WaitForSecondsRealtime(2.5f);yield return Capture("27-home-zoom");ui.ToggleHomeZoom();
            yield return new WaitForSecondsRealtime(1.5f);game.FeedHome();yield return new WaitForSecondsRealtime(2.2f);yield return Capture("28-home-feed");
            if(game.HomeFeedBalls==null||game.HomeFeedBalls.Thrown<3){Fail("Feed threw no balls");yield break;}
            float fed=Time.realtimeSinceStartup+30;while(game.HomeFeeding&&Time.realtimeSinceStartup<fed)yield return null;
            if(game.HomeFeeding){Fail("COghe did not eat the balls");yield break;}
            game.Personality.PlayNow(game.HomeRoom.Find("BALL"));yield return new WaitForSecondsRealtime(3.45f);yield return Capture("29-home-ball-toss");
            ui.ShowPopup(COgheProductPopup.Pause);yield return Capture("47-home-pause");ui.Resume();
            // Style: inks held into COghe, the mix, a hat, things inside, a locked item, a fifth color's question, back home
            COgheHomeRoom.UnlockedLevelOverride=38;COgheStyle.ResetForTests(new COgheStyle{Seed=4});COgheStyle.Current.ApplyTo(game);
            ui.OpenStyle();yield return new WaitForSecondsRealtime(1.5f);
            if(ui.Page!=COgheProductPage.Style){Fail("Style did not open");yield break;}
            yield return Capture("30-style");
            yield return StyleHold("INK_OCEAN",0,.4f,2.6f,"31-style-hold");if(failed)yield break;
            yield return StyleHold("INK_CORAL",.8f,-.2f,1.2f,null);if(failed)yield break;
            yield return new WaitForSecondsRealtime(1.8f);yield return Capture("32-style-mix");
            ui.SetStyleTab(1,0);ui.ChooseStyleItem("HAT_STRAW");yield return new WaitForSecondsRealtime(1.2f);yield return Capture("33-style-hat");
            ui.SetStyleTab(1,1);ui.ChooseStyleItem("FLOAT_FISH");ui.Resume();ui.ChooseStyleItem("FLOAT_STARS");ui.Resume();
            yield return new WaitForSecondsRealtime(1.6f);yield return Capture("34-style-inside");
            ui.SetStyleTab(1,0);ui.ChooseStyleItem("HAT_ASTRO");yield return Capture("35-style-locked");ui.Resume();
            ui.SetStyleTab(0,0);yield return StyleHold("INK_GOLD",-.8f,-.3f,.8f,null);yield return StyleHold("INK_FIREFLY",.3f,-.6f,.8f,null);if(failed)yield break;
            ui.ChooseStyleItem("INK_SAKURA");if(ui.Popup!=COgheProductPopup.StyleReplaceInk){Fail("A fifth color did not ask");yield break;}
            yield return Capture("36-style-replace");ui.Resume();
            ui.LeaveStyle();yield return new WaitForSecondsRealtime(2.5f);yield return Capture("37-home-styled");
            COgheStyle.ResetForTests();
            // the shop (Mrk 02/10): Drops, prices, buying, what a win unlocks, Plus, trying on; test ads show the banner's place
            COgheShop.ResetForTests(new COgheShop.State{Migrated=true});COgheShop.TestsOwnUnlocked=false;COgheShop.Earn(null,150,"proof");
            COgheHomeRoom.UnlockedLevelOverride=32;COgheAds.Provider=new COgheTestAds();
            ui.ShowMenu();yield return new WaitForSecondsRealtime(1.5f);yield return Capture("38-menu-shop");
            ui.OpenHome();yield return new WaitForSecondsRealtime(3);yield return Capture("39-home-shop");
            ui.ShowPopup(COgheProductPopup.Collection);yield return Capture("40-home-items-prices");
            ui.OfferItem("SWING",null);yield return Capture("41-buy");ui.Resume();
            int firstBoss=System.Array.FindIndex(ui.Catalog.Levels,l=>l!=null&&l.Boss)+1;ui.ShowVictoryForTests(ui.Catalog.Levels[firstBoss-1].Id,firstBoss);yield return new WaitForSecondsRealtime(.6f);yield return Capture("42-victory-drops");
            yield return new WaitForSecondsRealtime(3.5f);ui.VictoryStepForTests();yield return Capture("43-unlocks");ui.Resume();
            ui.ShowShopPopup(COgheProductPopup.Plus);yield return Capture("44-plus");ui.Resume();
            ui.ShowMenu();ui.OpenHome();yield return new WaitForSecondsRealtime(2);ui.OpenStyle();yield return new WaitForSecondsRealtime(1.2f);
            ui.SetStyleTab(1,0);ui.ChooseStyleItem("HAT_STRAW");yield return new WaitForSecondsRealtime(.8f);yield return Capture("45-style-tryon");
            ui.LeaveStyle();yield return Capture("46-style-keep-or-take-off");
            COgheAds.Provider=null;COgheShop.ResetForTests();COgheStyle.ResetForTests();
            COgheHomeRoom.UnlockedLevelOverride=null;
            File.WriteAllText(Path.Combine(output,"native-result.json"),"{\"result\":\"passed\",\"nativeFirstLevelSolved\":true,\"advancedByTapToLevel\":2,\"savedProgressWritten\":false,\"captures\":47}");
            Debug.Log("COGHE PRODUCT NATIVE PROOF PASSED");Application.Quit(0);
        }
        /// <summary>Hold a syringe on COghe (x, y: across the body from its middle, in units of 2.5 cm) like a finger would.</summary>
        private IEnumerator StyleHold(string ink,float x,float y,float seconds,string capture)
        {
            ui.ChooseStyleItem(ink);
            var drawn=game.Matter.GetComponent<VenomSurface>().DrawnParticles;var c=Vector3.zero;foreach(var d in drawn)c+=d;c/=drawn.Length;
            var view=game.Owner.View.transform;
            if(!ui.BeginStyleHold(game.Owner.View.WorldToScreenPoint(c+view.right*x*.025f+view.up*y*.025f),false)){Fail("A hold on COghe did not start ("+ink+")");yield break;}
            if(capture!=null){yield return new WaitForSecondsRealtime(seconds*.6f);yield return Capture(capture);yield return new WaitForSecondsRealtime(seconds*.4f);}
            else yield return new WaitForSecondsRealtime(seconds);
            ui.EndStyleHold();
        }
        private IEnumerator Capture(string name)
        {
            yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));
            Debug.Log("PRODUCT CAPTURE "+name);yield return null;
        }
        private void Update(){if(deadline>0&&Time.realtimeSinceStartup>deadline)Fail("Native proof timed out");}
        private void Fail(string reason){if(failed)return;failed=true;Debug.LogError(reason);File.WriteAllText(Path.Combine(output,"native-result.json"),"{\"result\":\"failed\",\"reason\":\""+reason+"\"}");Application.Quit(1);}
    }
}
#endif
