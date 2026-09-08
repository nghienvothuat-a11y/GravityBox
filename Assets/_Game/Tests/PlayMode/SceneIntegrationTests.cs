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
        public IEnumerator GameplayScene_BootstrapsElevenLevelsAndChangesOnlyOnManualNext()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Scenes/Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            GameBootstrap bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<Canvas>(), Is.Not.Null);
            var levels = bootstrap.Levels;
            Assert.That(levels.Catalog.Levels.Length, Is.EqualTo(11));
            Assert.That(Time.fixedDeltaTime, Is.EqualTo(1f / 120f).Within(0.000001f));
            for (int index = 0; index < levels.Catalog.Levels.Length; index++)
            {
                Assert.That(levels.Index, Is.EqualTo(index));
                var outlet = levels.Current.Exit;
                float radius = levels.Ball.Profile.Radius;
                levels.Ball.Body.position = outlet.transform.TransformPoint(new Vector3(0, 0, -outlet.WallHalfDepth - radius * 1.2f));
                levels.Ball.Body.linearVelocity = outlet.transform.forward * 1.5f;
                UnityEngine.Physics.SyncTransforms();
                outlet.BeginTracking();
                float deadline = Time.unscaledTime + 1;
                while (!outlet.HasExited && Time.unscaledTime < deadline) yield return null;
                Assert.That(outlet.HasExited, Is.True, levels.Definition.Id);
                Assert.That(levels.Ball.Body.isKinematic, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(1));
                yield return new WaitForSecondsRealtime(levels.Catalog.CompletionDelay + 0.1f);
                Assert.That(levels.Index, Is.EqualTo(index), "Keep the experiment until the player changes it.");
                Assert.That(levels.Session.State, Is.EqualTo(SessionState.Completing));
                levels.Next();
            }
            Assert.That(levels.Index, Is.Zero, "Manual next wraps around all eleven levels.");
            levels.Load(0);
            levels.TogglePause();
            Assert.That(Time.timeScale, Is.Zero);
            levels.ResetLevel();
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
            Scene gameplay = SceneManager.GetActiveScene();
            Scene empty = SceneManager.CreateScene("Empty after eleven-level integration");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(gameplay);
        }
    }
}
#endif
