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
        public IEnumerator Mouse_CursorRestoredAfterReleasePreservesCompletedDrag()
        {
            mouse = InputSystem.AddDevice<Mouse>();
            Vector2 start = new Vector2(Screen.width * 0.35f, Screen.height * 0.45f);
            Vector2 end = start + new Vector2(Screen.width * 0.2f, 0);
            // A fast native gesture can release and restore the cursor before the
            // next rendered frame. Keep the complete event ordering in one update.
            SendMouse(start, false);
            SendMouse(start, true);
            SendMouse(end, true);
            SendMouse(end, false);
            SendMouse(start, false);
            InputSystem.Update();
            Assert.That(mouse.position.ReadValue(), Is.EqualTo(start));
            yield return new WaitForSeconds(0.25f);
            float expectedDistance = Vector2.Distance(start, end) / Mathf.Max(1, Mathf.Min(Screen.width, Screen.height));
            Assert.That(bootstrap.Levels.DragCount, Is.EqualTo(1));
            Assert.That(bootstrap.Levels.DragDistance, Is.EqualTo(expectedDistance).Within(0.001f),
                "The release contributes its endpoint; the later hover movement contributes no drag.");
            Assert.That(Quaternion.Angle(Quaternion.identity, bootstrap.Levels.Current.Rotation.Orientation), Is.GreaterThan(5));
        }

        [UnityTest]
        public IEnumerator ShapeSelector_ShowsEveryNameAndHintAndLoadsTheLastBoxWithoutRotating()
        {
            mouse = InputSystem.AddDevice<Mouse>();
            GameHud hud = Object.FindFirstObjectByType<GameHud>();
            Button open = null;
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                if (button.name == "Experiments") open = button;
            Assert.That(open, Is.Not.Null);
            yield return Click(open);
            Assert.That(hud.ModalOpen, Is.True);
            Assert.That(bootstrap.Levels.Session.State, Is.EqualTo(GravityBox.Foundation.SessionState.Paused));
            ScrollRect scroll = hud.GetComponentInChildren<ScrollRect>();
            Assert.That(scroll, Is.Not.Null);
            Assert.That(scroll.viewport.GetComponent<RectMask2D>(), Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            Assert.That(scroll.content.childCount, Is.EqualTo(bootstrap.Levels.Catalog.Levels.Length));
            foreach (var definition in bootstrap.Levels.Catalog.Levels)
            {
                Transform card = scroll.content.Find("Select " + definition.Id);
                Assert.That(card.GetComponent<Button>(), Is.Not.Null, definition.DisplayName);
                Text name = card.Find("Name " + definition.Id).GetComponent<Text>();
                Text description = card.Find("Hint " + definition.Id).GetComponent<Text>();
                Assert.That(name.text, Is.EqualTo(definition.DisplayName));
                Assert.That(description.text, Is.EqualTo(definition.TeachingHint));
                Assert.That(description.preferredHeight, Is.LessThanOrEqualTo(description.rectTransform.rect.height + 1),
                    definition.DisplayName + " teaching hint must fit its card.");
            }

            Vector2 dragStart = RectTransformUtility.WorldToScreenPoint(null, scroll.viewport.TransformPoint(scroll.viewport.rect.center));
            SendMouse(dragStart, false); yield return null;
            SendMouse(dragStart, true); yield return null;
            SendMouse(dragStart + Vector2.up * Screen.height * 0.1f, true); yield return null;
            SendMouse(dragStart + Vector2.up * Screen.height * 0.1f, false); yield return null;
            Assert.That(bootstrap.Levels.DragCount, Is.Zero, "Swiping the selector must not turn the physical box.");

            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 0;
            Canvas.ForceUpdateCanvases();
            yield return null;
            int last = bootstrap.Levels.Catalog.Levels.Length - 1;
            Button lastCard = scroll.content.Find("Select " + bootstrap.Levels.Catalog.Levels[last].Id).GetComponent<Button>();
            var lastRect = (RectTransform)lastCard.transform;
            Vector2 lastCenter = RectTransformUtility.WorldToScreenPoint(null, lastRect.TransformPoint(lastRect.rect.center));
            Assert.That(RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport, lastCenter), Is.True,
                "The last shape remains reachable inside the clipped viewport.");
            yield return Click(lastCard);
            Assert.That(bootstrap.Levels.Index, Is.EqualTo(last));
            Assert.That(hud.ModalOpen, Is.False);
            Assert.That(bootstrap.Levels.Session.State, Is.EqualTo(GravityBox.Foundation.SessionState.Active));
            Assert.That(bootstrap.Levels.DragCount, Is.Zero);
            Assert.That(Quaternion.Angle(Quaternion.identity, bootstrap.Levels.Current.Rotation.Orientation), Is.LessThan(0.1f));

            // Keep the layered view regression independent of the last catalog entry.
            // Select it through the same clipped selector instead of loading it directly.
            yield return Click(open);
            scroll = hud.GetComponentInChildren<ScrollRect>();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 0;
            Canvas.ForceUpdateCanvases();
            yield return null;
            GravityBox.Gameplay.LevelDefinition layered = null;
            foreach (var definition in bootstrap.Levels.Catalog.Levels)
                if (definition.Shape == GravityBox.Gameplay.ContainerShape.LayeredMaze) layered = definition;
            Assert.That(layered, Is.Not.Null);
            Button layeredCard = scroll.content.Find("Select " + layered.Id).GetComponent<Button>();
            // With more rows, the layered card need not be visible at the bottom.
            // Bring the actual card into the viewport before sending real mouse input.
            var layeredRect = (RectTransform)layeredCard.transform;
            float cardY = scroll.viewport.InverseTransformPoint(layeredRect.TransformPoint(layeredRect.rect.center)).y;
            scroll.content.anchoredPosition += Vector2.up * (scroll.viewport.rect.center.y - cardY);
            Canvas.ForceUpdateCanvases();
            yield return null;
            Vector2 layeredCenter = RectTransformUtility.WorldToScreenPoint(null, layeredRect.TransformPoint(layeredRect.rect.center));
            Assert.That(RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport, layeredCenter), Is.True);
            yield return Click(layeredCard);
            Assert.That(bootstrap.Levels.Definition.Shape, Is.EqualTo(GravityBox.Gameplay.ContainerShape.LayeredMaze));
            Assert.That(bootstrap.Levels.DragCount, Is.Zero);
            MazeLayerView layerView = bootstrap.Levels.Current.GetComponent<MazeLayerView>();
            Assert.That(layerView, Is.Not.Null, "The layered maze must expose its physical deck view.");
            Assert.That(layerView.LayerCount, Is.EqualTo(3));
            Assert.That(layerView.Overview, Is.False);
            Button layerButton = null;
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                if (button.name == "Status") layerButton = button;
            Assert.That(layerButton, Is.Not.Null);
            Assert.That(layerButton.interactable, Is.True);
            yield return Click(layerButton);
            Assert.That(layerView.Overview, Is.True);
            foreach (Collider collider in bootstrap.Levels.Current.GetComponentsInChildren<Collider>())
                Assert.That(collider.enabled, Is.True, "Overview must preserve every physical surface.");
            Assert.That(bootstrap.Levels.Ball.Body.isKinematic, Is.False);
            Assert.That(bootstrap.Levels.DragCount, Is.Zero);
            yield return Click(layerButton);
            Assert.That(layerView.Overview, Is.False);
        }

        private IEnumerator Click(Button button)
        {
            var rect = (RectTransform)button.transform;
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            SendMouse(point, false); yield return null;
            SendMouse(point, true); yield return null;
            SendMouse(point, false); yield return null;
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
