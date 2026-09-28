#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using UnityEngine;

namespace GravityBox.Venom.ChapterProof
{
    // Author route only. All movement, cuts, tasks and exit requests go through visible screen input.
    public sealed class COgheSimultaneousScenario
    {
        private readonly VenomCampaign game;
        private readonly Func<Vector3, IEnumerator> tap;
        private readonly Func<float, Func<bool>, string, IEnumerator> until;
        private readonly Func<float, IEnumerator> orbit;
        public readonly COgheViewExpansionScenario Route;
        public int Holder { get; private set; }
        public int Worker { get; private set; }
        public COgheSimultaneousScenario(VenomCampaign game, Func<Vector3, IEnumerator> tap,
            Func<float, Func<bool>, string, IEnumerator> until, Func<float, IEnumerator> orbit = null)
        { this.game = game; this.tap = tap; this.until = until; this.orbit = orbit; Route = new COgheViewExpansionScenario(game, tap, until, orbit); }
        private IEnumerator Turn()
        { if (orbit != null) yield return orbit(90); else { game.CameraRig.Orbit(-270, 720); yield return null; } }
        public IEnumerator Start(string label, int actor)
        {
            var task = Route.Task(label);
            for (int attempt = 0; attempt < 4 && !task.Busy; attempt++)
            {
                game.SelectFragment(actor); yield return tap(task.HandPoint);
                if (!task.Busy && attempt < 3) yield return Turn();
            }
            if (!task.Busy) throw new InvalidOperationException("Held input not picked " + label + ": " + task.LastFailure);
        }
        public IEnumerator Hold(string label, int actor)
        {
            yield return Start(label, actor);
            yield return until(40, () => Route.Task(label).Holding, "Approach and continuously brace " + label);
        }
        public IEnumerator Split()
        {
            yield return Route.Cut();
            float min = float.PositiveInfinity, max = float.NegativeInfinity;
            for (int i = 0; i < 32; i++)
            {
                float x = game.Motion.Centre(i).x;
                if (x < min) { Holder = i; min = x; } if (x > max) { Worker = i; max = x; }
            }
            if (game.Matter.Groups[Holder] == game.Matter.Groups[Worker]) throw new InvalidOperationException("Cut did not leave independent actors");
            game.SelectFragment(Worker);
            bool banks = game.Definition.Order == 24 || game.Definition.Order == 25 || game.Definition.Order >= 29;
            yield return Route.Walk(new Vector3(banks || game.Definition.Order == 26 ? -.20f : .06f, -.30f, -.33f), "Worker clears cutting blade before holder leaves");
        }
        private COgheCooperativeDrive Find(COgheCooperativeDrive.MechanismKind kind)
        {
            foreach (var drive in game.Owner.Apparatus.GetComponentsInChildren<COgheCooperativeDrive>()) if (drive.Kind == kind) return drive;
            throw new InvalidOperationException("Missing cooperative drive " + kind);
        }
        public IEnumerator Solve()
        {
            int n = game.Definition.Order;
            yield return Split();
            yield return Hold("A", Holder);
            game.SelectFragment(Worker);
            if (n == 25 || n >= 29) yield return Route.Tube(game.Owner.Apparatus.GetComponentInChildren<COgheTubeNetwork>());
            if (n == 24 || n == 25 || n >= 29)
            {
                var bridge = Find(COgheCooperativeDrive.MechanismKind.BrakeBridge);
                yield return Route.Operate("B");
                yield return until(15, () => bridge.Caught, "Bridge catches only after both roles overlap");
                if (n >= 29)
                {
                    // Move the worker away before bringing the holder over, preserving two separate roles.
                    yield return Hold("D", Worker);
                    game.SelectFragment(Holder);
                    if (n == 29)
                    {
                        yield return Route.Walk(new Vector3(-.04f, -.30f, .20f), "Holder enters retained return bridge");
                        yield return Route.Walk(new Vector3(.21f, -.30f, .20f), "Holder reaches other bank");
                    }
                    else
                    {
                        yield return Route.Walk(new Vector3(-.28f, -.30f, -.12f), "Holder reaches ramp foot");
                        yield return Route.Walk(new Vector3(-.24f, -.16f, .25f), "Holder reaches upper valve landing");
                    }
                    yield return Hold("C", Holder);
                    yield return until(20, () => game.FinalExitAvailable, "Second pair completes and catches");
                    if (n == 30)
                    {
                        game.SelectFragment(Worker); yield return Route.Walk(new Vector3(-.04f, -.30f, .04f), "Worker returns on visible lower bridge");
                        yield return Route.Walk(new Vector3(-.28f, -.30f, -.12f), "Worker reaches clear ramp foot");
                    }
                    yield return Route.Merge(n == 30 ? new Vector3(-.24f, -.16f, .25f) : new Vector3(.35f, -.30f, .02f));
                }
                else
                {
                    if (n == 25)
                    {
                        game.SelectFragment(Worker);
                        yield return Route.Walk(new Vector3(-.04f, -.30f, .20f), "Worker returns over caught bridge");
                    }
                    yield return Route.Merge(new Vector3(-.35f, -.30f, .02f));
                    yield return Route.Walk(new Vector3(-.04f, -.30f, .20f), "Whole body enters caught bridge");
                    yield return Route.Walk(new Vector3(.21f, -.30f, .20f), "Whole body reaches exit bank");
                }
            }
            else
            {
                yield return Start("B", Worker);
                if (n == 28)
                {
                    COgheCooperativeDrive first = null;
                    foreach (var drive in game.Owner.Apparatus.GetComponentsInChildren<COgheCooperativeDrive>()) if (!drive.FinalGate) first = drive;
                    yield return until(25, () => first.Caught, "Return output catches first");
                    game.SelectFragment(Worker); yield return Route.Operate("T");
                    yield return Hold("A", Holder); yield return Start("B", Worker);
                }
                yield return until(40, () => game.FinalExitAvailable, "Two active spring inputs catch shared output");
                yield return Route.Merge(new Vector3(.05f, -.30f, -.10f));
            }
            if (n == 26 || n == 28 || n == 30)
            {
                if (n != 30)
                {
                    yield return Route.Walk(new Vector3(-.28f, -.30f, -.12f), "Reach ramp foot");
                    yield return Route.Walk(new Vector3(-.24f, -.16f, .25f), "Climb fixed approach");
                }
                yield return Route.Walk(new Vector3(0, -.16f, .27f), "Cross raised paired-pressure deck");
                yield return Route.Walk(new Vector3(.30f, -.16f, .28f), "Reach upper wall exit landing");
            }
        }
    }
}
#endif
