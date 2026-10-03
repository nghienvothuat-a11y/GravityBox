#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_TEST_TOOLS
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Opt-in device check of fullscreen presentation and the real victory dance.
    /// Launch with -coghe-mobile-ui-proof, or Android boolean intent extra coghe_ui_proof.
    /// Uses an in-memory wallet/progress; never writes the player's saves.</summary>
    public sealed class COgheMobileUIProof : MonoBehaviour
    {
        private string output;
        private VenomCampaign game;
        private COgheProductUI ui;
        private readonly System.Text.StringBuilder report = new System.Text.StringBuilder();
        private int overlaps;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            bool run = Array.IndexOf(Environment.GetCommandLineArgs(), "-coghe-mobile-ui-proof") >= 0;
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
                run |= intent.Call<bool>("getBooleanExtra", "coghe_ui_proof", false);
#endif
            if (!run) return;
            VenomCampaignSave.PersistenceEnabled = false;
            COgheProductMode.OverrideForTests = true;
            COgheShop.ResetForTests(new COgheShop.State { Migrated = true });
            COgheEntitlements.ResetForTests();
            var root = new GameObject("Mobile UI proof"); DontDestroyOnLoad(root);
            root.AddComponent<COgheMobileUIProof>();
        }

        private IEnumerator Start()
        {
            output = Path.Combine(Application.persistentDataPath, "MobileUIProof"); Directory.CreateDirectory(output);
            Application.runInBackground = true;
            yield return new WaitForSecondsRealtime(2);
            game = FindFirstObjectByType<VenomCampaign>(); ui = game.ProductUI;
            report.AppendLine($"screen={Screen.width}x{Screen.height} safe={Screen.safeArea} fullscreen={Screen.fullScreen} camera={game.Owner.View.pixelRect}");
            COgheAds.Provider = new COgheTestAds(); ui.ShowMenu();
            yield return Capture("01-menu");
            ui.Play();
            if (COgheIntro.Playing)
            {
                var intro = FindFirstObjectByType<COgheIntro>(); while (!intro.Started) yield return null;
                intro.Speed = 30; intro.Skip(); while (COgheIntro.Playing) yield return null;
            }
            game.AutoAdvance = false;
            yield return new WaitForSecondsRealtime(.5f);
            yield return Capture("02-game");
            game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Owner.Outlet.position));
            float deadline = Time.realtimeSinceStartup + 45;
            while (!game.Owner.Completed && Time.realtimeSinceStartup < deadline) yield return null;
            if (!game.Owner.Completed) { Finish("failed: level 1 did not complete"); yield break; }
            yield return new WaitForSecondsRealtime(1.1f);
            // Restart the offer with a fixture ID so it stays visible throughout each dance check.
            for (int variant = 0; variant < 3; variant++)
            {
                game.Owner.Celebration.SetVariantForTests(variant);
                ui.ShowVictoryForTests("ui-proof-" + variant, 1);
                for (int frame = 0; frame < 60; frame++)
                {
                    yield return new WaitForSecondsRealtime(.04f);
                    CheckDance();
                    if (frame == 15 || frame == 45) yield return Capture($"03-victory-{variant}-{frame}");
                }
            }
            // Claim the test reward, then inspect the one updated amount (no duplicated text).
            ui.ShowVictoryForTests("ui-proof-triple", 1);
            ui.GetComponentsInChildren<Button>().Single(b => b.name == "Triple drops").onClick.Invoke();
            yield return new WaitForSecondsRealtime(2.5f);
            var ad = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.name == "Close test ad");
            if (ad != null) ad.onClick.Invoke();
            yield return Capture("04-reward");
            // Remove the ad provider to also check the taller, banner-free layout.
            COgheAds.Provider = null; ui.ShowVictoryForTests("ui-proof-no-ad", 1);
            yield return Capture("05-victory-no-banner"); CheckDance();
            game.Progress.HomeUnlocked = true; ui.OpenHome();
            yield return new WaitForSecondsRealtime(1);
            yield return Capture("06-home");
            ui.OpenStyle(); yield return new WaitForSecondsRealtime(1);
            yield return Capture("07-style");
            Finish(overlaps == 0 ? "passed" : "failed: dance overlaps UI");
        }

        private void CheckDance()
        {
            var min = new Vector2(float.MaxValue, float.MaxValue); var max = -min;
            // Measure the drawn deforming mesh, including the raised tendrils, not just physics particles.
            foreach (var mesh in game.Matter.GetComponentsInChildren<MeshFilter>())
            {
                if (!mesh.GetComponent<Renderer>().enabled) continue;
                foreach (var v in mesh.sharedMesh.vertices)
                {
                    Vector2 p = game.Owner.View.WorldToScreenPoint(mesh.transform.TransformPoint(v));
                    min = Vector2.Min(min, p); max = Vector2.Max(max, p);
                }
            }
            var body = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            foreach (var label in ui.GetComponentsInChildren<Text>())
            {
                if (label.name != "Drops earned" && label.name != "Victory" && label.name != "Next level") continue;
                Check(label.rectTransform, body);
            }
            foreach (var button in ui.GetComponentsInChildren<Button>()) Check((RectTransform)button.transform, body);
        }
        private void Check(RectTransform t, Rect body)
        {
            var corners = new Vector3[4]; t.GetWorldCorners(corners);
            var rect = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
            if (!rect.Overlaps(body)) return;
            overlaps++; if (overlaps < 10) report.AppendLine($"overlap: {t.name} rect={rect} skin={body}");
        }
        private IEnumerator Capture(string name)
        {
            yield return new WaitForSecondsRealtime(.2f); yield return new WaitForEndOfFrame();
            // CaptureScreenshot prefixes persistentDataPath on Android, even for an absolute filename.
            // Write the pixels ourselves so desktop and device reports use the same directory contract.
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(output, name + ".png"), shot.EncodeToPNG());
            Destroy(shot); yield return null;
        }
        private void Finish(string result)
        {
            report.AppendLine($"result={result} overlaps={overlaps} savesWritten=false");
            File.WriteAllText(Path.Combine(output, "result.txt"), report.ToString());
            Debug.Log("COGHE MOBILE UI PROOF " + result + " " + output); Application.Quit(overlaps == 0 && result == "passed" ? 0 : 1);
        }
    }
}
#endif
