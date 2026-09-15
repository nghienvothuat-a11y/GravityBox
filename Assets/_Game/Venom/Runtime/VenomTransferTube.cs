using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    public sealed class VenomTransferTube : MonoBehaviour
    {
        public float Length=.22f, Radius=.026f;
        public VenomSurfacePatch Entrance;
        public Vector3 End=>transform.TransformPoint(Vector3.forward*Length);
    }
}
