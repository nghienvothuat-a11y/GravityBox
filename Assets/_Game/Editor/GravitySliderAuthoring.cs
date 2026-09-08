using System;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    public static class GravitySliderAuthoring
    {
        public const float Stroke = 0.120f;
        public static readonly Vector3 GateSize = new Vector3(0.026f, 0.080f, 0.076f);

        public static PhysicalProp Build(Transform boxRoot, Material visualMaterial, PhysicsMaterial contactMaterial)
        {
            return Build(boxRoot, visualMaterial, contactMaterial, Vector3.zero, Quaternion.identity);
        }

        public static PhysicalProp Build(Transform boxRoot, Material visualMaterial, PhysicsMaterial contactMaterial,
            Vector3 closedPosition, Quaternion closedRotation)
        {
            if (boxRoot == null) throw new ArgumentNullException(nameof(boxRoot));
            Rigidbody box = boxRoot.GetComponent<Rigidbody>();
            if (box == null || !box.isKinematic) throw new ArgumentException("The slider housing must belong to the rotating kinematic box.", nameof(boxRoot));
            if ((boxRoot.lossyScale - Vector3.one).sqrMagnitude > 0.000001f)
                throw new ArgumentException("Author the slider and box at unit world scale, in metres.", nameof(boxRoot));

            var root = new GameObject("Gravity sliding gate", typeof(Rigidbody), typeof(BoxCollider), typeof(PhysicalProp));
            root.transform.SetParent(boxRoot, false);
            root.transform.localPosition = closedPosition;
            root.transform.localRotation = closedRotation;
            Vector3 slideAxisInBox = (closedRotation * Vector3.forward).normalized;
            Rigidbody body = root.GetComponent<Rigidbody>();
            body.mass = 0.18f;
            body.isKinematic = false;
            body.useGravity = false; // EnvironmentForceSystem applies the same world gravity as the steel ball.
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.solverIterations = 16;
            body.solverVelocityIterations = 8;
            body.maxDepenetrationVelocity = 1;
            body.sleepThreshold = 0.00001f;
            body.linearDamping = body.angularDamping = 0;
            BoxCollider collider = root.GetComponent<BoxCollider>();
            collider.size = GateSize;
            collider.center = Vector3.zero;
            collider.contactOffset = 0.0005f;
            collider.sharedMaterial = contactMaterial;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Sliding gate metal";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = GateSize;
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = visualMaterial;

            ConfigurableJoint joint = root.AddComponent<ConfigurableJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = box;
            joint.anchor = Vector3.zero;
            // Linear limits are symmetric. Placing the fixed anchor at half travel produces [0, Stroke].
            joint.connectedAnchor = closedPosition + slideAxisInBox * (Stroke * 0.5f);
            joint.axis = Vector3.forward;
            joint.secondaryAxis = Vector3.up;
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
            joint.linearLimit = new SoftJointLimit { limit = Stroke * 0.5f, bounciness = 0, contactDistance = 0.0005f };
            joint.linearLimitSpring = new SoftJointLimitSpring { spring = 0, damper = 0 };
            joint.xDrive = joint.yDrive = joint.zDrive = new JointDrive();
            joint.angularXDrive = joint.angularYZDrive = joint.slerpDrive = new JointDrive();
            joint.projectionMode = JointProjectionMode.None;
            joint.enableCollision = true;
            joint.enablePreprocessing = true;
            joint.massScale = joint.connectedMassScale = 1;
            joint.breakForce = joint.breakTorque = float.PositiveInfinity;

            GravitySliderGuide guide = root.AddComponent<GravitySliderGuide>();
            // The full doorway spans local Z [-.040, .040]. Its upper edge must clear the gate's lower edge.
            guide.Configure(joint, closedPosition, slideAxisInBox, Stroke, GateSize.z * 0.5f + 0.040f);
            return root.GetComponent<PhysicalProp>();
        }
    }
}
