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
        private Vector2 stickOrigin, stickPosition;
        public bool Holding => dragging;
        public Vector2 StickOrigin => stickOrigin;
        public Vector2 StickPosition => stickPosition;
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
                if (keys.digit1Key.wasPressedThisFrame) { End(); level.LoadExperiment(1); return; }
                if (keys.digit2Key.wasPressedThisFrame) { End(); level.LoadExperiment(2); return; }
                if (keys.digit3Key.wasPressedThisFrame) { End(); level.LoadExperiment(3); return; }
                if (keys.tabKey.wasPressedThisFrame) level.Locomotion?.SelectNext();
            }
            if (!level.CanControl) { End(); return; }
            if (level.DirectControl && !dragging)
            {
                Vector2 direction = keys == null ? Vector2.zero : new Vector2(
                    (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1 : 0)-(keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1 : 0),
                    (keys.wKey.isPressed || keys.upArrowKey.isPressed ? 1 : 0)-(keys.sKey.isPressed || keys.downArrowKey.isPressed ? 1 : 0));
                level.Locomotion.SetInput(ScreenDirection(Vector2.ClampMagnitude(direction,1)));
            }
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
        private bool InPlayArea(Vector2 p)
        {
            float scale = Screen.width/540f;
            return p.y > (level.DirectControl ? 280 : 220)*scale && p.y < Screen.height-200*scale;
        }
        private void Begin(Vector2 p)
        {
            dragging = true; previous = stickOrigin = stickPosition = p;
            if (level.DirectControl) { level.Locomotion.SelectAt(p); level.Locomotion.SetInput(Vector3.zero); }
            else level.Rotation.BeginDrag();
        }
        private void Move(Vector2 p)
        {
            if (!dragging) return;
            if (level.DirectControl)
            {
                stickPosition = p;
                float radius = Mathf.Min(Screen.width,Screen.height)*.12f;
                Vector2 displacement = (p-stickOrigin)/Mathf.Max(1,radius);
                float length = displacement.magnitude;
                Vector2 command = length < .12f ? Vector2.zero : displacement.normalized*Mathf.Clamp01((length-.12f)/.88f);
                level.Locomotion.SetInput(ScreenDirection(command)); return;
            }
            Vector2 delta = (p-previous)/Mathf.Max(1,Mathf.Min(Screen.width,Screen.height)); previous=p;
            level.Rotation.Drag(delta, level.View.transform.up, level.View.transform.right);
        }
        private Vector3 ScreenDirection(Vector2 direction)
        {
            Vector3 right = Vector3.ProjectOnPlane(level.View.transform.right,Vector3.up).normalized;
            Vector3 forward = Vector3.Cross(right,Vector3.up).normalized;
            return right*direction.x+forward*direction.y;
        }
        private void End()
        {
            if (level != null) { if (dragging) level.Rotation.EndDrag(); level.Locomotion?.SetInput(Vector3.zero); }
            dragging = pressed = released = false; finger = -1;
        }
        private void OnApplicationFocus(bool focused) { if (!focused) End(); }
    }
}
