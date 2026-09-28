#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom.ChapterProof
{
    // Author-only acceptance routes. Gameplay, hints and win rules never read this class.
    public sealed class COgheViewExpansionScenario
    {
        private readonly VenomCampaign game;
        private readonly Func<Vector3, IEnumerator> tap;
        private readonly Func<float, Func<bool>, string, IEnumerator> until;
        private readonly Func<float, IEnumerator> orbit;
        public COgheViewExpansionScenario(VenomCampaign game, Func<Vector3, IEnumerator> tap,
            Func<float, Func<bool>, string, IEnumerator> until, Func<float, IEnumerator> orbit = null)
        { this.game = game; this.tap = tap; this.until = until; this.orbit = orbit; }
        public COgheTapRail Task(string label)
        { foreach (var task in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>()) if (task.Label == label) return task; throw new InvalidOperationException("Missing task " + label); }
        private T Component<T>() where T : Component => game.Owner.Apparatus.GetComponentInChildren<T>();
        private IEnumerator Turn(float angle)
        { if (orbit != null) yield return orbit(angle); else { game.CameraRig.Orbit(-angle * 3, 720); yield return null; } }
        public IEnumerator Operate(string label)
        {
            var task = Task(label); int before = task.CompletedJourneys;
            if (task.RequiredRail != null) yield return until(12, () => task.InterlockOpen, "Wait for actual interlock " + label);
            for (int attempt = 0; attempt < 4 && !task.Busy; attempt++)
            {
                yield return tap(task.HandPoint + Vector3.up * .003f);
                if (!task.Busy && attempt < 3) yield return Turn(90);
            }
            if (!task.Busy) throw new InvalidOperationException("Handle rejected " + label + ": " + task.LastFailure);
            yield return until(35, () => task.CompletedJourneys > before, "Complete physical journey " + label);
        }
        public IEnumerator Walk(Vector3 point, string why, float distance = .055f)
        {
            int actor = game.Motion.Selected;
            if (Vector3.Distance(game.Motion.Centre(actor), point + Vector3.up * .018f) < distance) yield break;
            yield return StartWalk(point);
            yield return until(35, () => Vector3.Distance(game.Motion.Centre(actor), point + Vector3.up * .018f) < distance, why);
        }
        public IEnumerator StartWalk(Vector3 point)
        {
            int actor = game.Motion.Selected;
            for (int attempt = 0; attempt < 4; attempt++)
            {
                game.SelectFragment(actor);
                yield return tap(point);
                var command = game.Motion.Get(actor);
                if (command != null && Vector3.Distance(game.Root.TransformPoint(command.Target), point) < .04f) yield break;
                if (attempt < 3) yield return Turn(90);
            }
            throw new InvalidOperationException("No visible surface command at " + point);
        }
        private IEnumerator CrossBridge()
        {
            yield return until(12, () => { foreach(var deck in game.Owner.Apparatus.GetComponentsInChildren<COgheDockedBridgeDeck>())if(!deck.Rail.AtEnd)return false;return true; }, "All bridge decks reach their actual docks");
            yield return Walk(new Vector3(-.04f, -.30f, .20f), "Step onto docked bridge");
            yield return Walk(new Vector3(.21f, -.30f, .20f), "Reach receiving bank");
        }
        private COgheViewTransmission Transmission => Component<COgheViewTransmission>();
        private IEnumerator PowerBoth(string selector)
        {
            COgheViewTransmission drive=null;
            foreach(var candidate in game.Owner.Apparatus.GetComponentsInChildren<COgheViewTransmission>())if(candidate.Power==Task("P").Rail&&candidate.Selector==Task(selector).Rail)drive=candidate;
            if(drive==null)throw new InvalidOperationException("Missing two-output power train");
            yield return Operate("P"); yield return until(10, () => drive.FirstCaught, "First output reaches and retains real catch");
            yield return Operate("P"); yield return Operate(selector); yield return Operate("P");
            yield return until(12, () => drive.SecondCaught, "Second output catches after switching source");
            if (!drive.First.AtEnd) throw new InvalidOperationException("First output lost its catch");
        }
        public IEnumerator Tube(COgheTubeNetwork tube, int destination = 1)
        {
            int actor = game.Motion.Selected; int entry = destination == 1 ? 0 : 1;
            yield return AimTube(tube, entry, actor);
            yield return until(35, () => tube.IsParticleInside(actor), "Enter transfer tube from actual mouth");
            yield return until(40, () => !tube.IsParticleInside(actor), "All tissue clears transfer at receiving platform");
            if (game.Matter.EscapedCount != 0) throw new InvalidOperationException("Transfer was mistaken for final exit");
        }
        public IEnumerator AimTube(COgheTubeNetwork tube, int entry, int actor)
        {
            for(int attempt=0;attempt<4;attempt++)
            {
                game.SelectFragment(actor);
                yield return tap(game.Root.TransformPoint(tube.Nodes[entry].LocalPosition));
                if(tube.IsParticleInside(actor) || tube.IsApproachingEntry(actor,entry)) yield break;
                if(attempt<3)yield return Turn(90);
            }
            throw new InvalidOperationException("Tube mouth not picked: "+tube.DebugEntryState(actor,entry));
        }
        public IEnumerator TapPad(int actor, bool hold)
        {
            var pad=Component<COgheTapPad>(); game.SelectFragment(actor);
            if(Mathf.Abs(game.CameraRig.OrbitYaw)>1) yield return Turn(-game.CameraRig.OrbitYaw);
            for(int attempt=0;attempt<4;attempt++)
            {
                bool owns=pad.Actor>=0 && game.Matter.Groups[pad.Actor]==game.Matter.Groups[actor];
                if(owns==hold)yield break;
                game.SelectFragment(actor); yield return tap(pad.Sensor.transform.position);
                owns=pad.Actor>=0 && game.Matter.Groups[pad.Actor]==game.Matter.Groups[actor];
                if(owns==hold)yield break;
                if(attempt<3)yield return Turn(90);
            }
            throw new InvalidOperationException("Could not pick pad from four views");
        }
        public IEnumerator Branch(int from, int to)
        {
            var tube = Component<COgheTubeNetwork>(); int actor = game.Motion.Selected;
            yield return AimTube(tube, from, actor);
            yield return until(35, () => tube.IsParticleInside(actor), "Enter Y tube");
            yield return until(35, () => tube.LastReachedNode == 1 && tube.AnyWaiting, "Reach Y junction");
            int edge = to == 0 ? 0 : to == 2 ? 1 : 2;
            var e = tube.Edges[edge]; int sample=Mathf.RoundToInt((e.Path.Length-1)*(e.A==1?.25f:.75f));
            Vector3 point=e.Path[sample];
            for(int attempt=0;attempt<4;attempt++)
            {
                yield return tap(game.Root.TransformPoint(point));
                if(tube.LastChosenEdge==edge)break;
                if(attempt<3)yield return Turn(90);
            }
            if(tube.LastChosenEdge!=edge)throw new InvalidOperationException("Visible branch selection did not pick expected bore "+edge);
            yield return until(40, () => !tube.IsParticleInside(actor), "Leave selected Y branch");
        }
        private int Extreme(bool left)
        {
            int actor = 0; float best = left ? float.PositiveInfinity : float.NegativeInfinity;
            for (int i = 0; i < 32; i++)
            { float x = game.Motion.Centre(i).x; if (left ? x < best : x > best) { actor = i; best = x; } }
            return actor;
        }
        public IEnumerator Cut()
        {
            var knife = Component<COgheGuillotine>(); yield return tap(knife.Rail.Body.position);
            yield return until(20, () => game.Matter.TotalFragmentCount > 1, "Knife cuts the actual tissue");
        }
        public IEnumerator Merge(Vector3 point)
        {
            var groups = new HashSet<int>();
            for (int i = 0; i < 32; i++) if (groups.Add(game.Matter.Groups[i])) { game.SelectFragment(i); yield return StartWalk(point); }
            yield return until(40, () => game.Matter.TotalFragmentCount == 1, "All parts reunite inside the chamber");
        }
        private IEnumerator Cooperation(int n)
        {
            if (n >= 23)
            {
                yield return new COgheSimultaneousScenario(game, tap, until, orbit).Solve();
                yield break;
            }
            if (n == 22)
            {
                yield return Cut(); int left = Extreme(true), right = Extreme(false);
                game.SelectFragment(left); yield return tap(new Vector3(-.45f, -.30f, .09f));
                game.SelectFragment(right); yield return Walk(new Vector3(.10f, -.30f, -.12f), "Choose and move the other part");
                yield return Merge(new Vector3(.20f, -.30f, .03f)); yield break;
            }
        }

        public IEnumerator Solve()
        {
            int n = game.Definition.Order;
            if (n >= 22) yield return Cooperation(n);
            else switch (n)
            {
                case 11: case 13: case 21:
                    if (n == 13) yield return Operate("A");
                    yield return Operate(n == 13 ? "B" : "A"); yield return CrossBridge();
                    yield return Walk(new Vector3(.35f, -.30f, .15f), "Reach far return ramp");
                    yield return Walk(new Vector3(.35f, -.42f, -.20f), "Descend toward revealed parking pocket"); break;
                case 12:
                    yield return Operate("A"); yield return Walk(new Vector3(.25f, -.30f, -.35f), "Reach revealed ramp mouth");
                    yield return Walk(new Vector3(.30f, -.40f, -.09f), "Descend revealed wide ramp"); break;
                case 14: yield return Operate("A"); break;
                case 15: yield return PowerBoth("G"); yield return Walk(new Vector3(.28f, -.30f, .16f), "Pass the retained first door"); break;
                case 16:
                    yield return Operate("A"); yield return Operate("B");
                    yield return Walk(new Vector3(-.18f, -.30f, .20f), "First deck");
                    yield return Walk(new Vector3(0, -.30f, .20f), "Fixed island");
                    yield return Walk(new Vector3(.20f, -.30f, .20f), "Second deck"); break;
                case 17: yield return Tube(Component<COgheTubeNetwork>()); break;
                case 18:
                    yield return Operate("A"); yield return Branch(0, 2); yield return Operate("B");
                    yield return Branch(2, 0); yield return Operate("A"); yield return Branch(0, 3); break;
                case 19:
                    yield return Operate("A");
                    yield return Walk(new Vector3(-.30f,-.30f,.13f),"Enter side chamber");yield return Operate("B");
                    yield return until(12,()=>Transmission.FirstCaught,"Bridge catches independently of the room selector");
                    yield return Walk(new Vector3(-.30f,-.30f,-.12f),"Return to vestibule");yield return Operate("A");
                    yield return Walk(new Vector3(.30f,-.30f,-.10f),"Reach main doorway");
                    yield return Walk(new Vector3(.30f,-.30f,.07f),"Cross retained bridge");break;
                case 20:
                    yield return Operate("A"); yield return Operate("B"); yield return Tube(Component<COgheTubeNetwork>());
                    yield return Operate("C"); yield return CrossBridge(); yield return Operate("D"); break;
            }
            yield return until(12, () => game.FinalExitAvailable, "All actual exit mechanisms clear");
            for(int attempt=0; attempt<4 && !game.Owner.Completed; attempt++)
            {
                yield return tap(game.Owner.Outlet.position);
                if(game.Motion.Get(game.Motion.Selected)?.Exit == true) break;
                if(attempt<3) yield return Turn(90);
            }
            if(!game.Owner.Completed && game.Motion.Get(game.Motion.Selected)?.Exit != true) throw new InvalidOperationException("Exit remains visually occluded from four views");
            yield return until(45, () => game.Owner.Completed, "Entire reunited body exits");
        }
    }
}
#endif
