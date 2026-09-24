using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    [Serializable]
    public struct VenomCameraZone
    {
        public string Label;
        public Bounds LocalBounds;
        public bool OverrideCameraEuler;
        public Vector3 CameraEuler;
        public VenomCameraZone(string label,Vector3 centre,Vector3 size,Vector3? cameraEuler=null)
        {
            Label=label;LocalBounds=new Bounds(centre,size);
            OverrideCameraEuler=cameraEuler.HasValue;CameraEuler=cameraEuler.GetValueOrDefault();
        }
    }

    [CreateAssetMenu(menuName="Gravity Box/Venom/Campaign level")]
    public sealed class VenomCampaignDefinition : ScriptableObject
    {
        public string Id, Title, Lesson;
        public int Order;
        public bool CanRotate=true, Boss, Passive;
        public Vector3 CameraEuler=new Vector3(24,-25,0);
        public float ViewRadius=.48f; // Legacy capture/archived framing fallback.
        public VenomCameraZone[] CameraZones=Array.Empty<VenomCameraZone>();
        [Tooltip("-1 opens the overview; otherwise start at this authored inspection zone.")]
        public int InitialCameraZone=-1;
        [Tooltip("Optional self-contained campaign. Empty retains the Origin catalog and save IDs.")]
        public string[] SceneSequence=Array.Empty<string>();
        [Tooltip("Empty uses the existing production save. Experiments must use a separate key.")]
        public string ProgressKey="";
    }
}
