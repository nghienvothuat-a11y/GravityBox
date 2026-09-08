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
                Assert.That(levels.Current.Exit.TryCapture(levels.Ball), Is.True);
                yield return new WaitForSeconds(levels.Catalog.CompletionDelay + 0.05f);
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
