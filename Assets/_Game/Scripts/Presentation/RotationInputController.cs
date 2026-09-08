using GravityBox.Foundation;
using GravityBox.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GravityBox.Presentation
{
    public sealed class RotationInputController : MonoBehaviour
    {
        private LevelManager levels;
        private Camera view;
        private GameHud hud;
        private bool dragging;
        private int fingerId = -1;
        private Vector2 previous;
        private InputAction mousePress;
        private bool queuedPress, queuedRelease;
        private Vector2 pressPosition, releasePosition;

        public void Initialize(LevelManager manager, Camera camera, GameHud overlay)
        {
            levels = manager;
            view = camera;
            hud = overlay;
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            mousePress = new InputAction("Box drag", InputActionType.Button, "<Mouse>/leftButton");
            mousePress.started += OnMousePressed;
            mousePress.canceled += OnMouseReleased;
            mousePress.Enable();
        }
        private void OnDisable()
        {
            End();
            mousePress?.Dispose();
            mousePress = null;
            EnhancedTouchSupport.Disable();
        }

        private void OnMousePressed(InputAction.CallbackContext context)
        {
            if (!(context.control?.device is Mouse mouse)) return;
            queuedPress = true;
            pressPosition = mouse.position.ReadValue();
        }

        private void OnMouseReleased(InputAction.CallbackContext context)
        {
            if (!(context.control?.device is Mouse mouse)) return;
            queuedRelease = true;
            releasePosition = mouse.position.ReadValue();
        }

        private void Update()
        {
            if (levels == null || levels.Current == null) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.rKey.wasPressedThisFrame) { End(); levels.ResetLevel(); }
                if (keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame) levels.TogglePause();
                if (keyboard.dKey.wasPressedThisFrame && Debug.isDebugBuild) hud.ToggleDiagnostics();
                if (keyboard.f12Key.wasPressedThisFrame && Debug.isDebugBuild)
                    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.persistentDataPath, "gravity-box.png"));
            }
            if (levels.Session.State != SessionState.Active || hud.ModalOpen) { End(); return; }

            var touches = Touch.activeTouches;
            if (touches.Count > 0 || fingerId >= 0)
            {
                bool found = false;
                for (int i = 0; i < touches.Count; i++)
                {
                    Touch touch = touches[i];
                    if (fingerId < 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        if (hud.BlocksRotation(touch.screenPosition)) continue;
                        fingerId = touch.finger.index;
                        Begin(touch.screenPosition);
                    }
                    if (touch.finger.index != fingerId) continue;
                    found = true;
                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
                    {
                        Move(touch.screenPosition);
                        End();
                    }
                    else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) End();
                    else Move(touch.screenPosition);
                    break;
                }
                if (!found) End();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 position = mouse.position.ReadValue();
            // Keep press/release positions from events: a fast drag may begin and end
            // between rendered frames, and polling isPressed alone would drop it.
            if (queuedPress && !hud.BlocksRotation(pressPosition)) Begin(pressPosition);
            queuedPress = false;
            // A cursor warp after release belongs to hover, not to the completed drag.
            if (queuedRelease) Move(releasePosition);
            else if (mouse.leftButton.isPressed) Move(position);
            if (queuedRelease) End();
        }

        private void Begin(Vector2 position)
        {
            dragging = true;
            previous = position;
            levels.Current.Rotation.BeginDrag();
            levels.RecordDrag(0, true);
            hud.NotifyDrag();
        }

        private void Move(Vector2 position)
        {
            if (!dragging) return;
            Vector2 delta = (position - previous) / Mathf.Max(1, Mathf.Min(Screen.width, Screen.height));
            previous = position;
            levels.Current.Rotation.Drag(delta, view.transform.up, view.transform.right);
            levels.RecordDrag(delta.magnitude, false);
        }

        private void End()
        {
            if (dragging && levels != null && levels.Current != null) levels.Current.Rotation.EndDrag();
            dragging = false;
            fingerId = -1;
            queuedPress = queuedRelease = false;
        }

        private void OnApplicationFocus(bool focused) { if (!focused) End(); }
    }
}
