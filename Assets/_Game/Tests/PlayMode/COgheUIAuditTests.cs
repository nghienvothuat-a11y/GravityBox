using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    // The UI layout audit (Mrk 03/10: "check every screen for things on top of each other"): every page and popup, laid out
    // for a 16:9 phone, a 19.5:9 phone and a 4:3 tablet; no visible button or line of text may overlap another, and nothing
    // may leave the screen. Text is measured by its glyphs, not its (generous) rectangle; scrolling lists are clipped.
    public partial class COgheProductUITests
    {
        private int Checked; private readonly StringBuilder Inventory = new StringBuilder();
        private static readonly Vector2[] AuditScreens = { new Vector2(720, 1280), new Vector2(1080, 2340), new Vector2(1536, 2048) };

        private struct UIBox { public string Name; public Rect R; public Transform T; public Button Owner; }

        private static Rect WorldRect(RectTransform t)
        {
            var c = new Vector3[4]; t.GetWorldCorners(c);
            return Rect.MinMaxRect(c[0].x, c[0].y, c[2].x, c[2].y);
        }
        /// <summary>The ink of a text: the bounds of its glyphs (empty if it draws nothing).</summary>
        private static Rect GlyphRect(Text text)
        {
            var gen = text.cachedTextGenerator; if (gen == null || gen.vertexCount == 0) return Rect.zero;
            var verts = gen.verts; float unit = 1 / text.pixelsPerUnit;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < verts.Count; i++)
            {
                var p = text.rectTransform.TransformPoint(verts[i].position * unit);
                min = Vector2.Min(min, p); max = Vector2.Max(max, p);
            }
            return min.x > max.x ? Rect.zero : Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        private static Rect Clip(Transform t, Rect r)
        {
            for (var p = t.parent; p != null; p = p.parent)
            {
                var mask = p.GetComponent<RectMask2D>(); if (mask == null) continue;
                var m = WorldRect((RectTransform)p.transform);
                float xMin = Mathf.Max(r.xMin, m.xMin), yMin = Mathf.Max(r.yMin, m.yMin), xMax = Mathf.Min(r.xMax, m.xMax), yMax = Mathf.Min(r.yMax, m.yMax);
                if (xMax <= xMin || yMax <= yMin) return Rect.zero;
                r = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            }
            return r;
        }
        private static List<UIBox> Collect(Transform root)
        {
            var list = new List<UIBox>();
            if (root == null) return list;
            foreach (var b in root.GetComponentsInChildren<Button>())
            {
                var image = b.targetGraphic as Image; if (image == null || image.color.a < .05f) continue;
                var r = Clip(b.transform, WorldRect((RectTransform)b.transform)); if (r.width < 1) continue;
                list.Add(new UIBox { Name = "button '" + b.name + "'", R = r, T = b.transform, Owner = b });
            }
            // pictures that are not part of a button: droplets, item pictures, glyphs (backgrounds and panels are not)
            foreach (var im in root.GetComponentsInChildren<Image>())
            {
                if (im.GetComponent<Button>() != null || im.GetComponentInParent<Button>() != null || im.color.a < .05f || im.sprite == null) continue;
                bool picture = im.name == "Drop" || im.name == "Icon" || System.Enum.IsDefined(typeof(COgheIcon), im.name);
                if (!picture) continue;
                var r = Clip(im.transform, WorldRect(im.rectTransform)); if (r.width < 1) continue;
                list.Add(new UIBox { Name = "picture '" + im.name + "'", R = r, T = im.transform });
            }
            foreach (var t in root.GetComponentsInChildren<Text>())
            {
                if (!t.enabled || string.IsNullOrWhiteSpace(t.text) || t.color.a < .05f) continue;
                var r = Clip(t.transform, GlyphRect(t)); if (r.width < 1) continue;
                list.Add(new UIBox { Name = "text '" + t.text.Replace("\n", " ") + "' (" + t.name + ")", R = r, T = t.transform, Owner = t.GetComponentInParent<Button>() });
            }
            return list;
        }
        /// <summary>Overlaps between boxes that do not belong together (a button and its own label/icon do), and boxes off screen.</summary>
        private void Audit(string scene, Vector2 screen, StringBuilder report, ref int problems)
        {
            Canvas.ForceUpdateCanvases();
            float unit = ui.GetComponentInChildren<Canvas>().scaleFactor;
            var safeRoot = ui.SafeRoot; var screenRect = WorldRect(safeRoot);
            var roots = new List<Transform>();
            Transform page = null, popup = null;
            foreach (Transform child in safeRoot) { if (child.name.StartsWith("Page")) page = child; if (child.name.StartsWith("Popup")) popup = child; }
            foreach (var root in new[] { page, popup })
            {
                if (root == null) continue;
                var boxes = Collect(root);
                if (root == page) foreach (Transform child in safeRoot) if (child.name == "Notice") boxes.AddRange(Collect(child));
                Checked += boxes.Count;
                if (screen == AuditScreens[0]) Inventory.AppendLine($"{scene} [{root.name}] {boxes.Count}: " + string.Join(" | ", boxes.Select(x => x.Name)));
                for (int i = 0; i < boxes.Count; i++)
                {
                    var a = boxes[i];
                    if (a.R.xMin < screenRect.xMin - 1 || a.R.xMax > screenRect.xMax + 1 || a.R.yMin < screenRect.yMin - 1 || a.R.yMax > screenRect.yMax + 1)
                    { problems++; report.AppendLine($"{screen.x}x{screen.y} {scene}: {a.Name} leaves the screen"); }
                    for (int j = i + 1; j < boxes.Count; j++)
                    {
                        var b = boxes[j];
                        if (a.Owner != null && a.Owner == b.Owner) continue;                   // a button with its own label / icon
                        if (a.T.IsChildOf(b.T) || b.T.IsChildOf(a.T)) continue;
                        float w = Mathf.Min(a.R.xMax, b.R.xMax) - Mathf.Max(a.R.xMin, b.R.xMin), h = Mathf.Min(a.R.yMax, b.R.yMax) - Mathf.Max(a.R.yMin, b.R.yMin);
                        if (w / unit <= 2 || h / unit <= 2) continue;                           // touching is fine
                        problems++; report.AppendLine($"{screen.x}x{screen.y} {scene}: {a.Name} overlaps {b.Name} ({w / unit:F0}×{h / unit:F0})");
                    }
                }
            }
        }
        private void Press(string button)
        {
            var b = ui.GetComponentsInChildren<Button>().FirstOrDefault(x => x.name == button);
            Assert.IsNotNull(b, "No button " + button); b.onClick.Invoke();
        }

        [UnityTest] public IEnumerator NoScreenHasButtonsOrTextOnTopOfEachOther()
        {
            var report = new StringBuilder(); int problems = 0;
            var look = new COgheStyle { Inks = new[] { "INK_OCEAN", null, null, null } }; for (int i = 0; i < 32; i++) look.Amount[i] = new Vector4(.6f, 0, 0, 0);
            try
            {
                // the detector itself: two labels on top of each other must be caught
                ui.ShowMenu(); yield return null;
                var probe = new GameObject("Probe", typeof(RectTransform)).GetComponent<RectTransform>(); probe.SetParent(ui.SafeRoot.GetChild(ui.SafeRoot.childCount - 1), false);
                var page0 = ui.SafeRoot.Cast<Transform>().First(t => t.name.StartsWith("Page"));
                for (int k = 0; k < 2; k++)
                {
                    var t = new GameObject("Probe " + k, typeof(RectTransform), typeof(Text)).GetComponent<Text>(); t.rectTransform.SetParent(page0, false);
                    t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text = "OVERLAP PROBE"; t.fontSize = 20; t.rectTransform.sizeDelta = new Vector2(200, 40);
                }
                Object.Destroy(probe.gameObject);
                int before = problems; var scratch = new StringBuilder(); Audit("probe", AuditScreens[0], scratch, ref problems);
                Assert.Greater(problems, before, "The audit catches overlapping text: " + scratch); problems = before;
                COgheShop.ResetForTests(new COgheShop.State { Migrated = true, Drops = 1234 }); COgheShop.TestsOwnUnlocked = false;
                COgheHomeRoom.UnlockedLevelOverride = 26; COgheAds.Provider = new CountingAds { Height = 0 };
                foreach (var screen in AuditScreens)
                {
                    COgheProductUI.LayoutAreaForTests = new Rect(0, 0, screen.x, screen.y);
                    var ads = new CountingAds(); ads.Height = 50 * screen.x / 360f; COgheAds.ResetForTests(); COgheAds.Provider = ads;
                    ui.ShowMenu(); ui.RelayoutForTests(); yield return null; Audit("Main menu (banner)", screen, report, ref problems);
                    ui.Notify("All puzzles complete. Visit Home!"); yield return null; Audit("Main menu notice", screen, report, ref problems);
                    ui.ShowPopup(COgheProductPopup.Plus); yield return null; Audit("Plus", screen, report, ref problems); ui.Resume();
                    game.Progress.HomeUnlocked = false; ui.OpenHome(); yield return null; Audit("Home locked", screen, report, ref problems); ui.Resume();
                    game.Progress.HomeUnlocked = true; COgheStyle.ResetForTests(look); COgheStyle.Current.ApplyTo(game);
                    ui.OpenHome(); yield return new WaitForSecondsRealtime(.3f); Audit("Home (banner)", screen, report, ref problems);
                    ui.Notify("Trampoline unlocks at level 30"); yield return null; Audit("Home notice", screen, report, ref problems);
                    ui.ShowPopup(COgheProductPopup.Pause); yield return null; Audit("Home pause", screen, report, ref problems); ui.Resume();
                    ui.ShowPopup(COgheProductPopup.Collection); yield return null; Audit("Home items", screen, report, ref problems);
                    ui.OfferItem("SWING", null); yield return null; Audit("Buy (item)", screen, report, ref problems); ui.Resume();
                    ui.ShowShopPopup(COgheProductPopup.Buy); yield return null; Audit("Drops", screen, report, ref problems); ui.Resume();
                    ui.ShowShopPopup(COgheProductPopup.Gift); yield return null; Audit("Daily gift", screen, report, ref problems); ui.Resume();
                    ui.OpenStyle(); yield return new WaitForSecondsRealtime(.3f); Audit("Style colors", screen, report, ref problems);
                    Press("Rinse"); yield return null; Audit("Style rinse", screen, report, ref problems); ui.Resume();
                    ui.ChooseStyleItem("INK_GOLD"); yield return null; Audit("Style ink on trial", screen, report, ref problems);
                    ui.SetStyleTab(1, 0); yield return null; Audit("Style hats", screen, report, ref problems);
                    ui.ChooseStyleItem("HAT_STRAW"); yield return null; Audit("Style hat on trial", screen, report, ref problems);
                    ui.ChooseStyleItem("HAT_ASTRO"); yield return null; Audit("Style locked", screen, report, ref problems); ui.Resume();
                    ui.SetStyleTab(1, 1); yield return null; Audit("Style inside", screen, report, ref problems);
                    ui.LeaveStyle(); yield return null; Audit("Style keep this look", screen, report, ref problems);
                    Press("Take it off"); yield return null;
                    ui.ShowVictoryForTests(ui.Catalog.Levels[9].Id, 10); yield return null; Audit("Victory (banner)", screen, report, ref problems);
                    ui.VictoryStepForTests(); yield return null; Audit("New for COghe", screen, report, ref problems); ui.Resume();
                    ui.ShowMenu(); ui.Play(); yield return new WaitForSecondsRealtime(.3f); Audit("Level", screen, report, ref problems);
                    foreach (var popup in new[] { COgheProductPopup.Pause, COgheProductPopup.Help, COgheProductPopup.Restart, COgheProductPopup.Failure, COgheProductPopup.Levels })
                    { ui.ShowPopup(popup); yield return null; Audit("Level " + popup, screen, report, ref problems); }
                    ui.Resume(); ui.Notify("All puzzles complete. Visit Home!"); yield return null; Audit("Level notice", screen, report, ref problems);
                }
            }
            finally
            {
                COgheProductUI.LayoutAreaForTests = null; COgheShop.ResetForTests(); COgheAds.ResetForTests(); COgheStyle.ResetForTests();
                COgheHomeRoom.UnlockedLevelOverride = null;
                Directory.CreateDirectory("Artifacts/UIAudit"); File.WriteAllText("Artifacts/UIAudit/report.txt", report.ToString());
                File.WriteAllText("Artifacts/UIAudit/inventory.txt", $"checked {Checked} boxes\n" + Inventory);
            }
            ui.RelayoutForTests();
            Assert.AreEqual(0, problems, "UI overlaps (Artifacts/UIAudit/report.txt):\n" + report);
        }
    }
}
