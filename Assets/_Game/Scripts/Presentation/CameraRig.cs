using GravityBox.Foundation;
using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        public float FramingRadius = 0.23f;
        public Vector3 ViewDirection = new Vector3(6, 11, -12);
        private Camera view;
        private LevelManager levels;
        public void Initialize(LevelManager manager)
        {
            levels = manager;
            view = GetComponent<Camera>();
            LateUpdate();
        }
        private void Awake() { view = GetComponent<Camera>(); Reframe(Vector3.zero, FramingRadius); }
        private void LateUpdate()
        {
            Vector3 center = Vector3.zero;
            // Content is authored in metres. Framing must follow its actual size,
            // including near clipping, rather than the old six-metre prototype.
            float radius = levels != null && levels.Current != null
                ? levels.Current.BoundsHalfExtent * 1.05f
                : FramingRadius;
            radius = Mathf.Max(radius, 0.001f);
            if (levels != null && levels.Ball != null &&
                (levels.Session.State == SessionState.Completing || levels.Session.State == SessionState.Finished))
            {
                Vector3 ball = levels.Current.Exit.LastEscapedBall != null ? levels.Current.Exit.LastEscapedBall.Body.position : levels.Ball.Body.position;
                float margin = levels.Ball.Profile.Radius * 2;
                // Show the complete escape, then keep the apparatus readable while
                // the ball continues its real trajectory beyond the framing area.
                float excess = Mathf.Clamp(ball.magnitude + margin - radius, 0, radius * 2);
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
            float distance = radius / Mathf.Sin(angle);
            view.nearClipPlane = Mathf.Max(0.0001f, radius * 0.02f);
            view.farClipPlane = Mathf.Max(1, distance + radius * 30);
            transform.position = center + ViewDirection.normalized * distance;
            transform.LookAt(center);
        }
    }
}
