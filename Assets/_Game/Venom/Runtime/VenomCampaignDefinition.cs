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
        public VenomCameraZone(string label,Vector3 centre,Vector3 size)
        {Label=label;LocalBounds=new Bounds(centre,size);}
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
    }
}
