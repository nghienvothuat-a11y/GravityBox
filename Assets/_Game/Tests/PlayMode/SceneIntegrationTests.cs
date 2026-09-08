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
        public IEnumerator GameplayScene_BootstrapsAndAdvancesToZeroGAndFinishes()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Scenes/Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            GameBootstrap bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.Levels.Index, Is.Zero);
            Assert.That(Object.FindFirstObjectByType<Canvas>(), Is.Not.Null);
            var levels = bootstrap.Levels;
            for (int i = 0; i < 16; i++)
            {
                Assert.That(levels.Index, Is.EqualTo(i));
                foreach (var plate in levels.Current.Plates) plate.SetActive(true);
                var outlet = levels.Current.Exit;
                levels.Ball.Body.position = outlet.transform.TransformPoint(new Vector3(0, 0, -0.7f));
                levels.Ball.Body.linearVelocity = outlet.transform.forward * 6;
                UnityEngine.Physics.SyncTransforms();
                outlet.BeginTracking();
                float deadline = Time.unscaledTime + 2;
                while (!outlet.HasExited && Time.unscaledTime < deadline) yield return null;
                Assert.That(outlet.HasExited, Is.True, levels.Definition.Id);
                Assert.That(levels.Ball.Body.isKinematic, Is.False);
                Vector3 escapedAt = levels.Ball.Body.position;
                yield return new WaitForSecondsRealtime(0.3f);
                Assert.That(levels.Index, Is.EqualTo(i), "Keep the escape visible before advancing.");
                Assert.That(Vector3.Distance(escapedAt, levels.Ball.Body.position), Is.GreaterThan(0.1f));
                yield return new WaitForSecondsRealtime(levels.Catalog.CompletionDelay);
            }
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Finished));
            levels.Load(0);
            levels.TogglePause();
            Assert.That(Time.timeScale, Is.Zero);
            levels.ResetLevel();
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(levels.Session.State, Is.EqualTo(SessionState.Active));
            Scene gameplay = SceneManager.GetActiveScene();
            Scene empty = SceneManager.CreateScene("Empty after integration");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(gameplay);
        }
    }
}
#endif
