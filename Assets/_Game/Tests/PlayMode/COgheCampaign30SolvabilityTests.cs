using System;
using System.Collections;
using System.Collections.Generic;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GravityBox.Tests
{
    /// <summary>
    /// End-to-end authoring proof for the ten inserted campaign levels.  These
    /// tests use the same movement, grasp, split, merge and exit APIs as touch
    /// input; they never teleport tissue or assign a solved mechanism state.
    /// </summary>
    public sealed class COgheCampaign30SolvabilityTests
    {
        private const float Dt = 1f / 120f;
        private SimulationMode previousMode;
        private VenomCampaign game;

        [UnitySetUp]
        public IEnumerator Before()
        {
            previousMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            VenomCampaignSave.PersistenceEnabled = false;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Time.timeScale = 1;
            Physics.simulationMode = previousMode;
            VenomCampaignSave.PersistenceEnabled = true;
            yield return null;
        }

        private IEnumerator Load(int slot)
        {
            void Loaded(Scene scene, LoadSceneMode mode)
            {
                game = Object.FindFirstObjectByType<VenomCampaign>();
                if (game == null) return;
                game.AutoAdvance = false;
                game.Owner.enabled = false;
                game.Owner.Rotation.enabled = false;
            }
            SceneManager.sceneLoaded += Loaded;
            try { yield return SceneManager.LoadSceneAsync($"COgheOrigin{slot:00}"); }
            finally { SceneManager.sceneLoaded -= Loaded; }
            Assert.NotNull(game, $"Integrated scene {slot:00}");
            yield return null;
        }

        private void Tick()
        {
            game.Owner.Step(Dt);
            game.Owner.Rotation.Step(Dt);
            Physics.Simulate(Dt);
        }

        private string State
        {
            get
            {
                var parts = new List<string>();
                foreach (int anchor in Anchors())
                    parts.Add($"{anchor}:{Count(anchor)} at {game.Root.InverseTransformPoint(game.Motion.Centre(anchor)):F3}");
                var rails = new List<string>();
                foreach (var rail in game.Owner.Apparatus.GetComponentsInChildren<COgheRailSlider>())
                    rails.Add($"{rail.name}={rail.Position:F4}/{rail.Travel:F3} locked={rail.Locked}");
                var active=game.Motion.Get(0);
                if(active!=null) rails.Add($"command={active.Target:F3} cursor={active.Cursor} path={string.Join(" > ",active.Path)}");
                string tube = string.Empty;
                if (game.Tube != null)
                {
                    float min = float.PositiveInfinity, max = float.NegativeInfinity;
                    for (int i = 0; i < 32; i++)
                    {
                        float z = game.Tube.transform.InverseTransformPoint(game.Matter.Bodies[i].position).z;
                        min = Mathf.Min(min, z); max = Mathf.Max(max, z);
                    }
                    tube = $" tube={min:F3}..{max:F3}/{game.Tube.Length:F3}";
                }
                Vector3 remainMin = Vector3.one * float.PositiveInfinity, remainMax = Vector3.one * float.NegativeInfinity;
                for (int i = 0; i < 32; i++) if (!game.Matter.Escaped[i])
                {
                    Vector3 p = game.Owner.Outlet.InverseTransformPoint(game.Matter.Bodies[i].position);
                    remainMin = Vector3.Min(remainMin, p); remainMax = Vector3.Max(remainMax, p);
                }
                int assisted = 0;
                for (int i = 0; i < 32; i++) if (!game.Matter.Escaped[i] && game.ExitAssisting(i)) assisted++;
                return $"failure={game.Failure}; activity={game.Activity}; groups={string.Join(" | ", parts)}; " +
                       $"escaped={game.Matter.EscapedCount}; assisted={assisted}; remain={remainMin:F3}..{remainMax:F3};{tube} {string.Join("; ", rails)}";
            }
        }

        private IEnumerator WaitFor(float seconds, Func<bool> condition, string reason, Action refresh = null)
        {
            int ticks = Mathf.CeilToInt(seconds / Dt);
            for (int i = 0; i < ticks && !condition() && !game.Owner.Lost; i++)
            {
                if (i % 120 == 0) refresh?.Invoke();
                Tick();
                if (i % 240 == 0) yield return null;
            }
            if (!condition()) COgheExpansionIntegrationTests.Capture(game, $"campaign30-{game.Definition.Order:00}-solution-failed");
            Assert.IsTrue(condition(), reason + "; " + State);
        }

        private List<int> Anchors()
        {
            var result = new List<int>();
            var groups = new HashSet<int>();
            for (int i = 0; i < 32; i++)
                if (!game.Matter.Escaped[i] && groups.Add(game.Matter.Groups[i])) result.Add(i);
            return result;
        }

        private int Count(int anchor)
        {
            int count = 0;
            for (int i = 0; i < 32; i++) if (game.Matter.Groups[i] == game.Matter.Groups[anchor]) count++;
            return count;
        }

        private IEnumerator Walk(int anchor, Vector3 point, float seconds = 28f, float tolerance = .038f)
        {
            game.SelectFragment(anchor);
            game.Motion.Move(anchor, point, true);
            yield return WaitFor(seconds,
                () => game.Owner.Completed || Vector3.Distance(game.Motion.Centre(anchor), point) < tolerance,
                "Walk to " + game.Root.InverseTransformPoint(point));
        }

        private IEnumerator Pull(int anchor, COgheRailSlider rail, float direction, Func<bool> finished, float seconds = 28f)
        {
            Vector3 target = rail.Frame.TransformPoint(rail.Start + rail.Axis * (direction > 0 ? rail.Travel + .12f : -.12f));
            float elapsed = 0;
            int strokes = 0;
            while (!finished() && elapsed < seconds && !game.Owner.Lost)
            {
                game.SelectFragment(anchor);
                game.SelectProp(rail.GetComponent<VenomMovableProp>());
                yield return WaitFor(25f, () => game.Attached || finished(), "Approach and grasp " + rail.name);
                if (finished()) { game.ReleaseProp(); break; }
                float before = rail.Position;
                game.SetPropTarget(target);
                for (int i = 0; i < Mathf.CeilToInt(5f / Dt) && !finished() && !game.Owner.Lost; i++)
                {
                    if (i % 90 == 0) game.SetPropTarget(target);
                    Tick();
                    if (i % 240 == 0) yield return null;
                }
                elapsed += 5f;
                strokes++;
                game.ReleaseProp();
                if (Mathf.Abs(rail.Position - before) < .001f && !finished()) break;
            }
            Assert.IsTrue(finished(), $"Operate {rail.name} in recoverable strokes={strokes}; {State}");
        }

        private IEnumerator Leave(int anchor)
        {
            game.SelectFragment(anchor);
            game.Motion.Move(anchor, game.Owner.Outlet.position - game.Owner.Outlet.forward * .024f, false, true);
            yield return WaitFor(42f, () => game.Owner.Completed, "Complete through the real final aperture");
            Assert.AreEqual(32, game.Matter.EscapedCount);
            COgheExpansionIntegrationTests.Capture(game, $"campaign30-{game.Definition.Order:00}-solution-complete");
        }

        private IEnumerator HoldPad(int anchor, COgheTissueSensor pad)
        {
            yield return Walk(anchor, pad.transform.position + pad.transform.up * .018f);
            yield return WaitFor(10f, () => pad.Active, "Hold actual tissue load on " + pad.name);
        }

        private IEnumerator Reunion(Vector3 point)
        {
            foreach (int anchor in Anchors()) game.Motion.Move(anchor, point, true);
            yield return WaitFor(42f, () => game.Matter.TotalFragmentCount == 1, "Reunite all tissue before exit");
        }

        [UnityTest]
        public IEnumerator Inserted05DoesNotLeaveADetachedLobeWhenWaitingAtSlideEntry()
        {
            foreach(int segment in new[]{0,4,12})
            {
                yield return Load(5);
                for(int i=0;i<32;i++)
                    Assert.Greater(game.Root.InverseTransformPoint(game.Matter.Bodies[i].position).y-game.Matter.Profile.ParticleRadius,.116f,
                        "Spawn the entire particle volume above the deck; an embedded tail can resolve underneath it.");
                for(int i=0;i<120;i++)Tick();
                var trough=game.Root.Find("Continuous curved trough collider").GetComponent<COgheSurfacePickProxy>();
                game.CameraRig.Frame(720,1280,0,true);
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(trough.Surfaces[segment].transform.position));
                CollectionAssert.Contains(trough.Surfaces,game.Feedback.CommandSurface);
                float gap=0;
                for(int i=0;i<1800;i++)
                {
                    Tick();
                    if(i%12==0&&game.Root.InverseTransformPoint(game.Motion.Centre(0)).x<.26f)
                    {
                        gap=Mathf.Max(gap,SpatialTissueGap());
                        if(gap>.043f)
                        {
                            for(int p=0;p<32;p++)
                            {
                                game.Motion.Support(p,out var support,out _,out _);
                                Debug.Log($"SLIDE_NODE {p} pos={game.Root.InverseTransformPoint(game.Matter.Bodies[p].position):F4} vel={game.Matter.Bodies[p].linearVelocity:F4} support={support?.name}");
                            }
                            COgheExpansionIntegrationTests.Capture(game,"campaign30-05-lobe-regression");
                            Assert.Fail($"Entry split at {i*Dt:F2}s, gap={gap:F5}; {State}");
                        }
                    }
                    if(i%240==0)yield return null;
                }
                Debug.Log($"SLIDE_ENTRY_WAIT segment={segment} gap={gap:F5}; {State}");
                COgheExpansionIntegrationTests.Capture(game,$"campaign30-05-entry-wait-{segment}");
                Assert.Less(gap,.043f,"A connected particle graph must also remain one spatial body, not two lobes joined by stretched invisible springs; "+State);
                Assert.AreEqual(0,game.Matter.CutCount);
            }
        }

        private float SpatialTissueGap()
        {
            var reached=new bool[32];reached[0]=true;float largest=0;
            for(int count=1;count<32;count++)
            {
                float nearest=float.PositiveInfinity;int next=-1;
                for(int i=0;i<32;i++)if(reached[i])for(int j=0;j<32;j++)if(!reached[j])
                {
                    float distance=Vector3.Distance(game.Matter.Bodies[i].position,game.Matter.Bodies[j].position);
                    if(distance<nearest){nearest=distance;next=j;}
                }
                reached[next]=true;largest=Mathf.Max(largest,nearest);
            }
            return largest;
        }

        [UnityTest]
        public IEnumerator Inserted05SlideHasSolidUndersideAndInwardFacingGuards()
        {
            yield return Load(5);
            var trough=game.Root.Find("Continuous curved trough collider").GetComponent<COgheSurfacePickProxy>();
            var collider=trough.GetComponent<MeshCollider>();
            var patch=trough.Surfaces[9];Vector3 middle=patch.transform.position;
            Assert.IsTrue(collider.Raycast(new Ray(middle-patch.Normal*.10f,patch.Normal),out _, .12f),
                "A falling/rotating body must meet a physical underside, not enter a one-sided sheet.");
            foreach(float side in new[]{-1f,1f})
                Assert.IsTrue(collider.Raycast(new Ray(middle+patch.Normal*.014f,game.Root.forward*side),out _, .13f),
                    "Both guards must collide from INSIDE the trough.");
        }

        [UnityTest]
        public IEnumerator Inserted05SolidBedResistsTurningAndInversion()
        {
            yield return Load(5);for(int i=0;i<120;i++)Tick();
            var trough=game.Root.Find("Continuous curved trough collider").GetComponent<COgheSurfacePickProxy>();
            var vertices=trough.GetComponent<MeshCollider>().sharedMesh.vertices;
            game.CameraRig.Frame(720,1280,0,true);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(trough.Surfaces[4].transform.position));
            yield return WaitFor(6,()=>game.Root.InverseTransformPoint(game.Motion.Centre(0)).x>-.245f,"Enter the slide before rotation stress");
            int bedSamples=0;
            foreach(var angles in new[]{new Vector3(40,0,-35),new Vector3(-45,0,30),new Vector3(0,0,180),new Vector3(0,0,0)})
            {
                game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(angles));
                for(int tick=0;tick<480;tick++)
                {
                    Tick();
                    for(int node=0;node<32;node++)
                    {
                        if(game.Matter.Escaped[node])continue;
                        Vector3 p=game.Root.InverseTransformPoint(game.Matter.Bodies[node].position);
                        if(Mathf.Abs(p.z)>.10f)continue;
                        for(int s=0;s<trough.Surfaces.Length;s++)
                        {
                            Vector3 a=vertices[s*4],b=vertices[(s+1)*4];
                            if(p.x<=a.x||p.x>=b.x)continue;
                            Vector3 tangent=(b-a).normalized,normal=new Vector3(-tangent.y,tangent.x,0);
                            float height=Vector3.Dot(p-a,normal);
                            if(height>.005f&&height<.055f)bedSamples++;
                            Assert.IsFalse(height<-.003f&&height>-.021f,
                                $"Particle {node} embedded in the solid 24 mm slide at tilt {angles}, tick {tick}, height {height:F5}.");
                            break;
                        }
                    }
                    if(tick%120==0)yield return null;
                }
            }
            Assert.Greater(bedSamples,30,"Stress must include real tissue above the slide, not only an empty chamber.");
            Assert.IsFalse(game.Owner.Lost,State);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            Assert.AreEqual(0,game.Matter.CutCount);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-05-after-rotation-stress");
        }

        [UnityTest]
        public IEnumerator Inserted05StartsUphillAndRetryRestoresTheOpeningPose()
        {
            yield return Load(5);
            Quaternion opening=Quaternion.AngleAxis(45,Quaternion.Euler(game.Definition.CameraEuler)*Vector3.forward);
            Assert.Less(Quaternion.Angle(game.Root.rotation,opening),.1f);
            var deck=Array.Find(game.Surfaces,p=>p.name=="Grippy start cradle");
            var tray=Array.Find(game.Surfaces,p=>p.name=="Grippy receiving cradle");
            Assert.Greater(tray.transform.position.y,deck.transform.position.y+.2f,
                "The exit tray must visibly start uphill, not only appear tilted through a camera change.");
            for(int i=0;i<1200;i++)Tick();
            Assert.Less(game.Root.InverseTransformPoint(game.Motion.Centre(0)).x,-.29f,
                "Waiting for instructions must keep the creature on its grippy starting deck.");
            var trough=game.Root.Find("Continuous curved trough collider").GetComponent<COgheSurfacePickProxy>();
            game.CameraRig.Frame(720,1280,0,true);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(trough.Surfaces[4].transform.position));
            CollectionAssert.Contains(trough.Surfaces,game.Feedback.CommandSurface);
            for(int i=0;i<1440;i++)Tick();
            Assert.Less(game.Root.InverseTransformPoint(game.Motion.Centre(0)).x,.25f,
                "A tap without rotating cannot slide uphill into the receiving tray.");
            Assert.AreEqual(0,game.Matter.EscapedCount);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-05-uphill-opening");
            game.Owner.Rotation.SetTargetOrientation(Quaternion.identity);
            yield return WaitFor(5,()=>Quaternion.Angle(game.Root.rotation,Quaternion.identity)<1,"Rotate back to the original downhill pose");
            game.ResetLevel();Physics.SyncTransforms();
            Assert.Less(Quaternion.Angle(game.Owner.Rotation.Orientation,opening),.1f,"Retry restores the tilted physics pose, not the solved orientation.");
            for(int i=0;i<360;i++)Tick();
            Assert.Less(Quaternion.Angle(game.Root.rotation,opening),.1f,"The displayed pivot follows the reset physics pose.");
            Assert.Less(game.Root.InverseTransformPoint(game.Motion.Centre(0)).x,-.29f);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.AreEqual(0,game.Matter.CutCount);
        }

        [UnityTest]
        public IEnumerator Inserted05SlidesThroughTheCurvedTroughAndExits()
        {
            foreach(float tilt in new[]{0f,12f,24f,36f})
                yield return LearningSlideRoute(tilt,(tilt-24f)/300f);
        }

        private IEnumerator LearningSlideRoute(float tilt,float lateral)
        {
            yield return Load(5);
            for(int i=0;i<120;i++)Tick();
            COgheExpansionIntegrationTests.Capture(game,"campaign30-05-slide-start");
            var trough=game.Root.Find("Continuous curved trough collider").GetComponent<COgheSurfacePickProxy>();
            Vector3 entry=trough.Surfaces[4].transform.position+game.Root.forward*lateral;
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(entry));
            CollectionAssert.Contains(trough.Surfaces,game.Feedback.CommandSurface,
                "A screen tap on the visible trough must select its actual surface, including the guards.");
            bool slideContact=false;
            yield return WaitFor(18f,()=>
            {
                for(int i=0;i<32;i++)if(game.Motion.Support(i,out var shape,out _,out _))
                    foreach(var patch in trough.Surfaces)if(shape==patch.Shape)slideContact=true;
                return slideContact;
            },"Leave the high deck and physically contact the slippery bed");
            game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(0,0,-tilt));
            yield return WaitFor(18f,()=>
            {
                Vector3 p=game.Root.InverseTransformPoint(game.Motion.Centre(0));
                return p.x>.28f&&p.y>-.205f&&p.y<-.10f&&Mathf.Abs(p.z)<.13f;
            },$"At {tilt} degrees, gravity must deliver tissue to the elevated tray, not merely the floor beneath it");
            COgheExpansionIntegrationTests.Capture(game,$"campaign30-05-slide-caught-{tilt:00}");
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"The whole body must arrive together.");
            Assert.IsFalse(game.Owner.Completed,"Arriving in the tray does not itself win.");
            game.Owner.Rotation.SetTargetOrientation(Quaternion.identity);
            yield return WaitFor(4f,()=>game.Owner.Completed||Quaternion.Angle(game.Root.rotation,Quaternion.identity)<2f,"Return the box to a readable pose");
            if(!game.Owner.Completed)
            {
                game.CameraRig.Frame(720,1280,0,true);
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
                Assert.IsTrue(game.Motion.Get(0)?.Exit??false,"The visible exit must accept a real screen tap from the tray.");
            }
            yield return WaitFor(30f,()=>game.Owner.Completed,"Exit through the visible aperture after the catch");
            Assert.AreEqual(32,game.Matter.EscapedCount);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-05-slide-complete");
        }

        [UnityTest]
        public IEnumerator Inserted05WaitsOnItsDeckAndCanReturnAfterAWrongTilt()
        {
            yield return Load(5);
            for(int i=0;i<1200;i++)Tick();
            Vector3 rest=game.Root.InverseTransformPoint(game.Motion.Centre(0));
            Assert.Less(rest.x,-.29f);Assert.Greater(rest.y,.11f);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount,"Waiting for instructions cannot tear the creature apart.");
            game.Owner.Rotation.SetTargetOrientation(Quaternion.Euler(0,0,30));
            yield return WaitFor(4f,()=>Quaternion.Angle(game.Root.rotation,Quaternion.Euler(0,0,30))<2,"Tilt the wrong way");
            for(int i=0;i<360;i++)Tick();
            game.Owner.Rotation.SetTargetOrientation(Quaternion.identity);
            yield return WaitFor(4f,()=>Quaternion.Angle(game.Root.rotation,Quaternion.identity)<2,"Restore after a wrong tilt");
            var deck=Array.Find(game.Surfaces,p=>p.name=="Grippy start cradle");
            game.CameraRig.Frame(720,1280,0,true);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(deck.transform.position));
            Assert.AreSame(deck,game.Feedback.CommandSurface,"The visible deck is directly commandable.");
            yield return WaitFor(24f,()=>Vector3.Distance(game.Motion.Centre(0),deck.transform.position+game.Root.up*.018f)<.05f,"Recover onto the start deck");
            Assert.IsFalse(game.Owner.Lost);Assert.AreEqual(0,game.Matter.EscapedCount);
            Assert.AreEqual(1,game.Matter.TotalFragmentCount);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-05-slide-recovered");
            game.CameraRig.ToggleFollow();game.CameraRig.Frame(720,1612,1,true);
            var visual=game.Root.Find("Learning slide presentation");
            Assert.NotNull(visual);Assert.IsEmpty(visual.GetComponentsInChildren<Collider>());
            foreach(var renderer in visual.GetComponentsInChildren<MeshRenderer>())Assert.IsTrue(renderer.enabled,"Functional slide art must remain visible while following the creature.");
        }

        [UnityTest]
        public IEnumerator Inserted11DropsOntoTheBroadRingFlowsThroughTheTubeAndExits()
        {
            yield return Load(11);
            for(int i=0;i<120;i++)Tick();
            game.CameraRig.Frame(720,1280,0,true);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-11-rebuilt-start");
            var departure=Array.Find(game.Surfaces,p=>p.name=="Short slippery departure");
            Vector3 point=game.Root.TransformPoint(new Vector3(-.173f,.1133f,-.08f));
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(point));
            Assert.AreSame(departure,game.Feedback.CommandSurface,"The visible purple edge must receive the touch, not the roof, glass or portal behind it.");
            float downward=0;
            yield return WaitFor(28f, () => {downward=Mathf.Max(downward,-game.Matter.Bodies[0].linearVelocity.y);return game.InTube;},
                "Touching the purple edge must produce a short physical fall and real ring contact");
            Assert.Greater(downward,.25f,"The authored route must fall, not certify a direct crawl to the ring instead.");
            Assert.AreEqual(0,game.Matter.EscapedCount,"Transfer is not the final exit.");
            COgheExpansionIntegrationTests.Capture(game,"campaign30-11-real-catch");
            yield return WaitFor(18f, () => !game.InTube && game.Root.InverseTransformPoint(game.Motion.Centre(0)).x > .15f,
                "All tissue reaches the second room");
            for(int i=0;i<120;i++)Tick();
            COgheExpansionIntegrationTests.Capture(game,"campaign30-11-reformed");
            game.CameraRig.Frame(720,1280,0,true);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            yield return WaitFor(28f,()=>game.Owner.Completed,"The visible final aperture must accept a tap and let the whole body escape");
            Assert.AreEqual(32,game.Matter.EscapedCount);
        }

        [UnityTest]
        public IEnumerator Inserted11AcceptsWideDepartureTapsAndCanRetryAfterAMiss()
        {
            foreach(float offset in new[]{-.035f,.035f})
            {
                yield return Load(11);for(int i=0;i<120;i++)Tick();
                game.CameraRig.SelectZone(0);game.CameraRig.Frame(720,1280,0,true);
                var edge=Array.Find(game.Surfaces,p=>p.name=="Short slippery departure");
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Root.TransformPoint(new Vector3(-.173f,.1133f,-.08f+offset))));
                Assert.AreSame(edge,game.Feedback.CommandSurface,"Offset touch must hit the visible edge: "+offset);
                yield return WaitFor(24f,()=>game.InTube,"Catch without a timed second tap, lateral offset "+offset);
                yield return WaitFor(18f,()=>!game.InTube&&game.Motion.Centre(0).x>.15f,"Whole body transfers after offset "+offset);
                Assert.AreEqual(1,game.Matter.TotalFragmentCount);Assert.AreEqual(0,game.Matter.EscapedCount);
                game.CameraRig.SelectZone(1);game.CameraRig.Frame(720,1280,0,true);
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
                yield return WaitFor(24f,()=>game.Owner.Completed,"Reach the visible exit from the receiving deck");
            }
            yield return Load(11);for(int i=0;i<120;i++)Tick();
            // A wrong floor command abandons the departure. Reach the floor with
            // normal navigation, then climb back using actual visible surfaces.
            game.CameraRig.SelectZone(0);game.CameraRig.Frame(720,1280,0,true);
            Vector3 floor=game.Root.TransformPoint(new Vector3(-.34f,-.22f,-.20f));
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(floor));
            Assert.Less(game.Feedback.CommandPoint.y,-.20f,"The floor command must actually pick the floor, not a foreground deck.");
            yield return WaitFor(28f,()=>game.Motion.Centre(0).y<-.15f,"A missed/abandoned approach can reach the safe floor");
            COgheExpansionIntegrationTests.Capture(game,"campaign30-11-recovery-floor");
            var deck=Array.Find(game.Surfaces,p=>p.name=="Climb deck");
            game.CameraRig.Frame(720,1280,0,true);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(deck.transform.position));
            Assert.AreSame(deck,game.Feedback.CommandSurface,"The recovery deck is visible and touchable");
            yield return WaitFor(28f,()=>game.Motion.Centre(0).y>.145f,"Climb back onto the deck without reset");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Root.TransformPoint(new Vector3(-.173f,.1133f,-.08f))));
            yield return WaitFor(24f,()=>game.InTube,"Recovered body can perform the designed fall again");
            game.ResetLevel();for(int i=0;i<120;i++)Tick();
            Assert.IsFalse(game.InTube);Assert.IsTrue(game.Tube.Entrance.Shape.enabled);
            Assert.AreEqual(32,game.Matter.Bodies.Length);Assert.AreEqual(0,game.Matter.EscapedCount);
            Assert.Less(Vector3.Distance(game.Motion.Centre(0),game.Owner.Spawn.position),.06f,"Retry during flow restores the body on the start deck");
        }

        [UnityTest]
        public IEnumerator Inserted09MeshesGOpensTheRackAndExits()
        {
            yield return Load(9);
            var train = Object.FindFirstObjectByType<COgheGearTrain>();
            var g = Array.Find(game.Props, p => p.name == "G sliding gear").GetComponent<COgheRailSlider>();
            yield return Pull(0, g, 1, () => train.ExitUnlocked);
            yield return Leave(0);
        }

        private IEnumerator ScreenPull(COgheRailSlider rail,Func<bool> complete,bool wrongFirst=false,bool release=true,float direction=1)
        {
            var prop=rail.GetComponent<VenomMovableProp>();
            game.CameraRig.Frame(720,1280,0,true);
            var stem=prop.ManipulationGrip.Find("Bridge handle stem");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(stem!=null?stem.position:prop.ManipulationGrip.position));
            yield return WaitFor(28f,()=>game.Attached,"Screen tap reaches "+rail.name);
            if(wrongFirst)
            {
                var wrong=rail.Frame.TransformPoint(rail.Start-rail.Axis*.12f);
                game.TouchPoint(game.Owner.View.WorldToScreenPoint(wrong));
                for(int i=0;i<180;i++){Tick();if(i%120==0)yield return null;}
                Assert.Less(rail.Position,.004f,"The physical end stop prevents pushing past the closed end");
                Assert.IsTrue(game.Attached,"Reverse direction without having to select the cover again");
            }
            Vector3 end=rail.Frame.TransformPoint(rail.Start+rail.Axis*(direction>0?rail.Travel+.10f:-.10f));
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(end));
            yield return WaitFor(12f,complete,"One continuous grasp operates "+rail.name,()=>game.TouchPoint(game.Owner.View.WorldToScreenPoint(end)));
            if(!game.Attached)COgheExpansionIntegrationTests.Capture(game,"campaign30-"+game.Definition.Order+"-stroke-released");
            Assert.IsTrue(game.Attached,"Must not need to reselect the handle during a stroke; "+State);
            if(release)game.ReleaseProp();
        }

        [UnityTest] public IEnumerator Existing24DirectBearingTapsSolveWithoutFloorWaypoints()
        {
            yield return Load(24);
            var a=Array.Find(game.Props,p=>p.name=="A sliding bearing").GetComponent<COgheRailSlider>();
            var b=Array.Find(game.Props,p=>p.name=="B sliding bearing").GetComponent<COgheRailSlider>();
            var train=Object.FindFirstObjectByType<COgheGearTrain>();
            yield return ScreenPull(a,()=>a.AtEnd);
            yield return ScreenPull(b,()=>b.Latched&&b.Position<=b.CatchTolerance,direction:-1);
            yield return WaitFor(15,()=>train.ExitUnlocked,"Both directly selected bearings open the real shutter");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            yield return WaitFor(35,()=>game.Owner.Completed,"Exit after the direct A to B interaction");
            Assert.AreEqual(32,game.Matter.EscapedCount);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-24-direct-solved");
        }

        [UnityTest] public IEnumerator Existing24ReverseOrderAndIdleReleaseSolveFromScreenTaps()
        {
            yield return Load(24);
            var a=Array.Find(game.Props,p=>p.name=="A sliding bearing").GetComponent<COgheRailSlider>();
            var b=Array.Find(game.Props,p=>p.name=="B sliding bearing").GetComponent<COgheRailSlider>();
            var train=Object.FindFirstObjectByType<COgheGearTrain>();
            yield return ScreenPull(b,()=>b.Latched&&b.Position<=b.CatchTolerance,release:false,direction:-1);
            Assert.IsFalse(train.ExitUnlocked,"One bearing alone cannot unlock the exit");
            yield return WaitFor(4,()=>!game.Attached,"Idle releases B without an extra control");
            yield return ScreenPull(a,()=>a.AtEnd,release:false);
            yield return WaitFor(4,()=>!game.Attached,"Idle releases A without an extra control");
            yield return WaitFor(15,()=>train.ExitUnlocked,"Reverse order drives the same real shutter");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            yield return WaitFor(35,()=>game.Owner.Completed,"Exit after B then A and natural release");
            Assert.AreEqual(32,game.Matter.EscapedCount);
        }

        [UnityTest]
        public IEnumerator Inserted14PullsTheCoverIntoItsPocketAndExits()
        {
            yield return Load(14);
            var mechanism=Object.FindFirstObjectByType<COgheExitRailLock>();
            COgheExpansionIntegrationTests.Capture(game,"campaign30-14-reviewed-start");
            yield return ScreenPull(mechanism.Rail,()=>mechanism.ExitUnlocked,true);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-14-reviewed-open");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            yield return WaitFor(35f,()=>game.Owner.Completed,"Screen tap exits after uncovering the hole");
        }

        [UnityTest]
        public IEnumerator Inserted16OpensAThenPullsBAndExits()
        {
            yield return Load(16);
            var sequence=Object.FindFirstObjectByType<COgheSequentialWinch>();
            COgheExpansionIntegrationTests.Capture(game,"campaign30-16-reviewed-start");
            var handle=sequence.Handle.GetComponent<VenomMovableProp>();
            game.CameraRig.Frame(720,1280,0,true);
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(handle.ManipulationGrip.position));
            var approach=typeof(VenomCampaign).GetField("approachProp",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Assert.AreNotSame(handle,approach.GetValue(game),"A and its closed alcove must prevent selecting B through the cover");
            game.ReleaseProp();
            yield return ScreenPull(sequence.Access,()=>sequence.AccessOpen,false,false);
            yield return WaitFor(4f,()=>!game.Attached,"Idle releases the cover after three seconds");
            Assert.IsTrue(sequence.AccessOpen,"The access latch stays open after letting go");
            COgheExpansionIntegrationTests.Capture(game,"campaign30-16-reviewed-B-exposed");
            yield return ScreenPull(sequence.Handle,()=>sequence.Complete);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-16-reviewed-open");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            yield return WaitFor(35f,()=>game.Owner.Completed,"Screen tap exits after the winch raises the shutter");
        }

        [UnityTest]
        public IEnumerator Inserted18DocksTheSingleBridgeWalksAcrossAndExits()
        {
            yield return Load(18);
            var deck=Object.FindFirstObjectByType<COgheDockedBridgeDeck>();
            foreach(var body in game.Matter.Bodies)Assert.Greater(body.position.y,-.16f,"All tissue must spawn above the solid bank");
            Assert.IsNull(Object.FindFirstObjectByType<COgheExitRailLock>());
            var plate=Array.Find(game.Surfaces,p=>p.name=="Moving aperture rear");
            Assert.NotNull(plate);
            Vector3 from=game.Root.TransformPoint(new Vector3(.09f,-.10f,.08f));
            Assert.IsTrue(plate.Shape.Raycast(new Ray(from,game.Root.right),out _, .09f),"The solid plate initially blocks the real partition aperture");
            COgheExpansionIntegrationTests.Capture(game,"campaign30-18-reviewed-start");
            yield return ScreenPull(deck.Rail,()=>deck.Rail.AtEnd);
            Assert.IsTrue(plate.gameObject.activeInHierarchy&&plate.Shape.enabled,"Docking must preserve the aperture plate");
            Assert.IsFalse(plate.Shape.Raycast(new Ray(from,game.Root.right),out _, .09f),"The actual opening aligns instead of deleting the plate");
            COgheExpansionIntegrationTests.Capture(game,"campaign30-18-reviewed-docked");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(deck.DockedTop.transform.position));
            Assert.IsFalse(game.Attached,"A deck tap means crawl, not manipulate");
            yield return WaitFor(24f,()=>Vector3.Distance(game.Motion.Centre(0),deck.DockedTop.transform.position+Vector3.up*.020f)<.045f,"Screen tap walks onto the deck");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            yield return WaitFor(35f,()=>game.Owner.Completed,"Screen tap crosses the aligned bore and final exit");
            Assert.AreEqual(32,game.Matter.EscapedCount);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-18-solution-complete");
            game.ResetLevel();for(int i=0;i<120;i++)Tick();
            Assert.IsTrue(plate.gameObject.activeInHierarchy&&plate.Shape.enabled);
            Assert.IsTrue(plate.Shape.Raycast(new Ray(from,game.Root.right),out _, .09f),"Retry restores the real blocking plate");
        }

        [UnityTest]
        public IEnumerator Inserted21UsesOneGearAtAThenBAndExits()
        {
            yield return Load(21);
            var mechanism = Object.FindFirstObjectByType<COgheDualDockTransmission>();
            COgheExpansionIntegrationTests.Capture(game,"campaign30-21-gears-start");
            yield return Pull(0, mechanism.Carriage, -1, () => mechanism.StageAComplete, 34f);
            Assert.IsTrue(mechanism.AtA);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-21-gears-A");
            float gearTop=game.Root.InverseTransformPoint(mechanism.CarriageWheel.position).y+mechanism.PitchRadius*(1+2f/mechanism.ToothCount);
            float gateBottom=game.Root.InverseTransformPoint(mechanism.AccessGate.Body.position).y-.07f;
            Assert.Greater(gateBottom-gearTop,.010f,"The raised shutter must clear the tooth tips, not just the carriage");
            yield return Pull(0, mechanism.Carriage, 1, () => mechanism.StageBComplete, 34f);
            Assert.IsTrue(mechanism.AtB);
            COgheExpansionIntegrationTests.Capture(game,"campaign30-21-gears-B");
            yield return Leave(0);
            Assert.AreEqual(32,game.Matter.EscapedCount);
        }

        [UnityTest]
        public IEnumerator Inserted23ParksThePhysicalCrateBeforeDrivingGAndExits()
        {
            yield return Load(23);
            var crate = Array.Find(game.Props, p => p.name == "Amber blocking crate").GetComponent<COgheRailSlider>();
            var g = Array.Find(game.Props, p => p.name == "G rail carriage").GetComponent<COgheRailSlider>();
            var train = Object.FindFirstObjectByType<COgheGearTrain>();
            yield return Pull(0, crate, 1, () => crate.AtEnd);
            Assert.Less(game.Root.InverseTransformPoint(crate.Body.position).z, -.075f, "The crate must visibly sit inside the parking bay");
            yield return Pull(0, g, 1, () => train.ExitUnlocked, 34f);
            yield return Leave(0);
        }

        [UnityTest]
        public IEnumerator Inserted26CutsHoldsAWithOnePartPullsBWithTheOtherReunitesAndExits()
        {
            yield return Load(26);
            var knife = Object.FindFirstObjectByType<COgheGuillotine>();
            var winch = Object.FindFirstObjectByType<COgheCooperativeWinch>();
            game.Motion.Move(0, knife.Sensor.position, true);
            yield return WaitFor(24f, () => game.Matter.TotalFragmentCount > 1, "The authored guillotine makes two controllable parts");
            var parts = Anchors();
            parts.Sort((a, b) => game.Motion.Centre(a).x.CompareTo(game.Motion.Centre(b).x));
            int left = parts[0], right = parts[parts.Count - 1];
            yield return HoldPad(left, winch.Input);
            yield return Pull(right, winch.Handle, 1, () => winch.Complete, 32f);
            yield return Reunion(game.Root.TransformPoint(new Vector3(.29f, -.274f, .02f)));
            yield return Leave(Anchors()[0]);
        }

        [UnityTest]
        public IEnumerator Inserted10BossConnectsGRevealsBOpensTheFinalShutterAndExits()
        {
            yield return Load(10);
            var sequence = Object.FindFirstObjectByType<COgheSequentialWinch>();
            var g = Array.Find(game.Props, p => p.name == "G factory carriage").GetComponent<COgheRailSlider>();
            yield return Pull(0, g, 1, () => sequence.AccessOpen, 34f);
            yield return Pull(0, sequence.Handle, 1, () => sequence.Complete, 34f);
            yield return Leave(0);
        }
    }
}
