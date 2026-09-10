using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
namespace GravityBox.Venom
{
    public sealed class VenomInput : MonoBehaviour
    {
        private VenomLevelController level; private Vector2 previous; private bool dragging; private int finger = -1;
        private InputAction press; private bool pressed, released; private Vector2 beganAt, endedAt;
        public void Initialize(VenomLevelController controller) => level = controller;
        private void OnEnable()
        {
            EnhancedTouchSupport.Enable(); press = new InputAction("Matter box drag", InputActionType.Button, "<Mouse>/leftButton");
            press.started += c => { pressed = true; beganAt = Mouse.current.position.ReadValue(); };
            press.canceled += c => { released = true; endedAt = Mouse.current.position.ReadValue(); }; press.Enable();
        }
        private void OnDisable() { End(); press?.Dispose(); EnhancedTouchSupport.Disable(); }
        private void Update()
        {
            if (level == null) return;
            var keys = Keyboard.current;
            if (keys != null)
            {
                if (keys.rKey.wasPressedThisFrame) { End(); level.ResetExperiment(); }
                if (keys.pKey.wasPressedThisFrame || keys.escapeKey.wasPressedThisFrame) { End(); level.TogglePause(); }
                if (keys.f12Key.wasPressedThisFrame) ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.persistentDataPath,"venom.png"));
            }
            if (!level.Rotation.InputEnabled) { End(); return; }
            if (Touch.activeTouches.Count > 0 || finger >= 0)
            {
                bool found = false;
                foreach (var touch in Touch.activeTouches)
                {
                    if (finger < 0 && touch.phase == UnityEngine.InputSystem.TouchPhase.Began && InPlayArea(touch.screenPosition))
                    { finger = touch.finger.index; Begin(touch.screenPosition); }
                    if (touch.finger.index != finger) continue;
                    found = true; Move(touch.screenPosition);
                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) End();
                    break;
                }
                if (!found) End(); return;
            }
            if (pressed && InPlayArea(beganAt)) Begin(beganAt); pressed = false;
            if (released) { Move(endedAt); End(); }
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed) Move(Mouse.current.position.ReadValue());
        }
        private static bool InPlayArea(Vector2 p) => p.y > Screen.height*.20f && p.y < Screen.height*.78f;
        private void Begin(Vector2 p) { dragging = true; previous = p; level.Rotation.BeginDrag(); }
        private void Move(Vector2 p)
        {
            if (!dragging) return;
            Vector2 delta = (p-previous)/Mathf.Max(1,Mathf.Min(Screen.width,Screen.height)); previous=p;
            level.Rotation.Drag(delta, level.View.transform.up, level.View.transform.right);
        }
        private void End() { if (dragging && level != null) level.Rotation.EndDrag(); dragging = pressed = released = false; finger = -1; }
        private void OnApplicationFocus(bool focused) { if (!focused) End(); }
    }
}
