using GravityBox.Foundation;
using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        public float FramingRadius = 5.3f;
        public Vector3 ViewDirection = new Vector3(7.5f, 5.2f, -13f);
        private Camera view;
        private LevelManager levels;
        public void Initialize(LevelManager manager) => levels = manager;
        private void Awake() { view = GetComponent<Camera>(); Reframe(Vector3.zero, FramingRadius); }
        private void LateUpdate()
        {
            Vector3 center = Vector3.zero;
            float radius = FramingRadius;
            if (levels != null && levels.Ball != null &&
                (levels.Session.State == SessionState.Completing || levels.Session.State == SessionState.Finished))
            {
                Vector3 ball = levels.Ball.Body.position;
                float excess = Mathf.Max(0, ball.magnitude + 0.6f - FramingRadius);
                center = ball.normalized * excess * 0.5f;
                radius += excess * 0.5f;
            }
            Reframe(center, radius);
        }
        private void Reframe(Vector3 center, float radius)
        {
            view.fieldOfView = 40;
            float vertical = view.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float horizontal = Mathf.Atan(Mathf.Tan(vertical) * view.aspect);
            float angle = Mathf.Min(vertical * 0.6f, horizontal * 0.92f);
            transform.position = center + ViewDirection.normalized * (radius / Mathf.Sin(angle));
            transform.LookAt(center);
        }
    }
}
