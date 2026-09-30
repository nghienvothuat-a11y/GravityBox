using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GravityBox.Venom
{
    public sealed partial class VenomCampaign
    {
        private struct MouseSample { public Vector2 Position; public int Phase; }
        private readonly Queue<MouseSample> mouseSamples=new Queue<MouseSample>();
        private bool eventMouseDown,mouseNeedsRelease,mouseOwnsPointer;

        private void OnEnable()
        {ResetPointerInput();InputSystem.onEvent+=ObserveCampaignMouse;}
        private void OnDisable()
        {InputSystem.onEvent-=ObserveCampaignMouse;ResetPointerInput();}
        private void OnApplicationFocus(bool focused)
        {if(!focused)ResetPointerInput();}

        private void OnApplicationPause(bool paused){if(paused)ResetPointerInput();}

        private void ResetPointerInput()
        {
            mouseSamples.Clear();eventMouseDown=Mouse.current!=null&&Mouse.current.leftButton.isPressed;
            mouseNeedsRelease=eventMouseDown;mouseOwnsPointer=false;
            pointerDown=pointerMoved=false;touchFinger=-1;
            viewTouchSuppressed=true;viewNeedAllReleased=true;viewPinchDistance=0;viewGestureBlocked=false;
            if(Owner!=null&&Owner.Rotation!=null)Owner.Rotation.EndDrag();
        }
        private void DiscardMouseInputForTouch()
        {
            mouseSamples.Clear();eventMouseDown=Mouse.current!=null&&Mouse.current.leftButton.isPressed;
            mouseNeedsRelease=eventMouseDown;
            if(mouseOwnsPointer){pointerDown=pointerMoved=false;Owner.Rotation.EndDrag();}
            mouseOwnsPointer=false;
        }
        private void ObserveCampaignMouse(InputEventPtr inputEvent,InputDevice device)
        {
            if(!(device is Mouse mouse)||(!inputEvent.IsA<StateEvent>()&&!inputEvent.IsA<DeltaStateEvent>()))return;
            Vector2 p=mouse.position.ReadValueFromEvent(inputEvent,out var position)?position:mouse.position.ReadValue();
            bool down=mouse.leftButton.ReadValueFromEvent(inputEvent,out var value)?value>.5f:eventMouseDown;
            if(Owner==null||!Owner.CanControl||InputLocked||ProductUI!=null&&ProductUI.BlockWorldInput||Touch.activeTouches.Count>0||touchFinger>=0)
            {mouseSamples.Clear();eventMouseDown=down;mouseNeedsRelease=down;return;}
            if(mouseNeedsRelease){eventMouseDown=down;if(!down)mouseNeedsRelease=false;return;}
            if(down&&!eventMouseDown)mouseSamples.Enqueue(new MouseSample{Position=p,Phase=0});
            if(down||eventMouseDown)mouseSamples.Enqueue(new MouseSample{Position=p,Phase=1});
            if(!down&&eventMouseDown)mouseSamples.Enqueue(new MouseSample{Position=p,Phase=2});
            eventMouseDown=down;
        }
        private void ConsumeMouseInput()
        {
            // An entire drag, release and cursor restore can precede one Update.
            // Replay event positions; the device's final position loses that path.
            while(mouseSamples.Count>0)
            {
                var sample=mouseSamples.Dequeue();
                if(sample.Phase==0){BeginPointer(sample.Position);mouseOwnsPointer=pointerDown;}
                else if(sample.Phase==1)MovePointer(sample.Position);
                else{EndPointer(sample.Position);mouseOwnsPointer=false;}
            }
        }

        // A pinch owns the whole contact sequence, including the remaining finger.
        private bool viewTouchSuppressed,viewNeedAllReleased;
        private bool viewGestureBlocked;
        private float viewPinchDistance;
        private bool ConsumeViewTouches()
        {
            var touches=Touch.activeTouches;
            int live=0;Touch first=default,second=default;
            foreach(var t in touches)
                if(t.phase!=UnityEngine.InputSystem.TouchPhase.Ended&&t.phase!=UnityEngine.InputSystem.TouchPhase.Canceled)
                {if(live==0)first=t;else if(live==1)second=t;live++;}
            if(touches.Count==0)
            {viewTouchSuppressed=false;viewNeedAllReleased=false;viewGestureBlocked=false;viewPinchDistance=0;touchFinger=-1;return false;}
            DiscardMouseInputForTouch();
            if(viewNeedAllReleased)return true;
            if(live>2)viewGestureBlocked=true;
            if(live>=2&&viewPinchDistance==0&&(!PlayArea(first.screenPosition)||!PlayArea(second.screenPosition)))viewGestureBlocked=true;
            if(live==1&&first.phase==UnityEngine.InputSystem.TouchPhase.Began&&!PlayArea(first.screenPosition))viewGestureBlocked=true;
            if(viewGestureBlocked){pointerDown=false;touchFinger=-1;viewTouchSuppressed=true;viewPinchDistance=0;return true;}
            if(live>=2)
            {
                pointerDown=pointerMoved=false;touchFinger=-1;Owner.Rotation.EndDrag();
                viewTouchSuppressed=true;
                float distance=Vector2.Distance(first.screenPosition,second.screenPosition);
                if(viewPinchDistance>0&&PlayArea(first.screenPosition)&&PlayArea(second.screenPosition))CameraRig.PinchAt(distance/viewPinchDistance,(first.screenPosition+second.screenPosition)*.5f);
                viewPinchDistance=Mathf.Max(1,distance);return true;
            }
            if(viewTouchSuppressed){if(live==0){viewTouchSuppressed=false;viewPinchDistance=0;}return true;}
            foreach(var t in touches)
            {
                if(touchFinger<0&&t.phase==UnityEngine.InputSystem.TouchPhase.Began){touchFinger=t.finger.index;BeginPointer(t.screenPosition);}
                if(t.finger.index!=touchFinger)continue;
                if(t.phase==UnityEngine.InputSystem.TouchPhase.Ended){EndPointer(t.screenPosition);touchFinger=-1;}
                else if(t.phase==UnityEngine.InputSystem.TouchPhase.Canceled){pointerDown=false;touchFinger=-1;}
                else MovePointer(t.screenPosition);
                break;
            }
            return true;
        }
    }
}
