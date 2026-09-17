using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    public sealed class VenomMovableProp : MonoBehaviour
    {
        public Rigidbody Body;
        public bool Manipulable;
        public bool ProvidesStep;
        [Tooltip("Optional reachable handle; forward points from the prop toward the creature.")]
        public Transform ManipulationGrip;
        public bool ManipulationHandleOnly;
        public Transform ManipulationPlane;
        public Vector3 InitialPosition;
        public Quaternion InitialRotation;
        private Collider[] collisionShapes;
        public Collider[] CollisionShapes=>collisionShapes??(collisionShapes=GetComponentsInChildren<Collider>());
        public void Capture(Transform root){InitialPosition=root.InverseTransformPoint(Body.position);InitialRotation=Quaternion.Inverse(root.rotation)*Body.rotation;}
        public void ResetTo(Transform root)
        {Body.position=root.TransformPoint(InitialPosition);Body.rotation=root.rotation*InitialRotation;Body.linearVelocity=Body.angularVelocity=Vector3.zero;}
    }
}
