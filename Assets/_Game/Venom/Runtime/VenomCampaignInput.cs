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

        private void ResetPointerInput()
        {
            mouseSamples.Clear();eventMouseDown=Mouse.current!=null&&Mouse.current.leftButton.isPressed;
            mouseNeedsRelease=eventMouseDown;mouseOwnsPointer=false;
            pointerDown=pointerMoved=false;touchFinger=-1;
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
            if(Owner==null||!Owner.CanControl||Touch.activeTouches.Count>0||touchFinger>=0)
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
    }
}
