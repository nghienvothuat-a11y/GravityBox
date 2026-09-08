#if UNITY_EDITOR
using System.Collections;
using GravityBox.App;
using GravityBox.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GravityBox.Tests
{
    public sealed class InputIntegrationTests
    {
        private GameBootstrap bootstrap;
        private Mouse mouse;
        private Touchscreen touchscreen;
        private InputSettings.BackgroundBehavior previousBackground;
        private InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousBackground = InputSystem.settings.backgroundBehavior;
            previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Scenes/Gameplay.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            Assert.That(bootstrap.Levels.Session.State, Is.EqualTo(GravityBox.Foundation.SessionState.Active));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            if (touchscreen != null) InputSystem.RemoveDevice(touchscreen);
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
            Scene gameplay = SceneManager.GetActiveScene();
            Scene empty = SceneManager.CreateScene("After input test");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(gameplay);
        }

        [UnityTest]
        public IEnumerator Mouse_DragAndResetButtonWorkWithoutRotatingThroughUI()
        {
            mouse = InputSystem.AddDevice<Mouse>();
            Vector2 start = new Vector2(Screen.width * 0.35f, Screen.height * 0.45f);
            SendMouse(start, false); yield return null;
            SendMouse(start, true); yield return null;
            for (int i = 1; i <= 5; i++)
            {
                SendMouse(start + new Vector2(Screen.width * 0.04f * i, 0), true);
                yield return null;
            }
            SendMouse(start + new Vector2(Screen.width * 0.2f, 0), false);
            yield return new WaitForSeconds(0.25f);
            Assert.That(bootstrap.Levels.DragCount, Is.EqualTo(1));
            Assert.That(Quaternion.Angle(Quaternion.identity, bootstrap.Levels.Current.Rotation.Orientation), Is.GreaterThan(5));

            Button reset = null;
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None)) if (button.name == "Reset") reset = button;
            Assert.That(reset, Is.Not.Null);
            var rect = (RectTransform)reset.transform;
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            SendMouse(point, false); yield return null;
            SendMouse(point, true); yield return null;
            SendMouse(point, false); yield return null;
            Assert.That(bootstrap.Levels.ResetCount, Is.EqualTo(1));
            Assert.That(bootstrap.Levels.DragCount, Is.EqualTo(1));
            Assert.That(Quaternion.Angle(Quaternion.identity, bootstrap.Levels.Current.Rotation.Orientation), Is.LessThan(0.1f));
        }

        [UnityTest]
        public IEnumerator Mouse_DragCompletedBetweenFramesIsNotLost()
        {
            mouse = InputSystem.AddDevice<Mouse>();
            Vector2 start = new Vector2(Screen.width * 0.35f, Screen.height * 0.45f);
            SendMouse(start, false); InputSystem.Update();
            SendMouse(start, true); InputSystem.Update();
            SendMouse(start + new Vector2(Screen.width * 0.2f, 0), false); InputSystem.Update();
            yield return new WaitForSeconds(0.25f);
            Assert.That(bootstrap.Levels.DragDistance, Is.GreaterThan(0.15f));
            Assert.That(Quaternion.Angle(Quaternion.identity, bootstrap.Levels.Current.Rotation.Orientation), Is.GreaterThan(5));
        }

        [UnityTest]
        public IEnumerator Touch_DragsTheBoxWithOneFinger()
        {
            touchscreen = InputSystem.AddDevice<Touchscreen>();
            Vector2 start = new Vector2(Screen.width * 0.4f, Screen.height * 0.45f);
            SendTouch(start, UnityEngine.InputSystem.TouchPhase.Began); yield return null;
            for (int i = 1; i <= 5; i++)
            {
                SendTouch(start + new Vector2(Screen.width * 0.04f * i, 0), UnityEngine.InputSystem.TouchPhase.Moved);
                yield return null;
            }
            SendTouch(start + new Vector2(Screen.width * 0.2f, 0), UnityEngine.InputSystem.TouchPhase.Ended);
            yield return new WaitForSeconds(0.25f);
            Assert.That(bootstrap.Levels.DragCount, Is.EqualTo(1));
            Assert.That(Quaternion.Angle(Quaternion.identity, bootstrap.Levels.Current.Rotation.Orientation), Is.GreaterThan(5));
        }

        private void SendMouse(Vector2 point, bool pressed)
        {
            MouseState state = new MouseState { position = point };
            if (pressed) state = state.WithButton(MouseButton.Left);
            InputSystem.QueueStateEvent(mouse, state);
        }

        [UnityTest]
        public IEnumerator Touch_ReleasePositionContributesFinalMotion()
        {
            touchscreen = InputSystem.AddDevice<Touchscreen>();
            Vector2 start = new Vector2(Screen.width * 0.4f, Screen.height * 0.45f);
            SendTouch(start, UnityEngine.InputSystem.TouchPhase.Began); yield return null;
            // Some very short gestures contain only begin/end with no moved sample.
            SendTouch(start + new Vector2(Screen.width * 0.2f, 0), UnityEngine.InputSystem.TouchPhase.Ended);
            yield return new WaitForSeconds(0.25f);
            Assert.That(bootstrap.Levels.DragDistance, Is.GreaterThan(0.15f));
            Assert.That(Quaternion.Angle(Quaternion.identity, bootstrap.Levels.Current.Rotation.Orientation), Is.GreaterThan(5));
        }
        private void SendTouch(Vector2 point, UnityEngine.InputSystem.TouchPhase phase)
        {
            InputSystem.QueueStateEvent(touchscreen, new TouchState { touchId = 1, phase = phase, position = point });
        }
    }
}
#endif
