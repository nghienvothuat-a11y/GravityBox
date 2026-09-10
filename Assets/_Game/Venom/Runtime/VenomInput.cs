using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
namespace GravityBox.Venom
{
    public sealed class VenomInput : MonoBehaviour
    {
        private VenomLevelController level; private Vector2 previous; private bool dragging; private int finger = -1;
        private InputAction press; private bool pressed, released; private Vector2 beganAt, endedAt;
        private Vector2 stickOrigin, stickPosition;
        private bool rotating, requireTouchRelease;
        private int climbTouches;
        private bool tapPending, tapDragged;
        private Vector2 tapStart;
        private int guidanceTouches;
        private bool guidanceRelease;
        private struct PointerEvent { public Vector2 Position; public int Phase; }
        private readonly Queue<PointerEvent> mouseEvents=new Queue<PointerEvent>();
        private bool eventMouseDown;
        private bool pointerDiagnostics;
        public bool Rotating => rotating;
        public bool Holding => dragging;
        public Vector2 StickOrigin => stickOrigin;
        public Vector2 StickPosition => stickPosition;
        public void Initialize(VenomLevelController controller) => level = controller;
        private void OnEnable()
        {
            EnhancedTouchSupport.Enable(); press = new InputAction("Matter box drag", InputActionType.Button, "<Mouse>/leftButton");
            press.started += c => { if(c.control.device is Mouse mouse){pressed = true; beganAt = mouse.position.ReadValue();} };
            press.canceled += c => { if(c.control.device is Mouse mouse){released = true; endedAt = mouse.position.ReadValue();} }; press.Enable();
            InputSystem.onEvent+=ObserveMouse;
        }
        private void OnDisable() { InputSystem.onEvent-=ObserveMouse;End(); press?.Dispose(); EnhancedTouchSupport.Disable(); }
        private void ObserveMouse(InputEventPtr inputEvent,InputDevice device)
        {
            if(level==null||level.Guidance==null||!(device is Mouse mouse)||!level.CanControl)return;
            if(!inputEvent.IsA<StateEvent>()&&!inputEvent.IsA<DeltaStateEvent>())return;
            Vector2 p=mouse.position.ReadValueFromEvent(inputEvent,out var position)?position:mouse.position.ReadValue();
            bool down=mouse.leftButton.ReadValueFromEvent(inputEvent,out var value)?value>.5f:eventMouseDown;
            if(pointerDiagnostics)Debug.Log($"VENOM POINTER t={inputEvent.time:F4} down={down} previous={eventMouseDown} event={p} current={mouse.position.ReadValue()} delta={mouse.delta.ReadValueFromEvent(inputEvent)}");
            if(down&&!eventMouseDown)mouseEvents.Enqueue(new PointerEvent{Position=p,Phase=0});
            if(down||eventMouseDown)mouseEvents.Enqueue(new PointerEvent{Position=p,Phase=1});
            if(!down&&eventMouseDown)mouseEvents.Enqueue(new PointerEvent{Position=p,Phase=2});
            eventMouseDown=down;
        }
        private void Update()
        {
            if (level == null) return;
            var keys = Keyboard.current;
            if (keys != null)
            {
                if (keys.rKey.wasPressedThisFrame) { End(); level.ResetExperiment(); }
                if (keys.pKey.wasPressedThisFrame || keys.escapeKey.wasPressedThisFrame) { End(); level.TogglePause(); }
                if (keys.f12Key.wasPressedThisFrame) ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.persistentDataPath,"venom.png"));
                if (Debug.isDebugBuild&&keys.f9Key.wasPressedThisFrame)pointerDiagnostics=!pointerDiagnostics;
                if (keys.digit1Key.wasPressedThisFrame) { End(); level.LoadExperiment(1); return; }
                if (keys.digit2Key.wasPressedThisFrame) { End(); level.LoadExperiment(2); return; }
                if (keys.digit3Key.wasPressedThisFrame) { End(); level.LoadExperiment(3); return; }
                if (keys.digit5Key.wasPressedThisFrame) { End(); level.LoadExperiment(5); return; }
                if (keys.digit4Key.wasPressedThisFrame) { End(); level.LoadExperiment(4); return; }
                if (keys.digit7Key.wasPressedThisFrame) { End(); level.LoadExperiment(7); return; }
                if (keys.digit8Key.wasPressedThisFrame) { End(); level.LoadExperiment(8); return; }
                if (keys.zKey.wasPressedThisFrame && level.WallCrawl) level.ToggleZoom();
                if (keys.tabKey.wasPressedThisFrame) level.Locomotion?.SelectNext();
            }
            if (!level.CanControl) { End(); return; }
            if(level.Guidance!=null){UpdateGuidance();pressed=released=false;return;}
            if(level.WallCrawl){UpdateClimbing(keys);pressed=released=false;return;}
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
            float scale = VenomCameraFraming.UiScale(level,Screen.width,Screen.height);
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
            return level.WallCrawl?level.Climbing.ScreenDirection(direction):VenomCameraFraming.ScreenToFloor(level.View,direction);
        }
        private void UpdateGuidance()
        {
            int count=0;Vector2 first=Vector2.zero,second=Vector2.zero;
            bool ended=false;Vector2 endPoint=Vector2.zero;
            foreach(var touch in Touch.activeTouches)
            {
                if(touch.phase==UnityEngine.InputSystem.TouchPhase.Canceled){CancelGesture();guidanceRelease=true;continue;}
                if(touch.phase==UnityEngine.InputSystem.TouchPhase.Ended){ended=true;endPoint=touch.screenPosition;continue;}
                if(count==0)first=touch.screenPosition;else if(count==1)second=touch.screenPosition;count++;
            }
            if(count>0 || guidanceTouches>0 || guidanceRelease)
            {
                mouseEvents.Clear();
                if(count==0&&ended&&guidanceTouches==1&&!guidanceRelease)MoveTap(endPoint);
                ApplyGuidanceTouch(count,first,second);return;
            }
            var mouse=Mouse.current;if(mouse==null)return;
            Vector2 p=mouse.position.ReadValue();
            if(mouse.rightButton.isPressed)
            {
                if(mouse.rightButton.wasPressedThisFrame && InPlayArea(p)){CancelGesture();BeginRotation(p);}
                if(rotating)Rotate(p);return;
            }
            if(rotating && !tapPending)End();
            // Preserve every phase even when an entire drag and cursor restoration
            // arrive between two rendered frames. Polling only the endpoint can
            // collapse such a gesture into a tap.
            while(mouseEvents.Count>0)
            {
                var sample=mouseEvents.Dequeue();
                if(sample.Phase==0)BeginTap(sample.Position);
                else if(sample.Phase==1)MoveTap(sample.Position);
                else FinishTap();
            }
        }
        public void BeginTap(Vector2 p)
        {
            if(!level.CanControl || !InPlayArea(p))return;
            tapPending=true;tapDragged=false;tapStart=previous=p;
        }
        public void MoveTap(Vector2 p)
        {
            if(!tapPending)return;
            float slop=10*VenomCameraFraming.UiScale(level,Screen.width,Screen.height);
            if(!tapDragged && (p-tapStart).magnitude>slop){tapDragged=true;BeginRotation(tapStart);}
            if(tapDragged)Rotate(p);else previous=p;
        }
        public void FinishTap()
        {
            if(tapPending && !tapDragged && level.CanControl)level.Guidance.Touch(previous);
            tapPending=false;tapDragged=false;if(rotating){level.Rotation.EndDrag();rotating=false;}
        }
        public void ApplyGuidanceTouch(int count,Vector2 first,Vector2 second)
        {
            count=Mathf.Min(2,count);
            if(count==0)
            {
                if(!guidanceRelease)FinishTap();else tapPending=false;
                if(rotating){level.Rotation.EndDrag();rotating=false;}
                guidanceTouches=0;guidanceRelease=false;return;
            }
            if(guidanceRelease){guidanceTouches=count;return;}
            if(guidanceTouches==2 && count==1){tapPending=false;guidanceRelease=true;guidanceTouches=count;return;}
            if(count!=guidanceTouches)
            {
                tapPending=false;
                if(!InPlayArea(first) || count==2 && !InPlayArea(second)){guidanceRelease=true;guidanceTouches=count;return;}
                if(count==1)BeginTap(first);else BeginRotation((first+second)*.5f);
            }
            guidanceTouches=count;
            if(count==1)MoveTap(first);else Rotate((first+second)*.5f);
        }
        private void UpdateClimbing(Keyboard keys)
        {
            int count=0;Vector2 first=Vector2.zero,second=Vector2.zero;
            foreach(var touch in Touch.activeTouches)
            {
                if(touch.phase==UnityEngine.InputSystem.TouchPhase.Ended || touch.phase==UnityEngine.InputSystem.TouchPhase.Canceled)continue;
                if(count==0)first=touch.screenPosition;else if(count==1)second=touch.screenPosition;
                count++;
            }
            if(count>0 || climbTouches>0 || requireTouchRelease)
            {
                ApplyClimbTouch(count,first,second);return;
            }
            var mouse=Mouse.current;
            if(mouse!=null && mouse.rightButton.isPressed)
            {
                Vector2 p=mouse.position.ReadValue();
                if(!rotating && mouse.rightButton.wasPressedThisFrame && InPlayArea(p)){End();BeginRotation(p);}
                if(rotating)Rotate(p);
            }
            else
            {
                if(rotating)End();
                if(pressed && InPlayArea(beganAt))Begin(beganAt);
                if(released)End();else if(mouse!=null && mouse.leftButton.isPressed)Move(mouse.position.ReadValue());
            }
            if(!dragging)
            {
                Vector2 keyDirection=keys==null?Vector2.zero:new Vector2(
                    (keys.dKey.isPressed||keys.rightArrowKey.isPressed?1:0)-(keys.aKey.isPressed||keys.leftArrowKey.isPressed?1:0),
                    (keys.wKey.isPressed||keys.upArrowKey.isPressed?1:0)-(keys.sKey.isPressed||keys.downArrowKey.isPressed?1:0));
                level.Locomotion.SetInput(ScreenDirection(Vector2.ClampMagnitude(keyDirection,1)));
            }
        }
        // Kept independent of the device API so finger-count transitions can be
        // replayed in tests. Lifting one finger after a rotation cannot issue a crawl.
        public void ApplyClimbTouch(int count,Vector2 first,Vector2 second)
        {
            count=Mathf.Min(count,2);
            if(count==0){End();climbTouches=0;requireTouchRelease=false;return;}
            if(requireTouchRelease){climbTouches=count;return;}
            if(climbTouches==2 && count==1){End();climbTouches=1;requireTouchRelease=true;return;}
            if(count!=climbTouches)
            {
                End();
                if(!InPlayArea(first) || count==2 && !InPlayArea(second)){requireTouchRelease=true;climbTouches=count;return;}
                if(count==1)Begin(first);else BeginRotation((first+second)*.5f);
            }
            climbTouches=count;
            if(count==1)Move(first);else Rotate((first+second)*.5f);
        }
        private void BeginRotation(Vector2 p){rotating=true;previous=p;level.Locomotion.SetInput(Vector3.zero);level.Rotation.BeginDrag();}
        private void Rotate(Vector2 p)
        {
            Vector2 delta=(p-previous)/Mathf.Max(1,Mathf.Min(Screen.width,Screen.height));previous=p;
            level.Rotation.Drag(delta,level.View.transform.up,level.View.transform.right);
        }
        public void CancelGesture()=>End();
        private void End()
        {
            if (level != null) { if (dragging || rotating) level.Rotation.EndDrag(); level.Locomotion?.SetInput(Vector3.zero); level.Climbing?.ScreenDirection(Vector2.zero); }
            dragging = rotating = pressed = released = false; finger = -1;climbTouches=0;requireTouchRelease=false;
            tapPending=tapDragged=guidanceRelease=false;guidanceTouches=0;
            mouseEvents.Clear();eventMouseDown=false;
        }
        private void OnApplicationFocus(bool focused) { if (!focused) End(); }
    }
}
