using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    // Glass stripe audit (Mrk 18/10/2026: "rà soát 65 màn"): measures, changes nothing. A 9 cm stripe covers the glass above
    // each fixed ledge, exactly as wide as its edge; moving tops (crates, rails, decks) have only the floor's stripe. COghe
    // is ~7.4 cm wide and a node grips glass up to 3.4 cm away, so from a ledge's end or a high moving top it may reach bare
    // glass. For every spot where COghe can stand against the glass, it is put there and told to climb onto the bare glass
    // just above or beside the stripe; where it gets there, it is then sent to the exit. Results: Artifacts/GlassAudit.
    public sealed partial class COgheSpatialCampaignTests
    {
        [UnityTest, Explicit, Timeout(7200000)] public IEnumerator GlassStripeReachAudit()
        {
            int from = int.Parse(Environment.GetEnvironmentVariable("AUDIT_FROM") ?? "1"), to = int.Parse(Environment.GetEnvironmentVariable("AUDIT_TO") ?? "65");
            string dir = "Artifacts/GlassAudit"; Directory.CreateDirectory(dir);
            string report = $"{dir}/audit-{from:00}-{to:00}.tsv";
            File.WriteAllText(report, "order\tscene\tsupport\tmoving\tpane\tspot\ttarget\tresult\tbareTouch\tdetour\tglass/stand\tleadsTo\timage\tscreen\n");
            yield return Load(1); var scenes = (string[])game.Definition.SceneSequence.Clone();
            for (int order = from; order <= to && order <= scenes.Length; order++)
            {
                yield return LoadScene(scenes[order - 1]);
                string scene = scenes[order - 1].Replace("COgheSpatial", "");
                if (order <= 6) { File.AppendAllText(report, $"{order}\t{scene}\t-\t-\t-\t-\t-\tbare-by-design\t-\t-\t-\t-\t-\t-\n"); continue; }
                int k = 0; var followed = new HashSet<string>();
                foreach (var probe in StripeProbes())
                {
                    k++;
                    game.ResetLevel(); yield return null;
                    Place(probe.spot);
                    yield return Run(1.2f, null, null);
                    Vector3 c = game.Motion.Centre(0);
                    bool standing = Vector3.Distance(Flat(c), Flat(probe.spot)) < .04f && Mathf.Abs(c.y - probe.spot.y) < .035f
                        && Enumerable.Range(0, 32).Any(i => game.Motion.Support(i, out var s, out _, out _) && s == probe.support.Shape);
                    string head = $"{order}\t{scene}\t{probe.support.name}\t{probe.support.MotionFrame != null}\t{probe.pane.name}\t{RootLocal(probe.spot):F3}\t{probe.kind}";
                    if (!standing) { File.AppendAllText(report, head + $"\tcannot-stand\t-\t-\t-\t-\t-\t-\n"); continue; }
                    int anchor = game.Motion.Selected; bool bare = false; float detour = 0;
                    game.Motion.Move(anchor, probe.target);
                    yield return Run(7f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.target) < .045f, () =>
                    {
                        detour = Mathf.Max(detour, Vector3.Distance(game.Motion.Centre(anchor), probe.spot));
                        for (int i = 0; i < 32 && !bare; i++)
                            if (game.Motion.HasGrip(i) && game.Motion.Support(i, out var s, out var at, out _) && s == probe.pane.Shape && probe.pane.Grip(at)) bare = true;
                    });
                    bool reached = Vector3.Distance(game.Motion.Centre(anchor), probe.target) < .045f;
                    // on the glass: hanging from bare glass (3+ nodes), standing on nothing
                    int glassFeet = 0, standFeet = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        if (!game.Motion.Support(i, out var s, out var at, out _)) continue;
                        var patch = s != null ? s.GetComponent<VenomSurfacePatch>() : null; if (patch == null) continue;
                        if (patch.ExteriorGlass && game.Motion.HasGrip(i) && patch.Grip(at)) glassFeet++;
                        else if (Vector3.Dot(patch.Normal, game.Root.up) > .7f) standFeet++;
                    }
                    bool onGlass = reached && glassFeet >= 3 && standFeet == 0;
                    string image = "-", screen = "-", leads = "-";
                    if (onGlass)
                    {
                        string state = $"audit-{k:00}";
                        Capture(order, state); image = $"Artifacts/COgheSpatial/Frames/{order:00}-{state}.png"; screen = ScreenOf(game.Motion.Centre(anchor));
                        // what the glass leads to, once per support: the higher tops and the exit
                        string key = probe.support.name + "|" + (probe.support.MotionFrame != null);
                        if (followed.Add(key))
                        {
                            var reachedTops = new List<string>();
                            float floorY = probe.support.transform.position.y;
                            var tops = game.Surfaces.Where(p => p != null && p.isActiveAndEnabled && !p.ExteriorGlass && !p.IsCurved && p != probe.support
                                && Vector3.Dot(p.Normal, game.Root.up) > .9f && p.transform.position.y > floorY + .04f && Mathf.Min(p.Size.x, p.Size.y) >= .05f)
                                .GroupBy(p => p.name).Select(g => g.OrderByDescending(p => p.transform.position.y).First()).ToList();
                            Vector3 hang = game.Motion.Centre(anchor);
                            foreach (var top in tops)
                            {
                                // back on the glass where the climb ended, then to the middle of that top
                                game.ResetLevel(); yield return null; Place(hang); yield return Run(.3f, null, null);
                                Vector3 goal = top.transform.position + top.Normal * .037f;
                                game.Motion.Move(anchor, goal);
                                yield return Run(12f, () => Vector3.Distance(game.Motion.Centre(anchor), goal) < .05f, null);
                                if (Vector3.Distance(game.Motion.Centre(anchor), goal) < .05f) { reachedTops.Add(top.name); Capture(order, $"{state}-to-{reachedTops.Count}"); }
                            }
                            game.ResetLevel(); yield return null; Place(hang); yield return Run(.3f, null, null);
                            Vector3 outlet = game.Owner.Outlet.position; game.Motion.Move(anchor, outlet);
                            float exitMin = Vector3.Distance(game.Motion.Centre(anchor), outlet);
                            yield return Run(15f, () => game.Owner.Completed || exitMin < .05f, () => exitMin = Mathf.Min(exitMin, Vector3.Distance(game.Motion.Centre(anchor), outlet)));
                            // and back down to where it climbed from (a body that cannot come back is a trap)
                            game.ResetLevel(); yield return null; Place(hang); yield return Run(.3f, null, null);
                            game.Motion.Move(anchor, probe.spot);
                            yield return Run(12f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.spot) < .05f, null);
                            bool back = Vector3.Distance(game.Motion.Centre(anchor), probe.spot) < .05f;
                            leads = (reachedTops.Count > 0 ? string.Join("+", reachedTops) : "no higher top") + $"; exit {exitMin:F2} m; back {(back ? "yes" : "NO")}";
                            if (!back) Capture(order, state + "-noback");
                        }
                        else leads = "(as above)";
                    }
                    string result = onGlass ? "ON-GLASS" : reached ? "reached-standing" : "held";
                    File.AppendAllText(report, head + $"\t{result}\t{bare}\t{detour:F3}\t{glassFeet}/{standFeet}\t{leads}\t{image}\t{screen}\n");
                }
                Debug.Log($"GLASS AUDIT {order} {scene}: {k} probes");
            }
        }

        // Where a world point lands in the 720×1280 capture (the camera framed as Capture frames it).
        private string ScreenOf(Vector3 world)
        {
            game.CameraRig.Frame(720, 1280, 0, true); var cam = game.Owner.View; cam.aspect = 720f / 1280f;
            var v = cam.WorldToViewportPoint(world); cam.ResetAspect();
            return $"{v.x * 720:F0},{(1 - v.y) * 1280:F0}";
        }

        // Second pass: does a climb found by the audit skip the puzzle? From the level's real start (nothing operated), walk to
        // the spot, climb onto the bare glass, then tap the exit as a player would. One climb per support and target kind.
        [UnityTest, Explicit, Timeout(7200000)] public IEnumerator GlassStripeShortcutCheck()
        {
            string tsv = Environment.GetEnvironmentVariable("AUDIT_TSV") ?? "Artifacts/GlassAudit/audit-07-65.tsv";
            var rows = File.ReadAllLines(tsv).Skip(1).Select(l => l.Split('\t')).Where(c => c.Length > 7 && c[7] == "ON-GLASS")
                .GroupBy(c => c[0] + "|" + c[2] + "|" + c[3] + "|" + c[6]).Select(g => g.First()).ToList();
            string report = Environment.GetEnvironmentVariable("AUDIT_SHORTCUTS") ?? "Artifacts/GlassAudit/shortcuts.tsv";
            File.WriteAllText(report, "order\tscene\tsupport\tmoving\ttarget\tspot\twalk\tonGlass\tcompleted\texitMin\timage\n");
            yield return Load(1); var scenes = (string[])game.Definition.SceneSequence.Clone();
            foreach (var group in rows.GroupBy(c => int.Parse(c[0])).OrderBy(g => g.Key))
            {
                int order = group.Key; yield return LoadScene(scenes[order - 1]);
                var probes = StripeProbes(); int k = 0;
                foreach (var c in group)
                {
                    k++;
                    var want = ParseVector(c[5]);
                    var probe = probes.FirstOrDefault(p => p.support.name == c[2] && (p.support.MotionFrame != null).ToString() == c[3] && p.kind == c[6] && Vector3.Distance(RootLocal(p.spot), want) < .002f);
                    string head = $"{order}\t{c[1]}\t{c[2]}\t{c[3]}\t{c[6]}\t{c[5]}";
                    if (probe.pane == null) { File.AppendAllText(report, head + "\tno-match\t-\t-\t-\t-\n"); continue; }
                    game.ResetLevel(); yield return Run(.5f, null, null);
                    int anchor = game.Motion.Selected;
                    game.Motion.Move(anchor, probe.spot);
                    yield return Run(30f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.spot) < .05f, null);
                    float walk = Vector3.Distance(game.Motion.Centre(anchor), probe.spot);
                    game.Motion.Move(anchor, probe.target);
                    yield return Run(10f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.target) < .045f, null);
                    bool onGlass = Vector3.Distance(game.Motion.Centre(anchor), probe.target) < .045f;
                    float exitMin = float.PositiveInfinity; Vector3 outlet = game.Owner.Outlet.position;
                    yield return Tap(outlet);
                    yield return Run(30f, () => game.Owner.Completed || game.Matter.EscapedCount > 0, () => exitMin = Mathf.Min(exitMin, Vector3.Distance(game.Motion.Centre(anchor), outlet)));
                    bool done = game.Owner.Completed || game.Matter.EscapedCount > 0;
                    string image = "-";
                    if (done || exitMin < .06f) { Capture(order, $"shortcut-{k:00}"); image = $"Artifacts/COgheSpatial/Frames/{order:00}-shortcut-{k:00}.png"; }
                    File.AppendAllText(report, head + $"\t{(walk < .05f ? "yes" : $"no ({walk:F2} m)")}\t{onGlass}\t{(done ? "COMPLETED" : "no")}\t{exitMin:F3}\t{image}\n");
                }
            }
        }

        // Third pass, for every climb reachable from the start: the route taken there, each higher top the glass leads to
        // (then a tap on the exit from there), and whether COghe can get back down (a body that cannot is trapped).
        [UnityTest, Explicit, Timeout(7200000)] public IEnumerator GlassStripeShortcutDetail()
        {
            var audit = File.ReadAllLines(Environment.GetEnvironmentVariable("AUDIT_TSV") ?? "Artifacts/GlassAudit/audit-07-65.tsv").Skip(1).Select(l => l.Split('\t')).Where(c => c.Length > 11 && c[7] == "ON-GLASS").ToList();
            var leads = audit.Where(c => c[11] != "(as above)" && c[11] != "-").GroupBy(c => c[0] + "|" + c[2] + "|" + c[3]).ToDictionary(g => g.Key, g => g.First()[11]);
            var rows = File.ReadAllLines(Environment.GetEnvironmentVariable("AUDIT_SHORTCUTS") ?? "Artifacts/GlassAudit/shortcuts.tsv").Skip(1).Select(l => l.Split('\t')).Where(c => c.Length > 7 && c[6] == "yes" && c[7] == "True").ToList();
            string report = Environment.GetEnvironmentVariable("AUDIT_DETAIL") ?? "Artifacts/GlassAudit/detail.tsv";
            File.WriteAllText(report, "order\tscene\tsupport\tkind\troute\ttops\texit\tback\timages\n");
            yield return Load(1); var scenes = (string[])game.Definition.SceneSequence.Clone();
            foreach (var group in rows.GroupBy(c => int.Parse(c[0])).OrderBy(g => g.Key))
            {
                int order = group.Key; yield return LoadScene(scenes[order - 1]); var probes = StripeProbes(); int k = 0;
                foreach (var c in group)
                {
                    k++; var want = ParseVector(c[5]);
                    var probe = probes.FirstOrDefault(p => p.support.name == c[2] && (p.support.MotionFrame != null).ToString() == c[3] && p.kind == c[4] && Vector3.Distance(RootLocal(p.spot), want) < .002f);
                    if (probe.pane == null) continue;
                    string lead = leads.TryGetValue(c[0] + "|" + c[2] + "|" + c[3], out var l) ? l : "";
                    var topNames = lead.Split(';')[0].Split('+').Where(n => n != "no higher top" && n.Length > 0).ToList();
                    var images = new List<string>(); string state = $"detail-{k:00}";
                    // the route from the start to the spot
                    game.ResetLevel(); yield return Run(.5f, null, null); int anchor = game.Motion.Selected;
                    game.Motion.Move(anchor, probe.spot); var o = game.Motion.Get(anchor);
                    string route = o != null ? string.Join(">", o.Path.Select(v => v.ToString("F2"))) : "-";
                    yield return Run(30f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.spot) < .05f, null);
                    game.Motion.Move(anchor, probe.target);
                    yield return Run(10f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.target) < .045f, null);
                    Vector3 hang = game.Motion.Centre(anchor);
                    Capture(order, state); images.Add($"{order:00}-{state}");
                    var tops = new List<string>(); string exit = "-";
                    foreach (var name in topNames)
                    {
                        var top = game.Surfaces.Where(p => p != null && p.isActiveAndEnabled && p.name == name && Vector3.Dot(p.Normal, game.Root.up) > .9f).OrderByDescending(p => p.transform.position.y).FirstOrDefault();
                        if (top == null) continue;
                        // from the climb, not a placed body: start again, walk, climb, then go
                        game.ResetLevel(); yield return Run(.5f, null, null);
                        game.Motion.Move(anchor, probe.spot); yield return Run(30f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.spot) < .05f, null);
                        game.Motion.Move(anchor, probe.target); yield return Run(10f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.target) < .045f, null);
                        Vector3 goal = top.transform.position + top.Normal * .037f;
                        game.Motion.Move(anchor, goal); yield return Run(15f, () => Vector3.Distance(game.Motion.Centre(anchor), goal) < .05f, null);
                        bool there = Vector3.Distance(game.Motion.Centre(anchor), goal) < .05f;
                        tops.Add(name + (there ? "" : "(no)"));
                        if (!there) continue;
                        string shot = $"{state}-{tops.Count}"; Capture(order, shot); images.Add($"{order:00}-{shot}");
                        yield return Tap(game.Owner.Outlet.position);
                        yield return Run(25f, () => game.Owner.Completed || game.Matter.EscapedCount > 0, null);
                        if (game.Owner.Completed || game.Matter.EscapedCount > 0) { exit = "COMPLETED via " + name; Capture(order, shot + "-won"); images.Add($"{order:00}-{shot}-won"); }
                    }
                    // and back down: to the spot, then to where the level starts
                    game.ResetLevel(); yield return Run(.5f, null, null); Place(hang); yield return Run(.5f, null, null);
                    game.Motion.Move(anchor, probe.spot); yield return Run(15f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.spot) < .05f, null);
                    bool back = Vector3.Distance(game.Motion.Centre(anchor), probe.spot) < .05f;
                    if (!back)
                    {
                        Vector3 start = game.Owner.Spawn.position; game.Motion.Move(anchor, start);
                        yield return Run(20f, () => Vector3.Distance(game.Motion.Centre(anchor), start) < .06f, null);
                        back = Vector3.Distance(game.Motion.Centre(anchor), start) < .06f;
                        if (!back) { Capture(order, state + "-stuck"); images.Add($"{order:00}-{state}-stuck"); }
                    }
                    File.AppendAllText(report, $"{order}\t{c[1]}\t{c[2]}\t{c[4]}\t{route}\t{string.Join("+", tops)}\t{exit}\t{(back ? "yes" : "NO")}\t{string.Join(",", images)}\n");
                }
            }
        }


        // Fourth pass: the real outcome of a suspected shortcut (Owner.Completed, not a few nodes through the hole), from the
        // level's start: walk to the spot, climb, crawl to a top (or none), tap the exit, wait a minute. AUDIT_CASES lists
        // "order|support|kind|top" separated by ';'. With AUDIT_FILM set, frames are written there (15 per second, JPG).
        [UnityTest, Explicit, Timeout(7200000)] public IEnumerator GlassStripeShortcutWins()
        {
            var cases = (Environment.GetEnvironmentVariable("AUDIT_CASES") ?? "").Split(';').Where(c => c.Length > 0).Select(c => c.Split('|')).ToList();
            string film = Environment.GetEnvironmentVariable("AUDIT_FILM");
            var shortcuts = File.ReadAllLines(Environment.GetEnvironmentVariable("AUDIT_SHORTCUTS") ?? "Artifacts/GlassAudit/shortcuts.tsv").Skip(1).Select(l => l.Split('\t')).ToList();
            string report = Environment.GetEnvironmentVariable("AUDIT_WINS") ?? "Artifacts/GlassAudit/wins.tsv";
            if (!File.Exists(report)) File.WriteAllText(report, "order\tscene\tsupport\tkind\ttop\tfirstEscape\tescaped\tcompleted\tlost\tfailure\n");
            yield return Load(1); var scenes = (string[])game.Definition.SceneSequence.Clone();
            foreach (var c in cases)
            {
                int order = int.Parse(c[0]); yield return LoadScene(scenes[order - 1]); var probes = StripeProbes(false);
                var row = shortcuts.First(r => r[0] == c[0] && r[2] == c[1] && r[4] == c[2]); var want = ParseVector(row[5]);
                var probe = probes.First(p => p.support.name == c[1] && p.kind == c[2] && Vector3.Distance(RootLocal(p.spot), want) < .002f);
                string dir = film != null ? $"{film}/{order:00}-{c[1].Replace(' ', '_')}" : null; int frame = 0;
                if (dir != null) Directory.CreateDirectory(dir);
                IEnumerator Go(float seconds, Func<bool> stop)
                {
                    for (int i = 0; i < seconds / Dt && (stop == null || !stop()); i++)
                    {
                        Tick();
                        if (dir != null && i % 8 == 7) { FilmFrame($"{dir}/f{frame++:00000}.jpg"); yield return null; }
                        else if (i % 240 == 0) yield return null;
                    }
                }
                game.ResetLevel(); yield return Go(.5f, null); int anchor = game.Motion.Selected;
                var marks = new System.Text.StringBuilder(); marks.AppendLine($"{frame} start");
                game.Motion.Move(anchor, probe.spot); yield return Go(30f, () => Vector3.Distance(game.Motion.Centre(anchor), probe.spot) < .05f);
                marks.AppendLine($"{frame} climb");
                game.Motion.Move(anchor, probe.target);
                string firstGrip = "-";
                yield return Go(10f, () =>
                {
                    if (firstGrip == "-")
                        for (int i = 0; i < 32; i++)
                            if (game.Motion.HasGrip(i) && game.Motion.Support(i, out var s, out var at, out _) && s.GetComponent<VenomSurfacePatch>() is var pane && pane != null && pane.ExteriorGlass && pane.Grip(at))
                            {
                                var l = pane.transform.InverseTransformPoint(at);
                                var near = (pane.SlipRegions ?? Array.Empty<Rect>()).Where(r => Mathf.Abs(r.center.x - l.x) < r.width * .5f + .12f && Mathf.Abs(r.center.y - l.y) < r.height * .5f + .12f).Select(r => $"[{r.xMin:F3},{r.yMin:F3}..{r.xMax:F3},{r.yMax:F3}]");
                                firstGrip = $"{pane.name} local=({l.x:F3},{l.y:F3}) root={RootLocal(at):F3} stripes={string.Join(" ", near)} body={RootLocal(game.Motion.Centre(anchor)):F3}";
                                break;
                            }
                    return Vector3.Distance(game.Motion.Centre(anchor), probe.target) < .045f;
                });
                marks.AppendLine($"{frame} grip {firstGrip}");
                if (c[3].Length > 0)
                {
                    var top = game.Surfaces.Where(p => p != null && p.isActiveAndEnabled && p.name == c[3] && Vector3.Dot(p.Normal, game.Root.up) > .9f).OrderByDescending(p => p.transform.position.y).First();
                    Vector3 goal = top.transform.position + top.Normal * .037f; marks.AppendLine($"{frame} top");
                    game.Motion.Move(anchor, goal); yield return Go(15f, () => Vector3.Distance(game.Motion.Centre(anchor), goal) < .05f);
                }
                marks.AppendLine($"{frame} exit");
                yield return Tap(game.Owner.Outlet.position);
                float first = -1; float t0 = game.Matter.SimulationTime; string exitGrip = "-";
                yield return Go(60f, () =>
                {
                    if (first < 0 && game.Matter.EscapedCount > 0) first = game.Matter.SimulationTime - t0;
                    if (exitGrip == "-") exitGrip = FirstBareGrip(anchor);
                    return game.Owner.Completed || game.Owner.Lost;
                });
                marks.AppendLine($"{frame} exitgrip {exitGrip}");
                yield return Go(2f, null); marks.AppendLine($"{frame} end");
                if (dir != null) File.WriteAllText($"{dir}/marks.txt", marks.ToString());
                File.AppendAllText(report, $"{order}\t{scenes[order - 1]}\t{c[1]}\t{c[2]}\t{c[3]}\t{first:F1}\t{game.Matter.EscapedCount}\t{game.Owner.Completed}\t{game.Owner.Lost}\t{game.Failure}\n");
            }
        }
        // The first node holding bare glass, where it is and which stripes are near (to place a fix).
        private string FirstBareGrip(int anchor)
        {
            for (int i = 0; i < 32; i++)
                if (game.Motion.HasGrip(i) && game.Motion.Support(i, out var s, out var at, out _) && s.GetComponent<VenomSurfacePatch>() is var pane && pane != null && pane.ExteriorGlass && pane.Grip(at))
                {
                    var l = pane.transform.InverseTransformPoint(at);
                    var near = (pane.SlipRegions ?? Array.Empty<Rect>()).Where(r => Mathf.Abs(r.center.x - l.x) < r.width * .5f + .12f && Mathf.Abs(r.center.y - l.y) < r.height * .5f + .12f).Select(r => $"[{r.xMin:F3},{r.yMin:F3}..{r.xMax:F3},{r.yMax:F3}]");
                    return $"{pane.name} local=({l.x:F3},{l.y:F3}) root={RootLocal(at):F3} stripes={string.Join(" ", near)} body={RootLocal(game.Motion.Centre(anchor)):F3}";
                }
            return "-";
        }
        private void FilmFrame(string path)
        {
            game.CameraRig.Frame(720, 1280, 0, true); var camera = game.Owner.View; var target = RenderTexture.GetTemporary(720, 1280, 24); var previous = RenderTexture.active;
            var texture = new Texture2D(720, 1280, TextureFormat.RGB24, false);
            try { camera.targetTexture = target; camera.Render(); RenderTexture.active = target; texture.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToJPG(88)); }
            finally { camera.targetTexture = null; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(texture); }
        }


        // Exit-tap sweep: COghe dropped on every spot by the glass and on every top of a level, the exit tapped from there; the
        // route finder then tries every way, glass included. A win from a spot is reported (the spot may still need checking
        // for being reachable in play). AUDIT_LEVELS: display numbers separated by ','.
        [UnityTest, Explicit, Timeout(7200000)] public IEnumerator GlassExitTapSweep()
        {
            var levels = (Environment.GetEnvironmentVariable("AUDIT_LEVELS") ?? "").Split(',').Where(t => t.Length > 0).Select(int.Parse).ToList();
            string report = Environment.GetEnvironmentVariable("AUDIT_SWEEP") ?? "Artifacts/GlassAudit/exit-sweep.tsv";
            File.WriteAllText(report, "order\tscene\tfrom\tspot\twon\tfirstGlass\n");
            yield return Load(1); var scenes = (string[])game.Definition.SceneSequence.Clone();
            foreach (int order in levels)
            {
                yield return LoadScene(scenes[order - 1]);
                var starts = new List<(string name, Vector3 spot)> { ("spawn", Vector3.zero) };
                foreach (var probe in StripeProbes(false)) if (!starts.Any(s => s.name == probe.support.name && Vector3.Distance(s.spot, probe.spot) < .02f)) starts.Add((probe.support.name, probe.spot));
                foreach (var top in game.Surfaces.Where(p => p != null && p.isActiveAndEnabled && !p.ExteriorGlass && !p.IsCurved && Vector3.Dot(p.Normal, game.Root.up) > .9f && Mathf.Min(p.Size.x, p.Size.y) >= .06f))
                    starts.Add((top.name + " (top)", top.transform.position + top.Normal * .042f));
                int won = 0;
                foreach (var (name, spot) in starts)
                {
                    game.ResetLevel(); yield return null;
                    if (name != "spawn") Place(spot);
                    yield return Run(1f, null, null);
                    if (name != "spawn" && Vector3.Distance(game.Motion.Centre(0), spot) > .06f) { File.AppendAllText(report, $"{order}\t{scenes[order - 1]}\t{name}\t{RootLocal(spot):F3}\tcannot-stand\t-\n"); continue; }
                    int anchor = game.Motion.Selected; string glass = "-";
                    yield return Tap(game.Owner.Outlet.position);
                    yield return Run(40f, () => game.Owner.Completed || game.Owner.Lost, () => { if (glass == "-") glass = FirstBareGrip(anchor); });
                    bool win = game.Owner.Completed; if (win) won++;
                    File.AppendAllText(report, $"{order}\t{scenes[order - 1]}\t{name}\t{RootLocal(spot):F3}\t{(win ? "WON" : "no")}\t{glass}\n");
                }
                Debug.Log($"GLASS EXIT SWEEP {order}: {starts.Count} starts, {won} won");
            }
        }

        private static Vector3 ParseVector(string s)
        {
            var v = s.Trim('(', ')').Split(',').Select(t => float.Parse(t.Trim(), System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            return new Vector3(v[0], v[1], v[2]);
        }

        private struct StripeProbe { public VenomSurfacePatch support, pane; public Vector3 spot, target; public string kind; }
        private Vector3 RootLocal(Vector3 world) => game.Root.InverseTransformPoint(world);
        private Vector3 Flat(Vector3 world) { var l = game.Root.InverseTransformPoint(world); l.y = 0; return l; }

        // Where COghe stands against a side pane: each up-facing support (fixed or moving) with an edge within 4 cm of the
        // glass, at both ends and the middle of that edge; targets are the bare glass just above the stripe and beside it.
        private List<StripeProbe> StripeProbes(bool bareOnly = true)
        {
            var list = new List<StripeProbe>(); const float body = .037f, band = .09f;
            var up = game.Root.up;
            foreach (var pane in game.Surfaces.Where(p => p != null && p.isActiveAndEnabled && p.ExteriorGlass && !p.IsCurved && Mathf.Abs(Vector3.Dot(p.Normal, up)) < .5f))
                foreach (var support in game.Surfaces.Where(p => p != null && p.isActiveAndEnabled && !p.ExteriorGlass && !p.IsCurved && Vector3.Dot(p.Normal, up) > .9f))
                {
                    var t = support.transform; var q = new[] { new Vector3(-1, -1), new Vector3(1, -1), new Vector3(1, 1), new Vector3(-1, 1) }
                        .Select(v => pane.transform.InverseTransformPoint(t.TransformPoint(new Vector3(v.x * support.Size.x * .5f, v.y * support.Size.y * .5f, 0)))).ToArray();
                    for (int i = 0; i < 4; i++)
                    {
                        var a = q[i]; var b = q[(i + 1) % 4];
                        if (Mathf.Abs(a.z) > .04f || Mathf.Abs(b.z) > .04f || Mathf.Abs(a.y - b.y) > .003f) continue;
                        float x0 = Mathf.Min(a.x, b.x), x1 = Mathf.Max(a.x, b.x), y = (a.y + b.y) * .5f, z = Mathf.Max(a.z, b.z);
                        if (x1 - x0 < .05f) continue;   // too narrow to stand on
                        var spots = x1 - x0 >= .09f ? new[] { x0 + .035f, (x0 + x1) * .5f, x1 - .035f } : new[] { (x0 + x1) * .5f };
                        for (int s = 0; s < spots.Length; s++)
                        {
                            Vector3 spot = pane.transform.TransformPoint(new Vector3(spots[s], y + body + .005f, z + body));
                            void Try(string kind, float tx, float ty)
                            {
                                if (Mathf.Abs(tx) > pane.Size.x * .5f - .02f || Mathf.Abs(ty) > pane.Size.y * .5f - .02f) return;
                                Vector3 onGlass = pane.transform.TransformPoint(new Vector3(tx, ty, 0));
                                if (bareOnly && !pane.Grip(onGlass) || pane.Hole && Vector2.Distance(new Vector2(tx, ty), pane.HoleCentre) < pane.HoleRadius + .03f) return;
                                Vector3 target = onGlass + pane.Normal * .022f;
                                if (Physics.OverlapSphere(target, .012f, ~0, QueryTriggerInteraction.Ignore).Any(c => !c.TryGetComponent<VenomContact>(out _))) return;   // inside a block
                                list.Add(new StripeProbe { support = support, pane = pane, spot = spot, target = target, kind = kind });
                            }
                            Try("above", spots[s], y + band + .045f);
                            if (s == 0) Try("beside", x0 - .05f, y + .045f);
                            if (s == spots.Length - 1) Try("beside", x1 + .05f, y + .045f);
                        }
                    }
                }
            return list;
        }

        // Moves the whole body (after a reset) so its centre is at a point; bonds are relative, so it keeps its shape.
        private void Place(Vector3 centre)
        {
            Vector3 delta = centre - game.Motion.Centre(0);
            foreach (var body in game.Matter.Bodies) { body.position += delta; body.linearVelocity = body.angularVelocity = Vector3.zero; body.transform.position = body.position; }
            Physics.SyncTransforms();
        }

        private IEnumerator Run(float seconds, Func<bool> stop, Action each)
        {
            for (int i = 0; i < seconds / Dt && (stop == null || !stop()); i++) { Tick(); each?.Invoke(); if (i % 240 == 0) yield return null; }
        }
    }
}
