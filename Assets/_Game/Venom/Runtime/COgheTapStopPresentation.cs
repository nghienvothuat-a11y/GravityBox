using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Shows the next authored detent; presentation never moves the carriage.</summary>
    public sealed class COgheTapStopPresentation : MonoBehaviour
    {
        public COgheTapRail Task;
        public Transform NextMarker;
        public Vector3 Offset;
        private void LateUpdate()
        {
            if (Task == null || !Task.HasStops || NextMarker == null) return;
            NextMarker.position = Task.Rail.Frame.TransformPoint(Task.Rail.Start +
                Task.Rail.Axis.normalized * Task.Stops[Task.NextStop] + Offset);
        }
    }
}
