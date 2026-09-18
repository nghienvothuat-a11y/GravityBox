using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Visual cable paths follow the authored handle, drum and shutter. No puzzle state or force.</summary>
    public sealed class COgheWinchPresentation : MonoBehaviour
    {
        public COgheSequentialWinch Sequence;
        public Material Material;
        private LineRenderer input, output;
        private Vector3 lastHandle, lastDoor;

        private void Start()
        {
            input = Cable("Input cable", 3);
            output = Cable("Load cable", 3);
            Refresh(true);
        }

        private LineRenderer Cable(string title, int count)
        {
            var line = new GameObject(title, typeof(LineRenderer)).GetComponent<LineRenderer>();
            line.transform.SetParent(transform, false);
            line.sharedMaterial = Material;
            line.useWorldSpace = true;
            line.positionCount = count;
            line.startWidth = line.endWidth = .003f;
            line.numCapVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return line;
        }

        private void LateUpdate() => Refresh(false);

        private void Refresh(bool force)
        {
            if (input == null || Sequence == null || Sequence.Handle == null || Sequence.Door == null || Sequence.Drum == null) return;
            Vector3 hand = Sequence.Handle.transform.TransformPoint(0, .020f, .014f);
            Vector3 door = Sequence.Door.transform.TransformPoint(0, .070f, 0);
            if (!force && (hand-lastHandle).sqrMagnitude < .00000001f && (door-lastDoor).sqrMagnitude < .00000001f) return;
            lastHandle=hand;lastDoor=door;
            Vector3 drum=Sequence.Drum.position;
            Vector3 guide=Sequence.Access.Frame.TransformPoint(new Vector3(.12f,-.135f,.13f));
            input.SetPosition(0,hand);input.SetPosition(1,guide);input.SetPosition(2,drum);
            Vector3 pulley=Sequence.Door.Frame.TransformPoint(Sequence.Door.Start+Vector3.up*(Sequence.Door.Travel+.090f));
            output.SetPosition(0,drum);output.SetPosition(1,pulley);output.SetPosition(2,door);
        }
    }
}
