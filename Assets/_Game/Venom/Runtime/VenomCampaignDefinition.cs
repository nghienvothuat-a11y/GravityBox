using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    [CreateAssetMenu(menuName="Gravity Box/Venom/Campaign level")]
    public sealed class VenomCampaignDefinition : ScriptableObject
    {
        public string Id, Title, Lesson;
        public int Order;
        public bool CanRotate=true, Boss, Passive;
        public Vector3 CameraEuler=new Vector3(24,-25,0);
        public float ViewRadius=.48f;
    }
}
