using UnityEngine;

namespace GravityBox.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        public float FramingRadius = 5.3f;
        public Vector3 ViewDirection = new Vector3(7.5f, 5.2f, -13f);
        private Camera view;
        private float lastAspect;

        private void Awake() { view = GetComponent<Camera>(); Reframe(); }
        private void LateUpdate() { if (Mathf.Abs(view.aspect - lastAspect) > 0.001f) Reframe(); }
        private void Reframe()
        {
            lastAspect = view.aspect;
            view.fieldOfView = 40;
            float vertical = view.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float horizontal = Mathf.Atan(Mathf.Tan(vertical) * view.aspect);
            float angle = Mathf.Min(vertical * 0.6f, horizontal * 0.92f);
            float distance = FramingRadius / Mathf.Sin(angle);
            transform.position = ViewDirection.normalized * distance;
            transform.LookAt(Vector3.zero);
        }
    }
}
