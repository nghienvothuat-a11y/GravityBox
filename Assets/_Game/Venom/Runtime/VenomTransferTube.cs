using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    public sealed class VenomTransferTube : MonoBehaviour
    {
        public float Length=.22f, Radius=.026f, FlowSpeed=.11f;
        public VenomSurfacePatch Entrance;
        public bool AutoEnterOnContact;
        // Optional authored instruction cue; presentation only, never a movement target.
        public Transform DepartureHint;
        public Vector3 End=>transform.TransformPoint(Vector3.forward*Length);
    }
}
