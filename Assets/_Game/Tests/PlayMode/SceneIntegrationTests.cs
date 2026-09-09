#if UNITY_EDITOR
using System.Collections;
using GravityBox.App;
using GravityBox.Foundation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
    public sealed class SceneIntegrationTests
    {
        [UnityTest]
        public IEnumerator ExitAssistHudReflectsTheMotorAndResetClearsIt()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Scenes/Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var levels = Object.FindFirstObjectByType<GameBootstrap>().Levels;
            levels.Load(1);
            var exit = levels.Current.Exit;
            levels.Ball.Body.position = exit.transform.TransformPoint(new Vector3(.029f, 0, -.021f));
            levels.Ball.Body.linearVelocity = Vector3.zero;
            UnityEngine.Physics.SyncTransforms(); exit.BeginTracking();
            UnityEngine.UI.Text status = null, hint = null;
            foreach (var label in Object.FindFirstObjectByType<GravityBox.Presentation.GameHud>().GetComponentsInChildren<UnityEngine.UI.Text>())
            {
                if (label.name == "Status") status = label;
                if (label.name == "Teaching hint") hint = label;
            }
            float deadline = Time.unscaledTime + .6f;
            bool sawAssist = false;
            while (Time.unscaledTime < deadline && !exit.HasExited)
            {
                yield return null;
                if (status.text.Contains("EXIT ASSIST"))
                {
                    sawAssist = true;
                    Assert.That(hint.preferredHeight, Is.LessThanOrEqualTo(hint.rectTransform.rect.height + 1));
                }
            }
            Assert.That(sawAssist, Is.True);
            Assert.That(exit.HasExited, Is.True);
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
            Assert.That(levels.Ball.Body.isKinematic, Is.False);
            levels.ResetLevel(); yield return null;
            Assert.That(exit.AssistActive, Is.False);
            Assert.That(hint.text, Is.EqualTo(levels.Definition.TeachingHint));
            Scene gameplay = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("Empty after exit assist HUD"));
            yield return SceneManager.UnloadSceneAsync(gameplay);
        }

        [UnityTest]
        public IEnumerator GameplayScene_BootstrapsSixteenLevelsAndChangesOnlyOnManualNext()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Scenes/Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            GameBootstrap bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<Canvas>(), Is.Not.Null);
            var levels = bootstrap.Levels;
            Assert.That(levels.Catalog.Levels.Length, Is.EqualTo(16));
            Assert.That(Time.fixedDeltaTime, Is.EqualTo(1f / 120f).Within(0.000001f));
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Assert.That(levels.Index, Is.EqualTo(index));
                var outlet = levels.Current.Exit;
                foreach (var ball in levels.Balls)
                {
                    float radius = ball.Profile.Radius;
                    ball.Body.position = outlet.transform.TransformPoint(new Vector3(0,0,-outlet.WallHalfDepth-radius*1.2f));
                    ball.Body.linearVelocity = outlet.transform.forward * 1.5f;
                    UnityEngine.Physics.SyncTransforms(); outlet.BeginTracking();
                    float deadline = Time.unscaledTime + 1;
                    while (!outlet.HasBallExited(ball) && Time.unscaledTime < deadline) yield return null;
                    Assert.That(outlet.HasBallExited(ball), Is.True, levels.Definition.Id);
                    Assert.That(ball.Body.isKinematic, Is.False);
                    if (outlet.EscapedCount < levels.Balls.Count)
                    {
                        Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
                        yield return new WaitForSecondsRealtime(.6f);
                        Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active), "An escaped ball outside bounds must not fail its partner.");
                        var texts = Object.FindFirstObjectByType<GravityBox.Presentation.GameHud>().GetComponentsInChildren<UnityEngine.UI.Text>();
                        Assert.That(System.Array.Find(texts,t=>t.name=="Progress").text, Does.Contain("OUT 1/2"));
                    }
                }
                Assert.That(outlet.HasExited, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(1));
                yield return new WaitForSecondsRealtime(levels.Catalog.CompletionDelay + 0.1f);
                Assert.That(levels.Index, Is.EqualTo(index), "Keep the experiment until the player changes it.");
                Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
                levels.Next();
            }
            Assert.That(levels.Index, Is.Zero, "Manual next wraps around all sixteen levels.");
            levels.Load(0);
            levels.TogglePause();
            Assert.That(Time.timeScale, Is.Zero);
            levels.ResetLevel();
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
            Scene gameplay = SceneManager.GetActiveScene();
            Scene empty = SceneManager.CreateScene("Empty after sixteen-level integration");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(gameplay);
        }
    }
}
#endif
