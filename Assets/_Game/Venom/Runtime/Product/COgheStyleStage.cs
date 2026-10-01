using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace GravityBox.Venom
{
    /// <summary>The Style screen's stage: an invisible UI target over COghe that owns the pointer from press to release, so a
    /// hold to inject never reaches Home movement, items or the room camera. Only the first finger counts.</summary>
    public sealed class COgheStyleStage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        /// <summary>Pressed at a screen point (true: by a finger, which hides what is under it).</summary>
        public Action<Vector2, bool> Down;
        public Action<Vector2> Move;
        public Action Up;
        private int pointer = int.MinValue;
        public bool Holding => pointer != int.MinValue;

        public void OnPointerDown(PointerEventData e)
        {
            if (Holding) return;   // a second finger is ignored
            pointer = e.pointerId;
            Down?.Invoke(e.position, e is ExtendedPointerEventData x && x.pointerType == UIPointerType.Touch);
        }
        public void OnDrag(PointerEventData e) { if (e.pointerId == pointer) Move?.Invoke(e.position); }
        public void OnPointerUp(PointerEventData e) { if (e.pointerId == pointer) Release(); }
        /// <summary>Ends the hold (release, cancel, lost focus, leaving the screen).</summary>
        public void Release() { if (!Holding) return; pointer = int.MinValue; Up?.Invoke(); }
        private void OnDisable() => Release();
        private void OnApplicationPause(bool paused) { if (paused) Release(); }
    }
}
